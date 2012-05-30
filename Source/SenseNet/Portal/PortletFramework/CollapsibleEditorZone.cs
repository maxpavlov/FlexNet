using System;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class CollapsibleEditorZone : EditorZone
    {
        // Members and properties /////////////////////////////////////////////////
        bool _applyError = false;

        public override string InstructionText
        {
            get
            {
                var srm = ContentRepository.i18n.SenseNetResourceManager.Current;
                return srm.GetString("PortletFramework", "EditorZoneInstructionText");
            }
        }
        public override string HeaderText
        {
            get
            {
                var srm = ContentRepository.i18n.SenseNetResourceManager.Current;
                var sformat = srm.GetString("PortletFramework", "EditorZoneHeaderText");
                string result;
                try
                {
                    var typeName = string.Empty;
                    var p = WebPartToEdit as PortletBase;
                    if (p != null)
                        typeName = p.Name.Length > 1 ? p.Name : "";
                    else
                        typeName = WebPartToEdit.Title;
                    result = string.Format(sformat, typeName);
                }
                catch(Exception e) //logged
                {
                    Logger.WriteException(e);
                    result = sformat;
                }
                return result;
            }
            set { base.HeaderText = value; }
        }
        public bool DisableTabScript { get; set; }
        public bool EditorPartsAdded { get; set; }

        // Events /////////////////////////////////////////////////////////////////
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ApplyVerb.Visible = false;
            this.HeaderCloseVerb.Visible = false;
        }
        protected override void RaisePostBackEvent(string eventArgument)
        {
            var wpm = System.Web.UI.WebControls.WebParts.WebPartManager.GetCurrentWebPartManager(this.Page);

            switch (eventArgument)
            {
                case "cancel":
                case "headerClose":
                    if (wpm != null)
                    {
                        wpm.DisplayMode = System.Web.UI.WebControls.WebParts.WebPartManager.EditDisplayMode;
                        wpm.EndWebPartEditing();

                    }
                    break;
                case "ok":
                    this.ApplyAndSyncChanges();
                    if (this._applyError)
                    {
                        SenseNet.Portal.UI.Controls.PortalRemoteControl.ToggleEditorZone(this.WebPartToEdit, this.Page);
                        return;
                    }
                    if (wpm != null)
                    {
                        wpm.DisplayMode = System.Web.UI.WebControls.WebParts.WebPartManager.EditDisplayMode;
                        wpm.EndWebPartEditing();
                    }
                        
                    
                    break;
                default:
                    base.RaisePostBackEvent(eventArgument);
                    break;
            }
        }


        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-dialog-editportlet");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
        }
        protected override void Render(HtmlTextWriter writer)
        {
            this.RenderBeginTag(writer);
            this.RenderContents(writer);
            this.RenderEndTag(writer);
        }
        protected override void RenderBody(HtmlTextWriter writer)
        {
            var hasControls = this.HasControls();
            if (hasControls)
                RenderEditorPartControlsInternal(writer);
            else
                this.RenderEmptyZoneText(writer);
        }

        private void RenderEditorPartControlsInternal(HtmlTextWriter writer)
        {
          
            var editorPartChrome = this.EditorPartChrome;
            foreach(EditorPart editorPart in EditorParts)
            {
                if (editorPart.Display || editorPart.Visible)
                    editorPartChrome.RenderEditorPart(writer, editorPart);
            }            
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this.HasHeader)
                this.RenderHeader(writer);

            RenderBeginContents(writer);
            this.RenderBody(writer);
            RenderEndContents(writer);

            if (this.HasFooter)
                this.RenderFooter(writer);

        }
        protected override void RenderHeader(HtmlTextWriter writer)
        {
//            var headerTemplate = @"  <div class='sn-view-header'>
//                                                <div class='sn-icon-big snIconBigEdit_Portlet'></div>
//                                                <div class='sn-view-header-text'>
//                                                    <h2 class='sn-view-title' title='{2}'>{0}</h2>
//                                                    {1}
//                                                </div>
//                                        </div>";
//            writer.Write(String.Format(headerTemplate, this.HeaderText, this.InstructionText, WebPartToEdit.GetType().Name));
        }
        protected override void RenderFooter(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-dialog-footer");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            base.RenderFooter(writer);
            writer.RenderEndTag();
        }
        protected override EditorPartChrome CreateEditorPartChrome()
        {
            return new CollapsibleEditorPartChrome(this);
        }

        private EditorPart GetEditor(string name)
        {
            var enumerator = this.EditorParts.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var e = enumerator.Current as EditorPart;
                if (e.GetType().Name == name)
                    return e;
            }
            return null;
        }

        protected virtual void RenderEmptyZoneText(HtmlTextWriter writer)
        {
            var emptyText = new LiteralControl(EmptyZoneText);
            emptyText.RenderControl(writer);
        }
        protected virtual void RenderEndContents(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            writer.RenderEndTag();
        }
        protected virtual void RenderBeginContents(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-dialog-main");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-accordion");
            writer.AddAttribute(HtmlTextWriterAttribute.Id,"ptEditAccordion");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        // Internals //////////////////////////////////////////////////////////////
        private void ApplyAndSyncChanges()
        {
            if (this.WebPartToEdit == null)
                return;
            var editorParts = this.EditorParts;

            ApplyChanges(editorParts);

            if (this._applyError)
                return;

            SyncChanges(editorParts);

            PortletNotifyCheckin();
        }
        private void PortletNotifyCheckin()
        {
            var portlet = this.WebPartToEdit as CacheablePortlet;
            if (portlet != null)
                portlet.NotifyCheckin();
        }
        private void SyncChanges(EditorPartCollection editorParts)
        {
            foreach (EditorPart part3 in editorParts)
                part3.SyncChanges();
        }
        private void ApplyChanges(EditorPartCollection editors)
        {
            foreach (EditorPart part2 in editors)
            {
                if (!part2.Display) continue;
                if (!part2.Visible) continue;
                if (part2.ApplyChanges())
                    this._applyError = true;                
            }
        }
        
    }
}