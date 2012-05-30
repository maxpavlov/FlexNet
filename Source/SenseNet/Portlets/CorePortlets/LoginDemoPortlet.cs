using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using System.Web.Security;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Portal.Portlets
{
    public sealed class LoginDemoPortlet : PortletBase
    {
        private const string LAYOUTCONTROLPATH = "/Root/System/SystemPlugins/Portlets/LoginDemo/layout.ascx";

        public LoginDemoPortlet()
        {
            this.Name = "Login Demo";
            this.Description = "A demo login portlet displaying different user accounts to simulate immediate login.";
            this.Category = new PortletCategory(PortletCategoryType.Other);
        }

        protected override void CreateChildControls()
        {
            var layout = this.Page.LoadControl(LAYOUTCONTROLPATH);
            this.Controls.Add(layout);

            foreach (var control in layout.Controls)
            {
                var link = control as LinkButton;
                if (link == null)
                    continue;

                link.Click += new EventHandler(link_Click);
            }

            this.ChildControlsCreated = true;
        }

        void link_Click(object sender, EventArgs e)
        {
            var link = sender as LinkButton;
            if (link == null)
                return;

            var fullUserName = link.CommandArgument;

            if (string.IsNullOrEmpty(fullUserName))
                return;

            var slashIndex = fullUserName.IndexOf('\\');
            var domain = fullUserName.Substring(0, slashIndex);
            var username = fullUserName.Substring(slashIndex + 1);

            var user = User.Load(domain, username) as IUser;
            if (user == null)
                return;

            FormsAuthentication.RedirectFromLoginPage(user.Name, false);
        }
    }
}