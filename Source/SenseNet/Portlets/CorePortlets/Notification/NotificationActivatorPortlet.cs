using System;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.Messaging;
using SNCR = SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using System.Web;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.Portlets
{
    public class NotificationActivatorPortlet : ContextBoundPortlet
    {
        public NotificationActivatorPortlet()
        {
            this.Name = "Notification activator";
            this.Description = "This portlet allows you to set your subscription to active or inactive";
            this.Category = new PortletCategory("Notification", "Portlets for handling notifications");

            this.HiddenProperties.Add("Renderer");
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var contentPath = HttpContext.Current.Request["ContentPath"] ?? string.Empty;
            var userPath = HttpContext.Current.Request["UserPath"] ?? string.Empty;
            var isActive = HttpContext.Current.Request["IsActive"] ?? string.Empty;
            var node = Node.LoadNode(contentPath);
            var user = string.IsNullOrEmpty(userPath) ? User.Current as User : Node.Load<User>(userPath);

            if (string.IsNullOrEmpty(isActive) || node == null)
                return;

            if (isActive.ToLower().Equals("false"))
                Subscription.ActivateSubscription(user, node);
            else
                Subscription.InactivateSubscription(user, node);

            CallDone();
        }
    }
}
