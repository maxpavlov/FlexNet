using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using System.Net;
using System.IO;
using System.Web.Hosting;
using SenseNet.Portal.Virtualization;
using SenseNet.Portal.AppModel;
using SenseNet.Portal;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.AppModel;
using System.Diagnostics;
using SenseNet.ContentRepository.Schema;


namespace SenseNet.ContentRepository.Tests.AppModel
{
    internal class TestRequest : SimpleWorkerRequest
    {
        public TestRequest(string page, string query) : base(page, query, new StringWriter()) 
        {

        }
    }

    [TestClass]
    public class AppModelTest : TestBase
    {
        #region Infrastructure
        public AppModelTest()
        {
        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        #endregion

        [ClassInitialize]
        public static void CreateSandbox(TestContext testContext)
        {
            var site = Node.Load<Site>("/Root/TestSiteForAppModelTest");
            if (site == null)
            {
                site = new Site(Repository.Root);
                site.Name = "TestSiteForAppModelTest";
                var urlList = new Dictionary<string, string>();
                urlList.Add("testhost", "Windows");
                site.UrlList = urlList;
                site.Save();
            }
            var homePage = EnsureSiteStartPage(site);
            var webContent = Node.Load<GenericContent>("/Root/TestSiteForAppModelTest/Home/WebContent1");
            if (webContent == null)
            {
                webContent = new GenericContent(homePage, "WebContent");
                webContent.Name = "WebContent1";
                webContent.Save();
            }
            var file = Node.Load<File>("/Root/TestSiteForAppModelTest/Home/File1");
            if (file == null)
            {
                file = new File(homePage);
                file.Name = "File1";
                file.GetBinary("Binary").SetStream(Tools.GetStreamFromString("File1 content"));
                file.Save();
            }

            //---- Appmodel

            var siteAppsFolder = Node.Load<SystemFolder>("/Root/TestSiteForAppModelTest/(apps)");
            if (siteAppsFolder == null)
            {
                siteAppsFolder = new SystemFolder(site);
                siteAppsFolder.Name = "(apps)";
                siteAppsFolder.Save();
            }
            var siteAppsPageFolder = Node.Load<Folder>("/Root/TestSiteForAppModelTest/(apps)/Page");
            if (siteAppsPageFolder == null)
            {
                siteAppsPageFolder = new SystemFolder(siteAppsFolder);
                siteAppsPageFolder.Name = "Page";
                siteAppsPageFolder.Save();
            }
            var siteAppsPageBrowsePage = Node.Load<Page>("/Root/TestSiteForAppModelTest/(apps)/Page/Browse");
            if (siteAppsPageBrowsePage == null)
            {
                siteAppsPageBrowsePage = new Page(siteAppsPageFolder);
                siteAppsPageBrowsePage.Name = "Browse";
                siteAppsPageBrowsePage.GetBinary("Binary").SetStream(Tools.GetStreamFromString("<html><body><h1>Page Browse App</h1></body></html>"));
                siteAppsPageBrowsePage.Save();
            }
            var siteAppsPageEditPage = Node.Load<Page>("/Root/TestSiteForAppModelTest/(apps)/Page/Edit");
            if (siteAppsPageEditPage == null)
            {
                siteAppsPageEditPage = new Page(siteAppsPageFolder);
                siteAppsPageEditPage.Name = "Edit";
                siteAppsPageEditPage.GetBinary("Binary").SetStream(Tools.GetStreamFromString("<html><body><h1>Page EditPage</h1></body></html>"));
                siteAppsPageEditPage.Save();
            }

            var siteAppsGenericContentFolder = Node.Load<Folder>("/Root/TestSiteForAppModelTest/(apps)/GenericContent");
            if (siteAppsGenericContentFolder == null)
            {
                siteAppsGenericContentFolder = new SystemFolder(siteAppsFolder);
                siteAppsGenericContentFolder.Name = "GenericContent";
                siteAppsGenericContentFolder.Save();
            }
            var siteAppsGenericContentBrowsePage = Node.Load<Page>("/Root/TestSiteForAppModelTest/(apps)/GenericContent/Browse");
            if (siteAppsGenericContentBrowsePage == null)
            {
                siteAppsGenericContentBrowsePage = new Page(siteAppsGenericContentFolder);
                siteAppsGenericContentBrowsePage.Name = "Browse";
                siteAppsGenericContentBrowsePage.GetBinary("Binary").SetStream(Tools.GetStreamFromString("<html><body><h1>GenericContent Browse App</h1></body></html>"));
                siteAppsGenericContentBrowsePage.Save();
            }

            var siteAppsGenericContentEditPage = Node.Load<Page>("/Root/TestSiteForAppModelTest/(apps)/GenericContent/Edit");
            if (siteAppsGenericContentEditPage == null)
            {
                siteAppsGenericContentEditPage = new Page(siteAppsGenericContentFolder);
                siteAppsGenericContentEditPage.Name = "Edit";
                siteAppsGenericContentEditPage.GetBinary("Binary").SetStream(Tools.GetStreamFromString("<html><body><h1>GenericContent EditPage</h1></body></html>"));
                siteAppsGenericContentEditPage.Save();
            }


            //---- SelfDispatcher node
            var selfDispatcherContent = Node.Load<GenericContent>("/Root/TestSiteForAppModelTest/Home/SelfDispatcherContent1");
            if (selfDispatcherContent == null)
            {
                selfDispatcherContent = new GenericContent(homePage, "WebContent");
                selfDispatcherContent.Name = "SelfDispatcherContent1";
                selfDispatcherContent.BrowseApplication = Node.LoadNode("/Root/TestSiteForAppModelTest/(apps)/GenericContent/Edit");
                selfDispatcherContent.Save();
            }
        }
        private static Page EnsureSiteStartPage(Site site)
        {
            var startPageName = "Home";
            var homePage = Node.Load<Page>(RepositoryPath.Combine(site.Path, startPageName));
            if (homePage == null)
            {
                homePage = new Page(site);
                homePage.Name = startPageName;
                homePage.GetBinary("Binary").SetStream(Tools.GetStreamFromString("<html><body><h1>TestPage</h1></body></html>"));
                homePage.Save();
                site.StartPage = homePage;
                site.Save();
            }
            else if(site.StartPage == null)
            {
                site.StartPage = homePage;
                site.Save();
            }

            return homePage;
        }
        private void RemoveSiteStartPage(Site site)
        {
            if (site.StartPage == null)
                return;
            site.StartPage = null;
            site.Save();
        }

        [ClassCleanup]
        public static void DestroySandbox()
        {
            var site = Node.Load<Site>("/Root/TestSiteForAppModelTest");
            if (site != null)
                site.ForceDelete();
        }


        [TestMethod]
        public void AppModel_ActionManager_SiteBrowse_WithStartPage()
        {
            EnsureSiteStartPage(Node.Load<Site>("/Root/TestSiteForAppModelTest"));

            var portalContext = CreatePortalContext("/", "");

            var action = HttpActionManager.CreateAction(portalContext);

            //Trace.WriteLine("Action: " + typeof(RewriteHttpAction).FullName);
            //Trace.WriteLine("AppNode: " + (action.AppNode == null ? "[null]" : action.AppNode.Path));
            //Trace.WriteLine("TargetNode: " + (action.TargetNode == null ? "[null]" : action.TargetNode.Path));
            //Trace.WriteLine("StartPage: " + (portalContext.Site.StartPage == null ? "[null]" : portalContext.Site.StartPage.Path));

            Assert.IsInstanceOfType(action, typeof(RedirectHttpAction), "Type of the action is: " + action.GetType().FullName + " expected: RedirectHttpAction");
            Assert.IsNull(action.AppNode, "action.AppNode is not null");
            Assert.IsNotNull(action.TargetNode, "action.TargetNode is null");
            //Assert.IsTrue(action.AppNode.Path == "/Root/TestSiteForAppModelTest/(apps)/GenericContent/Browse", "action.AppNode.Path is not \"/Root/TestSiteForAppModelTest/(apps)/GenericContent/Browse\"");
            Assert.IsTrue(action.TargetNode.Path == "/Root/TestSiteForAppModelTest", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest\"");
        }
        [TestMethod]
        public void AppModel_ActionManager_SiteBrowse_WithoutStartPage()
        {
            RemoveSiteStartPage(Node.Load<Site>("/Root/TestSiteForAppModelTest"));

            var portalContext = CreatePortalContext("/", "");

            var action = HttpActionManager.CreateAction(portalContext);

            //Trace.WriteLine("Action: " + typeof(RewriteHttpAction).FullName);
            //Trace.WriteLine("AppNode: " + (action.AppNode == null ? "[null]" : action.AppNode.Path));
            //Trace.WriteLine("TargetNode: " + (action.TargetNode == null ? "[null]" : action.TargetNode.Path));
            //Trace.WriteLine("StartPage: " + (portalContext.Site.StartPage == null ? "[null]" : portalContext.Site.StartPage.Path));

            Assert.IsInstanceOfType(action, typeof(RewriteHttpAction));
            Assert.IsNotNull(action.AppNode, "action.AppNode is null");
            Assert.IsNotNull(action.TargetNode, "action.TargetNode is null");
            Assert.IsTrue(action.AppNode.Path == "/Root/TestSiteForAppModelTest/(apps)/GenericContent/Browse", "action.AppNode.Path is not \"/Root/TestSiteForAppModelTest/(apps)/GenericContent/Browse\"");
            Assert.IsTrue(action.TargetNode.Path == "/Root/TestSiteForAppModelTest", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest\"");
        }
        [TestMethod]
        public void AppModel_ActionManager_PageBrowse()
        {
            var portalContext = CreatePortalContext("/home", "");

            var action = HttpActionManager.CreateAction(portalContext);

            Assert.IsInstanceOfType(action, typeof(RewriteHttpAction));
            Assert.IsNotNull(action.AppNode, "action.AppNode is null");
            Assert.IsNotNull(action.TargetNode, "action.TargetNode is null");
            Assert.IsTrue(action.AppNode.Path == "/Root/TestSiteForAppModelTest/Home", "action.AppNode.Path is not \"/Root/TestSiteForAppModelTest/Home\"");
            Assert.IsTrue(action.TargetNode.Path == "/Root/TestSiteForAppModelTest/Home", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest/Home\"");
        }
        [TestMethod]
        public void AppModel_ActionManager_PageEdit()
        {
            var portalContext = CreatePortalContext("/home", "action=edit");

            var action = HttpActionManager.CreateAction(portalContext);

            Assert.IsInstanceOfType(action, typeof(RewriteHttpAction));
            Assert.IsNotNull(action.AppNode, "action.AppNode is null");
            Assert.IsNotNull(action.TargetNode, "action.TargetNode is null");
            Assert.IsTrue(action.AppNode.Path == "/Root/TestSiteForAppModelTest/(apps)/Page/Edit", "action.AppNode.Path is not \"/Root/TestSiteForAppModelTest/(apps)/Page/Edit\"");
            Assert.IsTrue(action.TargetNode.Path == "/Root/TestSiteForAppModelTest/Home", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest/Home\"");
        }
        [TestMethod]
        public void AppModel_ActionManager_GenericContentBrowse()
        {
            var portalContext = CreatePortalContext("/home/webcontent1", "");

            var action = HttpActionManager.CreateAction(portalContext);

            Assert.IsInstanceOfType(action, typeof(RewriteHttpAction));
            Assert.IsNotNull(action.AppNode, "action.AppNode is null");
            Assert.IsNotNull(action.TargetNode, "action.TargetNode is null");
            Assert.IsTrue(action.AppNode.Path == "/Root/TestSiteForAppModelTest/(apps)/GenericContent/Browse", "action.AppNode.Path is not \"/Root/TestSiteForAppModelTest/(apps)/GenericContent/Browse\"");
            Assert.IsTrue(action.TargetNode.Path == "/Root/TestSiteForAppModelTest/Home/WebContent1", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest/Home/WebContent1\"");
        }
        [TestMethod]
        public void AppModel_ActionManager_GenericContentEdit()
        {
            var portalContext = CreatePortalContext("/home/webcontent1", "action=edit");

            var action = HttpActionManager.CreateAction(portalContext);

            Assert.IsInstanceOfType(action, typeof(RewriteHttpAction));
            Assert.IsNotNull(action.AppNode, "action.AppNode is null");
            Assert.IsNotNull(action.TargetNode, "action.TargetNode is null");
            Assert.IsTrue(action.AppNode.Path == "/Root/TestSiteForAppModelTest/(apps)/GenericContent/Edit", "action.AppNode.Path is not \"/Root/TestSiteForAppModelTest/(apps)/GenericContent/Edit\"");
            Assert.IsTrue(action.TargetNode.Path == "/Root/TestSiteForAppModelTest/Home/WebContent1", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest/Home/WebContent1\"");
        }
        [TestMethod]
        public void AppModel_ActionManager_FileBrowse()
        {
            var portalContext = CreatePortalContext("/home/file1", "");

            var action = HttpActionManager.CreateAction(portalContext);
            var downloadAction = action as DownloadHttpAction;

            Assert.IsNotNull(downloadAction, "action is not DownloadAction");
            Assert.IsTrue(downloadAction.BinaryPropertyName == "Binary", "downloadAction.BinaryPropertyName is not \"Binary\"");
            Assert.IsNotNull(downloadAction.TargetNode, "action.TargetNode is null");
            Assert.IsTrue(downloadAction.TargetNode.Path == "/Root/TestSiteForAppModelTest/Home/File1", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest/Home/File1\"");
        }
        [TestMethod]
        public void AppModel_ActionManager_PageProperty()
        {
            var portalContext = CreatePortalContext("/home", "NodeProperty=PersonalizationSettings");

            var action = HttpActionManager.CreateAction(portalContext);
            var downloadAction = action as DownloadHttpAction;

            Assert.IsNotNull(downloadAction, "action is not DownloadAction");
            Assert.IsTrue(downloadAction.BinaryPropertyName == "PersonalizationSettings", "downloadAction.BinaryPropertyName is not \"PersonalizationSettings\"");
            Assert.IsNotNull(downloadAction.TargetNode, "action.TargetNode is null");
            Assert.IsTrue(downloadAction.TargetNode.Path == "/Root/TestSiteForAppModelTest/Home", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest/Home\"");
        }
        [TestMethod]
        [Description("1st test: action should point to edit application instead of browse (as set in SelfDispatcherContent1.BrowseApplication property); 2nd test: clear browseapplication data -> original flow")]
        public void AppModel_ActionManager_SelfDispatcher()
        {
            // create browse action
            var portalContext = CreatePortalContext("/home/SelfDispatcherContent1", "");

            var action = HttpActionManager.CreateAction(portalContext);

            Assert.IsInstanceOfType(action, typeof(RewriteHttpAction));
            Assert.IsNotNull(action.AppNode, "action.AppNode is null");
            Assert.IsNotNull(action.TargetNode, "action.TargetNode is null");
            Assert.IsTrue(action.AppNode.Path == "/Root/TestSiteForAppModelTest/(apps)/GenericContent/Edit", "action.AppNode.Path is not \"/Root/TestSiteForAppModelTest/(apps)/GenericContent/Edit\"");
            Assert.IsTrue(action.TargetNode.Path == "/Root/TestSiteForAppModelTest/Home/SelfDispatcherContent1", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest/Home/SelfDispatcherContent1\"");


            var targetNode = Node.LoadNode(action.TargetNode.Id) as GenericContent;
            targetNode.BrowseApplication = null;
            targetNode.Save();

            action = HttpActionManager.CreateAction(portalContext);

            Assert.IsInstanceOfType(action, typeof(RewriteHttpAction));
            Assert.IsNotNull(action.AppNode, "action.AppNode is null");
            Assert.IsNotNull(action.TargetNode, "action.TargetNode is null");
            Assert.IsTrue(action.AppNode.Path == "/Root/TestSiteForAppModelTest/(apps)/GenericContent/Browse", "action.AppNode.Path is not \"/Root/TestSiteForAppModelTest/(apps)/GenericContent/Browse\"");
            Assert.IsTrue(action.TargetNode.Path == "/Root/TestSiteForAppModelTest/Home/SelfDispatcherContent1", "action.TargetNode.Path is not \"/Root/TestSiteForAppModelTest/Home/SelfDispatcherContent1\"");
        }


        private PortalContext CreatePortalContext(string page, string query)
        {
            var request = CreateRequest(page, query);
            var httpContext = new HttpContext(request);
            var portalContext = PortalContext.Create(httpContext);
            return portalContext;
        }
        private SimulatedHttpRequest CreateRequest(string page, string query)
        {
            var writer = new StringWriter();
            var request = new SimulatedHttpRequest(@"\", @"C:\Inetpub\wwwroot", page, query, writer, "testhost");
            return request;
        }

        //=============================================================================================================

        [TestMethod]
        public void AppModel_PathResolver_Type()
        {
            var expectedList = new string[]
            {
                "/Root/AA/BB/AppFolder/This/AppName",
                "/Root/AA/BB/AppFolder/HTMLContent/AppName",
                "/Root/AA/BB/AppFolder/WebContent/AppName",
                "/Root/AA/BB/AppFolder/ListItem/AppName",
                "/Root/AA/BB/AppFolder/GenericContent/AppName",
            };
            var paths = ApplicationResolver.GetAvailablePaths("/Root/AA/BB", ActiveSchema.NodeTypes["HTMLContent"], "AppFolder", "AppName", HierarchyOption.Type);
            Assert.IsTrue(paths.Count() == expectedList.Count(), "Counts are not equals");
            var i = -1;
            foreach (var path in paths)
                Assert.IsTrue(path == expectedList[++i], "#" + i);
        }
        [TestMethod]
        public void AppModel_PathResolver_Path()
        {
            var expectedList = new string[]
            {
                "/Root/AA/BB/AppFolder/This/AppName",
                "/Root/AA/BB/AppFolder/AppName",
                "/Root/AA/AppFolder/AppName",
                "/Root/AppFolder/AppName",
            };
            var paths = ApplicationResolver.GetAvailablePaths("/Root/AA/BB", ActiveSchema.NodeTypes["WordDocument"], "AppFolder", "AppName", HierarchyOption.Path);
            Assert.IsTrue(paths.Count() == expectedList.Count(), "Counts are not equals");
            var i = -1;
            foreach (var path in paths)
                Assert.IsTrue(path == expectedList[++i], "#" + i);
        }
        [TestMethod]
        public void AppModel_PathResolver_PathAndType()
        {
            var expectedList = new string[]
            {
                "/Root/AA/BB/AppFolder/This/AppName",
                "/Root/AA/BB/AppFolder/HTMLContent/AppName",
                "/Root/AA/AppFolder/HTMLContent/AppName",
                "/Root/AppFolder/HTMLContent/AppName",
                "/Root/AA/BB/AppFolder/WebContent/AppName",
                "/Root/AA/AppFolder/WebContent/AppName",
                "/Root/AppFolder/WebContent/AppName",
                "/Root/AA/BB/AppFolder/ListItem/AppName",
                "/Root/AA/AppFolder/ListItem/AppName",
                "/Root/AppFolder/ListItem/AppName",
                "/Root/AA/BB/AppFolder/GenericContent/AppName",
                "/Root/AA/AppFolder/GenericContent/AppName",
                "/Root/AppFolder/GenericContent/AppName",
            };
            var paths = ApplicationResolver.GetAvailablePaths("/Root/AA/BB", ActiveSchema.NodeTypes["HTMLContent"], "AppFolder", "AppName", HierarchyOption.PathAndType);
            Assert.IsTrue(paths.Count() == expectedList.Count(), "Counts are not equals");
            var i = -1;
            foreach (var path in paths)
                Assert.IsTrue(path == expectedList[++i], "#" + i);
        }
        [TestMethod]
        public void AppModel_PathResolver_TypeAndPath()
        {
            var expectedList = new string[]
            {
                "/Root/AA/BB/AppFolder/This/AppName",
                "/Root/AA/BB/AppFolder/HTMLContent/AppName",
                "/Root/AA/BB/AppFolder/WebContent/AppName",
                "/Root/AA/BB/AppFolder/ListItem/AppName",
                "/Root/AA/BB/AppFolder/GenericContent/AppName",
                "/Root/AA/AppFolder/HTMLContent/AppName",
                "/Root/AA/AppFolder/WebContent/AppName",
                "/Root/AA/AppFolder/ListItem/AppName",
                "/Root/AA/AppFolder/GenericContent/AppName",
                "/Root/AppFolder/HTMLContent/AppName",
                "/Root/AppFolder/WebContent/AppName",
                "/Root/AppFolder/ListItem/AppName",
                "/Root/AppFolder/GenericContent/AppName",
            };
            var paths = ApplicationResolver.GetAvailablePaths("/Root/AA/BB", ActiveSchema.NodeTypes["HTMLContent"], "AppFolder", "AppName", HierarchyOption.TypeAndPath);
            Assert.IsTrue(paths.Count() == expectedList.Count(), "Counts are not equals");
            var i = -1;
            foreach (var path in paths)
                Assert.IsTrue(path == expectedList[++i], "#" + i);
        }

        [TestMethod]
        public void AppModel_ResolveFromPredefinedPaths_First()
        {
            var paths = new string[]
            {
                "/Root/AA/BB/CC",
                "/Root/AA",
                "/Root/System",
                "/Root",
            };
            var nodeHead  = ApplicationResolver.ResolveFirstByPaths(paths);
            Assert.IsNotNull(nodeHead);
            Assert.IsTrue(nodeHead.Path == "/Root/System", "Path does not equal the expected");
        }
        [TestMethod]
        public void AppModel_ResolveFromPredefinedPaths_All()
        {
            var paths = new string[]
            {
                "/Root/AA/BB/CC",
                "/Root/AA/BB",
                "/Root/System/Schema/ContentTypes/GenericContent/ListItem",
                "/Root/AA",
            };
            var expectedList = GetPathsInSubTree("/Root/System/Schema/ContentTypes/GenericContent/ListItem");

            var nodeHeads = ApplicationResolver.ResolveAllByPaths(paths, true);
            var paths1 = nodeHeads.Select(h => h.Path).Except(expectedList).ToArray();
            Assert.IsTrue(paths1.Count() == 0, "Expected empty array but: " + String.Join(", ", paths1));

            //---------------------------------

            expectedList = new string[]
            {
                "/Root/System/Schema/ContentTypes/GenericContent/ListItem",
            };
            nodeHeads = ApplicationResolver.ResolveAllByPaths(paths, false);
            Assert.IsTrue(nodeHeads.Select(h => h.Path).Except(expectedList).Count() == 0, "##2");

        }
        private string[] GetPathsInSubTree(string path)
        {
            var paths = new List<string>();
            paths.Add(path);
            var index = 0;
            while (index < paths.Count)
            {
                var folder = Node.LoadNode(paths[index++]) as IFolder;
                if(folder != null)
                    paths.AddRange(folder.Children.Select(c => c.Path));
            }
            paths.RemoveAt(0);
            return paths.ToArray();
        }
    }
}
