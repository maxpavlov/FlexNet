using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using System.Web;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using SenseNet.Portal.Virtualization;
using SenseNet.ApplicationModel;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Handlers
{
    [ContentHandler]
    public class HttpHandlerApplication : Application, IHttpHandler
    {
        public HttpHandlerApplication(Node parent) : this(parent, null) { }
        public HttpHandlerApplication(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected HttpHandlerApplication(NodeToken nt) : base(nt) { }

        //============================================================================================= IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }
        public void ProcessRequest(HttpContext context)
        {
            switch (this.Name)
            {
                case "CreateNewTemplatedItem":
                    CreateNewTemplatedItem(context);
                    break;
                case "QuickAddNew":
                    QuickAddNew(context);
                    break;
                case "QuickEdit":
                    QuickEdit(context);
                    break;
                case "Install":
                    Install(context);
                    break;
            }
        }

        //============================================================================== CreateNewTemplatedItem

        private void CreateNewTemplatedItem(HttpContext context)
        {
            var id = CreateContentFromTemplate(PortalContext.Current.ContextNode, context.Request.Params["ContentTemplate"]);
            var back = new StringBuilder(PortalContext.Current.OriginalUri.GetLeftPart(UriPartial.Path));
            if (id > 0)
                back.Append("?SelectedContentId=").Append(id);
            context.Response.Redirect(back.ToString());
        }
        private int CreateContentFromTemplate(Node target, string templateName)
        {
            if (target == null)
                return 0;
            if (string.IsNullOrEmpty(templateName))
                return 0;
            var templatePath = RepositoryPath.Combine(Repository.ContentTemplateFolderPath, templateName);
            var templateNode = Node.LoadNode(templatePath);
            if (templateNode == null)
                return 0;

            var index = 0;
            string newPath = null;
            string newName = null;
            while (Node.Exists(newPath = GetNewPath(target, templateNode, index++, out newName))) ;
            if (newPath == null || newName == null)
                return 0;

            try
            {
                templateNode.CopyTo(target, newName);

                var copied = Node.LoadNode(newPath);
                copied.CreationDate = copied.ModificationDate = DateTime.Now;
                copied.Save();

                return copied.Id;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                return 0;
            }
        }
        private string GetNewPath(Node container, Node templateNode, int index, out string newName)
        {
            var ext = System.IO.Path.GetExtension(templateNode.Name);
            var fileName = System.IO.Path.GetFileNameWithoutExtension(templateNode.Name);

            newName = index == 0 ?
                String.Format("New {0}", templateNode.Name) :
                String.Format("New {1} ({2}){3}", container.Path, fileName, index, ext);

            return String.Format("{0}/{1}", container.Path, newName);
        }

        //============================================================================== QuickAddNew

        private void QuickAddNew(HttpContext context)
        {
            var content = ContentManager.CreateContentFromRequest();
            if (!content.IsValid)
                throw new InvalidContentException("Content was not saved.");

            content.Save();

            var back = new StringBuilder(PortalContext.Current.OriginalUri.GetLeftPart(UriPartial.Path)).Append("?SelectedContentId=").Append(content.Id);
            context.Response.Redirect(back.ToString());
        }

        //============================================================================== QuickEdit

        private void QuickEdit(HttpContext context)
        {
            var content = Content.Create(PortalContext.Current.ContextNode);
            ContentManager.ModifyContentFromRequest(content);
            if (!content.IsValid)
                throw new InvalidContentException("Content was not saved.");

            content.Save();

            var back = new StringBuilder(PortalContext.Current.OriginalUri.GetLeftPart(UriPartial.Path)).Append("?SelectedContentId=").Append(content.Id);
            context.Response.Redirect(back.ToString());
        }

        //============================================================================== Install

        private void Install(HttpContext context)
        {
            var sourceNode = PortalContext.Current.ContextNode;
            var targetNode = Node.LoadNode(Int32.Parse(context.Request.Params["TargetNode"]));
            var newName = context.Request.Params["NewName"];

            sourceNode.CopyTo(targetNode, newName);

            var newPath = new StringBuilder(PortalContext.Current.OriginalUri.GetLeftPart(UriPartial.Authority)).Append(targetNode.Path).Append("/").Append(newName);
                
            // FIXME! Make path site relative!
            context.Response.Redirect(newPath.ToString());
        }
    }
}
