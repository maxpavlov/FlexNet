using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Versioning;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Search;
using SenseNet.Search.Parser;

namespace SenseNet.ContentRepository.Tests.Search
{
    [TestClass]
    public class SearchTest2 : TestBase
    {
        #region Test infrastructure
        public SearchTest2()
        {
            //
            // TODO: Add constructor logic here
            //
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
        #region Sandbox
        private static string _testRootName = "_RepositoryTest_SearchTest2";
        private static string __testRootPath = String.Concat("/Root/", _testRootName);
        private static List<string> _installedContentTypes = new List<string>();

        private Folder __testRoot;
        private Folder TestRoot
        {
            get
            {
                if (__testRoot == null)
                {
                    __testRoot = (Folder)Node.LoadNode(__testRootPath);
                    if (__testRoot == null)
                    {
                        Folder folder = new Folder(Repository.Root);
                        folder.Name = _testRootName;
                        folder.Save();
                        __testRoot = (Folder)Node.LoadNode(__testRootPath);
                    }
                }
                return __testRoot;
            }
        }

        [ClassCleanup]
        public static void RemoveContentTypes()
        {
            ContentType ct;
            if (Node.Exists(__testRootPath))
                Node.ForceDelete(__testRootPath);
            foreach (var ctName in _installedContentTypes)
            {
                ct = ContentType.GetByName(ctName);
                if (ct != null)
                    ct.Delete();
            }
            ct = ContentType.GetByName("Automobile1");
            if (ct != null)
                ct.Delete();
        }
        #endregion

        [TestMethod]
        public void NodeQuery_Autofilter()
        {
            var queryText = @"<SearchExpression xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/SearchExpression"">
                  <Or>
                    <String op=""Contains"" property=""Path"">/<currentuser property=""Name""/>/</String>
                  </Or>
                </SearchExpression>";
            var filterText = @"<SearchExpression xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/SearchExpression"">
                  <And>
                    <Not>
                        <String op=""StartWith"" property=""Path"">/Root/Trash</String>
                    </Not>
                  </And>
                </SearchExpression>";
            var expected = @"<SearchExpression xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/SearchExpression"">
                  <And>
                    <Not>
                        <String op=""StartWith"" property=""Path"">/Root/Trash</String>
                    </Not>
                    <Or>
                      <String op=""Contains"" property=""Path"">/<currentuser property=""Name""/>/</String>
                    </Or>
                  </And>
                </SearchExpression>";

            var extended = ContentQuery.AddFilterToNodeQuery(queryText, filterText);

            Assert.IsTrue(
                expected.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "")
                ==
                extended.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "")
                );
        }

