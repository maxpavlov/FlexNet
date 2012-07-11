﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.Data;
using System.IO;

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
        public void ContentNaming_IncrementNameSuffixOnSave()
        {
            // increment check
            var content1 = Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar");
            content1.Save();
            var content2 = Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar");
            content2.Save();
            var content3 = Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar");
            content3.Save();
            Assert.IsTrue(content1.Name == "mycar");    // if mycar does not exist, name does not change
            Assert.IsTrue(content2.Name == "mycar(1)"); // first increment
            Assert.IsTrue(content3.Name == "mycar(2)"); // second increment

            // 9 - 10 order problem: if mycar(9) and mycar(10) exists, mycar(11) is the next even though 10 is smaller than 9 if compared as strings
            Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar(9)").Save();
            Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar(10)").Save();
            var content4 = Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar");
            content4.Save();
            Assert.IsTrue(content4.Name == "mycar(11)");

            // (string) suffix problem 1: string(test) should be incremented to string(test)(1)
            var content5 = Content.CreateNew(ContentType_Car1Name, TestRoot, "string(test)");
            content5.Save();
            var content6 = Content.CreateNew(ContentType_Car1Name, TestRoot, "string(test)");
            content6.Save();
            Assert.IsTrue(content5.Name == "string(test)");    // if string(test) does not exist, name does not change
            Assert.IsTrue(content6.Name == "string(test)(1)"); // first increment

            // (string) suffix problem 2: string should be incremented to string(guid), since string(test) already exists
            var content7 = Content.CreateNew(ContentType_Car1Name, TestRoot, "string");
            content7.Save();
            var content8 = Content.CreateNew(ContentType_Car1Name, TestRoot, "string");
            content8.Save();
            Assert.IsTrue(content7.Name == "string");       // did not exist yet
            Assert.IsTrue(SuffixIsGuid(content8.Name));
        }
        [TestMethod]
        public void ContentNaming_IncrementNameSuffixOnSaveWithExtension()
        {
            // increment check
            var content1 = Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar.xml");
            content1.Save();
            var content2 = Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar.xml");
            content2.Save();
            var content3 = Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar.xml");
            content3.Save();
            Assert.IsTrue(content1.Name == "mycar.xml");    // if mycar does not exist, name does not change
            Assert.IsTrue(content2.Name == "mycar(1).xml"); // first increment
            Assert.IsTrue(content3.Name == "mycar(2).xml"); // second increment

            // 9 - 10 order problem: if mycar(9) and mycar(10) exists, mycar(11) is the next even though 10 is smaller than 9 if compared as strings
            Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar(9).xml").Save();
            Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar(10).xml").Save();
            var content4 = Content.CreateNew(ContentType_Car1Name, TestRoot, "mycar.xml");
            content4.Save();
            Assert.IsTrue(content4.Name == "mycar(11).xml");

            // (string) suffix problem 1: string(test) should be incremented to string(test)(1)
            var content5 = Content.CreateNew(ContentType_Car1Name, TestRoot, "string(test).xml");
            content5.Save();
            var content6 = Content.CreateNew(ContentType_Car1Name, TestRoot, "string(test).xml");
            content6.Save();
            Assert.IsTrue(content5.Name == "string(test).xml");    // if string(test) does not exist, name does not change
            Assert.IsTrue(content6.Name == "string(test)(1).xml"); // first increment

            // (string) suffix problem 2: string should be incremented to string(guid), since string(test) already exists
            var content7 = Content.CreateNew(ContentType_Car1Name, TestRoot, "string.xml");
            content7.Save();
            var content8 = Content.CreateNew(ContentType_Car1Name, TestRoot, "string.xml");
            content8.Save();
            Assert.IsTrue(content7.Name == "string.xml");
            Assert.IsTrue(SuffixIsGuid(content8.Name));
        }
        [TestMethod]
        public void ContentNaming_AutoIncrementOnDemand()
        {
            var content1 = Content.CreateNew(ContentType_Car2Name, TestRoot, "ondemand");
            content1.Save();

            // take a non-autoincrement type, and save it with autoincrement
            bool typeNotAutoIncrement = false;
            var content2 = Content.CreateNew(ContentType_Car2Name, TestRoot, "ondemand");
            try
            {
                content2.Save();
            }
            catch (NodeAlreadyExistsException)
            {
                typeNotAutoIncrement = true;
            }
            Assert.IsTrue(typeNotAutoIncrement);    // the type is non-autoincremental

            content2.ContentHandler.AllowIncrementalNaming = true;
            content2.Save();
            Assert.IsTrue(content2.Name == "ondemand(1)");  // non-autoincremental type is saved autoincrementally
        }
        [TestMethod]
        public void ContentNaming_NameOnlyChangesAfterSave()
        {
            var content1 = Content.CreateNew(ContentType_Car1Name, TestRoot, "changeonsave");
            content1.Save();

            var content2 = Content.CreateNew(ContentType_Car1Name, TestRoot, "changeonsave");
            Assert.IsTrue(content2.Name == "changeonsave");     // name is not changed before saving, no queries are run (queries would be slow)
            content2.Save();
            Assert.IsTrue(content2.Name == "changeonsave(1)");  // name is changed after saving - we check here that name should actually be changed, so previous assert is correct
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
            msg = IncrementNameSuffixTest("(1)", "(2)"); Assert.IsNull(msg, msg); Assert.IsNull(msg, msg);
            msg = IncrementNameSuffixTest(")", ")(1)"); Assert.IsNull(msg, msg); Assert.IsNull(msg, msg);

            msg = IncrementNameSuffixTest("Car(string)(12)", "Car(string)(13)"); Assert.IsNull(msg, msg);

            // suffices are guids for the following. 
            // if the last suffixed name from db is in the form 'name(x)' where x is not a number, we are not able to decide the next suffix, so it is guid
            string nameBase;
            var actual = ContentNamingHelper.IncrementNameSuffix("Car(string)", out nameBase);
            Assert.IsTrue(SuffixIsGuid(actual));
            actual = ContentNamingHelper.IncrementNameSuffix("Car(8)).xml", out nameBase);
            Assert.IsTrue(SuffixIsGuid(actual));
            actual = ContentNamingHelper.IncrementNameSuffix("Car()", out nameBase);
            Assert.IsTrue(SuffixIsGuid(actual));
        }
        [TestMethod]
        public void ContentNaming_ParseSuffix()
        {
            string msg;
            msg = ParseSuffixTest("Car(1)", 1); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car(1).xml", 1); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car(test)(1)", 1); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car(test)(1).xml", 1); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car", 0); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car(test)", 0); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car(test)(1", 0); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car.xml", 0); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car(test).xml", 0); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car(test)(1.xml", 0); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car((8).xml", 8); Assert.IsNull(msg, msg);
            msg = ParseSuffixTest("Car(8)).xml", 0); Assert.IsNull(msg, msg);
        }
        private string IncrementNameSuffixTest(string name, string expected)
        {
            string nameBase;
            var actual = ContentNamingHelper.IncrementNameSuffix(name, out nameBase);
            if(actual == expected)
                return null;
            return String.Format("Name is {0}, expected: {1}", actual, expected);
        }
        private string ParseSuffixTest(string name, int expected)
        {
            name = Path.GetFileNameWithoutExtension(name);
            string nameBase;
            bool inValidNumber;
            var actual = ContentNamingHelper.ParseSuffix(name, out nameBase, out inValidNumber);
            if (actual == expected)
                return null;
            return String.Format("Result is {0}, expected: {1}", actual, expected);
        }
        private bool SuffixIsGuid(string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            Guid guid;
            var guidstr = name.Substring(name.Length - 36 - 1, 36);
            return Guid.TryParse(guidstr, out guid);
        }
    }
}