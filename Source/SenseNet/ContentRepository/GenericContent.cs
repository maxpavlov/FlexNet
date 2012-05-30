using System;
using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Diagnostics;
using System.Text;
using SenseNet.Search;
using SenseNet.Search.Indexing;
using System.Diagnostics;

namespace SenseNet.ContentRepository
{
    public enum SavingMode { RaiseVersion, RaiseVersionAndLock, KeepVersion }
    public enum CheckInCommentsMode { None, Recommended, Compulsory }

    [ContentHandler]
    public partial class GenericContent : Node, IIndexableDocument
    {
        protected GenericContent(Node parent)
            : base(parent)
        {
            VersionSetup();
            Initialize();
        }
        public GenericContent(Node parent, string nodeTypeName)
            : base(parent, nodeTypeName)
        {
            VersionSetup();
            Initialize();
        }
        protected GenericContent(NodeToken nt)
            : base(nt)
        {
            VersionSetup();
        }

        private Content _content;
        public Content Content
        {
            get
            {
                if (_content == null)
                    _content = Content.Create(this);
                return _content;
            }
        }

        protected virtual void Initialize()
        {
            if (this.Id > 0)
                return;
            var content = this.Content;
            foreach(var item in content.Fields)
                item.Value.SetDefaultValue();
        }

        public ContentType ContentType
        {
            get { return ContentType.GetByName(this.NodeType.Name); }
        }

        public override string DisplayName
        {
            get
            {
                var result = base.DisplayName;
                if (String.IsNullOrEmpty(result) && this.Id != 0)
                    result = this.Name;
                return result;
            }
            set
            {
                base.DisplayName = value;
            }
        }

        [RepositoryProperty("Description", RepositoryDataType.Text)]
        public virtual string Description
        {
            get
            {
                return base.GetProperty<string>("Description");
            }
            set
            {
                this["Description"] = value;
            }
        }

        [RepositoryProperty("Hidden", RepositoryDataType.Int)]
        public virtual bool Hidden
        {
            get { return base.GetProperty<int>("Hidden") != 0; }
            set { this["Hidden"] = value ? 1 : 0; }
        }

        public const string VERSIONINGMODE = "VersioningMode";
        [RepositoryProperty(VERSIONINGMODE, RepositoryDataType.Int)]
        public virtual VersioningType VersioningMode
        {
            get
            {
                var vt = base.GetProperty<VersioningType>(VERSIONINGMODE);
                if (vt > VersioningType.Inherited)
                    return vt;

                using (new SystemAccount())
                {
                    var parent = this.Parent as GenericContent;
                    return parent == null ? VersioningType.None : (VersioningType)parent.InheritableVersioningMode;
                }
            }
            set
            {
                this[VERSIONINGMODE] = value;
            }
        }

        private bool _inheritableVersioningModeChanged = false;

        [RepositoryProperty("InheritableVersioningMode", RepositoryDataType.Int)]
        public virtual InheritableVersioningType InheritableVersioningMode
        {
            get
            {
                if (!HasProperty("InheritableVersioningMode"))
                    return InheritableVersioningType.None;

                var ivt = (InheritableVersioningType)this["InheritableVersioningMode"];
                if (ivt > InheritableVersioningType.Inherited)
                    return ivt;

                using (new SystemAccount())
                {
                    var parent = this.Parent as GenericContent;
                    return parent == null ? InheritableVersioningType.None : parent.InheritableVersioningMode;
                }
            }
            set
            {
                if (!HasProperty("InheritableVersioningMode"))
                    return;

                if (value != base.GetProperty<InheritableVersioningType>("InheritableVersioningMode"))
                {
                    _inheritableVersioningModeChanged = true;
                    this["InheritableVersioningMode"] = value;
                }
            }
        }

        [RepositoryProperty("ApprovingMode", RepositoryDataType.Int)]
        public virtual ApprovingType ApprovingMode
        {
            get
            {
                if (!HasProperty("ApprovingMode"))
                    return ApprovingType.False;
                var at = base.GetProperty<ApprovingType>("ApprovingMode");
                if (at > ApprovingType.Inherited)
                    return at;

                using (new SystemAccount())
                {
                    var parent = this.Parent as GenericContent;
                    return parent == null ? ApprovingType.False : parent.InheritableApprovingMode;
                }
            }
            set
            {
                if (HasProperty("ApprovingMode"))
                    this["ApprovingMode"] = value;
            }
        }

