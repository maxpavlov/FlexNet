using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Tests.ContentHandlers;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.Controls;
using SNC = SenseNet.ContentRepository;

namespace SenseNet.ContentRepository.Tests.ContentHandler
{
	[TestClass()]
    public class ContentTest : TestBase
	{
		#region NEM KITOROLNI !! //Generate ContentType from System.Type
		//public void __info()
		//{
		//    StringBuilder sb = new StringBuilder();
		//    __typeInfo(typeof(SenseNet.ContentRepository.File), sb);
		//    __typeInfo(typeof(SenseNet.ContentRepository.Folder), sb);
		//    __typeInfo(typeof(SenseNet.Portal.Page), sb);
		//    __typeInfo(typeof(SenseNet.Portal.Site), sb);
		//    __typeInfo(typeof(SenseNet.ContentRepository.OrganizationalUnit), sb);
		//    __typeInfo(typeof(SenseNet.ContentRepository.User), sb);
		//    __typeInfo(typeof(SenseNet.ContentRepository.Group), sb);
		//    __typeInfo(typeof(SenseNet.Portal.PageTemplate), sb);

		//    Assert.Inconclusive();
		//}
		//private void __typeInfo(Type type, StringBuilder sb)
		//{
		//    //<ContentType name="___" handler="___" xmlns="http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition">\r\n
		//    sb.Append("<ContentType name=\"");
		//    sb.Append(type.Name);
		//    sb.Append("\" handler=\"");
		//    sb.Append(type.FullName);
		//    sb.Append("\" xmlns=\"http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition\">\r\n");

		//    //\t<DisplayName>____</DisplayName>\r\n
		//    sb.Append("\t<DisplayName>").Append(type.Name).Append("</DisplayName>\r\n");

		//    //\t\t<Description>____</Description>\r\n
		//    sb.Append("\t\t<Description>[").Append(type.Name).Append(" description]</Description>\r\n");

		//    //\t\t<Icon>____</Icon>\r\n
		//    sb.Append("\t\t<Icon>").Append(type.Name).Append(".gif</Icon>\r\n");

		//    //\t\t<Fields>\r\n
		//    sb.Append("\t\t<Fields>\r\n");
		//    //sb.Append(type.Name + "\r\n");

		//    foreach (PropertyInfo propInfo in type.GetProperties())
		//    {
		//        if (propInfo.Name != "Properties")
		//        {
		//            string fieldType = "??";
		//            switch (propInfo.PropertyType.FullName)
		//            {
		//                case "SenseNet.ContentRepository.Group": fieldType = "ReferenceField"; break;
		//                case "SenseNet.Portal.MasterPage": fieldType = "ReferenceField"; break;
		//                case "SenseNet.Portal.Page": fieldType = "ReferenceField"; break;
		//                case "SenseNet.Portal.PageTemplate": fieldType = "ReferenceField"; break;
		//                case "SenseNet.ContentRepository.Storage.BinaryData": fieldType = "BinaryField"; break;
		//                case "SenseNet.ContentRepository.Storage.Node": fieldType = "ReferenceField"; break;
		//                case "SenseNet.ContentRepository.Storage.NodeList`1[[SenseNet.ContentRepository.Storage.Node]]": fieldType = "ReferenceField"; break;
		//                case "SenseNet.ContentRepository.Storage.NodeList`1[[SenseNet.ContentRepository.Storage.Security.Group]]": fieldType = "ReferenceField"; break;
		//                case "SenseNet.ContentRepository.Storage.NodeList`1[[SenseNet.ContentRepository.Storage.Security.User]]": fieldType = "ReferenceField"; break;
		//                case "SenseNet.ContentRepository.Storage.Schema.NodeType": fieldType = "NodeTypeField"; break;
		//                case "SenseNet.ContentRepository.Storage.Security.Group": fieldType = "ReferenceField"; break;
		//                case "SenseNet.ContentRepository.Storage.Security.LockHandler": fieldType = "LockField"; break;
		//                case "SenseNet.ContentRepository.Storage.Security.SecurityHandler": fieldType = "SecurityField"; break;
		//                case "SenseNet.ContentRepository.Storage.Security.User": fieldType = "ReferenceField"; break;
		//                case "SenseNet.ContentRepository.Storage.VersionNumber": fieldType = "VersionField"; break;
		//                case "SenseNet.Portal.Site": fieldType = "ReferenceField"; break;
		//                case "SenseNet.ContentRepository.User": fieldType = "ReferenceField"; break;
		//                case "System.Boolean": fieldType = "BooleanField"; break;
		//                case "System.DateTime": fieldType = "DateTimeField"; break;
		//                case "System.Int32": fieldType = "IntegerField"; break;
		//                case "System.Int64": fieldType = "NumberField"; break;
		//                case "System.String": fieldType = "ShortTextField"; break;
		//                default:
		//                    if (propInfo.PropertyType.IsGenericType)
		//                    //if(propInfo.PropertyType.IsSubclassOf(typeof(List<Node>)))
		//                        fieldType = "ReferenceField";
		//                    else
		//                        fieldType = "-- Unknown Field --";
		//                    break;
		//            }
		//            //sb.Append("\t" + propInfo.Name + "\t" + propInfo.PropertyType.FullName + "\t" + fieldType + "\r\n");

		//            //\t\t\t<Field name="____FieldName____" type="ShortText">\r\n
		//            sb.Append("\t\t\t<Field name=\"").Append(propInfo.Name).Append("\" type=\"").Append(fieldType).Append("\">\r\n");
		//            //\t\t\t\t<DisplayName>____</DisplayName>\r\n
		//            sb.Append("\t\t\t\t<DisplayName>").Append(propInfo.Name).Append("</DisplayName>\r\n");

