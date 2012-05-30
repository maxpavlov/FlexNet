using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class WebCategoryComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            var xDescriptor = (PropertyDescriptor)x;
            var yDescriptor = (PropertyDescriptor)y;

            if (xDescriptor == null || yDescriptor == null)
                return 0;

            var xDescriptorCategory = xDescriptor.Attributes[typeof(WebCategoryAttribute)] as WebCategoryAttribute;
            var yDescriptorCategory = yDescriptor.Attributes[typeof(WebCategoryAttribute)] as WebCategoryAttribute;

            if (xDescriptorCategory == null || yDescriptorCategory == null)
                return 0;

            return xDescriptorCategory.Index.CompareTo(yDescriptorCategory.Index);

        }
    }
}
