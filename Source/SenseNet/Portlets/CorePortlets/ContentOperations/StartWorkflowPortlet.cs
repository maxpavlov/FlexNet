using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Workflow;
using Content = SenseNet.ContentRepository.Content;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    public class StartWorkflowPortlet : ContentAddNewPortlet
    {
        //========================================================================================= Properties

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Workflow type")]
        [WebDescription("Please choose from the available workflow types. If you leave this entry empty, the portlet will behave like a simple add content portlet.")]
        [WebCategory(EditorCategory.Workflow, EditorCategory.Workflow_Order)]
        [WebOrder(10)]
        [Editor(typeof(DropDownPartField), typeof(IEditorPartField))]
        [DropDownPartOptions("+InTree:/Root/System/Schema/ContentTypes/GenericContent/Workflow -Name:Workflow")]
        public string WorkflowType { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Confirmation content path")]
        [WebDescription("Define a content that will be displayed to the user after the workflow is started")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(20)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions()]
        public string ConfirmContentPath { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Start the workflow on the current content")]
        [WebDescription("If set the workflow will start on the current content. Use this option when you are building a smart application page.")]
        [WebCategory(EditorCategory.Workflow, EditorCategory.Workflow_Order)]
        [WebOrder(30)]
        public bool StartOnCurrentContent { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Workflow container path")]
        [WebDescription("Container folder for the started workflow instances. If you leave this entry empty, the system will place the workflow content under the current content.")]
        [WebCategory(EditorCategory.Workflow, EditorCategory.Workflow_Order)]
        [WebOrder(40)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions()]
        public string WorkflowContainerPath { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Workflow template path")]
        [WebDescription("Template workflow content that contains all the initial data. If the workflow does not need any initial data you can leave this entry empty.")]
        [WebCategory(EditorCategory.Workflow, EditorCategory.Workflow_Order)]
        [WebOrder(50)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(AllowedContentTypes = "Workflow", TreeRoots = "/Root/ContentTemplates;/Root")]
        public string WorkflowTemplatePath { get; set; }

        //========================================================================================= Controls

        private IButtonControl _button;
        protected IButtonControl StartWorkflowButton
        {
            get { return _button ?? (_button = this.FindControlRecursive("StartWorkflow") as IButtonControl); }
        }

        //========================================================================================= Constructor

        public StartWorkflowPortlet()
        {
            this.Name = "Workflow start";
            this.Description = "This portlet starts a workflow";

            if (this.HiddenProperties == null)
                this.HiddenProperties = new List<string>();

            this.HiddenPropertyCategories = new List<string> { EditorCategory.Cache };
        }

        //========================================================================================= Overrides

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (this.StartWorkflowButton != null)
                this.StartWorkflowButton.Click += StartWorkflowButton_Click;
        }
        
        protected override ContentView GetContentView(Content newContent)
        {
            if (!string.IsNullOrEmpty(ContentViewPath))
                return base.GetContentView(newContent);

            var contentList = ContentList.GetContentListByParentWalk(GetContextNode());
            if (contentList != null)
            {
                //try to find a content view at /Root/.../MyList/WorkflowTemplates/MyWorkflow.ascx
                var wfTemplatesPath = RepositoryPath.Combine(contentList.Path, "WorkflowTemplates");
                var viewPath = RepositoryPath.Combine(wfTemplatesPath, newContent.Name + ".ascx");

                if (Node.Exists(viewPath))
                    return ContentView.Create(newContent, Page, ViewMode.InlineNew, viewPath);

                //try to find it by type name, still locally
                viewPath = RepositoryPath.Combine(wfTemplatesPath, newContent.ContentType.Name + ".ascx");

                if (Node.Exists(viewPath))
                    return ContentView.Create(newContent, Page, ViewMode.InlineNew, viewPath);

                //last attempt: global view for the workflow type
                return ContentView.Create(newContent, Page, ViewMode.InlineNew, "StartWorkflow.ascx");
            }
            else
            {
                var viewPath = string.Format("{0}/{1}/{2}", Repository.ContentViewFolderName, newContent.ContentType.Name, "StartWorkflow.ascx");
                var resolvedPath = string.Empty;
                if (!SkinManager.TryResolve(viewPath, out resolvedPath))
                {
                    resolvedPath = RepositoryPath.Combine(Repository.SkinGlobalFolderPath,
                                                          SkinManager.TrimSkinPrefix(viewPath));

                    if (!Node.Exists(resolvedPath))
                        resolvedPath = string.Empty;
                }

                if (!string.IsNullOrEmpty(resolvedPath))
                    return ContentView.Create(newContent, Page, ViewMode.InlineNew, resolvedPath);
            }

            return base.GetContentView(newContent);
        }

        protected override string GetRequestedContentType()
        {
            //workflow from template
            if (!string.IsNullOrEmpty(this.WorkflowTemplatePath))
                return this.WorkflowTemplatePath;

            //workflow by type
            return !string.IsNullOrEmpty(this.WorkflowType) ? this.WorkflowType : base.GetRequestedContentType();
        }

        protected override string GetSelectedContentType()
        {
            //workflow from template
            if (!string.IsNullOrEmpty(this.WorkflowTemplatePath))
                return this.WorkflowTemplatePath;

            //workflow by type
            return !string.IsNullOrEmpty(this.WorkflowType) ? this.WorkflowType : base.GetSelectedContentType();
        }

        protected override string GetParentPath()
        {
            var wfContainer = this.WorkflowContainerPath;
            if (string.IsNullOrEmpty(wfContainer))
                return base.GetParentPath();

            if (!wfContainer.StartsWith("/Root/") && this.ContextNode != null)
            {
                wfContainer = RepositoryPath.Combine(this.ContextNode.Path, wfContainer);
            }

            return wfContainer;
        }

        protected virtual void ValidateWorkflow()
        {
            if (this.ContentView == null || this.ContentView.Content == null)
                return;

            var workflow = this.ContentView.Content.ContentHandler as WorkflowHandlerBase;
            if (workflow == null)
                return;

            var refNode = workflow.RelatedContent;
            if (refNode != null)
            {
                var contentList = ContentList.GetContentListByParentWalk(this.ContextNode);

                if (contentList != null)
                {
                    if (!workflow.Path.StartsWith(RepositoryPath.Combine(contentList.Path, "Workflows") + "/"))
                        throw new InvalidOperationException(string.Format("Workflow must be under the list ({0})",
                                                                          workflow.Path));

                    if (!refNode.Path.StartsWith(contentList.Path + "/"))
                        throw new InvalidOperationException(string.Format("Related content must be in the list ({0})",
                                                                          refNode.Path));
                }
            }
        }

        protected override bool AllowCreationForEmptyAllowedContentTypes(string parentPath)
        {
            // startworkflow portlet uses custom type, so it does not rely upon allowed content types list
            // skip this check
            return true;
        }

        //========================================================================================= Event handlers

        protected void StartWorkflowButton_Click(object sender, EventArgs e)
        {
            if (this.ContentView == null)
                return;

            this.ContentView.NeedToValidate = true;
            this.ContentView.UpdateContent();

            var content = this.ContentView.Content;

            if (this.ContentView.IsUserInputValid && content.IsValid)
            {
                try
                {
                    if (this.StartOnCurrentContent)
                    {
                        var workflow = content.ContentHandler as WorkflowHandlerBase;
                        if (workflow != null)
                        {
                            content.Fields["RelatedContent"].SetData(this.ContextNode);
                            //workflow.RelatedContent = this.ContextNode;
                        }
                    }

                    ValidateWorkflow();

                    //need to create workflow in elevated mode
                    using (new SystemAccount())
                    {
                        content.Fields["OwnerSiteUrl"].SetData(PortalContext.Current.RequestedUri.GetLeftPart(UriPartial.Authority));
                        content.Save();
                    }

                    //TODO: review this ... this is a temporary solution
                    var wfContent = Node.LoadNode(content.Id);

                    //start workflow
                    InstanceManager.Start(wfContent as WorkflowHandlerBase);

                }
                catch (Exception ex)
                {
                    //cleanup: delete the instance if it was saved before the error
                    if (content.Id != 0)
                    {
                        using (new SystemAccount())
                        {
                            content.ForceDelete();
                        }
                    }

                    Logger.WriteException(ex);
                    this.ContentView.ContentException = ex;

                    return;
                }

                if (!string.IsNullOrEmpty(ConfirmContentPath))
                {
                    //if confirm page or content is given, redirect there
                    var confirmBrowseAction = Helpers.Actions.BrowseUrl(Content.Load(ConfirmContentPath));
                    if (!string.IsNullOrEmpty(confirmBrowseAction))
                        HttpContext.Current.Response.Redirect(confirmBrowseAction, false);
                }

                CallDone(false);
            }
        }
    }
}