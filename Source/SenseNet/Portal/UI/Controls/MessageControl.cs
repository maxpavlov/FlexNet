using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace SenseNet.Portal.UI.Controls
{
    public enum MessageControlButtons
    {
        Ok,
        OkCancel,
        YesNo
    }
    public class MessageControl : CompositeControl
    {
        // Members ///////////////////////////////////////////////////////
        private static readonly string OkButtonId = "OkBtn";
        private static readonly string YesButtonId = "YesBtn";
        private static readonly string NoButtonId = "NoBtn";
        private static readonly string CancelButtonId = "CancelBtn";

        private PlaceHolder _headerHolder;
        private PlaceHolder _messageHolder;
        private PlaceHolder _footerHolder;
        private PlaceHolder _controlHolder;
        private PlaceHolder _confirmationHolder;
        private List<string> _errorMessageList;
        private bool _showErrror;
        private string _errorMessage;

        // Properties ////////////////////////////////////////////////////é
        [PersistenceMode(PersistenceMode.Attribute)]
        public MessageControlButtons Buttons { get; set; }
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate MessageTemplate { get; set; }
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate FooterTemplate { get; set; }
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate ControlTemplate { get; set; }
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate HeaderTemplate { get; set; }
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate ConfirmationTemplate { get; set; } 
   
        public List<string> Errors
        {
            get
            {
                if (_errorMessageList == null)
                    _errorMessageList = new List<string>();
                return _errorMessageList;
            }
        }
        protected bool HasError
        {
            get { return Errors.Count() != 0; }
        }

        public void ShowError(string message)
        {
            Errors.Add(message);
            _showErrror = true;
            _errorMessage = message;

            var button = this.FindControlRecursive(OkButtonId) as Button;
            if (button != null)
            {
                button.Visible = true;
                button.CommandName = "ErrorOk";
            }
                

            button = this.FindControlRecursive(YesButtonId) as Button;
            if (button != null) button.Visible = false;
            button = this.FindControlRecursive(NoButtonId) as Button;
            if (button != null) button.Visible = false;
            button = this.FindControlRecursive(CancelButtonId) as Button;
            if (button != null) button.Visible = false;
            if  (_controlHolder != null)
                _controlHolder.Visible = false;
                
            if (_confirmationHolder != null)
                _confirmationHolder.Visible = true;

                        
            var messageControl = this.FindControlRecursive("DialogMessage") as ITextControl;
            if (messageControl != null) messageControl.Text = _errorMessage;

            var messagePanel = this.FindControlRecursive("DialogMessagePanel");
            if (messagePanel != null) messagePanel.Visible = true;

            var rusLabel = this.FindControlRecursive("RusLabel");
            if (rusLabel != null) rusLabel.Visible = false;


        }
        
        // Events ////////////////////////////////////////////////////////
        private static readonly object ButtonsActionEventKey = new object();
        public event CommandEventHandler ButtonsAction
        {
            add { Events.AddHandler(ButtonsActionEventKey, value); }
            remove { Events.AddHandler(ButtonsActionEventKey, value); }
        }
        protected virtual void OnButtonAction(object sender, CommandEventArgs e)
        {
            var handler = Events[ButtonsActionEventKey] as CommandEventHandler;
            if (handler != null)
                handler(this, e);
        }
        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
            base.OnInit(e);
        }
        protected override void CreateChildControls()
        {
            InstatiateTemplates();
            base.CreateChildControls();
        }
        protected override void OnPreRender(EventArgs e)
        {
            if (HasError)
            {
                var errorMessage = this.FindControlRecursive("ErrorMessage") as Label;
                if (errorMessage != null)
                {
                    var errors = new StringBuilder();
                    foreach (var message in _errorMessageList)
                    {
                        errors.Append(message);
                        errors.Append("<br />");
                    }
                    errorMessage.Visible = true;
                    errorMessage.Text = errors.ToString();
                }
            }
            base.OnPreRender(e);
        }
        
        // Internals /////////////////////////////////////////////////////
        private void InstatiateTemplates()
        {
            ChildControlsCreated = true;


            var template = HeaderTemplate;
            if (template != null)
            {
                _headerHolder = new PlaceHolder();
                template.InstantiateIn(_headerHolder);
                Controls.Add(_headerHolder);
            }

            template = ControlTemplate;
            if (template != null)
            {
                _controlHolder = new PlaceHolder();
                template.InstantiateIn(_controlHolder);
                Controls.Add(_controlHolder);
            }
            template = ConfirmationTemplate;
            if (template != null)
            {
                _confirmationHolder = new PlaceHolder();
                template.InstantiateIn(_confirmationHolder);
                Controls.Add(_confirmationHolder);
            }

            template = MessageTemplate;
            if (template != null)
            {
                _messageHolder = new PlaceHolder();
                template.InstantiateIn(_messageHolder);
                Controls.Add(_messageHolder);
            }

            template = FooterTemplate;
            if (template != null)
            {
                _footerHolder = new PlaceHolder();
                template.InstantiateIn(_footerHolder);
                Controls.Add(_footerHolder);
                
                BindEvents();
            }
            else
            {
                _footerHolder = new PlaceHolder();
                CreateButtons(_footerHolder);
                Controls.Add(_footerHolder);
                BindEvents();
            }
        }
        private void CreateButtons(Control holder)
        {
            if (holder == null) 
                throw new ArgumentNullException("holder");
            Button okButton = null;
            Button cancelButton = null;
            Button yesButton = null;
            Button noButton = null;

            okButton = new Button { ID = OkButtonId };
            okButton.Text = "OK";
            okButton.CommandName = HasError ? "ErrorOk" : "OK";
            holder.Controls.Add(okButton);
            
            cancelButton = new Button { ID = CancelButtonId };
            cancelButton.Text = "Cancel";
            cancelButton.CommandName = "Cancel";
            holder.Controls.Add(cancelButton);

            yesButton = new Button { ID = YesButtonId };
            yesButton.Text = "Yes";
            yesButton.CommandName = "Yes";
            holder.Controls.Add(yesButton);
            
            noButton = new Button { ID = NoButtonId };
            noButton.Text = "No";
            noButton.CommandName = "No";
            holder.Controls.Add(noButton);
            
            switch (Buttons)
            {
                case MessageControlButtons.Ok:
                    okButton.Visible = true;
                    cancelButton.Visible = false;
                    yesButton.Visible = false;
                    noButton.Visible = false;
                    break;
                case MessageControlButtons.OkCancel:
                    okButton.Visible = true;
                    cancelButton.Visible = true;
                    yesButton.Visible = false;
                    noButton.Visible = false;
                    break;
                case MessageControlButtons.YesNo:
                    okButton.Visible = false;
                    cancelButton.Visible = false;
                    yesButton.Visible = true;
                    noButton.Visible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void BindEvents()
        {
            BindEvent(OkButtonId);
            BindEvent(YesButtonId);
            BindEvent(NoButtonId);
            BindEvent(CancelButtonId);
        }
        private void BindEvent(string buttonId)
        {
            if (String.IsNullOrEmpty(buttonId)) 
                throw new ArgumentException("buttonId");
            var buttonControl = this.FindControlRecursive(buttonId) as IButtonControl;
            if (buttonControl != null)
                buttonControl.Command += OnButtonAction;
        }
    }
}
