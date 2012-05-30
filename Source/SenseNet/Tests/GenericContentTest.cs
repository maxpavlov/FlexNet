using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Portal;

namespace SenseNet.ContentRepository.Tests
{
	/// <summary>
	///This is a test class for SenseNet.ContentRepository.GenericContent and is intended
	///to contain all SenseNet.ContentRepository.GenericContent Unit Tests
	///</summary>
	[TestClass()]
    public class GenericContentTest : TestBase
	{
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
		//Use ClassInitialize to run code before rucming the first test in the class
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
		//Use TestInitialize to run code before rucming each test
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


		private static string _testRootName = "_GenericContentTests";
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
        }


		//-------------------------------------------------------------------- Save ---------------------

		[TestMethod()]
		public void GenericContent_Save_VersionModeNone_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.False;

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Save();

			Assert.AreEqual(vn, test.Version, "#1");
			Assert.AreEqual(test.Version.Status, VersionStatus.Approved, "#2");
			Assert.AreNotEqual(cm, test.CustomMeta, "#3");
		}

		[TestMethod()]
		public void GenericContent_Save_VersionModeMajorOnly_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.False;

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Save();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#3");
			Assert.AreNotEqual(cm, test.CustomMeta, "#4");
		}

		[TestMethod()]
		public void GenericContent_Save_VersionModeMajorAndMinor_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.False;

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Save();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Draft, "#2");
			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == vn.Minor + 1, "#3");
			Assert.AreNotEqual(cm, test.CustomMeta, "#4");
		}

		[TestMethod()]
		public void GenericContent_Save_VersionModeInherited_ApprovingFalse_Test()
		{
			Page parent = CreatePage("GCSaveTestParent");
			parent.InheritableVersioningMode = InheritableVersioningType.MajorOnly;
			parent.Save();

			Page test = CreatePage("GCSaveTest", parent);

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.Inherited;
			test.ApprovingMode = ApprovingType.False;

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Save();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#3");
			Assert.AreNotEqual(cm, test.CustomMeta, "#4");
		}

		[TestMethod()]
		public void GenericContent_Save_VersionModeNone_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.True;

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Save();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Pending, "#2");
			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#3");
			Assert.AreNotEqual(cm, test.CustomMeta, "#4");
		}

		[TestMethod()]
		public void GenericContent_Save_VersionModeMajorOnly_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.True;

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Save();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Pending, "#2");
			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#3");
			Assert.AreNotEqual(cm, test.CustomMeta, "#4");
		}

		[TestMethod()]
		public void GenericContent_Save_VersionModeMajorAndMinor_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.True;

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Save();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Draft, "#2");
			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == vn.Minor + 1, "#3");
			Assert.AreNotEqual(cm, test.CustomMeta, "#4");
		}

		[TestMethod()]
		public void GenericContent_Save_VersionModeMajorOnly_ApprovingInherited_Test()
		{
			Page parent = CreatePage("GCSaveTestParent");
			parent.InheritableApprovingMode = ApprovingType.True;
			parent.Save();

			Page test = CreatePage("GCSaveTest", parent);

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.Inherited;

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Save();

			Assert.IsTrue(test.Version.Status == VersionStatus.Pending, "#2");
			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 0, "#3");
			Assert.AreNotEqual(cm, test.CustomMeta, "#4");
		}

		//-------------------------------------------------------------------- SaveSameVersion ----------

		[TestMethod()]
		public void GenericContent_SaveSameVersion_VersionModeNone_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.False;

			test.SaveSameVersion();

			Assert.AreEqual(vn, test.Version, "#1");
			Assert.AreEqual(test.Version.Status, VersionStatus.Approved, "#2");
		}

		[TestMethod()]
		public void GenericContent_SaveSameVersion_VersionModeMajorOnly_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.False;

			test.SaveSameVersion();

			Assert.AreEqual(vn, test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
		}

		[TestMethod()]
		public void GenericContent_SaveSameVersion_VersionModeMajorAndMinor_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.False;

			test.SaveSameVersion();

			Assert.AreEqual(vn, test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
		}

		[TestMethod()]
		public void GenericContent_SaveSameVersion_VersionModeNone_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.True;

			test.SaveSameVersion();

			Assert.AreEqual(vn, test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
		}

		[TestMethod()]
		public void GenericContent_SaveSameVersion_VersionModeMajorOnly_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.True;

			test.SaveSameVersion();
			
			Assert.AreEqual(vn, test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
		}

		[TestMethod()]
		public void GenericContent_SaveSameVersion_VersionModeMajorAndMinor_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.True;

			test.SaveSameVersion();

			Assert.AreEqual(vn, test.Version, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
		}

		//-------------------------------------------------------------------- CheckOut -----------------

		[TestMethod()]
		public void GenericContent_CheckOut_VersionModeNone_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#2");
			Assert.IsTrue(test.Version.Status == VersionStatus.Locked, "#3");
		}

		[TestMethod()]
		public void GenericContent_CheckOut_VersionModeMajorOnly_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#2");
			Assert.IsTrue(test.Version.Status == VersionStatus.Locked, "#3");
		}

		[TestMethod()]
		public void GenericContent_CheckOut_VersionModeMajorAndMinor_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor > 0, "#2");
			Assert.IsTrue(test.Version.Status == VersionStatus.Locked, "#3");
		}

		[TestMethod()]
		public void GenericContent_CheckOut_VersionModeNone_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#2");
			Assert.IsTrue(test.Version.Status == VersionStatus.Locked, "#3");
		}

		[TestMethod()]
		public void GenericContent_CheckOut_VersionModeMajorOnly_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#2");
			Assert.IsTrue(test.Version.Status == VersionStatus.Locked, "#3");
		}

		[TestMethod()]
		public void GenericContent_CheckOut_VersionModeMajorAndMinor_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			Assert.IsTrue(vn < test.Version, "#1");
			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor > 0, "#2");
			Assert.IsTrue(test.Version.Status == VersionStatus.Locked, "#3");
		}

		//-------------------------------------------------------------------- CheckIn ------------------

		[TestMethod()]
		public void GenericContent_CheckIn_VersionModeNone_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.CheckIn();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");

			List<VersionNumber> vnList = Node.GetVersionNumbers(test.Id);

			Assert.IsTrue(vnList.Count == 1, "#4");
		}

		[TestMethod()]
		public void GenericContent_CheckIn_VersionModeMajorOnly_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.CheckIn();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");

			List<VersionNumber> vnList = Node.GetVersionNumbers(test.Id);

			Assert.IsTrue(vnList.Count > 1, "#4");
		}

		[TestMethod()]
		public void GenericContent_CheckIn_VersionModeMajorAndMinor_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.CheckIn();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor > 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Draft, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");

			List<VersionNumber> vnList = Node.GetVersionNumbers(test.Id);

			Assert.IsTrue(vnList.Count > 1, "#4");
		}

		[TestMethod()]
		public void GenericContent_CheckIn_VersionModeNone_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.CheckIn();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Pending, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");

			List<VersionNumber> vnList = Node.GetVersionNumbers(test.Id);

			Assert.IsTrue(vnList.Count > 1, "#4");
		}

		[TestMethod()]
		public void GenericContent_CheckIn_VersionModeMajorOnly_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.CheckIn();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Pending, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");

			List<VersionNumber> vnList = Node.GetVersionNumbers(test.Id);

			Assert.IsTrue(vnList.Count > 1, "#4");
		}

		[TestMethod()]
		public void GenericContent_CheckIn_VersionModeMajorAndMinor_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.CheckIn();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor > 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Draft, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");

			List<VersionNumber> vnList = Node.GetVersionNumbers(test.Id);

			Assert.IsTrue(vnList.Count > 1, "#4");
		}

		//-------------------------------------------------------------------- UndoCheckOut -------------
		
		[TestMethod()]
		public void GenericContent_UndoCheckOut_VersionModeNone_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.UndoCheckOut();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreEqual(test.CustomMeta, cm, "#3");

			List<VersionNumber> vnList = Node.GetVersionNumbers(test.Id);

			Assert.IsTrue(vnList.Count == 1, "#4");
		}

		[TestMethod()]
		public void GenericContent_UndoCheckOut_VersionModeMajorOnly_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.UndoCheckOut();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_UndoCheckOut_VersionModeMajorAndMinor_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.UndoCheckOut();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_UndoCheckOut_VersionModeNone_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.UndoCheckOut();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_UndoCheckOut_VersionModeMajorOnly_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.UndoCheckOut();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_UndoCheckOut_VersionModeMajorAndMinor_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.UndoCheckOut();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreEqual(test.CustomMeta, cm, "#3");
		}

		//-------------------------------------------------------------------- Publish ------------------
		
		[TestMethod()]
        [ExpectedException(typeof(InvalidContentActionException))]
		public void GenericContent_Publish_VersionModeNone_ApprovingFalse_Test()
		{
            //Assert.Inconclusive("Approving off, None: CheckedOut ==> Publish");

			Page test = CreatePage("GCSaveTest");

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

            Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
            Assert.IsTrue(test.Version.Status == VersionStatus.Locked, "#2");

            //this throws an exception: cannot publish 
            //the content if approving is OFF
			test.Publish();
		}
        [TestMethod()]
        public void GenericContent_Save_CheckOutIn_VersionModeNone_ApprovingFalse_Bug3167()
        {
            var page = CreatePage("GCSaveTest");
            var pageId = page.Id;
            var versionString1 = page.Version.ToString();
            page.Binary.SetStream(Tools.GetStreamFromString("Binary1"));
            page.PersonalizationSettings.SetStream(Tools.GetStreamFromString("PersonalizationSettings1"));
            page.VersioningMode = VersioningType.None;
            page.ApprovingMode = ApprovingType.False;
            page.Save();

            page = Node.Load<Page>(pageId);
            page.CheckOut();
            var versionString2 = page.Version.ToString();
            page.Binary.SetStream(Tools.GetStreamFromString("Binary2"));
            page.PersonalizationSettings.SetStream(Tools.GetStreamFromString("PersonalizationSettings2"));
            page.Save();

            var versionString3 = page.Version.ToString();

            //page = Node.Load<Page>(pageId);

            page.CheckIn();

            page = Node.Load<Page>(pageId);

            var versionString4 = page.Version.ToString();
            var vnList = Node.GetVersionNumbers(page.Id);
            var binString = Tools.GetStreamString(page.Binary.GetStream());
            var persString = Tools.GetStreamString(page.PersonalizationSettings.GetStream());

            Assert.IsTrue(binString == "Binary2", "#1");
            Assert.IsTrue(page.Version.Major == 1 && page.Version.Minor == 0, "#2");
            Assert.IsTrue(persString == "PersonalizationSettings2", "#3");
            Assert.IsTrue(vnList.Count() == 1, "#3");
            Assert.IsTrue(versionString1 == "V1.0.A", "#4");
            Assert.IsTrue(versionString2 == "V2.0.L", "#5");
            Assert.IsTrue(versionString3 == "V2.0.L", "#6");
            Assert.IsTrue(versionString4 == "V1.0.A", "#7");
        }

		[TestMethod()]
        [ExpectedException(typeof(InvalidContentActionException))]
		public void GenericContent_Publish_VersionModeMajorOnly_ApprovingFalse_Test()
		{
            //Assert.Inconclusive("Approving off, MajorOnly: CheckedOut ==> Publish");

			Page test = CreatePage("GCSaveTest");

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

            Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
            Assert.IsTrue(test.Version.Status == VersionStatus.Locked, "#2");

            //this throws an exception: cannot publish 
            //the content if approving is OFF
			test.Publish();
		}

		[TestMethod()]
		public void GenericContent_Publish_VersionModeMajorAndMinor_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.False;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Publish();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_Checkin_VersionModeNone_ApprovingTrue_Test()
		{
            //Assert.Inconclusive("Approving on, None: CheckedOut ==> Publish");

            var test = CreatePage("GCSaveTest");
			var cm = test.CustomMeta;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.CheckIn();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Pending, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_Checkin_VersionModeMajorOnly_ApprovingTrue_Test()
		{
            //Assert.Inconclusive("Approving on, Major: CheckedOut ==> Publish");
            
            var test = CreatePage("GCSaveTest");

			var vn = test.Version;
			var cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.CheckIn();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Pending, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_Publish_VersionModeMajorAndMinor_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.True;

			test.CheckOut();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Publish();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 1, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Pending, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		//-------------------------------------------------------------------- Approve ------------------

		[TestMethod()]
		public void GenericContent_Approve_VersionModeNone_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.True;

			test.Save();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Approve();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_Approve_VersionModeMajorOnly_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.True;

			test.Save();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Approve();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_Approve_VersionModeMajorAndMinor_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.True;

			test.Save();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Publish();

			test.Approve();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Approved, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		//-------------------------------------------------------------------- Reject -------------------

		[TestMethod()]
		public void GenericContent_Reject_VersionModeNone_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.True;

			test.Save();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Reject();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Rejected, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_Reject_VersionModeMajorOnly_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorOnly;
			test.ApprovingMode = ApprovingType.True;

			test.Save();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Reject();

			Assert.IsTrue(test.Version.Major == 2 && test.Version.Minor == 0, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Rejected, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		[TestMethod()]
		public void GenericContent_Reject_VersionModeMajorAndMinor_ApprovingTrue_Test()
		{
			Page test = CreatePage("GCSaveTest");

			VersionNumber vn = test.Version;
			string cm = test.CustomMeta;

			test.VersioningMode = VersioningType.MajorAndMinor;
			test.ApprovingMode = ApprovingType.True;

			test.Save();

			test.CustomMeta = Guid.NewGuid().ToString();

			test.Publish();

			test.Reject();

			Assert.IsTrue(test.Version.Major == 1 && test.Version.Minor == 1, "#1");
			Assert.IsTrue(test.Version.Status == VersionStatus.Rejected, "#2");
			Assert.AreNotEqual(test.CustomMeta, cm, "#3");
		}

		//-------------------------------------------------------------------- Exception ----------------

		[TestMethod()]
		[ExpectedException(typeof(InvalidContentActionException))]
		public void GenericContent_Exception_VersionModeNone_ApprovingFalse_Test()
		{
			Page test = CreatePage("GCSaveTest");

			test.VersioningMode = VersioningType.None;
			test.ApprovingMode = ApprovingType.False;

			test.Reject();
		}

		//-------------------------------------------------------------------- Helper methods -----------

		public Page CreatePage(string name)
		{
			return CreatePage(name, this.TestRoot);
		}

		public static Page CreatePage(string name, Node parent)
		{
			Page page = Node.LoadNode(string.Concat(parent.Path, "/", name)) as Page;
            if (page != null)
                page.ForceDelete();
			page = new Page(parent);
			page.Name = name;
			page.Save();

			return page;
		}
	}
}