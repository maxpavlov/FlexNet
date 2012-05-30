using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Messaging;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    public class NotificationDeletePortlet : ContextBoundPortlet
    {
        public NotificationDeletePortlet()
        {
            this.Name = "Notification delete";
            this.Description = "This portlet allows you to delete a subscription";
            this.Category = new PortletCategory("Notification", "Portlets for handling notifications");
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            
            var contentPath = HttpContext.Current.Request["ContentPath"];
            if (string.IsNullOrEmpty(contentPath))
            {
                //subscription for the current content
                contentPath = this.ContextNode.Path;
            }

            var node = Node.LoadNode(contentPath);
            var userPath = HttpContext.Current.Request["UserPath"] ?? string.Empty;
            var user = string.IsNullOrEmpty(userPath) ? User.Current as User : Node.Load<User>(userPath);

            if (node == null || user == null)
                return;

            Subscription.UnSubscribe(user, node);
            
            CallDone();
        }
    }
}
