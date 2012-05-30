using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal;

namespace SenseNet.ContentRepository.Tests
{
	[TestClass()]
    public class PageTemplateManagerTest : TestBase
	{
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



		private static string _testRootName = "_PageTemplateManagerTests";
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
            if (Node.Exists(Repository.PageTemplatesFolderPath))
                Node.ForceDelete(Repository.PageTemplatesFolderPath);
        }



		/// <summary>
		///A test for GetBinaryData (int, Stream)
		///</summary>
        [DeploymentItem("SenseNet.ContentRepository.dll")]
        [TestMethod()]
        public void PageTemplateManager_GetBinaryData_Test()
        {
            string guidZoneName = Guid.NewGuid().ToString();

            Common.CreateFolderStructure("Global/pagetemplates");

            PageTemplate pageTemplate = CreatePageTemplate(guidZoneName, String.Concat(Repository.Root.Path, "/Global/pagetemplates"));

            bool result = false;

            MasterPage masterPage = Node.LoadNode(string.Concat(Repository.Root.Path, "/Global/pagetemplates/TestPageTemplate.Master")) as MasterPage;
            if (masterPage != null && masterPage.Binary != null && masterPage.Binary.GetStream() != null)
            {
                string masterString = Tools.GetStreamString(masterPage.Binary.GetStream());
                if (!string.IsNullOrEmpty(masterString))
                {
                    result = masterString.IndexOf(guidZoneName) > -1;
                }
            }
            Assert.IsTrue(result);
        }

		private PageTemplate CreatePageTemplate(string zoneName, string parentPath)
		{
            var folderpath = parentPath ?? this.TestRoot.Path;

			PageTemplate pageTemplate = null;
            pageTemplate = Node.LoadNode(string.Concat(folderpath, "/TestPageTemplate.html")) as PageTemplate;
			if (pageTemplate == null)
			{
                var parent = Node.LoadNode(folderpath);
                pageTemplate = new PageTemplate(parent);
				pageTemplate.Name = "TestPageTemplate.html";
			}
			
			BinaryData binaryData = new BinaryData();
			binaryData.FileName = new BinaryFileName("TestPageTemplate.html");
			string streamString = string.Concat(
										"<html>",
										"	<body>",
										"		<snpe-zone name=\"ZoneName_", zoneName, "\"></snpe-zone>",
										"		<snpe-edit name=\"Editor\"></snpe-edit>",
										"		<snpe-catalog name=\"Catalog\"></snpe-catalog>",
										"		<snpe:PortalRemoteControl2 ID=\"RemoteControl1\" runat=\"server\" />",
										"	</body>",
										"</html>"
									);
			Stream stream = Tools.GetStreamFromString(streamString);
			binaryData.SetStream(stream);

			pageTemplate.Binary = binaryData;
			pageTemplate.Save();

			return pageTemplate;
		}

		/// <summary>
		///A test for GetPageBinaryData (Page, PageTemplate)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_GetPageBinaryData_Test()
		{
			bool result = false;

			string guidZoneName = Guid.NewGuid().ToString();
			PageTemplate pageTemplate = CreatePageTemplate(guidZoneName, null);
			if (pageTemplate != null)
			{
				Page page = null;
				page = Node.LoadNode(string.Concat(this.TestRoot.Path, "/TestPage")) as Page;
				if (page == null)
				{
					page = new Page(this.TestRoot);
				}
				page.PageTemplateNode = pageTemplate;
				page.Name = "TestPage";
				page.PageNameInMenu = "TestPage";

				page.Save();

				if (page != null && page.Binary != null && page.Binary.GetStream() != null)
				{
					string pageString = Tools.GetStreamString(page.Binary.GetStream());
					if (!string.IsNullOrEmpty(pageString))
					{
						result = pageString.IndexOf(guidZoneName) > -1;
					}
				}
			}

			Assert.IsTrue(result);			
		}

        /// <summary>
        ///A test for GetASPXBinaryByPageTemplate (Page, PageTemplate)
        ///</summary>
        [DeploymentItem("SenseNet.ContentRepository.dll")]
        [TestMethod()]
        public void PageTemplateManager_GetASPXBinaryByPageTemplate_Test()
        {
            bool result = false;

            string guidZoneName = Guid.NewGuid().ToString();
            string pageName = Guid.NewGuid().ToString();

            PageTemplate pageTemplate = CreatePageTemplate(guidZoneName, null);
            if (pageTemplate != null)
            {
                Page page = null;
				page = Node.LoadNode(string.Concat(this.TestRoot.Path, "/", pageName)) as Page;
                if (page == null)
                {
					page = new Page(this.TestRoot);
                }

                page.PageTemplateNode = pageTemplate;
                page.Name = pageName;
                page.PageNameInMenu = pageName;

                page.Save();

                if (page != null && page.Binary != null)
                {
                    int oldID = page.Binary.Id;

                    page.Save();

                    result = (oldID == page.Binary.Id);
                }
            }

            Assert.IsTrue(result, "Page.Binary.Id changed during Page.Save(), means unnecessary db entries were created.");
        }

