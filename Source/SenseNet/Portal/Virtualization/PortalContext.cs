using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using SenseNet.Communication.Messaging;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Security;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.AppModel;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Workspaces;
using System.Web.Configuration;
using SenseNet.ApplicationModel;
using SenseNet.Search;

namespace SenseNet.Portal.Virtualization
{
    public class L2CacheImpl : IL2Cache
    {
        public object Get(string key)
        {
            var cache = GetCache();
            if (cache == null)
            {
                return null;
            }
            object value;
            if (cache.TryGetValue(key, out value))
            {
                //var node = value as Node;
                //if (node != null)
                //    Debug.WriteLine(String.Format("##L2Cache> HIT: {0}, Id:{1}, VerId:{2}, Ver:{3}, {4}", key, node.Id, node.VersionId, node.Version, node.Path));
                return value;
            }
            return null;
        }
        public void Set(string key, object value)
        {
            var cache = GetCache();
            if (cache == null)
            {
                if (HttpContext.Current == null)
                    return;
                //Debug.WriteLine("##L2Cache> CREATE CACHE");
                cache = new Dictionary<string, object>(100);
                HttpContext.Current.Items.Add("L2Cache", cache);
            }
            //var node = value as Node;
            //if (node != null)
            //    Debug.WriteLine(String.Format("##L2Cache> ADD: {0}, Id:{1}, VerId:{2}, Ver:{3}, {4}", key, node.Id, node.VersionId, node.Version, node.Path));
            try
            {
                cache.Add(key, value);
            }
            catch (Exception e)
            {
                throw new Exception("Content has been already added: " + key, e);
            }
        }
        public void Clear()
        {
            var cache = GetCache();
            if (cache != null)
            {
                //Debug.WriteLine("##L2Cache> CLEAR CACHE");
                cache.Clear();
            }
        }
        private Dictionary<string, object> GetCache()
        {
            if (HttpContext.Current == null)
                return null;
            return (Dictionary<string, object>)HttpContext.Current.Items["L2Cache"];
        }
    }

    internal class PortalContextInitInfo
    {
        public Uri RequestUri;
        public bool IsWebdavRequest;
        public Site RequestedSite;
        public string SiteUrl;
        public string SiteRelativePath;
        public string RepositoryPath;
        public NodeHead RequestedNodeHead;
        public string ActionName;
        public string AppNodePath;
        public string ContextNodePath;
        public string VersionRequest;
        public NodeHead BinaryHandlerRequestedNodeHead;
        public string DeviceName;
        public Site RequestedNodeSite;
    }

    public enum BackTargetType { None, CurrentContent, CurrentList, CurrentWorkspace, CurrentSite, CurrentPage, CurrentUser, Parent, NewContent }

    [DebuggerDisplay("{_originalUri} (RepositoryPath={_repositoryPath}, SiteUrl={_siteUrl}, NodeType={NodeTypeName}")]
    public class PortalContext : IHttpActionContext
    {
        //-------------------------------------------------------------------------------------------------- IAppContext

        string IHttpActionContext.RequestedUrl { get { return RequestedUri.ToString(); } }
        Uri IHttpActionContext.RequestedUri { get { return RequestedUri; } }
        NameValueCollection IHttpActionContext.Params { get { return OwnerHttpContext.Request.Params; } }
        NodeHead IHttpActionContext.GetRequestedNode()
        {
            return GetNode(RequestedUri);
        }
        string IHttpActionContext.RequestedActionName { get { return this.ActionName; } }
        string IHttpActionContext.RequestedApplicationNodePath { get { return this.ApplicationNodePath; } }
        string IHttpActionContext.RequestedContextNodePath { get { return this.ContextNodePath; } }
        IHttpActionFactory IHttpActionContext.GetActionFactory() { return new HttpActionFactory(); }
        public IHttpAction CurrentAction { get; set; } //---- IAppContext

        //--------------------------------------------------------------------------------------------------

        private static bool StartsWithRoot(string path)
        {
            return path.Equals(Repository.RootPath, StringComparison.InvariantCultureIgnoreCase)
                || path.StartsWith(string.Concat(Repository.RootPath, "/"), StringComparison.InvariantCultureIgnoreCase);
        }
        private NodeHead GetNode(Uri requestUri)
        {
            if (this.ContextNode == null)
                return null;
            var head = NodeHead.Get(this.ContextNode.Id);
            return head;
        }

        //--------------------------------------------------------------------------------------------------

        private bool _isWebdavRequest;
        public bool IsWebdavRequest
        {
            get { return _isWebdavRequest; }
        }

        #region Distributed Action child classes

        [Serializable]
        public class ReloadSiteListDistributedAction : DistributedAction
        {
            public override void DoAction(bool onRemote, bool isFromMe)
            {
                if (!(onRemote && isFromMe))
                {
                    ReloadSiteList();
                }
            }
        }

        [Serializable]
        internal class ReloadSmartUrlListDistributedAction : DistributedAction
        {
            public override void DoAction(bool onRemote, bool isFromMe)
            {
                if (!(onRemote && isFromMe))
                {
                    //invalidate smarturl list
                    _smartUrls = null;
                }
            }
        }

        #endregion

        //================================================ Static part ================================================

        public static string ActionParamName { get { return "action"; } }
        public static string AppNodeParamName { get { return "app"; } }
        public static string ContextNodeParamName { get { return "context"; } }
        public static string BackUrlParamName { get { return "back"; } }
        public static string BackTargetParamName { get { return "backtarget"; } }
        public static string VersionParamName { get { return "version"; } }

        [Obsolete("Use AuthenticationHelper.DenyAccess instead", true)]
        public static void ForceBasicAuthentication(HttpContext context)
        {
            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.StatusCode = 401;
            context.Response.Status = "401 Unauthorized";
            context.Response.AddHeader("WWW-Authenticate", "Basic");

            //context.Response.Flush();
            context.Response.End();
        }

