using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using SenseNet.ContentRepository.Storage.ApplicationMessaging;
using SenseNet.ContentRepository.Storage;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using SenseNet.Portal.AppModel;
using System.Diagnostics;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Data;
using System.Threading;
using System.Configuration;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.Virtualization
{
    public class PortalContextModule : IHttpModule
    {
        // ============================================================================================ Members
        private static Dictionary<string, int> _clientCacheConfig;
        public static Dictionary<string, int> ClientCacheConfig
        { 
            get { return _clientCacheConfig; }
        }
        private static int? _binaryHandlerClientCacheMaxAge;


        // ============================================================================================ Properties
        private static readonly string DENYCROSSSITEACCESSENABLEDKEY = "DenyCrossSiteAccessEnabled";
        private static bool? _denyCrossSiteAccessEnabled = null;
        public static bool DenyCrossSiteAccessEnabled
        {
            get
            {
                if (!_denyCrossSiteAccessEnabled.HasValue)
                {
                    var section = ConfigurationManager.GetSection(Repository.PORTALSECTIONKEY) as System.Collections.Specialized.NameValueCollection;
                    if (section != null)
                    {
                        var valStr = section[DENYCROSSSITEACCESSENABLEDKEY];
                        if (!string.IsNullOrEmpty(valStr))
                        {
                            bool val;
                            if (bool.TryParse(valStr, out val))
                                _denyCrossSiteAccessEnabled = val;
                        }
                    }
                    if (!_denyCrossSiteAccessEnabled.HasValue)
                        _denyCrossSiteAccessEnabled = true;
                }
                return _denyCrossSiteAccessEnabled.Value;
            }
        }


        // ============================================================================================ IHttpModule
        public void Init(HttpApplication context)
        {
            InitCacheHeaderConfig();
            context.BeginRequest += new EventHandler(OnEnter);
        }
        void OnEnter(object sender, EventArgs e)
        {
            HttpContext httpContext = (sender as HttpApplication).Context;
            var request = httpContext.Request;


            //trace
            bool traceReportEnabled;
            var traceQueryString = request.QueryString["trace"];

            if (!String.IsNullOrEmpty(traceQueryString))
                traceReportEnabled = (traceQueryString == "true") ? true : false;
            else
                traceReportEnabled = RepositoryConfiguration.TraceReportEnabled;

            if (traceReportEnabled)
            {
                var slot = Thread.GetNamedDataSlot(Tracing.OperationTraceDataSlotName);
                var data = Thread.GetData(slot);

                if (data == null)
                    Thread.SetData(slot, new OperationTraceCollector());
            }
            //trace


            var initInfo = PortalContext.CreateInitInfo(httpContext);

            // check if request came to a restricted site via another site
            if (DenyCrossSiteAccessEnabled)
            {
                if (initInfo.RequestedNodeSite != null && initInfo.RequestedSite != null)
                {
                    if (initInfo.RequestedNodeSite.DenyCrossSiteAccess && initInfo.RequestedSite.Id != initInfo.RequestedNodeSite.Id)
                    {
                        HttpContext.Current.Response.StatusCode = 404;
                        HttpContext.Current.Response.Flush();
                        HttpContext.Current.Response.End();
                        return;
                    }
                }
            }

            // add cache-control headers and handle ismodifiedsince requests
            HandleResponseForClientCache(initInfo);


            PortalContext portalContext = PortalContext.Create(httpContext, initInfo);

            SetThreadCulture(portalContext);

            var action = HttpActionManager.CreateAction(portalContext);
            Logger.WriteVerbose("HTTP Action.", CollectLoggedProperties, portalContext);

            action.Execute();
        }
        public void Dispose()
        {
        }


        // ============================================================================================ Methods
        private static void SetThreadCulture(PortalContext portalContext)
        {
            // Set the CurrentCulture and the CurrentUICulture of the current thread based on the site language.
            // If the site language was set to "UserDefined", or was set to an empty value, the thread culture
            // remain unmodified and will contain its default value (based on Web- and machine.config).
            if (portalContext.Site != null)
            {
                string language = portalContext.Site.Language;
                // If the language was set to a non-empty value, and was not set to fallback, set the thread locale
                // Otherwise do nothing (the ASP.NET engine already set the locale).
                if (!string.IsNullOrEmpty(language) && string.Compare(language, "FallbackToDefault", true) != 0)
                {
                    var specificCulture = System.Globalization.CultureInfo.CreateSpecificCulture(language);
                    var currentThread = System.Threading.Thread.CurrentThread;
                    currentThread.CurrentCulture = specificCulture;
                    currentThread.CurrentUICulture = specificCulture;
                }
            }
        }
        private static IDictionary<string, object> CollectLoggedProperties(IHttpActionContext context)
        {
            var action = context.CurrentAction;
            var props = new Dictionary<string, object>
                {
                    {"ActionType", action.GetType().Name},
                    {"TargetNode",  action.TargetNode == null ? "[null]" : action.TargetNode.Path},
                    {"AppNode",  action.AppNode == null ? "[null]" : action.AppNode.Path}
                };

            if (action is DefaultHttpAction)
            {
                props.Add("RequestUrl", context.RequestedUrl);
                return props;
            }
            var redirectAction = action as RedirectHttpAction;
            if (redirectAction != null)
            {
                props.Add("TargetUrl", redirectAction.TargetUrl);
                props.Add("EndResponse", redirectAction.EndResponse);
                return props;
            }
            var remapAction = action as RemapHttpAction;
            if (remapAction != null)
            {
                if (remapAction.HttpHandlerType != null)
                    props.Add("HttpHandlerType", remapAction.HttpHandlerType.Name);
                else
                    props.Add("HttpHandlerNode", remapAction.HttpHandlerNode.Path);
                return props;
            }
            var rewriteAction = action as RewriteHttpAction;
            if (rewriteAction != null)
            {
                props.Add("Path", rewriteAction.Path);
                return props;
            }
            return props;
        }
        private void InitCacheHeaderConfig()
        {
            // ClientCacheHeaders
            string clientCacheSettings = ConfigurationManager.AppSettings["ClientCacheHeaders"];
            if (!string.IsNullOrEmpty(clientCacheSettings))
            {
                try
                {
                    var configuration = new Dictionary<string, int>();
                    foreach (string configElement in clientCacheSettings.Split(";".ToCharArray()))
                    {
                        string[] subElements = configElement.Split("=".ToCharArray());
                        configuration.Add(subElements[0], Int32.Parse(subElements[1]));
                    }
                    _clientCacheConfig = configuration;
                }
                catch (Exception ex) //rethrow
                {
                    Logger.WriteError(ex);
                    throw new ConfigurationErrorsException("Invalid client cache configuration. Check the ClientCacheHeaders section in the AppSettings.", ex);
                }
            }

            // BinaryHandlerClientCacheMaxAge
            int binaryHandlerClientCacheMaxAge;
            if (Int32.TryParse(ConfigurationManager.AppSettings["BinaryHandlerClientCacheMaxAge"], out binaryHandlerClientCacheMaxAge))
                _binaryHandlerClientCacheMaxAge = binaryHandlerClientCacheMaxAge;
        }
        private void HandleResponseForClientCache(PortalContextInitInfo initInfo)
        {
            var context = HttpContext.Current;

            // binaryhandler
            if (_binaryHandlerClientCacheMaxAge.HasValue && initInfo.BinaryHandlerRequestedNodeHead != null)
            {
                HttpHeaderTools.SetCacheControlHeaders(_binaryHandlerClientCacheMaxAge.Value);

                // handle is-modified-since requests only for requests coming from proxy
                if (PortalContext.ProxyIPs.Contains(context.Request.UserHostAddress))
                    HttpHeaderTools.EndResponseForClientCache(initInfo.BinaryHandlerRequestedNodeHead.ModificationDate);
                return;
            }

            // images, and other content requested with their path (e.g. /Root/Global/images/myimage.png)
            string extension = System.IO.Path.GetExtension(context.Request.Url.AbsolutePath).ToLower();
            if (_clientCacheConfig != null && _clientCacheConfig.ContainsKey(extension))
            {
                // get requested nodehead
                if (initInfo.RequestedNodeHead == null)
                    return;

                int seconds = _clientCacheConfig[extension];
                HttpHeaderTools.SetCacheControlHeaders(seconds);

                // handle is-modified-since requests only for requests coming from proxy
                if (PortalContext.ProxyIPs.Contains(context.Request.UserHostAddress))
                    HttpHeaderTools.EndResponseForClientCache(initInfo.RequestedNodeHead.ModificationDate);

                return;
            }

            // applications
            if (initInfo.RequestedNodeHead != null)
            {
                Application app = null;
                // elevate to sysadmin, as we are startupuser here, and group 'everyone' should have permissions to application without elevation
                using (new SystemAccount())
                {
                    app = ApplicationStorage.Instance.GetApplication(string.IsNullOrEmpty(initInfo.ActionName) ? "browse" : initInfo.ActionName, initInfo.RequestedNodeHead, initInfo.DeviceName);
                }
                if (app != null)
                {
                    var maxAge = app.NumericMaxAge;
                    var cacheControl = app.CacheControlEnumValue;

                    if (cacheControl.HasValue && maxAge.HasValue)
                    {
                        HttpHeaderTools.SetCacheControlHeaders(maxAge.Value, cacheControl.Value);

                        if (PortalContext.ProxyIPs.Contains(context.Request.UserHostAddress))
                            HttpHeaderTools.EndResponseForClientCache(initInfo.RequestedNodeHead.ModificationDate);
                    }

                    return;
                }
            }
        }
     }
}
