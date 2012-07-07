using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.Portlets
{
    public class PurgePortlet : PortletBase
    {
        //====================================================================== Constructor

        public PurgePortlet()
        {
            this.Name = "Trashbin purge";
            this.Description = "This portlet empties the trash";
            this.Category = new PortletCategory(PortletCategoryType.Portal);
        }

        //====================================================================== Properties

        private string _viewPath = "/Root/System/SystemPlugins/Portlets/Purge/Purge.ascx";

        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ViewPath
        {
            get { return _viewPath; }
            set { _viewPath = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        private MessageControl _msgControl;
        protected MessageControl MessageControl
        {
            get
            {
                if (_msgControl == null && this.Controls.Count > 0)
                {
                    _msgControl = this.Controls[0].FindControl("PurgeMessage") as MessageControl;
                }

                return _msgControl;
            }
        }

        //====================================================================== Methods

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);

            base.OnInit(e);
        }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();

            var c = Page.LoadControl(ViewPath);
            if (c == null)
                return;

            this.Controls.Add(c);

            if (this.MessageControl != null)
            {
                this.MessageControl.ButtonsAction += MessageControl_ButtonsAction;
            }

            ChildControlsCreated = true;
        }

        //====================================================================== Event handlers


        protected void MessageControl_ButtonsAction(object sender, CommandEventArgs e)
        {
            switch (e.CommandName.ToLower())
            {
                case "ok":
                case "yes":
                    try
                    {
                        TrashBin.Purge();
                        CallDone();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                        if (MessageControl != null)
                            MessageControl.ShowError(ex.Message);
                    }
                    break;
                case "cancel":
                case "no":
                case "errorok":
                    CallDone();
                    break;
            }
        }
    }
}
