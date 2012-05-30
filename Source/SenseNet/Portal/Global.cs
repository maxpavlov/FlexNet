using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using System.Web.Routing;
using System.Web.Mvc;
using System.Collections.Generic;
using SenseNet.Search.Indexing;
using SenseNet.Portal.Virtualization;
using System.Net;
using File = System.IO.File;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Data;
using System.Linq;

namespace SenseNet.Portal
{
    public class Global : System.Web.HttpApplication
    {
        internal static string RunOnceGuid = "101C50EF-24FD-441A-A15B-BD33DE431665";

        //protected void Application_Start(object sender, EventArgs e)
        //{
        //    //
        //    //  Important: leave this comment here
        //    //  After restarting the AppDomain (e.g. IISReset), most of the assemblies are missing from AppDomain because .NET doesn't load the plugin/third-party
        //    //  assemblies. The BuildManager.GetReferencedAssemblies() call ensures that all assemblies are loaded into the AppDomain
        //    //  even with no references to any project assemblies.
        //    //  Idea comes from: ASP.NET MVC 2.0 source code - AreaRegistration.RegisterAllAreas() call.
        //    //

        //    System.Web.Compilation.BuildManager.GetReferencedAssemblies();


        //    //delete write.lock if necessary
        //    var lockFilePath = Path.Combine(StorageContext.Search.IndexDirectoryPath, "write.lock");
        //    if (File.Exists(lockFilePath))
        //    {
        //        var retryIntervalString = ConfigurationManager.AppSettings["LuceneLockDeleteRetryInterval"];
        //        var retryInterval = 5;
        //        if (!string.IsNullOrEmpty(retryIntervalString))
        //            int.TryParse(retryIntervalString, out retryInterval);

        //        var endRetry = DateTime.Now.AddMinutes(retryInterval);

        //        //retry for a given period of time if something locks the file
        //        while (DateTime.Now < endRetry)
        //        {
        //            try
        //            {
        //                File.Delete(lockFilePath);
        //                break;
        //            }
        //            catch (Exception ex)
        //            {
        //                //we can't use the logging mechanism here - yet
        //                //Logger.WriteException(ex);
        //            }
        //        }
        //    }

        //    //start Lucene if install process is finished - don't remove this
        //    var runOnceMarkerPath = Server.MapPath("/" + RunOnceGuid);

        //    try
        //    {
        //        if (!File.Exists(runOnceMarkerPath))
        //        {
        //            var reader = LuceneManager.IndexCount;
        //        }
        //    }
        //    catch (TypeInitializationException ex)
        //    {
        //        //if LuceneManager failed to initialize because we are installed
        //        //into a virtual folder (which is not permitted for now), than
        //        //return and present an info page (this is done in Default.aspx)
        //        //Otherwise rethrow the exception.
        //        if (!PortalContext.IsWebSiteRoot)
        //            return;

        //        throw new ApplicationException("LuceneManager failed to initialize", ex);
        //    }

        //    // 
        //    //  There is no need dummy reference in IIS scenario.
        //    //  var dummy3 = typeof(SenseNet.Portal.Portlets.ContentEditorPortlet);
        //    //

        //    RegisterEventTracers();
        //    RegisterRoutes(RouteTable.Routes);
        //    SenseNet.Portal.Virtualization.RepositoryPathProvider.Register();
        //}
        private static readonly int[] dontCareErrorCodes = new int[] { 401, 403, 404 };


