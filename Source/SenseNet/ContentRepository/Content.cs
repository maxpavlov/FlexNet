using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using System.Linq;
using System.IO;
using SenseNet.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using SenseNet.ContentRepository.Fields;
using System.Xml.XPath;
using System.Web.Configuration;
using SenseNet.ApplicationModel;
using SenseNet.Search;
using System.Collections;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.ContentRepository
{
    public enum FieldSerializationOptions { All, None, Custom }
    public enum ActionSerializationOptions { All, None/*, BrowseOnly*/ }
    public class SerializationOptions
    {
        class DefaultSerializationOptions : SerializationOptions
        {
            public override FieldSerializationOptions Fields { get { return FieldSerializationOptions.All; } set { } }
            public override IEnumerable<string> FieldNames { get { return null; } set { } }
            public override ActionSerializationOptions Actions { get { return ActionSerializationOptions.All; } set { } }
        }
        public virtual FieldSerializationOptions Fields { get; set; }
        public virtual IEnumerable<string> FieldNames { get; set; }
        public virtual ActionSerializationOptions Actions { get; set; }

        public static readonly SerializationOptions _default = new DefaultSerializationOptions();
        public static SerializationOptions Default { get { return _default; } }
    }
    public interface IActionLinkResolver
    {
        string ResolveRelative(string targetPath);
        string ResolveRelative(string targetPath, string actionName);
    }
    //internal class DefaultActionLinkResolver : IActionLinkResolver
    //{
    //    public static DefaultActionLinkResolver Instance = new DefaultActionLinkResolver();

    //    public string ResolveRelative(string targetPath)
    //    {
    //        return ResolveRelative(targetPath, null);
    //    }
    //    public string ResolveRelative(string targetPath, string actionName)
    //    {
    //        return string.Empty;
    //    }
    //}
	/// <summary>
	/// <c>Content</c> class is responsible for the general management of different types of <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>s.
	/// </summary>
	/// <remarks>
	/// Through this class you can generally load, create, validate and save any kind 
	/// of <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandlers</see>.
	/// 
	/// The type of a Content is defined by the <see cref="SenseNet.ContentRepository.Schema.ContentType">ContentType</see>, 
	/// which represents the ContentTypeDefinition. The most important component of a <c>Content</c> 
	/// is <see cref="SenseNet.ContentRepository.Field">Field</see> list, which is defined also in ContentTypeDefinition. 
	/// 
	/// Basically a <c>Content</c> is a wrapper of the <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> assigned 
	/// when the <c>Content</c> itself was created, and the <see cref="SenseNet.ContentRepository.Field">Fields</see> are managing 
	/// the properties of the ContentHandler.
	/// </remarks>
	/// <example>
	/// 
	/// The following code shows a method that handles a <c>Content</c> without user interface:
	/// <code>
	/// //sets the Index field of an identified Content if possible
	/// public void SetIndex(int id, int expectedIndex)
	/// {
	///     // load the content
	///     var content = Content.Load(id);
	///
	///     // check the existence
	///     if(content == null)
	///          return;
	///
	///     int originalIndex = (int)content["Index"];
	///
	///     // avoid the unnecessary validation and saving
	///     if(originalIndex == expectedIndex)
	///         return;
	///
	///     // set the field
	///     content["Index"] = expectedIndex;
	///     
	///     // check the validity
	///     if (content.IsValid)
	///     {
	///         //TODO: exception handling if needed
	///         content.Save();
	///     }
	///     else
	///     {
	///         //TODO: excepton throwing if index is invalid by current FieldSetting
	///     }
	/// }
	/// </code>
	/// </example>
    public class Content : FeedContent, ICustomTypeDescriptor
	{
        private static readonly string XsltRenderingWithContentSerializationKey = "XsltRenderingWithContentSerialization";
        private static bool? _contentNavigatorEnabled;
        public static bool ContentNavigatorEnabled
        {
            get
            {
                if (_contentNavigatorEnabled == null)
                    _contentNavigatorEnabled = System.Configuration.ConfigurationManager.AppSettings[XsltRenderingWithContentSerializationKey] != "true";
                return _contentNavigatorEnabled.Value;
            }
        }

		//========================================================================= Fields

        private ContentType _contentType;

		private Node _contentHandler;
		private IDictionary<string, Field> _fields;
		private bool _isValidated;
		private bool _isValid;

		//========================================================================= Properties

		/// <summary>
		/// Gets the <see cref="SenseNet.ContentRepository.Schema.ContentType">ContentType</see> of the instance.
		/// </summary>
        public ContentType ContentType
        {
            get { return _contentType; }
        }
		/// <summary>
		/// Gets the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> of the instance.
		/// </summary>
		public Node ContentHandler
		{
			get { return _contentHandler; }
		}
		public SenseNet.ContentRepository.Storage.Security.SecurityHandler Security
		{
			get { return _contentHandler.Security; }
		}
		/// <summary>
		/// Gets the field <see cref="System.Collections.Generic.Dictionary(string, Field)">Dictionary</see> of the instance.
		/// </summary>
		public IDictionary<string, Field> Fields
		{
			get { return _fields; }
		}
		/// <summary>
		/// Gets the friendly name of the Content or If it is null or empty, the value comes from the ContentTypeDefinition.
		/// </summary>
		public string DisplayName
		{
            get
            {
                //var displayName = string.Empty;
                //var gc = _contentHandler as GenericContent;
                //if (gc != null)
                //    displayName = gc.DisplayName;

                //if (!string.IsNullOrEmpty(displayName))
                //    return displayName;

                //return (_contentHandler.GetPropertySafely("DisplayName") as string) ?? _contentType.DisplayName;

                return String.IsNullOrEmpty(_contentHandler.DisplayName) ? _contentType.DisplayName : _contentHandler.DisplayName;
            }
            set
            {
                _contentHandler.DisplayName = value;
            }
		}
		/// <summary>
		/// Gets the Description of Content. This value comes from the ContentTypeDefinition.
		/// </summary>
		public string Description
		{
            get { return (_contentHandler.GetPropertySafely("Description") as string) ?? _contentType.Description; }
		}
		public string Icon
		{
			get
            {
                var genericHandler = ContentHandler as GenericContent;
                if (genericHandler != null)
                    return genericHandler.Icon;

                var ctypeHandler = ContentHandler as ContentType;
                if (ctypeHandler != null)
                    return ctypeHandler.Icon;

                return _contentType.Icon;
            }
		}
		/// <summary>
		/// Indicates the validity of the content. It is <c>true</c> if all contained fields are valid; otherwise, <c>false</c>.
		/// </summary>
		public bool IsValid
		{
			get
			{
				if (!_isValidated)
					Validate();
				return _isValid;
			}
		}

	    public bool IsNew
	    {
	        get
	        {
	            var isdt = this.ContentHandler as ISupportsDynamicFields;

                //IsNew on Node: Id == 0
                //It is overridden in RuntimeContentHandler
                return isdt != null ? isdt.IsNewContent : this.ContentHandler.IsNew;
	        }
	    }


        public QuerySettings ChildrenQuerySettings 
        { 
            get; 
            set; 
        }


        private QueryResult _queryResult;
        private IEnumerable<Content> _children;

        public string ChildrenQueryFilter { get; set; }

	    private bool _allChildren = false;
        public bool AllChildren
        {
            get { return _allChildren; }
            set { _allChildren = value; }
        }

        private void EnsureQueryResult()
        {
            if (_queryResult == null)
            {
                var folder = ContentHandler as GenericContent;
                if (folder != null)
                {
                    if (ChildrenQuerySettings == null && string.IsNullOrEmpty(ChildrenQueryFilter))
                    {
                        //TODO: porting queystion
                        //can it not be GetChildren(null, null)
                        _queryResult = folder.GetChildren(null);
                    }
                    else
                    {
                        _queryResult = folder.GetChildren(ChildrenQueryFilter, ChildrenQuerySettings, _allChildren);
                    }

                    return;
                }

                var contentType = ContentHandler as ContentType;
                if (contentType != null)
                {
                    if (ChildrenQuerySettings == null && string.IsNullOrEmpty(ChildrenQueryFilter))
                    {
                        //TODO: porting queystion
                        //can it not be GetChildren(null, null)
                        _queryResult = contentType.GetChildren(null);
                    }
                    else
                    {
                        _queryResult = contentType.GetChildren(ChildrenQueryFilter, ChildrenQuerySettings);
                    }

                    return;
                }
            }
        }

        public virtual int ChildCount
        {
            get
            {
                EnsureQueryResult();
                return _queryResult == null ? 0 : _queryResult.Count;
            }
        }
        public virtual IEnumerable<Content> Children
        {
            get
            {
                if (_children == null)
                {
                    EnsureQueryResult();

                    if (_queryResult != null)
                    {
                        //var nodes =
                        //    ((ChildrenQuerySettings == null) || (ChildrenQuerySettings.Top == 0))
                        //        ? _queryResult.Nodes
                        //        : _queryResult.CurrentPage;

                        var nodes = _queryResult.Nodes;
                        _children = nodes.Select(n => Content.Create(n)).ToArray();
                    }
                }

                return _children;
            }
        }

        public IEnumerable<Node> Versions
        {
            get { return ContentHandler.LoadVersions(); }
        }

        public PropertyDescriptorCollection PropertyDescriptors { get; set; }

		//------------------------------------------------------------------------- Shortcuts

		/// <summary>
		/// Gets the Id of the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		/// </summary>
		public int Id
		{
			get { return _contentHandler.Id; }
		}
		/// <summary>
		/// Gets the path of the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		/// </summary>
		public string Path
		{
			get { return _contentHandler.Path; }
		}
		/// <summary>
		/// Gets the name of the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		/// </summary>
		public string Name
		{
			get { return _contentHandler.Name; }
		}

        public bool IsContentList
        {
            get { return this.ContentHandler.ContentListType != null && this.ContentHandler.ContentListId == 0; }
        }
        public bool IsContentListItem
        {
            get { return this.ContentHandler.ContentListId != 0; }
        }
        public bool IsLastPublicVersion { get { return ContentHandler.IsLastPublicVersion; } }
        public bool IsLatestVersion { get { return ContentHandler.IsLatestVersion; } }

		/// <summary>
		/// Gets or sets the value of an indexed <see cref="SenseNet.ContentRepository.Field">Field</see>. 
		/// Type of return value is determined by derived <see cref="SenseNet.ContentRepository.Field">Field</see>.
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns>An <see cref="System.Object">object</see> that represents the <see cref="SenseNet.ContentRepository.Field">Field</see>'s value.</returns>
		public object this[string fieldName]
		{
			get { return _fields[fieldName].GetData(); }
			set { _fields[fieldName].SetData(value); }
		}

	    public string WorkspaceName
	    {
	        get 
            {
                var gc = this.ContentHandler as GenericContent;
                return gc == null ? string.Empty : gc.WorkspaceName;
            }
	    }

        public string WorkspaceTitle
        {
            get
            {
                var gc = this.ContentHandler as GenericContent;
                return gc == null ? string.Empty : gc.WorkspaceTitle;
            }
        }

        public string WorkspacePath
	    {
	        get 
            {
                var gc = this.ContentHandler as GenericContent;
                return gc == null ? string.Empty : gc.WorkspacePath;
            }
	    }

        public CheckInCommentsMode CheckInCommentsMode
        {
            get
            {
                var gc = this.ContentHandler as GenericContent;
                return gc == null ? CheckInCommentsMode.None : gc.CheckInCommentsMode;
            }
        }

		//========================================================================= Construction

		private Content(Node contentHandler, ContentType contentType)
		{
            InitializeInstance(contentHandler, contentType);
		}

        protected virtual void InitializeInstance(Node contentHandler, ContentType contentType)
        {
            _contentHandler = contentHandler;
            _contentType = contentType;
            _fields = new Dictionary<string, Field>();

            Content targetContent = null;
            var cLink = contentHandler as ContentLink;
            if (cLink != null)
                targetContent = cLink.LinkedContent == null ? null : Content.Create(cLink.LinkedContent);

            if (targetContent != null)
                InitializeFieldsWithContentLink(contentType, targetContent);
            else
                InitializeFields(contentType);

            if (_contentType == null)
                throw new ArgumentNullException("contentType");
            if (_contentType.Name == null)
                throw new InvalidOperationException("ContentType name is null");

            //field collection of the temporary fieldsetting content
            //or journal node must not contain the ContentList fields
            if (contentHandler is FieldSettingContent || _contentType.Name.CompareTo("JournalNode") == 0)
                return;

            ContentList list;

            try
            {
                list = contentHandler.LoadContentList() as ContentList;
                if (list == null)
                    return;
            }
            catch (Exception ex)
            {
                //handle errors that occur during heavy load
                if (contentHandler == null)
                    throw new ArgumentNullException("Content handler became null.", ex);
                
                throw new InvalidOperationException("Error during content list load.", ex);
            }

            try
            {
                foreach (var fieldSetting in list.FieldSettings)
                {
                    var field = Field.Create(this, fieldSetting);
                    _fields.Add(field.Name, field);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error during content list field creation.", ex);
            }
        }
        private void InitializeFields(ContentType contentType)
        {
            foreach (var fieldSetting in contentType.FieldSettings)
            {
                var field = Field.Create(this, fieldSetting);
                _fields.Add(field.Name, field);
            }
        }
        private static readonly List<string> NotLinkedFields = new List<string>(new[] { "Id", "ParentId", "VersionId", "Name", "Path", "Index", "InTree", "InFolder", "Depth", "Type", "TypeIs" });
        private void InitializeFieldsWithContentLink(ContentType contentType, Content linkedContent)
        {
            var linkedType = linkedContent.ContentType;
            foreach (var fieldSetting in linkedType.FieldSettings)
            {
                if (NotLinkedFields.Contains(fieldSetting.Name))
                    continue;
                var field = Field.Create(linkedContent, fieldSetting);
                field.IsLinked = true;
                _fields.Add(field.Name, field);
            }
            foreach (var fieldSetting in contentType.FieldSettings)
            {
                if (_fields.ContainsKey(fieldSetting.Name))
                    continue;
                var field = Field.Create(this, fieldSetting);
                _fields.Add(field.Name, field);
            }
        }



		/// <summary>
		/// Loads the appropiate <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> by the given ID and wraps to a <c>Content</c>.
		/// </summary>
		/// <returns>The latest version of the <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> that has the given ID wrapped by a <c>Content</c> instance.</returns>
		public static Content Load(int id)
		{
			Node node = Node.LoadNode(id);
			if (node == null)
				return null;
			return Create(node);
		}
		/// <summary>
		/// Loads the appropiate <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> by the given Path and wraps to a <c>Content</c>.
		/// </summary>
		/// <returns>The latest version of the <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> that has the given Path wrapped by a <c>Content</c> instance.</returns>
		public static Content Load(string path)
		{
			Node node = Node.LoadNode(path);
			if (node == null)
				return null;
			return Create(node);
		}
		/// <summary>
		/// Loads the appropiate <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> by the given ID and version number and wraps to a <c>Content</c>.
		/// </summary>
		/// <returns>The given version of the <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> that has the given ID wrapped by a <c>Content</c>.</returns>
		public static Content Load(int id, VersionNumber version)
		{
			Node node = Node.LoadNode(id, version);
			if (node == null)
				return null;
			return Create(node);
		}
		/// <summary>
		/// Loads the appropiate <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> by the given Path and version number and wraps to a <c>Content</c>.
		/// </summary>
		/// <returns>The given version of the <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> that has the given Path wrapped by a <c>Content</c>.</returns>
		public static Content Load(string path, VersionNumber version)
		{
			Node node = Node.LoadNode(path, version);
			if (node == null)
				return null;
			return Create(node);
		}
		/// <summary>
		/// Executes the given <see cref="SenseNet.ContentRepository.Storage.Search.NodeQuery">NodeQuery</see> and wraps each <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> to a <c>Content</c>.
		/// </summary>
		/// <returns>A Content list that is contain the result <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>s wrapped by a <c>Content</c>.</returns>
        public static IEnumerable<Content> Query(NodeQuery query)
        {
            List<Content> result = new List<Content>();
            foreach (Node node in query.Execute().Nodes)
                result.Add(Create(node));
            return result;
        }

        public static IEnumerable<Content> Query(NodeQuery query, IEnumerable<string> fieldNames)
        {
            var result = Query(query);
            var propDescColl = GetPropertyDescriptors(fieldNames);

            if (propDescColl != null && propDescColl.Count > 0)
            {
                foreach (var content in result)
                {
                    content.PropertyDescriptors = propDescColl;
                }
            }

            return result;
        }
        [Obsolete("Use Content.Create instead")]
        public Content()
        {

        }
		/// <summary>
		/// Creates a <c>Content</c> instance from an instantiated <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		/// </summary>
		/// <returns>Passed <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> wrapped by a <c>Content</c>.</returns>
        public static Content Create(Node contentHandler)
        {
            //if (contentHandler == null)
            //    throw new ArgumentNullException("contentHandler");
            //ContentType contentType = ContentTypeManager.Current.GetContentTypeByHandler(contentHandler);
            //if (contentType == null)
            //{
            //    if (contentHandler.Name == ContentType.NodeTypeName)
            //        contentType = contentHandler as ContentType;
            //    if (contentType == null) 
            //        throw new ApplicationException(String.Concat(SR.Exceptions.Content.Msg_UnknownContentType, ": ", contentHandler.NodeType.Name));
            //}
            //var extendedHandler = contentHandler as ISupportsDynamicFields;
            //if (extendedHandler != null)
            //    contentType = ExtendContentType(extendedHandler, contentType);
            //return new Content(contentHandler, contentType);
            return Create<Content>(contentHandler);
        }

        public static Content Create<T>(Node contentHandler) where T: Content, new()
        {
            if (contentHandler == null)
                throw new ArgumentNullException("contentHandler");
            ContentType contentType = ContentTypeManager.Current.GetContentTypeByHandler(contentHandler);
            if (contentType == null)
            {
                var rtch = contentHandler as RuntimeContentHandler;
                if (rtch != null)
                    return new Content(rtch, rtch.ContentType);
                if (contentHandler.Name == typeof(ContentType).Name)
                    contentType = contentHandler as ContentType;
                if (contentType == null)
                    throw new ApplicationException(String.Concat(SR.Exceptions.Content.Msg_UnknownContentType, ": ", contentHandler.NodeType.Name));
            }
            var extendedHandler = contentHandler as ISupportsDynamicFields;
            if (extendedHandler != null)
                contentType = ExtendContentType(extendedHandler, contentType);
            var result = new T();
            result.InitializeInstance(contentHandler, contentType);
            return result;
        }

        private static ContentType ExtendContentType(ISupportsDynamicFields extendedHandler, ContentType contentType)
        {
            var extendedContentType = ContentType.Create(extendedHandler, contentType);
            if (extendedContentType == null)
                throw new ApplicationException("Cannot create content from a " + extendedHandler.GetType().FullName);
            return extendedContentType;
        }
		/// <summary>
		/// Creates an appropriate new <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> instance by the given parameters ant wraps to a <c>Content</c>.
		/// This method calls the appropriate constructor determined by the passed arguments but do not saves the new <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		/// </summary>
		/// <param name="contentTypeName">Determines the type of <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>. 
		/// Fully qualified type name is contained by the <see cref="SenseNet.ContentRepository.Schema.ContentType">ContentType</see> named this parameter's value.
		/// </param>
		/// <param name="parent"><see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> instance as a parent in the big tree.</param>
		/// <param name="name">The expected name of the new <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.</param>
		/// <param name="args">Additional parameters required by the expected constructor of <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		/// In this version neither argument can be null.</param>
		/// <returns>An instantiated <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> wrapped by a <c>Content</c>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when any custom argument is null.</exception>
        public static Content CreateNew(string contentTypeName, Node parent, string nameBase, params object[] args)
        {
            using (var traceOperation = Logger.TraceOperation("Content.CreateNew"))
            {
                if (args == null)
                    args = new object[0];

                ContentType contentType = ContentTypeManager.Current.GetContentTypeByName(contentTypeName);
                if (contentType == null)
                    throw new ApplicationException(String.Concat(SR.Exceptions.Content.Msg_UnknownContentType, ": ", contentTypeName));
                Type type = TypeHandler.GetType(contentType.HandlerName);

                Type[] signature = new Type[args.Length + 2];
                signature[0] = typeof(Node);
                signature[1] = typeof(string);
                object[] arguments = new object[signature.Length];
                arguments[0] = parent;
                arguments[1] = contentTypeName;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == null)
                        throw new ArgumentOutOfRangeException("args", SR.Exceptions.Content.Msg_CannotCreateNewContentWithNullArgument);
                    signature[i + 2] = args[i].GetType();
                    arguments[i + 2] = args[i];
                }

                var ctorInfo = type.GetConstructor(signature);
                Node node = null;
                var nodeCreateRetryCount = 0;
                Exception nodeCreateException = null;

                while (true)
                {
                    try
                    {
                        node = (Node) ctorInfo.Invoke(arguments);
                        
                        //log previous exception if exists
                        if (nodeCreateException != null)
                            Logger.WriteWarning("Error during node creation: " + Tools.CollectExceptionMessages(nodeCreateException));

                        break;
                    }
                    catch (Exception ex)
                    {
                        //store the exception for later use
                        nodeCreateException = ex;

                        //retry a few times to handle errors that occur during heavy load
                        nodeCreateRetryCount++;

                        if (nodeCreateRetryCount > 2)
                            throw new Exception(string.Format("Node creation failed. ContentType name: {0}, Parent: {1}, Name: {2}", 
                                contentTypeName ?? string.Empty, parent == null ? "NULL" : parent.Path, nameBase ?? string.Empty), ex);

                        Thread.Sleep(10);
                    }
                }

                var name = ContentNamingHelper.GetNewName(nameBase, contentType, parent);

                if (!string.IsNullOrEmpty(name))
                    node.Name = name;

                traceOperation.IsSuccessful = true;

                //try to re-use the already created content in GenericContent
                var gc = node as GenericContent;

                return gc == null ? Content.Create(node) : gc.Content;
            }
        }

        public static Content CreateNewAndParse(string contentTypeName, Node parent, string name, Dictionary<string, string> fieldData)
        {
            using (var traceOperation = Logger.TraceOperation("Content.CreateNewAndParse"))
            {
                var content = CreateNew(contentTypeName, parent, name);
                Modify(content, fieldData);

                traceOperation.IsSuccessful = true;
                return content;
            }
        }

        [Obsolete("Use ContentTemplate.CreateTemplatedAndParse instead of this method.", true)]
        public static Content CreateTemplated(Node parent, Node template, string nameBase)
        {
            return ContentTemplate.CreateTemplated(parent, template, nameBase);
        }

        [Obsolete("Use ContentTemplate.CreateTemplatedAndParse instead of this method.", true)]
        public static Content CreateTemplatedAndParse(Node parent, Node template, string nameBase, Dictionary<string, string> fieldData)
        {
            return ContentTemplate.CreateTemplatedAndParse(parent, template, nameBase, fieldData);
        }

		//========================================================================= Methods
        public static void Modify(Content content, Dictionary<string, string> fieldData)
        {

            using (var traceOperation = Logger.TraceOperation("Content.Modify"))
            {
                var ok = true;
                foreach (var fieldName in fieldData.Keys)
                {
                    if (!content.Fields.ContainsKey(fieldName))
                        throw new ApplicationException("Unknown field: " + fieldName);
                    ok &= content.Fields[fieldName].Parse(fieldData[fieldName]);
                }
                if (!ok)
                {
                    content._isValidated = true;
                    content._isValid = false;
                }
                else
                {
                    content.Validate();
                }
                traceOperation.IsSuccessful = true;
            }
        }

		/// <summary>
		/// Returns an array contains all existing <see cref="SenseNet.ContentRepository.Schema.ContentType">ContentType</see> name.
		/// </summary>
        public static string[] GetContentTypeNames()
        {
            Dictionary<string, ContentType> contentTypes = ContentTypeManager.Current.ContentTypes;
            string[] names = new string[contentTypes.Count];
            contentTypes.Keys.CopyTo(names,0);
            return names;
        }

        public Content GetContentList()
        {
            if (!this.IsContentList)
                return null;
            var listNode = this.ContentHandler.LoadContentList();
            return Content.Create(listNode);
        }

		/// <summary>
		/// Valiadates each contained <see cref="SenseNet.ContentRepository.Field">Field</see>s and returns <c>true</c> if all fields are valid.
		/// </summary>
		/// <returns>It is <c>true</c> if all contained <see cref="SenseNet.ContentRepository.Field">Field</see>s are valid; otherwise, <c>false</c>.</returns>
        public bool Validate()
        {
            if (_isValidated)
                return _isValid;

            using (var traceOperation = Logger.TraceOperation("Content.Validate"))
            {
                _isValid = true;
                foreach (var item in _fields)
                {
                    ////-- force set default value (moved to Field.SetDefaultValue())
                    //var field = _fields[key];
                    //if (!field.ReadOnly && (field.FieldSetting.Compulsory ?? false) && !field.HasValue() && (field.FieldSetting.DefaultValue != null) && !field.IsChanged)
                    //    field.Parse(field.FieldSetting.DefaultValue);
                    //_isValid = _isValid & field.Validate();
                    _isValid = _isValid & item.Value.Validate();
                }
                _isValidated = true;
                traceOperation.IsSuccessful = true;
            }
            return _isValid;
        }
		internal void FieldChanged()
		{
			_isValidated = false;
		}
		private void SaveFields()
		{
			SaveFields(true);
		}
        private void SaveFields(bool validOnly)
        {
            using (var traceOperation = Logger.TraceOperation("Content.SaveFields"))
            {
                _isValid = true;
                foreach (string key in _fields.Keys)
                {
                    Field field = _fields[key];
                    field.Save(validOnly);
                    _isValid = _isValid && field.IsValid;
                }
                _isValidated = true;
                traceOperation.IsSuccessful = true;
            }
        }

		/// <summary>
		/// Validates and saves the wrapped <c>ContentHandler</c> into the Sense/Net Content Repository with considering the versioning settings.
		/// </summary>
		/// <remarks>
		/// This method executes followings:
		/// <list type="bullet">
		///     <item>
		///         Saves all <see cref="SenseNet.ContentRepository.Field">Field</see>s into the properties 
		///         of wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		///     </item>
		///     <item>
		///         If <c>Content</c> is not valid 
		///         throws an <see cref="SenseNet.ContentRepository.InvalidContentException">InvalidContentException</see>.
		///     </item>
		///     <item>
		///         Saves the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> into the Sense/Net Content Repository.
		///     </item>
		/// </list>
		/// 
		/// If the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> inherited from 
		/// the <see cref="SenseNet.ContentRepository.GenericContent">GenericContent</see> after the saving
		/// its version is depends its <see cref="SenseNet.ContentRepository.GenericContent.VersioningMode">VersioningMode</see> setting.
		/// </remarks>
		/// <exception cref="InvalidContentException">Thrown when <c>Content</c> is invalid.</exception>
		public void Save()
		{
			Save(true);
		}

        public void Save(bool validOnly)
        {
            if (_contentHandler.Locked)
                Save(validOnly, SavingMode.KeepVersion);
            else
                Save(validOnly, SavingMode.RaiseVersion);
        }

        public void Save(SavingMode mode)
        {
            Save(true, mode);
        }

	    /// <summary>
		/// Saves the wrapped <c>ContentHandler</c> into the Sense/Net Content Repository with considering the versioning settings.
		/// </summary>
		/// <remarks>
		/// This method executes followings:
		/// <list type="bullet">
		///     <item>
		///         Saves all <see cref="SenseNet.ContentRepository.Field">Field</see>s into the properties 
		///         of wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		///     </item>
		///     <item>
		///         If passed <paramref name="validOnly">validOnly</paramref> parameter is true  and <c>Content</c> is not valid 
		///         throws an <see cref="SenseNet.ContentRepository.InvalidContentException">InvalidContentException</see>
		///     </item>
		///     <item>
		///         Saves the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> into the Sense/Net Content Repository.
		///     </item>
		/// </list>
		/// 
		/// If the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> inherited from 
		/// the <see cref="SenseNet.ContentRepository.GenericContent">GenericContent</see> after the saving
		/// its version is depends its <see cref="SenseNet.ContentRepository.GenericContent.VersioningMode">VersioningMode</see> setting.
		/// </remarks>
		/// <exception cref="InvalidContentException">Thrown when <paramref name="validOnly"> is true  and<c>Content</c> is invalid.</exception>
        public void Save(bool validOnly, SavingMode mode)
        {
            using (var traceOperation = Logger.TraceOperation("Content.Save"))
            {
                SaveFields(validOnly);
                if (validOnly && !IsValid)
                    throw InvalidContentExceptionHelper();

                var genericContent = _contentHandler as GenericContent;
                if (genericContent != null)
                    genericContent.Save(mode);
                else
                    _contentHandler.Save();

                foreach (string key in _fields.Keys)
                    _fields[key].OnSaveCompleted();

                var template = _contentHandler.Template;
                if (template != null) {
                    ContentTemplate.CopyContents(this);
                }

                traceOperation.IsSuccessful = true;
            }
        }

		/// <summary>
		/// Validates and saves the wrapped <c>ContentHandler</c> into the Sense/Net Content Repository without considering the versioning settings.
		/// </summary>
		/// <remarks>
		/// This method executes followings:
		/// <list type="bullet">
		///     <item>
		///         Saves all <see cref="SenseNet.ContentRepository.Field">Field</see>s into the properties 
		///         of wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		///     </item>
		///     <item>
		///         If <c>Content</c> is not valid throws an <see cref="SenseNet.ContentRepository.InvalidContentException">InvalidContentException</see>.
		///     </item>
		///     <item>
		///         Saves the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> into the Sense/Net Content Repository.
		///     </item>
		/// </list>
		/// 
		/// After the saving the version of wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> will not changed.
		/// </remarks>
		/// <exception cref="InvalidContentException">Thrown when <c>Content</c> is invalid.</exception>
		public void SaveSameVersion()
		{
            SaveSameVersion(true);
		}
        /// <summary>
        /// Validates and saves the wrapped <c>ContentHandler</c> into the Sense/Net Content Repository without considering the versioning settings.
        /// </summary>
        /// <remarks>
        /// This method executes followings:
        /// <list type="bullet">
        ///     <item>
        ///         Saves all <see cref="SenseNet.ContentRepository.Field">Field</see>s into the properties 
        ///         of wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
        ///     </item>
        ///     <item>
        ///         If passed <paramref name="validOnly">validOnly</paramref> parameter is true  and <c>Content</c> is not valid 
        ///         throws an <see cref="SenseNet.ContentRepository.InvalidContentException">InvalidContentException</see>
        ///     </item>
        ///     <item>
        ///         Saves the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> into the Sense/Net Content Repository.
        ///     </item>
        /// </list>
        /// 
        /// After the saving the version of wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> will not changed.
        /// </remarks>
        /// <exception cref="InvalidContentException">Thrown when <paramref name="validOnly"> is true  and<c>Content</c> is invalid.</exception>
        public void SaveSameVersion(bool validOnly)
        {
            using (var traceOperation = Logger.TraceOperation("Content.SaveSameVersion"))
            {
                SaveFields(validOnly);
                if (validOnly && !IsValid)
                    throw InvalidContentExceptionHelper();
                GenericContent genericContent = _contentHandler as GenericContent;
                if (genericContent == null)
                    _contentHandler.Save();
                else
                    genericContent.Save(SavingMode.KeepVersion);

                var template = _contentHandler.Template;
                if (template != null) {
                    ContentTemplate.CopyContents(this);
                }

                traceOperation.IsSuccessful = true;
            }
        }
        /// <summary>
		/// Validates and publishes the wrapped <c>ContentHandler</c> if it is a <c>GenericContent</c> otherwise saves it normally.
		/// </summary>
		/// <remarks>
		/// This method executes followings:
		/// <list type="bullet">
		///     <item>
		///         Saves all <see cref="SenseNet.ContentRepository.Field">Field</see>s into the properties 
		///         of wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		///     </item>
		///     <item>
		///         If <c>Content</c> is not valid throws an <see cref="SenseNet.ContentRepository.InvalidContentException">InvalidContentException</see>.
		///     </item>
		///     <item>
		///         If the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> inherited from 
		///         the <see cref="SenseNet.ContentRepository.GenericContent">GenericContent</see> calls its
		///         <see cref="SenseNet.ContentRepository.GenericContent.Publish">Publish</see> method otherwise saves it normally.
		///     </item>
		/// </list>
		/// </remarks>
		/// <exception cref="InvalidContentException">Thrown when <c>Content</c> is invalid.</exception>
        public void Publish()
        {
            using (var traceOperation = Logger.TraceOperation("Content.Publish"))
            {
                SaveFields();

                var genericContent = _contentHandler as GenericContent;
                if (genericContent == null)
                    _contentHandler.Save();
                else
                    genericContent.Publish();
                traceOperation.IsSuccessful = true;
            }
        }
        public void Approve()
        {
            using (var traceOperation = Logger.TraceOperation("Content.Approve"))
            {
                SaveFields();

                var genericContent = _contentHandler as GenericContent;
                if (genericContent == null)
                    _contentHandler.Save();
                else
                    genericContent.Approve();
                traceOperation.IsSuccessful = true;
            }
        }
        public void Reject()
        {
            using (var traceOperation = Logger.TraceOperation("Content.Reject"))
            {
                SaveFields();

                var genericContent = _contentHandler as GenericContent;
                if (genericContent == null)
                    _contentHandler.Save();
                else
                    genericContent.Reject();
                traceOperation.IsSuccessful = true;
            }
        }
		/// <summary>
		/// Saves all <see cref="SenseNet.ContentRepository.Field">Field</see>s into the properties of wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		/// 
		/// If <c>Content</c> is not valid throws an <see cref="SenseNet.ContentRepository.InvalidContentException">InvalidContentException</see>.
		/// 
		/// If the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> inherited from 
		/// the <see cref="SenseNet.ContentRepository.GenericContent">GenericContent</see> calls its
		/// <see cref="SenseNet.ContentRepository.GenericContent.CheckIn">CheckIn</see> method otherwise calls the
		/// <see cref="SenseNet.ContentRepository.Storage.Node.Lock.Unlock">Unlock</see> method with
		/// <c><see cref="SenseNet.ContentRepository.Storage.VersionStatus">VersionStatus</see>.Public</c> and 
		/// <c><see cref="SenseNet.ContentRepository.Storage.VersionRaising">VersionRaising</see>.None</c> parameters.
		/// 
		/// </summary>
		/// <remarks>
		/// This method executes followings:
		/// <list type="bullet">
		///     <item>
		///         Saves all <see cref="SenseNet.ContentRepository.Field">Field</see>s into the properties 
		///         of wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>.
		///     </item>
		///     <item>
		///         If <c>Content</c> is not valid throws an <see cref="SenseNet.ContentRepository.InvalidContentException">InvalidContentException</see>.
		///     </item>
		///     <item>
		/// 		If the wrapped <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> inherited from 
		/// 		the <see cref="SenseNet.ContentRepository.GenericContent">GenericContent</see> calls its
		/// 		<see cref="SenseNet.ContentRepository.GenericContent.CheckIn">CheckIn</see> method otherwise calls the
		/// 		<see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see>'s
		/// 		<see cref="SenseNet.ContentRepository.Storage.Security.LockHandler.Unlock(VersionStatus, VersionRaising)">Lock.Unlock</see> method with
		/// 		<c><see cref="SenseNet.ContentRepository.Storage.VersionStatus">VersionStatus</see>.Public</c> and 
		/// 		<c><see cref="SenseNet.ContentRepository.Storage.VersionRaising">VersionRaising</see>.None</c> parameters.
		///     </item>
		/// </list>
		/// </remarks>
		/// <exception cref="InvalidContentException">Thrown when <c>Content</c> is invalid.</exception>
        public void CheckIn()
        {
            using (var traceOperation = Logger.TraceOperation("Content.CheckIn"))
            {
                SaveFields();

                var genericContent = _contentHandler as GenericContent;
                if (genericContent == null)
                    _contentHandler.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
                else
                    genericContent.CheckIn();
                traceOperation.IsSuccessful = true;
            }
        }

        public void CheckOut()
        {
            using (var traceOperation = Logger.TraceOperation("Content.CheckOut"))
            {
                SaveFields();

                var genericContent = _contentHandler as GenericContent;
                if (genericContent == null)
                    _contentHandler.Lock.Lock();
                else
                    genericContent.CheckOut();
                traceOperation.IsSuccessful = true;
            }
        }

        public void UndoCheckOut()
        {
            using (var traceOperation = Logger.TraceOperation("Content.UndoCheckOut"))
            {
                SaveFields();

                var genericContent = _contentHandler as GenericContent;
                if (genericContent == null)
                    _contentHandler.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
                else
                    genericContent.UndoCheckOut();
                traceOperation.IsSuccessful = true;
            }
        }

        public void ForceUndoCheckOut()
        {
            using (var traceOperation = Logger.TraceOperation("Content.ForceUndoCheckOut"))
            {
                if (!SavingAction.HasForceUndoCheckOutRight(this.ContentHandler))
                    throw new Storage.Security.SenseNetSecurityException(this.Path, Storage.Schema.PermissionType.ForceCheckin);

                SaveFields();

                var genericContent = _contentHandler as GenericContent;
                if (genericContent == null)
                    _contentHandler.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
                else
                    genericContent.UndoCheckOut();
                traceOperation.IsSuccessful = true;
            }
        }

		public void DontSave()
		{
			//:)
		}

	    public bool Approvable
	    {
	        get
	        {
	            var genericContent = _contentHandler as GenericContent;

	            return genericContent != null && genericContent.Approvable;
	        }
	    }

        public bool Publishable
        {
            get
            {
                var genericContent = _contentHandler as GenericContent;

                return genericContent != null && genericContent.Publishable;
            }
        }

		/// <summary>
		/// Deletes the Node and all of its contents from the database. This operation removes all child nodes too.
		/// </summary>
		/// <param name="contentId">Identifier of the Node that will be deleted.</param>
		public static void DeletePhysical(int contentId)
		{
			Node.DeletePhysical(contentId);
		}
		/// <summary>
		/// Deletes the Node and all of its contents from the database. This operation removes all child nodes too.
		/// </summary>
		/// <param name="path">The path of the Node that will be deleted.</param>
		public static void DeletePhysical(string path)
		{
			Node.DeletePhysical(path);
		}
		/// <summary>
		/// Deletes the represented <see cref="SenseNet.ContentRepository.Storage.Node">Node</see> and all of its contents from the database. This operation removes all child nodes too.
		/// </summary>
		public void DeletePhysical()
		{
			this.ContentHandler.Delete();
		}

        public static void Delete(int contentId)
        {
            Node.Delete(contentId);
        }

        public static void Delete(string path)
        {
            Node.Delete(path);
        }

        public void Delete()
        {
            this.ContentHandler.Delete();
        }

        public void ForceDelete()
        {
            this.ContentHandler.ForceDelete();
        }

	    public void Delete(bool byPassTrash)
        {
            if (!byPassTrash)
            {
                this.ContentHandler.Delete();
            }
            else
            {
                //only GenericContent has a byPassTrash functinality
                var gc = this.ContentHandler as GenericContent;
                if (gc != null)
                    gc.Delete(byPassTrash);
                else
                    this.ContentHandler.Delete();
            }
        }

		private Exception InvalidContentExceptionHelper()
		{
			var fields= new Field[Fields.Count];
			Fields.Values.CopyTo(fields, 0);

		    return new InvalidContentException(String.Concat("Cannot save the Content. Invalid Fields: ",
                String.Join(", ", (from field in fields where !field.IsValid select field.DisplayName ?? field.Name).ToArray())));
		}

        [Obsolete("Use the methods of the ContentNamingHelper class instead")]
        public static string GenerateNameFromTitle(string parent, string title)
        {
            return ContentNamingHelper.UriCleanup(title);
        }

        [Obsolete("Use the methods of the ContentNamingHelper class instead")]
        public static string GenerateNameFromTitle(string title)
        {
            return ContentNamingHelper.UriCleanup(title);
        }

		/*-------------------------------------------------------------------------- Transfer Methods */

        //---- for powershell provider
        public static Content Import(
            string data,
            int contentId,
            string parentPath,
            string name,
            string contentTypeName,
            bool withReferences,
            bool onlyReferences,
            out string[] referenceFields)
        {
            var references = new List<string>();
            var xml = new XmlDocument();
            xml.LoadXml(data);

            XmlNode nameNode = xml.SelectSingleNode("/ContentMetaData/ContentName");

            var clearPermissions = xml.SelectSingleNode("/ContentMetaData/Permissions/Clear") != null;
            var hasBreakPermissions = xml.SelectSingleNode("/ContentMetaData/Permissions/Break") != null;
            var hasPermissions = xml.SelectNodes("/ContentMetaData/Permissions/Identity").Count > 0;

            Content content = null;
            if (contentId > 0)
            {
                content = Content.Load(contentId);
                if (content == null)
                    throw new ApplicationException("Content does not exist. Id: " + contentId);
            }
            else
            {
                var path = RepositoryPath.Combine(parentPath, name);
                content = Content.Load(path);
                if (content == null)
                {
                    var parent = Node.LoadNode(parentPath);
                    if (parent == null)
                        throw new ApplicationException("Content not found: " + parentPath);
                    content = Content.CreateNew(contentTypeName, parent, name);
                }
            }
            var changed = content.Id == 0;
            var nodeList = xml.SelectNodes("/ContentMetaData/Fields/*");
            foreach (XmlNode fieldNode in nodeList)
            {
                var subType = FieldSubType.General;
                var subTypeString = ((XmlElement)fieldNode).GetAttribute("subType");
                if (subTypeString.Length > 0)
                    subType = (FieldSubType)Enum.Parse(typeof(FieldSubType), subTypeString);
                var fieldName = Field.ParseImportName(fieldNode.LocalName, subType);

                Field field;
                if (!content.Fields.TryGetValue(fieldName, out field))
                    throw new TransferException(true, "Field not found", content.ContentHandler.Path, content.ContentHandler.NodeType.Name, fieldName);

                var isReference = field is ReferenceField;
                if (isReference)
                    references.Add(field.Name);
                if (isReference && !withReferences)
                    continue;
                if (!isReference && onlyReferences)
                    continue;
                //if (field is BinaryField)
                //    continue;

                try
                {
                    field.Import(fieldNode);
                    changed = true;
                }
                catch (ReferenceNotFoundException ex)
                {
                    //skip missing user reference according to config
                    if (Repository.SkipImportingMissingReferences && Repository.SkipReferenceNames.Contains(field.Name))
                    {
                        Logger.WriteException(ex);

                        var console = RepositoryInstance.Instance != null && RepositoryInstance.Instance.StartSettings != null
                            ? RepositoryInstance.Instance.StartSettings.Console
                            : null;

                        //log this to the screen or log file if exists
                        if (console != null)
                            console.WriteLine("---------- Reference skipped: " + field.Name);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            referenceFields = references.ToArray();
            return content;
        }

        //---- for old-way-importer
        public bool ImportFieldData(ImportContext context)
        {
            return ImportFieldData(context, true);
        }
        public bool ImportFieldData(ImportContext context, bool saveContent)
		{
            bool changed = context.IsNewContent;
            foreach (XmlNode fieldNode in context.FieldData)
            {
                var subType = FieldSubType.General;
                var subTypeString = ((XmlElement)fieldNode).GetAttribute("subType");
                if (subTypeString.Length > 0)
                    subType = (FieldSubType)Enum.Parse(typeof(FieldSubType), subTypeString);
                var fieldName = Field.ParseImportName(fieldNode.LocalName, subType);

                Field field;
                if (!this.Fields.TryGetValue(fieldName, out field))
                    throw new TransferException(true, "Field not found", this.ContentHandler.Path, this.ContentHandler.NodeType.Name, fieldName);

                var refField = field as ReferenceField;
                if (!context.UpdateReferences)
                {
                    field.Import(fieldNode, context);
                    changed = true;
                }
                else
                {
                    if (refField != null)
                    {
                        try
                        {
                            field.Import(fieldNode, context);
                            changed = true;

                            //FIX THIS: in case of new content, set NodeCreatedBy correctly
                            if (field.Name == "CreatedBy" || field.Name == "ModifiedBy")
                            {
                                var fdata = field.GetData();
                                var refNodes = fdata as IEnumerable<Node>;
                                var refNode = refNodes == null ? fdata as Node : refNodes.FirstOrDefault();

                                if (refNode != null)
                                {
                                    if (field.Name == "CreatedBy") 
                                        this.ContentHandler.NodeCreatedBy = refNode;
                                    if (field.Name == "ModifiedBy")
                                        this.ContentHandler.NodeModifiedBy = refNode;
                                }
                            }
                        }
                        catch (ReferenceNotFoundException ex)
                        {
                            //skip missing user reference according to config
                            if (Repository.SkipImportingMissingReferences && Repository.SkipReferenceNames.Contains(refField.Name))
                            {
                                Logger.WriteException(ex);

                                var console = RepositoryInstance.Instance != null && RepositoryInstance.Instance.StartSettings != null
                                    ? RepositoryInstance.Instance.StartSettings.Console
                                    : null;

                                //log this to the screen or log file if exists
                                if (console != null)
                                    console.WriteLine("---------- Reference skipped: " + refField.Name);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }

            if (!changed)
                return true;

            SaveFields(context.NeedToValidate);
            if (context.NeedToValidate)
            {
                if (!this.IsValid)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string key in this.Fields.Keys)
                    {
                        Field field = this.Fields[key];
                        if (!field.IsValid)
                        {
                            sb.Append(field.GetValidationMessage());
                            sb.Append(Environment.NewLine);
                        }
                    }
                    context.ErrorMessage = sb.ToString();
                    return false;
                }
            }

            //if (context.IsNewContent)
            //    this.SaveSameVersion();
            //else
            //    this.Save(context.NeedToValidate);
            if (saveContent)
                this.SaveSameVersion();

            return true;
		}
		public void ExportFieldData(XmlWriter writer, ExportContext context)
		{
			if (this.ContentHandler is ContentType)
				return;
			foreach (var field in this.Fields.Values)
                if (field.Name != "Name" && field.Name != "Versions")
					field.Export(writer, context);
		}
		public void ExportFieldData2(XmlWriter writer, ExportContext context)
		{
			if (this.ContentHandler is ContentType)
				return;
			foreach (var field in this.Fields.Values)
                if (field.Name != "Name" && field.Name != "Versions")
					field.Export2(writer, context);
		}

        //------------------------------------------------------------------------- Xml Methods

        protected override void WriteXml(XmlWriter writer, bool withChildren, SerializationOptions options)
        {
            WriteXmlHeaderAndFields(writer, options);

            if (withChildren)
            {
                var folder = ContentHandler as IFolder;
                if (folder != null)
                {
                    writer.WriteStartElement("Children");
                    WriteXml(folder.Children, writer, options);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        public Action<XmlWriter> XmlWriterExtender;
        private const string FieldXmlCacheKey = "ContentFieldXml";

        protected override void WriteXml(XmlWriter writer, string queryFilter, QuerySettings querySettings, SerializationOptions options)
        {
            WriteXmlHeaderAndFields(writer, options);

            var folder = ContentHandler as GenericContent;
            if (folder != null)
            {
                var result = folder.GetChildren(queryFilter, querySettings, AllChildren);

                if (XmlWriterExtender != null)
                    XmlWriterExtender(writer);

                writer.WriteStartElement("Children");
                //
                //If TOP is set, we want to see only the current page of 
                //the results. If not, the whole node list will be rendered.

                //WriteXml(
                //    querySettings.Top > 0 ? result.CurrentPage : result.Nodes, writer);
                WriteXml(result.Nodes, writer, options);
                
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteXmlHeaderAndFields(XmlWriter writer, SerializationOptions options)
        {
            writer.WriteStartElement("Content");
            base.WriteHead(writer, this.ContentType.Name, this.ContentType.DisplayName, this.Name, this.ContentType.Icon, this.Path, this.ContentHandler is IFolder);

            var fieldsXml = this.ContentHandler.GetCachedData(FieldXmlCacheKey) as string;
            //string fieldsXml = null;

            if (string.IsNullOrEmpty(fieldsXml))
            {
                using (var sw = new StringWriter())
                {
                    using (var xw = new XmlTextWriter(sw))
                    {
                        xw.WriteStartElement("Fields");
                        this.WriteFieldsData(xw, options);
                        xw.WriteEndElement();

                        fieldsXml = sw.ToString();

                        //insert into cache
                        if (this.ContentHandler is GenericContent)
                            this.ContentHandler.SetCachedData(FieldXmlCacheKey, fieldsXml);
                    }
                }
            }

            //write fields xml
            writer.WriteRaw(fieldsXml);

            if (options == null || options.Actions == ActionSerializationOptions.All)
                base.WriteActions(writer, this.Path, Actions);
        }

        protected override void WriteXml(XmlWriter writer, string referenceMemberName, SerializationOptions options)
        {
            writer.WriteStartElement("Content");
            base.WriteHead(writer, this.ContentType.Name, this.Name, this.ContentType.Icon, this.Path, this.ContentHandler is IFolder);

            var fieldsXml = this.ContentHandler.GetCachedData(FieldXmlCacheKey) as string;

            if (string.IsNullOrEmpty(fieldsXml))
            {
                using (var sw = new StringWriter())
                {
                    using (var xw = new XmlTextWriter(sw))
                    {
                        xw.WriteStartElement("Fields");
                        this.WriteFieldsData(xw, options);
                        xw.WriteEndElement();

                        fieldsXml = sw.ToString();

                        //insert into cache
                        if (this.ContentHandler is GenericContent)
                            this.ContentHandler.SetCachedData(FieldXmlCacheKey, fieldsXml);
                    }
                }
            }

            //write fields xml
            writer.WriteRaw(fieldsXml);

            if (options == null || options.Actions == ActionSerializationOptions.All)
                base.WriteActions(writer, this.Path, Actions);

            if (!string.IsNullOrEmpty(referenceMemberName))
            {
                var folder = ContentHandler as IFolder;
                writer.WriteStartElement(referenceMemberName);
                //WriteXml(ContentHandler.GetReferences(referenceMemberName), writer);
                WriteXml(this[referenceMemberName] as IEnumerable<Node>, writer, options);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        private void WriteFieldsData(XmlWriter writer, SerializationOptions options)
        {
            if (options == null)
                options = SerializationOptions.Default;
            switch (options.Fields)
            {
                case FieldSerializationOptions.All:
                    foreach (var field in this.Fields.Values)
                        WriteFieldData(field, writer);
                    return;
                case FieldSerializationOptions.Custom:
                    if (options.FieldNames == null)
                        return;
                    if (options.FieldNames.Count() == 0)
                        return;
                    foreach (var fieldName in options.FieldNames)
                    {
                        Field field;
                        if (this.Fields.TryGetValue(fieldName, out field))
                            WriteFieldData(field, writer);
                    }
                    return;
                case FieldSerializationOptions.None:
                    return;
                default:
                    throw new NotImplementedException("Unknown FieldSerializationOptions: " + options.Fields);
            }
        }
        private void WriteFieldData(Field field, XmlWriter writer)
        {
            if (field.Name == "Name" || (field.Name == "Versions" && !Security.HasPermission(Storage.Schema.PermissionType.RecallOldVersion)))
                return;

            try
            {
                field.WriteXml(writer);
            }
            catch (SenseNetSecurityException)
            {
                //access denied to the field
            }
            catch (InvalidOperationException ex)
            {
                //access denied to a reference field...
                if (ex.InnerException is SenseNetSecurityException)
                    return;

                //unknown error
                Logger.WriteException(ex);
            }
            catch (Exception ex)
            {
                //unknown error
                Logger.WriteException(ex);
            }
        }

        //================================================================================== Actions

        private IEnumerable<ActionBase> _actions;
        public IEnumerable<ActionBase> Actions
        {
            get
            {
                if (_actions == null)
                    _actions = GetActions();

                return _actions;
            }
        }

        /// <summary>
        /// Returns all conventional (non-virtual) actions available on the Content.
        /// </summary>
        /// <returns>An IEnumerable&lt;ActionBase&gt;</returns> 
        public IEnumerable<ActionBase> GetActions()
        {
            return ActionFramework.GetActions(this, null);
        }
        /// <summary>
        /// Returns all conventional (non-virtual) actions available on the Content.
        /// </summary>
        /// <returns>An IEnumerable&lt;ActionBase&gt;</returns>
        [Obsolete("Use the Actions property instead")]
        public IEnumerable<ActionBase> GetContentActions()
        {
            return GetActions();
        }
        ///// <summary>
        ///// Returns actions declared by ancestor list definition if this Content is placed under a ContentList else returns an empty IEnumerable&lt;IRepositoryAction&gt;
        ///// </summary>
        ///// <returns>An IEnumerable&lt;IRepositoryAction&gt;</returns>
        //public IEnumerable<IAction> GetListItemActions()
        //{
        //    if (!this.IsContentListItem)
        //        return new IAction[0];
        //    var listNode = this.ContentHandler.LoadContentList();
        //    var list = (ContentList)listNode;
        //    //return list.Actions;
        //  if (list.Actions != null)
        //    return from action in list.Actions select action.Clone();
        //  return new List<IAction>();
        //}
        ///// <summary>
        ///// Returns actions declared by list definition if this Content wraps a ContentList else returns an empty IEnumerable&lt;IRepositoryAction&gt;
        ///// </summary>
        ///// <returns>An IEnumerable&lt;IRepositoryAction&gt;</returns>
        //public IEnumerable<IAction> GetListActions()
        //{
        //    if (!this.IsContentList)
        //        return new IAction[0];
        //    return from action in ((ContentList)this.ContentHandler).Actions select action.Clone();
        //}

        //=================================================================================== Runtime Content

        private Content(RuntimeContentHandler contentHandler, ContentType contentType)
        {
            _contentHandler = contentHandler;
            _contentType = contentType;
            _fields = new Dictionary<string, Field>();
            foreach (FieldSetting fieldSetting in contentType.FieldSettings)
            {
                Field field = Field.Create(this, fieldSetting);
                _fields.Add(field.Name, field);
            }
        }

        public static Content Create(object objectToEdit, string ctd)
        {
            if (objectToEdit == null)
                throw new ArgumentNullException("objectToEdit");
            ContentType contentType = ContentType.Create(objectToEdit.GetType(), ctd);
            if (contentType == null)
                throw new ApplicationException("Cannot create content from a " + objectToEdit.GetType().FullName);
            var node = new RuntimeContentHandler(objectToEdit, contentType);
            return new Content(node, contentType);
        }

        public class RuntimeContentHandler : Node
        {
            Type _type;
            object _object;
            public ContentType ContentType { get; private set; }

            public override bool IsContentType { get { return false; } }

            public override string Name
            {
                get
                {
                    return GetPropertySafely("Name") as string;
                }
                set
                {
                    SetPropertySafely("Name", value);
                }
            }

            public override string Path
            {
                get
                {
                    return "/Root/xxx";
                    return GetPropertySafely("Path") as string;
                }
            }

            public override int ContentListId { get { return 0; } }
            public override Storage.Schema.ContentListType ContentListType { get { return null; } }
            public override int ContentListTypeId { get { return 0; } }
            public override int NodeTypeId { get { return 0; } }
            public override Storage.Schema.NodeType NodeType { get { return null; } }

            public override int CreatedById { get { return 1; } }
            public override Node CreatedBy { get { return User.Administrator; } set { } }
            public override DateTime CreationDate { get { return DateTime.Now; } set { } }
            public override DateTime ModificationDate { get { return DateTime.Now; } set { } }
            public override Node ModifiedBy { get { return User.Administrator; } set { } }
            public override int ModifiedById { get { return 1; } }
            public override Node NodeCreatedBy { get { return User.Administrator; } set { } }
            public override int NodeCreatedById { get { return 1; } }
            public override DateTime NodeCreationDate { get { return DateTime.Now; } set { } }
            public override DateTime NodeModificationDate { get { return DateTime.Now; } set { } }
            public override Node NodeModifiedBy { get { return User.Administrator; } set { } }
            public override int NodeModifiedById { get { return 1; } }
            public override int Index { get; set; }


            public override string DisplayName
            {
                get
                {
                    return GetPropertySafely("DisplayName") as string;
                }
                set
                {
                    SetPropertySafely("DisplayName", value);
                }
            }

            private bool _isNew = true;
            public override bool IsNew
            {
                get
                {
                    return _isNew;
                }
            }

            public void SetIsNew(bool isNew)
            {
                _isNew = isNew;
            }

            public RuntimeContentHandler(object objectToEdit, ContentType contentType) : base()
            {
                _object = objectToEdit;
                _type = _object.GetType();
                this.ContentType = contentType;
            }

            public override bool HasProperty(string name)
            {
                return _type.GetProperty(name) != null;
            }

            public override object GetPropertySafely(string name)
            {
                return _type.GetProperty(name) != null ? GetProperty(name) : null;
            }

            /// <summary>
            /// Sets the value to the specified property. Use it only if you want to hide the excetion if the field does not exist.
            /// </summary>
            /// <param name="name">Name of the property</param>
            /// <param name="value">New value</param>
            protected void SetPropertySafely(string name, object value)
            {
                if (_type.GetProperty(name) != null)
                    SetProperty(name, value);
            }

            public object GetProperty(string name)
            {
                var prop = _type.GetProperty(name);
                var getter = prop.GetGetMethod();
                var value = getter.Invoke(_object, null);
                return value;
            }
            public void SetProperty(string name, object value)
            {
                var prop = _type.GetProperty(name);
                var setter = prop.GetSetMethod();
                setter.Invoke(_object, new object[] { value });
            }

            public override void Save() { }
        }

        //============================================================================= ICustomTypeDescriptor

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return new AttributeCollection(null);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return null;
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return new EventDescriptorCollection(null);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return new EventDescriptorCollection(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return PropertyDescriptors ?? GetContentProperties();
        }

	    object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        //----------------------------------------------------------- ICustomTypeDescriptor helpers

        private PropertyDescriptorCollection GetContentProperties()
        {
            var props = new List<PropertyDescriptor>();

            foreach (var field in this.Fields.Values)
            {
                var fs = FieldSetting.GetRoot(field.FieldSetting);

                props.Add(new FieldSettingPropertyDescriptor(field.Name, field.Name, fs));
                props.Add(new FieldSettingPropertyDescriptor(fs.BindingName, field.Name, fs));
            }

            return new PropertyDescriptorCollection(props.ToArray());
        }

        public static PropertyDescriptorCollection GetPropertyDescriptors(IEnumerable<string> fieldNames)
        {
            var props = new List<PropertyDescriptor>();

            if (fieldNames == null)
                return null;

            foreach (var fullName in fieldNames)
            {
                var fieldName = string.Empty;
                var fs = FieldSetting.GetFieldSettingFromFullName(fullName, out fieldName);
                var bindingName = fs == null ? FieldSetting.GetBindingNameFromFullName(fullName) : fs.BindingName;

                props.Add(new FieldSettingPropertyDescriptor(bindingName, fieldName, fs));
            }

            return new PropertyDescriptorCollection(props.ToArray());
        }

        public class FieldSettingPropertyDescriptor : PropertyDescriptor
        {
            private FieldSetting _fieldSetting;
            private readonly string _fieldName;

            public FieldSettingPropertyDescriptor(string bindingName, string fieldName, FieldSetting fieldSetting)
                : base(bindingName, null)
            {
                this._fieldSetting = FieldSetting.GetRoot(fieldSetting);
                this._fieldName = fieldName;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return typeof(Content); }
            }

            public override object GetValue(object component)
            {
                var content = component as Content;

                if (content == null)
                    throw new ArgumentException("Component must be a content!", "component");

                if (!content.Fields.ContainsKey(_fieldName))
                    return null;
                
                if (_fieldSetting == null && _fieldName.StartsWith("#"))
                {
                    // this is a contentlist field. We can find the
                    //appropriate field setting for it now, when we have
                    //the exact content list
                    var cl = content.ContentHandler.LoadContentList() as ContentList;

                    if (cl != null)
                    {
                        foreach (var clfs in cl.FieldSettings)
                        {
                            if (clfs.Name.CompareTo(_fieldName) != 0) 
                                continue;

                            _fieldSetting = clfs.GetEditable();
                            break;
                        }
                    }
                }

                var fs = FieldSetting.GetRoot(content.Fields[_fieldName].FieldSetting);
                object result;

                if (_fieldSetting == null || fs == null)
                {
                    //we have not enough info for fullname check
                    result = content[_fieldName];
                }
                else
                {
                    //return the value only if fieldname refers to
                    //the same field (not just a field with the same name)
                    result = _fieldSetting.FullName.CompareTo(fs.FullName) != 0 ? null : content[_fieldName];
                }

                //format or change the value based on its type

                //CHOICE
                var sList = result as List<string>;
                if (sList != null)
                {
                    var chf = _fieldSetting as ChoiceFieldSetting;

                    if (chf != null)
                    {
                        result = new ChoiceOptionValueList<string>(sList, chf);
                    }
                    else
                    {
                        result = new StringValueList<string>(sList);
                    }

                    return result;
                }

                //REFERENCE
                var nodeList = result as List<Node>;
                if (nodeList != null)
                {
                    return new NodeValueList<Node>(nodeList);
                }

                //NUMBER
                if (result != null && content.Fields[_fieldName] is NumberField)
                {
                    if ((decimal)result == ActiveSchema.DecimalMinValue)
                        return null;
                }

                //INTEGER
                if (result != null && content.Fields[_fieldName] is IntegerField)
                {
                    if ((int)result == int.MinValue)
                        return null;
                }

                //HYPERLINK
                if (result != null && content.Fields[_fieldName] is HyperLinkField)
                {
                    var linkData = result as HyperLinkField.HyperlinkData;
                    if (linkData == null)
                        return null;

                    var sb = new StringBuilder();
                    sb.Append("<a");
                    if (linkData.Href != null)
                        sb.Append(" href=\"").Append(linkData.Href).Append("\"");
                    if (linkData.Target != null)
                        sb.Append(" target=\"").Append(linkData.Target).Append("\"");
                    if (linkData.Title != null)
                        sb.Append(" title=\"").Append(linkData.Title).Append("\"");
                    sb.Append(">");
                    sb.Append(linkData.Text ?? "");
                    sb.Append("</a>");
                    return sb.ToString();
                }

                return result;
            }

            public override bool IsReadOnly
            {
                get { return true; }
            }

            public override Type PropertyType
            {
                get { return _fieldSetting == null ? typeof(object) : _fieldSetting.FieldDataType; }
            }

            public override void ResetValue(object component)
            {

            }

            public override void SetValue(object component, object value)
            {

            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }
    }

    public class StringValueList<T> : List<string>
    {
        public StringValueList() : base() { }
        public StringValueList(IEnumerable<string> list) : base(list) { }

        public override string ToString()
        {
            return string.Join(", ", this.ToArray());
        }
    }

    public class NodeValueList<T> : List<Node>
    {
        public NodeValueList() : base() { }
        public NodeValueList(IEnumerable<Node> list) : base(list) { }

        public override string ToString()
        {
            return string.Join(", ", (from node in this 
                                     select node.ToString()).ToArray());
        }
    }

    public class ChoiceOptionValueList<T> : List<string>
    {
        private readonly ChoiceFieldSetting _fieldSetting;
        private readonly bool _displayValue;

        public ChoiceOptionValueList() : base()
        {
        }

        public ChoiceOptionValueList(IEnumerable<string> list, ChoiceFieldSetting fieldSetting) : this(list, fieldSetting, false) {}

        public ChoiceOptionValueList(IEnumerable<string> list, ChoiceFieldSetting fieldSetting, bool displayValue) : base(list)
        {
            _fieldSetting = fieldSetting;
            _displayValue = displayValue;
        }

        public override string ToString()
        {
            if (_fieldSetting == null)
                return string.Empty;

            var resultOptions = _displayValue ?
                (from opt in _fieldSetting.Options
                                 where this.Contains(opt.Value)
                                 select opt.Value).ToList() :
                (from opt in _fieldSetting.Options
                                 where this.Contains(opt.Value)
                                 select opt.Text).ToList();

            if (_fieldSetting.AllowExtraValue.HasValue && _fieldSetting.AllowExtraValue.Value)
            {
                resultOptions.AddRange(from str in this
                                       where _fieldSetting.Options.Count(opt => opt.Value.CompareTo(str) == 0) == 0
                                       select str);
            }

            return string.Join(", ", resultOptions.ToArray());
        }
    } 
}
