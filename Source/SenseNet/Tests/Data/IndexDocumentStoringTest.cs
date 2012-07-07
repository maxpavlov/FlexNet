using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Search.Indexing;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.Search;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.ContentRepository.Tests.Data
{
    [TestClass]
    public class IndexDocumentStoringTest : TestBase
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
        private static string __testRootName = "_IndexDocumentStoringTest";
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
        public void IndexDocuments_LoadDocuments()
        {
            var nodes = ContentQuery.Query("InTree:/Root .AUTOFILTERS:OFF .TOP:10").Nodes;
            var versionIds = nodes.Select(n => n.VersionId).ToArray();

            var docs = StorageContext.Search.LoadIndexDocumentByVersionId(versionIds);

            Assert.IsTrue(docs.Select(doc => doc.VersionId).Except(versionIds).Count() == 0, "Returned index documents had unexpected version IDs");
            Assert.IsTrue(versionIds.Except(docs.Select(doc => doc.VersionId)).Count() == 0, "Not all version IDs were found in returned results");
        }
    }

}
