using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.Helpers
{
    public class Security
    {
        public static bool IsInRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                return false;

            IGroup roleNode;

            //we need to use system account here to avoid access denied exception
            using (new SystemAccount())
            {
                roleNode = Node.LoadNode("/Root/IMS/BuiltIn/Portal/" + role) as IGroup;
            }

            return roleNode != null && User.Current.IsInGroup(roleNode);
        }

        public static bool IsUserInRole(string rolePath, string userPath)
        {
            if (string.IsNullOrEmpty(rolePath) || string.IsNullOrEmpty(userPath))
                return false;

            var userNode = Node.LoadNode(userPath) as User;

            //we need to use system account here to avoid access denied exception
            using (new SystemAccount())
            {
                var roleNode = Node.LoadNode(rolePath) as IGroup;
                if (roleNode != null && userNode != null)
                    return userNode.IsInGroup(roleNode);
            }

            return false;
        }
    }
}