		//            //\t\t\t\t<Description>____</Description>\r\n
		//            sb.Append("\t\t\t\t<Description>[").Append(propInfo.Name).Append(" description]</Description>\r\n");

		//            //\t\t\t\t<Icon>____</Icon>\r\n
		//            sb.Append("\t\t\t\t<Icon>field.gif</Icon>\r\n");

		//            //\t\t\t\t<Bind property="____PropertyName____" />\r\n
		//            sb.Append("\t\t\t\t<Bind property=\"").Append(propInfo.Name).Append("\" />\r\n");

		//            if (propInfo.CanWrite)
		//            {
		//                //\t\t\t\t<Configuration></Configuration>\r\n
		//                sb.Append("\t\t\t\t<Configuration></Configuration>\r\n");
		//            }
		//            else
		//            {
		//                //\t\t\t\t<Configuration>
		//                sb.Append("\t\t\t\t<Configuration>\r\n");
		//                //\t\t\t\t\t<ReadOnly>true</ReadOnly>
		//                sb.Append("\t\t\t\t\t<ReadOnly>true</ReadOnly>\r\n");
		//                //\t\t\t\t</Configuration>\r\n
		//                sb.Append("\t\t\t\t</Configuration>\r\n");
		//            }
		//            //\t\t\t</Field>\r\n
		//            sb.Append("\t\t\t</Field>\r\n");

		//        }
		//    }

		//    //\t</Fields>\r\n
		//    sb.Append("\t</Fields>\r\n");

		//    //</ContentType>\r\n
		//    sb.Append("</ContentType>\r\n");
		//}
		#endregion

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

		private static string _testRootName = "_RepositoryTest_ContentTest";
		private static string __testRootPath = String.Concat("/Root/", _testRootName);
		private Folder __testRoot;
		private Folder _testRoot
		{
			get
			{
				if (__testRoot == null)
				{
					__testRoot = (Folder)Node.LoadNode(__testRootPath);
					if (__testRoot == null)
					{
						Folder folder = new Folder(Repository.Root);
						folder.Name = _testRootName;
						folder.Save();
						__testRoot = (Folder)Node.LoadNode(__testRootPath);
					}
				}
				return __testRoot;
			}
		}

        [ClassCleanup]
        public static void RemoveContentTypes()
        {
            if (Node.Exists(__testRootPath))
                Node.ForceDelete(__testRootPath);
            ContentType ct;
            ct = ContentType.GetByName("NonGeneric");
            if (ct != null)
                ct.Delete();
            ct = ContentType.GetByName("Automobile");
            if (ct != null)
                ct.Delete();
        }

