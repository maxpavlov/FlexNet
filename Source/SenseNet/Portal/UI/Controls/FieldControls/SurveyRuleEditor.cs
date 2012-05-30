using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
    public class SurveyRuleEditor : FieldControl
    {
        private string _controlPath = "/Root/System/SystemPlugins/ListView/SurveyRuleEditor.ascx";
        public string ControlPath
        {
            get { return _controlPath; }
            set { _controlPath = value; }
        }

        protected virtual bool ControlStateLoaded { get; set; }

        public SurveyRuleEditor()
        {
            InnerControlID = "InnerListView";
        }

        private List<SurveyRule> _surveyRulesList;
        protected List<SurveyRule> SurveyRulesList
        {
            get { return _surveyRulesList; }
            set { _surveyRulesList = value; }
        }


        private ListView _dataListView;
        protected ListView DataListView
        {
            get { return _dataListView; }
            set { _dataListView = value; }
        }

        private DropDownList _ddlSurveyQuestions;
        protected DropDownList DdlSurveyQuestions
        {
            get { return _ddlSurveyQuestions; }
            set { _ddlSurveyQuestions = value; }
        }

        private string _selectedQuestion = "-1";

        protected override void OnInit(EventArgs e)
        {
            this.ControlStateLoaded = false;
            Page.RegisterRequiresControlState(this); // this calls LoadControlState

            if (!UseBrowseTemplate && !UseEditTemplate && !UseInlineEditTemplate && !string.IsNullOrEmpty(this.ControlPath))
            {
                var c = Page.LoadControl(this.ControlPath) as SurveyRuleUserControl;
                if (c != null)
                {
                    c.QuestionSelected += RuleControl_QuestionSelected;
                    DdlSurveyQuestions = c.FindControlRecursive("ddlSurveyQuestion") as DropDownList;

                    DataListView = c.FindControlRecursive("InnerListView") as ListView;

                    if (!this.Page.IsPostBack || SurveyRulesList == null)
                        SurveyRulesList = new List<SurveyRule>();

                    SetQuestions();
                    this.Controls.Add(c);
                }
            }

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DataListView != null && this.DataListView.DataSource == null)
            {
                this.DataListView.DataSource = this.SurveyRulesList;
                this.DataListView.DataBind();
            }
        }

        protected void RuleControl_QuestionSelected(object sender, EventArgs e)
        {
            this.SurveyRulesList = GetDataFromList().ToList();
        }

        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                var state = savedState as object[];
                if (state != null && state.Length == 2)
                {
                    base.LoadControlState(state[0]);

                    if (state[1] != null)
                    {
                        this.SurveyRulesList = (List<SurveyRule>)state[1];
                        this.ControlStateLoaded = true;
                    }
                }
            }
            else
                base.LoadControlState(savedState);
        }

        protected override object SaveControlState()
        {
            var state = new object[2];

            state[0] = base.SaveControlState();
            state[1] = this.SurveyRulesList;

            return state;
        }

        private IEnumerable<SurveyRule> GetDataFromList()
        {
            var result = new List<SurveyRule>();

            foreach (var item in DataListView.Items)
            {
                var txt = ((Literal)item.FindControl("ltrAnswerName")).Text;
                var hid = ((HiddenField)item.FindControl("hidAnswerValue")).Value;
                var jmp = ((DropDownList)item.FindControl("ddlJumpToPage")).Text;

                result.Add(new SurveyRule(txt, hid, jmp, 0));
            }

            return result;
        }

        public override object GetData()
        {
            var result = string.Empty;
            var sw = new StringWriter();
            var ws = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using (var writer = XmlWriter.Create(sw, ws))
            {
                if (writer != null)
                {
                    writer.WriteStartElement("SurveyRule");
                    writer.WriteAttributeString("Question", DdlSurveyQuestions.SelectedValue);

                    foreach (var rule in GetDataFromList())
                    {
                        rule.WriteXml(writer);
                    }

                    writer.WriteEndElement();
                    writer.Flush();

                    result = sw.ToString();
                }
            }

            return result;
        }

        private void SetQuestions()
        {
            var currentSurvey = ContentRepository.Content.Create(PortalContext.Current.ContextNode);
            var customFields = currentSurvey.Fields["FieldSettingContents"] as ReferenceField;
            if (customFields == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Survey", "ReferenceFieldError") });
                return;
            }

            var questions = customFields.OriginalValue as List<Node>;
            if (questions == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Survey", "OriginalValueError") });
                return;
            }

            var cfs = NodeType.GetByName("ChoiceFieldSetting");
            var choiceTypeIds = cfs.GetAllTypes().ToIdArray();

            var pbfs = NodeType.GetByName("PageBreakFieldSetting");

            DdlSurveyQuestions.Items.Add(new ListItem("Choose a question!", "-100"));

            for (var i = questions.Count - 1; i >= 0; i--)
            {
                var questionTypeId = questions[i].NodeType.Id;

                if (choiceTypeIds.Contains(questionTypeId))
                {
                    var q = questions[i] as FieldSettingContent;
                    if (q == null)
                        continue;

                    DdlSurveyQuestions.Items.Add(
                        q.FieldSetting.DisplayName.Length > 15
                            ? new ListItem(q.FieldSetting.DisplayName.Substring(0, 15) + "...", q.Name)
                            : new ListItem(q.FieldSetting.DisplayName, q.Name));
                }

                if (questions[i].NodeType == pbfs)
                    continue;
            }
        }

        public override void SetData(object data)
        {
            // saving the field
            if (this.ControlStateLoaded)
                return;

            // loading the field
            // TODO: set question and page jumping values regarding to the following data:
            string data2 = data as string;

            if (string.IsNullOrEmpty(data2))
                return;

            data2 = data2.Replace("&lt;", "<").Replace("&gt;", ">");
            ParseRules(data2);
        }

        private void ParseRules(string xml)
        {
            var survey = SenseNet.ContentRepository.Content.Create(PortalContext.Current.ContextNode);
            var customFields = survey.Fields["FieldSettingContents"] as ReferenceField;
            var questions = customFields.OriginalValue as List<SenseNet.ContentRepository.Storage.Node>;
            var pageBreaks = from q in questions where q.NodeType.Name == "PageBreakFieldSetting" select q;

            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch
            {
                return;
            }

            _selectedQuestion = doc.DocumentElement.GetAttribute("Question");

            if (_selectedQuestion == "-100")
            {
                DataListView.DataSource = null;
                DataListView.DataBind();
                return;
            }

            foreach (XPathNavigator node in doc.DocumentElement.CreateNavigator().SelectChildren(XPathNodeType.Element))
            {
                var answer = node.GetAttribute("Answer", "");
                var answerId = node.GetAttribute("AnswerId", "");
                var jumpToPage = node.Value;
                SurveyRulesList.Add(new SurveyRule(answer, answerId, jumpToPage, pageBreaks.Count()));
            }

            DdlSurveyQuestions.SelectedValue = _selectedQuestion;

            DataListView.DataSource = SurveyRulesList;
            DataListView.DataBind();
        }

    }
}
