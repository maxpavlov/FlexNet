using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal.Portlets.ContentHandlers;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SNC = SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using Content = SenseNet.ContentRepository.Content;

namespace SenseNet.Portal.Portlets
{
	public class FormPortlet : PortletBase
	{
		//-- Variables ---------------------------------------------------

		string _formPath;
		string _contentViewPath;
        string _afterSubmitViewPath;
        string _permissionErrorViewPath;
		Form _currentForm;
		SNC.Content _cFormItem;
		protected ContentView _cvFormItem;

		//-- Properties ---------------------------------------------------

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Form path")]
        [WebDescription("Path of the form to be displayed")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions()]
        [WebOrder(100)]
        public string FormPath
		{
			get { return _formPath; }
			set { _formPath = value; }
		}

		private Form CurrentForm
		{
			get
			{
				if (_currentForm != null && _currentForm.Path == _formPath)
					return _currentForm;
				if (!string.IsNullOrEmpty(_formPath))
					_currentForm = Node.LoadNode(_formPath) as Form;
				return _currentForm;
			}
		}

		[WebBrowsable(true)]
        [Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView, PortletViewType.Ascx)]
        [WebOrder(110)]
        public string ContentViewPath
		{
			get { return _contentViewPath; }
			set { _contentViewPath = value; }
		}

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("After submit view path")]
        [WebDescription("Path of the content view which provides the UI elements for the 'After submit' dialog")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
        [WebOrder(120)]
        public string AfterSubmitViewPath
        {
            get { return _afterSubmitViewPath; }
            set { _afterSubmitViewPath = value; }
        }

	    #region from changeset #19024

	    [WebBrowsable(true), Personalizable(true)]
	    [WebDisplayName("Permission error view path")]
	    [WebDescription("Content view path")]
	    [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
	    [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
	    [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
	    [WebOrder(120)]
	    public string PermissionErrorViewPath
	    {
	        get { return _permissionErrorViewPath; }
	        set { _permissionErrorViewPath = value; }
	    }

	    #endregion


        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        private bool isContentValid;
	    private int _formItemID;
        
        //-- Constructors ------------------------------------------------
        public FormPortlet()
        {
            this.Name = "Form";
            this.Description = "Submit new items to a form list with this porlet";
            this.Category = new PortletCategory(PortletCategoryType.Application);
            isContentValid = false;

            this.HiddenProperties.Add("Renderer");
        }
		//-- Initialize --------------------------------------------------
        //protected override void Initialize()
        //{
        //    base.Initialize();
        //    Controls.Clear();
        //    CreateControls();
        //}

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.RegisterRequiresControlState(this);
        }

        protected override void LoadControlState(object savedState)
        {
            object[] data = savedState as object[];
            if (data == null) base.LoadControlState(savedState);
            else if (data.Length != 3) base.LoadControlState(data);
            else
            {
                isContentValid = Convert.ToBoolean(data[0]);
                _formItemID = Convert.ToInt32(data[1]);
                base.LoadControlState(data[2]);
            }
        }
        protected override object SaveControlState()
        {
            object[] data = new object[3];
            data[0] = isContentValid;
            data[1] = _formItemID;
            data[2] = base.SaveControlState();
            return data;
        }


        protected override void CreateChildControls()
        {
            Controls.Clear();

            if (Page.IsPostBack && isContentValid)
            {
                BuildAfterSubmitForm(null);
            }
            else
            {
                CreateControls();
            }


            ChildControlsCreated = true;

        }

		private void CreateControls()
		{
            if (CurrentForm != null)
            {


                if (!CurrentForm.Security.HasPermission(PermissionType.AddNew))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(PermissionErrorViewPath))
                        {
                            _cvFormItem = ContentView.Create(SNC.Content.Create(CurrentForm), this.Page, ViewMode.Browse, PermissionErrorViewPath);
                            this.Controls.Add(_cvFormItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                        this.Controls.Clear();
                        this.Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
                    }
                }
                else
                {
                    CreateNewFormItem();
                }
            }
		}

		//-- Methods -----------------------------------------------------

		private void CreateNewFormItem()
		{
			if (CurrentForm == null)
				return;

		    var act = CurrentForm.GetAllowedChildTypes();
            var allowedType = act.FirstOrDefault(ct => ct.IsInstaceOfOrDerivedFrom("FormItem"));
		    var typeName = allowedType == null ? "FormItem" : allowedType.Name;

            _cFormItem = SNC.Content.CreateNew(typeName, CurrentForm, null);

		    try
		    {
                if (string.IsNullOrEmpty(ContentViewPath))
                    _cvFormItem = ContentView.Create(_cFormItem, this.Page, ViewMode.New);
                else
                    _cvFormItem = ContentView.Create(_cFormItem, this.Page, ViewMode.New, ContentViewPath);

                _cvFormItem.ID = "cvNewFormItem";
                _cvFormItem.Init += new EventHandler(_cvFormItem_Init);
                _cvFormItem.UserAction += new EventHandler<UserActionEventArgs>(_cvFormItem_UserAction);

                this.Controls.Add(_cvFormItem);
		    }
		    catch (Exception ex) //logged
		    {
                Logger.WriteException(ex);
		        this.Controls.Clear();
                this.Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
		    }
			
		}

		void _cvFormItem_UserAction(object sender, UserActionEventArgs e)
		{
			switch (e.ActionName)
			{
				case "save":
					e.ContentView.UpdateContent();
                    isContentValid = e.ContentView.Content.IsValid && e.ContentView.IsUserInputValid;
					if (e.ContentView.IsUserInputValid && e.ContentView.Content.IsValid)
					{
                        try
                        {
                            e.ContentView.Content.Save();
                            //if(_cvFormItem != null)
                            //	_cvFormItem.Controls.Clear();
                            Controls.Clear();

                            _formItemID = e.ContentView.Content.Id;

                            BuildAfterSubmitForm(e.ContentView.Content);
                        }
                        catch (FormatException ex) //logged
                        {
                            Logger.WriteException(ex);
                            e.ContentView.ContentException = new FormatException("Invalid format!", ex);
                        }
                        catch (Exception exc) //logged
                        {
                            Logger.WriteException(exc);
                            e.ContentView.ContentException = exc;
                        }
					}
					break;
			}
		}

        private void BuildAfterSubmitForm(SNC.Content formitem)
	    {
            if (!string.IsNullOrEmpty(AfterSubmitViewPath))
            {
                BuildAfterSubmitForm_ContentView(formitem);
            }
            else
            {
                Controls.Add(new LiteralControl(string.Concat("<div class=\"sn-form-submittext\">", CurrentForm.AfterSubmitText, "</div>")));
                Button ok = new Button();
                ok.ID = "btnOk";
                ok.Text = "Ok";
                ok.CssClass = "sn-submit";
                ok.Click += new EventHandler(ok_Click);
                Controls.Add(ok);
            }
	    }

        private void BuildAfterSubmitForm_ContentView(SNC.Content formitem)
	    {
            if (CurrentForm == null)
                return;

            //FormItem fi = new FormItem(CurrentForm);
            //_cFormItem = SNC.Content.Create(fi);
            if (formitem == null && _formItemID != 0)
            {
                formitem = Content.Load(_formItemID);
            }

            if (formitem != null)
            {
                _cFormItem = formitem;

                _cvFormItem = ContentView.Create(_cFormItem, this.Page, ViewMode.New, AfterSubmitViewPath);

                _cvFormItem.ID = "cvAfterSubmitFormItem";
                //_cvFormItem.Init += new EventHandler(_cvFormItem_Init);
                _cvFormItem.UserAction += new EventHandler<UserActionEventArgs>(_cvAfterSubmitFormItem_UserAction);

                this.Controls.Add(_cvFormItem);
            }
            else if (!string.IsNullOrEmpty(AfterSubmitViewPath))
            {
                this.Controls.Add(Page.LoadControl(AfterSubmitViewPath));
            }
	    }

        void _cvAfterSubmitFormItem_UserAction(object sender, UserActionEventArgs e)
        {
            switch (e.ActionName)
            {
                case "ok":
                    CreateControls();
                    break;
            }
        }
	    void ok_Click(object sender, EventArgs e)
		{
	        isContentValid = false;
			Controls.Clear();
	        CreateControls();

            CallDone();
		}

		void _cvFormItem_Init(object sender, EventArgs e)
		{
			Button btnSave = (sender as ContentView).FindControl("btnSave") as Button;

			if (btnSave != null)
			{
				btnSave.Text = "Send";
				btnSave.Focus();
			}
		}

		//-- Events ------------------------------------------------------



		//-- Helper ------------------------------------------------------



	}
}
