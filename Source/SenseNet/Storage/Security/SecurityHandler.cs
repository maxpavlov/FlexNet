using System;
using System.Collections;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using System.Globalization;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Xml;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Storage.Security
{
	public sealed class SecurityHandler
	{
		private Node _node;

		private int NodeId
		{
			get { return _node.Id; }
		}

		internal SecurityHandler(Node node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			_node = node;
		}

        public static void Reset()
        {
            PermissionEvaluator.Reset();
        }

        internal static void Move(string sourcePath, string targetPath)
        {
            Reset();
        }
        internal static void Delete(string sourcePath)
        {
            Reset();
        }
        internal static void Rename(string originalPath, string newPath)
        {
            Reset();
        }

        //======================================================================================================== Administration methods

        public void SetPermission(IUser user, bool isInheritable, PermissionType permissionType, PermissionValue permissionValue)
		{
			if (user == null)
				throw new ArgumentNullException("user");
            SetPermission(user as ISecurityMember, isInheritable, permissionType, permissionValue);
		}
        public void SetPermission(IGroup group, bool isInheritable, PermissionType permissionType, PermissionValue permissionValue)
		{
			if (group == null)
				throw new ArgumentNullException("group");
            SetPermission(group as ISecurityMember, isInheritable, permissionType, permissionValue);
		}
        public void SetPermission(IOrganizationalUnit orgUnit, bool isInheritable, PermissionType permissionType, PermissionValue permissionValue)
        {
            if (orgUnit == null)
                throw new ArgumentNullException("orgUnit");
            SetPermission(orgUnit as ISecurityMember, isInheritable, permissionType, permissionValue);
        }
        public void SetPermission(ISecurityMember securityMember, bool isInheritable, PermissionType permissionType, PermissionValue permissionValue)
        {
            if (securityMember == null)
                throw new ArgumentNullException("securityMember");
            if (permissionType == null)
                throw new ArgumentNullException("permissionType");

            Assert(PermissionType.SetPermissions);

            var entry = PermissionEvaluator.Instance.GetExplicitEntry(this._node.Path, securityMember.Id);
            var allowBits = 0;
            var denyBits = 0;
            if (entry != null)
            {
                allowBits = entry.AllowBits;
                denyBits = entry.DenyBits;
            }
            SetBits(ref allowBits, ref denyBits, permissionType, permissionValue);

            var memberId = securityMember.Id;
            var permSet = new PermissionSet(memberId, isInheritable, allowBits, denyBits);
            entry = permSet.ToEntry(this.NodeId);

            DataProvider.Current.SetPermission(entry);

            Reset();
        }

        public void SetPermissions(int principalId, bool isInheritable, PermissionValue[] permissionValues)
        {
            Assert(PermissionType.SetPermissions);
            SetPermissionsWithoutReset(principalId, isInheritable, permissionValues);
            Reset();
        }
        private void SetPermissionsWithoutReset(int principalId, bool isInheritable, PermissionValue[] permissionValues)
        {
            var permSet = new PermissionSet(principalId, isInheritable, permissionValues);
            var allowBits = permSet.AllowBits;
            var denyBits = permSet.DenyBits;
            
            SetBits(ref allowBits, ref denyBits);
            permSet = new PermissionSet(principalId, isInheritable, allowBits, denyBits);
            var entry = permSet.ToEntry(NodeId);

            DataProvider.Current.SetPermission(entry);
        }

        public void RemoveExplicitEntries()
        {
            if (GetExplicitEntries().Length == 0)
                return;
            RemoveExplicitEntriesWithoutReset();
            Reset();
        }
        private void RemoveExplicitEntriesWithoutReset()
        {
            foreach (var entry in GetExplicitEntries())
            {
                var e = new PermissionSet(entry.PrincipalId, true, 0, 0).ToEntry(entry.DefinedOnNodeId);
                DataProvider.Current.SetPermission(e);
            }
        }

        //public void ReplacePermissionsOnChildNodes()
        //{
        //    DataProvider.Current.ReplacePermissionsOnChildNodes(_node.Id);
        //    Reset();
        //}
        //public void BreakInheritance(int inheritanceSourceNodeId)
        //{
        //    DataProvider.Current.BreakInheritance(_node.Id, inheritanceSourceNodeId);
        //    Reset();
        //}
        public void BreakInheritance()
        {
            if (!_node.IsInherited)
                return;
            BreakInheritanceWithoutReset();
            Reset();
        }
        private void BreakInheritanceWithoutReset()
        {
            foreach (var entry in GetEffectiveEntries())
                SetPermissions(entry.PrincipalId, entry.Propagates, entry.PermissionValues);
            DataBackingStore.BreakPermissionInheritance(_node);
        }
        public void RemoveBreakInheritance()
        {
            if (_node.IsInherited)
                return;
            RemoveBreakInheritanceWithoutReset();
            Reset();
        }
        private void RemoveBreakInheritanceWithoutReset()
        {
            //foreach (var entry in GetEffectiveEntries())
            //    SetPermissions(entry.PrincipalId, entry.Propagates, entry.PermissionValues);
            DataBackingStore.RemoveBreakPermissionInheritance(_node);
        }

        public static void ExplicateGroupMembership()
		{
			DataProvider.Current.ExplicateGroupMemberships();
            Reset();
        }
        public static void ExplicateOrganizationUnitMemberships(IUser user)
        {
            DataProvider.Current.ExplicateOrganizationUnitMemberships(user);
            Reset();
        }

        public void ImportPermissions(XmlNode permissionsNode, string metadataPath)
        {
            Assert(PermissionType.SetPermissions);

            var permissionTypes = ActiveSchema.PermissionTypes;

            //-- parsing and executing 'Break'
            var breakNode = permissionsNode.SelectSingleNode("Break");
            if (breakNode != null)
            {
                if (_node.IsInherited)
                    BreakInheritanceWithoutReset();
            }
            else
            {
                if (!_node.IsInherited)
                    RemoveBreakInheritanceWithoutReset();
            }
            //-- parsing and executing 'Clear'
            var clearNode = permissionsNode.SelectSingleNode("Clear");
            if (clearNode != null)
                RemoveExplicitEntriesWithoutReset();

            var identityElementIndex = 0;
            foreach (XmlElement identityElement in permissionsNode.SelectNodes("Identity"))
            {
                identityElementIndex++;

                //-- checking identity path
                var path = identityElement.GetAttribute("path");
                if (String.IsNullOrEmpty(path))
                    throw ImportPermissionExceptionHelper(String.Concat("Missing or empty path attribute of the Identity element ", identityElementIndex, "."), metadataPath, null);
                var pathCheck =RepositoryPath.IsValidPath(path);
                if (pathCheck != RepositoryPath.PathResult.Correct)
                    throw ImportPermissionExceptionHelper(String.Concat("Invalid path of the Identity element ", identityElementIndex, ": ", path, " (", pathCheck, ")."), metadataPath, null);

                //-- getting identity node
                var identityNode = Node.LoadNode(path);
                if(identityNode==null)
                    throw ImportPermissionExceptionHelper(String.Concat("Identity ", identityElementIndex, " was not found: ", path, "."), metadataPath, null);

                //-- initializing value array
                var values = new PermissionValue[permissionTypes.Count];
                foreach (var permType in permissionTypes)
                    values[permType.Id - 1] = PermissionValue.NonDefined;

                //-- parsing value array
                foreach (XmlElement permissionElement in identityElement.SelectNodes("*"))
                {
                    var permName = permissionElement.LocalName;
                    var permType = permissionTypes.Where(p => String.Compare(p.Name, permName, true) == 0).FirstOrDefault();
                    if(permType==null)
                        throw ImportPermissionExceptionHelper(String.Concat("Permission type was not found in Identity ", identityElementIndex, "."), metadataPath, null);

                    var permValue = PermissionValue.NonDefined;
                    switch (permissionElement.InnerText.ToLower())
                    {
                        case "allow": permValue = PermissionValue.Allow; break;
                        case "deny": permValue = PermissionValue.Deny; break;
                        default:
                            throw ImportPermissionExceptionHelper(String.Concat("Invalid permission value in Identity ", identityElementIndex, ": ", permissionElement.InnerText, ". Allowed values: Allow, Deny"), metadataPath, null);
                    }

                    values[permType.Id - 1] = permValue;
                }

                //-- setting permissions
                SetPermissionsWithoutReset(identityNode.Id, true, values);
            }

            Reset();
        }
        private Exception ImportPermissionExceptionHelper(string message, string metadataPath, Exception innerException)
        {
            var msg = String.Concat("Importing permissions was failed. Metadata: ", metadataPath, ". Reason: ", message);
            return new ApplicationException(msg, innerException);
        }
        public void ExportPermissions(XmlWriter writer)
        {
            //-- specification
            //<Permissions>
            //  <Clear />
            //  <Break />
            //  <Identity path="/Root/IMS/BuiltIn/Portal/Administrator">
            //    <See>Allow</See>
            //    <Open>Allow</Open>
            //  </Identity>
            //</Permissions>

            if (!_node.IsInherited)
                writer.WriteElementString("Break", null);
            var entries = _node.Security.GetExplicitEntries();
            foreach (var entry in entries)
                entry.Export(writer);
        }

        public void Assert(params PermissionType[] permissionTypes)
        {
            Assert(_node, permissionTypes);
        }
        public void Assert(string message, params PermissionType[] permissionTypes)
        {
            Assert(_node, message, permissionTypes);
        }
        public static void Assert(Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            Assert(node.Path, node.CreatedById, node.NodeModifiedById, null, permissionTypes);
        }
        public static void Assert(Node node, string message, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            Assert(node.Path, node.CreatedById, node.NodeModifiedById, message, permissionTypes);
        }
        public static void Assert(NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            Assert(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, null, permissionTypes);
        }
        public static void Assert(NodeHead nodeHead, string message, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            Assert(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, message, permissionTypes);
        }
        private static void Assert(string path, int creatorId, int lastModifierId, string message, params PermissionType[] permissionTypes)
        {
            //if (path == RepositoryConfiguration.VisitorUserId)
            //    return;
            IUser user = AccessProvider.Current.GetCurrentUser();
            var userId = user.Id;
            if (user.Id == -1)
                return;
            if (HasPermission(path, creatorId, lastModifierId, permissionTypes))
                return;
            throw GetAccessDeniedException(path, creatorId, lastModifierId, message, permissionTypes, user);
        }

        public void AssertSubtree(params PermissionType[] permissionTypes)
        {
            AssertSubtree(_node, permissionTypes);
        }
        public void AssertSubtree(string message, params PermissionType[] permissionTypes)
        {
            AssertSubtree(_node, message, permissionTypes);
        }
        public static void AssertSubtree(Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            AssertSubtree(node.Path, node.CreatedById, node.NodeModifiedById, null, permissionTypes);
        }
        public static void AssertSubtree(Node node, string message, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            AssertSubtree(node.Path, node.CreatedById, node.NodeModifiedById, message, permissionTypes);
        }
        public static void AssertSubtree(NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            AssertSubtree(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, null, permissionTypes);
        }
        public static void AssertSubtree(NodeHead nodeHead, string message, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            AssertSubtree(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, message, permissionTypes);
        }
        private static void AssertSubtree(string path, int creatorId, int lastModifierId, string message, params PermissionType[] permissionTypes)
        {
            //if (path == RepositoryConfiguration.VisitorUserId)
            //    return;
            IUser user = AccessProvider.Current.GetCurrentUser();
            var userId = user.Id;
            if (user.Id == -1)
                return;
            if (HasSubTreePermission(path, creatorId, lastModifierId, permissionTypes))
                return;
            throw GetAccessDeniedException(path, creatorId, lastModifierId, message, permissionTypes, user);
        }

        public bool HasPermission(params PermissionType[] permissionTypes)
        {
            return HasPermission(_node, permissionTypes);
        }
        public static bool HasPermission(Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return HasPermission(node.Path, node.CreatedById, node.NodeModifiedById, permissionTypes);
        }
        public static bool HasPermission(NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return HasPermission(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, permissionTypes);
        }
        private static bool HasPermission(string path, int creatorId, int lastModifierId, params PermissionType[] permissionTypes)
        {
            if (permissionTypes == null)
                throw new ArgumentNullException("permissionTypes");
            if (permissionTypes.Length == 0)
                return false;
            var user = AccessProvider.Current.GetCurrentUser();
            if (user.Id == -1)
                return true;
            var isCreator = user.Id == creatorId;
            var isLastModifier = user.Id == lastModifierId;
            return PermissionEvaluator.Instance.HasPermission(path.ToLower(), user, isCreator, isLastModifier, permissionTypes);
        }

        public bool HasPermission(IUser user, params PermissionType[] permissionTypes)
        {
            return HasPermission(user, _node, permissionTypes);
        }
        public bool HasPermission(IUser user, Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return HasPermission(user, node.Path, node.CreatedById, node.NodeModifiedById, permissionTypes);
        }
        public bool HasPermission(IUser user, NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return HasPermission(user, nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, permissionTypes);
        }
        public static bool HasPermission(IUser user, string path, int creatorId, int lastModifierId, params PermissionType[] permissionTypes)
        {
            if (user.Id != AccessProvider.Current.GetCurrentUser().Id)
                Assert(path, creatorId, lastModifierId, null, PermissionType.SeePermissions);
            if (path == null)
                throw new ArgumentNullException("path");
            if (permissionTypes == null)
                throw new ArgumentNullException("permissionTypes");
            if (permissionTypes.Length == 0)
                return false;
            if (user.Id == -1)
                return true;
            var isCreator = user.Id == creatorId;
            var isLastModifier = user.Id == lastModifierId;
            return PermissionEvaluator.Instance.HasPermission(path.ToLower(), user, isCreator, isLastModifier, permissionTypes);
        }

        public bool HasSubTreePermission(params PermissionType[] permissionTypes)
        {
            return HasSubTreePermission(_node, permissionTypes);
        }
        public static bool HasSubTreePermission(Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return HasSubTreePermission(node.Path, node.CreatedById, node.NodeModifiedById, permissionTypes);
        }
        public static bool HasSubTreePermission(NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return HasSubTreePermission(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, permissionTypes);
        }
        private static bool HasSubTreePermission(string path, int creatorId, int lastModifierId, params PermissionType[] permissionTypes)
        {
            if (permissionTypes == null)
                throw new ArgumentNullException("permissionTypes");
            if (permissionTypes.Length == 0)
                return false;
            var user = AccessProvider.Current.GetCurrentUser();
            if (user.Id == -1)
                return true;
            var isCreator = user.Id == creatorId;
            bool isLastModifier = user.Id == lastModifierId;
            return PermissionEvaluator.Instance.HasSubTreePermission(path.ToLower(), user, isCreator, isLastModifier, permissionTypes);
        }

        public bool HasSubTreePermission(IUser user, params PermissionType[] permissionTypes)
        {
            return HasSubTreePermission(user, _node, permissionTypes);
        }
        public static bool HasSubTreePermission(IUser user, Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return HasSubTreePermission(user, node.Path, node.CreatedById, node.NodeModifiedById, permissionTypes);
        }
        public static bool HasSubTreePermission(IUser user, NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return HasSubTreePermission(user, nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, permissionTypes);
        }
        private static bool HasSubTreePermission(IUser user, string path, int creatorId, int lastModifierId, params PermissionType[] permissionTypes)
        {
            var userId = user.Id;
            if (userId != AccessProvider.Current.GetCurrentUser().Id)
                Assert(path, creatorId, lastModifierId, null, PermissionType.SeePermissions);
            if (path == null)
                throw new ArgumentNullException("path");
            if (permissionTypes == null)
                throw new ArgumentNullException("permissionTypes");
            if (permissionTypes.Length == 0)
                return false;
            if (userId == -1)
                return true;
            var isCreator = userId == creatorId;
            var isLastModifier = userId == lastModifierId;
            return PermissionEvaluator.Instance.HasSubTreePermission(path.ToLower(), user, isCreator, isLastModifier, permissionTypes);
        }

        public PermissionValue GetPermission(params PermissionType[] permissionTypes)
        {
            return GetPermission(_node, permissionTypes);
        }
        public static PermissionValue GetPermission(Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return GetPermission(node.Path, node.CreatedById, node.NodeModifiedById, permissionTypes);
        }
        public static PermissionValue GetPermission(NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return GetPermission(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, permissionTypes);
        }
        private static PermissionValue GetPermission(string path, int creatorId, int lastModifierId, params PermissionType[] permissionTypes)
        {
            if (permissionTypes == null)
                throw new ArgumentNullException("permissionTypes");
            if (permissionTypes.Length == 0)
                return PermissionValue.Deny;
            var user = AccessProvider.Current.GetCurrentUser();
            if (user.Id == -1)
                return PermissionValue.Allow;
            var isCreator = user.Id == creatorId;
            var isLastModifier = user.Id == lastModifierId;
            return PermissionEvaluator.Instance.GetPermission(path.ToLower(), user, isCreator, isLastModifier, permissionTypes);
        }

        public PermissionValue GetPermission(IUser user, params PermissionType[] permissionTypes)
        {
            return GetPermission(user, _node, permissionTypes);
        }
        public PermissionValue GetPermission(IUser user, Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return GetPermission(user, node.Path, node.CreatedById, node.NodeModifiedById, permissionTypes);
        }
        public PermissionValue GetPermission(IUser user, NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return GetPermission(user, nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, permissionTypes);
        }
        private static PermissionValue GetPermission(IUser user, string path, int creatorId, int lastModifierId, params PermissionType[] permissionTypes)
        {
            var userId = user.Id;
            if (userId != AccessProvider.Current.GetCurrentUser().Id)
                Assert(path, creatorId, lastModifierId, null, PermissionType.SeePermissions);
            if (path == null)
                throw new ArgumentNullException("path");
            if (permissionTypes == null)
                throw new ArgumentNullException("permissionTypes");
            if (permissionTypes.Length == 0)
                return PermissionValue.Deny;
            if (userId == -1)
                return PermissionValue.Allow;
            var isCreator = userId == creatorId;
            var isLastModifier = userId == lastModifierId;
            return PermissionEvaluator.Instance.GetPermission(path.ToLower(), user, isCreator, isLastModifier, permissionTypes);
        }

        public PermissionValue GetSubtreePermission(params PermissionType[] permissionTypes)
        {
            return GetSubtreePermission(_node, permissionTypes);
        }
        public static PermissionValue GetSubtreePermission(Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return GetSubtreePermission(node.Path, node.CreatedById, node.NodeModifiedById, permissionTypes);
        }
        public static PermissionValue GetSubtreePermission(NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return GetSubtreePermission(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, permissionTypes);
        }
        private static PermissionValue GetSubtreePermission(string path, int creatorId, int lastModifierId, params PermissionType[] permissionTypes)
        {
            if (permissionTypes == null)
                throw new ArgumentNullException("permissionTypes");
            if (permissionTypes.Length == 0)
                return PermissionValue.Deny;
            var user = AccessProvider.Current.GetCurrentUser();
            if (user.Id == -1)
                return PermissionValue.Allow;
            var isCreator = user.Id == creatorId;
            var isLastModifier = user.Id == lastModifierId; 
            return PermissionEvaluator.Instance.GetSubtreePermission(path.ToLower(), user, isCreator, isLastModifier, permissionTypes);
        }

        public PermissionValue GetSubtreePermission(IUser user, params PermissionType[] permissionTypes)
        {
            return GetSubtreePermission(user, _node, permissionTypes);
        }
        public PermissionValue GetSubtreePermission(IUser user, Node node, params PermissionType[] permissionTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return GetSubtreePermission(user, node.Path, node.CreatedById, node.NodeModifiedById, permissionTypes);
        }
        public PermissionValue GetSubtreePermission(IUser user, NodeHead nodeHead, params PermissionType[] permissionTypes)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return GetSubtreePermission(user, nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId, permissionTypes);
        }
        private static PermissionValue GetSubtreePermission(IUser user, string path, int creatorId, int lastModifierId, params PermissionType[] permissionTypes)
        {
            var userId = user.Id;
            if (userId != AccessProvider.Current.GetCurrentUser().Id)
                Assert(path, creatorId, lastModifierId, null, PermissionType.SeePermissions);
            if (path == null)
                throw new ArgumentNullException("path");
            if (permissionTypes == null)
                throw new ArgumentNullException("permissionTypes");
            if (permissionTypes.Length == 0)
                return PermissionValue.Deny;
            if (userId == -1)
                return PermissionValue.Allow;
            var isCreator = userId == creatorId;
            var isLastModifier = userId == lastModifierId;
            return PermissionEvaluator.Instance.GetSubtreePermission(path.ToLower(), user, isCreator, isLastModifier, permissionTypes);
        }

        public PermissionValue[] GetAllPermissions()
        {
            return GetAllPermissions(_node);
        }
        public static PermissionValue[] GetAllPermissions(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return GetAllPermissions(node.Path, node.CreatedById, node.NodeModifiedById);
        }
        public static PermissionValue[] GetAllPermissions(NodeHead nodeHead)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return GetAllPermissions(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId);
        }
        private static PermissionValue[] GetAllPermissions(string path, int creatorId, int lastModifierId)
        {
            var user = AccessProvider.Current.GetCurrentUser();
            if (user.Id == -1)
            {
                var result = new PermissionValue[PermissionType.NumberOfPermissionTypes];
                for (int i = 0; i < PermissionType.NumberOfPermissionTypes; i++)
                    result[i] = PermissionValue.Allow;
                return result;
            }
            var isCreator = user.Id == creatorId;
            var isLastModifier = user.Id == lastModifierId;
            return PermissionEvaluator.Instance.GetAllPermissions(path.ToLower(), user, isCreator, isLastModifier);
        }

        public PermissionValue[] GetAllPermissions(IUser user)
        {
            return GetAllPermissions(user, _node);
        }
        public PermissionValue[] GetAllPermissions(IUser user, Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return GetAllPermissions(user, node.Path, node.CreatedById, node.NodeModifiedById);
        }
        public PermissionValue[] GetAllPermissions(IUser user, NodeHead nodeHead)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead");
            return GetAllPermissions(user, nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId);
        }
        private static PermissionValue[] GetAllPermissions(IUser user, string path, int creatorId, int lastModifierId)
        {
            var userId = user.Id;
            if (userId != AccessProvider.Current.GetCurrentUser().Id)
                Assert(path, creatorId, lastModifierId, null, PermissionType.SeePermissions);
            if (userId == -1)
            {
                var result = new PermissionValue[PermissionType.NumberOfPermissionTypes];
                for (int i = 0; i < PermissionType.NumberOfPermissionTypes; i++)
                    result[i] = PermissionValue.Allow;
                return result;
            }
            var isCreator = userId == creatorId;
            var isLastModifier = userId == lastModifierId;
            return PermissionEvaluator.Instance.GetAllPermissions(path.ToLower(), user, isCreator, isLastModifier);
        }

        public static PermittedLevel GetPermittedLevel(NodeHead nodeHead)
        {
            return GetPermittedLevel(nodeHead.Path, nodeHead.CreatorId, nodeHead.LastModifierId);
        }
        public static PermittedLevel GetPermittedLevel(string path, int creatorId, int lastModifierId)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            var user = AccessProvider.Current.GetCurrentUser();
            if (user.Id == -1)
                return PermittedLevel.All;
            bool isCreator = user.Id == creatorId;
            bool isLastModifier = user.Id == lastModifierId;
            return PermissionEvaluator.Instance.GetPermittedLevel(path.ToLower(), user, isCreator, isLastModifier);
        }
        public static PermittedLevel GetPermittedLevel(string path, int creatorId, int lastModifierId, IUser user)
        {
            var userId = user.Id;
            if (path == null)
                throw new ArgumentNullException("path");
            if (userId != AccessProvider.Current.GetCurrentUser().Id)
                Assert(path, creatorId, lastModifierId, null, PermissionType.SeePermissions);
            if (userId == -1)
                return PermittedLevel.All;
            bool isCreator = userId == creatorId;
            bool isLastModifier = user.Id == lastModifierId;
            return PermissionEvaluator.Instance.GetPermittedLevel(path.ToLower(), user, isCreator, isLastModifier);
        }

		public SecurityEntry[] GetAllEntries()
		{
            Assert(PermissionType.SeePermissions);
            return PermissionEvaluator.Instance.GetAllEntries(_node.Path.ToLower());
		}
        public SecurityEntry[] GetExplicitEntries()
        {
            Assert(PermissionType.SeePermissions);
            return PermissionEvaluator.Instance.GetExplicitEntries(_node.Path.ToLower());
        }

        public SecurityEntry[] GetEffectiveEntries()
        {
            Assert(PermissionType.SeePermissions);
            return GetEffectiveEntries(_node.Path.ToLower());
        }
        public static SecurityEntry[] GetEffectiveEntries(string path)
        {
            Assert(NodeHead.Get(path), PermissionType.SeePermissions);
            return PermissionEvaluator.Instance.GetEffectiveEntries(path.ToLower());
        }

        //========================================================================================================

        public bool IsInGroup(int groupId)
        {
            if (this._node is IUser)
                return PermissionEvaluator.Instance.IsInGroup(_node.Id, groupId);
            return DataProvider.IsInGroup(_node.Id, groupId);
        }
        public List<int> GetPrincipals()
        {
            var iUser = this._node as IUser;
            if (iUser != null)
                return PermissionEvaluator.Instance.GetPrincipals(iUser, false, false);

            return null;
        }

        //========================================================================================================

        public AclEditor GetAclEditor()
        {
            Assert(PermissionType.SeePermissions);
            return new AclEditor(_node);
        }

        public SnAccessControlList GetAcl()
        {
            return GetAcl(_node.Id, _node.Path, _node.CreatedById, _node.NodeModifiedById);
        }
        public static SnAccessControlList GetAcl(int nodeId, string path, int creatorId, int lastModifierId)
        {
            Assert(path, creatorId, lastModifierId, null, PermissionType.SeePermissions);
            return PermissionEvaluator.Instance.GetAcl(nodeId, path.ToLower(), creatorId, lastModifierId);
        }
        public void SetAcl(SnAccessControlList acl)
        {
            SetAcl(this._node, acl);
        }
        public static void SetAcl(SnAccessControlList acl, int nodeId)
        {
            var node = Node.LoadNode(nodeId);
            if (node == null)
                throw GetNodeNotFoundEx(nodeId);
            SetAcl(node, acl);
        }
        public static void SetAcl(SnAccessControlList acl, string path)
        {
            var node = Node.LoadNode(path);
            if (node == null)
                throw GetNodeNotFoundEx(path);
            SetAcl(node, acl);
        }
        private static void SetAcl(Node node, SnAccessControlList acl)
        {
            Assert(node, PermissionType.SetPermissions);
            var entriesToSet = GetEntriesFromAcl(new AclEditor(node), new AclEditor(node).Acl, acl);
            WriteEntries(entriesToSet);
            Reset();
        }
        private static IEnumerable<SecurityEntry> GetEntriesFromAcl(AclEditor ed, SnAccessControlList origAcl, SnAccessControlList acl)
        {
            var newEntries = new List<SecurityEntry>();

            foreach (var entry in acl.Entries)
            {
                var origEntry = origAcl.Entries.Where(x => x.Identity.NodeId == entry.Identity.NodeId && x.Propagates == entry.Propagates).FirstOrDefault();
                if (origEntry == null)
                {
                    ed.AddEntry(entry);
                }
                else
                {
                    //---- play modifications
                    var ident = entry.Identity.NodeId;
                    var propagates = entry.Propagates;
                    var perms = entry.Permissions.ToArray();
                    var origPerms = origEntry.Permissions.ToArray();

                    //---- reset deny bits
                    for (int i = ActiveSchema.PermissionTypes.Count - 1; i >= 0; i--)
                    {
                        var perm = perms[i];
                        var origPerm = origPerms[i];
                        if (perm.DenyEnabled)
                            if (origPerm.Deny && !perm.Deny) // reset
                            {
                                ed.SetPermission(ident, propagates, ActiveSchema.PermissionTypes[perm.Name], PermissionValue.NonDefined);
                                //Trace.WriteLine("@> Reset deny " + perm.Name);
                            }
                    }

                    //---- reset allow bits
                    for (int i = 0; i < ActiveSchema.PermissionTypes.Count; i++)
                    {
                        var perm = perms[i];
                        var origPerm = origPerms[i];
                        if (perm.AllowEnabled)
                            if (origPerm.Allow && !perm.Allow) // reset
                            {
                                ed.SetPermission(ident, propagates, ActiveSchema.PermissionTypes[perm.Name], PermissionValue.NonDefined);
                                //Trace.WriteLine("@> Reset allow " + perm.Name);
                            }
                    }
                    //---- set allow bits
                    for (int i = 0; i < ActiveSchema.PermissionTypes.Count; i++)
                    {
                        var perm = perms[i];
                        var origPerm = origPerms[i];
                        if (perm.AllowEnabled)
                            if (!origPerm.Allow && perm.Allow) // set
                            {
                                ed.SetPermission(ident, propagates, ActiveSchema.PermissionTypes[perm.Name], PermissionValue.Allow);
                                //Trace.WriteLine("@> Set allow " + perm.Name);
                            }
                    }
                    //---- set deny bits
                    for (int i = ActiveSchema.PermissionTypes.Count - 1; i >= 0; i--)
                    {
                        var perm = perms[i];
                        var origPerm = origPerms[i];
                        if (perm.DenyEnabled)
                            if (!origPerm.Deny && perm.Deny) // set
                            {
                                ed.SetPermission(ident, propagates, ActiveSchema.PermissionTypes[perm.Name], PermissionValue.Deny);
                                //Trace.WriteLine("@> Set deny " + perm.Name);
                            }
                    }

                    //---- reset entry if it is subset of the original (entry will be removed)
                    var newEntry = ed.GetEntry(entry.Identity.NodeId, entry.Propagates);
                    var newPerms = newEntry.Permissions.ToArray();
                    var deletable = true;
                    for (int i = 0; i < newPerms.Length; i++)
                    {
                        var newPerm = newPerms[i];
                        var origPerm = origPerms[i];
                        if (newPerm.AllowEnabled && newPerm.Allow)
                        {
                            deletable = false;
                            break;
                        }
                        if (newPerm.DenyEnabled && newPerm.Deny)
                        {
                            deletable = false;
                            break;
                        }
                    }
                    if (deletable)
                        newEntry.SetPermissionsBits(0, 0);
                }
            }
            var entries = PermissionEvaluator.Instance.SetAcl(ed.Acl);
            return entries;
        }

        //========================================================================================================

        private const int SEEBIT = 0x1;
        private const int OPENBIT = 0x2;
        private const int OPENMINORBIT = 0x4;
        private const int SAVEBIT = 0x8;
        private const int PUBLISHBIT = 0x10;
        private const int FORCECHECKINBIT = 0x20;
        private const int ADDNEWBIT = 0x40;
        private const int APPROVEBIT = 0x80;
        private const int DELETEBIT = 0x100;
        private const int RECALLOLDVERSIONBIT = 0x200;
        private const int DELETEOLDVERSIONBIT = 0x400;
        private const int SEEPERMISSIONSBIT = 0x800;
        private const int SETPERMISSIONSBIT = 0x1000;
        private const int RUNAPPLICATIONBIT = 0x2000;
        private const int MANAGELISTSANDWORKSPACESBIT = 0x4000;
        private const int SAVEGROUPBITS = 0x7F8;
        internal static void SetBits(ref int allowBits, ref int denyBits, PermissionType permissionType, PermissionValue permissionValue)
        {
            var actionBit = 0x1 << (permissionType.Id - 1);
            switch (permissionValue)
            {
                case PermissionValue.Deny:
                    if (actionBit == SEEBIT)
                        denyBits |= SAVEGROUPBITS + OPENMINORBIT + OPENBIT + SEEBIT + MANAGELISTSANDWORKSPACESBIT;
                    else if (actionBit == OPENBIT)
                        denyBits |= SAVEGROUPBITS + OPENMINORBIT + OPENBIT + MANAGELISTSANDWORKSPACESBIT;
                    else if (actionBit == OPENMINORBIT)
                        denyBits |= SAVEGROUPBITS + OPENMINORBIT + MANAGELISTSANDWORKSPACESBIT;
                    else if (actionBit == SEEPERMISSIONSBIT)
                        denyBits |= SETPERMISSIONSBIT;
                    else if (actionBit == SAVEBIT)
                        denyBits |= MANAGELISTSANDWORKSPACESBIT;
                    else if (actionBit == ADDNEWBIT)
                        denyBits |= MANAGELISTSANDWORKSPACESBIT;
                    else if (actionBit == DELETEBIT)
                        denyBits |= MANAGELISTSANDWORKSPACESBIT;
                    denyBits |= actionBit;
                    allowBits &= ~denyBits;
                    break;
                case PermissionValue.NonDefined:
                    var abits = 0;
                    var dbits = 0;
                    if (actionBit == SEEBIT)
                    {
                        abits |= SAVEGROUPBITS + OPENMINORBIT + OPENBIT + SEEBIT;
                        dbits |= ~(SEEBIT);
                    }
                    else if (actionBit == OPENBIT)
                    {
                        abits |= SAVEGROUPBITS + OPENMINORBIT + OPENBIT;
                        dbits |= ~(SEEBIT | OPENBIT);
                    }
                    else if (actionBit == OPENMINORBIT)
                    {
                        abits |= SAVEGROUPBITS + OPENMINORBIT;
                        dbits |= ~(SEEBIT | OPENBIT | OPENMINORBIT);
                    }
                    else if ((actionBit & SAVEGROUPBITS) != 0)
                    {
                        abits |= actionBit;
                        dbits |= ~(actionBit | OPENMINORBIT | OPENBIT | SEEBIT);
                    }
                    else if (actionBit == SEEPERMISSIONSBIT)
                    {
                        abits |= SETPERMISSIONSBIT + SEEPERMISSIONSBIT;
                        dbits |= ~SEEPERMISSIONSBIT;
                    }
                    else if (actionBit == SETPERMISSIONSBIT)
                    {
                        abits |= SETPERMISSIONSBIT;
                        dbits |= ~(SETPERMISSIONSBIT | SEEPERMISSIONSBIT);
                    }
                    else if (actionBit == RUNAPPLICATIONBIT)
                    {
                        abits |= RUNAPPLICATIONBIT;
                        dbits |= ~(RUNAPPLICATIONBIT);
                    }
                    else if (actionBit == MANAGELISTSANDWORKSPACESBIT)
                    {
                        abits |= MANAGELISTSANDWORKSPACESBIT;
                        dbits |= ~(MANAGELISTSANDWORKSPACESBIT);   
                    }
                    else
                    {
                        dbits = ~0;
                    }
                    allowBits &= ~abits;
                    denyBits &= dbits;
                    break;
                case PermissionValue.Allow:
                    if ((actionBit & SAVEGROUPBITS) > 0)
                        allowBits |= actionBit + SEEBIT + OPENBIT + OPENMINORBIT;
                    else if (actionBit == OPENMINORBIT)
                        allowBits |= actionBit + SEEBIT + OPENBIT;
                    else if (actionBit == OPENBIT)
                        allowBits |= actionBit + SEEBIT;
                    else if (actionBit == SETPERMISSIONSBIT)
                        allowBits |= SEEPERMISSIONSBIT;
                    else if (actionBit == MANAGELISTSANDWORKSPACESBIT)
                        allowBits |= actionBit + SEEBIT + OPENBIT + OPENMINORBIT + SAVEBIT + ADDNEWBIT + DELETEBIT;
                    allowBits |= actionBit;
                    denyBits &= ~allowBits;
                    break;
                default:
                    throw new NotSupportedException("Unknown PermissionValue: " + permissionValue);
            }
        }
        internal static void SetBits(ref int allowBits, ref int denyBits)
        {
            if ((denyBits & SEEBIT) != 0)
                denyBits |= SAVEGROUPBITS + OPENMINORBIT + OPENBIT + MANAGELISTSANDWORKSPACESBIT;
            else if ((denyBits & OPENBIT) != 0)
                denyBits |= SAVEGROUPBITS + OPENMINORBIT + MANAGELISTSANDWORKSPACESBIT;
            else if ((denyBits & OPENMINORBIT) != 0)
                denyBits |= SAVEGROUPBITS + MANAGELISTSANDWORKSPACESBIT;
            if ((denyBits & SAVEBIT) != 0)
                denyBits |= MANAGELISTSANDWORKSPACESBIT;
            if ((denyBits & ADDNEWBIT) != 0)
                denyBits |= MANAGELISTSANDWORKSPACESBIT;
            if ((denyBits & DELETEBIT) != 0)
                denyBits |= MANAGELISTSANDWORKSPACESBIT;
            if ((denyBits & SEEPERMISSIONSBIT) != 0)
                denyBits |= SETPERMISSIONSBIT;

            var defBits = allowBits | denyBits;
            if ((defBits & SEEBIT) == 0)
                allowBits &= ~(SAVEGROUPBITS + OPENMINORBIT + OPENBIT);
            else if ((defBits & OPENBIT) == 0)
                allowBits &= ~(SAVEGROUPBITS + OPENMINORBIT);
            else if ((defBits & OPENMINORBIT) == 0)
                allowBits &= ~SAVEGROUPBITS;
            if ((defBits & SEEPERMISSIONSBIT) == 0)
                allowBits &= ~SETPERMISSIONSBIT;
        }
        private static string BitsToString(int allowBits, int denyBits)
        {
            var chars = new char[ActiveSchema.PermissionTypes.Count];
            var max = chars.Length - 1;
            for (int i = 0; i < chars.Length; i++)
            {
                if ((allowBits & (1 << i)) != 0) chars[max - i] = '+';
                else if ((denyBits & (1 << i)) != 0) chars[max - i] = '-';
                else chars[max - i] = '_';
            }
            return new String(chars);
        }

        //========================================================================================================

        private static Exception GetNodeNotFoundEx(object idOrPath)
        {
            throw new InvalidOperationException("Node is not found: " + idOrPath);
        }
/*!!!*/ private static Exception GetAccessDeniedException(string path, int creatorId, int lastModifierId, string message, PermissionType[] permissionTypes, IUser user)
        {
            //TODO: #### az exception-ben legyen informacio, hogy a see pattant-e el!

            PermissionType deniedPermission = null;
            foreach (var permType in permissionTypes)
            {
                if (!HasSubTreePermission(path, creatorId, lastModifierId, permType))
                {
                    deniedPermission = permType;
                    break;
                }
            }

            if (deniedPermission == null)
                throw new SenseNetSecurityException(path, null, user);
            if (message != null)
                throw new SenseNetSecurityException(path, deniedPermission, user, message);
            else
                throw new SenseNetSecurityException(path, deniedPermission, user);
        }
        
        private static void WriteEntries(IEnumerable<SecurityEntry> entries)
        {
            foreach (var entry in entries)
                DataProvider.Current.SetPermission(entry);
        }

    }
}
