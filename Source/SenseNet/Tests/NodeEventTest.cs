using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.ContentRepository.Tests.ContentHandlers;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository.Tests.Storage
{
	public class TestObserver : NodeObserver
	{
		private static TestObserver _instance;
		public static TestObserver Instance
		{
			get { return _instance; }
		}

		public TestObserver()
		{
			TestObserver._instance = this;
		}

		StringBuilder _log = new StringBuilder();

		public static void ResetLog()
		{
			if (_instance == null)
				return;
			_instance._log.Length = 0;
		}
		public static string GetLog()
		{
			if (_instance == null)
				return String.Empty;
			return _instance._log.ToString();
		}

		protected override void OnNodeCreating(object sender, CancellableNodeEventArgs e)
		{
			_log.Append("TestObserver.OnNodeCreating").Append(Environment.NewLine);
		}
		protected override void OnNodeCreated(object sender, NodeEventArgs e)
		{
			_log.Append("TestObserver.OnNodeCreated").Append(Environment.NewLine);
		}
		protected override void OnNodeModifying(object sender, CancellableNodeEventArgs e)
		{
			_log.Append("TestObserver.OnNodeModifying").Append(Environment.NewLine);
		}
		protected override void OnNodeModified(object sender, NodeEventArgs e)
		{
			_log.Append("TestObserver.OnNodeModified").Append(Environment.NewLine);
		}
	}

	[TestClass]
    public class NodeEventTest : TestBase
	{
		public NodeEventTest()
		{
		}

        private TestContext testContextInstance;
        public override TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
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

		private static string _testRootName = "_NodeEventTests";
		private static string __testRootPath = String.Concat("/Root/", _testRootName);
		private Node __testRoot;
		private Node _testRoot
		{
			get
			{
				if (__testRoot == null)
				{
					__testRoot = Node.LoadNode(__testRootPath);
					if (__testRoot == null)
					{
						Node node = NodeType.CreateInstance("SystemFolder", Node.LoadNode("/Root"));
						node.Name = _testRootName;
						node.Save();
						__testRoot = Node.LoadNode(__testRootPath);
					}
				}
				return __testRoot;
			}
		}
        [ClassCleanup]
        public static void DestroyPlayGround()
        {
            if (Node.Exists(__testRootPath))
                Node.ForceDelete(__testRootPath);

            if (ActiveSchema.NodeTypes[EventTestNode.DefaultNodeTypeName] != null)
                ContentTypeInstaller.RemoveContentType(ContentType.GetByName(EventTestNode.DefaultNodeTypeName));
        }

		private StringBuilder _eventLog;

		[TestMethod]
		public void NodeEvent_CreatingModifying()
		{
			ContentType.GetByName("Folder");

			Node node = NodeType.CreateInstance("Folder", _testRoot);

			node.Creating += new CancellableNodeEventHandler(Node_Creating);
			node.Created += new EventHandler<NodeEventArgs>(Node_Created);
			node.Modifying += new CancellableNodeEventHandler(Node_Modifying);
			node.Modified += new EventHandler<NodeEventArgs>(Node_Modified);

			_eventLog = new StringBuilder();
			TestObserver.ResetLog();

			node.Save();
			node.Index = 2;
			node.Save();

			node.Creating -= new CancellableNodeEventHandler(Node_Creating);
			node.Created -= new EventHandler<NodeEventArgs>(Node_Created);
			node.Modifying -= new CancellableNodeEventHandler(Node_Modifying);
			node.Modified -= new EventHandler<NodeEventArgs>(Node_Modified);

			string cr = Environment.NewLine;
			string expectedLog = String.Concat("Node_Creating", cr, "Node_Created", cr, "Node_Modifying", cr, "Node_Modified", cr);
			string expectedStaticLog = String.Concat("TestObserver.OnNodeCreating", cr, "TestObserver.OnNodeCreated", cr, "TestObserver.OnNodeModifying", cr, "TestObserver.OnNodeModified", cr);

			Assert.IsTrue(_eventLog.ToString() == expectedLog, "#1");
			Assert.IsTrue(TestObserver.GetLog() == expectedStaticLog, "#2");
		}
		void Node_Creating(object sender, CancellableNodeEventArgs e)
		{
			LogEvent("Node_Creating");
		}
		void Node_Created(object sender, NodeEventArgs e)
		{
			LogEvent("Node_Created");
		}
		void Node_Modified(object sender, NodeEventArgs e)
		{
			LogEvent("Node_Modified");
		}
		void Node_Modifying(object sender, CancellableNodeEventArgs e)
		{
			LogEvent("Node_Modifying");
		}


		[TestMethod]
		public void NodeEvent_OverriddenCreating()
		{
			if (ActiveSchema.NodeTypes[EventTestNode.DefaultNodeTypeName] == null)
				ContentTypeInstaller.InstallContentType(EventTestNode.ContentTypeDefinition);
			_eventLog = new StringBuilder();

			Node node = NodeType.CreateInstance(EventTestNode.DefaultNodeTypeName, _testRoot);
			node.Creating += new CancellableNodeEventHandler(Node_OverriddenCreating);

			try
			{
				node.Save();
			}
			catch (CancelNodeEventException e)
			{
				LogEvent(e.Data["CancelMessage"].ToString());
			}

			node.Index = 12;
			node.Save();
			node.Creating -= new CancellableNodeEventHandler(Node_OverriddenCreating);

			string expectedLog = String.Concat("Index cannot be 0", Environment.NewLine, "Node_OverriddenCreating", Environment.NewLine);
			Assert.IsTrue(_eventLog.ToString() == expectedLog);

		}
		void Node_OverriddenCreating(object sender, CancellableNodeEventArgs e)
		{
			LogEvent("Node_OverriddenCreating");
		}


		//[TestMethod]
		//public void NodeEvent_CannotMoveContentType()
		//{
		//    string msg = "Did not throw expected CancelNodeEventException exception. ";
		//    try
		//    {
		//        Node.Move("/Root/System/Schema/ContentTypes/GenericContent/User", _testRoot.Path);
		//        Assert.Fail(msg + "#1");
		//    }
		//    catch (CancelNodeEventException) { }
		//    try
		//    {
		//        Node.Move("/Root/System/Schema/ContentTypes/GenericContent", _testRoot.Path);
		//        Assert.Fail(msg + "#2");
		//    }
		//    catch (CancelNodeEventException) { }
		//    try
		//    {
		//        Node.Move("/Root/System/Schema/ContentTypes", _testRoot.Path);
		//        Assert.Fail(msg + "#3");
		//    }
		//    catch (CancelNodeEventException) { }
		//    try
		//    {
		//        Node.Move("/Root/System/Schema", _testRoot.Path);
		//        Assert.Fail(msg + "#4");
		//    }
		//    catch (CancelNodeEventException) { }
		//    try
		//    {
		//        Node.Move("/Root/System", _testRoot.Path);
		//        Assert.Fail(msg + "#5");
		//    }
		//    catch (CancelNodeEventException) { }
		//}


		//[TestMethod]
		//public void NodeEvent_CannotMoveIntoContentTypes()
		//{
		//    SenseNet.ContentRepository.Schema.ContentType.GetByName("Folder");

		//    Node node = NodeType.CreateInstance("Folder", _testRoot);
		//    node.Name = "A";
		//    node.Save();
		//    node = NodeType.CreateInstance("Folder", node);
		//    node.Name = "B";
		//    node.Save();
		//    node = NodeType.CreateInstance("Folder", node);
		//    node.Name = "Schema";
		//    node.Save();
		//    node = NodeType.CreateInstance("Folder", node);
		//    node.Name = "ContentTypes";
		//    node.Save();
		//    node = NodeType.CreateInstance("Folder", node);
		//    node.Name = "GenericContent";
		//    node.Save();

		//    Node.Move("/Root/_NodeEventTests/A/B/Schema", "/Root/System");

		//    Assert.Inconclusive();
		//}

		private void LogEvent(string msg)
		{
			if (_eventLog == null)
				return;
			_eventLog.Append(msg).Append(Environment.NewLine);
		}
	}
}