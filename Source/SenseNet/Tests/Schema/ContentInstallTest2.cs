using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using SNC = SenseNet.ContentRepository;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using System.Collections;
using System.Xml;

namespace SenseNet.ContentRepository.Tests.Schema
{
	[TestClass()]
    public class ContentInstallTest2 : TestBase
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

		private static string _testRootName = "_RegistrationTests";
		private static string _testRootPath = String.Concat("/Root/", _testRootName);
		/// <summary>
		/// Do not use. Instead of TestRoot property
		/// </summary>
		private Node _testRoot;
		public Node TestRoot
		{
			get
			{
				if (_testRoot == null)
				{
					_testRoot = Node.LoadNode(_testRootPath);
					if (_testRoot == null)
					{
						Node node = NodeType.CreateInstance("SystemFolder", Node.LoadNode("/Root"));
						node.Name = _testRootName;
						node.Save();
						_testRoot = Node.LoadNode(_testRootPath);
					}
				}
				return _testRoot;
			}
		}
        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.ForceDelete(_testRootPath);

            if (ActiveSchema.NodeTypes["TestNode11"] != null)
                ContentTypeInstaller.RemoveContentType(ContentType.GetByName("TestNode11"));
            if (ActiveSchema.NodeTypes["TestNode10"] != null)
                ContentTypeInstaller.RemoveContentType(ContentType.GetByName("TestNode10"));