        private static readonly string CONTEXT_ITEM_KEY = "_CurrentPortalContext";
        private static readonly string QUERYSTRING_NODEPROPERTY_KEY = "NodeProperty";
        public static readonly string DefaultNodePropertyName = "Binary";
        public static readonly string InRepositoryPageSuffix = "/InRepositoryPage.aspx";
        public static readonly string WebRootFolderPath = "/Root/System/WebRoot";
        private static readonly string PortalSectionKey = "sensenet/portalSettings";

        private static NameValueCollection _urlPaths;
        private static NameValueCollection _startPages;
        private static NameValueCollection _authTypes;
        private static NameValueCollection _loginPages;

        //-------------------------------------------------------------------------------- Creation and accessor
        

        internal static PortalContextInitInfo CreateInitInfo(HttpContext context)
        {
            var requestUri = context.Request.Url;
            var absPathLower = requestUri.AbsolutePath.ToLower();

            //---- STEP 1: Search for a matching Site URL based on the Request URL
            string matchingSiteUrl = null;

            // Drop the scheme (http://) and the querystring parts of the URL
            // Example: htttp://localhost:1315/public/folder1? ==> localhost:1315/public/folder1
            string nakedRequestUrl = requestUri.GetComponents(UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.Unescaped);

            if (nakedRequestUrl.EndsWith(InRepositoryPageSuffix))
                nakedRequestUrl = nakedRequestUrl.Remove(nakedRequestUrl.Length - InRepositoryPageSuffix.Length);

            // Get the matching site url (if any)
            Site requestedSite = null;
            foreach (string siteUrl in Sites.Keys)
            {
                if (nakedRequestUrl.StartsWith(siteUrl) &&
                    (nakedRequestUrl.Length == siteUrl.Length || nakedRequestUrl[siteUrl.Length] == '/' || nakedRequestUrl[siteUrl.Length] == '?'))
                {
                    matchingSiteUrl = siteUrl;
                    requestedSite = Sites[siteUrl];
                    break;
                }
            }

            //---- STEP 2: WebDav
            bool isWebdavRequest = false;
            var useragent = context.Request.Headers["User-Agent"];
            if (!string.IsNullOrEmpty(useragent))
            {
                var ualower = useragent.ToLower();
                isWebdavRequest = ualower.Contains("webdav") || ualower == "microsoft office protocol discovery" || ualower == "microsoft office existence discovery";
            }

            if (isWebdavRequest && absPathLower.StartsWith(TrashBin.TrashBinPath.ToLower()))
            {
                EndResponse(404);
                return null;
            }

            //---- STEP 3: Create Repository Path
            string repositoryPath = null;
            string siteRelativePath = null;

            //---- if Webroot
            if (PortalContext.WebRootFiles.Any(n => n == absPathLower))
            {
                repositoryPath = ContentRepository.Storage.RepositoryPath.Combine(PortalContext.WebRootFolderPath, absPathLower);
                siteRelativePath = null;
            }

            //---- if DWSS (Document Workspace Web Service Protocol)
            if (absPathLower.EndsWith("_vti_bin/dws.asmx") ||
                absPathLower.EndsWith("_vti_bin/webs.asmx") ||
                absPathLower.EndsWith("_vti_bin/lists.asmx") ||
                absPathLower.EndsWith("_vti_bin/versions.asmx") ||
                absPathLower.EndsWith("_vti_bin/owssvr.dll"))
            {
                if (absPathLower.StartsWith(TrashBin.TrashBinPath.ToLower()))
                {
                    EndResponse(404);
                    return null;
                }

                var requestPath = absPathLower;
                var prefix = "_vti_bin";
                var prefixIdx = requestPath.IndexOf(prefix);
                var redirectPath = string.Concat(PortalContext.WebRootFolderPath, "/DWS", requestPath.Substring(prefixIdx + prefix.Length)); // ie. /Root/System/WebRoot/DWS/lists.asmx ... 
                repositoryPath = redirectPath.Replace("owssvr.dll", "owssvr.aspx");
                isWebdavRequest = false;    // user agent might be webdav, but we should redirect to given content, and not to webdav service
            }

            //---- if FPP (FrontPage Protocol)
            if (absPathLower.EndsWith("_vti_inf.html") ||
                absPathLower.EndsWith("_vti_rpc") ||
                absPathLower.EndsWith("_vti_aut/author.dll") ||
                absPathLower.EndsWith("_vti_bin/workflow.asmx"))
            {
                if (absPathLower.StartsWith(TrashBin.TrashBinPath.ToLower()))
                {
                    EndResponse(404);
                    return null;
                }

                // NOTE: workflow.asmx is not actually implemented, we return a HTTP 200 using FppHandler.cs
                repositoryPath = string.Concat(PortalContext.WebRootFolderPath, "/DWS/Fpp.ashx");
                isWebdavRequest = false;    // user agent might be webdav, but we should redirect to given content, and not to webdav service
            }

            // otherwise (not webroot, not fpp, not dws): set repositorypath to addressed repository content path
            if (repositoryPath == null)
            {
                if (matchingSiteUrl != null)
                {
                    //siteRelativePath = requestUri.AbsolutePath; //-- ez nem jo, mert nincs unescape (%20)
                    siteRelativePath = nakedRequestUrl.Substring(matchingSiteUrl.Length);
                    repositoryPath = StartsWithRoot(siteRelativePath) ? siteRelativePath : string.Concat(Sites[matchingSiteUrl].Path, siteRelativePath);

                    // Remove trailing slash
                    if (repositoryPath.EndsWith("/"))
                        repositoryPath = repositoryPath.Substring(0, repositoryPath.Length - 1);
                }
                else
                {
                    // The request does not belong to a site (eg. "http://localhost/Root/System/Skins/Test.css")
                    repositoryPath = HttpUtility.UrlDecode(requestUri.AbsolutePath);

                    //TODO: check this
                    string appPath = context.Request.ApplicationPath;
                    if (appPath != null && appPath.Length > 1)
                        repositoryPath = repositoryPath.Substring(appPath.Length - 1);
                }
            }

            //---- STEP 4: if Cassini
            if (requestedSite != null && siteRelativePath == "/default.aspx")
            {
                repositoryPath = requestedSite.Path;
                siteRelativePath = "";
            }

            //---- STEP 5: Appmodel elements: requested nodeHead, action, app, context
            var requestedNodeHead = NodeHead.Get(repositoryPath);
            var actionName = context.Request.Params[ActionParamName];
            var appNodePath = context.Request.Params[AppNodeParamName];
            var contextNodePath = context.Request.Params[ContextNodeParamName];
            var versionRequest = context.Request.Params[VersionParamName];
            AssertAppmodelelements(requestedNodeHead, actionName, appNodePath, contextNodePath);

            //---- STEP 6: BinaryHandler requestednodehead for preliminary is-modified-since handling
            NodeHead binaryhandlerRequestedNodeHead = null;
            if (absPathLower.EndsWith("/binaryhandler.ashx"))
                binaryhandlerRequestedNodeHead = SenseNet.Portal.Handlers.BinaryHandler.RequestedNodeHead;

            Site requestedNodeSite = null;
            if (repositoryPath != null)
                requestedNodeSite = Site.GetSiteByNodePath(repositoryPath);

            return new PortalContextInitInfo()
            {
                RequestUri = requestUri,
                IsWebdavRequest = isWebdavRequest,
                RequestedSite = requestedSite,
                SiteUrl = matchingSiteUrl,
                SiteRelativePath = siteRelativePath,
                RepositoryPath = repositoryPath,
                RequestedNodeHead = requestedNodeHead,
                ActionName = actionName,
                AppNodePath = appNodePath,
                ContextNodePath = contextNodePath,
                VersionRequest = versionRequest,
                BinaryHandlerRequestedNodeHead = binaryhandlerRequestedNodeHead,
                DeviceName = PortalContext.GetRequestedDevice(),
                RequestedNodeSite = requestedNodeSite
            };
        }
        internal static PortalContext Create(HttpContext context)
        {
            var initInfo = CreateInitInfo(context);
            return Create(context, initInfo);
        }
        internal static PortalContext Create(HttpContext context, PortalContextInitInfo initInfo)
        {
            PortalContext pc = new PortalContext();
            pc.Initialize(context, initInfo);

            context.Items.Add(CONTEXT_ITEM_KEY, pc);
            context.Items.Add(ApplicationStorage.DEVICEPARAMNAME, pc.DeviceName);

            return pc;
        }
        private static void AssertAppmodelelements(NodeHead requestedNodeHead, string actionName, string appNodePath, string contextNodePath)
        {
            var paramCount = (actionName == null ? 0 : 1) + (appNodePath == null ? 0 : 1) + (contextNodePath == null ? 0 : 1);
            if (paramCount > 1)
                throw new InvalidOperationException("More than one Application model parameters are not applicable.");
            if (contextNodePath != null && !IsApplicationNode(requestedNodeHead))
                throw new InvalidOperationException("Requested node is not an application.");
        }

