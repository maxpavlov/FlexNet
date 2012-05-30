using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Reflection;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Data;
using System.Linq;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Search;
using System.Diagnostics;

namespace SenseNet.ContentRepository
{
	[ContentHandler]
	public class ContentList : Folder, IContentList
	{
		private class SlotTable
		{
			Dictionary<DataType, List<int>> _slotTable;
			Dictionary<DataType, int> _currentSlots;

			public SlotTable(Dictionary<string, List<string>> bindings)
			{
				_slotTable = new Dictionary<DataType, List<int>>();
				_currentSlots = new Dictionary<DataType, int>();
				foreach (DataType dataType in Enum.GetValues(typeof(DataType)))
				{
					_slotTable.Add(dataType, new List<int>());
					_currentSlots.Add(dataType, -1);
				}
				foreach (string key in bindings.Keys)
				{
					foreach (string binding in bindings[key])
					{
						DataType dataType;
						int ordinalNumber;
						ContentList.DecodeBinding(binding, out dataType, out ordinalNumber);
						_slotTable[dataType].Add(ordinalNumber);
					}
				}
			}

			public int ReserveSlot(DataType dataType)
			{
				List<int> slots = _slotTable[dataType];
				int currentSlot = _currentSlots[dataType];
				while (slots.Contains(++currentSlot)) ;
				slots.Add(currentSlot);
				_currentSlots[dataType] = currentSlot;
				return currentSlot;
			}
		}

        private static readonly string ContentListDefinitionXmlNamespaceOld = "http://schemas.sensenet" + ".hu/SenseNet/ContentRepository/Lis"+"terTypeDefinition";
		public static readonly string ContentListDefinitionXmlNamespace = "http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition";
        private static string ContentListDefinitionSchemaManifestResourceName = "SenseNet.ContentRepository.Schema.ContentListDefinition.xsd";
        public static readonly string ContentListFileNameExtension = "ContentListDefinition";
        private static string DefaultContentListDefinition
        {
            get
            {
                return String.Concat("<ContentListDefinition xmlns='", ContentListDefinitionXmlNamespace, "'><Fields /></ContentListDefinition>");
            }
        }

        private string _displayName;
		private string _description;
		private string _icon;
		private List<FieldSetting> _fieldSettings;
		private ContentListType _contentListType;
        //private IEnumerable<IAction> _actions;

		//================================================================================= Properties

        [RepositoryProperty("ContentListBindings", RepositoryDataType.Text)]
        public Dictionary<string, List<string>> ContentListBindings
		{
            get { return ParseBindingsXml(base.GetProperty<string>("ContentListBindings")); }
            set { this["ContentListBindings"] = CreateBindingsXml(value); }
		}
        [RepositoryProperty("ContentListDefinition", RepositoryDataType.Text)]
		public string ContentListDefinition
		{
            get { return base.GetProperty<string>("ContentListDefinition"); }
			set
			{
                var doc = GetValidDocument(value);
                this["ContentListDefinition"] = value;
                Build(doc, this.ContentListBindings, true);
			}
		}

        [RepositoryProperty("DefaultView")]
        public string DefaultView
        {
            get { return GetProperty<string>("DefaultView"); }
            set { this["DefaultView"] = value; }
        }

        internal List<FieldSetting> FieldSettings
		{
			get { return _fieldSettings; }
		}
        //public IEnumerable<IAction> Actions
        //{
        //    get { return _actions; }
        //}

        //// readonly 
        //public IEnumerable<FieldSetting> GetFieldSettings()
        //{
        //    return FieldSettings.AsReadOnly();
        //}

        public IEnumerable<Node> FieldSettingContents
        {
            get
            {
                return from fs in this.FieldSettings
                       where ActiveSchema.NodeTypes[fs.GetType().Name] != null
                       select new FieldSettingContent(fs.GetEditable(), this) as Node;
            }
        }

