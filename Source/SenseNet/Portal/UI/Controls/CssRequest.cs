using System;
using System.Web.UI;

namespace SenseNet.Portal.UI.Controls
{
    public class CssRequest : Control
    {
        /* ========================================================================= Properties */
        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        [Obsolete("Use Path instead.")]
        public string CSSPath
        {
            get { return Path; }
            set { Path = value; }
        }

        private int _order;
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        private string _rel = "stylesheet";
        public string Rel
        {
            get { return _rel; }
            set { _rel = value; }
        }

        private string _type = "text/css";
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _media = "all";
        public string Media
        {
            get { return _media; }
            set { _media = value; }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private bool _allowBundling = true;
        public bool AllowBundling
        {
            get { return _allowBundling; }
            set { _allowBundling = value; }
        }

        /* ========================================================================= Methods */

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (string.IsNullOrEmpty(this.Path))
                return;

            UITools.AddStyleSheetToHeader(UITools.GetHeader(), this.Path, this.Order, this.Rel, this.Type, this.Media, this.Title, this.AllowBundling);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            return;
        }
    }
}
