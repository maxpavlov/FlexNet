using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Search;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.ContentRepository.Tests.Search
{
    [TestClass]
    public class QueryResultTests : TestBase
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
        #region Playground
        private static string _testRootName = "_QueryResultTests";
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
            if (_testRoot != null)
                _testRoot.ForceDelete();
        }
        #endregion

        [TestMethod]
        public void BufferedNodeEnum_WithoutInvisibles()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vv";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;
                var list = new NodeList<Node>(GetAllNodeIds());

                var currentSet = new List<int>();
                foreach (var node in list)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = visibility.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void BufferedNodeEnum_WithInvisibles_1()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv vvvv- vvv-v vv-vv v-vvv -vvv- v---- ----- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;
                var list = new NodeList<Node>(GetAllNodeIds());

                var currentSet = new List<int>();
                foreach (var node in list)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = visibility.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void BufferedNodeEnum_WithInvisibles_2()
        {
            //var visibility = "--vv- vvvv- ----- -vvvv ----- ----- -vvv- ----- --";
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv --v-- ----- -vvv- ----- ----- -vvv- ----- -v";

            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;
                var list = new NodeList<Node>(GetAllNodeIds());

                var currentSet = new List<int>();
                foreach (var node in list)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = visibility.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void QueryResult_Paging_WithoutInvisibles_FirstPage()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vv";
            var expected =   "----- vvvvv ----- ----- ----- ----- ----- ----- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37] .AUTOFILTERS:OFF", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void QueryResult_Paging_WithoutInvisibles_FirstPage_Skip0()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vv";
            var expected =   "----- vvvvv ----- ----- ----- ----- ----- ----- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37] .AUTOFILTERS:OFF", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Skip = 0, Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void QueryResult_Paging_WithoutInvisibles_ThirdPage()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vv";
            var expected =   "----- ----- ----- vvvvv ----- ----- ----- ----- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37] .AUTOFILTERS:OFF", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Skip = 10, Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void QueryResult_Paging_WithoutInvisibles_LastPage()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vv";
            var expected =   "----- ----- ----- ----- ----- ----- ----- vvv-- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37] .AUTOFILTERS:OFF", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Skip = 30, Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void QueryResult_Paging_WithoutInvisibles_OverPaging()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vvvvv vv";
            var expected =   "----- ----- ----- ----- ----- ----- ----- ----- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37]", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Skip = 100, Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }

        [TestMethod]
        public void QueryResult_Paging_WithInvisibles_FirstPage()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv v--v- -vvv- ----- ----- --vvv -vvv- v-v-- -v";
            var expected =   "----- v--v- -vvv- ----- ----- ----- ----- ----- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37] .AUTOFILTERS:OFF", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void QueryResult_Paging_WithInvisibles_SecondPage()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv v--v- -vvv- ----- ----- --vvv -vvv- v-v-- -v";
            var expected =   "----- ----- -vvv- ----- ----- --vv- ----- ----- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37] .AUTOFILTERS:OFF", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Skip = 5, Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void QueryResult_Paging_WithInvisibles_ThirdPage()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv v--v- -vvv- ----- ----- --vvv -vvv- v-v-- -v";
            var expected =   "----- ----- ----- ----- ----- --vvv -vv-- ----- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37] .AUTOFILTERS:OFF", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Skip = 10, Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void QueryResult_Paging_WithInvisibles_LastPage()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv v--v- -vvv- ----- ----- --vvv -vvv- v-v-- -v";
            var expected =   "----- ----- ----- ----- ----- ----- ----- v-v-- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37] .AUTOFILTERS:OFF", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Skip = 30, Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }
        [TestMethod]
        public void QueryResult_Paging_WithInvisibles_OverPaging()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv v--v- -vvv- ----- ----- --vvv -vvv- v-v-- -v";
            var expected = "----- ----- ----- ----- ----- ----- ----- ----- --";
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;

                var qt = String.Format("+ParentId:{0} +Type:Car +Index:[5 TO 37]", TestRoot.Id);
                var sort = new[] { new SortInfo { FieldName = "Index" } };
                var result = ContentQuery.Query(qt, new QuerySettings { Skip = 100, Top = 5, Sort = sort });
                var page = result.Nodes;
                var currentSet = new List<int>();
                foreach (var node in page)
                    currentSet.Add(node.Index);

                var currentString = IndicesToVisibility(currentSet);
                var expectedString = expected.Replace(" ", String.Empty);

                Assert.AreEqual(expectedString, currentString);

            }
            finally
            {
                User.Current = User.Administrator;
            }
        }

        [TestMethod]
        public void QueryResult_PermittedCount()
        {
            //                01234 56789 01234 56789 01234 56789 01234 56789 01
            var visibility = "vvvvv v--v- -vvv- ----- ----- --vvv -vvv- v-v-- -v";
            var expectedCount = visibility.Replace(" ", "").Replace("-", "").Length;
            EnsureStructure();
            SetBufferedNodeEnumeratorBufferSize(5);
            try
            {
                SetVisibility(visibility);
                User.Current = User.Visitor;
                var list = new NodeList<Node>(GetAllNodeIds());
                var permittedCount = list.GetPermittedCount();
                Assert.AreEqual(permittedCount, expectedCount);
            }
            finally
            {
                User.Current = User.Administrator;
            }
        }

        [TestMethod]
        public void QueryResult_SortOrder_ModifyContent()
        {
            //create parent
            var parent = new Folder(TestRoot)
                             {
                                 Name = Guid.NewGuid().ToString(),
                                 InheritableVersioningMode = InheritableVersioningType.MajorAndMinor
                             };
            parent.Save();

            //create sample content
            Content car = null;
            var expIdList = new List<int>();
            var centerId = 0;

            for (var i = 0; i < 9; i++)
            {
                car = Content.CreateNew("Car", parent, "car-" + i.ToString().PadLeft(3, '0'));
                car.Save();

                expIdList.Add(car.Id);

                if (i == 4)
                    centerId = car.Id;
            }

            var query = ContentQuery.CreateQuery(string.Format("+InTree:\"{0}\" +TypeIs:Car .AUTOFILTERS:OFF", parent.Path),
                                            new QuerySettings { Sort = new[] {new SortInfo { FieldName = "Path" }}});

            var result = query.Execute();

            var actualIdList1 = result.Identifiers.ToList();
            var actualIdList2 = result.Nodes.Select(n => n.Id).ToList();

            var expString = string.Join(",", expIdList);
            var actualString1 = string.Join(",", actualIdList1);
            var actualString2 = string.Join(",", actualIdList2);

            Assert.AreEqual(expString, actualString1, "Result.Identifiers list is different than expected #1");
            Assert.AreEqual(expString, actualString2, "Result.Nodes list is different than expected #1");

            //change version of one of the cars
            car = Content.Load(centerId);
            car.CheckOut();

            //execute the query again
            result = query.Execute();

            actualIdList1 = result.Identifiers.ToList();
            actualIdList2 = result.Nodes.Select(n => n.Id).ToList();

            expString = string.Join(",", expIdList);
            actualString1 = string.Join(",", actualIdList1);
            actualString2 = string.Join(",", actualIdList2);

            Assert.AreEqual(expString, actualString1, "Result.Identifiers list is different than expected #2");
            Assert.AreEqual(expString, actualString2, "Result.Nodes list is different than expected #2");

            parent.ForceDelete();
        }

        //==========================================================================================

        private class StorageContextAccessor : Accessor
        {
            public static StorageContextAccessor Create()
            {
                var x = new PrivateType(typeof(StorageContext));
                var y = (StorageContext)x.GetStaticProperty("Instance");
                return new StorageContextAccessor(y);
            }

            private StorageContextAccessor(StorageContext context) : base(context) { }
            public IEnumerable<int> ParseDefaultTopAndGrowth(string value)
            {
                var x = (IEnumerable<int>)base.CallPrivateMethod("ParseDefaultTopAndGrowth", value);
                return x;
            }
        }
        [TestMethod]
        public void QueryExecutor_ForceTopByConfig()
        {
            StorageContext.Search.DefaultTopAndGrowth = new[] { 10, 30, 80, 0 };
            var qt = "Type:ContentType .AUTOFILTERS:OFF";
            var sort = new[] { new SortInfo { FieldName = "Id" } };
            var result = ContentQuery.Query(qt, new QuerySettings { Skip = 0, Top = 1000, Sort = sort });
            var page = result.Nodes;
            var expectedSet = new List<int>();
            foreach (var node in page)
                expectedSet.Add(node.Id);
            var expectedStr = String.Join(", ", expectedSet);

            result = ContentQuery.Query(qt, new QuerySettings { Skip = 0, Top = 0, Sort = sort });
            page = result.Nodes;
            var currentSet = new List<int>();
            foreach (var node in page)
                currentSet.Add(node.Id);
            var currentStr = String.Join(", ", currentSet);
            
            Assert.AreEqual(expectedStr, currentStr);
            //Assert.IsTrue(currentSet.Except(expectedSet).Count() == 0, "Returned list contains unexpected ids.");
            //Assert.IsTrue(expectedSet.Except(currentSet).Count() == 0, "Expected list contains ids that were not returned.");
        }
        [TestMethod]
        public void QueryExecutor_ForceTopByConfig_CheckUsage()
        {
            var lq = LucQuery.Parse("Type:ContentType .TOP:500 .AUTOFILTERS:OFF");
            var result = lq.Execute();
            Assert.IsTrue(lq.TraceInfo.Searches == 1, String.Format("Searches is {0}, expected: 1.", lq.TraceInfo.Searches));

            StorageContext.Search.DefaultTopAndGrowth = new[] { 10, 30, 50, 0 };
            lq = LucQuery.Parse("Type:ContentType .AUTOFILTERS:OFF");
            result = lq.Execute();
            Assert.IsTrue(lq.TraceInfo.Searches == 4, String.Format("Searches is {0}, expected: 4.", lq.TraceInfo.Searches));

            StorageContext.Search.DefaultTopAndGrowth = new[] { 10, 100, 1000, 10000, 0 };
            lq = LucQuery.Parse("Type:ContentType .AUTOFILTERS:OFF");
            result = lq.Execute();
            Assert.IsTrue(lq.TraceInfo.Searches == 3, String.Format("Searches is {0}, expected: 3.", lq.TraceInfo.Searches));
        }
        [TestMethod]
        public void QueryExecutor_ParseDefaultTopAndGrowth1()
        {
            var acc = StorageContextAccessor.Create();

            var tops = acc.ParseDefaultTopAndGrowth("0").ToArray();

            Assert.IsTrue(tops.Length == 1, String.Format("#1: tops.length is {0}, expected: 1", tops.Length));
            Assert.IsTrue(tops[0] == 0, String.Format("#2: tops[0] is {0}, expected: 0", tops[0]));

            tops = acc.ParseDefaultTopAndGrowth("10").ToArray();

            Assert.IsTrue(tops.Length == 1, String.Format("#3: tops.length is {0}, expected: 1", tops.Length));
            Assert.IsTrue(tops[0] == 10, String.Format("#4: tops[0] is {0}, expected: 10", tops[0]));

            tops = acc.ParseDefaultTopAndGrowth("10, 20, 0").ToArray();

            Assert.IsTrue(tops.Length == 3, String.Format("#5: tops.length is {0}, expected: 3", tops.Length));
            Assert.IsTrue(tops[0] == 10, String.Format("#6: tops[0] is {0}, expected: 10", tops[0]));
            Assert.IsTrue(tops[1] == 20, String.Format("#7: tops[1] is {0}, expected: 20", tops[1]));
            Assert.IsTrue(tops[2] == 0, String.Format("#8: tops[2] is {0}, expected: 0", tops[2]));
        }
        [TestMethod]
        public void QueryExecutor_ParseDefaultTopAndGrowth2()
        {
            var acc = StorageContextAccessor.Create();
            try
            {
                var x = acc.ParseDefaultTopAndGrowth("-10");
                Assert.Fail("ConfigurationException was not thrown.");
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Assert.IsInstanceOfType(e.InnerException, typeof(ConfigurationException));
            }
        }
        [TestMethod]
        public void QueryExecutor_ParseDefaultTopAndGrowth3()
        {
            var acc = StorageContextAccessor.Create();
            try
            {
                var x = acc.ParseDefaultTopAndGrowth("10, 10, 0");
                Assert.Fail("ConfigurationException was not thrown.");
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Assert.IsInstanceOfType(e.InnerException, typeof(ConfigurationException));
            }
        }
        [TestMethod]
        public void QueryExecutor_ParseDefaultTopAndGrowth4()
        {
            var acc = StorageContextAccessor.Create();
            try
            {
                var x = acc.ParseDefaultTopAndGrowth("10, 0, 11");
                Assert.Fail("ConfigurationException was not thrown.");
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Assert.IsInstanceOfType(e.InnerException, typeof(ConfigurationException));
            }
        }
        [TestMethod]
        public void QueryExecutor_ParseDefaultTopAndGrowth5()
        {
            var acc = StorageContextAccessor.Create();
            try
            {
                var x = acc.ParseDefaultTopAndGrowth("0, 10, 11");
                Assert.Fail("ConfigurationException was not thrown.");
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Assert.IsInstanceOfType(e.InnerException, typeof(ConfigurationException));
            }
        }
        [TestMethod]
        public void QueryExecutor_ParseDefaultTopAndGrowth6()
        {
            var acc = StorageContextAccessor.Create();
            try
            {
                var x = acc.ParseDefaultTopAndGrowth("10, 0, 0");
                Assert.Fail("ConfigurationException was not thrown.");
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Assert.IsInstanceOfType(e.InnerException, typeof(ConfigurationException));
            }
        }
        //[TestMethod]
        //public void QueryExecutor_ForceTopByConfig_Timing()
        //{
        //    //---- warmup
        //    var lq = LucQuery.Parse("Type:ContentType .AUTOFILTERS:OFF");
        //    var result = lq.Execute();

        //    //---- one search
        //    StorageContext.Search.DefaultTopValues = new[] { 10000, 0 };
        //    result = lq.Execute();
        //    var trace = lq.TraceInfo;
        //    Assert.IsTrue(trace.Searches == 1, String.Format("Searches is {0}, expected: 1.", trace.Searches));
        //    var f1 = trace.FullExecutingTime;
        //    var k1 = trace.KernelTime;
        //    var c1 = trace.CollectingTime;
        //    var p1 = trace.PagingTime;

        //    //---- four searches
        //    StorageContext.Search.DefaultTopValues = new[] { 10, 30, 50, 0 };
        //    result = lq.Execute();
        //    trace = lq.TraceInfo;
        //    Assert.IsTrue(trace.Searches == 4, String.Format("Searches is {0}, expected: 4.", trace.Searches));
        //    var f2 = trace.FullExecutingTime;
        //    var k2 = trace.KernelTime;
        //    var c2 = trace.CollectingTime;
        //    var p2 = trace.PagingTime;

        //    //---- check
        //    var f = Math.Abs(f1 - f2);
        //    var k = Math.Abs(k1 * 3 - k2);
        //    var c = Math.Abs(c1 * 4 - c2);
        //    var p = Math.Abs(p1 - p2);
        //    Assert.IsTrue(f < f1, String.Format("f is {0}, expected: max {1}. f1: {2}, f2: {3}", f, f1, f1, f2));
        //    Assert.IsTrue(k < k1, String.Format("k is {0}, expected: max {1}. k1: {2}, k2: {3}", k, k1, k1, k2));
        //    Assert.IsTrue(c < c1, String.Format("c is {0}, expected: max {1}. c1: {2}, c2: {3}", c, c1, c1, c2));
        //    Assert.IsTrue(p < p1, String.Format("p is {0}, expected: max {1}. p1: {2}, p2: {3}", p, p1, p1, p2));
        //}

        /**/
        //==========================================================================================

        const int NODECOUNT = 42;

        private void EnsureStructure()
        {
            var car = Content.Load(RepositoryPath.Combine(TestRoot.Path, "Car0"));
            if (car != null)
                return;
            for (int i = 0; i < NODECOUNT; i++)
            {
                car = Content.CreateNew("Car", TestRoot, "Car" + i);
                car["Index"] = i;
                car.Save();
            }
        }
        void SetBufferedNodeHeadEnumeratorBufferSize(int size)
        {
            var nodeHeadResolverAcc = new PrivateType(typeof(BufferedNodeHeadEnumerator));
            nodeHeadResolverAcc.SetStaticField("BufferSize", size);
        }
        void SetBufferedNodeEnumeratorBufferSize(int size)
        {
            var nodeResolverAcc = new PrivateType(typeof(BufferedNodeEnumerator<Node>));
            nodeResolverAcc.SetStaticField("BufferSize", size);
        }
        private void SetVisibility(string visibility)
        {
            var nodes = GetAllNodes().ToArray();
            var v = visibility.Replace(" ", string.Empty);
            for (int i = 0; i < v.Length; i++)
            {
                var permValue = v[i] == 'v' ? PermissionValue.Allow : PermissionValue.Deny;
                nodes[i].Security.SetPermission(User.Visitor, true, PermissionType.See, permValue);
                nodes[i].Security.SetPermission(User.Visitor, true, PermissionType.Open, permValue);
            }
        }
        private string IndicesToVisibility(IEnumerable<int> indices)
        {
            var chars = new char[NODECOUNT];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = '-';

            foreach (var i in indices)
                chars[i] = 'v';

            return new String(chars);
        }
        private IEnumerable<int> GetAllNodeIds()
        {
            var idSet = GetAllNodeHeads().Select(h => h.Id);
            return idSet;
        }
        private IEnumerable<NodeHead> GetAllNodeHeads()
        {
            return GetAllPaths().Select(p => NodeHead.Get(p));
        }
        private IEnumerable<Node> GetAllNodes()
        {
            return GetAllPaths().Select(p => Node.LoadNode(p));
        }
        private IEnumerable<string> GetAllPaths()
        {
            for (var i = 0; i < NODECOUNT; i++)
            {
                var name = "Car" + i;
                yield return RepositoryPath.Combine(TestRoot.Path, name);
            }
        }

    }
}
