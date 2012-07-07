using System;
using System.Reflection;
using System.Web.Hosting;
using System.Web;
using System.Collections;
using System.Diagnostics;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.Collections.Generic;
using System.Configuration;
using SenseNet.Diagnostics;
using System.Linq;


namespace SenseNet.Portal.Virtualization
{

    public class RepositoryPathProvider : VirtualPathProvider
    {
        public static void Register()
        {
            HostingEnvironment.RegisterVirtualPathProvider(new RepositoryPathProvider());
        }

        private RepositoryPathProvider()
        {
        }

        private static bool IsFileExistsInRepository(string path)
        {
            //PathInfoRemove2:
            //return RepositoryPathInfo.GetPathInfo(path) != null;
            return NodeHead.Get(path) != null;
        }

        private bool IsFileExistsInAssembly(string virtualPath)
        {
            if(!virtualPath.Contains("!Assembly"))
                return false;
            var pathElements = virtualPath.Split('/');
            int idx = 0;
            int asmIdx = -1;
            while(idx<pathElements.Length && asmIdx<0)
            {
                if(pathElements[idx].ToLower().Equals("!assembly"))
                    asmIdx = idx;
                idx++;
            }
            if(asmIdx+2>pathElements.Length)
                return false;


            var asmFullName = pathElements[asmIdx + 1];
            var resourceFullName = pathElements[asmIdx + 2];

            var asm = Assembly.Load(asmFullName);
            if(asm==null)
                return false;

            return asm.GetManifestResourceNames().Contains(resourceFullName);
        }

        private static bool _dfssinit = false;
        private static DiskFSSupportMode _diskFSSupportMode;
        public static DiskFSSupportMode DiskFSSupportMode
        {
            get
            {
                if (!_dfssinit)
                {
                    try
                    {
                        _diskFSSupportMode = (DiskFSSupportMode)Enum.Parse(typeof(DiskFSSupportMode),
                            ConfigurationSettings.AppSettings["DiskFSSupportMode"] ?? DiskFSSupportMode.Fallback.ToString());
                    }
                    catch (Exception e) //logged
                    {
                        Logger.WriteException(e);
                        _diskFSSupportMode = DiskFSSupportMode.Fallback;
                    }
                    finally
                    {
                        _dfssinit = true;
                    }
                }
                return _diskFSSupportMode;
            }
        }
        

