using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using System.Reflection;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class PortletInventoryItem
    {
        public PortletBase Portlet { get; set; }
        public System.IO.Stream ImageStream { get; set; }

        public SenseNet.ContentRepository.Fields.ImageField.ImageFieldData GetImageFieldData()
        {
            if (this.ImageStream == null)
                return null;

            var binaryData = new BinaryData();
            binaryData.SetStream(this.ImageStream);
            return new SenseNet.ContentRepository.Fields.ImageField.ImageFieldData(null, binaryData);
        }
        public static PortletInventoryItem Create(PortletBase portlet, Assembly assembly)
        {
            var portletItem = new PortletInventoryItem();
            portletItem.Portlet = portlet;

            // get resource image
            var imageName = string.Concat(portlet.GetType().ToString(), ".png");
            var imageStream = assembly.GetManifestResourceStream(imageName);
            if (imageStream != null)
                portletItem.ImageStream = imageStream;

            return portletItem;
        }
    }
}