        public IEnumerable<Node> AvailableContentTypeFieldSettingContents
        {
            get
            {
                var availableFields = new List<FieldSetting>();
                var fsContents = new List<Node>();

                GetAvailableContentTypeFields(availableFields);

                foreach (var fs in availableFields)
                {
                    try
                    {
                        //nodetype check is needed here because newly created
                        //field types doesn't necessary have a ctd
                        if ((fs.VisibleBrowse != FieldVisibility.Hide || 
                             fs.VisibleEdit != FieldVisibility.Hide || 
                             fs.VisibleNew != FieldVisibility.Hide) && ActiveSchema.NodeTypes[fs.GetType().Name] != null)
                            fsContents.Add(new FieldSettingContent(fs.GetEditable(), this));
                    }
                    catch (RegistrationException ex)
                    {
                        //ctd doesn't exist for this field type
                        Logger.WriteException(ex);
                    }
                }

                return fsContents;
            }
        }

        public const string AVAILABLEVIEWS = "AvailableViews";
        [RepositoryProperty(AVAILABLEVIEWS, RepositoryDataType.Reference)]
        public IEnumerable<Node> AvailableViews
        {
            get
            {
                return this.GetReferences(AVAILABLEVIEWS);
                       
            }
            set
            {
                this.SetReferences(AVAILABLEVIEWS, value);
            }
        }

		//================================================================================= Construction

        public ContentList(Node parent) : this(parent, null) { }
		public ContentList(Node parent, string nodeTypeName) : base(parent, nodeTypeName)
		{
			Initialize();
		}
		protected ContentList(NodeToken nt) : base(nt)
		{
			Initialize();
			//Build();
		}

		private void Initialize()
		{
			_fieldSettings = new List<FieldSetting>();
		}
		private void Build()
		{
                //-- only Loading calls
                string def = this.ContentListDefinition;
                if (String.IsNullOrEmpty(def))
                    return;
                Build(new XPathDocument(new StringReader(def)), this.ContentListBindings, false);
		}
        private void Build(IXPathNavigable definitionXml, Dictionary<string, List<string>> bindings, bool modify)
        {
            XPathNavigator nav = definitionXml.CreateNavigator();
            XmlNamespaceManager nsres = new XmlNamespaceManager(nav.NameTable);
            XPathNavigator root = nav.SelectSingleNode("/*[1]", nsres);
            nsres.AddNamespace("x", root.NamespaceURI);
            List<FieldSetting> fieldSettings;

            Dictionary<string, FieldDescriptor> fieldDescriptorList = ParseContentTypeElement(root, nsres);
            _contentListType = ManageContentListType(fieldDescriptorList, bindings, modify, out fieldSettings);

            _fieldSettings = fieldSettings;
            SetFieldSlots();
        }

