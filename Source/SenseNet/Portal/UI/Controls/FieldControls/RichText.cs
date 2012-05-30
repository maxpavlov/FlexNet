using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI.Controls;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:RichText ID=\"RichText1\" runat=server></{0}:RichText>")]
    public class RichText : FieldControl, ITemplateFieldControl
    {
        /* ============================================================================= Members */
        private static readonly string GlobalOptionsPath = "/Root/System/SystemPlugins/Controls/RichTextConfig.config";
        private static readonly string GlobalAjaxOptionsPath = "/Root/System/SystemPlugins/Controls/AjaxRichTextConfig.config";
        internal static readonly StringBuilder AdvancedOptions = InitalizeAdvancedOptions();
        internal static readonly StringBuilder SimpleOptions = InitializeSimpleOptions();
        private static readonly string markupString = "sncontentintext";
        protected string PlaceHolderID;
        private readonly RichTextEditor _inputTextBox;
        private int _maxLength;


        /* ============================================================================= Properties */
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Options { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public string ConfigPath { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool DraggableToolbar
        {
            set { this._inputTextBox.DraggableToolbar = value; }
        }

        #region backward compatibility

        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme
        {
            get
            {
                return _inputTextBox.Theme;
            } 
            set
            {
                _inputTextBox.Theme = value;    
            }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public int MaxLength
        {
            get { return _maxLength; }
            set { _maxLength = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Plugins
        {
            set { this._inputTextBox.Plugins = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme_advanced_buttons1
        {
            set { this._inputTextBox.Theme_advanced_buttons1 = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme_advanced_buttons2
        {
            set { this._inputTextBox.Theme_advanced_buttons2 = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme_advanced_buttons3
        {
            set { this._inputTextBox.Theme_advanced_buttons3 = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme_advanced_buttons4
        {
            set { this._inputTextBox.Theme_advanced_buttons4 = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme_advanced_path_location
        {
            set { this._inputTextBox.Theme_advanced_path_location = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme_advanced_toolbar_align
        {
            set { this._inputTextBox.Theme_advanced_toolbar_align = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool Theme_advanced_resize_horizontal
        {
            set { this._inputTextBox.Theme_advanced_resize_horizontal = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool Theme_advanced_resizing
        {
            set { this._inputTextBox.Theme_advanced_resizing = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme_advanced_toolbar_location
        {
            set { this._inputTextBox.Theme_advanced_toolbar_location = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme_advanced_statusbar_location
        {
            set { this._inputTextBox.Theme_advanced_statusbar_location = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Content_css
        {
            set { this._inputTextBox.Content_css = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool Auto_resize
        {
            set { this._inputTextBox.Auto_resize = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Table_styles
        {
            set { this._inputTextBox.Table_styles = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Table_cell_styles
        {
            set { this._inputTextBox.Table_cell_styles = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Table_row_styles
        {
            set { this._inputTextBox.Table_row_styles = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Theme_advanced_styles
        {
            set { this._inputTextBox.Theme_advanced_styles = value; }
        }
        [Obsolete("Use Options property instead.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Font_size_style_values
        {
            set { _inputTextBox.Font_size_style_values = value; }
        }

        #endregion

        public override OutputMethod DefaultOutputMethod { get { return ContentRepository.Schema.OutputMethod.Html; } }

        /* ============================================================================= Constructor */
        /// <summary>
        /// RichText constuctor.
        /// </summary>
        public RichText()
		{
            _inputTextBox = new RichTextEditor();
            InnerControlID = "InnerTextBox";
            PlaceHolderID = "InnerPlaceHolder";
        }


        /* ============================================================================= Methods */
        public override void SetData(object data)
		{
            var text = Convert.ToString(data);
            _inputTextBox.Text = text;

            if (!IsTemplated)
                return;

            // synchronize data with controls given in the template
            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var innerControl = GetInnerControl() as RichTextEditor;
            if (title != null)
                title.Text = this.Field.DisplayName;
            if (desc != null)
                desc.Text = this.Field.Description;
            if (innerControl != null)
                innerControl.Text = Convert.ToString(text);
		}
		public override object GetData()
		{
            if (!IsTemplated)
                return _inputTextBox.Text;

            var innerControl = GetInnerControl() as RichTextEditor;
            return innerControl != null ? innerControl.Text : string.Empty;
        }

        /// <summary>
        /// Creates dynamic inner control(s) and add it or them to Control collection.
        /// </summary>
        /// <param name="e">default</param>
        protected override void OnInit(EventArgs e)
        {           
            base.OnInit(e);
            SetPropertiesOrDefault();
            _inputTextBox.ID = String.Concat(this.ID, "_", this.ContentHandler.Id.ToString());

            InitImagePickerParams();
        }
        protected virtual void InitImagePickerParams()
        {
            var script = string.Format("SN.tinymceimagepickerparams = {{ DefaultPath:'{0}' }};", this.ContentHandler.ParentPath);
            UITools.RegisterStartupScript("tinymceimagepickerparams", script, this.Page);
        }
        protected override void CreateChildControls()
        {
            if (IsTemplated)
                return;

            if (this.ControlMode != FieldControlControlMode.Browse)
                this.Controls.Add(_inputTextBox);

            base.CreateChildControls();
        }
        private void SetPropertiesOrDefault()
        {
            var textbox = _inputTextBox;
            if (UseEditTemplate)
            {
                textbox = GetInnerControl() as RichTextEditor;
                if (textbox == null)
                    return;
            }



            textbox.Width = this.Width.IsEmpty ? 500 : this.Width;
            textbox.Height = this.Height.IsEmpty ? 190 : this.Height;
            textbox.MaxLength = this.MaxLength;

            if (!string.IsNullOrEmpty(Options))
                textbox.Options = Options;
            else
            {
                var isInAsyncPostback = false;
                if (Page != null)
                    isInAsyncPostback = ScriptManager.GetCurrent(Page).IsInAsyncPostBack;
                var optionPath = (isInAsyncPostback) ? GlobalAjaxOptionsPath : GlobalOptionsPath;
                textbox.Options = String.IsNullOrEmpty(this.ConfigPath) ? GetOptions(optionPath) : GetOptions(ConfigPath);
                if (string.IsNullOrEmpty(textbox.Options))
                    textbox.Options = SimpleOptions.ToString();
                Options = textbox.Options;                    
            }

            #region backward compatibility

            if (String.IsNullOrEmpty(textbox.Plugins))
                textbox.Plugins = "sncontentpicker, safari,pagebreak,style,layer,table,advlink,emotions,inlinepopups,insertdatetime,preview,media,searchreplace,contextmenu,paste,noneditable,visualchars,nonbreaking,xhtmlxtras,template,fullscreen";
            if (String.IsNullOrEmpty(textbox.Theme_advanced_buttons1))
                textbox.Theme_advanced_buttons1 = "fullscreen,|,newdocument, snimage,|,bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,|,styleselect,formatselect,fontselect,fontsizeselect";
            if (String.IsNullOrEmpty(textbox.Theme_advanced_buttons2))
                textbox.Theme_advanced_buttons2 = "cut,copy,paste,pastetext,pasteword,|,search,replace,|,bullist,numlist,|,outdent,indent,blockquote,|,undo,redo,|,link,unlink,anchor,cleanup,help,code,|,insertdate,inserttime,preview,|,forecolor,backcolor";
            if (String.IsNullOrEmpty(textbox.Theme_advanced_buttons3))
                textbox.Theme_advanced_buttons3 = "tablecontrols,|,hr,removeformat,visualaid,|,sub,sup,|,charmap,emotions,iespell,media,advhr,|,print,|,ltr,rtl";
            if (String.IsNullOrEmpty(textbox.Theme_advanced_buttons4))
                textbox.Theme_advanced_buttons4 = "insertlayer,moveforward,movebackward,absolute,|,styleprops,|,cite,abbr,acronym,del,ins,attribs,|,visualchars,nonbreaking,template,pagebreak";
            if (String.IsNullOrEmpty(textbox.Theme_advanced_toolbar_location))
                textbox.Theme_advanced_toolbar_location = "external";
            if (String.IsNullOrEmpty(textbox.Theme_advanced_toolbar_align))
                textbox.Theme_advanced_toolbar_align = "left";
            if (String.IsNullOrEmpty(textbox.Theme_advanced_statusbar_location))
                textbox.Theme_advanced_statusbar_location = "bottom";
            if (string.IsNullOrEmpty(textbox.Theme_advanced_path_location))
                textbox.Theme_advanced_path_location = "bottom";

            #endregion

            if (!string.IsNullOrEmpty(this.CssClass))
                textbox.CssClass = this.CssClass;
        }
        protected override void RenderContents(HtmlTextWriter writer)
        {
            // templates
            if (UseBrowseTemplate)
            {
                // get placeholder and render contents into it
                var container = this.GetInnerPlaceHolder();
                if (container == null)
                {
                    base.RenderContents(writer);
                    return;
                }
                if (_inputTextBox.Text.Contains(markupString))
                {
                    RenderPortletPreview(writer, container);
                }
                else
                    RenderSimpleInPlaceHolder(writer, container);

                return;
            }
            if ((UseEditTemplate) || (UseInlineEditTemplate))
            {
                // if the field or fieldcontrol is readonly the textbox should not be rendered, but browse mode controls should be
                if (this.ReadOnly || this.Field.ReadOnly)
                {
                    var innerControl = this.GetInnerControl() as TextBox;
                    if (innerControl != null)
                    {
                        var parentControl = innerControl.Parent;
                        parentControl.Controls.Remove(innerControl);
                        var container = new PlaceHolder();
                        parentControl.Controls.Add(container);
                        if (_inputTextBox.Text.Contains(markupString))
                        {
                            RenderPortletPreview(writer, container);
                        }
                        else
                            RenderSimpleInPlaceHolder(writer, container);

                        return;
                    }
                }
                base.RenderContents(writer);
                return;
            }

            if (this.ControlMode == FieldControlControlMode.Browse || this.Field.ReadOnly || this.ReadOnly)
            {
                if (_inputTextBox.Text.Contains(markupString))
                    RenderPortletPreview(writer, null);
                else
                    RenderSimple(writer);
            }
            else
            {
                RenderEditor(writer);
            }
		}
        private void RenderPortletPreview(HtmlTextWriter writer, Control container)
        {
            var controlCollection = container == null ? this.Controls : container.Controls;

            // custom view logic
            var text = _inputTextBox.Text;

            string currentText = text;
            var markup = PortletMarkup.GetFirstMarkup(currentText);
            while (markup != null)
            {
                var textBefore = currentText.Substring(0, markup.StartIndex);

                // render contents
                controlCollection.Add(new Literal { Text = textBefore });
                markup.AddToControls(controlCollection);

                currentText = currentText.Substring(markup.EndIndex, currentText.Length - markup.EndIndex);

                markup = PortletMarkup.GetFirstMarkup(currentText);
            }
            controlCollection.Add(new Literal { Text = currentText });

            base.RenderContents(writer);
        }
		private void RenderSimple(TextWriter writer)
		{
			writer.Write(_inputTextBox.Text);
		}
        private void RenderSimpleInPlaceHolder(HtmlTextWriter writer, Control container)
        {
            var literalControl = new LiteralControl(_inputTextBox.Text);
            container.Controls.Add(literalControl);
            base.RenderContents(writer);
        }
		private void RenderEditor(HtmlTextWriter writer)
		{
			if (this.RenderMode == FieldControlRenderMode.InlineEdit)
            {
                var titleText = String.Concat(this.Field.DisplayName, " ", this.Field.Description);
                _inputTextBox.Attributes.Add("Title", titleText);
            }

            _inputTextBox.RenderControl(writer);// render read/write control
        }
        public override void DoAutoConfigure(FieldSetting setting)
        {
            var longTextSetting = setting as LongTextFieldSetting;
            if (longTextSetting == null)
                throw new ApplicationException("A RichText field control can only be used in conjunction with a LongText field.");

            switch (longTextSetting.TextType)
            {
                case TextType.AdvancedRichText:
                    Options = RichText.AdvancedOptions.ToString();
                    break;
                case TextType.RichText:
                default:
                    Options = RichText.SimpleOptions.ToString();
                    break;
            }

            base.DoAutoConfigure(setting);
        }


        /* ============================================================================= Static methods */
        /// <summary>
        /// Gets the option text of the RichTextEditor for the RichText fieldcontrol.
        /// </summary>
        /// <returns>Returns an empty string if config file does not exists.</returns>
        internal static string GetOptions(string optionPath)
        {
            var configHead = NodeHead.Get(optionPath);
            if (configHead == null)
                return string.Empty;
            var content = ContentRepository.Content.Load(configHead.Id);
            
            Debug.Assert(content != null,"Hey, it seems nodehead loading is ok, but the Content.Load couldn't find the node associated with the nodehead instance!");

            var binaryField = content.Fields["Binary"];
            var binaryData = binaryField.GetData() as BinaryData;
            if (binaryData == null)
                return string.Empty;
                
            var result = ContentRepository.Tools.GetStreamString(binaryData.GetStream());
            return result;
        }
        private static StringBuilder InitializeSimpleOptions()
        {
            var result = new StringBuilder(41);
            result.AppendFormat(@"{{{0}", Environment.NewLine);
            result.AppendFormat(@"{0}mode : ""exact"",{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"{0}theme : ""simple""{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"}}");
            return result;
        }
        private static StringBuilder InitalizeAdvancedOptions()
        {
            StringBuilder result = new StringBuilder(1258);
            result.AppendFormat(@"{{{0}", Environment.NewLine);
            result.AppendFormat(@"{0}mode : 'textareas',{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"{0}theme : 'advanced',{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"{0}plugins : ", Environment.NewLine);
            result.AppendFormat(@"'safari,pagebreak,style,layer,table,advlink,emotions,inlinepopups,insertdatetime,prev");
            result.AppendFormat(@"iew,media,searchreplace,contextmenu,paste,noneditable,visualchars,nonbreaking,xhtmlxtras,template,ful");
            result.AppendFormat(@"lscreen',{0}", Environment.NewLine);
            result.AppendFormat(@"{0}// Theme options{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"{0}theme_advanced_buttons1 : 'fullscreen,|,newdocument, ", Environment.NewLine);
            result.AppendFormat(@"snimage,|,bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,|,");
            result.AppendFormat(@"styleselect,formatselect,fontselect,fontsizeselect',{0}", Environment.NewLine);
            result.AppendFormat(@"{0}theme_advanced_buttons2 : ", Environment.NewLine);
            result.AppendFormat(@"'cut,copy,paste,pastetext,pasteword,|,search,replace,|,bullist,numlist,|,outdent,indent,blockquote,|,");
            result.AppendFormat(@"undo,redo,|,link,unlink,anchor,cleanup,help,code,|,insertdate,inserttime,preview,|,forecolor,backcolo");
            result.AppendFormat(@"r',{0}", Environment.NewLine);
            result.AppendFormat(@"{0}theme_advanced_buttons3 : ", Environment.NewLine);
            result.AppendFormat(@"'tablecontrols,|,hr,removeformat,visualaid,|,sub,sup,|,charmap,emotions,iespell,media,advhr,|,print,|");
            result.AppendFormat(@",ltr,rtl',{0}", Environment.NewLine);
            result.AppendFormat(@"{0}theme_advanced_buttons4 : ", Environment.NewLine);
            result.AppendFormat(@"'insertlayer,moveforward,movebackward,absolute,|,styleprops,|,cite,abbr,acronym,del,ins,attribs,|,vis");
            result.AppendFormat(@"ualchars,nonbreaking,template,pagebreak',{0}", Environment.NewLine);
            result.AppendFormat(@"{0}theme_advanced_toolbar_location : 'external',{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"{0}theme_advanced_toolbar_align : 'left',{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"{0}theme_advanced_statusbar_location : 'bottom',{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"{0}theme_advanced_resizing : true,{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"{0}theme_advanced_path_location : 'bottom'{1}", UITools.ControlChars.Tab, Environment.NewLine);
            result.AppendFormat(@"}}");
            return result;
        }


        /* ============================================================================= ITemplateFieldControl */
        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID);
        }
        public PlaceHolder GetInnerPlaceHolder()
        {
            return this.FindControlRecursive(PlaceHolderID) as PlaceHolder;
        }
        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID);
        }
        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID);
        }
    }
}