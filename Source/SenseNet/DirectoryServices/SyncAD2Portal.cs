using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Security.ADSync;
using SenseNet.Search;

namespace SenseNet.DirectoryServices
{
    public class SyncAD2Portal
    {
        /* ==================================================================================== Members */
        // reference to configuration
        private AD2PortalConfiguration _config;

        private List<PropertyMapping> _propertyMappings;
        private List<SyncTree> _syncTrees;
        private string _deletedFromADPath;
        private Dictionary<string, int> _portalUsers;
        private Dictionary<string, int> _portalContainers;
        private Dictionary<string, int> _portalGroups;
        private bool _useOnTheFlyMemberQuery;

        /* ==================================================================================== AD -> portal : Methods */

        // pl.: DC=Nativ,DC=Local   -->   NATIV
        // ha a configban DC=Nativ,DC=local --> /Root/IMS/NATIV
        private string GetADDomainName(DirectoryEntry entry)
        {
            var ADDomainName = entry.Properties["distinguishedName"][0] as string;
            //return name.Replace(",DC=", ".").Substring(3);

            foreach (SyncTree syncTree in _syncTrees)
            {
                // ez a synctree tartalmazza az adott domain összerendelést
                if (syncTree.ADPath.EndsWith(ADDomainName))
                {
                    bool IMSfound = false;
                    foreach (string pathPart in syncTree.PortalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (IMSfound)
                            return pathPart;
                        if (pathPart == "IMS")
                            IMSfound = true;
                    }
                }
            }
            return null;
        }

