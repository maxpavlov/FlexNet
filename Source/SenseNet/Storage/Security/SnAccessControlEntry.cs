using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Storage.Security
{
    [Serializable]
    [DebuggerDisplay("Ident: {Identity.NodeId}-{Identity.Name}, Propagates: {Propagates}, Permissions: {PermissionsToString()} ")]
    public partial class SnAccessControlEntry
    {
        public SnIdentity Identity { get; set; }
        public IEnumerable<SnPermission> Permissions { get; set; }
        public bool Propagates { get; set; }

        public string PermissionsToString()
        {
            var chars = new char[ActiveSchema.PermissionTypes.Count];
            foreach (var perm in Permissions)
            {
                var i = chars.Length - ActiveSchema.PermissionTypes[perm.Name].Id;
                if (perm.Allow)
                    chars[i] = '+';
                else if (perm.Deny)
                    chars[i] = '-';
                else
                    chars[i] = '_';
            }
            return new String(chars);
        }
    }
}
