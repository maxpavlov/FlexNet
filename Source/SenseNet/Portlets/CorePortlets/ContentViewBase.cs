using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace SenseNet.Portal.Portlets
{
    public class ContentViewBase : UserControl
    {
        public ContentViewModel Model { get; set; }

        internal void SetModel(ContentViewModel model)
        {
            this.Model = model;

        }

        public string GetLastId()
        {
            return string.Empty;
        }

    }
}
