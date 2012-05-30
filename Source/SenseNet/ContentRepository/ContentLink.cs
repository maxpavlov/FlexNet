using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class ContentLink : GenericContent
    {
        public ContentLink(Node parent) : this(parent, null) { }
        public ContentLink(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected ContentLink(NodeToken tk) : base(tk) { }

        private bool? _isAlive;
        public bool IsAlive
        {
            get
            {
                //check contentlink only once for performance reasons
                if (!_isAlive.HasValue)
                {
                    using (new SystemAccount())
                    {
                        var l = LinkedContent;

                        _isAlive = l != null && l.Security.HasPermission(AccessProvider.Current.GetOriginalUser(),
                                                            PermissionType.See, PermissionType.Open);
                    }
                }

                return _isAlive.Value;
            }
        }

        [RepositoryProperty("Link", RepositoryDataType.Reference)]
        public Node Link
        {
            get { return base.GetReference<Node>("Link"); }
            set
            {
                base.SetReference("Link", value);
                _isAlive = null;
                _resolved = false;
            }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "Link":
                    return this.Link;
                //case "Id":
                //    return base.Id;
                //case "Name":
                //    return base.Name;
                //case "VersionId":
                //    return base.VersionId;
                //case "Path":
                //    return base.Path;
                //case "ParentId":
                //    return base.ParentId;
                //case "DisplayName":
                //    return base.DisplayName;
                default:
                    //if (this.IsAlive && HasField(name))
                    //    return LinkedContent.GetProperty(name);
                    //else
                        return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "Link":
                    this.Link = (Node)value;
                    break;
                //case "Name":
                //    base.Name = (string)value;
                //    break;
                default:
                    //if (this.IsAlive && HasField(name))
                    //    throw new InvalidOperationException("Cannot write a property of the linked content.");
                    //else
                        base.SetProperty(name, value);
                    break;
            }
        }

        private bool _resolved;
        private GenericContent _linkedContent;
        public GenericContent LinkedContent
        {
            get
            {
                if (!_resolved)
                {
                    _linkedContent = Link as GenericContent;
                    _resolved = true;
                }
                return _linkedContent;
            }
        }

        public override string Icon
        {
            get
            {
                if (IsAlive)
                    return LinkedContent.Icon;
                return base.Icon;
            }
        }

        private  bool HasField(string name)
        {
            //var ct = ContentType.GetByName(LinkedContent.NodeType.Name);
            //return LinkedContent.HasProperty(name) || ct.FieldSettings.Count(fs => fs.Name == name) > 0; 

            if (LinkedContent.HasProperty(name))
                return true;
            var ct = ContentType.GetByName(LinkedContent.NodeType.Name);
            return ct.FieldSettings.Exists(delegate(FieldSetting fs) { return fs.Name == name; });
        }
    }
}
