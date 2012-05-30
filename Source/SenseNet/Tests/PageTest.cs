using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using System.IO;
using SenseNet.Portal;

namespace SenseNet.ContentRepository.Tests
{
    /// <summary>
    ///This is a test class for SenseNet.Portal.Page and is intended
    ///to contain all SenseNet.Portal.Page Unit Tests
    ///</summary>
    [TestClass()]
    public class PageTest : TestBase
    {


        private static string _testPageTemplateName = "TestPageTemplate.html";
        private static string _pageTemplateHtml = @"<html>
											<body>
												<snpe-zone name='ZoneName_1'></snpe-zone>
												<snpe-edit name='Editor'></snpe-edit>
												<snpe-catalog name='Catalog'></snpe-catalog>
												<snpe:PortalRemoteControl ID='RemoteControl1' runat='server' />
											</body>
										    </html>";
        private static string _pageTemplatePath;
        private static string _pageName = "TestPage";
        private static string _rootNodePath;


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Folder f = new Folder(Repository.Root);
            f.Save();
            _rootNodePath = f.Path;
            CreateTestPageTemplate();

        }

        private static void CreateTestPageTemplate()
        {
            PageTemplate pt = null;

            pt = new PageTemplate(Node.LoadNode(_rootNodePath));
            pt.Name = _testPageTemplateName;
            BinaryData binaryData = new BinaryData();
            binaryData.FileName = new BinaryFileName(_testPageTemplateName);
            string streamString = _pageTemplateHtml;

            Stream stream = Tools.GetStreamFromString(streamString);
            binaryData.SetStream(stream);

            pt.Binary = binaryData;
            pt.Save();

            _pageTemplatePath = pt.Path;
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            Node.ForceDelete(_rootNodePath);
        }

        /// <summary>
        ///A test for Page (Node)
        ///</summary>
        [TestMethod()]
        public void Page_Constructor()
        {
            Node parent = Repository.Root;

            Page target = new Page(parent);

            Assert.IsNotNull(target, "1. Page is null.");
        }
        [TestMethod()]
        public void Page_Constructor_HasAllProperties()
        {
            Node parent = Repository.Root;
            Page target = new Page(parent);

            
            //Assert.IsTrue(target.HasProperty("PageNameInMenu"), "DisplayName is null.");
			Assert.IsTrue(target.HasProperty("Hidden"), "Hidden is null.");
			Assert.IsTrue(target.HasProperty("Keywords"), "Keywords is null.");
			Assert.IsTrue(target.HasProperty("MetaDescription"), "MetaDescription is null.");
			Assert.IsTrue(target.HasProperty("MetaTitle"), "MetaTitle is null.");
        }

        [TestMethod()]
        public void Page_Save_BinaryIDWithNoChange()
        {
            //string path = CreateTestPage();
            Page page = CreateTestPage();
            Assert.IsNotNull(page, "Test page has not been created.");
            
            int old_id = page.Binary.Id;
            page.Index++;
            page.Save();
            Assert.AreEqual(old_id, page.Binary.Id, "IDs changed.");
        }

        [TestMethod()]
        public void Page_Save_BinaryIDWithNewVersion()
        {
            //string path = CreateTestPage();
            //Page page = Page.Load(path) as Page;
            Page page = CreateTestPage();
            Assert.IsNotNull(page, "Test page has not been created.");
            //long length = page.Binary.GetStream().Length;
            int old_id = page.Binary.Id;
            page.Save(VersionRaising.NextMajor, VersionStatus.Locked);
            Assert.AreNotEqual(old_id, page.Binary.Id, "IDs changed.");
            //Assert.IsTrue(page.Binary.GetStream().Length == length);
            Assert.IsTrue(page.Binary.GetStream().Length > 0);
        }


        // Creating test node
        private Page CreateTestPage()
        {
            string testPagePath = RepositoryPath.Combine(_rootNodePath, _pageName);
            if (Node.Exists(testPagePath))
                Node.ForceDelete(testPagePath);

			//if (Node.Exists("/Root/TestPage"))
            //    Node.DeletePhysical("/Root/TestPage");

            Page f = new Page(Node.LoadNode(_rootNodePath));
            f.Name = _pageName;
            f.PageTemplateNode = PageTemplate.LoadNode(_pageTemplatePath) as PageTemplate;
            f.Save();
            return f;
        }


    }


}