        [RepositoryProperty("InheritableApprovingMode", RepositoryDataType.Int)]
        public virtual ApprovingType InheritableApprovingMode
        {
            get
            {
                if (!HasProperty("InheritableApprovingMode"))
                    return ApprovingType.False;

                var at = base.GetProperty<ApprovingType>("InheritableApprovingMode");
                if (at > ApprovingType.Inherited)
                    return at;

                using (new SystemAccount())
                {
                    var parent = this.Parent as GenericContent;
                    return parent == null ? ApprovingType.False : parent.InheritableApprovingMode;
                }
            }
            set
            {
                if (HasProperty("InheritableApprovingMode"))
                    this["InheritableApprovingMode"] = value;
            }
        }

        //[RepositoryProperty("ContentTypes", RepositoryDataType.Reference)]
        //public IEnumerable<ContentType> ContentTypes
        //{
        //    get
        //    {
        //        //return real schema items, not just the nodes
        //        return from refCt in this.GetReferences("ContentTypes")
        //               select ContentType.GetByName(refCt.Name);
        //    }
        //    set
        //    {
        //        if (this.NodeType.Name == typeof(Folder).Name)
        //            return; // throw new InvalidOperationException("Cannot set ContentTypes property on the Content that's type is exactly 'Folder'.");

        //        //set the property with the real schema items
        //        var cts = from refCt in value
        //                  select ContentType.GetByName(refCt.Name);
        //        this.SetReferences("ContentTypes", cts);
        //    }
        //}
        public const string ALLOWEDCHILDTYPES = "AllowedChildTypes";
        [RepositoryProperty(ALLOWEDCHILDTYPES, RepositoryDataType.Text)]
        public virtual IEnumerable<ContentType> AllowedChildTypes
        {
            get
            {
                var value = this.GetProperty<string>(ALLOWEDCHILDTYPES);
                if (String.IsNullOrEmpty(value))
                    return ContentType.EmptyAllowedChildTypes;
                var names =     value.Split(ContentType.XmlListSeparators, StringSplitOptions.RemoveEmptyEntries);
                var result = new List<ContentType>(names.Length);
                ContentType ct;
                for (int i = 0; i < names.Length; i++)
                    if ((ct = ContentType.GetByName(names[i])) != null)
                        result.Add(ct);
                return result;
            }
            set
            {
                var names = value == null ? null : String.Join(" ", value.Select(x => x.Name));
                this[ALLOWEDCHILDTYPES] = names;
            }
        }


        public virtual CheckInCommentsMode CheckInCommentsMode
        {
            get
            {
                //overwritten in File type!
                return CheckInCommentsMode.None;
            }
        }

        public User CheckedOutTo
        {
            get
            {
                return this.LockedBy as User;
            }
        }

        public bool InheritedVersioning
        {
            get
            {
                var vt = base.GetProperty<VersioningType>(VERSIONINGMODE);
                return vt <= VersioningType.Inherited;
            }
        }

        public bool InheritedInheritableVersioning
        {
            get
            {
                var vt = base.GetProperty<InheritableVersioningType>("InheritableVersioningMode");
                return vt <= InheritableVersioningType.Inherited;
            }
        }

        public bool InheritedApproving
        {
            get
            {
                var at = base.GetProperty<ApprovingType>("ApprovingMode");
                return at <= ApprovingType.Inherited;
            }
        }

        public bool InheritedInheritableApproving
        {
            get
            {
                var at = base.GetProperty<ApprovingType>("InheritableApprovingMode");
                return at <= ApprovingType.Inherited;
            }
        }

        public bool HasApproving
        {
            get
            {
                return this.ApprovingMode == ApprovingType.True ? true : false;
            }
        }

        private bool? _workflowsRunning;

