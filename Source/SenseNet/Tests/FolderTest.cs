using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass()]
    public class FolderTest : TestBase
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
        ///A test for Folder (Node)
        ///</summary>
        [TestMethod()]
        public void Folder_Constructor()
        {
            Node parent = Repository.Root;

            Folder target = new Folder(parent);

            Assert.IsNotNull(target, "1. Folder is null.");
        }
        
        [TestMethod()]
        public void Folder_CanLoad()
        {
            Folder target = Repository.Root;

            Node result = Folder.LoadNode(Repository.Root.Id);

            Assert.IsInstanceOfType(result, typeof(Folder), "Result is not a Folder type.");
            Assert.IsNotNull(result, "Folder load by ID is returned null.");

            result = null;
            result = Folder.LoadNode(Repository.Root.Path);
            Assert.IsNotNull(result, "Folder load by path is returned null.");

        }

    }


}