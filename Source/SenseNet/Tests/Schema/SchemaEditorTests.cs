using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using System.Reflection;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Data.SqlClient;
using System.Xml;
using System.Xml.XPath;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage.Search;
using System.Reflection.Emit;
using System.Collections;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Tests.Schema
{
	#region Accessors
	//internal class SchemaEditorAccessor : Accessor
	//{
	//    public SchemaEditorAccessor(SchemaEditor target) : base(target) { }
	//    public void RegisterSchema(SchemaEditor origSchema, SchemaWriter writer)
	//    {
	//        //private void RegisterSchema(SchemaEditor origSchema, SchemaEditor newSchema, SchemaWriter schemaWriter)
	//        CallPrivateStaticMethod("RegisterSchema", origSchema, _target, writer);
	//    }
	//}
	//internal class SchemaItemAccessor : Accessor
	//{
	//    public SchemaItemAccessor(SchemaItem target) : base(target) { }
	//    public int Id
	//    {
	//        get { return ((SchemaItem)_target).Id; }
	//        set { SetPrivateField("_id", value); }
	//    }
	//}
	#endregion

	[TestClass]
    public class SchemaEditorTests : TestBase
	{
		#region Test Infrastructure
		private TestContext testContextInstance;

		public override TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}
		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion
		#endregion

		//============================================================================== Simple tests

		[TestMethod]
		public void SchEd_WriterCalling_CreatePropertyType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- edit
			ed2.CreatePropertyType("PT1", DataType.String);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();CreatePropertyType(name=<PT1>, dataType=<String>, mapping=<0>, isContentListProperty=<False>);Close();");
		}
		[TestMethod]
		public void SchEd_WriterCalling_DeletePropertyType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed1.PropertyTypes["PT1"], 1);
			//-- create current
			ed2.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed2.PropertyTypes["PT1"], 1);

			//-- edit
			ed2.DeletePropertyType(ed2.PropertyTypes["PT1"]);
			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();DeletePropertyType(propertyType=<PT1>);Close();");
		}
		[TestMethod]
		public void SchEd_WriterCalling_ReCreatePropertyType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed1.PropertyTypes["PT1"], 1);
			//-- create current
			ed2.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed2.PropertyTypes["PT1"], 1);

			//-- edit
			ed2.DeletePropertyType(ed2.PropertyTypes["PT1"]);
			ed2.CreatePropertyType("PT1", DataType.String);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();CreatePropertyType(name=<PT1>, dataType=<String>, mapping=<0>, isContentListProperty=<False>);DeletePropertyType(propertyType=<PT1>);Close();");
		}

		[TestMethod]
		public void SchEd_WriterCalling_CreatePermissionType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original

			//-- create current

			//-- edit
			ed2.CreatePermissionType("P1");

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();CreatePermissionType(name=<P1>);Close();");
		}
		[TestMethod]
		public void SchEd_WriterCalling_DeletePermissionType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreatePermissionType("P1");
			SetSchemaItemId(ed1.PermissionTypes["P1"], 1);
			//-- create current
			ed2.CreatePermissionType("P1");
			SetSchemaItemId(ed2.PermissionTypes["P1"], 1);

			//-- edit
			ed2.DeletePermissionType(ed2.PermissionTypes["P1"]);
			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();DeletePermissionType(permissionType=<P1>);Close();");
		}
		[TestMethod]
		public void SchEd_WriterCalling_ReCreatePermissionType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreatePermissionType("P1");
			SetSchemaItemId(ed1.PermissionTypes["P1"], 1);
			//-- create current
			ed2.CreatePermissionType("P1");
			SetSchemaItemId(ed2.PermissionTypes["P1"], 1);

			//-- edit
			ed2.DeletePermissionType(ed2.PermissionTypes["P1"]);
			ed2.CreatePermissionType("P1");

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();DeletePermissionType(permissionType=<P1>);CreatePermissionType(name=<P1>);Close();");
		}

		[TestMethod]
		public void SchEd_WriterCalling_CreateNodeType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed1.PropertyTypes["PT1"], 1);
			//-- create current
			ed2.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed2.PropertyTypes["PT1"], 1);

			//-- edit
			ed2.CreateNodeType(null, "NT1");
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT1"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string expectedLog = @"
				Open();
				CreateNodeType(parent=<[null]>, name=<NT1>, className=<>);
				AddPropertyTypeToPropertySet(propertyType=<PT1>, owner=<NT1>, isDeclared=<True>);
				Close();
				".Replace("\r\n", "").Replace("\t", "");
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == expectedLog);
		}
		[TestMethod]
		public void SchEd_WriterCalling_ModifyNodeType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed1.PropertyTypes["PT1"], 1);
			ed1.CreateNodeType(null, "NT1");
			SetSchemaItemId(ed1.NodeTypes["NT1"], 1);
			ed1.AddPropertyTypeToPropertySet(ed1.PropertyTypes["PT1"], ed1.NodeTypes["NT1"]);
			//-- create current
			ed2.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed2.PropertyTypes["PT1"], 1);
			ed2.CreateNodeType(null, "NT1");
			SetSchemaItemId(ed2.NodeTypes["NT1"], 1);
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT1"]);

			//-- edit
			ed2.ModifyNodeType(ed2.NodeTypes["NT1"], "ClassName2");

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();ModifyNodeType(nodeType=<NT1>, parent=<[null]>, className=<ClassName2>);Close();");

		}
		[TestMethod]
		public void SchEd_WriterCalling_ModifyNodeType_ChangeParent()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreatePropertyType("PT1", DataType.String); SetSchemaItemId(ed1.PropertyTypes["PT1"], 1);
			ed1.CreatePropertyType("PT2", DataType.String); SetSchemaItemId(ed1.PropertyTypes["PT1"], 2);
			ed1.CreatePropertyType("PT3", DataType.String); SetSchemaItemId(ed1.PropertyTypes["PT1"], 3);
			NodeType nt1 = ed1.CreateNodeType(null, "NT1"); SetSchemaItemId(ed1.NodeTypes["NT1"], 1);
			NodeType nt2 = ed1.CreateNodeType(null, "NT2"); SetSchemaItemId(ed1.NodeTypes["NT2"], 2);
			NodeType nt3 = ed1.CreateNodeType(nt1, "NT3"); SetSchemaItemId(ed1.NodeTypes["NT3"], 3);
			ed1.AddPropertyTypeToPropertySet(ed1.PropertyTypes["PT1"], ed1.NodeTypes["NT1"]);
			ed1.AddPropertyTypeToPropertySet(ed1.PropertyTypes["PT2"], ed1.NodeTypes["NT2"]);
			ed1.AddPropertyTypeToPropertySet(ed1.PropertyTypes["PT3"], ed1.NodeTypes["NT3"]);

			//-- create current
			ed2.CreatePropertyType("PT1", DataType.String); SetSchemaItemId(ed2.PropertyTypes["PT1"], 1);
			ed2.CreatePropertyType("PT2", DataType.String); SetSchemaItemId(ed2.PropertyTypes["PT1"], 2);
			ed2.CreatePropertyType("PT3", DataType.String); SetSchemaItemId(ed2.PropertyTypes["PT1"], 3);
			nt1 = ed2.CreateNodeType(null, "NT1"); SetSchemaItemId(ed2.NodeTypes["NT1"], 1);
			nt2 = ed2.CreateNodeType(null, "NT2"); SetSchemaItemId(ed2.NodeTypes["NT2"], 2);
			nt3 = ed2.CreateNodeType(nt1, "NT3"); SetSchemaItemId(ed2.NodeTypes["NT3"], 3);
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT1"]);
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT2"], ed2.NodeTypes["NT2"]);
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT3"], ed2.NodeTypes["NT3"]);

			//-- edit
			ed2.ModifyNodeType(ed2.NodeTypes["NT3"], ed2.NodeTypes["NT2"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string expectedLog = @"
				Open();
				ModifyNodeType(nodeType=<NT3>, parent=<NT2>, className=<>);
				RemovePropertyTypeFromPropertySet(propertyType=<PT1>, owner=<NT3>);
				AddPropertyTypeToPropertySet(propertyType=<PT2>, owner=<NT3>, isDeclared=<True>);
				Close();
				".Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
			string log = wr.Log.Replace("\r\n", "").Replace(" ", "");
			Assert.IsTrue(log == expectedLog, "#1");
			Assert.IsNull(nt1.Parent, "#2");
			Assert.IsNull(nt2.Parent, "#3");
			Assert.IsTrue(nt3.Parent == nt2, "#4");
			Assert.IsTrue(nt1.PropertyTypes.Count == 1, "#5");
			Assert.IsTrue(nt2.PropertyTypes.Count == 1, "#6");
			Assert.IsTrue(nt3.PropertyTypes.Count == 2, "#7");
			Assert.IsNotNull(nt1.PropertyTypes["PT1"], "#8");
			Assert.IsNotNull(nt2.PropertyTypes["PT2"], "#9");
			Assert.IsNotNull(nt3.PropertyTypes["PT3"], "#10");
			Assert.IsNull(nt3.PropertyTypes["PT1"], "#11");
			Assert.IsNotNull(nt3.PropertyTypes["PT2"], "#12");
		}

		[TestMethod]
		public void SchEd_WriterCalling_DeleteNodeType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed1.PropertyTypes["PT1"], 1);
			ed1.CreateNodeType(null, "NT1");
			SetSchemaItemId(ed1.NodeTypes["NT1"], 1);
			ed1.AddPropertyTypeToPropertySet(ed1.PropertyTypes["PT1"], ed1.NodeTypes["NT1"]);
			//-- create current
			ed2.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed2.PropertyTypes["PT1"], 1);
			ed2.CreateNodeType(null, "NT1");
			SetSchemaItemId(ed2.NodeTypes["NT1"], 1);
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT1"]);

			//-- edit
			ed2.DeleteNodeType(ed2.NodeTypes["NT1"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string expectedLog = @"
				Open();
				DeleteNodeType(nodeType=<NT1>);
				Close();
				".Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
			string log = wr.Log.Replace("\r\n", "").Replace(" ", "");
			Assert.IsTrue(log == expectedLog);
		}
		[TestMethod]
		public void SchEd_WriterCalling_ReCreateNodeType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed1.PropertyTypes["PT1"], 1);
			ed1.CreateNodeType(null, "NT1");
			SetSchemaItemId(ed1.NodeTypes["NT1"], 1);
			ed1.AddPropertyTypeToPropertySet(ed1.PropertyTypes["PT1"], ed1.NodeTypes["NT1"]);
			//-- create current
			ed2.CreatePropertyType("PT1", DataType.String);
			SetSchemaItemId(ed2.PropertyTypes["PT1"], 1);
			ed2.CreateNodeType(null, "NT1");
			SetSchemaItemId(ed2.NodeTypes["NT1"], 1);
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT1"]);

			//-- edit
			ed2.DeleteNodeType(ed2.NodeTypes["NT1"]);
			ed2.CreateNodeType(null, "NT1");
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT1"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string expectedLog = @"
				Open();
				DeleteNodeType(nodeType=<NT1>);
				CreateNodeType(parent=<[null]>, name=<NT1>, className=<>);
				AddPropertyTypeToPropertySet(propertyType=<PT1>, owner=<NT1>, isDeclared=<True>);
				Close();
				".Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
			string log = wr.Log.Replace("\r\n", "").Replace(" ", "");
			Assert.IsTrue(log == expectedLog);
		}

		[TestMethod]
        public void SchEd_WriterCalling_CreateContentListType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreateContentListPropertyType(DataType.String, 0);
			SetSchemaItemId(ed1.PropertyTypes["#String_0"], 1);
			//-- create current
			ed2.CreateContentListPropertyType(DataType.String, 0);
			SetSchemaItemId(ed2.PropertyTypes["#String_0"], 1);

			//-- edit
			ed2.CreateContentListType("LT1");
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["#String_0"], ed2.ContentListTypes["LT1"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string expectedLog = @"
				Open();
				CreateContentListType(name=<LT1>);
				AddPropertyTypeToPropertySet(propertyType=<#String_0>, owner=<LT1>, isDeclared=<True>);
				Close();
				".Replace("\r\n", "").Replace("\t", "");
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == expectedLog);
		}
		[TestMethod]
        public void SchEd_WriterCalling_DeleteContentListType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			ed1.CreateContentListPropertyType(DataType.String, 0);
			SetSchemaItemId(ed1.PropertyTypes["#String_0"], 1);
			ed1.CreateContentListType("LT1");
			SetSchemaItemId(ed1.ContentListTypes["LT1"], 1);
			ed1.AddPropertyTypeToPropertySet(ed1.PropertyTypes["#String_0"], ed1.ContentListTypes["LT1"]);
			//-- create current
			ed2.CreateContentListPropertyType(DataType.String, 0);
			SetSchemaItemId(ed2.PropertyTypes["#String_0"], 1);
			ed2.CreateContentListType("LT1");
			SetSchemaItemId(ed2.ContentListTypes["LT1"], 1);
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["#String_0"], ed2.ContentListTypes["LT1"]);

			//-- edit
			ed2.DeleteContentListType(ed2.ContentListTypes["LT1"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string expectedLog = @"
				Open();
				DeleteContentListType(contentListType=<LT1>);
				Close();
				".Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
			string log = wr.Log.Replace("\r\n", "").Replace(" ", "");
			Assert.IsTrue(log == expectedLog);
		}

		//============================================================================== Complex tests

		[TestMethod]
		public void SchEd_WriterCalling_Complex_01()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			PropertyType ptX = CreatePropertyType(ed1, "X", DataType.String, 1);
			PropertyType ptY = CreatePropertyType(ed1, "Y", DataType.String, 2);
			PropertyType ptZ = CreatePropertyType(ed1, "Z", DataType.String, 3);
			NodeType ntA = CreateNodeType(ed1, null, "A", null, 1);
			NodeType ntB = CreateNodeType(ed1, ntA, "B", null, 2);
			NodeType ntC = CreateNodeType(ed1, ntB, "C", null, 3);
			ed1.AddPropertyTypeToPropertySet(ptX, ntB);
			ed1.AddPropertyTypeToPropertySet(ptY, ntC);
			ed1.AddPropertyTypeToPropertySet(ptX, ntA);

			//-- create current
			XmlDocument xd = new XmlDocument();
			xd.LoadXml(ed1.ToXml());
			ed2.Load(xd);
			ptX = ed2.PropertyTypes["X"];
			ptY = ed2.PropertyTypes["Y"];
			ptZ = ed2.PropertyTypes["Z"];
			ntA = ed2.NodeTypes["A"];
			ntB = ed2.NodeTypes["B"];
			ntC = ed2.NodeTypes["C"];

			//-- edit
			ed2.RemovePropertyTypeFromPropertySet(ptX, ntA);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();RemovePropertyTypeFromPropertySet(propertyType=<X>, owner=<A>);Close();");

		}
		[TestMethod]
		public void SchEd_WriterCalling_OverridePropertyOnNodeType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			PropertyType pt1 = CreatePropertyType(ed1, "PT1", DataType.String, 1);
			NodeType nt1 = CreateNodeType(ed1, null, "NT1", "NT1", 1);
			NodeType nt2 = CreateNodeType(ed1, nt1, "NT2", "NT2", 2);
			NodeType nt3 = CreateNodeType(ed1, nt2, "NT3", "NT3", 3);
			NodeType nt4 = CreateNodeType(ed1, nt3, "NT4", "NT4", 4);
			NodeType nt5 = CreateNodeType(ed1, nt4, "NT5", "NT5", 5);
			ed1.AddPropertyTypeToPropertySet(pt1, nt2);

			//-- create current
			XmlDocument xd = new XmlDocument();
			xd.LoadXml(ed1.ToXml());
			ed2.Load(xd);

			//-- edit
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT4"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();UpdatePropertyTypeDeclarationState(propType=<PT1>, newSet=<NT4>, isDeclared=<True>);Close();");
		}
		[TestMethod]
		public void SchEd_WriterCalling_AddPropertyToAncestorNodeType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			PropertyType pt1 = CreatePropertyType(ed1, "PT1", DataType.String, 1);
			NodeType nt1 = CreateNodeType(ed1, null, "NT1", "NT1", 1);
			NodeType nt2 = CreateNodeType(ed1, nt1, "NT2", "NT2", 2);
			NodeType nt3 = CreateNodeType(ed1, nt2, "NT3", "NT3", 3);
			NodeType nt4 = CreateNodeType(ed1, nt3, "NT4", "NT4", 4);
			NodeType nt5 = CreateNodeType(ed1, nt4, "NT5", "NT5", 5);
			ed1.AddPropertyTypeToPropertySet(pt1, nt4);

			//-- create current
			XmlDocument xd = new XmlDocument();
			xd.LoadXml(ed1.ToXml());
			ed2.Load(xd);

			//-- edit
			ed2.AddPropertyTypeToPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT2"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string expectedLog = @"
				Open();
				AddPropertyTypeToPropertySet(propertyType=<PT1>, owner=<NT2>, isDeclared=<True>);
				AddPropertyTypeToPropertySet(propertyType=<PT1>, owner=<NT3>, isDeclared=<False>);
				Close();
				".Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
			string log = wr.Log.Replace("\r\n", "").Replace(" ", "");
			Assert.IsTrue(log == expectedLog);
		}
		[TestMethod]
		public void SchEd_WriterCalling_RemoveOverriddenPropertyFromNodeType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			PropertyType pt1 = CreatePropertyType(ed1, "PT1", DataType.String, 1);
			NodeType nt1 = CreateNodeType(ed1, null, "NT1", "NT1", 1);
			NodeType nt2 = CreateNodeType(ed1, nt1, "NT2", "NT2", 2);
			NodeType nt3 = CreateNodeType(ed1, nt2, "NT3", "NT3", 3);
			NodeType nt4 = CreateNodeType(ed1, nt3, "NT4", "NT4", 4);
			NodeType nt5 = CreateNodeType(ed1, nt4, "NT5", "NT5", 5);
			ed1.AddPropertyTypeToPropertySet(pt1, nt4);
			ed1.AddPropertyTypeToPropertySet(pt1, nt2);

			//-- create current
			XmlDocument xd = new XmlDocument();
			xd.LoadXml(ed1.ToXml());
			ed2.Load(xd);

			//-- edit
			ed2.RemovePropertyTypeFromPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT4"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string log = wr.Log.Replace("\r\n", "");
			Assert.IsTrue(log == "Open();UpdatePropertyTypeDeclarationState(propType=<PT1>, newSet=<NT4>, isDeclared=<False>);Close();");
		}
		[TestMethod]
		public void SchEd_WriterCalling_RemoveAncestorOfOverriddenPropertyFromNodeType()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();

			//-- create original
			PropertyType pt1 = CreatePropertyType(ed1, "PT1", DataType.String, 1);
			NodeType nt1 = CreateNodeType(ed1, null, "NT1", "NT1", 1);
			NodeType nt2 = CreateNodeType(ed1, nt1, "NT2", "NT2", 2);
			NodeType nt3 = CreateNodeType(ed1, nt2, "NT3", "NT3", 3);
			NodeType nt4 = CreateNodeType(ed1, nt3, "NT4", "NT4", 4);
			NodeType nt5 = CreateNodeType(ed1, nt4, "NT5", "NT5", 5);
			ed1.AddPropertyTypeToPropertySet(pt1, nt4);
			ed1.AddPropertyTypeToPropertySet(pt1, nt2);

			//-- create current
			XmlDocument xd = new XmlDocument();
			xd.LoadXml(ed1.ToXml());
			ed2.Load(xd);

			//-- edit
			ed2.RemovePropertyTypeFromPropertySet(ed2.PropertyTypes["PT1"], ed2.NodeTypes["NT2"]);

			//-- register
			ed2Acc.RegisterSchema(ed1, wr);

			//-- test
			string expectedLog = @"
				Open();
				RemovePropertyTypeFromPropertySet(propertyType=<PT1>, owner=<NT2>);
				RemovePropertyTypeFromPropertySet(propertyType=<PT1>, owner=<NT3>);
				Close();".Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
			string log = wr.Log.Replace("\r\n", "").Replace(" ", "");
			Assert.IsTrue(log == expectedLog);
		}

		//================================================= Tools =================================================

		private NodeType CreateNodeType(SchemaEditor editor, NodeType parent, string name, string className, int id)
		{
			NodeType nt = editor.CreateNodeType(parent, name, className);
			SetSchemaItemId(nt, id);
			return nt;
		}
		private PropertyType CreatePropertyType(SchemaEditor editor, string name, DataType dataType, int id)
		{
			PropertyType pt = editor.CreatePropertyType(name, dataType);
			SetSchemaItemId(pt, id);
			return pt;
		}
		private PermissionType CreatePermissionType(SchemaEditor ed, string name, int id)
		{
			PermissionType perm = ed.CreatePermissionType(name);
			SetSchemaItemId(perm, id);
			return perm;
		}
		private void SetSchemaItemId(SchemaItem item, int id)
		{
			SchemaItemAccessor slotAcc = new SchemaItemAccessor(item);
			slotAcc.Id = id;
		}
	}
}