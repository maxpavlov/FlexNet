//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web;

//namespace SenseNet.Services.SupportTypes
//{

//    internal class SvcEmulationModule : IHttpModule
//    {
//        private static readonly string CONTENTSTORE_SVC = "/ContentStore.svc/";

//        #region IHttpModule Members

//        public void Dispose()
//        {
//        }

//        public void Init(HttpApplication context)
//        {
//            context.BeginRequest += new EventHandler(BeginRequest);
//        }

//        void BeginRequest(object sender, EventArgs e)
//        {
//            HttpApplication application = sender as HttpApplication;
//            HttpContext httpContext = application.Context;

//            //treeLoaderUrl: '/ContentStore.svc/GetFeed2?onlyFolders=true&start=0&limit=0',
//            //gridLoaderUrl: '/ContentStore.svc/GetItem2?node={0}&onlyFileChildren=false',
//            //copyUrl: '/ContentStore.svc/Copy?node={0}&target={1}',
//            //moveUrl: '/ContentStore.svc/Move?node={0}&target={1}',
//            //deleteUrl: '/ContentStore.svc/Delete?node={0}',
//            //getContentTypeNamesUrl: '/ContentStore.svc/GetContentTypes',
//            //searchUrl: '/ContentStore.svc/Search?expr={0}'

//            // Map REST handler
//            string requestPath = httpContext.Request.Path;
//            int pos = requestPath.IndexOf(CONTENTSTORE_SVC);

//            if (pos > -1)
//            {
//                // The "command" handled other way in the .ashx handler, emulate that
//                string command = requestPath.Substring(pos + CONTENTSTORE_SVC.Length);
                
//                // Create and map a handler
//                // The other parameters are the same (node={0}, etc) - no other tricks needed
//                RESTHandler restHandlerInstance = new RESTHandler(command);
//                httpContext.RemapHandler(restHandlerInstance);
//            }
//        }

//        #endregion
//    }


//}
