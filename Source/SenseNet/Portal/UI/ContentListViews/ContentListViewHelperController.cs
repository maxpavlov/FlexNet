using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using SenseNet.Portal.PortletFramework;
using System.IO;
using SenseNet.Portal.UI.ContentListViews;
using System.Web.Routing;

namespace SenseNet.Portal.UI.ContentListViews
{
    public class ContentListViewHelperController : Controller
    {
        //hack
        public static string GetViewSelectLink(string path, string uiContextId, string view, string back)
        {
            return String.Format("/ContentListViewHelper.mvc/SetView?path={0}&uiContextId={1}&view={2}&back={3}",
            Uri.EscapeDataString(path),
            uiContextId, 
            view, 
            back);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SetView(string path, string uiContextId, string view, string back)
        {
            path = HttpUtility.UrlDecode(path);
            uiContextId = HttpUtility.UrlDecode(uiContextId);
            view = HttpUtility.UrlDecode(view);
            back = HttpUtility.UrlDecode(back);

            string hash = ViewFrame.GetHashCode(path, uiContextId);
            ViewFrame.SetView(hash, view);
            return Redirect(HttpUtility.UrlDecode(back));
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SetDefault(string path, string view, string back)
        {
            path = HttpUtility.UrlDecode(path);
            view = HttpUtility.UrlDecode(view);
            back = HttpUtility.UrlDecode(back);

            var list = Node.Load<ContentList>(path);
            list.DefaultView = view;
            list.Save();
            return Redirect(HttpUtility.UrlDecode(back));
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CopyViewLocal(string listPath, string viewPath, string back)
        {
            if (string.IsNullOrEmpty(listPath))
                throw new ArgumentNullException("listPath");
            if (string.IsNullOrEmpty(viewPath))
                throw new ArgumentNullException("viewPath");

            listPath = HttpUtility.UrlDecode(listPath);
            viewPath = HttpUtility.UrlDecode(viewPath);
            back = HttpUtility.UrlDecode(back);

            ViewManager.CopyViewLocal(listPath, viewPath, true);
            
            return Redirect(HttpUtility.UrlDecode(back));
        }
    }
}
