using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Schema;
using  SenseNet.ContentRepository.Schema;
using System.Collections;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Portal.UI.Controls;
using System.Web.UI.WebControls;

using SNC = SenseNet.ContentRepository;
using SenseNet.ContentRepository.Tests.ContentHandler;

namespace SenseNet.ContentRepository.Tests
{
	#region Accessors
	internal class Accessor
	{
		protected object _target;
		protected Type _type;

		public Accessor(object target)
		{
			_target = target;
			_type = target.GetType();
		}

		protected object GetInternalValue(string name)
		{
			PropertyInfo info = _type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
			return info.GetValue(_target, null);
		}
		protected void SetInternalValue(string name, object value)
		{
			PropertyInfo info = _type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
			info.SetValue(_target, value, null);
		}
		protected object GetPublicValue(string name)
		{
			PropertyInfo info = _type.GetProperty(name);
			return info.GetValue(_target, null);
		}
		protected void SetPublicValue(string name, object value)
		{
			PropertyInfo info = _type.GetProperty(name);
			info.SetValue(_target, value, null);
		}
		protected object CallPrivateMethod(string name, params object[] parameters)
		{
			//MethodInfo info = null;
			//if (parameters.Length == 0)
			//{
			//    info = _type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
			//}
			//else
			//{
			//    Type[] paramTypes = new Type[parameters.Length];
			//    for (int i = 0; i < parameters.Length; i++)
			//        paramTypes[i] = parameters[i].GetType();
			//    info = _type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);
			//}
			//return info.Invoke(_target, parameters);

			Type[] paramTypes = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
				paramTypes[i] = parameters[i].GetType();
			return CallPrivateMethod(name, paramTypes, parameters);
		}
		protected object CallPrivateMethod(string name, Type[] paramTypes, object[] parameters)
		{
			MethodInfo info = null;
			if (parameters.Length == 0)
				info = _type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
			else
				info = _type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);
			System.Diagnostics.Debug.Assert(info != null, "Private method not found: " + name);
			return info.Invoke(_target, parameters);
		}
		protected object CallPrivateStaticMethod(string name, Type[] paramTypes, params object[] parameters)
		{
			return CallPrivateStaticMethod(_type, name, paramTypes, parameters);
		}
		protected static object CallPrivateStaticMethod(Type targetType, string name, Type[] paramTypes, params object[] parameters)
		{
			//MethodInfo info = _type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
			foreach (MethodInfo info in targetType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
			{
				ParameterInfo[] paramInfos = info.GetParameters();
				if (info.Name == name)
				{
					if (paramInfos.Length == paramTypes.Length)
					{
						bool ok = true;
						for (int i = 0; i < paramInfos.Length; i++)
						{
							//if (paramInfos[i].ParameterType != paramTypes[i])
							//{
							if (!paramInfos[i].ParameterType.IsAssignableFrom(paramTypes[i]))
							{
								ok = false;
								break;
							}
							//}
						}
						if (ok)
							return info.Invoke(null, parameters);
					}
				}
			}
			throw new ApplicationException("Method not found in a test");
		}
		protected object GetPrivateField(string name)
		{
			FieldInfo fi = null;
			Type type = _type;
			while (type != null && fi == null)
			{
				fi = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
				type = type.BaseType;
			}
			return fi.GetValue(_target);
		}
		protected void SetPrivateField(string name, object value)
		{
			FieldInfo fi = null;
			Type type = _type;
			while (type != null && fi == null)
			{
				fi = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
				type = type.BaseType;
			}
			fi.SetValue(_target, value);
		}

	}
	internal class SchemaEditorAccessor : Accessor
	{
		public SchemaEditorAccessor(SchemaEditor target) : base(target) { }
		public void RegisterSchema(SchemaEditor origSchema, SchemaWriter writer)
		{
			//private void RegisterSchema(SchemaEditor origSchema, SchemaEditor newSchema, SchemaWriter schemaWriter)
			CallPrivateStaticMethod("RegisterSchema", new Type[] { typeof(SchemaEditor), typeof(SchemaEditor), typeof(SchemaWriter) }, origSchema, _target, writer);
		}
	}
	internal class SchemaItemAccessor : Accessor
	{
		public SchemaItemAccessor(SchemaItem target) : base(target) { }
		public int Id
		{
			get { return ((SchemaItem)_target).Id; }
			set { SetPrivateField("_id", value); }
		}
	}
	//internal class ContentTypeAccessor : Accessor
	//{
	//    public ContentTypeAccessor(ContentType target) : base(target) { }
	//    public ContentType LoadOrCreateNew(string contentTypeDefinitionXml)
	//    {
	//        return (ContentType)CallPrivateStaticMethod("LoadOrCreateNew", contentTypeDefinitionXml);
	//    }
	//    public void ApplyChangesInEditor(ContentType settings, SchemaEditor editor)
	//    {
	//        base.CallPrivateStaticMethod("ApplyChangesInEditor", settings, editor);
	//    }
	//    public NodeTypeRegistrationAccessor ParseAttributes(Type type)
	//    {
	//        object ntr = base.CallPrivateStaticMethod("ParseAttributes", type);
	//        if (ntr == null)
	//            return null;
	//        NodeTypeRegistrationAccessor ntrAcc = new NodeTypeRegistrationAccessor(ntr);
	//        return ntrAcc;
	//    }
	//}
	internal class ContentTypeManagerAccessor : Accessor
	{
		public ContentTypeManagerAccessor(ContentTypeManager target) : base(target) { }
		public ContentType LoadOrCreateNew(string contentTypeDefinitionXml)
		{
			return (ContentType)CallPrivateStaticMethod("LoadOrCreateNew", new Type[] { typeof(string) }, contentTypeDefinitionXml);
		}
		public void ApplyChangesInEditor(ContentType settings, SchemaEditor editor)
		{
			base.CallPrivateStaticMethod("ApplyChangesInEditor", new Type[] { typeof(ContentType), typeof(SchemaEditor) }, settings, editor);
		}
		public NodeTypeRegistrationAccessor ParseAttributes(Type type)
		{
			object ntr = base.CallPrivateStaticMethod("ParseAttributes", new Type[] { typeof(Type) }, type);
			if (ntr == null)
				return null;
			NodeTypeRegistrationAccessor ntrAcc = new NodeTypeRegistrationAccessor(ntr);
			return ntrAcc;
		}
	}
	internal class NodeTypeRegistrationAccessor : Accessor
	{
		List<PropertyTypeRegistrationAccessor> _propertyTypeRegistrations;

