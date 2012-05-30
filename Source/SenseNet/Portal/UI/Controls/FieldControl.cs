using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.Diagnostics;
using ConfigurationException=SenseNet.ContentRepository.Storage.Data.ConfigurationException;
using Content = SenseNet.ContentRepository.Content;
using SenseNet.ContentRepository.Schema;
using System.Web.UI.HtmlControls;
using System.Web;
using System.Linq;
using System.Text.RegularExpressions;

namespace SenseNet.Portal.UI.Controls
{    
    /// <summary>
    /// <c>FieldControl</c> class is responsible for visulaizing an underlying <see cref="SenseNet.ContentRepository.Field">Field</see> in various <see cref="SenseNet.Portal.UI.Controls.FieldControlRenderMode">FieldControlRenderMode</see>s.
    /// </summary>
    /// <remarks>
    /// It is responsible for retreiving <see cref="SenseNet.ContentRepository.Field">Field</see> data, writing this data back and rendering data set based on the rendering mode given.
    /// 
    /// It is neither responsible for data validation (this is done at <see cref="SenseNet.ContentRepository.Field">Field</see> level) nor for error handling (this is done at <see cref="SenseNet.Portal.UI.ContentView">ContentView</see> level).
    /// However, error visualization is its responsibity.
    /// 
    /// FieldControls are typically created when displaying data in more than one <see cref="SenseNet.Portal.UI.Controls.FieldControlRenderMode">FieldControlRenderMode</see>s. 
    /// If a Field is to be viewed only in Browse mode, this should be donw with public methods and properties provided by <see cref="SenseNet.Portal.UI.ContentView">ContentView</see>.
    /// </remarks>
    [ParseChildren(true)]
    public abstract class FieldControl : ViewControlBase, INamingContainer, IFieldControl 
    {
        private const string FrameTemplateName = "FrameTemplate.ascx";
        private const string FrameControlPlaceHolderName = "ControlPlaceHolder";

        protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Div; } }
        protected string InnerControlID = "InnerControl";
        protected string TitleControlID = "LabelForTitle";
        protected string DescriptionControlID = "LabelForDesc";
        protected string RequiredControlID = "ControlForRequired";
        protected string InputUnitPanelID = "InputUnitPanel";
        
        
        // General Fields ////////////////////////////////////////////////////////////////////
        protected Control _container;
        private string _title;
        private string _description;
        /// <summary>
        /// Name of the field that the FieldControl maps to
        /// </summary>
        protected string _fieldName;
        /// <summary>
        /// The <see cref="SenseNet.ContentRepository.Field">Field</see> wrapped by the FieldControl
        /// </summary>
        protected Field _field;
        private Content _content;
        private FieldControlRenderMode _renderMode = FieldControlRenderMode.Default;
        // Editor Fields /////////////////////////////////////////////////////////////////////
        private bool _readOnly;
        private bool _inline;
        private string _inputUnitCssClass;
        private string _errorMessage;

        // General Properties/////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets or sets the title of the FieldControl. When set it overrides the default value which is the Title of the Field property, set in the CTD
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        /// <summary>
        /// Gets or sets the description of the FieldControl. When set it overrides the default value which is the Description of the Field property, set in the CTD
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        /// <summary>
        /// Gets or sets the fieldname of the FieldControl. This is the name which is mapped against the CTD (and RepositoryProperties are resolved)
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }
        /// <summary>
        /// Gets or sets the render mode which determines output generated when rendering.
        /// </summary>
        [Obsolete("Use ControlMode and FrameMode attributes instead")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public FieldControlRenderMode RenderMode
        {
            get
            {
                if (_renderMode != FieldControlRenderMode.Default)
                    return _renderMode;
                if (this.ContentView == null)
                    return FieldControlRenderMode.Edit;
                return this.ContentView.DefaultControlRenderMode;
            }
            set
            {
                _renderMode = value;
            }
        }
        /// <summary>
        /// Gets or sets the control mode: Browse or Edit
        /// </summary>
        private FieldControlControlMode _controlMode = FieldControlControlMode.None;
        [PersistenceMode(PersistenceMode.Attribute)]
        public FieldControlControlMode ControlMode
        {
            get
            {
                // backward compatibility (controlmode not, but rendermode has been set)
                if (_controlMode == FieldControlControlMode.None && _renderMode != FieldControlRenderMode.Default)
                {
                    switch (this.RenderMode)
                    {
                        case FieldControlRenderMode.Browse:
                            return FieldControlControlMode.Browse;
                        case FieldControlRenderMode.Default:
                        case FieldControlRenderMode.Edit:
                        case FieldControlRenderMode.InlineEdit:
                            return FieldControlControlMode.Edit;
                    }
                }

                // controlmode has been set externally
                if (_controlMode != FieldControlControlMode.None)
                    return _controlMode;

                // fallback to contentview
                if (this.ContentView != null)
                    return this.ContentView.ViewControlMode;

                return FieldControlControlMode.Edit;
            }
            set
            {
                _controlMode = value;
            }
        }
        /// <summary>
        /// Gets or sets the frame mode: NoFrame or ShowFrame
        /// </summary>
        private FieldControlFrameMode _frameMode = FieldControlFrameMode.None;
        [PersistenceMode(PersistenceMode.Attribute)]
        public FieldControlFrameMode FrameMode
        {
            get
            {
                // backward compatibility (framemode not, but rendermode has been set)
                if (_frameMode == FieldControlFrameMode.None && _renderMode != FieldControlRenderMode.Default)
                {
                    switch (this.RenderMode)
                    {
                        case FieldControlRenderMode.Browse:
                        case FieldControlRenderMode.InlineEdit:
                            return FieldControlFrameMode.NoFrame;
                        case FieldControlRenderMode.Default:
                        case FieldControlRenderMode.Edit:
                            return FieldControlFrameMode.ShowFrame;
                    }
                }

                // framemode has been set externally
                if (_frameMode != FieldControlFrameMode.None)
                    return _frameMode;

                // fallback to contentview
                if (this.ContentView != null)
                    return this.ContentView.ViewControlFrameMode;

                return FieldControlFrameMode.ShowFrame;
            }
            set
            {
                _frameMode = value;
            }
        }
        /// <summary>
        /// Gets or sets the <see cref="SenseNet.ContentRepository.Field">Field</see> property
        /// </summary>
        public Field Field
        {
            get { return _field; }
            internal set
            {
                _field = value;
                _content = _field.Content;
            }
        }
        /// <summary>
        /// Gets the <see cref="SenseNet.ContentRepository.Content">Content</see> belonging to the <see cref="SenseNet.ContentRepository.Field">Field</see> property
        /// </summary>
        public Content Content
        {
            get { return _content; }
        }
        /// <summary>
        /// Gets the <see cref="SenseNet.ContentRepository.Storage.Node">Node</see> belonging to the <see cref="SenseNet.ContentRepository.Content">Content</see> owned by the <see cref="SenseNet.ContentRepository.Field">Field</see> property.
        /// </summary>
        public Node ContentHandler
        {
            get { return _content.ContentHandler; }
        }

        // Editor Properties /////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Gets or sets wether the control is readonly. This property determines how the control is rendered.
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool ReadOnly
        {
            get { return _readOnly || (this.Field == null ? false : this.Field.ReadOnly) || this.RenderMode == FieldControlRenderMode.Browse; }
            set { _readOnly = value; }
        }
        
        /// <summary>
        /// Gets or sets wether the control is inline. This property determines how the control is rendered.
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool Inline
        {
            get { return _inline; }
            set { _inline = value; }
        }
        
        /// <summary>
        /// Gets or sets the class of the wrapper tag of the control.
        /// </summary>
        [PersistenceMode(PersistenceMode.Attribute)]
        public string InputUnitCssClass
        {
            get
            {
                var requiredClass = (this.FieldIsCompulsory ? " sn-required" : string.Empty);

                return (String.IsNullOrEmpty(_inputUnitCssClass) ? "sn-inputunit ui-helper-clearfix" + requiredClass : _inputUnitCssClass + requiredClass);
            }
            set { _inputUnitCssClass = value; }
        }
        
        /// <summary>
        /// Gets error messages related to the FieldControl.
        /// </summary>
        protected string ErrorMessage
        {
            get { return _errorMessage; }
        }
        
        /// <summary>
        /// Gets wether the FieldControl is invalid (has the ErrorMessage set)
        /// </summary>
        protected bool HasError
        {
            get { return !String.IsNullOrEmpty(_errorMessage); }
        }

        protected bool FieldIsCompulsory
        {
            get
            {
                return this.Field != null && this.Field.FieldSetting != null &&
                       this.Field.FieldSetting.Compulsory.HasValue && this.Field.FieldSetting.Compulsory.Value;
            }
        }

        #region template

        protected bool UseBrowseTemplate { get { return ControlMode == FieldControlControlMode.Browse && BrowseTemplate != null; } }
        protected bool UseEditTemplate { get { return ControlMode == FieldControlControlMode.Edit && EditTemplate != null; } }
        [Obsolete("InlineEdit template is no more used. Use UseEditTemplate.")]
        protected bool UseInlineEditTemplate { get { return RenderMode == FieldControlRenderMode.InlineEdit && InlineEditTemplate != null; } }
        protected bool UseValidationErrorTemplate { get { return ValidationErrorTemplate != null; } }
        
        protected FieldControl()
        {
            _container = new Control();
        }

        [TemplateContainer(typeof(FieldControl))]
        public ITemplate FrameTemplate { get; set; }

        [TemplateContainer(typeof(FieldControl))]
        public ITemplate BrowseTemplate { get; set; }

        private ITemplate _editTemplate;
        [TemplateContainer(typeof(FieldControl))]
        public ITemplate EditTemplate
        {
            get { return _editTemplate; }
            set { _editTemplate = value; }
        }

        [Obsolete("Use the EditTemplate property")]
        [TemplateContainer(typeof(FieldControl))]
        public ITemplate InlineEditTemplate
        {
            get { return _editTemplate; }
            set { _editTemplate = value; }
        }

        [TemplateContainer(typeof(FieldControl))]
        public ITemplate ValidationErrorTemplate { get; set; }

        public bool IsTemplated
        {
            get
            {
                return (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate);
            }
        }

        #endregion

        // General methods ///////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Gets object data.
        /// </summary>
        /// <remarks>
        /// Exception handling and displayed is done at ContentView level; FormatExceptions and Exceptions are handled and displayed at this level.
        /// Should you need custom or localized error messages, throw a FieldControlDataException with your own error message.
        /// </remarks>
        /// <returns>Object representing data of the wrapped Field</returns>
        public abstract object GetData();

        public string GetOutputData(OutputMethod method)
        {
            var data = GetData();
            if (data == null)
                return null;
            switch (method)
            {
                case OutputMethod.Raw:
                    return data.ToString();
                case OutputMethod.Text:
                    return HttpUtility.HtmlEncode(data);
                case OutputMethod.Html:
                    return SenseNet.Portal.Security.Sanitize(data.ToString());
                case OutputMethod.Default:
                    return GetOutputData(this.OutputMethod);
                default:
                    throw new NotImplementedException("Unknown OutputMethod: " + this.Field.FieldSetting.OutputMethod);
            }
            
        }
        
        /// <summary>
        /// Sets data within the FieldControl
        /// </summary>
        /// <param name="data">Data of the <see cref="SenseNet.ContentRepository.Field">Field</see> wrapped</param>
        public abstract void SetData(object data);

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InitTemplates();

            // title and description
            var title = this.FindControlRecursive(TitleControlID) as Label;
            var desc = this.FindControlRecursive(DescriptionControlID) as Label;
            if (title != null) title.Text = Field.DisplayName;
            if (desc != null) desc.Text = Field.Description;

            if (_field == null) //TODO:NullField
                return;
            SetDataInternal();
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DataBind();
        }
		
        protected void SetDataInternal()
		{
            if (_field.Content.IsNew)
            {
                var d = _field.FieldSetting.EvaluateDefaultValue();
                if (d != null)
                    _field.Parse(d);
            }
            var data = _field.GetData();
            this.SetData(data);
        }

        // Editor methods ///////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Clears the error message.
        /// </summary>
        public void ClearError()
        {
            _errorMessage = null;
        }
        
        /// <summary>
        /// Sets the error message
        /// </summary>
        /// <param name="message">Error message to be set</param>
        public void SetErrorMessage(string message)
        {
            _errorMessage = message;
        }

        // Rendering /////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            #region template

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                if (HasError)
                    ShowErrorMessage();

                if (this.FieldIsCompulsory)
                {
                    var rc = GetRequiredControl();
                    if (rc != null)
                        rc.Visible = true;

                    var iu = GetInputUnitPanel();
                    if (iu != null)
                        iu.Attributes["class"] += " sn-required";
                }

                RenderContents(writer);
                return;
            }

            #endregion

            #region backward compatibility

            if (RenderMode != FieldControlRenderMode.Browse && RenderMode != FieldControlRenderMode.InlineEdit)
                this.RenderBeginTag(writer);
            this.RenderContents(writer);
            if (this.HasError)
                RenderErrorMessage(writer);
            if (RenderMode != FieldControlRenderMode.Browse && RenderMode != FieldControlRenderMode.InlineEdit)
                this.RenderEndTag(writer);

            #endregion
        }



        /// <summary>
        /// Renders the beginning of the control
        /// </summary>
        /// <remarks>
        /// The opening tag of the wrapping container, Title and Description are rendered but not the control itself.
        /// </remarks>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

            #region template

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                base.RenderBeginTag(writer);
                return;
            }

            #endregion

            #region backward compatibility

            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), this.InputUnitCssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), "sn-iu-label");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            RenderFieldTitle(writer);
            writer.WriteBreak();
            RenderFieldDescription(writer);

            writer.RenderEndTag(); // sn-iu-label end

            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), "sn-iu-control");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            #endregion
        }

        /// <summary>
        /// Renders the end part of the control
        /// </summary>
        /// <remarks>
        /// Tags left open in RenderBeginTag are closed.
        /// </remarks>
        public override void RenderEndTag(HtmlTextWriter writer)
        {

            #region template

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                base.RenderEndTag(writer);
                return;
            }

            #endregion

            #region backward compatibility

            writer.RenderEndTag(); // sn-iu-control end
            writer.RenderEndTag(); // sn-inputunit end

            #endregion
        }

        protected Control GetRequiredControl()
        {
            return this.FindControlRecursive(RequiredControlID);
        }

        protected HtmlGenericControl GetInputUnitPanel()
        {
            return this.FindControlRecursive(InputUnitPanelID) as HtmlGenericControl;
        }

        #region backward compatibility

        /// <summary>
        /// Renders the error message
        /// </summary>
        protected virtual void RenderErrorMessage(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-iu-error");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(_errorMessage);
            writer.RenderEndTag();
        }

        /// <summary>
        /// Renders the description of the FieldControl.
        /// </summary>
        public virtual void RenderFieldDescription(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), "sn-iu-desc");
            writer.AddAttribute(HtmlTextWriterAttribute.For.ToString(), String.Concat("editor_", this.ClientID));
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            if (_description == null)
                writer.Write(this.Field.Description);
            else
                writer.Write(_description);
            writer.RenderEndTag();
        }

        /// <summary>
        /// Renders the title of the FieldControl.
        /// </summary>
        public virtual void RenderFieldTitle(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), "sn-iu-title");
            writer.AddAttribute(HtmlTextWriterAttribute.For.ToString(), String.Concat("editor_", this.ClientID));
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            var title = String.Concat(_title ?? this.Field.DisplayName, Field.FieldSetting.Compulsory == true ? " *" : "");
            writer.Write(title);
            writer.RenderEndTag();
        }

        #endregion

        // Internals /////////////////////////////////////////////////////////////////////////

        protected virtual void InitTemplates()
        {
            var fieldControlName = this.GetType().Name;

            if (UseBrowseTemplate)
                AddLocalTemplate(_container, Controls, BrowseTemplate, false);
            else if (ControlMode == FieldControlControlMode.Browse && Repository.IsGlobalTemplateEnabled)
                AddGlobalTemplate(fieldControlName);

            if (UseEditTemplate)
                AddLocalTemplate(_container, Controls, EditTemplate, true);
            else if (ControlMode == FieldControlControlMode.Edit && Repository.IsGlobalTemplateEnabled)
                AddGlobalTemplate(fieldControlName);
        }

        /// <summary>
        /// Instantiates the error template.
        /// </summary>
        private void InstantiateErrorTemplate()
        {
            if (!UseValidationErrorTemplate)
            {

                return;
            }
                
            var c = this.FindControlRecursive("ErrorPlaceHolder") as PlaceHolder;
            if (c == null)
                return;
            ValidationErrorTemplate.InstantiateIn(c);
            c.Visible = false;
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        protected virtual void ShowErrorMessage()
        {
            var c = this.FindControlRecursive("ErrorPlaceHolder") as PlaceHolder;
            if (c == null) 
                return;
            c.Visible = true;
            var errorMessageControl = this.FindControlRecursive("ErrorLabel") as Label;
            if (errorMessageControl != null)
                errorMessageControl.Text = _errorMessage;
        }

        private void AddGlobalTemplate(string fieldControlName)
        {
            var controlModeName = Enum.GetName(typeof(FieldControlControlMode), this.ControlMode);

            var templateFileName = String.Concat(controlModeName, "Template.ascx");
            var templateFolderPath = RepositoryPath.Combine(Repository.FieldControlTemplatesPath, fieldControlName);
            var templateFilePath = SkinManager.Resolve(RepositoryPath.Combine(templateFolderPath, templateFileName));

            var templateFileHead = NodeHead.Get(templateFilePath);
            if (templateFileHead == null) 
                return;

            var page = this.Page ?? System.Web.HttpContext.Current.Handler as System.Web.UI.Page;
            System.Diagnostics.Debug.Assert(page != null, "Page property is null. Perhaps, you work with templated controls.");


            // render control with frame
            if (this.FrameMode == FieldControlFrameMode.ShowFrame)
            {
                var frameTemplateFilePath = SkinManager.Resolve(RepositoryPath.Combine(Repository.FieldControlTemplatesPath, FrameTemplateName));
                var frameTemplateFileHead = NodeHead.Get(frameTemplateFilePath);
                if (frameTemplateFileHead == null)
                    return;

                FrameTemplate = page.LoadTemplate(frameTemplateFilePath);
                AddTemplateTo(_container, Controls, FrameTemplate, true);

                var innerContainer = this.FindControlRecursive(FrameControlPlaceHolderName);

                PlaceHolder tempPlaceholder = new PlaceHolder();

                switch (this.ControlMode)
                {
                    case FieldControlControlMode.Browse:
                        BrowseTemplate = page.LoadTemplate(templateFilePath);
                        AddTemplateTo(tempPlaceholder, innerContainer.Controls, BrowseTemplate, true);
                        break;
                    case FieldControlControlMode.Edit:
                        EditTemplate = page.LoadTemplate(templateFilePath);
                        AddTemplateTo(tempPlaceholder, innerContainer.Controls, EditTemplate, true);
                        break;
                }

                return;
            }

            // render control without frame
            switch (this.ControlMode)
            {
                case FieldControlControlMode.Browse:
                    BrowseTemplate = page.LoadTemplate(templateFilePath);
                    AddTemplateTo(_container, Controls, BrowseTemplate, true);
                    break;
                case FieldControlControlMode.Edit:
                    EditTemplate = page.LoadTemplate(templateFilePath);
                    AddTemplateTo(_container, Controls, EditTemplate, true);
                    break;
            }            
        }

        private void AddLocalTemplate(Control target, ControlCollection owner, ITemplate source, bool addErrorTemplate)
        {
            // render control with frame
            if (this.FrameMode == FieldControlFrameMode.ShowFrame)
            {
                var page = this.Page ?? System.Web.HttpContext.Current.Handler as System.Web.UI.Page;
                System.Diagnostics.Debug.Assert(page != null, "Page property is null. Perhaps, you work with templated controls.");

                var frameTemplateFilePath = SkinManager.Resolve(RepositoryPath.Combine(Repository.FieldControlTemplatesPath, FrameTemplateName));
                var frameTemplateFileHead = NodeHead.Get(frameTemplateFilePath);
                if (frameTemplateFileHead == null)
                    return;

                FrameTemplate = page.LoadTemplate(frameTemplateFilePath);
                AddTemplateTo(_container, Controls, FrameTemplate, true);

                var innerContainer = this.FindControlRecursive(FrameControlPlaceHolderName);

                PlaceHolder tempPlaceholder = new PlaceHolder();

                AddTemplateTo(tempPlaceholder, innerContainer.Controls, source, true);

                return;
            }

            // render control without frame
            AddTemplateTo(target, owner, source, addErrorTemplate);
        }

        private void AddTemplateTo(Control target, ControlCollection owner, ITemplate source, bool addErrorTemplate)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (source == null)
                throw new ArgumentNullException("source");

            source.InstantiateIn(target);
            owner.Add(target);
            if (addErrorTemplate)
                InstantiateErrorTemplate();
        }

        public OutputMethod OutputMethod
        {
            get
            {
                var method = this.Field.FieldSetting.OutputMethod;
                if (method != ContentRepository.Schema.OutputMethod.Default)
                    return method;
                method = this.DefaultOutputMethod;
                if (method == ContentRepository.Schema.OutputMethod.Default)
                    throw new NotImplementedException(String.Format("Invalid OutputMethod. It cannot be Default. Control: {0} : {1}", this.FieldName, this.GetType().FullName));
                return method;
            }
        }
        public virtual OutputMethod DefaultOutputMethod
        {
            get { return ContentRepository.Schema.OutputMethod.Text; }
        }

        public virtual object Data
        {
            get { return GetOutputData(this.Field.FieldSetting.OutputMethod); }
        }
        public object RawData
        {
            get { return GetOutputData(OutputMethod.Raw); }
        }
        public object TextData
        {
            get { return GetOutputData(OutputMethod.Text); }
        }
        public object HtmlData
        {
            get { return GetOutputData(OutputMethod.Html); }
        }

        public virtual void DoAutoConfigure(FieldSetting setting)
        {
            //nop
        }
    }


}
