using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.Diagnostics;

namespace SenseNet.ContentRepository.Storage
{
    [Obsolete("Use Node.GetCachedData and SetCachedData instead", true)]
    public interface ISharedExtendable
    {
        IDictionary<string, object> GetExtendedData();
        void SetExtendedData(IDictionary<string, object> data);
    }

    public delegate void CancellableNodeEventHandler(object sender, CancellableNodeEventArgs e);
    public delegate void CancellableNodeOperationEventHandler(object sender, CancellableNodeOperationEventArgs e);

    internal enum AccessLevel { Header, Major, Minor }
    public enum VersionRaising { None, NextMinor, NextMajor }

    /// <summary>
    /// <para>Represents a structured set of data that can be stored in the Sense/Net Content Repository.</para>
    /// <para>A node can be loaded from the Sense/Net Content Repository, the represented data can be modified via the properties of the node, and the changes can be persisted back.</para>
    /// </summary>
    /// <remarks>Remark</remarks>
    /// <example>Here is an example from the test project that creates a new Node with two integer Properties TestInt1 and TestInt2
    /// <code>
    /// [ContentHandler]
    /// public class TestNode : Node
    /// {
    /// public TestNode(Node parent) : base(parent) { }
    /// public TestNode(NodeToken token) : base(token) { }
    /// [RepositoryProperty("TestInt1")]
    /// public int TestInt
    /// {
    /// get { return (int)this["TestInt1"]; }
    /// set { this["TestInt1"] = value; }
    /// }
    /// [RepositoryProperty("TestInt2", DataType.Int)]
    /// public string TestInt2
    /// {
    /// get { return this["TestInt2"].ToString(); }
    /// set { this["TestInt2"] = Convert.ToInt32(value); }
    /// }
    /// }
    /// </code></example>
    [DebuggerDisplay("Id={Id}, Name={Name}, Version={Version}, Path={Path}")]
    public abstract class Node
    {
        private NodeData _data;
        internal NodeData Data { get { return _data; } }

        private bool _copying;
        public bool CopyInProgress
        {
            get { return _copying; }
        }

        /// <summary>
        /// Set this to override AllowIncrementalNaming setting of ContentType programatically
        /// </summary>
        public bool? AllowIncrementalNaming;

        public string NodeOperation { get; set; }

        private static IIndexPopulator Populator
        {
            get { return StorageContext.Search.SearchEngine.GetPopulator(); }
        }

        //public bool AllowDeferredIndexingOnSave { get; set; }

        private SecurityHandler _security;
        private LockHandler _lockHandler;
        public bool IsHeadOnly { get; private set; }

        private static readonly string[] SEE_ENABLED_PROPERTIES = { "Name", "Path", "Id", "Index", "NodeType", "ContentListId", "ContentListType", "Parent", "IsModified", "IsDeleted", "NodeCreationDate", "NodeModificationDate", "NodeCreatedBy", "NodeModifiedBy", "CreationDate", "ModificationDate", "CreatedById", "ModifiedById" };
        public static readonly List<string> EXCLUDED_COPY_PROPERTIES = new List<string> { "ApprovingMode", "InheritableApprovingMode", "InheritableVersioningMode", "VersioningMode", "AvailableContentTypeFields", "FieldSettingContents" };

        public IEnumerable<Node> PhysicalChildArray
        {
            get { return this.GetChildren(); }
        }

        protected virtual IEnumerable<Node> GetChildren()
        {
            var nodeHeads = DataBackingStore.GetNodeHeads(QueryChildren().Identifiers);
            var user = AccessProvider.Current.GetCurrentUser();

            //use loop here instead of LoadNodes to check permissions
            return new NodeList<Node>((from nodeHead in nodeHeads
                                       where nodeHead != null && Security.HasPermission(user, nodeHead, PermissionType.See)
                                       select nodeHead.Id));
        }
        protected int GetChildCount()
        {
            return QueryChildren().Count;
        }
        private NodeQueryResult QueryChildren()
        {
            if (this.Id == 0)
                return new NodeQueryResult(new NodeList<Node>());

            if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
            {
                var query = new NodeQuery();
                query.Add(new IntExpression(IntAttribute.ParentId, ValueOperator.Equal, this.Id));
                return query.Execute();
            }
            else
            {
                //fallback to the direct db query
                return NodeQuery.QueryChildren(this.Id);
            }
        }

        public virtual int NodesInTree
        {
            get
            {
                var childQuery = new NodeQuery();
                childQuery.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, this.Path));
                var childResult = childQuery.Execute();
                var childNum = childResult.Count;