		public Type Type
		{
			get { return (Type)GetPublicValue("Type"); }
			set { SetPublicValue("Type", value); }
		}
		public string Name
		{
			get { return (string)GetPublicValue("Name"); }
		}
		public string ParentName
		{
			get { return (string)GetPublicValue("ParentName"); }
			set { SetPublicValue("ParentName", value); }
		}
		public List<PropertyTypeRegistrationAccessor> PropertyTypeRegistrations
		{
			get { return _propertyTypeRegistrations; }
		}

		public NodeTypeRegistrationAccessor(object target)
			: base(target)
		{
			_propertyTypeRegistrations = new List<PropertyTypeRegistrationAccessor>();
			object listobject = GetPublicValue("PropertyTypeRegistrations");
			IEnumerable list = listobject as IEnumerable;
			foreach (object item in list)
			{
				PropertyTypeRegistrationAccessor p = new PropertyTypeRegistrationAccessor(item);
				p.Parent = this;
				_propertyTypeRegistrations.Add(p);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("NTR: name="); sb.Append(this.Name).Append(", ParentName=").Append(this.ParentName ?? "[null]");
			sb.Append(", Type=").Append(this.Type.FullName ?? "[null]").Append("\r\n");
			foreach (PropertyTypeRegistrationAccessor ptrAcc in _propertyTypeRegistrations)
				sb.Append("\t").Append(ptrAcc).Append("\r\n");
			return sb.ToString();
		}
	}
	internal class PropertyTypeRegistrationAccessor : Accessor
	{
		private NodeTypeRegistrationAccessor _parent;

		public string Name
		{
			get { return (string)GetPublicValue("Name"); }
		}
		public RepositoryDataType DataType
		{
			get { return (RepositoryDataType)GetPublicValue("DataType"); }
			set { SetPublicValue("DataType", value); }
		}
		public bool IsDeclared
		{
			get { return (bool)GetPublicValue("IsDeclared"); }
			set { SetPublicValue("IsDeclared", value); }
		}
		public NodeTypeRegistrationAccessor Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		public PropertyTypeRegistrationAccessor(object target) : base(target) { }

