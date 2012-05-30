using System;
using System.Collections.Generic;
using System.Web.UI;
using SenseNet.Portal.UI;

namespace SenseNet.Portal.Portlets.Controls
{
    public partial class TagCloudControl : UserControl
    {
        protected global::System.Web.UI.WebControls.Repeater TagCloudRepeater;

        public string SearchPortletPath { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            UITools.AddScript("$skin/scripts/sn/SN.TagCloud.js");
        }
    }
}