using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass()]
    public class UserTest : TestBase
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
        ///A test for User (Node)
        ///</summary>
        [TestMethod()]
        public void User_Constructor()
        {
            Node parent = Repository.Root; 

            User target = new User(parent);

            Assert.IsNotNull(target, "1. User is null.");
        }
        [TestMethod()]
        public void User_Properties_Root()
        {
            Node parent = Repository.Root;
            User target = new User(parent);

            Assert.IsFalse(target.PropertyTypes.Count == 0, "User.Properties collection is 0.");
            Assert.IsNotNull(target.HasProperty("Enabled"), "Enabled property is null.");
			Assert.IsNotNull(target.HasProperty("Email"), "Email property is null.");
			Assert.IsNotNull(target.HasProperty("FullName"), "FullName property is null.");
			Assert.IsNotNull(target.HasProperty("PasswordHash"), "PasswordHash property is null.");
        }

    }


}