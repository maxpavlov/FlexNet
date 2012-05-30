//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web.Mvc;
//using System.Web;
//using SenseNet.ApplicationModel;
//using SNCR = SenseNet.ContentRepository;
//using SenseNet.ContentRepository.Storage;

//namespace SenseNet.Workflow
//{
//    public class WorkflowInstanceManagerController : Controller
//    {
//        [AcceptVerbs(HttpVerbs.Get)]
//        public ActionResult Index()
//        {
//            return Content("booted up");
//        }


//        [AcceptVerbs(HttpVerbs.Get)]
//        public ActionResult StartWorkflow(string wfname)
//        {
//            var wfInstance = Node.Load<WorkflowHandlerBase>(wfname);
//            //var wfInstance = new WorkflowInstanceHandlerBase() { Path = "/root/wf1", Version = "10P" };
//            InstanceManager.Start(wfInstance);
//            //InstanceManager.Start(null);
//            return this.Content("done");
//        }

//        [AcceptVerbs(HttpVerbs.Get)]
//        public ActionResult ExecuteDelays(string wfname)
//        {
//            var wfInstance = Node.Load<WorkflowHandlerBase>(wfname);
//            //var wfInstance = new WorkflowInstanceHandlerBase() { Path = "/root/wf1", Version = "10P" };
//            InstanceManager.ExecuteDelays(wfInstance);
//            //InstanceManager.Start(null);
//            return this.Content("done");
//        }

//    }
//}
