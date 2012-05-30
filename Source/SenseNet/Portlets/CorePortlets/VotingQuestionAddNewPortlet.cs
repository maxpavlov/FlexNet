using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal.Portlets
{
    public class VotingQuestionAddNewPortlet : ContentAddNewPortlet
    {

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Second question contentview path")]
        [WebDescription("Path of the contentview which renders a message if voting already has a question.")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
        public string SecondQuestionContentViewPath { get; set; }


        public VotingQuestionAddNewPortlet()
        {
            Name = "Voting question creator";
            Description = "This portlet creates a new question (if there is no other one) for the given voting (context bound)";
            Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }

        protected override void CreateChildControls()
        {
            var currentContent = ContentRepository.Content.Create(ContextNode);

            ReferenceField refField;

            try
            {
                refField = currentContent.Fields["FieldSettingContents"] as ReferenceField;
            }
            catch (Exception)
            {
                base.CreateChildControls();
                return;   
            }
            

            if (refField != null)
            {
                var originalValue = refField.OriginalValue as List<Node>;

                _currentUserControl = LoadUserInterface(Page, GuiPath);

                var addedFields = from fs in originalValue where fs.Name.StartsWith("#") select fs;

                if (addedFields.Count() == 0)
                {
                    base.CreateChildControls();
                    return;
                }
            }

            if (!String.IsNullOrEmpty(SecondQuestionContentViewPath))
            {
                var contentView = ContentView.Create(currentContent, Page, ViewMode.InlineNew, SecondQuestionContentViewPath);
                Controls.Add(contentView);
            }
            else
            {
                Controls.Add(new Literal { Text = "Voting already has a question." });
            }
        }
    }
}