        [TestMethod]
        public void NodeQuery_PathWithWhitespace()
        {
            var nquery = new NodeQuery();
            nquery.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, "/Root/a b"));
            var lquery = LucQuery.Create(nquery);
            var s = lquery.ToString();
            Assert.IsTrue(s == "InTree:\"/root/a b\" .AUTOFILTERS:OFF");
        }

        [TestMethod]
        public void ContentQuery_RecursiveQuery()
        {
            var q = new ContentQuery { Text = "Members:{{Id:1}} .AUTOFILTERS:OFF", Settings = new QuerySettings { EnableAutofilters = false } };
            var r = q.ExecuteToIds(ExecutionHint.ForceIndexedEngine);
            Assert.IsTrue(r.Count() > 0, "#01");
            var id = r.First();
            Assert.IsTrue(id == Group.Administrators.Id, "#02");

            q = new ContentQuery { Text = "Members:{{Name:admin*}} .AUTOFILTERS:OFF", Settings = new QuerySettings { EnableAutofilters = false } };
            r = q.ExecuteToIds(ExecutionHint.ForceIndexedEngine);
            Assert.IsTrue(r.Count() > 0, "#11");
            id = r.First();
            Assert.IsTrue(id == Group.Administrators.Id, "#12");
        }
        [TestMethod]
        public void ContentQuery_RecursiveQuery_Empty()
        {
            ContentQuery q;
            IEnumerable<int> r;
            int id;

            q = new ContentQuery { Text = "+Members:{{Name:NOBODY42}} +Name:Administrators .AUTOFILTERS:OFF" };
            r = q.ExecuteToIds(ExecutionHint.ForceIndexedEngine);
            Assert.IsTrue(r.Count() == 0, "#05");

            q = new ContentQuery { Text = "Members:{{Name:NOBODY42}} Name:Administrators .AUTOFILTERS:OFF" };
            r = q.ExecuteToIds(ExecutionHint.ForceIndexedEngine);
            Assert.IsTrue(r.Count() > 0, "#07");
            id = r.First();
            Assert.IsTrue(id == Group.Administrators.Id, "#08");
        }

        [TestMethod]
        public void ContentQuery_LucQueryAddSimpleAndClause()
        {
            var inputText = ".SKIP:10 Name:My* .TOP:5 Meta:'.TOP:6' Type:Folder";
            var extensionText = "InTree:/Root/JohnSmith";
            var inputQuery = LucQuery.Parse(inputText);
            var extensionQuery = LucQuery.Parse(extensionText);
            inputQuery.AddAndClause(extensionQuery);
            var combinedAndText = inputQuery.ToString();

            var expectedAndText = "+(Name:my* Meta:.TOP:6 Type:folder) +InTree:/root/johnsmith .TOP:5 .SKIP:10";

            Assert.AreEqual(expectedAndText, combinedAndText);
        }
        [TestMethod]
        public void ContentQuery_LucQueryAddDoubleAndClause()
        {
            var inputText = ".SKIP:10 Name:My* .TOP:5 Meta:'.TOP:6' Type:Folder";
            var extensionText = ".AUTOFILTERS:OFF InTree:/Root/JohnSmith .TOP:100 InTree:/Root/System";
            var inputQuery = LucQuery.Parse(inputText);
            var extensionQuery = LucQuery.Parse(extensionText);
            inputQuery.AddAndClause(extensionQuery);
            var combinedAndText = inputQuery.ToString();

            var expectedAndText = "+(Name:my* Meta:.TOP:6 Type:folder) +(InTree:/root/johnsmith InTree:/root/system) .TOP:5 .SKIP:10";

            Assert.AreEqual(expectedAndText, combinedAndText);
        }
        [TestMethod]
        public void ContentQuery_LucQueryAddSimpleOrClause()
        {
            var inputText = ".SKIP:10 Name:My* .TOP:5 Meta:'.TOP:6' Type:Folder";
            var extensionText = "InTree:/Root/JohnSmith";
            var inputQuery = LucQuery.Parse(inputText);
            var extensionQuery = LucQuery.Parse(extensionText);
            inputQuery.AddOrClause(extensionQuery);
            var combinedAndText = inputQuery.ToString();

            var expectedAndText = "(Name:my* Meta:.TOP:6 Type:folder) InTree:/root/johnsmith .TOP:5 .SKIP:10";

            Assert.AreEqual(expectedAndText, combinedAndText);
        }
        [TestMethod]
        public void ContentQuery_LucQueryAddDoubleOrClause()
        {
            var inputText = ".SKIP:10 Name:My* .TOP:5 Meta:'.TOP:6' Type:Folder";
            var extensionText = ".AUTOFILTERS:OFF InTree:/Root/JohnSmith .TOP:100 InTree:/Root/System";
            var inputQuery = LucQuery.Parse(inputText);
            var extensionQuery = LucQuery.Parse(extensionText);
            inputQuery.AddOrClause(extensionQuery);
            var combinedAndText = inputQuery.ToString();

            var expectedAndText = "(Name:my* Meta:.TOP:6 Type:folder) (InTree:/root/johnsmith InTree:/root/system) .TOP:5 .SKIP:10";

            Assert.AreEqual(expectedAndText, combinedAndText);
        }

        [TestMethod]
        public void ContentQuery_CountOnly()
        {
            var expectedCount = ContentType.GetContentTypeNames().Length;
            var query = LucQuery.Parse("Type:ContentType .COUNTONLY .AUTOFILTERS:OFF .LIFESPAN:OFF");
            var result = query.Execute().ToArray();
            var totalCount = query.TotalCount;

            Assert.IsTrue(result.Length == 0, String.Format("Result length is: {0}, expected: 0.", result.Length));
            Assert.IsTrue(expectedCount == totalCount, String.Format("TotalCount is: {0}, expected: {1}.", totalCount, expectedCount));
        }
    }
}
