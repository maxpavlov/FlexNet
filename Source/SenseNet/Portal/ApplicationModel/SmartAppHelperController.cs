using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SenseNet.ApplicationModel;
using SenseNet.Diagnostics;
using SNCR = SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI;

namespace SenseNet.Portal.AppModel
{
    public class SmartAppHelperController : Controller
    {

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetActions(string path, string scenario, string back, string parameters)
        {
            path = HttpUtility.UrlDecode(path);
            scenario = HttpUtility.UrlDecode(scenario);
            parameters = HttpUtility.UrlDecode(parameters);

            //this line caused an error in back url encoding (multiple back 
            //parameters when the user goes deep, through multiple actions)
            //back = HttpUtility.UrlDecode(back);

            var actions = new List<ActionBase>();

            var sc = ScenarioManager.GetScenario(scenario, parameters);
            if (sc != null)
            {
                var context = SNCR.Content.Load(path);
                actions = sc.GetActions(context, back).ToList();
            }

            return Json(actions, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Icon(string key, string size, string skin)
        {
            key = HttpUtility.UrlDecode(key);
            size = HttpUtility.UrlDecode(size);
            skin = HttpUtility.UrlDecode(skin);

            var path = IconHelper.RenderIconTag(key);
            var iconNode = Node.Load<SNCR.File>(path);

            if (iconNode == null) return null;

            byte[] image = new byte[iconNode.Binary.GetStream().Length];

            iconNode.Binary.GetStream().Read(image, 0, image.Length);
            return File(image, iconNode.Binary.ContentType);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Logout(string back)
        {
            var info = new CancellableLoginInfo { UserName = SNCR.User.Current.Username };
            LoginExtender.OnLoggingOut(info);

            FormsAuthentication.SignOut();

            if (!info.Cancel)
            {
                Logger.WriteAudit(AuditEvent.Logout, new Dictionary<string, object> { { "UserName", SNCR.User.Current.Username }, { "ClientAddress", Request.ServerVariables["REMOTE_ADDR"] } });
                LoginExtender.OnLoggedOut(new LoginInfo { UserName = SNCR.User.Current.Username });
            }

            Session.Clear();

            back = string.IsNullOrEmpty(back) ? "/" : HttpUtility.UrlDecode(back);

            return this.Redirect(back);
        }
    }
}
