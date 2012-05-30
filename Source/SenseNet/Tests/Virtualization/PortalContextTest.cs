using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal.Virtualization;
using SenseNet.Portal;

namespace SenseNet.ContentRepository.Tests
{
    /// <summary>
    ///This is a test class for SenseNet.Portal.Virtualization.PortalContext and is intended
    ///to contain all SenseNet.Portal.Virtualization.PortalContext Unit Tests
    ///</summary>
    [TestClass()]
    public class PortalContextTest : TestBase
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

        private void CreateTestSite()
        {
            Site site = new Site(Repository.Root);
            site.Name = "Fake Test Site";
            Dictionary<string, string> urlList = new Dictionary<string, string>(3);
            urlList.Add("localhost/fakesiteforms", "Forms");
            urlList.Add("localhost/fakesitewindows", "Windows");
            urlList.Add("localhost/fakesitenone", "None");
            site.UrlList = urlList;
            site.Save();
        }

        private void CleanupTestSite()
        {
            var node = SenseNet.ContentRepository.Storage.Node.LoadNode("/Root/Fake Test Site");
            if (node != null)
                node.ForceDelete();
        }

        ///<summary>
		///A this method tests the URL - RepositoryPath conversion functionality of the PortalContext module.
        ///</summary>
        //[DeploymentItem("SenseNet.ContentRepository.dll")]
        [TestMethod()]
        public void PortalContext_RepositoryPathResolve_OffSite()
        {
            CleanupTestSite();
            CreateTestSite();

            string pagePath = "/fakesiteforms/Root/System/alma.jpg/";
            string expectedRepositoryPath = "/Root/System/alma.jpg";

            System.IO.StringWriter simulatedOutput = new System.IO.StringWriter();
            SimulatedHttpRequest simulatedWorkerRequest = new SimulatedHttpRequest(@"\", @"C:\Inetpub\wwwroot", pagePath, "", simulatedOutput, "localhost");
            HttpContext simulatedHttpContext = new HttpContext(simulatedWorkerRequest);
            PortalContext portalContext = PortalContext.Create(simulatedHttpContext);

            bool success_Path = (portalContext.RepositoryPath == expectedRepositoryPath);
            bool success_AuthMode = (portalContext.AuthenticationMode == "Forms");

            CleanupTestSite();

            Assert.IsTrue(success_Path, "success_Path");
            Assert.IsTrue(success_AuthMode, "success_AuthMode");
        }

        ///<summary>
		///A this method tests the URL - RepositoryPath conversion functionality of the PortalContext module.
        ///</summary>
        //[DeploymentItem("SenseNet.ContentRepository.dll")]
        [TestMethod()]
		public void PortalContext_RepositoryPathResolve_OnSite()
        {
            CleanupTestSite();
            CreateTestSite();

            string pagePath = "/fakesitewindows/Pictures/alma.jpg/";
            string expectedRepositoryPath = "/Root/Fake Test Site/Pictures/alma.jpg";

            System.IO.StringWriter simulatedOutput = new System.IO.StringWriter();
            SimulatedHttpRequest simulatedWorkerRequest = new SimulatedHttpRequest(@"\", @"C:\Inetpub\wwwroot", pagePath, "", simulatedOutput, "localhost");
            HttpContext simulatedHttpContext = new HttpContext(simulatedWorkerRequest);
            PortalContext portalContext = PortalContext.Create(simulatedHttpContext);

            bool success_Path = (portalContext.RepositoryPath == expectedRepositoryPath);
            bool success_AuthMode = (portalContext.AuthenticationMode == "Windows");

            CleanupTestSite();

            Assert.IsTrue(success_Path, "success_Path");
            Assert.IsTrue(success_AuthMode, "success_AuthMode");
        }


    }


}