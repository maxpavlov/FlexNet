using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;


namespace SenseNet.Portal.UI.Controls
{
    public enum DefaultButtonType
    {
        None = 0,
        CheckIn = 1,
        CheckOut = 2,
        UndoCheckOut = 3,
        Save = 4,
        Publish = 5,
        Approve = 6,
        Reject = 7,
        Cancel = 8,
        ForceUndoCheckOut = 9,
        Custom = 10
    }
    [ToolboxData("<{0}:DefaultButtons runat=server />")]
    public class DefaultButtons : CompositeControl
    {
        // Members /////////////////////////////////////////////////////
        private readonly DefaultButtonsFactory ButtonsFactory = InitFactory();
        private static DefaultButtonsFactory InitFactory()
        {
            return new DefaultButtonsFactory();
        }

        internal ContentView ParentContentView
        {
            get
            {
                if (this.Parent != null && this.Parent is ContentView)
                    return this.Parent as ContentView;
                return null;
            }
        }
        internal Node CurrentNode
        {
            get
            {
                return this.ParentContentView != null ? ParentContentView.Content.ContentHandler : null;
            }
        }
        
        public GenericContent CurrentGC
        {
            get
            {
                return CurrentNode as GenericContent;
            }
        }

        public virtual bool NewContent
        {
            get
            {
                if (CurrentGC == null)
                    return CurrentNode != null && CurrentNode.Id < 1;
                return CurrentGC.Id < 1;
            }
        }

        private bool _visibleCheckIn = true;
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool VisibleCheckIn
        {
            get { return _visibleCheckIn; }
            set { _visibleCheckIn = value; }
        }

        private bool _visibleCheckOut = true;
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool VisibleCheckOut
        {
            get { return _visibleCheckOut; }
            set { _visibleCheckOut = value; }
        }

        private bool _visibleUndoCheckOut = true;
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool VisibleUndoCheckOut
        {
            get { return _visibleUndoCheckOut ; }
            set { _visibleUndoCheckOut  = value; }
        }

        private bool _visibleSave = true;
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool VisibleSave
        {
            get { return _visibleSave; }
            set { _visibleSave = value; }
        }

        private bool _visiblePublish = true; 
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool VisiblePublish
        {
            get { return _visiblePublish; }
            set { _visiblePublish = value; }
        }

        private bool _visibleApprove = true;
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool VisibleApprove
        {
            get { return _visibleApprove; }
            set { _visibleApprove = value; }
        }

        private bool _visibleReject = true;
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool VisibleReject
        {
            get { return _visibleReject; }
            set { _visibleReject = value; }
        }

        private bool _visibleCancel = true;
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool VisibleCancel
        {
            get { return _visibleCancel; }
            set { _visibleCancel = value; }
        }

        private bool _visibleForceUndoCheckOut = true;
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool VisibleForceUndoCheckOut
        {
            get { return _visibleForceUndoCheckOut; }
            set { _visibleForceUndoCheckOut = value; }
        }
        
        [PersistenceMode(PersistenceMode.Attribute)]
        public string ButtonCssClass { get; set; }

        // Events //////////////////////////////////////////////////////
        private static readonly object ClickButtonEventKey = new object();
        public event EventHandler ButtonClick
        {
            add { Events.AddHandler(ClickButtonEventKey, value); }
            remove { Events.RemoveHandler(ClickButtonEventKey, value); }
        }

        protected override void OnInit(EventArgs e)
        {
            ButtonsFactory.CurrentControl = this;
        }
        protected override void CreateChildControls()
        {
            Controls.Clear();
            if (NewContent)
                CreateButton(DefaultButtonType.Save);
            else
            {
                CreateButton(DefaultButtonType.CheckIn);
                CreateButton(DefaultButtonType.CheckOut);
                CreateButton(DefaultButtonType.UndoCheckOut);
                CreateButton(DefaultButtonType.Publish);
                CreateButton(DefaultButtonType.Approve);
                CreateButton(DefaultButtonType.Reject);
                CreateButton(DefaultButtonType.ForceUndoCheckOut);
                CreateButton(DefaultButtonType.Save);
                //CreateButton(DefaultButtonType.Cancel);
            }
            CreateButton(DefaultButtonType.Cancel);
            ChildControlsCreated = true;
        }
        protected void ButtonClickHandler(object sender, EventArgs e)
        {
            OnButtonClickHandler(sender, e);
        }
        protected virtual void OnButtonClickHandler(object sender, EventArgs e)
        {
            var button = sender as IButtonControl;
            var buttonName= button == null ? "??" : button.CommandName;
            using (var traceOperation = Logger.TraceOperation("DefaultButtons.OnButtonClickHandler: " + buttonName))
            {
                if (button != null)
                    ParentContentView.OnUserAction(sender, button.CommandName, "Click");

                var handler = base.Events[ClickButtonEventKey] as EventHandler;
                if (handler != null)
                    handler(this, e);

                traceOperation.IsSuccessful = true;
            }
        }
        
        protected virtual void CreateButton(DefaultButtonType actionButton)
        {
            var button = ButtonsFactory.CreateActionButtons(actionButton);
            if (button == null) return;
            if (!string.IsNullOrEmpty(ButtonCssClass))
                ((Button) button).CssClass = ButtonCssClass;
            ((Button)button).Click += ButtonClickHandler;
            Controls.Add(button);
        }
    
    }
}