		[TestMethod()]
		public void Content_UsingFields_WriteProperties()
		{
			string[] propset0 = new string[] { "Trabant", "Netudki" };
			string[] propset1 = new string[] { "Honda", "Gyeby" };
			string[] propset2 = new string[] { "Ferrari", "Kavics" };

			Node automobileNode = LoadOrCreateAutomobileAndSave(AutomobileHandler.ExtendedCTD, "Automobile12", propset0[0], propset0[1]);
			SNC.Content automobileContent = SNC.Content.Create(automobileNode);

			//-- Betoltes utan: 
			//-- A field-ek erteke megegyezik a teljes es a gyors eleressel is (propset0). 
			//-- A property-k es a Field.OriginalValues alapallapotban (propset0)
			Assert.IsTrue((string)automobileContent["Manufacturer"] == propset0[0], "#01");
			Assert.IsTrue((string)automobileContent["Driver"] == propset0[1], "#02");
			Assert.IsTrue((string)automobileContent.Fields["Manufacturer"].GetData() == propset0[0], "#03");
			Assert.IsTrue((string)automobileContent.Fields["Driver"].GetData() == propset0[1], "#04");
			Assert.IsTrue((string)automobileContent.Fields["Manufacturer"].OriginalValue == propset0[0], "#05");
			Assert.IsTrue((string)automobileContent.Fields["Driver"].OriginalValue == propset0[1], "#06");
			Assert.IsTrue((string)automobileNode["Manufacturer"] == propset0[0], "#07");
			Assert.IsTrue((string)automobileNode["Driver"] == propset0[1], "#08");

			//-- Field set (teljes eleresu) utan: 
			//-- A field-ek erteke felveszi az uj erteket, de megegyezik a teljes es a gyors eleressel is (propset1). 
			//-- A property-k es a Field.OriginalValues alapallapotban (propset0)
			automobileContent.Fields["Manufacturer"].SetData(propset1[0]);
			automobileContent.Fields["Driver"].SetData(propset1[1]);
			Assert.IsTrue((string)automobileContent["Manufacturer"] == propset1[0], "#11");
			Assert.IsTrue((string)automobileContent["Driver"] == propset1[1], "#12");
			Assert.IsTrue((string)automobileContent.Fields["Manufacturer"].GetData() == propset1[0], "#13");
			Assert.IsTrue((string)automobileContent.Fields["Driver"].GetData() == propset1[1], "#14");
			Assert.IsTrue((string)automobileContent.Fields["Manufacturer"].OriginalValue == propset0[0], "#15");
			Assert.IsTrue((string)automobileContent.Fields["Driver"].OriginalValue == propset0[1], "#16");
			Assert.IsTrue((string)automobileNode["Manufacturer"] == propset0[0], "#17");
			Assert.IsTrue((string)automobileNode["Driver"] == propset0[1], "#18");

			//-- Field set (gyors eleresu) utan: 
			//-- A field-ek erteke felveszi az uj erteket, de megegyezik a teljes es a gyors eleressel is (propset2). 
			//-- A property-k es a Field.OriginalValues alapallapotban (propset0)
			automobileContent["Manufacturer"] = propset2[0];
			automobileContent["Driver"] = propset2[1];
			Assert.IsTrue((string)automobileContent["Manufacturer"] == propset2[0], "#21");
			Assert.IsTrue((string)automobileContent["Driver"] == propset2[1], "#22");
			Assert.IsTrue((string)automobileContent.Fields["Manufacturer"].GetData() == propset2[0], "#23");
			Assert.IsTrue((string)automobileContent.Fields["Driver"].GetData() == propset2[1], "#24");
			Assert.IsTrue((string)automobileContent.Fields["Manufacturer"].OriginalValue == propset0[0], "#25");
			Assert.IsTrue((string)automobileContent.Fields["Driver"].OriginalValue == propset0[1], "#26");
			Assert.IsTrue((string)automobileNode["Manufacturer"] == propset0[0], "#27");
			Assert.IsTrue((string)automobileNode["Driver"] == propset0[1], "#28");

			//-- Save utan: 
			//-- A field-ek erteke nem valtozik (propset2). 
			//-- A property-kbe es a Field.OriginalValues-be beirodik a Field.Values (propset2)
			automobileContent.Save();
			Assert.IsTrue((string)automobileContent["Manufacturer"] == propset2[0], "#31");
			Assert.IsTrue((string)automobileContent["Driver"] == propset2[1], "#32");
			Assert.IsTrue((string)automobileContent.Fields["Manufacturer"].GetData() == propset2[0], "#33");
			Assert.IsTrue((string)automobileContent.Fields["Driver"].GetData() == propset2[1], "#34");
			Assert.IsTrue((string)automobileContent.Fields["Manufacturer"].OriginalValue == propset2[0], "#35");
			Assert.IsTrue((string)automobileContent.Fields["Driver"].OriginalValue == propset2[1], "#36");
			Assert.IsTrue((string)automobileNode["Manufacturer"] == propset2[0], "#37");
			Assert.IsTrue((string)automobileNode["Driver"] == propset2[1], "#38");
		}
		[TestMethod()]
		public void Content_UsingFields_WriteNodeAttribute()
		{
			string contentTypeDef = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Name' type='ShortText' />
						<Field name='Path' type='ShortText' />
						<Field name='Created' type='WhoAndWhen'>
							<Bind property='CreatedBy' />
							<Bind property='CreationDate' />
						</Field>
						<Field name='Modified' type='WhoAndWhen'>
							<Bind property='ModifiedBy' />
							<Bind property='ModificationDate' />
						</Field>
						<Field name='Manufacturer' type='ShortText' />
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>";
            ContentTypeInstaller.InstallContentType(contentTypeDef);

			Node automobileNode = Node.LoadNode(String.Concat(_testRoot.Path, "/Automobile12"));
			if (automobileNode != null)
				automobileNode.ForceDelete();


			automobileNode = new AutomobileHandler(_testRoot);
			automobileNode.Name = "Automobile12";
			automobileNode["Manufacturer"] = "Honda";
			automobileNode["Driver"] = "Gyeby";
			automobileNode.Save();

			string path = automobileNode.Path;

			SNC.Content automobileContent = SNC.Content.Create(automobileNode);
			automobileContent["Index"] = 987;
			automobileContent.Save();

			automobileNode = Node.LoadNode(path);
			int index = automobileNode.Index;
			automobileNode.ForceDelete();

			Assert.IsTrue(index == 987);
		}
		[TestMethod()]
		public void Content_Create1000()
		{
			Node automobileNode = LoadOrCreateAutomobileAndSave(AutomobileHandler.ExtendedCTD, "Automobile12", "Trabant", "Netudki");
			SNC.Content automobileContent = SNC.Content.Create(automobileNode);

			Stopwatch stopper = Stopwatch.StartNew();
			int count = 1000;
			for (int i = 0; i < count; i++)
				automobileContent = SNC.Content.Create(automobileNode);
			stopper.Stop();

			TimeSpan time = TimeSpan.FromTicks(stopper.ElapsedTicks / count);
			double ms = time.TotalMilliseconds;
            //Trace.WriteLine("Content.Create average milliseconds: " + ms);
		}

