//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web.UI;
//using System.Web.UI.HtmlControls;
//using System.Web.UI.WebControls;
//using SenseNet.Portal.Virtualization;

//namespace SenseNet.Portal.UI.Controls
//{
//    [ToolboxData("<{0}:QueryEditor ID=\"QueryEditor1\" runat=server></{0}:QueryEditor>")]
//    public class QueryEditor : FieldControl, INamingContainer, ITemplateFieldControl
//    {

//        private readonly string ObjectTag = @"
//            <div id=""silverlightControlHost"" style=""height: 100%; width: 100%"">
//		        <object data=""data:application/x-silverlight,"" type=""application/x-silverlight-2"" width=""{2}"" height=""{3}"">
//                    <param name=""InitParams"" value=""ControlID={0},ContextPath={1}"" />
//			        <param name=""source"" value=""/ClientBin/QueryEditor.xap""/>
//			        <param name=""background"" value=""white"" />
//			        <param name=""minRuntimeVersion"" value=""3.0.40818.0"" />
//			        <param name=""autoUpgrade"" value=""true"" />
//			        <a href=""http://go.microsoft.com/fwlink/?LinkID=149156&v=3.0.40818.0"" style=""text-decoration: none;"">
//     			        <img src=""http://go.microsoft.com/fwlink/?LinkId=108181"" alt=""Get Microsoft Silverlight"" style=""border-style: none""/>
//			        </a>
//		        </object>
//                <iframe id='_sl_historyFrame' style='visibility:hidden;height:0;width:0;border:0px'></iframe>
//            </div>";
//        private readonly TextBox queryTextBox;

//        // Properties ///////////////////////////////////////////////////////////////////
//        [PersistenceMode(PersistenceMode.Attribute)]
//        public int MaxLength { get; set; }
//        [PersistenceMode(PersistenceMode.Attribute)]
//        public int Rows { get; set; }
//        [PersistenceMode(PersistenceMode.Attribute)]
//        public int Columns { get; set; }
//        [PersistenceMode(PersistenceMode.Attribute)]
//        public bool QueryTextEnabled { get; set; }
//        [PersistenceMode(PersistenceMode.Attribute)]
//        public bool QueryEditorEnabled { get; set; }

//        // Constructor //////////////////////////////////////////////////////////////////
//        public QueryEditor()
//        {
//            queryTextBox = new TextBox { ID = InnerControlID };
//        }

//        // Events ///////////////////////////////////////////////////////////////////////
//        public override void SetData(object data)
//        {
//            var t = (string)data;
//            queryTextBox.Text = t;

//            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
//                return;

//            var title = GetLabelForTitleControl() as Label;
//            var desc = GetLabelForDescription() as Label;
//            var innerCtl = GetInnerControl() as TextBox;
//            if (title != null) title.Text = Field.DisplayName;
//            if (desc != null) desc.Text = Field.Description;
//            if (innerCtl != null) innerCtl.Text = t;
//        }
//        public override object GetData()
//        {
//            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
//                return queryTextBox.Text;
//            var innerCtl = GetInnerControl() as TextBox;
//            return innerCtl != null ? innerCtl.Text : queryTextBox.Text;
//        }
//        protected override void OnInit(EventArgs e)
//        {
//            base.OnInit(e);

//            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
//                return;

//            if (this.QueryTextEnabled)
//            {
//                queryTextBox.CssClass = String.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-ctrl-text" : this.CssClass;
//                queryTextBox.Rows = this.Rows;
//                if (Columns != 0)
//                    queryTextBox.Columns = this.Columns;
//                queryTextBox.MaxLength = this.MaxLength;
//                queryTextBox.TextMode = TextBoxMode.MultiLine;
//            } else
//                queryTextBox.Attributes.Add("style", "display:none");                

//            Controls.Add(queryTextBox);
//        }
//        protected override void RenderContents(HtmlTextWriter writer)
//        {
//            if (UseBrowseTemplate)
//            {
//                base.RenderContents(writer);
//                return;
//            }
//            if (UseEditTemplate)
//            {
//                ManipulateTemplateControls();
//                base.RenderContents(writer);
//                return;
//            }
//            if (UseInlineEditTemplate)
//            {
//                ManipulateTemplateControls();
//                base.RenderContents(writer);
//                return;
//            }

//            if (this.RenderMode == FieldControlRenderMode.Browse)
//                RenderSimple(writer);
//            else
//                RenderEditor(writer);
//        }

//        // Internals ////////////////////////////////////////////////////////////////////
//        private void ManipulateTemplateControls()
//        {
//            var queryInput = GetInnerControl() as TextBox;
//            var lt = GetLabelForTitleControl() as Label;
//            var ld = GetLabelForDescription() as Label;
//            if (queryInput == null) return;

//            if (Field.ReadOnly)
//            {
//                var p = queryInput.Parent;
//                if (p != null)
//                {
//                    p.Controls.Remove(queryInput);
//                    if (lt != null) lt.AssociatedControlID = string.Empty;
//                    if (ld != null) ld.AssociatedControlID = string.Empty;
//                    p.Controls.Add(new LiteralControl(queryInput.Text));
//                }
//            }
//            else if (ReadOnly)
//            {
//                queryInput.Enabled = !ReadOnly;
//                queryInput.EnableViewState = false;
//            }

//        }
//        private void RenderSimple(HtmlTextWriter writer)
//        {
//            RenderEditor(writer);
//        }
//        private void RenderEditor(HtmlTextWriter writer)
//        {
//            if (this.QueryEditorEnabled)
//            {
//                var objectHtmlTag = String.Format(ObjectTag,
//                                                  queryTextBox.ClientID, //{0}
//                                                  PortalContext.Current.ContextNodePath, //{1}
//                                                  Width.IsEmpty ? "300" : Width.Value.ToString(), //{2}
//                                                  Height.IsEmpty ? "200" : Height.Value.ToString()); //{3}
//                writer.Write(objectHtmlTag);
//            }
//            if (this.QueryTextEnabled)
//                queryTextBox.RenderControl(writer);
//        }

//        #region ITemplateFieldControl Members

//        public Control GetInnerControl()
//        {
//            return this.FindControlRecursive(InnerControlID) as TextBox;
//        }
//        public Control GetLabelForDescription()
//        {
//            return this.FindControlRecursive(DescriptionControlID) as Label;
//        }
//        public Control GetLabelForTitleControl()
//        {
//            return this.FindControlRecursive(TitleControlID) as Label;
//        }

//        #endregion

//    }
//}
