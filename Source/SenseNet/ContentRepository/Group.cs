using System;
using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Security.ADSync;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class Group : GenericContent, IGroup, IADSyncable
    {
        [Obsolete("Use typeof(Group).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(Group).Name;

        public Group(Node parent) : this(parent, null) { }
        public Group(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected Group(NodeToken token) : base(token) { }

        //////////////////////////////////////// Public Properties ////////////////////////////////////////

        [RepositoryProperty("Members", RepositoryDataType.Reference)]
        public IEnumerable<Node> Members
        {
            get { return this.GetReferences("Members"); }
            set { this.SetReferences("Members", value); }
        }
        public List<Group> Groups
        {
            get
            {
                var groups = new List<Group>();
                foreach (Node node in Members)
                    if (node is Group)
                        groups.Add((Group)node);
                return groups;
            }
        }
        public List<User> Users
        {
            get
            {
                var users = new List<User>();
                foreach (Node node in Members)
                    if (node is User)
                        users.Add((User)node);
                return users;
            }
        }

        private Domain _domain;
        public Domain Domain
        {
            get { return _domain ?? (_domain = Node.GetAncestorOfType<Domain>(this)); }
        }

        //////////////////////////////////////// Private Members ////////////////////////////////////////
        private bool _syncObject = true;


        //////////////////////////////////////// Static Members ////////////////////////////////////////

        public static Group Administrators
        {
            get { return (Group)Node.LoadNode(RepositoryConfiguration.AdministratorsGroupId); }
        }
        public static Group Everyone
        {
            get { return (Group)Node.LoadNode(RepositoryConfiguration.EveryoneGroupId); }
        }
        public static Group Creators
        {
            get { return (Group)Node.LoadNode(RepositoryConfiguration.CreatorsGroupId); }
        }
        public static Group LastModifiers
        {
            get { return (Group)Node.LoadNode(RepositoryConfiguration.LastModifiersGroupId); }
        }
        public static Group RegisteredUsers
        {
            get { return Node.Load<Group>("/Root/IMS/BuiltIn/Portal/RegisteredUsers"); }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "Members":
                    return this.Members;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            base.SetProperty(name, value);
        }

        public List<IUser> GetMemberUsers()
        {
            List<IUser> memberUsers = new List<IUser>();
            foreach (Node node in Members)
            {
                IUser user = node as IUser;
                if (user != null) memberUsers.Add(user);
            }
            return memberUsers;
        }

        public List<IGroup> GetMemberGroups()
        {
            List<IGroup> memberGroups = new List<IGroup>();
            foreach (Node node in Members)
            {
                IGroup group = node as IGroup;
                if (group != null) memberGroups.Add(group);
            }
            return memberGroups;
        }

        public List<IUser> GetAllMemberUsers()
        {
            List<IUser> memberUsers = new List<IUser>();

            GetMembers(memberUsers, this);
            
            return memberUsers;
        }

        private void GetMembers(List<IUser> memberUsers, IGroup group)
        {
            foreach (var member in group.Members)
            {
                if (member is IUser)
                {
                    memberUsers.Add(member as IUser);
                }
                else if (member is IGroup)
                {
                    GetMembers(memberUsers, member as Group);
                }
            }
        }

        public List<IGroup> GetAllMemberGroups()
        {
            throw new NotImplementedException();
        }

        public void AddMember(IGroup group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            Node groupNode = group as Node;
            if (groupNode == null)
                throw new ArgumentOutOfRangeException("group", "The given value is not a Node.");

            this.AddReference("Members", groupNode);
            Save();
        }

        public void AddMember(IUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            Node userNode = user as Node;
            if (userNode == null)
                throw new ArgumentOutOfRangeException("user", "The given value is not a Node.");

            this.AddReference("Members", userNode);
            Save();
        }

        public void RemoveMember(IGroup group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            Node groupNode = group as Node;
            if (groupNode == null)
                throw new ArgumentOutOfRangeException("group", "The given value is not a Node.");

            this.RemoveReference("Members", groupNode);

            Save();
        }

        public void RemoveMember(IUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            Node userNode = user as Node;
            if (userNode == null)
                throw new ArgumentOutOfRangeException("user", "The given value is not a Node.");

            this.RemoveReference("Members", userNode);

            Save();
        }

        public override void Save()
        {
            AssertValidMembers();

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
            AssertValidMembers();

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
                ADProvider.UpdateADContainer(this, RepositoryPath.Combine(target.Path, this.Name));
            }
        }

        protected void AssertValidMembers()
        {
            //only check existing groups
            if (this.Id == 0)
                return;

            if (RepositoryConfiguration.SpecialGroupNames.Contains(this.Name))
            {
                if (this.Members.Count() > 0)
                    throw new InvalidOperationException(string.Format("The {0} group is a special system group, members cannot be added to it.", this.DisplayName));
            }

            foreach (var member in this.Members)
            {
                var group = member as Group;
                if (group == null)
                    continue;

                if (group.Id == this.Id)
                    throw new InvalidOperationException(string.Format("Group cannot contain itself as a member. Please remove {0} from the Members list.", this.DisplayName));

                if (this.Security.IsInGroup(group.Id))
                    throw new InvalidOperationException(string.Format("Circular group membership is not allowed. Please remove {0} from the Members list.", group.DisplayName));
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
        public void UpdateLastSync(Guid? guid)
        {
            if (guid.HasValue)
                this["SyncGuid"] = ((Guid)guid).ToString();
            this["LastSync"] = DateTime.Now;

            // update object without syncing to AD
            _syncObject = false;

            this.Save();
        }
    }
}
