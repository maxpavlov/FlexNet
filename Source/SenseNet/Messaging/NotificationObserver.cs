using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Messaging
{
    internal class NotificationObserver : NodeObserver
    {
        protected override void OnNodeCreated(object sender, NodeEventArgs e)
        {
            if (!Configuration.Enabled)
                return;
            
            var node = e.SourceNode;
            var who = GetLastModifierUserName(node);
            var type = node.CopyInProgress ? NotificationType.CopiedFrom : NotificationType.Created;
            Event.CreateAndSave(node, type, who);
        }
        protected override void OnNodeModified(object sender, NodeEventArgs e)
        {
            if (!Configuration.Enabled)
                return;

            var node = e.SourceNode;
            var who = GetLastModifierUserName(node);
            var type = node.Version.Status == VersionStatus.Approved ? NotificationType.MajorVersionModified : NotificationType.MinorVersionModified;
            if (node.Path == e.OriginalSourcePath)
            {
                Event.CreateAndSave(node, type, who);
                return;
            }
            var currentUser = User.Current;
            var creatorId = node.CreatedById;
            var lastModifierId = currentUser.Id;
            Event.CreateAndSave(e.OriginalSourcePath, creatorId, lastModifierId, NotificationType.RenamedTo, who);
            Event.CreateAndSave(node.Path, creatorId, lastModifierId, NotificationType.RenamedFrom, who);
        }
        protected override void OnNodeDeleted(object sender, NodeEventArgs e)
        {
            if (!Configuration.Enabled)
                return;

            OnNodeDeletedPhysically(sender, e);
        }
        protected override void OnNodeDeletedPhysically(object sender, NodeEventArgs e)
        {
            if (!Configuration.Enabled)
                return;

            var currentUser = User.Current;
            var who = GetUserName(currentUser);
            var node = e.SourceNode;

            Event.CreateAndSave(node, NotificationType.Deleted, who);
        }
        protected override void OnNodeMoved(object sender, NodeOperationEventArgs e)
        {
            if (!Configuration.Enabled)
                return;

            var currentUser = User.Current;
            var srcnode = e.SourceNode;
            var creatorId = srcnode.CreatedById;
            var lastModifierId = currentUser.Id;
            var who = GetUserName(currentUser);
            if (IsInTrash(e.OriginalSourcePath))
            {
                Event.CreateAndSave(srcnode.Path, creatorId, lastModifierId, NotificationType.Restored, who);
                return;
            }
            if (IsInTrash(srcnode.Path))
            {
                Event.CreateAndSave(e.OriginalSourcePath, creatorId, lastModifierId, NotificationType.Deleted, who);
                return;
            }
            Event.CreateAndSave(e.OriginalSourcePath, creatorId, lastModifierId, NotificationType.MovedTo, who);
            Event.CreateAndSave(srcnode.Path, creatorId, lastModifierId, NotificationType.MovedFrom, who);
        }

        private string GetLastModifierUserName(Node node)
        {
            return GetUserName(node.ModifiedBy as IUser);
        }
        private string GetUserName(IUser user)
        {
            var name = user.FullName;
            if (!String.IsNullOrEmpty(name))
                return name;
            return user.Username;
        }
        private bool IsInTrash(string path)
        {
            return IsIn(path, TrashBin.TrashBinPath);
        }
        private bool IsIn(string path, string containerPath)
        {
            if (path.Length <= containerPath.Length + 1)
                return false;
            if (!path.StartsWith(containerPath))
                return false;
            var lastChar = path[containerPath.Length];
            var separators = RepositoryPath.PathSeparatorChars;
            for (int i = 0; i < separators.Length; i++)
                if (lastChar == separators[i])
                    return true;
            return false;
        }
    }
}