		[TestMethod]
		[ExpectedException(typeof(KeyNotFoundException))]
		public void Content_MissingField()
		{
			SNC.Content content = SNC.Content.Load(_testRoot.Path);
			Field field = content.Fields["MissingField"];
		}
		[TestMethod()]
		public void Content_FieldControl()
		{
			Node automobileNode = LoadOrCreateAutomobileAndSave(AutomobileHandler.ExtendedCTD, "Automobile12", "Honda", "Gyeby");
			SNC.Content automobileContent = SNC.Content.Create(automobileNode);
			Field manuField = automobileContent.Fields["Manufacturer"];
			Field drivField = automobileContent.Fields["Driver"];

			ShortText shortText = new ShortText();
			ShortTextAccessor shortTextAcc = new ShortTextAccessor(shortText);
			ShortText shortTextEditor = new ShortText();
			ShortTextAccessor shortTextEditorAcc = new ShortTextAccessor(shortTextEditor);

			//-- contentview szimulacio: RegisterFieldControl
			shortTextAcc.ConnectToField(manuField);
			shortTextEditorAcc.ConnectToField(drivField);

			Assert.IsTrue(shortTextAcc.Text == (string)automobileContent["Manufacturer"], "#01");
			Assert.IsTrue(shortTextEditorAcc.InputTextBox.Text == (string)automobileContent["Driver"], "#02");

			shortTextEditorAcc.InputTextBox.Text = "Netudki";

			//-- contentview szimulacio: PostData
			drivField.SetData(shortTextEditor.GetData());

			// automobileContent.IsValid?

			string result = (string)automobileContent["Driver"];
			Assert.IsTrue(result == "Netudki", "#03");
		}
        //[TestMethod()]
        //public void Content_FolderEdit()
        //{
        //    Assert.Inconclusive("field.ConnectToView");
        //    SNC.Content folderContent = SNC.Content.Create(_testRoot);
        //    List<GenericControl> controls = new List<GenericControl>();
        //    foreach (string key in folderContent.Fields.Keys)
        //    {
        //        Field field = folderContent.Fields[key];
        //        GenericControl control = new GenericControl();
        //        controls.Add(control);
        //        field.ConnectToView(control);
        //    }
        //    StringBuilder sb = new StringBuilder();
        //    foreach (GenericControl control in controls)
        //        sb.Append(control.FieldName).Append(" = ").Append(control.label1.Text).Append("\r\n");
        //    Trace.WriteLine(sb);

