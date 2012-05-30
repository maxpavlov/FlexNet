using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.UI.PortletFramework
{
    public sealed class WebOrderAttribute : Attribute
    {
        private int _index;
        public WebOrderAttribute() : this(0) { }
        public WebOrderAttribute(int index) 
        {
            Index = index;
        }
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }
    }
}
