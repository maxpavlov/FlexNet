using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass]
    public class AutoNamingTests : TestBase
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
        private static string _testRootName = "_AutoNamingTests";
        private static string _testRootPath = String.Concat("/Root/", _testRootName);
        /// <summary>
        /// Do not use. Instead of TestRoot property
        /// </summary>
        private Node _testRoot;
        private static string ContentType_Car1Name = "Car1_AutoNamingTests";
        private static string ContentType_Car2Name = "Car2_AutoNamingTests";
        private static ContentType ContentType_Car1;
        private static ContentType ContentType_Car2;
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
        [ClassInitialize]
        public static void InstallContentTypes(TestContext testContext)
        {
            var ctdformat = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <ContentType name=""{0}"" parentType=""Car"" handler=""SenseNet.ContentRepository.GenericContent""
                             xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
                  <AllowIncrementalNaming>{1}</AllowIncrementalNaming>
                </ContentType>";
            var ctd1 = String.Format(ctdformat, ContentType_Car1Name, "true");
            var ctd2 = String.Format(ctdformat, ContentType_Car2Name, "false");
            ContentTypeInstaller.InstallContentType(ctd1, ctd2);
            ContentType_Car1 = ContentType.GetByName(ContentType_Car1Name);
            ContentType_Car2 = ContentType.GetByName(ContentType_Car2Name);
        }
        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.ForceDelete(_testRootPath);
            if (ContentType_Car1 != null)
                ContentTypeInstaller.RemoveContentType(ContentType_Car1);
            if (ContentType_Car2 != null)
                ContentTypeInstaller.RemoveContentType(ContentType_Car2);
        }
        #endregion

        [TestMethod]
        public void ContentNaming_FromDisplayName()
        {
            Assert.AreEqual("arvizturotukorfurogep", ContentNamingHelper.GetNameFromDisplayName("árvíztűrőtükörfúrógép"));
            Assert.AreEqual("ARVIZTUROTUKORFUROGEP", ContentNamingHelper.GetNameFromDisplayName("ÁRVÍZTŰRŐTÜKÖRFÚRÓGÉP"));
            Assert.AreEqual("ArvizturoTukorfurogep", ContentNamingHelper.GetNameFromDisplayName("ÁrvíztűrőTükörfúrógép"));
            Assert.AreEqual("a-()b-c", ContentNamingHelper.GetNameFromDisplayName("!@#$%a^&*()b{}|:<>?c/.,"));
            Assert.AreEqual("arvizturotukorfurogep.txt", ContentNamingHelper.GetNameFromDisplayName("árvíztűrőtükörfúrógép.txt"));
            Assert.AreEqual("arvizturotukorfurogep.doc.txt", ContentNamingHelper.GetNameFromDisplayName("árvíztűrőtükörfúrógép.txt", "árvíztűrőtükörfúrógép.doc"));
            Assert.AreEqual("arvizturotukorfurogep.doc.txt", ContentNamingHelper.GetNameFromDisplayName(".txt", "árvíztűrőtükörfúrógép.doc"));
        }
        [TestMethod]
        public void ContentNaming_AllowIncrementalNaming_Allowed()
        {
            Content content1, content2;
            do
            {
                content1 = Content.CreateNew(ContentType_Car1Name, TestRoot, null);
                content2 = Content.CreateNew(ContentType_Car1Name, TestRoot, null);
            } while (content1.Name != content2.Name);
            content1.Save();
            content2.Save();
        }
        [TestMethod]
        [ExpectedException(typeof(NodeAlreadyExistsException))]
        public void ContentNaming_AllowIncrementalNaming_Disallowed()
        {
            Content content1, content2;
            do
            {
                content1 = Content.CreateNew(ContentType_Car2Name, TestRoot, null);
                content2 = Content.CreateNew(ContentType_Car2Name, TestRoot, null);
            } while (content1.Name != content2.Name);
            content1.Save();
            content2.Save();
        }

        [TestMethod]
        public void ContentNaming_IncrementNameSuffix()
        {
            string msg;

            msg = IncrementNameSuffixTest("Car", "Car(1)"); Assert.IsNull(msg, msg);
            msg = IncrementNameSuffixTest("Car(12)", "Car(13)"); Assert.IsNull(msg, msg);
            msg = IncrementNameSuffixTest("Car.xml", "Car(1).xml"); Assert.IsNull(msg, msg);
            msg = IncrementNameSuffixTest("Car(8).xml", "Car(9).xml"); Assert.IsNull(msg, msg);
            msg = IncrementNameSuffixTest("Car((8).xml", "Car((9).xml"); Assert.IsNull(msg, msg);
            msg = IncrementNameSuffixTest("Car(8)).xml", "Car(8))(1).xml"); Assert.IsNull(msg, msg);
            msg = IncrementNameSuffixTest("Car()", "Car()(1)"); Assert.IsNull(msg, msg); Assert.IsNull(msg, msg);
            msg = IncrementNameSuffixTest("(1)", "(2)"); Assert.IsNull(msg, msg); Assert.IsNull(msg, msg);
            msg = IncrementNameSuffixTest(")", ")(1)"); Assert.IsNull(msg, msg); Assert.IsNull(msg, msg);
        }
        private string IncrementNameSuffixTest(string name, string expected)
        {
            var actual = ContentNamingHelper.IncrementNameSuffix(name);
            if(actual == expected)
                return null;
            return String.Format("Name is {0}, expected: {1}", actual, expected);
        }
    }
}
