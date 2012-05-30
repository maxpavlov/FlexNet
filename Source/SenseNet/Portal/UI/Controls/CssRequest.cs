using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace SenseNet.Portal.UI.Controls
{
    public class CssRequest : UserControl
    {
        /* ========================================================================= Properties */
        private string _cssPath;
        public string CSSPath
        {
            get { return _cssPath; }
            set { _cssPath = value; }
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

        /* ========================================================================= Methods */
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (string.IsNullOrEmpty(this.CSSPath))
                return;

            UITools.AddStyleSheetToHeader(UITools.GetHeader(), this.CSSPath, this.Order, this.Rel, this.Type, this.Media, this.Title);
        }
    }
}
