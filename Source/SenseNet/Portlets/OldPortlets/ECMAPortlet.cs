//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using SenseNet.Portal.UI.PortletFramework;
//using SenseNet.ContentRepository;
//using SenseNet.ContentRepository.Storage;
//using RemObjects.Script;
//using RemObjects.Script.EcmaScript;
//using SenseNet.Search;

//namespace SenseNet.Portal.Portlets
//{
//    public class ECMAPortlet : PortletBase
//    {

//        Object GetData(EcmaScriptComponent engine, string query)
//        {
//            var arr = engine.GlobalObject.CreateArray();
//            var result = ContentQuery.Query(query);
//            int i = 0;
//            foreach(var x in result.CurrentPage)
//            {
//                arr.PutIndex(i++, x);
//            }
//            return arr;
//        }

//        protected override void  Render(System.Web.UI.HtmlTextWriter writer)
//{
//            //var se = new EcmaScriptComponent();
//            se.Globals.SetVariable("Render", new Action<string>(s => writer.Write(s)));
            
//            se.Globals.SetVariable("Query", new Func<string, object>(qtext => GetData(se, qtext)));
//            se.Source = @"function GetData() 
//                         {
//                                var result = [];
//                                var novels = Query('+Genre:novel* .SORT: CreationDate .TOP:2');
//                                var scifis = Query('+Genre:sci* .SORT: CreationDate .TOP:2');
//                                
//                                for(var i in novels)
//                                {
//                                    result.push(novels[i]);
//                                }
//                                for(var j in scifis)
//                                {
//                                    result.push(scifis[j]);
//                                }                                
//                                return result;
//                        };
//                        function _createSomeNiftyQuery()
//                        {
//                                
//                        };
//
//                        ";
            
//            se.Run();
//            var result = se.RunFunction("GetData");
//            writer.Write(result.GetType().Name);
//            writer.Write("<br />");
//            var zx = ContentQuery.Query("x");
//            //zx.CurrentPage
//    // base.Render(writer);
//}
//        //protected override object GetModel()
//        //{
            
//        //    //var folder = SearchFolder.Create(new Node[] { });
            
            
//        //}
//    }
//}
