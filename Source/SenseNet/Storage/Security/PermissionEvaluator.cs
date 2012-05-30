using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data.Common;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Data;
using System.Data;

namespace SenseNet.ContentRepository.Storage.Security
{
    internal class PermissionEvaluator
    {
        #region //==================================================================== Distributed Action

        [Serializable]
        internal class PermissionEvaluatorResetDistributedAction : SenseNet.Communication.Messaging.DistributedAction
        {
            public override void DoAction(bool onRemote, bool isFromMe)
            {
                if (onRemote && isFromMe)
                    return;
                PermissionEvaluator.ResetPrivate();
            }
        }

        private static void DistributedReset()
        {
            new PermissionEvaluatorResetDistributedAction().Execute();
        }
        private static void ResetPrivate()
        {
            instance = null;
        }
        #endregion

        private static readonly SecurityEntry[] EmptyEntryArray = new SecurityEntry[0];

        //============================================================================= Singleton model

        private static PermissionEvaluator instance;
        private static object instanceLock = new object();

        internal static PermissionEvaluator Instance
        {
            get
            {
                if (instance != null)
                    return instance;
                lock (instanceLock)
                {
                    if (instance != null)
                        return instance;
                    var inst = new PermissionEvaluator();
                    inst.Initialize();
                    instance = inst;
                    return instance;
                }
            }
        }

        private PermissionEvaluator() { }

