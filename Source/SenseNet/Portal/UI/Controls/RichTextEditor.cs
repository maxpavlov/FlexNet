using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: WebResource("SenseNet.Portal.UI.Controls.RichTextEditor.js", "application/x-javascript")]
[assembly: TagPrefix("SenseNet.Portal.UI.Controls", "sn")]

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:RichTextEditor ID=\"RichTextEditor1\" runat=server></{0}:RichTextEditor>")]
    public class RichTextEditor : TextBox, IScriptControl, IPostBackDataHandler
    {
        // Members and properties /////////////////////////////////////////////////
        
        
        public RichTextEditor()
        {
            Theme_advanced_resize_horizontal = true;
        }
        
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Options { get; set; }

        [PersistenceMode(PersistenceMode.Attribute)]
        public string ConfigPath { get; set; }
        
        
        [Obsolete("DraggableToolbar property and its functionality will be removed by final release.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool DraggableToolbar { get; set; }
        
        #region backward compatibility

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Plugins { get; set; }
        
        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme { get; set;}

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme_advanced_buttons1 { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme_advanced_buttons2 { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme_advanced_buttons3 { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme_advanced_buttons4 { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme_advanced_toolbar_location { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme_advanced_toolbar_align { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme_advanced_statusbar_location { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme_advanced_path_location { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public bool Theme_advanced_resizing { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true), Category("General")]
        public bool Theme_advanced_resize_horizontal { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Content_css { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public bool Auto_resize { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Table_styles { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Table_cell_styles { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Table_row_styles { get; set; }
        
        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Theme_advanced_styles { get; set; }

        [Obsolete("Use Options property instead.")]
        [Browsable(true)]
        [Category("General")]
        public string Font_size_style_values { get; set; }

        #endregion

        public override TextBoxMode TextMode
        {
            get { return TextBoxMode.MultiLine; }
        }

        // Events /////////////////////////////////////////////////////////////////
        protected override void OnInit(EventArgs e)
        {
            UITools.AddPickerCss();
            UITools.AddScript(UITools.ClientScriptConfigurations.SNPickerPath);
            UITools.AddScript(UITools.ClientScriptConfigurations.TinyMCEPath);
            base.OnInit(e);
            this.CssClass = "snpeRTE";
        }

        protected override void OnPreRender(EventArgs e)
        {
            ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            ScriptManager.RegisterStartupScript(
                this.Page,
                typeof (System.Web.UI.Page),
                String.Concat("tinymce_gzip_script"),
                this.GetGzipInitScript(),
                true
                );
            ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
            base.Render(writer);
        }

        // IScriptControl /////////////////////////////////////////////////////////
        protected virtual IEnumerable<ScriptReference> GetScriptReferences()
        {
            var referenceExtenderScript = CreateScriptReferenceExtender();

            return new[] {referenceExtenderScript};
        }
        protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            var descriptor = CreateDescriptor();

            return new ScriptDescriptor[] {descriptor};
        }
        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences()
        {
            return GetScriptReferences();
        }
        IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors()
        {
            return GetScriptDescriptors();
        }

        // IPostBackDataHandler ///////////////////////////////////////////////////
        bool IPostBackDataHandler.LoadPostData(string postDataKey,
                                               System.Collections.Specialized.NameValueCollection postCollection)
        {
            Text = postCollection[postDataKey];
            return true;
        }
        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            return;
        }

        // Internals //////////////////////////////////////////////////////////////
        private ScriptReference CreateScriptReferenceExtender()
        {
            var webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(this.GetType(),
                                                                          "SenseNet.Portal.UI.Controls.RichTextEditor.js");
            return new ScriptReference
                       {
                           Path = webResourceUrl,
                           NotifyScriptLoaded = true
                       };
        }

        private ScriptControlDescriptor CreateDescriptor()
        {
            var descriptor = new ScriptControlDescriptor("SenseNet.Portal.UI.Controls.RichTextEditor", ClientID);
            SetScriptControlProperties(descriptor);
            return descriptor;
        }

        private void SetScriptControlProperties(ScriptComponentDescriptor descriptor)
        {
            if (!String.IsNullOrEmpty(this.ConfigPath))
            {
                Options = RichText.GetOptions(ConfigPath);
                descriptor.AddProperty("options", this.Options);
            }
            else
                descriptor.AddProperty("options",this.Options);
            
            if (!String.IsNullOrEmpty(Options))
                return;

            descriptor.AddProperty("theme", Theme);
            descriptor.AddProperty("plugins", Plugins);
            descriptor.AddProperty("theme_advanced_buttons1", Theme_advanced_buttons1);
            descriptor.AddProperty("theme_advanced_buttons2", Theme_advanced_buttons2);
            descriptor.AddProperty("theme_advanced_buttons3", Theme_advanced_buttons3);
            descriptor.AddProperty("theme_advanced_buttons4", Theme_advanced_buttons4);
            descriptor.AddProperty("theme_advanced_toolbar_location", Theme_advanced_toolbar_location);
            descriptor.AddProperty("theme_advanced_path_location", Theme_advanced_path_location);
            descriptor.AddProperty("theme_advanced_resizing", Theme_advanced_resizing);
            descriptor.AddProperty("theme_advanced_resize_horizontal", Theme_advanced_resize_horizontal);
            descriptor.AddProperty("theme_advanced_toolbar_align", Theme_advanced_toolbar_align);
            descriptor.AddProperty("content_css", Content_css);
            descriptor.AddProperty("auto_resize", Auto_resize);
            descriptor.AddProperty("table_styles", Table_styles);
            descriptor.AddProperty("table_cell_styles", Table_cell_styles);
            descriptor.AddProperty("table_row_styles", Table_row_styles);
            descriptor.AddProperty("theme_advanced_styles", Theme_advanced_styles);
            descriptor.AddProperty("font_size_style_values", String.IsNullOrEmpty(Font_size_style_values) ? string.Empty : Font_size_style_values);
        }

        private string GetGzipInitScript()
        {
            var sb = new StringBuilder();

            sb.Append("function createTinyMCE_GZ() {");
            sb.Append("if (typeof(tinyMCE_GZ) !== 'undefined') {");
            sb.Append("tinyMCE_GZ.init({");
            sb.Append(Environment.NewLine);
            sb.Append(String.Format("plugins:'{0}',", this.Plugins));
            sb.Append(Environment.NewLine);
            sb.Append(String.Format("themes : 'simple,advanced',"));
            sb.Append(String.Format("languages : 'en', disk_cache : true, debug : false"));
            sb.Append(Environment.NewLine);
            sb.Append("});");
            sb.Append(Environment.NewLine);
            sb.Append("};");
            sb.Append(Environment.NewLine);
            sb.Append("};");
            sb.Append(Environment.NewLine);
            sb.Append("createTinyMCE_GZ();");

            return sb.ToString();
        }
    }
}
