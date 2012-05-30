using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Versioning;
using System.Reflection;

namespace SenseNet.ContentRepository.Tests.Versioning
{
    [TestClass]
    public class VersionExistenceTests : TestBase
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

        #region Playground
        private static string _testRootName = "_VersionExistenceTests";
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
        #endregion

        [TestMethod]
        public void Versioning_LostVersion_NodeDataIsNull()
        {
            //-- Preparing
            var content = Content.CreateNew("Car", TestRoot, "car");
            var gcontent = (GenericContent)content.ContentHandler;
            gcontent.ApprovingMode = ApprovingType.False;
            gcontent.VersioningMode = VersioningType.None;
            gcontent.Save();
            var contentId = gcontent.Id;

            //-- Thread #1
            gcontent.CheckOut();

            //-- Thread #2
            var head = DataBackingStore.GetNodeHead(contentId);

            //-- Thread #1
            gcontent.CheckIn();

            //-- Thread #2
            var data = DataBackingStore.GetNodeData(head, head.LastMinorVersionId).NodeData;
            Assert.IsNull(data);
        }
        [TestMethod]
        public void Versioning_LostVersion_NodeIsNotNull()
        {
            //-- Preparing
            var content = Content.CreateNew("Car", TestRoot, "car");
            var gcontent = (GenericContent)content.ContentHandler;
            gcontent.ApprovingMode = ApprovingType.False;
            gcontent.VersioningMode = VersioningType.None;
            gcontent.Save();
            var contentId = gcontent.Id;

            //-- Thread #1
            gcontent.CheckOut();

            //-- Thread #2
            var head = DataBackingStore.GetNodeHead(contentId);

            //-- Thread #1
            gcontent.CheckIn();

            //-- Thread #2
            var node = LoadNode(head, VersionNumber.LastAccessible);
            Assert.IsNotNull(node);
        }
        [TestMethod]
        public void Versioning_LostVersion_NodeIsDeleted()
        {
            //-- Preparing
            var content = Content.CreateNew("Car", TestRoot, "car");
            var gcontent = (GenericContent)content.ContentHandler;
            gcontent.ApprovingMode = ApprovingType.False;
            gcontent.VersioningMode = VersioningType.None;
            gcontent.Save();
            var contentId = gcontent.Id;

            //-- Thread #1
            gcontent.CheckOut();

            //-- Thread #2
            var head = DataBackingStore.GetNodeHead(contentId);

            //-- Thread #1
            gcontent.ForceDelete();

            //-- Thread #2
            var node = LoadNode(head, VersionNumber.LastAccessible);
            Assert.IsNull(node);
        }

        [TestMethod]
        public void Versioning_LostVersions_AllNodesAreExist()
        {
            //-- Preparing
            //var gcontents = (from i in  Enumerable.Repeat(typeof(int), 5) select (GenericContent)Content.CreateNew("Car", TestRoot, "car").ContentHandler).ToArray();
            //var ids = gcontents.Select(i =>
            //{
            //    var c = (GenericContent)Content.CreateNew("Car", TestRoot, "car").ContentHandler;
            //    c.ApprovingMode = ApprovingType.False;
            //    c.VersioningMode = VersioningType.None;
            //    c.Save();
            //    return c;
            //}).Select(c => c.Id).ToArray();
            //gcontents = Node.LoadNodes(ids).Cast<GenericContent>().ToArray();
            //var versionids = gcontents.Select(c => c.VersionId).ToArray();

            var gcontents = new GenericContent[5];
            var ids = new int[5];
            var versionids = new int[5];
            for (int i = 0; i < gcontents.Length; i++)
            {
                var c = (GenericContent)Content.CreateNew("Car", TestRoot, "car").ContentHandler;
                c.ApprovingMode = ApprovingType.False;
                c.VersioningMode = VersioningType.None;
                c.Save();

                gcontents[i] = c;
                ids[i] = c.Id;
                versionids[i] = c.VersionId;
            }

            //-- Thread #1
            gcontents[1].CheckOut();
            gcontents[3].CheckOut();

            //-- Thread #2
            var heads = DataBackingStore.GetNodeHeads(ids);

            //-- Thread #1
            gcontents[1].CheckIn();
            gcontents[3].CheckIn();

            //-- Thread #2
            var nodes = LoadNodes(heads, VersionNumber.LastAccessible);
            var v2 = nodes.Select(c => c.VersionId).ToArray();

            Assert.IsTrue(versionids.Except(v2).Count() == 0);
        }
        [TestMethod]
        public void Versioning_LostVersions_TwoNodesAreDeleted()
        {
            //-- Preparing
            var gcontents = (from i in Enumerable.Repeat(typeof(int), 5) select (GenericContent)Content.CreateNew("Car", TestRoot, "car").ContentHandler).ToArray();
            var ids = gcontents.Select(i =>
            {
                var c = (GenericContent)Content.CreateNew("Car", TestRoot, "car").ContentHandler;
                c.ApprovingMode = ApprovingType.False;
                c.VersioningMode = VersioningType.None;
                c.Save();
                return c;
            }).Select(c => c.Id).ToArray();
            gcontents = Node.LoadNodes(ids).Cast<GenericContent>().ToArray();
            var versionids = gcontents.Select(c => c.VersionId).ToArray();

            //-- Thread #1
            gcontents[1].CheckOut();
            gcontents[3].CheckOut();

            //-- Thread #2
            var heads = DataBackingStore.GetNodeHeads(ids);

            //-- Thread #1
            gcontents[1].CheckIn();
            gcontents[3].ForceDelete();
            gcontents[2].ForceDelete();

            //-- Thread #2
            var nodes = LoadNodes(heads, VersionNumber.LastAccessible);
            var v2 = nodes.Select(c => c.VersionId).ToArray();

            var diff = versionids.Except(v2);
            Assert.IsTrue(diff.Count() == 2);
            Assert.IsTrue(diff.Contains(versionids[2]));
            Assert.IsTrue(diff.Contains(versionids[3]));
        }

        private Node LoadNode(NodeHead head, VersionNumber version)
        {
            var nodeAcc = new PrivateType(typeof(Node));
            var node = (Node)nodeAcc.InvokeStatic("LoadNode", head, version);
            return node;
        }
        private List<Node> LoadNodes(IEnumerable<NodeHead> heads, VersionNumber version)
        {
            var nodeAcc = new PrivateType(typeof(Node));
            var nodes = (List<Node>)nodeAcc.InvokeStatic("LoadNodes", heads, version);
            return nodes;
        }
    }
}
