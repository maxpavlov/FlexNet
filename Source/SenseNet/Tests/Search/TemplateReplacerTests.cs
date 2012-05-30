using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Portal.Virtualization;
using SenseNet.Search;
using SenseNet.Portal;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search.Parser;

namespace SenseNet.ContentRepository.Tests.Search
{
    [TestClass]
    public class TemplateReplacerTests : TestBase
    {
        private TestContext testContextInstance;
        public override TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private const string TestSiteName = "TempRepTestSite";

        private static string TestSitePath
        {
            get { return RepositoryPath.Combine("/Root", TestSiteName); }
        }

        //================================================== Test infrastructure

        [ClassCleanup]
        public static void RemoveSite()
        {
            CleanupTestSite();
        }


        //================================================== Test methods

        [TestMethod]
        public void LucQueryTemplateReplacer_1()
        {
            var text = "@@CurrentUser@@";
            var expected = User.Current.Id.ToString();
            Assert.AreEqual(expected, LucQueryTemplateReplacer.ReplaceTemplates(text));

            text = " @@CurrentUser@@ ";
            expected = string.Format(" {0} ", User.Current.Id);
            Assert.AreEqual(expected, LucQueryTemplateReplacer.ReplaceTemplates(text));
        }

        [TestMethod]
        public void LucQueryTemplateReplacer_2()
        {
            var text = "@@CurrentUser.Path@@";
            Assert.AreEqual(User.Current.Path, LucQueryTemplateReplacer.ReplaceTemplates(text));

            //text = "@@CurrentSite.Path@@";
            //Assert.AreEqual(Site.Current.Path, LucQueryTemplateReplacer.ReplaceTemplates(text));

            text = "@@CurrentDate@@";
            var expected = DateTime.Today.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern);
            Assert.AreEqual(expected, LucQueryTemplateReplacer.ReplaceTemplates(text));
        }

        [TestMethod]
        public void LucQueryTemplateReplacer_3()
        {
            //CreatePortalContext();

            var text = "ABC @@CurrentUser@@ DEF";
            var expected = string.Format("ABC {0} DEF", User.Current.Id);
            Assert.AreEqual(expected, LucQueryTemplateReplacer.ReplaceTemplates(text));

            text = "ABC @@CurrentUser@@ DEF @@CurrentUser.Path@@ GHI";
            expected = string.Format("ABC {0} DEF {1} GHI", User.Current.Id, User.Current.Path);
            Assert.AreEqual(expected, LucQueryTemplateReplacer.ReplaceTemplates(text)); 
        }

        //================================================== Helper methods

        private static void CreatePortalContext()
        {
            CreateTestSite();

            const string pagePath = "/fakesiteforms/Root/System/alma.jpg/";

            var simulatedOutput = new System.IO.StringWriter();
            var simulatedWorkerRequest = new SimulatedHttpRequest(@"\", @"C:\Inetpub\wwwroot", pagePath, "", simulatedOutput, "localhost");
            var simulatedHttpContext = new HttpContext(simulatedWorkerRequest);
            var portalContext = PortalContext.Create(simulatedHttpContext);
        }

        private static void CreateTestSite()
        {
            var node = Node.LoadNode(TestSitePath);
            if (node != null)
                return;

            var site = new Site(Repository.Root) { Name = TestSiteName };
            var urlList = new Dictionary<string, string>(3)
                              {
                                  {"localhost/fakesiteforms", "Forms"},
                                  {"localhost/fakesitewindows", "Windows"},
                                  {"localhost/fakesitenone", "None"}
                              };
            site.UrlList = urlList;
            site.Save();
        }

        private static void CleanupTestSite()
        {
            var node = Node.LoadNode(TestSitePath);
            if (node != null)
                node.ForceDelete();
        }
    }
}