        // domain, orgunit, container (folder)
        private void CreateNewPortalContainer(DirectoryEntry entry, string parentPath, Guid guid, SyncTree syncTree)
        {
            switch (Common.GetADObjectType(entry, _config.NovellSupport))
            {
                case ADObjectType.Organization:
                case ADObjectType.OrgUnit:
                    CreateNewPortalOrgUnit(entry, parentPath, (Guid)guid, syncTree);
                    break;
                case ADObjectType.Container:
                    CreateNewPortalFolder(entry, parentPath, (Guid)guid, syncTree);
                    break;
                case ADObjectType.Domain:
                    CreateNewPortalDomain(entry, parentPath, (Guid)guid, syncTree);
                    break;
                default:
                    AdLog.LogErrorADObject("Unsupported AD object!", entry.Path);
                    break;
            }
        }
        private void CreateNewPortalDomain(DirectoryEntry entry, string parentPath, Guid guid, SyncTree syncTree)
        {
            try
            {
                AdLog.LogADObject(string.Format("New portal domain - creating under {0}", parentPath), entry.Path);
                Domain newNode = new Domain(Node.LoadNode(parentPath));

                UpdatePortalDomainProperties(entry, newNode, syncTree);

                Common.UpdateLastSync(newNode, guid);
                //newNode.Save();  - update lastsync already saves node
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
            }
        }
        private void CreateNewPortalFolder(DirectoryEntry entry, string parentPath, Guid guid, SyncTree syncTree)
        {
            try
            {
                AdLog.LogADObject(string.Format("New portal folder - creating under {0}", parentPath), entry.Path);
                //Folder newNode = new Folder(Node.LoadNode(parentPath));
                //Node newNode = new GenericContent(Node.LoadNode(parentPath), "ADFolder");
                var newNode = new ADFolder(Node.LoadNode(parentPath));

                UpdatePortalFolderProperties(entry, newNode, syncTree);

                Common.UpdateLastSync(newNode, guid);
                //newNode.Save();  - update lastsync already saves node

                if (!_portalContainers.ContainsKey(guid.ToString()))
                    _portalContainers.Add(guid.ToString(), newNode.Id);
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
            }
        }
        private void CreateNewPortalOrgUnit(DirectoryEntry entry, string parentPath, Guid guid, SyncTree syncTree)
        {
            try
            {
                AdLog.LogADObject(string.Format("New portal orgunit - creating under {0}", parentPath), entry.Path);
                OrganizationalUnit newOu = new OrganizationalUnit(Node.LoadNode(parentPath));

                UpdatePortalOrgUnitProperties(entry, newOu, syncTree);

                Common.UpdateLastSync(newOu, guid);
                //newOu.Save(); - update lastsync already saves node

                if (_portalContainers != null)
                {
                    if (!_portalContainers.ContainsKey(guid.ToString()))
                        _portalContainers.Add(guid.ToString(), newOu.Id);
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
            }
        }
        private void CreateNewPortalUser(DirectoryEntry entry, string parentPath, Guid guid, SyncTree syncTree)
        {
            try
            {
                AdLog.LogADObject(string.Format("New portal user - creating under {0}", parentPath), entry.Path);
                var newUser = new User(Node.LoadNode(parentPath), _config.UserType);

                // user actions
                UpdatePortalUserProperties(entry, newUser, syncTree);

                Common.UpdateLastSync(newUser, guid);
                //newUser.Save(); - update lastsync already saves node

                if (_portalUsers != null)
                {
                    if (!_portalUsers.ContainsKey(guid.ToString()))
                        _portalUsers.Add(guid.ToString(), newUser.Id);
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
            }
        }
        private void CreateNewPortalGroup(DirectoryEntry entry, string parentPath, Guid guid, SyncTree syncTree)
        {
            try
            {
                AdLog.LogADObject(string.Format("New portal group - creating under {0}", parentPath), entry.Path);
                var newGroup = new Group(Node.LoadNode(parentPath));

                UpdatePortalGroupProperties(entry, newGroup, syncTree);

                Common.UpdateLastSync(newGroup, guid);
                //newGroup.Save(); - update lastsync already saves node

                if (_portalGroups != null)
                {
                    if (!_portalGroups.ContainsKey(guid.ToString()))
                        _portalGroups.Add(guid.ToString(), newGroup.Id);
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
            }
        }

        // domain, orgunit, container (folder)
        private void UpdatePortalContainerProperties(DirectoryEntry entry, Node node, SyncTree syncTree)
        {
            switch (Common.GetADObjectType(entry, _config.NovellSupport))
            {
                case ADObjectType.OrgUnit:
                    UpdatePortalOrgUnitProperties(entry, node, syncTree);
                    break;
                case ADObjectType.Container:
                    UpdatePortalFolderProperties(entry, node, syncTree);
                    break;
                case ADObjectType.Domain:
                    UpdatePortalDomainProperties(entry, node, syncTree);
                    break;
            }
        }
        private void UpdatePortalOrgUnitProperties(DirectoryEntry entry, Node node, SyncTree syncTree)
        {
            AdLog.LogObjects("Updating portal orgunit properties", entry.Path, node.Path);

            node.Name = Common.GetADObjectName(entry.Name);
            // node.Save() nem kell, később mentődik
        }
        private void UpdatePortalDomainProperties(DirectoryEntry entry, Node node, SyncTree syncTree)
        {
            AdLog.LogObjects("Updating portal domain properties", entry.Path, node.Path);

            node.Name = GetADDomainName(entry);
            // node.Save() nem kell, később mentődik
        }
        private void UpdatePortalFolderProperties(DirectoryEntry entry, Node node, SyncTree syncTree)
        {
            AdLog.LogObjects("Updating portal folder properties", entry.Path, node.Path);

            node.Name = Common.GetADObjectName(entry.Name);
            // node.Save() nem kell, később mentődik
        }
        private void UpdatePortalUserProperties(DirectoryEntry entry, Node node, SyncTree syncTree)
        {
            AdLog.LogObjects("Updating portal user properties", entry.Path, node.Path);

            var user = (IUser)node;

            if (_config.SyncEnabledState)
                user.Enabled = !Common.IsAccountDisabled(entry, _config.NovellSupport);

            Common.UpdatePortalUserCustomProperties(entry, node, _propertyMappings, _config.SyncUserName);

            // node.Save() nem kell, később mentődik
        }
        private void UpdatePortalGroupProperties(DirectoryEntry entry, Node node, SyncTree syncTree)
        {
            AdLog.LogObjects("Updating portal group properties", entry.Path, node.Path);

            node.Name = Common.GetADObjectName(entry.Name);

            // set members
            var group = (Group)node;
            var portalMembers = group.Members;

            var adMembers = GetADGroupMembers(entry, syncTree);
            var removeMembers = new List<Node>();

            // add new members: 
            foreach (Guid guid in adMembers.Keys)
            {
                try
                {
                    //bool validResult;

                    //Node portalNode = GetNodeByGuid(guid, adMembers[guid].objType, out validResult);
                    //string adPath = adMembers[guid].Path;

                    //var portalNodePath = syncTree.GetPortalPath(adPath);
                    //portalNodePath = portalNodePath.Substring(0, portalNodePath.LastIndexOf('/'));
                    //portalNodePath = RepositoryPath.Combine(portalNodePath, adMembers[guid].SamAccountName);
                    //Node portalNode = Node.Load<Node>(portalNodePath);

                    Node portalNode = null;
                    string guidStr = guid.ToString();

                    if (_useOnTheFlyMemberQuery)
                    {
                        portalNode = Common.GetPortalObjectByGuid(guid);
                    }
                    else
                    {
                        switch (adMembers[guid].objType)
                        {
                            case ADObjectType.User:
                                portalNode = (_portalUsers.ContainsKey(guidStr)) ? Node.LoadNode(_portalUsers[guidStr]) : null;
                                break;
                            case ADObjectType.Group:
                                portalNode = (_portalGroups.ContainsKey(guidStr)) ? Node.LoadNode(_portalGroups[guidStr]) : null;
                                break;
                            default:
                                break;
                        }
                    }

                    if (portalNode != null)
                    {
                        if (!portalMembers.Any(n => n.Id == portalNode.Id))
                        {
                            switch (adMembers[guid].objType)
                            {
                                case ADObjectType.Group:
                                    group.AddMember((IGroup)portalNode);
                                    break;
                                case ADObjectType.User:
                                    group.AddMember((IUser)portalNode);
                                    break;
                                default:
                                    // log: AD group membere se nem user, se nem group
                                    AdLog.LogErrorObjects("Member is neither a user nor a group", adMembers[guid].Path, portalNode.Path);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // log: a group a portálon nem létező user-t tartalmaz
                        // a synctree-k elvileg tartalmazzák, mert a GetADGroupMembers csak synctree által tartalmazott objektumokat ad vissza
                        AdLog.LogErrorADObject("Member does not exist in portal", adMembers[guid].Path);
                    }
                }
                catch
                {
                    AdLog.LogErrorADObject("Could not add member to group", adMembers[guid].Path);
                }
            }

            // remove old members
            // add nodes of portal group members to removeMembers list, 
            // that have no corresponding AD objects in AD group
            foreach (Node member in portalMembers)
            {
                string guidStr = member["SyncGuid"] as string;
                if (guidStr != null)
                {
                    if (!adMembers.Keys.Contains(new Guid(guidStr)))
                        removeMembers.Add(member);
                }
                else
                {
                    // log: a portál csoport szinkronizálatlan objektumot is tartalmaz
                    AdLog.LogError(string.Format("Portal group contains unsynchronized object (group: {0}, object: {1}", group.Path, member.Path));
                }
            }

            // remove members from portal group
            foreach (Node member in removeMembers)
            {
                var portalUser = member as IUser;
                if (portalUser != null)
                    group.RemoveMember(portalUser);
                else
                {
                    var portalGroup = member as IGroup;
                    if (portalGroup != null)
                        group.RemoveMember(portalGroup);
                }
            }
            // node.Save() nem kell, később mentődik
        }

        // domain, orgunit, container (folder)
        private void DeletePortalContainer(Node node)
        {
            try
            {
                AdLog.LogPortalObject("Deleting portal container (orgunit/domain/folder)", node.Path);

                if (Node.Exists(node.Path))
                {
                    // move all underlying users to deleted folder
                    var users = Common.GetContainerUsers(node);

                    // delete user nodes
                    foreach (Node userNode in users)
                    {
                        DeletePortalUser(userNode);
                    }

                    // delete container node if allowed
                    if (Common.GetContainerUsers(node).Count() == 0)
                        Node.DeletePhysical(node.Id);
                    else
                        AdLog.LogErrorPortalObject("Portal container cannot be deleted, it contains users!", node.Path);
                }
            }
            catch (Exception ex)
            {
                AdLog.LogErrorADObject(ex.Message, node.Path);
            }
        }
        private void DeletePortalUser(Node node)
        {
            try
            {
                AdLog.LogPortalObject("Deleting (disabling) portal user", node.Path);

                ((IUser)node).Enabled = false;
                Common.DisablePortalUserCustomProperties(node, _propertyMappings);
                Common.UpdateLastSync(node, null);
                //node.Save(); - update lastsync already saves node

                Node.Move(node.Path, _deletedFromADPath);
            }
            catch (Exception ex)
            {
                AdLog.LogErrorADObject(ex.Message, node.Path);
            }
        }
        private void DeletePortalGroup(Node node)
        {
            try
            {
                AdLog.LogPortalObject("Deleting portal group", node.Path);

                node.Delete();
            }
            catch (Exception ex)
            {
                AdLog.LogErrorADObject(ex.Message, node.Path);
            }
        }

        // gets members of an AD group and returns the corresponding list of <Guid, ADObjectType> objects
        private Dictionary<Guid, ADGroupMember> GetADGroupMembers(DirectoryEntry group, SyncTree syncTree)
        {
            var members = new Dictionary<Guid, ADGroupMember>();
            var memberCount = group.Properties["member"].Count;
            AdLog.LogADObject(string.Format("Group contains {0} member(s).", memberCount), group.Path);
            for (int i = 0; i < memberCount; i++)
            {
                string sMemberDN = group.Properties["member"][i].ToString();

                var objSyncTree = GetSyncTreeForObject(sMemberDN);
                if (objSyncTree == null)
                {
                    AdLog.LogWarning(string.Format("AD group contains an object that is not contained in any of the synctrees, group's synctree will be used to retrieve the object (group: {0}, object: {1})", group.Path, sMemberDN));
                    objSyncTree = syncTree;
                }

                using (DirectoryEntry oADMember = objSyncTree.ConnectToObject(sMemberDN))
                {
                    if (oADMember != null)
                    {
                        var guid = Common.GetADObjectGuid(oADMember, _config.GuidProp);
                        if (guid != null)
                        {
                            var userNameProp = oADMember.Properties[_config.UserNameProp];
                            var userNameValue = userNameProp == null ? null : userNameProp.Value;
                            if (userNameValue == null)
                            {
                                AdLog.LogError(string.Format("Property {0} of AD group member \"{1}\" is missing or value is null", _config.UserNameProp, sMemberDN));
                                continue;
                            }

                            members.Add(
                                ((Guid) guid),
                                new ADGroupMember()
                                    {
                                        objType = Common.GetADObjectType(oADMember, _config.NovellSupport),
                                        Path = oADMember.Path,
                                        SamAccountName = userNameValue.ToString()
                                    });
                        }
                    }
                    else
                    {
                        AdLog.LogWarning(string.Format("AD group member could not be retrieved (group: {0}, object: {1})", group.Path, sMemberDN));
                    }
                }
            }
            return members;
        }
        // checks if the AD object corresponding to the given portal guid exists under synchronized path - if not, it should be deleted from portal...
        private bool ADObjectPathSynced(Guid guid, SearchResultCollection ADObjects, Node node)
        {
            bool exists = false;
            foreach (SearchResult result in ADObjects)
            {
                if (Common.GetADResultGuid(result, _config.GuidProp) == guid)
                {
                    var nodeADpath = result.Path;
                    AdLog.Log(string.Format("AD object for portal object {0} (guid {1}) found ({2}), checking synctrees", node.Path, guid.ToString(), nodeADpath));

                    foreach (SyncTree syncTree in _syncTrees)
                    {
                        if (syncTree.ContainsADPath(nodeADpath))
                            return true;
                    }
                    AdLog.Log(string.Format("No corresponding synctree for AD object ({0}) found, object should be deleted", nodeADpath));
                }
            }
            return exists;
        }
        // returns synchronized portal nodes
        private IEnumerable<Node> GetAllPortalObjects(ADObjectType objType, SyncTree syncTree)
        {
            var typeText = string.Empty;
            if (objType == ADObjectType.AllContainers)
            {
                typeText = string.Concat(
                    "(TypeIs:", Common.GetNodeType(ADObjectType.OrgUnit).Name, 
                    " OR TypeIs:", Common.GetNodeType(ADObjectType.Container).Name, 
                    " OR TypeIs:", Common.GetNodeType(ADObjectType.Domain).Name, ")");
            }
            else
            {
                typeText = string.Concat("TypeIs:", Common.GetNodeType(objType).Name);
            }

            var startPath = string.Concat('"', syncTree.PortalPath.TrimEnd(new char[] { '/' }), '"');
            var queryText = string.Concat(typeText, " AND InTree:", startPath);

            var settings = new QuerySettings {EnableAutofilters = false, EnableLifespanFilter = false};
            var query = ContentQuery.CreateQuery(queryText, settings);
            var result = query.Execute();
            return result.Nodes;
        }
        private static Dictionary<string, int> GetAllPortalObjects(ADObjectType objType)
        {
            var typeText = string.Empty;
            if (objType == ADObjectType.AllContainers)
            {
                typeText = string.Concat(
                    "(TypeIs:", Common.GetNodeType(ADObjectType.OrgUnit).Name,
                    " OR TypeIs:", Common.GetNodeType(ADObjectType.Container).Name,
                    " OR TypeIs:", Common.GetNodeType(ADObjectType.Domain).Name, ")");
            }
            else
            {
                typeText = string.Concat("TypeIs:", Common.GetNodeType(objType).Name);
            }

            var queryText = string.Concat(typeText, " AND InTree:/Root/IMS");

            var settings = new QuerySettings { EnableAutofilters = false, EnableLifespanFilter = false };
            var query = ContentQuery.CreateQuery(queryText, settings);
            var result = query.Execute();

            var guidIdList = (from node in result.Nodes
                              where !string.IsNullOrEmpty(node.GetProperty<string>("SyncGuid"))
                              select new {Guid = node.GetProperty<string>("SyncGuid").ToLower(), ID = node.Id});

            return guidIdList.ToDictionary(a => a.Guid, a => a.ID);
        }
        // adpath: OU=OtherOrg,OU=ExampleOrg,DC=Nativ,DC=local
        // portalParentPath: "/Root/IMS/Nativ.Local/ExampleOrg"
        private void EnsurePortalPath(SyncTree syncTree, string ADPath, string portalParentPath)
        {
            // portalParentPath does not exist
            if (!Node.Exists(portalParentPath))
            {
                // get parent AD object
                string ADparentPath = syncTree.GetADParentObjectPath(ADPath);
                // ensurepath
                EnsurePortalPath(syncTree, ADparentPath, RepositoryPath.GetParentPath(portalParentPath));
            }

            // portalParentPath exists, so AD object should be synchronized here
            // domain, container, orgunit
            using (DirectoryEntry entry = syncTree.ConnectToObject(ADPath))
            {
                var guid = Common.GetADObjectGuid(entry, _config.GuidProp);
                if (!guid.HasValue)
                    return;

                SyncOneADObject(null, entry, (Guid)guid, ADObjectType.AllContainers, portalParentPath, CreateNewPortalContainer, UpdatePortalContainerProperties, syncTree);
            }
        }
        // connect to the appropriate synctree AD corresponding to the domain of the requested object
        private SyncTree GetSyncTreeForObject(string objectPath)
        {
            foreach (SyncTree syncTree in _syncTrees)
            {
                if (syncTree.ContainsADPath(objectPath))
                    return syncTree;
            }
            return null;
        }
        private void SyncSingleObjectFromAD(string ldapPath)
        {
            SyncTree syncTree = null;
            DirectoryEntry entry = null;
            foreach (SyncTree sTree in _syncTrees)
            {
                if (sTree.ContainsADPath(ldapPath))
                {
                    entry = sTree.ConnectToObject(ldapPath);
                    syncTree = sTree;
                }
            }

            if (syncTree == null)
            {
                AdLog.LogErrorADObject("Configured SyncTree could not be found for this path", ldapPath);
                return;
            }

            string nodePortalParentPath = syncTree.GetPortalParentPath(ldapPath);
            if (!Node.Exists(nodePortalParentPath))
            {
                AdLog.LogErrorADObject(string.Format("Portal parent path ({0}) does not exist", nodePortalParentPath), ldapPath);
                return;
            }

            if (entry == null)
            {
                AdLog.LogErrorADObject("AD Entry is not found", ldapPath);
                return;
            }

            var guid = Common.GetADObjectGuid(entry, _config.GuidProp);
            if (!guid.HasValue)
            {
                AdLog.LogErrorADObject("AD Entry guid cannot be retrieved", ldapPath);
                return;
            }

            var adObjectType = Common.GetADObjectType(entry, false);
            Action<DirectoryEntry, string, Guid, SyncTree> CreateNewObject = null;
            Action<DirectoryEntry, Node, SyncTree> UpdateProperties = null;
            switch (adObjectType) 
            {
                case ADObjectType.User:
                    CreateNewObject = CreateNewPortalUser;
                    UpdateProperties = UpdatePortalUserProperties;
                    break;
                case ADObjectType.Group:
                    CreateNewObject = CreateNewPortalGroup;
                    UpdateProperties = UpdatePortalGroupProperties;
                    break;
                case ADObjectType.Container:
                case ADObjectType.Organization:
                case ADObjectType.OrgUnit:
                    CreateNewObject = CreateNewPortalContainer;
                    UpdateProperties = UpdatePortalContainerProperties;
                    break;
                default:
                    AdLog.LogErrorADObject("Syncing of this type is not supported.", ldapPath);
                    return;
            }

            // check if node already exists:
            var node = Common.GetPortalObjectByGuid(guid.Value);
            if (node == null)
            {
                if (!Node.Exists(nodePortalParentPath))
                    EnsurePortalPath(syncTree, syncTree.GetADParentObjectPath(ldapPath), RepositoryPath.GetParentPath(nodePortalParentPath));

                CreateNewObject(entry, nodePortalParentPath, guid.Value, syncTree);
            }
            else
            {
                if (RepositoryPath.GetParentPath(node.Path) != nodePortalParentPath)
                {
                    Node.Move(node.Path, nodePortalParentPath);

                    // reload node for further processing (set properties)
                    node = Node.LoadNode(node.Id);
                }

                UpdateProperties(entry, node, syncTree);
                Common.UpdateLastSync(node, null);
            }
        }


        /* ==================================================================================== AD -> portal : Main algorithms */
        // sync one object
        // két helyről hívhatjuk:
        // - SyncObjectsFromAD --> innen SearchResult objektumot kapunk
        // - SyncObjectsFromAD/EnsurePath --> innen Entryt kapunk
        //      - utóbbiból helyes működésnél csak létre kell hozni új objektumot, de ha már létezik az objektum, akkor
        //        moveoljuk, ne keletkezzen két azonos GUID-ú objektum a portálon
        private void SyncOneADObject(SearchResult result, DirectoryEntry ADentry,
            Guid guid,
            ADObjectType objType,
            string nodePortalParentPath,
            Action<DirectoryEntry, string, Guid, SyncTree> CreateNewObject,
            Action<DirectoryEntry, Node, SyncTree> UpdateProperties,
            SyncTree syncTree)
        {
            //bool validResult;
            //var node = GetNodeByGuid(guid, objType, out validResult);
            Node node = null;
            string guidStr = guid.ToString();
            switch (objType)
            {
                case ADObjectType.AllContainers:
                    node = (_portalContainers.ContainsKey(guidStr)) ? Node.LoadNode(_portalContainers[guidStr]) : null;
                    break;
                case ADObjectType.User:
                    node = (_portalUsers.ContainsKey(guidStr)) ? Node.LoadNode(_portalUsers[guidStr]) : null;
                    break;
                case ADObjectType.Group:
                    node = (_portalGroups.ContainsKey(guidStr)) ? Node.LoadNode(_portalGroups[guidStr]) : null;
                    break;
                default:
                    break;
            }
            if (node != null)
            {
                // existing portal object
                try
                {
                    bool isNodeSynced = false;

                    // check path, move object if necessary
                    if (RepositoryPath.GetParentPath(node.Path) != nodePortalParentPath)
                    {
                        AdLog.LogADObject(string.Format("Moving object from {0} to {1}", node.Path, nodePortalParentPath), result.Path);
                        Node.Move(node.Path, nodePortalParentPath);

                        // reload node for further processing (set properties)
                        node = Node.LoadNode(node.Id);
                        isNodeSynced = true;
                    }

                    if (ADentry != null)
                    {
                        // ensurepath-ból jön, mindenképp szinkronizáljuk
                        UpdateProperties(ADentry, node, syncTree);
                        AdLog.LogADObject(String.Format("Saving synced portal object: {0}", node.Path), ADentry.Path);
                        Common.UpdateLastSync(node, null);
                        //node.Save(); - update lastsync already saves node
                    }
                    else
                    {
                        // syncobjectsből jövünk, csak resultunk van (entrynk nincs)

                        // set properties and lastsync date - csak akkor szinkronizálunk, ha lastmod > x 
                        // (ha az objektum át lett mozgatva, a lastmod is változik AD-ben)
                        if (_config.AlwaysSyncObjects || Common.IsPortalObjectInvalid(node, result, _config.NovellSupport))
                        {
                            using (var entry = result.GetDirectoryEntry())
                            {
                                UpdateProperties(entry, node, syncTree);
                                isNodeSynced = true;
                            }
                        }

                        if (isNodeSynced)
                        {
                            AdLog.LogADObject(String.Format("Saving synced portal object: {0}", node.Path), result.Path);
                            Common.UpdateLastSync(node, null);
                            //node.Save(); - update lastsync already saves node
                        }
                    }
                }
                catch (Exception ex)
                {
                    AdLog.LogException(ex);
                    // log: adott objektum szinkronizálása nem sikerült
                    if (result != null)
                        AdLog.LogErrorADObject("Syncing of AD object not successful.", result.Path);
                }
            }
            else
            {
                if (ADentry != null)
                {
                    // ensurepath-ból jövünk
                    CreateNewObject(ADentry, nodePortalParentPath, guid, syncTree);
                }
                else
                {
                    // syncobjectsből jövünk, csak resultunk van
                    // new portal object
                    using (var entry = result.GetDirectoryEntry())
                    {
                        CreateNewObject(entry, nodePortalParentPath, guid, syncTree);
                    }
                }
            }
        }

        // sync objects from AD to portal
        private void SyncObjectsFromAD(SyncTree syncTree,
            ADObjectType objType,
            SearchResultCollection allADObjects,
            Action<DirectoryEntry, string, Guid, SyncTree> CreateNewObject,
            Action<DirectoryEntry, Node, SyncTree> UpdateProperties)
        {
            foreach (SearchResult result in allADObjects)
            {
                try
                {
                    string nodeADpath = result.Path;

                    if (syncTree.IsADPathExcluded(nodeADpath))
                        continue;

                    AdLog.LogOuterADObject("Syncing", result.Path);

                    var guid = Common.GetADResultGuid(result, _config.GuidProp);

                    if (!guid.HasValue)
                    {
                        // no AD guid present for object
                        AdLog.LogErrorADObject("No AD GUID present", result.Path);
                        continue;
                    }

                    // új objektumok (ou, user, group) felvétele, átmozgatások
                    // - ha létezik az adott guid-ú objektum -> path ellenőrzés, átmozgatás
                    // - ha nem létezik, létrehozás

                    string nodePortalParentPath = syncTree.GetPortalParentPath(nodeADpath);
                    if (!Node.Exists(nodePortalParentPath))
                    {
                        // adpath: OU=OtherOrg,OU=ExampleOrg,DC=Nativ,DC=local
                        // portalParentPath: "/Root/IMS/NATIV/ExampleOrg"
                        EnsurePortalPath(syncTree, syncTree.GetADParentObjectPath(result.Path), RepositoryPath.GetParentPath(nodePortalParentPath));
                    }

                    SyncOneADObject(result, null,
                        (Guid)guid,
                        objType,
                        nodePortalParentPath,
                        CreateNewObject,
                        UpdateProperties,
                        syncTree);
                }
                catch (Exception ex)
                {
                    // syncing of one object of the current tree failed
                    AdLog.LogException(ex);
                }
            }
        }

        // delete portal objects that have no corresponding synchronized objects in AD
        private void DeleteObjectsFromAD(SyncTree syncTree,
            ADObjectType objType,
            SearchResultCollection allADObjects,
            Action<Node> DeletePortalObject)
        {
            try
            {
                AdLog.LogOuter("Querying all portal objects...");
                var portalNodes = GetAllPortalObjects(objType, syncTree);
                AdLog.LogOuter("Checking if portal objects exist under synchronized path in AD...");
                foreach (Node node in portalNodes)
                {
                    try
                    {
                        // check if object exists under synchronized path in AD
                        var guid = Common.GetPortalObjectGuid(node);
                        if ((!guid.HasValue) || (!ADObjectPathSynced((Guid)guid, allADObjects, node)))
                        {
                            if (!guid.HasValue)
                                AdLog.Log(string.Format("No guid set for portal object: {0} ", node.Path));

                            // deleted from AD or not under synchronized path any more
                            DeletePortalObject(node);
                        }
                    }
                    catch (Exception ex)
                    {
                        AdLog.LogException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
            }
        }

        private void SyncContainersFromAD(SyncTree syncTree)
        {
            AdLog.LogMainActivity("Syncing containers (domains, orgunits, containers)", syncTree.ADPath, syncTree.PortalPath);
            SyncObjectsFromAD(syncTree,
                ADObjectType.AllContainers,
                syncTree.AllADContainers,
                CreateNewPortalContainer,
                UpdatePortalContainerProperties);
        }
        private void SyncUsersFromAD(SyncTree syncTree)
        {
            AdLog.LogMainActivity("Syncing users", syncTree.ADPath, syncTree.PortalPath);
            SyncObjectsFromAD(syncTree,
                ADObjectType.User,
                syncTree.AllADUsers,
                CreateNewPortalUser,
                UpdatePortalUserProperties);
        }
        private void SyncGroupsFromAD(SyncTree syncTree)
        {
            AdLog.LogMainActivity("Syncing groups", syncTree.ADPath, syncTree.PortalPath);
            SyncObjectsFromAD(syncTree,
                ADObjectType.Group,
                syncTree.AllADGroups,
                CreateNewPortalGroup,
                UpdatePortalGroupProperties);
        }
        private void DeletePortalUsers(SyncTree syncTree)
        {
            AdLog.LogMainActivity("Deleting portal users", syncTree.ADPath, syncTree.PortalPath);
            DeleteObjectsFromAD(syncTree,
                ADObjectType.User,
                syncTree.AllADUsers,
                DeletePortalUser);
        }
        private void DeletePortalGroups(SyncTree syncTree)
        {
            AdLog.LogMainActivity("Deleting portal groups", syncTree.ADPath, syncTree.PortalPath);
            DeleteObjectsFromAD(syncTree,
                ADObjectType.Group,
                syncTree.AllADGroups,
                DeletePortalGroup);
        }
        private void DeletePortalContainers(SyncTree syncTree)
        {
            AdLog.LogMainActivity("Deleting portal containers (domains, orgunits, containers)", syncTree.ADPath, syncTree.PortalPath);
            DeleteObjectsFromAD(syncTree,
                ADObjectType.AllContainers,
                syncTree.AllADContainers,
                DeletePortalContainer);
        }


        /* ==================================================================================== AD -> portal : Public methods */
        /// <summary>
        /// Syncs all objects of all configured sync trees from Active Directory(ies).
        /// </summary>
        public void SyncFromAD()
        {
            IUser originalUser = User.Current;
            Common.ChangeToAdminAccount();


            // init portal objects
            AdLog.LogMain("Cacheing portal users...");
            _portalUsers = GetAllPortalObjects(ADObjectType.User);
            AdLog.LogMain("Cacheing portal groups...");
            _portalGroups = GetAllPortalObjects(ADObjectType.Group);
            AdLog.LogMain("Cacheing portal containers...");
            _portalContainers = GetAllPortalObjects(ADObjectType.AllContainers);


            foreach (SyncTree syncTree in _syncTrees)
            {
                try
                {
                    SyncContainersFromAD(syncTree);
                }
                catch (Exception ex)
                {
                    // syncing of the whole tree failed
                    AdLog.LogException(ex);
                }
            }
            foreach (SyncTree syncTree in _syncTrees)
            {
                try
                {
                    SyncUsersFromAD(syncTree);
                }
                catch (Exception ex)
                {
                    // syncing of the whole tree failed
                    AdLog.LogException(ex);
                }
            }
            foreach (SyncTree syncTree in _syncTrees)
            {
                try
                {
                    if (syncTree.SyncGroups)
                    {
                        SyncGroupsFromAD(syncTree);
                    }
                    else
                    {
                        AdLog.LogMainActivity("Groups under synctree are skipped", syncTree.ADPath, syncTree.PortalPath);
                    }
                }
                catch (Exception ex)
                {
                    // syncing of the whole tree failed
                    AdLog.LogException(ex);
                }
            }
            foreach (SyncTree syncTree in _syncTrees)
            {
                try
                {
                    DeletePortalUsers(syncTree);
                }
                catch (Exception ex)
                {
                    // syncing of the whole tree failed
                    AdLog.LogException(ex);
                }
            }
            foreach (SyncTree syncTree in _syncTrees)
            {
                try
                {
                    DeletePortalGroups(syncTree);
                }
                catch (Exception ex)
                {
                    // syncing of the whole tree failed
                    AdLog.LogException(ex);
                }
            }
            foreach (SyncTree syncTree in _syncTrees)
            {
                try
                {
                    DeletePortalContainers(syncTree);
                }
                catch (Exception ex)
                {
                    // syncing of the whole tree failed
                    AdLog.LogException(ex);
                }
            }

            // dispose synctrees (searchresultcollection objects contained in synctree)
            foreach (SyncTree syncTree in _syncTrees)
            {
                syncTree.Dispose();
            }

            AdLog.EndLog();

            Common.RestoreOriginalUser(originalUser);
        }
        /// <summary>
        /// Syncs a single object from Active directory according to configuration.
        /// </summary>
        /// <param name="ldapPath">The LDAP path of the object, ie. CN=MyGroup,OU=MyOrg,DC=Nativ,DC=local</param>
        public void SyncObjectFromAD(string ldapPath)
        {
            // we haven't cached portal users and groups, so we will query for group members
            _useOnTheFlyMemberQuery = true;

            SyncSingleObjectFromAD(ldapPath);

            _useOnTheFlyMemberQuery = false;
        }
        /// <summary>
        /// Gets portal info for given LDAP path. If resulting SyncInfo object's SyncTreeFound is false, no other properties are filled.
        /// </summary>
        /// <param name="ldapPath">The LDAP path of the object, ie. CN=MyGroup,OU=MyOrg,DC=Nativ,DC=local</param>
        /// <returns></returns>
        public SyncInfo GetSyncInfo(string ldapPath)
        {
            SyncInfo result = new SyncInfo();

            SyncTree syncTree = null;
            foreach (SyncTree sTree in _syncTrees)
            {
                if (sTree.ContainsADPath(ldapPath))
                {
                    syncTree = sTree;
                }
            }

            if (syncTree == null)
                return result;
            
            result.SyncTreeFound = true;
            result.SyncTreeADPath = syncTree.ADPath;
            result.SyncTreePortalPath = syncTree.PortalPath;
            result.SyncTreeADIPAddress = syncTree.IPAddress;
            result.TargetPortalPath = syncTree.GetPortalPath(ldapPath);
            result.PortalNodeExists = string.IsNullOrEmpty(result.TargetPortalPath) ? false : Node.Exists(result.TargetPortalPath);
            var parentPath = syncTree.GetPortalParentPath(ldapPath);
            result.PortalParentExists = string.IsNullOrEmpty(parentPath) ? false : Node.Exists(parentPath);

            return result;
        }


        /* ==================================================================================== Constructor */
        public SyncAD2Portal()
        {
            _config = AD2PortalConfiguration.Current;
            _syncTrees = _config.GetSyncTrees();
            _propertyMappings = _config.GetPropertyMappings();
            _deletedFromADPath = _config.DeletedFromADPath;
        }
    }
}
