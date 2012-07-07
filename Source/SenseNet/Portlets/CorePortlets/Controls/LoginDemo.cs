using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.Security;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Portlets.Controls
{
    public class LoginDemo : UserControl
    {
        protected override void CreateChildControls()
        {
            foreach (var control in this.Controls)
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

            //FormsAuthentication.RedirectFromLoginPage(user.Name, false);
            FormsAuthentication.SetAuthCookie(user.Username, false);
            Response.Redirect(Request.RawUrl);
        }
    }
}
