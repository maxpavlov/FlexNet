using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Tests.Data
{
    [TestClass]
    public class IndexBackupRestoreTest : TestBase
    {
        #region Test infrastructure
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
        //Use ClassInitialize to run code before running the first test in the class
        #endregion
        #region Playground
        private static string __testRootName = "_IndexBackupRestoreTest";
        private static string _testRootPath = String.Concat("/Root/", __testRootName);
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
                        node.Name = __testRootName;
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
        #endregion

        [TestMethod]
        public void IndexBackup_BackupFileAndDbEntryAreExist()
        {
            var backupDirPath = System.IO.Path.Combine(TestContext.TestDeploymentDir,
                SenseNet.ContentRepository.Storage.StorageContext.Search.IndexDirectoryBackupPath);
            var backupZipPath = System.IO.Path.Combine(backupDirPath, SenseNet.Search.Indexing.BackupTools.BACKUPFILENAME);
            var recoveredZipPath = System.IO.Path.Combine(backupDirPath, SenseNet.Search.Indexing.BackupTools.RECOVEREDFILENAME);

            var dummyNodesArray = SenseNet.Search.ContentQuery.Query("Type:ContentType .SKIP:10 .TOP:3 .AUTOFILTERS:OFF").Nodes.ToArray();

            var content = Content.CreateNew("Car", TestRoot, "CarBackup");
            content.ContentHandler.Index = 1;
            content.Save();
            var id = content.Id;
            var node = Node.LoadNode(id);

            SenseNet.Search.Indexing.BackupTools.BackupIndex();

            var count = 0;
            while (++count < 100)
            {
                node.Index++;
                node.Save();

                if (System.IO.File.Exists(backupZipPath))
                    break;
            }
            var backup = SenseNet.ContentRepository.Storage.Data.DataProvider.RecoverIndexBackupFromDb(recoveredZipPath);
            Assert.IsTrue(System.IO.File.Exists(backupZipPath), "Zip file does not exist.");
            Assert.IsNotNull(backup, "Backup file is not stored in the database.");

        }
    }
}