		public override string ToString()
		{
			return String.Concat("PTR: name=", this.Name, ", DataType=", this.DataType, ", IsDeclared=", this.IsDeclared);
		}
	}
	internal class TypeCollectionAccessor<T> : Accessor, IEnumerable<T> where T : SchemaItem
	{
		public TypeCollection<T> Target { get { return (TypeCollection<T>)_target; } }

		public static TypeCollectionAccessor<T> Create(SchemaEditor schemaEditor)
		{
			PrivateType prt = new PrivateType("SenseNet.Storage", "SenseNet.ContentRepository.Storage.Schema.ISchemaRoot");
			Type argType = prt.ReferencedType;
			PrivateObject pro = new PrivateObject(typeof(TypeCollection<T>), new Type[] { argType }, new object[] { schemaEditor });
			return new TypeCollectionAccessor<T>(pro.Target);
		}
		public static TypeCollectionAccessor<T> Create(TypeCollection<T> initialList)
		{
			PrivateObject pro = new PrivateObject(typeof(TypeCollection<T>), new Type[] { typeof(TypeCollection<T>) }, new object[] { initialList });
			return new TypeCollectionAccessor<T>(pro.Target);
		}

		public TypeCollectionAccessor(object target) : base(target) { }

		public void Add(T item)
		{
			CallPrivateMethod("Add", item);
		}
		public int Count
		{
			get { return Target.Count; }
		}
		public T this[string name]
		{
			get { return Target[name]; }
			set
			{
				var indexerInfo = Target.GetType().GetMethod("set_Item", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string), typeof(T) }, null);
				indexerInfo.Invoke(_target, new object[] { name, value });
			}
		}
		public T this[int index]
		{
			get { return Target[index]; }
			set
			{
				var indexerInfo = Target.GetType().GetMethod("set_Item", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(int), typeof(T) }, null);
				indexerInfo.Invoke(_target, new object[] { index, value });
			}
		}

		public bool Contains(T item)
		{
			return Target.Contains(item);
		}
		public int IndexOf(T item)
		{
			return Target.IndexOf(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Target.CopyTo(array, arrayIndex);
		}
		public T[] CopyTo(int arrayIndex)
		{
			return Target.CopyTo(arrayIndex);
		}
		public T[] ToArray()
		{
			return Target.ToArray();
		}
		public int[] ToIdArray()
		{
			return Target.ToIdArray();
		}
		public string[] ToNameArray()
		{
			return Target.ToNameArray();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Target.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return Target.GetEnumerator();
		}

		public T GetItemById(int itemId)
		{
			return Target.GetItemById(itemId);
		}

		public void AddRange(TypeCollection<T> items)
		{
			CallPrivateMethod("AddRange", items);
		}
		public void Insert(int index, T item)
		{
			CallPrivateMethod("Insert", index, item);
		}
		public void Clear()
		{
			CallPrivateMethod("Clear");
		}
		internal bool Remove(T item)
		{
			return (bool)CallPrivateMethod("Remove", item);
		}
		internal void RemoveAt(int index)
		{
			CallPrivateMethod("RemoveAt", index);
		}

	}
	internal class BinaryEditorAccessor : Accessor
	{
		public BinaryEditorAccessor(Binary target) : base(target) { }
		public TextBox TextBox
		{
			get { return base.GetPrivateField("_textBox") as TextBox; }
		}
		public void OnInit()
		{
			base.CallPrivateMethod("OnInit", EventArgs.Empty);
		}
		public void SetContent(SNC.Content content)
		{
			base.SetPrivateField("_content", content);
		}
		public void ConnectToField(Field field)
		{
			base.SetPrivateField("_field", field);
			((Binary)_target).SetData(field.GetData());
		}
	}

	internal class FieldControlAccessor : Accessor
	{
		public FieldControlAccessor(FieldControl target) : base(target) { }
		public void ConnectToField(Field field)
		{
			base.SetPrivateField("_field", field);
			((FieldControl)_target).SetData(field.GetData());
		}
		public void SetDataInternal()
		{
			CallPrivateMethod("SetDataInternal");
		}
	}
	internal class ShortTextAccessor : FieldControlAccessor
	{
		public ShortTextAccessor(ShortText target) : base(target) { }
		public String Text
		{
			get { return base.GetPrivateField("_text") as string; }
		}
		public TextBox InputTextBox
		{
            get { return base.GetPrivateField("_shortTextBox") as TextBox; }
		}
	}
	internal class PasswordAccessor : FieldControlAccessor
	{
		public PasswordAccessor(Password target) : base(target) { }
		public TextBox InputTextBox
		{
            get { return base.GetPrivateField("_pwdTextBox1") as TextBox; }
		}
		public TextBox InputTextBox2
		{
            get { return base.GetPrivateField("_pwdTextBox2") as TextBox; }
		}
	}
	internal class WhoAndWhenAccessor : FieldControlAccessor
	{
		public WhoAndWhenAccessor(WhoAndWhen target) : base(target) { }
		//public Label Label1
		//{
		//    get { return base.GetPrivateField("label1") as Label; }
		//}
		//public Label Label2
		//{
		//    get { return base.GetPrivateField("label2") as Label; }
		//}
	}
	internal class ColorControlAccessor : Accessor
	{
		public ColorControlAccessor(ColorEditorControl target) : base(target) { }
		//public Label Label1
		//{
		//    get { return base.GetPrivateField("label1") as Label; }
		//}
		//public Label Label2
		//{
		//    get { return base.GetPrivateField("label2") as Label; }
		//}
		public void ConnectToField(Field field)
		{
			base.SetPrivateField("_field", field);
			((ColorEditorControl)_target).SetData(field.GetData());
		}
	}
	#endregion

	#region Mock: TestSchemaWriter
	internal class TestSchemaWriter : SchemaWriter
	{
		StringBuilder _log;

		public string Log
		{
			get { return _log.ToString(); }
		}

		public TestSchemaWriter()
		{
			_log = new StringBuilder();
		}

		public override void Open()
		{
			WriteLog(MethodInfo.GetCurrentMethod());
		}
		public override void Close()
		{
			WriteLog(MethodInfo.GetCurrentMethod());
		}

		public override void CreatePropertyType(string name, DataType dataType, int mapping, bool isContentListProperty)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), name, dataType, mapping, isContentListProperty);
		}
		public override void DeletePropertyType(PropertyType propertyType)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), propertyType.Name);
		}

		public override void CreateNodeType(NodeType parent, string name, string className)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), parent == null ? "[null]" : parent.Name, name, className);
		}
		public override void ModifyNodeType(NodeType nodeType, NodeType parent, string className)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), nodeType.Name, parent == null ? "[null]" : parent.Name, className);
		}
		public override void DeleteNodeType(NodeType nodeType)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), nodeType.Name);
		}
        public override void CreateContentListType(string name)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), name);
		}
        public override void DeleteContentListType(ContentListType contentListType)
		{
            WriteLog(MethodInfo.GetCurrentMethod(), contentListType.Name);
		}
		public override void AddPropertyTypeToPropertySet(PropertyType propertyType, PropertySet owner, bool isDeclared)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), propertyType.Name, owner.Name, isDeclared);
		}
		public override void RemovePropertyTypeFromPropertySet(PropertyType propertyType, PropertySet owner)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), propertyType.Name, owner.Name);
		}
		public override void UpdatePropertyTypeDeclarationState(PropertyType propType, NodeType newSet, bool isDeclared)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), propType.Name, newSet.Name, isDeclared);
		}

		public override void CreatePermissionType(string name)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), name);
		}
		public override void DeletePermissionType(PermissionType permissionType)
		{
			WriteLog(MethodInfo.GetCurrentMethod(), permissionType.Name);
		}

		private void WriteLog(MethodBase methodBase, params object[] prms)
		{
			_log.Append(methodBase.Name).Append("(");
			ParameterInfo[] prmInfos = methodBase.GetParameters();
			for (int i = 0; i < prmInfos.Length; i++)
			{
				if (i > 0)
					_log.Append(", ");

				_log.Append(prmInfos[i].Name).Append("=<");
				_log.Append(prms[i]).Append(">");
			}
			_log.Append(");").Append("\r\n");
		}

	}
	#endregion
}