                return childNum;
            }
        }

        internal void MakePrivateData()
        {
            if (!_data.IsShared)
                return;
            _data = NodeData.CreatePrivateData(_data);
        }

        #region //================================================================================================= General Properties

        /// <summary>
        /// The unique identifier of the node.
        /// </summary>
        /// <remarks>Notice if you develop a data provider for the system you have to convert your Id to integer.</remarks>
        public int Id
        {
            get
            {
                if (_data == null)
                    return 0;
                return _data.Id;
            }
            internal set
            {
                MakePrivateData();
                _data.Id = value;
            }
        }
        public virtual int NodeTypeId
        {
            get { return _data.NodeTypeId; }
        }
        public virtual int ContentListTypeId
        {
            get { return _data.ContentListTypeId; }
        }
        /// <summary>
        /// Gets the <see cref="SenseNet.ContentRepository.Storage.Schema.NodeType">NodeType</see> of the instance.
        /// </summary>
        public virtual NodeType NodeType
        {
            get { return NodeTypeManager.Current.NodeTypes.GetItemById(_data.NodeTypeId); }
        }
        public virtual ContentListType ContentListType
        {
            get
            {
                if (_data.ContentListTypeId == 0)
                    return null;
                return NodeTypeManager.Current.ContentListTypes.GetItemById(_data.ContentListTypeId);
            }
            internal set
            {
                MakePrivateData();
                _data.ContentListTypeId = value.Id;
            }
        }
        public virtual int ContentListId
        {
            get { return _data.ContentListId; }
            internal set
            {
                MakePrivateData();
                _data.ContentListId = value;
            }
        }

        public bool IsLastPublicVersion { get; private set; }
        public bool IsLatestVersion { get; private set; }

        /// <summary>
        /// Gets the parent node.
        /// Use this.ParentId, this.ParentPath, this.ParentName instead of Parent.Id, Parent.Path, Parent.Name
        /// </summary>
        /// <value>The parent.</value>
        public Node Parent
        {
            get
            {
                if (_data.ParentId == 0)
                    return null;
                try
                {
                    return Node.LoadNode(_data.ParentId);
                }
                catch (Exception e) //rethrow
                {
                    throw Exception_ReferencedNodeCouldNotBeLoadedException("Parent", _data.ParentId, e);
                }
            }
        }
        /// <summary>
        /// Gets the Id of parent.
        /// </summary>
        /// <value>The parent.</value>
        public int ParentId
        {
            get { return Data.ParentId; }
        }
        public string ParentPath
        {
            get { return RepositoryPath.GetParentPath(this.Path); }
        }
        public string ParentName
        {
            get { return RepositoryPath.GetFileName(this.ParentPath); }
        }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <remarks>
        /// A node is uniquely identified by the Name within a leaf in the Sense/Net Content Repository tree.
        /// This guarantees that there can be no two nodes with the same path in the Repository.
        /// </remarks>
        /// <value>The name.</value>
        public virtual string Name
        {
            get { return _data.Name; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                MakePrivateData();
                _data.Name = value;
            }

        }
        public virtual string DisplayName
        {
            get { return _data.DisplayName; }
            set
            {
                MakePrivateData();
                _data.DisplayName = value;
            }
        }
        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        public virtual string Path
        {
            get { return _data.Path; }
        }
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public virtual int Index
        {
            get { return _data.Index; }
            set
            {
                MakePrivateData();
                _data.Index = value;
            }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is modified.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is modified; otherwise, <c>false</c>.
        /// </value>
        public bool IsModified
        {
            get { return this._data.AnyDataModified; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is deleted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is deleted; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeleted
        {
            get { return _data.IsDeleted; }
        }
        /// <summary>
        /// Gets a value indicating whether the default permissions of this instance are inherited from its parent.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the default permissions of this instance are inherited from its parent; otherwise <c>false</c>.
        /// </value>
        public bool IsInherited
        {
            get { return _data.IsInherited; }
        }
        /// <summary>
        /// Gets the security.
        /// </summary>
        /// <value>The security.</value>
        public SecurityHandler Security
        {
            get
            {
                if (_security == null)
                    _security = new SecurityHandler(this);
                return _security;
            }
        }
        /// <summary>
        /// Gets the lock.
        /// </summary>
        /// <value>The lock.</value>
        public LockHandler Lock
        {
            get
            {
                if (_lockHandler == null)
                    _lockHandler = new LockHandler(this);
                return _lockHandler;
            }
        }

        public long NodeTimestamp
        {
            get { return _data.NodeTimestamp; }
        }
        public long VersionTimestamp
        {
            get { return _data.VersionTimestamp; }
        }

        public virtual bool IsNew
        {
            get
            {
                return this.Id == 0;
            } 
        }

        //--------------------------------------------------------- Node-dependent Creation, Modification

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        /// <value>The creation date.</value>
        public virtual DateTime NodeCreationDate
        {
            get
            {
                return _data.NodeCreationDate;
            }
            set
            {
                if (value < DataProvider.Current.DateTimeMinValue)
                    throw SR.Exceptions.General.Exc_LessThanDateTimeMinValue();
                if (value > DataProvider.Current.DateTimeMaxValue)
                    throw SR.Exceptions.General.Exc_BiggerThanDateTimeMaxValue();
                MakePrivateData();
                _data.NodeCreationDate = value;
            }
        }
        /// <summary>
        /// Gets or sets the modification date.
        /// </summary>
        /// <value>The modification date.</value>
        public virtual DateTime NodeModificationDate
        {
            get
            {
                return _data.NodeModificationDate;
            }
            set
            {
                if (value < DataProvider.Current.DateTimeMinValue)
                    throw SR.Exceptions.General.Exc_LessThanDateTimeMinValue();
                if (value > DataProvider.Current.DateTimeMaxValue)
                    throw SR.Exceptions.General.Exc_BiggerThanDateTimeMaxValue();
                MakePrivateData();
                _data.NodeModificationDate = value;
            }
        }
        /// <summary>
        /// Gets or sets the user who created the instance.
        /// </summary>
        public virtual Node NodeCreatedBy
        {
            get
            {
                Node node = null;
                if (_data.NodeCreatedById == 0)
                    return null;
                try
                {
                    node = Node.LoadNode(_data.NodeCreatedById);
                }
                catch (Exception e) //rethrow
                {
                    throw Exception_ReferencedNodeCouldNotBeLoadedException("NodeCreatedBy", _data.CreatedById, e);
                }
                if (node is IUser)
                    return node;
                throw new ApplicationException("'NodeCreatedBy' should be IUser.");
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Id == 0)
                    throw new ArgumentOutOfRangeException("value", "Referenced 'CreatedBy' node must be saved.");
                if (!(value is IUser))
                    throw new ArgumentOutOfRangeException("value", "'CreatedBy' must be IUser.");
                MakePrivateData();
                _data.NodeCreatedById = value.Id;
            }
        }
        /// <summary>
        /// Gets or sets the user who modified the instance.
        /// </summary>
        public virtual Node NodeModifiedBy
        {
            get
            {
                Node node = null;
                if (_data.NodeModifiedById == 0)
                    return null;
                try
                {
                    node = Node.LoadNode(_data.NodeModifiedById);
                }
                catch (Exception e) //rethrow
                {
                    throw Exception_ReferencedNodeCouldNotBeLoadedException("NodeModifiedById", _data.CreatedById, e);
                }
                if (node is IUser)
                    return node;
                throw new ApplicationException("'NodeModifiedById' should be IUser.");
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Id == 0)
                    throw new ArgumentOutOfRangeException("value", "Referenced 'NodeModifiedBy' node must be saved.");
                if (!(value is IUser))
                    throw new ArgumentOutOfRangeException("value", "'NodeModifiedBy' must be IUser.");
                MakePrivateData();
                _data.NodeModifiedById = value.Id;
            }
        }
        public virtual int NodeCreatedById
        {
            get { return _data.NodeCreatedById; }
        }
        public virtual int NodeModifiedById
        {
            get { return _data.NodeModifiedById; }
        }
        //--------------------------------------------------------- Node versioned Properties (Node attributes #2)

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public VersionNumber Version
        {
            get { return _data.Version; }
            set { MakePrivateData(); _data.Version = value; }
        }
        /// <summary>
        /// Gets the version id.
        /// </summary>
        /// <value>The version id.</value>
        public int VersionId
        {
            get { return _data.VersionId; }
        }
        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        /// <value>The creation date.</value>
        public virtual DateTime CreationDate
        {
            get { return _data.CreationDate; }
            set
            {
                if (value < DataProvider.Current.DateTimeMinValue)
                    throw SR.Exceptions.General.Exc_LessThanDateTimeMinValue();
                if (value > DataProvider.Current.DateTimeMaxValue)
                    throw SR.Exceptions.General.Exc_BiggerThanDateTimeMaxValue();
                MakePrivateData();
                _data.CreationDate = value;
            }
        }
        /// <summary>
        /// Gets or sets the modification date.
        /// </summary>
        /// <value>The modification date.</value>
        public virtual DateTime ModificationDate
        {
            get { return _data.ModificationDate; }
            set
            {
                if (value < DataProvider.Current.DateTimeMinValue)
                    throw SR.Exceptions.General.Exc_LessThanDateTimeMinValue();
                if (value > DataProvider.Current.DateTimeMaxValue)
                    throw SR.Exceptions.General.Exc_BiggerThanDateTimeMaxValue();
                MakePrivateData();
                _data.ModificationDate = value;
            }
        }
        /// <summary>
        /// Gets or sets the user who created the instance.
        /// </summary>
        public virtual Node CreatedBy
        {
            get
            {
                Node node = null;
                if (_data.CreatedById == 0)
                    return null;
                try
                {
                    node = Node.LoadNode(_data.CreatedById);
                }
                catch (Exception e) //rethrow
                {
                    throw Exception_ReferencedNodeCouldNotBeLoadedException("CreatedBy", _data.CreatedById, e);
                }
                if (node is IUser)
                    return node;
                throw new ApplicationException("'CreatedBy' should be IUser.");
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Id == 0)
                    throw new ArgumentOutOfRangeException("value", "Referenced 'CreatedBy' node must be saved.");
                if (!(value is IUser))
                    throw new ArgumentOutOfRangeException("value", "'CreatedBy' must be IUser.");
                MakePrivateData();
                _data.CreatedById = value.Id;
            }
        }
        /// <summary>
        /// Gets the user id who created the instance.
        /// </summary>
        public virtual int CreatedById
        {
            get { return _data.CreatedById; }
        }
        /// <summary>
        /// Gets or sets the user who modified the instance.
        /// </summary>
        public virtual Node ModifiedBy
        {
            get
            {
                Node node = null;
                if (_data.ModifiedById == 0)
                    return null;
                try
                {
                    node = Node.LoadNode(_data.ModifiedById);
                }
                catch (Exception e) //rethrow
                {
                    throw Exception_ReferencedNodeCouldNotBeLoadedException("ModifiedById", _data.CreatedById, e);
                }
                if (node is IUser)
                    return node;
                throw new ApplicationException("'ModifiedById' should be IUser.");
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Id == 0)
                    throw new ArgumentOutOfRangeException("value", "Referenced 'ModifiedBy' node must be saved.");
                if (!(value is IUser))
                    throw new ArgumentOutOfRangeException("value", "'ModifiedBy' must be IUser.");
                MakePrivateData();
                _data.ModifiedById = value.Id;
            }
        }
        /// <summary>
        /// Gets the user id who modified the instance
        /// </summary>
        public virtual int ModifiedById
        {
            get { return _data.ModifiedById; }
        }

        //--------------------------------------------------------- Properties for locking

        public bool Locked
        {
            get { return LockedById != 0; }
            //internal set { _data.Locked = value; }
        }
        public int LockedById
        {
            get { return _data == null ? 0 : _data.LockedById; }
            internal set
            {
                MakePrivateData();
                _data.LockedById = value;
                _data.Locked = value != 0;
            }
        }
        public IUser LockedBy
        {
            get { return this.Locked ? Node.LoadNode(LockedById) as IUser : null; }
        }
        public string ETag
        {
            get { return _data.ETag; }
            set { MakePrivateData(); _data.ETag = value; }
        }
        public int LockType
        {
            get { return _data.LockType; }
            set { MakePrivateData(); _data.LockType = value; }
        }
        public int LockTimeout
        {
            get { return _data.LockTimeout; }
            internal set { MakePrivateData(); _data.LockTimeout = value; }
        }
        public DateTime LockDate
        {
            get { return _data.LockDate; }
            set { MakePrivateData(); _data.LockDate = value; }
        }
        public string LockToken
        {
            get { return _data.LockToken; }
            internal set { MakePrivateData(); _data.LockToken = value; }
        }
        public DateTime LastLockUpdate
        {
            get { return _data.LastLockUpdate; }
            internal set { MakePrivateData(); _data.LastLockUpdate = value; }
        }

        #endregion

        #region //================================================================================================= Dynamic property accessors

        public object this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case "Id": return this.Id;
                    case "NodeType": return this.NodeType;
                    case "ContentListId": return this.ContentListId;
                    case "ContentListType": return this.ContentListType;
                    case "Parent": return this.Parent;
                    case "ParentId": return this._data.ParentId;
                    case "Name": return this.Name;
                    case "DisplayName": return this.DisplayName;
                    case "Path": return this.Path;
                    case "Index": return this.Index;
                    case "IsModified": return this.IsModified;
                    case "IsDeleted": return this.IsDeleted;
                    case "IsInherited": return this.IsInherited;
                    case "NodeCreationDate": return this.NodeCreationDate;
                    case "NodeCreatedBy": return this.NodeCreatedBy;
                    case "Version": return this.Version;
                    case "VersionId": return this.VersionId;
                    case "CreationDate": return this.CreationDate;
                    case "ModificationDate": return this.ModificationDate;
                    case "CreatedBy": return this.CreatedBy;
                    case "CreatedById": return this.CreatedById;
                    case "ModifiedBy": return this.ModifiedBy;
                    case "ModifiedById": return this.ModifiedById;
                    case "Locked": return this.Locked;
                    case "Lock": return this.Lock;
                    case "LockedById": return this.LockedById;
                    case "LockedBy": return this.LockedBy;
                    case "ETag": return this.ETag;
                    case "LockType": return this.LockType;
                    case "LockTimeout": return this.LockTimeout;
                    case "LockDate": return this.LockDate;
                    case "LockToken": return this.LockToken;
                    case "LastLockUpdate": return this.LastLockUpdate;
                    case "Security": return this.Security;
                    default: return this[GetPropertyTypeByName(propertyName)];
                }
            }
            set
            {
                switch (propertyName)
                {
                    case "Id":
                    case "IsModified":
                    case "NodeType":
                    case "Parent":
                    case "Path":
                    case "Security":
                        throw new InvalidOperationException(String.Concat("Property is read only: ", propertyName));
                    case "CreationDate": this.CreationDate = (DateTime)value; break;
                    case "CreatedBy": this.CreatedBy = (Node)value; break;
                    case "Index": this.Index = value == null ? 0 : (int)value; break;
                    case "ModificationDate": this.ModificationDate = (DateTime)value; break;
                    case "ModifiedBy": this.ModifiedBy = (Node)value; break;
                    case "Name": this.Name = (string)value; break;
                    case "DisplayName": this.DisplayName = (string)value; break;
                    case "Version": this.Version = (VersionNumber)value; break;
                    default: this[GetPropertyTypeByName(propertyName)] = value; break;
                }
            }
        }
        internal object this[int propertyId]
        {
            get { return this[GetPropertyTypeById(propertyId)]; }
            set { this[GetPropertyTypeById(propertyId)] = value; }
        }
        public object this[PropertyType propertyType]
        {
            get
            {
                AssertSeeOnly(propertyType.Name);

                switch (propertyType.DataType)
                {
                    case DataType.Binary:
                    case DataType.Reference:
                        return GetAccessor(propertyType);
                    default:
                        return _data.GetDynamicRawData(propertyType) ?? propertyType.DefaultValue;
                }
            }
            set
            {
                //if (propertyType.DataType == DataType.Binary || propertyType.DataType == DataType.Reference)
                //    throw new NotSupportedException(String.Concat("Storage2: ", propertyType.DataType, " property is read only. Property name: ", propertyType.Name));
                //data.SetDynamicRawData(propertyType, value);
                MakePrivateData();
                switch (propertyType.DataType)
                {
                    case DataType.Binary:
                        ChangeAccessor((BinaryData)value, propertyType);
                        break;
                    case DataType.Reference:
                        var nodeList = value as NodeList<Node>;
                        if (nodeList == null)
                            nodeList = value == null ? new NodeList<Node>() : new NodeList<Node>((IEnumerable<Node>)value);
                        ChangeAccessor(nodeList, propertyType);
                        break;
                    default:
                        _data.SetDynamicRawData(propertyType, value);
                        break;
                }

            }
        }

        private static readonly List<string> _propertyNames = new List<string>(new[] {"Id","NodeType","ContentListId","ContentListType","Parent","ParentId","Name","DisplayName","Path",
            "Index","IsModified","IsDeleted","IsInherited","NodeCreationDate","NodeCreatedBy","Version","VersionId","CreationDate","ModificationDate","CreatedBy",
            "CreatedById","ModifiedBy","ModifiedById","Locked","Lock","LockedById","LockedBy","ETag","LockType","LockTimeout","LockDate","LockToken","LastLockUpdate","Security"});

        public TypeCollection<PropertyType> PropertyTypes { get { return _data.PropertyTypes; } }
        public virtual bool HasProperty(string name)
        {
            if( PropertyTypes[name] != null)
                return true;
            return _propertyNames.Contains(name);
        }
        public bool HasProperty(PropertyType propType)
        {
            return HasProperty(propType.Id);
        }
        public bool HasProperty(int propertyTypeId)
        {
            return PropertyTypes.GetItemById(propertyTypeId) != null;
        }

        private Dictionary<string, IDynamicDataAccessor> __accessors;
        private Dictionary<string, IDynamicDataAccessor> _accessors
        {
            get
            {
                if (__accessors == null)
                {
                    var accDict = new Dictionary<string, IDynamicDataAccessor>();

                    foreach (var propType in this.PropertyTypes)
                    {
                        IDynamicDataAccessor acc = null;
                        if (propType.DataType == DataType.Binary)
                            acc = new BinaryData();
                        if (propType.DataType == DataType.Reference)
                            acc = new NodeList<Node>();
                        if (acc == null)
                            continue;
                        acc.OwnerNode = this;
                        acc.PropertyType = propType;
                        accDict[propType.Name] = acc;
                    }

                    __accessors = accDict;
                }
                return __accessors;
            }
        }

        //---------------- General axis

        public T GetProperty<T>(string propertyName)
        {
            return (T)this[propertyName];
        }
        internal T GetProperty<T>(int propertyId)
        {
            return (T)this[propertyId];
        }
        public T GetProperty<T>(PropertyType propertyType)
        {
            return (T)this[propertyType];
        }
        public virtual object GetPropertySafely(string propertyName)
        {
            if (this.HasProperty(propertyName))
            {
                var result = this[propertyName];
                return result;
            }
            return null;
        }

        private IDynamicDataAccessor GetAccessor(PropertyType propType)
        {
            IDynamicDataAccessor value;
            if (!_accessors.TryGetValue(propType.Name, out value))
                throw NodeData.Exception_PropertyNotFound(propType.Name);
            //if (_data.GetDynamicRawData(propType) == null)
            //    if(propType.DataType == DataType.Binary)
            //        return null;
            return value;
        }
        private PropertyType GetPropertyTypeByName(string name)
        {
            var propType = PropertyTypes[name];
            if (propType == null)
                throw NodeData.Exception_PropertyNotFound(name);
            return propType;
        }
        private PropertyType GetPropertyTypeById(int id)
        {
            var propType = PropertyTypes.GetItemById(id);
            if (propType == null)
                throw NodeData.Exception_PropertyNotFound(id);
            return propType;
        }
        internal void ChangeAccessor(IDynamicDataAccessor newAcc, PropertyType propType)
        {
            var value = _data.GetDynamicRawData(propType);
            if (value != null)
            {
                var oldAcc = _accessors[propType.Name];
                if (oldAcc == null)
                    throw new NullReferenceException("Accessor not found: " + propType.Name);
                oldAcc.RawData = value;
                oldAcc.OwnerNode = null;
                oldAcc.PropertyType = null;
            }

            MakePrivateData();
            if (newAcc == null)
            {
                _data.SetDynamicRawData(propType, null);
            }
            else
            {
                _data.SetDynamicRawData(propType, newAcc.RawData);
                newAcc.OwnerNode = this;
                newAcc.PropertyType = propType;
                if (_accessors.ContainsKey(propType.Name))
                    _accessors[propType.Name] = newAcc;
                else
                    _accessors.Add(propType.Name, newAcc);
            }
        }

        //---------------- Binary axis

        public BinaryData GetBinary(string propertyName)
        {
            return (BinaryData)this[propertyName];
        }
        internal BinaryData GetBinary(int propertyId)
        {
            return (BinaryData)this[propertyId];
        }
        public BinaryData GetBinary(PropertyType property)
        {
            return (BinaryData)this[property];
        }
        public void SetBinary(string propertyName, BinaryData data)
        {
            if (data == null)
                GetBinary(propertyName).Reset();
            else
                GetBinary(propertyName).CopyFrom(data);
        }

        //---------------- Reference axes

        public IEnumerable<Node> GetReferences(string propertyName)
        {
            return (IEnumerable<Node>)this[propertyName];
        }
        internal IEnumerable<Node> GetReferences(int propertyId)
        {
            return (IEnumerable<Node>)this[propertyId];
        }
        public IEnumerable<Node> GetReferences(PropertyType property)
        {
            return (IEnumerable<Node>)this[property];
        }

        public void SetReferences<T>(string propertyName, IEnumerable<T> nodes) where T : Node
        {
            ClearReference(propertyName);
            AddReferences(propertyName, nodes);
        }
        internal void SetReferences<T>(int propertyId, IEnumerable<T> nodes) where T : Node
        {
            ClearReference(propertyId);
            AddReferences(propertyId, nodes);
        }
        public void SetReferences<T>(PropertyType property, IEnumerable<T> nodes) where T : Node
        {
            ClearReference(property);
            AddReferences(property, nodes);
        }

        public void ClearReference(string propertyName)
        {
            GetNodeList(propertyName).Clear();
        }
        internal void ClearReference(int propertyId)
        {
            GetNodeList(propertyId).Clear();
        }
        public void ClearReference(PropertyType property)
        {
            GetNodeList(property).Clear();
        }

        public void AddReference(string propertyName, Node refNode)
        {
            GetNodeList(propertyName).Add(refNode);
        }
        internal void AddReference(int propertyId, Node refNode)
        {
            GetNodeList(propertyId).Add(refNode);
        }
        public void AddReference(PropertyType property, Node refNode)
        {
            GetNodeList(property).Add(refNode);
        }

        public void AddReferences<T>(string propertyName, IEnumerable<T> refNodes) where T : Node
        {
            AddReferences<T>(propertyName, refNodes, false);
        }
        internal void AddReferences<T>(int propertyId, IEnumerable<T> refNodes) where T : Node
        {
            AddReferences<T>(propertyId, refNodes, false);
        }
        public void AddReferences<T>(PropertyType property, IEnumerable<T> refNodes) where T : Node
        {
            AddReferences<T>(property, refNodes, false);
        }
        public void AddReferences<T>(string propertyName, IEnumerable<T> refNodes, bool distinct) where T : Node
        {
            AddReferences<T>(GetNodeList(propertyName), refNodes, distinct);
        }
        internal void AddReferences<T>(int propertyId, IEnumerable<T> refNodes, bool distinct) where T : Node
        {
            AddReferences<T>(GetNodeList(propertyId), refNodes, distinct);
        }
        public void AddReferences<T>(PropertyType property, IEnumerable<T> refNodes, bool distinct) where T : Node
        {
            AddReferences<T>(GetNodeList(property), refNodes, distinct);
        }

        public bool HasReference(string propertyName, Node refNode)
        {
            return GetNodeList(propertyName).Contains(refNode);
        }
        internal bool HasReference(int propertyId, Node refNode)
        {
            return GetNodeList(propertyId).Contains(refNode);
        }
        public bool HasReference(PropertyType property, Node refNode)
        {
            return GetNodeList(property).Contains(refNode);
        }

        public void RemoveReference(string propertyName, Node refNode)
        {
            GetNodeList(propertyName).Remove(refNode);
        }
        internal void RemoveReference(int propertyId, Node refNode)
        {
            GetNodeList(propertyId).Remove(refNode);
        }
        public void RemoveReference(PropertyType property, Node refNode)
        {
            GetNodeList(property).Remove(refNode);
        }

        public int GetReferenceCount(string propertyName)
        {
            return GetNodeList(propertyName).Count;
        }
        internal int GetReferenceCount(int propertyId)
        {
            return GetNodeList(propertyId).Count;
        }
        public int GetReferenceCount(PropertyType property)
        {
            return GetNodeList(property).Count;
        }

        //---------------- Single reference interface

        public T GetReference<T>(string propertyName) where T : Node
        {
            return GetNodeList(propertyName).GetSingleValue<T>();
        }
        internal T GetReference<T>(int propertyId) where T : Node
        {
            return GetNodeList(propertyId).GetSingleValue<T>();
        }
        public T GetReference<T>(PropertyType property) where T : Node
        {
            return GetNodeList(property).GetSingleValue<T>();
        }
        public void SetReference(string propertyName, Node node)
        {
            GetNodeList(propertyName).SetSingleValue<Node>(node);
        }
        internal void SetReference(int propertyId, Node node)
        {
            GetNodeList(propertyId).SetSingleValue<Node>(node);
        }
        public void SetReference(PropertyType property, Node node)
        {
            GetNodeList(property).SetSingleValue<Node>(node);
        }

        //---------------- reference tools

        private NodeList<Node> GetNodeList(string propertyName)
        {
            return (NodeList<Node>)this[propertyName];
        }
        private NodeList<Node> GetNodeList(int propertyId)
        {
            return (NodeList<Node>)this[propertyId];
        }
        private NodeList<Node> GetNodeList(PropertyType property)
        {
            return (NodeList<Node>)this[property];
        }

        private static void AddReferences<T>(NodeList<Node> nodeList, IEnumerable<T> refNodes, bool distinct) where T : Node
        {
            foreach (var node in refNodes)
                if (!distinct || !nodeList.Contains(node))
                    nodeList.Add(node);
        }

        #endregion

        #region //================================================================================================= Construction

        protected Node() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        protected Node(Node parent) : this(parent, null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="nodeTypeName">Name of the node type.</param>
        protected Node(Node parent, string nodeTypeName)
        {
            if (nodeTypeName == null)
                nodeTypeName = this.GetType().Name;

            var nodeType = NodeTypeManager.Current.NodeTypes[nodeTypeName];
            if (nodeType == null)
            {
                nodeTypeName = this.GetType().FullName;
                nodeType = NodeTypeManager.Current.NodeTypes[nodeTypeName];

                if (nodeType == null)
                    throw new RegistrationException(String.Concat(SR.Exceptions.Schema.Msg_UnknownNodeType, ": ", nodeTypeName));
            }

            int listId = 0;
            ContentListType listType = null;
            if (parent != null && !nodeType.IsInstaceOfOrDerivedFrom("SystemFolder"))
            {
                listId = (parent.ContentListType != null && parent.ContentListId == 0) ? parent.Id : parent.ContentListId;
                listType = parent.ContentListType;
            }
            if (listType != null && this is IContentList)
            {
                throw new ApplicationException("Cannot create a ContentList under another ContentList");
            }

            var data = DataBackingStore.CreateNewNodeData(parent, nodeType, listType, listId);
            //data.InitializeDynamicData();
            this._data = data;

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class in the loading procedure. Do not use this constructor directly from your code.
        /// </summary>
        /// <param name="token">The token.</param>
        protected Node(NodeToken token)
        {
            //-- caller: CreateTargetClass()
            if (token == null)
                throw new ArgumentNullException("token");
            string typeName = this.GetType().FullName;
            if (token.NodeType.ClassName != typeName)
            {
                var message = String.Concat("Cannot create a ", typeName, " instance because type name is different in the passed token: ", token.NodeType.ClassName);
                throw new RegistrationException(message);
            }
            FillData(this, token);
            SetVersionInfo(token.NodeHead);
        }

        #endregion

        public void RefreshVersionInfo()
        {
            SetVersionInfo(NodeHead.Get(this.Id));
        }
        private void SetVersionInfo(NodeHead nodeHead)
        {
            var versionId = this.VersionId;
            this.IsLastPublicVersion = nodeHead.LastMajorVersionId == versionId;
            this.IsLatestVersion = nodeHead.LastMinorVersionId == versionId;
        }

        #region //================================================================================================= Loader methods

        private static VersionNumber DefaultAbstractVersion { get { return VersionNumber.LastAccessible; } }

        

        //----------------------------------------------------------------------------- Static batch loaders

        public static List<Node> LoadNodes(IEnumerable<int> idArray)
        {
            return LoadNodes(DataBackingStore.GetNodeHeads(idArray), VersionNumber.LastAccessible);
        }
        private static List<Node> LoadNodes(IEnumerable<NodeHead> heads, VersionNumber version)
        {
            var headList = new List<NodeHead>();
            var versionIdList = new List<int>();

            //-- resolving versionid array
            foreach (var head in heads)
            {
                if (head == null)
                    continue;

                AccessLevel userAccessLevel;

                try
                {
                    userAccessLevel = GetUserAccessLevel(head);
                     
                }
                catch (SenseNetSecurityException)
                {
                    //the user does not have permission to see/open this node
                    continue;
                }

                var acceptedLevel = GetAcceptedLevel(userAccessLevel, version);
                if (acceptedLevel == AccessLevel.Header)
                    throw new NotImplementedException("Load header only");   
                var versionId = GetVersionId(head, acceptedLevel, version);

                //if user has not enough permissions, skip the node
                if (versionId <= 0) 
                    continue;

                headList.Add(head);
                versionIdList.Add(versionId);
            }

            //-- loading data
            var result = new List<Node>();
            var tokenArray = DataBackingStore.GetNodeData(headList.ToArray(), versionIdList.ToArray());
            for (int i = 0; i < tokenArray.Length; i++)
            {
                var token = tokenArray[i];
                var retry = 0;
                while (true)
                {
                    if (token.NodeData != null)
                    {
                        var node = CreateTargetClass(token);
                        result.Add(node);
                        break;
                    }
                    else
                    {
                        //==== retrying with reload nodehead
                        if (++retry > 1) //-- one time
                            break;

                        Logger.WriteVerbose("Version is lost.", new Dictionary<string, object> { { "nodeId", token.NodeHead.Path }, { "versionId", token.VersionId }, });
                        var head = NodeHead.Get(token.NodeHead.Id);
                        if (head == null) //-- deleted
                            break;

                        var userAccessLevel = GetUserAccessLevel(head);
                        var acceptedLevel = GetAcceptedLevel(userAccessLevel, version);
                        if (acceptedLevel == AccessLevel.Header)
                            throw new NotImplementedException("Load header only");
                        var versionId = GetVersionId(head, acceptedLevel, version);
                        token = DataBackingStore.GetNodeData(head, versionId);
                    }
                }
            }
            return result;
        }

        //----------------------------------------------------------------------------- Static single loaders

        public static T Load<T>(int nodeId) where T : Node
        {
            return (T)LoadNode(nodeId);
        }
        public static T Load<T>(int nodeId, VersionNumber version) where T : Node
        {
            return (T)LoadNode(nodeId, version);
        }
        public static T Load<T>(string path) where T : Node
        {
            return (T)LoadNode(path);
        }
        public static T Load<T>(string path, VersionNumber version) where T : Node
        {
            return (T)LoadNode(path, version);
        }

        /// <summary>
        /// Loads the appropiate node by the given path.
        /// </summary>
        /// <example>How to load a Node by passing the Sense/Net Content Repository path.
        /// In this case you will get a Node named node filled with the data of the latest version of Node /Root/MyFavoriteNode.
        /// <code>
        /// Node node = Node.LoadNode("/Root/MyFavoriteNode");
        /// </code>
        /// </example>
        /// <returns>The latest version of the Node has the given path.</returns>
        public static Node LoadNode(string path)
        {
            return LoadNode(path, DefaultAbstractVersion);
        }
        /// <summary>
        /// Loads the appropiate node by the given path and version.
        /// </summary>
        /// <example>How to load the version 2.0 of a Node by passing the Sense/Net Content Repository path.
        /// In this case you will get a Node named node filled with the data of the latest version of Node /Root/MyFavoriteNode.
        /// <code>
        /// VersionNumber versionNumber = new VersionNumber(2, 0);
        /// Node node = Node.LoadNode("/Root/MyFavoriteNode", versionNumber);
        /// </code>
        /// </example>
        /// <returns>The a node holds the data of the given version of the Node that has the given path.</returns>
        public static Node LoadNode(string path, VersionNumber version)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            return LoadNode(DataBackingStore.GetNodeHead(path), version);
        }
        /// <summary>
        /// Loads the appropiate node by the given ID.
        /// </summary>
        /// <example>How to load a the latest version of Node identified with ID 132. 
        /// In this case you will get a Node named node filled with the data of the latest version of Node 132.
        /// <code>
        /// Node node = Node.LoadNode(132);
        /// </code>
        /// </example>
        /// <returns>The latest version of the Node that has the given ID.</returns> 
        public static Node LoadNode(int nodeId)
        {
            return LoadNode(nodeId, DefaultAbstractVersion);
        }
        /// <summary>
        /// Loads the appropiate node by the given ID and version number.
        /// </summary>
        /// <example>How to load a the version 2.0 of Node identified with ID 132. In this case you will get a Node named node filled with the data of the given version of Node 132.
        /// <code>
        /// VersionNumber versionNumber = new VersionNumber(2, 0);
        /// Node node = Node.LoadNode(132, versionNumber);
        /// </code>
        /// </example>
        /// <returns>The given version of the Node that has the given ID.</returns>
        public static Node LoadNode(int nodeId, VersionNumber version)
        {
            return LoadNode(DataBackingStore.GetNodeHead(nodeId), version);
        }
        public static Node LoadNode(NodeHead head)
        {
            return LoadNode(head, null);
        }
        public static Node LoadNode(NodeHead head, VersionNumber version)
        {
            if (version == null)
                version = DefaultAbstractVersion;

            var retry = 0;
            while (true)
            {
                if (head == null)
                    return null;

                var userAccessLevel = GetUserAccessLevel(head);
                var acceptedLevel = GetAcceptedLevel(userAccessLevel, version);
                var versionId = GetVersionId(head, acceptedLevel != AccessLevel.Header ? acceptedLevel : AccessLevel.Major, version);

                // if the requested version does not exist, return immediately
                if (versionId == 0)
                    return null;

                //-- <L2Cache>
                var l2cacheKey = GetL2CacheKey(versionId);
                var cachedNode = StorageContext.L2Cache.Get(l2cacheKey);
                if (cachedNode != null)
                    return (Node)cachedNode;
                //-- </L2Cache>

                var token = DataBackingStore.GetNodeData(head, versionId);
                if (token.NodeData != null)
                {
                    var node = CreateTargetClass(token);
                    if (acceptedLevel == AccessLevel.Header)
                        node.IsHeadOnly = true;

                    //-- <L2Cache>
                    StorageContext.L2Cache.Set(l2cacheKey, node);
                    //-- </L2Cache>

                    return node;
                }
                //-- lost version
                if (++retry > 1)
                    return null;
                //-- retry
                Logger.WriteVerbose("Version is lost.", new Dictionary<string, object> { { "nodeId", head.Path }, { "versionId", versionId }, });
                head = NodeHead.Get(head.Id);
            }
        }
        //-- <L2Cache>
        private static string GetL2CacheKey(int versionId)
        {
            return String.Concat("node|", versionId, "|", AccessProvider.Current.GetCurrentUser().Id);
        }
        //-- </L2Cache>

        //----------------------------------------------------------------------------- Load algorithm steps

        private static AccessLevel GetUserAccessLevel(NodeHead head)
        {
            //int userId = AccessProvider.Current.GetCurrentUser().Id;
            //if (SecurityHandler.GetPermission(nodeId, userId, PermissionType.OpenMinor) == PermissionValue.Allow)
            //    return AccessLevel.Minor;
            //if (SecurityHandler.GetPermission(nodeId, userId, PermissionType.Open) == PermissionValue.Allow)
            //    return AccessLevel.Major;
            //if (SecurityHandler.GetPermission(nodeId, userId, PermissionType.See) == PermissionValue.Allow)
            //    return AccessLevel.Header;
            //throw new SenseNetSecurityException("Access denied.");

            var userId = AccessProvider.Current.GetCurrentUser().Id;
            var isOwner = head.CreatorId == userId;
            switch (SecurityHandler.GetPermittedLevel(head))
            {
                case PermittedLevel.None:
                    throw new SenseNetSecurityException(head.Path, "Access denied.");
                case PermittedLevel.HeadOnly:
                    return AccessLevel.Header;
                case PermittedLevel.PublicOnly:
                    return AccessLevel.Major;
                case PermittedLevel.All:
                    return AccessLevel.Minor;
                default:
                    throw new NotImplementedException();
            }
        }
        private static AccessLevel GetAcceptedLevel(AccessLevel userAccessLevel, VersionNumber requestedVersion)
        {
            //			HO	Ma	Mi
            //	-------------------		
            //	LA		HO	Ma	Mi
            //	-------------------		
            //	HO		HO	HO	HO
            //	LMa		X	Ma	Ma
            //	VMa		X	Ma	Ma
            //	LMi		X	X	Mi
            //	VMi		X	X	Mi

            var definedIsMajor = false;
            var definedIsMinor = false;
            if (!requestedVersion.IsAbstractVersion)
            {
                definedIsMajor = requestedVersion.IsMajor;
                definedIsMinor = !requestedVersion.IsMajor;
            }

            if (requestedVersion == VersionNumber.LastAccessible)
                return userAccessLevel;
            if (requestedVersion == VersionNumber.Header)
                return AccessLevel.Header;
            if (requestedVersion == VersionNumber.LastMajor || definedIsMajor)
            {
                if (userAccessLevel < AccessLevel.Major)
                    throw new SenseNetSecurityException("");
                return AccessLevel.Major;
            }
            if (requestedVersion == VersionNumber.LastMinor || definedIsMinor)
            {
                if (userAccessLevel < AccessLevel.Minor)
                    throw new SenseNetSecurityException("");
                return AccessLevel.Minor;
            }
            throw new NotImplementedException("####");
        }
        private static int GetVersionId(NodeHead nodeHead, AccessLevel acceptedLevel, VersionNumber version)
        {
            if (version.IsAbstractVersion)
            {
                switch (acceptedLevel)
                {
                    //-- get from last major/minor slot of nodeHead
                    case AccessLevel.Header:
                        return 0; //TODO: Storage2: Mi van a versionId-val, ha az acceptedLevel == Header
                    case AccessLevel.Major:
                        return nodeHead.LastMajorVersionId;
                    case AccessLevel.Minor:
                        return nodeHead.LastMinorVersionId;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                //-- lookup versionlist of node from nodeHead or read from DB
                return nodeHead.GetVersionId(version);
            }
        }
        private static Node CreateTargetClass(NodeToken token)
        {
            if (token == null)
                return null;

            var node = token.NodeType.CreateInstance(token);
            node.FireOnLoaded();
            return node;
        }

        public object GetCachedData(string name)
        {
            return this.Data.GetExtendedSharedData(name);
        }
        public void SetCachedData(string name, object value)
        {
            this.Data.SetExtendedSharedData(name, value);
        }
        public void ResetCachedData(string name)
        {
            this.Data.ResetExtendedSharedData(name);
        }

        private static void FillData(Node node, NodeToken token)
        {
            string typeName = node.GetType().FullName;
            string typeNameInHead = NodeTypeManager.Current.NodeTypes.GetItemById(token.NodeData.NodeTypeId).ClassName;
            if (typeNameInHead != typeName)
            {
                var message = String.Concat("Cannot create a ", typeName, " instance because type name is different in the passed head: ", typeNameInHead);
                throw new RegistrationException(message);
            }

            //node._data = NodeData.CreatePrivateData(token.NodeData);
            node._data = token.NodeData;
            node._data.IsShared = true;
        }

        /// <summary>
        /// Gets the list of avaliable versions of the Node identified by Id.
        /// </summary>
        /// <returns>A list of version numbers.</returns>
        public static List<VersionNumber> GetVersionNumbers(int nodeId)
        {
            return new List<VersionNumber>(DataProvider.Current.GetVersionNumbers(nodeId));
        }
        /// <summary>
        /// Gets the list of avaliable versions of the Node identified by path.
        /// </summary>
        /// <returns>A list of version numbers.</returns>
        public static List<VersionNumber> GetVersionNumbers(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            return new List<VersionNumber>(DataProvider.Current.GetVersionNumbers(path));
        }

        public IEnumerable<Node> LoadVersions()
        {
            Security.Assert(PermissionType.RecallOldVersion, PermissionType.Open);
            if(Security.HasPermission(PermissionType.OpenMinor))
                return LoadAllVersions();
            return LoadPublicVersions();
        }
        private IEnumerable<Node> LoadPublicVersions()
        {
            var head = NodeHead.Get(this.Id);
            var x = head.Versions.Where(v => v.VersionNumber.Status == VersionStatus.Approved).Select(v => Node.LoadNode(this.Id, v.VersionNumber)).Where(n => n != null).ToArray();
            return x;
        }
        private IEnumerable<Node> LoadAllVersions()
        {
            var head = NodeHead.Get(this.Id);
            var x = head.Versions.Select(v => Node.LoadNode(this.Id, v.VersionNumber)).Where(n => n != null).ToArray();
            return x;
        }

        public static Node LoadNodeByVersionId(int versionId)
        {

            NodeHead head = NodeHead.GetByVersionId(versionId);
            if (head == null)
                return null;

            SecurityHandler.Assert(head, PermissionType.RecallOldVersion, PermissionType.Open);

            var token = DataBackingStore.GetNodeData(head, versionId);

            Node node = null;
            if (token.NodeData != null)
                node = CreateTargetClass(token);
            return node;
        }

        #endregion

        #region //================================================================================================= Save methods

        public virtual void Save()
        {
            var settings = new NodeSaveSettings
            {
                Node = this,
                HasApproving = false,
                VersioningMode = VersioningMode.None,
            };
            settings.ExpectedVersionId = settings.CurrentVersionId;
            settings.Validate();
            this.Save(settings);
        }
        private static IDictionary<string, object> CollectAllProperties(NodeData data)
        {
            return data.GetAllValues();
        }
        private static IDictionary<string, object> CollectChangedProperties(object[] args)
        {
            var sb = new StringBuilder();
            //sb.Append("<ChangedData>");
            foreach (var changedValue in (IEnumerable<ChangedData>)args[2])
            {
                sb.Append("<").Append(changedValue.Name).Append(">");
                if (changedValue.Original.ToString().StartsWith("<![CDATA["))
                    sb.Append("<OldValue>").Append(changedValue.Original).Append("</OldValue>");
                else
                    sb.Append("<OldValue><![CDATA[").Append(changedValue.Original).Append("]]></OldValue>");
                if (changedValue.Value.ToString().StartsWith("<![CDATA["))
                    sb.Append("<NewValue>").Append(changedValue.Value).Append("</NewValue>");
                else
                    sb.Append("<NewValue><![CDATA[").Append(changedValue.Value).Append("]]></NewValue>");
                sb.Append("</").Append(changedValue.Name).Append(">");
            }
            //sb.Append("</ChangedData>");
            return new Dictionary<string, object>
            {
                {"Id", args[0]},
                {"Path", args[1]},
                {"ChangedData", sb}
            };
        }
        //private void ExtendAccessorsWithListProperties()
        //{
        //    //TODO: Handle remove list property?
        //    foreach (var propType in this.PropertyTypes)
        //    {
        //        if (_accessors.ContainsKey(propType.Name))
        //            continue;
        //        IDynamicDataAccessor acc = null;
        //        if (propType.DataType == DataType.Binary)
        //            acc = new BinaryData();
        //        if (propType.DataType == DataType.Reference)
        //            acc = new NodeList<Node>();
        //        if (acc == null)
        //            continue;
        //        acc.OwnerNode = this;
        //        acc.PropertyType = propType;
        //        _accessors[propType.Name] = acc;
        //    }

        //}
        /**/
        private void SaveCopied(NodeSaveSettings settings)
        {
            using (var traceOperation = Logger.TraceOperation("Node.SaveCopied"))
            {
                var currentUser = AccessProvider.Current.GetCurrentUser();
                if (currentUser is SystemUser)
                    currentUser = AccessProvider.Current.GetOriginalUser();
                var currentUserId = currentUser.Id;

                var currentUserNode = currentUser as Node;
                if (currentUserNode == null)
                    throw new InvalidOperationException("Cannot save the content because the current user account representation is not a Node.");

                var thisList = this as IContentList;
                if (thisList != null)
                {
                    var newListType = thisList.GetContentListType();
                    if (this.ContentListType != null || newListType != null)
                    {
                        if (this.ContentListType == null)
                        {
                            //-- AssignNewContentListType
                            this.ContentListType = newListType;
                        }
                        else if (newListType == null)
                        {
                            //-- AssignNullContentListType
                            throw new NotSupportedException();
                        }
                        else if (this.ContentListType.Id != newListType.Id)
                        {
                            //-- Change ContentListType
                            throw new NotSupportedException();
                        }
                    }
                }

                if (this.Id != 0)
                    throw new InvalidOperationException("Id of copied node must be 0.");

                if (IsDeleted)
                    throw new InvalidOperationException("Cannot save deleted node.");

                // Check permissions: got to have AddNew permission on the parent
                //SecurityHandler.AssertPermission(this.Parent.Path, PermissionType.AddNew);
                this.Parent.Security.Assert(PermissionType.AddNew);

                RepositoryPath.CheckValidName(this.Name);

                // Validate
                if (this.ParentId == 0)
                    throw new InvalidPathException(SR.Exceptions.General.Msg_ParentNodeDoesNotExists); // parent Node does not exists
                if (this.Name.Trim().Length == 0)
                    throw new InvalidPathException(SR.Exceptions.General.Msg_NameCannotBeEmpty);
                if (this.IsModified)
                    this.Name = this.Name.Trim();

                //==== Update the modification
                //-- save modification
                
                //-- update to current
                DateTime now = DateTime.Now;
                this.ModificationDate = now;
                this.Data.ModifiedById = currentUserId;
                this.NodeModificationDate = now;
                this.Data.NodeModifiedById = currentUserId;

                //-- collect data for populator
                var parentPath = RepositoryPath.GetParentPath(this.Path);
                var thisPath = RepositoryPath.Combine(parentPath, this.Name);
                var populatorData = Populator.BeginPopulateNode(this, settings, thisPath, thisPath);

                //-- save
                DataBackingStore.SaveNodeData(this, settings);

                //-- <L2Cache>
                StorageContext.L2Cache.Clear();
                //-- </L2Cache>

                ////-- save index document
                //SaveIndexDocument();

                //-- populate
                Populator.CommitPopulateNode(populatorData);

                if (this is IGroup)
                    //DataProvider.Current.ExplicateGroupMemberships();
                    SecurityHandler.ExplicateGroupMembership();

                IUser thisAsUser = this as IUser;
                if (thisAsUser != null)
                    //DataProvider.Current.ExplicateOrganizationUnitMemberships(thisAsUser);
                    SecurityHandler.ExplicateOrganizationUnitMemberships(thisAsUser);

                FireOnCreated();

                traceOperation.IsSuccessful = true;
            }

        }
        /**/

        ///// <summary>
        ///// Delete the current node version.
        ///// </summary>
        //public virtual void DeleteVersion()
        //{
        //    var deletedVersionNumber = this.Version.Clone();

        //    DeleteVersion(this);
        //    var nodeHead = NodeHead.Get(Id);
        //    var sharedData = DataBackingStore.GetNodeData(nodeHead, nodeHead.LastMinorVersionId);
        //    var privateData = NodeData.CreatePrivateData(sharedData.NodeData);

        //    _data = privateData;

        //    Logger.WriteVerbose("Version deleted.", GetLoggerPropertiesAfterDeleteVersion, new object[] { this, deletedVersionNumber });
        //}

        public void Save(VersionRaising versionRaising, VersionStatus versionStatus)
        {
            var settings = new NodeSaveSettings { Node = this, HasApproving = false };
            var curVer = settings.CurrentVersion;
            var history = NodeHead.Get(this.Id).Versions;
            var biggest = history.OrderBy(v => v.VersionNumber.VersionString).LastOrDefault();
            var biggestVer = biggest == null ? curVer : biggest.VersionNumber;

            switch (versionRaising)
            {
                case VersionRaising.None:
                    settings.VersioningMode = VersioningMode.None;
                    settings.ExpectedVersion = curVer.ChangeStatus(versionStatus);
                    settings.ExpectedVersionId = settings.CurrentVersionId;
                    break;
                case VersionRaising.NextMinor:
                    settings.VersioningMode = VersioningMode.Full;
                    settings.ExpectedVersion = new VersionNumber(biggestVer.Major, biggestVer.Minor + 1, versionStatus);
                    settings.ExpectedVersionId = 0;
                    break;
                case VersionRaising.NextMajor:
                    settings.VersioningMode = VersioningMode.Full;
                    settings.ExpectedVersion = new VersionNumber(biggestVer.Major + 1, 0, versionStatus);
                    settings.ExpectedVersionId = 0;
                    break;
                default:
                    break;
            }
            Save(settings);
        }
        public virtual void Save(NodeSaveSettings settings)
        {
            if (StorageContext.Search.SearchEngine.IndexingPaused)
                WaitForIndexingContinued("save the content");

            //TODO: New save _copying?
            if (_copying)
            {
                SaveCopied(settings);
                return;
            }

            settings.Validate();
            ApplySettings(settings);

            using (var traceOperation = Logger.TraceOperation(String.Format("Node.Save(NodeSavingSettings) Path: {0}/{1}", this.ParentPath, this.Name)))
            {
                ChecksBeforeSave();

                var currentUser = AccessProvider.Current.GetCurrentUser();
                if (currentUser is SystemUser)
                    currentUser = AccessProvider.Current.GetOriginalUser();
                var currentUserId = currentUser.Id;

                bool isNewNode = (this.Id == 0);

                // No changes -> return
                if (!settings.NodeChanged())
                {
                    Logger.WriteVerbose("Node is not saved because it has no changes", Logger.GetDefaultProperties, this);
                    traceOperation.IsSuccessful = true;
                    return;
                }


                //-- Rename?
                string thisName = this.Name;
                var originalName = (this.Data.SharedData == null) ? thisName : this.Data.SharedData.Name;
                var renamed = originalName.ToLower() != thisName.ToLower();
                var parentPath = RepositoryPath.GetParentPath(this.Path);
                var newPath = RepositoryPath.Combine(parentPath, thisName);
                var originalPath = renamed ? RepositoryPath.Combine(parentPath, originalName) : newPath;

                //==== Update the modification
                //if (_data.SharedData != null)
                //{
                //    DateTime now = DateTime.Now;
                //    if (_data.ModificationDate == _data.SharedData.ModificationDate)
                //        this.ModificationDate = now;
                //    if (_data.ModifiedById == _data.SharedData.ModifiedById)
                //        this.Data.ModifiedById = currentUserId;
                //    if (_data.NodeModificationDate == _data.SharedData.NodeModificationDate)
                //        this.NodeModificationDate = now;
                //    if (_data.NodeModifiedById == _data.SharedData.NodeModifiedById)
                //        this.Data.NodeModifiedById = currentUserId;
                //}

                DateTime now = DateTime.Now;
                if (!_data.ModificationDateChanged)
                    this.ModificationDate = now;
                if (!_data.ModifiedByIdChanged)
                    this.Data.ModifiedById = currentUserId;
                if (!_data.NodeModificationDateChanged)
                    this.NodeModificationDate = now;
                if (!_data.NodeModifiedByIdChanged)
                    this.Data.NodeModifiedById = currentUserId;

                //-- collect changed field values for logging and info for nodeobservers
                IEnumerable<ChangedData> changedData = null;
                if (!isNewNode)
                    changedData = this.Data.GetChangedValues();

                CancellableNodeEventArgs args = null;
                if (isNewNode)
                {
                    args = new CancellableNodeEventArgs(this, CancellableNodeEvent.Creating);
                    FireOnCreating(args);
                }
                else
                {
                    args = new CancellableNodeEventArgs(this, CancellableNodeEvent.Modifying, changedData);
                    FireOnModifying(args);
                }
                if (args.Cancel)
                {
                    throw new CancelNodeEventException(args.CancelMessage, args.EventType, this);
                }

                //-- collect data for populator
                var populatorData = Populator.BeginPopulateNode(this, settings, originalPath, newPath);

                //-- save
                DataBackingStore.SaveNodeData(this, settings);

                //-- <L2Cache>
                StorageContext.L2Cache.Clear();
                //-- </L2Cache>

                ////-- save index document
                //SaveIndexDocument();

                //-- populate
                AccessProvider.ChangeToSystemAccount();
                try
                {
                    Populator.CommitPopulateNode(populatorData);
                }
                finally
                {
                    AccessProvider.RestoreOriginalUser();
                }

                //-- security
                if (renamed)
                    SecurityHandler.Rename(originalPath, newPath);

                //-- log
                if (Logger.AuditEnabled)
                {
                    if (isNewNode)
                        Logger.WriteAudit(AuditEvent.ContentCreated, CollectAllProperties, this.Data);
                    else
                        Logger.WriteAudit(AuditEvent.ContentUpdated, CollectChangedProperties,
                            new object[] { this.Id, this.Path, changedData });
                }

                //-- memberships
                if (this is IGroup)
                    SecurityHandler.ExplicateGroupMembership();

                IUser thisAsUser = this as IUser;
                if (thisAsUser != null)
                    SecurityHandler.ExplicateOrganizationUnitMemberships(thisAsUser);

                //-- for additional change tracking
                //var pt = _data.PropertyTypes;
                //_data = NodeData.CreatePrivateData(this.Data);
                //if (_data.PropertyTypes.Count != pt.Count)
                //    ExtendAccessorsWithListProperties();               

                var nodeHead = NodeHead.Get(Id);

                //  Reload by: nodeHead.LastMinorVersionId
                //var sharedData = DataBackingStore.GetNodeData(nodeHead, nodeHead.LastMinorVersionId);
                //var privateData = NodeData.CreatePrivateData(sharedData.NodeData);
                //_data = privateData;
                //__accessors = null;
                var token = DataBackingStore.GetNodeData(nodeHead, nodeHead.LastMinorVersionId);
                var sharedData = token.NodeData;
                sharedData.IsShared = true;
                _data = sharedData;
                __accessors = null;

                //-- events
                if (isNewNode)
                    FireOnCreated();
                else
                    FireOnModified(originalPath, changedData);
                traceOperation.IsSuccessful = true;
            }
        }

        private void ApplySettings(NodeSaveSettings settings)
        {
            if (settings.ExpectedVersion != null)
                this.Version = settings.ExpectedVersion;

            if (settings.LockerUserId != null)
            {
                if (settings.LockerUserId != 0)
                {
                    if (!this.Locked)
                    {
                        //-- Lock
                        LockToken = Guid.NewGuid().ToString();
                        LockedById = AccessProvider.Current.GetCurrentUser().Id;
                        LockDate = DateTime.Now;
                        LastLockUpdate = DateTime.Now;
                        LockTimeout = LockHandler.DefaultLockTimeOut;
                    }
                    else
                    {
                        //-- RefreshLock
                        if (this.LockedById != AccessProvider.Current.GetCurrentUser().Id)
                            throw new SenseNetSecurityException(this.Id, "Node is locked by another user");
                        LastLockUpdate = DateTime.Now;
                    }
                }
                else
                {
                    //-- Unlock
                    if (Locked)
                    {
                        this.LockedById = 0;
                        this.LockToken = string.Empty;
                        this.LockTimeout = 0;
                        this.LockDate = new DateTime(1800, 1, 1);
                        this.LastLockUpdate = new DateTime(1800, 1, 1);
                        this.LockType = 0;
                    }
                }
            }
        }
        private void ChecksBeforeSave()
        {
            var currentUser = AccessProvider.Current.GetCurrentUser();
            if (currentUser is SystemUser)
                currentUser = AccessProvider.Current.GetOriginalUser();

            var currentUserNode = currentUser as Node;
            if (currentUserNode == null)
                throw new InvalidOperationException("Cannot save the content because the current user account representation is not a Node.");

            var thisList = this as IContentList;
            if (thisList != null)
            {
                var newListType = thisList.GetContentListType();
                if (this.ContentListType != null || newListType != null)
                {
                    if (this.ContentListType == null)
                    {
                        //-- AssignNewContentListType
                        this.ContentListType = newListType;
                    }
                    else if (newListType == null)
                    {
                        //-- AssignNullContentListType
                        throw new NotSupportedException();
                    }
                    else if (this.ContentListType.Id != newListType.Id)
                    {
                        //-- Change ContentListType
                        throw new NotSupportedException();
                    }
                }
            }

            if (IsDeleted)
                throw new InvalidOperationException("Cannot save deleted node.");

            //-- Check permissions
            if (this.Id == 0)
                this.Parent.Security.Assert(PermissionType.AddNew);
            else
                Security.Assert(PermissionType.Save);


            AssertLock();
            RepositoryPath.CheckValidName(this.Name);

            // Validate
            if (this.ParentId == 0)
                throw new InvalidPathException(SR.Exceptions.General.Msg_ParentNodeDoesNotExists);
            if (this.Name.Trim().Length == 0)
                throw new InvalidPathException(SR.Exceptions.General.Msg_NameCannotBeEmpty);
            if (this.IsModified)
                this.Name = this.Name.Trim();

        }
        //public void SaveIndexDocument()
        //{
        //    if (this.Id == 0)
        //        throw new NotSupportedException("Cannot save the indexing information before node is not saved.");
        //    var doc = IndexDocumentProvider.GetIndexDocumentInfo(this);
        //    if (doc != null)
        //        DataBackingStore.SaveIndexDocument(this, doc);
        //}
        #endregion

        #region //================================================================================================= Move methods

        public static IEnumerable<NodeType> GetChildTypesToAllow(int nodeId)
        {
            return DataProvider.Current.LoadChildTypesToAllow(nodeId);
        }
        public IEnumerable<NodeType> GetChildTypesToAllow()
        {
            return DataProvider.Current.LoadChildTypesToAllow(this.Id);
        }

        /// <summary>
        /// Moves the Node indentified by its path to another location. The destination node is also identified by path. 
        /// </summary>
        /// <remarks>Use this method if you do not want to instantiate the nodes.</remarks>
        public static void Move(string sourcePath, string targetPath)
        {
            RepositoryPath.CheckValidPath(sourcePath);
            RepositoryPath.CheckValidPath(targetPath);
            Node sourceNode = Node.LoadNode(sourcePath);
            if (sourceNode == null)
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Source node does not exist with {0} path", sourcePath));
            Node targetNode = Node.LoadNode(targetPath);
            if (targetNode == null)
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Target node does not exist with {0} path", targetPath));
            targetNode.AssertLock();
            sourceNode.MoveTo(targetNode);
        }
        /// <summary>
        /// Modes the Node instance to another loacation. The new location is a Node instance which will be parent node.
        /// </summary>
        public virtual void MoveTo(Node target)
        {
            if (StorageContext.Search.SearchEngine.IndexingPaused)
                WaitForIndexingContinued("move the content");

            this.AssertLock();

            if (target == null)
                throw new ArgumentNullException("target");

            //check permissions
            this.Security.AssertSubtree(PermissionType.Delete);
            target.Security.Assert(PermissionType.Open);
            target.Security.Assert(PermissionType.AddNew);

            var originalPath = this.Path;
            var correctTargetPath = RepositoryPath.Combine(target.Path, RepositoryPath.PathSeparator);
            var correctCurrentPath = RepositoryPath.Combine(this.Path, RepositoryPath.PathSeparator);

            if (correctTargetPath.IndexOf(correctCurrentPath, StringComparison.Ordinal) != -1)
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Node cannot be moved under itself."));

            if (target.Id == this.ParentId)
                throw new InvalidOperationException("Node cannot be moved to its parent.");

            var targetPath = RepositoryPath.Combine(target.Path, this.Name);
            if (Node.Exists(targetPath))
                throw new ApplicationException(String.Concat("Target folder already contains a content named '", this.Name, "'."));

            var args = new CancellableNodeOperationEventArgs(this, target, CancellableNodeEvent.Moving);
            FireOnMoving(args);
            if (args.Cancel)
                throw new CancelNodeEventException(args.CancelMessage, args.EventType, this);

            var pathToInvalidate = String.Concat(this.Path, "/");

            try
            {
                DataProvider.Current.MoveNode(this.Id, target.Id);
            }
            catch (DataOperationException e) //rethrow
            {
                throw new ApplicationException("Cannot move", e);
            }

            SecurityHandler.Move(this.Path, target.Path);
            PathDependency.FireChanged(pathToInvalidate);
            PathDependency.FireChanged(this.Path);

            Populator.DeleteTree(this.Path);

            var nodeHead = NodeHead.Get(Id);
            var userAccessLevel = GetUserAccessLevel(nodeHead);
            var acceptedLevel = GetAcceptedLevel(userAccessLevel, VersionNumber.LastAccessible);
            var versionId = GetVersionId(nodeHead, acceptedLevel != AccessLevel.Header ? acceptedLevel : AccessLevel.Major, VersionNumber.LastAccessible);

            var sharedData = DataBackingStore.GetNodeData(nodeHead, versionId);
            var privateData = NodeData.CreatePrivateData(sharedData.NodeData);
            _data = privateData;

            Populator.PopulateTree(this.Path);

            Logger.WriteVerbose("Node moved", GetLoggerPropertiesAfterMove, new object[] { this, originalPath, targetPath });
            FireOnMoved(target, originalPath);
        }
        

        public static void MoveMore(List<Int32> nodeList, string targetPath, ref List<Exception> errors)
        {
            MoveMoreInternal2(new NodeList<Node>(nodeList), Node.LoadNode(targetPath), ref errors);
            return;

            //if (nodeList == null) throw new ArgumentNullException("nodeList");
            //if (nodeList.Count == 0) return;
            //if (string.IsNullOrEmpty(targetPath)) return;

            //MoveMoreInternal(nodeList, targetPath, ref errors);
        }
        //private static void MoveMoreInternal(IEnumerable<Int32> nodeList, string targetPath, ref List<Exception> errors)
        //{
        //    if (StorageContext.Search.SearchEngine.IndexingPaused)
        //        WaitForIndexingContinued("move the contents");

        //    Node targetNode = null;
            
        //    // check the target
        //    try
        //    {
        //        RepositoryPath.CheckValidPath(targetPath);

        //        targetNode = LoadNode(targetPath);
        //        if (targetNode == null)
        //            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Target node does not exist with {0} path.", targetPath));

        //        targetNode.AssertLock();

        //        targetNode.Security.Assert(PermissionType.Open);
        //        targetNode.Security.Assert(PermissionType.AddNew);

        //    } 
        //    catch(Exception exception)
        //    {
        //        errors.Add(exception);
        //        return;
        //    }

        //    // load nodes (permissions are checked automatically)
        //    var sourceNodes = new List<Node>();
        //    var originalPaths = new Dictionary<int, string>();
        //    foreach (var i in nodeList)
        //    {
        //        try
        //        {
        //            var sourceNode = Node.LoadNode(i);
        //            if (sourceNode == null)
        //                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Source node does not exist with {0} id.", i));

        //            sourceNodes.Add(sourceNode);
        //            originalPaths.Add(sourceNode.Id, sourceNode.Path);
        //        }
        //        catch(InvalidOperationException exception)
        //        {
        //            errors.Add(exception);
        //        }
        //    }

        //    // check conflicts
        //    var nodesToRemove = new List<int>();
        //    foreach (var currentNode in sourceNodes)
        //    {
        //        try
        //        {
        //            var correctTargetPath = RepositoryPath.Combine(targetNode.Path, RepositoryPath.PathSeparator);
        //            var correctCurrentPath = RepositoryPath.Combine(currentNode.Path, RepositoryPath.PathSeparator);

        //            if (correctTargetPath.IndexOf(correctCurrentPath, StringComparison.Ordinal) != -1)
        //                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
        //                                                                  "Node cannot be moved under itself."));

        //            if ((currentNode.Parent != null) && (targetNode.Id == currentNode.ParentId))
        //                throw new InvalidOperationException("Node cannot be moved to its parent.");

        //            var targetNodePath = RepositoryPath.Combine(targetNode.Path, currentNode.Name);
        //            if (Node.Exists(targetNodePath))
        //                throw new ApplicationException(String.Concat(
        //                    "Target folder already contains a content named '", currentNode.Name, "'."));

        //        }
        //        catch (Exception e)
        //        {
        //            errors.Add(e);
        //            nodesToRemove.Add(currentNode.Id);
        //        }
        //    }
        //    sourceNodes.RemoveAll(n => nodesToRemove.Contains(n.Id));
        //    nodesToRemove.Clear();

        //    // fire moving events
        //    foreach (var node in sourceNodes)
        //    {
        //        try
        //        {
        //            var args = new CancellableNodeOperationEventArgs(node, targetNode, CancellableNodeEvent.Moving);
        //            node.FireOnMoving(args);
        //            if (args.Cancel)
        //                throw new CancelNodeEventException(args.CancelMessage, args.EventType, node);
        //        }
        //        catch (Exception exception)
        //        {
        //            errors.Add(exception);
        //            //col2.Remove(node);
        //            nodesToRemove.Add(node.Id);
        //        }
        //    }
        //    sourceNodes.RemoveAll(n => nodesToRemove.Contains(n.Id));
        //    nodesToRemove.Clear();

        //    // Call MoveNode on DataProvider
        //    foreach (var node in sourceNodes)
        //    {
        //        try
        //        {
        //            DataProvider.Current.MoveNode(node.Id, targetNode.Id);
        //        }
        //        catch (DataOperationException e)
        //        {
        //            errors.Add(new ApplicationException("Cannot move", e));
        //            //col2.Remove(node);
        //            nodesToRemove.Add(node.Id);
        //        }
        //    }
        //    sourceNodes.RemoveAll(n => nodesToRemove.Contains(n.Id));
        //    nodesToRemove.Clear();

        //    // fire changed event
        //    foreach (var node in sourceNodes)
        //    {
        //        var pathToInvalidate = String.Concat(node.Path, "/");
        //        SecurityHandler.Move(node.Path, targetNode.Path);
        //        PathDependency.FireChanged(pathToInvalidate);
        //        PathDependency.FireChanged(node.Path);
        //    }

        //    try
        //    {
        //        var movedPaths = sourceNodes.Select(movedNode => originalPaths[movedNode.Id]).ToList();

        //        Populator.DeleteForest(movedPaths);
        //    }
        //    catch (Exception e)
        //    {
        //        errors.Add(e);
        //    }

        //    foreach (var node in sourceNodes)
        //    {
        //        var nodeHead = NodeHead.Get(node.Id);
        //        var userAccessLevel = GetUserAccessLevel(nodeHead);
        //        var acceptedLevel = GetAcceptedLevel(userAccessLevel, VersionNumber.LastAccessible);
        //        var versionId = GetVersionId(nodeHead, acceptedLevel != AccessLevel.Header ? acceptedLevel : AccessLevel.Major, VersionNumber.LastAccessible);

        //        var sharedData = DataBackingStore.GetNodeData(nodeHead, versionId);
        //        var privateData = NodeData.CreatePrivateData(sharedData.NodeData);
        //        node._data = privateData;

        //        try
        //        {
        //            Populator.PopulateTree(node.Path);
        //        }
        //        catch(Exception exception)
        //        {
        //            errors.Add(exception);
        //        }
        //        Logger.WriteVerbose("Node moved", GetLoggerPropertiesAfterMove, new object[] { node, originalPaths[node.Id], targetPath });
        //    }

        //    foreach (var node in sourceNodes)
        //        node.FireOnMoved(targetNode, originalPaths[node.Id] );
        //}
        private static void MoveMoreInternal2(NodeList<Node> sourceNodes, Node target, ref  List<Exception> errors)
        {
            if(target==null)
                throw new ArgumentNullException("target");
            foreach (var sourceNode in sourceNodes)
            {
                try
                {
                    sourceNode.MoveTo(target);
                }
                catch (Exception e) //not logged, not thrown
                {
                    errors.Add(e);
                }
            }
        }

        private static IDictionary<string, object> GetLoggerPropertiesAfterMove(object[] args)
        {
            var props = Logger.GetDefaultProperties(args[0]);
            props.Add("OriginalPath", args[1]);
            props.Add("NewPath", args[2]);
            return props;
        }

        #endregion

        #region //================================================================================================= Copy methods

        /// <summary>
        /// Copy the Node indentified by its path to another location. The destination node is also identified by path. 
        /// </summary>
        /// <remarks>Use this method if you do not want to instantiate the nodes.</remarks>
        public static void Copy(string sourcePath, string targetPath)
        {
            RepositoryPath.CheckValidPath(sourcePath);
            RepositoryPath.CheckValidPath(targetPath);
            Node sourceNode = Node.LoadNode(sourcePath);
            if (sourceNode == null)
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Source node does not exist with {0} path", sourcePath));
            Node targetNode = Node.LoadNode(targetPath);
            if (targetNode == null)
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Target node does not exist with {0} path", targetPath));
            sourceNode.CopyTo(targetNode);
        }

        public static void Copy(List<int> nodeList, string targetPath, ref List<Exception> errors)
        {
            if (nodeList == null)
                throw new ArgumentNullException("nodeList");
            if (nodeList.Count == 0)
                return;
            RepositoryPath.CheckValidPath(targetPath);

            CopyMoreInternal(nodeList, targetPath, ref errors);
        }
        private static void CopyMoreInternal(IEnumerable<int> nodeList, string targetNodePath, ref List<Exception> errors)
        {
            if (StorageContext.Search.SearchEngine.IndexingPaused)
                WaitForIndexingContinued("copy the contents");

            var col2 = new List<Node>();

            var targetNode = LoadNode(targetNodePath);
            if (targetNode == null)
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Target node does not exist with {0} path", targetNodePath));


            

            using (var traceOperation = Logger.TraceOperation("Node.CopyMoreInternal"))
            {
                //
                // #1 check copy conditions
                //
                foreach (var nodeId in nodeList)
                {
                    var n = LoadNode(nodeId);
                    if (n==null)    // node has already become unavailable
                        continue;
                    var msg = n.CheckListAndItemCopyingConditions(targetNode);
                    if (msg == null)
                    {
                        col2.Add(n);
                        continue;
                    }
                    errors.Add(new InvalidOperationException(msg));
                }
                var nodesToRemove = new List<int>();
                string correctTargetPath;
                foreach (var node in col2)
                {                   
                    correctTargetPath = RepositoryPath.Combine(targetNode.Path, RepositoryPath.PathSeparator);
                    var correctCurrentPath = RepositoryPath.Combine(node.Path, RepositoryPath.PathSeparator);

                    if (correctTargetPath.IndexOf(correctCurrentPath) == -1) 
                        continue;
                    errors.Add(new InvalidOperationException(String.Format("Node cannot be copied under itself: {0}.", correctCurrentPath)));
                    //col2.Remove(node);
                    nodesToRemove.Add(node.Id);
                }
                col2.RemoveAll(n => nodesToRemove.Contains(n.Id));
                nodesToRemove.Clear();

                targetNode.AssertLock();

                //
                // fire copying and cancel events
                //
                foreach (var node in col2)
                {
                    var args = new CancellableNodeOperationEventArgs(node, targetNode, CancellableNodeEvent.Copying);
                    node.FireOnCopying(args);
                    if (!args.Cancel) 
                        continue;
                    errors.Add(new CancelNodeEventException(args.CancelMessage, args.EventType, node));
                    //col2.Remove(node);
                    nodesToRemove.Add(node.Id);
                }
                col2.RemoveAll(n => nodesToRemove.Contains(n.Id));
                nodesToRemove.Clear();

                //
                //  copying
                //
                var targetChildren = targetNode.GetChildren();
                var targetNodeId = targetNode.Id;
                foreach (var node in col2)
                {
                    var originalPath = node.Path;
                    var newName = node.Name;
                    var targetName = newName;

                    int i = 0;

                    try
                    {
                        while (NameExists(targetChildren, targetName))
                        {
                            if (targetNodeId != node.ParentId)
                                throw new ApplicationException(String.Concat("This folder already contains a content named '", newName, "'."));
                            targetName = node.GenerateCopyName(i++);
                        }
                        correctTargetPath = RepositoryPath.Combine(targetNode.Path, RepositoryPath.PathSeparator);
                        var newPath = correctTargetPath + targetName;

                        node.DoCopy(newPath, targetName);

                        Logger.WriteVerbose("Node copied", GetLoggerPropertiesAfterCopy, new object[] { node, originalPath, newPath });
                        node.FireOnCopied(targetNode);
                    } catch(Exception e)
                    {
                        errors.Add(e);
                        //col2.Remove(node);
                    }
                }

                traceOperation.IsSuccessful = true;

            }


        }

        /// <summary>
        /// Copies the Node instance to another loacation. The new location is a Node instance which will be parent node.
        /// </summary>
        public virtual void CopyTo(Node target)
        {
            CopyTo(target, this.Name);
        }
        /// <summary>
        /// Copies the Node instance to another loacation. The new location is a Node instance which will be parent node.
        /// </summary>
        public virtual void CopyTo(Node target, string newName)
        {
            if (StorageContext.Search.SearchEngine.IndexingPaused)
                WaitForIndexingContinued("copy the content");

            using (var traceOperation = Logger.TraceOperation("Node.SaveCopied"))
            {
                if (target == null)
                    throw new ArgumentNullException("target");

                string msg = CheckListAndItemCopyingConditions(target);
                if (msg != null)
                    throw new InvalidOperationException(msg);

                var originalPath = this.Path;
                string newPath;
                var correctTargetPath = RepositoryPath.Combine(target.Path, RepositoryPath.PathSeparator);
                var correctCurrentPath = RepositoryPath.Combine(this.Path, RepositoryPath.PathSeparator);

                if (correctTargetPath.IndexOf(correctCurrentPath) != -1)
                    throw new InvalidOperationException("Node cannot be copied under itself.");

                target.AssertLock();

                var args = new CancellableNodeOperationEventArgs(this, target, CancellableNodeEvent.Copying);
                FireOnCopying(args);

                if (args.Cancel)
                    throw new CancelNodeEventException(args.CancelMessage, args.EventType, this);

                var targetName = newName;

                int i = 0;
                var nodeList = target.GetChildren();
                while (NameExists(nodeList, targetName))
                {
                    if (target.Id != this.ParentId)
                        throw new ApplicationException(String.Concat("This folder already contains a content named '", newName, "'."));
                    targetName = GenerateCopyName(i++);
                }

                newPath = correctTargetPath + targetName;
                DoCopy(newPath, targetName);

                Logger.WriteVerbose("Node copied", GetLoggerPropertiesAfterCopy, new object[] { this, originalPath, newPath });
                FireOnCopied(target);

                traceOperation.IsSuccessful = true;
            }
        }
        private string CheckListAndItemCopyingConditions(Node target)
        {
            string msg = null;
            bool sourceIsOuter = this.ContentListType == null;
            bool sourceIsList = !sourceIsOuter && this.ContentListId == 0;
            bool sourceIsItem = !sourceIsOuter && this.ContentListId != 0;
            
            //hack
            bool sourceIsSystemFolder = this.NodeType.IsInstaceOfOrDerivedFrom("SystemFolder");

            bool targetIsOuter = target.ContentListType == null;
            bool targetIsList = !targetIsOuter && target.ContentListId == 0;
            bool targetIsItem = !targetIsOuter && target.ContentListId != 0;
            if (sourceIsOuter && !targetIsOuter && !sourceIsSystemFolder)
            {
                msg = "Cannot copy outer item into a list. ";
            }
            else if (sourceIsList && !targetIsOuter)
            {
                msg = "Cannot copy a list into an another list. ";
            }
            else if (sourceIsItem)
            {
                //change: we don't mind if somebody copies an item out from the list
                //(it will lose the list fields though...)
                if (targetIsOuter)
                    msg = null; //"Cannot copy a list item out from the list. ";
                else if (targetIsList && this.ContentListType != target.ContentListType)
                    msg = "Cannot copy a list item into an another list. ";
                else if (targetIsItem && this.ContentListId != target.ContentListId)
                    msg = "Cannot copy a list item into an another list. ";
            }
            return msg;
        }

        private string GenerateCopyName(int index)
        {
            if (index == 0)
                return String.Concat("Copy of ", this.Name);
            return String.Concat("Copy (", index, ") of ", this.Name);
        }
        private void DoCopy(string targetPath, string newName)
        {
            bool first = true;
            var sourcePath = this.Path;
            foreach (var sourceNode in NodeEnumerator.GetNodes(sourcePath, ExecutionHint.ForceRelationalEngine))
            {
                var targetNodePath = targetPath + sourceNode.Path.Substring(sourcePath.Length);
                targetNodePath = RepositoryPath.GetParentPath(targetNodePath);
                var targetNode = Node.LoadNode(targetNodePath);
                var copy = sourceNode.MakeCopy(targetNode, newName);
                copy.Save();
                CopyExplicitPermissionsTo(sourceNode, copy);
                if (first)
                {
                    newName = null;
                    first = false;
                }
            }
        }
        private void CopyExplicitPermissionsTo(Node sourceNode, Node targetNode)
        {
            AccessProvider.ChangeToSystemAccount();
            try
            {
                var entriesToCopy = sourceNode.Security.GetExplicitEntries();
                if (entriesToCopy.Count() == 0)
                    return;

                var ed = targetNode.Security.GetAclEditor();
                foreach (var entry in entriesToCopy)
                    foreach (var permType in ActiveSchema.PermissionTypes)
                        ed.SetPermission(entry.PrincipalId, entry.Propagates, permType, entry.PermissionValues[permType.Id - 1]);
                ed.Apply();
                if (!targetNode.IsInherited)
                    targetNode.Security.RemoveBreakInheritance();
            }
            finally
            {
                AccessProvider.RestoreOriginalUser();
            }
        }

        public Node MakeTemplatedCopy(Node target, string newName)
        {
            var copy = MakeCopy(target, newName);
            copy._copying = false;

            return copy;
        }

        public virtual Node MakeCopy(Node target, string newName)
        {
            var copy = this.NodeType.CreateInstance(target);
            copy._copying = true;
            var name = newName ?? this.Name;
            var path = RepositoryPath.Combine(target.Path, name);
            copy.Data.Name = name;
            copy.Data.Path = path;
            this.Data.CopyGeneralPropertiesTo(copy.Data);
            CopyDynamicProperties(copy);

            // These properties must be copied this way. 
            copy["VersioningMode"] = this["VersioningMode"];
            copy["InheritableVersioningMode"] = this["InheritableVersioningMode"];
            copy["ApprovingMode"] = this["ApprovingMode"];
            copy["InheritableApprovingMode"] = this["InheritableApprovingMode"];

            return copy;
        }
        protected virtual void CopyDynamicProperties(Node target)
        {
            this.Data.CopyDynamicPropertiesTo(target.Data);
        }
        private static IDictionary<string, object> GetLoggerPropertiesAfterCopy(object[] args)
        {
            var props = Logger.GetDefaultProperties(args[0]);
            props.Add("OriginalPath", args[1]);
            props.Add("NewPath", args[2]);
            return props;
        }
        #endregion

        #region //==========================================================================Templated creation

        private Node _template;
        public Node Template
        {
            get { return _template; }
            set { _template = value; }
        }

        [Obsolete("Use ContentTemplate.CreateFromTemplate instead.", true)]
        public static Node CreateFromTemplate(Node target, Node template, string name)
        {
            Node newNode = template.MakeCopy(target, name);
            newNode._template = template;

            return newNode;
        }

        [Obsolete("Use ContentTemplate.CopyChildren instead.", true)]
        private void DeepcopyChildren()
        {
            using (var traceOperation = Logger.TraceOperation("Node.DeepcopyChildren"))
            {
                if (_template != null)
                {
                    IEnumerable<Node> children = _template.GetChildren();
                    foreach (Node child in children)
                        child.CopyTo(this);
                }
                traceOperation.IsSuccessful = true;
            }
        }

        #endregion

        #region //================================================================================================= Delete methods

        /// <summary>
        /// Deletes a Node and all of its contents from the database. This operation removes all child nodes too.
        /// </summary>
        /// <param name="sourcePath">The path of the Node that will be deleted.</param>
        [Obsolete("DeletePhysical is obsolete. Use ForceDelete to delete Node permanently.")]
        public static void DeletePhysical(string sourcePath)
        {
            RepositoryPath.CheckValidPath(sourcePath);
            var sourceNode = Node.LoadNode(sourcePath);
            if (sourceNode == null)
                throw new InvalidOperationException(String.Concat("Source node does not exist with ", sourcePath, " path"));
            sourceNode.Delete();
        }
        /// <summary>
        /// Deletes a Node and all of its contents from the database. This operation removes all child nodes too.
        /// </summary>
        /// <param name="nodeId">Identifier of the Node that will be deleted.</param>
        [Obsolete("DeletePhysical is obsolete. Use ForceDelete to delete Node permanently.")]
        public static void DeletePhysical(int nodeId)
        {
            var sourceNode = Node.LoadNode(nodeId);
            if (sourceNode == null)
                throw new InvalidOperationException(String.Concat("Source node does not exist with ", nodeId, " nodeId"));
            sourceNode.Delete();
        }
        /// <summary>
        /// Deletes the Node instance and all of its contents. This operation removes the appropriate nodes from the database.
        /// </summary>
        [Obsolete("The DeletePhysical is obsolete. Use ForceDelete to delete Node permanently.")]
        public virtual void DeletePhysical()
        {
            Delete();
        }

        public static void ForceDelete(string sourcePath)
        {
            RepositoryPath.CheckValidPath(sourcePath);
            var sourceNode = Node.LoadNode(sourcePath);
            if (sourceNode == null)
                throw new InvalidOperationException(String.Concat("Source node does not exist with ", sourcePath, " path"));
            sourceNode.ForceDelete();
        }

        public static void ForceDelete(int nodeId)
        {
            var sourceNode = Node.LoadNode(nodeId);
            if (sourceNode == null)
                throw new InvalidOperationException(String.Concat("Source node does not exist with ", nodeId, " nodeId"));
            sourceNode.ForceDelete();
        }
        
        /// <summary>
        /// This method deletes the node permanently
        /// </summary>
        public virtual void ForceDelete()
        {
            if (StorageContext.Search.SearchEngine.IndexingPaused)
                WaitForIndexingContinued("delete the content");

            this.Security.AssertSubtree(PermissionType.Delete);

            this.AssertLock();

            var myPath = Path;

            var args = new CancellableNodeEventArgs(this, CancellableNodeEvent.DeletingPhysically);
            FireOnDeletingPhysically(args);
            if (args.Cancel)
                throw new CancelNodeEventException(args.CancelMessage, args.EventType, this);

            var contentListTypesInTree = (this is IContentList) ?
                new List<ContentListType>(new[] { this.ContentListType }) : 
                DataProvider.Current.GetContentListTypesInTree(this.Path);

            var logProps = CollectAllProperties(this.Data);
            try
            {
                DataProvider.Current.DeleteNodePsychical(this.Id);
            }
            catch (Exception e) //rethrow
            {
                var msg = new StringBuilder("You cannot delete this content ");
                if (e.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                    msg.Append("because it is referenced by another content.");
                throw new ApplicationException(msg.ToString(), e);
            }

            MakePrivateData();
            this._data.IsDeleted = true;

            var hadContentList = RemoveContentListTypesInTree(contentListTypesInTree) > 0;

            SecurityHandler.Delete(myPath);
            Populator.DeleteTree(myPath);

            if (hadContentList)
                FireAnyContentListDeleted();

            PathDependency.FireChanged(myPath);
            Logger.WriteAudit(AuditEvent.ContentDeleted, logProps);
            FireOnDeletedPhysically();
        }
        private int RemoveContentListTypesInTree(List<ContentListType> contentListTypesInTree)
        {
            var count = 0;
            if (contentListTypesInTree.Count > 0)
            {
                var editor = new SchemaEditor();
                editor.Load();
                foreach (var t in contentListTypesInTree)
                {
                    if (t != null)
                    {
                        editor.DeleteContentListType(editor.ContentListTypes[t.Name]);
                        count++;
                    }
                }
                editor.Register();
            }
            return count;
        }

        /// <summary>
        /// Delete the node
        /// </summary>
        public static void Delete(string sourcePath)
        {
            RepositoryPath.CheckValidPath(sourcePath);
            var sourceNode = Node.LoadNode(sourcePath);
            if (sourceNode == null)
                throw new InvalidOperationException(String.Concat("Source node does not exist with ", sourcePath, " path"));
            sourceNode.Delete();
        }

        /// <summary>
        /// Delete the node
        /// </summary>
        public static void Delete(int nodeId)
        {
            var sourceNode = Node.LoadNode(nodeId);
            if (sourceNode == null)
                throw new InvalidOperationException(String.Concat("Source node does not exist with ", nodeId, " nodeId"));
            sourceNode.Delete();
        }

        /// <summary>
        /// Batch delete.
        /// </summary>
        /// <param name="nodeList">Represents an Id collection which holds the identifiers of the nodes will be deleted.</param>
        /// <param name="errors">If any error occures, it is added to the errors collection passed by errors parameter.</param>
        /// <exception cref="ArgumentNullException">You must specify a list collection instance.</exception>
        public static void Delete(List<int> nodeList, ref List<Exception> errors)
        {
            if (nodeList == null) 
                throw new ArgumentNullException("nodeList");
            if (nodeList.Count == 0)
                return;               
            Node.DeleteMoreInternal(nodeList, ref errors);  
        }
        
        //
        // TODO: need to consider> method based upon the original DeleteInternal, this contains duplicated source code
        //
        private static void DeleteMoreInternal(ICollection<Int32> nodeList, ref List<Exception> errors)
        {
            if (StorageContext.Search.SearchEngine.IndexingPaused)
                WaitForIndexingContinued("delete the contents");

            if (nodeList == null)
                throw new ArgumentNullException("nodeList");
            if (nodeList.Count == 0)
                return;

            var col2 = new List<Node>();
            var col3 = new List<Node>();

            foreach (Int32 n in nodeList)
            {
                var node = LoadNode(n);
                try
                {
                    node.Security.AssertSubtree(PermissionType.Delete);
                    node.AssertLock();                    
                    
                } catch(Exception e)
                {
                    errors.Add(e);
                    continue;
                }
                col2.Add(node);
            }

            
            foreach (var nodeRef in col2)
            {
                var internalError = false;
                var myPath = nodeRef.Path;

                var args = new CancellableNodeEventArgs(nodeRef, CancellableNodeEvent.DeletingPhysically);
                nodeRef.FireOnDeletingPhysically(args);
                if (args.Cancel)
                    throw new CancelNodeEventException(args.CancelMessage, args.EventType, nodeRef);


                //var logProps = CollectAllProperties(nodeRef.Data);

                try
                {
                    DataProvider.Current.DeleteNodePsychical(nodeRef.Id);   
                }
                catch (Exception e) //rethrow
                {
                    var msg = new StringBuilder("You cannot delete this content ");
                    if (e.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                        msg.Append("because it is referenced by another content.");
                    //throw new ApplicationException(msg.ToString(), e);
                    internalError = true;
                    errors.Add(new ApplicationException(msg.ToString(), e));
                }
                if (internalError)
                    continue;

                col3.Add(nodeRef);

                nodeRef._data.IsDeleted = true;

                if (nodeRef is IContentList)
                {
                    if (nodeRef.ContentListType != null)
                    {
                        var editor = new SchemaEditor();
                        editor.Load();
                        editor.DeleteContentListType(editor.ContentListTypes[nodeRef.ContentListType.Name]);
                        editor.Register();
                    }
                }
                SecurityHandler.Delete(myPath);
            }

            var ids = new List<Int32>();
            for (int index = 0; index < col3.Count; index++)
            {
                var n = col3[index];
                ids.Add(n.Id);
            }
            try
            {
                Populator.DeleteForest(ids);    
            } catch(Exception e)
            {
                errors.Add(e);
            }
            

            for (int index = 0; index < col3.Count; index++)
            {
                var n = col3[index];
                PathDependency.FireChanged(n.Path);
                //Logger.WriteAudit(AuditEvent.ContentDeleted, n.Path);
                n.FireOnDeletedPhysically();
            }
        }




        /// <summary>
        /// Delete current node
        /// </summary>
        public virtual void Delete()
        {
            ForceDelete();
        }

        ///// <summary>
        ///// Delete the current node version.
        ///// </summary>
        //public virtual void DeleteVersion()
        //{
        //    var deletedVersionNumber = this.Version.Clone();

        //    DeleteVersion(this);
        //    var nodeHead = NodeHead.Get(Id);
        //    var sharedData = DataBackingStore.GetNodeData(nodeHead, nodeHead.LastMinorVersionId);
        //    var privateData = NodeData.CreatePrivateData(sharedData.NodeData);

        //    _data = privateData;

        //    Logger.WriteVerbose("Version deleted.", GetLoggerPropertiesAfterDeleteVersion, new object[] { this, deletedVersionNumber });
        //}
        //private static IDictionary<string, object> GetLoggerPropertiesAfterDeleteVersion(object[] args)
        //{
        //    var props = Logger.GetDefaultProperties(args[0]);
        //    props.Add("DeletedVersion", args[1]);
        //    return props;
        //}

        ///// <summary>
        ///// Delete the specified node version.
        ///// </summary>
        //public static void DeleteVersion(int majorNumber, int minorNumber, int nodeId)
        //{
        //    var oldVersion = Node.LoadNode(nodeId, new VersionNumber(majorNumber, minorNumber));
        //    if (oldVersion != null)
        //        DeleteVersion(oldVersion);
        //}
        //private static void DeleteVersion(Node oldVersion)
        //{
        //    int majorNumber = oldVersion.Version.Major;
        //    int minorNumber = oldVersion.Version.Minor;
        //    int nodeId=oldVersion.Id;
        //    int versionId = oldVersion.VersionId;

        //    var populatorData = Populator.BeginDeleteVersion(oldVersion);
        //    DataProvider.Current.DeleteVersion(majorNumber, minorNumber, nodeId);
        //    NodeIdDependency.FireChanged(nodeId);
        //    Populator.CommitDeleteVersion(populatorData);
        //}

        #endregion

        private static void WaitForIndexingContinued(string msg)
        {
            for (int i = 0; i < 100; i++)
            {
Debug.WriteLine(String.Format("#> {0} ---- WaitForIndexingContinued", AppDomain.CurrentDomain.FriendlyName));
                System.Threading.Thread.Sleep(100);
                if (!StorageContext.Search.SearchEngine.IndexingPaused)
                    return;
            }
            throw new Exception(String.Format("Cannot {0} during maintenance process. Please try again some minutes later.", msg));
        }

        #region //================================================================================================= Events

        private List<Type> _disabledObservers;
        public IEnumerable<Type> DisabledObservers { get { return _disabledObservers; } }
        public void DisableObserver(Type observerType)
        {
            if (_disabledObservers == null)
                _disabledObservers = new List<Type>();
            if (!_disabledObservers.Contains(observerType))
                _disabledObservers.Add(observerType);
        }

        public event CancellableNodeEventHandler Creating;
        public event EventHandler<NodeEventArgs> Created;
        public event CancellableNodeEventHandler Modifying;
        public event EventHandler<NodeEventArgs> Modified;
        public event CancellableNodeEventHandler Deleting;
        public event EventHandler<NodeEventArgs> Deleted;
        public event CancellableNodeEventHandler DeletingPhysically;
        public event EventHandler<NodeEventArgs> DeletedPhysically;
        public event CancellableNodeOperationEventHandler Moving;
        public event EventHandler<NodeOperationEventArgs> Moved;
        public event CancellableNodeOperationEventHandler Copying;
        public event EventHandler<NodeOperationEventArgs> Copied;
        //TODO: public event EventHandler Undeleted;
        //TODO: public event EventHandler Locked;
        //TODO: public event EventHandler Unlocked;
        //TODO: public event EventHandler LockRemoved;

        private void FireOnCreating(CancellableNodeEventArgs e)
        {
            OnCreating(this, e);
            if (e.Cancel)
                return;
            NodeObserver.FireOnNodeCreating(Creating, this, e, _disabledObservers);
        }
        private void FireOnCreated()
        {
            NodeEventArgs e = new NodeEventArgs(this, NodeEvent.Created);
            OnCreated(this, e);
            NodeObserver.FireOnNodeCreated(Created, this, e, _disabledObservers);
        }
        private void FireOnModifying(CancellableNodeEventArgs e)
        {
            OnModifying(this, e);
            if (e.Cancel)
                return;
            NodeObserver.FireOnNodeModifying(Modifying, this, e, _disabledObservers);
        }
        private void FireOnModified(string originalSourcePath, IEnumerable<ChangedData> changedData)
        {
            NodeEventArgs e = new NodeEventArgs(this, NodeEvent.Modified, originalSourcePath, changedData);
            OnModified(this, e);
            NodeObserver.FireOnNodeModified(Modified, this, e, _disabledObservers);
        }
        private void FireOnDeleting(CancellableNodeEventArgs e)
        {
            OnDeleting(this, e);
            if (e.Cancel)
                return;
            NodeObserver.FireOnNodeDeleting(Deleting, this, e, _disabledObservers);
        }
        private void FireOnDeleted()
        {
            NodeEventArgs e = new NodeEventArgs(this, NodeEvent.Deleted);
            OnDeleted(this, e);
            NodeObserver.FireOnNodeDeleted(Deleted, this, e, _disabledObservers);
        }
        private void FireOnDeletingPhysically(CancellableNodeEventArgs e)
        {
            OnDeletingPhysically(this, e);
            if (e.Cancel)
                return;
            NodeObserver.FireOnNodeDeletingPhysically(DeletingPhysically, this, e, _disabledObservers);
        }
        private void FireOnDeletedPhysically()
        {
            NodeEventArgs e = new NodeEventArgs(this, NodeEvent.DeletedPhysically);
            OnDeletedPhysically(this, e);
            NodeObserver.FireOnNodeDeletedPhysically(DeletedPhysically, this, e, _disabledObservers);
        }
        private void FireOnMoving(CancellableNodeOperationEventArgs e)
        {
            OnMoving(this, e);
            if (e.Cancel)
                return;
            NodeObserver.FireOnNodeMoving(Moving, this, e, _disabledObservers);
        }
        private void FireOnMoved(Node targetNode, string originalSourcePath)
        {
            NodeOperationEventArgs e = new NodeOperationEventArgs(this, targetNode, NodeEvent.Moved, originalSourcePath);
            OnMoved(this, e);
            NodeObserver.FireOnNodeMoved(Moved, this, e, _disabledObservers);
        }
        private void FireOnCopying(CancellableNodeOperationEventArgs e)
        {
            OnCopying(this, e);
            if (e.Cancel)
                return;
            NodeObserver.FireOnNodeCopying(Copying, this, e, _disabledObservers);
        }
        private void FireOnCopied(Node targetNode)
        {
            NodeOperationEventArgs e = new NodeOperationEventArgs(this, targetNode, NodeEvent.Copied);
            OnCopied(this, e);
            NodeObserver.FireOnNodeCopied(Copied, this, e, _disabledObservers);
        }
        private void FireOnLoaded()
        {
            var e = new NodeEventArgs(this, NodeEvent.Loaded);
            OnLoaded(this, e);
            //NodeObserver.FireOnNodeLoaded(Created, this, e, _disabledObservers);
        }

        protected virtual void OnCreating(object sender, CancellableNodeEventArgs e) { }
        protected virtual void OnCreated(object sender, NodeEventArgs e) { }
        protected virtual void OnModifying(object sender, CancellableNodeEventArgs e) { }
        protected virtual void OnModified(object sender, NodeEventArgs e) { }
        protected virtual void OnDeleting(object sender, CancellableNodeEventArgs e) { }
        protected virtual void OnDeleted(object sender, NodeEventArgs e) { }
        protected virtual void OnDeletingPhysically(object sender, CancellableNodeEventArgs e) { }
        protected virtual void OnDeletedPhysically(object sender, NodeEventArgs e) { }
        protected virtual void OnMoving(object sender, CancellableNodeOperationEventArgs e) { }
        protected virtual void OnMoved(object sender, NodeOperationEventArgs e) { }
        protected virtual void OnCopying(object sender, CancellableNodeOperationEventArgs e) { }
        protected virtual void OnCopied(object sender, NodeOperationEventArgs e) { }
        protected virtual void OnLoaded(object sender, NodeEventArgs e) { }

        #endregion

        public static event EventHandler AnyContentListDeleted;
        private void FireAnyContentListDeleted()
        {
            if (AnyContentListDeleted != null)
                AnyContentListDeleted(this, EventArgs.Empty);
        }


        #region //================================================================================================= Public Tools

        public bool IsPropertyChanged(string propertyName)
        {
            return Data.IsPropertyChanged(propertyName);
        }

        public static bool Exists(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            return DataProvider.NodeExists(path);
        }

        public Node LoadContentList()
        {
            if (this.ContentListId == 0)
                return null;
            return Node.LoadNode(this.ContentListId);
        }

        public static Node GetAncestorOfNodeType(Node child, string typeName)
        {
            while ((child != null) && (!child.NodeType.IsInstaceOfOrDerivedFrom(typeName)))
                child = child.Parent;

            return child;
        }

        public static T GetAncestorOfType<T>(Node child) where T : Node
        {
            T ancestor = null;

            while ((child != null) && ((ancestor = child as T) == null))
                child = child.Parent;

            return ancestor;
        }

        public Node GetAncestor(int ancestorIndex)
        {
            if (ancestorIndex == 0) return this;
            if (ancestorIndex < 0)
                throw new NotSupportedException("AncestorIndex < 0");

            //TODO: implement unsafe str* handling to get number of slashes
            string[] path = this.Path.Split('/');
            if (ancestorIndex >= path.Length)
                throw new ApplicationException("ancestorIndex overflow");

            //TODO: implement unsafe str* handling
            string ancestorPath = string.Join("/", path, 0, path.Length - ancestorIndex);

            Node ancestor = Node.LoadNode(ancestorPath);

            return ancestor;
        }
        public bool IsDescendantOf(Node ancestor)
        {
            return (this.Path.StartsWith(ancestor.Path + "/"));
        }
        public bool IsDescendantOf(Node ancestor, out int distance)
        {
            distance = -1;
            if (!IsDescendantOf(ancestor))
                return false;
            distance = this.NodeLevel() - ancestor.NodeLevel();
            return true;
        }

        ///// <summary>
        ///// Check if there is a Node with the same name with the same parent Node.
        ///// </summary>
        ///// <returns>True if path is available.</returns>
        //public bool IsPathAvailable
        //{
        //    get
        //    {
        //        string parentPath = RepositoryPath.GetParentPath(this.Path);
        //        string newPath = string.Concat(parentPath, RepositoryPath.PathSeparator, this.Name);
        //        return !DataProvider.NodeExists(newPath, this.Id);
        //    }
        //}

        /// <summary>
        /// Returns the level of hierachy the node is located at. The virtual Root node has always
        /// a level of 0.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int NodeLevel()
        {
            return this.Path.Split('/').Length - 2;
        }

        public long GetFullSize()
        {
            return Node.GetTreeSize(this.Path, false);
        }

        public long GetTreeSize()
        {
            return Node.GetTreeSize(this.Path, true);
        }

        public static long GetTreeSize(string path)
        {
            return GetTreeSize(path, true);
        }

        private static long GetTreeSize(string path, bool includeChildren)
        {
            return DataProvider.Current.GetTreeSize(path, includeChildren);
        }

        #endregion

        #region //================================================================================================= Private Tools

        private void AssertSeeOnly(string propertyName)
        {
            if (IsHeadOnly && !SEE_ENABLED_PROPERTIES.Contains(propertyName)) // && AccessProvider.Current.GetCurrentUser().Id != -1)
            {
                throw new InvalidOperationException(String.Concat("Invalid property access attempt on a See-only node. The accessible properties are: ", String.Join(", ", SEE_ENABLED_PROPERTIES), "."));
            }
        }
        private void AssertLock()
        {
            if ((Lock.LockedBy != null && Lock.LockedBy.Id != AccessProvider.Current.GetCurrentUser().Id) && Lock.Locked)
                throw new LockedNodeException(Lock);
        }
        private static bool NameExists(IEnumerable<Node> nodeList, string name)
        {
            foreach (Node node in nodeList)
                if (node.Name == name)
                    return true;
            return false;
        }
        private static bool IsValidName(string name)
        {
            return !RepositoryPath.NameContainsInvalidChar(name);
        }
        private Exception Exception_ReferencedNodeCouldNotBeLoadedException(string referenceCategory, int referenceId, Exception innerException)
        {
            return new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "The '{0}' could not be loaded because it has been deleted, or the actual user hasn't got sufficient rights to see that node.\nThe NodeId of this node is {1}, the reference id is {2}.", referenceCategory, this.Id, referenceId), innerException);
        }

        #endregion





    }
}
