using System;
using System.Linq;
using System.Runtime.Serialization;
using SenseNet.Search;
using sn = SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.Xml.Serialization;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.Services.ContentStore
{
    [DataContract]
    [KnownType(typeof(EntityReference))]
    [XmlInclude(typeof(EntityReference))]
    [KnownType(typeof(ContentProperty))]
    [XmlInclude(typeof(ContentProperty))]
    public class Content 
    {
        #region Members

        // Non-serialized stuff /////////////////////////////////////////////
        
        private Node ContentNode { get; set; }

        private sn.Content _contentFromNode;
        private sn.Content ContentFromNode
        {
            get { return _contentFromNode ?? (_contentFromNode = sn.Content.Create(ContentNode)); }
        }

        private sn.Schema.ContentType _contentAsType;
        private sn.Schema.ContentType ContentAsType
        {
            get { return _contentAsType ?? (_contentAsType = ContentNode as sn.Schema.ContentType); }
        }

        private bool withChildren;

        // Serialized interface /////////////////////////////////////////////

        private int? _id;
        [DataMember]
        public int Id
        {
            get { return _id ?? ContentNode.Id; }
            set { _id = value; }
        }

        // hack for JS case sensitivity
        [DataMember]
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }
        // endhack

        private string _name;
        [DataMember]
        public string Name
        {
            get { return _name ?? ContentNode.Name; }
            set { _name = value; }
        }

        private string _path;
        [DataMember]
        public string Path
        {
            get { return _path ?? ContentNode.Path; }
            set { _path = value; } 
        }

        // hack, uses and references unknown
        [DataMember]
        public string text
        {
            get { return Name; }
            set { Name = value; }
        }
        // endhack

        private string _displayName;
        [DataMember]
        public string DisplayName
        {
            get
            {
                if (_displayName != null)
                    return _displayName;

                if (ContentAsType != null)
                    return ContentAsType.DisplayName;

                // Needs revision! This shouldn't be what it is.
                return ContentFromNode.DisplayName;
            }
            set { _displayName = value; }
        }

        private string _description;
        [DataMember]
        public string Description
        {
            get
            {
                if (_description != null)
                    return _description;

                if (ContentAsType != null)
                    return ContentAsType.Description;

                return ContentFromNode.Description;
            }
            set { _description = value; }
        }

        private string _descriptionProp;
        [DataMember]
        public string DescriptionProp
        {
            get
            {
                if (_descriptionProp == null)
                    _descriptionProp = (ContentNode.HasProperty("Description") && ContentNode["Description"] != null) ? ContentNode["Description"].ToString() : string.Empty;
                return _descriptionProp;
            }
            set { _descriptionProp = value; }
        }

        [DataMember]
        public EntityReference _dummy;
        
        private string _nodeTypeName;
        [DataMember]
        public string NodeTypeName
        {
            get { return _nodeTypeName ?? ContentNode.NodeType.Name; }
            set { _nodeTypeName = value; }
        }

        private string _nodeTypeTitle;
        [DataMember]
        public string NodeTypeTitle
        {
            get 
            {
                if (_nodeTypeTitle != null)
                    return _nodeTypeTitle;

                var cType = ContentRepository.Schema.ContentType.GetContentTypes().Where(c => c.Name == ContentNode.NodeType.Name).FirstOrDefault();
                if (cType != null)
                {
                    if (!string.IsNullOrEmpty(cType.DisplayName))
                        return cType.DisplayName;
                    return cType.Name;
                }
                return string.Empty;
            }
            set { _nodeTypeTitle = value; }
        }

        private object _nodeType;
        [DataMember]
        public object NodeType
        {
            get { return _nodeType ?? new EntityReference() { Uri = ContentNode.NodeType.NodeTypePath }; }
            set { _nodeType = value; }
        }

        private string _contentTypeName;
        [DataMember]
        public string ContentTypeName
        {
            get { return _contentTypeName ?? ContentFromNode.ContentType.Name; }
            set { _contentTypeName = value; }
        }

        private object _contentType;
        [DataMember]
        public object ContentType
        {
            get { return _contentType ?? new EntityReference() { Uri = ContentFromNode.ContentType.Path }; }
            set { _contentType = value; }
        }

        private string _icon;
        [DataMember]
        public String Icon
        {
            get { return _icon ?? ContentFromNode.Icon; }
            set { _icon = value; }
        }

        private string _iconPath;
        [DataMember]
        public String IconPath
        {
            get
            {
                if (_iconPath == null) {
                    _iconPath = string.Concat("/Root/Global/images/icons/16/", ContentFromNode.Icon, ".png"); }
                return _iconPath; }
            set { _iconPath = value; }
        }

        private int? _index;
        [DataMember]
        public int Index
        {
            get { return _index ?? ContentNode.Index; }
            set { _index = value; }
        }

        private DateTime? _creationDate;
        [DataMember]
        public DateTime CreationDate
        {
            get { return _creationDate ?? ContentNode.CreationDate; }
            set { _creationDate = value; }
        }

        private DateTime? _modificationDate;
        [DataMember]
        public DateTime ModificationDate
        {
            get { return _modificationDate ?? ContentNode.ModificationDate; }
            set { _modificationDate = value; }
        }

        private string _modifiedBy;
        [DataMember]
        public string ModifiedBy
        {
            get { return _modifiedBy ?? ContentNode.ModifiedBy.Name; }
            set { _modifiedBy = value; }
        }

        private string _lockedBy;
        [DataMember]
        public string LockedBy
        {
            get
            {
                if (_lockedBy != null)
                    return _lockedBy;
                
                if (ContentNode.Lock.Locked)
                    return ContentNode.Lock.LockedBy.Name;
                
                return string.Empty;
            }
            set { _lockedBy = value; }
        }

        [DataMember]
        public Content[] Children { get; set; }
        [DataMember]
        public int ChildCount { get; set; }

        [DataMember]
        public ContentProperty[] Properties { get; set; }

        private bool? _leaf;
        [DataMember]
        public bool Leaf
        {
            get { return _leaf ?? !(ContentNode is sn.IFolder); }
            set { _leaf = value; }
        }

        // hack for compatibility, uses unknown
        [DataMember]
        public bool leaf
        {
            get { return Leaf; }
            set { Leaf = value; }
        }
        // endhack

        [DataMember]
        public bool IsSystemContent
        {
            get
            {
                return ContentNode.NodeType.IsInstaceOfOrDerivedFrom("SystemFolder") ||
                       ContentNode.NodeType.IsInstaceOfOrDerivedFrom("SystemFile");
            }
            set { }
        }

        // hack for compatibility, uses unknown
        private string _iconCls;
        [DataMember]
        public string iconCls
        { 
            get { return _iconCls ?? String.Concat("snIconSmall_",Icon); }
            set { _iconCls = value; }
        }
        // endhack

        private int? _parentId;
        [DataMember]
        public int ParentId
        {
            get
            {
                if (_parentId == null)
                    _parentId = ContentNode.ParentId;
                return _parentId.Value;
            }
            set { _parentId = value; }
        }

        private bool? _hasBinary;
        [DataMember]
        public bool HasBinary
        {
            get { return _hasBinary ?? (ContentNode is sn.IFile); }
            set { _hasBinary = value; }
        }

        #endregion

        public Content() { }
        public Content(Node contentNode) : this(contentNode, true, false, false, false, 0, 1) { }
        public Content(Node contentNode, bool includeProperties, bool includeChildren, bool onlyFileChildren, bool isContentType, int start, int limit)
        {
            ContentNode = contentNode;
            withChildren = includeChildren;

            if (includeProperties)
            {
                var types = new[] { DataType.Currency, DataType.DateTime, 
                    DataType.Int, DataType.String, DataType.Reference };
				this.Properties = contentNode.PropertyTypes
                    .Where(pt => types.Contains(pt.DataType))
                    .Where(pt => contentNode[pt] != null)
                    .Select(pt => new ContentProperty(pt.DataType, pt.Name, EnsureData(contentNode[pt]))).ToArray();
            }

		    if (!includeChildren) 
                return;

            Children = new ContentStoreService().GetFeed2(ContentNode.Path, onlyFileChildren, false, start, limit);

            var gc = ContentNode as sn.GenericContent;
            if (gc == null) 
                return;

            ChildCount = gc.GetChildren(new QuerySettings { EnableAutofilters = false, EnableLifespanFilter = false }).Count;
        }

        private object EnsureData(object data)
        {
            if (data == null)
                return null;

            switch (data.GetType().FullName)
            {
                case "System.DateTime":
                    var date = (DateTime)data;

                    var SafeMax = DateTime.MaxValue.AddDays(-1);
                    var SafeMin = DateTime.MinValue.AddDays(1);

                    if (date > SafeMax)
                        date = SafeMax;
                    else if (date < SafeMin)
                        date = SafeMin;

                    return date;
                default:
                    return data;
            }
        }
    }
}