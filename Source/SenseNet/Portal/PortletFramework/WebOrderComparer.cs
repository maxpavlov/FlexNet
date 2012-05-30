using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class WebOrderComparer : IComparer<PropertyDescriptor>
    {
        public int Compare(PropertyDescriptor x, PropertyDescriptor y)
        {
            if (x == null || y == null)
                return 0;

            var xDescriptorCategory = x.Attributes[typeof(WebOrderAttribute)] as WebOrderAttribute;
            var yDescriptorCategory = y.Attributes[typeof(WebOrderAttribute)] as WebOrderAttribute;

            if (xDescriptorCategory == null || yDescriptorCategory == null)
                return 0;

            return xDescriptorCategory.Index.CompareTo(yDescriptorCategory.Index);
        }
    }
}
