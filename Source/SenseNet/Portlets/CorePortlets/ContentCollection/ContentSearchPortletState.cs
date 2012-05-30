using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets.ContentCollection
{
    [Serializable]
    public class ContentSearchPortletState : ContentCollectionPortletState
    {
        public ContentSearchPortletState(PortletBase portlet)
            : base(portlet)
        {

        }

        public string ExportQueryFields { get; set; }
    }
}
