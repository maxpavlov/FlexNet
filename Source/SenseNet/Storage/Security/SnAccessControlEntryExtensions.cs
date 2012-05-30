using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Security
{
    public partial class SnAccessControlEntry
    {
        public static SnAccessControlEntry CreateEmpty(int principalId, bool propagates)
        {
            var perms = new List<SnPermission>();
            foreach (var permType in ActiveSchema.PermissionTypes) // .OrderBy(x => x.Id)
                perms.Add(new SnPermission { Name = permType.Name });
            return new SnAccessControlEntry { Identity = SnIdentity.Create(principalId), Permissions = perms, Propagates = propagates };
        }
        public void GetPermissionBits(out int allowBits, out int denyBits)
        {
            allowBits = 0;
            denyBits = 0;
            var index = 0;
            foreach (var perm in this.Permissions)
            {
                if (perm.Deny)
                    denyBits |= 1 << index;
                else if (perm.Allow)
                    allowBits |= 1 << index;
                index++;
            }
        }
        public void SetPermissionsBits(int allowBits, int denyBits)
        {
            var index = 0;
            foreach (var perm in this.Permissions)
            {
                var mask = 1 << index;
                perm.Deny = (denyBits & mask) != 0;
                perm.Allow = (allowBits & mask) != 0;
                index++;
            }
        }
    }
}
