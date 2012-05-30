using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Schema;
using System.Reflection;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using System.Linq;
using System.Diagnostics;
using SenseNet.ContentRepository.Tests;
using SenseNet.ContentRepository.Storage.Security;
using System.Threading;
using SenseNet.ContentRepository.Tests.ContentHandlers;
using SenseNet.ContentRepository.Versioning;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository.Tests.Search
{
    [TestClass()]
    public class SearchTest : TestBase
    {
        #region Accessors
        private class ObjectAccessor : Accessor
        {
            public ObjectAccessor(object target) : base(target) { }
            public new object GetPublicValue(string name)
            {
                return base.GetPublicValue(name);
            }

        }
        //private class SearchOrderAccessor : Accessor
        //{
        //    public SearchOrderAccessor(SearchOrder target) : base(target) { }

        //    public NodeAttribute NodeAttribute
        //    {
        //        get
        //        {
        //            object propertyToOrder = GetInternalValue("PropertyToOrder");
        //            ObjectAccessor acc = new ObjectAccessor(propertyToOrder);
        //            return (NodeAttribute)acc.GetPublicValue("NodeAttribute");
        //        }
        //    }
        //    public PropertyType PropertySlot
        //    {
        //        get
        //        {
        //            object propertyToOrder = GetInternalValue("PropertyToOrder");
        //            ObjectAccessor acc = new ObjectAccessor(propertyToOrder);
        //            return (PropertyType)acc.GetPublicValue("PropertySlot");
        //        }
        //    }
        //    public bool IsSlot
        //    {
        //        get
        //        {
        //            object propertyToOrder = GetInternalValue("PropertyToOrder");
        //            ObjectAccessor acc = new ObjectAccessor(propertyToOrder);
        //            return (bool)acc.GetPublicValue("IsSlot");
        //        }
        //    }
        //    public OrderDirection Direction
        //    {
        //        get { return (OrderDirection)GetInternalValue("Direction"); }
        //    }
        //}
        //private class TypeExpressionAccessor : Accessor
        //{
        //    public TypeExpressionAccessor(TypeExpression target) : base(target) { }

        //    internal NodeType NodeType
        //    {
        //        get { return (SenseNet.ContentRepository.Storage.Schema.NodeType)GetInternalValue("NodeType"); }
        //    }
        //    internal bool ExactMatch
        //    {
        //        get { return (bool)GetInternalValue("ExactMatch"); }
        //    }
        //}
        //private class SearchExpressionAccessor : Accessor
        //{
        //    public SearchExpressionAccessor(SearchExpression target) : base(target) { }

        //    internal string FullTextExpression
        //    {
        //        get { return (string)GetInternalValue("FullTextExpression"); }
        //    }
        //}
        //private class NotExpressionAccessor : Accessor
        //{
        //    public NotExpressionAccessor(NotExpression target) : base(target) { }

        //    internal Expression Expression
        //    {
        //        get { return (SenseNet.ContentRepository.Storage.Search.Expression)GetInternalValue("Expression"); }
        //    }
        //}
        #endregion

        //################################################################################################

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
            InstallNodeTypes();
            GenerateManyNodes(10);
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            CreateTestStructure();
            _existName = string.Empty;
        }
        //
        //Use TestCleanup to run code after each test has run
        //
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion



        static List<string> _pathsToDelete = new List<string>();
        static void AddPathToDelete(string path)
        {
            lock (_pathsToDelete)
            {
                if (_pathsToDelete.Contains(path))
                    return;
                _pathsToDelete.Add(path);
            }
        }
        [ClassCleanup]
        public static void DestroyPlayground()
        {
            foreach (string path in _pathsToDelete)
            {
                try
                {
                    Node n = Node.LoadNode(path);
                    if (n != null)
                        Node.ForceDelete(path);
                }
                catch
                {
                    throw;
                }
            }
            try
            {
                TestTools.RemoveNodesAndType("RepositoryTest_RefTestNode");
            }
            catch
            {
                throw;
            }
        }


        [TestMethod()]
        public void SearchOrder_Constructor_Name()
        {
            SearchOrder target = new SearchOrder(StringAttribute.Name);

            //SearchOrderAccessor acc = new SearchOrderAccessor(target);

            Assert.IsTrue(target.Direction == OrderDirection.Asc, "#1");
            Assert.IsTrue(target.PropertyToOrder.IsSlot == false, "#2");
            Assert.IsTrue(target.PropertyToOrder.NodeAttribute == (NodeAttribute)StringAttribute.Name, "#3");
        }
        [TestMethod()]
        public void SearchOrder_Constructor_NameAndDirectionAsc()
        {
            SearchOrder target = new SearchOrder(StringAttribute.Name, OrderDirection.Asc);

            //SearchOrderAccessor acc = new SearchOrderAccessor(target);

            Assert.IsTrue(target.Direction == OrderDirection.Asc, "#1");
            Assert.IsTrue(target.PropertyToOrder.IsSlot == false, "#2");
            Assert.IsTrue(target.PropertyToOrder.NodeAttribute == (NodeAttribute)StringAttribute.Name, "#3");
        }
        [TestMethod()]
        public void SearchOrder_Constructor_NameAndDirectionDesc()
        {
            SearchOrder target = new SearchOrder(StringAttribute.Name, OrderDirection.Desc);

            //SearchOrderAccessor acc = new SearchOrderAccessor(target);

            Assert.IsTrue(target.Direction == OrderDirection.Desc, "#1");
            Assert.IsTrue(target.PropertyToOrder.IsSlot == false, "#2");
            Assert.IsTrue(target.PropertyToOrder.NodeAttribute == (NodeAttribute)StringAttribute.Name, "#3");
        }
        [TestMethod()]
        public void SearchOrder_Constructor_Slot()
        {
            SchemaEditor ed = new SchemaEditor();
            PropertyType slot = ed.CreatePropertyType("slot", DataType.String);

            SearchOrder target = new SearchOrder(slot);
            //SearchOrderAccessor acc = new SearchOrderAccessor(target);

            Assert.IsTrue(target.Direction == OrderDirection.Asc, "#1");
            Assert.IsTrue(target.PropertyToOrder.IsSlot == true, "#2");
            Assert.IsTrue(Object.ReferenceEquals(target.PropertyToOrder.PropertySlot, slot), "#3");
        }
        [TestMethod()]
        public void SearchOrder_Constructor_SlotAndDirectionAsc()
        {
            SchemaEditor ed = new SchemaEditor();
            PropertyType slot = ed.CreatePropertyType("slot", DataType.String);

            SearchOrder target = new SearchOrder(slot, OrderDirection.Asc);
            //SearchOrderAccessor acc = new SearchOrderAccessor(target);

            Assert.IsTrue(target.Direction == OrderDirection.Asc, "#1");
            Assert.IsTrue(target.PropertyToOrder.IsSlot == true, "#2");
            Assert.IsTrue(Object.ReferenceEquals(target.PropertyToOrder.PropertySlot, slot), "#3");
        }
        [TestMethod()]
        public void SearchOrder_Constructor_SlotAndDirectionDesc()
        {
            SchemaEditor ed = new SchemaEditor();
            PropertyType slot = ed.CreatePropertyType("slot", DataType.String);

            SearchOrder target = new SearchOrder(slot, OrderDirection.Desc);
            //SearchOrderAccessor acc = new SearchOrderAccessor(target);

            Assert.IsTrue(target.Direction == OrderDirection.Desc, "#1");
            Assert.IsTrue(target.PropertyToOrder.IsSlot == true, "#2");
            Assert.IsTrue(Object.ReferenceEquals(target.PropertyToOrder.PropertySlot, slot), "#3");
        }

        [TestMethod()]
        public void TypeExpression_Constructor_NodeType1()
        {
            NodeType set = new SchemaEditor().CreateNodeType(null, "set");

            TypeExpression target = new TypeExpression(set);

            //TypeExpressionAccessor acc = new TypeExpressionAccessor(target);
            Assert.IsTrue(Object.ReferenceEquals(target.NodeType, set), "#1");
            Assert.IsTrue(target.ExactMatch == false, "#2");
        }
        [TestMethod()]
        public void TypeExpression_Constructor_PropertySetAndNotExactMacth()
        {
            NodeType set = new SchemaEditor().CreateNodeType(null, "set");

            TypeExpression target = new TypeExpression(set, false);

            //TypeExpressionAccessor acc = new TypeExpressionAccessor(target);
            Assert.IsTrue(Object.ReferenceEquals(target.NodeType, set), "#1");
            Assert.IsTrue(target.ExactMatch == false, "#2");
        }
        [TestMethod()]
        public void TypeExpression_Constructor_PropertySetAndExactMacth()
        {
            NodeType set = new SchemaEditor().CreateNodeType(null, "set");

            TypeExpression target = new TypeExpression(set, true);

            //TypeExpressionAccessor acc = new TypeExpressionAccessor(target);
            Assert.IsTrue(Object.ReferenceEquals(target.NodeType, set), "#1");
            Assert.IsTrue(target.ExactMatch == true, "#2");
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchExpression_Constructor_Null()
        {
            SearchExpression exp = new SearchExpression(null);
            //SearchExpressionAccessor acc = new SearchExpressionAccessor(exp);

            Assert.IsTrue(exp.FullTextExpression == "");
        }
        [TestMethod()]
        public void SearchExpression_Constructor_String()
        {
            SearchExpression exp = new SearchExpression("FullTextExpression");
            //SearchExpressionAccessor acc = new SearchExpressionAccessor(exp);

            Assert.IsTrue(exp.FullTextExpression == "FullTextExpression");
        }

        [TestMethod()]
        public void NotExpression_Constructor_String()
        {
            SearchExpression exp = new SearchExpression("FullTextExpression");
            NotExpression notExp = new NotExpression(exp);
            //NotExpressionAccessor acc = new NotExpressionAccessor(notExp);

            Assert.IsTrue(Object.ReferenceEquals(notExp.Expression, exp));
        }

        [TestMethod()]
        public void ReferenceExpression_Constuctor()
        {
            PropertyType slot = new SchemaEditor().CreatePropertyType("slot", DataType.Reference);
            SearchExpression exp = new SearchExpression("FullTextExpression");
            Node node = Repository.Root;

            ReferenceExpression refExp;

            refExp = new ReferenceExpression(ReferenceAttribute.Parent);
            refExp = new ReferenceExpression(slot);
            refExp = new ReferenceExpression(ReferenceAttribute.Parent, exp);
            refExp = new ReferenceExpression(slot, exp);
            refExp = new ReferenceExpression(ReferenceAttribute.Parent, node);
            refExp = new ReferenceExpression(slot, node);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReferenceExpression_Constuctor_WrongLeftValue()
        {
            PropertyType slot = new SchemaEditor().CreatePropertyType("slot", DataType.String);
            ReferenceExpression refExp = new ReferenceExpression(slot);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReferenceExpression_Constuctor_NullExpr()
        {
            ReferenceExpression refExp = new ReferenceExpression(ReferenceAttribute.Parent, (Expression)null);
        }

        [TestMethod()]
        public void NodeQueryTest001_XML()
        {
            NodeQuery query = new NodeQuery();
            ExpressionList orList = new ExpressionList(ChainOperator.Or);
            query.Add(new ReferenceExpression(ReferenceAttribute.Parent, orList));
            orList.Add(new StringExpression(StringAttribute.Name, StringOperator.EndsWith, ".txt"));
            orList.Add(new StringExpression(StringAttribute.Name, StringOperator.EndsWith, ".doc"));

            var nodes = query.Execute();

            Assert.IsTrue(nodes == null || nodes.Count == 0);
        }

        [TestMethod()]
        public void NodeQuery_001_SQL()
        {
            //Node.Daughter.Name = "Julia"

            NodeQuery query = new NodeQuery();
            query.Add(
                new ReferenceExpression(ActiveSchema.PropertyTypes["Daughter"],
                    new StringExpression(StringAttribute.Name, StringOperator.Equal, _julia.Name)));

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _adam.Name, _eva.Name));
        }

        [TestMethod()]
        public void NodeQuery_002_SQL()
        {
            //Node.Mother.Daughter.Name = "Julia"

            NodeQuery query = new NodeQuery();
            query.Add(
                new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                    new ReferenceExpression(ActiveSchema.PropertyTypes["Daughter"],
                        new StringExpression(StringAttribute.Name, StringOperator.Equal, _julia.Name))));

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _julia.Name, _peter.Name));
        }

        [TestMethod()]
        public void NodeQuery_003_SQL()
        {
            //Node.Mother.Husband.Name = "Adam"

            NodeQuery query = new NodeQuery();
            query.Add(
                new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                    new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                        new StringExpression(StringAttribute.Name, StringOperator.Equal, _adam.Name))));

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _julia.Name, _peter.Name));
        }

        [TestMethod()]
        public void NodeQuery_004_SQL()
        {
            //Node.Mother.Husband.Name = 'Adam'
            //And
            //Node.Father.NickName = 'ad'

            NodeQuery query = new NodeQuery();
            query.Add(
                new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                    new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                        new StringExpression(StringAttribute.Name, StringOperator.Equal, _adam.Name))));

            query.Add(
                new ReferenceExpression(ActiveSchema.PropertyTypes["Father"],
                    new StringExpression(ActiveSchema.PropertyTypes["NickName"], StringOperator.Equal, _adam.NickName)));

            var nodes = query.Execute();
            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _julia.Name, _peter.Name));
        }

        [TestMethod()]
        public void NodeQuery_005_SQL()
        {
            //Node.Mother.Husband.Name = "Adam"
            //And
            //Node.Brother.Age = 12

            NodeQuery query = new NodeQuery();
            query.Add(
                new ExpressionList(ChainOperator.And,

                    new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                        new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                            new StringExpression(StringAttribute.Name, StringOperator.Equal, _adam.Name))),

                    new ReferenceExpression(ActiveSchema.PropertyTypes["Brother"],
                        new IntExpression(ActiveSchema.PropertyTypes["Age"], ValueOperator.Equal, _peter.Age))
            ));

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _julia.Name));
        }

        [TestMethod()]
        public void NodeQuery_006_SQL()
        {
            //Node.Mother.Husband.Name = "Adam"
            //Or
            //Node.Brother.Age = 12

            NodeQuery query = new NodeQuery();
            query.Add(
                new ExpressionList(ChainOperator.Or,

                    new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                        new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                            new StringExpression(StringAttribute.Name, StringOperator.Equal, _adam.Name))),

                    new ReferenceExpression(ActiveSchema.PropertyTypes["Brother"],
                        new IntExpression(ActiveSchema.PropertyTypes["Age"], ValueOperator.Equal, _peter.Age))
            ));

            var nodes = query.Execute().Nodes.ToList();

            Assert.IsTrue(CheckNodes(nodes, _peter.Name));
            Assert.IsTrue(CheckNodes(nodes, _julia.Name));
            Assert.IsTrue(CheckNodes(nodes, _liza.Name));
        }

        [TestMethod()]
        public void NodeQuery_007_SQL()
        {
            //Node.Mother.Mother.Husband.Name = "Carl"
            //And
            //Node.Sister.Husband.Age = 25

            NodeQuery query = new NodeQuery();
            query.Add(new ExpressionList(ChainOperator.And,

                        new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                            new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                                new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                                    new StringExpression(StringAttribute.Name, StringOperator.Equal, _carl.Name)))),

                        new ReferenceExpression(ActiveSchema.PropertyTypes["Sister"],
                            new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                                new IntExpression(ActiveSchema.PropertyTypes["Age"], ValueOperator.Equal, _jack.Age)))
            ));

            var nodes = query.Execute();
            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _peter.Name));
        }

        [TestMethod()]
        public void NodeQuery_008_SQL()
        {
            //Node.Parent.CreatedBy.Name = User.Administrator.Name
            //And
            //Node.Mother.Mother.Husband.Name = "Carl"
            //And
            //Node.CreatedBy.Parent.Name = "Users"
            //And
            //Node.Sister.Housband.Age = 25

            NodeQuery query = new NodeQuery();
            query.Add(new ExpressionList(ChainOperator.And,

                        new ReferenceExpression(ReferenceAttribute.Parent,
                            new ReferenceExpression(ReferenceAttribute.CreatedBy,
                                new StringExpression(StringAttribute.Name, StringOperator.Equal, User.Administrator.Name))),

                        new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                            new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                                new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                                    new StringExpression(StringAttribute.Name, StringOperator.Equal, _carl.Name)))),

                        new ReferenceExpression(ReferenceAttribute.CreatedBy,
                            new ReferenceExpression(ReferenceAttribute.Parent,
                                new ReferenceExpression(ReferenceAttribute.CreatedBy,
                                    new StringExpression(StringAttribute.Name, StringOperator.Equal, User.Administrator.Name)))),

                        new ReferenceExpression(ActiveSchema.PropertyTypes["Sister"],
                            new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                                new IntExpression(ActiveSchema.PropertyTypes["Age"], ValueOperator.Equal, _jack.Age)))

            ));

            var nodes = query.Execute();
            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _peter.Name));
        }

        [TestMethod()]
        public void NodeQuery_009_SQL()
        {
            //Node.Mother.Mother.Name = "Helen"
            //And
            //Node.Mother.Mother.Age = 64

            NodeQuery query = new NodeQuery();
            query.Add(new ExpressionList(ChainOperator.And,
                        new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                            new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                                    new StringExpression(StringAttribute.Name, StringOperator.Equal, _helen.Name))),

                        new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                            new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                                new IntExpression(ActiveSchema.PropertyTypes["Age"], ValueOperator.Equal, _helen.Age)))
            ));

            var nodes = query.Execute();
            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _peter.Name, _julia.Name));
        }

        [TestMethod()]
        public void NodeQuery_010_SQL()
        {
            //Node.Parent.Name = Repository.Root.Name

            NodeQuery query = new NodeQuery();
            query.Add(
                new ReferenceExpression(ReferenceAttribute.Parent,
                    new ReferenceExpression(ReferenceAttribute.Parent,
                        new StringExpression(StringAttribute.Name, StringOperator.Equal, Repository.Root.Name))));

            var nodeList = query.Execute().Nodes.ToList();
            if (!CheckNodes(nodeList, _f_1_1_1.Name))
            {
                //Trace.WriteLine("Should be on the list: " + _f_1_1_1.Name);
                //Trace.WriteLine("The list:");
                //foreach (Node n in nodeList)
                //{
                //    Trace.WriteLine(n.Name);
                //}
                Assert.Fail("CheckNodes: assertation failed.");
            }
            if (!CheckNodesNotInList(nodeList, _peter.Name))
            {
                //Trace.WriteLine("Should not be on the list: " + _peter.Name);
                //foreach (Node n in nodeList)
                //{
                //    Trace.WriteLine(n.Name);
                //}
                Assert.Fail("CheckNodesNotInList: assertation failed.");
            }
        }

        [TestMethod()]
        public void NodeQuery_011_SQL()
        {
            //Node.Parent.Parent.Name = Repository.Root.Name
            //Or
            //Node.Parent.Name = "Skins"

            NodeQuery query = new NodeQuery();
            query.Add(new ExpressionList(ChainOperator.Or,

                new ReferenceExpression(ReferenceAttribute.Parent,
                    new ReferenceExpression(ReferenceAttribute.Parent,
                        new StringExpression(StringAttribute.Name, StringOperator.Equal, Repository.Root.Name))),

                new ReferenceExpression(ReferenceAttribute.Parent,
                    new StringExpression(StringAttribute.Name, StringOperator.Equal, _f_1_1.Name))

                        ));

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _f_1_1_1.Name, _f_1_1_2.Name));
        }

        //[TestMethod()]
        public void NodeQuery_012_SQL()
        {
            //Node.Parent.Parent.Name = Repository.Root.Name
            //And
            //Node.Parent.Parent.Path = Repository.Root.Path
            //And
            //Node.CreatedBy.Parent.Name = "Users"

            NodeQuery query = new NodeQuery();
            query.Add(new ExpressionList(ChainOperator.And,

                new ReferenceExpression(ReferenceAttribute.Parent,
                    new ReferenceExpression(ReferenceAttribute.Parent,
                        new StringExpression(StringAttribute.Name, StringOperator.Equal, Repository.Root.Name))),

                new ReferenceExpression(ReferenceAttribute.Parent,
                    new ReferenceExpression(ReferenceAttribute.Parent,
                        new StringExpression(StringAttribute.Path, StringOperator.Equal, Repository.Root.Path))),

                new ReferenceExpression(ReferenceAttribute.CreatedBy,
                    new ReferenceExpression(ReferenceAttribute.Parent,
                        new ReferenceExpression(ReferenceAttribute.CreatedBy,
                            new StringExpression(StringAttribute.Name, StringOperator.Equal, User.Administrator.Name))))

                        ));

            var nodeList = query.Execute().Nodes.ToList();

            Assert.IsTrue(CheckNodes(nodeList, _f_1_1_1.Name, User.Administrator.Name), "#1");
            Assert.IsTrue(CheckNodesNotInList(nodeList, _f_1_1.Name, _f_1_2.Name), "#2");
        }

        [TestMethod()]
        public void NodeQuery_013_SQL()
        {
            //Node.Parent.Parent.Name = Repository.Root.Name
            //Or
            //Node.Mother.Husband.Name = "Joe"

            NodeQuery query = new NodeQuery();
            query.Add(
                new ExpressionList(ChainOperator.Or,

                    new ReferenceExpression(ReferenceAttribute.Parent,
                        new ReferenceExpression(ReferenceAttribute.Parent,
                            new StringExpression(StringAttribute.Name, StringOperator.Equal, Repository.Root.Name))),

                    new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                        new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                            new StringExpression(StringAttribute.Name, StringOperator.Equal, _joe.Name))
                            )));

            var nodes = query.Execute().Nodes.ToList();

            if (!CheckNodes(nodes, _tom.Name, _liza.Name, _f_1_1_1.Name))
            {
                //Trace.WriteLine(string.Format("They should be on the list: '{0}', '{1}', '{2}'.", _tom.Name, _liza.Name, _f_1_1_1.Name));
                //Trace.WriteLine("The list:");
                //foreach (Node n in nodes)
                //{
                //    Trace.WriteLine(n.Name);
                //}
                Assert.Fail("CheckNodes: assertation failed.");
            }
            if (!CheckNodesNotInList(nodes, _peter.Name, _f_1_1.Name))
            {
                //Trace.WriteLine(string.Format("They should NOT be on the list: '{0}', '{1}'.\n\nThe list:", _peter.Name, _f_1_1.Name));
                //foreach (Node n in nodes)
                //{
                //    Trace.WriteLine(n.Name);
                //}
                Assert.Fail("CheckNodesNotInList: assertation failed.");
            }
        }

        [TestMethod()]
        public void NodeQuery_014_SQL()
        {
            //Node.Parent.Parent.Name = Repository.Root.Name

            NodeQuery query = new NodeQuery();
            query.Add(new ReferenceExpression(ReferenceAttribute.Parent,
                        new ReferenceExpression(ReferenceAttribute.Parent,
                            new StringExpression(StringAttribute.Name, StringOperator.Equal, Repository.Root.Name))));

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _f_1_1_1.Name));
        }

        [TestMethod()]
        public void NodeQuery_015_SQL()
        {
            //Node.Mother.Husband.Name = "Joe"

            NodeQuery query = new NodeQuery();
            query.Add(new ReferenceExpression(ActiveSchema.PropertyTypes["Mother"],
                        new ReferenceExpression(ActiveSchema.PropertyTypes["Husband"],
                            new StringExpression(StringAttribute.Name, StringOperator.Equal, "Joe"))
                            ));

            var nodes = query.Execute().Nodes.ToList();

            Assert.IsTrue(CheckNodes(nodes, _tom.Name, _liza.Name));
            Assert.IsTrue(CheckNodesNotInList(nodes, _peter.Name, _julia.Name, _f_1_2.Name));
        }

        [TestMethod()]
        public void NodeQuery_016_SQL()
        {
            //Node.CreatedBy.Name = User.Current.Name
            //And
            //Node.Parent.Parent.Name = Repository.Root.Name

            NodeQuery query = new NodeQuery();
            query.Add(
                new ExpressionList(ChainOperator.And,

                    //new ReferenceExpression(ReferenceAttribute.CreatedBy, new StringExpression(StringAttribute.Name, StringOperator.Equal, User.Current.Name)),
                    new ReferenceExpression(ReferenceAttribute.CreatedBy, new IntExpression(IntAttribute.Id, ValueOperator.Equal, User.Current.Id)),

                    new ReferenceExpression(ReferenceAttribute.Parent,
                        new ReferenceExpression(ReferenceAttribute.Parent,
                            new StringExpression(StringAttribute.Name, StringOperator.Equal, Repository.Root.Name)))
                )
            );

            var nodes = query.Execute().Nodes.ToList();

            Assert.IsTrue(CheckNodes(nodes, _f_1_1_1.Name, _f_1_1_2.Name, _f_1_2_1.Name, _f_1_2_2.Name));
            Assert.IsTrue(CheckNodesNotInList(nodes, _f_1_1.Name, _f_1_2.Name, _f_1_3.Name, _f_1_4.Name));
        }

        [TestMethod()]
        public void NodeQuery_017_SQL()
        {
            //Node.CreatedBy.Name = "Xyz"
            //Or
            //Node.Father.Name = "Joe"

            NodeQuery query = new NodeQuery();
            query.Add(
                new ExpressionList(ChainOperator.Or,

                    new ReferenceExpression(ReferenceAttribute.CreatedBy,
                        new StringExpression(StringAttribute.Name, StringOperator.Equal, "Xyz")),

                    new ReferenceExpression(ActiveSchema.PropertyTypes["Father"],
                        new StringExpression(StringAttribute.Name, StringOperator.Equal, _joe.Name))

            ));

            var nodes = query.Execute().Nodes.ToList();

            Assert.IsTrue(CheckNodes(nodes, _tom.Name, _liza.Name));
            Assert.IsTrue(CheckNodesNotInList(nodes, _peter.Name, _julia.Name));
        }

        [TestMethod()]
        public void NodeQuery_018_Simple_SQL()
        {
            NodeQuery query = CreateComplexQuery();

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _adam.Name, _eva.Name));
            Assert.IsTrue(nodes.Count == 2);
        }

        //[TestMethod()]
        //public void NodeQuery_019_Search_SQL()
        //{
        //    NodeQuery query = CreateComplexQuery();

        //    query.Add(new SearchExpression("Ad*"));

        //    var nodes = query.Execute();

        //    Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _adam.Name));
        //    Assert.IsTrue(nodes.Count == 1);
        //}

        [TestMethod()]
        public void NodeQuery_020_SearchOrder_SQL()
        {
            NodeQuery query = CreateComplexQuery();

            query.Add(new SearchExpression("Ad*"));

            query.Orders.Add(new SearchOrder(StringAttribute.Name, OrderDirection.Desc));

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _adam.Name));
            Assert.IsTrue(nodes.Count == 1);
        }

        [TestMethod()]
        public void NodeQuery_021_SearchPaging_SQL()
        {
            NodeQuery query = CreateComplexQuery();

            query.Add(new SearchExpression("Ad*"));

            query.PageSize = 10;
            query.StartIndex = 1;

            var nodes = query.Execute();

            var countOk = nodes.Count == 1;
            var nodesOk = CheckNodes(nodes.Nodes.ToList(), _adam.Name);

            var repeat = 0;
            while ((!countOk || !nodesOk) && repeat++ < 10)
            {
                //Trace.WriteLine("********************** DELAY ********************* NodeQuery_021_SearchPaging_SQL: " + repeat);
                Thread.Sleep(1000);
                nodes = query.Execute();

                countOk = nodes.Count == 1;
                nodesOk = CheckNodes(nodes.Nodes.ToList(), _adam.Name);

            }
            string msg = string.Empty;
            if (!countOk || !nodesOk)
                msg = String.Concat("Result list: [", String.Join(", ", (from node in nodes.Nodes select node.Name).ToArray()), "]. Expected list: [", _adam.Name, "]");

            Assert.IsTrue(countOk, msg);
            Assert.IsTrue(nodesOk, msg);
        }

        [TestMethod()]
        public void NodeQuery_022_SearchOrderPaging_SQL()
        {
            NodeQuery query = CreateComplexQuery();

            query.Add(new SearchExpression("Ad*"));

            query.Orders.Add(new SearchOrder(StringAttribute.Name, OrderDirection.Desc));

            query.PageSize = 10;
            query.StartIndex = 1;

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _adam.Name), "#1");
            Assert.IsTrue(nodes.Count == 1, "#2");
        }

        [TestMethod()]
        public void NodeQuery_023_Order_SQL()
        {
            NodeQuery query = CreateComplexQuery();

            query.Orders.Add(new SearchOrder(StringAttribute.Name, OrderDirection.Desc));

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _adam.Name, _eva.Name));
            Assert.IsTrue(nodes.Count == 2);
        }

        [TestMethod()]
        public void NodeQuery_024_OrderPaging_SQL()
        {
            NodeQuery query = CreateComplexQuery();

            query.Orders.Add(new SearchOrder(StringAttribute.Name, OrderDirection.Desc));

            query.PageSize = 10;
            query.StartIndex = 1;

            var nodes = query.Execute().Nodes.ToList();

            Assert.IsTrue(nodes.Count == 2);
            Assert.IsTrue(nodes[0].Name == _eva.Name);
            Assert.IsTrue(nodes[1].Name == _adam.Name);
        }

        [TestMethod()]
        public void NodeQuery_025_Paging_SQL()
        {
            NodeQuery query = CreateComplexQuery();

            query.PageSize = 10;
            query.StartIndex = 1;

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _adam.Name, _eva.Name));
            Assert.IsTrue(nodes.Count == 2);
        }

        [TestMethod()]
        public void NodeQuery_026_VersionSecurity_SQL()
        {
            AccessProvider.Current.SetCurrentUser(User.Administrator);
            RefTestNode sec = CreateRefTestNode("SecurityQueryTestNode");
            try
            {
                sec.NickName = "Before";
                sec.Save(VersionRaising.NextMajor, VersionStatus.Approved);

                sec.VersioningMode = VersioningType.MajorAndMinor;
                sec.NickName = "After";
                sec.Save(VersionRaising.NextMinor, VersionStatus.Draft);

                Assert.IsTrue(sec.Version.Minor > 0, "#1");
                Assert.IsTrue(sec.NickName == "After", "#2");

                NodeQuery query = new NodeQuery();
                query.Add(new IntExpression(IntAttribute.Id, ValueOperator.Equal, sec.Id));

                AccessProvider.Current.SetCurrentUser(User.Visitor);

                var nodes = query.Execute().Nodes.ToList();

                Assert.IsTrue((nodes[0] as RefTestNode).NickName == "Before", "#3");
            }
            finally
            {
                AccessProvider.Current.SetCurrentUser(User.Administrator);
                sec.ForceDelete();
            }
        }

        [TestMethod()]
        public void NodeQuery_027_VersionSecurity_SQL()
        {
            AccessProvider.Current.SetCurrentUser(User.Administrator);

            RefTestNode son = CreateRefTestNode("RTNSon");
            son.VersioningMode = VersioningType.MajorAndMinor;
            son.Save(VersionRaising.NextMajor, VersionStatus.Approved);
            son.Save(VersionRaising.NextMinor, VersionStatus.Draft);

            RefTestNode father = CreateRefTestNode("RTNFather");
            father.VersioningMode = VersioningType.MajorAndMinor;

            father.Save(VersionRaising.NextMajor, VersionStatus.Approved);

            father.Son = son;
            father.Save(VersionRaising.NextMinor, VersionStatus.Draft);

            try
            {
                NodeQuery query = new NodeQuery();
                query.Add(new ReferenceExpression(ActiveSchema.PropertyTypes["Son"],
                            new StringExpression(StringAttribute.Name, StringOperator.Equal, son.Name)));

                AccessProvider.Current.SetCurrentUser(User.Visitor);
                var guestNodes = query.Execute();
                Assert.IsTrue(guestNodes == null || guestNodes.Count == 0, "#1");

                AccessProvider.Current.SetCurrentUser(User.Administrator);
                var adminNodes = query.Execute();
                Assert.IsTrue(adminNodes.Nodes.First().Id == father.Id, "#2");
            }
            finally
            {
                AccessProvider.Current.SetCurrentUser(User.Administrator);
                if (son != null) son.ForceDelete();
                if (father != null) father.ForceDelete();
            }
        }

        //[TestMethod()]
        //public void NodeQuery_028_VersionSecuritySearch_SQL()
        //{
        //    AccessProvider.Current.SetCurrentUser(User.Administrator);
        //    RefTestNode test = CreateRefTestNode("RTNVerSecSearch");
        //    test.VersioningMode = VersioningType.MajorAndMinor;
        //    test.SaveAsNextMajorVersion();

        //    test.NickName = "RTNVerSecSearchNick";
        //    test.SaveAsNextMinorVersion();

        //    NodeQuery query = new NodeQuery();
        //    query.Add(new SearchExpression("RTNVerSecSearchNick*"));

        //    Thread.Sleep(10000);

        //    try
        //    {
        //        AccessProvider.Current.SetCurrentUser(User.Visitor);
        //        var guestNodes = query.Execute();
        //        Assert.IsTrue(guestNodes == null || guestNodes.Count == 0, "#1");

        //        AccessProvider.Current.SetCurrentUser(User.Administrator);
        //        var adminNodes = query.Execute();
        //        Assert.IsTrue(adminNodes != null && adminNodes.Count > 0 && adminNodes[0].Id == test.Id, "#2");
        //    }
        //    finally
        //    {
        //        AccessProvider.Current.SetCurrentUser(User.Administrator);
        //        if (test != null) test.Delete();
        //    }
        //}

        [TestMethod()]
        public void NodeQuery_029_NodeAttributumIndex_SQL()
        {
            _adam.Index = 100;
            _adam.Save();

            NodeQuery query = new NodeQuery();

            query.Add(new IntExpression(IntAttribute.Index, ValueOperator.Equal, 100));
            query.Add(new StringExpression(StringAttribute.Name, StringOperator.Equal, "Adam"));

            var nodes = query.Execute();

            Assert.IsTrue(CheckNodes(nodes.Nodes.ToList(), _adam.Name));
        }

        [TestMethod()]
        public void NodeQuery_030_TypeExpression_SQL()
        {
            NodeQuery query = new NodeQuery();
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["RepositoryTest_RefTestNode"]));
            var userList = query.Execute();

            Assert.IsTrue(userList != null && userList.Count > 0);
        }

        [TestMethod()]
        public void NodeQuery_031_IndexAttr_SQL()
        {
            NodeQuery query = new NodeQuery();
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["RepositoryTest_RefTestNode"]));
            query.Orders.Add(new SearchOrder(IntAttribute.Index, OrderDirection.Asc));
            var userList = query.Execute();
            Assert.IsTrue(userList != null && userList.Count > 0);
        }

        //[TestMethod()]
        //public void NodeQuery_032_IndexAttrPaging_SQL()
        //{
        //    NodeQuery query = new NodeQuery();
        //    /*query.Add(new TypeExpression(ActiveSchema.NodeTypes["RepositoryTest_RefTestNode"]));
        //    query.Orders.Add(new SearchOrder(IntAttribute.Index, OrderDirection.Asc));
        //    query.PageSize = 10;
        //    query.StartIndex = 0;*/
        //    query.Add(new IntExpression(IntAttribute.Locked, ValueOperator.Equal, 1));

        //    var userList = query.Execute();
        //    Assert.IsTrue(userList != null && userList.Count > 0);
        //}

        [TestMethod()]
        public void NodeQuery_033_OrderCustomProperty_SQL()
        {
            NodeQuery query = new NodeQuery();
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["RepositoryTest_RefTestNode"]));

            query.Orders.Add(new SearchOrder(ActiveSchema.PropertyTypes["NickName"], OrderDirection.Desc));

            var userList = query.Execute();

            Assert.IsTrue(userList != null && userList.Count > 0);
        }

        [TestMethod()]
        public void NodeQuery_034_OrderCustomPropertyPaging_SQL()
        {
            NodeQuery query = new NodeQuery();
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["RepositoryTest_RefTestNode"]));

            query.Orders.Add(new SearchOrder(ActiveSchema.PropertyTypes["Age"], OrderDirection.Asc));
            query.Orders.Add(new SearchOrder(ActiveSchema.PropertyTypes["NickName"], OrderDirection.Asc));

            query.PageSize = 15;
            query.StartIndex = 1;

            var userList = query.Execute();

            Assert.IsTrue(userList != null && userList.Count > 0);
        }


        [TestMethod()]
        public void NodeQuery_035_Top_SQL()
        {
            NodeQuery query = new NodeQuery();
            query.Add(new SearchExpression("Root*"));
            query.Orders.Add(new SearchOrder(StringAttribute.Name, OrderDirection.Desc));

            query.Top = 5;

            var nodeList = query.Execute();

            Assert.IsTrue(nodeList.Count == 5);
        }

        [TestMethod()]
        public void NodeQuery_SimpleSearch_Test()
        {
            NodeQuery query = new NodeQuery();

            query.Add(new StringExpression(StringAttribute.Name, StringOperator.StartsWith, "Ad"));
            query.Add(new SearchExpression("\"ad*\""));

            query.PageSize = 10000;
            query.StartIndex = 1;
            query.Orders.Add(new SearchOrder(StringAttribute.Name, OrderDirection.Desc));

            var nodeList = query.Execute().Nodes.ToList();

            var inOk = CheckNodes(nodeList, _adam.Name, User.Administrator.Name);
            var outOk = CheckNodesNotInList(nodeList, _peter.Name);

            if (!inOk || !outOk)
            {
                //Trace.WriteLine("********************** DELAY ********************* NodeQuery_SimpleSearch_Test");
                Thread.Sleep(1000);
                nodeList = query.Execute().Nodes.ToList();

                inOk = CheckNodes(nodeList, _adam.Name, User.Administrator.Name);
                outOk = CheckNodesNotInList(nodeList, _peter.Name);
            }
            string msg = string.Empty;
            if (!inOk || !outOk)
                msg = String.Concat("Result list: [", String.Join(", ", (from node in nodeList select node.Name).ToArray()), "]. Expected list: [", _adam.Name, ", ", User.Administrator.Name, "]");

            Assert.IsTrue(inOk, msg);
            Assert.IsTrue(outOk, msg);
        }

        [TestMethod]
        public void NodeQuery_Reference_Bug2275()
        {
            var query = new NodeQuery(
                new ReferenceExpression(ReferenceAttribute.CreatedBy, User.Administrator),
                new StringExpression(StringAttribute.Name, StringOperator.NotEqual, "Administrator")
                );
            var result = query.Execute();
            Assert.IsTrue(result.Count > 0);
        }

        #region Helper

        public static void InstallNodeTypes()
        {
            if (ActiveSchema.NodeTypes["RepositoryTest_RefTestNode"] == null)
            {
                ContentTypeInstaller.InstallContentType(RefTestNode.ContentTypeDefinition);
            }
        }

        private static void GenerateManyNodes(int count)
        {
            Node hasManyNodes = Node.LoadNode(string.Concat(Repository.Root.Path, "/", "ManyNodes"));
            if (hasManyNodes == null)
            {
                RefTestNode lastRtf = CreateRefTestNode("ManyNodes");
                for (int i = 0; i < count; i++)
                {
                    RefTestNode rtf = CreateRefTestNode(string.Concat("ManyNodes_", i));

                    if (i % 10 != 0)
                    {
                        rtf.Mother = lastRtf;
                        rtf.Save();
                    }

                    lastRtf = rtf;
                }
            }
        }

        Folder _f_1_1 = null;
        Folder _f_1_2 = null;
        Folder _f_1_3 = null;
        Folder _f_1_4 = null;

        Folder _f_1_1_1 = null;
        Folder _f_1_1_2 = null;

        Folder _f_1_2_1 = null;
        Folder _f_1_2_2 = null;

        RefTestNode _carl = null;
        RefTestNode _helen = null;
        RefTestNode _adam = null;
        RefTestNode _eva = null;
        RefTestNode _peter = null;
        RefTestNode _julia = null;
        RefTestNode _jack = null;

        RefTestNode _joe = null;
        RefTestNode _suzan = null;
        RefTestNode _tom = null;
        RefTestNode _liza = null;

        public void CreateTestStructure()
        {
            //-- 1. level ----------------------------------------------------------------
            _f_1_1 = CreateFolder("NodeQueryTestFolder_1_1");
            _f_1_2 = CreateFolder("NodeQueryTestFolder_1_2");
            _f_1_3 = CreateFolder("NodeQueryTestFolder_1_3");
            _f_1_4 = CreateFolder("NodeQueryTestFolder_1_4");

            //-- 2. level ----------------------------------------------------------------
            _f_1_1_1 = CreateFolder("NodeQueryTestFolder_1_1_1", _f_1_1);
            _f_1_1_2 = CreateFolder("NodeQueryTestFolder_1_1_2", _f_1_1);

            _f_1_2_1 = CreateFolder("NodeQueryTestFolder_1_2_1", _f_1_2);
            _f_1_2_2 = CreateFolder("NodeQueryTestFolder_1_2_2", _f_1_2);

            //-- RefTestNode -------------------------------------------------------------

            //Grandfather: Carl
            //Grandmother: Helen
            //Father, housband: Adam
            //Mother, wife: Eva
            //Son, brother: Peter
            //Daughter, sister: Julia

            _carl = CreateRefTestNode("Carl");
            _helen = CreateRefTestNode("Helen");
            _adam = CreateRefTestNode("Adam");
            _eva = CreateRefTestNode("Eva");
            _peter = CreateRefTestNode("Peter");
            _julia = CreateRefTestNode("Julia");
            _jack = CreateRefTestNode("Jack");

            _carl.Wife = _helen;
            //_carl.Daughter = ;
            //_carl.Son = ;
            _carl.NickName = "ca";
            _carl.Age = 65;
            _carl.Save();

            _helen.Husband = _carl;
            _helen.Daughter = _eva;
            //_helen.Son = ;
            _helen.NickName = "he";
            _helen.Age = 64;
            _helen.Save();

            _adam.Wife = _eva;
            _adam.Daughter = _julia;
            _adam.Son = _peter;
            _adam.NickName = "ad";
            _adam.Age = 44;
            _adam.Save();

            _eva.Husband = _adam;
            _eva.Daughter = _julia;
            _eva.Son = _peter;
            _eva.NickName = "ev";
            _eva.Age = 42;
            _eva.Mother = _helen;
            _eva.Save();

            _peter.Mother = _eva;
            _peter.Father = _adam;
            _peter.NickName = "pe";
            _peter.Age = 12;
            _peter.Sister = _julia;
            _peter.Save();

            _julia.Husband = _jack;
            _julia.Mother = _eva;
            _julia.Father = _adam;
            _julia.NickName = "ju";
            _julia.Age = 23;
            _julia.Brother = _peter;
            _julia.Save();

            _jack.Wife = _julia;
            _jack.NickName = "ja";
            _jack.Age = 25;
            _jack.Save();

            //Father, housband: Joe
            //Mother, wife: Suzan
            //Son, brother: Tom
            //Daughter, sister: Liza

            _joe = CreateRefTestNode("Joe");
            _suzan = CreateRefTestNode("Suzan");
            _tom = CreateRefTestNode("Tom");
            _liza = CreateRefTestNode("Liza");

            _joe.Wife = _suzan;
            _joe.Daughter = _liza;
            _joe.Son = _tom;
            _joe.NickName = "jo";
            _joe.Age = 50;
            _joe.Save();

            _suzan.Husband = _joe;
            _suzan.Daughter = _liza;
            _suzan.Son = _tom;
            _suzan.NickName = "su";
            _suzan.Age = 45;
            _suzan.Save();

            _tom.Mother = _suzan;
            _tom.Father = _joe;
            _tom.NickName = "to";
            _tom.Age = 12;
            _tom.Sister = _liza;
            _tom.Save();

            _liza.Mother = _suzan;
            _liza.Father = _joe;
            _liza.NickName = "li";
            _liza.Age = 19;
            _liza.Brother = _tom;
            _liza.Save();
        }

        //-- Test helper ---------------------------------------------------------------------

        private static NodeQuery CreateComplexQuery()
        {
            //	(
            //		Node.Doughter.Father.Name = "Adam"
            //		Or
            //		Node.Doughter.Mother.Name = "Adam"
            //	)
            //	Or
            //	(
            //		Node.Son.Father.Name = "Adam"
            //		Or
            //		Node.Son.Mother.Name = "Adam"
            //	)

            PropertyType motherSlot = ActiveSchema.PropertyTypes["Mother"];
            PropertyType fatherSlot = ActiveSchema.PropertyTypes["Father"];
            PropertyType daughterSlot = ActiveSchema.PropertyTypes["Daughter"];
            PropertyType sonSlot = ActiveSchema.PropertyTypes["Son"];

            StringExpression nameExp1 = new StringExpression(StringAttribute.Name, StringOperator.Equal, "Adam");

            ReferenceExpression refExp1 = new ReferenceExpression(fatherSlot, nameExp1);
            ReferenceExpression refExp2 = new ReferenceExpression(motherSlot, nameExp1);
            ExpressionList orList1 = new ExpressionList(ChainOperator.Or);
            orList1.Add(refExp1);
            orList1.Add(refExp2);

            ReferenceExpression refExp3 = new ReferenceExpression(daughterSlot, orList1);
            ReferenceExpression refExp4 = new ReferenceExpression(sonSlot, orList1);
            ExpressionList orList2 = new ExpressionList(ChainOperator.Or);
            orList2.Add(refExp3);
            orList2.Add(refExp4);

            ExpressionList orList3 = new ExpressionList(ChainOperator.Or);
            orList3.Add(refExp3);
            orList3.Add(refExp4);

            NodeQuery query = new NodeQuery();
            query.Add(orList3);
            return query;
        }

        //-- Create nodes --------------------------------------------------------------------

        public static RefTestNode CreateRefTestNode(string name)
        {
            RefTestNode rtn = null;
            rtn = Node.LoadNode(string.Concat(Repository.Root.Path, "/", name)) as RefTestNode;
            if (rtn == null)
            {
                rtn = new RefTestNode(Repository.Root);
                rtn.Name = name;
                rtn.Save();
            }
            return rtn;
        }

        public static RefTestNode CreateRefTestNode(string name, Node parent)
        {
            RefTestNode rtn = null;
            string path = RepositoryPath.Combine(parent.Path, name);
            rtn = Node.LoadNode(path) as RefTestNode;
            if (rtn == null)
            {
                rtn = new RefTestNode(parent);
                rtn.Name = name;
                rtn.Save();
                AddPathToDelete(path);
            }
            return rtn;
        }

        public static Folder CreateFolder(string name)
        {
            return CreateFolder(name, Repository.Root);
        }

        public static Folder CreateFolder(string name, Folder parent)
        {
            Folder folder = null;
            string path = RepositoryPath.Combine(parent.Path, name);
            folder = Node.LoadNode(path) as Folder;
            if (folder == null)
            {
                folder = new SystemFolder(parent);
                folder.Name = name;
                folder.Save();
                AddPathToDelete(path);
            }
            return folder;
        }

        //-- Node exists methods -------------------------------------------------------------

        private static string _existName = string.Empty;

        private static bool ExistNode(Node node)
        {
            return node != null && node.Name == _existName;
        }

        private static bool CheckNodes(List<Node> nodes, params string[] nameArray)
        {
            if (nameArray != null && nameArray.Length > 0)
            {
                for (int i = 0; i < nameArray.Length; i++)
                {
                    _existName = nameArray[i];
                    if (!nodes.Exists(ExistNode))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static bool CheckNodesNotInList(List<Node> nodes, params string[] nameArray)
        {
            if (nameArray != null && nameArray.Length > 0)
            {
                for (int i = 0; i < nameArray.Length; i++)
                {
                    _existName = nameArray[i];
                    if (nodes.Exists(ExistNode))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //-- Etc methods ---------------------------------------------------------------------

        private static void SwitchToSqlProvider()
        {
            Type abstractProviderType = TypeHandler.GetType("SenseNet.ContentRepository.Storage.Data.DataProvider");

            MethodInfo createDataProviderMethod = abstractProviderType.GetMethod("CreateDataProvider", BindingFlags.NonPublic | BindingFlags.Static);
            object providerInstance = createDataProviderMethod.Invoke(null, new object[] { "SqlProvider" });
            FieldInfo currentField = abstractProviderType.GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
            currentField.SetValue(null, providerInstance);
        }

        #endregion
    }
}
