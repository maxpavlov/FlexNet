using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Security
{
    public partial class SnPermission
    {
        public bool AllowEnabled
        {
            get
            {
                return string.IsNullOrEmpty(this.AllowFrom);
            }
        }

        public bool DenyEnabled
        {
            get
            {
                return string.IsNullOrEmpty(this.DenyFrom);
            }
        }

        public PermissionValue ToPermissionValue()
        {
            if (Deny)
                return PermissionValue.Deny;
            if (Allow)
                return PermissionValue.Allow;
            return PermissionValue.NonDefined;
        }
    }
}
