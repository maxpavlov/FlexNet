using System;
using System.Web;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository;
using Content = SenseNet.ContentRepository.Content;

namespace SenseNet.Workflow.UI
{
    public class ConfirmPortlet : ContextBoundPortlet
    {
        //========================================================================================= Constructor

        public ConfirmPortlet()
        {
            Name = "Workflow based confirmation portlet";
            Description = "Use this Portlet for e-mail confirmations and similar tasks";
            this.Category = new PortletCategory(PortletCategoryType.Workflow);
        }

        protected override void CreateChildControls()
        {
            var content = Content.Create(ContextNode);
            var view = ContentView.Create(content, Page, ViewMode.Browse);
            Controls.Add(view);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                var confirmItem = ContextNode as GenericContent;
                if (confirmItem != null)
                {
                    using (new SystemAccount())
                    {
                        confirmItem.SetProperty("Confirmed", 1);
                        confirmItem.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }
    }
}
