using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SN = SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.Portal;
using SenseNet.Portal.Portlets;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Search.Parser;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Schema;
using System.Web.UI.WebControls;
using SenseNet.Portal.Virtualization;
using SenseNet.Messaging;

namespace SenseNet.Portal.Portlets
{
    public class NotificationListPortlet : ContextBoundPortlet
    {
        private string _contentViewPath = "/Root/System/SystemPlugins/Notifications/NotificationList.ascx";

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("View path")]
        [WebDescription("Path of the .ascx user control which provides the elements of the portlet")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ContentViewPath
        {
            get { return _contentViewPath; }
            set { _contentViewPath = value; }
        }

        private User _user;
        public User User
        {
            get { return _user ?? (_user = this.ContextNode as User ?? User.Current as User); }
        }

        public bool HasPermission
        {
            get
            {
                return this.User.Id == User.Current.Id ||
                       this.User.Security.HasPermission(PermissionType.Save);
            }
        }

        public NotificationListPortlet()
        {
            Name = "Notification list";
            Description = "A portlet for displaying the notifications subscribed by the current user";
            this.Category = new PortletCategory("Notification", "Portlets for handling notifications");
        }

        protected override object GetModel()
        {
            var ctdFile = Node.Load<ContentType>("/Root/System/Schema/ContentTypes/GenericContent/Subscription");
            var ctdStream = ctdFile.Binary.GetStream();
            var subscriptionCtd = Tools.GetStreamString(ctdStream);

            return (from subscripton in Subscription.GetSubscriptionsByUser(this.User.Path)
                    where !string.IsNullOrEmpty(subscripton.ContentPath) && Node.Exists(subscripton.ContentPath)
                    select SN.Content.Create(subscripton, subscriptionCtd)).ToList();
        }

        protected override void CreateChildControls()
        {
            //base.CreateChildControls();

            if (Cacheable && CanCache && IsInCache)
                return;

            if (!this.HasPermission)
            {
                Controls.Add(new LiteralControl(HttpContext.GetGlobalResourceObject("Notification", "NotEnoughPermissions") as string));
                return;
            }

            try
            {
                var modelData = GetModel() as IEnumerable<SN.Content>;

                var viewControl = Page.LoadControl(ContentViewPath);
                if (viewControl != null)
                {
                    var contentList = viewControl.FindControl("ContentList");
                    if (contentList != null)
                    {
                        ContentQueryPresenterPortlet.DataBindingHelper.SetDataSourceAndBind(contentList, modelData);
                    }

                    Controls.Add(viewControl);

                    ChildControlsCreated = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                Controls.Clear();
                Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
            }
        }
    }
}
