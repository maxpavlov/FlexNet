using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using System.Web;

namespace SenseNet.Portal.UI.Controls
{
    public class BackButton : Button
    {
        private string _target = "Parent";
        public string Target
        {
            get { return _target; }
            set { _target = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (string.IsNullOrEmpty(PortalContext.Current.BackUrl) && string.IsNullOrEmpty(this.Target))
            {
                this.OnClientClick = "window.close(); return false;";
                this.Text = "Close";
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (!string.IsNullOrEmpty(PortalContext.Current.BackUrl))
            {
                var pbase = Page as PageBase;
                if (pbase != null)
                    pbase.Done();

                return;
            }

            if (string.IsNullOrEmpty(this.Target)) 
                return;

            string redirectUrl = null;
            switch (this.Target.Trim().ToLower())
            {
                case "parent":
                    var contextNode = ContextBoundPortlet.GetContextNodeForControl(this) ??
                                      PortalContext.Current.ContextNode;
                    if (contextNode != null)
                        redirectUrl = contextNode.ParentPath;
                    break;
                case "currentsite":
                    redirectUrl = "/";
                    break;
                case "currentworkspace":
                    var cws = PortalContext.Current.ContextWorkspace;
                    if (cws != null)
                        redirectUrl = cws.Path;
                    break;
                case "currentlist":
                    var cl = PortalContext.Current.ContextWorkspace;
                    if (cl != null)
                        redirectUrl = cl.Path;
                    break;
            }

            if (!string.IsNullOrEmpty(redirectUrl))
                HttpContext.Current.Response.Redirect(redirectUrl, true);
        }
    }
}
