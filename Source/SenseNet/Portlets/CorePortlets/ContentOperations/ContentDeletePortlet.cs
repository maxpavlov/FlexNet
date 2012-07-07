using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections.Generic;

namespace SenseNet.Portal.Portlets
{
    public class ContentDeletePortlet : ContentCollectionPortlet
    {
        private string _viewPathSingleDelete = "/Root/System/SystemPlugins/Portlets/ContentDelete/Confirmation.ascx";
        private string _viewPathBatchDelete = "/Root/System/SystemPlugins/Portlets/ContentDelete/DeleteBatch.ascx";
        private Control _ui;

        public ContentDeletePortlet()
        {
            this.Name = "Delete";
            this.Description = "This portlet handles content delete operations (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);

            Cacheable = false;   // by default, caching is switched off
            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Cache };
        }

        //=========================================================== Portlet properties

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("View for single delete")]
        [WebDescription("Path of the .ascx user control which provides the elements of the delete dialog")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string UserInterfacePath
        {
            get { return _viewPathSingleDelete; }
            set { _viewPathSingleDelete = value; }
        }

        [WebDisplayName("View for batch delete")]
        [WebDescription("Path of the .ascx user control which provides the UI elements for the 'batch delete' behavior of the portlet")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(110)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ViewPathBatchDelete
        {
            get { return _viewPathBatchDelete; }
            set { _viewPathBatchDelete = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        //=========================================================== Controls

        private MessageControl _msgControl;
        protected MessageControl MessageControl
        {
            get
            {
                return _msgControl ?? (_msgControl = this.FindControlRecursive("MessageControl") as MessageControl);
            }
        }

        //=========================================================== Overrides

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.HiddenPropertyCategories == null)
                this.HiddenPropertyCategories = new List<string>();
            this.HiddenPropertyCategories.Add("Cache"); // this is an administrative portlet we don't need to use Cache functionality.
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
                        
            try
            {
                _ui = RequestIdList.Count == 0 ?
                    Page.LoadControl(UserInterfacePath) as UserControl :
                    Page.LoadControl(ViewPathBatchDelete) as UserControl;

                if (_ui != null)
                {
                    Controls.Add(_ui);
                    BindEvents();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                ShowException(ex);
            }               

            ChildControlsCreated = true;            
        }

        //=========================================================== Event handlers

        protected virtual void MessageControlButtonsAction(object sender, CommandEventArgs e)
        {
            ProcessCommand(e);  // use this method for testing, instead of mock a button with its events.
        }

        private void BindEvents()
        {
            if (MessageControl == null)
                return;

            MessageControl.ButtonsAction += MessageControlButtonsAction;

            //switching visibility of the controls on and off within the user control

            var gContent = ContextNode as GenericContent;
            var bag = ContextNode as TrashBag;
            var title = gContent == null ? ContextNode.Name : gContent.DisplayName;
            var isInTrash = ContentsAreInTrash();

            var folder = bag == null ? ContextNode as IFolder : bag.DeletedContent as IFolder;
            if (folder == null)
                SetLabel(MessageControl, "DeleteOneLabel", title, ContextNode.ParentPath);
            else
                SetLabel(MessageControl, "DeleteFolderLabel", title, ContextNode.ParentPath);


            TrashBin trashBin = null;

            try
            {
                trashBin = TrashBin.Instance;
            }
            catch (SenseNetSecurityException)
            {
                //trashbin is not accessible due to lack of permissions
            }
            
            if (trashBin == null)   // trashbin ain't available 
            {
                MessageControl.Errors.Add("Trashbin node not found under /Root/Trash, trashbin functionality unavailable.");
                ToggleControlVisibility(MessageControl, "PermanentDeleteLabel", true);
                ToggleControlVisibility(MessageControl, "BinEnabledLabel", false);
                ToggleControlVisibility(MessageControl, "PermanentDeleteCheckBox", false);
            } 
            else
            {   // trashbin is available
                var nodesInTreeCount = GetNodesInTreeCount();

                bool? trashDisabled = !ContentsAreTrashable();

                if (gContent != null)
                {
                    ToggleControlVisibility(MessageControl, "BinDisabledGlobalLabel", !trashBin.IsActive);

                    if (isInTrash)
                    {
                        ToggleControlVisibility(MessageControl, "BinEnabledLabel", false);
                        ToggleControlVisibility(MessageControl, "PermanentDeleteCheckBox", false);
                        ToggleControlVisibility(MessageControl, "BinNotConfiguredLabel", false);
                        ToggleControlVisibility(MessageControl, "PermanentDeleteLabel", true);
                        ToggleControlVisibility(MessageControl, "PurgeFromTrashLabel", true);
                        ToggleControlVisibility(MessageControl, "TooMuchContentLabel", false);
                    }
                    else if (trashBin.BagCapacity == 0 || trashBin.BagCapacity > nodesInTreeCount)
                    {
                        if (trashDisabled.Value)
                        {
                            ToggleControlVisibility(MessageControl, "BinEnabledLabel", false);
                            ToggleControlVisibility(MessageControl, "BinNotConfiguredLabel", trashBin.IsActive);
                            ToggleControlVisibility(MessageControl, "PermanentDeleteLabel", true);                            
                        } 
                        else
                        {
                            ToggleControlVisibility(MessageControl, "BinEnabledLabel", trashBin.IsActive);
                            ToggleControlVisibility(MessageControl, "PermanentDeleteCheckBox", trashBin.IsActive);
                            ToggleControlVisibility(MessageControl, "BinNotConfiguredLabel", false);
                            ToggleControlVisibility(MessageControl, "PermanentDeleteLabel", !trashBin.IsActive);
                        }
                    }
                    else
                    {
                        // too much content flow
                        ToggleControlVisibility(MessageControl, "BinEnabledLabel", false);
                        ToggleControlVisibility(MessageControl, "PermanentDeleteCheckBox", false);
                        ToggleControlVisibility(MessageControl, "BinNotConfiguredLabel", trashDisabled.Value && trashBin.IsActive);
                        ToggleControlVisibility(MessageControl, "PermanentDeleteLabel", true);
                        ToggleControlVisibility(MessageControl, "TooMuchContentLabel", !trashDisabled.Value && trashBin.IsActive);
                    }
                }
                else
                {
                    MessageControl.Errors.Add("System Message: You can't delete a content which is not derived from GenericContent.");
                }
            }
        }
        
        private void ProcessCommand(CommandEventArgs e)
        {
            switch (e.CommandName.ToLower())
            {
                case "yes":
                    try
                    {
                        var permanentDelete = this.FindControlRecursive("PermanentDeleteCheckBox") as CheckBox;


                        var originalName = ContextNode.Name;
                        var back = PortalContext.Current.BackUrl;
                        var oldUrlName = string.Format("/{0}", originalName);
                        TrashBin trashBin = null;

                        try
                        {
                            trashBin = TrashBin.Instance;
                        }
                        catch (SenseNetSecurityException)
                        {
                            //trashbin is not accessible
                        }

                        var trashIsAvailable = trashBin != null && (trashBin.BagCapacity <= 0 || trashBin.BagCapacity > GetNodesInTreeCount());

                        if ((permanentDelete != null && permanentDelete.Checked) || !trashIsAvailable)
                            DeleteContentsForever();
                        else
                            DeleteContentsToTrash();

                        if (RequestIdList.Count == 0 && back.Contains(oldUrlName))
                        {
                            back = back.Replace(oldUrlName, string.Empty);
                            var p = Page as PageBase;
                            if (p != null) p.Response.Redirect(back, false);
                        }
                        else
                            CallDone(false);
                    } 
                    catch(Exception ex)
                    {
                        Logger.WriteException(ex);
                        ShowException(ex);
                    }
                    break;
                case "ok":
                case "errorok":
                case "no":
                case "cancel":
                    CallDone();
                    break;
                default:
                    break;
            }
        }

        //=========================================================== Helper methods

        /// <summary>
        /// Sets the label Text property based on the formatted text is included in that property.
        /// </summary>
        /// <param name="control">The control which should contain the label with labelId.</param>
        /// <param name="labelId">The id of the label control withing the control.</param>
        /// <param name="name">The name that is displayed in the label text.</param>
        /// <param name="path">The path that is displayed in the label text.</param>
        private static void SetLabel(Control control, string labelId, string name, string path)
        {
            var label = control.FindControlRecursive(labelId) as Label;
            if (label == null)
                return;

            var format = label.Text;
            try
            {
                label.Text = String.Format(format, name, path);
                label.Visible = true;
            }
            catch (ArgumentNullException argumentNullException)
            {
                label.BackColor = System.Drawing.Color.Black;
                label.ForeColor = System.Drawing.Color.Red;
                label.Text = "You have a wrong formatted string in " + label.ID;
                Logger.WriteException(argumentNullException);
            }
            catch (FormatException formatException)
            {
                label.BackColor = System.Drawing.Color.Black;
                label.ForeColor = System.Drawing.Color.Red;
                label.Text = "You have a wrong formatted string in " + label.ID;
                Logger.WriteException(formatException);
            }
        }

        private static void ToggleControlVisibility(Control userControl, string controlId, bool value)
        {
            if (userControl == null || string.IsNullOrEmpty(controlId))
                return;

            var control = userControl.FindControlRecursive(controlId);
            if (control != null)
                control.Visible = value;
        }

        private void ShowException(Exception ex)
        {
            if (MessageControl != null)
            {
                MessageControl.ShowError(ex.Message);
            }
            else
            {
                var errorMessage = new Label {ForeColor = System.Drawing.Color.Red, Text = ex.Message};
                Controls.Add(errorMessage);
            }
        }

        private void DeleteContentsToTrash()
        {
            if (RequestIdList.Count == 0)
            {
                ContextNode.Delete();
            }
            else
            {
                foreach (var node in RequestNodeList)
                {
                    node.Delete();
                }
            }
        }

        private void DeleteContentsForever()
        {
            if (RequestIdList.Count == 0)
            {
                TrashBin.ForceDelete(ContextNode as GenericContent);
            }
            else
            {
                foreach (var node in RequestNodeList)
                {
                    TrashBin.ForceDelete(node as GenericContent);
                }
            }
        }

        private int GetNodesInTreeCount()
        {
            if (RequestIdList.Count == 0)
                return ContextNode.NodesInTree;
            
            return (from node in RequestNodeList
                    select node.NodesInTree).Sum();
        }

        private bool ContentsAreInTrash()
        {
            if (RequestIdList.Count == 0)
                return TrashBin.IsInTrash(ContextNode as GenericContent);

            return (RequestNodeList.Count(n => TrashBin.IsInTrash(n as GenericContent)) > 0);
        }

        private bool ContentsAreTrashable()
        {
            if (RequestIdList.Count == 0)
            {
                var gc = ContextNode as GenericContent;
                return (gc != null && gc.IsTrashable);
            }

            return RequestNodeList.Select(node => node as GenericContent).All(gc => gc != null && gc.IsTrashable);
        }
    }
}
