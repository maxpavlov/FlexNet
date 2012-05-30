using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;
using System.IO;
using System.Web;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.Portal.PortletFramework
{
    enum WebdavModes    { Open, Browse, Create }

    [Obsolete("This class was obsoleted by the SenseNet.ApplicationModel namespace.")]
    public class PortalActionLinkResolver : IActionLinkResolver
    {
        public static readonly PortalActionLinkResolver Instance = new PortalActionLinkResolver();

        private PortalActionLinkResolver() { }

        private string Resolve(string actionName)
        {
            //var contextNodeRelPath = PortalContext.Current.ContextNode.Path.Substring(PortalContext.Current.Site.Path.Length + 1);
            //return ResolveAction(contextNodeRelPath, actionName);
            return ResolveAction(string.Empty, actionName, null);
        }
        public string ResolveRelative(string targetPath)
        {
            return ResolveRelative(targetPath, null);
        }
        public string ResolveRelative(string targetPath, string actionname)
        {
            return ResolveRelative(targetPath, actionname, null);
        }
        public string ResolveRelative(string targetPath, string actionName, string backUrl)
        {
            // HACK: Permission control ( Prioritized through CEO order XD )
            Node targetNode = Node.LoadNode(targetPath);
            if (targetNode != null)
            {
                string securityActionName = actionName;
                if (String.IsNullOrEmpty(securityActionName))
                    securityActionName = "Browse";
                PermissionType ptype;
                switch (securityActionName)
                {
                    case "Rss":
                    case "Browse":
                    case "WebdavOpen":
                    case "WebdavBrowse":
                    case "ListActions":
                        ptype = PermissionType.See;
                        break;
                    case "Add":
                    case "WebdavCreate":
                        ptype = PermissionType.AddNew;
                        break;
                    case "Delete":
                        ptype = PermissionType.Delete;
                        break;
                    case "Edit":
                    case "AddExisting":
                    case "Remove":
                    // In case of an unknown action, we assume it needs Save rights
                    default:
                        ptype = PermissionType.Save;
                        break;
                }
                if (targetNode.Security.GetPermission(ptype) != SenseNet.ContentRepository.Storage.Security.PermissionValue.Allow)
                    return String.Empty;
            }

            if (PortalContext.Current == null || PortalContext.Current.Site == null)
                return ResolveAction(targetPath, actionName, backUrl);
            var relRoot = PortalContext.Current.Site.Path;
            if (targetPath == relRoot)
                return Resolve(actionName);
            if(!targetPath.StartsWith(relRoot + "/"))
                return ResolveAction(targetPath, actionName, backUrl);
            var targetRelPath = targetPath.Substring(relRoot.Length + 1);
            return ResolveAction(targetRelPath, actionName, backUrl);
        }
        private string ResolveAction(string path, string actionName, string backUrl)
        {
            var actionPart = String.Empty;
            var backUrlPart = String.Empty;
            var queryString = String.Empty;
            var resolvedPath = String.Concat(path.StartsWith("/") ? "" : "/", path);

            if (actionName != null)
            {
                switch (actionName)
                {
                    case "Browse":
                        if (path.EndsWith(".ascx"))
                        {
                            actionName = "Edit";
                            actionPart = String.Concat("?", PortalContext.ActionParamName, "=Edit");
                        }
                        break;
                    case "WebdavOpen": return GetWebdavAction(path, WebdavModes.Open);
                    case "WebdavBrowse": return GetWebdavAction(path, WebdavModes.Browse);
                    case "WebdavCreate": return GetWebdavAction(path, WebdavModes.Create);

                    default: actionPart = String.Concat("?", PortalContext.ActionParamName, "=", actionName); break;
                }

                string backUrlParam = String.Concat("&", PortalContext.BackUrlParamName, "=");
                switch (actionName)
                {
                    case "Rss":
                    case "WebdavOpen":
                    case "WebdavBrowse":
                    case "Browse":
                        break;
                    default:
                        if (!string.IsNullOrEmpty(backUrl))
                            backUrlPart = string.Concat(backUrlParam, backUrl);
                        break;
                }
            }

            if (!String.IsNullOrEmpty(actionPart))
                queryString = String.Concat(actionPart, backUrlPart);
            var actionUrl = String.Concat(resolvedPath, queryString);
            // FIXME: Add logic to check URI length and act accordingly
            return actionUrl;
        }

        private string GetWebdavAction(string path, WebdavModes mode)
        {
            string url = GetWebdavUrl(path);
            if (String.IsNullOrEmpty(url))
                return null;
            else
                switch (mode)
                {
                    case WebdavModes.Open: return string.Format("javascript:SN.WebDav.OpenDocument(\"{0}\")", url);
                    case WebdavModes.Create: return string.Format("javascript:SN.WebDav.CreateDocument(\"{0}\")", url);
                    case WebdavModes.Browse: return string.Format("javascript:SN.WebDav.BrowseFolder(\"{0}\")", url);
                    default: return null;
                }
        }

        private string GetWebdavUrl(string path)
        {
            if (PortalContext.Current != null)
            {
                if (PortalContext.Current.AuthenticationMode.CompareTo("Windows") == 0)
                {
                    var relRoot = PortalContext.Current.Site.Path;

                    if (!path.StartsWith(relRoot))
                        path = RepositoryPath.Combine(relRoot, path);

                    if (path.StartsWith("/Root"))
                        path = path.Remove(0, 5);

                    Uri currentUri = PortalContext.Current.OwnerHttpContext.Request.Url;

                    return RepositoryPath.Combine(currentUri.GetLeftPart(UriPartial.Authority), path);
                }
            }

            return string.Empty;
        }
    }
}
