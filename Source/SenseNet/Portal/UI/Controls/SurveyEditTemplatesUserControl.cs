using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Schema;
using Content = SenseNet.ContentRepository.Content;
using SenseNet.ApplicationModel;

namespace SenseNet.Portal.UI.Controls
{
    public class SurveyEditTemplatesUserControl : System.Web.UI.UserControl
    {
        private string[] templateNames = { "Landing", "InvalidSurvey", "MailTemplate" };

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetControls();
        }

        private void SetControls()
        {
            var contextNode = PortalContext.Current.ContextNode;

            foreach (var templateName in templateNames)
            {
                var templateNode = contextNode.GetReference<Node>(string.Concat(templateName, "Page"));
                if (templateNode == null) continue;

                var templateContent = ContentRepository.Content.Create(templateNode);

                var placeHolder = this.FindControlRecursive(string.Concat("ph", templateName));
                var pageContentView = contextNode.GetReference<Node>("PageContentView").Path;
                var contentView = ContentView.Create(templateContent, this.Page, ViewMode.Browse, pageContentView);

                if (placeHolder == null || contentView == null) continue;

                placeHolder.Controls.Clear();
                placeHolder.Controls.Add(contentView);

                var btnEdit = this.FindControlRecursive(string.Concat("btnEdit", templateName)) as Button;

                if (btnEdit == null) continue;

                if (SecurityHandler.HasPermission(contextNode, new[] { PermissionType.AddNew, PermissionType.Save })
                    && SecurityHandler.HasPermission(templateNode, PermissionType.Save))
                {
                    btnEdit.Visible = true;
                }
            }
        }

        protected void BtnEdit_Click(object sender, EventArgs e)
        {
            var templateName = (sender as Button).CommandArgument;

            var contextNode = PortalContext.Current.ContextNode;
            var templateNode = contextNode.GetReference<Node>(string.Concat(templateName, "Page"));
            var templateNodePath = templateNode.Path;

            if (templateNodePath.Contains("System/SystemPlugins"))
            {
                var targetPath = RepositoryPath.Combine(contextNode.Path, "configuration");

                if (!Node.Exists(targetPath))
                {
                    var folder = new Folder(contextNode, "Folder") { Name = "configuration", DisplayName = "Configuration" };
                    folder.Save();
                }

                var wcd = NodeType.CreateInstance("WebContentDemo", Node.LoadNode(targetPath));

                wcd["Name"] = templateNode["Name"];
                wcd["DisplayName"] = templateNode["DisplayName"];
                wcd["Subtitle"] = templateNode["Subtitle"];
                wcd["Body"] = templateNode["Body"];
                wcd.Save();

                templateNodePath = wcd.Path;

                contextNode.SetReference(string.Concat(templateName, "Page"), wcd);
                contextNode.Save();
            }

            Response.Redirect(ActionFramework.GetActionUrl(templateNodePath, "EditSurveyTemplate", PortalContext.Current.BackUrl));
        }
    }
}