//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web;
//using System.Runtime.Serialization.Json;
//using System.IO;
//using SenseNet.ContentRepository.Storage.Schema;
//using SenseNet.ContentRepository.Storage.Search;
//using SenseNet.ContentRepository.Storage;

//using SenseNet.ContentRepository.Schema;
//using SenseNet.Services.Instrumentation;
//using SenseNet.Services.ContentStore;

//namespace SenseNet.Services.SupportTypes
//{
//    public class RESTHandler : IHttpHandler
//    {
//        bool _isReusable;
//        string _command;

//        public RESTHandler()
//        {
//            _isReusable = true;
//        }

//        internal RESTHandler(string command)
//        {
//            _command = command;
//            _isReusable = false;
//        }


//        #region IHttpHandler Members

//        public bool IsReusable
//        {
//            get { return _isReusable; }
//        }

//        public void ProcessRequest(HttpContext context)
//        {
//            string command = _command ?? HttpContext.Current.Request["function"];
//            string path = HttpContext.Current.Request["node"];
//            string withProperties = HttpContext.Current.Request["withProperties"];
//            string onlyFileChildren = HttpContext.Current.Request["onlyFileChildren"];
//            string onlyFiles = HttpContext.Current.Request["onlyFiles"];
//            string onlyFolders = HttpContext.Current.Request["onlyFolders"];
//            string target = HttpContext.Current.Request["target"];
//            string expr = HttpContext.Current.Request["expr"];
//            string nodeList = HttpContext.Current.Request["nodeList"];
//            string targetPath = HttpContext.Current.Request["targetPath"];

//            switch (command)
//            {
//                case "GetItem":
//                    HandleGetItem(path);
//                    return;
//                case "GetFeed":
//                    HandleGetFeed(path);
//                    return;
//                case "GetItem2":
//                    HandleGetItem2(path, "false", onlyFileChildren);
//                    return;
//                case "GetItem3":
//                    HandleGetItem2(path, withProperties, onlyFileChildren);
//                    return;
//                case "GetFeed2":
//                    HandleGetFeed2(path, onlyFiles, onlyFolders);
//                    return;
//                case "GetContentTypeNames":
//                    HandleContentTypeNames();
//                    return;
//                // ------------- new items
//                case "DeleteMore":
//                    new ContentStoreService().DeleteMore(nodeList);
//                    return;
//                case "Delete":
//                    // path is not a path rather an Id in this case
//                    new ContentStoreService().Delete(path);
//                    return;
//                case "GetContentTypes":
//                    HandleContentTypeNames(); // just an alias to "GetContentTypeNames"
//                    return;
//                case "Copy":
//                    // path is not a path rather an Id in this case
//                    new ContentStoreService().Copy(path, target);
//                    //new ContentStoreService().Copy(Int32.Parse(path), Int32.Parse(target));
//                    return;
//                case "CopyMore":
//                    new ContentStoreService().CopyMore(nodeList,targetPath);
//                    return;
//                case "Move":
//                    // path is not a path rather an Id in this case
//                    new ContentStoreService().Move(path, target);
//                    return;
//                case "MoveMore":
//                    new ContentStoreService().MoveMore(nodeList,targetPath);
//                    break;
//                case "Search":
//                    WriteJsonResponse(new ContentStoreService().Search(expr));
//                    return;
//                case "Query":
//                    HandleQuery(expr, withProperties);
//                    return;
//                default:
//                    throw new NotSupportedException(string.Format("The command '{0}' is not supported.", command));
//            }

//        }


//        public void HandleContentTypeNames()
//        {
//            WriteJsonResponse(GetContentTypes());
//        }
//        private Content[] GetContentTypes()
//        {
//            var result = ContentType.GetContentTypes();
//            return result.Select(node => new Content(node, false, false, false, true,0,0)).ToArray();
//        }

//        public void HandleGetFeed(string path)
//        {
//            var result = new ContentStoreService().GetFeed(path);
//            WriteJsonResponse(result);
//        }

//        public void HandleGetItem(string path)
//        {
//            var result = new ContentStoreService().GetItem(path);
//            WriteJsonResponse(result);
//        }

//        public void HandleGetFeed2(string path, string onlyFiles, string onlyFolders)
//        {
//            bool ofiles = false;
//            bool.TryParse(onlyFiles, out ofiles);

//            bool oFolders = false;
//            bool.TryParse(onlyFolders, out oFolders);

//            var result = new ContentStoreService().GetFeed2(path, ofiles, oFolders, 0,0);
//            WriteJsonResponse(result);
//        }

//        public void HandleQuery(string queryXml, string withProperties)
//        {
//            bool oWithProperties = false;
//            bool.TryParse(withProperties, out oWithProperties);

//            var result = new ContentStoreService().Query(queryXml, oWithProperties);
//            WriteJsonResponse(result);
//        }

//        public void HandleGetItem2(string path, string withProperties, string onlyFileChildren)
//        {
//            bool oFileChildren = false;
//            bool oWithProperties = false;
//            bool.TryParse(onlyFileChildren, out oFileChildren);
//            bool.TryParse(withProperties, out oWithProperties);

//            var startParam = HttpContext.Current.Request["start"];
//            var limitParam = HttpContext.Current.Request["limit"];
//            var start = 0; 
//            var limit = 0;

//            Int32.TryParse(startParam, out start);
//            Int32.TryParse(limitParam, out limit);

//            var result = new ContentStoreService().GetItem3(path, oWithProperties, oFileChildren, start, limit);
//            WriteJsonResponse(result);
//        }

//        private void WriteJsonResponse(object response)
//        {
//            HttpContext.Current.Response.ContentType = "application/json";

//            DataContractJsonSerializer jsonSerializer =
//                new DataContractJsonSerializer(response.GetType(), new Type[] { typeof(EntityReference), typeof(Content) });

//            using (MemoryStream ms = new MemoryStream())
//            {
//                jsonSerializer.WriteObject(ms, response);
//                ms.Position = 0;
//                string result;
//                using (var sr = new StreamReader(ms))
//                {
//                    result = sr.ReadToEnd();
//                }
//                HttpContext.Current.Response.Write(result);
//            }
//        }
//        #endregion
//    }
//}