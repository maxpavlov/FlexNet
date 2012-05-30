using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Portlets
{
    internal abstract class UIControlsFactoryBase
    {
        public abstract Button CreateImportButton();
        public abstract TextBox CreateImportTextArea();
        public abstract DropDownList CreateZoneList();

        public abstract LiteralControl CreateLineBreak();
        public abstract Button CreateExportPortletButton();
        public abstract TextBox CreateExportResultTextArea();
        public abstract DropDownList CreatePortletList();

        public abstract Label CreateErrorMessageLabel();
        public abstract Label CreateFeedbackLabel();

        public abstract Button CreateExportAllButton();
        public abstract Button CreateImportAllButton();
        public abstract RadioButton CreateOverwriteRadioButton();
    }
}
