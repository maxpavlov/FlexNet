using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.Web;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Schema;
using System.Diagnostics;
using SenseNet.Portal.Virtualization;
using SenseNet.ApplicationModel;

namespace SenseNet.Portal.AppModel
{
    internal abstract class HttpAction : IHttpAction
    {
        private NodeHead _appNode;

        public NodeHead AppNode
        {
            get
            {
                if (_appNode != null)
                    return _appNode;
                if (TargetNode == null)
                    return null;
                if (!IsApplication(TargetNode))
                    return null;
                return TargetNode;
            }
            set
            {
                if (value != null)
                {
                    if (!IsApplication(value))
                    {
                        var nodeType = value.GetNodeType();
                        throw new ApplicationException(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "Node is not an application. Path: {0}, NodeType: {1}, ClassName: {2}",
                                value.Path,
                                nodeType.Name,
                                nodeType.ClassName));
                    }
                }
                _appNode = value;
            }
        }
        public NodeHead TargetNode { get; set; }
        public IHttpActionContext Context { get; set; }

        public abstract void Execute();

        private bool IsApplication(NodeHead appNode)
        {
            var type = TypeHandler.GetType(appNode.GetNodeType().ClassName);
            //if (typeof(Page).IsAssignableFrom(type))
            //    return true;
            //if (typeof(IHttpHandler).IsAssignableFrom(type))
            //    return true;
            return typeof(Application).IsAssignableFrom(type);
        }

        public virtual bool CheckPermission()
        {
            bool isOwner;
            if (TargetNode != null)
            {
                isOwner = TargetNode.CreatorId == User.Current.Id;
                if (!SecurityHandler.HasPermission(TargetNode, PermissionType.See))
                    ThrowNotFound();
                if (!SecurityHandler.HasPermission(TargetNode, PermissionType.RunApplication))
                {
                    Trace.WriteLine(String.Format("#> ######################### Current user ({0} has not RunApplication permission to the context node: {1})", User.Current.Username, TargetNode.Path));
                    return false;
                }
            }
            if (_appNode != null)
            {
                isOwner = _appNode.CreatorId == User.Current.Id;
                if (!SecurityHandler.HasPermission(_appNode, PermissionType.RunApplication))
                {
                    Trace.WriteLine(String.Format("#> ######################### Current user ({0} has not RunApplication permission to the application node: {1})", User.Current.Username, _appNode.Path));
                    return false;
                }

                if (TargetNode != null)
                {
                    if (!ActionFramework.HasRequiredPermissions(Node.Load<Application>(_appNode.Id), TargetNode))
                        return false;
                }
            }
            return true;
        }

        public virtual void AssertPermissions()
        {
            bool isOwner;
            if (TargetNode != null)
            {
                isOwner = TargetNode.CreatorId == User.Current.Id;
                if (!SecurityHandler.HasPermission(TargetNode, PermissionType.See))
                    ThrowNotFound();
                if (!SecurityHandler.HasPermission(TargetNode, PermissionType.RunApplication))
                    ThrowForbidden();
            }
            
            if (_appNode == null) 
                return;

            isOwner = _appNode.CreatorId == User.Current.Id;
            if (!SecurityHandler.HasPermission(_appNode, PermissionType.RunApplication))
                ThrowForbidden();

            if (TargetNode == null) 
                return;

            if (!ActionFramework.HasRequiredPermissions(Node.Load<Application>(_appNode.Id), TargetNode))
                ThrowForbidden();
        }

        protected void ThrowNotFound()
        {
            throw new HttpException(404, string.Format("Not found: {0}", TargetNode == null ? string.Empty : TargetNode.Name));
        }
        protected void ThrowForbidden()
        {
            throw new HttpException(403, string.Format("Forbidden: {0}", TargetNode == null ? string.Empty : TargetNode.Name));
        }
    }

    internal class DefaultHttpAction : HttpAction, IDefaultHttpAction
    {
        public override void Execute()
        {
            // Do nothing
        }
    }
    internal class RedirectHttpAction : HttpAction, IRedirectHttpAction
    {
        public string TargetUrl { get; set; }
        public bool EndResponse { get; set; }
        public bool Permanent { get; set; }

        public override void Execute()
        {
            if(Permanent)
                RedirectPermanently(HttpContext.Current.Response, TargetUrl);
            else
                HttpContext.Current.Response.Redirect(TargetUrl, EndResponse);
        }
        private static void RedirectPermanently(HttpResponse response, string url)
        {
            response.Clear();
            response.Status = "301 Moved Permanently";
            response.AddHeader("Location", url);
            response.End();
        }
    }
    internal class RewriteHttpAction : HttpAction, IRewriteHttpAction
    {
        public string Path { get; set; }
        public bool? RebaseClientPath { get; set; }

        public string FilePath { get; set; }
        public string PathInfo { get; set; }
        public string QueryString { get; set; }
        public bool? SetClientFilePath { get; set; }

        public override void Execute()
        {
            if (Path != null)
            {
                if (RebaseClientPath == null)
                    HttpContext.Current.RewritePath(Path);
                else
                    HttpContext.Current.RewritePath(Path, RebaseClientPath.Value);
            }
            else
            {
                if (SetClientFilePath == null)
                    HttpContext.Current.RewritePath(FilePath, PathInfo, QueryString);
                else
                    HttpContext.Current.RewritePath(FilePath, PathInfo, QueryString, SetClientFilePath.Value);
            }
        }
    }
    internal class DownloadHttpAction : RewriteHttpAction, IDownloadHttpAction
    {
        public string BinaryPropertyName { get; set; }

        public override void AssertPermissions()
        {
            var isOwner = TargetNode.CreatorId == User.Current.Id;
            if (!SecurityHandler.HasPermission(TargetNode, PermissionType.See))
                base.ThrowNotFound();
            if (!SecurityHandler.HasPermission(TargetNode, PermissionType.Open))
                base.ThrowForbidden();
        }
    }
    internal class RemapHttpAction : HttpAction, IRemapHttpAction
    {
        public NodeHead HttpHandlerNode { get; set; }
        public Type HttpHandlerType { get; set; }

        public override void Execute()
        {
            IHttpHandler handler;

            if (HttpHandlerType != null)
            {
                handler = (IHttpHandler)Activator.CreateInstance(HttpHandlerType);
            }
            else
            {
                using (new SystemAccount())
                {
                    //handler = (IHttpHandler)Node.LoadNode(HttpHandlerNode.Id);
                    VersionNumber version = null;
                    var versionStr = PortalContext.Current.VersionRequest;

                    handler = string.IsNullOrEmpty(versionStr) || !VersionNumber.TryParse(versionStr, out version)
                        ? (IHttpHandler)Node.LoadNode(HttpHandlerNode.Id) 
                        : (IHttpHandler)Node.LoadNode(HttpHandlerNode.Id, version);
                }
            }
            HttpContext.Current.RemapHandler(handler);
        }
    }
}