        public override bool FileExists(string virtualPath)
        {
            //[5052] ##L2Cache> ~/Root/Sites/Default_Site.cshtml 
            //[5052] ##L2Cache> ~/Root/Sites/Default_Site.vbhtml 
            //[5052] ##L2Cache> ~/Root/Sites.cshtml 
            //[5052] ##L2Cache> ~/Root/Sites.vbhtml 
            //[5052] ##L2Cache> ~/Root.cshtml 
            //[5052] ##L2Cache> ~/Root.vbhtml 
            if (virtualPath.EndsWith(".cshtml") || virtualPath.EndsWith(".vbhtml"))
                return base.FileExists(virtualPath);


            if (DiskFSSupportMode == DiskFSSupportMode.Prefer &&
                base.FileExists(virtualPath))
                return true;

            PortalContext currentPortalContext = PortalContext.Current;
            
            // Indicates that the VirtualFile is requested by a HttpRequest (a Page.LoadControl also can be a caller, or an aspx for its codebehind file...)
            bool isRequestedByHttpRequest =
                (HttpContext.Current != null) ?
                (string.Compare(virtualPath, HttpContext.Current.Request.Url.LocalPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                : false;

            if (isRequestedByHttpRequest && currentPortalContext.IsRequestedResourceExistInRepository)
            {
                return true;
            }
            else if (IsFileExistsInRepository(virtualPath))
            {
                return true;
            }
            else if (IsFileExistsInAssembly(virtualPath))
            {
                return true;
            }
            else
            {
                // Otherwise it may exist in the filesystem - call the base
                return base.FileExists(virtualPath);
            }
        }

        

        public override VirtualFile GetFile(string virtualPath)
        {
            // office protocol: instruct microsoft office to open the document without further webdav requests when simply downloading the file
            // webdav requests would cause an authentication window to pop up when downloading a docx
            if (HttpContext.Current != null && HttpContext.Current.Response != null)
            {
                if (Repository.WebdavEditExtensions.Any(extension => virtualPath.EndsWith(extension)))
                    HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment");
            }

            if (DiskFSSupportMode == DiskFSSupportMode.Prefer &&
                base.FileExists(virtualPath))
            {
                var result = base.GetFile(virtualPath);

                //let the client code log file downloads
                if (virtualPath != null && PortalContext.Current != null && PortalContext.Current.ContextNodePath != null)
                    if (virtualPath.CompareTo(PortalContext.Current.ContextNodePath) == 0)
                        File.Downloaded(virtualPath);

                return result;
            }

            PortalContext currentPortalContext = PortalContext.Current;

            // Indicates that the VirtualFile is requested by a HttpRequest (a Page.LoadControl also can be a caller, or an aspx for its codebehind file...)
            bool isRequestedByHttpRequest =
                (HttpContext.Current != null) ?
                (string.Compare(virtualPath, HttpContext.Current.Request.Url.LocalPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                : false;

            if (isRequestedByHttpRequest && currentPortalContext.IsRequestedResourceExistInRepository)
            {
                return new RepositoryFile(virtualPath, currentPortalContext.RepositoryPath);
            }
            else if (IsFileExistsInRepository(virtualPath)) //OPT: nem kéne még egyszer megnézni, hogy bent van-e...
            {
                return new RepositoryFile(virtualPath, virtualPath);
            }
            else if(IsFileExistsInAssembly(virtualPath))
            {
                return new EmbeddedFile(virtualPath);
            }
            else
            {
                // Otherwise it may exist in the filesystem - call the base
                return base.GetFile(virtualPath);
            }
        }

        public override System.Web.Caching.CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return null;
        }

        
        // Return a hash value indicating a key to test this file and dependencies have not been modified
        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            if (DiskFSSupportMode == DiskFSSupportMode.Prefer &&
                base.FileExists(virtualPath))
            {
                var result = base.GetFileHash(virtualPath, virtualPathDependencies);
                return result;
            }

            string vp;
            if (virtualPath.EndsWith(PortalContext.InRepositoryPageSuffix))
                vp = virtualPath.Substring(0, virtualPath.Length - PortalContext.InRepositoryPageSuffix.Length);
            else
                vp = virtualPath;

            if (IsFileExistsInRepository(vp))
            {
                HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();
                foreach (string virtualDependency in virtualPathDependencies)
                {
                    string vd;
                    if (virtualDependency.EndsWith(PortalContext.InRepositoryPageSuffix))
                        vd = virtualDependency.Substring(0, virtualDependency.Length - PortalContext.InRepositoryPageSuffix.Length);
                    else
                        vd = virtualDependency;

                    //PathInfoRemove3:
                    //RepositoryPathInfo repositoryPathInfo = RepositoryPathInfo.GetPathInfo(vd);

                    //if (repositoryPathInfo != null)
                    //{
                    //    hashCodeCombiner.AddLong(Convert.ToInt64(repositoryPathInfo.ModificationDate.GetHashCode()));
                    //    PortalContext portalContext = PortalContext.Current;
                    //    if (portalContext != null)
                    //    {
                            
                    //    }
                    //}

                    var nodeDesc = NodeHead.Get(vd);
                    if (nodeDesc != null)
                    {
                        hashCodeCombiner.AddLong(Convert.ToInt64(nodeDesc.ModificationDate.GetHashCode()));
                    }

                }
                return hashCodeCombiner.CombinedHashString;
            }
            else
            {
                return base.GetFileHash(vp, virtualPathDependencies);
            }
        }
    }
}