using System;
using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository.Storage;
using System.Xml;
using System.IO;
using SenseNet.ContentRepository;

namespace SenseNet.DirectoryServices
{
    public class SyncConfiguration
    {
        protected string _configPath = string.Empty;

        protected XmlDocument ConfigXml
        {
            get
            {
                return GetXmlDocument(_configPath);
            }
        }

        /*============================================================================== Properties */
        public string GuidProp
        {
            get
            {
                if (NovellSupport)
                    return "GUID";

                return "objectguid";
            }
        }
        public bool NovellSupport
        {
            get
            {
                var node = ConfigXml.SelectSingleNode("//General/NovellSupport");
                if (node == null)
                    return false;

                return Convert.ToBoolean(node.InnerText);
            }
        }
        public KeyValuePair<string, string> CustomADAdminAccount
        {
            get
            {
                var node = ConfigXml.SelectSingleNode("//General/CustomADAdminAccount");
                if (node == null)
                    return new KeyValuePair<string, string>();

                var userName = node.SelectSingleNode("UserName").InnerText;
                var passwd = node.SelectSingleNode("Password").InnerText;

                return new KeyValuePair<string, string>(userName, passwd);
            }
        }
        public string CustomADAdminAccountName
        {
            get
            {
                if (CustomADAdminAccount.Key != null)
                    return CustomADAdminAccount.Key;

                return null;
            }
        }
        public string CustomADAdminAccountPwd
        {
            get
            {
                if (!string.IsNullOrEmpty(CustomADAdminAccountName))
                    return CustomADAdminAccount.Value;

                return null;
            }
        }
        public bool SyncEnabledState
        {
            get
            {
                var node = ConfigXml.SelectSingleNode("//General/SyncEnabledState");
                if (node == null)
                    return false;

                return Convert.ToBoolean(node.InnerText);
            }
        }
        public bool SyncUserName
        {
            get
            {
                var node = ConfigXml.SelectSingleNode("//General/SyncUserName");
                if (node == null)
                    return false;

                return Convert.ToBoolean(node.InnerText);
            }
        }


        /*============================================================================== Methods */
        public List<SyncTree> GetSyncTrees()
        {
            if (ConfigXml == null)
                return null;

            var syncTrees = new List<SyncTree>();
            foreach (XmlNode node in ConfigXml.SelectNodes("//SyncTrees/SyncTree"))
            {
                var ADExceptions = new List<string>();
                foreach (XmlNode exceptionNode in node.SelectNodes("ADExceptions/ADException"))
                {
                    ADExceptions.Add(exceptionNode.InnerText);
                }
                var portalExceptions = new List<string>();
                foreach (XmlNode exceptionNode in node.SelectNodes("PortalExceptions/PortalException"))
                {
                    portalExceptions.Add(exceptionNode.InnerText);
                }

                string deletedADObjectsPath = null;
                var deletedADObjectsPathNode = node.SelectSingleNode("DeletedADObjectsPath");
                if (deletedADObjectsPathNode != null)
                {
                    deletedADObjectsPath = deletedADObjectsPathNode.InnerText;
                }

                bool syncGroups = true;
                var syncGroupsNode = node.SelectSingleNode("SyncGroups");
                if (syncGroupsNode != null)
                {
                    syncGroups = Convert.ToBoolean(syncGroupsNode.InnerText);
                }

                syncTrees.Add(
                    new SyncTree()
                    {
                        Config = this,
                        ADPath = node.SelectSingleNode("AdPath").InnerText,
                        PortalPath = node.SelectSingleNode("PortalPath").InnerText,
                        IPAddress = node.SelectSingleNode("DomainIp").InnerText,
                        DeletedADObjectsPath = deletedADObjectsPath,
                        ADExceptions = ADExceptions,
                        PortalExceptions = portalExceptions,
                        SyncGroups = syncGroups
                    }
                    );
            }
            return syncTrees;
        }
        public List<PropertyMapping> GetPropertyMappings()
        {
            if (ConfigXml == null)
                return null;

            var propertyMappings = new List<PropertyMapping>();
            foreach (XmlNode node in ConfigXml.SelectNodes("//PropertyMappings/PropertyMapping"))
            {
                var adProperties = new List<SyncProperty>();
                foreach (XmlNode adPropNode in node.SelectNodes("AdProperty"))
                {
                    AddSyncProperty(adProperties, adPropNode);
                }

                var portalProperties = new List<SyncProperty>();
                foreach (XmlNode portalPropNode in node.SelectNodes("PortalProperty"))
                {
                    AddSyncProperty(portalProperties, portalPropNode);
                }

                var separator = (node.Attributes["separator"] == null) ? null : node.Attributes["separator"].Value;

                propertyMappings.Add(
                    new PropertyMapping()
                    {
                        ADProperties = adProperties,
                        PortalProperties = portalProperties,
                        Separator = separator
                    }
                    );

            }
            return propertyMappings;
        }

