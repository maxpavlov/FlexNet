using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.Xml;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Scripting;
using System.IO;
using System.Web;
using LucField = Lucene.Net.Documents.Field;
using SenseNet.Search;
using SenseNet.Search.Indexing;

namespace SenseNet.ContentRepository.Schema
{
    public enum FieldVisibility
    {
        Show, Hide, Advanced
    }
    public enum OutputMethod
    {
        Default, Raw, Text, Html
    }

    /// <summary>
    /// The <c>FieldSetting</c> class represents the contents of the Field/Configuration element within the Content Type Definition.
    /// </summary>
    /// <remarks>
    /// The <c>FieldSetting</c> is basically the validation logic for a <see cref="SenseNet.ContentRepository.Field">Field</see> object.
    /// 
    /// Looking at the big picture, a Content Type Definition defines a Content Type by defining <see cref="SenseNet.ContentRepository.Field">Field</see>s (within the Field tag of the CTD) and FieldSettings to each fields (within the Field/Configuration element in the CTD)
    /// A ContentType is therefore a collection of Fields with a FieldSetting assigned to each.
    /// 
    /// FieldSettings by default are assigned to <see cref="SenseNet.ContentRepository.Field">Field</see>s automatically (e.g. <see cref="SenseNet.ContentRepository.Fields.ShortTextFieldSetting">ShortTextFieldSetting</see> is assigned to <see cref="SenseNet.Portal.UI.Controls.ShortText">ShortText</see>).
    /// However, a custom FieldSetting can be assigned to a <see cref="SenseNet.ContentRepository.Field">Field</see> by specifying the handler attribute of the Field/Configuration element.
    /// </remarks>
    /// 
    [System.Diagnostics.DebuggerDisplay("Name={Name}, Type={ShortName}, Owner={Owner == null ? \"[null]\" : Owner.Name}  ...")]
    public abstract class FieldSetting
    {
        public const string CompulsoryName = "Compulsory";
        public const string OutputMethodName = "OutputMethod";
        public const string ReadOnlyName = "ReadOnly";
        public const string DefaultValueName = "DefaultValue";
        public const string DefaultOrderName = "DefaultOrder";
        public const string VisibleName = "Visible";
        public const string VisibleBrowseName = "VisibleBrowse";
        public const string VisibleEditName = "VisibleEdit";
        public const string VisibleNewName = "VisibleNew";
        public const string ControlHintName = "ControlHint";
        public const string AddToDefaultViewName = "AddToDefaultView";
        public const string ShortNameName = "ShortName";
        public const string FieldClassNameName = "FieldClassName";
        public const string DescriptionName = "Description";
        public const string IconName = "Icon";
        public const string AppInfoName = "AppInfo";
        public const string OwnerName = "Owner";
        public const string FieldIndexName = "FieldIndex";

        // Member variables /////////////////////////////////////////////////////////////////
        protected bool _mutable = false;
        private string _displayName;
        private string _description;
        private string _icon;
        private string _appInfo;

        private bool? _configIsReadOnly;
        private bool? _required;
        private string _defaultValue;
        private OutputMethod? _outputMethod;

        private bool? _visible;
        private FieldVisibility? _visibleBrowse;
        private FieldVisibility? _visibleEdit;
        private FieldVisibility? _visibleNew;
        private int? _defaultOrder;
        private string _controlHint;
        private int? _fieldIndex;