		private Dictionary<string, FieldDescriptor> ParseContentTypeElement(XPathNavigator contentTypeElement, IXmlNamespaceResolver nsres)
		{
			Dictionary<string, FieldDescriptor> result = null;
			foreach (XPathNavigator subElement in contentTypeElement.SelectChildren(XPathNodeType.Element))
			{
				switch (subElement.LocalName)
				{
                    case "DisplayName":
                        _displayName = subElement.Value;
				        break;
					case "Description":
						_description = subElement.Value;
						break;
					case "Icon":
						_icon = subElement.Value;
						break;
					case "Fields":
						result = ParseFieldElements(subElement, nsres);
						break;
                    case "Actions":
                        //_actions = null;
                        //ParseActions(subElement, nsres);
                        Logger.WriteWarning("Ignoring obsolete Actions element in List definition: " + this.Name);
                        break;
                    default:
                        throw new NotSupportedException(String.Concat("Unknown element in ContentListDefinition: ", subElement.LocalName));
				}
			}
			return result;
		}
		private Dictionary<string, FieldDescriptor> ParseFieldElements(XPathNavigator fieldsElement, IXmlNamespaceResolver nsres)
		{
			Dictionary<string, FieldDescriptor> fieldDescriptorList = new Dictionary<string, FieldDescriptor>();
            ContentType listType = ContentType.GetByName("ContentList");
			foreach (XPathNavigator fieldElement in fieldsElement.SelectChildren(XPathNodeType.Element))
			{
				FieldDescriptor fieldDescriptor = FieldDescriptor.Parse(fieldElement, nsres, listType);
				fieldDescriptorList.Add(fieldDescriptor.FieldName, fieldDescriptor);
			}
			return fieldDescriptorList;
		}
		private ContentListType ManageContentListType(Dictionary<string, FieldDescriptor> fieldInfoList, Dictionary<string, List<string>> oldBindings, bool modify, out List<FieldSetting> fieldSettings)
		{
			fieldSettings = new List<FieldSetting>();
			if (!modify)
			{
				//-- Load
                foreach (string name in fieldInfoList.Keys)
                    fieldSettings.Add(FieldSetting.Create(fieldInfoList[name], oldBindings[name]));
                return this.ContentListType;
			}

			SchemaEditor editor = new SchemaEditor();
			editor.Load();
			bool hasChanges = false;
            var listType = this.ContentListType;
			Dictionary<string, List<string>> newBindings = new Dictionary<string, List<string>>();
			SlotTable slotTable = new SlotTable(oldBindings);
			if (listType == null)
			{
				//-- new
				listType = editor.CreateContentListType(Guid.NewGuid().ToString());
				foreach (string name in fieldInfoList.Keys)
					fieldSettings.Add(CreateNewFieldType(fieldInfoList[name], newBindings, listType, slotTable, editor));
				hasChanges = true;
			}
			else
			{
				//-- merge
				listType = editor.ContentListTypes[listType.Name];
				hasChanges |= RemoveUnusedFields(fieldInfoList, oldBindings, listType, editor);
				foreach (string name in fieldInfoList.Keys)
				{
					FieldSetting origField = GetFieldTypeByName(name, _fieldSettings);
					if (origField == null)
					{
						fieldSettings.Add(CreateNewFieldType(fieldInfoList[name], newBindings, listType, slotTable, editor));
						hasChanges = true;
					}
					else
					{
						List<string> bindList = new List<string>(origField.Bindings.ToArray());
                        fieldSettings.Add(FieldSetting.Create(fieldInfoList[name], bindList));
                        newBindings.Add(name, bindList);
					}
				}
			}
			if (hasChanges)
				editor.Register();
            this.ContentListBindings = newBindings;
			return ActiveSchema.ContentListTypes[listType.Name];
		}
		private FieldSetting CreateNewFieldType(FieldDescriptor fieldInfo, Dictionary<string, List<string>> newBindings, ContentListType listType, SlotTable slotTable, SchemaEditor editor)
		{
			List<string> bindList = new List<string>();
			foreach (RepositoryDataType slotType in FieldManager.GetDataTypes(fieldInfo.FieldTypeShortName))
			{
				if (slotType == RepositoryDataType.NotDefined)
					continue;
				int slotNumber = slotTable.ReserveSlot((DataType)slotType);
				string binding = EncodeBinding(slotType, slotNumber);
				bindList.Add(binding);

				PropertyType pt = editor.PropertyTypes[binding];
				if (pt == null)
					pt = editor.CreateContentListPropertyType((DataType)slotType, slotNumber);
				editor.AddPropertyTypeToPropertySet(pt, listType);
			}
			newBindings.Add(fieldInfo.FieldName, bindList);

			return FieldSetting.Create(fieldInfo, bindList);
		}
		private bool RemoveUnusedFields(Dictionary<string, FieldDescriptor> fieldInfoList, Dictionary<string, List<string>> oldBindings, ContentListType listType, SchemaEditor editor)
		{
			bool hasChanges = false;
			for (int i = _fieldSettings.Count - 1; i >= 0; i--)
			{
				FieldSetting oldType = _fieldSettings[i];
				bool needtoDelete = !fieldInfoList.ContainsKey(oldType.Name);
				if (!needtoDelete)
				{
					FieldDescriptor newType = fieldInfoList[oldType.Name];
					if (oldType.DataTypes.Length != newType.DataTypes.Length)
					{
						needtoDelete = true;
					}
					else
					{
						for (int j = 0; j < oldType.DataTypes.Length; j++)
						{
							if (oldType.DataTypes[j] != newType.DataTypes[j])
							{
								needtoDelete = true;
								break;
							}
						}
					}
				}
				if (needtoDelete)
				{
					hasChanges = true;
					foreach (string binding in oldType.Bindings)
					{
						PropertyType oldPropertyType = editor.PropertyTypes[binding];
						editor.RemovePropertyTypeFromPropertySet(oldPropertyType, listType);
					}
					_fieldSettings.RemoveAt(i);
					oldBindings.Remove(oldType.Name);
				}
			}
			//-- Apply changes. Slot reusing prerequisit: values of unused slots must be null.
            //if (hasChanges)
            //    editor.Register();
            return hasChanges;
		}
        private void SetFieldSlots()
        {
            //-- Field slot indices and readonly.
            foreach (FieldSetting fieldSetting in this.FieldSettings)
            {
                if (fieldSetting.DataTypes.Length == 0)
                    continue;
                Type[][] slots = fieldSetting.HandlerSlots;
                for (int i = 0; i < fieldSetting.Bindings.Count; i++)
                {
                    string propName = fieldSetting.Bindings[i];
                    Type propertyType = null;
                    bool readOnly = false;

                    //-- generic property
                    RepositoryDataType dataType = fieldSetting.DataTypes[i];
                    switch (dataType)
                    {
                        case RepositoryDataType.String:
                        case RepositoryDataType.Text:
                            propertyType = typeof(string);
                            break;
                        case RepositoryDataType.Int:
                            propertyType = typeof(Int32);
                            break;
                        case RepositoryDataType.Currency:
                            propertyType = typeof(decimal);
                            break;
                        case RepositoryDataType.DateTime:
                            propertyType = typeof(DateTime);
                            break;
                        case RepositoryDataType.Binary:
                            propertyType = typeof(BinaryData);
                            break;
                        case RepositoryDataType.Reference:
                            propertyType = typeof(NodeList<Node>);
                            break;
                        default:
                            throw new ContentRegistrationException(String.Concat("Unknown datatype: ", dataType), this.Name, fieldSetting.Name);
                    }

                    for (int j = 0; j < slots[i].Length; j++)
                    {
                        ////-- for your information:
                        //typeof(Node).IsAssignableFrom(typeof(User)) ==> true
                        //typeof(User).IsAssignableFrom(typeof(Node)) ==> false
                        //-- this is the bad code:
                        //if (propertyType.IsAssignableFrom(slots[i][j]))
                        //-- this is the good:
                        try
                        {
                            var a = fieldSetting.Name[0] == '#';
                            var b = fieldSetting.DataTypes.Length == 0;
                            var x = fieldSetting.HandlerSlotIndices[i];
                        }
                        catch
                        {
                            throw;
                        }
                        if (slots[i][j].IsAssignableFrom(propertyType))
                        {
                            fieldSetting.HandlerSlotIndices[i] = j;
                            fieldSetting.PropertyIsReadOnly = readOnly;
                            break;
                        }
                    }
                }
                fieldSetting.Initialize();
            }
        }

