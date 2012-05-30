using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using System.Threading;
using System.IO;
using SenseNet.Portal;
using SenseNet.ContentRepository.Storage.Search;

namespace SenseNet.ContentRepository.Tests.Security
{
	[TestClass()]
    public class LockHandlerTest : TestBase
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
		private static string _rootNodePath;

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

		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			Folder f = new SystemFolder(Repository.Root);
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

		[TestMethod()]
		public void LockHandler_Lock_LockedTest()
		{
			Node node = CreateTestPage();

			LockHandler target = new LockHandler(node);

			target.Lock();
			DateTime dt1 = target.LastLockUpdate;

			Thread.Sleep(10);

			target.Lock();
			DateTime dt2 = target.LastLockUpdate;

			Assert.IsTrue(dt1 < dt2);
		}

		[TestMethod()]
		public void LockHandler_Lock_Test()
		{
			Node node = CreateTestPage();

			LockHandler target = new LockHandler(node);

			if (target.Locked)
			{
				target.Unlock(VersionStatus.Approved, VersionRaising.None);
			}
			target.Lock();

			Assert.IsTrue(
				!(string.IsNullOrEmpty(GetNodeInfo_LockToken(node))) &&
				GetNodeInfo_LockedById(node) == User.Current.Id &&
				GetNodeInfo_LockDate(node) != DateTime.MinValue &&
				GetNodeInfo_LastLockUpdate(node) != DateTime.MinValue &&
				GetNodeInfo_LockTimeout(node) > 0
			);
		}

		[TestMethod()]
		public void LockHandler_Unlock_Test()
		{
			Node node = CreateTestPage();

			LockHandler target = new LockHandler(node);
			if (!target.Locked)
			{
				target.Lock();
			}
			target.Unlock(VersionStatus.Approved, VersionRaising.None);

			Assert.IsTrue(!GetNodeInfo_Locked(node));
		}

		[TestMethod()]
		public void LockHandler_RefreshLock_Test()
		{
			Node node = CreateTestPage();

			LockHandler target = new LockHandler(node);
			if (!target.Locked)
			{
				target.Lock();
			}
			long time = GetNodeInfo_LastLockUpdate(node).Ticks;

			Thread.Sleep(10);

			target.RefreshLock();

			Assert.IsTrue(GetNodeInfo_LastLockUpdate(node).Ticks > time);
		}


        [TestMethod]
        public void LockHandler_UnlockAndVersionRaising()
        {
            var folderName = "UnlockAndVersionRaising";
            var contentName = "Car1";
            var testRoot = Node.Load<Folder>(_rootNodePath);
            var folderPath = RepositoryPath.Combine(_rootNodePath, folderName);
            var folder = Node.Load<Folder>(folderPath);
            if (folder != null)
                folder.ForceDelete();

            folder = new Folder(testRoot);
            folder.Name = folderName;
            folder.InheritableVersioningMode = SenseNet.ContentRepository.Versioning.InheritableVersioningType.MajorAndMinor;
            folder.Save();

            List<VersionNumber> versions;
            var contentPath = RepositoryPath.Combine(folderPath, contentName);

            var content = Content.CreateNew("Car", folder, contentName);
            content.Save();
            versions = Node.GetVersionNumbers(content.Id);

            content = Content.Load(contentPath);
            content.Publish();

            content = Content.Load(contentPath);
            ((GenericContent)content.ContentHandler).CheckOut();

            content = Content.Load(contentPath);
            content.Publish();

            content = Content.Load(contentPath);
            ((GenericContent)content.ContentHandler).CheckOut();

            content = Content.Load(contentPath);
            content.Publish();
        }

		//======================================================================== Helpers

		private Page CreateTestPage()
		{
			string testPagePath = RepositoryPath.Combine(_rootNodePath, Guid.NewGuid().ToString());
			if (Node.Exists(testPagePath))
                Node.ForceDelete(testPagePath);

			Page f = new Page(Node.LoadNode(_rootNodePath));
			f.PageTemplateNode = PageTemplate.LoadNode(_pageTemplatePath) as PageTemplate;
			f.Save();
			return f;
		}

		private bool GetNodeInfo_Locked(Node node)
		{
			return (bool)GetNodeInfo(node).Invoke("get_Locked");
		}
		private string GetNodeInfo_LockToken(Node node)
		{
			return (string)GetNodeInfo(node).Invoke("get_LockToken");
		}
		private int GetNodeInfo_LockedById(Node node)
		{
			return (int)GetNodeInfo(node).Invoke("get_LockedById");
		}
		private int GetNodeInfo_LockTimeout(Node node)
		{
			return (int)GetNodeInfo(node).Invoke("get_LockTimeout");
		}
		private DateTime GetNodeInfo_LockDate(Node node)
		{
			return (DateTime)GetNodeInfo(node).Invoke("get_LockDate");
		}
		private DateTime GetNodeInfo_LastLockUpdate(Node node)
		{
			return (DateTime)GetNodeInfo(node).Invoke("get_LastLockUpdate");
		}
		private PrivateObject GetNodeInfo(Node node)
		{
			var po = new PrivateObject(node);
			var po2 = po.Invoke("get_Data");
			return new PrivateObject(po2);
		}
	}
}