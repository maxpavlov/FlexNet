using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Portlets.ContentHandlers;

namespace SenseNet.Portal.Portlets
{
    public class CancelEventPortlet : ContentDeletePortlet
    {
        public CancelEventPortlet()
        {
            this.Name = "Cancel event";
            this.Description = "This portlet handles event cancel operations (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }

        protected override void MessageControlButtonsAction(object sender, System.Web.UI.WebControls.CommandEventArgs e)
        {
            if (e.CommandName == "Yes")
            {
                var subs = ContextNode as EventRegistrationFormItem;
                subs.SendCancellationMail();
            }
            base.MessageControlButtonsAction(sender, e);
        }
    }
}
