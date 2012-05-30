using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal;

namespace SenseNet.ContentRepository.Tests
{
    /// <summary>
    ///This is a test class for SenseNet.Portal.Site and is intended
    ///to contain all SenseNet.Portal.Site Unit Tests
    ///</summary>
    [TestClass()]
    public class SiteTest : TestBase
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

        [TestMethod()]
        public void Site_Constructor()
        {
            Node parent = Repository.Root; 
            Site target = new Site(parent);
            Assert.IsNotNull(target, "1. Site is null.");
        }

        [TestMethod]
        public void Site_SiteUrl_Insert()
        {
            var originalSite = new Site(Repository.Root);
            originalSite.UrlList.Add("mytestinterneturl", "Forms");
            var originalSiteUrlList = originalSite.UrlList;
            originalSite.Save();
            var site = Node.Load<Site>(originalSite.Id);
            var b = site.UrlList.Count == originalSiteUrlList.Count;
            originalSite.ForceDelete();
            Assert.IsTrue(b, "Site url list are NOT equal.");
        }
        [TestMethod]
        public void Site_SiteUrl_InsertWithDictionary()
        {
            var originalSite = new Site(Repository.Root);
            originalSite.UrlList = new Dictionary<string, string>(originalSite.UrlList) { { "mytestinterneturl2", "Forms" } };
            var originalSiteUrlList = originalSite.UrlList;
            originalSite.Save();
            var site = Node.Load<Site>(originalSite.Id);
            bool b = site.UrlList.Count == originalSiteUrlList.Count;
            originalSite.ForceDelete();
            Assert.IsTrue(b, "Site url list are NOT equal.");
        }

        //public static class SiteTools
        //{
        //    public static void AddUrlToSite(Site s, string name, string value)
        //    {
        //        var urlList = s.UrlList;
        //        urlList.Add(name, value);
        //    }
        //    public static void AddUrlToSiteWithDictionary(Site s, string name, string value)
        //    {
        //        var newUrlList = new Dictionary<string, string>(s.UrlList) {{name, value}}; // Copy the UrlList to another object (using copy constructor).
        //        s.UrlList = newUrlList;
        //    }
        //}

    }


}