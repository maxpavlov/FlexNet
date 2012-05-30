using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.AppModel;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.ContentRepository.Storage.Search;

namespace SenseNet.ContentRepository.Tests.AppModel
{
    [TestClass]
    public class EventModelTest : TestBase
    {
        #region Infrastructure
        public EventModelTest()
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

        #region TestRoot - ClassInitialize - ClassCleanup
        private static string _testRootName = "_EventModelTest";
        private static string _testRootPath = String.Concat("/Root/", _testRootName);
        private Folder _testRoot;
        private Folder TestRoot
        {
            get
            {
                if (_testRoot == null)
                {
                    _testRoot = (Folder)Node.LoadNode(_testRootPath);
                    if (_testRoot == null)
                    {
                        Folder folder = new Folder(Repository.Root);
                        folder.Name = _testRootName;
                        folder.Save();
                        _testRoot = (Folder)Node.LoadNode(_testRootPath);
                    }
                }
                return _testRoot;
            }
        }

        [ClassInitialize]
        public static void CreateSandbox(TestContext testContext)
        {
        }
        [ClassCleanup]
        public static void DestroySandbox()
        {
            Node.ForceDelete(_testRootPath);
            ContentTypeInstaller.RemoveContentType("TestRepositoryEventHandlerNode");
            ContentTypeInstaller.RemoveContentType("TestRepositoryCancelEventHandlerNode");
        }
        #endregion

