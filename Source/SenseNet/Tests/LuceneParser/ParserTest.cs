using System;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Search.Parser;
using SenseNet.ContentRepository.Storage;
using System.Collections.Generic;
using System.Globalization;
using SenseNet.Search;

namespace SenseNet.ContentRepository.Tests.LuceneParser
{
    [TestClass]
    public class ParserTest : TestBase
    {
        //private class LuceneSearhEngineAccessor : Accessor
        //{
        //    public LuceneSearhEngineAccessor(LuceneSearchEngine target) : base(target) { }
        //    public void SetParsers(IDictionary<string, IQueryFieldValueParser> parsers)
        //    {
        //        this.SetPrivateField("_parsers", parsers);
        //    }
        //}
        //private class TestIntParser : QueryFieldValueToIntParser
        //{
        //    protected override bool TryParse(string value, out int parsed)
        //    {
        //        return Int32.TryParse(value, out parsed);
        //    }
        //}
        //private class TestFloatParser : QueryFieldValueToSingleParser
        //{
        //    protected override bool TryParse(string value, out float parsed)
        //    {
        //        return Single.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
        //    }
        //}
        //private class TestDateTimeParser : QueryFieldValueToLongParser
        //{
        //    protected override bool TryParse(string value, out long parsed)
        //    {
        //        DateTime dateTimeValue;
        //        if (!DateTime.TryParse(value, out dateTimeValue))
        //        {
        //            parsed = 0;
        //            return false;
        //        }
        //        parsed = dateTimeValue.Ticks;
        //        return true;
        //    }
        //}

        #region test infrastructure

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
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        #endregion

        Lucene.Net.Util.Version LUCENEVERSION = Lucene.Net.Util.Version.LUCENE_CURRENT;