        /*============================================================================== Static methods */
        protected static void AddSyncProperty(List<SyncProperty> properties, XmlNode propNode)
        {
            var maxLengthAttrib = propNode.Attributes["maxLength"];
            var maxLength = Int32.MinValue;

            if (maxLengthAttrib != null)
                Int32.TryParse(maxLengthAttrib.Value, out maxLength);

            var unique = false;
            var uniqueAttrib = propNode.Attributes["unique"];
            if (uniqueAttrib != null)
                bool.TryParse(uniqueAttrib.Value, out unique);

            properties.Add(
                new SyncProperty() { MaxLength = maxLength, Name = propNode.InnerText, Unique = unique }
                );
        }
        protected static XmlDocument GetXmlDocument(string ConfigPath)
        {
            ContentRepository.File configNode = Node.Load<ContentRepository.File>(ConfigPath);

            if (configNode == null)
                return null;

            var configXml = new XmlDocument();
            using (TextReader reader = new StreamReader(configNode.Binary.GetStream()))
            {
                configXml.LoadXml(reader.ReadToEnd());
            }
            return configXml;
        }
        public static string GetUserNameProp(List<PropertyMapping> propMappings)
        {
            foreach (var propMapping in propMappings)
            {
                if (propMapping.PortalProperties[0].Name == "Name")
                {
                    return propMapping.ADProperties[0].Name;
                }
            }
            return "sAMAccountName";
        }
    }

    public class Portal2ADConfiguration : SyncConfiguration
    {
        private const int DefaultADNameMaxLength = 20;
        private const int DefaultADsAMAccountNameMaxLength = 20;

        private static object synchlock = new object();
        private static Portal2ADConfiguration _current;
        public static Portal2ADConfiguration Current
        {
            get
            {
                if (_current == null)
                {
                    lock (synchlock)
                    {
                        if (_current == null)
                        {
                            _current = new Portal2ADConfiguration();
                        }
                    }
                }
                return _current;
            }
        }
        public Portal2ADConfiguration()
        {
            _configPath = "/Root/System/SystemPlugins/Tools/DirectoryServices/Portal2ADConfig.xml";
        }


        /*============================================================================== General settings - Portal2AD */
        public bool CreatedAdUsersDisabled
        {
            get
            {
                var configXml = ConfigXml;
                if (configXml == null)
                    return false;

                var node = configXml.SelectSingleNode("//General/CreatedAdUsersDisabled");
                if (node == null)
                    return false;

                return Convert.ToBoolean(node.InnerText);
            }
        }
        public int ADNameMaxLength
        {
            get
            {
                var configXml = ConfigXml;
                if (configXml == null)
                    return DefaultADNameMaxLength;

                var node = configXml.SelectSingleNode("//General/ADNameMaxLength");
                if (node == null)
                    return DefaultADNameMaxLength;

                return Convert.ToInt32(node.InnerText);
            }
        }
        public int ADsAMAccountNameMaxLength
        {
            get
            {
                var configXml = ConfigXml;
                if (configXml == null)
                    return DefaultADsAMAccountNameMaxLength;

                var node = configXml.SelectSingleNode("//General/ADsAMAccountNameMaxLength");
                if (node == null)
                    return DefaultADsAMAccountNameMaxLength;

                return Convert.ToInt32(node.InnerText);
            }
        }
        public bool SaveFailedPassword
        {
            get
            {
                var configXml = ConfigXml;
                if (configXml == null)
                    return false;

                var node = configXml.SelectSingleNode("//General/SaveClearTextPassword");
                if (node == null)
                    return false;

                return Convert.ToBoolean(node.InnerText);
            }
        }
        public bool AllowRename
        {
            get
            {
                var node = ConfigXml.SelectSingleNode("//General/AllowRename");
                if (node == null)
                    return false;

                return Convert.ToBoolean(node.InnerText);
            }
        }
        public bool AllowMove
        {
            get
            {
                var node = ConfigXml.SelectSingleNode("//General/AllowMove");
                if (node == null)
                    return false;

                return Convert.ToBoolean(node.InnerText);
            }
        }
    }

    public class AD2PortalConfiguration : SyncConfiguration
    {
        private static object synchlock = new object();
        private static AD2PortalConfiguration _current;
        public static AD2PortalConfiguration Current
        {
            get
            {
                if (_current == null)
                {
                    lock (synchlock)
                    {
                        if (_current == null)
                        {
                            _current = new AD2PortalConfiguration();
                        }
                    }
                }
                return _current;
            }
        }
        public AD2PortalConfiguration()
        {
            _configPath = "/Root/System/SystemPlugins/Tools/DirectoryServices/AD2PortalConfig.xml";
        }


        /*============================================================================== General settings - AD2Portal */
        public string UserNameProp
        {
            get
            {
                return GetUserNameProp(this.GetPropertyMappings());
            }
        }
        public string UserType
        {
            get
            {
                var configXml = ConfigXml;
                if (configXml == null)
                    return typeof(User).Name;

                var node = configXml.SelectSingleNode("//General/UserType");
                return (node != null) ? node.InnerText : typeof(User).Name;
            }
        }
        public string DeletedFromADPath
        {
            get
            {
                var configXml = ConfigXml;
                if (configXml == null)
                    return null;

                var node = configXml.SelectSingleNode("//General/DeletedPortalObjectsPath");
                return node.InnerText;
            }
        }
        public bool AlwaysSyncObjects
        {
            get
            {
                var configXml = ConfigXml;
                if (configXml == null)
                    return true;

                var node = configXml.SelectSingleNode("//General/AlwaysSyncObjects");
                return Convert.ToBoolean(node.InnerText);
            }
        }
    }
}
