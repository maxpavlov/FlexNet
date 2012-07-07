using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.Portal.Portlets
{
    public class ActionPresenterPortlet : ContextBoundPortlet
    {
        public enum IncludeBackUrlMode { Default, True, False }

        public ActionPresenterPortlet()
        {
            Name = "Action presenter";
            Description = "This portlet shows an ActionLink (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Portal);

            this.HiddenProperties.Add("Renderer");
        }

        private string _controlPath = "/Root/System/SystemPlugins/Controls/ActionPresenter.ascx";

        [WebBrowsable(true)]
        [Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ControlPath
        {
            get { return _controlPath; } 
            set { _controlPath = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Action name")]
        [WebDescription("Name of the Action")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(110)]
        public string ActionName { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Action text")]
        [WebDescription("Displayed text of the Action")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(120)]
        public string ActionText { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Parameters")]
        [WebDescription("Advanced action parameters. Ie. 'ContentTypeName=Article'. Separate more parameters with ';', '&' or ','")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(130)]
        public string ParameterString { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Icon path")]
        [WebDescription("The full path of the icon of the action link")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(140)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Icon)]
        public string IconUrl { get; set; }

        //default value is true
        private bool _iconVisible = true;

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Icon is visible")]
        [WebDescription("Controls the visibility of the action link icon")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(150)]
        public bool IconVisible 
        {
            get { return _iconVisible; }
            set { _iconVisible = value; } 
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Include back url in action link")]
        [WebDescription("If set to True, action url will contain a back url. System default is <strong>True</strong> except for the <strong>Browse</strong> action.")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(160)]
        public IncludeBackUrlMode IncludeBackUrl { get; set; }

        private ActionLinkButton _actionLink;
        protected ActionLinkButton ActionLink
        {
            get { return _actionLink ?? (_actionLink = this.FindControlRecursive("ActionLink") as ActionLinkButton); }
        }

        //================================================================ Overrides

        protected override void CreateChildControls()
        {
            Controls.Clear();

            try
            {
                var viewControl = Page.LoadControl(ControlPath) as UserControl;
                if (viewControl != null)
                {
                    Controls.Add(viewControl);
                    SetParameters();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteException(exc);
            }

            ChildControlsCreated = true;
        }

        //================================================================ Helper methods

        private void SetParameters()
        {
            if (ActionLink == null)
                return;

            ActionLink.ActionName = ActionName;
            ActionLink.Text = ActionText;
            ActionLink.ParameterString = ParameterString;
            ActionLink.IconUrl = IconUrl;
            ActionLink.IconVisible = IconVisible;

            if (this.IncludeBackUrl != IncludeBackUrlMode.Default)
                ActionLink.IncludeBackUrl = this.IncludeBackUrl == IncludeBackUrlMode.True;

            var ctx = GetContextNode();
            if (ctx != null)
                ActionLink.NodePath = ctx.Path;
        }
    }
}
