using System;
using System.Runtime.Serialization;
using SenseNet.ContentRepository.Storage.Schema;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Security
{
    [Serializable]
    public class SenseNetSecurityException : Exception
    {
        public SenseNetSecurityException(string path, PermissionType permissionType)
            : base(GetMessage(null, path, 0, permissionType, null)) { }
        public SenseNetSecurityException(string path, PermissionType permissionType, IUser user)
            : base(GetMessage(null, path, 0, permissionType, user)) { }
        public SenseNetSecurityException(string path, PermissionType permissionType, IUser user, string message)
            : base(GetMessage(message, path, 0, permissionType, user)) { }
        public SenseNetSecurityException(string path, string message)
            : base(GetMessage(message, path, 0, null, null)) { }

        public SenseNetSecurityException(int nodeId, PermissionType permissionType)
            : base(GetMessage(null, null, nodeId, permissionType, null)) { }
        public SenseNetSecurityException(int nodeId, PermissionType permissionType, IUser user)
            : base(GetMessage(null, null, nodeId, permissionType, user)) { }
        public SenseNetSecurityException(int nodeId, PermissionType permissionType, IUser user, string message)
            : base(GetMessage(message, null, nodeId, permissionType, user)) { }
        public SenseNetSecurityException(int nodeId, string message)
            : base(GetMessage(message, null, nodeId, null, null)) { }

        public SenseNetSecurityException(string message) : base(message)
        { }

        protected SenseNetSecurityException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}

        private static string GetMessage(string msg, string path, int nodeId, PermissionType permissionType, IUser user)
        {
            var sb = new StringBuilder(msg ?? "Access denied.");
            if (path != null)
                sb.Append(" Path: ").Append(path);
            if (nodeId != 0)
                sb.Append(" NodeId: ").Append(nodeId);
            if (permissionType != null)
                sb.Append(" PermissionType: ").Append(permissionType.Name);
            if (user != null)
                sb.Append(" User: ").Append(user.Username).Append(" UserId: ").Append(user.Id);
            return sb.ToString();
        }
    }
}