        public static bool IsApplicationNode(Node node)
        {
            return IsApplicationNodeType(node.NodeType);
        }
        public static bool IsApplicationNode(NodeHead nodeHead)
        {
            return IsApplicationNodeType(nodeHead.GetNodeType());
        }
        public static bool IsApplicationNodeType(NodeType nodeType)
        {
            if (nodeType.IsInstaceOfOrDerivedFrom("Page"))
                return true;

            Type appType = TypeHandler.GetType(nodeType.ClassName);
            return typeof(IHttpHandler).IsAssignableFrom(appType);
        }

        public static PortalContext Current
        {
            get
            {
                if (HttpContext.Current == null)
                    return null;
                return HttpContext.Current.Items[CONTEXT_ITEM_KEY] as PortalContext;
            }
        }

        //-------------------------------------------------------------------------------- Smart URL

        private static Dictionary<string, string> _smartUrls;
        private static object _smartUrlsLock = new object();
        internal static Dictionary<string, string> SmartUrls
        {
            get
            {
                if (_smartUrls == null)
                {
                    lock (_smartUrlsLock)
                    {
                        if (_smartUrls == null)
                        {
                            _smartUrls = ReloadSmartUrlList();
                        }
                    }
                }

                return _smartUrls;
            }
        }

        private static Dictionary<string, string> ReloadSmartUrlList()
        {
            using (new SystemAccount())
            {
                NodeQueryResult pageResult;
                if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
                {
                    var pageQuery = new NodeQuery();
                    pageQuery.Add(new TypeExpression(ActiveSchema.NodeTypes[typeof(Page).Name]));
                    pageResult = pageQuery.Execute();
                }
                else
                {
                    pageResult = NodeQuery.QueryNodesByType(ActiveSchema.NodeTypes[typeof (Page).Name], false);
                }

                var smartUrls = new Dictionary<string, string>();

                if (pageResult == null)
                    throw new ApplicationException("SmartURL: Query returned null.");

                foreach (Page page in pageResult.Nodes)
                {
                    if (page == null)
                        throw new ApplicationException("SmartURL: a result in the query resultset is null.");

                    var smartUrl = page.SmartUrl;
                    if (string.IsNullOrEmpty(smartUrl))
                        continue;

                    if (!smartUrl.StartsWith("/"))
                        smartUrl = string.Concat("/", smartUrl);

                    smartUrl = smartUrl.ToLowerInvariant();

                    var site = GetSiteByNodePath(page.Path);
                    if (site == null)
                        continue;

                    var siteRelativeUrl = page.Path.Substring(site.Path.Length).ToLowerInvariant();

                    // smart url key: site identifier + smart url keyword
                    var smartUrlKey = string.Concat(site.Path.ToLowerInvariant(), ":", smartUrl);

                    if (smartUrls.ContainsKey(smartUrlKey))
                    {
                        Logger.WriteError("Cannot set a smart url multiple times on a site.",
                                          new Dictionary<string, object> { { "Site", site.Path }, { "SmartUrl", smartUrl } });
                        // Should we throw exception if multiple smart urls have been set?
                        continue;
                    }

                    smartUrls.Add(smartUrlKey, siteRelativeUrl);
                }

                return smartUrls;
            }
        }

