using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.UI.PortletFramework
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class EditorOptions : Attribute
    {
        public EditorOptions(params object[] args)
        {
        }
    }
}
