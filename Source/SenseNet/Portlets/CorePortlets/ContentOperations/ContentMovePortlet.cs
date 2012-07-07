using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using System.ComponentModel;

namespace SenseNet.Portal.Portlets
{
    public class ContentMovePortlet : ContentCollectionPortlet
    {
        public ContentMovePortlet()
        {
            this.Name = "Move";
            this.Description = "This portlet handles content move operations (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);

            Cacheable = false;   // by default, caching is switched off
            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Cache };
        }

        private string _viewPathMoveTo = "/Root/System/SystemPlugins/Portlets/ContentMove/MoveTo.ascx";
        private string _viewPathMove = "/Root/System/SystemPlugins/Portlets/ContentMove/Move.ascx";

        //================================================================ Portlet properties

        [WebDisplayName("View path for 'move under current content'")]
        [WebDescription("Path of the .ascx user control which provides the UI elements for the 'MOVE TO' behavior of the Move dialog")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ViewPathMoveTo
        {
            get { return _viewPathMoveTo; }
            set { _viewPathMoveTo = value; }
        }

        [WebDisplayName("View path for 'move current content'")]
        [WebDescription("Path of the .ascx user control which provides the UI elements for the 'MOVE' behavior of the Move dialog")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(110)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ViewPathMove
        {
            get { return _viewPathMove; }
            set { _viewPathMove = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }


        //================================================================ Controls

        private Label _contentLabel;
        protected Label ContentLabel
        {
            get { return _contentLabel ?? (_contentLabel = this.FindControlRecursive("ContentName") as Label); }
        }

        private Button _moveToButton;
        protected Button MoveToButton
        {
            get
            {
                return _moveToButton ?? (_moveToButton = this.FindControlRecursive("MoveToButton") as Button);
            }
        }

        private Button _moveCurrentButton;
        protected Button MoveCurrentButton
        {
            get
            {
                return _moveCurrentButton ?? (_moveCurrentButton = this.FindControlRecursive("MoveCurrentButton") as Button);
            }
        }

        private Button _doneButton;
        protected Button DoneButton
        {
            get
            {
                return _doneButton ?? (_doneButton = this.FindControlRecursive("DoneButton") as Button);
            }
        }

        private Button _cancelButton;
        protected Button CancelButton
        {
            get
            {
                return _cancelButton ?? (_cancelButton = this.FindControlRecursive("CancelButton") as Button);
            }
        }

        private TextBox _targetBox;
        protected TextBox MoveTargetTextBox
        {
            get
            {
                return _targetBox ?? (_targetBox = this.FindControlRecursive("MoveTargetTextBox") as TextBox);
            }
        }

        private Button _openPickerButton;
        protected Button OpenPickerButton
        {
            get
            {
                return _openPickerButton ?? (_openPickerButton = this.FindControlRecursive("OpenPickerButton") as Button);
            }
        }

        private Panel _panelMove;
        protected Panel MovePanel
        {
            get
            {
                return _panelMove ?? (_panelMove = this.FindControlRecursive("MovePanel") as Panel);
            }
        }

        private Panel _panelMsg;
        protected Panel MessagePanel
        {
            get
            {
                return _panelMsg ?? (_panelMsg = this.FindControlRecursive("MessagePanel") as Panel);
            }
        }

        private PlaceHolder _plcError;
        protected PlaceHolder ErrorPlaceholder
        {
            get
            {
                return _plcError ?? (_plcError = this.FindControlRecursive("ErrorPanel") as PlaceHolder);
            }
        }

        private Label _errorLabel;
        protected Label ErrorLabel
        {
            get
            {
                return _errorLabel ?? (_errorLabel = this.FindControlRecursive("ErrorLabel") as Label);
            }
        }

        //================================================================ Overrides

        protected override void CreateChildControls()
        {
            Controls.Clear();

            try
            {
                var viewControl = RequestIdList.Count == 0 ?
                    Page.LoadControl(ViewPathMove) as UserControl :
                    Page.LoadControl(ViewPathMoveTo) as UserControl;

                if (viewControl == null)
                    return;

                Controls.Add(viewControl);

                var genericContent = GetContextNode() as GenericContent;
                if (genericContent == null)
                    return;

                if (ContentLabel != null)
                    ContentLabel.Text = genericContent.DisplayName;

                BindEvents();
            }
            catch (Exception exc)
            {
                Logger.WriteException(exc);
            }

            ChildControlsCreated = true;
        }

        //====================================================================== Event handlers

        protected void MoveToButton_Click(object sender, EventArgs e)
        {
            try
            {
                HideErrorPanel();

                var contextNode = ContextNode;
                if (contextNode == null)
                    return;

                var exceptions = new List<Exception>();

                Node.MoveMore(RequestIdList, contextNode.Path, ref exceptions);

                if (exceptions.Count > 0)
                {
                    var sbError = new StringBuilder();
                    foreach (var exception in exceptions)
                    {
                        sbError.AppendFormat(" {0}", exception.Message);
                    }

                    SetError(sbError.ToString());
                }
                else
                {
                    if (RequestIdList.Count == 0 || RequestIdList.Count == 1)
                    {
                        var back = PortalContext.Current.BackUrl;
                        var oldUrlName = string.Format("/{0}?action=", (RequestIdList.Count == 1 ? RequestNodeList[0].Name : contextNode.Name));
                        var newUrlName = "?action=";

                        //redirect immediately to parent, because the current content 
                        //is moved somewhere else and cannot handle postback
                        if (back.Contains(oldUrlName))
                        {
                            back = back.Replace(oldUrlName, newUrlName);

                            var p = Page as PageBase;
                            if (p != null)
                                p.Response.Redirect(back, false);
                        }
                    }
                    
                    SetControlVisibility(true);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        protected void MoveCurrentButton_Click(object sender, EventArgs e)
        {
            try
            {
                HideErrorPanel();

                if (MoveTargetTextBox == null)
                    return;

                if (string.IsNullOrEmpty(MoveTargetTextBox.Text))
                {
                    SetError("Please choose a folder or content list to move to.");
                    return;
                }

                var target = Node.LoadNode(MoveTargetTextBox.Text);
                if (target == null)
                {
                    SetError("Target is invalid.");
                    return;
                }

                var contextNode = ContextNode;
                if (contextNode == null)
                {
                    SetError("Context node is invalid.");
                    return;
                }

                Node.Move(contextNode.Path, target.Path);

                var back = PortalContext.Current.BackUrl;
                var oldUrlName = string.Format("/{0}?action=", contextNode.Name);
                var newUrlName = "?action=";

                //redirect immediately to parent, because the current content 
                //is moved somewhere else and cannot handle postback
                if (back.Contains(oldUrlName))
                {
                    back = back.Replace(oldUrlName, newUrlName);

                    var p = Page as PageBase;
                    if (p != null)
                        p.Response.Redirect(back, false);
                }
                else
                {
                    CallDone();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                SetError(ex.Message);
            }
        }

        //====================================================================== Helper methods

        protected void BindEvents()
        {
            if (MoveToButton != null)
            {
                MoveToButton.Click += MoveToButton_Click;
                MoveToButton.Visible = RequestIdList.Count > 0;
            }

            if (MoveCurrentButton != null)
            {
                MoveCurrentButton.Click += MoveCurrentButton_Click;
                MoveCurrentButton.Visible = RequestIdList.Count == 0;
            }

            if (OpenPickerButton != null)
            {
                UITools.AddPickerCss();
                UITools.AddScript(UITools.ClientScriptConfigurations.SNPickerPath);
                OpenPickerButton.OnClientClick = GetOpenContentPicklerScript();
            }
        }

        protected void SetControlVisibility(bool finished)
        {
            if (MessagePanel != null)
                MessagePanel.Visible = finished;
            if (MovePanel != null)
                MovePanel.Visible = !finished;

            if (CancelButton != null)
                CancelButton.Visible = !finished;
            if (MoveToButton != null)
                MoveToButton.Visible = !finished;
            if (MoveCurrentButton != null)
                MoveCurrentButton.Visible = !finished;

            if (DoneButton != null)
                DoneButton.Visible = finished;
        }

        private void HideErrorPanel()
        {
            if (ErrorPlaceholder != null)
                ErrorPlaceholder.Visible = false;
            if (ErrorLabel != null)
                ErrorLabel.Visible = false;
        }

        private string GetOpenContentPicklerScript()
        {
            if (MoveTargetTextBox == null)
                return string.Empty;

            var callBack = string.Format("function(resultData) {{ if (!resultData) return; $('#{0}').val(resultData[0].Path); }}",
                MoveTargetTextBox.ClientID);

            var script = string.Format(@"javascript: SN.PickerApplication.open({{ MultiSelectMode: 'none', callBack: {0} }}); return false;",
                callBack);

            return script;
        }

        private void SetError(string errorMessage)
        {
            if (ErrorLabel == null)
                return;

            ErrorLabel.Visible = true;

            if (ErrorPlaceholder != null)
                ErrorPlaceholder.Visible = true;

            ErrorLabel.Text = errorMessage;
        }
    }
}
