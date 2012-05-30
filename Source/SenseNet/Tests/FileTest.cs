using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass()]
    public class FileTest : TestBase
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

		private static string _testRootName = "_FileTests";
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

        /// <summary>
        ///A test for File (Node)
        ///</summary>
        [TestMethod()]
        public void File_Constructor()
        {
            Node parent = Repository.Root;

            File target = new File(parent);

            Assert.IsNotNull(target, "1. File is null.");
        }

        [TestMethod()]
        public void File_HasProperties()
        {
            Node parent = Repository.Root;
            File target = new File(parent);
            Assert.IsFalse(target.PropertyTypes.Count == 0, "File.Properties collection is 0.");
        }

        [TestMethod()]
        public void File_CanCreateByBinary()
        {
            Node parent = Repository.Root;
            BinaryData binary = new BinaryData();
			binary.SetStream(TestTools.GetTestStream());

            File target = File.CreateByBinary(Repository.Root, binary);

            Assert.IsNotNull(target.Size, "Size is null.");
            Assert.IsFalse(target.Size == 0, "Size is 0.");


        }

		//[TestMethod()]
		//public void File_Save_BinaryFromDefaultInstallIDWithNoChange()
		//{
		//    string path = "/Root/Public Site/About us/Management/Photos/Alex.jpg";
		//    File file = File.Load(path) as File;
		//    int old_id = file.Binary.Id;
		//    file.Save();
		//    Assert.AreEqual(old_id, file.Binary.Id, "IDs changed.");
		//}

		[TestMethod()]
		public void File_Save_BinaryIDWithNoChange()
		{
			string path = CreateTestFile();
			File file = File.LoadNode(path) as File;
			int old_id = file.Binary.Id;
			file.Save();
			Assert.AreEqual(old_id, file.Binary.Id, "IDs changed.");
		}

        [TestMethod()]
        public void File_Save_BinaryIDWithNewVersion()
        {
			string path = CreateTestFile();
			File file = File.LoadNode(path) as File;
			int old_id = file.Binary.Id;
            file.Save(VersionRaising.NextMajor, VersionStatus.Approved);
            Assert.AreNotEqual(old_id, file.Binary.Id, "IDs changed.");
        }

        //// Creating test node
        private string CreateTestFile()
        {
            File f = new File(this.TestRoot);
            f.Name = Guid.NewGuid().ToString();
            BinaryData data = new BinaryData();
			data.SetStream(TestTools.GetTestStream());
            f.Binary = data;
            f.Save();
            return RepositoryPath.Combine(this.TestRoot.Path, f.Name);
        }
    }



}