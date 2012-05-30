using System.Web.UI;

namespace SenseNet.Portal.UI.Controls
{
    public interface ITemplateFieldControl
    {
        Control GetInnerControl();
        Control GetLabelForDescription();
        Control GetLabelForTitleControl();
    }
}