        //    -- Ha nincs hiba: sikeres
        //}
		[TestMethod()]
		public void Content_UsingFieldControls_ReadComplexProperties()
		{
			Node automobileNode = CreateNewAutomobileAndSave(@"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<Fields>
		<Field name='Modified' type='WhoAndWhen'>
			<Bind property='ModifiedBy' />
			<Bind property='ModificationDate' />
		</Field>
		<Field name='Manufacturer' type='ShortText'>
			<Configuration>
				<Compulsory>true</Compulsory>
				<MaxLength>100</MaxLength>
				<Format>TitleCase</Format>
			</Configuration>
		</Field>
		<Field name='Driver' type='ShortText'>
			<Configuration>
				<Compulsory>true</Compulsory>
				<MaxLength>100</MaxLength>
			</Configuration>
		</Field>
	</Fields>
</ContentType>
", "Automobile12", "Trabant", "Netudki");
			SNC.Content automobileContent = SNC.Content.Create(automobileNode);

			WhoAndWhen whoAndWhenControl = new WhoAndWhen();
			WhoAndWhenAccessor whoAndWhenAcc = new WhoAndWhenAccessor(whoAndWhenControl);
			whoAndWhenControl.FieldName = "Modified";
			whoAndWhenAcc.ConnectToField(automobileContent.Fields["Modified"]);
			//automobileContent.Fields["Modified"].ConnectToView(whoAndWhenControl);
			ShortText manuControl = new ShortText();
			ShortTextAccessor manuControlAcc = new ShortTextAccessor(manuControl);
			manuControl.FieldName = "Manufacturer";
			manuControlAcc.ConnectToField(automobileContent.Fields["Manufacturer"]);
			//automobileContent.Fields["Manufacturer"].ConnectToView(manuControl);
			ShortText driverControl = new ShortText();
			ShortTextAccessor driverControlAcc = new ShortTextAccessor(driverControl);
			driverControl.FieldName = "Driver";
			driverControlAcc.ConnectToField(automobileContent.Fields["Driver"]);
			//automobileContent.Fields["Driver"].ConnectToView(driverControl);

			string who = whoAndWhenControl.label1.Text;
			string when = whoAndWhenControl.label2.Text;
			string manu = manuControlAcc.Text;
			string driver = driverControlAcc.Text;

			Assert.IsFalse(String.IsNullOrEmpty(who), "#1");
			Assert.IsFalse(String.IsNullOrEmpty(when), "#2");
			Assert.IsFalse(String.IsNullOrEmpty(manu), "#3");
			Assert.IsFalse(String.IsNullOrEmpty(driver), "#4");
		}
		[TestMethod()]
		public void Content_UsingFieldControls_ReadWriteWithConversion()
		{
			Node automobileNode = LoadOrCreateAutomobile(@"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<Fields>
		<Field name='Manufacturer' type='ShortText'>
			<Configuration>
				<Compulsory>true</Compulsory>
				<MaxLength>100</MaxLength>
				<Format>TitleCase</Format>
			</Configuration>
		</Field>
		<Field name='Driver' type='ShortText'>
			<Configuration>
				<Compulsory>true</Compulsory>
				<MaxLength>100</MaxLength>
			</Configuration>
		</Field>
		<Field name='BodyColor' type='Color'>
			<Bind property='BodyColor' />
		</Field>
	</Fields>
</ContentType>
", "Automobile12", "Trabant", "Netudki");
			SNC.Content automobileContent = SNC.Content.Create(automobileNode);

			automobileContent["BodyColor"] = Color.Red;
			automobileContent.Save();
			automobileContent = SNC.Content.Load(automobileContent.ContentHandler.Id);

			ColorEditorControl colorControl = new ColorEditorControl();
			ColorControlAccessor colorControlAcc = new ColorControlAccessor(colorControl);
			colorControl.FieldName = "BodyColor";
			colorControlAcc.ConnectToField(automobileContent.Fields["BodyColor"]);
			ShortText manuControl = new ShortText();
			ShortTextAccessor manuControlAcc = new ShortTextAccessor(manuControl);
			manuControl.FieldName = "Manufacturer";
			manuControlAcc.ConnectToField(automobileContent.Fields["Manufacturer"]);
			ShortText driverControl = new ShortText();
			ShortTextAccessor driverControlAcc = new ShortTextAccessor(driverControl);
			driverControl.FieldName = "Driver";
			driverControlAcc.ConnectToField(automobileContent.Fields["Driver"]);

			string colorString = colorControl.textBox1.Text;
			Color color = colorControl.textBox1.BackColor;
			string manu = manuControlAcc.Text;
			string driver = driverControlAcc.Text;
			
			Assert.IsTrue(colorString == "#FF0000", "#1");
			Assert.IsTrue(color == ColorField.ColorFromString(ColorField.ColorToString(Color.Red)), "#2");
			Assert.IsTrue(manu == "Trabant", "#2");
			Assert.IsTrue(driver == "Netudki", "#2");
			//-- Ha nincs hiba: sikeres
		}

		[TestMethod()]
		public void Content_NonGeneric_FieldReadWrite()
		{
            ContentTypeInstaller.InstallContentType(NonGenericHandler.ContentTypeDefinition);
			SNC.Content content = SNC.Content.CreateNew("NonGeneric", _testRoot, "Test123");
			content["Index"] = 123;
			content["TestString"] = "TestString123";
			content.Save();

			Node node = Node.LoadNode(RepositoryPath.Combine(_testRoot.Path, "Test123"));
			int index = node.Index;
			string testString = (string)node["TestString"];
			node.Delete();

			Assert.IsTrue(index == 123, "#1");
			Assert.IsTrue(testString == "TestString123", "#2");
		}

		[TestMethod]
		public void Content_CarContentChoiceField()
		{
			SNC.Content content = SNC.Content.Load(String.Concat(_testRoot.Path, "/Car123"));
			if (content == null)
				content = SNC.Content.CreateNew("Car", _testRoot, "Car123");
			content["Style"] = new List<string>(new string[] { "Sedan" });
			content.Save();
			content = SNC.Content.Load(String.Concat(_testRoot.Path, "/Car123"));

			Assert.IsTrue(((List<string>)content["Style"])[0] == "Sedan");
		}

		[TestMethod]
		public void Content_CreateList()
		{
			string contentTypeName = "Automobile";
			if (ContentTypeManager.Current.GetContentTypeByName(contentTypeName) == null)
				ContentTypeInstaller.InstallContentType(AutomobileHandler.ExtendedCTD);

			var idList = new List<int>();

			for (int i = 1; i <= 5; i++)
			{
				string contentName = "AutoListItem" + i;
				SNC.Content auto = SNC.Content.Load(RepositoryPath.Combine(_testRoot.Path, contentName));
				if (auto == null)
					auto = SNC.Content.CreateNew(contentTypeName, _testRoot, contentName);
				auto["Manufacturer"] = "Manuf" + i;
				auto.Save();
				idList.Add(auto.Id);
			}

			//----

			NodeQuery query = new NodeQuery();
			query.Add(new TypeExpression(ActiveSchema.NodeTypes[contentTypeName]));
			query.Add(new StringExpression(StringAttribute.Name, StringOperator.StartsWith, "AutoListItem"));
			IEnumerable<SNC.Content> contentList = SNC.Content.Query(query);
			var contentListCount = contentList.ToList().Count;

			//----

			foreach (var id in idList)
				Node.ForceDelete(id);

			//----

			Assert.IsTrue(contentListCount == 5);
		}

        [TestMethod]
        public void Content_FieldTitleFallback()
        {
            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields>
                                    <Field name='Name' type='ShortText'/>
									<Field name='Manufacturer' type='ShortText'/>
									<Field name='Driver' type='ShortText'><DisplayName>Automobile.Driver.DisplayName</DisplayName></Field>
								</Fields>
							</ContentType>",
                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile1' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields>
									<Field name='Manufacturer' type='ShortText'><DisplayName>Automobile1.Manufacturer.DisplayName</DisplayName></Field>
									<Field name='Driver' type='ShortText'/>
								</Fields>
							</ContentType>");

            var ct = ContentType.GetByName("Automobile");
            var ct1 = ContentType.GetByName("Automobile1");
            var content = Content.CreateNew("Automobile", _testRoot, new Guid().ToString());
            var content1 = Content.CreateNew("Automobile1", _testRoot, new Guid().ToString());

            var automobileTypeManufacturerTitle = ct.FieldSettings.Where(f => f.Name == "Manufacturer").First().DisplayName;
            var automobile1TypeManufacturerTitle = ct1.FieldSettings.Where(f => f.Name == "Manufacturer").First().DisplayName;
            var automobileTypeDriverTitle = ct.FieldSettings.Where(f => f.Name == "Driver").First().DisplayName;
            var automobile1TypeDriverTitle = ct1.FieldSettings.Where(f => f.Name == "Driver").First().DisplayName;

            var automobileContentManufacturerTitle = content.Fields["Manufacturer"].DisplayName;
            var automobile1ContentManufacturerTitle = content1.Fields["Manufacturer"].DisplayName;
            var automobileContentDriverTitle = content.Fields["Driver"].DisplayName;
            var automobile1ContentDriverTitle = content1.Fields["Driver"].DisplayName;

            Assert.IsTrue(automobileTypeManufacturerTitle == null, "#1");
            Assert.IsTrue(automobile1TypeManufacturerTitle == "Automobile1.Manufacturer.DisplayName", "#2");
            Assert.IsTrue(automobileTypeDriverTitle == "Automobile.Driver.DisplayName", "#3");
            Assert.IsTrue(automobile1TypeDriverTitle == "Automobile.Driver.DisplayName", "#4");

            Assert.IsTrue(automobileContentManufacturerTitle == "Manufacturer", "#5");
            Assert.IsTrue(automobile1ContentManufacturerTitle == "Automobile1.Manufacturer.DisplayName", "#6");
            Assert.IsTrue(automobileContentDriverTitle == "Automobile.Driver.DisplayName", "#7");
            Assert.IsTrue(automobile1ContentDriverTitle == "Automobile.Driver.DisplayName", "#8");
        }

		private string InstallContentType(string contentTypeDefInstall, string contentTypeDefModify)
		{
			XmlDocument schema = new XmlDocument();
			schema.LoadXml(@"<?xml version='1.0' encoding='utf-8' ?>
<StorageSchema xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/Storage/Schema'>
	<UsedPropertyTypes>
		<PropertyType itemID='1' name='Binary' dataType='Binary' mapping='0' />
		<PropertyType itemID='2' name='VersioningMode' dataType='Int' mapping='0' />
		<PropertyType itemID='3' name='Make' dataType='String' mapping='0' />
		<PropertyType itemID='4' name='Model' dataType='String' mapping='1' />
		<PropertyType itemID='5' name='Style' dataType='String' mapping='2' />
		<PropertyType itemID='6' name='Color' dataType='String' mapping='3' />
		<PropertyType itemID='7' name='EngineSize' dataType='String' mapping='4' />
		<PropertyType itemID='8' name='Power' dataType='String' mapping='5' />
		<PropertyType itemID='9' name='Price' dataType='String' mapping='6' />
		<PropertyType itemID='10' name='Description' dataType='Text' mapping='0' />
		<PropertyType itemID='11' name='Enabled' dataType='Int' mapping='1' />
		<PropertyType itemID='12' name='Domain' dataType='String' mapping='7' />
		<PropertyType itemID='13' name='Email' dataType='String' mapping='8' />
		<PropertyType itemID='14' name='FullName' dataType='String' mapping='9' />
		<PropertyType itemID='15' name='PasswordHash' dataType='String' mapping='10' />
		<PropertyType itemID='16' name='Memberships' dataType='Binary' mapping='1' />
		<PropertyType itemID='17' name='PendingUserLang' dataType='String' mapping='11' />
		<PropertyType itemID='18' name='Language' dataType='Int' mapping='2' />
		<PropertyType itemID='19' name='Url' dataType='String' mapping='12' />
		<PropertyType itemID='20' name='AuthenticationType' dataType='String' mapping='13' />
		<PropertyType itemID='21' name='StartPage' dataType='String' mapping='14' />
		<PropertyType itemID='22' name='LoginPage' dataType='String' mapping='15' />
		<PropertyType itemID='23' name='StatisticalLog' dataType='Int' mapping='3' />
		<PropertyType itemID='24' name='AuditLog' dataType='Int' mapping='4' />
		<PropertyType itemID='26' name='PageNameInMenu' dataType='String' mapping='16' />
		<PropertyType itemID='27' name='Hidden' dataType='Int' mapping='6' />
		<PropertyType itemID='28' name='Keywords' dataType='String' mapping='17' />
		<PropertyType itemID='29' name='MetaDescription' dataType='String' mapping='18' />
		<PropertyType itemID='30' name='MetaTitle' dataType='String' mapping='19' />
		<PropertyType itemID='31' name='PageTemplateNode' dataType='Reference' mapping='0' />
		<PropertyType itemID='32' name='DefaultPortletSkin' dataType='String' mapping='20' />
		<PropertyType itemID='33' name='HiddenPageFrom' dataType='String' mapping='21' />
		<PropertyType itemID='34' name='Authors' dataType='String' mapping='22' />
		<PropertyType itemID='35' name='CustomMeta' dataType='String' mapping='23' />
		<PropertyType itemID='36' name='Comment' dataType='String' mapping='24' />
		<PropertyType itemID='37' name='PersonalizationSettings' dataType='Binary' mapping='2' />
		<PropertyType itemID='38' name='Title' dataType='String' mapping='25' />
		<PropertyType itemID='39' name='Subtitle' dataType='String' mapping='26' />
		<PropertyType itemID='40' name='Header' dataType='Text' mapping='1' />
		<PropertyType itemID='41' name='Body' dataType='Text' mapping='2' />
		<PropertyType itemID='42' name='Links' dataType='Text' mapping='3' />
		<PropertyType itemID='43' name='ContentLanguage' dataType='String' mapping='27' />
		<PropertyType itemID='44' name='Author' dataType='String' mapping='28' />
		<PropertyType itemID='45' name='ContractId' dataType='String' mapping='29' />
		<PropertyType itemID='46' name='Project' dataType='String' mapping='30' />
		<PropertyType itemID='47' name='Responsee' dataType='String' mapping='31' />
		<PropertyType itemID='48' name='Lawyer' dataType='String' mapping='32' />
		<PropertyType itemID='49' name='MasterPageNode' dataType='Reference' mapping='1' />
		<PropertyType itemID='50' name='Members' dataType='Reference' mapping='2' />
		<PropertyType itemID='51' name='Manufacturer' dataType='String' mapping='33' />
		<PropertyType itemID='52' name='Driver' dataType='String' mapping='34' />
		<PropertyType itemID='53' name='InheritableVersioningMode' dataType='Int' mapping='35' />
		<PropertyType itemID='54' name='HasApproving' dataType='Int' mapping='36' />
	</UsedPropertyTypes>
	<NodeTypeHierarchy>
		<NodeType itemID='7' name='PersonalizationFile' className='SenseNet.ContentRepository.PersonalizationFile'>
			<PropertyType name='Binary' />
			<PropertyType name='VersioningMode' />
			<PropertyType name='InheritableVersioningMode' />
			<PropertyType name='HasApproving' />
		</NodeType>
		<NodeType itemID='5' name='GenericContent' className='SenseNet.ContentRepository.GenericContent'>
			<PropertyType name='VersioningMode' />
			<PropertyType name='InheritableVersioningMode' />
			<PropertyType name='HasApproving' />
			<NodeType itemID='3' name='User' className='SenseNet.ContentRepository.User'>
				<PropertyType name='VersioningMode' />
				<PropertyType name='Enabled' />
				<PropertyType name='Domain' />
				<PropertyType name='Email' />
				<PropertyType name='FullName' />
				<PropertyType name='PasswordHash' />
				<PropertyType name='Memberships' />
			</NodeType>
			<NodeType itemID='1' name='Folder' className='SenseNet.ContentRepository.Folder'>
				<PropertyType name='VersioningMode' />
				<NodeType itemID='16' name='Page' className='SenseNet.Portal.Page'>
					<PropertyType name='Binary' />
					<PropertyType name='PageNameInMenu' />
					<PropertyType name='Hidden' />
					<PropertyType name='Keywords' />
					<PropertyType name='MetaDescription' />
					<PropertyType name='MetaTitle' />
					<PropertyType name='PageTemplateNode' />
					<PropertyType name='DefaultPortletSkin' />
					<PropertyType name='HiddenPageFrom' />
					<PropertyType name='Authors' />
					<PropertyType name='CustomMeta' />
					<PropertyType name='Comment' />
					<PropertyType name='PersonalizationSettings' />
				</NodeType>
				<NodeType itemID='15' name='OrganizationalUnit' className='SenseNet.ContentRepository.OrganizationalUnit'>
				</NodeType>
				<NodeType itemID='14' name='Site' className='SenseNet.Portal.Site'>
					<PropertyType name='Description' />
					<PropertyType name='PendingUserLang' />
					<PropertyType name='Language' />
					<PropertyType name='Url' />
					<PropertyType name='AuthenticationType' />
					<PropertyType name='StartPage' />
					<PropertyType name='LoginPage' />
					<PropertyType name='StatisticalLog' />
					<PropertyType name='AuditLog' />
				</NodeType>
			</NodeType>
			<NodeType itemID='10' name='WebContentDemo' className='SenseNet.ContentRepository.GenericContent'>
				<PropertyType name='Keywords' />
				<PropertyType name='Title' />
				<PropertyType name='Subtitle' />
				<PropertyType name='Header' />
				<PropertyType name='Body' />
				<PropertyType name='Links' />
				<PropertyType name='ContentLanguage' />
				<PropertyType name='Author' />
			</NodeType>
			<NodeType itemID='9' name='File' className='SenseNet.ContentRepository.File'>
				<PropertyType name='Binary' />
				<NodeType itemID='13' name='PageTemplate' className='SenseNet.Portal.PageTemplate'>
					<PropertyType name='MasterPageNode' />
				</NodeType>
				<NodeType itemID='12' name='Contract' className='SenseNet.ContentRepository.File'>
					<PropertyType name='Description' />
					<PropertyType name='Language' />
					<PropertyType name='Keywords' />
					<PropertyType name='ContractId' />
					<PropertyType name='Project' />
					<PropertyType name='Responsee' />
					<PropertyType name='Lawyer' />
				</NodeType>
				<NodeType itemID='11' name='MasterPage' className='SenseNet.Portal.MasterPage' />
			</NodeType>
			<NodeType itemID='8' name='Car' className='SenseNet.ContentRepository.GenericContent'>
				<PropertyType name='Make' />
				<PropertyType name='Model' />
				<PropertyType name='Style' />
				<PropertyType name='Color' />
				<PropertyType name='EngineSize' />
				<PropertyType name='Power' />
				<PropertyType name='Price' />
				<PropertyType name='Description' />
			</NodeType>
		</NodeType>
		<NodeType itemID='4' name='ContentType' className='SenseNet.ContentRepository.Schema.ContentType'>
			<PropertyType name='Binary' />
		</NodeType>
		<NodeType itemID='2' name='Group' className='SenseNet.ContentRepository.Group'>
			<PropertyType name='VersioningMode' />
			<PropertyType name='Members' />
		</NodeType>
	</NodeTypeHierarchy>
	<PermissionTypes>
		<PermissionType itemID='1' name='See' />
		<PermissionType itemID='2' name='Open' />
		<PermissionType itemID='3' name='OpenMinor' />
		<PermissionType itemID='4' name='Save' />
		<PermissionType itemID='5' name='Publish' />
		<PermissionType itemID='6' name='ForceCheckin' />
		<PermissionType itemID='7' name='AddNew' />
		<PermissionType itemID='8' name='Approve' />
		<PermissionType itemID='9' name='Delete' />
		<PermissionType itemID='10' name='RecallOldVersion' />
		<PermissionType itemID='11' name='DeleteOldVersion' />
		<PermissionType itemID='12' name='SeePermissions' />
		<PermissionType itemID='13' name='SetPermissions' />
		<PermissionType itemID='14' name='RunApplication' />
	</PermissionTypes>
</StorageSchema>
");

			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			ed1.Load(schema);
			ed2.Load(schema);

			ContentTypeManagerAccessor ctmAcc = new ContentTypeManagerAccessor(ContentTypeManager.Current);
			ContentType cts = ctmAcc.LoadOrCreateNew(contentTypeDefInstall);
			ctmAcc.ApplyChangesInEditor(cts, ed2);

			SchemaEditorAccessor ed2Acc = new SchemaEditorAccessor(ed2);
			TestSchemaWriter wr = new TestSchemaWriter();
			ed2Acc.RegisterSchema(ed1, wr);

			if (contentTypeDefModify != null)
			{
				XmlDocument schema2 = new XmlDocument();
				schema2.LoadXml(ed2.ToXml());
				SchemaEditor ed3 = new SchemaEditor();
				ed3.Load(schema2);
				SchemaEditorAccessor ed3Acc = new SchemaEditorAccessor(ed3);


				cts = ctmAcc.LoadOrCreateNew(contentTypeDefModify);
				ctmAcc.ApplyChangesInEditor(cts, ed3);
				wr = new TestSchemaWriter();
				ed3Acc.RegisterSchema(ed2, wr);
			}

			return wr.Log;
		}
		private Node CreateNewAutomobileAndSave(string contentDefXml, string name, string manuf, string driver)
		{
			Node automobileNode = CreateNewAutomobile(contentDefXml, name, manuf, driver);
			automobileNode.Save();
			return automobileNode;
		}
		private Node CreateNewAutomobile(string contentDefXml, string name, string manuf, string driver)
		{
            ContentTypeInstaller.InstallContentType(contentDefXml);

			Node automobileNode = Node.LoadNode(String.Concat(__testRootPath, "/", name));
			if (automobileNode != null)
				automobileNode.ForceDelete();
			automobileNode = _CreateAutomobile(name);
			SetAutomobileProperties(automobileNode, manuf, driver);
			return automobileNode;
		}
		private Node LoadOrCreateAutomobileAndSave(string contentDefXml, string name, string manuf, string driver)
		{
			Node automobileNode = LoadOrCreateAutomobile(contentDefXml, name, manuf, driver);
			automobileNode.Save();
			return automobileNode;
		}
		private Node LoadOrCreateAutomobile(string contentDefXml, string name, string manuf, string driver)
		{
            ContentTypeInstaller.InstallContentType(contentDefXml);
			Node automobileNode = Node.LoadNode(String.Concat(_testRoot.Path, "/", name));
			if (automobileNode == null)
				automobileNode = _CreateAutomobile(name);
			SetAutomobileProperties(automobileNode, manuf, driver);
			return automobileNode;
		}
		private Node _CreateAutomobile(string name)
		{
			var automobileNode = new AutomobileHandler(_testRoot);
			automobileNode.Name = name;
			return automobileNode;
		}
		private void SetAutomobileProperties(Node automobileNode, string manuf, string driver)
		{
			if (automobileNode.HasProperty("Manufacturer"))
				automobileNode["Manufacturer"] = manuf;
			if (automobileNode.HasProperty("Driver"))
				automobileNode["Driver"] = driver;
		}

	}

	public class GenericControl : Control, SenseNet.Portal.UI.IFieldControl
	{
		public Label label1;
		private string _fieldName;
		private Field _field;
		private bool _readOnly;
		private string _errorMessage;
		private FieldControlRenderMode _renderMode;

		public Field Field
		{
			get { return _field; }
			set { _field = value; }
		}
		public string FieldName
		{
			get { return _fieldName; }
			set { _fieldName = value; }
		}
		public FieldControlRenderMode RenderMode
		{
			get { return _renderMode; }
			set { _renderMode = value; }
		}

		public GenericControl()
		{
			label1 = new Label();
		}

		public void SetData(object data)
		{
			label1.Text = data.ToString();
		}
		public bool ReadOnly
		{
			get { return _readOnly; }
			set { _readOnly = value; }
		}
		public bool Inline
		{
			get { return false; }
			set { return; }
		}
		public object GetData()
		{
			return label1.Text;
		}
		public void ClearError()
		{
			_errorMessage = "";
		}
		public void SetErrorMessage(string message)
		{
			_errorMessage = message;
		}
	}

	public class ColorEditorControl : Control, SenseNet.Portal.UI.IFieldControl
	{
		public TextBox textBox1;
		private string _fieldName;
		private Field _field;
		private bool _readOnly;
		private string _errorMessage;
		private FieldControlRenderMode _renderMode;

		public bool ReadOnly
		{
			get { return _readOnly; }
			set { _readOnly = value; }
		}
		public Field Field
		{
			get { return _field; }
			set { _field = value; }
		}
		public string FieldName
		{
			get { return _fieldName; }
			set { _fieldName = value; }
		}
		public FieldControlRenderMode RenderMode
		{
			get { return _renderMode; }
			set { _renderMode = value; }
		}

		public ColorEditorControl()
		{
			textBox1 = new TextBox();
		}

		public object GetData()
		{
			return ColorField.ColorFromString(textBox1.Text);
		}
		public void SetData(object data)
		{
			Color color = data == null ? Color.Empty : (Color)data;
			textBox1.Text = ColorField.ColorToString(color);
			textBox1.BackColor = color;
		}

		public void ClearError()
		{
			_errorMessage = "";
		}
		public void SetErrorMessage(string message)
		{
			_errorMessage = message;
		}

		public bool Inline
		{
			get
			{
				return false;
			}
			set
			{
				return;
			}
		}

	}
}