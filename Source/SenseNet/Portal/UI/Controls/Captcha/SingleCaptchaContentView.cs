using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI;

namespace SenseNet.Portal.UI.Controls.Captcha
{
    public class SingleCaptchaContentView : SingleContentView
    {
        protected override void Click(object sender, EventArgs e)
        {
            var cc = FindControl("Captcha") as CaptchaControl;
            var ev = FindControl("ErrorView1") as ErrorView;
            if (cc != null && !cc.UserValidated)
            {
                if (ev != null)
                {
                    this.ContentException = new Exception(((System.Web.UI.IValidator) cc).ErrorMessage);
                }
                return;    
            }

            base.Click(sender, e);

        }
    }
}