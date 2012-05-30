using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.Schema;
using System.Collections.Generic;
using System;
using System.Reflection;
using SenseNet.ContentRepository.Tests.Schema;
using System.Text;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Fields;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Tests.ContentHandler
{
	[TestClass]
    public class ContentListTest : TestBase
	{
		private class ContentListAccessor : Accessor
		{
            public ContentListAccessor(object target) : base(target) { }
			public static string GenerateNameFromUsedSlots(IDictionary<DataType, int> slots)
			{
				return (string)Accessor.CallPrivateStaticMethod(typeof(ContentList), "GenerateNameFromUsedSlots", new Type[] { typeof(Dictionary<DataType, int>) }, new object[] { slots });
			}
			public static Dictionary<DataType, int> RestoreUsedSlotsFromName(string name)
			{
				return (Dictionary<DataType, int>)Accessor.CallPrivateStaticMethod(typeof(ContentList), "RestoreUsedSlotsFromName", new Type[] { typeof(string) }, new object[] { name });
			}
		}

		#region test infrastructure
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

		private const string _listDef1 = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Cars title</DisplayName>
	<Description>Cars description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ListField1' type='ShortText'>
			<DisplayName>ContentListField1</DisplayName>
			<Description>ContentListField1 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField2' type='WhoAndWhen'>
			<DisplayName>ContentListField2</DisplayName>
			<Description>ContentListField2 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField3' type='ShortText'>
			<DisplayName>ContentListField3</DisplayName>
			<Description>ContentListField3 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>200</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";

		private static string _testRootName = "_ListTests";
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
                
            ContentType ct;
            ct = ContentType.GetByName("TestNode11");
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
        }

		[TestMethod]
		public void ContentList_1()
		{
			string listDef = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Cars title</DisplayName>
	<Description>Cars description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ListField1' type='ShortText'>
			<DisplayName>ContentListField1</DisplayName>
			<Description>ContentListField1 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField2' type='WhoAndWhen'>
			<DisplayName>ContentListField2</DisplayName>
			<Description>ContentListField2 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField3' type='ShortText'>
			<DisplayName>ContentListField3</DisplayName>
			<Description>ContentListField3 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>200</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";
			string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
			if (Node.Exists(path))
                Node.ForceDelete(path);

			ContentList list = new ContentList(this.TestRoot);
            list.Name = "Cars";
            list.ContentListDefinition = listDef;
            list.AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") };

            list.Save();

            Node car = new GenericContent(list, "Car");
			car.Name = "Kispolszki";
			car["#String_0"] = "ABC 34-78";
			car.Save();

			Content content = Content.Create(car);

			//-- Sikeres, ha nem dob hibat
		}

        [TestMethod]
        public void ContentList_AddField()
        {
            string listDef = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Cars title</DisplayName>
	<Description>Cars description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ListField1' type='ShortText'>
			<DisplayName>ContentListField1</DisplayName>
			<Description>ContentListField1 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField2' type='WhoAndWhen'>
			<DisplayName>ContentListField2</DisplayName>
			<Description>ContentListField2 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField3' type='ShortText'>
			<DisplayName>ContentListField3</DisplayName>
			<Description>ContentListField3 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>200</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";
            string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
            if (Node.Exists(path))
                Node.ForceDelete(path);

            var list = new ContentList(this.TestRoot)
                           {
                               Name = "Cars",
                               ContentListDefinition = listDef,
                               AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") }
                           };

            list.Save();

            Node car = new GenericContent(list, "Car");
            car.Name = "Kispolszki";
            car["#String_0"] = "ABC 34-78";
            car.Save();

            list = Node.Load<ContentList>(list.Path);
            var fs = new ShortTextFieldSetting
                         {
                             Name = "#NewField",
                             ShortName = "ShortText",
                             MaxLength = 100,
                             MinLength = 10,
                             DisplayName = "New field title"
                         };

            list.AddField(fs);

            var cc = Content.Load(car.Path);

            Assert.IsTrue(cc.Fields.ContainsKey(fs.Name));
        }

        [TestMethod]
        public void ContentList_UpdateField()
        {
            string listDef = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Cars title</DisplayName>
	<Description>Cars description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ListField1' type='ShortText'>
			<DisplayName>ContentListField1</DisplayName>
			<Description>ContentListField1 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField2' type='WhoAndWhen'>
			<DisplayName>ContentListField2</DisplayName>
			<Description>ContentListField2 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField3' type='ShortText'>
			<DisplayName>ContentListField3</DisplayName>
			<Description>ContentListField3 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>200</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";
            string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
            if (Node.Exists(path))
                Node.ForceDelete(path);

            var list = new ContentList(this.TestRoot)
            {
                Name = "Cars",
                ContentListDefinition = listDef,
                AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") }
            };

            list.Save();

            Node car = new GenericContent(list, "Car");
            car.Name = "Kispolszki";
            car["#String_0"] = "ABC 34-78";
            car.Save();

            var fs = Content.Create(car).Fields["#ListField1"].FieldSetting;
            fs.DisplayName = "New TITLE";

            list.UpdateField(fs);

            var cc = Content.Load(car.Path);

            Assert.IsTrue(cc.Fields["#ListField1"].FieldSetting.DisplayName.CompareTo("New TITLE") == 0);
        }

        [TestMethod]
        public void ContentList_DeleteField()
        {
            string listDef = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Cars title</DisplayName>
	<Description>Cars description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ListField1' type='ShortText'>
			<DisplayName>ContentListField1</DisplayName>
			<Description>ContentListField1 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField2' type='WhoAndWhen'>
			<DisplayName>ContentListField2</DisplayName>
			<Description>ContentListField2 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField3' type='ShortText'>
			<DisplayName>ContentListField3</DisplayName>
			<Description>ContentListField3 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>200</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";
            string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
            if (Node.Exists(path))
                Node.ForceDelete(path);

            var list = new ContentList(this.TestRoot)
            {
                Name = "Cars",
                ContentListDefinition = listDef,
                AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") }
            };

            list.Save();

            Node car = new GenericContent(list, "Car");
            car.Name = "Kispolszki";
            car["#String_0"] = "ABC 34-78";
            car.Save();

            list = Node.Load<ContentList>(list.Path);
            var fs = Content.Create(car).Fields["#ListField3"].FieldSetting;

            list.DeleteField(fs);

            var cc = Content.Load(car.Path);

            Assert.IsTrue(!cc.Fields.ContainsKey("#ListField3"));
        }

        [TestMethod]
        public void ContentList_AvailableFields()
        {
            ContentType c = ContentType.GetByName("CT_Root");
            if (c != null)
                ContentTypeInstaller.RemoveContentType(c);
            ContentTypeManager.Reset();

            ContentTypeInstaller installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();

            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='CT_Root' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='InheritanceTest' type='Integer'>
							<Configuration><MinValue>-5</MinValue><MaxValue>7</MaxValue></Configuration>
						</Field>
					</Fields>
				</ContentType>");

            installer.AddContentType("<ContentType name='CT_A' parentType='CT_Root' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'><Fields><Field name='InheritanceTest' type='Integer'><Configuration><MinValue>0</MinValue><MaxValue>10</MaxValue></Configuration></Field></Fields></ContentType>");
            installer.AddContentType("<ContentType name='CT_A_A' parentType='CT_A' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'><Fields><Field name='InheritanceTest' type='Integer'><Configuration><MinValue>1</MinValue><MaxValue>20</MaxValue></Configuration></Field></Fields></ContentType>");
            installer.AddContentType("<ContentType name='CT_B' parentType='CT_Root' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'><Fields><Field name='InheritanceTest' type='Integer'><Configuration><MinValue>2</MinValue><MaxValue>30</MaxValue></Configuration></Field></Fields></ContentType>");
            installer.AddContentType("<ContentType name='CT_B_B' parentType='CT_B' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'><Fields><Field name='InheritanceTest' type='Integer'><Configuration><MinValue>3</MinValue><MaxValue>40</MaxValue></Configuration></Field></Fields></ContentType>");

            installer.ExecuteBatch();

            string listDef = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Cars title</DisplayName>
	<Description>Cars description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ListField1' type='ShortText'>
			<DisplayName>ContentListField1</DisplayName>
			<Description>ContentListField1 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField2' type='WhoAndWhen'>
			<DisplayName>ContentListField2</DisplayName>
			<Description>ContentListField2 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField3' type='ShortText'>
			<DisplayName>ContentListField3</DisplayName>
			<Description>ContentListField3 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>200</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";

            var b = new bool[21];

            ContentType CT_Root = ContentType.GetByName("CT_Root");
            FieldSetting FS_Root = CT_Root.FieldSettings[0];

            ContentType CT_A = ContentType.GetByName("CT_A");
            ContentType CT_B = ContentType.GetByName("CT_B");
            FieldSetting FS_A = CT_A.FieldSettings[0];
            ContentType CT_A_A = ContentType.GetByName("CT_A_A");
            FieldSetting FS_A_A = CT_A_A.FieldSettings[0];

            string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
            if (Node.Exists(path))
                Node.ForceDelete(path);

            var list = new ContentList(this.TestRoot);
            list.Name = "Cars";
            list.ContentListDefinition = listDef;
            list.AllowedChildTypes = new[] { CT_A, CT_B };

            list.Save();

            b[0] = FS_Root.ParentFieldSetting == null;
            b[1] = FS_A.ParentFieldSetting == FS_Root;
            b[2] = FS_A_A.ParentFieldSetting == FS_A;

            var fields = list.GetAvailableFields();
        }

        [TestMethod]
        public void ContentList_SaveFieldSetting()
        {
            string listDef = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Cars title</DisplayName>
	<Description>Cars description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ListField1' type='ShortText'>
			<DisplayName>ContentListField1</DisplayName>
			<Description>ContentListField1 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField2' type='WhoAndWhen'>
			<DisplayName>ContentListField2</DisplayName>
			<Description>ContentListField2 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ListField3' type='ShortText'>
			<DisplayName>ContentListField3</DisplayName>
			<Description>ContentListField3 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>200</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";
            string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
            if (Node.Exists(path))
                Node.ForceDelete(path);

            var list = new ContentList(this.TestRoot)
            {
                Name = "Cars",
                ContentListDefinition = listDef,
                AllowedChildTypes = new[] { ContentType.GetByName("Car") }
            };

            list.Save();

            var fsNodes = new List<Node>(list.FieldSettingContents);
            var fs = fsNodes[0] as FieldSettingContent;
            var fsc = Content.Create(fsNodes[0]);
            var title = "New field title";

            fsc["DisplayName"] = title;
            fsc.Save();

            Assert.IsTrue(fs.FieldSetting.DisplayName.CompareTo(title) == 0);
        }

		[TestMethod]
        public void ContentList_WithoutDefinition()
		{
			string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
			if (Node.Exists(path))
                Node.ForceDelete(path);

            ContentList list = new ContentList(this.TestRoot);
            list.Name = "Cars";
            list.AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") };

            list.Save();

            Node car = new GenericContent(list, "Car");
			car.Name = "Kispolszki";
			car.Save();

			//-- Sikeres, ha nem dob hibat
		}

		[TestMethod]
		[ExpectedException(typeof(ApplicationException))]
        public void ContentList_CreateListUnderList()
		{
			string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
			if (Node.Exists(path))
                Node.ForceDelete(path);

            ContentList list = new ContentList(this.TestRoot);
            list.Name = "Cars";
            list.Save();

            list = new ContentList(list);
		}

		[TestMethod]
        public void ContentList_Modify()
		{
			List<string> listDefs = new List<string>();
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
	</Fields>
</ContentListDefinition>
");
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#LF0' type='ShortText' />
		<ContentListField name='#LF1' type='ShortText' />
	</Fields>
</ContentListDefinition>
");
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#LF0' type='ShortText' />
	</Fields>
</ContentListDefinition>
");
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
	</Fields>
</ContentListDefinition>
");
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#LF0' type='ShortText' />
		<ContentListField name='#LF1' type='ShortText' />
		<ContentListField name='#LF2' type='ShortText' />
	</Fields>
</ContentListDefinition>
");
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#LF0' type='ShortText' />
		<ContentListField name='#LF2' type='ShortText' />
	</Fields>
</ContentListDefinition>
");
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#LF0' type='ShortText' />
		<ContentListField name='#LF1' type='ShortText' />
		<ContentListField name='#LF2' type='ShortText' />
	</Fields>
</ContentListDefinition>
");
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#LF2' type='ShortText' />
	</Fields>
</ContentListDefinition>
");
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#LF0' type='ShortText' />
		<ContentListField name='#LF3' type='ShortText' />
		<ContentListField name='#LF2' type='ShortText' />
	</Fields>
</ContentListDefinition>
");
            listDefs.Add(@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#LF0' type='ShortText' />
		<ContentListField name='#LF1' type='ShortText' />
		<ContentListField name='#LF2' type='ShortText' />
	</Fields>
</ContentListDefinition>
");

			string listName = "List1";
            string listPath = RepositoryPath.Combine(this.TestRoot.Path, listName);
            if (Node.Exists(listPath))
                Node.ForceDelete(listPath);

			ContentList list = new ContentList(this.TestRoot);
            list.Name = listName;
            list.AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") };

            list.Save();

            Node car = new GenericContent(list, "Car");
			car.Name = "Kispolszki";
			car.Save();
			int carId = car.Id;

			StringBuilder log = new StringBuilder();
            for (int def = 0; def < listDefs.Count; def++)
			{
                Exception ex = null;
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        ex = null;
                        list = Node.Load<ContentList>(listPath);
                        list.ContentListDefinition = listDefs[def];
                        list.Save();
                        break;
                    }
                    catch(Exception e)
                    {
                        ex = e;
                        System.Threading.Thread.Sleep(200);
Debug.WriteLine("@> {0}. {1} / {2}", i, def, listDefs.Count);
                    }
                }
                if (ex != null)
                    throw new ApplicationException("Exception after 10 iteration: " + ex.Message, ex);


				car = Node.LoadNode(carId);
				log.Append("Def_").Append(def).Append(": ");
				for (int i = 0; i < 4; i++)
				{
					var propName = "#String_" + i;
					if(car.HasProperty(propName))
						log.Append("[").Append(propName).Append(" = ").Append(car.PropertyTypes[propName].Mapping).Append("]");
				}
				log.Append("\r\n");
			}

			string realLog = log.Replace("\r\n", "").Replace(" ", "").Replace("\t", "").ToString();
			string expectedLog = @"
				Def_0: 
				Def_1: [#String_0 = 800000000][#String_1 = 800000001]
				Def_2: [#String_0 = 800000000]
				Def_3: 
				Def_4: [#String_0 = 800000000][#String_1 = 800000001][#String_2 = 800000002]
				Def_5: [#String_0 = 800000000][#String_2 = 800000002]
				Def_6: [#String_0 = 800000000][#String_1 = 800000001][#String_2 = 800000002]
				Def_7: [#String_2 = 800000002]
				Def_8: [#String_0 = 800000000][#String_1 = 800000001][#String_2 = 800000002]
				Def_9: [#String_0 = 800000000][#String_2 = 800000002][#String_3 = 800000003]
				".Replace("\r\n", "").Replace(" ", "").Replace("\t", "");

			Assert.IsTrue(realLog == expectedLog);
		}

        [TestMethod]
        public void ContentList_FieldInitialize_Bug2943()
        {
            string listDef = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#ListField1' type='Integer'>
			<Configuration>
				<MinValue>-100</MinValue>
				<MaxValue>100</MaxValue>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";
            string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
            if (Node.Exists(path))
                Node.ForceDelete(path);

            ContentList list;
            Content content;
            var bb = new List<bool>();

            list = new ContentList(this.TestRoot);
            list.Name = "Cars";
            list.ContentListDefinition = listDef;
            list.AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") };
            list.Save();

            content = Content.CreateNew("Car", list, "TestCar");

            content["#ListField1"] = 0;
            bb.Add(content.IsValid);
            content["#ListField1"] = 10;
            bb.Add(content.IsValid);
            content["#ListField1"] = 0;
            bb.Add(content.IsValid);
            content["#ListField1"] = -10;
            bb.Add(content.IsValid);
            content["#ListField1"] = -100;
            bb.Add(content.IsValid);
            content["#ListField1"] = 100;
            bb.Add(content.IsValid);
            content["#ListField1"] = -101;
            bb.Add(!content.IsValid);
            content["#ListField1"] = 101;
            bb.Add(!content.IsValid);

            content = Content.CreateNew("Car", list, "TestCar1");

            content["#ListField1"] = 0;
            bb.Add(content.IsValid);
            content["#ListField1"] = 10;
            bb.Add(content.IsValid);
            content["#ListField1"] = 0;
            bb.Add(content.IsValid);
            content["#ListField1"] = -10;
            bb.Add(content.IsValid);
            content["#ListField1"] = -101;
            bb.Add(!content.IsValid);
            content["#ListField1"] = 101;
            bb.Add(!content.IsValid);

            var i = 0;
            foreach (var b in bb)
                Assert.IsTrue(b, "#" + i++);
        }

        [TestMethod]
        public void ContentList_XmlNamespaceCompatibility_FeatureOn_Hu()
        {
            var ns = "http://schemas.sensenet" + ".hu/SenseNet/ContentRepository/Lis" + "terTypeDefinition";
            var msg = CheckListTypeXmlNamespaceCompatibility(ns, "hu", true);
            Assert.IsTrue(msg == null, msg ?? "Ok");
        }
        [TestMethod]
        public void ContentList_XmlNamespaceCompatibility_FeatureOn_Com()
        {
            var ns = "http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition";
            var msg = CheckListTypeXmlNamespaceCompatibility(ns, "com", true);
            Assert.IsTrue(msg == null, msg ?? "Ok");
        }
        [TestMethod]
        public void ContentList_XmlNamespaceCompatibility_FeatureOn_X()
        {
            var ns = "howmanyeggsintheeasterbasket";
            var msg = CheckListTypeXmlNamespaceCompatibility(ns, "com", true);
            Assert.IsFalse(msg == null, msg ?? "Forbidden namespace (x) is accepted");
        }
        [TestMethod]
        public void ContentList_XmlNamespaceCompatibility_FeatureOff_Hu()
        {
            var ns = "http://schemas.sensenet" + ".hu/SenseNet/ContentRepository/Lis" + "terTypeDefinition";
            var msg = CheckListTypeXmlNamespaceCompatibility(ns, "hu", false);
            Assert.IsFalse(msg == null, "Forbidden namespace (hu) is accepted");
        }
        [TestMethod]
        public void ContentList_XmlNamespaceCompatibility_FeatureOff_Com()
        {
            var ns = "http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition";
            var msg = CheckListTypeXmlNamespaceCompatibility(ns, "com", false);
            Assert.IsTrue(msg == null, msg ?? "Ok");
        }
        [TestMethod]
        public void ContentList_XmlNamespaceCompatibility_FeatureOff_X()
        {
            var ns = "howmanyeggsintheeasterbasket";
            var msg = CheckListTypeXmlNamespaceCompatibility(ns, "x", false);
            Assert.IsFalse(msg == null, "Forbidden namespace (x) is accepted");
        }

        private void SetBackwardCompatibilityXmlNamespaces(bool newValue)
        {
            var pt = new PrivateType(typeof(RepositoryConfiguration));
            pt.SetStaticField("_backwardCompatibilityXmlNamespaces", newValue);
        }
        private string CheckListTypeXmlNamespaceCompatibility(string xmlNamespace, string namespaceName, bool featureEnabled)
        {
            var compat = RepositoryConfiguration.BackwardCompatibilityXmlNamespaces;
            SetBackwardCompatibilityXmlNamespaces(featureEnabled);

            var fieldName = "#ListField1";
            string listDef = String.Format(@"<ContentListDefinition xmlns='{0}'><Fields>
                <ContentListField name='{1}' type='ShortText' /></Fields></ContentListDefinition>", xmlNamespace, fieldName);

            string listPath = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
            if (Node.Exists(listPath))
                Node.ForceDelete(listPath);

            ContentList list;
            int listId = 0;

            try
            {
                list = new ContentList(this.TestRoot);
                list.Name = "Cars";
                list.ContentListDefinition = listDef;
                list.Save();
                listId = list.Id;
                list = Node.Load<ContentList>(listId);
            }
            catch (Exception e)
            {
                SetBackwardCompatibilityXmlNamespaces(compat);
                return String.Concat("Cannot create List (", namespaceName, "): ", e.Message);
            }

            var fieldValue = Guid.NewGuid().ToString();
            var content = Content.CreateNew("Car", list, "XmlNamespaceCompatibilityContent");

            if (!content.Fields.ContainsKey(fieldName))
            {
                SetBackwardCompatibilityXmlNamespaces(compat);
                return String.Concat("Missing field (", namespaceName, ")");
            }

            content[fieldName] = fieldValue;

            try
            {
                content.Save();
            }
            catch (Exception e)
            {
                SetBackwardCompatibilityXmlNamespaces(compat);
                var msg = String.Concat("Cannot save a ListItem (", namespaceName, "): ", e.Message, e.StackTrace.Replace("\r", " ").Replace("\n", " "));
Debug.WriteLine(msg);
                return msg;
            }

            var id = content.Id;
            try
            {
                content = Content.Load(id);
            }
            catch (Exception e)
            {
                SetBackwardCompatibilityXmlNamespaces(compat);
                return String.Concat("Cannot load back a ListItem (", namespaceName, "): ", e.Message);
            }

            var loadedValue = (string)content[fieldName];

            //content.Delete();
            Node.ForceDelete(id);

            SetBackwardCompatibilityXmlNamespaces(compat);

            if (loadedValue != fieldValue)
                return String.Concat("Inconsistent field value (", namespaceName, ")");

            if(list.ContentListDefinition != listDef)
                return String.Concat("List definition xml is modified (", namespaceName, ")");

            return null;
        }

        [TestMethod]
        public void ContentList_DeleteAndUnregister_Bug1648()
        {
            string listDef1 = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#CDU_ListField' type='Integer' />
	</Fields>
</ContentListDefinition>
";
            string listDef2 = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#CDU_ListField' type='ShortText' />
	</Fields>
</ContentListDefinition>
";

            ContentList list;
            Content content;
            var listName = "ContentList_DeleteAndUnregister";
            var path = RepositoryPath.Combine(this.TestRoot.Path, listName);

            //----------------

            if (Node.Exists(path))
                Node.ForceDelete(path);

            list = new ContentList(this.TestRoot);
            list.Name = listName;
            list.ContentListDefinition = listDef1;
            list.AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") };
            list.Save();

            content = Content.CreateNew("Car", list, "TestCar");
            content["#CDU_ListField"] = 123;
            content.Save();

            //----------------

            if (Node.Exists(path))
                Node.ForceDelete(path);

            list = new ContentList(this.TestRoot);
            list.Name = listName;
            list.ContentListDefinition = listDef2;
            list.AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") };
            list.Save();

            content = Content.CreateNew("Car", list, "TestCar");
            content["#CDU_ListField"] = "Sample data";
            content.Save();
        }
        [TestMethod]
        public void ContentList_DeleteAndUnregister_InTree()
        {
            var listDefs = new [] { @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#CDUInTree_ListField1' type='Integer' />
	</Fields>
</ContentListDefinition>
",
@"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#CDUInTree_ListField2' type='Integer' />
	</Fields>
</ContentListDefinition>
"};
            var c = Content.CreateNew("Folder", TestRoot, Guid.NewGuid().ToString());
            c.Save();
            var rootFolder = c.ContentHandler;

            var listTypes = new List<ContentListType>();
            foreach (var listDef in listDefs)
            {
                var list = new ContentList(rootFolder);
                list.Name = Guid.NewGuid().ToString();
                list.ContentListDefinition = listDef;
                list.Save();
                if(list.ContentListType == null)
                    Assert.Inconclusive();
                listTypes.Add(list.ContentListType);
            }

            rootFolder.ForceDelete();

            var count = 0;
            foreach (var listType in listTypes)
                if (ActiveSchema.ContentListTypes[listType.Name] != null)
                    count++;

            Assert.IsTrue(count == 0, String.Format("There is/are {0} ContentListType. Expected: 0", count));
        }

        [TestMethod]
        [Description("WARNING: Do not add this test to any test lists!")]
        public void ContentList_Concurrent_AddStringField()
        {
            string listDef0 = @"<?xml version='1.0' encoding='utf-8'?>
                <ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	                <Fields>
		                <ContentListField name='#StringField1' type='ShortText' />
	                </Fields>
                </ContentListDefinition>
                ";

            string path = RepositoryPath.Combine(this.TestRoot.Path, "Cars");
            if (Node.Exists(path))
                Node.ForceDelete(path);

            var list = new ContentList(this.TestRoot);
            list.Name = "Cars";
            list.ContentListDefinition = listDef0;
            list.AllowedChildTypes = new ContentType[] { ContentType.GetByName("Car") };

            list.Save();
            var listId = list.Id;

            Node car = new GenericContent(list, "Car");
            car.Name = "Trabant Tramp";
            car["#String_0"] = "ABC 34-78";
            car.Save();

            var t1 = new System.Threading.Thread(ContentList_Concurrent_AddStringField_Thread);
            var t2 = new System.Threading.Thread(ContentList_Concurrent_AddStringField_Thread);
            t1.Start();
            t2.Start();

            var startingTime = DateTime.Now;
            while (counter < 2)
            {
                System.Threading.Thread.Sleep(1000);
                if ((DateTime.Now - startingTime).TotalSeconds > 10)
                    break;
            }
            if (t1.ThreadState == System.Threading.ThreadState.Running)
                t1.Abort();
            if (t2.IsAlive)
                t2.Abort();

            Assert.IsTrue(counter == 2);
        }

        public int counter;
        private void ContentList_Concurrent_AddStringField_Thread()
        {
            try
            {
                string listDef1 = @"<?xml version='1.0' encoding='utf-8'?>
                <ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	                <Fields>
		                <ContentListField name='#StringField1' type='ShortText' />
		                <ContentListField name='#StringField2' type='ShortText' />
	                </Fields>
                </ContentListDefinition>
                ";
                var path = RepositoryPath.Combine(TestRoot.Path, "Cars");
                var list = Node.Load<ContentList>(path);
                list.ContentListDefinition = listDef1;

                list.Save();
            }
            catch (Exception e)
            {
                while (e != null)
                {
                    Debug.WriteLine(String.Format("@> T{0} Test ------------------------------- ERROR: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, e.Message));
                    e = e.InnerException;
                }
            }

            counter++;
        }

		//[TestMethod]
        //public void ContentList_ContentListTypeName()
		//{
		//    string name;
		//    IDictionary<DataType, int> usedSlots = new Dictionary<DataType, int>();
		//    Dictionary<DataType, int> slots;
		//    foreach (DataType s in Enum.GetValues(typeof(DataType)))
		//        usedSlots.Add(s, 0);

		//    usedSlots[DataType.String] = 1;
		//    usedSlots[DataType.Text] = 2;
		//    usedSlots[DataType.Int] = 3;
		//    usedSlots[DataType.Currency] = 4;
		//    usedSlots[DataType.DateTime] = 5;
		//    usedSlots[DataType.Binary] = 6;
		//    usedSlots[DataType.Reference] = 7;
		//    name = ContentListAccessor.GenerateNameFromUsedSlots(usedSlots);
        //    slots = ContentListAccessor.RestoreUsedSlotsFromName(name);
		//    Assert.IsTrue(name == "#1234567", "#1");
		//    foreach (DataType key in slots.Keys)
		//        Assert.IsTrue(slots[key] == usedSlots[key], "#2 " + key);

		//    usedSlots[DataType.String] = 0x0;
		//    usedSlots[DataType.Text] = 0x8;
		//    usedSlots[DataType.Int] = 0x3F;
		//    usedSlots[DataType.Currency] = 0x40;
		//    usedSlots[DataType.DateTime] = 0x1FFF;
		//    usedSlots[DataType.Binary] = 0x2000;
		//    usedSlots[DataType.Reference] = 0x0FFFFFFF;
        //    name = ContentListAccessor.GenerateNameFromUsedSlots(usedSlots);
        //    slots = ContentListAccessor.RestoreUsedSlotsFromName(name);
		//    //                      _--__----____--------________
		//    Assert.IsTrue(name == "#088BFC040DFFFE0002000EFFFFFFF", "#2");
		//    foreach(DataType key in slots.Keys)
		//        Assert.IsTrue(slots[key]==usedSlots[key], "#2 " + key);
		//}
	}
}