        //private void ParseActions(XPathNavigator actionsElement, IXmlNamespaceResolver nsres)
        //{
        //    var actions = new List<IAction>();
        //    foreach (XPathNavigator actionElement in actionsElement.SelectChildren(XPathNodeType.Element))
        //        actions.Add(ContentTypeManager.RepositoryActionFactory.Parse(actionElement, nsres));
        //    _actions = actions.AsReadOnly();
        //}

		//================================================================================= Node, IContentList

		public override void Save()
		{
			if (String.IsNullOrEmpty(this.ContentListDefinition))
				this.ContentListDefinition = DefaultContentListDefinition;

			base.Save();
		}

        public override void Save(SavingMode mode)
        {
            if (String.IsNullOrEmpty(this.ContentListDefinition))
                this.ContentListDefinition = DefaultContentListDefinition;

            base.Save(mode);
        }

        public override void Save(NodeSaveSettings settings)
        {
            Security.Assert(PermissionType.ManageListsAndWorkspaces);
            base.Save(settings);
        }

        public override void ForceDelete()
        {
            Security.Assert(PermissionType.ManageListsAndWorkspaces);
            base.ForceDelete();
        }

		public ContentListType GetContentListType()
		{
			return _contentListType;
		}

		//================================================================================= Generic Property handling

