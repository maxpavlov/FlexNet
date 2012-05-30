using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Controls
{
    public class ListGrid : ListView
    {
        /* ========================================================================================== Properties */
        public bool ShowCheckboxes
        {
            get
            {
                var parentView = this.Parent as ContentListViews.ListView;
                if (parentView != null)
                    return parentView.ShowCheckboxes.HasValue ? parentView.ShowCheckboxes.Value : false;
                return false;
            }
        }
        public Control CheckBoxHeader
        {
            get { return this.FindControlRecursive("checkboxHeader"); }
        }

        public string DefaulSortExpression
        {
            get; set;
        }

        public SortDirection DefaulSortDirection
        {
            get;
            set;
        }

        /* ========================================================================================== Methods */
        protected override void OnInit(EventArgs e)
        {
            this.EnableViewState = false;

            base.OnInit(e);
            UITools.AddScript(UITools.ClientScriptConfigurations.SNListGridPath);
        }
        protected override void CreateChildControls()
        {
            try
            {
                base.CreateChildControls();

                // hide checkboxheader when not needed
                var checkboxHeader = this.CheckBoxHeader;
                if (checkboxHeader != null)
                {
                    if (!this.ShowCheckboxes)
                        checkboxHeader.Visible = false;
                }

            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);

                this.Controls.Add(new LiteralControl("ListView error: " + ex.Message));
            }
        }
        protected override void Render(HtmlTextWriter writer)
        {
            RenderBeginTag(writer);
            base.Render(writer);
            RenderEndTag(writer);

            var script = string.Format("SN.ListGrid.init('{0}');", this.ClientID);
            UITools.RegisterStartupScript("startup_" + this.ClientID, script, Page);

        }
        public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            //writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-listgrid-container");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            base.RenderBeginTag(writer);
        }
        public override void RenderEndTag(System.Web.UI.HtmlTextWriter writer)
        {
            base.RenderEndTag(writer);
            writer.RenderEndTag();
        }
    }
}