		/// <summary>
		///A test for GetStreamFromString (string)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_GetStreamFromString_Test()
		{
			string expected = "TestString";

			Stream actualStream = Tools.GetStreamFromString(expected);
			string actual = Tools.GetStreamString(actualStream);

			Assert.AreEqual(expected, actual, "PageTemplateManager.GetStreamFromString did not return the expected value.");
		}

		/// <summary>
		///A test for GetStreamString (Stream)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_GetStreamString_Test()
		{
			string expected = "TestString";

			Stream stream = Tools.GetStreamFromString(expected);
			string actual = Tools.GetStreamString(stream);

			Assert.AreEqual(expected, actual, "PageTemplateManager.GetStreamString did not return the expected value.");
		}

		/// <summary>
		///A test for CheckPageTemplateBinaryStream (PageTemplate)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_CheckPageTemplateBinaryStream_Test()
		{
			PageTemplate pageTemplate = new PageTemplate(this.TestRoot);
			BinaryData binaryData = new BinaryData();
			string streamData = "TestString";
			binaryData.SetStream(Tools.GetStreamFromString(streamData));
			pageTemplate.Binary = binaryData;

			bool expected = true;
			bool actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor.CheckPageTemplateBinaryStream(pageTemplate);

			Assert.AreEqual(expected, actual, "PageTemplateManager.CheckPageTemplateBinaryStream did not return the expected value.");
		}

		/// <summary>
		///A test for CheckPageTemplateBinaryStream (PageTemplate)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_CheckPageTemplateBinaryStream_NullStreamTest()
		{
			PageTemplate pageTemplate = new PageTemplate(this.TestRoot);
			BinaryData binaryData = new BinaryData();
			binaryData.SetStream(null);
			pageTemplate.Binary = binaryData;

			bool expected = false;
			bool actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor.CheckPageTemplateBinaryStream(pageTemplate);

			Assert.AreEqual(expected, actual, "PageTemplateManager.CheckPageTemplateBinaryStream did not return the expected value.");
		}

		/// <summary>
		///A test for CheckPageTemplateBinaryStream (PageTemplate)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_CheckPageTemplateBinaryStream_NullBinaryTest()
		{
			PageTemplate pageTemplate = new PageTemplate(this.TestRoot);

			bool expected = false;
			bool actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor.CheckPageTemplateBinaryStream(pageTemplate);

			Assert.AreEqual(expected, actual, "PageTemplateManager.CheckPageTemplateBinaryStream did not return the expected value.");
		}

		/// <summary>
		///A test for CheckPageTemplateBinaryStream (PageTemplate)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_CheckPageTemplateBinaryStream_NullPageTemplateTest()
		{
			PageTemplate pageTemplate = null;

			bool expected = false;
			bool actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor.CheckPageTemplateBinaryStream(pageTemplate);

			Assert.AreEqual(expected, actual, "PageTemplateManager.CheckPageTemplateBinaryStream did not return the expected value.");
		}

		/// <summary>
		///A test for GetFileNameExtension (string)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_GetFileNameExtension_Test()
		{
			string fileName = "word.doc";

			string expected = "doc";
			string actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor.GetFileNameExtension(fileName);

			Assert.AreEqual(expected, actual, "PageTemplateManager.GetFileNameExtension did not return the expected value.");
		}

		/// <summary>
		///A test for GetFileNameExtension (string)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_GetFileNameExtension_EmptyTest()
		{
			string fileName = string.Empty;

			string expected = string.Empty;
			string actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor.GetFileNameExtension(fileName);

			Assert.AreEqual(expected, actual, "PageTemplateManager.GetFileNameExtension did not return the expected value.");
		}

		/// <summary>
		///A test for GetFileNameWithoutExt ()
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_GetFileNameWithoutExt_Test()
		{
			object target = SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor.CreatePrivate();
			SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor accessor = new SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor(target);

			string expected = "word";

			PageTemplate pageTemplate = new PageTemplate(this.TestRoot);
			pageTemplate.Name = "word.doc";
			accessor.PageTemplateNode = pageTemplate;
			string actual = accessor.GetFileNameWithoutExt();

			Assert.AreEqual(expected, actual, "PageTemplateManager.GetFileNameWithoutExt did not return the expected value.");
		}

		/// <summary>
		///A test for GetFileNameWithoutExt (string)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_GetFileNameWithoutExt_StaticTest()
		{
			string fileName = "word.doc";

			string expected = "word";
			string actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor.GetFileNameWithoutExt(fileName);

			Assert.AreEqual(expected, actual, "PageTemplateManager.GetFileNameWithoutExt did not return the expected value.");
		}

		/// <summary>
		///A test for GetFileNameWithoutExt (string)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void PageTemplateManager_GetFileNameWithoutExt_StaticNullTest()
		{
			string fileName = string.Empty;

			string expected = string.Empty;
			string actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_PageTemplateManagerAccessor.GetFileNameWithoutExt(fileName);

			Assert.AreEqual(expected, actual, "PageTemplateManager.GetFileNameWithoutExt did not return the expected value.");
		}
	}
}