        [TestMethod]
        public void Parser_x1()
        {
            string dump;
            #region orig
            //dump = GetNewQueryDump("text");                    // TermQ(Term(_Text:text))
            //dump = GetLucQueryDump("F:text");                  // TermQuery(Term(F:text))
            //dump = GetLucQueryDump("+F:text");                 // BooleanQuery(Clause(Occur(+), TermQuery(Term(FieldName:FieldValue))))
            //dump = GetLucQueryDump("word1 word2");             // BooleanQuery(Clause(Occur(), TermQuery(Term(_Text:word1))), Clause(Occur(), TermQuery(Term(_Text:word2))))

            //dump = GetLucQueryDump("te?t");                    // WildcardQuery(Term(_Text:te?t))
            //dump = GetLucQueryDump("test*");                   // PrefixQuery(Term(_Text:test))
            //dump = GetLucQueryDump("te*t");                    // WildcardQuery(Term(_Text:te*t))
            //dump = GetLucQueryDump("t*e*t");                   // WildcardQuery(Term(_Text:t*e*t))
            //dump = GetLucQueryDump("t??e*t*q");                // WildcardQuery(Term(_Text:t??e*t*q))

            //dump = GetLucQueryDump("roam~");                   // FuzzyQuery(Term(_Text:roam), minSimilarity:0,5)
            //dump = GetLucQueryDump("test~0.89");               // FuzzyQuery(Term(_Text:test), minSimilarity:0,89)
            ////exception: dump = GetLucQueryDump("test~1.1");

            //dump = GetNewQueryDump("F1:\"aa bb\"~123");        // PhQ(Term(F1:aa), Term(F1:bb), Slop:123)
            //dump = GetNewQueryDump("F1:\"aa bb cc\"~123");     // PhQ(Term(F1:aa), Term(F1:bb), Term(F1:cc), Slop:123)
            //dump = GetNewQueryDump("\"aa bb cc*\"");           // PhQ(Term(_Text:aa), Term(_Text:bb), Term(_Text:cc), Slop:0)

            //dump = GetLucQueryDump("mod_date:[20020101 TO 20030101]");  // ConstantScoreRangeQuery(mod_date:[20020101 TO 20030101])
            //dump = GetLucQueryDump("title:{Aida TO Carmen}");           // ConstantScoreRangeQuery(title:{aida TO carmen})
            //dump = GetQueryDump(new ConstantScoreRangeQuery("F1", "a", "b", false, true)); // ConstantScoreRangeQuery(F1:{a TO b])
            //dump = GetQueryDump(new ConstantScoreRangeQuery("F1", "a", "b", true, false)); // ConstantScoreRangeQuery(F1:[a TO b})
            ////exception: dump = GetLucQueryDump("title:[Aida TO Carmen}");
            ////exception: dump = GetLucQueryDump("title:{Aida TO Carmen]");

            //dump = GetLucQueryDump("jakarta^4");                              // TermQuery(Term(_Text:jakarta)Term(_Text:jakarta), Boost(4))
            //dump = GetLucQueryDump("jakarta^4 apache");                       // BooleanQuery(Clause(Occur(), TermQuery(Term(_Text:jakarta), Boost(4))), Clause(Occur(), TermQuery(Term(_Text:apache))))"
            //dump = GetLucQueryDump("\"jakarta apache\"^4 \"Apache Lucene\""); // BooleanQuery(Clause(Occur(), PhraseQuery(Term(_Text:jakarta), Term(_Text:apache), Slop:0)), Clause(Occur(), PhraseQuery(Term(_Text:apache), Term(_Text:lucene), Slop:0)))
            //dump = GetLucQueryDump("title:(+return +\"pink panther\")");      // BooleanQuery(Clause(Occur(+), TermQuery(Term(title:return))), Clause(Occur(+), PhraseQuery(Term(title:pink), Term(title:panther), Slop:0)))

            //dump = GetLucQueryDump("a (+b -d) +(e -(+f -g))"); // BooleanQuery(Clause(Occur(), TermQuery(Term(_Text:a))), Clause(Occur(), BooleanQuery(Clause(Occur(+), TermQuery(Term(_Text:b))), Clause(Occur(-), TermQuery(Term(_Text:d))))), Clause(Occur(+), BooleanQuery(Clause(Occur(), TermQuery(Term(_Text:e))), Clause(Occur(-), BooleanQuery(Clause(Occur(+), TermQuery(Term(_Text:f))), Clause(Occur(-), TermQuery(Term(_Text:g))))))))

            //dump = GetLucQueryDump("(a (+b -d))");   // BooleanQuery(Clause(Occur(), TermQuery(Term(_Text:a))), Clause(Occur(), BooleanQuery(Clause(Occur(+), TermQuery(Term(_Text:b))), Clause(Occur(-), TermQuery(Term(_Text:d))))))
            //dump = GetLucQueryDump("f:(a (+b -d))"); // BooleanQuery(Clause(Occur(), TermQuery(Term(f:a))), Clause(Occur(), BooleanQuery(Clause(Occur(+), TermQuery(Term(f:b))), Clause(Occur(-), TermQuery(Term(f:d))))))
            //dump = GetLucQueryDump("F:(a (+G:b -d))"); // BooleanQuery(Clause(Occur(), TermQuery(Term(F:a))), Clause(Occur(), BooleanQuery(Clause(Occur(+), TermQuery(Term(G:b))), Clause(Occur(-), TermQuery(Term(F:d))))))

            ////exception: dump = GetLucQueryDump("F:{ TO a}");
            ////exception: dump = GetLucQueryDump("F:{a TO }");

            //dump = GetNewQueryDump("F:(a (b + c)^0.89)");           // BoolQ(Cl(( ), TermQ(Term(F:a))), Cl(( ), BoolQ(Cl(( ), TermQ(Term(F:b))), Cl((+), TermQ(Term(F:c))))))

            //dump = GetNewQueryDump("a OR b");    // BoolQ(Cl(( ), TermQ(Term(_Text:a))), Cl(( ), TermQ(Term(_Text:b))))
            //dump = GetNewQueryDump("a OR +b");   // BoolQ(Cl(( ), TermQ(Term(_Text:a))), Cl((+), TermQ(Term(_Text:b))))
            //dump = GetLucQueryDump("a OR -b");   // BoolQ(Cl((-), TermQ(Term(_Text:b))))
            //dump = GetLucQueryDump("+a OR b");   // BoolQ(Cl(( ), TermQ(Term(_Text:b))))
            //dump = GetLucQueryDump("+a OR +b");  // BoolQ(Cl((+), TermQ(Term(_Text:b))))
            //dump = GetLucQueryDump("+a OR -b");  // BoolQ(Cl((-), TermQ(Term(_Text:b))))
            //dump = GetLucQueryDump("-a OR b");   // BoolQ(Cl(( ), TermQ(Term(_Text:b))))
            //dump = GetLucQueryDump("-a OR +b");  // BoolQ(Cl((+), TermQ(Term(_Text:b))))
            //dump = GetLucQueryDump("-a OR -b");  // BoolQ(Cl((-), TermQ(Term(_Text:b))))
#endregion
            var queries = new string[]
            {
                //"text",
                //"F:text",
                //"+F:text",
                //"word1 word2",

                //"te?t",
                //"test*",
                //"te*t",
                //"t*e*t",
                //"t??e*t*q",

                //"roam~",
                //"test~0.89",
                ////exception: dump = GetLucQueryDump("test~1.1");

                //"F1:\"aa bb\"~123",
                //"F1:\"aa bb cc\"~123",
                "\"aa bb cc*\"",

                //"mod_date:[20020101 TO 20030101]",
                //"title:{Aida TO Carmen}",
                ////"title:[Aida TO Carmen}",
                ////"title:{Aida TO Carmen]",

                //"jakarta^4",
                //"jakarta^4 apache",
                //"\"jakarta apache\"^4 \"Apache Lucene\"",
                //"title:(+return +\"pink panther\")",

                "a (+b -d) +(e -(+f -g))",

                "(a (+b -d))",
                "f:(a (+b -d))",
                "F:(a (+G:b -d))",

                ////("F:{ TO a}";
                ////("F:{a TO }";

                "F:(a (b + c)^0.89)",
            };
            var sb = new StringBuilder();
            foreach (var q in queries)
                sb.Append(q).Append(" ==> ").AppendLine(GetLucQueryDump(q));

            Assert.Inconclusive();
        }
        [TestMethod]
        public void Parser_x2()
        {
            var queries = new string[]
            {
                "a OR b",
                "a OR +b",
                "a OR -b",
                "+a OR b",
                "+a OR +b",
                "+a OR -b",
                "-a OR b",
                "-a OR +b",
                "-a OR -b",
                "a OR b AND c",
                "a OR +b AND c",
                "a OR -b AND c",
                "+a OR b AND c",
                "+a OR +b AND c",
                "+a OR -b AND c",
                "-a OR b AND c",
                "-a OR +b AND c",
                "-a OR -b AND c",
                "a AND b OR c",
                "a AND +b OR c",
                "a AND -b OR c",
                "+a AND b OR c",
                "+a AND +b OR c",
                "+a AND -b OR c",
                "-a AND b OR c",
                "-a AND +b OR c",
                "-a AND -b OR c",

                "(a AND b) OR c",
                "a AND (b OR c)",

                "(aaa AND bbb)^0.8 OR ccc^0.4"
            };
            //a OR b ==> ( a b)
            //a OR +b ==> ( a+b)
            //a OR -b ==> ( a-b)
            //+a OR b ==> (+a b)
            //+a OR +b ==> (+a+b)
            //+a OR -b ==> (+a-b)
            //-a OR b ==> (-a b)
            //-a OR +b ==> (-a+b)
            //-a OR -b ==> (-a-b)
            //a OR b AND c ==> ( a (+b+c)))
            //a OR +b AND c ==> ( a (+b+c)))
            //a OR -b AND c ==> ( a (-b+c)))
            //+a OR b AND c ==> (+a (+b+c)))
            //+a OR +b AND c ==> (+a (+b+c)))
            //+a OR -b AND c ==> (+a (-b+c)))
            //-a OR b AND c ==> (-a (+b+c)))
            //-a OR +b AND c ==> (-a (+b+c)))
            //-a OR -b AND c ==> (-a (-b+c)))
            //a AND b OR c ==> ( (+a+b)) c)
            //a AND +b OR c ==> ( (+a+b)) c)
            //a AND -b OR c ==> ( (+a-b)) c)
            //+a AND b OR c ==> ( (+a+b)) c)
            //+a AND +b OR c ==> ( (+a+b)) c)
            //+a AND -b OR c ==> ( (+a-b)) c)
            //-a AND b OR c ==> ( (-a+b)) c)
            //-a AND +b OR c ==> ( (-a+b)) c)
            //-a AND -b OR c ==> ( (-a-b)) c)
            //(a AND b) OR c ==> ( (+a+b)) c)
            //a AND (b OR c) ==> (+a+( b c)))
            //(aaa AND bbb)^0.8 OR ccc^0.4 ==> ( (+aaa+bbbBoost(0,8))) ccc)Boost(0,4))))


            var sb = new StringBuilder();
            foreach (var q in queries)
                sb.Append(q).Append(" ==> ").AppendLine(GetNewQueryDump(q));
            var s = sb.ToString()
                .Replace("a)))", "a")
                .Replace("b)))", "b")
                .Replace("c)))", "c")
                .Replace("( )", " ")
                .Replace("(+)", "+")
                .Replace("(-)", "-")
                .Replace(", TermQ(T(_Text:", "")
                .Replace("Cl(", "")
                .Replace("BoolQ(", "(")
                .Replace(", ", "")
                //.Replace(")", "")
                ;

            Assert.Inconclusive();
        }

