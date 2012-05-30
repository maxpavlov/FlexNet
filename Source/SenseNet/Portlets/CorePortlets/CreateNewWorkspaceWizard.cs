using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.AppModel;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using SenseNet.Diagnostics;
using SenseNet.Portal.Workspaces;
using Content=SenseNet.ContentRepository.Content;

namespace SenseNet.Portal.Portlets
{
    [Serializable]
    public class WorkspaceWizardSetting
    {
        public string SelectedWorkspacePath { get; set; }
        public string WorkspaceNewName { get; set; }
        public string WorkspaceNewDescription { get; set; }
        
        public string GetSelectedWorkspaceName()
        {
            return SelectedWorkspacePath.Substring(SelectedWorkspacePath.LastIndexOf("/") + 1);
        }
    }

    public class WorkspaceWizardFacade
    {
        // Members ////////////////////////////////////////////////////////
        public string WorkspaceTemplatesPath { get; set; }
        public string TargetPath { get; set; }
        public Control CurrentUserControl { get; set; }
        public Label ErrorMessageControl
        {
            get
            {
                var errorMessageControl = CurrentUserControl.FindControl("ErrorMessage") as Label;
                return errorMessageControl;
            }
        }
        public RadioButtonList WorkspaceListControl
        {
            get
            {
                var wizardControl = CurrentUserControl.FindControl("Wizard1");

                if (wizardControl != null)
                {
                    var wizardStep = wizardControl.FindControl("ChooseWorkspaceStep") as TemplatedWizardStep;

                    if (wizardStep != null)
                    {
                        return wizardStep.ContentTemplateContainer.FindControl("WorkspaceList") as RadioButtonList;
                    }
                }

                return null;
            }
        }
        public TextBox WorkspaceNewNameControl
        {
            get
            {
                return ((TemplatedWizardStep)CurrentUserControl.FindControl("Wizard1").FindControl("WorkspaceFormStep")).ContentTemplateContainer.FindControl("WorkspaceNameText") as TextBox;
            }
        }
        public Label NewWorkspaceTypeNameControl
        {
            get
            {
                return ((TemplatedWizardStep)CurrentUserControl.FindControl("Wizard1").FindControl("WorkspaceFormStep")).ContentTemplateContainer.FindControl("NewWorkspaceTypeName") as Label;
            }
        }
        public TextBox WorkspaceNewDescriptionControl
        {
            get
            {
                return ((TemplatedWizardStep)CurrentUserControl.FindControl("Wizard1").FindControl("WorkspaceFormStep")).ContentTemplateContainer.FindControl("WorkspaceDescText") as TextBox;
            }
        }
        public WorkspaceWizardSetting WizardState
        {
            get
            {
                return GetWizardState();
            }
            set
            {
                SetWizardState(value);
            }
        }
        public HtmlAnchor NewWorkspaceLink
        {
            get
            {
                return ((TemplatedWizardStep)CurrentUserControl.FindControl("Wizard1").FindControl("Complete")).CustomNavigationTemplateContainer.FindControl("NewWorkspaceLink") as HtmlAnchor;
            }
        }
        public Label ProgressHeaderLabel
        {
            get
            {
                return ((TemplatedWizardStep)CurrentUserControl.FindControl("Wizard1").FindControl("Progress")).ContentTemplateContainer.FindControl("ProgressHeaderLabel") as Label;
            }
        }

        public bool HasError { get; set; }

        // Constructors ///////////////////////////////////////////////////
        public WorkspaceWizardFacade()
        {
            WorkspaceTemplatesPath = "/Root/System/WorkspaceTemplates";
            TargetPath = "/Root";
        }

        // Methods ////////////////////////////////////////////////////////
        internal void Initialize()
        {
            try
            {
                BindEventHandlers();
                LoadWorkspaceList();
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                HasError = true;
                ErrorMessageControl.Visible = true;
                ErrorMessageControl.Text = String.Format("Error occured during creating child controls: {0}", exc.Message);
            }
        }

        // Events ////////////////////////////////////////////////////////
        void wizard_PreviousButtonClick(object sender, WizardNavigationEventArgs e)
        {
            var currentStepIndex = e.CurrentStepIndex;
            switch (currentStepIndex)
            {
                case 0:
                    break;
                case 1:
                    ChangeWizardState();
                    break;
                case 2:
                    break;
            }
        }
        void wizard_NextButtonClick(object sender, WizardNavigationEventArgs e)
        {
            var currentStepIndex = e.CurrentStepIndex;
            switch (currentStepIndex)
            {
                case 0:
                    var list = WorkspaceListControl;
                    var selectedValue = list.SelectedValue;
                    if (WizardState == null)
                    {
                        var workspaceSettings = new WorkspaceWizardSetting
                        {
                            SelectedWorkspacePath = selectedValue
                        };
                        WizardState = workspaceSettings;
                    }
                    else
                        WizardState.SelectedWorkspacePath = selectedValue;
                    break;
                case 1:
                    ChangeWizardState();
                    break;
                case 2:
                    break;
            }
        }
        void wizard_ActiveStepChanged(object sender, EventArgs e)
        {
            var wiz = (Wizard)sender;
            switch (wiz.ActiveStepIndex)
            {
                case 0:
                    // set workspace list control selectedvalue and selectedindex
                    break;
                case 1:
                    NewWorkspaceTypeNameControl.Text = WizardState.GetSelectedWorkspaceName();
                    WorkspaceNewNameControl.Text = WizardState.WorkspaceNewName;
                    WorkspaceNewDescriptionControl.Text = WizardState.WorkspaceNewDescription;
                    break;
                case 2:
                    var setting = WizardState;
                    var headerString = String.Format(@"the wizard is creating your '{0}' workspace named '{1}'.", setting.GetSelectedWorkspaceName(), setting.WorkspaceNewName);
                    ProgressHeaderLabel.Text = headerString;
                    break;
                case 3:
                    CreateNewWorkspace();
                    break;

            }
        }

