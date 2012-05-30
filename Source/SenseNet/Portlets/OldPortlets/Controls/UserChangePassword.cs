using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace SenseNet.Portal.Portlets.Controls
{
    public partial class UserChangePassword : UserControl
    {
        public event EventHandler Click;

        public string NewPassword
        {
            get
            {
                TextBox newPasswordBox = this.FindControl("NewPassword") as TextBox;
                //TextBox newPasswordText = this.Controls
                //    .OfType<TextBox>()
                //    .Where(tb => tb.ID == "NewPassword")
                //    .FirstOrDefault();
                 
                if (newPasswordBox == null)
                    return null;
                //
                //  TODO: prepare newpassword
                //
                return newPasswordBox.Text;
            }
        }

        public string ReenteredNewPassword
        {
            get
            {
                TextBox reenteredPasswordBox = this.FindControl("ReenteredNewPassword") as TextBox;
                if (reenteredPasswordBox == null)
                    return null;
                //
                //  TODO: prepare reenteredPasswordBox
                //
                return reenteredPasswordBox.Text;
            }
        }

        public string Message
        {
            get
            {
                Label errMsg = this.FindControl("Message") as Label;
                if (errMsg == null)
                    return null;

                return errMsg.Text;
            }
            set
            {
                Label errMsg = this.FindControl("Message") as Label;
                if (errMsg == null)
                    return;

                errMsg.Text = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Button b = this.FindControl("ChangePasswordButton") as Button;
            if (b != null)
                b.Click += new EventHandler(b_Click);
        }

        void b_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        protected virtual void OnClick(EventArgs e)
        {
            if (Click != null)
                Click(this, e);
        }
    }
}