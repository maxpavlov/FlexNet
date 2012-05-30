using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.DirectoryServices
{
    public class PropertyMapping
    {
        /* ==================================================================================== Properties */
        private List<SyncProperty> _ADProperties;
        public List<SyncProperty> ADProperties
        {
            get { return _ADProperties; }
            set { _ADProperties = value; }
        }

        private List<SyncProperty> _portalProperties;
        public List<SyncProperty> PortalProperties
        {
            get { return _portalProperties; }
            set { _portalProperties = value; }
        }

        private string separator;
        public string Separator
        {
            get { return separator; }
            set { separator = value; }
        }

        /* ==================================================================================== Methods */

        // végigmegy az ad propertyken és összefűzi egy string-gé (pl fullName = initials + firstname + surname)
        public string ConcatADPropValues(DirectoryEntry entry)
        {
            var portalValue = string.Empty;
            bool first = true;
            foreach (SyncProperty adProp in this.ADProperties)
            {
                var adValue = Common.GetEntryValue(entry, adProp);

                portalValue = string.Concat(
                    portalValue,
                    first ? string.Empty : this.Separator,
                    adValue);
                if (first)
                    first = false;
            }
            return portalValue;
        }

        // végigmegy a portal propertyken és összefűzi egy string-gé
        public string ConcatPortalPropValues(Node node)
        {
            var adValue = string.Empty;
            bool first = true;
            foreach (SyncProperty portalProp in this.PortalProperties)
            {
                string portalValue = Common.GetNodeValue(node, portalProp);
                if (portalValue == null)
                    portalValue = string.Empty;

                adValue = string.Concat(
                    adValue,
                    first ? string.Empty : this.Separator,
                    portalValue);
                if (first)
                    first = false;
            }
            return adValue;
        }
    }
}