            ContentType ct;
            ct = ContentType.GetByName("TestNode11");
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
            ct = ContentType.GetByName("TestNode10");
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
            ct = ContentType.GetByName("DataTypeCollisionTestHandler1");
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
            ct = ContentType.GetByName("DataTypeCollisionTestHandler");
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
            ct = ContentType.GetByName("TestSurvey");
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
        }

		[TestMethod]
		public void ContentType_Install_InheritedClass()
		{
			string contentTypeADef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode10' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='X' type='Integer' />
								</Fields>
							</ContentType>";

			string contentTypeBDef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode11' parentType='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode11' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Y' type='Integer' />
								</Fields>
							</ContentType>";

			string expectedLog = @"
				Open();
				CreatePropertyType(name=<Y>, dataType=<Int>, mapping=<1>, isContentListProperty=<False>);
				CreateNodeType(parent=<TestNode10>, name=<TestNode11>, className=<SenseNet.ContentRepository.Tests.ContentHandlers.TestNode11>);
				AddPropertyTypeToPropertySet(propertyType=<X>, owner=<TestNode11>, isDeclared=<False>);
				AddPropertyTypeToPropertySet(propertyType=<Y>, owner=<TestNode11>, isDeclared=<True>);
				Close();
				".Replace("\r\n", "").Replace("\t", "").Replace(" ", "");

			string log = InstallContentType(contentTypeADef, contentTypeBDef).Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
			Assert.IsTrue(log == expectedLog);
		}
		[TestMethod]
		public void ContentType_FullInstall_InheritedClass()
		{
			string contentTypeADef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode10' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='X' type='Integer' />
								</Fields>
							</ContentType>";

			string contentTypeBDef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode11' parentType='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode11' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Y' type='Integer' />
								</Fields>
							</ContentType>";

			ContentTypeInstaller installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
			installer.AddContentType(contentTypeADef);
			installer.AddContentType(contentTypeBDef);
			installer.ExecuteBatch();

			NodeType testNodeType10 = ActiveSchema.NodeTypes["TestNode10"];
			NodeType testNodeType11 = ActiveSchema.NodeTypes["TestNode11"];

			Content content = Content.CreateNew("TestNode11", this.TestRoot, "TestNode11-1");
			ContentType contentType10 = ContentTypeManager.Current.GetContentTypeByName("TestNode10");
			ContentType contentType11 = ContentTypeManager.Current.GetContentTypeByName("TestNode11");
			ContentType contentTypeByHandler = ContentTypeManager.Current.GetContentTypeByHandler(content.ContentHandler);
			ContentType contentTypeByHandlerTypeName = ContentTypeManager.Current.GetContentTypeByName(content.ContentHandler.NodeType.Name);

			Assert.IsNotNull(testNodeType10, "#1");
			Assert.IsNotNull(testNodeType11, "#2");
			Assert.IsTrue(testNodeType11.Parent == testNodeType10, "#3");
			Assert.IsNotNull(content.Fields["Y"], "#4");
			Assert.IsNotNull(content.Fields["X"], "#5");
			Assert.IsTrue(ReferenceEquals(contentTypeByHandler, contentTypeByHandlerTypeName), "#6");
			Assert.IsTrue(ReferenceEquals(contentType11, contentTypeByHandlerTypeName), "#7");
			Assert.IsTrue(ReferenceEquals(contentType11.ParentType, contentType10), "#8");
		}
		[TestMethod]
		public void ContentType_FullInstall_InheritedClass_ChangeHandlers()
		{
			//-- Step 1: Install TestNode10 and TestNode11 content types with TestNode10 and TestNode11 handlers

			string contentTypeADef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode10' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='X' type='Integer' />
								</Fields>
							</ContentType>";

			string contentTypeBDef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode11' parentType='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode11' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Y' type='Integer' />
								</Fields>
							</ContentType>";

			ContentTypeInstaller installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
			installer.AddContentType(contentTypeADef);
			installer.AddContentType(contentTypeBDef);
			installer.ExecuteBatch();

			//-- Step 2: Reinstall TestNode10 and TestNode11 content types with TestNode12 and TestNode13 handlers

			contentTypeADef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode12' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='A' type='Integer'><DisplayName>A</DisplayName></Field>
									<Field name='B' type='Integer'><DisplayName>B</DisplayName></Field>
									<Field name='C' type='Integer'><DisplayName>C</DisplayName></Field>
								</Fields>
							</ContentType>";

			contentTypeBDef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode11' parentType='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode13' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<!--
									<Field name='B' override='true' type='Integer' />
									<Field name='C' override='true' type='Integer'><DisplayName>CC</DisplayName></Field>
									-->
									<Field name='B' type='Integer' />
									<Field name='C' type='Integer'><DisplayName>CC</DisplayName></Field>
									<Field name='D' type='Integer'><DisplayName>D</DisplayName></Field>
								</Fields>
							</ContentType>";

			installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
			installer.AddContentType(contentTypeADef);
			installer.AddContentType(contentTypeBDef);
			installer.ExecuteBatch();

			NodeType testNodeType10 = ActiveSchema.NodeTypes["TestNode10"];
			NodeType testNodeType11 = ActiveSchema.NodeTypes["TestNode11"];

			Content content = Content.CreateNew("TestNode11", this.TestRoot, "TestNode11-1");
			ContentType contentType10 = ContentTypeManager.Current.GetContentTypeByName("TestNode10");
			ContentType contentType11 = ContentTypeManager.Current.GetContentTypeByName("TestNode11");
			ContentType contentTypeByHandler = ContentTypeManager.Current.GetContentTypeByHandler(content.ContentHandler);
			ContentType contentTypeByHandlerTypeName = ContentTypeManager.Current.GetContentTypeByName(content.ContentHandler.NodeType.Name);

			Assert.IsNotNull(testNodeType10, "#1");
			Assert.IsNotNull(testNodeType11, "#2");
			Assert.IsTrue(testNodeType11.Parent == testNodeType10, "#3");
			Assert.IsTrue(ReferenceEquals(contentTypeByHandler, contentTypeByHandlerTypeName), "#4");
			Assert.IsTrue(ReferenceEquals(contentType11, contentTypeByHandlerTypeName), "#5");
			Assert.IsTrue(ReferenceEquals(contentType11.ParentType, contentType10), "#6");
			Assert.IsTrue(content.Fields.Count == 4, "#7");
            Assert.IsTrue(content.Fields["A"].DisplayName == "A", "#8");
            Assert.IsTrue(content.Fields["B"].DisplayName == "B", "#9");
            Assert.IsTrue(content.Fields["C"].DisplayName == "CC", "#10");
            Assert.IsTrue(content.Fields["D"].DisplayName == "D", "#11");
		}
		[TestMethod]
		[ExpectedException(typeof(ContentRegistrationException))]
		public void ContentType_FullInstall_InheritedClass_ChangeHandlers1()
		{
			//-- Step 1: Install TestNode10 and TestNode11 content types with TestNode10 and TestNode11 handlers

			string contentTypeADef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode10' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='X' type='Integer' />
								</Fields>
							</ContentType>";

			string contentTypeBDef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode11' parentType='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode11' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Y' type='Integer' />
								</Fields>
							</ContentType>";

			ContentTypeInstaller installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
			installer.AddContentType(contentTypeADef);
			installer.AddContentType(contentTypeBDef);
			installer.ExecuteBatch();

			//-- Step 2: Reinstall TestNode10 content type with an unknown handler

			contentTypeADef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode10' handler='Unknown' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='A' type='Integer'><DisplayName>A</DisplayName></Field>
									<Field name='B' type='Integer'><DisplayName>B</DisplayName></Field>
									<Field name='C' type='Integer'><DisplayName>C</DisplayName></Field>
								</Fields>
							</ContentType>";

			installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
			installer.AddContentType(contentTypeADef);
			installer.AddContentType(contentTypeBDef);
			installer.ExecuteBatch();
		}
		[TestMethod]
		public void ContentType_FullInstall_InheritedClass_ChangeHandlers2()
		{
			//-- Step 1: Install TestNode10 and TestNode11 content types with TestNode10 and TestNode11 handlers

			string contentTypeADef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode10' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='X' type='Integer' />
								</Fields>
							</ContentType>";

			string contentTypeBDef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='TestNode11' parentType='TestNode10' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNode11' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Y' type='Integer' />
								</Fields>
							</ContentType>";

			ContentTypeInstaller installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
			installer.AddContentType(contentTypeADef);
			installer.AddContentType(contentTypeBDef);
			installer.ExecuteBatch();

			//-- Step 2: change handler of TestNode10 to unknown in the database directly

			ContentTypeManager.Reset();

			Content content = Content.CreateNew("TestNode10", this.TestRoot, "TestNode10unknown");
		}

		[TestMethod]
		public void ContentType_FullInstall_LongFlat()
		{
			string typeName = "TestSurvey";
			StringBuilder sb = new StringBuilder();

			sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
			sb.Append("<ContentType name='").Append(typeName).Append("' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>\r\n");
			sb.Append("	<DisplayName>TestSurvey</DisplayName>\r\n");
			sb.Append("	<Description>Test Survey</Description>\r\n");
			sb.Append("	<Icon>Survey</Icon>\r\n");
			sb.Append("	<Fields>\r\n");
			int k = 2;
			for (int i = 0; i < 80 * k; i++)
			{
				sb.Append("		<Field name='ShortText_").Append(i + 1).Append("' type='ShortText'>\r\n");
				sb.Append("			<DisplayName>ShortText #").Append(i + 1).Append("</DisplayName>\r\n");
				sb.Append("			<Description>ShortText #").Append(i + 1).Append("</Description>\r\n");
				sb.Append("			<Icon>field.gif</Icon>\r\n");
				sb.Append("		</Field>\r\n");
			}
			for (int i = 0; i < 40 * k; i++)
			{
				sb.Append("		<Field name='Integer_").Append(i + 1).Append("' type='Integer'>\r\n");
				sb.Append("			<DisplayName>Integer #").Append(i + 1).Append("</DisplayName>\r\n");
				sb.Append("			<Description>Integer #").Append(i + 1).Append("</Description>\r\n");
				sb.Append("			<Icon>field.gif</Icon>\r\n");
				sb.Append("		</Field>\r\n");
			}
			for (int i = 0; i < 25 * k; i++)
			{
				sb.Append("		<Field name='DateTime_").Append(i + 1).Append("' type='DateTime'>\r\n");
				sb.Append("			<DisplayName>DateTime #").Append(i + 1).Append("</DisplayName>\r\n");
				sb.Append("			<Description>DateTime #").Append(i + 1).Append("</Description>\r\n");
				sb.Append("			<Icon>field.gif</Icon>\r\n");
				sb.Append("		</Field>\r\n");
			}
			for (int i = 0; i < 15 * k; i++)
			{
				sb.Append("		<Field name='Number_").Append(i + 1).Append("' type='Number'>\r\n");
				sb.Append("			<DisplayName>Number #").Append(i + 1).Append("</DisplayName>\r\n");
				sb.Append("			<Description>Number #").Append(i + 1).Append("</Description>\r\n");
				sb.Append("			<Icon>field.gif</Icon>\r\n");
				sb.Append("		</Field>\r\n");
			}
			sb.Append("	</Fields>");
			sb.Append("</ContentType>");

			ContentTypeInstaller.InstallContentType(sb.ToString());
			ContentType ct = ContentType.GetByName(typeName);
			int fieldSettingCount = ct.FieldSettings.Count - ct.ParentType.FieldSettings.Count;

			Content content = Content.Load(RepositoryPath.Combine(this.TestRoot.Path, "Survey1"));
			if (content == null)
				content = Content.CreateNew(typeName, this.TestRoot, "Survey1");

			for (int i = 0; i < 80 * k; i++)
			{
				string fieldName=String.Concat("ShortText_", i+1);
				string fieldValue=String.Concat("value ", i+1);
				content[fieldName] = fieldValue;
			}
			for (int i = 0; i < 40 * k; i++)
			{
				string fieldName = String.Concat("Integer_", i + 1);
				int fieldValue = i + 1;
				content[fieldName] = fieldValue;
			}
			for (int i = 0; i < 25 * k; i++)
			{
				string fieldName = String.Concat("DateTime_", i + 1);
				DateTime fieldValue = DateTime.Now.AddMinutes(i+1);
				content[fieldName] = fieldValue;
			}
			for (int i = 0; i < 15 * k; i++)
			{
				string fieldName = String.Concat("Number_", i + 1);
				double fieldValue = i + 1;
				content[fieldName] = fieldValue;
			}
			content.Save();
			int id = content.ContentHandler.Id;

			//ContentTypeManager.Current.RemoveContentType(typeName);
			//Node node = Node.Load(id);

			Assert.IsTrue(fieldSettingCount == (80 + 40 + 25 + 15) * k, "#1");
			//Assert.IsNull(node,"#2");
		}

		[TestMethod]
		public void ContentType_FullInstall_DataTypeCollision()
		{
            var ct = ContentType.GetByName("DataTypeCollisionTestHandler");
            if(ct != null)
                ContentTypeInstaller.RemoveContentType(ct);

			string ctd1 = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='DataTypeCollisionTestHandler' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.DataTypeCollisionTestHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='TestString' type='LongText'>
										<Bind property='TestString' />
									</Field>
								</Fields>
							</ContentType>";
			ContentTypeInstaller installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
			installer.AddContentType(ctd1);
            var thrown = false;
            try
            {
                installer.ExecuteBatch();
            }
            catch (RegistrationException)
            {
                thrown = true;
            }
            catch (ContentRegistrationException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);
		}
		[TestMethod]
		public void ContentType_FullInstall_DataTypeCollision_Inherited()
		{
            var ct = ContentType.GetByName("DataTypeCollisionTestHandler");
            if(ct != null)
                ContentTypeInstaller.RemoveContentType(ct);

			string ctd1 = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='DataTypeCollisionTestHandler' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.DataTypeCollisionTestHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='TestString' type='ShortText'>
										<Bind property='TestString' />
									</Field>
								</Fields>
							</ContentType>";
			string ctd2 = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='DataTypeCollisionTestHandler1' parentType='DataTypeCollisionTestHandler' handler='SenseNet.ContentRepository.Tests.ContentHandlers.DataTypeCollisionTestHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='TestString' type='LongText'>
										<Bind property='TestString' />
									</Field>
								</Fields>
							</ContentType>";
			ContentTypeInstaller installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
			installer.AddContentType(ctd2);
			installer.AddContentType(ctd1);
            var thrown = false;
            try
            {
                installer.ExecuteBatch();
            }
            catch (RegistrationException)
            {
                thrown = true;
            }
            catch (ContentRegistrationException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);
		}

		//TODO: Change structure: not implemented
		//[TestMethod()]
		//public void ContentType_FullInstall_InheritedClass_ChangeParent()
		//{
		//    Assembly asm = SchemaTestTools.DynamicAssembly;

		//    //-- Step 1: Install content types: TestType1, TestType2 and TestType1/TestType3
		//    string contentTypeDef1 = @"<?xml version='1.0' encoding='utf-8'?>
		//		<ContentType name='TestType1' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
		//			<Fields>
		//				<Field name='TestField' type='ShortText'>
		//					<DisplayName>TestField1</DisplayName>
		//				</Field>
		//			</Fields>
		//		</ContentType>";
		//    string contentTypeDef2 = @"<?xml version='1.0' encoding='utf-8'?>
		//		<ContentType name='TestType2' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
		//			<Fields>
		//				<Field name='TestField' type='ShortText'>
		//					<DisplayName>TestField2</DisplayName>
		//				</Field>
		//			</Fields>
		//		</ContentType>";
		//    string contentTypeDef3 = @"<?xml version='1.0' encoding='utf-8'?>
		//		<ContentType name='TestType3' parentType='TestType1' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
		//			<Fields>
		//				<Field name='TestField' type='ShortText'></Field>
		//			</Fields>
		//		</ContentType>";

		//    ContentTypeInstaller installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
		//    installer.AddContentType(contentTypeDef1);
		//    installer.AddContentType(contentTypeDef2);
		//    installer.AddContentType(contentTypeDef3);
		//    installer.ExecuteBatch();

		//    //-- Step 2: Reinstall TestType3 under TestType2
		//    contentTypeDef3 = @"<?xml version='1.0' encoding='utf-8'?>
		//		<ContentType name='TestType3' parentType='TestType2' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'></ContentType>";

		//    ContentTypeInstaller.InstallContentType(contentTypeDef3);

		//    NodeType nt1 = ActiveSchema.NodeTypes["TestType1"];
		//    NodeType nt2 = ActiveSchema.NodeTypes["TestType2"];
		//    NodeType nt3 = ActiveSchema.NodeTypes["TestType3"];
		//    ContentType ct1 = ContentTypeManager.Current.GetContentTypeByName(nt1.Name);
		//    ContentType ct2 = ContentTypeManager.Current.GetContentTypeByName(nt2.Name);
		//    ContentType ct3 = ContentTypeManager.Current.GetContentTypeByName(nt3.Name);

		//    SNC.Content c3 = SNC.Content.CreateNew("TestType3", this.TestRoot, "ChangeParentTest3");

		//    Assert.IsTrue(ct3.Path == Path.Combine(ct2.Path, ct3.Name));
		//    Assert.IsTrue(c3.Fields["TestField"].Title == "TestField2");
		//}


		//================================================= Tools =================================================

		private string InstallContentType(string contentTypeDefInstall, string contentTypeDefModify)
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();

			ContentTypeManagerAccessor ctmAcc = new ContentTypeManagerAccessor(ContentTypeManager.Current);
			ContentType cts = ctmAcc.LoadOrCreateNew(contentTypeDefInstall);
            if (contentTypeDefModify != null)
            {
                cts.Save(false);
                var parent = ContentType.GetByName(cts.ParentName);
                ContentTypeManager.Current.AddContentType(cts);
            }

			ctmAcc.ApplyChangesInEditor(cts, ed2);

			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();
			ed2Acc.RegisterSchema(ed1, wr);

			if (contentTypeDefModify != null)
			{
				//-- Id-k beallitasa es klonozas
				SchemaEditor ed3 = new SchemaEditor();
				SchemaEditorAccessor ed3Acc = new SchemaEditorAccessor(ed3);
				SchemaItemAccessor schItemAcc;
				int id = 1;
				foreach (PropertyType pt in ed2.PropertyTypes)
				{
					PropertyType clone = ed3.CreatePropertyType(pt.Name, pt.DataType, pt.Mapping);
					schItemAcc = new SchemaItemAccessor(pt);
					schItemAcc.Id = id++;
					schItemAcc = new SchemaItemAccessor(clone);
					schItemAcc.Id = pt.Id;
				}
				id = 1;
				foreach (NodeType nt in ed2.NodeTypes)
				{
					NodeType clone = ed3.CreateNodeType(nt.Parent, nt.Name, nt.ClassName);
					foreach (PropertyType pt in nt.PropertyTypes)
						ed3.AddPropertyTypeToPropertySet(ed3.PropertyTypes[pt.Name], clone);
					schItemAcc = new SchemaItemAccessor(nt);
					schItemAcc.Id = id++;
					schItemAcc = new SchemaItemAccessor(clone);
					schItemAcc.Id = nt.Id;
				}

				cts = ctmAcc.LoadOrCreateNew(contentTypeDefModify);
				ctmAcc.ApplyChangesInEditor(cts, ed3);
				wr = new TestSchemaWriter();
				ed3Acc.RegisterSchema(ed2, wr);
			}

			return wr.Log;
		}
	}
}