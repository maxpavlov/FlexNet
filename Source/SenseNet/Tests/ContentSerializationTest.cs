using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Search;
using System.IO;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using System.Xml;
using System.Xml.Linq;

namespace SenseNet.ContentRepository.Tests
{
	[TestClass]
    public class ContentSerializationTest : TestBase
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
        private static string _testRootName = "_ContentSerializationTests";
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
        public void ContentSerialization_1()
        {
            var query = new NodeQuery(new IntExpression(IntAttribute.ParentId, ValueOperator.Equal, Repository.Root.Id));
            var folder = SearchFolder.Create(query);
            var xml0 = XDocument.Load(new StreamReader(folder.GetXml(true)));
            var s0 = xml0.Element(XName.Get("Content")).Element(XName.Get("Children")).ToString();

            var content = Content.Create(Repository.Root);
            var xml1 = XDocument.Load(new StreamReader(content.GetXml(true)));
            var s1 = xml1.Element(XName.Get("Content")).Element(XName.Get("Children")).ToString();

            Assert.IsTrue(s0 == s1);
        }
    }
}
