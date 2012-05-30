using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Tests.ContentHandlers;
using SenseNet.Portal.UI.Controls;
using SNC = SenseNet.ContentRepository;
using System.Linq;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.ContentRepository.Tests.Schema
{
    public static class Extensions
    {
        public static void ExecuteBatch(this ContentTypeInstaller installer, List<string> installedContentTypes)
        {
            var inst = new PrivateObject(installer);
            var docs = inst.GetField("_docs");
            foreach (string name in ((System.Collections.IDictionary)docs).Keys)
                if (!installedContentTypes.Contains(name))
                    installedContentTypes.Add(name);
            installer.ExecuteBatch();
        }
    }

    [TestClass]
    public class ContentInstallTest : TestBase
    {
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


        private static string _testRootName = "_RepositoryTest_ContentTypeTest";
        private static string __testRootPath = String.Concat("/Root/", _testRootName);
        private static List<string> _installedContentTypes = new List<string>();

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
            try
            {
                var expressions = (from name in _installedContentTypes
                                   where ActiveSchema.NodeTypes[name] != null
                                   select new TypeExpression(ActiveSchema.NodeTypes[name])).ToArray();
                if (expressions.Count() > 0)
                {
                    var q = new NodeQuery(ChainOperator.Or, expressions);
                    var pathsToDelete = from node in q.Execute().Nodes select node.Path;
                    pathsToDelete.OrderByDescending(p => p);
                    var paths = pathsToDelete.ToArray();
                    foreach (var path in pathsToDelete)
                        Node.ForceDelete(path);

                    ContentType ct;
                    if (Node.Exists(__testRootPath))
                        Node.ForceDelete(__testRootPath);
                    foreach (var ctName in _installedContentTypes)
                    {
                        ct = ContentType.GetByName(ctName);
                        if (ct != null)
                            ct.Delete();
                    }
                    ct = ContentType.GetByName("Automobile1");
                    if (ct != null)
                        ct.Delete();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [TestMethod]
        public void ContentType_IsDeletable()
        {
            TestTools.RemoveNodesAndType("Automobile");

            var installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
            installer.AddContentType(AutomobileHandler.ExtendedCTD);
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile5' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile6' parentType='Automobile5' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.ExecuteBatch(_installedContentTypes);

            PrivateObject x = new PrivateObject(ContentType.GetByName("Automobile5"));
            var b1 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile5"));
            var b2 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile6"));

            var c1 = SNC.Content.CreateNew("Automobile5", _testRoot, "Auto1");
            c1.Save();

            var b3 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile5"));
            var b4 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile6"));

            var c2 = SNC.Content.CreateNew("Automobile6", _testRoot, "Auto2");
            c2.Save();

            var b5 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile5"));
            var b6 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile6"));

            Node.ForceDelete(c1.Id);

            var b7 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile5"));
            var b8 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile6"));

            Node.ForceDelete(c2.Id);

            var b9 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile5"));
            var b10 = (bool)x.Invoke("IsDeletable", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, ContentType.GetByName("Automobile6"));

            Assert.IsTrue(b1, "#1");
            Assert.IsTrue(b2, "#2");
            Assert.IsFalse(b3, "#3");
            Assert.IsTrue(b4, "#4");
            Assert.IsFalse(b5, "#5");
            Assert.IsFalse(b6, "#6");
            Assert.IsFalse(b7, "#7");
            Assert.IsFalse(b8, "#8");
            Assert.IsTrue(b9, "#9");
            Assert.IsTrue(b10, "#10");
        }

        [TestMethod]
        public void ContentType_NameCannotContainsADot()
        {
            //---- valid
            var ctd = @"<ContentType name=""Valid_ContentType_Name"" parentType=""GenericContent"" handler=""SenseNet.ContentRepository.GenericContent"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
</ContentType>";
            ContentTypeInstaller.InstallContentType(ctd);
            var validIsExist = ContentType.GetByName("Valid_ContentType_Name") != null;
            if (validIsExist)
                ContentTypeInstaller.RemoveContentType("Valid_ContentType_Name");
            Assert.IsTrue(validIsExist, "#1");

            //---- invalid
            ctd = @"<ContentType name=""Invalid.ContentType.Name"" parentType=""GenericContent"" handler=""SenseNet.ContentRepository.GenericContent"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
</ContentType>";
            bool exceptionIsThrown = false;
            bool validExceptionIsThrown = false;
            try
            {
                ContentTypeInstaller.InstallContentType(ctd);
                exceptionIsThrown = false;
            }
            catch (ContentRegistrationException e)
            {
                exceptionIsThrown = true;
                validExceptionIsThrown = true;
            }
            catch (Exception e)
            {
                exceptionIsThrown = true;
            }
            var invalidIsExist = ContentType.GetByName("Invalid.ContentType.Name") != null;
            if (invalidIsExist)
                ContentTypeInstaller.RemoveContentType("Invalid.ContentType.Name");
            Assert.IsTrue(exceptionIsThrown, "#2");
            Assert.IsTrue(validExceptionIsThrown, "#3");
            Assert.IsFalse(invalidIsExist, "#4");
        }
        [TestMethod]
        public void ContentType_FieldNameCannotContainsADot()
        {
            //---- valid
            var ctd = @"<ContentType name=""Valid_ContentType_Name"" parentType=""GenericContent"" handler=""SenseNet.ContentRepository.GenericContent"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
    <Fields><Field name=""Valid_Field_Name"" type=""ShortText"" /></Fields>
</ContentType>";
            ContentTypeInstaller.InstallContentType(ctd);
            var validIsExist = ContentType.GetByName("Valid_ContentType_Name") != null;
            if (validIsExist)
                ContentTypeInstaller.RemoveContentType("Valid_ContentType_Name");
            Assert.IsTrue(validIsExist, "#1");

            //---- invalid
            ctd = @"<ContentType name=""Valid_ContentType_Namee"" parentType=""GenericContent"" handler=""SenseNet.ContentRepository.GenericContent"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
    <Fields><Field name=""Invalid.Field.Name"" type=""ShortText"" /></Fields>
</ContentType>";
            bool exceptionIsThrown = false;
            bool validExceptionIsThrown = false;
            try
            {
                ContentTypeInstaller.InstallContentType(ctd);
                exceptionIsThrown = false;
            }
            catch (ContentRegistrationException e)
            {
                exceptionIsThrown = true;
                validExceptionIsThrown = true;
            }
            catch (Exception e)
            {
                exceptionIsThrown = true;
            }
            var invalidIsExist = ContentType.GetByName("Valid_ContentType_Name") != null;
            if (invalidIsExist)
                ContentTypeInstaller.RemoveContentType("Valid_ContentType_Name");
            Assert.IsTrue(exceptionIsThrown, "#2");
            Assert.IsTrue(validExceptionIsThrown, "#3");
            Assert.IsFalse(invalidIsExist, "#4");
        }
        [TestMethod]
        public void ContentList_FieldNameCannotContainsADot()
        {
            string path = RepositoryPath.Combine(this._testRoot.Path, "Cars");
            if (Node.Exists(path))
                Node.ForceDelete(path);

            //---- valid
            string listTypeDef = @"<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields><ContentListField name='#Valid_Field_Name' type='ShortText'/></Fields>
</ContentListDefinition>
";
            var list = new ContentList(this._testRoot);
            list.Name = "Cars";
            list.ContentListDefinition = listTypeDef;
            list.Save();

            var validIsExist = Node.LoadNode(path) != null;
            if (validIsExist)
                Node.ForceDelete(path);
            Assert.IsTrue(validIsExist, "#1");

            //---- invalid
            listTypeDef = @"<ContentType name=""Valid_ContentType_Namee"" parentType=""GenericContent"" handler=""SenseNet.ContentRepository.GenericContent"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
    <Fields><Field name=""Invalid.Field.Name"" type=""ShortText"" /></Fields>
</ContentType>";
            bool exceptionIsThrown = false;
            bool validExceptionIsThrown = false;
            try
            {
                list = new ContentList(this._testRoot);
                list.Name = "Cars";
                list.ContentListDefinition = listTypeDef;
                list.Save();
                exceptionIsThrown = false;
            }
            catch (ContentRegistrationException e)
            {
                exceptionIsThrown = true;
                validExceptionIsThrown = true;
            }
            catch (Exception e)
            {
                exceptionIsThrown = true;
            }
            var invalidIsExist = Node.LoadNode(path) != null;
            if (invalidIsExist)
                Node.ForceDelete(path);
            Assert.IsTrue(exceptionIsThrown, "#2");
            Assert.IsTrue(validExceptionIsThrown, "#3");
            Assert.IsFalse(invalidIsExist, "#4");
        }

        [TestMethod()]
        public void ContentType_ReInstall_RemoveFieldsAndInheritance()
        {
            TestTools.RemoveNodesAndType("Automobile");

            //=================================================================================== #1

            var installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile2' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile3' parentType='Automobile2' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.ExecuteBatch(_installedContentTypes);

            var pt = ActiveSchema.PropertyTypes["Manufacturer"];
            var nt1 = ActiveSchema.NodeTypes["Automobile"];
            var nt2 = ActiveSchema.NodeTypes["Automobile2"];
            var nt3 = ActiveSchema.NodeTypes["Automobile3"];
            var pt1 = nt1.PropertyTypes["Manufacturer"] != null; // true
            var pt2 = nt2.PropertyTypes["Manufacturer"] != null; // true
            var pt3 = nt3.PropertyTypes["Manufacturer"] != null; // true
            var decl1 = nt1.DeclaredPropertyTypes.Contains(pt);  // true
            var decl2 = nt2.DeclaredPropertyTypes.Contains(pt);  // false
            var decl3 = nt3.DeclaredPropertyTypes.Contains(pt);  // false

            Assert.IsNotNull(pt, "#00");
            Assert.IsTrue(pt1, "#01");
            Assert.IsTrue(pt2, "#02");
            Assert.IsTrue(pt3, "#03");
            Assert.IsTrue(decl1, "#04");
            Assert.IsFalse(decl2, "#05");
            Assert.IsFalse(decl3, "#06");

            //-----------------------------------------------------------------------------------

            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");

            pt = ActiveSchema.PropertyTypes["Manufacturer"];
            nt1 = ActiveSchema.NodeTypes["Automobile"];
            nt2 = ActiveSchema.NodeTypes["Automobile2"];
            nt3 = ActiveSchema.NodeTypes["Automobile3"];
            pt1 = nt1.PropertyTypes["Manufacturer"] != null; // true
            pt2 = nt2.PropertyTypes["Manufacturer"] != null; // true
            pt3 = nt3.PropertyTypes["Manufacturer"] != null; // true
            decl1 = nt1.DeclaredPropertyTypes.Contains(pt);	 // true
            decl2 = nt2.DeclaredPropertyTypes.Contains(pt);	 // false
            decl3 = nt3.DeclaredPropertyTypes.Contains(pt);	 // false

            Assert.IsNotNull(pt, "#10");
            Assert.IsTrue(pt1, "#11");
            Assert.IsTrue(pt2, "#12");
            Assert.IsTrue(pt3, "#13");
            Assert.IsTrue(decl1, "#14");
            Assert.IsFalse(decl2, "#15");
            Assert.IsFalse(decl3, "#16");

            //=================================================================================== #2

            installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile2' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile3' parentType='Automobile2' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.ExecuteBatch(_installedContentTypes);

            //-----------------------------------------------------------------------------------

            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText' />
					</Fields>
				</ContentType>");

            pt = ActiveSchema.PropertyTypes["Driver"];
            nt1 = ActiveSchema.NodeTypes["Automobile"];
            nt2 = ActiveSchema.NodeTypes["Automobile2"];
            nt3 = ActiveSchema.NodeTypes["Automobile3"];
            pt1 = nt1.PropertyTypes["Driver"] != null;       // false
            pt2 = nt2.PropertyTypes["Driver"] != null;		 // false
            pt3 = nt3.PropertyTypes["Driver"] != null;		 // false
            decl1 = nt1.DeclaredPropertyTypes.Contains(pt);	 // false
            decl2 = nt2.DeclaredPropertyTypes.Contains(pt);	 // false
            decl3 = nt3.DeclaredPropertyTypes.Contains(pt);	 // false

            Assert.IsNull(pt, "#20");
            Assert.IsFalse(pt1, "#21");
            Assert.IsFalse(pt2, "#22");
            Assert.IsFalse(pt3, "#23");
            Assert.IsFalse(decl1, "#24");
            Assert.IsFalse(decl2, "#25");
            Assert.IsFalse(decl3, "#26");


            //=================================================================================== #3

            installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile2' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<!--<Field name='Driver' override='true' type='ShortText' />-->
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile3' parentType='Automobile2' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.ExecuteBatch(_installedContentTypes);

            //-----------------------------------------------------------------------------------

            installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText' />
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile2' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.ExecuteBatch(_installedContentTypes);

            pt = ActiveSchema.PropertyTypes["Driver"];
            nt1 = ActiveSchema.NodeTypes["Automobile"];
            nt2 = ActiveSchema.NodeTypes["Automobile2"];
            nt3 = ActiveSchema.NodeTypes["Automobile3"];
            pt1 = nt1.PropertyTypes["Driver"] != null;       // false
            pt2 = nt2.PropertyTypes["Driver"] != null;		 // true
            pt3 = nt3.PropertyTypes["Driver"] != null;		 // true
            decl1 = nt1.DeclaredPropertyTypes.Contains(pt);	 // false
            decl2 = nt2.DeclaredPropertyTypes.Contains(pt);	 // true
            decl3 = nt3.DeclaredPropertyTypes.Contains(pt);	 // false

            Assert.IsNull(pt, "#30");
            Assert.IsFalse(pt1, "#31");
            Assert.IsFalse(pt2, "#32");
            Assert.IsFalse(pt3, "#33");
            Assert.IsFalse(decl1, "#34");
            Assert.IsFalse(decl2, "#35");
            Assert.IsFalse(decl3, "#36");

            //=================================================================================== #4

            installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile2' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<!--<Field name='Driver' override='true' type='ShortText' />-->
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile3' parentType='Automobile2' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.ExecuteBatch(_installedContentTypes);

            //-----------------------------------------------------------------------------------

            installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile2' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");
            installer.ExecuteBatch(_installedContentTypes);

            pt = ActiveSchema.PropertyTypes["Driver"];
            nt1 = ActiveSchema.NodeTypes["Automobile"];
            nt2 = ActiveSchema.NodeTypes["Automobile2"];
            nt3 = ActiveSchema.NodeTypes["Automobile3"];
            pt1 = nt1.PropertyTypes["Driver"] != null;       // false
            pt2 = nt2.PropertyTypes["Driver"] != null;		 // true
            pt3 = nt3.PropertyTypes["Driver"] != null;		 // true
            decl1 = nt1.DeclaredPropertyTypes.Contains(pt);	 // false
            decl2 = nt2.DeclaredPropertyTypes.Contains(pt);	 // true
            decl3 = nt3.DeclaredPropertyTypes.Contains(pt);	 // false

            Assert.IsNotNull(pt, "#40");
            Assert.IsFalse(pt1, "#41");
            Assert.IsTrue(pt2, "#42");
            Assert.IsTrue(pt3, "#43");
            Assert.IsFalse(decl1, "#44");
            Assert.IsTrue(decl2, "#45");
            Assert.IsFalse(decl3, "#46");
        }


        [TestMethod]
        public void ContentType_Edit_()
        {
            TestTools.RemoveNodesAndType("Automobile");

            //-- Make a type with one-field (Automobile.Manufacturer)
            ContentTypeInstaller.InstallContentType(AutomobileHandler.ContentTypeDefinition);

            //-- Save a content
            SNC.Content auto1 = SNC.Content.CreateNew("Automobile", _testRoot, "X_" + Guid.NewGuid().ToString());
            auto1.Save();
            int autoId = auto1.Id;

            //-- Edit the ContentType with simulating ContentView/FieldControl --> Content/Field --> ContentHandler/Property chain
            ContentType ct = Node.LoadNode("/Root/System/Schema/ContentTypes/GenericContent/Automobile") as ContentType;
            SNC.Content ctContent = SNC.Content.Create(ct);

            Binary binEd = new Binary();
            BinaryEditorAccessor binEdAcc = new BinaryEditorAccessor(binEd);
            binEdAcc.SetContent(ctContent);
            binEd.Mode = BinaryEditorMode.Text;
            BinaryField binaryField = ctContent.Fields["Binary"] as BinaryField;
            binEdAcc.ConnectToField(binaryField);

            TextBox textBox = binEdAcc.TextBox;

            string textBefore = textBox.Text;
            //-- contains additional field: Driver
            textBox.Text = AutomobileHandler.ExtendedCTD;

            //-- simulating the content view
            binaryField.SetData(binEd.GetData());
            ctContent.Validate();

            bool contentIsValid = ctContent.IsValid;
            bool successfullySaved;
            try
            {
                ctContent.Save();
                successfullySaved = true;
            }
            catch
            {
                successfullySaved = false;
            }

            //-- betoltjuk az egyfield-es verzioval mentett content-et
            SNC.Content auto2 = SNC.Content.Load(autoId);

            //-- the new field must be existing
            var driverFieldisNotNull = auto2.Fields.ContainsKey("Driver");

            //-- clean
            Node.ForceDelete(autoId);

            //-- check
            Assert.IsTrue(contentIsValid, "#1");
            Assert.IsTrue(successfullySaved, "#2");
            Assert.IsTrue(driverFieldisNotNull, "#3");
        }

        [TestMethod()]
        public void ContentType_Install_Simple()
        {
            string contentTypeDef = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Manufacturer' type='ShortText' />
									<Field name='Driver' type='ShortText' />
								</Fields>
							</ContentType>";
            string expectedLog = @"
                Open();
                CreatePropertyType(name=<EnableLifespan>,dataType=<Int>,mapping=<5>,isContentListProperty=<False>);
                CreatePropertyType(name=<ValidFrom>,dataType=<DateTime>,mapping=<0>,isContentListProperty=<False>);
                CreatePropertyType(name=<ValidTill>,dataType=<DateTime>,mapping=<1>,isContentListProperty=<False>);
                CreatePropertyType(name=<AllowedChildTypes>,dataType=<Text>,mapping=<4>,isContentListProperty=<False>);
                CreatePropertyType(name=<ApprovingMode>,dataType=<Int>,mapping=<7>,isContentListProperty=<False>);
                CreatePropertyType(name=<InheritableApprovingMode>,dataType=<Int>,mapping=<8>,isContentListProperty=<False>);
                CreatePropertyType(name=<TrashDisabled>,dataType=<Int>,mapping=<9>,isContentListProperty=<False>);
                CreatePropertyType(name=<ExtensionData>,dataType=<Text>,mapping=<5>,isContentListProperty=<False>);
                CreatePropertyType(name=<BrowseApplication>,dataType=<Reference>,mapping=<3>,isContentListProperty=<False>);
                CreatePropertyType(name=<IsTaggable>,dataType=<Int>,mapping=<10>,isContentListProperty=<False>);
                CreatePropertyType(name=<Tags>,dataType=<Text>,mapping=<6>,isContentListProperty=<False>);
                CreatePropertyType(name=<IsRateable>,dataType=<Int>,mapping=<11>,isContentListProperty=<False>);
                CreatePropertyType(name=<RateStr>,dataType=<String>,mapping=<35>,isContentListProperty=<False>);
                CreatePropertyType(name=<RateAvg>,dataType=<Currency>,mapping=<0>,isContentListProperty=<False>);
                CreatePropertyType(name=<RateCount>,dataType=<Int>,mapping=<12>,isContentListProperty=<False>);
                CreatePropertyType(name=<CheckInComments>,dataType=<Text>,mapping=<7>,isContentListProperty=<False>);
                CreateNodeType(parent=<GenericContent>,name=<Automobile>,className=<SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler>);
                AddPropertyTypeToPropertySet(propertyType=<VersioningMode>,owner=<Automobile>,isDeclared=<False>);
                AddPropertyTypeToPropertySet(propertyType=<InheritableVersioningMode>,owner=<Automobile>,isDeclared=<False>);
                AddPropertyTypeToPropertySet(propertyType=<HasApproving>,owner=<Automobile>,isDeclared=<False>);
                AddPropertyTypeToPropertySet(propertyType=<Manufacturer>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<Driver>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<Hidden>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<Description>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<EnableLifespan>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<ValidFrom>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<ValidTill>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<AllowedChildTypes>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<ApprovingMode>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<InheritableApprovingMode>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<TrashDisabled>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<ExtensionData>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<BrowseApplication>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<IsTaggable>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<Tags>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<IsRateable>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<RateStr>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<RateAvg>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<RateCount>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<CheckInComments>,owner=<Automobile>,isDeclared=<True>);
                Close();".Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
            string log = InstallContentType(contentTypeDef, null);
            log = log.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
            ContentTypeManager.Reset();
            Assert.IsTrue(log == expectedLog);
        }
        [TestMethod()]
        public void ContentType_Install_Complex()
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
            string expectedLog = @"
                Open();
                CreatePropertyType(name=<EnableLifespan>,dataType=<Int>,mapping=<5>,isContentListProperty=<False>);
                CreatePropertyType(name=<ValidFrom>,dataType=<DateTime>,mapping=<0>,isContentListProperty=<False>);
                CreatePropertyType(name=<ValidTill>,dataType=<DateTime>,mapping=<1>,isContentListProperty=<False>);
                CreatePropertyType(name=<AllowedChildTypes>,dataType=<Text>,mapping=<4>,isContentListProperty=<False>);
                CreatePropertyType(name=<ApprovingMode>,dataType=<Int>,mapping=<7>,isContentListProperty=<False>);
                CreatePropertyType(name=<InheritableApprovingMode>,dataType=<Int>,mapping=<8>,isContentListProperty=<False>);
                CreatePropertyType(name=<TrashDisabled>,dataType=<Int>,mapping=<9>,isContentListProperty=<False>);
                CreatePropertyType(name=<ExtensionData>,dataType=<Text>,mapping=<5>,isContentListProperty=<False>);
                CreatePropertyType(name=<BrowseApplication>,dataType=<Reference>,mapping=<3>,isContentListProperty=<False>);
                CreatePropertyType(name=<IsTaggable>,dataType=<Int>,mapping=<10>,isContentListProperty=<False>);
                CreatePropertyType(name=<Tags>,dataType=<Text>,mapping=<6>,isContentListProperty=<False>);
                CreatePropertyType(name=<IsRateable>,dataType=<Int>,mapping=<11>,isContentListProperty=<False>);
                CreatePropertyType(name=<RateStr>,dataType=<String>,mapping=<35>,isContentListProperty=<False>);
                CreatePropertyType(name=<RateAvg>,dataType=<Currency>,mapping=<0>,isContentListProperty=<False>);
                CreatePropertyType(name=<RateCount>,dataType=<Int>,mapping=<12>,isContentListProperty=<False>);
                CreatePropertyType(name=<CheckInComments>,dataType=<Text>,mapping=<7>,isContentListProperty=<False>);
                CreateNodeType(parent=<GenericContent>,name=<Automobile>,className=<SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler>);
                AddPropertyTypeToPropertySet(propertyType=<VersioningMode>,owner=<Automobile>,isDeclared=<False>);
                AddPropertyTypeToPropertySet(propertyType=<InheritableVersioningMode>,owner=<Automobile>,isDeclared=<False>);
                AddPropertyTypeToPropertySet(propertyType=<HasApproving>,owner=<Automobile>,isDeclared=<False>);
                AddPropertyTypeToPropertySet(propertyType=<Manufacturer>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<Driver>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<Hidden>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<Description>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<EnableLifespan>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<ValidFrom>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<ValidTill>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<AllowedChildTypes>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<ApprovingMode>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<InheritableApprovingMode>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<TrashDisabled>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<ExtensionData>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<BrowseApplication>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<IsTaggable>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<Tags>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<IsRateable>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<RateStr>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<RateAvg>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<RateCount>,owner=<Automobile>,isDeclared=<True>);
                AddPropertyTypeToPropertySet(propertyType=<CheckInComments>,owner=<Automobile>,isDeclared=<True>);
                Close();".Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
            string log = InstallContentType(contentTypeDef, null);
            log = log.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
            ContentTypeManager.Reset();
            Assert.IsTrue(log == expectedLog);
        }
        [TestMethod()]
        public void ContentType_ReInstall_AddGenericField()
        {
            string ctdInstall = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Manufacturer' type='ShortText' />
								</Fields>
							</ContentType>";
            string ctdModify = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Manufacturer' type='ShortText' />
									<Field name='Driver' type='ShortText' />
								</Fields>
							</ContentType>";
            string expectedLog = @"Open();
				AddPropertyTypeToPropertySet(propertyType=<Driver>, owner=<Automobile>, isDeclared=<True>);
				Close();";
            string log = InstallContentType(ctdInstall, ctdModify);

            ContentType ct = ContentTypeManager.Current.ContentTypes["Automobile"];
            FieldSetting driverField = null;
            foreach (FieldSetting ft in ct.FieldSettings)
            {
                if (ft.Name == "Driver")
                {
                    driverField = ft;
                    break;
                }
            }

            ContentTypeManager.Reset();
            Assert.IsTrue(log.Replace("\r\n", "").Replace("\t", "").Replace(" ", "")
                == expectedLog.Replace("\r\n", "").Replace("\t", "").Replace(" ", ""), "#1");
            Assert.IsNotNull(driverField, "#2");

        }
        [TestMethod()]
        public void ContentType_ReInstall_RemoveField()
        {
            string ctdInstall = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Manufacturer' type='ShortText' />
									<Field name='Driver' type='ShortText' />
								</Fields>
							</ContentType>";
            string ctdModify = @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
								<Fields>
									<Field name='Manufacturer' type='ShortText' />
								</Fields>
							</ContentType>";
            string expectedLog = @"Open();
				RemovePropertyTypeFromPropertySet(propertyType=<Driver>, owner=<Automobile>);
				DeletePropertyType(propertyType=<Driver>);
				Close();";
            string log = InstallContentType(ctdInstall, ctdModify);
            ContentTypeManager.Reset();
            Assert.IsTrue(log.Replace("\r\n", "").Replace("\t", "").Replace(" ", "")
                == expectedLog.Replace("\r\n", "").Replace("\t", "").Replace(" ", ""));
        }

        [TestMethod()]
        public void ContentType_InstallAndCreateNode1()
        {
            string nodeName = "Automobile12";

            //-- torles, ha van
            string nodePath = RepositoryPath.Combine(_testRoot.Path, nodeName);
            Node automobileNode = Node.LoadNode(nodePath);
            if (automobileNode != null)
                automobileNode.Delete(); //Operations.DeletePhysical(nodePath);

            //-- Tipusregisztracio
            string ctd = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>
				";
            ContentTypeInstaller.InstallContentType(ctd);

            Folder folder = (Folder)Node.LoadNode(_testRoot.Path);

            automobileNode = new AutomobileHandler(folder);
            automobileNode.Name = nodeName;
            automobileNode["Manufacturer"] = "Honda";
            automobileNode["Driver"] = "Gyeby";
            automobileNode.Save();

            //-- Letrehozott node megkeresese
            NodeQuery query = new NodeQuery();
            query.Add(new StringExpression(ActiveSchema.PropertyTypes["Driver"], StringOperator.Equal, "Gyeby"));
            var resultList = query.Execute();
            Assert.IsTrue(resultList.Count > 0);
            Node result = resultList.Nodes.First<Node>();
            Assert.IsNotNull(result);

            //-- Takaritas
            automobileNode.ForceDelete();
        }
        [TestMethod()]
        public void ContentType_ReInstall_RemoveNodeField1()
        {
            string nodeName = "Automobile12";

            //-- torles, ha van
            string nodePath = RepositoryPath.Combine(_testRoot.Path, nodeName);
            Node automobileNode = Node.LoadNode(nodePath);
            if (automobileNode != null)
                automobileNode.Delete();

            //-- Tipusregisztracio
            string ctd = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>
				";
            ContentTypeInstaller.InstallContentType(ctd);

            automobileNode = new AutomobileHandler(_testRoot);
            automobileNode.Name = nodeName;
            automobileNode["Manufacturer"] = "Honda";
            automobileNode["Driver"] = "Gyeby";
            automobileNode.Save();

            int propertyCount = automobileNode.PropertyTypes.Count;
            int id = automobileNode.Id;
            SNC.Content automobileContent = SNC.Content.Create(automobileNode);
            bool hasFieldBefore = automobileContent.Fields.ContainsKey("Id");

            //-- Reinstall: remove Seats field
            string ctdMod = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>
				";
            ContentTypeInstaller.InstallContentType(ctdMod);
            var contentType = ContentType.GetByName("Automobile");

            //-- Letrehozott node betoltes
            automobileNode = Node.LoadNode(nodePath);
            int idAfter = automobileNode.Id;
            automobileContent = SNC.Content.Create(automobileNode);
            bool hasFieldAfter = automobileContent.Fields.ContainsKey("Seats");

            //-- Takaritas
            automobileNode.ForceDelete();

            //-- Teszt
            Assert.IsTrue(ActiveSchema.NodeTypes["Automobile"].PropertyTypes.Count == propertyCount, "#1");
            Assert.IsNotNull(ActiveSchema.PropertyTypes["Manufacturer"], "#2");
            Assert.IsTrue(id == idAfter, "#3");
            Assert.IsTrue(hasFieldBefore, "#4");
            Assert.IsFalse(hasFieldAfter, "#5");
        }
        [TestMethod()]
        public void ContentType_ReInstall_RemoveGenericField1()
        {
            //TestTools.RemoveNodesAndType("Automobile");

            Node automobileNode;
            string nodeName = "Automobile12";

            //-- Tipusregisztracio
            string ctd = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>
				";
            ContentTypeInstaller.InstallContentType(ctd);

            automobileNode = new AutomobileHandler(_testRoot);
            automobileNode.Name = nodeName;
            automobileNode["Manufacturer"] = "Honda";
            automobileNode["Driver"] = "Gyeby";
            automobileNode.Save();

            int propertyCount = automobileNode.PropertyTypes.Count;

            //-- Reinstall: remove Driver field
            string ctdMod = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
					</Fields>
				</ContentType>
				";
            ContentTypeInstaller.InstallContentType(ctdMod);

            //-- Property existence test

            //-- Takaritas
            automobileNode.ForceDelete();

            //-- Teszt
            Assert.IsTrue(ActiveSchema.NodeTypes["Automobile"].PropertyTypes.Count == propertyCount - 1, "PropertyType is not removed from NodeType");
            Assert.IsNull(ActiveSchema.PropertyTypes["Driver"], "PropertyType is not removed from TypeSystem");

        }
        [TestMethod()]
        public void ContentType_ReInstall_RemoveGenericField2()
        {
            var installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile2' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.ExecuteBatch(_installedContentTypes);

            //=================================================================================== #1

            installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
					</Fields>
				</ContentType>");
            installer.AddContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile2' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");
            installer.ExecuteBatch(_installedContentTypes);

            var pt = ActiveSchema.PropertyTypes["Driver"]; // null
            var nt1 = ActiveSchema.NodeTypes["Automobile"];
            var nt2 = ActiveSchema.NodeTypes["Automobile2"];
            var pt1 = nt1.PropertyTypes["Driver"] == null; // true
            var pt2 = nt2.PropertyTypes["Driver"] == null; // true

            Assert.IsTrue(pt1, "#1");
            Assert.IsTrue(pt2, "#2");
            Assert.IsNull(pt, "#3");
        }

        [TestMethod()]
        public void ContentType_SettingStructureCheck()
        {
            string xmlSource = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<DisplayName>Automobile [demo]</DisplayName>
	<Description>This is a demo automobile node definition</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<Field name='Manufacturer' type='ShortText' handler='SenseNet.ContentRepository.Fields.ShortTextField'>
			<DisplayName>Manufacturer's name</DisplayName>
			<Description>Enter the manufacturer's name</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<Compulsory>true</Compulsory>
				<MaxLength>100</MaxLength>
				<Format>TitleCase</Format>
			</Configuration>
		</Field>
		<Field name='Driver' type='ShortText'>
			<DisplayName>Driver's name</DisplayName>
			<Description>Enter the driver's name</Description>
			<Icon>icon.gif</Icon>
			<Bind property='Driver' />
			<Configuration>
				<Compulsory>true</Compulsory>
				<MaxLength>100</MaxLength>
			</Configuration>
		</Field>
	</Fields>
</ContentType>
";
            //PortalRoot n = Portal1.Root;

            ContentTypeInstaller.InstallContentType(xmlSource);

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(AutomobileHandler.ExtendedCTD);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("x", ContentType.ContentDefinitionXmlNamespace);

            string ctsName = GetStringValueFromXml(xml, nsmgr, "/x:ContentType/@name");
            string parentTypeName = GetStringValueFromXml(xml, nsmgr, "/x:ContentType/@parentType");
            ContentType parentContentType = ContentTypeManager.Current.GetContentTypeByName(parentTypeName);
            string ctsPath = RepositoryPath.Combine(parentContentType.Path, ctsName);
            string ctsHandlerName = GetStringValueFromXml(xml, nsmgr, "/x:ContentType/@handler");
            string ctsTitle = GetStringValueFromXml(xml, nsmgr, "/x:ContentType/x:DisplayName");
            string ctsDesc = GetStringValueFromXml(xml, nsmgr, "/x:ContentType/x:Description");
            string ctsIcon = GetStringValueFromXml(xml, nsmgr, "/x:ContentType/x:Icon");

            var ct = ContentType.GetByName("Automobile");
            Assert.IsTrue(ct.Name == ctsName, "#1");
            Assert.IsTrue(ct.Path == ctsPath, "#2");
            Assert.IsTrue(ct.ParentTypeName == parentTypeName, "#3");
            Assert.IsTrue(ct.ParentType.Id == parentContentType.Id, "#4");
            Assert.IsTrue(ct.HandlerName == ctsHandlerName, "#5");
            Assert.IsTrue(ct.DisplayName == ctsTitle, "#6");
            Assert.IsTrue(ct.Description == ctsDesc, "#7");
            Assert.IsTrue(ct.Icon == ctsIcon, "#8");

            XmlNodeList fieldList = xml.SelectNodes("/x:ContentType/x:Fields/x:Field", nsmgr);
            Assert.IsTrue(ct.FieldSettings.Count == fieldList.Count + ContentTypeManager.Current.GetContentTypeByName("GenericContent").FieldSettings.Count, "#7");
            for (int i = 0; i < fieldList.Count; i++)
            {
                XmlNode fieldNode = fieldList[i];

                string fsName = GetStringValueFromXml(fieldNode, nsmgr, "@name");
                FieldSetting fs = ct.GetFieldSettingByName(fsName); //.FieldSettings[i];

                string fsType = GetStringValueFromXml(fieldNode, nsmgr, "@type");
                string fsHandler = GetStringValueFromXml(fieldNode, nsmgr, "@handler");
                string fsDisplayName = GetStringValueFromXml(fieldNode, nsmgr, "x:DisplayName");
                string fsDesc = GetStringValueFromXml(fieldNode, nsmgr, "x:Description");
                string fsIcon = GetStringValueFromXml(fieldNode, nsmgr, "x:Icon");

                Assert.IsTrue(fs.Name == fsName, "#10");
                if (fsHandler != null)
                    Assert.IsTrue(fs.FieldClassName == fsHandler, "#11");
                Assert.IsTrue(fs.DisplayName == fsDisplayName, "#12");
                Assert.IsTrue(fs.Description == fsDesc, "#13");
                Assert.IsTrue(fs.Icon == fsIcon, "#14");

                XmlNodeList bindingNodes = fieldNode.SelectNodes("x:Bind", nsmgr);
                if (bindingNodes.Count == 0)
                {
                    Assert.IsTrue(fs.Bindings.Count == 1, "#15");
                    Assert.IsTrue(fs.Bindings[0] == fs.Name, "#16");
                }
                else
                {
                    for (int j = 0; j < bindingNodes.Count; j++)
                        Assert.IsTrue(fs.Bindings[j] == bindingNodes[j].Attributes["property"].Value, "#17");
                }
                //XmlNodeList configNodes = fieldNode.SelectNodes("x:Configuration/*", nsmgr);
                //if (configNodes.Count == 0)
                //{
                //    Assert.IsTrue(fs.Configuration.Count == 0, "#18");
                //}
                //else
                //{
                //    for (int j = 0; j < bindingNodes.Count; j++)
                //        Assert.IsTrue(fs.Configuration[configNodes[j].LocalName] == configNodes[j].InnerXml, "#19");
                //}
            }

        }

        [TestMethod]
        public void ContentType_AppInfo()
        {
            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <AppInfo>ContentType <x:x>AppInfo</x:x> test</AppInfo>
                                <Fields>
                                    <Field name='Name' type='ShortText'>
                                        <AppInfo><![CDATA[Field <y>AppInfo</y> test]]></AppInfo>
                                    </Field>
									<Field name='Manufacturer' type='ShortText'>
                                        <AppInfo>Field <y>AppInfo</y> test</AppInfo>
                                    </Field>
									<Field name='Driver' type='ShortText' />
								</Fields>
							</ContentType>",
                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile1' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields>
									<Field name='Manufacturer' type='ShortText' />
									<Field name='Driver' type='ShortText'>
                                        <AppInfo>Field AppInfo test</AppInfo>
                                    </Field>
								</Fields>
							</ContentType>");

            var ct = ContentType.GetByName("Automobile");
            var appInfo0 = ct.AppInfo;
            var appInfo1 = ct.GetFieldSettingByName("Name").AppInfo;
            var appInfo2 = ct.GetFieldSettingByName("Manufacturer").AppInfo;
            var appInfo3 = ct.GetFieldSettingByName("Driver").AppInfo;

            Assert.IsTrue(appInfo0 == "ContentType <x:x xmlns:x=\"xx\">AppInfo</x:x> test");
            Assert.IsTrue(appInfo1 == "Field &lt;y&gt;AppInfo&lt;/y&gt; test");
            Assert.IsTrue(appInfo2 == "Field <y xmlns=\"http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition\">AppInfo</y> test");
            Assert.IsTrue(appInfo3 == null);

            ct = ContentType.GetByName("Automobile1");
            appInfo0 = ct.AppInfo;
            appInfo1 = ct.GetFieldSettingByName("Name").AppInfo;
            appInfo2 = ct.GetFieldSettingByName("Manufacturer").AppInfo;
            appInfo3 = ct.GetFieldSettingByName("Driver").AppInfo;

            Assert.IsTrue(appInfo0 == "ContentType <x:x xmlns:x=\"xx\">AppInfo</x:x> test");
            Assert.IsTrue(appInfo1 == "Field &lt;y&gt;AppInfo&lt;/y&gt; test");
            Assert.IsTrue(appInfo2 == "Field <y xmlns=\"http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition\">AppInfo</y> test");
            Assert.IsTrue(appInfo3 == "Field AppInfo test");
        }

        [TestMethod]
        public void ContentType_AllowedChildTypes()
        {
            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <AllowedChildTypes>Car Auto Automobile1 Auto2</AllowedChildTypes>
                                <Fields>
									<Field name='Manufacturer' type='ShortText' />
									<Field name='Driver' type='ShortText' />
								</Fields>
							</ContentType>",
                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile1' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields>
									<Field name='Manufacturer' type='ShortText' />
									<Field name='Driver' type='ShortText' />
								</Fields>
							</ContentType>");
            var ct = ContentType.GetByName("Automobile");
            var s0 = String.Join(", ", ct.AllowedChildTypes);
            var s1 = String.Join(", ", ct.AllowedChildTypes.Select(x => x.Name));
            Assert.IsTrue(s0 == "Car, Automobile1", "#1");
            Assert.IsTrue(s0 == s1, "#2");

            var automobile = Content.CreateNew("Automobile", _testRoot, Guid.NewGuid().ToString());
            automobile.Save();

            var childContent = Content.CreateNew("Domain", automobile.ContentHandler, Guid.NewGuid().ToString());
            bool thrown = false;
            try
            {
                childContent.Save();
            }
            catch (InvalidOperationException e)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Exception wasn't thrown");
        }
        [TestMethod]
        public void Content_AllowChildTypes()
        {
            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='ContentList_for_AllowChildTypes' parentType='ContentList' handler='SenseNet.ContentRepository.ContentList' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields />
							</ContentType>",
                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields>
									<Field name='Manufacturer' type='ShortText' />
									<Field name='Driver' type='ShortText' />
								</Fields>
							</ContentType>",
                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='Automobile1' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields>
									<Field name='Manufacturer' type='ShortText' />
									<Field name='Driver' type='ShortText' />
								</Fields>
							</ContentType>");

            var content = Content.CreateNew("ContentList_for_AllowChildTypes", _testRoot, Guid.NewGuid().ToString());
            var rootGc = (GenericContent)content.ContentHandler;
            rootGc.AllowedChildTypes = new[] { ContentType.GetByName("Folder"), ContentType.GetByName("Car"), ContentType.GetByName("Automobile") };
            rootGc.Save();

            content = Content.CreateNew("Folder", rootGc, "AA");
            content.Save();
            var folderA = (GenericContent)content.ContentHandler;

            content = Content.CreateNew("Folder", content.ContentHandler, "BB");
            content.Save();
            var folderB = (GenericContent)content.ContentHandler;

            content = Content.CreateNew("Automobile", content.ContentHandler, "Automobile1");
            content.Save();
            var automobileGc = (GenericContent)content.ContentHandler;

            //------------------------------------ car test

            automobileGc.AllowChildType("Folder");
            var count = automobileGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 1, String.Format("#1: count is {0}, expected: 1.", count));

            automobileGc.AllowedChildTypes = null;
            count = automobileGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 0, String.Format("#2: count is {0}, expected: 0.", count));

            automobileGc.AllowChildTypes(new[] { "Folder", "File", "Car", "Automobile1" });
            count = automobileGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 4, String.Format("#3: count is {0}, expected: 4.", count));
            var names = String.Join(", ", automobileGc.AllowedChildTypes.Select(x => x.Name));
            Assert.IsTrue(names == "Folder, File, Car, Automobile1", String.Format("#4: names is '{0}', expected: 'Folder, File, Car, Automobile1'", names));

            //------------------------------------ Folder not affected test

            try
            {
                folderB.AllowChildType("Folder");
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch
            {
            }

            folderB.AllowChildType("Folder", throwOnError: false); //error but not throw

            count = rootGc.AllowedChildTypes.Count();
            folderB.AllowChildType("Folder", setOnAncestorIfInherits: true); //real but not affected
            var count1 = rootGc.AllowedChildTypes.Count();
            Assert.IsTrue(count1 - count == 0, String.Format("#10: count is {0}, expected: 0.", count1 - count));

            //------------------------------------ FolderB test

            rootGc.AllowedChildTypes = null;
            rootGc.Save();
            count = rootGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 0, String.Format("#20: count is {0}, expected: 0.", count));

            folderB.AllowChildType("Folder", true);
            rootGc = Node.Load<GenericContent>(rootGc.Id);
            count = rootGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 1, String.Format("#21: count is {0}, expected: 1.", count));
            names = String.Join(", ", rootGc.AllowedChildTypes.Select(x => x.Name));
            Assert.IsTrue(names == "Folder", String.Format("#22: names is '{0}', expected: 'Folder'", names));

            rootGc.AllowedChildTypes = null;

            folderB.AllowChildTypes(new[] { "Folder", "File", "Car", "Automobile1" }, setOnAncestorIfInherits: true);
            rootGc = Node.Load<GenericContent>(rootGc.Id);
            count = rootGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 4, String.Format("#23: count is {0}, expected: 4.", count));
            names = String.Join(", ", automobileGc.AllowedChildTypes.Select(x => x.Name));
            Assert.IsTrue(names == "Folder, File, Car, Automobile1", String.Format("#24: names is '{0}', expected: 'Folder, File, Car, Automobile1'", names));

            //------------------------------------ FolderA test

            rootGc.AllowedChildTypes = null;
            rootGc.Save();
            count = rootGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 0, String.Format("#30: count is {0}, expected: 0.", count));

            folderB.AllowChildType("Folder", setOnAncestorIfInherits: true);
            rootGc = Node.Load<GenericContent>(rootGc.Id);
            count = rootGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 1, String.Format("#31: count is {0}, expected: 1.", count));
            names = String.Join(", ", rootGc.AllowedChildTypes.Select(x => x.Name));
            Assert.IsTrue(names == "Folder", String.Format("#32: names is '{0}', expected: 'Folder'", names));

            rootGc.AllowedChildTypes = null;

            folderB.AllowChildTypes(new[] { "Folder", "File", "Car", "Automobile1" }, setOnAncestorIfInherits: true);
            rootGc = Node.Load<GenericContent>(rootGc.Id);
            count = rootGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 4, String.Format("#34: count is {0}, expected: 4.", count));
            names = String.Join(", ", rootGc.AllowedChildTypes.Select(x => x.Name));
            Assert.IsTrue(names == "Folder, File, Car, Automobile1", String.Format("#35: names is '{0}', expected: 'Folder, File, Car, Automobile1'", names));

            //------------------------------------ Remove az allowed type

            ContentTypeInstaller.RemoveContentType("Automobile1");

            rootGc = Node.Load<GenericContent>(rootGc.Id);
            count = rootGc.AllowedChildTypes.Count();
            Assert.IsTrue(count == 3, String.Format("#40: count is {0}, expected: 3.", count));
            names = String.Join(", ", rootGc.AllowedChildTypes.Select(x => x.Name));
            Assert.IsTrue(names == "Folder, File, Car", String.Format("#41: names is '{0}', expected: 'Folder, File, Car'", names));
        }

        [TestMethod]
        public void Check_ChildTypesToAllow()
        {
            // types: A, B, C, D, Folder, X, Y, Z
            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='ChildTypesToAllow_A' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields />
							</ContentType>",

                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='ChildTypesToAllow_B' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields />
							</ContentType>",

                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='ChildTypesToAllow_C' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields />
							</ContentType>",

                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='ChildTypesToAllow_D' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <Fields />
							</ContentType>",

                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='ChildTypesToAllow_X' parentType='GenericContent' handler='SenseNet.ContentRepository.Folder' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <AllowedChildTypes>ChildTypesToAllow_A ChildTypesToAllow_B ChildTypesToAllow_C ChildTypesToAllow_D</AllowedChildTypes>
                                <Fields />
							</ContentType>",

                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='ChildTypesToAllow_Y' parentType='GenericContent' handler='SenseNet.ContentRepository.Folder' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <AllowedChildTypes>ChildTypesToAllow_A ChildTypesToAllow_B ChildTypesToAllow_C ChildTypesToAllow_D</AllowedChildTypes>
                                <Fields />
							</ContentType>",

                            @"<?xml version='1.0' encoding='utf-8'?>
							<ContentType name='ChildTypesToAllow_Z' parentType='GenericContent' handler='SenseNet.ContentRepository.Folder' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' xmlns:x='xx'>
                                <AllowedChildTypes>ChildTypesToAllow_A ChildTypesToAllow_B ChildTypesToAllow_C ChildTypesToAllow_D</AllowedChildTypes>
                                <Fields />
							</ContentType>"
                            );

            var sourceContent = Content.CreateNew("SystemFolder", _testRoot, null);
            sourceContent.Save();

            var sourceCh = sourceContent.ContentHandler;
                var f1 = Content.CreateNew("Folder", sourceCh, "F1"); f1.Save();
                    var x1 = Content.CreateNew("ChildTypesToAllow_X", f1.ContentHandler, "X1"); x1.Save();
                        var a1 = Content.CreateNew("ChildTypesToAllow_A", x1.ContentHandler, "A1"); a1.Save();
                        var b1 = Content.CreateNew("ChildTypesToAllow_B", x1.ContentHandler, "B1"); b1.Save();
                    var f2 = Content.CreateNew("Folder"             , f1.ContentHandler, "F2"); f2.Save();
                        var x2 = Content.CreateNew("ChildTypesToAllow_X", f2.ContentHandler, "X2"); x2.Save();
                        var z2 = Content.CreateNew("ChildTypesToAllow_Z", f2.ContentHandler, "Z2"); z2.Save();
                    var z1 = Content.CreateNew("ChildTypesToAllow_Z", f1.ContentHandler, "Z1"); z1.Save();
                        var c1 = Content.CreateNew("ChildTypesToAllow_C", z1.ContentHandler, "C1"); c1.Save();
                        var d1 = Content.CreateNew("ChildTypesToAllow_D", z1.ContentHandler, "D1"); d1.Save();

            var childTypes = Node.GetChildTypesToAllow(f1.Id).Select(x => x.Name).ToList();
            Assert.IsTrue(childTypes.Count == 3);
            Assert.IsTrue(childTypes.Contains("ChildTypesToAllow_X"), "Does not contain ChildTypesToAllow_X");
            Assert.IsTrue(childTypes.Contains("Folder"), "Does not contain Folder");
            Assert.IsTrue(childTypes.Contains("ChildTypesToAllow_Z"), "Does not contain ChildTypesToAllow_Z");

            //var targetContent = Content.CreateNew("SystemFolder", _testRoot, null);
            //var targetCh = (SystemFolder)targetContent.ContentHandler;
            //targetCh.AllowedChildTypes = (new[] { "ChildTypesToAllow_X", "ChildTypesToAllow_Y", "Folder" }).Select(x => ContentType.GetByName(x));
            //targetCh.Save();
        }

        [TestMethod]
        public void ContentType_Bug5093()
        {
            // repro summary:
            // 1 - create a CTD type
            // 2 - create an instance of the type
            // 3 - modify CTD and try redefining the Name field with an incorrect Name="Name" setting (note the big N in the name attribute)
            // 4 - system correctly reports that the Name attribute is unknown
            // 5 - now correct the CTD error and save the CTD
            // 6 - now try accessing the content

            //==== steps
            // 1 - create a CTD type
            ContentTypeInstaller.InstallContentType(@"<ContentType
                    name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                <Fields>
                    <Field name='Name' type='ShortText' />
	                <Field name='Manufacturer' type='ShortText' />
                </Fields>
            </ContentType>
            ");
            _installedContentTypes.Add("Automobile");

            // 2 - create an instance of the type
            var content = Content.CreateNew("Automobile", _testRoot, "Automobile_Bug5093");
            content["Manufacturer"] = "Company1";
            content.Save();
            var id = content.Id;

            // 3 - modify CTD and try redefining the Name field with an incorrect Name="Name" setting (note the big N in the name attribute)
            var thrown = false;
            try
            {
                ContentTypeInstaller.InstallContentType(@"<ContentType
                        name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	                <Fields>
                        <Field Name='Name' type='ShortText' />
		                <Field name='Manufacturer' type='ShortText' />
	                </Fields>
                </ContentType>
                ");
            }
            catch (ContentRegistrationException)
            {
                // 4 - system correctly reports that the Name attribute is unknown
                thrown = true;
            }

            // 5 - now correct the CTD error and save the CTD
            ContentTypeInstaller.InstallContentType(@"<ContentType
                    name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                <Fields>
                    <Field name='Name' type='ShortText' />
	                <Field name='Manufacturer' type='ShortText' />
                </Fields>
            </ContentType>
            ");

            // 6 - now try accessing the content
            var badCt = ContentType.GetByName("Automobile");

            var loaded = Content.Load(id);
            var data = (string)content["Manufacturer"];

            Assert.IsTrue(thrown, "Expected ContentRegistrationException was not thrown.");
            Assert.IsTrue(data == "Company1", String.Concat("Data is: '", data, "'. Expected: 'Company1'."));
        }
        [TestMethod]
        [ExpectedException(typeof(RegistrationException))]
        public void ContentType_Bug5564()
        {
            ContentTypeInstaller.InstallContentType(
                @"<ContentType name='CTBug5564_A' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                    <Fields>
                        <Field name='abcde' type='ShortText' />
                    </Fields>
                </ContentType>",
                @"<ContentType name='CTBug5564_B' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                    <Fields>
                        <Field name='abcde' type='DateTime' />
                    </Fields>
                </ContentType>"
            );
            _installedContentTypes.Add("CTBug5564_A");
            _installedContentTypes.Add("CTBug5564_B");
        }

        [TestMethod]
        public void ContentType_Bug572_UnknownAnalyzer()
        {
            // give Lucene.Net.Analysis.StandardAnalyzer instead of Lucene.Net.Analysis.Standard.StandardAnalyzer for an arbitrary field.
            // From then onwards the portal stops working, and throws an exception for EVERY REQUEST that the type given is not found
            // severity: portal CANNOT BE REPAIRED FROM THIS STATE

            var ctdsrc = @"<ContentType
                name='ContentType_Bug572_UnknownAnalyzer' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                <Fields>
                    <Field name='ContentType_Bug572_UnknownAnalyzer_TestField' type='ShortText'>
                        <Indexing>
                            <Analyzer>{0}</Analyzer>
                        </Indexing>
                    </Field>
                </Fields>
            </ContentType>";

            var contentType = ContentType.GetByName("GenericContent");
            var thrown = false;

            try
            {
                ContentTypeInstaller.InstallContentType(String.Format(ctdsrc, "Lucene.Net.Analysis.StandardAnalyzer"));
                _installedContentTypes.Add("ContentType_Bug572_UnknownAnalyzer");

                contentType = ContentType.GetByName("ContentType_Bug572_UnknownAnalyzer");
            }
            catch (RegistrationException e)
            {
                thrown = true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("THE SYSTEM MAYBE UNUSABLE: Registration exception was not thrown.", ex);
            }

            ContentTypeInstaller.InstallContentType(String.Format(ctdsrc, "Lucene.Net.Analysis.Standard.StandardAnalyzer"));
            if (!_installedContentTypes.Contains("ContentType_Bug572_UnknownAnalyzer"))
                _installedContentTypes.Add("ContentType_Bug572_UnknownAnalyzer");

            Assert.IsTrue(thrown, "THE SYSTEM MAYBE UNUSABLE: Registration exception was not thrown.");
        }

        [TestMethod]
        public void ContentType_Bug574_DeletingDynamicProperty()
        {
            //  1. add a new field to Contract CTD:
            //      <Field name="MyKeywords" type="LongText">
            //        <DisplayName>MyKeywords</DisplayName>
            //        <Indexing>
            //          <Analyzer>Lucene.Net.Analysis.Standard.StandardAnalyzer</Analyzer>
            //        </Indexing>
            //      </Field>
            //  2. create a Contract content under /Root/Sites/Default_site
            //  3. write sthing into MyKeywords field, like 'asdfasf'
            //  4. edit the Contract CTD, and delete the MyKeywords Field element
            //  5. exception occurs:
            //       The DELETE statement conflicted with the REFERENCE constraint "FK_TextPropertiesNVarchar_SchemaPropertyTypes". 
            //       The conflict occurred in database "SenseNetContentRepository", table "dbo.TextPropertiesNVarchar", column 'PropertyTypeId'. 
            //       The statement has been terminated. 
            //
            //  6. open the content, and you will find that the field is gone.
            //  7. reinsert the deleted Field into the CTD. no exceptions.
            //  8. open the content and you will find that the field reappears, even the previously entered data is there.
            //  expected behav:
            //  - no exceptions when field deleted
            //  - field data should be deleted from sql and index
            //  - when reinserting field, content should show empty field

            var ctdsource = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='Contract' parentType='File' handler='SenseNet.ContentRepository.File' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
    <DisplayName>Contract</DisplayName>
	<Description>An example content type to demonstrate the ECMS features of Sense/Net 6.0.</Description>
	<Icon>Document</Icon>
	<Fields>
		<Field name='ContractId' type='ShortText'>
			<DisplayName>Contract ID</DisplayName>
		</Field>
		<Field name='Project' type='ShortText'>
			<DisplayName>Project</DisplayName>
		</Field>
		<Field name='Language' type='Choice'>
			<DisplayName>Project</DisplayName>
			<Configuration>
				<AllowMultiple>false</AllowMultiple>
				<AllowExtraValue>true</AllowExtraValue>
				<Options>
					<Option value='hu' selected='true'>Magyar</Option>
					<Option value='en'>English</Option>
					<Option value='de'>Deutsch</Option>
				</Options>
			</Configuration>
		</Field>
		<Field name='Responsee' type='Reference'>
			<DisplayName>Responsee</DisplayName>
			<Configuration>
                <AllowMultiple>false</AllowMultiple>
				<AllowedTypes>
					<Type>User</Type>
				</AllowedTypes>
			</Configuration>
		</Field>
		<Field name='Lawyer' type='ShortText'>
			<DisplayName>Lawyer</DisplayName>
		</Field>
		<Field name='Keywords' type='LongText'>
			<DisplayName>Keywords</DisplayName>
            <Indexing>
                <Analyzer>Lucene.Net.Analysis.WhitespaceAnalyzer</Analyzer>
            </Indexing>
        </Field>
		<Field name='Description' type='LongText'>
			<DisplayName>Description</DisplayName>
		</Field>
        {0}
    </Fields>
</ContentType>
";
            var originalCtd = String.Format(ctdsource, @"");
            var extendedCtd = String.Format(ctdsource, @"<Field name='MyKeywords' type='LongText'>
            <DisplayName>MyKeywords</DisplayName>
            <Indexing>
                <Analyzer>Lucene.Net.Analysis.Standard.StandardAnalyzer</Analyzer>
            </Indexing>
        </Field>");

            //=====  1. add a new field to Contract CTD:
            ContentTypeInstaller.InstallContentType(extendedCtd);

            //=====  2. create a Contract content under /Root/Sites/Default_site
            var content = Content.CreateNew("Contract", _testRoot, "ContentType_Bug574_DeletingDynamicProperty");

            //=====  3. write string into MyKeywords field, like 'asdfasf'
            content["MyKeywords"] = "asdfasf";
            content.Save();
            var contentId = content.Id;

            //=====  4. edit the Contract CTD, and delete the MyKeywords Field element
            ContentTypeInstaller.InstallContentType(originalCtd);

            //-----
            content = Content.Load(contentId);
            Field field;
            bool fieldExists = content.Fields.TryGetValue("MyKeywords", out field);

            Assert.IsFalse(fieldExists, "MyKeywords field was not deleted");
        }

        [TestMethod]
        public void ContentType_OverridingFieldInTheMiddleLevelCTD()
        {
            //======================= Install a two-level-depth structure
            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Manufacturer' type='ShortText'/>
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>",
                @"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile2' parentType='Automobile' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields />
				</ContentType>");

            //======================= Override a field in the middle level CTD
            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields>
						<Field name='Name' type='ShortText' />
						<Field name='Manufacturer' type='ShortText' />
						<Field name='Driver' type='ShortText' />
					</Fields>
				</ContentType>");

var ct = ContentType.GetByName("GenericContent");
var fs = ct.FieldSettings.Where(q => q.Name == "Name").First();
Assert.IsTrue(fs.ParentFieldSetting == null, "#0");

            var ct0 = ContentType.GetByName("GenericContent");
            var fs0 = GetFieldSettingFromContentType(ct0, "Name");
            var ct1 = ContentType.GetByName("Automobile");
            var fs1 = GetFieldSettingFromContentType(ct1, "Name");
            var ct2 = ContentType.GetByName("Automobile2");
            var fs2 = GetFieldSettingFromContentType(ct2, "Name");

            Assert.IsTrue(fs0.Owner == ct0, "#1");
            Assert.IsTrue(fs1.Owner == ct1, "#2");
            Assert.IsTrue(fs2.Owner == ct1, "#3");
            Assert.IsTrue(fs1 == fs2, "#4");
            Assert.IsTrue(fs0.ParentFieldSetting == null, "#5");
            Assert.IsTrue(fs1.ParentFieldSetting == fs0, "#6");
            Assert.IsTrue(fs2.ParentFieldSetting == fs0, "#7");
        }
        private FieldSetting GetFieldSettingFromContentType(ContentType ct, string name)
        {
            return ct.FieldSettings.Where(f => f.Name == name).FirstOrDefault();
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
		<PropertyType itemID='38' name='DisplayName' dataType='String' mapping='25' />
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
				<PropertyType name='DisplayName' />
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
            ContentTypeManager.Current.AddContentType(cts);
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
                ContentTypeManager.Current.AddContentType(cts);
                ctmAcc.ApplyChangesInEditor(cts, ed3);
                wr = new TestSchemaWriter();
                ed3Acc.RegisterSchema(ed2, wr);
            }

            return wr.Log;
        }

        private string GetStringValueFromXml(XmlNode rootNode, XmlNamespaceManager nsmgr, string xpath)
        {
            XmlNode node = rootNode.SelectSingleNode(xpath, nsmgr);
            if (node == null)
                return null;
            if (node is XmlAttribute)
                return node.Value;
            return node.InnerXml;
        }
        private int GetIntValueFromXml(XmlNode rootNode, XmlNamespaceManager nsmgr, string xpath)
        {
            string s = GetStringValueFromXml(rootNode, nsmgr, xpath);
            return s == null ? 0 : Convert.ToInt32(s);
        }

        //======================================================================================================

        [TestMethod]
        public void ContentType_XmlNamespaceCompatibility_FeatureOn_Hu()
        {
            var ns = "http://schemas.sensenet" + ".hu/SenseNet/ContentRepository/ContentTypeDefinition";
            var msg = CheckContentTypeXmlNamespaceCompatibility(ns, "hu", true);
            Assert.IsTrue(msg == null, msg ?? "Ok");
        }
        [TestMethod]
        public void ContentType_XmlNamespaceCompatibility_FeatureOn_Com()
        {
            var ns = "http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition";
            var msg = CheckContentTypeXmlNamespaceCompatibility(ns, "com", true);
            Assert.IsTrue(msg == null, msg ?? "Ok");
        }
        [TestMethod]
        public void ContentType_XmlNamespaceCompatibility_FeatureOn_X()
        {
            var ns = "howmanyeggsintheeasterbasket";
            var msg = CheckContentTypeXmlNamespaceCompatibility(ns, "x", true);
            Assert.IsFalse(msg == null, "Forbidden namespace (x) is accepted");
        }
        [TestMethod]
        public void ContentType_XmlNamespaceCompatibility_FeatureOff_Hu()
        {
            var ns = "http://schemas.sensenet" + ".hu/SenseNet/ContentRepository/ContentTypeDefinition";
            var msg = CheckContentTypeXmlNamespaceCompatibility(ns, "hu", false);
            Assert.IsFalse(msg == null, "Forbidden namespace (hu) is accepted");
        }
        [TestMethod]
        public void ContentType_XmlNamespaceCompatibility_FeatureOff_Com()
        {
            var ns = "http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition";
            var msg = CheckContentTypeXmlNamespaceCompatibility(ns, "com", false);
            Assert.IsTrue(msg == null, msg ?? "Ok");
        }
        [TestMethod]
        public void ContentType_XmlNamespaceCompatibility_FeatureOff_X()
        {
            var ns = "howmanyeggsintheeasterbasket";
            var msg = CheckContentTypeXmlNamespaceCompatibility(ns, "x", false);
            Assert.IsFalse(msg == null, "Forbidden namespace (x) is accepted");
        }

        private void SetBackwardCompatibilityXmlNamespaces(bool newValue)
        {
            var pt = new PrivateType(typeof(RepositoryConfiguration));
            pt.SetStaticField("_backwardCompatibilityXmlNamespaces", newValue);
        }
        private string CheckContentTypeXmlNamespaceCompatibility(string xmlNamespace, string namespaceName, bool featureEnabled)
        {
            var compat = RepositoryConfiguration.BackwardCompatibilityXmlNamespaces;

            var contentTypeName = "XmlNamespaceCompatibility";
            if (!_installedContentTypes.Contains(contentTypeName))
                _installedContentTypes.Add(contentTypeName);

            var fieldName = "XmlNamespaceCompatibilityField";
            if (ContentType.GetByName(contentTypeName) != null)
                ContentTypeInstaller.RemoveContentType(contentTypeName);

            string contentTypeDef = String.Format(@"<ContentType name='{0}' parentType='GenericContent' 
                        handler='SenseNet.ContentRepository.GenericContent' xmlns='{1}'>
					<Fields><Field name='{2}' type='ShortText' /></Fields>
				</ContentType>", contentTypeName, xmlNamespace, fieldName);

            try
            {
                SetBackwardCompatibilityXmlNamespaces(featureEnabled);
                ContentTypeInstaller.InstallContentType(contentTypeDef);
                SetBackwardCompatibilityXmlNamespaces(compat);
            }
            catch (Exception e)
            {
                return String.Concat("InstallError (", namespaceName, "): ", e.Message);
            }
            finally
            {
                SetBackwardCompatibilityXmlNamespaces(compat);
            }

            var fieldValue = Guid.NewGuid().ToString();
            var content = Content.CreateNew(contentTypeName, _testRoot, "XmlNamespaceCompatibilityContent");
            if (!content.Fields.ContainsKey(fieldName))
            {
                Node.ForceDelete(content.Id);
                return String.Concat("Missing field (", namespaceName, ")");
            }
            content[fieldName] = fieldValue;
            content.Save();
            var id = content.Id;

            content = Content.Load(id);
            var loadedValue = (string)content[fieldName];

            Node.ForceDelete(id);

            if (loadedValue != fieldValue)
                return String.Concat("Inconsistent field value (", namespaceName, ")");

            var src = Tools.GetStreamString(ContentType.GetByName(contentTypeName).Binary.GetStream());
            src = src.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("'", "\"").Replace(" ", "");
            var src1 = contentTypeDef.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("'", "\"").Replace(" ", "");
            if (src != src1)
                return String.Concat("Content type xml is modified (", namespaceName, ")");

            return null;
        }
    }

}