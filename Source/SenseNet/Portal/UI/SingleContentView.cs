using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI
{
    public class SingleContentView : ContentView, INamingContainer
    {
        protected virtual void Click(object sender, EventArgs e)
        {
            string actionName = "";
            IButtonControl button = sender as IButtonControl;
            if (button != null)
                actionName = button.CommandName;

            this.OnUserAction(sender, actionName, "Click");
        }
    }
}