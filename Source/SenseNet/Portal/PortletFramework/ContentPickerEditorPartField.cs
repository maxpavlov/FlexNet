using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class ContentPickerEditorPartField : TextBox, IEditorPartField
    {
        /* ====================================================================================================== IEditorPartField */
        public EditorOptions Options { get; set; }
        public string EditorPartCssClass { get; set; }
        public string TitleContainerCssClass { get; set; }
        public string TitleCssClass { get; set; }
        public string DescriptionCssClass { get; set; }
        public string ControlWrapperCssClass { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PropertyName { get; set; }
        public void RenderTitle(HtmlTextWriter writer)
        {
            writer.Write(String.Format(@"<div class=""{0}""><span class=""{1}"" title=""{5}"">{2}</span><br/><span class=""{3}"">{4}</span></div>", TitleContainerCssClass, TitleCssClass, Title, DescriptionCssClass, Description, PropertyName));
        }
        public void RenderDescription(HtmlTextWriter writer)
        {
        }


        /* ====================================================================================================== Properties */
        public Button OpenPickerButton { get; set; }

        private ContentPickerEditorPartOptions _contentPickerOptions;
        public ContentPickerEditorPartOptions ContentPickerOptions
        {
            get
            {
                if (_contentPickerOptions == null)
                {
                    _contentPickerOptions = this.Options as ContentPickerEditorPartOptions;
                    if (_contentPickerOptions == null)
                        _contentPickerOptions = new ContentPickerEditorPartOptions();
                }
                return _contentPickerOptions;
            }
        }


        /* ====================================================================================================== Methods */
        public ContentPickerEditorPartField()
        {
            OpenPickerButton = new Button();
        }
        protected override void OnInit(EventArgs e)
        {
            UITools.AddPickerCss();
            UITools.AddScript(UITools.ClientScriptConfigurations.SNPickerPath);

            OpenPickerButton.ID = String.Concat(ID, "ReferenceEditorButton");
            OpenPickerButton.Text = "...";
            base.OnInit(e);
        }
        protected override void Render(HtmlTextWriter writer)
        {
            OpenPickerButton.OnClientClick = GetOpenContentPickler();
            
            var clientId = String.Concat(ClientID, "Div");
            string htmlPart = @"<div class=""{0}"" id=""{1}"">";
            writer.Write(String.Format(htmlPart, EditorPartCssClass, clientId));
            RenderTitle(writer);
            ToolTip = Description;

            base.Render(writer);
            OpenPickerButton.RenderControl(writer);

            writer.Write("</div>");
        }
        private string GetOpenContentPickler()
        {
            var script = string.Format(@"javascript: var selectedNodePaths = $('#{0}').val(); SN.PickerApplication.open({{ {1} }}); return false;", this.ClientID, GetPickerParameters());
            return script;
        }
        private string GetPickerParameters()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(ContentPickerOptions.TreeRoots))
                sb.Append(string.Format("TreeRoots: {0},", GetArrayParams(ContentPickerOptions.TreeRoots) ));
            if (!string.IsNullOrEmpty(ContentPickerOptions.DefaultPath))
                sb.Append(string.Format("DefaultPath: '{0}',", ContentPickerOptions.DefaultPath ));
            if (!string.IsNullOrEmpty(ContentPickerOptions.AllowedContentTypes))
                sb.Append(string.Format("AllowedContentTypes: {0},", GetArrayParams(ContentPickerOptions.AllowedContentTypes) ));
            if (!string.IsNullOrEmpty(ContentPickerOptions.DefaultContentTypes))
                sb.Append(string.Format("DefaultContentTypes: {0},", GetArrayParams(ContentPickerOptions.DefaultContentTypes) ));
            if (!string.IsNullOrEmpty(ContentPickerOptions.TargetPath))
                sb.Append(string.Format("TargetPath: '{0}',", ContentPickerOptions.TargetPath ));
            if (!string.IsNullOrEmpty(ContentPickerOptions.TargetField))
                sb.Append(string.Format("TargetField: '{0}',", ContentPickerOptions.TargetField ));

            var pars = sb.ToString();

            var callBack =
                string.Format(
                    "function(resultData) {{ if (!resultData) return; $('#{0}').val(resultData[0].Path); }}",
                    ClientID);
            var baseConfig = string.Format("MultiSelectMode: 'none', AdminDialog: 'true', SelectedNodePaths: [selectedNodePaths], callBack: {0}", callBack);

            return string.Concat(pars, baseConfig);
        }
        private static string GetArrayParams(string pars)
        {
            var strings = pars.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            var strings2 = strings.Select(s => string.Concat("'", s, "'")).ToArray();
            return string.Concat("[", string.Join(",", strings2), "]");
        }
    }
}