        public bool WorkflowsRunning
        {
            get
            {
                if (_workflowsRunning.HasValue)
                    return _workflowsRunning.Value;

                if (!StorageContext.Search.IsOuterEngineEnabled)
                {
                    _workflowsRunning = false;
                    return false;
                }

                var cl = ContentList.GetContentListForNode(this);
                if (cl == null)
                {
                    _workflowsRunning = false;
                    return false;
                }

                var query = string.Format("+InTree:\"{0}\" +TypeIs:Workflow +WorkflowStatus:1 +RelatedContent:{1} .AUTOFILTERS:OFF .LIFESPAN:OFF .COUNTONLY",
                                          cl.Path + "/Workflows", this.Id);

                _workflowsRunning = ContentQuery.Query(query).Count > 0;

                return _workflowsRunning.Value;
            }
        }

        private GenericContent _workspace;
        public GenericContent Workspace
        {
            get
            {
                if (_workspace == null)
                {
                    _workspace = Node.GetAncestorOfNodeType(this, "Workspace") as GenericContent;
                }

                return _workspace;
            }
        }

        public string WorkspaceName
        {
            get { return this.Workspace == null ? string.Empty : this.Workspace.Name; }
        }

        public string WorkspaceTitle
        {
            get { return this.Workspace == null ? string.Empty : this.Workspace.DisplayName; }
        }

        public string WorkspacePath
        {
            get { return this.Workspace == null ? string.Empty : this.Workspace.Path; }
        }

        [RepositoryProperty("TrashDisabled", RepositoryDataType.Int)]
        public bool TrashDisabled
        {
            get
            {
                //TODO: re-think trash enabled/disabled logic
                //var pcont = this.Parent as GenericContent;
                //if (pcont != null && pcont.TrashDisabled)
                //    return true;

                return base.GetProperty<int>("TrashDisabled") != 0;
            }
            set { this["TrashDisabled"] = value ? 1 : 0; }
        }

        [RepositoryProperty("EnableLifespan", RepositoryDataType.Int)]
        public bool EnableLifespan
        {
            get { return base.GetProperty<int>("EnableLifespan") != 0; }
            set { this["EnableLifespan"] = value ? 1 : 0; }
        }

        [RepositoryProperty("ValidFrom", RepositoryDataType.DateTime)]
        public DateTime ValidFrom
        {
            get { return base.GetProperty<DateTime>("ValidFrom"); }
            set { this["ValidFrom"] = value; }
        }

        [RepositoryProperty("ValidTill", RepositoryDataType.DateTime)]
        public DateTime ValidTill
        {
            get { return base.GetProperty<DateTime>("ValidTill"); }
            set { this["ValidTill"] = value; }
        }

