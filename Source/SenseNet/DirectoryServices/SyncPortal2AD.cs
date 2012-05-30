using System;
using System.Collections.Generic;
using SenseNet.ContentRepository;
using System.DirectoryServices;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.DirectoryServices
{
    public class SyncPortal2AD
    {
        /* ==================================================================================== Members */
        // reference to configuration
        private Portal2ADConfiguration _config;

        private List<PropertyMapping> _propertyMappings;
        private List<SyncTree> _syncTrees;
        private bool _createdUsersDisabled;

        public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public struct SyncTreeADObject
        {
            public SyncTree syncTree;
            public DirectoryEntry entry;
        }

        /* ==================================================================================== portal -> AD : Methods */
        private SyncTreeADObject GetADObjectByGuid(Guid guid)
        {
            foreach (SyncTree syncTree in _syncTrees)
            {
                var entry = syncTree.GetADObjectByGuid((Guid)guid);
                if (entry != null)
                    return new SyncTreeADObject() { entry = entry, syncTree = syncTree };
            }
            return new SyncTreeADObject();
        }
        private SyncTree GetSyncTreeContainingPortalPath(string path)
        {
            // get containing synctree
            foreach (SyncTree syncTree in _syncTrees)
            {
                if (syncTree.ContainsPortalPath(path))
                    return syncTree;
            }
            return null;
        }
        private void MoveADObjectIfNecessary(DirectoryEntry entry, SyncTree entrySyncTree, Node node, string newPath)
        {
            if (!_config.AllowMove)
                return;

            // ebben a synctree-ben van
            var oldSyncTree = entrySyncTree;

            // ebben a synctree-ben kell lennie
            var newSyncTree = GetSyncTreeContainingPortalPath(newPath);

            // itt már csak azonos szerveren történő moveolás lehetséges (különböző szerverre történő moveolás feljebb történik)
            // a sync tartományok esetleg különbözhetnek
            var assumedPortalParentPath = oldSyncTree.GetPortalParentPath(entry.Path);
            var portalParentPath = RepositoryPath.GetParentPath(newPath);

            // változott a path, moveolni kell
            if (assumedPortalParentPath != portalParentPath)
            {
                var parentEntryName = newSyncTree.GetADPath(portalParentPath);
                using (DirectoryEntry parentEntry = newSyncTree.ConnectToObject(parentEntryName))
                {
                    entry.MoveTo(parentEntry);
                }
            }
        }


        /* ==================================================================================== portal -> AD : Main algorithms */
        private void UpdateADUserProperties(DirectoryEntry entry, SyncTree entrySyncTree, Node node, string newPath, string passwd)
        {
            var user = (User)node;

            var enabled = user.Enabled;
            Common.UpdateADUserCustomProperties(entry, user, _propertyMappings, enabled, _config.ADsAMAccountNameMaxLength, _config.SyncEnabledState, _config.SyncUserName);

            entry.CommitChanges();

            Common.RenameADObjectIfNecessary(entry, node, _config.ADNameMaxLength, _config.AllowRename);

            // move object
            this.MoveADObjectIfNecessary(entry, entrySyncTree, node, newPath);

            // set password
            if (passwd != null)
                Common.SetPassword(entry, passwd);

            Common.UpdateLastSync(node, null);
        }
        private void UpdateADGroupCustomProperies(DirectoryEntry entry, Node node)
        {
            if (_config.SyncUserName)
                entry.Properties["sAMAccountName"].Value = node.Name.MaximizeLength(_config.ADsAMAccountNameMaxLength);
            
            // dobsonl 20101005: probably not necessary
            //entry.Properties["groupType"].Value = ADGroupOptions.GlobalSecurityGroup;
            var group = (Group)node;

            // membership
            // 1 remove AD group users
            entry.Properties["member"].Clear();

            // 1 go through portal group users
            // 2 decide which synctree does the user belong to
            // 3 add synced user

            foreach (Node portalMember in group.Members)
            {
                var syncTree = GetSyncTreeContainingPortalPath(portalMember.Path);
                if (syncTree == null)
                {
                    AdLog.LogWarning("Portal group contains member under path that is not synchronized!");
                    continue;
                }
                var guid = Common.GetPortalObjectGuid(portalMember);
                if (!guid.HasValue)
                {
                    AdLog.LogErrorPortalObject("Portal group contains member that has no SyncGuid property set!", portalMember.Path);
                    continue;
                }
                using (DirectoryEntry ADmember = syncTree.GetADObjectByGuid((Guid)guid))
                {
                    if (ADmember == null)
                    {
                        AdLog.LogErrorPortalObject("No corresponding AD user found to portal member", node.Path);
                        continue;
                    }
                    entry.Properties["member"].Add(ADmember.Properties["distinguishedName"].Value.ToString());
                }
            }
        }
        private void UpdateADContainerProperties(DirectoryEntry entry, SyncTree entrySyncTree, Node node, string newPath, string passwd)
        {
            if (Common.GetADObjectType(node.NodeType) == ADObjectType.Group)
            {
                UpdateADGroupCustomProperies(entry, node);
                entry.CommitChanges();
            }
            Common.RenameADObjectIfNecessary(entry, node, _config.ADNameMaxLength, _config.AllowRename);
            
            // move object
            this.MoveADObjectIfNecessary(entry, entrySyncTree, node, newPath);
            
            Common.UpdateLastSync(node, null);
        }
        private void CreateADUser(SyncTree syncTree, string parentADPath, User user, string passwd)
        {
            using (DirectoryEntry parentObj = syncTree.ConnectToObject(parentADPath))
            {
                var prefix = Common.GetADObjectPrefix(ADObjectType.User);
                var userName = user.Name.MaximizeLength(_config.ADNameMaxLength);
                using (DirectoryEntry newObj = parentObj.Children.Add(String.Concat(prefix, userName), "user"))
                {
                    newObj.Properties["userAccountControl"].Value = ADAccountOptions.UF_NORMAL_ACCOUNT | ADAccountOptions.UF_DONT_EXPIRE_PASSWD;

                    // user actions

                    // user enabled/disabled: akkor enabled, ha a user maga enabled és globálisan nincs letiltva az enabled állapot konfigban
                    var enabled = ((!_createdUsersDisabled) && (user.Enabled));

                    Common.UpdateADUserCustomProperties(newObj, user, _propertyMappings, enabled, _config.ADsAMAccountNameMaxLength, _config.SyncEnabledState, _config.SyncUserName);

                    newObj.CommitChanges();

                    //if (doNotExpirePassword)
                    //{
                    //    oNewADUser.Properties["userAccountControl"].Value = ADAccountOptions.UF_NORMAL_ACCOUNT | ADAccountOptions.UF_DONT_EXPIRE_PASSWD;
                    //}
                    //else
                    //{
                    //    oNewADUser.Properties["userAccountControl"].Value = ADAccountOptions.UF_NORMAL_ACCOUNT;
                    //}
                    
                    // set password
                    if (passwd != null)
                        Common.SetPassword(newObj, passwd);

                    Common.SetPortalObjectGuid(newObj, user, _config.GuidProp);
                }
            }
        }
        private void CreateADOrgUnit(SyncTree syncTree, string parentADPath, Node node)
        {
            using (DirectoryEntry parentObj = syncTree.ConnectToObject(parentADPath))
            {
                var prefix = Common.GetADObjectPrefix(ADObjectType.OrgUnit);
                using (DirectoryEntry newObj = parentObj.Children.Add(String.Concat(prefix, node.Name), "organizationalUnit"))
                {
                    newObj.CommitChanges();
                    Common.SetPortalObjectGuid(newObj, node, _config.GuidProp);
                }
            }
        }
        private void CreateADGroup(SyncTree syncTree, string parentADPath, Node node)
        {
            using (DirectoryEntry parentObj = syncTree.ConnectToObject(parentADPath))
            {
                var prefix = Common.GetADObjectPrefix(ADObjectType.Group);
                using (DirectoryEntry newObj = parentObj.Children.Add(String.Concat(prefix, node.Name), "group"))
                {
                    // a members.clear után nem engedné létrehozni constraint miatt
                    //UpdateADGroupCustomProperies(newObj, node);
                    newObj.Properties["sAMAccountName"].Value = node.Name.MaximizeLength(_config.ADsAMAccountNameMaxLength);
                    newObj.Properties["groupType"].Value = ADGroupOptions.GlobalSecurityGroup;
                    newObj.CommitChanges();
                    Common.SetPortalObjectGuid(newObj, node, _config.GuidProp);
                }
            }
        }
        private void CreateADContainer(SyncTree syncTree, string parentADPath, Node node)
        {
            using (DirectoryEntry parentObj = syncTree.ConnectToObject(parentADPath))
            {
                var prefix = Common.GetADObjectPrefix(ADObjectType.Container);
                using (DirectoryEntry newObj = parentObj.Children.Add(String.Concat(prefix, node.Name), "container"))
                {
                    newObj.CommitChanges();
                    Common.SetPortalObjectGuid(newObj, node, _config.GuidProp);
                }
            }
        }
        private void UpdateADObject(Node node, string newPath, string passwd, Action<DirectoryEntry, SyncTree, Node, string, string> UpdateObjectProperties)
        {
            // ha az objektum nincs szinkronizálva
            if (!IsSyncedObject(node.Path))
                return;

            // ha a mozgatás nem megengedett tartományok között történik
            if (!AllowMoveADObject(node, newPath))
                return;

            AdLog.LogPortalObject("Updating AD object", node.Path);

            var guid = Common.GetPortalObjectGuid(node);
            if (guid.HasValue)
            {
                var ADObject = GetADObjectByGuid((Guid)guid);
                using (DirectoryEntry entry = ADObject.entry)
                {
                    if (entry != null)
                    {
                        var entrySyncTree = ADObject.syncTree;
                        UpdateObjectProperties(entry, entrySyncTree, node, newPath, passwd);
                    }
                    else
                    {
                        AdLog.LogErrorPortalObject(string.Format("AD object with the given GUID ({0}) does not exist", guid.ToString()), node.Path);
                    }
                }
            }
            else
            {
                AdLog.LogErrorPortalObject("Portal node does not have a syncguid", node.Path);
            }
        }


        /* ==================================================================================== Public Methods */
        public void CreateNewADUser(User user, string newPath, string passwd)
        {
            IUser originalUser = User.Current;
            Common.ChangeToAdminAccount();

            try
            {
                var parentPath = RepositoryPath.GetParentPath(newPath);

                // get containing synctree
                var syncTree = GetSyncTreeContainingPortalPath(parentPath);
                if (syncTree == null)
                {
                    // not synced object
                    return;
                }
                AdLog.LogPortalObject("Creating new AD user", user.Path);

                var parentADPath = syncTree.GetADPath(parentPath);

                CreateADUser(syncTree, parentADPath, user, passwd);
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
                throw new Exception(ex.Message, ex);
            }
        }
        public void CreateNewADContainer(Node node, string newPath)
        {
            IUser originalUser = User.Current;
            Common.ChangeToAdminAccount();

            try
            {
                var parentPath = RepositoryPath.GetParentPath(newPath);

                // get containing synctree
                var syncTree = GetSyncTreeContainingPortalPath(newPath);
                if (syncTree == null)
                {
                    // not synced object
                    return;
                }
                AdLog.LogPortalObject("Creating new AD orgunit/group/container", node.Path);

                var parentADPath = syncTree.GetADPath(parentPath);

                // create new AD object
                var adObjType = Common.GetADObjectType(node.NodeType);
                switch (adObjType)
                {
                    case ADObjectType.OrgUnit:
                        CreateADOrgUnit(syncTree, parentADPath, node);
                        break;
                    case ADObjectType.Group:
                        CreateADGroup(syncTree, parentADPath, node);
                        break;
                    case ADObjectType.Container:
                        CreateADContainer(syncTree, parentADPath, node);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
                throw new Exception(ex.Message, ex);
            }
        }
        public void UpdateADUser(User user, string newPath, string passwd)
        {
            IUser originalUser = User.Current;
            Common.ChangeToAdminAccount();

            try
            {
                UpdateADObject(user, newPath, passwd, UpdateADUserProperties);
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
                throw new Exception(ex.Message, ex);
            }
        }
        public void UpdateADContainer(Node node, string newPath)
        {
            IUser originalUser = User.Current;
            Common.ChangeToAdminAccount();

            try
            {
                UpdateADObject(node, newPath, null, UpdateADContainerProperties);
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
                throw new Exception(ex.Message, ex);
            }
        }
        public void DeleteADObject(string nodePath, Guid? guid)
        {
            IUser originalUser = User.Current;
            Common.ChangeToAdminAccount();

            try
            {
                if (!IsSyncedObject(nodePath))
                    return;

                AdLog.LogPortalObject("Deleting AD object", nodePath);

                //var guid = Common.GetPortalObjectGuid(node);
                if (guid.HasValue)
                {
                    SyncTreeADObject ADObject = GetADObjectByGuid((Guid)guid);
                    using (DirectoryEntry entry = ADObject.entry)
                    {
                        if (entry != null)
                        {
                            // disable users under AD object and move them to specific folder
                            var deletedPath = ADObject.syncTree.DeletedADObjectsPath;
                            bool entryDeleted = false;
                            using (DirectoryEntry deletedParent = ADObject.syncTree.ConnectToObject(deletedPath))
                            {
                                using (SearchResultCollection resultColl = ADObject.syncTree.GetUsersUnderADObject(entry))
                                {
                                    foreach (SearchResult result in resultColl)
                                    {
                                        using (DirectoryEntry userEntry = result.GetDirectoryEntry())
                                        {
                                            var userPath = userEntry.Path;

                                            // disable user and move to deleted folder
                                            if (deletedParent != null)
                                                userEntry.MoveTo(deletedParent);
                                            else
                                                AdLog.LogError("Folder for deleted users could not be found on AD server!");

                                            Common.DisableUserAccount(userEntry);
                                            Common.DisableADObjectCustomProperties(userEntry, _propertyMappings, _config.ADNameMaxLength, _config.ADsAMAccountNameMaxLength);
                                            userEntry.CommitChanges();

                                            // ha a parent objektum maga egy user volt, akkor őt később már nem kell törölni
                                            if (entry.Path == userPath)
                                                entryDeleted = true;
                                        }
                                    }
                                }
                            }

                            // delete remaining entries under this entry including itself (if it has not been deleted yet)
                            if (!entryDeleted)
                            {
                                // double check user containment: if it still contains users, raise an error!
                                using (SearchResultCollection resultColl = ADObject.syncTree.GetUsersUnderADObject(entry))
                                {
                                    if (resultColl.Count == 0)
                                        entry.DeleteTree();
                                    else
                                        AdLog.LogErrorADObject("AD container cannot be deleted, it contains users!", entry.Path);
                                }
                            }
                        }
                        else
                        {
                            AdLog.LogErrorPortalObject(string.Format("AD object with the given GUID ({0}) does not exist", guid.ToString()), nodePath);
                        }
                    }
                }
                else
                {
                    AdLog.LogErrorPortalObject("Portal node does not have a syncguid", nodePath);
                }
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
                throw new Exception(ex.Message, ex);
            }
        }
        public bool AllowMoveADObject(Node node, string newPath)
        {
            try
            {
                // get containing synctree for previous and new path
                var newParentPath = RepositoryPath.GetParentPath(newPath);
                var newSyncTree = GetSyncTreeContainingPortalPath(newParentPath);

                var oldParentPath = RepositoryPath.GetParentPath(node.Path);
                var oldSyncTree = GetSyncTreeContainingPortalPath(oldParentPath);


                if ((newSyncTree == null) && (oldSyncTree == null))
                {
                    // not synced object
                    return true;
                }

                if (newSyncTree == null)
                {
                    // kikerül szinkronizált tartományból, NEM MEGENGEDETT
                    return false;
                }

                if (oldSyncTree == null)
                {
                    // bekerül szinkronizált tartományba, NEM MEGENGEDETT
                    return false;
                }

                // tartománybeli vagy tartományközi moveolás?
                if (newSyncTree.PortalPath != oldSyncTree.PortalPath)
                {
                    // más tartományba mozgatjuk
                    if (newSyncTree.IPAddress != oldSyncTree.IPAddress)
                    {
                        // a szerver sem egyezik meg: NEM MEGENGEDETT
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                AdLog.LogException(ex);
                throw new Exception(ex.Message, ex);
            }
        }
        // node is under synchronized path
        public bool IsSyncedObject(string nodePath)
        {
            var parentPath = RepositoryPath.GetParentPath(nodePath);

            // get containing synctree
            var syncTree = GetSyncTreeContainingPortalPath(parentPath);

            // not synced object
            if (syncTree == null)
                return false;

            return true;
        }

        
        /* ==================================================================================== Constructor */
        public SyncPortal2AD()
        {
            _config = Portal2ADConfiguration.Current;
            _syncTrees = _config.GetSyncTrees();
            _propertyMappings = _config.GetPropertyMappings();
            _createdUsersDisabled = _config.CreatedAdUsersDisabled;
        }
    }
}
