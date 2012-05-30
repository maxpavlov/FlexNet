using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Portlets
{
    internal class UIControlsFactory : UIControlsFactoryBase
    {
        public override Button CreateImportButton()
        {
            return new Button {ID = "ImportButton", Text = "Import portlet"};
        }

        public override TextBox CreateImportTextArea()
        {
            return new TextBox
                       {
                           ID = "ImportTextArea",
                           TextMode = TextBoxMode.MultiLine,
                           Rows = 10,
                           Columns = 30
                       };
        }

        public override LiteralControl CreateLineBreak()
        {
            return new LiteralControl("<br />");
        }

        public override Button CreateExportPortletButton()
        {
            return new Button {ID = "ExportButton", Text = "Export portlet"};
        }

        public override DropDownList CreatePortletList()
        {
            return new DropDownList {ID = "PortletList"};
        }

        public override TextBox CreateExportResultTextArea()
        {
            return new TextBox
                       {
                           ID = "ExportResultTextArea",
                           TextMode = TextBoxMode.MultiLine,
                           Rows = 10,
                           Columns = 30
                       };
        }

        public override DropDownList CreateZoneList()
        {
            return new DropDownList {ID = "ZoneList"};
        }

        public override Label CreateErrorMessageLabel()
        {
            return new Label {ID = "ErrorMessage"};
        }

        public override Button CreateExportAllButton()
        {
            return new Button
                       {ID = "ExportAll", Text = "Export all", ToolTip = "Export all portlet beneath the current page."};
        }

        public override Button CreateImportAllButton()
        {
            return new Button
                       {
                           ID = "ImportAll",
                           Text = "Import all",
                           ToolTip = "Import all portlet from underneath the current page into the selected zone."
                       };
        }

        public override Label CreateFeedbackLabel()
        {
            return new Label {ID = "FeedbackMessage"};
        }

        public override RadioButton CreateOverwriteRadioButton()
        {
            return new RadioButton { ID = "OverwritePorlet", Checked = false, Text = "Overwrite existing portlet:" };
        }
    }
}