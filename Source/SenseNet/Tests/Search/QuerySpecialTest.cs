using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using SNC = SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using System.Collections;
using System.Xml;
using SenseNet.ContentRepository.Storage.Search;
using System.Linq;

namespace SenseNet.ContentRepository.Tests.Search
{
    [TestClass]
    public class QuerySpecialTest : TestBase
    {
        #region Test Infrastructure
        private TestContext testContextInstance;

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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            //Assert.Inconclusive("Query feature is not implemented");

            string typeName = "ForMultiPagingSearch";
            StringBuilder sb = new StringBuilder();

            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            sb.Append("<ContentType name='").Append(typeName).Append("' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>\r\n");
            sb.Append("	<DisplayName>TestSurvey</DisplayName>\r\n");
            sb.Append("	<Description>Test Survey</Description>\r\n");
            sb.Append("	<Icon>Survey</Icon>\r\n");
            sb.Append("	<Fields>\r\n");
            int k = 2;
            //for (int i = 0; i < 80 * k; i++)
            //{
            //    sb.Append("		<Field name='ShortText_").Append(i + 1).Append("' type='ShortText'>\r\n");
            //    sb.Append("			<DisplayName>ShortText #").Append(i + 1).Append("</DisplayName>\r\n");
            //    sb.Append("			<Description>ShortText #").Append(i + 1).Append("</Description>\r\n");
            //    sb.Append("			<Icon>field.gif</Icon>\r\n");
            //    sb.Append("		</Field>\r\n");
            //}
            for (int i = 0; i < 40 * k; i++)
            {
                sb.Append("		<Field name='Integer_").Append(i + 1).Append("' type='Integer'>\r\n");
                sb.Append("			<DisplayName>Integer #").Append(i + 1).Append("</DisplayName>\r\n");
                sb.Append("			<Description>Integer #").Append(i + 1).Append("</Description>\r\n");
                sb.Append("			<Icon>field.gif</Icon>\r\n");
                sb.Append("		</Field>\r\n");
            }
            //for (int i = 0; i < 25 * k; i++)
            //{
            //    sb.Append("		<Field name='DateTime_").Append(i + 1).Append("' type='DateTime'>\r\n");
            //    sb.Append("			<DisplayName>DateTime #").Append(i + 1).Append("</DisplayName>\r\n");
            //    sb.Append("			<Description>DateTime #").Append(i + 1).Append("</Description>\r\n");
            //    sb.Append("			<Icon>field.gif</Icon>\r\n");
            //    sb.Append("		</Field>\r\n");
            //}
            //for (int i = 0; i < 15 * k; i++)
            //{
            //    sb.Append("		<Field name='Number_").Append(i + 1).Append("' type='Number'>\r\n");
            //    sb.Append("			<DisplayName>Number #").Append(i + 1).Append("</DisplayName>\r\n");
            //    sb.Append("			<Description>Number #").Append(i + 1).Append("</Description>\r\n");
            //    sb.Append("			<Icon>field.gif</Icon>\r\n");
            //    sb.Append("		</Field>\r\n");
            //}
            sb.Append("	</Fields>");
            sb.Append("</ContentType>");

            ContentTypeInstaller.InstallContentType(sb.ToString());
            ContentType ct = ContentType.GetByName(typeName);

            for (int j = 0; j < 10; j++)
            {
                string name = "Node" + j;
                Content content = Content.Load(RepositoryPath.Combine(TestRoot.Path, name));
                if (content == null)
                    content = Content.CreateNew(typeName, TestRoot, name);

                //for (int i = 0; i < 80 * k; i++)
                //{
                //    string fieldName=String.Concat("ShortText_", i+1);
                //    string fieldValue=String.Concat("value ", i+1);
                //    content[fieldName] = fieldValue;
                //}
                for (int i = 0; i < 40 * k; i++)
                    content[String.Concat("Integer_", i + 1)] = j;
                //for (int i = 0; i < 25 * k; i++)
                //{
                //    string fieldName = String.Concat("DateTime_", i + 1);
                //    DateTime fieldValue = DateTime.Now.AddMinutes(i+1);
                //    content[fieldName] = fieldValue;
                //}
                //for (int i = 0; i < 15 * k; i++)
                //{
                //    string fieldName = String.Concat("Number_", i + 1);
                //    double fieldValue = i + 1;
                //    content[fieldName] = fieldValue;
                //}
                content.Save();
                //int id = content.ContentHandler.Id;

            }

            //----------------------------------------------
        }
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

        private static string _testRootName = "_QuerySpecialTest";
        private static string _testRootPath = String.Concat("/Root/", _testRootName);
        /// <summary>
        /// Do not use. Instead of TestRoot property
        /// </summary>
        private static Node _testRoot;
        public static Node TestRoot
        {
            get
            {
                if (_testRoot == null)
                {
                    _testRoot = Node.LoadNode(_testRootPath);
                    if (_testRoot == null)
                    {
                        Node node = NodeType.CreateInstance("SystemFolder", Node.LoadNode("/Root"));
                        node.Name = _testRootName;
                        node.Save();
                        _testRoot = Node.LoadNode(_testRootPath);
                    }
                }
                return _testRoot;
            }
        }

