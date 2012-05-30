using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using Content = System.Web.UI.WebControls.Content;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal.Virtualization;
using System.Text.RegularExpressions;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:CommandButtons runat=server />")]
    public class CommandButtons : UserControl
    {
        /* ====================================================================== Members */
        private bool _isNewContent;
        private string _originalName; //original name for tracking path changes
        private static readonly string CheckInCompulsoryScript = "return SN.Util.CheckComment();";


        /* ====================================================================== Public Properties */
        private string _layoutControlPath = "/Root/System/SystemPlugins/Controls/CommandButtons.ascx";
        [PersistenceMode(PersistenceMode.Attribute)]
        public string LayoutControlPath
        {
            get { return _layoutControlPath; }
            set { _layoutControlPath = value; }
        }

        private string _checkInControlPath = "/Root/Global/contentviews/CheckInDialog.ascx";
        [PersistenceMode(PersistenceMode.Attribute)]
        public string CheckInControlPath
        {
            get { return _checkInControlPath; }
            set { _checkInControlPath = value; }
        }

        [PersistenceMode(PersistenceMode.Attribute)]
        public string HideButtons { get; set; }

        /* ====================================================================== Properties */

        protected string OpenCheckInScript
        {
            get
            {
                //need to add some string resource to the title
                return @"javascript:$('#CheckInDialog').dialog({modal:true, resizable: false, open: function(type,data) { $(this).parent().appendTo(""form""); }, title: '"
                       + HttpContext.GetGlobalResourceObject("Portal", "CheckInPortletTitle")
                       + "' });return false;";
            }
        }

        private Control _layoutControl;
        public Control LayoutControl
        {
            get
            {
                if (this._layoutControl == null)
                    _layoutControl = this.Page.LoadControl(this.LayoutControlPath);

                return _layoutControl;
            }
        }

        private List<string> _hiddenButtons;
        public List<string> HiddenButtons
        {
            get
            {
                if (_hiddenButtons == null)
                {
                    if (HideButtons != null)
                        _hiddenButtons = HideButtons.Split(';', ' ', ',').ToList();
                    else
                        _hiddenButtons = new List<string>();
                }
                return _hiddenButtons;
            }
        }

        private ContentView _contentView;
        public ContentView ContentView
        {
            get { return _contentView ?? (_contentView = GetContentViewForControl(this)); }
        }
        public IButtonControl CheckoutSaveButton
        {
            get
            {
                return this.LayoutControl.FindControl("CheckoutSave") as IButtonControl;
            }
        }
        public IButtonControl SaveButton
        {
            get
            {
                return this.LayoutControl.FindControl("Save") as IButtonControl;
            }
        }
        public IButtonControl SaveCheckinButton
        {
            get
            {
                return this.LayoutControl.FindControl("SaveCheckin") as IButtonControl;
            }
        }
        public IButtonControl CheckoutSaveCheckinButton
        {
            get
            {
                return this.LayoutControl.FindControl("CheckoutSaveCheckin") as IButtonControl;
            }
        }
        public IButtonControl PublishButton
        {
            get
            {
                return this.LayoutControl.FindControl("Publish") as IButtonControl;
            }
        }
        public IButtonControl CancelButton
        {
            get
            {
                return this.LayoutControl.FindControl("Cancel") as IButtonControl;
            }
        }


        /* ====================================================================== Methods */
        protected override void CreateChildControls()
        {
            if (this.LayoutControl != null)
            {
                this.Controls.Add(this.LayoutControl);
                AssociateEventHandlers();
                SetButtonVisibility();

                var cicm = this.ContentView.Content.CheckInCommentsMode;

                //if checkin popup is needed
                if (cicm > CheckInCommentsMode.None)
                {
                    var checkinButton1 = this.SaveCheckinButton as Button;
                    var checkinButton2 = this.CheckoutSaveCheckinButton as Button;

                    if (this.ID.CompareTo("CheckInCommandButtons") == 0)
                    {
                        //this is the command buttons control 
                        //on the checkin dialog
                        if (cicm == CheckInCommentsMode.Compulsory)
                        {
                            //this.ContentView.Content.Fields["CheckInComments"].FieldSetting.Compulsory = true;

                            //checkin comment is compulsory, add client side validation script
                            if (checkinButton1 != null)
                                checkinButton1.OnClientClick = CheckInCompulsoryScript;

                            if (checkinButton2 != null)
                                checkinButton2.OnClientClick = CheckInCompulsoryScript;
                        }

                        //switch disable mechanism off
                        if (checkinButton1 != null)
                            checkinButton1.CssClass += " sn-notdisabled";

                        if (checkinButton2 != null)
                            checkinButton2.CssClass += " sn-notdisabled";
                    }
                    else if (string.IsNullOrEmpty(PortalContext.Current.ActionName) || PortalContext.Current.ActionName.ToLower().CompareTo("checkin") != 0)
                    {
                        //this is not _that_ command button on the checkin page: clear checkin 
                        //comments for this version and add the checkin dialog
                        this.ContentView.Content["CheckInComments"] = string.Empty;
                        this.Controls.Add(Page.LoadControl(this.CheckInControlPath));

                        if (checkinButton1 != null)
                        {
                            checkinButton1.OnClientClick = OpenCheckInScript;
                            checkinButton1.CssClass += " sn-notdisabled";
                        }

                        if (checkinButton2 != null)
                        {
                            checkinButton2.OnClientClick = OpenCheckInScript;
                            checkinButton2.CssClass += " sn-notdisabled";
                        }
                    }
                }
            }

            base.CreateChildControls();
        }
        protected virtual void AssociateEventHandlers()
        {
            if (this.CheckoutSaveButton != null)
                this.CheckoutSaveButton.Click += new EventHandler(CheckoutSaveButton_Click);
            if (this.SaveCheckinButton != null)
                this.SaveCheckinButton.Click += new EventHandler(SaveCheckinButton_Click);
            if (this.CheckoutSaveCheckinButton != null)
                this.CheckoutSaveCheckinButton.Click += new EventHandler(CheckoutSaveCheckinButton_Click);
            if (this.PublishButton != null)
                this.PublishButton.Click += new EventHandler(PublishButton_Click);
            if (this.CancelButton != null)
                this.CancelButton.Click += new EventHandler(CancelButton_Click);
            if (this.SaveButton != null)
                this.SaveButton.Click +=new EventHandler(SaveButton_Click);
        }
        protected virtual void SetButtonVisibility()
        {
            HideAllButtons();

            if (this.ContentView == null)
                return;

            SetButtonVisible(this.CancelButton, true);

            var contentType = this.ContentView.ContentHandler as ContentType;
            if (contentType != null)
            {
                if (SecurityHandler.HasPermission(contentType, PermissionType.Save))
                    SetButtonVisible(this.CheckoutSaveCheckinButton, true);

                return;
            }

            var genericContent = this.ContentView.ContentHandler as GenericContent;
            if (genericContent == null)
                return;

            switch (genericContent.Version.Status)
            {
                case VersionStatus.Approved:
                    if (SavingAction.HasCheckOut(genericContent))
                        SetButtonVisible(this.CheckoutSaveButton, true);
                    if (SavingAction.HasSave(genericContent))
                        SetButtonVisible(this.CheckoutSaveCheckinButton, true);
                    break;
                case VersionStatus.Locked:
                    if (SavingAction.HasSave(genericContent))
                        SetButtonVisible(this.SaveButton, true);
                    if (SavingAction.HasCheckIn(genericContent))
                        SetButtonVisible(this.SaveCheckinButton, true);
                    break;
                case VersionStatus.Draft:
                    if (genericContent.Id == 0)
                    {
                        if (SavingAction.HasSave(genericContent))
                            SetButtonVisible(this.SaveButton, true);
                    }
                    else
                    {
                        if (SavingAction.HasCheckOut(genericContent))
                            SetButtonVisible(this.CheckoutSaveButton, true);
                    }

                    if (SavingAction.HasPublish(genericContent))
                        SetButtonVisible(this.PublishButton, true);
                    
                    break;
                case VersionStatus.Pending:
                    if (genericContent.Id == 0)
                    {
                        if (SavingAction.HasSave(genericContent))
                            SetButtonVisible(this.SaveButton, true);
                    }
                    else
                    {
                        if (SavingAction.HasCheckOut(genericContent))
                            SetButtonVisible(this.CheckoutSaveButton, true);
                    }
                    break;
                case VersionStatus.Rejected:
                    if (SavingAction.HasCheckOut(genericContent))
                        SetButtonVisible(this.CheckoutSaveButton, true);
                    break;
            }
        }
        protected virtual void DoCheckoutSave()
        {
            DoAction(c => c.CheckOut());
        }
        protected virtual void DoSave()
        {
            DoAction(c => c.Save());
        }
        protected virtual void DoSaveCheckin()
        {
            DoAction(c => c.CheckIn());
        }
        protected virtual void DoCheckoutSaveCheckin()
        {
            DoAction(c => c.Save());
        }
        protected virtual void DoPublish()
        {
            DoAction(c => c.Publish());
        }
        protected virtual void DoCancel()
        {
            //store the 'is new' info before calling page finish
            if (this.ContentView != null && this.ContentView.Content != null)
            {
                _isNewContent = this.ContentView.Content.IsNew;
            }

            FinishPage();
        }


        /* ====================================================================== Event Handlers */
        public void CheckoutSaveButton_Click(object sender, EventArgs e)
        {
            bool cancelled;
            this.ContentView.OnCommandButtonsAction(sender, CommandButtonType.CheckoutSave, out cancelled);
            if (!cancelled)
                DoCheckoutSave();
        }
        public void SaveButton_Click(object sender, EventArgs e)
        {
            bool cancelled;
            this.ContentView.OnCommandButtonsAction(sender, CommandButtonType.Save, out cancelled);
            if (!cancelled)
                DoSave();
        }
        public void SaveCheckinButton_Click(object sender, EventArgs e)
        {
            bool cancelled;
            this.ContentView.OnCommandButtonsAction(sender, CommandButtonType.SaveCheckin, out cancelled);
            if (!cancelled)
                DoSaveCheckin();
        }
        public void CheckoutSaveCheckinButton_Click(object sender, EventArgs e)
        {
            bool cancelled;
            this.ContentView.OnCommandButtonsAction(sender, CommandButtonType.CheckoutSaveCheckin, out cancelled);
            if (!cancelled)
                DoCheckoutSaveCheckin();
        }
        public void PublishButton_Click(object sender, EventArgs e)
        {
            bool cancelled;
            this.ContentView.OnCommandButtonsAction(sender, CommandButtonType.Publish, out cancelled);
            if (!cancelled)
                DoPublish();
        }
        public void CancelButton_Click(object sender, EventArgs e)
        {
            bool cancelled;
            this.ContentView.OnCommandButtonsAction(sender, CommandButtonType.Cancel, out cancelled);
            if (!cancelled)
                DoCancel();
        }


        /* ====================================================================== Helper Methods */
        protected virtual void FinishPage()
        {
            var pageBase = this.Page as PageBase;
            if (pageBase == null)
                return;

            var back = PortalContext.Current.BackUrl;
            var oldUrlName = string.Format("/{0}", _originalName);
            var newUrlName = string.Format("/{0}", this.ContentView.Content.Name);

            //if the user invoked the Edit action from the content itself,
            //we should redirect the response to a backurl containing the new name
            if (!_isNewContent && !string.IsNullOrEmpty(_originalName) && _originalName.CompareTo(this.ContentView.Content.Name) != 0)
            {
                if (back.EndsWith(oldUrlName))
                {
                    var oldIndex = back.LastIndexOf(oldUrlName);
                    back = back.Remove(oldIndex) + newUrlName;
                }
                else if (back.Contains(string.Concat(oldUrlName, "?")))
                {                                   
                    var paramsIndex = back.IndexOf("?");
                    var parameters = back.Substring(paramsIndex);
                    back = back.Remove(paramsIndex).Remove(back.LastIndexOf(oldUrlName)) + newUrlName + parameters;
                }

                if (back != null && !string.IsNullOrEmpty(back.Trim()))
                    pageBase.Response.Redirect(back);
            }

            var newNode = _isNewContent && this.ContentView != null && this.ContentView.Content != null
                              ? this.ContentView.Content.ContentHandler
                              : null;

            pageBase.Done(newNode);
        }
        protected virtual void SetButtonVisible(IButtonControl button, bool visible)
        {
            var control = button as Control;
            if (control != null)
            {
                if (HiddenButtons.Contains(control.ID))
                    control.Visible = false;
                else
                    control.Visible = visible;
            }
        }
        protected virtual void HideAllButtons()
        {
            SetButtonVisible(this.CheckoutSaveButton, false);
            SetButtonVisible(this.CheckoutSaveCheckinButton, false);
            SetButtonVisible(this.SaveButton, false);
            SetButtonVisible(this.SaveCheckinButton, false);
            SetButtonVisible(this.PublishButton, false);
            SetButtonVisible(this.CancelButton, false);
        }
        protected virtual ContentView GetContentViewForControl(Control control)
        {
            var cv = control.Parent as ContentView;
            if (cv != null)
                return cv;

            return control.Parent == null ? null : GetContentViewForControl(control.Parent);
        }



        protected virtual void DoAction(Action<SenseNet.ContentRepository.Content> contentAction)
        {
            DoAction(true, true, contentAction);
        }
        protected virtual void DoAction(bool updateContent, bool finishPage, Action<SenseNet.ContentRepository.Content> contentAction)
        {
            if (this.ContentView == null)
                return;

            //store the 'is new' info before calling page finish
            _isNewContent = this.ContentView.Content.IsNew;
            _originalName = this.ContentView.Content.Name;

            if (updateContent)
            {
                this.ContentView.NeedToValidate = true;
                this.ContentView.UpdateContent();
            }

            if (this.ContentView.IsUserInputValid)
            {
                try
                {
                    contentAction(this.ContentView.Content);
                }
                catch (Exception ex)
                {
                    this.ContentView.ContentException = ex;
                }
                SetButtonVisibility();
            }

            if (this.ContentView.ContentException == null && this.ContentView.IsUserInputValid)
            {
                if (finishPage)
                    FinishPage();
            }
        }
    }
}