        // Internals //////////////////////////////////////////////////////
        private void BindEventHandlers()
        {

            var wizard = CurrentUserControl.FindControl("Wizard1") as Wizard;
            if (wizard == null) 
                return;
            wizard.ActiveStepChanged += new EventHandler(wizard_ActiveStepChanged);
            wizard.NextButtonClick += new WizardNavigationEventHandler(wizard_NextButtonClick);
            wizard.PreviousButtonClick += new WizardNavigationEventHandler(wizard_PreviousButtonClick);
        }
        private void ChangeWizardState()
        {
            var setting = GetWizardState();
            setting.WorkspaceNewDescription = WorkspaceNewDescriptionControl.Text;
            setting.WorkspaceNewName = WorkspaceNewNameControl.Text;
            SetWizardState(setting);
        }
        internal void CreateNewWorkspace()
        {
            try
            {
                var state = WizardState;
                var newName = state.WorkspaceNewName;
                var sourceNodePath = state.SelectedWorkspacePath;
                var targetNode = PortalContext.Current.ContextNodeHead ?? NodeHead.Get(TargetPath);
                
                var sourceNode = Node.LoadNode(sourceNodePath);
                var contentTypeName = sourceNode.Name;


                Content workspace = null;

                workspace = ContentTemplate.HasTemplate(contentTypeName)
                    ? ContentTemplate.CreateTemplated(Node.LoadNode(targetNode.Id), ContentTemplate.GetTemplate(contentTypeName), newName) 
                    : Content.CreateNew(contentTypeName, Node.LoadNode(targetNode.Id), newName);

                workspace.Fields["Description"].SetData(state.WorkspaceNewDescription);
                workspace.Save();
                
                var newPath = new StringBuilder(PortalContext.Current.OriginalUri.GetLeftPart(UriPartial.Authority)).Append(targetNode.Path).Append("/").Append(newName);
                NewWorkspaceLink.HRef = newPath.ToString();
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                HasError = true;
                ErrorMessageControl.Visible = true;
                ErrorMessageControl.Text = exc.Message;
                // log exception
            }
        }
        private void LoadWorkspaceList()
        {
            var query = new NodeQuery();
            var path = ActiveSchema.NodeTypes["Workspace"].NodeTypePath;
            var s = String.Concat(Repository.ContentTypesFolderPath, RepositoryPath.PathSeparator, path, RepositoryPath.PathSeparator);
            query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, s));
            var nodes = query.Execute().Nodes;

            FillWorkspaceListControl(WorkspaceListControl, nodes);
        }
        private void FillWorkspaceListControl(ListControl list, IEnumerable<Node> children)
        {
            if (list == null)
            {
                HasError = true;
            }
            else
            {
                foreach (var content in children)
                    list.Items.Add(new ListItem {Text = content.Name, Value = content.Path});

                if (list.Items.Count == 0)
                {
                    HasError = true;
                    ErrorMessageControl.Visible = true;
                    ErrorMessageControl.Text = "Workspace list couldn't be loaded.";
                }
                else
                    list.Items[0].Selected = true;
            }
        }
        private void SetWizardState(WorkspaceWizardSetting value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            var formatter = new LosFormatter();
            var writer = new StringWriter();
            formatter.Serialize(writer, value);
            var input = CurrentUserControl.FindControl("_settings") as HtmlInputHidden;
            if (input == null)
                return;
            input.Value = writer.ToString();
        }
        private WorkspaceWizardSetting GetWizardState()
        {
            var input = CurrentUserControl.FindControl("_settings") as HtmlInputHidden;
            if (input == null)
                return null;
            var value = input.Value;
            if (string.IsNullOrEmpty(input.Value))
                return null;
            var formatter = new LosFormatter();
            var result = formatter.Deserialize(value);
            return result as WorkspaceWizardSetting;
        }

    }

    public class CreateNewWorkspaceWizard : PortletBase
    {
        // Personalized properties ////////////////////////////////////////
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("View path")]
        [WebDescription("Holds the path of the portlet user interface control.")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string WorkspaceWizardUserControlPath { get; set; }
        
        // Members ////////////////////////////////////////////////////////
        private WorkspaceWizardFacade WizardLogic = InitializePortletLogic();
        private static WorkspaceWizardFacade InitializePortletLogic()
        {
            return new WorkspaceWizardFacade();
        }

        // Constructors ///////////////////////////////////////////////////
        public CreateNewWorkspaceWizard()
        {
            this.Name = "New workspace wizard";
            this.Description = "This portlet creates a new workspace";
            this.Category = new PortletCategory(PortletCategoryType.System);
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
            
            WizardLogic.CurrentUserControl = LoadUserInterface(Page, WorkspaceWizardUserControlPath);
            Controls.Add(WizardLogic.CurrentUserControl);

            WizardLogic.Initialize();

            if (WizardLogic.HasError)
                return;

            ChildControlsCreated = true;
        }
        // Internals //////////////////////////////////////////////////////
        private Control LoadUserInterface(TemplateControl page, string path)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            Control ui;
            try
            {
                ui = page.LoadControl(path);
            }
            catch (Exception e) //logged
            {
                Logger.WriteException(e);
                HasError = true;
                var msg = String.Format("{0}", e.Message);
                var msgControl = new Label { ID = "RuntimeErrMsg", Text = msg, ForeColor = Color.Red };
                return msgControl;
            }
            return ui;
        }


    }
}
