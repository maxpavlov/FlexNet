using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Storage.Security
{
    public class AclEditor
    {
        private SnAccessControlList acl;
        internal SnAccessControlList Acl { get { return acl; } set { acl = value; } }

        private Node node;

        private List<AclEditor> mergedEditors = new List<AclEditor>();

        public AclEditor(Node node)
        {
            this.node = node;
            this.acl = node.Security.GetAcl();
        }

        public AclEditor Merge(AclEditor other)
        {
            if (other.node.Id == this.node.Id)
                throw new InvalidOperationException("Cannot merge with same Node.");
            mergedEditors.Add(other);
            return this;
        }

        public void Apply()
        {
            ApplyChanges();
            SecurityHandler.Reset();
        }
        private void ApplyChanges()
        {
            foreach (var editor in mergedEditors)
                editor.Apply();
            node.Security.SetAcl(this.acl);
        }

        public AclEditor SetPermission(ISecurityMember securityMember, bool propagates, PermissionType permissionType, PermissionValue permissionValue)
        {
            return SetPermission(securityMember.Id, propagates, permissionType, permissionValue);
        }
        internal AclEditor SetPermission(int principalId, bool propagates, PermissionType permissionType, PermissionValue permissionValue)
        {
            var entry = GetEntry(principalId, propagates);
            var perm = GetSnPerm(entry, permissionType);
            int allowBits;
            int denyBits;
            entry.GetPermissionBits(out allowBits, out denyBits);
            SecurityHandler.SetBits(ref allowBits, ref denyBits, permissionType, permissionValue);
            entry.SetPermissionsBits(allowBits, denyBits);
            return this;
        }
        private static void SetSnPerm(SnPermission perm, PermissionValue permissionValue)
        {
            switch (permissionValue)
            {
                case PermissionValue.NonDefined:
                    if (perm.Allow && perm.AllowFrom == null)
                        perm.Allow = false;
                    if (perm.Deny && perm.DenyFrom == null)
                        perm.Deny = false;
                    break;
                case PermissionValue.Allow:
                    if (!perm.Allow)
                        perm.Allow = true;
                    if (perm.Deny)
                        perm.Deny = false;
                    break;
                case PermissionValue.Deny:
                    if (!perm.Deny)
                        perm.Deny = true;
                    if (perm.Allow)
                        perm.Allow = false;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        //===========================================

        internal SnAccessControlEntry GetEntry(ISecurityMember principal, bool propagates)
        {
            return GetEntry(principal.Id, propagates) ?? CreateEntry(principal.Id, propagates);
        }
        internal SnAccessControlEntry GetEntry(int principalId, bool propagates)
        {
            return SearchEntry(principalId, propagates) ?? CreateEntry(principalId, propagates);
        }
        private SnPermission GetSnPerm(SnAccessControlEntry entry, PermissionType permType)
        {
            return entry.Permissions.Where(p => p.Name == permType.Name).First();
        }
        private SnAccessControlEntry SearchEntry(int principalId, bool propagates)
        {
            return acl.Entries.Where(e => e.Identity.NodeId == principalId && e.Propagates == propagates).FirstOrDefault();
        }
        private SnAccessControlEntry CreateEntry(int principalId, bool propagates)
        {
            var entry = SnAccessControlEntry.CreateEmpty(principalId, propagates); //TODO: CreateEmpty(principal);
            var list = acl.Entries.ToList();
            list.Add(entry);
            acl.Entries = list;
            return entry;
        }
        private void RemoveEntry(SnAccessControlEntry entry)
        {
            acl.Entries = acl.Entries.Except(new SnAccessControlEntry[] { entry }).ToList();
        }

        internal void AddEntry(SnAccessControlEntry entry)
        {
            var newEntry = CreateEntry(entry.Identity.NodeId, entry.Propagates);
            int allowBits, denyBits;
            entry.GetPermissionBits(out allowBits, out denyBits);
            SecurityHandler.SetBits(ref allowBits, ref denyBits);
            newEntry.SetPermissionsBits(allowBits, denyBits);
            var list = acl.Entries.ToList();
            list.Add(newEntry);
            acl.Entries = list.ToArray();
        }
    }
}
