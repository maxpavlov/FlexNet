using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Search;
using SenseNet.Services.ContentStore;

namespace SenseNet.Portal.UI.Controls
{
    [HandleError]
    public class DialogUploadController : Controller
    {
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUserUploads(string startUploadDate, string path, string rnd)
        {
            if (!HasPermission())
                return Json(null, JsonRequestBehavior.AllowGet);

            var results = ContentQuery.Query("+ModificationDate:>='" + startUploadDate + "' +InFolder:\"" + path + "\" +CreatedById:" + ContentRepository.User.Current.Id.ToString()).Nodes;

            return Json((from n in results
                         where n != null
                         select new Content(n, true, false, false, false, 0, 0)).ToArray(), JsonRequestBehavior.AllowGet);
        }


        //===================================================================== Helper methods
        private static readonly string PlaceholderPath = "/Root/System/PermissionPlaceholders/DialogUpload-mvc";
        private bool HasPermission()
        {
            var permissionContent = Node.LoadNode(PlaceholderPath);
            return !(permissionContent == null || !permissionContent.Security.HasPermission(PermissionType.RunApplication));
        }
        private void AssertPermission()
        {
            if (!HasPermission())
                throw new SenseNetSecurityException("Access denied for " + PlaceholderPath);
        }
    }
}
