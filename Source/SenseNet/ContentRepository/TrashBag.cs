using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class TrashBag : Folder
    {
        public TrashBag(Node parent) : this(parent, null) { }
        public TrashBag(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected TrashBag(NodeToken tk) : base(tk) { }

        [RepositoryProperty("KeepUntil", RepositoryDataType.DateTime)]
        public DateTime KeepUntil
        {
            get { return (DateTime)base.GetProperty("KeepUntil"); }
            set { this["KeepUntil"] = value; }
        }

        [RepositoryProperty("OriginalPath", RepositoryDataType.String)]
        public string OriginalPath
        {
            get { return (string)base.GetProperty("OriginalPath"); }
            set { this["OriginalPath"] = value; }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "KeepUntil":
                    return this.KeepUntil;
                case "OriginalPath":
                    return this.OriginalPath;
                default:
                    return base.GetProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "KeepUntil":
                    this.KeepUntil = (DateTime)value;
                    break;
                case "OriginalPath":
                    this.OriginalPath = (string)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        public bool IsPurgeable
        {
            get { return (DateTime.Now > KeepUntil); }
        }

        public override string Icon
        {
            get
            {
                return DeletedContent != null ? DeletedContent.Icon : base.Icon;
            }
        }

        public override bool IsTrashable
        {
            get
            {
                return false;
            }
        }

        public override void ForceDelete()
        {
            if (!IsPurgeable)
                throw new ApplicationException("Trashbags cannot be purged before their minimum retention date");
            base.ForceDelete();
        }

        public override void Delete()
        {
            ForceDelete();
        }

        private void Destroy()
        {
            using (new SystemAccount())
            {
                this.KeepUntil = DateTime.Today.AddDays(-1);
                this.ForceDelete();    
            }
        }

        public static TrashBag BagThis(GenericContent node)
        {
            var bin = TrashBin.Instance;
            if (bin == null)
                return null;

            if (node == null)
                throw new ArgumentNullException("node");

            //creating a bag has nothing to do with user permissions: Move will handle that
            TrashBag bag = null;
            
            using (new SystemAccount())
            {
                bag = new TrashBag(bin)
                          {
                              KeepUntil = DateTime.Now.AddDays(bin.MinRetentionTime),
                              OriginalPath = RepositoryPath.GetParentPath(node.Path),
                              DisplayName = node.DisplayName,
                              Link = node
                          };
                bag.Save();

                CopyPermissions(node, bag);

                //add delete permission for the owner
                //bag.Security.SetPermission(User.Current, true, PermissionType.Delete, PermissionValue.Allow);
            }

            try
            {
                Node.Move(node.Path, bag.Path);
            }
            catch(Exception ex)
            {
                Logger.WriteException(ex);

                bag.Destroy();

                throw new InvalidOperationException("Error moving item to the trash", ex);
            }

            return bag;
        }

        private static void CopyPermissions(Node source, Node target)
        {
            if (source == null || source.ParentId == 0 || target == null)
                return;

            foreach (var entry in source.Security.GetEffectiveEntries())
            {
                target.Security.SetPermissions(entry.PrincipalId, true, entry.PermissionValues);
            }
        }



        //////////////////////////////////////////

        //private bool? _isAlive;
        //public bool IsAlive
        //{
        //    get
        //    {
        //        //check contentlink only once for performance reasons
        //        if (!_isAlive.HasValue)
        //        {
        //            using (new SystemAccount())
        //            {
        //                var l = LinkedContent;
        //                _isAlive = l != null && l.Security.HasPermission(AccessProvider.Current.GetOriginalUser(),
        //                                                    PermissionType.See, PermissionType.Open);
        //            }
        //        }
        //        return _isAlive.Value;
        //    }
        //}

        [RepositoryProperty("Link", RepositoryDataType.Reference)]
        private Node Link
        {
            get { return base.GetReference<Node>("Link"); }
            set
            {
                base.SetReference("Link", value);
                _originalContent = value as GenericContent;
                //_isAlive = null;
            }
        }

        private bool _resolved;
        private GenericContent _originalContent;
        public GenericContent DeletedContent
        {
            get
            {
                if (_resolved)
                    return _originalContent;
                 _originalContent = Link as GenericContent;
                 _resolved = true;
                return _originalContent;
            }
        }

    }
}
