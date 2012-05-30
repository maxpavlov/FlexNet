using System;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI;
using Repo = SenseNet.ContentRepository;

namespace SenseNet.Workflow.UI
{
    public class ApprovePortlet : ContextBoundPortlet
    {
        public ApprovePortlet()
        {
            Name = "Workflow based approval portlet";
            Description = "Use this Portlet with workflow approval tasks";
            this.Category = new PortletCategory(PortletCategoryType.Workflow);
        }

        private Button _approveButton;
        protected Button ApproveButton
        {
            get
            {
                return _approveButton ?? (_approveButton = this.FindControlRecursive("Approve") as Button);
            }
        }

        private Button _rejectButton;
        protected Button RejectButton
        {
            get
            {
                return _rejectButton ?? (_rejectButton = this.FindControlRecursive("Reject") as Button);
            }
        }

        protected override void CreateChildControls()
        {
            var content = Repo.Content.Create(ContextNode);

            if (ContextNode.NodeType.IsInstaceOfOrDerivedFrom("ApprovalWorkflowTask"))
            {
                var view = ContentView.Create(content, Page, ViewMode.Browse);
                Controls.Add(view);

                if (RejectButton != null)
                    RejectButton.Click += RejectButton_Click;

                if (ApproveButton != null)
                    ApproveButton.Click += ApproveButton_Click;
            }

            ChildControlsCreated = true;
        }

        private void RejectButton_Click(object sender, EventArgs e)
        {
            ContextNode["Result"] = "no";
            ContextNode.Save();
            CallDone();
        }

        private void ApproveButton_Click(object sender, EventArgs e)
        {
            ContextNode["Result"] = "yes";
            ContextNode.Save();
            CallDone();
        }
    }
}
