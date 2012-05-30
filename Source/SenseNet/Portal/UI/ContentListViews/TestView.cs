using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace SenseNet.Portal.UI.ContentListViews
{
    public partial class TestView : UserControl
    {
        Button testButton;
        Label testLabel;

        protected override void CreateChildControls()
        {
            testButton = this.FindControl("TestButton") as Button;
            testLabel = this.FindControl("TestLabel") as Label;

            testButton.Click += new EventHandler(testButton_Click);

            this.ChildControlsCreated = true;
        }

        protected void testButton_Click(object sender, EventArgs e)
        {
            testLabel.Text = "Megnyomtad a gombot, kapsz egy libacombot.";
        }
    }
}
