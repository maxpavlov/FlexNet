using System;
using System.Collections.Generic;
using System.Web;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Diagnostics;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    public class ContentCheckInPortlet : ContextBoundPortlet
    {
        public ContentCheckInPortlet()
        {
            this.Name = "Checkin";
            this.Description = "This portlet allows a content to be checked in (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);

            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Cache };
            this.HiddenProperties.Add("Renderer");
            Cacheable = false;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var genericContent = this.ContextNode as GenericContent;
            var cic = CheckInCommentsMode.None;

            if (genericContent != null)
            {
                try
                {
                    cic = genericContent.CheckInCommentsMode;

                    //take action only if the action name is correct and 
                    //checkin comments are not needed
                    if (!string.IsNullOrEmpty(PortalContext.Current.ActionName) && 
                        PortalContext.Current.ActionName.ToLower() == "checkin" &&
                        cic == CheckInCommentsMode.None)
                        genericContent.CheckIn();
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }
            }

            //return only if there is no need to ask for checkin comments
            if (cic == CheckInCommentsMode.None)
                CallDone();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            var genericContent = this.ContextNode as GenericContent;
            if (genericContent != null && genericContent.CheckInCommentsMode > CheckInCommentsMode.None && SavingAction.HasCheckIn(genericContent))
            {
                var content = Content.Create(genericContent);

                //we need to reset the comments field before displaying it
                content["CheckInComments"] = string.Empty;

                var contentView = ContentView.Create(content, Page, ViewMode.InlineEdit, RepositoryPath.Combine(Repository.ContentViewFolderName, "CheckIn.ascx"));

                if (contentView != null)
                {
                    contentView.CommandButtonsAction += ContentView_CommandButtonsAction;
                    Controls.Add(contentView);
                }
            }

            ChildControlsCreated = true;
        }

        protected void ContentView_CommandButtonsAction(object sender, CommandButtonsEventArgs e)
        {
            if (e.ButtonType == CommandButtonType.Cancel)
                return;

            e.ContentView.UpdateContent();

            var cic = e.ContentView.Content["CheckInComments"] as string;
            var gc = e.ContentView.ContentHandler as GenericContent;

            if (gc == null || (gc.CheckInCommentsMode == CheckInCommentsMode.Compulsory && string.IsNullOrEmpty(cic)))
            {
                e.ContentView.ContentException = new Exception(HttpContext.GetGlobalResourceObject("Portal", "CheckInCommentsCompulsory") as string);
                e.Cancel = true;
            }
        }
    }
}