        internal static PermissionEvaluator Parse(string src)
        {
            var result = new PermissionEvaluator();
            var sa = src.Trim().Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            ParseInfo(sa[0].Trim(), result);
            ParseMembership(sa[1].Trim(), result);
            return result;
        }
        private static void ParseInfo(string src, PermissionEvaluator newInstance)
        {
            var sa = src.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in sa)
            {
                var permInfo = PermissionInfo.Parse(s);

                var parent = newInstance.GetParentInfo(permInfo.Path);
                if (parent != null)
                    parent.Children.Add(permInfo);
                permInfo.Parent = parent;
                newInstance.permissionTable.Add(permInfo.Path, permInfo);
            }
        }
        private static void ParseMembership(string src, PermissionEvaluator newInstance)
        {
            var sa = src.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in sa)
            {
                var sb = s.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int user = Int32.Parse(sb[0]);
                var groups = new List<int>();
                for (int i = 1; i < sb.Length; i++)
                    groups.Add(Int32.Parse(sb[i]));
                newInstance.membership.Add(user, groups);
            }
        }

        //============================================================================= Instance implementation

        private const string loadQuery = @"
                SELECT N.Path, N.CreatedById, N.ModifiedById, N.IsInherited, S.* FROM SecurityEntries S INNER JOIN Nodes N ON S.DefinedOnNodeId = N.NodeId ORDER BY N.Path
                SELECT * FROM SecurityMemberships ORDER BY UserId
                ";

        private Dictionary<string, PermissionInfo> permissionTable = new Dictionary<string, PermissionInfo>();
        private Dictionary<int, List<int>> membership = new Dictionary<int, List<int>>();

        //============================================================================= Build structure

        private void Initialize()
        {
            using (var proc = DataProvider.CreateDataProcedure(loadQuery))
            {
                proc.CommandType = CommandType.Text;
                using (var reader = proc.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var path = reader.GetString(0).ToLower();
                        var id = reader.GetInt32(5);
                        var creator = reader.GetInt32(1);
                        var lastModifier = reader.GetInt32(2);
                        var inherited = reader.GetByte(3) == 1;

                        AddPermissionSet(path, id, creator, lastModifier, inherited, CreatePermissionSet(reader));
                    }
                    reader.NextResult();
                    while (reader.Read())
                        AddMembershipRow(reader);
                }
            }
        }
        private PermissionSet CreatePermissionSet(DbDataReader reader)
        {
            var allowBits = 0;
            var denyBits = 0;
            for (var i = 0; i < 16; i++)
            {
                var value = (PermissionValue)reader.GetByte(i + 8);
                if (value == PermissionValue.Allow)
                    allowBits |= 1 << i;
                else if (value == PermissionValue.Deny)
                    denyBits |= 1 << i;
            }
            return new PermissionSet(reader.GetInt32(6), reader.GetByte(7) == 1, allowBits, denyBits);
        }
        private void AddPermissionSet(string path, int id, int creator, int lastModifier, bool inherited, PermissionSet entry)
        {
            if (!permissionTable.ContainsKey(path))
                permissionTable.Add(path, CreatePermissionInfo(path, id, creator, lastModifier, inherited));
            permissionTable[path].PermissionSets.Add(entry);
        }
        private PermissionInfo CreatePermissionInfo(string path, int id, int creator, int lastModifier, bool inherited)
        {
            var parent = GetParentInfo(path);
            var permInfo = new PermissionInfo
            {
                Path = path,
                Id = id,
                Creator = creator,
                LastModifier = lastModifier,
                Inherits = inherited,
                PermissionSets = new List<PermissionSet>(),
                Parent = parent,
                Children = new List<PermissionInfo>()
            };
            if (parent != null)
                parent.Children.Add(permInfo);
            return permInfo;
        }
        private PermissionInfo GetParentInfo(string path)
        {
            if (path.ToLower() == "/root")
                return null;

            return GetFirstInfo(RepositoryPath.GetParentPath(path));
        }
        private PermissionInfo GetFirstInfo(string path)
        {
            var p = path;
            PermissionInfo parent;
            while (true)
            {
                if (permissionTable.TryGetValue(p, out parent))
                    return parent;
                if (p.ToLower() == "/root")
                    break;
                p = RepositoryPath.GetParentPath(p);
            }
            return null;
        }

        private void AddMembershipRow(DbDataReader reader)
        {
            var containerId = reader.GetInt32(0);
            var userId = reader.GetInt32(1);
            var containerType = reader.GetString(2);
            if (!membership.ContainsKey(userId))
                membership.Add(userId, new List<int>());
            if (!membership[userId].Contains(containerId))
                membership[userId].Add(containerId);
        }

        //============================================================================= Static interface

        public static void Reset()
        {
            DistributedReset();
            //instance = null;
        }

        internal bool HasPermission(string path, IUser user, bool isCreator, bool isLastModifier, PermissionType[] permissionTypes)
        {
            if (user.Id == -1)
                return true;
            var value = GetPermission(path, user, isCreator, isLastModifier, permissionTypes);

            if (RepositoryConfiguration.TracePermissionCheck)
                if (value != PermissionValue.Allow)
                    Debug.WriteLine(String.Format("HasPermission> {0}, {1}, {2}, {3}", value, String.Join("|", permissionTypes.Select(x => x.Name).ToArray()), user.Username, path));

            return value == PermissionValue.Allow;

        }
        internal bool HasSubTreePermission(string path, IUser user, bool isCreator, bool isLastModifier, PermissionType[] permissionTypes)
        {
            if (user.Id == -1)
                return true;
            var value = GetSubtreePermission(path, user, isCreator, isLastModifier, permissionTypes);
            return value == PermissionValue.Allow;
        }
        internal PermissionValue GetPermission(string path, IUser user, bool isCreator, bool isLastModifier, PermissionType[] permissionTypes)
        {
            if (user.Id == -1)
                return PermissionValue.Allow;

            //==>
            var principals = GetPrincipals(user, isCreator, isLastModifier);

            var allow = 0;
            var deny = 0;

            var firstPermInfo = GetFirstInfo(path);
            if (firstPermInfo.Path == path)
                firstPermInfo.AggregateLevelOnlyValues(principals, ref allow, ref deny);
            for (var permInfo = firstPermInfo; permInfo != null; permInfo = permInfo.Inherits ? permInfo.Parent : null)
                permInfo.AggregateEffectiveValues(principals, ref allow, ref deny);
            //==<

            var mask = GetPermissionMask(permissionTypes);
            if ((deny & mask) != 0)
                return PermissionValue.Deny;
            if ((allow & mask) != mask)
                return PermissionValue.NonDefined;
            return PermissionValue.Allow;
        }
        internal PermissionValue GetSubtreePermission(string path, IUser user, bool isCreator, bool isLastModifier, PermissionType[] permissionTypes)
        {
            if (user.Id == -1)
                return PermissionValue.Allow;

            //======== #1: startbits: getpermbits
            //==>
            var principals = GetPrincipals(user, isCreator, isLastModifier);

            var allow = 0;
            var deny = 0;

            var firstPermInfo = GetFirstInfo(path);
            if (firstPermInfo.Path == path)
                firstPermInfo.AggregateLevelOnlyValues(principals, ref allow, ref deny);
            for (var permInfo = firstPermInfo; permInfo != null; permInfo = permInfo.Inherits ? permInfo.Parent : null)
                permInfo.AggregateEffectiveValues(principals, ref allow, ref deny);
            //==<
            var mask = GetPermissionMask(permissionTypes);
            if ((deny & mask) != 0)
                return PermissionValue.Deny;
            if ((allow & mask) != mask)
                return PermissionValue.NonDefined;

            //  +r     +1+++ | +1_++ | +1+++
            //  +r/a   +1_++ | +1+++ | +1-++
            // ==============|=======|=======
            //           +++ |   _++ |   -++

            //  +r     +1+++ | +1_++ | +1+++
            //  +r/a   -1_++ | -1+++ | -1-++
            // ==============|=======|=======
            //           +++ |   _++ |   -++

            //  +r     +1+++ | +1_++ | +1+++
            //  -r/a   +1_++ | +1+++ | +1-++
            // ==============|=======|=======
            //           _++ |   _++ |   -++
            // nem fugg a permissionset.inheritable ertektol
            // denybits: or, break: nem kell ujraszamolni
            // allowbits or, break: ujraszamolni

            //PermissionInfo subTreePermInfo;
            //if (entries.TryGetValue(path, out subTreePermInfo))
            //{
            //    subTreePermInfo.GetSubtreePermission(path, principals, isCreator, isLastModifier, mask, ref allow, ref deny);
            //}
            //else
            //{
            var p = path + "/";
            var permInfos = from key in permissionTable.Keys where key.StartsWith(p) orderby key select permissionTable[key];
            foreach (var permInfo in permInfos)
            {
                if (!permInfo.Inherits)
                {
                    allow = 0;
                    foreach (var entry in permInfo.PermissionSets)
                    {
                        if (!principals.Contains(entry.PrincipalId))
                            continue;
                        allow |= entry.AllowBits;
                        deny |= entry.DenyBits;
                    }
                }
                foreach (var entry in permInfo.PermissionSets)
                {
                    if (!principals.Contains(entry.PrincipalId))
                        continue;
                    deny |= entry.DenyBits;
                }
            }
            //}

            if ((deny & mask) != 0)
                return PermissionValue.Deny;
            if ((allow & mask) != mask)
                return PermissionValue.NonDefined;
            return PermissionValue.Allow;

        }
        internal PermissionValue[] GetAllPermissions(string path, IUser user, bool isCreator, bool isLastModifier)
        {
            if (user.Id == -1)
                return GetPermissionValues(-1, 0);
            //==>
            var principals = GetPrincipals(user, isCreator, isLastModifier);

            var allow = 0;
            var deny = 0;

            var firstPermInfo = GetFirstInfo(path);
            if (firstPermInfo.Path == path)
                firstPermInfo.AggregateLevelOnlyValues(principals, ref allow, ref deny);
            for (var permInfo = firstPermInfo; permInfo != null; permInfo = permInfo.Inherits ? permInfo.Parent : null)
                permInfo.AggregateEffectiveValues(principals, ref allow, ref deny);
            //==<

            return GetPermissionValues(allow, deny);
        }
        internal PermittedLevel GetPermittedLevel(string path, IUser user, bool isCreator, bool isLastModifier)
        {
            if (user.Id == -1)
                return PermittedLevel.All;
            //==>
            var principals = GetPrincipals(user, isCreator, isLastModifier);

            var allow = 0;
            var deny = 0;

            var firstPermInfo = GetFirstInfo(path);
            if (firstPermInfo == null)
                throw new ApplicationException(String.Format("PermissionInfo was not found. Path: {0}, User: {1}, isCreator: {2}, isLastModifier: {3}", path, user.Username, isCreator, isLastModifier));

            if (firstPermInfo.Path == path)
                firstPermInfo.AggregateLevelOnlyValues(principals, ref allow, ref deny);
            for (var permInfo = firstPermInfo; permInfo != null; permInfo = permInfo.Inherits ? permInfo.Parent : null)
                permInfo.AggregateEffectiveValues(principals, ref allow, ref deny);
            //==<

            var x = allow & ~deny;
            PermittedLevel level;
            if ((x & 0x4) != 0)
                level = PermittedLevel.All;
            else if ((x & 0x2) != 0)
                level = PermittedLevel.PublicOnly;
            else if ((x & 0x1) != 0)
                level = PermittedLevel.HeadOnly;
            else
                level = PermittedLevel.None;
            return level;

            ////HACK: harcoded implementation
            //if (userId == 1)
            //    return PermittedLevel.All;
            //if (path.StartsWith("/Root/System/ContentExplorer/explorer.aspx"))
            //    return PermittedLevel.None;
            //return PermittedLevel.PublicOnly;
        }
        internal SecurityEntry[] GetAllEntries(string path)
        {
            var firstPermInfo = GetFirstInfo(path);
            if (firstPermInfo == null)
                return EmptyEntryArray;
            return firstPermInfo.GetAllEntries().ToArray();
        }
        internal SecurityEntry[] GetExplicitEntries(string path)
        {
            //var firstPermInfo = GetFirstInfo(path);
            //if (firstPermInfo == null)
            //    return EmptyEntryArray;
            //return firstPermInfo.GetExplicitEntries().ToArray();

            PermissionInfo permInfo;
            if (!permissionTable.TryGetValue(path.ToLower(), out permInfo))
                return EmptyEntryArray;
            return permInfo.GetExplicitEntries().ToArray();
        }
        internal SecurityEntry GetExplicitEntry(string path, int identity)
        {
            PermissionInfo permInfo;
            if (!permissionTable.TryGetValue(path.ToLower(), out permInfo))
                return null;
            return permInfo.GetExplicitEntry(identity);
        }
        internal SecurityEntry[] GetEffectiveEntries(string path)
        {
            var firstPermInfo = GetFirstInfo(path);
            if (firstPermInfo == null)
                return EmptyEntryArray;
            return firstPermInfo.GetEffectiveEntries(firstPermInfo.Path == path).ToArray();
        }

        internal bool IsInGroup(int userId, int groupId)
        {
            if (groupId == RepositoryConfiguration.EveryoneGroupId)
                return true;

            if (membership.ContainsKey(userId))
                return membership[userId].Contains(groupId);

            return false;
        }

        //=============================================================================

        internal List<int> GetPrincipals(IUser user, bool isCreator, bool isLastModifier)
        {
            var principals = new List<int>(new int[] { user.Id, RepositoryConfiguration.EveryoneGroupId });
            if (membership.ContainsKey(user.Id))
                principals.AddRange(membership[user.Id]);
            if (isCreator)
                principals.Add(RepositoryConfiguration.CreatorsGroupId);
            if (isLastModifier)
                principals.Add(RepositoryConfiguration.LastModifiersGroupId);
            var extension = user.MembershipExtension;
            if (extension != null)
                principals.AddRange(extension.ExtensionIds);
            return principals;
        }
        private PermissionValue[] GetPermissionValues(int allowBits, int denyBits)
        {
            var result = new PermissionValue[PermissionType.NumberOfPermissionTypes];
            for (int i = 0; i < PermissionType.NumberOfPermissionTypes; i++)
            {
                var allow = (allowBits & 1) == 1;
                var deny = (denyBits & 1) == 1;
                allowBits = allowBits >> 1;
                denyBits = denyBits >> 1;
                if (deny)
                    result[i] = PermissionValue.Deny;
                else if (allow)
                    result[i] = PermissionValue.Allow;
                else
                    result[i] = PermissionValue.NonDefined;
            }
            return result;
        }
        private int GetPermissionMask(PermissionType[] permissionTypes)
        {
            int mask = 0;
            foreach (var permissionType in permissionTypes)
                mask = mask | (1 << (permissionType.Id - 1));
            return mask;
        }

        //============================================================================= for editing

        internal SnAccessControlList GetAcl(int nodeId, string path, int creatorId, int lastModifierId)
        {
            var acl = new SnAccessControlList { Path = path, NodeId = nodeId, Creator = SnIdentity.Create(creatorId), LastModifier = SnIdentity.Create(lastModifierId) };
            var firstPermInfo = GetFirstInfo(path);
            if (firstPermInfo == null)
                return acl;
            return firstPermInfo.BuildAcl(acl);
        }
        internal SecurityEntry[] SetAcl(SnAccessControlList acl)
        {
            var result = new List<SecurityEntry>();

            //var acl0 = GetAcl(nodeId, path, creatorId);

            foreach (var entry in acl.Entries)
            {
                var values = new PermissionValue[ActiveSchema.PermissionTypes.Count];
                foreach (var perm in entry.Permissions)
                {
                    //var id = ActiveSchema.PermissionTypes[perm.Name].Id;
                    //var allow = perm.AllowFrom == null ? perm.Allow : false;
                    //var deny = perm.DenyFrom == null ? perm.Deny : false;
                    //var value = deny ? PermissionValue.Deny : (allow ? PermissionValue.Allow : PermissionValue.NonDefined);
                    //values[id - 1] = value;

                    var id = ActiveSchema.PermissionTypes[perm.Name].Id;
                    var value = perm.Deny ? PermissionValue.Deny : (perm.Allow ? PermissionValue.Allow : PermissionValue.NonDefined);
                    values[id - 1] = value;
                }

                result.Add(new SecurityEntry(acl.NodeId, entry.Identity.NodeId, entry.Propagates, values));
            }

            return result.ToArray();
        }



    }
}