		public override object GetProperty(string name)
		{
			switch (name)
			{
                case "ContentListDefinition":
					return this.ContentListDefinition;
                case "FieldSettingContents":
			        return this.FieldSettingContents;
                case "AvailableContentTypeFields":
			        return this.AvailableContentTypeFieldSettingContents;
                case "DefaultView":
                    return this.DefaultView;
                case AVAILABLEVIEWS:
                    return this.AvailableViews;
				default:
					return base.GetProperty(name);
			}
		}
		public override void SetProperty(string name, object value)
		{
			switch (name)
			{
                case "ContentListDefinition":
					this.ContentListDefinition = (string)value;
					break;
                case "FieldSettingContents":
                case "AvailableContentTypeFields":
			        break;
                case "DefaultView":
                    this.DefaultView = (string)value;
                    break;
                case AVAILABLEVIEWS:
			        this.AvailableViews = (IEnumerable<Node>)value;
                    break;
				default:
					base.SetProperty(name, value);
					break;
			}
		}

        //================================================================================= Copy

        protected override void CopyDynamicProperties(Node target)
        {
            var content = (GenericContent)target;
            foreach (var propType in this.PropertyTypes)
                if (propType.Name != "ContentListBindings" && !EXCLUDED_COPY_PROPERTIES.Contains(propType.Name))
                    if (!propType.IsContentListProperty || target.PropertyTypes[propType.Name] != null)
                        content.SetProperty(propType.Name, this.GetProperty(propType.Name));
        }

		//================================================================================= Xml validation

        private IXPathNavigable GetValidDocument(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                xml = DefaultContentListDefinition;

            if (RepositoryConfiguration.BackwardCompatibilityXmlNamespaces)
                xml = xml.Replace(ContentListDefinitionXmlNamespaceOld, ContentListDefinitionXmlNamespace);

            var doc = new XPathDocument(new StringReader(xml));
            CheckValidation(doc);
            return doc;
        }
        private static void CheckValidation(IXPathNavigable xml)
		{
            //XmlValidator schema = LoadSchemaDocument();
            var schema = XmlValidator.LoadFromManifestResource(Assembly.GetExecutingAssembly(), ContentListDefinitionSchemaManifestResourceName);
			if (!schema.Validate(xml))
			{
				if (schema.Errors.Count == 0)
					throw new ContentRegistrationException(SR.Exceptions.Registration.Msg_InvalidContentListDefinitionXml);
				else
					throw new ContentRegistrationException(String.Concat(
						SR.Exceptions.Registration.Msg_InvalidContentListDefinitionXml, ": ", schema.Errors[0].Exception.Message),
						schema.Errors[0].Exception);
			}
		}

		//================================================================================= Tools

		private static FieldSetting GetFieldTypeByName(string fieldName, List<FieldSetting> fieldSettings)
		{
			int i = GetFieldTypeIndexByName(fieldName, fieldSettings);
			return i < 0 ? null : fieldSettings[i];
		}
		private static int GetFieldTypeIndexByName(string fieldName, List<FieldSetting> fieldSettings)
		{
			for (int i = 0; i < fieldSettings.Count; i++)
				if (fieldSettings[i].Name == fieldName)
					return i;
			return -1;
		}