        // Properties /////////////////////////////////////////////////////////////
        private string _name;
        /// <summary>
        /// Gets the name of the descripted Field. This value comes from the ContentTypeDefinition.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting Name is not allowed within readonly instance.");
                _name = value;
            }
        }

        private string _shortName;
        /// <summary>
        /// Gets the ShortName of the descripted Field. This value comes from the ContentTypeDefinition.
        /// </summary>
        public string ShortName
        {
            get { return _shortName; }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting ShortName is not allowed within readonly instance.");
                _shortName = value;
            }
        }

        private string _fieldClassName;
        /// <summary>
        /// Gets the fully qualified name of the descripted Field. This value comes from the ContentTypeDefinition or derived from the ShortName.
        /// </summary>
        public string FieldClassName
        {
            get { return _fieldClassName; }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting FieldClassName is not allowed within readonly instance.");
                _fieldClassName = value;
            }
        }

        /// <summary>
        /// Gets the displayname of the descripted Field. This value comes from the ContentTypeDefinition.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (_displayName != null)
                    return _displayName;
                if (ParentFieldSetting != null)
                    return ParentFieldSetting.DisplayName;
                return null;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting DisplayName is not allowed within readonly instance.");
                _displayName = value;
            }
        }

        /// <summary>
        /// Gets the description of the descripted Field. This value comes from the ContentTypeDefinition.
        /// </summary>
        public string Description
        {
            get
            {
                if (_description != null)
                    return _description;
                if (ParentFieldSetting != null)
                    return ParentFieldSetting.Description;
                return null;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting Description is not allowed within readonly instance.");
                _description = value;
            }
        }

        /// <summary>
        /// Gets the icon name of the descripted Field. This value comes from the ContentTypeDefinition.
        /// </summary>
        public string Icon
        {
            get
            {
                if (_icon != null)
                    return _icon;
                if (ParentFieldSetting != null)
                    return ParentFieldSetting.Icon;
                return null;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting Icon is not allowed within readonly instance.");
                _icon = value;
            }
        }

        private List<string> _bindings;
        /// <summary>
        /// Gets the property names of ContentHandler that are handled by the descripted Field. This value comes from the ContentTypeDefinition.
        /// </summary>
        public List<string> Bindings
        {
            get { return _bindings; }
            //private set { _bindings = value; }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting Bindings is not allowed within readonly instance.");
                _bindings = value;
            }
        }

        private ContentType _owner;
        /// <summary>
        /// Gets the owner ContentType declares or overrides the Field in the ContentTypeDefinition.
        /// </summary>
        public ContentType Owner
        {
            get { return _owner; }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting Owner is not allowed within readonly instance.");
                _owner = value;
            }
        }

        /// <summary>
        /// Returns the FieldSetting with the same name of the parent ContentType of the owner ContentType.
        /// </summary>
        /// <remarks>
        /// ContentTypes can inherit from one another but FieldSettings do not support inheritance.
        /// Therefore the parent of a FieldSetting means the FieldSetting of the parent ContentType of the owner ContentType.
        /// Visually:
        /// ContentType parent----ParentFieldSetting (e.g. <see cref="SenseNet.Portal.UI.Controls.ShortText">ShortText</see>)
        ///     ^
        ///     |
        ///     |
        /// ContentType owner-----FieldSetting (e.g. <see cref="SenseNet.Portal.UI.Controls.ShortText">ShortText</see>)
        /// </remarks>
        public FieldSetting ParentFieldSetting { get; internal set; }

        /// <summary>
        /// Gets the content of field's AppInfo element from ContentTypeDefinition XML.
        /// </summary>
        public string AppInfo
        {
            get
            {
                if (_appInfo != null)
                    return _appInfo;
                if (this.ParentFieldSetting == null)
                    return null;
                return this.ParentFieldSetting.AppInfo;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting AppInfo is not allowed within readonly instance.");
                _appInfo = value;
            }
        }

        public string FullName
        {
            get { return string.Format("{0}.{1}", Owner.Name, this.Name); }
        }

        public string BindingName
        {
            get { return GetBindingNameFromFullName(this.FullName); }
        }
        
        public int? FieldIndex
        {
            get { return _fieldIndex; }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting Index is not allowed within readonly instance.");
                if (value != null)
                {
                    _fieldIndex = value; 
                }
                else
                {
                    _fieldIndex = int.MaxValue;
                }
            }
        }

        /// <summary>
        /// Gets the type of the described Field's value
        /// </summary>
        public Type FieldDataType { get; private set; }

        internal int[] HandlerSlotIndices { get; private set; }

        internal Type[][] HandlerSlots
        {
            get { return FieldManager.GetHandlerSlots(this.ShortName); }
        }

        internal RepositoryDataType[] DataTypes
        {
            get { return FieldManager.GetDataTypes(this.ShortName); }
        }

        // Indexing control //////////////////////////////////////////////////

        internal PerFieldIndexingInfo IndexingInfo
        {
            get { return ContentTypeManager.GetPerFieldIndexingInfo(this.Name) ?? PerFieldIndexingInfo.DefaultInfo; }
        }
        protected virtual FieldIndexHandler CreateDefaultIndexFieldHandler()
        {
            return new LowerStringIndexHandler();
        }

        // Configured properties //////////////////////////////////////////////////

        internal bool PropertyIsReadOnly { get; set; }

        public bool ReadOnly
        {
            get
            {
                if (this.PropertyIsReadOnly)
                    return true;
                if (_configIsReadOnly != null)
                    return (bool)_configIsReadOnly;
                if (this.ParentFieldSetting == null)
                    return false;
                return this.ParentFieldSetting.ReadOnly;
            }
        }

        public bool? Compulsory
        {
            get
            {
                if (_required != null)
                    return (bool)_required;
                if (this.ParentFieldSetting == null)
                    return false;
                return this.ParentFieldSetting.Compulsory;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting Compulsory is not allowed within readonly instance.");
                _required = value;
            }
        }

        public OutputMethod OutputMethod
        {
            get
            {
                if (_outputMethod != null)
                    return (OutputMethod)_outputMethod;
                if (this.ParentFieldSetting == null)
                    return OutputMethod.Default;
                return this.ParentFieldSetting.OutputMethod;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting OutputMethod is not allowed within readonly instance.");
                _outputMethod = value == OutputMethod.Default ? (OutputMethod?)null : value;
            }
        }

        public string DefaultValue
        {
            get
            {
                return _defaultValue ?? (this.ParentFieldSetting == null ? null : 
                    this.ParentFieldSetting.DefaultValue);
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting FieldDataType is not allowed within readonly instance.");
                _defaultValue = value;
            }
        }

        [Obsolete("Visible property is obsolete. Please use one of the following values instead: VisibleBrowse, VisibleEdit, VisibleNew")]
        public bool Visible
        {
            get
            {
                if (_visible.HasValue)
                    return _visible.Value;

                return ParentFieldSetting == null || ParentFieldSetting.Visible;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting Visible is not allowed within readonly instance.");
                _visible = value;

                VisibleBrowse = value ? FieldVisibility.Show : FieldVisibility.Hide;
                VisibleEdit = value ? FieldVisibility.Show : FieldVisibility.Hide;
                VisibleNew = value ? FieldVisibility.Show : FieldVisibility.Hide;
            }
        }

        public FieldVisibility VisibleBrowse
        {
            get
            {
                if (_visibleBrowse.HasValue)
                    return _visibleBrowse.Value;

                return ParentFieldSetting == null ? FieldVisibility.Show : ParentFieldSetting.VisibleBrowse;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting VisibleBrowse is not allowed within readonly instance.");
                _visibleBrowse = value;
            }
        }

        public FieldVisibility VisibleEdit
        {
            get
            {
                if (_visibleEdit.HasValue)
                    return _visibleEdit.Value;

                return ParentFieldSetting == null ? FieldVisibility.Show : ParentFieldSetting.VisibleEdit;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting VisibleEdit is not allowed within readonly instance.");
                _visibleEdit = value;
            }
        }

        public FieldVisibility VisibleNew
        {
            get
            {
                if (_visibleNew.HasValue)
                    return _visibleNew.Value;

                return ParentFieldSetting == null ? FieldVisibility.Show : ParentFieldSetting.VisibleNew;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting VisibleNew is not allowed within readonly instance.");
                _visibleNew = value;
            }
        }

        public int DefaultOrder
        {
            get
            {
                if (_defaultOrder.HasValue)
                    return _defaultOrder.Value;

                return this.ParentFieldSetting == null ? 0 : this.ParentFieldSetting.DefaultOrder;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting DefaultOrder is not allowed within readonly instance.");
                _defaultOrder = value;
            }
        }

        public string ControlHint
        {
            get
            {
                return _controlHint ?? (ParentFieldSetting != null ? ParentFieldSetting.ControlHint : null);
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting ControlHint is not allowed within readonly instance.");
                _controlHint = value;
            }
        }

        internal Type GetHandlerSlot(int slotIndex)
        {
            if (this.HandlerSlotIndices == null)
                this.HandlerSlotIndices = new[] { 0 };

            return this.HandlerSlots[slotIndex][this.HandlerSlotIndices[slotIndex]];
        }

        // Constructors ///////////////////////////////////////////////////////////

        protected FieldSetting()
        {
            _mutable = true;
        }

        // Methods ////////////////////////////////////////////////////////////////

        public virtual void Initialize() { }

        protected virtual void ParseConfiguration(XPathNavigator configurationElement, IXmlNamespaceResolver xmlNamespaceResolver, ContentType contentType)
        {
        }
        protected virtual void SetDefaults()
        {
        }

        public virtual FieldValidationResult ValidateData(object value, Field field)
        {
            return FieldValidationResult.Successful;
        }

        public virtual IDictionary<string, FieldMetadata> GetFieldMetadata()
        {
            var defOrderFs = new IntegerFieldSetting
                                 {
                                     Name = DefaultOrderName,
                                     ShortName = "Integer",
                                     DisplayName = GetTitleString(DefaultOrderName),
                                     Description = GetDescString(DefaultOrderName),
                                     FieldClassName = typeof (IntegerField).FullName,
                                     DefaultValue = "0",
                                     Visible = false
                                 };

            return new Dictionary<string, FieldMetadata>
                {
                    {ShortNameName, new FieldMetadata
                        {
                            FieldName = ShortNameName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting =  new ShortTextFieldSetting
                                    {
                                        Name = ShortNameName,
                                        DisplayName = GetTitleString(ShortNameName),
                                        Description = GetDescString(ShortNameName),
                                        FieldClassName = typeof(ShortTextField).FullName,
                                        Visible = false
                                    }
                        }
                    }, 
                    {FieldClassNameName, new FieldMetadata
                        {
                            FieldName = FieldClassNameName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting =  new ShortTextFieldSetting
                                    {
                                        Name = FieldClassNameName,
                                        DisplayName = GetTitleString(FieldClassNameName),
                                        Description = GetDescString(FieldClassNameName),
                                        FieldClassName = typeof(ShortTextField).FullName,
                                        Visible = false
                                    }
                        }
                    },  
                    {OwnerName, new FieldMetadata
                        {
                            FieldName = OwnerName,
                            CanRead = true,
                            CanWrite = false,
                            FieldSetting =  new ReferenceFieldSetting
                                    {
                                        Name = OwnerName,
                                        DisplayName = GetTitleString(OwnerName),
                                        Description = GetDescString(OwnerName),
                                        FieldClassName = typeof(ReferenceField).FullName,
                                        AllowedTypes = new List<string> {"ContentType"},
                                        SelectionRoots = new List<string> { "/Root/System/Schema/ContentTypes" },
                                        Visible = false
                                    }
                        }
                    },  
                    {IconName, new FieldMetadata
                        {
                            FieldName = IconName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting =  new ShortTextFieldSetting
                                    {
                                        Name = IconName,
                                        DisplayName = GetTitleString(IconName),
                                        Description = GetDescString(IconName),
                                        FieldClassName = typeof(ShortTextField).FullName,
                                        Visible = false
                                    }
                        }
                    }, 
                    {AppInfoName, new FieldMetadata
                        {
                            FieldName = AppInfoName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting =  new ShortTextFieldSetting
                                    {
                                        Name = AppInfoName,
                                        DisplayName = GetTitleString(AppInfoName),
                                        Description = GetDescString(AppInfoName),
                                        FieldClassName = typeof(ShortTextField).FullName,
                                        Visible = false
                                    }
                        }
                    }, 
                    {DefaultValueName, new FieldMetadata
                        {
                            FieldName = DefaultValueName,
                            PropertyType = typeof(string),
                            FieldType = DynamicContentTools.GetSuggestedFieldType(typeof(string)),
                            DisplayName = GetTitleString(DefaultValueName),
                            Description = GetDescString(DefaultValueName),
                            CanRead = true,
                            CanWrite = true
                        }
                    }
                    , 
                    {ReadOnlyName, new FieldMetadata
                        {
                            FieldName = ReadOnlyName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting = new NullFieldSetting
                            {
                                Name = ReadOnlyName,
                                DisplayName = GetTitleString(ReadOnlyName),
                                Description = GetDescString(ReadOnlyName),
                                FieldClassName = typeof(BooleanField).FullName,
                                Visible = false
                            }
                        }
                    }
                    , 
                    {CompulsoryName, new FieldMetadata
                        {
                            FieldName = CompulsoryName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting = new NullFieldSetting
                            {
                                Name = CompulsoryName,
                                DisplayName = GetTitleString(CompulsoryName),
                                Description = GetDescString(CompulsoryName),
                                FieldClassName = typeof(BooleanField).FullName
                            }
                        }
                    }
                    , 
                    {OutputMethodName, new FieldMetadata
                        {
                            FieldName = OutputMethodName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting = new ChoiceFieldSetting
                            {
                                Name = OutputMethodName,
                                DisplayName = GetTitleString(OutputMethodName),
                                Description = GetDescString(OutputMethodName),
                                EnumTypeName = typeof(OutputMethod).FullName,
                                FieldClassName =  typeof(ChoiceField).FullName,
                                AllowMultiple = false,
                                AllowExtraValue = false,
                                DefaultValue = ((int)OutputMethod.Default).ToString(),
                                VisibleBrowse = FieldVisibility.Hide,
                                VisibleEdit = FieldVisibility.Hide,
                                VisibleNew = FieldVisibility.Hide
                            }
                        }
                    }
                    , 
                    {VisibleBrowseName, new FieldMetadata
                        {
                            FieldName = VisibleBrowseName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting = new ChoiceFieldSetting
                            {
                                Name = VisibleBrowseName,
                                DisplayName = GetTitleString(VisibleBrowseName),
                                Description = GetDescString(VisibleBrowseName),
                                EnumTypeName = typeof(FieldVisibility).FullName,
                                DisplayChoice = DisplayChoice.RadioButtons,
                                AllowMultiple = false,
                                AllowExtraValue = false,
                                DefaultValue = ((int)FieldVisibility.Show).ToString(),
                                FieldClassName = typeof(ChoiceField).FullName,
                            }
                        }
                    }
                    , 
                    {VisibleEditName, new FieldMetadata
                        {
                            FieldName = VisibleEditName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting = new ChoiceFieldSetting
                            {
                                Name = VisibleEditName,
                                DisplayName = GetTitleString(VisibleEditName),
                                Description = GetDescString(VisibleEditName),
                                EnumTypeName = typeof(FieldVisibility).FullName,
                                DisplayChoice = DisplayChoice.RadioButtons,
                                AllowMultiple = false,
                                AllowExtraValue = false,
                                DefaultValue = ((int)FieldVisibility.Show).ToString(),
                                FieldClassName = typeof(ChoiceField).FullName,
                            }
                        }
                    }
                    ,
                    {VisibleNewName, new FieldMetadata
                        {
                            FieldName = VisibleNewName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting = new ChoiceFieldSetting
                            {
                                Name = VisibleNewName,
                                DisplayName = GetTitleString(VisibleNewName),
                                Description = GetDescString(VisibleNewName),
                                EnumTypeName = typeof(FieldVisibility).FullName,
                                DisplayChoice = DisplayChoice.RadioButtons,
                                AllowMultiple = false,
                                AllowExtraValue = false,
                                DefaultValue = ((int)FieldVisibility.Show).ToString(),
                                FieldClassName = typeof(ChoiceField).FullName,
                            }
                        }
                    }
                    ,
                    {DefaultOrderName, new FieldMetadata
                        {
                            FieldName = DefaultOrderName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting = defOrderFs
                        }
                    }
                    , 
                    {AddToDefaultViewName, new FieldMetadata
                        {
                            FieldName = AddToDefaultViewName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting = new NullFieldSetting
                            {
                                        Name = AddToDefaultViewName,
                                        DisplayName = GetTitleString(AddToDefaultViewName),
                                        Description = GetDescString(AddToDefaultViewName),
                                        FieldClassName = typeof(BooleanField).FullName
                            }
                        }
                    }
                    , 
                    {FieldIndexName, new FieldMetadata
                        {
                            FieldName = FieldIndexName,
                            CanRead = true,
                            CanWrite = true,
                            FieldSetting = new IntegerFieldSetting()
                            {
                                        Name = FieldIndexName,
                                        DisplayName = GetTitleString(FieldIndexName),
                                        Description = GetDescString(FieldIndexName),
                                        FieldClassName = typeof(IntegerField).FullName
                            }
                        }
                    }
                };
        }

        public static string GetBindingNameFromFullName(string fullName)
        {
            return fullName.Replace('.', '_').Replace('#', '_');
        }

        public static FieldSetting GetFieldSettingFromFullName(string fullName, out string fieldName)
        {
            //fullName: "GenericContent.DisplayName", "ContentList.#ListField1", "Rating"
            var names = fullName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var typeName = names.Length == 2 ? names[0] : string.Empty;
            fieldName = names.Length == 1 ? names[0] : names[1];

            return (!string.IsNullOrEmpty(typeName) && !fieldName.StartsWith("#")) ? 
                ContentType.GetByName(typeName).GetFieldSettingByName(fieldName) : 
                null;
        }

        public static FieldSetting GetRoot(FieldSetting fieldSetting)
        {
            if (fieldSetting == null)
                return null;

            while (fieldSetting.ParentFieldSetting != null)
            {
                fieldSetting = fieldSetting.ParentFieldSetting;
            }

            return fieldSetting;
        }

        public FieldSetting GetEditable()
        {
            //the idea is to create an editable copy here, to prevent 
            //the user from manipulating the real schema objects
            var fieldSetting = FieldManager.CreateFieldSetting(this.GetType().FullName);
            fieldSetting.CopyPropertiesFrom(this);
            fieldSetting.Initialize();

            return fieldSetting;
        }

        protected virtual void CopyPropertiesFrom(FieldSetting source)
        {
            Name = source.Name;
            ShortName = source.ShortName;
            FieldClassName = source.FieldClassName;
            DisplayName = source.DisplayName;
            Description = source.Description;
            Icon = source.Icon;
            Owner = source.Owner;

            Bindings = new List<string>(source.Bindings);
            HandlerSlotIndices = new List<int>(source.HandlerSlotIndices).ToArray();

            AppInfo = source.AppInfo;
            Compulsory = source.Compulsory;
            OutputMethod = source.OutputMethod;
            DefaultValue = source.DefaultValue;
            PropertyIsReadOnly = source.ReadOnly;

            VisibleBrowse = source.VisibleBrowse;
            VisibleEdit = source.VisibleEdit;
            VisibleNew = source.VisibleNew;

            FieldIndex = source.FieldIndex;
        }

        public virtual object GetProperty(string name, out bool found)
        {
            object val = null;
            found = false;

            switch (name)
            {
                case DefaultValueName:
                    val = _defaultValue;
                    found = true;
                    break;
                case ReadOnlyName:
                    val = ReadOnly ? 1 : 0;
                    found = true;
                    break;
                case CompulsoryName:
                    val = Compulsory.HasValue ? (Compulsory.Value ? 1 : 0) : 0;
                    found = true;
                    break;
                case OutputMethodName:
                    found = true;
                    if (_outputMethod.HasValue)
                        val = (int)_outputMethod.Value;
                    break;
                case VisibleName:
                    val = Visible ? 1 : 0;
                    found = true;
                    break;
                case VisibleBrowseName:
                    found = true;
                    if (_visibleBrowse.HasValue)
                        val = (int)_visibleBrowse.Value;
                    break;
                case VisibleEditName:
                    found = true;
                    if (_visibleEdit.HasValue)
                        val = (int)_visibleEdit.Value;
                    break;
                case VisibleNewName:
                    found = true;
                    if (_visibleNew.HasValue)
                        val = (int)_visibleNew.Value;
                    break;
                case FieldIndexName:
                    found = true;
                    if (_fieldIndex.HasValue)
                        val = (int)_fieldIndex.Value;
                    break;
            }

            return found ? val : null;
        }

        public virtual bool SetProperty(string name, object value)
        {
            var found = false;

            switch (name)
            {
                case DefaultValueName:
                    if (value != null)
                        _defaultValue = value.ToString();
                    found = true;
                    break;
                case ReadOnlyName:
                    if (value != null)
                        _configIsReadOnly = (int)value == 1;
                    found = true;
                    break;
                case CompulsoryName:
                    if (value != null)
                        Compulsory = (int)value == 1;
                    found = true;
                    break;
                case OutputMethodName:
                    if (value != null)
                        OutputMethod = (OutputMethod)Convert.ToInt32(value);// (OutputMethod)value;
                    found = true;
                    break;
                case VisibleName:
                    if (value != null)
                        Visible = (int)value == 1;
                    found = true;
                    break;
                case VisibleBrowseName:
                    found = true;
                    if (value != null)
                        _visibleBrowse = (FieldVisibility)Convert.ToInt32(value);
                    break;
                case VisibleEditName:
                    found = true;
                    if (value != null)
                        _visibleEdit = (FieldVisibility)Convert.ToInt32(value);
                    break;
                case VisibleNewName:
                    found = true;
                    if (value != null)
                        _visibleNew = (FieldVisibility)Convert.ToInt32(value);
                    break;
                case FieldIndexName:
                    found = true;
                    if (value != null)
                    {
                        _fieldIndex = Convert.ToInt32(value);
                    }
                    else
                    {
                        _fieldIndex = int.MaxValue;
                    }
                    break;
            }

            return found;
        }

        protected void ParseEnumValue<T>(string value, ref T? member) where T : struct 
        {
            if (string.IsNullOrEmpty(value))
                return;

            member = (T?)Enum.Parse(typeof(T), value);
        }

        // Internals //////////////////////////////////////////////////////////////
        internal static FieldSetting Create(FieldDescriptor fieldDescriptor)
        {
            //-- for ContentType
            return Create(fieldDescriptor, null);
        }
        internal static FieldSetting Create(FieldDescriptor fieldDescriptor, List<string> bindings)
        {
            //-- for ContentList if bindings is not null
            var fieldSettingTypeName = string.IsNullOrEmpty(fieldDescriptor.FieldSettingTypeName)
                                           ? FieldManager.GetDefaultFieldSettingTypeName(fieldDescriptor.FieldTypeName)
                                           : fieldDescriptor.FieldSettingTypeName;
            var product = FieldManager.CreateFieldSetting(fieldSettingTypeName);

            SetProperties(product, fieldDescriptor, bindings);

            return product;
        }

        private void Parse(XPathNavigator nav, IXmlNamespaceResolver nsres, ContentType contentType)
        {
            Reset();
            if (nav == null)
                return;

            var iter = nav.Select(string.Concat("x:", ReadOnlyName), nsres);
            _configIsReadOnly = iter.MoveNext() ? (bool?) (iter.Current.InnerXml != "false") : null;

            iter = nav.Select(string.Concat("x:", CompulsoryName), nsres);
            _required = iter.MoveNext() ? (bool?)(iter.Current.InnerXml != "false") : null;

            iter = nav.Select(string.Concat("x:", OutputMethodName), nsres);
            _outputMethod = iter.MoveNext() ? (OutputMethod?)Enum.Parse(typeof(SenseNet.ContentRepository.Schema.OutputMethod), iter.Current.InnerXml, true) : null;

            iter = nav.Select(string.Concat("x:", DefaultValueName), nsres);
            _defaultValue = iter.MoveNext() ? iter.Current.Value : null;

            iter = nav.Select(string.Concat("x:", DefaultOrderName), nsres);
            _defaultOrder = iter.MoveNext() ? (int?) iter.Current.ValueAsInt : null;

            iter = nav.Select(string.Concat("x:", VisibleName), nsres);
            _visible = iter.MoveNext() ? (bool?)iter.Current.ValueAsBoolean : null;

            iter = nav.Select(string.Concat("x:", VisibleBrowseName), nsres);
            _visibleBrowse = iter.MoveNext() ? ParseVisibleValue(iter.Current.Value) : null;

            iter = nav.Select(string.Concat("x:", VisibleEditName), nsres);
            _visibleEdit = iter.MoveNext() ? ParseVisibleValue(iter.Current.Value) : null;

            iter = nav.Select(string.Concat("x:", VisibleNewName), nsres);
            _visibleNew = iter.MoveNext() ? ParseVisibleValue(iter.Current.Value) : null;

            iter = nav.Select(string.Concat("x:", ControlHintName), nsres);
            _controlHint = iter.MoveNext() ? iter.Current.Value : null;

            iter = nav.Select(string.Concat("x:", FieldIndexName), nsres);
            _fieldIndex = iter.MoveNext() ? (int?) iter.Current.ValueAsInt : null;

            ParseConfiguration(nav, nsres, contentType);
        }

        private static FieldVisibility? ParseVisibleValue(string visibleValue)
        {
            if (string.IsNullOrEmpty(visibleValue))
                return null;

            return (FieldVisibility)Enum.Parse(typeof(FieldVisibility), visibleValue);
        }

        internal void Modify(FieldDescriptor fieldDescriptor)
        {
            SetProperties(this, fieldDescriptor, null);
        }
        private static void SetProperties(FieldSetting setting, FieldDescriptor descriptor, List<string> bindings)
        {
            setting.Owner = descriptor.Owner;
            setting.Name = descriptor.FieldName;
            setting.ShortName = descriptor.FieldTypeShortName;
            setting.FieldClassName = descriptor.FieldTypeName;
            setting._displayName = descriptor.DisplayName;
            setting._description = descriptor.Description;
            setting._icon = descriptor.Icon;
            setting.Bindings = bindings ?? descriptor.Bindings;
            setting.HandlerSlotIndices = new int[descriptor.Bindings.Count];
            setting.FieldDataType = FieldManager.GetFieldDataType(descriptor.FieldTypeName);

            setting.Parse(descriptor.ConfigurationElement, descriptor.XmlNamespaceResolver, descriptor.Owner);

            if (descriptor.AppInfo != null)
                setting._appInfo = descriptor.AppInfo.InnerXml;

            var indexingInfo = new PerFieldIndexingInfo();

            if (!String.IsNullOrEmpty(descriptor.IndexingMode))
            {
                switch (descriptor.IndexingMode)
                {
                    case "Analyzed": indexingInfo.IndexingMode = LucField.Index.ANALYZED; break;
                    case "AnalyzedNoNorms": indexingInfo.IndexingMode = LucField.Index.ANALYZED_NO_NORMS; break;
                    case "No": indexingInfo.IndexingMode = LucField.Index.NO; break;
                    case "NotAnalyzed": indexingInfo.IndexingMode = LucField.Index.NOT_ANALYZED; break;
                    case "NotAnalyzedNoNorms": indexingInfo.IndexingMode = LucField.Index.NOT_ANALYZED_NO_NORMS; break;
                    default: throw new ContentRegistrationException("Invalid IndexingMode: " + descriptor.IndexingMode, descriptor.Owner.Name, descriptor.FieldName);
                }
            }
            if (!String.IsNullOrEmpty(descriptor.IndexStoringMode))
            {
                switch (descriptor.IndexStoringMode)
                {
                    case "No": indexingInfo.IndexStoringMode = LucField.Store.NO; break;
                    case "Yes": indexingInfo.IndexStoringMode = LucField.Store.YES; break;
                    default: throw new ContentRegistrationException("Invalid IndexStoringMode: " + descriptor.IndexStoringMode, descriptor.Owner.Name, descriptor.FieldName);
                }
            }
            if (!String.IsNullOrEmpty(descriptor.IndexingTermVector))
            {
                switch (descriptor.IndexingTermVector)
                {
                    case "No": indexingInfo.TermVectorStoringMode = LucField.TermVector.NO; break;
                    case "WithOffsets": indexingInfo.TermVectorStoringMode = LucField.TermVector.WITH_OFFSETS; break;
                    case "WithPositions": indexingInfo.TermVectorStoringMode = LucField.TermVector.WITH_POSITIONS; break;
                    case "WithPositionsOffsets": indexingInfo.TermVectorStoringMode = LucField.TermVector.WITH_POSITIONS_OFFSETS; break;
                    case "Yes": indexingInfo.TermVectorStoringMode = LucField.TermVector.YES; break;
                    default: throw new ContentRegistrationException("Invalid IndexingTermVector: " + descriptor.IndexingTermVector, descriptor.Owner.Name, descriptor.FieldName);
                }
            }

            indexingInfo.Analyzer = descriptor.Analyzer;
            indexingInfo.IndexFieldHandler = GetIndexFieldHandler(descriptor.IndexHandlerTypeName, setting);
            indexingInfo.IndexFieldHandler.OwnerIndexingInfo = indexingInfo;

            ContentTypeManager.SetPerFieldIndexingInfo(setting.Name, setting.Owner.Name, indexingInfo);
        }
        private static FieldIndexHandler GetIndexFieldHandler(string typeName, FieldSetting fieldSetting)
        {
            if (typeName == null)
                return fieldSetting.CreateDefaultIndexFieldHandler();
            var type = Storage.TypeHandler.GetType(typeName);
            return (FieldIndexHandler)Activator.CreateInstance(type);
        }

        public IEnumerable<string> GetValueForQuery(Field field)
        {
            return IndexingInfo.IndexFieldHandler.GetParsableValues(field);
        }

        internal FieldValidationResult Validate(object value, Field field)
        {
            if (((value == null) || (String.IsNullOrEmpty(value.ToString()))) && (this.Compulsory ?? false))
                return new FieldValidationResult(CompulsoryName);
            return ValidateData(value, field);
        }

        private void Reset()
        {
            SetDefaults();
            _configIsReadOnly = false;
            _required = false;
        }

        public string EvaluateDefaultValue()
        {
            var defaultValue = DefaultValue;
            return defaultValue == null ? null : Evaluator.Evaluate(defaultValue);
        }

        public void WriteXml(XmlWriter writer)
        {
            var isListField = this.Name[0] == '#';
            var elementName = isListField ? "ContentListField" : "Field";
            
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString("name", this.Name);

            WriteAttribute(writer, this._shortName, "type");

            //write handlername only if there is no shortname info
            if (string.IsNullOrEmpty(this._shortName))
                WriteAttribute(writer, this._fieldClassName, "handler");

            WriteElement(writer, this._displayName, "DisplayName");
            WriteElement(writer, this._description, "Description");
            WriteElement(writer, this._icon, "Icon");
            WriteElement(writer, this._appInfo, "AppInfo");

            if (!isListField)
            {
                WriteBinding(writer);
            }

            WriteConfigurationFrame(writer);

            writer.WriteEndElement();
            writer.Flush();
        }

        public string ToXml()
        {
            var sw = new StringWriter();
            using (var writer = XmlWriter.Create(sw, new XmlWriterSettings() { OmitXmlDeclaration = true } ))
            {
                this.WriteXml(writer);
            }

            return sw.ToString();
        }

        private void WriteConfigurationFrame(XmlWriter writer)
        {
            writer.WriteStartElement("Configuration");

            WriteElement(writer, this._configIsReadOnly, ReadOnlyName);
            WriteElement(writer, this._required, CompulsoryName);
            if(_outputMethod.HasValue && _outputMethod.Value != Schema.OutputMethod.Default)
                WriteElement(writer, this._outputMethod.ToString(), OutputMethodName);
            WriteElement(writer, this._defaultValue, DefaultValueName);
            WriteElement(writer, this._defaultOrder, DefaultOrderName);
            WriteElement(writer, this._visible, VisibleName);

            if (_visibleBrowse.HasValue)
                WriteElement(writer, _visibleBrowse.Value.ToString(), VisibleBrowseName);
            if (_visibleEdit.HasValue)
                WriteElement(writer, _visibleEdit.Value.ToString(), VisibleEditName);
            if (_visibleNew.HasValue)
                WriteElement(writer, _visibleNew.Value.ToString(), VisibleNewName);
            if (_fieldIndex.HasValue)
                WriteElement(writer, _fieldIndex.Value.ToString(), FieldIndexName);

            WriteElement(writer, this._controlHint, ControlHintName);

            this.WriteConfiguration(writer);

            writer.WriteEndElement();
        }

        protected abstract void WriteConfiguration(XmlWriter writer);

        protected void WriteElement(XmlWriter writer, bool? value, string elementName)
        {
            if (value.HasValue)
                WriteElement(writer, (bool)value ? "true" : "false", elementName);
        }

        protected void WriteElement(XmlWriter writer, int? value, string elementName)
        {
            if (value.HasValue)
                WriteElement(writer, value.ToString(), elementName);
        }

        protected void WriteElement(XmlWriter writer, decimal? value, string elementName)
        {
            if (value.HasValue)
                WriteElement(writer, XmlConvert.ToString(value.Value), elementName);
        }

        protected void WriteElement(XmlWriter writer, string value, string elementName)
        {
            if (value == null) 
                return;

            writer.WriteStartElement(elementName);
            writer.WriteString(value);
            writer.WriteEndElement();
        }

        protected void WriteAttribute(XmlWriter writer, string value, string attributeName)
        {
            if (string.IsNullOrEmpty(value))
                return;

            writer.WriteAttributeString(attributeName, value);
        }

        protected void WriteBinding(XmlWriter writer)
        {
            if (this.Bindings == null || this.Bindings.Count == 0 || 
                (this.Bindings.Count == 1 && this.Bindings[0].CompareTo(this.Name) == 0)) 
                return;

            foreach (var binding in this.Bindings)
            {
                writer.WriteStartElement("Bind");
                writer.WriteAttributeString("property", binding);
                writer.WriteEndElement();
            }
        }

        protected string GetTitleString(string resName)
        {
            return GetString("FieldTitle_" + resName);
        }

        protected string GetDescString(string resName)
        {
            return GetString("FieldDesc_" + resName);
        }

        protected string GetString(string resName)
        {
            return HttpContext.GetGlobalResourceObject("FieldEditor", resName) as string;
        }
    }
}
