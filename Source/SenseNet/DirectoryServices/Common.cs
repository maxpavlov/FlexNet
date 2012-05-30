using System;
using System.Collections.Generic;
using System.DirectoryServices;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Security.ADSync;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using SenseNet.Search;

namespace SenseNet.DirectoryServices
{
    public static class Common
    {
        /* ==================================================================================== Static Methods */
        // gets directoryentry from AD - no custom account is used and Novell is not supported
        public static DirectoryEntry ConnectToADSimple(string ldapPath)
        {
            var deADConn = new DirectoryEntry(ldapPath);

            Exception exADConnectException = null;
            bool bError = false;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var oNativeObject = deADConn.NativeObject;
                    bError = false;
                    break;
                }
                catch (Exception ex)
                {
                    bError = true;
                    exADConnectException = ex;
                    System.Threading.Thread.Sleep(3000);
                }
            }

            if (bError)
            {
                AdLog.LogException(exADConnectException);
                throw new Exception("Connecting to AD server failed", exADConnectException);
            }
            return deADConn;
        }
        // gets directoryentry from AD - custom account CAN BE used and Novell IS supported
        public static DirectoryEntry ConnectToAD(string ldapPath, string customADAdminAccountName, string customADAdminAccountPwd, bool novellSupport, string guidProp)
        {
            var deADConn = new DirectoryEntry(ldapPath);

            // use custom login to retrieve AD object
            if (!string.IsNullOrEmpty(customADAdminAccountName))
            {
                deADConn.AuthenticationType = AuthenticationTypes.ServerBind;
                deADConn.Username = customADAdminAccountName;
                deADConn.Password = customADAdminAccountPwd;
            }


            Exception exADConnectException = null;
            bool bError = false;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var oNativeObject = deADConn.NativeObject;
                    bError = false;
                    break;
                }
                catch (Exception ex)
                {
                    bError = true;
                    exADConnectException = ex;
                    System.Threading.Thread.Sleep(3000);
                }
            }

            if (bError)
            {
                AdLog.LogException(exADConnectException);
                throw new Exception("Connecting to AD server failed", exADConnectException);
            }


            // NOVELL - use a searcher to retrieve the objects' GUID 
            // - directoryentry properties does not include guid when connecting to Novell eDirectory
            if (novellSupport)
            {
                var dsDirSearcher = new DirectorySearcher(deADConn);

                var dn = ldapPath.Substring(ldapPath.LastIndexOf("/") + 1);

                dsDirSearcher.PropertiesToLoad.Add(guidProp);
                dsDirSearcher.SearchScope = SearchScope.Base;
                var result = dsDirSearcher.FindOne();
                var guid = result.Properties[guidProp][0];
                deADConn.Properties[guidProp].Add(guid);
            }

            return deADConn;
        }
        public static DirectoryEntry SearchADObject(DirectoryEntry searchRoot, string filter, bool novellSupport, string guidProp)
        {
            //Create a directory searcher
            var dsDirSearcher = new DirectorySearcher(searchRoot);

            //Set the search filter
            dsDirSearcher.Filter = filter;

            // NOVELL - force searcher to retrieve GUID
            if (novellSupport)
                dsDirSearcher.PropertiesToLoad.Add(guidProp);

            //Find the user
            var result = dsDirSearcher.FindOne();

            return (result != null ? result.GetDirectoryEntry() : null);
        }
        public static SearchResultCollection Search(DirectoryEntry searchRoot, string filter, bool novellSupport, string guidProp)
        {
            //Create a directory searcher
            var dsDirSearcher = new DirectorySearcher(searchRoot);

            //Set the search filter
            dsDirSearcher.Filter = filter;
            dsDirSearcher.SizeLimit = 10000;
            dsDirSearcher.PageSize = 10000;

            // NOVELL - force searcher to retrieve the objects' GUID 
            // - this is not done by default when connecting to Novell eDirectory
            if (novellSupport)
                dsDirSearcher.PropertiesToLoad.Add(guidProp);

            //Find the user
            try
            {
                var oResults = dsDirSearcher.FindAll();
                return oResults;
            }
            catch (Exception e)
            {
                AdLog.LogException(e);
            }
            return null;
        }
        public static void EnsurePath(string path)
        {
            if (!Node.Exists(path))
            {
                var parentPath = RepositoryPath.GetParentPath(path);
                EnsurePath(parentPath);
                Folder folder = new Folder(Node.LoadNode(parentPath));

                folder.Name = RepositoryPath.GetFileName(path);
                folder.Save();
            }
        }
        // creates guid object from bytearray, returns null if parameter is not in right format
        public static Guid? GetGuid(byte[] byteArray)
        {
            if (byteArray == null)
                return null;

            if (byteArray.Length != 16)
                return null;

            return new Guid(byteArray);
        }
        // gets the GUID of the searchresult object
        public static Guid? GetADResultGuid(SearchResult result, string guidProp)
        {
            var props = (result.Properties[guidProp]);
            if ((props == null) || (props.Count < 1))
                return null;

            return GetGuid(props[0] as byte[]);
        }
        // gets the GUID of an AD object
        public static Guid? GetADObjectGuid(DirectoryEntry entry, string guidProp)
        {
            var props = (entry.Properties[guidProp]);
            if ((props == null) || (props.Count < 1))
                return null;

            return GetGuid(props[0] as byte[]);
        }
        public static Guid? GetPortalObjectGuid(Node node)
        {
            if (!node.HasProperty("SyncGuid"))
                return null;
            var guidStr = node["SyncGuid"] as string;
            if (string.IsNullOrEmpty(guidStr))
                return null;
            return new Guid(guidStr);
        }
        public static Node GetPortalObjectByGuid(Guid guid)
        {
            return ContentQuery.Query(string.Format("SyncGuid:\"{0}\"", guid.ToString())).Nodes.FirstOrDefault();
        }
        public static void SetPortalObjectGuid(DirectoryEntry entry, Node node, string guidProp)
        {
            // get guid
            var guid = Common.GetADObjectGuid(entry, guidProp);
            if (guid.HasValue)
            {
                Common.UpdateLastSync(node, guid);
                //node.Save();
            }
            else
            {
                AdLog.LogErrorADObject("Created AD object does not have a guid", entry.Path);
            }
        }
        public static string Guid2OctetString(Guid guid)
        {
            byte[] byteGuid = guid.ToByteArray();
            string queryGuid = "";
            foreach (byte b in byteGuid)
            {
                queryGuid += @"\" + b.ToString("x2");
            }
            return queryGuid;
        }
        public static bool IsAccountDisabled(DirectoryEntry adUser, bool novellSupport)
        {
            // NOVELL - this property is not supported when connecting to Novell eDirectory
            // created users will always be enabled
            if (novellSupport)
                return false;

            int iFlagIndicator = (int)adUser.Properties["userAccountControl"].Value &
                                 Convert.ToInt32(ADAccountOptions.UF_ACCOUNTDISABLE);
            if (iFlagIndicator > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // goes through AD object's prop's propcollection and decides if any prop's value equals to the given one
        public static bool PropValueCollContains(PropertyValueCollection propValueColl, string value)
        {
            bool bContains = false;
            for (int i = 0; i < propValueColl.Count; i++)
            {
                if (propValueColl[i].Equals(value))
                {
                    bContains = true;
                    break;
                }
            }
            return bContains;
        }
        public static bool IsADObjectUser(DirectoryEntry adObject, bool novellSupport)
        {
            // NOVELL - only objectClass is supported and person instead of user
            if (novellSupport)
                return PropValueCollContains(adObject.Properties["objectClass"], "Person");

            if (adObject.Properties["objectCategory"].Value.ToString().ToLower().IndexOf("person") != -1)
            {
                return PropValueCollContains(adObject.Properties["objectClass"], "user");
            }
            else
            {
                return false;
            }
        }
        public static bool IsADObjectGroup(DirectoryEntry adObject)
        {
            return PropValueCollContains(adObject.Properties["objectClass"], "group");
        }
        public static bool IsADObjectOrgUnit(DirectoryEntry adObject)
        {
            return PropValueCollContains(adObject.Properties["objectClass"], "organizationalUnit");
        }
        public static bool IsADObjectOrganization(DirectoryEntry adObject)
        {
            return PropValueCollContains(adObject.Properties["objectClass"], "Organization");
        }
        public static bool IsADObjectDomain(DirectoryEntry adObject)
        {
            return PropValueCollContains(adObject.Properties["objectClass"], "domain");
        }
        public static bool IsADObjectContainer(DirectoryEntry adObject)
        {
            return PropValueCollContains(adObject.Properties["objectClass"], "container");
        }
        // decides if the AD object is a user or a group
        public static ADObjectType GetADObjectType(DirectoryEntry entry, bool novellSupport)
        {
            if (IsADObjectUser(entry, novellSupport))
                return ADObjectType.User;
            if (IsADObjectGroup(entry))
                return ADObjectType.Group;
            if (IsADObjectOrgUnit(entry))
                return ADObjectType.OrgUnit;
            if (IsADObjectOrganization(entry))
                return ADObjectType.Organization;
            if (IsADObjectContainer(entry))
                return ADObjectType.Container;
            if (IsADObjectDomain(entry))
                return ADObjectType.Domain;
            return ADObjectType.None;
        }
        // gets the portal NodeType corresponding to the AD object type
        public static NodeType GetNodeType(ADObjectType adObjectType)
        {
            switch (adObjectType)
            {
                case ADObjectType.User:
                    return NodeType.GetByName(typeof(User).Name);
                case ADObjectType.Group:
                    return NodeType.GetByName(typeof(Group).Name);
                case ADObjectType.OrgUnit:
                case ADObjectType.Organization:
                    return NodeType.GetByName(typeof(OrganizationalUnit).Name);
                case ADObjectType.Domain:
                    return NodeType.GetByName(typeof(Domain).Name);
                case ADObjectType.Container:
                    return NodeType.GetByName("ADFolder");
                default:
                    return null;
            }
        }
        public static ADObjectType GetADObjectType(NodeType nodeType)
        {
            switch (nodeType.Name)
            {
                case "User":
                    return ADObjectType.User;
                case "Group":
                    return ADObjectType.Group;
                case "OrganizationalUnit":
                    return ADObjectType.OrgUnit;
                case "Domain":
                    return ADObjectType.Domain;
                case "ADFolder":
                    return ADObjectType.Container;
                default:
                    return ADObjectType.None;
            }
        }
        public static string GetADObjectPrefix(ADObjectType adObjectType)
        {
            switch (adObjectType)
            {
                case ADObjectType.Container:
                case ADObjectType.User:
                case ADObjectType.Group:
                    return "CN=";
                case ADObjectType.OrgUnit:
                    return "OU=";
                case ADObjectType.Domain:
                    return "DC=";
                default:
                    return null;
            }
        }
        public static string CombineADPath(string path1, string path2)
        {
            // path1: DC=Nativ,DC=Local
            // path2: OU=ExampleOrg

            // return: OU=ExampleOrg,DC=Nativ,DC=Local
            return string.Concat(path2, ",", path1);
        }
        public static string GetADObjectNameFromPath(string portalPath)
        {
            var node = Node.LoadNode(portalPath);
            var nodeType = node.NodeType;
            var ADObjType = GetADObjectType(nodeType);
            var ADObjPrefix = GetADObjectPrefix(ADObjType);
            var ADObjName = node.Name;

            return string.Concat(ADObjPrefix, ADObjName);
        }
        /// <summary>
        /// Gets the portal representative for a given AD domain 
        /// </summary>
        public static string GetPortalDomainName(string ADDomainPath)
        {
            // ADDomainPath pl.: DC=Nativ,DC=local
            string[] directories = ADDomainPath.Split(new string[] { "DC=" }, StringSplitOptions.RemoveEmptyEntries);
            string portalPath = string.Empty;
            foreach (string dir in directories)
            {
                portalPath = string.Concat(portalPath, ".", dir.TrimEnd(new char[] { ',' }));
            }
            return portalPath.TrimStart(new char[] { '.' });
        }
        // gets the object name from the name as it comes from AD (ie: ExampleOrg from OU=ExampleOrg)
        public static string GetADObjectName(string name)
        {
            // name pl.: "OU=ExampleOrg"
            //return name.Substring(3);
            return Common.StripADName(name.Substring(name.IndexOf("=") + 1));
        }
        // set the LastSync property of portal node indicating the date of the last synchronization
        public static void UpdateLastSync(Node node, Guid? guid)
        {
            var syncNode = node as IADSyncable;
            if (syncNode != null)
                syncNode.UpdateLastSync(guid);
        }
        // should the portal object be synchronized?
        // note: this is only to decide whether the properties/name of the object has changed
        //       moving of objects is carried out independently
        public static bool IsPortalObjectInvalid(Node node, SearchResult result, bool novellSupport)
        {
            // NOVELL - objects are always synced, as there is no such property named "whenchanged"
            if (novellSupport)
                return true;

            var propColl = result.Properties["whenchanged"];
            var lastMod = Convert.ToDateTime(propColl[0]);

            var lastSync = (DateTime)node["LastSync"];
            lastSync = TimeZoneInfo.ConvertTime(lastSync, TimeZoneInfo.Utc);
            var syncNeeded = lastMod > lastSync;
            return syncNeeded;
        }
        public static void SetPassword(DirectoryEntry adUser, string password)
        {
            try
            {
                adUser.Invoke("SetPassword", new Object[] { password });
            }
            catch (Exception ex)
            {
                //Log the error
                AdLog.LogException(ex);
            }
        }
        public static void EnableUserAccount(DirectoryEntry adUser)
        {
            object natUser = adUser.NativeObject;
            Type t = natUser.GetType();
            t.InvokeMember("AccountDisabled", System.Reflection.BindingFlags.SetProperty, null, natUser, new object[] { false });
        }
        public static void DisableUserAccount(DirectoryEntry adUser)
        {
            object natUser = adUser.NativeObject;
            Type t = natUser.GetType();
            t.InvokeMember("AccountDisabled", System.Reflection.BindingFlags.SetProperty, null, natUser, new object[] { true });
        }
        public static void RenameADObjectIfNecessary(DirectoryEntry entry, Node node, int ADNameMaxLength, bool allowRename)
        {
            if (!allowRename)
                return;

            if (GetADObjectName(entry.Name) != node.Name)
            {
                var nodeNameMax = node.Name.MaximizeLength(ADNameMaxLength);
                var name = string.Concat(GetADObjectPrefix(GetADObjectType(node.NodeType)), nodeNameMax);
                entry.Rename(name);
            }
        }
        public static void UpdatePortalUserCustomProperties(DirectoryEntry entry, Node node, List<PropertyMapping> propertyMappings, bool syncUserName)
        {
            var user = (IUser)node;

            // sAMAccountName -> Name
            if (syncUserName)
                node.Name = entry.Properties[SyncConfiguration.GetUserNameProp(propertyMappings)].Value.ToString();

            // user actions
            foreach (PropertyMapping propMapping in propertyMappings)
            {
                if (propMapping.ADProperties.Count == 1)
                {
                    if (propMapping.PortalProperties.Count == 1)
                    {
                        // 1db ADproperty + 1db portalproperty
                        var portalProp = propMapping.PortalProperties[0];
                        var adProp = propMapping.ADProperties[0];
                        var adValue = GetEntryValue(entry, adProp);
                        SetNodeValue(node, portalProp, adValue);
                    }
                    else
                    {
                        // 1db ADproperty + xdb portalproperty
                        // az AD propertyt felvágjuk az elválasztók mentén (üreseket is meghagyjuk), majd
                        // ezek kerülnek be sorrendben a portal propertykbe
                        var adProp = propMapping.ADProperties[0];
                        var adValues = GetEntryValue(entry, adProp).Split(new string[] { propMapping.Separator }, StringSplitOptions.None);
                        int index = 0;
                        foreach (SyncProperty portalProp in propMapping.PortalProperties)
                        {
                            var adValue = (index < adValues.Length) ? adValues[index] : null;
                            SetNodeValue(node, portalProp, adValue);
                            index++;
                        }
                    }
                }
                else
                {
                    // 1db portalproperty + xdb ADproperty
                    // az AD propertyk értékét összefűzzük egy stringbe, majd beadjuk a portalpropertynek
                    var portalProp = propMapping.PortalProperties[0];
                    var adValue = propMapping.ConcatADPropValues(entry);
                    SetNodeValue(node, portalProp, adValue);
                }
            }
        }
        public static void DisablePortalUserCustomProperties(Node node, List<PropertyMapping> propertyMappings)
        {
            node.Name = node.Name.PrefixDeleted();

            foreach (PropertyMapping propMapping in propertyMappings)
            {
                foreach (SyncProperty portalProp in propMapping.PortalProperties)
                {
                    if (portalProp.Unique)
                    {
                        var propValue = GetNodeValue(node, portalProp);
                        if (propValue == null)
                            propValue = string.Empty;

                        var setValue = propValue.PrefixDeleted();
                        SetNodeValue(node, portalProp, setValue);
                    }
                }
            }
        }
        public static void UpdateADUserCustomProperties(DirectoryEntry entry, Node user, List<PropertyMapping> propertyMappings, bool enabled, int ADsAMAccountNameMaxLength, bool syncEnabledState, bool syncUserName)
        {
            if (syncEnabledState)
            {
                if (enabled)
                    Common.EnableUserAccount(entry);
                else
                    Common.DisableUserAccount(entry);
            }

            // Name -> sAMAccountName
            if (syncUserName)
                entry.Properties[SyncConfiguration.GetUserNameProp(propertyMappings)].Value = user.Name.MaximizeLength(ADsAMAccountNameMaxLength);

            foreach (PropertyMapping propMapping in propertyMappings)
            {
                if (propMapping.ADProperties.Count == 1)
                {
                    if (propMapping.PortalProperties.Count == 1)
                    {
                        // 1db ADproperty + 1db portalproperty
                        var adProp = propMapping.ADProperties[0];
                        var portalProp = propMapping.PortalProperties[0];
                        var portalValue = GetNodeValue(user, portalProp);
                        SetEntryValue(entry, adProp, portalValue);
                    }
                    else
                    {
                        // 1db ADproperty + xdb portalproperty
                        var adProp = propMapping.ADProperties[0];
                        var portalValue = propMapping.ConcatPortalPropValues(user);
                        SetEntryValue(entry, adProp, portalValue);
                    }
                }
                else
                {
                    // 1db portalproperty + xdb ADproperty
                    var portalProp = propMapping.PortalProperties[0];
                    var propValue = GetNodeValue(user, portalProp);
                    if (propValue == null)
                        propValue = string.Empty;
                    var portalValues = propValue.Split(new string[] { propMapping.Separator }, StringSplitOptions.None);
                    int index = 0;
                    foreach (SyncProperty adProp in propMapping.ADProperties)
                    {
                        var portalValue = (index < portalValues.Length) ? portalValues[index] : null;
                        SetEntryValue(entry, adProp, portalValue);
                        index++;
                    }
                }
            }
        }
        public static void DisableADObjectCustomProperties(DirectoryEntry entry, List<PropertyMapping> propertyMappings, int ADNameMaxLength, int ADsAMAccountNameMaxLength)
        {
            // entry.name
            var deletedEntryName = GetADObjectName(entry.Name).PrefixDeleted();
            var newName = string.Concat(GetADObjectPrefix(ADObjectType.User), deletedEntryName.MaximizeLength(ADNameMaxLength));
            entry.Rename(newName);

            // sAMAccountName
            entry.Properties[SyncConfiguration.GetUserNameProp(propertyMappings)].Value = deletedEntryName.MaximizeLength(ADsAMAccountNameMaxLength);

            foreach (PropertyMapping propMapping in propertyMappings)
            {
                foreach (SyncProperty adProp in propMapping.ADProperties)
                {
                    if (adProp.Unique)
                    {
                        var propValue = GetEntryValue(entry, adProp).PrefixDeleted();
                        SetEntryValue(entry, adProp, propValue);
                    }
                }
            }
        }
        public static string GetNodeValue(Node node, SyncProperty portalProp)
        {
            if (node.HasProperty(portalProp.Name))
            {
                var propValue = node[portalProp.Name];
                if (propValue == null)
                    return null;
                return propValue.ToString();
            }

            switch (portalProp.Name)
            {
                case "Name":
                    return node.Name;
                default:
                    // log: nincs ilyen property
                    return null;
            }
        }
        public static void SetNodeValue(Node node, SyncProperty portalProp, string value)
        {
            var propValue = value.MaximizeLength(portalProp.MaxLength);

            switch (portalProp.Name)
            {
                case "Name":
                    node.Name = propValue;
                    break;
                default:
                    if (node.HasProperty(portalProp.Name))
                        node[portalProp.Name] = propValue;
                    else
                    {
                        // log: nincs ilyen property
                    }
                    break;
            }
        }
        public static string GetEntryValue(DirectoryEntry entry, SyncProperty adProp)
        {
            var propValColl = entry.Properties[adProp.Name];

            if (propValColl == null)
                return string.Empty;

            string value = null;
            if (propValColl.Count >= 1)
            {
                value = propValColl[0] as string;
            }
            return value ?? string.Empty;
        }
        public static void SetEntryValue(DirectoryEntry entry, SyncProperty adProp, string value)
        {
            var propValue = value.MaximizeLength(adProp.MaxLength);
            var propValColl = entry.Properties[adProp.Name];

            // ha törlünk egy propertyt portálon, akkor AD-ban ne csak a valuet töröljük, hanem a propertyt magát
            // különben constraint errort kapunk
            // removeat: single-valued properties, clear: multi-valued properties
            if (string.IsNullOrEmpty(propValue))
            {
                // ha nincs besettelve AD-ban a property, és portálról sem nyúlunk hozzá, akkor ne csináljunk semmit
                if (propValColl.Count == 0)
                    return;
                // single valued property
                if (propValColl.Count == 1)
                {
                    propValColl.RemoveAt(0);
                    return;
                }
                // multi valued property
                propValColl.Clear();
                return;
            }

            // setting the value null clears the collection
            propValColl.Value = null;

            // setting the value to propValue adds propValue to the collection
            propValColl.Value = propValue;
        }
        public static IEnumerable<Node> GetContainerUsers(Node container)
        {
            var query = new NodeQuery();
            ExpressionList expressionList = new ExpressionList(ChainOperator.And);

            // nodetype
            TypeExpression typeExpression = new TypeExpression(Common.GetNodeType(ADObjectType.User));
            expressionList.Add(typeExpression);

            // from container as root
            StringExpression pathExpression = new StringExpression(StringAttribute.Path, StringOperator.StartsWith, container.Path);
            expressionList.Add(pathExpression);

            query.Add(expressionList);

            var result = query.Execute();
            return result.Nodes;
        }
        public static void ChangeToAdminAccount()
        {
            AccessProvider.Current.SetCurrentUser(User.Administrator);
        }
        public static void RestoreOriginalUser(IUser originalUser)
        {
            AccessProvider.Current.SetCurrentUser(originalUser);
        }
        public static string StripADName(string name)
        {
            // cserepesm: ad name may come in with ',' at the end ('ou=example,')
            name = name.Trim(',');

            var sb = new StringBuilder(name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                //if (!RepositoryPath.InvalidNameChars.Contains(name[i]))
                if (!RepositoryPath.IsInvalidNameChar(name[i]))
                    sb.Append(name[i]);
            }
            return sb.ToString().TrimEnd('.').Trim();  // leading and trailing whitespaces and trailing '.' is trimmed
        }


        /* ==================================================================================== VirtualUser helper methods */
        public static bool IsADAuthenticated(string adPath, string domain, string username, string pwd, string userNameProp)
        {
            if (string.IsNullOrEmpty(adPath))
                return false;

            string domainAndUsername = string.Concat(domain, @"\", username);

            DirectoryEntry entry = null;
            try
            {
                entry = new DirectoryEntry(adPath, domainAndUsername, pwd);

                // Bind to the native AdsObject to force authentication.
                var obj = entry.NativeObject;
                var search = new DirectorySearcher(entry);
                search.Filter = string.Format("({0}={1})", userNameProp, username);
                search.PropertiesToLoad.Add("cn");
                var result = search.FindOne();
                if (result == null)
                {
                    AdLog.LogErrorPortalObject("Could not find corresponding AD user", string.Concat(domain, "\\", username));
                    return false;
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
                return false;
            }
            finally
            {
                if (entry != null)
                    entry.Dispose();
            }
            return true;
        }
        public static bool IsADCustomAuthenticated(string adPath, string loginPropValue, string pwd, string loginProp, string customADAdminAccountName, string customADAdminAccountPwd)
        {
            if (string.IsNullOrEmpty(adPath))
                return false;

            DirectoryEntry searchRoot = null;
            try
            {
                searchRoot = new DirectoryEntry(adPath);
                searchRoot.AuthenticationType = AuthenticationTypes.None;

                if (!string.IsNullOrEmpty(customADAdminAccountName))
                {
                    searchRoot.AuthenticationType = AuthenticationTypes.ServerBind;
                    searchRoot.Username = customADAdminAccountName;
                    searchRoot.Password = customADAdminAccountPwd;
                }

                var objSearch = new DirectorySearcher(searchRoot);
                objSearch.SearchScope = SearchScope.Subtree;
                objSearch.Filter = string.Format("({0}={1})", loginProp, loginPropValue);
                var result = objSearch.FindAll();
                
                if (result.Count != 1)
                {
                    AdLog.LogErrorPortalObject("Could not find corresponding AD user", loginPropValue);
                    return false;
                }

                var userName = result[0].Path.Substring(adPath.Length + 1);

                searchRoot.AuthenticationType = AuthenticationTypes.ServerBind;
                searchRoot.Username = userName;
                searchRoot.Password = pwd;

                result = objSearch.FindAll();
                if (result.Count != 1)
                {
                    AdLog.LogErrorPortalObject("Could not find corresponding AD user", loginPropValue);
                    return false;
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
                return false;
            }
            finally
            {
                if (searchRoot != null)
                    searchRoot.Dispose();
            }
            return true;
        }
        public static bool SyncVirtualUserFromAD(string adPath, string username, User virtualUser, List<PropertyMapping> propertyMappings, string customADAdminAccountName, string customADAdminAccountPwd, bool novellSupport, string guidProp, bool syncUserName)
        {
            bool success = false;
            try
            {
                using (var serverEntry = ConnectToAD(adPath, customADAdminAccountName, customADAdminAccountPwd, novellSupport, guidProp))
                {
                    var filter = string.Format("({0}={1})", SyncConfiguration.GetUserNameProp(propertyMappings), username);
                    using (var entry = SearchADObject(serverEntry, filter, novellSupport, guidProp))
                    {
                        if (entry != null)
                        {
                            UpdatePortalUserCustomProperties(entry, virtualUser, propertyMappings, syncUserName);

                            var guid = Common.GetADObjectGuid(entry, guidProp);
                            if (guid.HasValue)
                            {
                                virtualUser["SyncGuid"] = ((Guid)guid).ToString();
                                success = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
            }
            return success;
        }
        public static bool SyncVirtualUserFromAD(string adPath, Guid guid, User virtualUser, List<PropertyMapping> propertyMappings, string customADAdminAccountName, string customADAdminAccountPwd, bool novellSupport, string guidProp, bool syncUserName)
        {
            bool success = false;
            try
            {
                using (var serverEntry = ConnectToAD(adPath, customADAdminAccountName, customADAdminAccountPwd, novellSupport, guidProp))
                {
                    var filter = string.Format("{0}={1}", guidProp, Common.Guid2OctetString(guid));
                    using (var entry = SearchADObject(serverEntry, filter, novellSupport, guidProp))
                    {
                        if (entry != null)
                        {
                            UpdatePortalUserCustomProperties(entry, virtualUser, propertyMappings, syncUserName);

                            virtualUser["SyncGuid"] = guid.ToString();
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
            }
            return success;
        }


        /* ==================================================================================== Helper methods */
        public static void SyncInitialUserProperties(User user)
        {
            var adPath = "WinNT://" + user.Domain + "/" + user.Name + ",user";

            try
            {
                using (var entry = ConnectToADSimple(adPath))
                {
                    if (entry != null)
                    {
                        var fn = entry.Properties["FullName"].Value.ToString();

                        user.FullName = string.IsNullOrEmpty(fn) ? user.Name : fn;
                        user.Save();
                    }
                }
            }
            catch(COMException ex)
            {
                SenseNet.Diagnostics.Logger.WriteException(ex);
            }
        }


        /* ==================================================================================== Extensions */
        public static string MaximizeLength(this string s, int max)
        {
            if ((s == null) || (max <= 0))
                return s;
            return s.Length > max ? s.Substring(0, max) : s;
        }
        public static string PrefixDeleted(this string s)
        {
            return string.Concat(DateTime.Now.ToString("yyMMddHHmm"), "_", s);
        }
    }
}