        [TestMethod]
        public void AppModel_Event_OneLevel_OneEvent()
        {
            Node contextNode;
            if (ContentType.GetByName("TestRepositoryEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryEventHandlerNode.ContentTypeDefinition);
            if (ContentType.GetByName("TestRepositoryCancelEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryCancelEventHandlerNode.ContentTypeDefinition);
            ResetEvents();
            TestRepositoryEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            _eventsHistory.Clear();

            // create a context node
            EnsureNode("[TestRoot]/SimpleTest", "Car");

            // get a context node and execute an operation on the node
            // watch event, assert: event is not fired
            contextNode = LoadNode("[TestRoot]/SimpleTest");
            contextNode.Index += 1;
            contextNode.Save();
            var eventsCount1 = _eventsHistory.Count; // == 0
            _eventsHistory.Clear();

            // create an event handler node and execute an operation on the context node
            // watch event, assert: event is fired
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1", "TestRepositoryCancelEventHandlerNode");
            contextNode = LoadNode("[TestRoot]/SimpleTest");
            contextNode.Index += 1;
            contextNode.Save();
            var eventsCount2 = _eventsHistory.Count; // == 1
            _eventsHistory.Clear();

            // remove the created event handler node and execute an operation on the context node
            // watch event, assert: event is not fired
            DeleteNode("[TestRoot]/Events/Car/Modifying");
            contextNode = LoadNode("[TestRoot]/SimpleTest");
            contextNode.Index += 1;
            contextNode.Save();
            var eventsCount3 = _eventsHistory.Count; // == 0
            _eventsHistory.Clear();

            TestRepositoryEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);

            Assert.IsTrue(eventsCount1 == 0, "#1");
            Assert.IsTrue(eventsCount2 == 1, "Event was not fired");
            Assert.IsTrue(eventsCount3 == 0, "Event was fired");
        }
        [TestMethod]
        public void AppModel_Event_OneLevel_MoreEvent()
        {
            Node contextNode;
            if (ContentType.GetByName("TestRepositoryEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryEventHandlerNode.ContentTypeDefinition);
            if (ContentType.GetByName("TestRepositoryCancelEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryCancelEventHandlerNode.ContentTypeDefinition);
            ResetEvents();
            TestRepositoryEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            _eventsHistory.Clear();

            // create a context node
            EnsureNode("[TestRoot]/SimpleTest", "Car");

            // get a context node and execute an operation on the node
            // watch event, assert: event is not fired
            contextNode = LoadNode("[TestRoot]/SimpleTest");
            contextNode.Index += 1;
            contextNode.Save();
            var eventsCount1 = _eventsHistory.Count; // == 0
            _eventsHistory.Clear();

            // create an event handler node and execute an operation on the context node
            // watch event, assert: event is fired
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler5", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler3", "TestRepositoryCancelEventHandlerNode");
            contextNode = LoadNode("[TestRoot]/SimpleTest");
            contextNode.Index += 1;
            contextNode.Save();
            var eventsCount2 = _eventsHistory.Count; // == 3
            var savedHistory = new List<string>(_eventsHistory);
            _eventsHistory.Clear();

            // remove the created event handler node and execute an operation on the context node
            // watch event, assert: event is not fired
            DeleteNode("[TestRoot]/Events/Car/Modifying");
            contextNode = LoadNode("[TestRoot]/SimpleTest");
            contextNode.Index += 1;
            contextNode.Save();
            var eventsCount3 = _eventsHistory.Count; // == 0
            _eventsHistory.Clear();

            TestRepositoryEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);

            Assert.IsTrue(eventsCount1 == 0, "#1");
            Assert.IsTrue(eventsCount2 == 3, "Events were not fired");
            Assert.IsTrue(eventsCount3 == 0, "Events were fired");
        }
        [TestMethod]
        public void AppModel_Event_EventOrderOnOneLevel()
        {
            Node contextNode;
            if (ContentType.GetByName("TestRepositoryEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryEventHandlerNode.ContentTypeDefinition);
            if (ContentType.GetByName("TestRepositoryCancelEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryCancelEventHandlerNode.ContentTypeDefinition);
            ResetEvents();
            TestRepositoryEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            _eventsHistory.Clear();

            // create a context node
            EnsureNode("[TestRoot]/SimpleTest", "Car");

            // create an event handler node and execute an operation on the context node
            // watch event, assert: event is fired
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler5", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler3", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler2", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler4", "TestRepositoryCancelEventHandlerNode");
            contextNode = LoadNode("[TestRoot]/SimpleTest");
            contextNode.Index += 1;
            contextNode.Save();
            var savedHistory = GetHistory();
            _eventsHistory.Clear();

            TestRepositoryEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);

            Assert.IsTrue(savedHistory == "1, 2, 3, 4, 5", String.Format("history is '{0}'. Expected: '1, 2, 3, 4, 5'", savedHistory));
        }
        [TestMethod]
        public void AppModel_Event_MoreLevel()
        {
            Node contextNode;
            if (ContentType.GetByName("TestRepositoryEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryEventHandlerNode.ContentTypeDefinition);
            if (ContentType.GetByName("TestRepositoryCancelEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryCancelEventHandlerNode.ContentTypeDefinition);
            ResetEvents();
            TestRepositoryEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            _eventsHistory.Clear();

            // create a context node
            EnsureNode("[TestRoot]/Level1/Level2/Car1", "Car");

            // create event handler nodes and execute an operation on the context node
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler5", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler2", "TestRepositoryCancelEventHandlerNode"); // 2, 5
            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");
            contextNode.Index += 1;
            contextNode.Save();
            var savedHistory1 = GetHistory();
            _eventsHistory.Clear();

            // create more event handler nodes and execute an operation on the context node
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler3", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler4", "TestRepositoryCancelEventHandlerNode"); // 2, 4, 1, 3, 5, 
            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");
            contextNode.Index += 1;
            contextNode.Save();
            var savedHistory2 = GetHistory();
            _eventsHistory.Clear();

            // remove some event handler nodes and execute an operation on the context node
            DeleteNode("[TestRoot]/Events/Car/Modifying/EventHandler1");
            DeleteNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler2"); // 3, 5, 4
            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");
            contextNode.Index += 1;
            contextNode.Save();
            var savedHistory3 = GetHistory();
            _eventsHistory.Clear();

            // remove more event handler nodes and execute an operation on the context node
            DeleteNode("[TestRoot]/Events/Car/Modifying/EventHandler3");
            DeleteNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler4"); // 5
            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");
            contextNode.Index += 1;
            contextNode.Save();
            var savedHistory4 = GetHistory();
            _eventsHistory.Clear();

            // remove all event handler nodes and execute an operation on the context node
            DeleteNode("[TestRoot]/Events/Car/Modifying/EventHandler5"); // --
            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");
            contextNode.Index += 1;
            contextNode.Save();
            var savedHistory5 = GetHistory();
            _eventsHistory.Clear();

            TestRepositoryEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);

            Assert.IsTrue(savedHistory1 == "2, 5", String.Format("#1a: history is '{0}'. Expected: '2, 5'", savedHistory1));
            Assert.IsTrue(savedHistory2 == "2, 4, 1, 3, 5", String.Format("#2a: history is '{0}'. Expected: '2, 4, 1, 3, 5'", savedHistory2));
            Assert.IsTrue(savedHistory3 == "4, 3, 5", String.Format("#3a: history is '{0}'. Expected: '4, 3, 5'", savedHistory3));
            Assert.IsTrue(savedHistory4 == "5", String.Format("#4a: history is '{0}'. Expected: '5'", savedHistory4));
            Assert.IsTrue(savedHistory5 == "", String.Format("#5a: history is '{0}'. Expected: ''", savedHistory5));
        }
        [TestMethod]
        public void AppModel_Event_Exceptions()
        {
            Node contextNode;
            if (ContentType.GetByName("TestRepositoryEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryEventHandlerNode.ContentTypeDefinition);
            if (ContentType.GetByName("TestRepositoryCancelEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryCancelEventHandlerNode.ContentTypeDefinition);
            ResetEvents();
            TestRepositoryEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            _eventsHistory.Clear();

            // create a context node
            EnsureNode("[TestRoot]/Level1/Level2/Car1", "Car");

            // create more event handler nodes and execute an operation on the context node
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler5", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler6Ex", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler2", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler1NullRefEx", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler3", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler3AppEx", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler4", "TestRepositoryCancelEventHandlerNode"); // 1NullRefEx, 2, 3AppEx, 4, 6Ex, 5, 1, 3
            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");
            contextNode.Index += 1;
            RepositoryEventException thrownExc = null;
            try
            {
                contextNode.Save();
            }
            catch (RepositoryEventException e)
            {
                thrownExc = e;
            }
            var savedHistory1 = GetHistory();
            _eventsHistory.Clear();

            TestRepositoryEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);

            Assert.IsTrue(savedHistory1 == "1NullRefEx, 2, 3AppEx, 4, 6Ex, 1, 3, 5", String.Format("history is '{0}'. Expected: '1NullRefEx, 2, 3AppEx, 4, 6Ex, 1, 3, 5'", savedHistory1));
            Assert.IsNotNull(thrownExc, "Exception was not thrown");
            var exList = thrownExc.Exceptions.ToList();
            Assert.IsTrue(exList.Count == 3, String.Format("{0} exception was thrown. Expected: 3", thrownExc.Exceptions.Count()));
            Assert.IsTrue(exList[0].Message == "NullRefEx", "#ex1");
            Assert.IsTrue(exList[1].Message == "AppEx", "#ex2");
            Assert.IsTrue(exList[2].Message == "Ex", "#ex3");
        }
        [TestMethod]
        public void AppModel_Event_ValidTypes()
        {
            Node contextNode;
            if (ContentType.GetByName("TestRepositoryEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryEventHandlerNode.ContentTypeDefinition);
            if (ContentType.GetByName("TestRepositoryCancelEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryCancelEventHandlerNode.ContentTypeDefinition);
            ResetEvents();

            // create a context node
            EnsureNode("[TestRoot]/SimpleTest", "Car");

            // create an invalid cancel event handler node
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1", "TestRepositoryEventHandlerNode");
            contextNode = LoadNode("[TestRoot]/SimpleTest");
            contextNode.Index += 1;
            try
            {
                contextNode.Save();
                Assert.Fail("Exception was not thrown #1");
            }
            catch (InvalidCastException)
            {
            }

            // create an invalid event handler node
            ResetEvents();
            EnsureNode("[TestRoot]/Events/Car/Modified/EventHandler1", "TestRepositoryCancelEventHandlerNode");
            contextNode = LoadNode("[TestRoot]/SimpleTest");
            contextNode.Index += 1;
            try
            {
                contextNode.Save();
                Assert.Fail("Exception was not thrown #2");
            }
            catch (InvalidCastException)
            {
            }
        }
        [TestMethod]
        public void AppModel_Event_Handled()
        {
            Node contextNode;
            if (ContentType.GetByName("TestRepositoryEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryEventHandlerNode.ContentTypeDefinition);
            if (ContentType.GetByName("TestRepositoryCancelEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryCancelEventHandlerNode.ContentTypeDefinition);
            ResetEvents();
            TestRepositoryEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            _eventsHistory.Clear();

            // create a context node
            EnsureNode("[TestRoot]/Level1/Level2/Car1", "Car");

            // create event handler nodes and execute an operation on the context node
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler3", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler5", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler2", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler4", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1Handled", "TestRepositoryCancelEventHandlerNode"); // 2, 4, 1, 1Handled, 3, 5
            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");
            contextNode.Index += 1;
            contextNode.Save();
            var savedHistory1 = GetHistory();
            _eventsHistory.Clear();

            TestRepositoryEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);

            Assert.IsTrue(savedHistory1 == "2, 4, 1, 1Handled", String.Format("#1a: history is '{0}'. Expected: '2, 4, 1, 1Handled'", savedHistory1));
        }
        [TestMethod]
        public void AppModel_Event_Cancelled()
        {
            Node contextNode;
            if (ContentType.GetByName("TestRepositoryEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryEventHandlerNode.ContentTypeDefinition);
            if (ContentType.GetByName("TestRepositoryCancelEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryCancelEventHandlerNode.ContentTypeDefinition);
            ResetEvents();
            TestRepositoryEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            _eventsHistory.Clear();

            // create a context node
            EnsureNode("[TestRoot]/Level1/Level2/Car1", "Car");

            // create event handler nodes and execute an operation on the context node
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler3", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler5", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler2", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler4", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler1Cancel", "TestRepositoryCancelEventHandlerNode"); // 2, 4, 1, 1Handled, 3, 5
            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");
            var originalIndex = contextNode.Index;
            contextNode.Index += 1;
            bool thrown = false;
            try
            {
                contextNode.Save();
            }
            catch (CancelNodeEventException e)
            {
                thrown = true;
            }
            var savedHistory1 = GetHistory();
            _eventsHistory.Clear();
            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");

            TestRepositoryEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);

            Assert.IsTrue(thrown, "RepositoryEventCancelledException was not thrown");
            Assert.IsTrue(savedHistory1 == "2, 4, 1, 1Cancel", String.Format("History is '{0}'. Expected: '2, 4, 1, 1Cancel'", savedHistory1));
            Assert.IsTrue(contextNode.Index == originalIndex, "Content was saved but an event was cancelled.");
        }
        [TestMethod]
        public void AppModel_Event_Bubbling()
        {
            Node contextNode;
            if (ContentType.GetByName("TestRepositoryEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryEventHandlerNode.ContentTypeDefinition);
            if (ContentType.GetByName("TestRepositoryCancelEventHandlerNode") == null)
                ContentTypeInstaller.InstallContentType(TestRepositoryCancelEventHandlerNode.ContentTypeDefinition);
            ResetEvents();
            TestRepositoryEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired += new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            _eventsHistory.Clear();

            // create a context node
            EnsureNode("[TestRoot]/Level1/Level2/Car1", "Car");

            // create event handler nodes and execute an operation on the context node
            EnsureNode("[TestRoot]/Level1/Level2/Events/Car/Modifying/EventHandler1", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Level2/Events/Car/Modifying/EventHandler2", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler3", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler4", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler5", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modifying/EventHandler6", "TestRepositoryCancelEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Level2/Events/Car/Modified/EventHandler7", "TestRepositoryEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Level2/Events/Car/Modified/EventHandler8", "TestRepositoryEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modified/EventHandler9", "TestRepositoryEventHandlerNode");
            EnsureNode("[TestRoot]/Level1/Events/Car/Modified/EventHandlerA", "TestRepositoryEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modified/EventHandlerB", "TestRepositoryEventHandlerNode");
            EnsureNode("[TestRoot]/Events/Car/Modified/EventHandlerC", "TestRepositoryEventHandlerNode");

            var eh = LoadNode("[TestRoot]/Level1/Events/Car/Modifying/EventHandler3") as RepositoryCancelEventHandler;
            eh.StopEventBubbling = true;
            eh.Save();

            contextNode = LoadNode("[TestRoot]/Level1/Level2/Car1");
            contextNode.Index += 1;
            contextNode.Save();
            var expectedHistory = "1, 2, 3, 4, 7, 8, 9, A, B, C";
            var savedHistory = GetHistory();
            _eventsHistory.Clear();

            TestRepositoryEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);
            TestRepositoryCancelEventHandlerNode.RepositoryEventFired -= new EventHandler<TestRepositoryEventArgs>(TestRepositoryEventHandlerNode_RepositoryEventFired);

            Assert.IsTrue(savedHistory == expectedHistory, String.Format("History is '{0}'. Expected: '{1}'", savedHistory, expectedHistory));
        }

        private List<string> _eventsHistory = new List<string>();
        void TestRepositoryEventHandlerNode_RepositoryEventFired(object sender, TestRepositoryEventArgs e)
        {
            var senderNode = (Node)sender ;
            _eventsHistory.Add(senderNode.Path);
        }

        //==========================================================================================

        private string GetHistory()
        {
            return string.Join(", ", (from item in _eventsHistory select item.Substring(item.IndexOf("EventHandler") + "EventHandler".Length)).ToArray());
        }

        private void ResetEvents()
        {
            var q = new NodeQuery(
                new StringExpression(StringAttribute.Path, StringOperator.StartsWith, TestRoot.Path + "/"),
                new StringExpression(StringAttribute.Name, StringOperator.Equal, "Events"));
            q.Orders.Add(new SearchOrder(StringAttribute.Path, OrderDirection.Desc));
            foreach (var node in q.Execute().Nodes)
               node.ForceDelete();
        }
        private void EnsureNode(string encodedPath, string typeName)
        {
            string path = DecodePath(encodedPath);
            if (Node.Exists(path))
                return;

            string name = RepositoryPath.GetFileName(path);
            string parentPath = RepositoryPath.GetParentPath(path);
            EnsureNode(parentPath, "Folder");

            CreateNode(parentPath, name, typeName);
        }
        private Node LoadNode(string encodedPath)
        {
            return Node.LoadNode(DecodePath(encodedPath));
        }
        private void CreateNode(string parentPath, string name, string typeName)
        {
            Content parent = Content.Load(parentPath);
            Content content = Content.CreateNew(typeName, parent.ContentHandler, name);
            content.Save();
        }
        private void DeleteNode(string encodedPath)
        {
            var node = Node.LoadNode(DecodePath(encodedPath));
            if (node != null)
                node.ForceDelete();
        }
        private string DecodePath(string encodedPath)
        {
            return encodedPath.Replace("[TestRoot]", this.TestRoot.Path);
        }
    }

    //----

    [ContentHandler]
    public class TestRepositoryEventHandlerNode : RepositoryEventHandler
    {
        public static string ContentTypeDefinition = @"<ContentType name='TestRepositoryEventHandlerNode' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.AppModel.TestRepositoryEventHandlerNode' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' />";

        public TestRepositoryEventHandlerNode(Node parent) : this(parent, "TestRepositoryEventHandler") { }
        public TestRepositoryEventHandlerNode(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected TestRepositoryEventHandlerNode(NodeToken nt) : base(nt) { }

        [RepositoryProperty("EnableEventBubbling", RepositoryDataType.Int)]
        public override bool StopEventBubbling
        {
            get { return this.GetProperty<int>("EnableEventBubbling") != 0; }
            set { this["EnableEventBubbling"] = value ? 1 : 0; }
        }

        public override void HandleEvent(object sender, RepositoryEventArgs e)
        {
            if (this.Name.EndsWith("AppEx"))
                throw new ApplicationException("AppEx");
            if (this.Name.EndsWith("NullRefEx"))
                throw new NullReferenceException("NullRefEx");
            if (this.Name.EndsWith("Ex"))
                throw new Exception("Ex");

            if (this.Name.EndsWith("Handled"))
            {
                e.Handled = true;
                return;
            }

            if (RepositoryEventFired != null)
                RepositoryEventFired(this, new TestRepositoryEventArgs { ContextPath = e.ContextNode.Path });
        }

        public static event EventHandler<TestRepositoryEventArgs> RepositoryEventFired;
    }

    [ContentHandler]
    public class TestRepositoryCancelEventHandlerNode : RepositoryCancelEventHandler
    {
        public static string ContentTypeDefinition = @"<ContentType name='TestRepositoryCancelEventHandlerNode' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.AppModel.TestRepositoryCancelEventHandlerNode' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition' />";

        public TestRepositoryCancelEventHandlerNode(Node parent) : this(parent, "TestRepositoryCancelEventHandlerNode") { }
        public TestRepositoryCancelEventHandlerNode(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected TestRepositoryCancelEventHandlerNode(NodeToken nt) : base(nt) { }

        [RepositoryProperty("EnableEventBubbling", RepositoryDataType.Int)]
        public override bool StopEventBubbling
        {
            get { return this.GetProperty<int>("EnableEventBubbling") != 0; }
            set { this["EnableEventBubbling"] = value ? 1 : 0; }
        }

        public override void HandleEvent(object sender, RepositoryCancelEventArgs e)
        {
            if (RepositoryEventFired != null)
                RepositoryEventFired(this, new TestRepositoryEventArgs { ContextPath = e.ContextNode.Path });

            if (this.Name.EndsWith("AppEx"))
                throw new ApplicationException("AppEx");
            if (this.Name.EndsWith("NullRefEx"))
                throw new NullReferenceException("NullRefEx");
            if (this.Name.EndsWith("Ex"))
                throw new Exception("Ex");

            if (this.Name.EndsWith("Cancel"))
            {
                e.Cancel = true;
                e.CancelMessage = this.Name;
                return;
            }
            if (this.Name.EndsWith("Handled"))
            {
                e.Handled = true;
                return;
            }
        }

        public static event EventHandler<TestRepositoryEventArgs> RepositoryEventFired;
    }

    //public delegate void TestRepositoryEventHandler(object sender, TestRepositoryEventArgs e);
    public class TestRepositoryEventArgs : EventArgs
    {
        public string ContextPath { get; set; }
    }

}