        //-------------------------------------------------------------------------------- Sites

        private static Dictionary<string, Site> _sites;
        private static object _sitesLock = new object();
        internal static Dictionary<string, Site> Sites
        {
            get
            {
                if (_sites == null)
                {
                    ReloadSiteList();
                }
                return _sites;
            }
        }

        private static void ReloadSiteList()
        {
            lock (_sitesLock)
            {
                NodeQueryResult result = null;

                try
                {
                    if (ContentRepository.Schema.ContentType.GetByName("Site") == null)
                        throw new ApplicationException("Unknown ContentType: Site");

                    //CONDITIONAL EXECUTE
                    using (new SystemAccount())
                    {
                        if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
                        {
                            var query = new NodeQuery();
                            var nt = ActiveSchema.NodeTypes[typeof(Site).Name];
                            query.Add(new TypeExpression(nt, false));
                            result = query.Execute();//.Nodes.ToList<Node>();
                        }
                        else
                        {
                            result = NodeQuery.QueryNodesByType(ActiveSchema.NodeTypes[typeof(Site).Name], false);//.Nodes.ToList<Node>();
                        }
                    }

                    _urlPaths = new NameValueCollection(result.Count);
                    _startPages = new NameValueCollection(result.Count);
                    _authTypes = new NameValueCollection(result.Count);
                    _loginPages = new NameValueCollection(result.Count);
                }
                catch (Exception e) //logged
                {
                    Logger.WriteException(e);
                }

                _sites = new Dictionary<string, Site>();

                // urlsettings come from webconfig
                var configSites = Configuration.UrlListSection.Current.Sites;

                // urlsettings come either from sites in content repository or from webconfig
                if (result != null)
                {
                    //Loading sites and start pages should be done with and admin account.
                    //Authorization will occur when the user tries to load 
                    //the start page of the selected site.

                    using (new SystemAccount())
                    {
                        foreach (Site site in result.Nodes)
                        {
                            var siteUrls = site.UrlList.Keys;

                            // siteurls come from webconfig
                            if (configSites.Count > 0 && configSites[site.Path] != null)
                                siteUrls = configSites[site.Path].Urls.GetUrlHosts();

                            foreach (string siteUrl in siteUrls)
                            {
                                try
                                {
                                    _sites.Add(siteUrl, site);
                                }
                                catch (ArgumentException) //rethrow
                                {
                                    throw new ArgumentException(String.Format("The url '{0}' has already been added to site '{1}' and cannot be added to site '{2}'", siteUrl, _sites[siteUrl].Name, site.Name));
                                }
                            }

                            string siteLoginPageUrl = (site.LoginPage != null ? site.LoginPage.Path : null);
                            foreach (string url in siteUrls)
                            {
                                _urlPaths.Add(url, site.Path);
                                _loginPages.Add(url, siteLoginPageUrl);
                                _startPages.Add(url, site.StartPage != null ? site.StartPage.Name : string.Empty);

                                // auth types come from webconfig or from site urllist
                                if (configSites.Count > 0 && configSites[site.Path] != null)
                                    _authTypes.Add(url, configSites[site.Path].Urls[url].Auth);
                                else
                                    _authTypes.Add(url, site.UrlList[url]);
                            }
                        }
                    }
                }
            }
        }

        public string GetCurrentAuthenticationMode()
        {
            if (_siteUrl != null)
            {
                var configSites = SenseNet.Configuration.UrlListSection.Current.Sites;
                // current auth mode comes from webconfig
                if (configSites.Count > 0)
                    return configSites[_site.Path].Urls[_siteUrl].Auth;

                // current auth mode comes from sites's urllist
                return _site.UrlList[_siteUrl];
            }
            return null;
        }

        //-------------------------------------------------------------------------------- Utlities

        public static string GetLoginPagePathByRequestUri(Uri requestUri)
        {
            if (requestUri == null)
                throw new ArgumentNullException("requestUrl");
            foreach (string loginPageKey in _loginPages.Keys)
            {
                string siteUrl = string.Concat(requestUri.Scheme, "://", loginPageKey);
                if (string.Concat(requestUri.AbsoluteUri, "/").StartsWith(string.Concat(siteUrl, "/")))
                {
                    return _loginPages[loginPageKey];
                }
            }
            return null;
        }
        public static string GetUrlByRepositoryPath(string url, string repositoryPath)
        {
            foreach (string key in _urlPaths.AllKeys)
            {
                if (string.Concat(url, "/").StartsWith(string.Concat(key, "/")))
                {
                    return key + repositoryPath.Substring(_urlPaths[key].Length);
                }
            }
            return null;
        }
        public static string GetUrlByRepositoryPath(string repositoryPath)
        {
            foreach (string key in _urlPaths.AllKeys)
            {
                if (string.Concat(repositoryPath, "/").StartsWith(string.Concat(_urlPaths[key], "/")))
                {
                    return key + repositoryPath.Substring(_urlPaths[key].Length);
                }
            }
            return null;
        }

        internal static Site GetSiteByNodePath(string path)
        {
            path = VirtualPathUtility.AppendTrailingSlash(path);

            return Sites.Values.FirstOrDefault(site => path.StartsWith(VirtualPathUtility.AppendTrailingSlash(site.Path)));
        }

