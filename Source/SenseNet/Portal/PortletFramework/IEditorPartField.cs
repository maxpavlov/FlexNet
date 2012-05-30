using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace SenseNet.Portal.UI.PortletFramework
{
    public interface IEditorPartField
    {
        string EditorPartCssClass { get; set; }
        string TitleContainerCssClass { get; set; }
        string TitleCssClass { get; set; }
        string DescriptionCssClass { get; set; }
        string ControlWrapperCssClass { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        EditorOptions Options { get; set; }
        string PropertyName { get; set; }

        void RenderTitle(HtmlTextWriter writer);
        void RenderDescription(HtmlTextWriter writer);
    }
}
