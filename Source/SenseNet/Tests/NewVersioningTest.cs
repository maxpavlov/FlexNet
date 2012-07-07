using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal;
using System.IO;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.ContentRepository.Tests
{
    /// <summary>
    /// Summary description for NewVersioningTest
    /// </summary>
    [TestClass]
    public class NewVersioningTest : TestBase
    {
        private static string _testRootName = "Versioning";
        private static string _testRootPath = String.Concat("/Root/", _testRootName);

        #region PlayGround
        
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

        #region default test attributes
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
        #endregion

        [TestInitialize]
        public void PrepareTest()
        {

            foreach (Node node in ((Folder)TestRoot).Children)
            {
                int lastUnlockedId = 0;
                do
                {
                    try
                    {
                        node.ForceDelete();
                        lastUnlockedId = 0;
                    }
                    catch (LockedNodeException e)
                    {
                        lastUnlockedId = node.Id;
                        e.LockHandler.Unlock(VersionStatus.Approved, VersionRaising.None);
                    }
                } while (lastUnlockedId != 0);
            }
        }

        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.LoadNode(_testRootPath).ForceDelete();
        }
        
        #endregion

        #region Keeping data after publish

        [TestMethod]
        public void GenericContent_KeepBinaryAfterPublish()
        {
            Page samplePage = new Page(TestRoot);
            
            samplePage.Name = "SamplePage";
            samplePage.VersioningMode = ContentRepository.Versioning.VersioningType.MajorAndMinor;
            samplePage.ApprovingMode = ContentRepository.Versioning.ApprovingType.True;
            
            //set binaries
            BinaryData pageBinaryData = CreateBinaryDataFromString("Page Binary");
            BinaryData psBinaryData = CreateBinaryDataFromString("Page PersonalizationSettings");

            samplePage.Binary = pageBinaryData;
            samplePage.PersonalizationSettings = psBinaryData;

            //save page
            samplePage.Save();
            samplePage.CheckOut();
            samplePage.Publish();

            //asserts
            //TODO: CheckBinariesInPageByString hosszutavon folosleges
            CheckBinariesInPageByString(samplePage, "Page Binary", "Page PersonalizationSettings");
            CheckBinariesInPageByByte(samplePage,pageBinaryData.GetStream(),psBinaryData.GetStream());
            
        }

        [TestMethod]
        public void GenericContent_KeepReferenceAfterPublish()
        {
            //create page template
            PageTemplate samplePageTemplate = new PageTemplate(TestRoot);
            samplePageTemplate.Name = "Sample Page Template";
            samplePageTemplate.Binary = CreateBinaryDataFromString("<html><head></head><body></body></html>");
            samplePageTemplate.Save();

            Page samplePage = new Page(TestRoot);

            samplePage.Name = "SamplePage";

            //set reference
            samplePage.PageTemplateNode = samplePageTemplate;

            samplePage.Save();
            samplePage.CheckOut();
            samplePage.Publish();

            //asserts
            CheckPageTemplateInPage(samplePage, samplePageTemplate.Id);

        }

        [TestMethod]
        public void GenericContent_KeepTextPropertyAfterPublish()
        {
            Page samplePage = new Page(TestRoot);

            samplePage.Name = "SamplePage";

            //set sample page
            string textProperty = "minta property";
            samplePage.Keywords = textProperty;

            samplePage.Save();
            samplePage.CheckOut();
            samplePage.Publish();

            //asserts
            Assert.AreEqual(textProperty, samplePage.Keywords, "Keywords property doesn't contain the expected strings.");

        }

        [TestMethod]
        public void GenericContent_KeepIndexAfterPublish()
        {
            Page samplePage = new Page(TestRoot);

            samplePage.Name = "SamplePage";

            //set index
            int index = 15;
            samplePage.Index = index;

            samplePage.Save();
            samplePage.CheckOut();
            samplePage.Publish();

            //asserts
            Assert.AreEqual(index, samplePage.Index, "Index property doesn't contain the expected value.");

        }

        [TestMethod]
        public void GenericContent_KeepIconAfterPublish()
        {
            Page samplePage = new Page(TestRoot);

            samplePage.Name = "SamplePage";

            //set index
            string icon = "icon.ic";
            samplePage.Icon = icon;

            samplePage.Save();
            samplePage.CheckOut();
            samplePage.Publish();

            //asserts
            Assert.AreEqual(icon, samplePage.Icon, "Icon property doesn't contain the expected value.");

        }

        #endregion

        #region Keeping data after undocheckout

        [TestMethod]
        public void GenericContent_KeepBinaryAfterUndocheckout()
        {
            Page samplePage = new Page(TestRoot);

            samplePage.Name = "SamplePage";

            //set binaries
            BinaryData pageBinaryData = CreateBinaryDataFromString("Page Binary");
            BinaryData psBinaryData = CreateBinaryDataFromString("Page PersonalizationSettings");

            samplePage.Binary = pageBinaryData;
            samplePage.PersonalizationSettings = psBinaryData;

            //save page
            samplePage.Save();
            samplePage.CheckOut();
            samplePage.UndoCheckOut();

            //asserts
            //TODO: CheckBinariesInPageByString hosszutavon folosleges
            CheckBinariesInPageByString(samplePage, "Page Binary", "Page PersonalizationSettings");
            CheckBinariesInPageByByte(samplePage, pageBinaryData.GetStream(), psBinaryData.GetStream());

        }

        [TestMethod]
        public void GenericContent_KeepReferenceAfterUndocheckout()
        {
            //create page template
            PageTemplate samplePageTemplate = new PageTemplate(TestRoot);
            samplePageTemplate.Name = "Sample Page Template";
            samplePageTemplate.Binary = CreateBinaryDataFromString("<html><head></head><body></body></html>");
            samplePageTemplate.Save();

            Page samplePage = new Page(TestRoot);

            samplePage.Name = "SamplePage";

            //set reference
            samplePage.PageTemplateNode = samplePageTemplate;

            samplePage.Save();
            samplePage.CheckOut();
            samplePage.UndoCheckOut();

            //asserts
            CheckPageTemplateInPage(samplePage, samplePageTemplate.Id);

        }

        [TestMethod]
        public void GenericContent_KeepTextPropertyAfterUndocheckout()
        {
            Page samplePage = new Page(TestRoot);

            samplePage.Name = "SamplePage";

            //set sample page
            string textProperty = "minta property";
            samplePage.Keywords = textProperty;

            samplePage.Save();
            samplePage.CheckOut();
            samplePage.UndoCheckOut();

            //asserts
            Assert.AreEqual(textProperty, samplePage.Keywords, "Keywords property doesn't contain the expected strings.");

        }

        [TestMethod]
        public void GenericContent_KeepIndexAfterUndocheckout()
        {
            Page samplePage = new Page(TestRoot);

            samplePage.Name = "SamplePage";

            //set index
            int index = 15;
            samplePage.Index = index;

            samplePage.Save();
            samplePage.CheckOut();
            samplePage.UndoCheckOut();

            //asserts
            Assert.AreEqual(index, samplePage.Index, "Index property doesn't contain the expected value.");

        }

        [TestMethod]
        public void GenericContent_KeepIconAfterUndocheckout()
        {
            Page samplePage = new Page(TestRoot);

            samplePage.Name = "SamplePage";

            //set index
            string icon = "icon.ic";
            samplePage.Icon = icon;

            samplePage.Save();
            samplePage.CheckOut();
            samplePage.UndoCheckOut();

            //asserts
            Assert.AreEqual(icon, samplePage.Icon, "Icon property doesn't contain the expected value.");

        }

        #endregion

        // -----------------------------------------------------------
        // -----------------------------------------------------------

        #region Helpers
        
        private BinaryData CreateBinaryDataFromString(string stringToTransform)
        {
            Stream streamFromString = Tools.GetStreamFromString(stringToTransform);

            BinaryData  binaryDataFromStream = new BinaryData();
            binaryDataFromStream.SetStream(streamFromString);

            return binaryDataFromStream;
        }

        private string GetStringFromBinaryData(BinaryData binary)
        {

            Stream stream = binary.GetStream();
            StreamReader sr = new StreamReader(stream, Encoding.UTF8);

            string value = sr.ReadToEnd();

            return value;
        }

        private bool CompareStreams(Stream given, Stream expected)
        {
            int i = 0;
            int j = 0;

                do
                {
                    i = given.ReadByte();
                    j = expected.ReadByte();
                    if (i != j) break;
                } while (i != -1 && j != -1);

            return i == j;
        }


        #endregion

        #region Asserts

        private void CheckBinariesInPageByString(Page page, string expectedBinaryValue, string expectedPSettingsValue)
        {
            string binaryValue = GetStringFromBinaryData(page.Binary);
            string personalizationSettingsValue = GetStringFromBinaryData(page.PersonalizationSettings);

            Assert.AreEqual(expectedBinaryValue, binaryValue, "The Binary of the given page doesn't match the expected value.");
            Assert.AreEqual(expectedPSettingsValue, personalizationSettingsValue, "The PersonalizationSettings of the given page doesn't match the expected value.");
        }

        private void CheckBinariesInPageByByte(Page page, Stream expectedBinaryStream, Stream expectedPSettingsStream)
        {
            bool binariesEqual = CompareStreams(expectedBinaryStream, page.Binary.GetStream());
            bool psettingsEqual = CompareStreams(expectedPSettingsStream, page.PersonalizationSettings.GetStream());
            
            Assert.IsTrue(binariesEqual, "The Binary of the given page doesn't match the expected value.");
            Assert.IsTrue(psettingsEqual, "The PersonalizationSettings of the given page doesn't match the expected value.");
        }

        private void CheckPageTemplateInPage(Page page, int expectedId)
        {
            bool pageTemplatesMatch = (page.PageTemplateNode != null) && (page.PageTemplateNode.Id == expectedId);

            Assert.IsTrue(pageTemplatesMatch, "PageTemplate doesn't match the expected value.");
        }

        #endregion
    }
}