        public static string GetSiteRelativePath(string fullpath)
        {
            return GetSiteRelativePath(fullpath, Current != null ? Current.Site : null);
        }

        public static string GetSiteRelativePath(string fullpath, Site site)
        {
            if (site == null)
                return fullpath;

            var sitePath = site.Path;

            if (fullpath.Length != sitePath.Length && fullpath.StartsWith(sitePath))
                return fullpath.Substring(sitePath.Length);

            return fullpath.Equals(sitePath) ? "/" : fullpath;
        }

        public string GetContentUrl(object content)
        {
            var c = content as Content;
            if (c == null)
                return string.Empty;

            var path = c.Path;
            var sitePath = Site.Path;

            if (path.CompareTo(sitePath) == 0)
                path = string.Empty;
            else if (path.StartsWith(sitePath + "/"))
                path = path.Remove(0, sitePath.Length);

            return string.Format("{0}?{1}={2}", path, BackUrlParamName, HttpUtility.UrlEncode(HttpContext.Current.Request.RawUrl));
        }

        public static bool IsWebSiteRoot
        {
            get
            {
                try
                {
                    return HttpRuntime.AppDomainAppVirtualPath.Length <= 1;
                }
                catch 
                {
                    //it is possible that logging module is not working yet
                    Trace.WriteLine("PortalContext.IsWebSiteRoot: Getting AppDomainAppVirtualPath is impossible.");
                }

                return true;
            }
        }

        private static void EndResponse(int statusCode)
        {
            HttpContext.Current.Response.StatusCode = statusCode;
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }

        //-------------------------------------------------------------------------------- Proxy

        private const string PROXYIPKEY = "ProxyIP";
        private static List<string> _proxyIPList;
        public static List<string> ProxyIPs
        {
            get
            {
                if (_proxyIPList == null)
                {
                    var value = new List<string>();
                    var settings = ConfigurationManager.GetSection(PortalSectionKey) as NameValueCollection;

                    if (settings != null)
                    {
                        var setting = settings[PROXYIPKEY];
                        if (!string.IsNullOrEmpty(setting))
                        {
                            value.AddRange(setting.Split(new [] {',', ';'}, StringSplitOptions.RemoveEmptyEntries));
                        }
                    }

                    _proxyIPList = value.Distinct().ToList();
                }

                return _proxyIPList;
            }
        }
        private const string PURGEURLDELAYINSECONDSKEY = "PurgeUrlDelayInSeconds";
        private static object __purgeUrlDelaySync = new object();
        private static int? _purgeUrlDelayInMilliSeconds;
        public static int? PurgeUrlDelayInMilliSeconds
        {
            get
            {
                if (!_purgeUrlDelayInMilliSeconds.HasValue)
                {
                    lock (__purgeUrlDelaySync)
                    {
                        if (!_purgeUrlDelayInMilliSeconds.HasValue)
                        {
                            var settings = ConfigurationManager.GetSection(PortalSectionKey) as NameValueCollection;
                            if (settings != null)
                            {
                                var setting = settings[PURGEURLDELAYINSECONDSKEY];
                                int value;
                                if (!string.IsNullOrEmpty(setting) && Int32.TryParse(setting, out value))
                                    _purgeUrlDelayInMilliSeconds = value * 1000;
                            }
                        }
                    }
                }
                return _purgeUrlDelayInMilliSeconds;
            }
        }


        //================================================ Instance part ================================================

        private HttpContext _ownerHttpContext;
        public HttpContext OwnerHttpContext { get { return _ownerHttpContext; } }

        private Site _site;
        public Site Site
        {
            get { return _site; }
        }

        public const string ContextWorkspaceResolverKey = "ContextWorkpsaceResolver";


        /// <summary>
        /// ContextWorkspaceResolver
        /// </summary>
        public static Func<PortalContext, Workspace> ContextWorkspaceResolver 
        { 
            get
            {
                var item = HttpContext.Current.Items[ContextWorkspaceResolverKey];
                if (item == null)
                    return null;
                var resolver = item as Func<PortalContext, Workspace>;
                if (item == null)
                    return null;
                return resolver;
            }
            set 
            {
                HttpContext.Current.Items.Add(ContextWorkspaceResolverKey, value);
            }
        }

        Workspace _contextWorkspace;

        public Workspace ContextWorkspace
        {
            get
            {
                if (_contextWorkspace == null)
                {
                    using (new SystemAccount())
                    {
                        if (ContextWorkspaceResolver != null)
                        {
                            _contextWorkspace = ContextWorkspaceResolver(this);
                        }
                        else
                        {
                            _contextWorkspace = Workspace.GetWorkspaceForNode(this.ContextNode);
                        }
                    }
                }

                return _contextWorkspace;
            }
        }

        [Obsolete("Use the ContextWorkspace property instead.")]
        public Workspace Workspace
        {
            get { return ContextWorkspace; }
        }

        private string _authenticationMode;
        public string AuthenticationMode
        {
            get { return _authenticationMode; }
        }

        private string _repositoryPath;
        public string RepositoryPath
        {
            get { return _repositoryPath; }
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
        }

        private int _nodeId;
        public int NodeId
        {
            get { return _nodeId; }
        }

        private NodeType _nodeType;
        public NodeType NodeType
        {
            get { return _nodeType; }
        }

        private string _queryStringNodePropertyName;
        public string QueryStringNodePropertyName
        {
            get { return _queryStringNodePropertyName; }
        }

        private Uri _originalUri;
        public Uri RequestedUri
        {
            get { return _originalUri; }
        }
        [Obsolete("Use RequestedUri property instead")]
        public Uri OriginalUri
        {
            get { return RequestedUri; }
        }

        private string _siteUrl;
        public string SiteUrl
        {
            get { return _siteUrl; }
        }

        //private string _pageRepositoryPath;
        //internal string PageRepositoryPath
        //{
        //    get { return _pageRepositoryPath; }
        //}

