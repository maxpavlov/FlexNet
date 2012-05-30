using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search;
using System.Drawing;
using SenseNet.ContentRepository.Schema;
using SenseNet.Search.Indexing;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using System.IO;
using Lucene.Net.Analysis.Tokenattributes;
using System.Diagnostics;
using Lucene.Net.Search;
using System.Globalization;

namespace SenseNet.ContentRepository.Tests.Search
{
    [TestClass]
    public class IndexingTests : TestBase
    {
        #region Test infrastructure
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
        #endregion
        #region TestRoot - ClassInitialize - ClassCleanup
        private static string _testRootName = "_IndexingTests";
        private static string __testRootPath = String.Concat("/Root/", _testRootName);
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

        //[ClassInitialize]
        //public static void InitializeEngine(TestContext testContext)
        //{
        //    StorageContext.Search.IsOuterEngineEnabled = true;
        //}
        [ClassCleanup]
        public static void DestroySandBox()
        {
            try
            {
                Node.ForceDelete(__testRootPath);
            }
            catch (Exception e)
            {
                int q = 1;
            }
        }

        #endregion

        private class ColorWithNameIndexHandler : FieldIndexHandler, IIndexValueConverter<Color>, IIndexValueConverter
        {
            public override IEnumerable<IndexFieldInfo> GetIndexFieldInfos(Field snField, out string textExtract)
            {
                textExtract = String.Empty;
                var value = snField.GetData();
                if (value == null)
                    return null;
                var color = (Color)value;
                var colorString = SenseNet.ContentRepository.Fields.ColorField.ColorToString(color).ToLower();

                return CreateFieldInfo(snField.Name, colorString);
            }
            public override bool TryParseAndSet(SenseNet.Search.Parser.QueryFieldValue value)
            {
                var v = value.StringValue.ToLower();
                if (v.StartsWith("#"))
                    return true;
                switch (v)
                {
                    case "red": value.Set("#ff0000"); return true;
                    case "green": value.Set("#00ff00"); return true;
                    case "blue": value.Set("#0000ff"); return true;
                    default: return false;
                }
            }
            public Color GetBack(string lucFieldValue)
            {
                return SenseNet.ContentRepository.Fields.ColorField.ColorFromString(lucFieldValue);
            }
            object IIndexValueConverter.GetBack(string lucFieldValue)
            {
                return GetBack(lucFieldValue);
            }
            public override IEnumerable<string> GetParsableValues(Field snField)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void Indexing_RecoverStoredField()
        {
            var ctdTemplate = @"<?xml version='1.0' encoding='utf-8'?>
                <ContentType name='{0}' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                  <Fields>
                    <Field name='BodyColorIndexingTests' type='Color'>
                      <Indexing>
                        {2}
                        <IndexHandler>{1}</IndexHandler>
                      </Indexing>
                    </Field>
                  </Fields>
                </ContentType>";
            var ctdCleanTemplate = @"<?xml version='1.0' encoding='utf-8'?>
                <ContentType name='{0}' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                </ContentType>";
            var typeName = "TypeForIndexingTest";
            var fieldHandlerName = typeof(ColorWithNameIndexHandler).FullName;


            Content content = null;
            ContentTypeInstaller.InstallContentType(String.Format(ctdTemplate, typeName, fieldHandlerName, "<Store>No</Store>"));
            var contentType = ContentType.GetByName(typeName); //-- typesystem load back

            var query = LucQuery.Parse("BodyColorIndexingTests:Red");
            try
            {
                //-- without storing: there is query result but there is not value to recover
                content = Content.CreateNew(typeName, TestRoot, "car");
                content["BodyColorIndexingTests"] = Color.Red;
                content.Save();

                var doc = query.Execute().FirstOrDefault();
                Assert.IsNotNull(doc, "Query result document is null");
                Assert.IsTrue(!doc.Names.Contains("BodyColorIndexingTests"), "Document has BodyColorIndexingTests field.");

                //-- clean
                content.ForceDelete();
                ContentTypeInstaller.InstallContentType(String.Format(ctdCleanTemplate, typeName));
                contentType = ContentType.GetByName(typeName); //-- typesystem load back

                //-- with storing: there are query result and value to recover
                ContentTypeInstaller.InstallContentType(String.Format(ctdTemplate, typeName, fieldHandlerName, "<Store>Yes</Store>"));
                content = Content.CreateNew(typeName, TestRoot, "car");
                content["BodyColorIndexingTests"] = Color.Red;
                content.Save();

                doc = query.Execute().FirstOrDefault();
                Assert.IsNotNull(doc, "Query result document is null");
                var fieldValueColor = doc.Get<Color>("BodyColorIndexingTests");
                var fieldValueObject = doc.Get("BodyColorIndexingTests");
                Assert.IsTrue(SenseNet.ContentRepository.Fields.ColorField.ColorToString(fieldValueColor) == SenseNet.ContentRepository.Fields.ColorField.ColorToString(Color.Red), "Colors are not equal.");
            }
            finally
            {
                if (content != null)
                    content.ForceDelete();
                ContentTypeInstaller.RemoveContentType(typeName);
            }

        }



