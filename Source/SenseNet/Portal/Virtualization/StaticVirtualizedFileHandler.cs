using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.Virtualization
{
    internal static class HelperExtensions
    {
        internal static string ChangeBackslashToSlash(this string originalString)
        {
            return originalString.Replace('/', '\\');
        }
    }

    internal class StaticVirtualizedFileHandler : IHttpHandler
    {
        private static readonly double CacheTimeframeInMinutes = 0.1;

        private static string _cacheFolderFileSystemPath;
        private static string CacheFolderFileSystemPath
        {
            get
            {
                if (_cacheFolderFileSystemPath == null)
                {
                    _cacheFolderFileSystemPath = System.Configuration.ConfigurationManager.AppSettings["CacheFolderFileSystemPath"];
                    if (_cacheFolderFileSystemPath == null)
                        //throw new System.Configuration.ConfigurationErrorsException("Missing CacheFolderFileSystemPath value.");
                        throw new SenseNet.ContentRepository.Storage.Data.ConfigurationException("Missing CacheFolderFileSystemPath value.");
                }
                return _cacheFolderFileSystemPath;
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var repositoryPath = context.Request.FilePath;

            var fullyQualyfiedPath = string.Concat(
                CacheFolderFileSystemPath,
                repositoryPath.ChangeBackslashToSlash()
                );

            if (IsLockFileExists(fullyQualyfiedPath))
            {
                // File locked - cannot server from filesystem, fallback to the virtual infrastucture
                ProcessRequestUsingVirtualFile(context, repositoryPath);
            }
            else
            {
                // File is not locked, use filesystem cache
                ProcessRequestUsingFilesystemCache(context, repositoryPath, fullyQualyfiedPath);
            }
        }

        private static void ProcessRequestUsingFilesystemCache(HttpContext context, string repositoryPath, string fullyQualyfiedPath)
        {
            // Check the filesystem cache, create the file if not exists
            if (!System.IO.File.Exists(fullyQualyfiedPath))
            {
                byte[] buffer = ReadVirtualFile(repositoryPath);

                string directoryName = Path.GetDirectoryName(fullyQualyfiedPath);
                if (!Directory.Exists(directoryName))
                    System.IO.Directory.CreateDirectory(directoryName);

                using (var cachedFile = System.IO.File.Create(fullyQualyfiedPath))
                {
                    cachedFile.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                var servedFromFileSystem = true;

                // csekkoljuk mikori a fileunk
                DateTime lastWriteTime = System.IO.File.GetLastWriteTime(fullyQualyfiedPath);
                DateTime now = DateTime.Now;
                var nodeDescription = NodeHead.Get(repositoryPath);

                if (lastWriteTime.AddMinutes(CacheTimeframeInMinutes) < now)
                {
                    // régebbi, mint x perc, update kell
                    CreateLockFile(fullyQualyfiedPath);

                    int maxTryNum = 20;
                    IOException lastError = null;
                    while (--maxTryNum >= 0)
                    {
                        try
                        {
                            System.IO.File.SetLastWriteTime(fullyQualyfiedPath, now);
                            break;
                        }
                        catch (IOException ioex) //TODO: catch block
                        {
                            lastError = ioex;
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                    if (lastError != null)
                        throw new IOException("Cannot write the file: " + fullyQualyfiedPath, lastError);

                    if (nodeDescription.ModificationDate > lastWriteTime)
                    {
                        // refresh file
                        byte[] buffer = ReadVirtualFile(repositoryPath);
                        servedFromFileSystem = false;

                        using (var cachedFile = System.IO.File.Open(fullyQualyfiedPath, FileMode.Truncate, FileAccess.Write))
                        {
                            cachedFile.Write(buffer, 0, buffer.Length);
                        }
                    }

                    DeleteLockFile(fullyQualyfiedPath);
                }

                //only log file download if it was not logged before by the virtual file provider
                if (servedFromFileSystem)
                {
                    //let the client code log file downloads
                    if (nodeDescription != null && ActiveSchema.NodeTypes[nodeDescription.NodeTypeId].IsInstaceOfOrDerivedFrom("File"))
                        ContentRepository.File.Downloaded(nodeDescription.Id);
                }
            }
            
            string extension = System.IO.Path.GetExtension(repositoryPath);
            context.Response.ContentType = MimeTable.GetMimeType(extension);

            context.Response.TransmitFile(fullyQualyfiedPath);
        }

        private static void ProcessRequestUsingVirtualFile(HttpContext context, string repositoryPath)
        {
            byte[] buffer = ReadVirtualFile(repositoryPath);

            context.Response.ClearContent();

            context.Response.OutputStream.Write(buffer, 0, buffer.Length);

            context.Response.ContentType =
                MimeTable.GetMimeType(System.IO.Path.GetExtension(repositoryPath));

            context.Response.End();
        }

        private static bool IsLockFileExists(string fullyQualyfiedPath)
        {
            return System.IO.File.Exists(GetLockFileName(fullyQualyfiedPath));
        }

        private static void CreateLockFile(string fullyQualyfiedPath)
        {
            var lockFile = System.IO.File.Create(GetLockFileName(fullyQualyfiedPath));
            lockFile.Dispose();
        }

        private static void DeleteLockFile(string fullyQualyfiedPath)
        {
            System.IO.File.Delete(GetLockFileName(fullyQualyfiedPath));
        }

        private static string GetLockFileName(string fullyQualyfiedPath)
        {
            return string.Concat(fullyQualyfiedPath, ".lock");
        }

        private static byte[] ReadVirtualFile(string repositoryPath)
        {
            byte[] buffer;
            int length;

            // Read the virtual file
            using (var virtualFile = System.Web.Hosting.VirtualPathProvider.OpenFile(repositoryPath))
            {
                if (virtualFile == null)
                    throw new ApplicationException(string.Format("The virtual file could not be read in the StaticVirtualizedFileHandler for the virtual path '{0}'", repositoryPath));

                length = (int)virtualFile.Length;
                buffer = new byte[length];
                virtualFile.Read(buffer, 0, length);
            }

            return buffer;
        }
    }

}
