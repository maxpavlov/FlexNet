using System;
using System.Linq;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository;
using SenseNet.Portal.UI;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.Portal.Portlets
{
    public class TagEditPortlet : ContentEditorPortlet
    {
        public TagEditPortlet()
        {
            Name = "Tag edit";
            Description = "Portlet for editing tags (context bound)";
            Category = new PortletCategory(PortletCategoryType.Application);
        }

        protected override void OnCommandButtons(CommandButtonsEventArgs e)
        {
            switch (e.ButtonType)
            {
                case CommandButtonType.CheckoutSave:
                case CommandButtonType.CheckoutSaveCheckin:
                case CommandButtonType.Publish:
                case CommandButtonType.Save:
                case CommandButtonType.SaveCheckin:
                    e.Cancel = true;
                    this.HandleTagReplacement(e.ContentView);
                    break;
            }
        }

        private void HandleTagReplacement(ContentView contentView)
        {
            var content = contentView.Content;
            var oldTag = content["DisplayName"].ToString();
            this.OnSave(contentView, content);
            var node = GetContextNodeForControl(this);
            if (oldTag != node.DisplayName)
            {
                TagManager.ReplaceTag(oldTag, node.DisplayName, Page.Request.Params["Paths"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }
        }

        private void OnSave(ContentView contentView, Content content)
        {
            contentView.UpdateContent();
            if (contentView.IsUserInputValid && content.IsValid)
            {
                try
                {
                    content.Save();
                }
                catch (Exception ex) //logged
                {
                    Logger.WriteException(ex);
                    contentView.ContentException = ex;
                }
            }
        }
    }
}
