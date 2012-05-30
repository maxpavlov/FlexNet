using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using SenseNet.Search.Parser;

namespace SenseNet.ContentRepository.Tests.LuceneParser
{
    [TestClass]
    public class LexerTest : TestBase
    {
        [DebuggerDisplay("{Token}:{Value}")]
        private class TokenChecker
        {
            public SnLucLexer.Token Token { get; set; }
            public string Value { get; set; }
        }

        #region test infrastructure
        public LexerTest()
        {
            //
            //TODO: Add constructor logic here
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

        //-- new
        [TestMethod]
        public void Lexer_Keywords()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.ControlKeyword, Value = ".AUTOFILTERS" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "OFF" },
                new TokenChecker { Token = SnLucLexer.Token.ControlKeyword, Value = ".SKIP" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.Number,  Value = "100" },
                new TokenChecker { Token = SnLucLexer.Token.ControlKeyword, Value = ".TOP" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.Number,  Value = "20" },
                new TokenChecker { Token = SnLucLexer.Token.ControlKeyword,  Value = ".SORT" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "FieldName1" },
                new TokenChecker { Token = SnLucLexer.Token.ControlKeyword,  Value = ".REVERSESORT" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "FieldName2" },
                new TokenChecker { Token = SnLucLexer.Token.ControlKeyword,  Value = ".SORT" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "FieldName3" },
            };
            var tokens = GetTokens(".AUTOFILTERS:OFF .SKIP:100 .TOP:20 .SORT:FieldName1 .REVERSESORT:FieldName2 .SORT:FieldName3");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_UnknownKeywords1()
        {
            try
            {
                var tokens = GetTokens("  :OFF . . : .:  ");
                Assert.Fail("Expected Parserexception was not thrown.");
            }
            catch (ParserException e)
            {
                Assert.AreEqual(0, e.LineInfo.Line);
                Assert.AreEqual(7, e.LineInfo.Column);
            }
        }
        [TestMethod]
        public void Lexer_UnknownKeywords2()
        {
            try
            {
                var tokens = GetTokens("  ..AUTOFILTERS::OFF . . : .:  ");
                Assert.Fail("Expected Parserexception was not thrown.");
            }
            catch (ParserException e)
            {
                Assert.AreEqual(0, e.LineInfo.Line);
                Assert.AreEqual(2, e.LineInfo.Column);
            }
        }
        [TestMethod]
        public void Lexer_FieldLimiters()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "Field" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,  Value = ":" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "value" },

                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "Field" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,  Value = ":" },
                new TokenChecker { Token = SnLucLexer.Token.GT,  Value = ">" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "value" },

                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "Field" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,  Value = ":" },
                new TokenChecker { Token = SnLucLexer.Token.LT,  Value = "<" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "value" },

                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "Field" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,  Value = ":" },
                new TokenChecker { Token = SnLucLexer.Token.GTE,  Value = ">=" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "value" },

                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "Field" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,  Value = ":" },
                new TokenChecker { Token = SnLucLexer.Token.LTE,  Value = "<=" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "value" },

                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "Field" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,  Value = ":" },
                new TokenChecker { Token = SnLucLexer.Token.NEQ,  Value = "<>" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "value" },
            };
            var tokens = GetTokens(" Field:value Field:>value Field:<value Field:>=value Field:<=value Field:<>value ");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_FieldBadLimiters()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "Field" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,  Value = ":" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "value" },

                new TokenChecker { Token = SnLucLexer.Token.String, Value = "Field>value" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "Field<value" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "Field>=value" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "Field<=value" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "Field<>value" },
            };
            var tokens = GetTokens(" Field:value Field>value Field<value Field>=value Field<=value Field<>value ");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_FieldListFieldPrefix()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "#Field1" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,  Value = ":" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "value" },
                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "#Field2" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,  Value = ":" },
                new TokenChecker { Token = SnLucLexer.Token.String,  Value = "value" },
            };
            var tokens = GetTokens("#Field1:value #Field2:value");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }

        [TestMethod]
        public void Lexer_FieldGrouping()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "title" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.LParen,  Value = "(" },
                new TokenChecker { Token = SnLucLexer.Token.Plus, Value = "+" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "return" },
                new TokenChecker { Token = SnLucLexer.Token.Plus,  Value = "+" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "pink panther" },
                new TokenChecker { Token = SnLucLexer.Token.RParen, Value= ")" },
            };
            var tokens = GetTokens("title:(+return +\"pink panther\")");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }

        //--
        [TestMethod]
        public void Lexer_TextAndNumber()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "text" },
                new TokenChecker { Token = SnLucLexer.Token.Number, Value = "9" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "text" },
                new TokenChecker { Token = SnLucLexer.Token.Number, Value = "12.34" },
                new TokenChecker { Token = SnLucLexer.Token.ControlKeyword, Value = ".SKIP" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "12aa" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "a12" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "12.aa" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "aa12." },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "12.34aa" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,  Value = "-" },
                new TokenChecker { Token = SnLucLexer.Token.Number, Value = "12" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,  Value = "-" },
                new TokenChecker { Token = SnLucLexer.Token.Number, Value = "12.34" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,  Value = "-" },
                new TokenChecker { Token = SnLucLexer.Token.ControlKeyword, Value = ".TOP" }
            };
            var tokens = GetTokens("text 9 text 12.34 .SKIP 12aa a12 12.aa aa12. 12.34aa -12 -12.34 -.TOP");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_AndOrNot()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "text" },
                new TokenChecker { Token = SnLucLexer.Token.And,    Value = "AND" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "and" },
                new TokenChecker { Token = SnLucLexer.Token.Or,     Value = "OR" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "or" },
                new TokenChecker { Token = SnLucLexer.Token.Not,    Value = "NOT" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "not" },
                new TokenChecker { Token = SnLucLexer.Token.Not,    Value = "!" },
                new TokenChecker { Token = SnLucLexer.Token.And,    Value = "&&" },
                new TokenChecker { Token = SnLucLexer.Token.Or,     Value = "||" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "text" }
            };
            var tokens = GetTokens("text AND and OR or NOT not ! && || text");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_StringWithInnerApos()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field,  Value = "fieldname" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "text text 'text' text" }
            };
            var tokens = GetTokens("fieldname:\"text text 'text' text\"");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_Numbers()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field,  Value= "NumberField" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.Number, Value= "45678" },
                new TokenChecker { Token = SnLucLexer.Token.Field,  Value= "NumberField" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.Number, Value= "456.78" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,  Value= "-" },
                new TokenChecker { Token = SnLucLexer.Token.Field,  Value= "NumberField" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,  Value= "-" },
                new TokenChecker { Token = SnLucLexer.Token.Number, Value= "78.456" }
            };
            var tokens = GetTokens("NumberField:45678 NumberField:456.78 -NumberField:-78.456");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_Path()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field,  Value= "Ancestor" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "/Root/System" }
            };
            var tokens = GetTokens("Ancestor:/Root/System");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_TwoPaths()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Plus, Value= "+" },
                new TokenChecker { Token = SnLucLexer.Token.Field, Value= "Ancestor" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "/Root/System" },
                new TokenChecker { Token = SnLucLexer.Token.Minus, Value= "-" },
                new TokenChecker { Token = SnLucLexer.Token.Field, Value= "Path" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "/Root/System" }
            };
            var tokens = GetTokens("+Ancestor:/Root/System -Path:/Root/System");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_Groups()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field,  Value= "Field1" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.LParen, Value= "(" },
                new TokenChecker { Token = SnLucLexer.Token.Plus,   Value= "+" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "aaa" },
                new TokenChecker { Token = SnLucLexer.Token.Plus,   Value= "+" },
                new TokenChecker { Token = SnLucLexer.Token.LParen, Value= "(" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "bbb" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "ccc" },
                new TokenChecker { Token = SnLucLexer.Token.RParen, Value= ")" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "ddd" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,  Value= "-" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "eee" },
                new TokenChecker { Token = SnLucLexer.Token.RParen, Value= ")" },
            };
            var tokens = GetTokens("Field1:(+aaa +(bbb ccc) ddd -eee)");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_Range_Brackets()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field,         Value= "Number" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,         Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.LBracket,      Value= "[" },
                new TokenChecker { Token = SnLucLexer.Token.Number,        Value= "1234" },
                new TokenChecker { Token = SnLucLexer.Token.To,            Value= "TO" },
                new TokenChecker { Token = SnLucLexer.Token.Number,        Value= "5678" },
                new TokenChecker { Token = SnLucLexer.Token.RBracket,      Value= "]" },
            };
            var tokens = GetTokens("Number:[1234 TO 5678]");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_Range_Braces()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field,         Value= "Number" },
                new TokenChecker { Token = SnLucLexer.Token.Colon,         Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.LBrace,        Value= "{" },
                new TokenChecker { Token = SnLucLexer.Token.Number,        Value= "1234" },
                new TokenChecker { Token = SnLucLexer.Token.To,            Value= "TO" },
                new TokenChecker { Token = SnLucLexer.Token.Number,        Value= "5678" },
                new TokenChecker { Token = SnLucLexer.Token.RBrace,        Value= "}" },
            };
            var tokens = GetTokens("Number:{1234 TO 5678}");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_Wildcards()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Plus,            Value= "+" },
                new TokenChecker { Token = SnLucLexer.Token.WildcardString,  Value= "startswith*" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,           Value= "-" },
                new TokenChecker { Token = SnLucLexer.Token.WildcardString,  Value= "*endswith" },
                new TokenChecker { Token = SnLucLexer.Token.WildcardString,  Value= "*contains*" },
                new TokenChecker { Token = SnLucLexer.Token.Plus,            Value= "+" },
                new TokenChecker { Token = SnLucLexer.Token.WildcardString,  Value= "starts*ends" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,           Value= "-" },
                new TokenChecker { Token = SnLucLexer.Token.WildcardString,  Value= "startswith?" },
                new TokenChecker { Token = SnLucLexer.Token.WildcardString,  Value= "?endswith" },
                new TokenChecker { Token = SnLucLexer.Token.Plus,            Value= "+" },
                new TokenChecker { Token = SnLucLexer.Token.WildcardString,  Value= "?contains?" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,           Value= "-" },
                new TokenChecker { Token = SnLucLexer.Token.WildcardString,  Value= "starts?ends" },
            };
            var tokens = GetTokens("+startswith* -*endswith *contains* +starts*ends -startswith? ?endswith +?contains? -starts?ends");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_FieldName()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field, Value = "contains" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "colon" }
            };
            var tokens = GetTokens("contains:colon");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_FieldNameInQuotedString()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "contains:colon" }
            };
            var tokens = GetTokens("\"contains:colon\"");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }

        [TestMethod]
        public void Lexer_QuotedStringAndEscapes()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "contains:colon" }
            };
            var tokens = GetTokens("\"contains\\:colon\"");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Lexer_NonQuotedStringAndEscapes()
        {
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.String, Value = "contains:colon" }
            };
            var tokens = GetTokens("contains\\:colon");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }

        [TestMethod]
        public void Lexer_X()
        {
            //dump: BooleanQuery(Clause(Occur(), TermQuery(Term(F:a))), Clause(Occur(), BooleanQuery(Clause(Occur(+), TermQuery(Term(G:b))), Clause(Occur(-), TermQuery(Term(F:d))))))
            var expectedTokens = new TokenChecker[]
            {
                new TokenChecker { Token = SnLucLexer.Token.Field,  Value= "F" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.LParen, Value= "(" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "a" },
                new TokenChecker { Token = SnLucLexer.Token.LParen, Value= "(" },
                new TokenChecker { Token = SnLucLexer.Token.Plus,   Value= "+" },
                new TokenChecker { Token = SnLucLexer.Token.Field,  Value= "G" },
                new TokenChecker { Token = SnLucLexer.Token.Colon, Value= ":" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "b" },
                new TokenChecker { Token = SnLucLexer.Token.Minus,  Value= "-" },
                new TokenChecker { Token = SnLucLexer.Token.String, Value= "d" },
                new TokenChecker { Token = SnLucLexer.Token.RParen, Value= ")" },
                new TokenChecker { Token = SnLucLexer.Token.RParen, Value= ")" }
            };
            var tokens = GetTokens("F:(a (+G:b -d))");
            var msg = CheckTokensAndEof(tokens, expectedTokens);
            Assert.IsNull(msg, msg);
        }

        [TestMethod]
        public void Lexer_CharTypeDoesNotThrow()
        {
            var s = new String(Enumerable.Range(1, 256 - 32).Select(i => (char)i).ToArray());
            var lexer = new SnLucLexer(s);
            var lexerAcc = new PrivateObject(lexer);
            var thrown = false;
            try
            {
                while ((bool)lexerAcc.Invoke("NextChar")) ;
            }
            catch (Exception e)
            {
                thrown = true;
            }
            Assert.IsFalse(thrown);
        }

        private IEnumerable<TokenChecker> GetTokens(string source)
        {
            var lexer = new SnLucLexer(source);
            var tokens = new List<TokenChecker>();
            do
            {
                //tokens.Add(new TokenChecker { Token = lexer.CurrentToken, Value = lexer.StringValue });
                tokens.Add(new TokenChecker { Token = lexer.CurrentToken, Value = lexer.StringValue });
            }
            while (lexer.NextToken());

            return tokens;
        }
        private string CheckTokensAndEof(IEnumerable<TokenChecker> tokensToCheck, IEnumerable<TokenChecker> expectedTokens)
        {
            if (tokensToCheck.Count() != expectedTokens.Count())
                return string.Format("Token counts are not equal. Expected: {0}, Current: {1}", expectedTokens.Count(), tokensToCheck.Count());

            for (int i = 0; i < tokensToCheck.Count(); i++)
            {
                if (tokensToCheck.ElementAt(i).Token != expectedTokens.ElementAt(i).Token)
                    return string.Format("Tokens are not equal on position {0}. Expected: {1}, Current: {2}", i, expectedTokens.ElementAt(i).Token, tokensToCheck.ElementAt(i).Token);
                if (tokensToCheck.ElementAt(i).Value != expectedTokens.ElementAt(i).Value)
                    return string.Format("Values are not equal on position {0}. Expected: {1}, Current: {2}", i, expectedTokens.ElementAt(i).Value, tokensToCheck.ElementAt(i).Value);
            }
            return null;
        }
    }
}
