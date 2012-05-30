using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI;

[assembly: WebResource("SenseNet.Portal.HcExplore.Scripts.jquery.js", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Views.HcExplore.aspx", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Views.HcExploreLogin.aspx", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Views.HcExploreBrowse.aspx", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Views.HcExploreEdit.aspx", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Views.HcExploreMenuItem.aspx", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Views.HcExploreUpload.aspx", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Views.HcExploreMaster.master", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Scripts.jquery.treeview.js", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Styles.jquery.treeview.css", "text/css")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Scripts.HcExplore.js", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Images.folder-closed.gif", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Images.minus.gif", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Images.plus.gif", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Images.folder.gif", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Images.treeview-default-line.gif", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Images.treeview-default.gif", "application/x-javascript")]
[assembly: WebResource("SenseNet.Portal.HcExplore.Images.delete.png", "application/x-javascript")]
namespace SenseNet.Portal.HcExplore
{
    public class HcExploreController : Controller
    {

        public ActionResult Index(string root)
        {
            if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated && SenseNet.ContentRepository.User.Current.IsInGroup(Group.Administrators))
            {
                if (string.IsNullOrEmpty(root) || (string.IsNullOrEmpty(root) && !Node.Exists(root)))
                    root = "/Root";

                if ((object) root != null)
                {
                    base.ViewData.Model = ((object) root);
                }

                var result = new ViewResult
                                 {
                                     ViewName = "~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExplore.aspx",
                                     MasterName = "~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExploreMaster.master",
                                     ViewData = base.ViewData,
                                     TempData = base.TempData
                                 };
                return result;
            }

            return RedirectToAction("Login", "HcExplore");
        }

        public ActionResult Login(string userName, string password)
        {
            if(!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                if (Membership.ValidateUser(userName, password))
                {
                    FormsAuthentication.SetAuthCookie(userName, true);
                    return RedirectToAction("Index", "HcExplore", new RouteValueDictionary() {{"root", "/Root"}});
                }
            }
            var result = new ViewResult
            {
                ViewName = "~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExplore.aspx",
                MasterName = "~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExploreMaster.master",
                ViewData = base.ViewData,
                TempData = base.TempData
            };
            return View("~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExploreLogin.aspx","~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExploreMaster.master");
        }

        public ActionResult Upload(string root, string nodeType, string fileName)
        {
            var createdFile = false;
            if (Request.Files.Count > 0)
            {
                foreach (string inputTagName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[inputTagName];
                    if (file.ContentLength > 0)
                    {
                        var binaryData = new BinaryData();
                        binaryData.ContentType = file.ContentType;
                        binaryData.FileName = Path.GetFileName(file.FileName);
                        binaryData.SetStream(file.InputStream);

                        var cnt = SenseNet.ContentRepository.Content.CreateNew(nodeType, Node.LoadNode(root),
                                                                               string.IsNullOrEmpty(fileName)
                                                                                   ? Path.GetFileName(file.FileName)
                                                                                   : fileName);
                        cnt.ContentHandler.SetBinary("Binary", binaryData);
                        cnt.Save();
                        createdFile = true;
                    }
                }
            }
            if (!string.IsNullOrEmpty(fileName) &&!createdFile)
            {
                var cnt = SenseNet.ContentRepository.Content.CreateNew(nodeType, Node.LoadNode(root),
                                                                       fileName);
                cnt.Save();
            }
            return RedirectToAction("Index", "HcExplore", new RouteValueDictionary() {{"root", root}});

        }

        public ActionResult Update(string root, FormCollection form)
        {
            var cnt = SenseNet.ContentRepository.Content.Load(root);
            foreach (var key in form.AllKeys)
            {
                if(cnt.Fields.ContainsKey(key))
                {
                    if (!cnt.Fields[key].HasValue() && !string.IsNullOrEmpty(form[key]))
                        SetDataToFiled(cnt.Fields[key],form[key]);
                    else if(cnt.Fields[key].HasValue() && !cnt.Fields[key].OriginalValue.Equals(form[key]))
                        SetDataToFiled(cnt.Fields[key],form[key]);
                }
            }
            cnt.Save();
            return RedirectToAction("Index", "HcExplore", new RouteValueDictionary() { { "root", root } });

        }

        public ActionResult Delete(string path)
        {
            try
            {
                var node = Node.LoadNode(path);
                if (node.Security.HasPermission(PermissionType.Delete))
                {
                    node.Delete();
                }
                return RedirectToAction("Index", "HcExplore", new RouteValueDictionary {{"root", RepositoryPath.GetParentPath(path)}});
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
            return RedirectToAction("Index", "HcExplore", new RouteValueDictionary {{"root", path}});
        }

        public static void SetDataToFiled(Field field, string value)
        {
            switch (field.FieldSetting.ShortName)
            {
                case "Integer":
                    field.SetData(int.Parse(value));
                    break;
                case "Boolean":
                    field.SetData(bool.Parse(value));
                    break;
                case "DateTime":
                    field.SetData(DateTime.Parse(value));
                    break;
                case "ShortText":
                case "LongText":
                default:
                    field.SetData(value);
                    break;
            }
        }
    }

    public static class ViewPageExtension
    {
        public static Node GetNode(this ViewPage<string> vPage, string path)
        {
            return Node.LoadNode(path);
        }
    }
}