		private string CreateBindingsXml(Dictionary<string, List<string>> bindingList)
		{
			/*
				<?xml version="1.0" encoding="utf-8"?>
				<Bindings>
					<Bind field="[fieldName]">[propName] [propName]</Bind>
					<Bind field="[fieldName]">[propName] [propName]</Bind>
				</Bindings>
			*/
			StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Bindings>");
			foreach (string name in bindingList.Keys)
			{
				List<string> propList = bindingList[name];
				sb.Append("\t<Bind field=\"").Append(name).Append("\">");
				for (int i = 0; i < propList.Count; i++)
					sb.Append(i > 0 ? " " : "").Append(propList[i]);
				sb.Append("</Bind>\r\n");
			}
			sb.Append("</Bindings>");
			return sb.ToString();
		}
		private Dictionary<string, List<string>> ParseBindingsXml(string bindings)
		{
			Dictionary<string, List<string>> bindingList = new Dictionary<string, List<string>>();
			if (String.IsNullOrEmpty(bindings))
				return bindingList;
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(bindings);
			foreach (XmlNode node in xml.SelectNodes("/Bindings/Bind"))
				bindingList.Add(node.Attributes["field"].Value, new List<string>(node.InnerText.Trim().Split(' ')));
			return bindingList;
		}

		internal static string EncodeBinding(RepositoryDataType slotType, int slotNumber)
		{
			return String.Concat("#", slotType, "_", slotNumber);
		}
		internal static void DecodeBinding(string binding, out DataType dataType, out int ordinalNumber)
		{
			int p = binding.IndexOf('_');
			dataType = (DataType)Enum.Parse(typeof(DataType), binding.Substring(1, p - 1));
			ordinalNumber = int.Parse(binding.Substring(p + 1));
		}

		public string GetPropertySingleId(string propertyName)
		{
            if (ContentListBindings[propertyName] != null)
			{
                return ContentListBindings[propertyName][0];
			}
			return string.Empty;
		}

        // Returns the node itself cast to ContentList if it's a ContentList, or the
        // ContentList containing the node.
        public static ContentList GetContentListForNode(Node n)
        {
            ContentList result = n as ContentList;

            if (result == null && n != null)
                result = n.LoadContentList() as ContentList;

            return result;
        }

        // For cases where we know we have no ContentListId on the node
        // Eg. Folders or configuration systemfolder contents
        public static ContentList GetContentListByParentWalk(Node child)
        {
            return Node.GetAncestorOfType<ContentList>(child);
        }

        //================================================================================= Field operations

        public override List<FieldSetting> GetAvailableFields(bool rootFields)
        {
            var availableFields = base.GetAvailableFields(rootFields);

            foreach (var fieldSetting in this.FieldSettings)
            {
                var fsRoot = FieldSetting.GetRoot(fieldSetting);

                if (!availableFields.Contains(fsRoot))
                    availableFields.Add(fsRoot);
            }

            return availableFields;
        }

        public void AddField(FieldSetting fieldSetting)
        {
            if (fieldSetting == null)
                throw new ArgumentNullException("fieldSetting");

            if (FieldExists(fieldSetting))
                throw new ArgumentException("Existing list field: " + fieldSetting.Name);

            AddFieldInternal(fieldSetting);
        }

        private void AddFieldInternal(FieldSetting fieldSetting)
        {
            if (string.IsNullOrEmpty(this.ContentListDefinition))
                this.ContentListDefinition = DefaultContentListDefinition;

            var doc = new XmlDocument();
            doc.LoadXml(this.ContentListDefinition);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("x", ContentListDefinitionXmlNamespace);

            var fields = doc.DocumentElement.SelectSingleNode("/x:ContentListDefinition/x:Fields", nsmgr);

            using (var writer = fields.CreateNavigator().AppendChild())
            {
                fieldSetting.WriteXml(writer);
            }
            
            this.ContentListDefinition = doc.OuterXml;
            this.Save();
        }

        public void AddOrUpdateField(FieldSetting fieldSetting)
        {
            if (fieldSetting == null)
                throw new ArgumentNullException("fieldSetting");

            if (FieldExists(fieldSetting))
                UpdateFieldInternal(fieldSetting);
            else
                AddFieldInternal(fieldSetting);
        }

