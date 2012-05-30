using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ContentRepository.Tests.Data
{
    /// <summary>
    ///This is a test class for SenseNet.ContentRepository.Storage.Data.ContextHandler and is intended
    ///to contain all SenseNet.ContentRepository.Storage.Data.ContextHandler Unit Tests
    ///</summary>
    [TestClass()]
    public class ContextHandlerTest : TestBase
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
        ///A test for GetObject (string)
        ///</summary>
		[DeploymentItem("SenseNet.Storage.dll")]
        [TestMethod()]
        public void ContextHandler_GetObject()
        {
            string ident = Guid.NewGuid().ToString();
            object expected = null;

            object actual = ContextHandler.GetObject(ident);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SetObject (string, object)
        ///</summary>
		[DeploymentItem("SenseNet.Storage.dll")]
        [TestMethod()]
        public void ContextHandler_SetObject()
        {
            string ident = Guid.NewGuid().ToString();
            object obj = "TestValue";

            ContextHandler.SetObject(ident, obj);
            object actual = ContextHandler.GetObject(ident);

            Assert.AreEqual(obj, actual);
        }

    }


}