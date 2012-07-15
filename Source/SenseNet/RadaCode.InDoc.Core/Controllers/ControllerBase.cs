using System.Diagnostics;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;

namespace RadaCode.InDoc.Core.Controllers
{
    public class ControllerBase : Controller
    {
        protected string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines[1].FindPartialView(ControllerContext, viewName, false);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Trace.WriteLine(filterContext.Exception, "Exception");
            base.OnException(filterContext);
        }

        /// <summary>
        /// Called before the action method is invoked. This overides the default behaviour by 
        /// populating RoadkillContext.Current.CurrentUser with the current logged in user after
        /// each action method.
        /// </summary>
        /// <param name="filterContext">Information about the current request and action.</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            AssertPermission();
        }

        private static readonly string PlaceholderPath = "/Root/System/PermissionPlaceholders/ContentStore-mvc";

        private void AssertPermission()
        {
            var permissionContent = Node.LoadNode(PlaceholderPath);
            if (permissionContent == null || !permissionContent.Security.HasPermission(PermissionType.RunApplication))
                throw new SenseNetSecurityException("Access denied for " + PlaceholderPath);
        }
    }
}
