using System;
using System.ComponentModel;
using System.Web.UI;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.UI;
using SenseNet.ContentRepository;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Portlets
{
    public class ContentEditorPortlet : ContextBoundPortlet
    {
        [WebBrowsable(true)]
        [Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
        public string ContentViewPath { get; set; }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        public ContentEditorPortlet()
        {
            this.Name = "Editor";
            this.Description = "This portlet edits the content with its edit contentview (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }

        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            Controls.Clear();

            var node = GetContextNode();

            if (node == null)
            {
                if (this.RenderException != null)
                {
                    Controls.Clear();
                    Controls.Add(new System.Web.UI.WebControls.Label() { Text = string.Format("Error loading content view: {0}", this.RenderException.Message) });
                } else
                {
                    Controls.Clear();
                    Controls.Add(new System.Web.UI.WebControls.Label() { Text = "Content could not be loaded" });
                }
                ChildControlsCreated = true;
                return;
            }

            var content = Content.Create(node);

            var contentView = String.IsNullOrEmpty(ContentViewPath) ?
                ContentView.Create(content, Page, ViewMode.InlineEdit) :
                ContentView.Create(content, Page, ViewMode.InlineEdit, ContentViewPath);

            //workaround for the hack below: if cannot change the RenderMode of a 
            //field control here, than set the default value for the whole content view
            //(we want to use the InlineView for the view, but Edit behavior for the controls...)
            //contentView.DefaultControlRenderMode = FieldControlRenderMode.Edit;

            contentView.CommandButtonsAction += new EventHandler<CommandButtonsEventArgs>(contentView_CommandButtonsAction);

            // backward compatibility: use eventhandler for contentviews using defaultbuttons and not commandbuttons
            contentView.UserAction += new EventHandler<UserActionEventArgs>(contentView_UserAction);

            try
            {
                Controls.Add(contentView);
            }
            catch(Exception ex)
            {
                Logger.WriteException(ex);

                this.Controls.Clear();

                var message = ex.Message.Contains("does not contain Field")
                                  ? string.Format("Content and view mismatch: {0}", ex.Message)
                                  : string.Format("Error: {0}", ex.Message);

                Controls.Add(new LiteralControl(message));
            }

            ChildControlsCreated = true;
        }

        protected void contentView_CommandButtonsAction(object sender, CommandButtonsEventArgs e)
        {
            this.OnCommandButtons(e);
        }

        protected virtual void OnCommandButtons(CommandButtonsEventArgs e)
        {
        }

        protected override object GetModel()
        {
            var node = GetContextNode();
            return Content.Create(node).GetXml();
        }

        #region backward compatibility
        void contentView_UserAction(object sender, UserActionEventArgs e)
        {
            var contentView = e.ContentView;
            var content = contentView.Content;

            switch (e.ActionName)
            {
                case "save":
                    OnSave(contentView, content);

                    if (!contentView.IsUserInputValid || !content.IsValid || contentView.ContentException != null)
                        return;

                    break;
            }

            CallDone();
        }

        protected virtual void OnSave(ContentView contentView, Content content)
        {
            contentView.UpdateContent();
            if (contentView.IsUserInputValid && content.IsValid)
            {
                try
                {
                    content.Save();
                }
                catch (Exception ex) //logged
                {
                    Logger.WriteException(ex);
                    contentView.ContentException = ex;
                }
            }
        }
        #endregion
    }
}
