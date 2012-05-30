using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.i18n;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
    public class SurveyRuleUserControl : System.Web.UI.UserControl
    {
        public event EventHandler QuestionSelected;

        public void SurveyQuestionSelected(object sender, EventArgs e)
        {
            var ddlSurveyQuestion = sender as DropDownList;
            var listView = this.Page.FindControlRecursive("InnerListView") as ListView;

            if (ddlSurveyQuestion == null || listView == null)
                return;

            if (ddlSurveyQuestion.SelectedValue == "-1" || ddlSurveyQuestion.SelectedValue == "-100")
            {
                listView.DataSource = null;
                listView.DataBind();
                if (this.QuestionSelected != null)
                    this.QuestionSelected(this, EventArgs.Empty);

                return;
            }

            var survey = ContentRepository.Content.Create(PortalContext.Current.ContextNode);

            var customFields = survey.Fields["FieldSettingContents"] as ReferenceField;
            if (customFields == null)
                return;

            var questions = customFields.OriginalValue as List<SenseNet.ContentRepository.Storage.Node>;

            var selectedQuestion = from q in questions where q.Name == ddlSurveyQuestion.SelectedValue select q;
            var pageBreaks = from q in questions where q.NodeType.Name == "PageBreakFieldSetting" select q;
            var answers = ((ChoiceFieldSetting) ((FieldSettingContent) selectedQuestion.First()).FieldSetting).Options;
            var rules = answers.Select(opt => new SurveyRule(opt.Text, opt.Value, "", pageBreaks.Count())).ToList();

            listView.DataSource = rules;
            listView.DataBind();

            if (this.QuestionSelected != null)
                this.QuestionSelected(this, EventArgs.Empty);
        }

        public void ListItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var dataItem = e.Item as ListViewDataItem;
            if (dataItem == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Survey", "ListViewDataItemError") });
                return;
            }

            var surveyRule = dataItem.DataItem as SurveyRule;
            if (surveyRule == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Survey", "SurveyRuleError") });
                return;
            }

            var tempAnswer = surveyRule.Answer;
            if (tempAnswer.Length > 15)
            {
                tempAnswer = tempAnswer.Substring(0, 15) + "...";
            }

            var aNameLiteral = dataItem.FindControl("ltrAnswerName") as Literal;
            if (aNameLiteral != null)
                aNameLiteral.Text = tempAnswer;

            var aValueHidden = dataItem.FindControl("hidAnswerValue") as HiddenField;
            if (aValueHidden != null)
                aValueHidden.Value = surveyRule.AnswerId;

            var ddl = dataItem.FindControl("ddlJumpToPage") as DropDownList;
            if (ddl == null)
                throw new Exception("Cannot find control with ID ddlJumpToPage!");

            for (var i = 1; i <= surveyRule.Pages; i++)
            {
                ddl.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }

            ddl.Items.Add(new ListItem("Finish", "-1"));
            ddl.Text = surveyRule.JumpToPage;
        }
    }
}