        [TestMethod]
        public void Querying_Analyzers()
        {
            var query = LucQuery.Parse("'Mr.John Smith'");
            var s = query.ToString();
            var pq = query.Query as Lucene.Net.Search.PhraseQuery;
            Assert.IsNotNull(pq, String.Concat("Parsed query is: ", pq.GetType().Name, ". Expected: PhraseQuery"));
            var terms = pq.GetTerms();
            Assert.IsTrue(terms.Length == 2, String.Concat("Count of terms is: ", terms.Length, ". Expected: 2"));
            Assert.IsTrue(terms[0].Text() == "mr.john", String.Concat("First term is ", terms[0].Text(), ". Expected: 'mr.john'"));
            Assert.IsTrue(terms[1].Text() == "smith", String.Concat("Second term is ", terms[1].Text(), ". Expected: 'smith'"));

            var qtext = "\"Mr.John Smith\"";
            //var qtext = "(InTree:/Root/Site1/Folder1/Folder2/Folder3 OR InTree:/Root/Site2/Folder1/Folder2/Folder3/Folder5/Folder6) AND Type:Folder AND _Text:\"Mr.John Smith\"";

            Lucene.Net.Search.Query q; 
            var k = 0;
            var stopper = Stopwatch.StartNew();
            for (int i = 0; i < 10000000; i++)
                k++;
            var t0 = stopper.ElapsedMilliseconds;
            stopper.Stop();

            stopper = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
                q = LucQuery.Parse(qtext).Query;
            var t1 = stopper.ElapsedMilliseconds;
            stopper.Stop();

            stopper = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
                q = new Lucene.Net.QueryParsers.QueryParser(Lucene.Net.Util.Version.LUCENE_29, "_Text", IndexManager.GetAnalyzer()).Parse(qtext);
            var t2 = stopper.ElapsedMilliseconds;
            stopper.Stop();
        }

        [TestMethod]
        public void Querying_SortingWithCultureInfo()
        {
            var words = new[] { "aa", "éé", "zz", "bb", "áá", "cs", "cz", "cc" };
            foreach (var word in words)
            {
                var content = Content.CreateNew("Car", TestRoot, "CarSorting");
                content["Make"] = word;
                content.Save();
            }
            var locale = CultureInfo.GetCultureInfo("hu-HU");
            var result = new List<string>();

            var savedlocale = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = locale;
                var cqresult = ContentQuery.Query("TypeIs:Car AND Name:CarSorting* .SORT:Make");
                foreach (var node in cqresult.Nodes)
                    result.Add((string)node["Make"]);
            }
            catch (Exception e)
            {
                int q = 1;
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = savedlocale;
            }
            var sum = String.Join(", ", result.ToArray());
            Assert.IsTrue("aa, áá, bb, cc, cz, cs, éé, zz" == sum, String.Concat("sum is '", sum, "'. Expected: 'aa, áá, bb, cc, cz, cs, éé, zz'"));
        }

        [TestMethod]
        public void Querying_ParserAndKeywordAnalyzer()
        {
            var queries = new Tuple<string, int>[] { 
                new Tuple<string, int>("Keywords:helo1", 1),    // 0
                new Tuple<string, int>("Keywords:helo", 0),     // 1
                new Tuple<string, int>("Keywords:helo*", 1),    // 2
                new Tuple<string, int>("Keywords:helo2", 0),    // 3
                new Tuple<string, int>("Keywords:hElo2", 1),    // 4
                new Tuple<string, int>("Keywords:bye/Bye", 1),  // 5
                new Tuple<string, int>("Keywords:bye/*", 1),    // 6
                new Tuple<string, int>("Keywords:naES", 0),     // 7
                new Tuple<string, int>("Keywords:naes", 1),     // 8
            };

            var content = Content.CreateNew("Contract", TestRoot, null);
            content["Keywords"] = "helo1 hElo2 bye/Bye naes";
            content.Save();

            foreach (var query in queries)
            {
                var count = ContentQuery.Query(query.Item1 + " .COUNTONLY").Count;
                var message = String.Format("Result count of '{0}' is {1}. Extpected: {2}", query.Item1, count, query.Item2);
                Assert.IsTrue(count == query.Item2, message);
            }
        }
    }
}
