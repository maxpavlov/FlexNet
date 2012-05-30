using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Portlets.Controls
{
    public partial class EmailForgottenPassword : UserControl
    {
        public event EventHandler Click;

        public string ResetEmailAddress
        {
            get
            {
                TextBox resetEmailTextBox = this.FindControl("ResetEmailAddress") as TextBox;
                if (resetEmailTextBox == null)
                    return null;
                return resetEmailTextBox.Text;
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
            Button b = this.FindControl("ResetPasswordButton") as Button;
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