        public void UpdateField(FieldSetting fieldSetting)
        {
            if (fieldSetting == null)
                throw new ArgumentNullException("fieldSetting");

            if (!FieldExists(fieldSetting))
                throw new ArgumentException("List field does not exist: " + fieldSetting.Name);

            foreach (var fs in this.FieldSettings)
            {
                if (fs.Name.CompareTo(fieldSetting.Name) != 0) 
                    continue;

                if (fs.ShortName.CompareTo(fieldSetting.ShortName) != 0)
                    throw new ArgumentException(string.Format("List field types does not match: {0}, {1}", fs.ShortName, fieldSetting.ShortName));
                    
                break;
            } 

            UpdateFieldInternal(fieldSetting);
        }

        private void UpdateFieldInternal(FieldSetting fieldSetting)
        {
            XmlDocument doc;
            var node = FindFieldXmlNode(fieldSetting.Name, out doc);
            var fields = node.ParentNode;
            
            fields.RemoveChild(node);

            using (var writer = fields.CreateNavigator().AppendChild())
            {
                fieldSetting.WriteXml(writer);
            }

            this.ContentListDefinition = doc.OuterXml;
            this.Save();
        }

        public void DeleteField(FieldSetting fieldSetting)
        {
            //do not throw an exception, if field does not exist
            if (FieldExists(fieldSetting))
                DeleteFieldInternal(fieldSetting);
        }

        public void UpdateContentListDefinition(IEnumerable<FieldSettingContent> fieldSettings)
        {
            var doc = new XmlDocument();
            doc.LoadXml(this.ContentListDefinition);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("x", ContentListDefinitionXmlNamespace);

            if (doc.DocumentElement == null)
                return;

            var fieldsNode = doc.DocumentElement.SelectSingleNode("/x:ContentListDefinition/x:Fields", nsmgr);
            fieldsNode.RemoveAll();

            using (var writer = fieldsNode.CreateNavigator().AppendChild())
            {
                foreach (var fieldSetting in fieldSettings)
                {
                    fieldSetting.FieldSetting.WriteXml(writer);
                }
            }

            this.ContentListDefinition = doc.OuterXml;
        }

        private void DeleteFieldInternal(FieldSetting fieldSetting)
        {
            DeleteFieldInternal(fieldSetting, true);
        }

	    private void DeleteFieldInternal(FieldSetting fieldSetting, bool saveImmediately)
        {
	        XmlDocument doc;
	        var node = FindFieldXmlNode(fieldSetting.Name, out doc);

            if (node == null)
                return;

	        node.ParentNode.RemoveChild(node);

            this.ContentListDefinition = doc.OuterXml;

	        if (!saveImmediately) 
                return;

	        this.Save();
        }

        private XmlNode FindFieldXmlNode(string fieldName, out XmlDocument doc)
        {
            doc = new XmlDocument();
            doc.LoadXml(this.ContentListDefinition);

            if (string.IsNullOrEmpty(fieldName))
                return null;

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("x", ContentListDefinitionXmlNamespace);

            var xTemplate = string.Format("/x:ContentListDefinition/x:Fields/x:ContentListField[@name='{0}']", fieldName);

            return string.IsNullOrEmpty(fieldName) ? null : 
                doc.DocumentElement.SelectSingleNode(xTemplate, nsmgr);
        }

        private bool FieldExists(FieldSetting fieldSetting)
        {
            if (fieldSetting == null || fieldSetting.Name == null)
                return false;

            return this.ContentListBindings.Keys.Contains(fieldSetting.Name);
        }

	    //================================================================================= Cached data

        private const string CONTENTLISTTYPEKEY = "ContentListType";
        private const string FIELDSETTINGSKEY = "FieldSettings";

        protected override void OnLoaded(object sender, NodeEventArgs e)
        {
            base.OnLoaded(sender, e);

            _contentListType = (ContentListType)base.GetCachedData(CONTENTLISTTYPEKEY);
            if (_contentListType != null)
            {
                _fieldSettings = (List<FieldSetting>)base.GetCachedData(FIELDSETTINGSKEY);
                return;
            }

            Build();
            base.SetCachedData(CONTENTLISTTYPEKEY, _contentListType);
            base.SetCachedData(FIELDSETTINGSKEY, _fieldSettings);
        }
	}
}