        public virtual object GetProperty(string name)
        {
            switch (name)
            {
                case "DisplayName":
                    return this.DisplayName;
                case "Hidden":
                    return this.Hidden;
                case "TrashDisabled":
                    return this.TrashDisabled;
                case VERSIONINGMODE:
                    return this.VersioningMode;
                case "InheritableVersioningMode":
                    return this.InheritableVersioningMode;
                case "ApprovingMode":
                    return this.ApprovingMode;
                case "InheritableApprovingMode":
                    return this.InheritableApprovingMode;
                case ALLOWEDCHILDTYPES:
                    return this.AllowedChildTypes;
                case "EnableLifespan":
                    return this.EnableLifespan;
                case "ValidFrom":
                    return this.ValidFrom;
                case "ValidTill":
                    return this.ValidTill;
                case "Workspace":
                    return this.Workspace;
                case "WorkspaceName":
                    return this.WorkspaceName;
                case "WorkspacePath":
                    return this.WorkspacePath;
                case "WorkspaceTitle":
                    return this.WorkspaceTitle;
                case "BrowseApplication":
                    return this.BrowseApplication;
                case "Approvable":
                    return this.Approvable;
                case "Publishable":
                    return this.Publishable;
                case "Versions":
                    return this.Versions;
                case "WorkflowsRunning":
                    return this.WorkflowsRunning;
                case "CheckedOutTo":
                    return this.CheckedOutTo;
                default:
                    return base[name];
            }
        }
        public virtual void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "DisplayName":
                    this.DisplayName = (string)value;
                    break;
                case "TrashDisabled":
                    this.TrashDisabled = (bool)value;
                    break;
                case "Hidden":
                    this.Hidden = (bool)value;
                    break;
                case ALLOWEDCHILDTYPES:
                    this.AllowedChildTypes = ((System.Collections.IEnumerable)value).Cast<ContentType>();
                    //this.ContentTypes = ((IEnumerable<Node>)value).Cast<ContentType>();
                    break;
                case "BrowseApplication":
                    this.BrowseApplication = value as Node;
                    break;
                case "EnableLifespan":
                    this.EnableLifespan = (bool)value;
                    break;
                case "ValidFrom":
                    this.ValidFrom = value == null ? DateTime.MinValue : (DateTime)value;
                    break;
                case "ValidTill":
                    this.ValidTill = value == null ? DateTime.MinValue : (DateTime)value;
                    break;
                case "Workspace":
                case "WorkspaceName":
                case "WorkspacePath":
                case "WorkspaceTitle":
                case "Approvable":
                case "Publishable":
                case "Versions":
                case "WorkflowsRunning":
                case "CheckedOutTo":
                    //do nothing, these props are readonly
                    break;
                default:
                    base[name] = value;
                    break;
            }
        }

        //============================================================================================= Allowed child types API

        public IEnumerable<string> GetAllowedChildTypeNames()
        {
            // in case of folders and pages inherit settings from parent
            if (this.NodeType.Name == "Folder" || this.NodeType.Name == "Page")
            {
                var parent = Parent as GenericContent;
                if (parent == null)
                    return ContentType.EmptyAllowedChildTypeNames;
                return parent.GetAllowedChildTypeNames();
            }

            // collect types set on local instance
            var names = new List<string>();        
            var hasLocalItems = false;
            foreach (var ct in this.AllowedChildTypes)
            {
                hasLocalItems = true; //-- Indicates that the local list has items. The length of list is not enough because the permission filters the list and if the filter skips all elements, the user gets the list that declared on the content type.
                if (ct.Security.HasPermission(PermissionType.See))
                    names.Add(ct.Name);
            }

            // SystemFolder and TrashBag allows every type if there is no setting on local instance
            var systemFolderName = "SystemFolder";
            if (!hasLocalItems && (this.NodeType.Name == systemFolderName || this.NodeType.Name == "TrashBag"))
                return new string[0];

            // SystemFolder can be created anywhere if the user has the necessary permissions on the CTD
            var systemFolderType = ContentType.GetByName(systemFolderName);
            if (systemFolderType.Security.HasPermission(PermissionType.See))
                if (!names.Contains(systemFolderName))
                    names.Add(systemFolderName);

            if (hasLocalItems)
                return names;

            // settings come from CTD if no local setting is present
            foreach (var ct in this.ContentType.AllowedChildTypes)
            {
                if (ct.Security.HasPermission(PermissionType.See))
                    if (!names.Contains(ct.Name))
                        names.Add(ct.Name);
            }

            return names;
        }
        public IEnumerable<ContentType> GetAllowedChildTypes()
        {
            // in case of folders and pages inherit settings from parent
            if (this.NodeType.Name == "Folder" || this.NodeType.Name == "Page")
            {
                var parent = Parent as GenericContent;
                if (parent == null)
                    return ContentType.EmptyAllowedChildTypes;
                return parent.GetAllowedChildTypes();
            }

            // collect types set on local instance
            var types = new List<ContentType>();
            var hasLocalItems = false;
            foreach (var ct in this.AllowedChildTypes)
            {
                hasLocalItems = true; //-- Indicates that the local list has items. The length of list is not enough because the permission filters the list and if the filter skips all elements, the user gets the list that declared on the content type.
                if (ct.Security.HasPermission(PermissionType.See))
                    types.Add(ct);
            }

            // SystemFolder and TrashBag allows every type if there is no setting on local instance
            var systemFolderName = "SystemFolder";
            if (!hasLocalItems && (this.NodeType.Name == systemFolderName || this.NodeType.Name == "TrashBag"))
                return new ContentType[0];

            // SystemFolder can be created anywhere if the user has the necessary permissions on the CTD
            var systemFolderType = ContentType.GetByName(systemFolderName);
            if (systemFolderType.Security.HasPermission(PermissionType.See))
                if (!types.Contains(systemFolderType))
                    types.Add(systemFolderType);

            if (hasLocalItems)
                return types;

            // settings come from CTD if no local setting is present
            foreach (var ct in this.ContentType.AllowedChildTypes)
            {
                if (ct.Security.HasPermission(PermissionType.See))
                    if (!types.Contains(ct))
                        types.Add(ct);
            }

            return types;
        }
        public bool IsAllowedChildType(string contentTypeName)
        {
            var list = GetAllowedChildTypeNames();
            if (list.Count() == 0)
                return true;
            return list.Contains(contentTypeName);
        }
        public bool IsAllowedChildType(ContentType contentType)
        {
            return IsAllowedChildType(contentType.Name);
        }

        public enum TypeAllow { Allowed, TypeIsNotPermitted, NotAllowed }
        internal void AssertAllowedChildType(Node node, bool move = false)
        {
            switch (CheckAllowedChildType(node))
            {
                case TypeAllow.Allowed:
                    return;
                case TypeAllow.TypeIsNotPermitted:
                    throw new SenseNetSecurityException(node.Path, PermissionType.Open, User.Current);
                case TypeAllow.NotAllowed:
                    if (move)
                        throw GetNotAllowedContentTypeExceptionOnMove(node, this);
                    else
                        throw GetNotAllowedContentTypeExceptionOnCreate(node, this);
                default:
                    break;
            }
        }
        internal TypeAllow CheckAllowedChildType(Node node)
        {
            var contentTypeName = node.NodeType.Name;

            //-- Ok if the new node is exactly TrashBag
            if (contentTypeName == "TrashBag")
                return TypeAllow.Allowed;

            //-- Ok if the new node is exactly SystemFolder and it is permitted
            if (contentTypeName == typeof(SystemFolder).Name)
            {
                var contentType = ContentType.GetByName(contentTypeName);
                if(contentType.Security.HasPermission(PermissionType.Open))
                    return TypeAllow.Allowed;
                return TypeAllow.TypeIsNotPermitted;
            }

            //-- Get parent if this is Folder or Page. Exit if current is SystemFolder or it is not a GenericContent
            var current = this;
            while (current != null && (current.NodeType.Name == "Folder" || current.NodeType.Name == "Page")) //HACK: using an object when it is unknown (Page)
                current = current.Parent as GenericContent;
            if (current != null && current.NodeType.Name == "SystemFolder")
                return TypeAllow.Allowed;
            if (current == null)
                return TypeAllow.Allowed;

            if(current.IsAllowedChildType(contentTypeName))
                return TypeAllow.Allowed;
            return TypeAllow.NotAllowed;
        }

        private Exception GetNotAllowedContentTypeExceptionOnCreate(Node node, GenericContent parent)
        {
            var ancestor = parent;
            while (ancestor.NodeType.Name == "Folder" || ancestor.NodeType.Name == "Page")
            {
                var p = ancestor.Parent as GenericContent;
                if (p == null)
                    break;
                ancestor = p;
            }

            var contentTypeName = node.NodeType.Name;
            var nodePath = String.Concat(node.Parent.Path, "/", node.Name);

            return new InvalidOperationException(String.Format("Cannot save the content '{0}' because its ancestor not allows the type '{1}'. Ancestor: {2} ({3}). Allowed types: {4}"
                , nodePath, contentTypeName, ancestor.Path, ancestor.NodeType.Name, String.Join(", ", parent.GetAllowedChildTypeNames())));
        }
        private Exception GetNotAllowedContentTypeExceptionOnMove(Node node, GenericContent target)
        {
            var ancestor = target;
            while (ancestor.NodeType.Name == "Folder" || ancestor.NodeType.Name == "Page")
            {
                var p = ancestor.Parent as GenericContent;
                if (p == null)
                    break;
                ancestor = p;
            }

            var contentTypeName = node.NodeType.Name;

            return new InvalidOperationException(String.Format("Cannot move the content '{0}' to '{1}' because target's ancestor not allows the type '{2}'. Ancestor: {3} ({4}). Allowed types: {5}"
                , node.Path, target.Path, contentTypeName, ancestor.Path, ancestor.NodeType.Name, String.Join(", ", target.GetAllowedChildTypeNames())));
        }
        public void AllowChildType(string contentTypeName, bool setOnAncestorIfInherits = false, bool throwOnError = true, bool save = false)
        {
            AllowChildTypes(new[] { contentTypeName }, setOnAncestorIfInherits, throwOnError, save);
        }
        public void AllowChildType(ContentType contentType, bool setOnAncestorIfInherits = false, bool throwOnError = true, bool save = false)
        {
            AllowChildTypes(new[] { contentType }, setOnAncestorIfInherits, throwOnError, save);
        }
        public void AllowChildTypes(IEnumerable<string> contentTypeNames, bool setOnAncestorIfInherits = false, bool throwOnError = true, bool save = false)
        {
            AllowChildTypes(contentTypeNames.Select(n => ContentType.GetByName(n)).Where(x => x != null), setOnAncestorIfInherits, throwOnError, save);
        }
        public void AllowChildTypes(IEnumerable<ContentType> contentTypes, bool setOnAncestorIfInherits = false, bool throwOnError = true, bool save = false)
        {
            if (contentTypes == null)
                throw new ArgumentNullException("contentTypeNames");
            switch (this.NodeType.Name)
            {
                case "Folder":
                case "Page":
                    if (setOnAncestorIfInherits)
                    {
                        var parent = this.Parent as GenericContent;
                        if (parent != null)
                        {
                            parent.AllowChildTypes(contentTypes, setOnAncestorIfInherits, throwOnError, true);
                        }
                        else
                        {
                            if (throwOnError)
                                throw GetCannotAllowContentTypeException();
                        }
                    }
                    else
                    {
                        if (throwOnError)
                            throw GetCannotAllowContentTypeException();
                    }
                    return;
                case "SystemFolder":
                    if (throwOnError)
                        throw GetCannotAllowContentTypeException();
                    return;
                default:
                    SetAllowedChildTypes(contentTypes, throwOnError, save); 
                    return;
            }
        }
        private void SetAllowedChildTypes(IEnumerable<ContentType> contentTypes, bool throwOnError = true, bool save = false)
        {
            var currentTypes = this.AllowedChildTypes;
            if (currentTypes.Count() == 0)
                currentTypes = this.ContentType.AllowedChildTypes;
            var addList = contentTypes.Except(currentTypes);
            if (addList.Count() == 0)
                return;
            var list = currentTypes.Union(contentTypes).Distinct();
            this.AllowedChildTypes = list.ToArray();
            if (save)
                this.Save();
        }
        private Exception GetCannotAllowContentTypeException()
        {
            return new InvalidOperationException(String.Format("Cannot allow ContentType on a {0}. Path: {1}", this.NodeType.Name, this.Path));
        }

        //tool: checks recursive the subtree
        public string CheckChildrenTypeConsistence()
        {
            var result = new StringBuilder();
            result.AppendLine("Path\tType\tChild name\tChild type\tReason");
            foreach (var node in NodeEnumerator.GetNodes(this.Path, ExecutionHint.ForceRelationalEngine))
            {
                var parentGC = node.Parent as GenericContent;
                if (parentGC == null)
                    continue;

                var checkResult = parentGC.CheckAllowedChildType(node);
                if(checkResult != TypeAllow.Allowed)
                {
                    result.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\r\n", parentGC.Path, parentGC.NodeType.Name, node.Name, node.NodeType.Name, 
                        String.Join(", ", parentGC.GetAllowedChildTypeNames()), checkResult);
                    result.AppendLine();
                }
            }
            return result.ToString();
        }

        //=============================================================================================

        public virtual List<FieldSetting> GetAvailableFields()
        {
            return GetAvailableFields(true);
        }

        public virtual List<FieldSetting> GetAvailableFields(bool rootFields)
        {
            var availableFields = new List<FieldSetting>();

            GetAvailableContentTypeFields(availableFields, rootFields);

            return availableFields;
        }

        protected void GetAvailableContentTypeFields(ICollection<FieldSetting> availableFields)
        {
            GetAvailableContentTypeFields(availableFields, true);
        }

        protected void GetAvailableContentTypeFields(ICollection<FieldSetting> availableFields, bool rootFields)
        {
            var contentTypes = this.GetAllowedChildTypes().ToArray();

            //if there are no available content types, 
            //it means all types are allowed
            if (contentTypes.Length == 0)
                contentTypes = ContentType.GetContentTypes();

            foreach (var contentType in contentTypes)
            {
                GetFields(contentType, availableFields, rootFields);
            }
        }

        protected static void GetFields(ContentType contentType, ICollection<FieldSetting> availableFields, bool rootFields)
        {
            foreach (var fieldSetting in contentType.FieldSettings)
            {
                var fsRoot = rootFields ? FieldSetting.GetRoot(fieldSetting) : fieldSetting;

                if (!availableFields.Contains(fsRoot))
                    availableFields.Add(fsRoot);
            }
        }

        public override void MoveTo(Node target)
        {
            var targetGc = target as GenericContent;
            if (targetGc != null)
                foreach (var nt in this.GetChildTypesToAllow())
                    if (!targetGc.IsAllowedChildType(nt.Name))
                        throw new InvalidOperationException(String.Format("Cannot move {0} ({1}) to {2} ({3}) because '{4}' type is not allowed in the new position."
                            , this.Path, this.NodeType.Name, target.Path, target.NodeType.Name, nt.Name));
            base.MoveTo(target);
        }

        public override Node MakeCopy(Node target, string newName)
        {
            var copy = base.MakeCopy(target, newName);
            var version = copy.Version;
            if (version.Status != VersionStatus.Locked)
                return copy;
            copy.Version = new VersionNumber(version.Major, version.Minor, version.IsMajor ? VersionStatus.Approved : VersionStatus.Draft);
            return copy;
        }
        protected override void CopyDynamicProperties(Node target)
        {
            var content = (GenericContent)target;

            foreach (var propType in this.PropertyTypes)
            {
                if (Node.EXCLUDED_COPY_PROPERTIES.Contains(propType.Name)) continue;

                if (!propType.IsContentListProperty || target.PropertyTypes[propType.Name] != null)
                {
                    var propVal = this.GetProperty(propType.Name);
                    var binProp = propVal as BinaryData;
                    if (binProp == null)
                        content.SetProperty(propType.Name, propVal);
                    else
                        content.SetBinary(propType.Name, binProp);
                }
            }
        }

        public virtual bool IsTrashable
        {
            get
            {
                var p = Parent as GenericContent;
                if (p == null)
                    return !this.TrashDisabled;
                else
                    return !(p.TrashDisabled || this.TrashDisabled);
            }
        }

        public override int NodesInTree
        {
            get
            {
                //TODO: it would be better to use GetChildren here, but QuerySettings
                //should be extended with CountOnly handling, before we can do that.

                var escapedPath = this.Path.Replace("(", "\\(").Replace(")", "\\)");
                return ContentQuery.Query(string.Format("InTree:\"{0}\" .COUNTONLY .AUTOFILTERS:OFF .LIFESPAN:OFF", escapedPath)).Count;
            }
        }

        public override void Delete()
        {
            Delete(false);
        }

        public virtual void Delete(bool bypassTrash)
        {
            //let the TrashBin handle the delete operation:
            //only move the node to the trash or delete it permanently

            if (bypassTrash)
                TrashBin.ForceDelete(this);
            else
                TrashBin.DeleteNode(this);
        }

        public GenericContent MostRelevantContext
        {
            get { return ContentList.GetContentListForNode(this) ?? this; }
        }

        // Use this to allow access to 
        public GenericContent MostRelevantSystemContext
        {
            get { return SystemFolder.GetSystemContext(this) ?? this; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public ContentType GetContentType()
        {
            return ContentType.GetByName(NodeType.Name);
        }

        public virtual string Icon
        {
            get
            {
                return GetContentType().Icon;
            }
        }

        public virtual NodeHead GetApplication(string actionName)
        {
            if (actionName == "Browse")
            {
                var app = this.BrowseApplication;
                if (app == null)
                    return null;

                return NodeHead.Get(app.Id);
            }

            return null;
        }

        [RepositoryProperty("BrowseApplication", RepositoryDataType.Reference)]
        public Node BrowseApplication
        {
            get
            {
                return base.GetReference<Node>("BrowseApplication");
            }
            set
            {
                this.SetReference("BrowseApplication", value);
            }
        }

        //==================================================== Versioning & Approving ====================================================

        private void VersionSetup()
        {
            if (this.Id == 0)
                this.Version = SavingAction.ComputeNewVersion(this.HasApproving, this.VersioningMode);
        }

        [Obsolete("Use Save(SavingMode.KeepVersion) instead")]
        public virtual void SaveSameVersion()
        {
            var action = SavingAction.Create(this);
            action.SaveSameVersion();
            action.Execute();
        }
        public override void Save()
        {
            if (Locked)
                Save(SavingMode.KeepVersion);
            else
                Save(SavingMode.RaiseVersion);
        }
        public virtual void Save(SavingMode mode)
        {
            var action = SavingAction.Create(this);
            switch (mode)
            {
                case SavingMode.RaiseVersion:
                    action.CheckOutAndSaveAndCheckIn();
                    break;
                case SavingMode.RaiseVersionAndLock:
                    action.CheckOutAndSave();
                    break;
                case SavingMode.KeepVersion:
                    action.SaveSameVersion();
                    break;
                default:
                    throw new NotImplementedException("Unknown SavingMode: " + mode);
            }
            action.Execute();
        }
        public virtual void CheckOut()
        {
            var action = SavingAction.Create(this);
            action.CheckOut();
            action.Execute();
        }
        public virtual void CheckIn()
        {
            var action = SavingAction.Create(this);
            action.CheckIn();
            action.Execute();
        }
        public virtual void UndoCheckOut()
        {
            var action = SavingAction.Create(this);
            action.UndoCheckOut();
            action.Execute();
        }
        public virtual void Publish()
        {
            var action = SavingAction.Create(this);
            action.Publish();
            action.Execute();
        }
        public virtual void Approve()
        {
            var action = SavingAction.Create(this);
            action.Approve();
            action.Execute();
        }
        public virtual void Reject()
        {
            var action = SavingAction.Create(this);
            action.Reject();
            action.Execute();
        }

        public bool Approvable
        {
            get
            {
                return SavingAction.HasApprove(this);
            }
        }
        public bool Publishable
        {
            get
            {
                return SavingAction.HasPublish(this);
            }
        }
        public IEnumerable<Node> Versions
        {
            get { return LoadVersions(); }
        }

        //==================================================== Children

        public virtual QueryResult GetChildren(QuerySettings settings)
        {
            return GetChildren(string.Empty, settings);
        }

        public virtual QueryResult GetChildren(string text, QuerySettings settings)
        {
            return GetChildren(text, settings, false);
        }

        public virtual QueryResult GetChildren(string text, QuerySettings settings, bool getAllChildren)
        {
            if (RepositoryInstance.ContentQueryIsAllowed)
            {
                var excapedPath = this.Path.Replace("(", "\\(").Replace(")", "\\)");
                var query = ContentQuery.CreateQuery(getAllChildren
                                                         ? string.Format("InTree:\"{0}\"", excapedPath)
                                                         : string.Format("InFolder:\"{0}\"", excapedPath), settings);

                if (!string.IsNullOrEmpty(text))
                    query.AddClause(text);

                return query.Execute();
            }
            else
            {
                var nqr = NodeQuery.QueryChildren(this.Path);

                return new QueryResult(nqr.Identifiers, nqr.Count);
            }
        }

        //==================================================== IIndexable Members

        public virtual IEnumerable<IIndexableField> GetIndexableFields()
        {
            return Content.Create(this).Fields.Values.Where(f => f.IsInIndex).Cast<IIndexableField>();
            //return this.Content.Fields.Values.Where(f => f.IsInIndex).Cast<IIndexableField>();
        }

    }
}
