using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Json;
using SenseNet.Services.SupportTypes;

namespace SenseNet.Services.ContentStore
{
 /// <summary>
 /// RESTfull API for managing content level
 /// effective ACLs.
 /// </summary>
    public class SecurityController : Controller
    {

        public ActionResult GetACL(string path)
        {
            var node = Node.LoadNode(path);
            var acl = node.Security.GetAcl();
            return Json(acl);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [JsonFilter(DataType = typeof(SnAccessControlList), Param = "acl")]
        public ActionResult SetACL(SnAccessControlList acl)
        {
            var node = Node.LoadNode(acl.Path);
            node.Security.SetAcl(acl);
            return null; 
        }


        public ActionResult SearchIdentities(SearchIdentitiesArgument parameter)
        {

            return null;
        }
    }


}
