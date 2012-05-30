using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal;

namespace SenseNet.ContentRepository.Tests
{
    /// <summary>
    ///This is a test class for SenseNet.Portal.PageTemplate and is intended
    ///to contain all SenseNet.Portal.PageTemplate Unit Tests
    ///</summary>
    [TestClass()]
    public class PageTemplateTest : TestBase
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


        /// <summary>
        ///A test for PageTemplate (Node)
        ///</summary>
        [TestMethod()]
        public void PageTemplate_Constructor()
        {
            Node parent = Repository.Root;

            PageTemplate target = new PageTemplate(parent);

            Assert.IsNotNull(target, "PageTemplate is null.");

        }
        
        /// <summary>
        ///A test for PageTemplateNode
        ///</summary>
        [TestMethod()]
        public void PageTemplate_SetMasterPageNode()
        {
            Node parent = Repository.Root;

            MasterPage target = new MasterPage(parent);
			target.Save();

            PageTemplate val = new PageTemplate(parent);

            val.MasterPageNode = target;

            Assert.IsNotNull(val.MasterPageNode, "#1");
            Assert.AreNotEqual(target, val.MasterPageNode, "#2"); // reference not equal because property getter returns a new instance.
            Assert.AreEqual(target.VersionId, val.MasterPageNode.VersionId, "#3");
        }

    }


}