        private string _currentSkin;
        public string CurrentSkin
        {
            get
            {
                if (string.IsNullOrEmpty(_currentSkin))
                    _currentSkin = SkinManager.GetCurrentSkinName();
                
                return _currentSkin; 
            }
        }

        private Page _page;
        public Page Page
        {
            get
            {
                if (_page != null) return _page;
                if (CurrentAction == null) return null;
                if (CurrentAction.AppNode == null) return null;

                var pageHead = CurrentAction.AppNode;
                Node pageNode = null;
                try
                {
                    pageNode = Node.LoadNode(pageHead.Id);
                }
                catch (SenseNetSecurityException e) //logged
                {
                    Logger.WriteException(e);
                    AuthenticationHelper.DenyAccess(OwnerHttpContext.ApplicationInstance);
                }
                _page = pageNode as Page;

                //if (_page == null && pageNode != null)  // Some node is loaded, but that's not a page
                //{
                //    throw new ApplicationException(string.Format("A node was loaded from the given path ({0}), but that's not a Page (CLR type:{1}, NodeType:{2}).", pageHead.Path, pageNode.GetType().Name, pageNode.NodeType.Name));
                //}
                return _page;
            }
            set
            {
                _page = value;
            }
        }

        private string _contextNodePath;
        public String ContextNodePath
        {
            get
            {
                if (_contextNodePath != null)
                    return _contextNodePath;
                var c = ContextNodeHead;
                return c == null ? null : c.Path;
            }
        }
        private NodeHead _contextNodeHead;
        public NodeHead ContextNodeHead
        {
            get { return _contextNodeHead; }
        }
        public Node ContextNode
        {
            get { return LoadContextNode(_contextNodeHead, _versionRequest); }
        }
        private string _versionRequest;
        public string VersionRequest
        {
            get { return _versionRequest; }
            set { _versionRequest = value; }
        }
        private static Node LoadContextNode(NodeHead head, string versionRequest)
        {
            Node node = null;
            if (head == null)
                return null;

            //we need to user system account here during startup
            //because authentication occurs after this
            var changeToSystem = (User.Current.Id == RepositoryConfiguration.StartupUserId);

            try
            {
                if (changeToSystem)
                    AccessProvider.ChangeToSystemAccount();

                if (String.IsNullOrEmpty(versionRequest))
                {
                    node = Node.LoadNode(head.Id);
                }
                else
                {
                    VersionNumber version;
                    if (VersionNumber.TryParse(versionRequest, out version))
                        node = Node.LoadNode(head.Id, version);
                }
            }
            finally
            {
                if (changeToSystem)
                    AccessProvider.RestoreOriginalUser();
            }

            return node;
        }

        private string _actionName;
        public string ActionName { get { return _actionName; } }
        private string _appNodePath;
        public string ApplicationNodePath { get { return _appNodePath; } }

        /// <summary>
        /// Returns the current application if it is the requested node
        /// </summary>
        /// <returns></returns>
        public Node GetApplicationContext()
        {
            var context = ContextNode;

            //If the requested node is an app and the action 
            //is browse, than return that node.
            if (context is Page && (ActionName == null || ActionName.ToLower() == "browse"))
                return context;

            //If a context node was given in the url than
            //the original requested node should be an app.
            //In this case the page property will contain that content.
            var contextNodePath = HttpContext.Current.Request.Params[ContextNodeParamName];
            if (!string.IsNullOrEmpty(contextNodePath))
                return this.Page;

            return null;
        }

        public Node GetUrlParameterContext()
        {
            var contextNodePath = HttpContext.Current.Request.Params[ContextNodeParamName];
            
            return !string.IsNullOrEmpty(contextNodePath) ? Node.LoadNode(contextNodePath) : null;
        }

        public bool IsRequestedResourceExistInRepository
        {
            get { return _contextNodeHead != null; /*_isRequestedResourceExistInRepository;*/ }
        }

        private string _siteRelativePath;
        public string SiteRelativePath
        {
            get { return _siteRelativePath; }
        }

        public string GeneratedBackUrl
        {
            get
            {
                return string.Format("{0}={1}", BackUrlParamName, HttpUtility.UrlEncode(Current.RequestedUri.ToString()));
            }
        }

        public string BackUrl
        {
            get
            {
                var encodedBackUrl = HttpContext.Current.Request.QueryString[BackUrlParamName];
                
                return String.IsNullOrEmpty(encodedBackUrl) ? String.Empty : HttpUtility.UrlDecode(encodedBackUrl);
            }
        }

        public BackTargetType BackTarget
        {
            get
            {
                var backTarget = HttpContext.Current.Request.QueryString[BackTargetParamName];

                BackTargetType backValue;
                if (string.IsNullOrEmpty(backTarget) || !Enum.TryParse(backTarget, true, out backValue))
                    return BackTargetType.None;
                
                return backValue;
            }
        }
        
        public string UrlWithoutBackUrl
        {
            get
            {
                var fullUrl = RequestedUri.AbsoluteUri;
                if (!HttpContext.Current.Request.QueryString.AllKeys.Contains(BackUrlParamName))
                    return fullUrl;
                
                var backIndex = fullUrl.IndexOf(BackUrlParamName + "=") - 1;
                var nextIndex = fullUrl.IndexOfAny(new[] {'&', '?'}, backIndex + 1);

                fullUrl = nextIndex > -1 ? fullUrl.Remove(backIndex, nextIndex - backIndex) : fullUrl.Remove(backIndex);

                //replace the first '&' with '?' if necessary
                if (fullUrl.Contains("&") && !fullUrl.Contains("?"))
                {
                    var atIndex = fullUrl.IndexOf('&');
                    fullUrl = fullUrl.Remove(atIndex, 1).Insert(atIndex, "?");
                }

                return fullUrl;
            }
        }