        [TestMethod]
        public void Parser_StringCase()
        {
            string msg;
            msg = Test("value");
            Assert.IsNull(msg, msg);
            msg = Test("VALUE");
            Assert.IsNull(msg, msg);
            msg = Test("Value");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_DefaultFieldOneTerm()
        {
            var msg = Test("Value1");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_DefaultFieldOneTermNot()
        {
            var msg = Test("-Value1");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_DefaultFieldOneTermMust()
        {
            var msg = Test("+Value1");
            //var msg = DumpTest("TermQ(Term(_Text:value1))", "+Value1");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_DefaultFieldMoreTerms()
        {
            var msg = Test("Value1 -Value2 +Value3 Value4");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_OneField()
        {
            var msg = Test("Field1:Value1");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_OneListField()
        {
            var msg = Test("#Field1:Value1");
            Assert.IsNull(msg, msg);
            var dump1 = GetQueryDump(ParseNewQuery("Field:Value"));
            var dump2 = GetQueryDump(ParseNewQuery("#Field:Value"));
            Assert.IsTrue(dump2.Replace("#Field", "Field") == dump1);
        }
        [TestMethod]
        public void Parser_OneFieldNot()
        {
            var msg = Test("-Field1:Value1");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_OneFieldMust()
        {
            var msg = Test("+Field1:Value1");
            //var msg = DumpTest("TermQ(Term(Field1:value1))", "+Field1:Value1");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_MoreFields()
        {
            var msg = Test("Field1:Value1 Field2:Value2 Field3:Value3");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_MoreFieldsNotShouldMust()
        {
            var msg = Test("F1:V1 -F2:V2 +F3:V3 F4:V4");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_BooleanFirstOccur1()
        {
            var queryText = "f1:v1 f2:v2";
            var dump = GetLucQueryDump(queryText);
            var msg = Test(queryText);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_BooleanFirstOccur2()
        {
            var queryText = "f1:v1 f2:v2 (f3:v3 f4:v4 (f5:v5 f6:v6))";
            var dump = GetLucQueryDump(queryText);
            var msg = Test(queryText);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_BooleanFirstOccur3()
        {
            var queryText = "f1:v1 (f2:v2 (f3:v3 f4:v4))";
            var dump = GetLucQueryDump(queryText);
            var msg = Test(queryText);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_BooleanFirstOccur4()
        {
            var queryText = "aaa AND +bbb";

            var dump = GetLucQueryDump(queryText);
            var msg = Test(queryText);
            Assert.IsNull(msg, msg);
        }

        [TestMethod]
        public void Parser_RangeExtension1()
        {
            var queryText = "F:[aaa TO bbb}";
            var expectedQuery = new TermRangeQuery("F", "aaa", "bbb", true, false);
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_RangeExtension2()
        {
            var queryText = "F:{aaa TO bbb]";
            var expectedQuery = new TermRangeQuery("F", "aaa", "bbb", false, true);
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_RangeExtension3()
        {
            var queryText = "F:[ TO bbb]";
            var expectedQuery = new TermRangeQuery("F", null, "bbb", true, true);
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_RangeExtension4()
        {
            var queryText = "F:[aaa TO ]";
            var expectedQuery = new TermRangeQuery("F", "aaa", null, true, true);
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }

        // new
        [TestMethod]
        public void Parser_FieldExtension_GT()
        {
            var queryText = "F:>aaa"; // "F:{aaa TO ]"
            var expectedQuery = new TermRangeQuery("F", "aaa", null, false, true);
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_FieldExtension_LT()
        {
            var queryText = "F:<bbb"; // "F:[ TO bbb}";
            var expectedQuery = new TermRangeQuery("F", null, "bbb", true, false);
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_FieldExtension_GTE()
        {
            var queryText = "F:>=aaa"; // "F:[aaa TO ]"
            var expectedQuery = new TermRangeQuery("F", "aaa", null, true, true);
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_FieldExtension_LTE()
        {
            var queryText = "F:<=bbb"; // "F:[ TO bbb]";
            var expectedQuery = new TermRangeQuery("F", null, "bbb", true, true);
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_FieldExtension_NEQ()
        {
            var queryText = "Field1:<>Value1";
            var expectedQueryText = "-Field1:Value1";

            var actualQuery = ParseNewQuery(queryText);
            var expectedQuery = ParseLucQuery(expectedQueryText);

            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }

        // test examples from this document: http://lucene.apache.org/java/2_4_0/queryparsersyntax.html
        [TestMethod]
        public void ParserFromDoc_Fields1_STOPWORDS_IS_NOT_IMPLEMENTED()
        {
            var msg = Test("title:\"The Right Way\" AND text:go");
            Assert.Inconclusive();
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Fields2_STOPWORDS_IS_NOT_IMPLEMENTED()
        {
            var msg = Test("title:\"Do it right\" AND right");
            Assert.Inconclusive();
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Fields3_STOPWORDS_IS_NOT_IMPLEMENTED()
        {
            var msg = Test("title:Do it right");
            Assert.Inconclusive();
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Wildcard1()
        {
            var msg = Test("te?t");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Wildcard2()
        {
            var msg = Test("test*");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Wildcard3()
        {
            var msg = Test("te*t");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Fuzzy1()
        {
            var msg = Test("roam~");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Fuzzy2()
        {
            var msg = Test("roam~0.8");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Proximity()
        {
            var msg = Test("\"jakarta apache\"~10");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Range1()
        {
            var msg = Test("mod_date:[20020101 TO 20030101]");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Range2()
        {
            var msg = Test("title:{Aida TO Carmen}");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Boosting1()
        {
            var msg = Test("jakarta apache");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Boosting2()
        {
            var msg = Test("jakarta^4 apache");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Boosting3()
        {
            var msg = Test("\"jakarta apache\"^4 \"Apache Lucene\"");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_BooleanOperators_OR1()
        {
            var msg = Test("\"jakarta apache\" jakarta");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_BooleanOperators_OR2()
        {
            var msg = Test("\"jakarta apache\" OR jakarta");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_BooleanOperators_AND1()
        {
            var msg = Test("\"jakarta apache\" AND \"Apache Lucene\"");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_BooleanOperators_AND2()
        {
            var msg = Test("+jakarta lucene");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_BooleanOperators_NOT1()
        {
            var msg = Test("\"jakarta apache\" NOT \"Apache Lucene\"");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_BooleanOperators_NOT2()
        {
            var msg = Test("NOT \"jakarta apache\"");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_BooleanOperators_NOT3()
        {
            var msg = Test("\"jakarta apache\" -\"Apache Lucene\"");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_Grouping()
        {
            //Expected: BooleanQuery(Clause(Occur(+), BooleanQuery(Clause(Occur(), TermQuery(Term(_Text:jakarta))), Clause(Occur(), TermQuery(Term(_Text:apache))))), Clause(Occur(+), TermQuery(Term(_Text:website)))),
            //  actual: BooleanQuery(Clause(Occur(), BooleanQuery(Clause(Occur(), BooleanQuery(Clause(Occur(), TermQuery(Term(_Text:jakarta))), Clause(Occur(), TermQuery(Term(_Text:apache))))))), Clause(Occur(), TermQuery(Term(_Text:website)))).	

            var msg = Test("(jakarta OR apache) AND website");
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void ParserFromDoc_FieldGrouping()
        {
            var msg = Test("title:(+return +\"pink panther\")");
            Assert.IsNull(msg, msg);
        }

        //========================================================================================

        [TestMethod]
        public void Parser_Values_IntegerRange()
        {
            Assert.Inconclusive();

            //var parsers = new Dictionary<string, IQueryFieldValueParser>
            //{
            //    {"IntegerField", new TestIntParser()}
            //};
            //new LuceneSearhEngineAccessor((LuceneSearchEngine)StorageContext.Search.SearchEngine).SetParsers(parsers);

            //var queryText = "IntegerField:{10 TO 100]";
            //var expectedQuery = NumericRangeQuery.NewIntRange("IntegerField", 10, 100, false, true);
            //var actualQuery = ParseNewQuery(queryText);
            //var msg = CompareQueries(expectedQuery, actualQuery);
            //Assert.IsNull(msg, "#1: " + msg);

            //queryText = "IntegerField:{ TO 100]";
            //expectedQuery = NumericRangeQuery.NewIntRange("IntegerField", null, 100, false, true);
            //actualQuery = ParseNewQuery(queryText);
            //msg = CompareQueries(expectedQuery, actualQuery);
            //Assert.IsNull(msg, "#2: " + msg);

            //queryText = "IntegerField:{10 TO ]";
            //expectedQuery = NumericRangeQuery.NewIntRange("IntegerField", 10, null, false, true);
            //actualQuery = ParseNewQuery(queryText);
            //msg = CompareQueries(expectedQuery, actualQuery);
            //Assert.IsNull(msg, "#3: " + msg);
        }
        [TestMethod]
        public void Parser_Values_FloatRange()
        {
            Assert.Inconclusive();

            //var parsers = new Dictionary<string, IQueryFieldValueParser>
            //{
            //    {"FloatField", new TestFloatParser()}
            //};
            //new LuceneSearhEngineAccessor((LuceneSearchEngine)StorageContext.Search.SearchEngine).SetParsers(parsers);

            //var queryText = "FloatField:{10.1 TO 10.5]";
            //var expectedQuery = NumericRangeQuery.NewFloatRange("FloatField", (Single)10.1, (Single)10.5, false, true);
            //var actualQuery = ParseNewQuery(queryText);
            //var msg = CompareQueries(expectedQuery, actualQuery);
            //Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_Values_DateRange()
        {
            Assert.Inconclusive();

            //var parsers = new Dictionary<string, IQueryFieldValueParser>
            //{
            //    {"DateField", new TestDateTimeParser()}
            //};
            //new LuceneSearhEngineAccessor((LuceneSearchEngine)StorageContext.Search.SearchEngine).SetParsers(parsers);

            //var queryText = "DateField:['2010-01-01 13:15:15' TO '2010-12-31']";
            //var expectedQuery = NumericRangeQuery.NewLongRange("DateField",
            //    DateTime.Parse("2010-01-01 13:15:15").Ticks, DateTime.Parse("2010-12-31").Ticks, true, true);
            //var actualQuery = ParseNewQuery(queryText);
            //var msg = CompareQueries(expectedQuery, actualQuery);
            //Assert.IsNull(msg, msg);
        }

        //========================================================================================

        [TestMethod]
        public void Parser_Keywords_1()
        {
            string src, msg, @out;

            //src = "Name:xy";
            //msg = CheckQueryObject(src, null, null, 0, 0, "");
            //Assert.IsNull(msg, "#1: " + msg);

            src = "Name:xy .LIFESPAN:ON .COUNTONLY";
            msg = CheckQueryObject(src, "Name:xy .COUNTONLY .LIFESPAN:ON", "Name:xy", true, true, null, null, true);
            Assert.IsNull(msg, "#1: " + msg);

            src = "Name:xy .COUNTONLY .LIFESPAN:ON";
            msg = CheckQueryObject(src, src, "Name:xy", true, true, null, null, true);
            Assert.IsNull(msg, "#1: " + msg);

            src = "Name:xy .AUTOFILTERS:OFF";
            msg = CheckQueryObject(src, src, "Name:xy", false, false, null, null, false);
            Assert.IsNull(msg, "#1: " + msg);

            src = "Name:xy .AUTOFILTERS:ON";
            msg = CheckQueryObject(src, "Name:xy", "Name:xy", true, false, null, null, null);
            Assert.IsNull(msg, "#2: " + msg);

            src = "Name:xy .LIFESPAN:ON";
            msg = CheckQueryObject(src, src, "Name:xy", true, true, null, null, null);
            Assert.IsNull(msg, "#3: " + msg);

            src = "Name:xy .LIFESPAN:OFF";
            msg = CheckQueryObject(src, "Name:xy", "Name:xy", true, false, null, null, null);
            Assert.IsNull(msg, "#4: " + msg);

            src = "Name:xy .AUTOFILTERS:OFF .LIFESPAN:ON";
            msg = CheckQueryObject(src, src, "Name:xy", false, true, null, null, null);
            Assert.IsNull(msg, "#5: " + msg);

            src = " .TOP:10 .SKIP:20 Name:xy .SORT:Field1 .REVERSESORT:Field2 .SORT:Field3 DisplayName:title ";
            @out = "Name:xy DisplayName:title .TOP:10 .SKIP:20 .SORT:Field1 .REVERSESORT:Field2 .SORT:Field3";
            msg = CheckQueryObject(src, @out, "Name:xy DisplayName:title", true, false, 10, 20, null, "+Field1", "-Field2", "+Field3");
            Assert.IsNull(msg, "#6: " + msg);
        }
        private string CheckQueryObject(string src, string qstring, string qqstring, bool autofilters, bool lifespan, int? top, int? skip, bool? countOnly, params string[] sortnames)
        {
            var query = LucQuery.Parse(src);

            var realqstring = query.ToString();
            var realqqstring = query.Query.ToString();
            var realtop = query.Top;
            var realskip = query.Skip;
            var realCountOnly = query.CountOnly;
            var sortcount = query.SortFields.Length;
            var realsortnames = new string[sortcount];
            var realautofilters = query.EnableAutofilters;
            var reallifespan = query.EnableLifespanFilter;
            for (int i = 0; i < sortcount; i++)
            {
                if (i + 1 <= sortcount)
                {
                    var name = query.SortFields[i].GetField();
                    var reverse  = query.SortFields[i].GetReverse();
                    realsortnames[i] = (reverse ? "-" : "+") + name;
                }
            }
            var msg = new StringBuilder();
            if (qstring != null)
                if (qstring != realqstring)
                    msg.Append("qstring='").Append(realqstring).Append("' expected='").Append(qstring).AppendLine("'");
            if (qqstring != null)
                if (qqstring != realqqstring)
                    msg.Append("qqstring='").Append(realqqstring).Append("' expected='").Append(qqstring).AppendLine("'");
            if (top != null)
                if (top.Value != realtop)
                    msg.Append("top=").Append(realtop).Append(" expected=").Append(top).AppendLine();
            if (skip != null)
                if (skip.Value != realskip)
                    msg.Append("skip=").Append(realskip).Append(" expected=").Append(skip).AppendLine();
            if (countOnly != null)
                if (countOnly.Value != realCountOnly)
                    msg.Append("countOnly=").Append(realCountOnly).Append(" expected=").Append(countOnly).AppendLine();
            var s = String.Join(", ", sortnames);
            var ss = String.Join(", ", realsortnames);
            if (s != ss)
                msg.Append("sortnames='").Append(ss).Append("' expected='").Append(s).AppendLine("'");
            if(realautofilters != autofilters)
                msg.Append("Autofilters='").Append(realautofilters).Append("' expected='").Append(autofilters).AppendLine("'");
            if(reallifespan != lifespan)
                msg.Append("Lifespan='").Append(reallifespan).Append("' expected='").Append(lifespan).AppendLine("'");

            return msg.Length == 0 ? null : msg.ToString();
        }

        //========================================================================================

        [TestMethod]
        public void Parser_ReferenceRecursive()
        {
            Assert.Inconclusive();
        }

        //========================================================================================

        [TestMethod]
        public void Parser_UnexpectedRParen()
        {
            var thrown = false;
            try
            {
                var q = ParseNewQuery("(asimov asdf)) qwer");
            }
            catch (Exception e)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);
        }
        [TestMethod]
        public void Parser_UnexpectedRParenInValue()
        {
            var thrown = false;
            try
            {
                var q = ParseNewQuery("F:(asimov) asdf) F:qwer");
            }
            catch (Exception e)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void Parser_NameContainsSpace()
        {
            var queryText = "Name:\"duis et lorem.doc\"";
            var expectedQuery = new TermQuery(new Term("Name", "duis et lorem.doc"));
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Parser_FieldValueIsEmpty()
        {
            var queryText = "Name:\"\"";
            var expectedQuery = new TermQuery(new Term("Name", String.Empty));
            var actualQuery = ParseNewQuery(queryText);
            var msg = CompareQueries(expectedQuery, actualQuery);
            Assert.IsNull(msg, msg);
        }

        [TestMethod]
        public void Parser_QueryParser_AndOperator()
        {
            var dump = new StringBuilder();
            var msg = new StringBuilder();

            #region without AND/OR
            Parser_QueryParser_AndOperatorDump("alma (korte -szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma (-korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma (-korte -szilva)", dump, msg);

            Parser_QueryParser_AndOperatorDump("alma -(korte -szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma -(-korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma -(-korte -szilva)", dump, msg);

            Parser_QueryParser_AndOperatorDump("-alma (korte -szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("-alma (-korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("-alma (-korte -szilva)", dump, msg);

            Parser_QueryParser_AndOperatorDump("-alma -(korte -szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("-alma -(-korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("-alma -(-korte -szilva)", dump, msg);

            Parser_QueryParser_AndOperatorDump("alma (korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma (korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma (+korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma (+korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma +(korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma +(korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma +(+korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma +(+korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma (korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma (korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma (+korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma (+korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma +(korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma +(korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma +(+korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma +(+korte +szilva)", dump, msg);
            #endregion

            #region with AND/OR
            Parser_QueryParser_AndOperatorDump("alma (korte OR szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma (korte AND szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma +(korte OR szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma +(korte AND szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma (korte OR szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma (korte AND szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma +(korte OR szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("+alma +(korte AND szilva)", dump, msg);

            Parser_QueryParser_AndOperatorDump("alma OR (korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma OR (korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma OR (+korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma OR (+korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma AND (korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma AND (korte +szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma AND (+korte szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma AND (+korte +szilva)", dump, msg);

            Parser_QueryParser_AndOperatorDump("alma OR (korte OR szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma OR (korte AND szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma AND (korte OR szilva)", dump, msg);
            Parser_QueryParser_AndOperatorDump("alma AND (korte AND szilva)", dump, msg);
            #endregion

            #region extras
            Parser_QueryParser_AndOperatorDump("alma OR (+zxcv +korte +szilva)", dump, msg);
            //Parser_QueryParser_AndOperatorDump("alma OR +(korte szilva asdf OR qwer)", dump, msg);
            //Parser_QueryParser_AndOperatorDump("asdf OR asdf OR NOT sdf OR qwer", dump, msg);
            //Parser_QueryParser_AndOperatorDump("alma OR +(zxcv OR (korte szilva) (asdf qwer))", dump, msg);
            #endregion

            var message = msg.ToString();
            Assert.IsTrue(message.Length == 0, message);
        }
        private void Parser_QueryParser_AndOperatorDump(string queryText, StringBuilder dump, StringBuilder msg)
        {
            QueryParser parser;
            SnLucParser snParser;

            parser = new QueryParser(LUCENEVERSION, LucObject.FieldName.AllText, IndexManager.GetAnalyzer());
            var lucQueryOr = parser.Parse(queryText);

            parser = new QueryParser(LUCENEVERSION, LucObject.FieldName.AllText, IndexManager.GetAnalyzer());
            parser.SetDefaultOperator(QueryParser.Operator.AND);
            var LucQueryAnd = parser.Parse(queryText);

            snParser = new SnLucParser();
            var snQueryOr = snParser.Parse(queryText);
            //snQueryOr = snQueryOr.Rewrite(IndexManager.GetIndexReader());

            snParser = new SnLucParser();
            var snQueryAnd = snParser.Parse(queryText, SnLucParser.DefaultOperator.And);
            //snQueryAnd = snQueryAnd.Rewrite(IndexManager.GetIndexReader());

            dump.Append(queryText);
            dump.Append('\t');
            dump.Append(lucQueryOr.ToString());
            dump.Append('\t');
            dump.Append(LucQueryAnd.ToString());
            dump.Append('\t');
            dump.Append(snQueryOr.ToString());
            dump.Append('\t');
            dump.Append(snQueryAnd.ToString());
            dump.AppendLine();

            var lucQueryOrString = lucQueryOr.ToString();
            var LucQueryAndString = LucQueryAnd.ToString();
            var snQueryOrString = snQueryOr.ToString();
            var snQueryAndString = snQueryAnd.ToString();

            if (lucQueryOrString != snQueryOrString)
                msg.Append("Error with  OR operator. Query: '").Append(queryText)
                    .Append("'. Expected: '").Append(lucQueryOrString)
                    .Append("'.   Actual: '").AppendLine(snQueryOrString);

            if (LucQueryAndString != snQueryAndString)
                msg.Append("Error with AND operator. Query: '").Append(queryText)
                    .Append("'. Expected: '").Append(LucQueryAndString)
                    .Append("'.   Actual: '").AppendLine(snQueryAndString);
        }

        [TestMethod]
        public void TextSplitter()
        {
            var parser = new SnLucParser();
            var query = parser.Parse("Default_Skin");
            var phraseQuery = query as PhraseQuery;
            Assert.IsNotNull(phraseQuery, "query is not PhraseQuery");
            var terms = phraseQuery.GetTerms();
            Assert.IsTrue(terms.Length == 2, String.Concat("terms.Length is ", terms.Length, ", expected 2"));
            Assert.IsTrue(terms[0].Text() == "default", String.Concat("term 0 is '", terms[0].Text(), "', expected 'default'"));
            Assert.IsTrue(terms[1].Text() == "skin", String.Concat("term 1 is '", terms[1].Text(), "', expected 'skin'"));

            parser = new SnLucParser();
            query = parser.Parse("_.,WORD1__WORD2%_%WORD3__");
            phraseQuery = query as PhraseQuery;
            Assert.IsNotNull(phraseQuery, "query is not PhraseQuery");
            terms = phraseQuery.GetTerms();
            Assert.IsTrue(terms.Length == 3, String.Concat("terms.Length is ", terms.Length, ", expected 3"));
            Assert.IsTrue(terms[0].Text() == "word1", String.Concat("term 0 is '", terms[0].Text(), "', expected 'word1'"));
            Assert.IsTrue(terms[1].Text() == "word2", String.Concat("term 1 is '", terms[1].Text(), "', expected 'word2'"));
            Assert.IsTrue(terms[2].Text() == "word3", String.Concat("term 2 is '", terms[2].Text(), "', expected 'word3'"));
        }

        //======================================================================================== Empty term query
        [TestMethod]
        public void EmptryTerm()
        {
            CompareQueries(ParseLucQuery(string.Empty), ParseNewQuery(string.Empty));

            var dict = new Dictionary<string, string> { { "a:value b:EMPTY", "a:value" } };
            foreach (var keyValuePair in dict)
            {
                CompareQueries(ParseLucQuery(keyValuePair.Value), ParseNewQuery_EmptyTerm(keyValuePair.Key));
            }
        }
        //======================================================================================== Query ToString tests

        [TestMethod]
        public void Parser_QueryToString()
        {
            var dummy = SenseNet.ContentRepository.Schema.ContentType.GetByName("ContentType");

            CheckQueryToString("Name:value");
            CheckQueryToString("Name:value*");
            CheckQueryToString("Name:value^0.7");
            CheckQueryToString("Name:value*^0.7");
            CheckQueryToString("Name:\"value1 value2\"");
            CheckQueryToString("_Text:\"value1 value2\"");
            CheckQueryToString("_Text:\"value1 value2\"~8");
            CheckQueryToString("\"value1 value2\"", "_Text:\"value1 value2\"");
            CheckQueryToString("_Text:*va??lue1*");
            CheckQueryToString("_Text:*va??lue1*^0.7");
            CheckQueryToString("_Text:value1~0.5");
            CheckQueryToString("_Text:value1~0.5^0.7");

            CheckQueryToString("Index:123");
            CheckQueryToString("Price:123.45");
            CheckQueryToString("ModificationDate:'2011.11.11'", "ModificationDate:\"2011-11-11\"");
            CheckQueryToString("ModificationDate:'2011.11.11 00:00'", "ModificationDate:\"2011-11-11\"");
            CheckQueryToString("ModificationDate:'2011.11.11 12:13'", "ModificationDate:\"2011-11-11 12:13\"");
            CheckQueryToString("ModificationDate:'2011.11.11 12:13:00'", "ModificationDate:\"2011-11-11 12:13\"");
            CheckQueryToString("ModificationDate:'2011.11.11 12:13:14'", "ModificationDate:\"2011-11-11 12:13:14\"");

            CheckQueryToString("Name:[a TO b]");
            CheckQueryToString("Name:[a TO b}");
            CheckQueryToString("Name:{a TO b]");
            CheckQueryToString("Name:{a TO b}");
            CheckQueryToString("Name:[ TO b}", "Name:<b");
            CheckQueryToString("Name:[ TO b]", "Name:<=b");
            CheckQueryToString("Name:[a TO }", "Name:>=a");
            CheckQueryToString("Name:{a TO ]", "Name:>a");
            CheckQueryToString("Name:<a");
            CheckQueryToString("Name:>a");
            CheckQueryToString("Name:<=a");
            CheckQueryToString("Name:>=a");
            CheckQueryToString("Index:[4 TO 9]");
            CheckQueryToString("Index:[4 TO 9}");
            CheckQueryToString("Index:{4 TO 9]");
            CheckQueryToString("Index:{4 TO 9}");
            CheckQueryToString("Index:[ TO 9}", "Index:<9");
            CheckQueryToString("Index:[ TO 9]", "Index:<=9");
            CheckQueryToString("Index:[4 TO }", "Index:>=4");
            CheckQueryToString("Index:{4 TO ]", "Index:>4");
            CheckQueryToString("Index:<5");
            CheckQueryToString("Index:>5");
            CheckQueryToString("Index:<=5");
            CheckQueryToString("Index:>=5");
            CheckQueryToString("Name:a Name:b");
            CheckQueryToString("+Name:a Name:b");
            CheckQueryToString("Name:a +Name:b");
            CheckQueryToString("+Name:a +Name:b");
            CheckQueryToString("-Name:a -Name:b");
            CheckQueryToString("+Name:a -Name:b");
            CheckQueryToString("-Name:a +Name:b");
            CheckQueryToString("+Name:a +Name:b");
            CheckQueryToString("Name:a Name:b");
            CheckQueryToString("-Name:a Name:b");
            CheckQueryToString("Name:a -Name:b");
            CheckQueryToString("-Name:a -Name:b");
            CheckQueryToString("+(Name:a Name:b) +Name:c");

            CheckQueryToString("Path:\"/root/(apps)/readme.txt\"");

        }
        private void CheckQueryToString(string text)
        {
            //var visitor = new ToStringVisitor();
            //visitor.Visit(LucQuery.Parse(text).Query);
            //Assert.AreEqual(text, visitor.ToString());
            Assert.AreEqual(text, LucQuery.Parse(text).QueryText);
        }
        private void CheckQueryToString(string text, string expected)
        {
            //var visitor = new ToStringVisitor();
            //visitor.Visit(LucQuery.Parse(text).Query);
            //Assert.AreEqual(expected, visitor.ToString());
            Assert.AreEqual(expected, LucQuery.Parse(text).QueryText);
        }

        //========================================================================================

        private string Test(string queryText)
        {
            return CompareQueries(ParseLucQuery(queryText), ParseNewQuery(queryText));
        }
        private string DumpTest(string expectedDump, string queryText)
        {
            return CompareDumps(expectedDump, GetQueryDump(ParseNewQuery(queryText)));
        }
        private string CompareQueries(Query expected, Query actual)
        {
            return CompareDumps(GetQueryDump(expected), GetQueryDump(actual));
        }
        private string CompareDumps(string expectedDump, string actualDump)
        {
            if (expectedDump.ToLower() == actualDump.ToLower())
                return null;
            return String.Format("Expected: {0},  actual: {1}.", expectedDump, actualDump);
        }

        private string GetLucQueryDump(string queryText)
        {
            return GetQueryDump(ParseLucQuery(queryText));
        }
        private string GetNewQueryDump(string queryText)
        {
            return GetQueryDump(ParseNewQuery(queryText));
        }
        private Query ParseLucQuery(string queryText)
        {
            var analyzer = IndexManager.GetAnalyzer();
            return new QueryParser(LUCENEVERSION, LucObject.FieldName.AllText, analyzer).Parse(queryText);
        }
        private Query ParseNewQuery(string queryText)
        {
            return new SnLucParser().Parse(queryText);
        }
        private Query ParseNewQuery_EmptyTerm(string queryText)
        {
            var parser = new SnLucParser();
            var query = parser.Parse(queryText);
            if (parser.ParseEmptyQuery)
            {
                var visitor = new EmptyTermVisitor();
                return visitor.Visit(query);
            }
            return query;
        }
        private string GetQueryDump(Query query)
        {
            var v = new DumpVisitor();
            v.Visit(query);
            return v.ToString();
        }
    }
}
