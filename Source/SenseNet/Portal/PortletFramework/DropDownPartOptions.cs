using System;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.PortletFramework
{
    public enum DropDownCommonType
    {
        Simple = 0,
        ContentTypeDropdown
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DropDownPartOptions : EditorOptions
    {
        /* ================================================================================================================= Properties */
        public DropDownCommonType CommonType { get; set; }
        public string Query { get; set; }
        public string CustomControlCss { get; set; }


        /* ================================================================================================================= Common constructors */
        public DropDownPartOptions()
        {
        }
        public DropDownPartOptions(DropDownCommonType commonType)
        {
            this.CommonType = commonType;
        }
        public DropDownPartOptions(string query)
        {
            this.Query = query;
        }
        public DropDownPartOptions(string query, string customControlCss)
        {
            this.Query = query;
            this.CustomControlCss = customControlCss;
        }
    }
}