        public string BackUrlTitle
        {
            get
            {
                var backurl = BackUrl;
                
                //remove '?back' or '&back' parameters to make the url readable
                var index = backurl.IndexOf(string.Format("?{0}=", BackUrlParamName));
                if (index > -1)
                    backurl = backurl.Remove(index);

                index = backurl.IndexOf(string.Format("&{0}=", BackUrlParamName));
                if (index > -1)
                    backurl = backurl.Remove(index);

                if (backurl.StartsWith("/"))
                    backurl = backurl.Remove(0, 1);

                return backurl;
            }
        }

        public NodeHead BinaryHandlerRequestedNodeHead { get; set; }

        public static string GetBackTargetUrl(Node newNode)
        {
            var bt = PortalContext.Current.BackTarget;
            
            return bt == BackTargetType.None ? string.Empty : GetBackTargetUrl(newNode, bt);
        }

        public static string GetBackTargetUrl(Node newNode, BackTargetType targetType)
        {
            if (PortalContext.Current == null)
                return string.Empty;

            var path = string.Empty;

            switch (targetType)
            {
                case BackTargetType.CurrentContent:
                    path = PortalContext.Current.ContextNodePath;
                    break;
                case BackTargetType.CurrentList:
                    var cl = ContentList.GetContentListForNode(PortalContext.Current.ContextNode);
                    if (cl != null) path = cl.Path;
                    break;
                case BackTargetType.CurrentWorkspace:
                    var cws = PortalContext.Current.ContextWorkspace;
                    if (cws != null) path = cws.Path;
                    break;
                case BackTargetType.CurrentSite:
                    path = PortalContext.Current.Site.Path;
                    break;
                case BackTargetType.CurrentPage:
                    var page = PortalContext.Current.Page;
                    if (page != null) path = page.Path;
                    break;
                case BackTargetType.CurrentUser:
                    path = User.Current.Path;
                    break;
                case BackTargetType.Parent:
                    var cn = PortalContext.Current.ContextNode;
                    if (cn != null) path = cn.ParentPath;
                    break;
                case BackTargetType.NewContent:
                    if (newNode != null)
                    {
                        if (newNode.IsNew)
                        {
                            //the node has not been saved yet (maybe the cancel button was pressed),
                            //we cannot redirect to it, so send the user to the parent instead
                            //path = PortalContext.Current.ContextNodePath;
                        }
                        else
                        {
                            path = newNode.Path;
                        }
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(path))
                path = PortalContext.GetSiteRelativePath(path);

            //if not found, return the back url
            if (targetType != BackTargetType.None && string.IsNullOrEmpty(path))
            {
                path = PortalContext.Current.BackUrl;

                //if it is empty, we have no other option than this (or maybe the parent)
                if (string.IsNullOrEmpty(path))
                    path = PortalContext.Current.ContextNodePath;
            }

            return path ?? string.Empty;
        }

        //------------------------------------------------------------------------ WebRoot files
        private static object _webRootFilesSync = new object();
        private static IEnumerable<string> __webRootFiles;
        private static IEnumerable<string> WebRootFiles 
        {
            get 
            {
                if (__webRootFiles == null)
                {
                    lock (_webRootFilesSync)
                    {
                        if (__webRootFiles == null)
                        {
                            var x = System.Configuration.ConfigurationManager.AppSettings["WebRootFiles"];
                            __webRootFiles = x == null ? new string[0] : x.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(n => string.Concat("/", n).ToLower());
                        }
                    }
                }
                return __webRootFiles;
            }
        }

        //------------------------------------------------------------------------ Allowed Arbitrary Content Type
        private static bool IsInAdminGroup(IUser user, IEnumerable<string> adminGroupPaths)
        {
            using (new SystemAccount())
            {
                foreach (var groupPath in adminGroupPaths)
                {
                    var node = Node.LoadNode(groupPath);
                    var container = node as ISecurityContainer;
                    if (container == null)
                        continue;
                    //if (user.IsInGroup(group))
                    //    return true;
                    if (user.IsInContainer(container))
                        return true;
                }
            }
            return false;
        }

        private static object _allowedContentTypesAdminGroupPathsSync = new object();
        private static IEnumerable<string> __allowedContentTypesAdminGroupPaths;
        private static IEnumerable<string> AllowedContentTypesAdminGroupPaths
        {
            get
            {
                if (__allowedContentTypesAdminGroupPaths == null)
                {
                    lock (_allowedContentTypesAdminGroupPathsSync)
                    {
                        if (__allowedContentTypesAdminGroupPaths == null)
                        {
                            var x = System.Configuration.ConfigurationManager.AppSettings["AdminGroupPathsForAllowedContentTypes"];
                            __allowedContentTypesAdminGroupPaths = x == null ? new string[0] : x.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                    }
                }
                return __allowedContentTypesAdminGroupPaths;
            }
        }

        private bool? _arbitraryContentTypeCreationAllowed;
        public bool ArbitraryContentTypeCreationAllowed
        {
            get
            {
                if (!_arbitraryContentTypeCreationAllowed.HasValue)
                    _arbitraryContentTypeCreationAllowed = PortalContext.IsInAdminGroup(User.Current, AllowedContentTypesAdminGroupPaths);
                return _arbitraryContentTypeCreationAllowed.Value;
            }
        }


        //------------------------------------------------------------------------ Logged-in User Cache

        private static object _adminGroupPathsSync = new object();
        private static IEnumerable<string> __adminGroupPaths;
        private static IEnumerable<string> AdminGroupPaths
        {
            get
            {
                if (__adminGroupPaths == null)
                {
                    lock (_adminGroupPathsSync)
                    {
                        if (__adminGroupPaths == null)
                        {
                            var x = System.Configuration.ConfigurationManager.AppSettings["AdminGroupPathsForLoggedInUserCache"];
                            __adminGroupPaths = x == null ? new string[0] : x.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                    }
                }
                return __adminGroupPaths;
            }
        }

        private bool? _loggedInUserCacheEnabled;
        public bool LoggedInUserCacheEnabled
        {
            get
            {
                if (!_loggedInUserCacheEnabled.HasValue)
                    _loggedInUserCacheEnabled = !PortalContext.IsInAdminGroup(User.Current, AdminGroupPaths);
                return _loggedInUserCacheEnabled.Value;
            }
        }


        //======================================================================== Creation

        private PortalContext() { }
        private void Initialize(HttpContext context, PortalContextInitInfo initInfo)
        {
            _ownerHttpContext = context;
            // use absolute uri to clone. requesturi.tostring messes up encoded parts, like backurl
            _originalUri = new Uri(initInfo.RequestUri.AbsoluteUri.ToString()); // clone
            _isWebdavRequest = initInfo.IsWebdavRequest;

            _site = initInfo.RequestedSite;
            _siteUrl = initInfo.SiteUrl;
            _siteRelativePath = initInfo.SiteRelativePath;
            _repositoryPath = initInfo.RepositoryPath;

            _actionName = initInfo.ActionName;
            _appNodePath = initInfo.AppNodePath;
            _contextNodePath = initInfo.ContextNodePath;
            _versionRequest = initInfo.VersionRequest;

            _deviceName = initInfo.DeviceName;

            if (_contextNodePath == null)
            {
                _contextNodeHead = initInfo.RequestedNodeHead;
            }
            else
            {
                _contextNodeHead = NodeHead.Get(initInfo.ContextNodePath);
                _appNodePath = initInfo.RequestedNodeHead.Path;
            }

            //if (_siteUrl != null)
            //    _authenticationMode = _site.UrlList[siteUrl];
            _authenticationMode = GetCurrentAuthenticationMode();

            if (_contextNodeHead != null /*_isRequestedResourceExistInRepository*/)
            {
                _nodeType = initInfo.RequestedNodeHead.GetNodeType();
                _nodeId = initInfo.RequestedNodeHead.Id;
            }

            //_queryStringNodePropertyName = HttpContext.Current.Request.QueryString[QUERYSTRING_NODEPROPERTY_KEY];
            _queryStringNodePropertyName = context.Request.QueryString[QUERYSTRING_NODEPROPERTY_KEY];
            if (_queryStringNodePropertyName != null)
                _queryStringNodePropertyName = _queryStringNodePropertyName.Replace('$', '#');

            BinaryHandlerRequestedNodeHead = initInfo.BinaryHandlerRequestedNodeHead;
        }

        //------------------------------------------------------------------------ ApplicationModel

        private static string _presenterFolderName = null;
        private string PresenterFolderName
        {
            get
            {
                if (_presenterFolderName == null)
                    _presenterFolderName = ConfigurationManager.AppSettings["PresenterFolderName"] ?? "(apps)";
                return _presenterFolderName;
            }
        }

        private static string _presenterPagePostfix = null;
        private string PresenterPagePostfix
        {
            get
            {
                if (_presenterPagePostfix == null)
                    _presenterPagePostfix = ConfigurationManager.AppSettings["PresenterPagePostfix"] ?? String.Empty;
                return _presenterPagePostfix;
            }
        }

        private string GetPresenterViewLocations(string nodePath, NodeType nodeType)
        {
            var action = HttpContext.Current.Request.Params[ActionParamName] ?? String.Empty;
            if (action.Length > 0)
                action = String.Concat("/", action, PresenterPagePostfix);

            string[] parts = nodePath.Split('/');

            var probs = new List<string>();

            var nt = nodeType;
            while (nt != null)
            {
                probs.Add(String.Concat("/{0}/", nt.Name, "{1}", action));
                nt = nt.Parent;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<r>");
            sb.Append("<p>").Append(nodePath).Append("/").Append(PresenterFolderName).Append("/This").Append(PresenterPagePostfix).Append(action).Append("</p>");

            var position = parts.Length + 1;
            string partpath;
            while (position-- > 2)
            {
                partpath = string.Join("/", parts, 0, position);
                foreach (var prob in probs)
                    sb.AppendFormat("<p>{0}{1}</p>", partpath, string.Format(prob, PresenterFolderName, PresenterPagePostfix));
            }
            partpath = "/Root/System";
            foreach (var prob in probs)
                sb.AppendFormat("<p>{0}{1}</p>", partpath, string.Format(prob, PresenterFolderName, PresenterPagePostfix));

            sb.Append("</r>");
            return sb.ToString();
        }

        //------------------------------------------------------------------------

        public string GetLoginPageUrl()
        {
            // examples:
            // _siteUrl:                       localhost:1315/beerco
            // loginPageRepositoryPath:        /Root/XY Site/Login
            // sitePath:                       /Root/XY Site
            // loginPageRelativePath:          /Login 
            // loginPageUrl:                   http(s)://localhost:1315/xy/Login

            if (Site == null)
                return null;

            if (Site.LoginPage == null)
                return null;

            string loginPageRepositoryPath = Site.LoginPage.Path;
            string sitePath = Site.Path;

            string loginPageRelativePath = loginPageRepositoryPath.Substring(sitePath.Length);

            string loginPageUrl = string.Concat(OriginalUri.Scheme, "://", _siteUrl, loginPageRelativePath);

            return loginPageUrl;
        }

        internal void BackwardCompatibility_SetPageRepositoryPath(string path)
        {
//            _pageRepositoryPath = path;
        }

        //------------------------------------------------------------------------

        public const string DEVICEPARAM = "device";
        public static string GetRequestedDevice()
        {
            if (HttpContext.Current == null)
                return null;

            var request = HttpContext.Current.Request;

            var device = request[DEVICEPARAM];
            if (!String.IsNullOrEmpty(device))
                return device;

            device = DeviceManager.GetRequestedDeviceName(request.UserAgent);
            if (device != null)
                return device;
            return HttpContext.Current.Request.Browser.Browser;
        }
    }
}
