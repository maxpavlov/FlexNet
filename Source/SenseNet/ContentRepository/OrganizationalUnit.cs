using System;
using System.Collections.Generic;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Security.ADSync;

namespace SenseNet.ContentRepository
{
	[ContentHandler]
    public class OrganizationalUnit : Folder, IOrganizationalUnit, IADSyncable
    {
        [Obsolete("Use typeof(OrganizationalUnit).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(OrganizationalUnit).Name;

        public OrganizationalUnit(Node parent) : this(parent, null) { }
		public OrganizationalUnit(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected OrganizationalUnit(NodeToken token) : base(token) { }

        private bool _syncObject = true;

        public override void Save()
        {
            var originalId = this.Id;

            base.Save();

            // AD Sync
            if (_syncObject)
            {
                ADFolder.SynchADContainer(this, originalId);
            }
            // default: object should be synced. if it was not synced now (sync properties updated only) next time it should be.
            _syncObject = true;
        }

        public override void Save(SavingMode mode)
        {
            var originalId = this.Id;

            base.Save(mode);

            // AD Sync
            if (_syncObject)
            {
                ADFolder.SynchADContainer(this, originalId);
            }
            // default: object should be synced. if it was not synced now (sync properties updated only) next time it should be.
            _syncObject = true;
        }

	    public override void ForceDelete()
        {
            base.ForceDelete();

            // AD Sync
            var ADProvider = DirectoryProvider.Current;
            if (ADProvider != null)
            {
                ADProvider.DeleteADObject(this);
            }
        }

        public override bool IsTrashable
        {
            get
            {
                return false;
            }
        }

        public override void MoveTo(Node target)
        {
            base.MoveTo(target);

            // AD Sync
            var ADProvider = DirectoryProvider.Current;
            if (ADProvider != null)
            {
                var targetNodePath = RepositoryPath.Combine(target.Path, this.Name);
                ADProvider.UpdateADContainer(this, targetNodePath);
            }
        }

        //=================================================================================== Events
        protected override void OnMoving(object sender, SenseNet.ContentRepository.Storage.Events.CancellableNodeOperationEventArgs e)
        {
            // AD Sync check
            var ADProvider = DirectoryProvider.Current;
            if (ADProvider != null)
            {
                var targetNodePath = RepositoryPath.Combine(e.TargetNode.Path, this.Name);
                var allowMove = ADProvider.AllowMoveADObject(this, targetNodePath);
                if (!allowMove)
                {
                    e.CancelMessage = "Moving of synced nodes is only allowed within AD server bounds!";
                    e.Cancel = true;
                }
            }

            base.OnMoving(sender, e);
        }

        //=================================================================================== IADSyncable Members
        public void UpdateLastSync(System.Guid? guid)
        {
            if (guid.HasValue)
                this["SyncGuid"] = ((System.Guid)guid).ToString();
            this["LastSync"] = System.DateTime.Now;

            // update object without syncing to AD
            _syncObject = false;

            this.Save();
        }

        //=================================================================================== ISecurityContainer members
        public bool IsMember(IUser user)
        {
            return user.IsInOrganizationalUnit(this);
        }
    }
}