        [ClassCleanup]
        public static void DestroyPlayground()
        {
            try
            {
                if (Node.Exists(_testRootPath))
                    Node.ForceDelete(_testRootPath);

                if (ActiveSchema.NodeTypes["TestNode11"] != null)
                    ContentTypeInstaller.RemoveContentType(ContentType.GetByName("TestNode11"));
                if (ActiveSchema.NodeTypes["TestNode10"] != null)
                    ContentTypeInstaller.RemoveContentType(ContentType.GetByName("TestNode10"));

                ContentType ct;
                ct = ContentType.GetByName("TestNode11");
                if (ct != null)
                    ContentTypeInstaller.RemoveContentType(ct);
                ct = ContentType.GetByName("TestNode10");
                if (ct != null)
                    ContentTypeInstaller.RemoveContentType(ct);
                ct = ContentType.GetByName("DataTypeCollisionTestHandler1");
                if (ct != null)
                    ContentTypeInstaller.RemoveContentType(ct);
                ct = ContentType.GetByName("DataTypeCollisionTestHandler");
                if (ct != null)
                    ContentTypeInstaller.RemoveContentType(ct);
                ct = ContentType.GetByName("TestSurvey");
                if (ct != null)
                    ContentTypeInstaller.RemoveContentType(ct);
                ct = ContentType.GetByName("ForMultiPagingSearch");
                if (ct != null)
                    ContentTypeInstaller.RemoveContentType(ct);
            }
            catch (Exception e)
            {
                //throw;
            }
        }

        //---- Bug 1797: Do not delete this test. Resolve bug instead.
        //[TestMethod]
        //public void Query_LongFlat_2IntAndRefFlatProp_002()
        //{
        //    var query = new NodeQuery();
        //    query.Add
        //        (
        //            new ExpressionList
        //            (
        //                ChainOperator.Or,
        //                new ExpressionList(
        //                    ChainOperator.And,
        //                    new IntExpression(ActiveSchema.PropertyTypes["Integer_1"], ValueOperator.GreaterThanOrEqual, 2),
        //                    new IntExpression(ActiveSchema.PropertyTypes["Integer_41"], ValueOperator.LessThanOrEqual, 5),
        //                    new ReferenceExpression(ReferenceAttribute.CreatedBy,
        //        //new StringExpression(StringAttribute.Name, StringOperator.Equal, "Administrator"))
        //                        new StringExpression(ActiveSchema.PropertyTypes["Domain"], StringOperator.Equal, "BuiltIn"))
        //                ),
        //                new IntExpression(ActiveSchema.PropertyTypes["Integer_70"], ValueOperator.Equal, 8)
        //            )
        //        );

        //    NodeQueryParameter[] p;
        //    var sql = query.Compile(out p);

        //    var nodes = query.Execute();
        //    var names = new List<string>(from n in nodes.Nodes orderby n.Name select n.Name);

        //    Assert.IsTrue(nodes.Count == 5, "#10");
        //    Assert.IsTrue(names[0] == "Node2", "#11");
        //    Assert.IsTrue(names[1] == "Node3", "#12");
        //    Assert.IsTrue(names[2] == "Node4", "#13");
        //    Assert.IsTrue(names[3] == "Node5", "#14");
        //    Assert.IsTrue(names[4] == "Node8", "#15");
        //}

        //[TestMethod]
        //public void Query_LongFlat_2IntSamePage_003()
        //{
        //    var query = new NodeQuery();
        //    query.Add(new IntExpression(ActiveSchema.PropertyTypes["Integer_21"], ValueOperator.LessThanOrEqual, 5));
        //    query.Add(new IntExpression(ActiveSchema.PropertyTypes["Integer_21"], ValueOperator.GreaterThanOrEqual, 2));

        //    NodeQueryParameter[] p;
        //    var sql = query.Compile(out p);

        //    var nodes = query.Execute();
        //    var names = new List<string>(from n in nodes.Nodes orderby n.Name select n.Name);

        //    Assert.IsTrue(nodes.Count == 4, "#0");
        //    Assert.IsTrue(names[0] == "Node2", "#1");
        //    Assert.IsTrue(names[1] == "Node3", "#2");
        //    Assert.IsTrue(names[2] == "Node4", "#3");
        //    Assert.IsTrue(names[3] == "Node5", "#4");
        //}

        [TestMethod]
        public void Query_LongFlat_2IntPaging_004()
        {
            var query = new NodeQuery();
            query.Add(new IntExpression(ActiveSchema.PropertyTypes["Integer_1"], ValueOperator.GreaterThanOrEqual, 2));
            query.Add(new IntExpression(ActiveSchema.PropertyTypes["Integer_21"], ValueOperator.LessThanOrEqual, 5));
            query.StartIndex = 1;
            query.PageSize = 3;
            var nodes = query.Execute();

            var names = new List<string>(from n in nodes.Nodes orderby n.Name select n.Name);

            Assert.IsTrue(nodes.Count == 3, "#0");
            Assert.IsTrue(names[0] == "Node2", "#1");
            Assert.IsTrue(names[1] == "Node3", "#2");
            Assert.IsTrue(names[2] == "Node4", "#3");
        }

        [TestMethod]
        public void QueryTemplateReplacerTest()
        {
            var queryXml = @"<SearchExpression xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/SearchExpression"">
  <And>
    <String op=""Contains"" property=""Path"">/<currentuser property=""Name""/>/</String>
  </And>
</SearchExpression>
";
            var query = NodeQuery.Parse(queryXml);
            var xml = new XmlDocument();
            xml.LoadXml(query.ToXml());
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("x", NodeQuery.XmlNamespace);
            var element = xml.SelectSingleNode("/x:SearchExpression/x:And/x:String[1]", nsmgr);
            var replacedValue = element == null ? null : element.InnerText;
            Assert.IsTrue(replacedValue == "/Administrator/");

        }

    }
}