        /* ============================================================================================================ static handlers */
        protected static void ApplicationStartHandler(object sender, EventArgs e, HttpApplication application)
        {
            var runOnceMarkerPath = application.Server.MapPath("/" + RunOnceGuid);
            var firstRun = File.Exists(runOnceMarkerPath);
            var startConfig = new SenseNet.ContentRepository.RepositoryStartSettings { StartLuceneManager = !firstRun };

            RepositoryInstance.WaitForWriterLockFileIsReleased(RepositoryInstance.WaitForLockFileType.OnStart);

            SenseNet.ContentRepository.Repository.Start(startConfig);

            //-- <L2Cache>
            StorageContext.L2Cache = new L2CacheImpl();
            //-- </L2Cache>

            RegisterRoutes(RouteTable.Routes);
            SenseNet.Portal.Virtualization.RepositoryPathProvider.Register();
        }
        protected static void ApplicationEndHandler(object sender, EventArgs e, HttpApplication application)
        {
            //LuceneManager.ShutDown();
            SenseNet.ContentRepository.Repository.Shutdown();
            Logger.WriteInformation("Application_End");
        }
        protected static void ApplicationErrorHandler(object sender, EventArgs e, HttpApplication application)
        {
            var ex = application.Server.GetLastError();
            var httpException = ex as HttpException;

            int? originalHttpCode = null;
            if (httpException != null)
                originalHttpCode = httpException.GetHttpCode();

            // if httpcode is contained in the dontcare list (like 404), don't log the exception
            var skipLogException = originalHttpCode.HasValue && dontCareErrorCodes.Contains(originalHttpCode.Value);

            if (!skipLogException)
            {
                try
                {
                    Logger.WriteException(ex);
                }
                catch
                {
                }
            }

            if (ex.InnerException != null && ex.InnerException.StackTrace != null &&
              (ex.InnerException.StackTrace.IndexOf("System.Web.UI.PageParser.GetCompiledPageInstanceInternal") != -1))
                return;

            if (HttpContext.Current == null)
                return;

            HttpResponse response;
            try
            {
                response = HttpContext.Current.Response;
            }
            catch (Exception)
            {
                response = null;
            }



            // HACK: HttpAction.cs (and possibly StaticFileHandler) throws 404 and 403 HttpExceptions. 
            // These are not exceptions to be displayed, but "fake" exceptions to handle 404 and 403 requests.
            // Therefore, here we set the statuscode and return, no further logic is executed.
            //var msg = ex.Message ?? string.Empty;
            //if (msg.StartsWith("Not found") || msg.StartsWith("Forbidden") || msg == "File does not exist.")
            if (originalHttpCode.HasValue && (originalHttpCode == 404 || originalHttpCode == 403))
            {
                response.StatusCode = originalHttpCode.Value;

                HttpContext.Current.ClearError();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
                return;
            }


            var errorPageHtml = string.Empty;

            var exception = ex;
            if (exception.InnerException != null) exception = exception.InnerException;

            var exceptionStatusCode = 0;
            var exceptionSubStatusCode = 0;
            var statusCodeExists = Global.GetStatusCode(exception, out exceptionStatusCode, out exceptionSubStatusCode);

            if (response != null)
            {

                if (!HttpContext.Current.Request.Url.AbsoluteUri.StartsWith("http://localhost"))
                {
                    if (originalHttpCode.HasValue)
                        response.StatusCode = originalHttpCode.Value;
                    
                    // If there is a specified status code in statusCodeString then set Response.StatusCode to it.
                    // Otherwise go on to global error page.
                    if (statusCodeExists)
                    {
                        application.Response.StatusCode = exceptionStatusCode;
                        application.Response.SubStatusCode = exceptionSubStatusCode;
                        response.Clear();
                        HttpContext.Current.ClearError();
                        //response.End();
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                        return;
                    }

                    application.Response.TrySkipIisCustomErrors = true; // keeps our custom error page defined below instead of using the page of IIS - works in IIS7 only

                    if (application.Response.StatusCode == 200)
                        application.Response.StatusCode = 500;

                    var path = String.Concat("/Root/System/ErrorMessages/", Portal.Site.Current.Name, "/UserGlobal.html");

                    var globalErrorNode = Node.LoadNode(path);

                    if (globalErrorNode != null)
                    {
                        var globalBinary = globalErrorNode.GetBinary("Binary");
                        var stream = globalBinary.GetStream();
                        if (stream != null)
                        {
                            var str = new StreamReader(stream);
                            errorPageHtml = str.ReadToEnd();
                        }
                    }
                    else
                    {
                        //Logger.WriteException(exc);
                        errorPageHtml = GetDefaultUserErrorPageHtml(application.Server.MapPath("/"), true);
                    }
                }
                else
                {
                    // if the page is requested from localhost
                    errorPageHtml = GetDefaultLocalErrorPageHtml(application.Server.MapPath("/"), true);
                }
            }
            else
            {
                // TODO: SQL Error handling
                //errorPageHtml = GetDefaultLocalErrorPageHtml(Server.MapPath("/"), false);

                //errorPageHtml = InsertErrorMessagesIntoHtml(exception, errorPageHtml);
            }

            errorPageHtml = InsertErrorMessagesIntoHtml(exception, errorPageHtml);

            application.Response.TrySkipIisCustomErrors = true;

            // If there is a specified status code in statusCodeString then set Response.StatusCode to it.
            // Otherwise go on to global error page.
            if (statusCodeExists)
            {
                application.Response.StatusCode = exceptionStatusCode;
                application.Response.SubStatusCode = exceptionSubStatusCode;
                response.Clear();
                HttpContext.Current.ClearError();
                //response.End();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            else
            {
                if (application.Response.StatusCode == 200)
                    application.Response.StatusCode = 500;
            }


            //
            //  If the ContentStore service throws an excepcion inside itself, the StatusCode will be set to 200
            //  which is not a valid process within Ajax request. We need to change the StatusCode 200 to 500 (Internal Server Error)
            //  for catching exception in the client side.
            //
            //var statusCode = Response.StatusCode;
            //if (statusCode == 200)
            //    Response.StatusCode = 500;

            //if (Response.StatusCode == 500)
            //{
            //    Response.StatusCode = 200;
            //}

            if (response != null)
            {
                response.Clear();
                response.Write(errorPageHtml);
            }

            HttpContext.Current.ClearError();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }


        /* ============================================================================================================ event handlers */
        protected void Application_Start(object sender, EventArgs e)
        {
            Global.ApplicationStartHandler(sender, e, this);
        }
        protected void Application_End(object sender, EventArgs e)
        {
            Global.ApplicationEndHandler(sender, e, this);
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Global.ApplicationErrorHandler(sender, e, this);
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

            //if (Request.HttpMethod == "GET")
            //{
            //    if (Request.AppRelativeCurrentExecutionFilePath.EndsWith(".aspx"))
            //    {
            //        Response.Filter = new ScriptDeferFilter(Response);
            //    }
            //}

            //
            //  TODO: after view infrastructure is multilingual, uncomment the following 2 rows.
            //

            //if (System.Threading.Thread.CurrentThread.CurrentUICulture.IsNeutralCulture)
            //    System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(System.Threading.Thread.CurrentThread.CurrentUICulture.Name);

            //System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
        }


        /* ============================================================================================================ helpers */
        private static bool GetStatusCode(Exception exception, out int exceptionStatusCode, out int exceptionSubStatusCode)
        {
            exceptionStatusCode = 0;
            exceptionSubStatusCode = 0;

            var statusCodes = ConfigurationManager.GetSection("ExceptionStatusCodes") as NameValueCollection;
            if (statusCodes == null) return false;

            var tmpExceptionFullName = exception.GetType().FullName;
            var tmpException = exception.GetType();

            while (tmpExceptionFullName != "System.Exception")
            {
                if (tmpExceptionFullName != null)
                {
                    var statusCodeFullString = statusCodes[tmpExceptionFullName];
                    if (!string.IsNullOrEmpty(statusCodeFullString))
                    {
                        string statusCodeString;
                        string subStatusCodeString;

                        if (statusCodes[tmpExceptionFullName].Contains("."))
                        {
                            statusCodeString = statusCodeFullString.Split('.')[0];
                            subStatusCodeString = statusCodeFullString.Split('.')[1];
                        }
                        else
                        {
                            statusCodeString = statusCodeFullString;
                            subStatusCodeString = "0";
                        }

                        if (Int32.TryParse(statusCodeString, out exceptionStatusCode) && Int32.TryParse(subStatusCodeString, out exceptionSubStatusCode))
                            return true;
                        return false;
                    }

                    if (tmpException != null) tmpException = tmpException.BaseType;
                    if (tmpException != null) tmpExceptionFullName = tmpException.FullName;
                }

                return false;
            }

            return false;
        }
        private static string InsertErrorMessagesIntoHtml(Exception exception, string errorPageHtml)
        {
            errorPageHtml = errorPageHtml.Replace("{exceptionType}", exception.GetType().ToString());
            errorPageHtml = errorPageHtml.Replace("{exceptionMessage}", exception.Message.Replace("\n", "<br />"));
            errorPageHtml = errorPageHtml.Replace("{exceptionToString}", exception.ToString().Replace("\n", "<br />"));
            errorPageHtml = errorPageHtml.Replace("{exceptionSource}", exception.Source.ToString().Replace("\n", "<br />"));
            errorPageHtml = errorPageHtml.Replace("{exceptionStackTrace}", exception.StackTrace.ToString());

            var unknownActionExc = exception as UnknownActionException;
            if (unknownActionExc != null)
            {
                errorPageHtml = errorPageHtml.Replace("{exceptionActionName}", unknownActionExc.ActionName);
            }

            return errorPageHtml;
        }
        /// <summary>
        /// Gets the default user error page HTML.
        /// </summary>
        /// <param name="p">The server path.</param>
        /// <returns></returns>
        private static string GetDefaultUserErrorPageHtml(string p, bool tryOnline)
        {
            return GetDefaultErrorPageHtml(p, "UserGlobal.html", "UserErrorPage.html", tryOnline);
        }
        /// <summary>
        /// Gets the default local error page HTML.
        /// </summary>
        /// <param name="p">The server path.</param>
        /// <returns></returns>
        private static string GetDefaultLocalErrorPageHtml(string p, bool tryOnline)
        {
            return GetDefaultErrorPageHtml(p, "Global.html", "ErrorPage.html", tryOnline);
        }
        public static void RegisterRoutes(RouteCollection routes)
        {
            var engine = (WebFormViewEngine)ViewEngines.Engines[0];
            engine.ViewLocationFormats = new[] {
                "~/root/MvcViews/{1}/{0}.aspx",
                "~/root/MvcViews/{1}/{0}.ascx",
                "~/root/MvcViews/Shared/{0}.aspx",
                "~/root/MvcViews/Shared/{0}.ascx"
            };

            engine.MasterLocationFormats = new[] {
                "~/root/MvcViews/{1}/{0}.master",
                "~/root/MvcViews/Shared/{0}.master"
            };

            engine.PartialViewLocationFormats = engine.ViewLocationFormats;

            routes.MapRoute(
              "Default", // Route name
              "{controller}.mvc/{action}/{pid}", // URL with parameters
              new { controller = "Home", action = "Index", pid = "" } // Parameter defaults
            );
        }
        /// <summary>
        /// Gets the default error page HTML.
        /// </summary>
        /// <param name="serverMapPath">The server map path.</param>
        /// <param name="page">The page name in Content Repository.</param>
        /// <param name="offlinePage">The offline page name in file system.</param>
        /// <returns></returns>
        private static string GetDefaultErrorPageHtml(string serverMapPath, string page, string offlinePage, bool tryOnline)
        {
            Node global = null;

            if (tryOnline)
            {
                global = Node.LoadNode(String.Concat("/Root/System/ErrorMessages/", Portal.Site.Current.Name + "/", page)) as Node ??
                             Node.LoadNode(String.Concat("/Root/System/ErrorMessages/Default/", page)) as Node;
            }

            if (global != null)
            {
                var globalBinary = global.GetBinary("Binary");
                var stream = globalBinary.GetStream() as Stream;
                if (stream != null)
                {
                    var str = new StreamReader(stream);
                    return str.ReadToEnd();
                }
            }
            else
            {
                try
                {
                    //string path = String.Concat(serverMapPath, ConfigurationManager.AppSettings["ErrorPage"]);
                    var path = String.Concat(serverMapPath, offlinePage);
                    using (var fs = System.IO.File.Open(path, System.IO.FileMode.Open, FileAccess.Read))
                    {
                        using (var sr = new StreamReader(fs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
                catch (Exception exc) //logged
                {
                    Logger.WriteException(exc);
                }
            }

            return "<html><head><title>{exceptionType}</title></head><body style=\"font-family:Consolas, 'Courier New', Courier, monospace; background-color:#0033CC;color:#CCCCCC; font-weight:bold\"><br /><br /><br /><div style=\"text-align:center;background-color:#CCCCCC;color:#0033CC\">{exceptionType}</div><br /><br /><div style=\"font-size:large\">{exceptionMessage}<br /></div><br /><div style=\"font-size:x-small\">The source of the exception: {exceptionSource}</div><br /><div style=\"font-size:x-small\">Output of the Exception.ToString():<br />{exceptionToString}<br /><br /></div></body></html>";
        }


        //////////////////////////////////////// Event Tracers ////////////////////////////////////////

        //private void RegisterEventTracers()
        //{
        //    // Event Tracing
        //    AppDomain appDomain = AppDomain.CurrentDomain;
        //    //appDomain.AssemblyLoad += new AssemblyLoadEventHandler(Domain_AssemblyLoad);
        //    //appDomain.AssemblyResolve += new ResolveEventHandler(Domain_AssemblyResolve);
        //    //appDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(Domain_ReflectionOnlyAssemblyResolve);
        //    //appDomain.ResourceResolve += new ResolveEventHandler(Domain_ResourceResolve);
        //    //appDomain.TypeResolve += new ResolveEventHandler(Domain_TypeResolve);
        //    appDomain.UnhandledException += new UnhandledExceptionEventHandler(Domain_UnhandledException);
        //}

        //private void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    Logger.WriteCritical("Domain_UnhandledException", Logger.GetDefaultProperties, e.ExceptionObject);
        //}

        //private System.Reflection.Assembly Domain_TypeResolve(object sender, ResolveEventArgs args)
        //{
        //    Logger.WriteVerbose("Domain_TypeResolve: " + args.Name);
        //    return null;
        //}

        //private System.Reflection.Assembly Domain_ResourceResolve(object sender, ResolveEventArgs args)
        //{
        //    Logger.WriteVerbose("Domain_ResourceResolve: " + args.Name);
        //    return null;
        //}

        //private System.Reflection.Assembly Domain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    Logger.WriteVerbose("Domain_ReflectionOnlyAssemblyResolve: " + args.Name);
        //    return null;
        //}

        //private System.Reflection.Assembly Domain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    Logger.WriteVerbose("Domain_AssemblyResolve: " + args.Name);
        //    return null;
        //}

        //private void Domain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        //{
        //    Logger.WriteVerbose("Domain_AssemblyLoad: " + args.LoadedAssembly.FullName);
        //}


    }
}
