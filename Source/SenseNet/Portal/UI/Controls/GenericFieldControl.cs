using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using Content = SenseNet.ContentRepository.Content;
using System.Web;
using System.Text;
using System.Configuration;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:GenericFieldControl runat=server />")]
    public class GenericFieldControl : ViewControlBase //WebControl
    {
        //=============================================================================== Configuration
        private static TagPrefixCollection _tagPrefixInfos = null;
        public static TagPrefixCollection TagPrefixInfos
        {
            get
            {
                if (_tagPrefixInfos == null)
                {
                    var section = ConfigurationManager.GetSection("system.web/pages") as PagesSection;
                    if (section != null)
                        _tagPrefixInfos = section.Controls;

                    if (_tagPrefixInfos == null)
                        _tagPrefixInfos = new TagPrefixCollection();
                }
                return _tagPrefixInfos;
            }
        }


        //===============================================================================

        private static object _lock = new Object();
        private static Dictionary<string, Type> _defaultControlTypes;
        private Panel _advancedPanel;
        private string _excludedFields = string.Empty;
        private string[] _excludedList;
        private string _fieldsOrder = string.Empty;
        private int _id = 1;
        private string _readonlyFields = string.Empty;
        private string[] _readonlyList;

        private static Dictionary<string, Type> ControlTable
        {
            get
            {
                if (_defaultControlTypes == null)
                {
                    lock (_lock)
                    {
                        if (_defaultControlTypes == null)
                        {
                            _defaultControlTypes = BindingFieldsAndControlsByAttribute();
                        }
                    }
                }
                return _defaultControlTypes;
            }
        }

        private Stack<int> history;

        public bool EnablePaging { get; set; }

        public bool ContentListFieldsOnly { get; set; }

        public string FieldsOrder
        {
            get { return _fieldsOrder; }
            set { _fieldsOrder = value; }
        }

        public string ReadonlyFields
        {
            get { return _readonlyFields; }
            set { _readonlyFields = value; }
        }

        public string ExcludedFields
        {
            get { return _excludedFields; }
            set { _excludedFields = value; }
        }

        protected string[] ReadonlyList
        {
            get
            {
                if (_readonlyList == null && !string.IsNullOrEmpty(ReadonlyFields))
                {
                    _readonlyList = ReadonlyFields.Split(new[] { ' ', ';' });
                }

                return _readonlyList;
            }
        }

        protected string[] ExcludedList
        {
            get
            {
                if (_excludedList == null && !string.IsNullOrEmpty(ExcludedFields))
                {
                    _excludedList = ExcludedFields.Split(new[] { ' ', ';' });
                }

                return _excludedList;
            }
        }

        protected Content Content
        {
            get
            {
                //ContextInfo was ment to provide custom content
                //to the generic field control, but ContentView
                //cannot handle fields outside of its boundary - yet

                //if (string.IsNullOrEmpty(ContextInfoID))
                //{
                var cv = this.Parent as ContentView;
                if (cv != null)
                    return cv.Content;
                //}
                //else
                //{
                //    var contextInfo = UITools.FindContextInfo(this, ContextInfoID);
                //    if (contextInfo != null)
                //        return Content.Load(contextInfo.Path);
                //}

                return null;
            }
        }

        protected Panel AdvancedPanel
        {
            get
            {
                if (_advancedPanel == null)
                {
                    _advancedPanel = new Panel { CssClass = "sn-advancedfields" };
                    _advancedPanel.Style.Add("display", "none");
                }

                return _advancedPanel;
            }
        }

        public Wizard Wizard
        {
            get
            {
                return _wizard;
            }
        }

        private static Dictionary<string, Type> BindingFieldsAndControlsByAttribute()
        {
            Dictionary<string, Type> controlTypes = GetControlTypes();

            var defaultControlTypes = new Dictionary<string, Type>();
            foreach (string shortName in FieldManager.FieldShortNamesFullNames.Keys)
            {
                string nameHint = FieldManager.DefaultFieldControlTypeNames[FieldManager.FieldShortNamesFullNames[shortName]];
                Type controlType = null;
                if (!controlTypes.TryGetValue(nameHint, out controlType))
                    controlType = typeof(ShortText);
                defaultControlTypes.Add(shortName, controlType);
            }
            return defaultControlTypes;
        }

        private static Dictionary<string, Type> GetControlTypes()
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();
            foreach (var type in TypeHandler.GetTypesByBaseType(typeof(FieldControl)))
                types.Add(type.FullName, type);
            return types;
        }

        public static List<string> GetVisibleFieldNames(Content content, ViewMode viewMode)
        {
            var names = new List<string>();

            if (content == null)
                return names;

            var fields = content.Fields;
            var fieldNames = SortFieldNames(content);

            foreach (var fieldName in fieldNames)
            {
                var field = fields[fieldName];
                var visible = true;

                switch (viewMode)
                {
                    case ViewMode.Browse:
                        visible = field.FieldSetting.VisibleBrowse != FieldVisibility.Hide;
                        break;
                    case ViewMode.Edit:
                    case ViewMode.InlineEdit:
                        visible = field.FieldSetting.VisibleEdit != FieldVisibility.Hide;
                        break;
                    case ViewMode.New:
                    case ViewMode.InlineNew:
                        visible = field.FieldSetting.VisibleNew != FieldVisibility.Hide;
                        break;
                }

                if (!visible)
                    continue;

                names.Add(fieldName);
            }

            return names;
        }

        private static IEnumerable<string> SortFieldNames(Content content)
        {
            var fieldNames = new List<string>();
            var mainPath = content.ContentType.Path;
            var mainFields = new SortedDictionary<string, List<string>>();
            var linkedFields = new SortedDictionary<string, List<string>>();
            var orderedFields = new List<Field>();

            orderedFields.AddRange(from f in content.Fields.Values orderby f.FieldSetting.FieldIndex select f);

            foreach (var field in orderedFields)
            {
                if (field.Name.StartsWith("#"))
                    continue;

                try
                {
                    fieldNames.Add(field.Name);
                }
                catch
                {
                    //unknown field
                }
            }

            //add main fields
            foreach (var fieldList in mainFields.Values)
            {
                fieldNames.AddRange(fieldList);
            }

            //add linked fields
            foreach (var fieldList in linkedFields.Values)
            {
                fieldNames.AddRange(fieldList);
            }

            //list fields
            fieldNames.AddRange((from f in orderedFields where f.Name.StartsWith("#") select f.Name));

            return fieldNames;
        }

        public static FieldControl CreateDefaultFieldControl(Field field)
        {
            FieldControl control = null;

            var choiceSetting = field.FieldSetting as ChoiceFieldSetting;
            var longTextSetting = field.FieldSetting as LongTextFieldSetting;
            var dateTimeFieldSetting = field.FieldSetting as DateTimeFieldSetting;
            string hint = field.FieldSetting.ControlHint;

            if (!string.IsNullOrEmpty(hint))
                control = CreateFieldControlByHint(hint);
            //choice field?
            else if (choiceSetting != null)
                control = CreateDefaultChoiceControl(choiceSetting);
            //longtext field?
            else if (longTextSetting != null)
                control = CreateDefaultLongTextControl(longTextSetting);
            //datetime field?
            else if (dateTimeFieldSetting != null)
                control = CreateDefaultDateTimeControl(dateTimeFieldSetting);

            //generic way, also a fallback logic if we don't have a field control by now
            if (control == null)
            {
                Type controlType = null;

                if (!ControlTable.TryGetValue(field.FieldSetting.ShortName, out controlType))
                    throw new ApplicationException(String.Concat("Cannot resolve the generic field control by '", field.GetType().FullName, "'"));

                control = (FieldControl)Activator.CreateInstance(controlType);
            }

            if (control == null)
                throw new ApplicationException(string.Concat("Failed to instantiate a field control for field ", field.Name));

            control.FieldName = field.Name;
            control.DoAutoConfigure(field.FieldSetting);

            return control;
        }

        private static FieldControl CreateFieldControlByHint(string hint)
        {
            FieldControl control = null;

            var designator = hint.Split(new char[] { ':' });

            if (designator.Count() != 2)
            {
                Logger.WriteWarning(string.Concat("Malformed field control hint: ", hint, ", falling back to default."));
                return null;
            }

            var namespaces = from TagPrefixInfo tag in TagPrefixInfos
                             where tag.TagPrefix.Equals(designator[0], StringComparison.InvariantCultureIgnoreCase)
                             select tag.Namespace;

            var types = namespaces.Select(ns => TypeHandler.GetType(string.Concat(ns, ".", designator[1]))).Where(t => t != null);

            if (types.Count() > 0)
                control = (FieldControl)Activator.CreateInstance(types.First());

            if (control == null)
                Logger.WriteWarning(string.Concat("Failed to instantiate field control by hint: ", hint, ", falling back to default."));

            return control;
        }

        private static FieldControl CreateDefaultDateTimeControl(DateTimeFieldSetting dateTimeFieldSetting)
        {
            Type controlType = null;
            FieldControl control = null;
            controlType = typeof(DatePicker);
            control = (FieldControl)Activator.CreateInstance(controlType);

            return control;
        }

        private static FieldControl CreateDefaultLongTextControl(LongTextFieldSetting longTextSetting)
        {
            Type controlType = null;
            FieldControl control = null;

            if (longTextSetting != null)
            {
                if (longTextSetting.ControlHint == "control:ChoiceOptionEditor")
                {
                    controlType = typeof(ChoiceOptionEditor);
                }
                if (longTextSetting.ControlHint == "control:SurveyRuleEditor")
                {
                    controlType = typeof(SurveyRuleEditor);
                }
                else
                {
                    switch (longTextSetting.TextType)
                    {
                        case TextType.RichText:
                            controlType = typeof(RichText);
                            break;
                        case TextType.AdvancedRichText:
                            controlType = typeof(RichText);
                            break;
                        default:
                            controlType = typeof(LongText);
                            break;
                    }
                }

                control = (FieldControl)Activator.CreateInstance(controlType);
            }

            return control;
        }

        private static FieldControl CreateDefaultChoiceControl(ChoiceFieldSetting choiceSetting)
        {
            Type controlType = null;
            ChoiceControl control = null;

            if (choiceSetting != null)
            {
                switch (choiceSetting.DisplayChoice)
                {
                    case DisplayChoice.CheckBoxes:
                        controlType = typeof(CheckBoxGroup);
                        break;
                    case DisplayChoice.DropDown:
                        controlType = typeof(DropDown);
                        break;
                    case DisplayChoice.RadioButtons:
                        controlType = typeof(RadioButtonGroup);
                        break;
                    default:
                        controlType = choiceSetting.AllowMultiple == true ? typeof(CheckBoxGroup) : typeof(DropDown);
                        break;
                }

                control = (ChoiceControl)Activator.CreateInstance(controlType);
            }

            return control;
        }

        //===============================================================================

        public GenericFieldControl()
        {
            history = new Stack<int>();
        }
        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            InitializeWizard();
            base.OnInit(e);
            AddControls();
        }

        private void InitializeWizard()
        {
            if (EnablePaging)
            {
                _wizard = new Wizard()
                              {
                                  DisplaySideBar = false,
                                  CssClass = "sn-wizard"
                              };

                _wizard.FinishPreviousButtonStyle.CssClass = "sn-btn-prev sn-button sn-submit";
                _wizard.StepPreviousButtonStyle.CssClass = "sn-btn-prev sn-button sn-submit";

                _wizard.StartNextButtonStyle.CssClass = "sn-btn-next sn-button sn-submit";
                _wizard.StepNextButtonStyle.CssClass = "sn-btn-next sn-button sn-submit";

                _wizard.FinishCompleteButtonStyle.CssClass = "sn-btn-finish sn-button sn-submit";

                _wizard.CancelButtonStyle.CssClass = "sn-btn-cancel sn-button sn-submit";
                

                var pbField = from pbf in this.Content.Fields where pbf.Value.FieldSetting.FieldClassName == "SenseNet.ContentRepository.Fields.PageBreakField" select pbf;

                var lastField = (from fs in this.Content.Fields orderby fs.Value.FieldSetting.FieldIndex where fs.Value.FieldSetting.Name.StartsWith("#") select fs.Value.FieldSetting).LastOrDefault() as PageBreakFieldSetting;

                for (var i = 0; i <= pbField.Count(); i++)
                {
                    _wizard.WizardSteps.Add(new WizardStep());
                }
                if (lastField == null)
                {
                    _wizard.WizardSteps.Add(new WizardStep());
                }
                this.Controls.Add(_wizard);
                var commandButtons = Page.FindControlRecursive("CommandButtons1") as CommandButtons;

                if (commandButtons != null)
                {
                    var cancelButton = commandButtons.CancelButton as Button;

                    if (cancelButton != null && cancelButton.Visible)
                    {
                        _wizard.DisplayCancelButton = true;
                        _wizard.CancelButtonClick += commandButtons.CancelButton_Click;
                    }
                    commandButtons.Visible = false;
                    _wizard.FinishButtonClick += commandButtons.SaveButton_Click;
                }
                _wizard.NextButtonClick += WizardNextButtonClick;
                _wizard.PreviousButtonClick += WizardPreviousButtonClick;
            }
        }

        protected override object SaveControlState()
        {
            var obj = base.SaveControlState();

            if (history != null)
            {
                if (obj != null)
                {
                    return new Pair(obj, history);
                }
                else
                {
                    return (history);
                }
            }
            else
            {
                return obj;
            }
        }

        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                Pair p = savedState as Pair;
                if (p != null)
                {
                    base.LoadControlState(p.First);
                    history = (Stack<int>)p.Second;
                }
                else
                {
                    if (savedState is Stack<int>)
                    {
                        history = (Stack<int>)savedState;
                    }
                    else
                    {
                        base.LoadControlState(savedState);
                    }
                }
            }
        }

        void WizardPreviousButtonClick(object sender, WizardNavigationEventArgs e)
        {
            _wizard.ActiveStepIndex = history.Pop();
        }

        void WizardNextButtonClick(object sender, WizardNavigationEventArgs e)
        {
            this.ContentView.UpdateContent();
            var sb = new StringBuilder();
            foreach (var item in _wizard.ActiveStep.Controls)
            {
                var fieldControl = item as FieldControl;
                if (fieldControl != null && !fieldControl.Field.IsValid)
                {
                    sb.Clear();
                    
                    foreach (var param in fieldControl.Field.ValidationResult.GetParameterNames())
                    {
                        sb.Append(param);
                        sb.Append(": ");
                        sb.AppendLine(fieldControl.Field.ValidationResult.GetParameter(param).ToString());
                    }
                    
                    fieldControl.SetErrorMessage(sb.ToString());
                    e.Cancel = true;
                }
            }

            history.Push(e.CurrentStepIndex);

            PageBreakFieldSetting currentPageBreak;

            try
            {
                currentPageBreak = (from pbf in this.Content.Fields
                                    where
                                        pbf.Value.FieldSetting.FieldClassName ==
                                        "SenseNet.ContentRepository.Fields.PageBreakField"
                                    orderby pbf.Value.FieldSetting.FieldIndex
                                    select pbf).ElementAt(e.CurrentStepIndex).Value.FieldSetting as PageBreakFieldSetting;
            }
            catch
            {
                currentPageBreak = null;
            }

            if (currentPageBreak != null)
            {
                var ruleXml = HttpUtility.HtmlDecode(currentPageBreak.Rule);

                var doc = new XmlDocument();
                try
                {
                    doc.LoadXml(ruleXml);
                }
                catch
                {
                    return;
                }

                var selectedQuestion = doc.DocumentElement.GetAttribute("Question");

                var rules = new Dictionary<string, int>();

                foreach (XPathNavigator node in doc.DocumentElement.CreateNavigator().SelectChildren(XPathNodeType.Element))
                {
                    var answerId = node.GetAttribute("AnswerId", "");
                    int jumpToPage;
                    int.TryParse(node.Value, out jumpToPage);
                    if (jumpToPage != -1)
                    {
                        rules.Add(answerId, jumpToPage);
                    }
                    else
                    {
                        rules.Add(answerId, _wizard.WizardSteps.Count - 1);
                    }
                }

                var answerControlEnum = (from fc in this.ContentView.FieldControls
                                         where fc.FieldName == selectedQuestion
                                         select fc);

                var answerControl = answerControlEnum.FirstOrDefault() as FieldControl;

                List<string> answer = null;

                if (answerControl != null)
                {
                    answer = answerControl.GetData() as List<string>;
                }

                if (answer != null && answer.Count != 0 && rules.ContainsKey(answer.FirstOrDefault()))
                {
                    _wizard.ActiveStepIndex = rules[answer.FirstOrDefault()];
                }
                else if (selectedQuestion != "-100")
                {
                    _wizard.ActiveStepIndex = _wizard.ActiveStepIndex + 1;
                    _wizard.ActiveStepIndex = _wizard.ActiveStepIndex - 1;
                    this.ContentView.ContentException = new Exception("Compulsory: " + answerControl.FieldName.TrimStart('#'));
                }

            }
            else
            {
                _wizard.ActiveStepIndex = _wizard.WizardSteps.Count - 1;
            }

        }

        private bool IsReadOnly(string fieldName)
        {
            return ReadonlyList != null && ReadonlyList.Contains(fieldName);
        }

        private Wizard _wizard;

        private int currentPage;

        private void AddControls()
        {
            var cv = this.Parent as ContentView;
            if (cv == null)
                return;

            var content = this.Content;
            if (content == null)
                return;

            var fields = content.Fields;

            currentPage = 0;

            if (ContentListFieldsOnly || string.IsNullOrEmpty(FieldsOrder) || FieldsOrder.Trim() == "*")
            {
                AddAllFields(cv, fields);
            }
            else
            {
                AddFieldsOrder(cv, fields);
            }

            if (AdvancedPanel != null && AdvancedPanel.Controls.Count > 0)
            {
                var advancedButton = Page.LoadControl("/Root/System/SystemPlugins/Controls/AdvancedPanelButton.ascx") as AdvancedPanelButton;

                if (advancedButton != null)
                {
                    this.Controls.Add(advancedButton);
                    this.Controls.Add(AdvancedPanel);

                    advancedButton.AdvancedPanelId = AdvancedPanel.ClientID;
                }
            }
        }

        private void AddNameAndUrlName(List<string> fieldNames, ContentView cv, IDictionary<string, Field> fields)
        {
            if (ContentListFieldsOnly)
                return;

            // name and urlname comes first
            if (ExcludedList == null || !ExcludedList.Contains("DisplayName"))
            {
                var nameField = fieldNames.Where(f => f == "DisplayName").FirstOrDefault();
                if (nameField != null)
                {
                    Field field = null;
                    if (fields.TryGetValue(nameField, out field))
                        AddFieldControl(cv, field);
                }
            }
            if (ExcludedList == null || !ExcludedList.Contains("Name"))
            {
                var urlNameField = fieldNames.Where(f => f == "Name").FirstOrDefault();
                if (urlNameField != null)
                {
                    Field field = null;
                    if (fields.TryGetValue(urlNameField, out field))
                        AddFieldControl(cv, field);
                }
            }
        }

        private void AddAllFields(ContentView cv, IDictionary<string, Field> fields)
        {
            var visibleFieldNames = GetVisibleFieldNames(this.Content, this.ContentView.ViewMode);
            AddNameAndUrlName(visibleFieldNames, cv, fields);

            foreach (var fieldName in visibleFieldNames)
            {
                if (fieldName == "Name" || fieldName == "DisplayName")
                    continue;

                if (ExcludedList != null && ExcludedList.Contains(fieldName))
                    continue;

                if (ContentListFieldsOnly && !fieldName.StartsWith("#"))
                    continue;

                Field field;
                if (!fields.TryGetValue(fieldName, out field))
                    continue;

                AddFieldControl(cv, field);
            }

            if (EnablePaging) AddThankYouPage();
        }

        private void AddFieldsOrder(ContentView cv, IDictionary<string, Field> fields)
        {
            string[] fieldList = FieldsOrder.Split(' ');
            AddNameAndUrlName(fieldList.ToList(), cv, fields);

            foreach (string fieldName in fieldList)
            {
                if (fieldName == "Name" || fieldName == "DisplayName")
                    continue;

                Field field = null;
                if (fields.TryGetValue(fieldName, out field))
                    AddFieldControl(cv, field);
            }

            if (EnablePaging) AddThankYouPage();
        }

        private void AddFieldControl(ContentView cv, Field field)
        {
            if (field == null)
                return;
            if (field.FieldSetting.FieldClassName == typeof(PageBreakField).ToString())
            {
                currentPage++;
                return;
            }

            Control control = CreateDefaultFieldControl(field);

            var fieldControl = control as FieldControl;
            if (IsReadOnly(field.Name) && fieldControl != null)
                fieldControl.ReadOnly = true;

            if (fieldControl != null)
                fieldControl.FrameMode = FieldControlFrameMode.ShowFrame;

            control.ID = String.Concat("Generic", _id++);

            var visibility = GetFieldVisibility(cv.ViewMode, field);

            if (!EnablePaging)
            {
                if (visibility == FieldVisibility.Advanced && AdvancedPanel != null)
                {
                    AdvancedPanel.Controls.Add(control);
                }
                else
                {
                    Controls.Add(control);
                }
            }
            else
            {
                _wizard.WizardSteps[currentPage].Controls.Add(control);
            }
        }

        private void AddThankYouPage()
        {
            var parent = ContentRepository.Content.Load((int)this.Content["ParentId"]);
            Node landingPage = null;
            Node landingView = null;

            if (parent.Fields.ContainsKey("LandingPage") && parent.Fields.ContainsKey("PageContentView"))
            {
                landingPage = parent["LandingPage"] as Node;
                landingView = parent["PageContentView"] as Node;
            }

            if (landingPage == null || landingView == null) return;

            var landingContent = ContentRepository.Content.Create(landingPage);
            var landingCV = SingleContentView.Create(landingContent, this.Page, ViewMode.Browse, landingView.Path);
            _wizard.WizardSteps[_wizard.WizardSteps.Count - 1].Controls.Add(landingCV);
            _wizard.WizardSteps[_wizard.WizardSteps.Count - 2].AllowReturn = false;
        }

        public static FieldVisibility GetFieldVisibility(ViewMode viewMode, Field field)
        {
            if (field == null)
                return FieldVisibility.Show;

            switch (viewMode)
            {
                case ViewMode.Browse:
                    return field.FieldSetting.VisibleBrowse;
                case ViewMode.Edit:
                case ViewMode.InlineEdit:
                    return field.FieldSetting.VisibleEdit;
                case ViewMode.New:
                case ViewMode.InlineNew:
                    return field.FieldSetting.VisibleNew;
                default:
                    return FieldVisibility.Show;
            }
        }
    }
}
