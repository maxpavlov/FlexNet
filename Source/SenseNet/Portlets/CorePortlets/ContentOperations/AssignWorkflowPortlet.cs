using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets
{
    public class AssignWorkflowPortlet : ContentAddNewPortlet
    {
        //========================================================================================= Controls

        private IButtonControl _button;
        protected IButtonControl AssignWorkflowButton
        {
            get { return _button ?? (_button = this.FindControlRecursive("AssignWorkflow") as IButtonControl); }
        }

        //========================================================================================= Constructor

        public AssignWorkflowPortlet()
        {
            this.Name = "Workflow assign";
            this.Description = "This portlet assignes a workflow to a content list (context bound)";

            this.HiddenPropertyCategories = new List<string> { EditorCategory.Cache };
        }

        //========================================================================================= Overrides

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (this.AssignWorkflowButton != null)
                this.AssignWorkflowButton.Click += AssignWorkflowButton_Click;
        }
        
        protected void AssignWorkflowButton_Click(object sender, EventArgs e)
        {
            if (this.ContentView == null)
                return;

            this.ContentView.NeedToValidate = true;
            this.ContentView.UpdateContent();

            var content = this.ContentView.Content;

            // check if user tries to create types other than workflow, that would be a security issue, since AllowCreationForEmptyAllowedContentTypes is skipped
            if (!content.ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Workflow"))
            {
                this.ContentView.ContentException = new SenseNetSecurityException("Only content of Workflow content type can be created with this portlet.");
                return;
            }

            if (this.ContentView.IsUserInputValid && content.IsValid)
            {
                try
                {
                    content["ContentWorkflow"] = true;
                    content.Save();

                    //TODO: 'add field' functionality must not be used until 
                    //'remove field' feature is not implemented
                    //AddContentListField(content);

                    CallDone(false);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                    this.ContentView.ContentException = ex;
                }
            }
        }

        protected override bool AllowCreationForEmptyAllowedContentTypes(string parentPath)
        {
            // only workflows are allowed to be created (AssignWorkflowButton_Click), so we do not rely upon allowed content types list
            // skip this check
            return true;
        }

        //========================================================================================= Helper methods

        private static void AddContentListField(ContentRepository.Content content)
        {
            if (content == null)
                return;

            var contentList = ContentList.GetContentListByParentWalk(content.ContentHandler);
            if (contentList == null)
                return;

            //build longtext field for custom status messages
            var fs = new LongTextFieldSetting
                         {
                             ShortName = "LongText",
                             Name =
                                 "#" + ContentNamingHelper.GetNameFromDisplayName(content.Name, content.DisplayName) +
                                 "_status",
                             DisplayName = content.DisplayName + " status",
                             Icon = content.Icon
                         };

            contentList.AddOrUpdateField(fs);
        }
    }
}
