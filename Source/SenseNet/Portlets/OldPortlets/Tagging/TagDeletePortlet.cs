using System;
using System.Linq;
using SenseNet.ContentRepository.Storage;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets
{
    public class TagDeletePortlet : ContentDeletePortlet
    {
        public TagDeletePortlet()
        {
            Name = "Tag delete";
            Description = "Portlet for deleting tags (context bound)";
            Category = new PortletCategory(PortletCategoryType.Application);
        }

        protected override void CreateChildControls()
        {
            UserInterfacePath = "/Root/System/SystemPlugins/Portlets/TagAdmin/DeleteConfirmation.ascx";
            base.CreateChildControls();

        }

        protected override void MessageControlButtonsAction(object sender, CommandEventArgs e)
        {
            if (Page.Request.QueryString["toBlacklist"] == "true")
            {
                var contextNode = GetContextNode();
                var blacklistNode = Node.LoadNode(contextNode.Parent.Path + "/Blacklist");
                blacklistNode["BlackListItems"] += " " + contextNode.DisplayName;
                blacklistNode.Save();
            }

            var pathList = Page.Request.Params["Paths"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            TagManager.ReplaceTag(GetContextNode().DisplayName, String.Empty, pathList);
            base.MessageControlButtonsAction(sender, e);
        }
    }
}
