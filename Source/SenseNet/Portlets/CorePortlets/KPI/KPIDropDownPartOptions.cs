using System;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.PortletFramework
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KPIDropDownPartOptions : EditorOptions
    {
        /* ================================================================================================================= Properties */
        public string Query { get; set; }
        public string MasterDropDownCss { get; set; }


        /* ================================================================================================================= Common constructors */
        public KPIDropDownPartOptions() {}
        public KPIDropDownPartOptions(string query, string masterDropDownCss)
        {
            this.Query = query;
            this.MasterDropDownCss = masterDropDownCss;
        }
    }
}
