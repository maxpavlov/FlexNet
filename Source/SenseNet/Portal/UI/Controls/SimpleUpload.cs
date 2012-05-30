using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:SimpleUpload ID=\"SimpleUpload1\" runat=\"server\"></{0}:SimpleUpload")]
    public class SimpleUpload : FileUpload
    {
        private Button postBackButton;
        private HtmlInputHidden hiddenInput;

        public string SubmitButtonId { get; set; }

        public SimpleUpload()
        {
            postBackButton = new Button();            
            hiddenInput = new HtmlInputHidden();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            hiddenInput.ID = "ParentId";
            hiddenInput.Value = PortalContext.Current.ContextNodeHead.Id.ToString();
            
            postBackButton.ID = "PostBackButton";
            postBackButton.Text = "Upload";
            
            // note: this is not the traditional BackUrl as PortalContext.Current.BackUrl
            if (String.IsNullOrEmpty(SubmitButtonId))
            {
                SetPostBackUrl(postBackButton, false);
                Controls.Add(postBackButton);
            } else
                SetPostbackValues(SubmitButtonId);
            
            Controls.Add(hiddenInput);
        }

        private static void SetPostBackUrl(IButtonControl button, bool redirectToOriginalPath)
        {
            if (button == null)
                throw new ArgumentNullException("button");

            var backUrl = PortalContext.Current.RequestedUri.PathAndQuery;
            backUrl = redirectToOriginalPath ? PortalContext.Current.BackUrl : Uri.EscapeDataString(backUrl);
            var postBackUrl = String.IsNullOrEmpty(backUrl) ? "/UploadProxy.ashx" : String.Concat("/UploadProxy.ashx", "?back=", backUrl);
            button.PostBackUrl = postBackUrl;
        }

        private void SetPostbackValues(string buttonId)
        {
            var buttonControl = Parent.FindControlRecursive(buttonId) as Button;
            if (buttonControl != null)
            {
                SetPostBackUrl(buttonControl, true);
                buttonControl.Click += new EventHandler(buttonControl_Click);
            }
                
        }

        void buttonControl_Click(object sender, EventArgs e)
        {
            var i = 42;
        }




    }
}
