using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Controls
{
    [ParseChildren(false)]
    [ToolboxData(@"<{0}:Toolbar runat=server CssClass=""sn-toolbar"" />")]
    public class Toolbar : Panel
    {
        private bool _checkVisibiity = true;

        /// <summary>
        /// Check the visibility of child controls. If none of them is visible (except LiteralControls), the toolbar will not render itself.
        /// </summary>
        public bool CheckVisibiity
        {
            get { return _checkVisibiity; }
            set { _checkVisibiity = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CssClass += " sn-toolbar";
            CssClass = CssClass.Trim();
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-toolbar-inner");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            base.RenderContents(writer);
            writer.RenderEndTag();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.CheckVisibiity)
                CheckVisibleControls();

            if (!this.Visible)
                return;

            base.Render(writer);

        }

        private void CheckVisibleControls()
        {
            try
            {
                if (!FindVisibleControl(this))
                    this.Visible = false;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        private bool FindVisibleControl(Control control)
        {
            if (control == null || control is LiteralControl)
                return false;

            foreach (Control childControl in control.Controls)
            {
                if (childControl is LiteralControl || childControl is ToolbarSeparator)
                    continue;

                if (FindVisibleControl(childControl))
                    return true;

                if (childControl is Panel || childControl is PlaceHolder)
                    continue;

                if (childControl.Visible)
                    return true;
            }

            return false;
        }
    }

    [ParseChildren(false)]
    [ToolboxData(@"<{0}:ToolbarItemGroup runat=server CssClass=""sn-toolbar-btngroup"" />")]
    public class ToolbarItemGroup : Panel
    {
        public string Align { get; set; }

        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);

            CssClass += " sn-toolbar-btngroup";
            CssClass = CssClass.Trim();

            if (!string.IsNullOrEmpty(Align))
            {
                switch (Align.ToLower())
                {
                    case "left": CssClass += " sn-toolbar-leftgroup"; break;
                    case "right": CssClass += " sn-toolbar-rightgroup"; break;
                    case "center": CssClass += " sn-toolbar-centergroup"; break;
                }
            }
        }
    }

    [ParseChildren(false)]
    [ToolboxData(@"<{0}:ToolbarSeparator runat=server CssClass=""sn-toolbar-separator"" />")]
    public class ToolbarSeparator : Label
    {
        public string Align { get; set; }

        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);

            CssClass += " sn-toolbar-separator";
            CssClass = CssClass.Trim();

            if (!string.IsNullOrEmpty(Align))
            {
                switch (Align.ToLower())
                {
                    case "left": CssClass += " sn-toolbar-leftgroup"; break;
                    case "right": CssClass += " sn-toolbar-rightgroup"; break;
                    case "center": CssClass += " sn-toolbar-centergroup"; break;
                }
            }
        }
    }
}
