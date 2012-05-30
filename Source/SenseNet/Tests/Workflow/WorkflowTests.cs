using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using System.Threading;
using SenseNet.Workflow;
using SenseNet.Search;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Tests.Workflow
{
    public class WfWatcher
    {
        public static string _message;
        public static bool Finished { get; private set; }
        public static int Duration { get; private set; }
        public static void SendMessage(string message)
        {
            _message = message;
            Finished = true;
        }
        public static void ReceiveMessage()
        {
            _message = null;
            Finished = false;
        }

        static int _wait = 10;     // in milliseconds
        static int _timeout = 300; // in _waits
        public static bool WaitForFinished(out string message)
        {
            Duration = 0;
            while (++Duration < _timeout)
            {
                if (Finished)
                {
                    message = _message;
                    WfWatcher.ReceiveMessage();
                    return true;
                }
                Thread.Sleep(_wait);
            }
            message = _message;
            return false;
        }
    }

    [ContentHandler]
    public class WaitForMultipleContentWorkflow : WorkflowHandlerBase
    {
        public WaitForMultipleContentWorkflow(Node parent) : this(parent, null) { }
        public WaitForMultipleContentWorkflow(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected WaitForMultipleContentWorkflow(NodeToken nt) : base(nt) { }
        public static readonly string CTD = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ContentType name=""WaitForMultipleContentWorkflow""
             parentType=""Workflow""
             handler=""SenseNet.ContentRepository.Tests.Workflow.WaitForMultipleContentWorkflow""
             xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
</ContentType>
";

        private const string TRIGGERS = "Triggers";
        [RepositoryProperty(TRIGGERS, RepositoryDataType.Reference)]
        public IEnumerable<Node> Triggers
        {
            get { return base.GetReferences(TRIGGERS); }
            set { base.SetReferences(TRIGGERS, value); }
        }

        private const string WAITFORALL = "WaitForAll";
        [RepositoryProperty(WAITFORALL, RepositoryDataType.Int)]
        public bool WaitForAll
        {
            get { return base.GetProperty<int>(WAITFORALL) != 0; }
            set { base.SetProperty(WAITFORALL, value ? 1 : 0); }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case TRIGGERS:
                    return Triggers;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case TRIGGERS:
                    Triggers = (IEnumerable<Node>)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }
        public override Dictionary<string, object> CreateParameters()
        {
            var @params = base.CreateParameters();
            @params.Add("RelatedContents", this.Triggers.Select(n => new WfContent(n)).ToArray());
            @params.Add(WAITFORALL, this.WaitForAll);
            return @params;
        }
    }

    [ContentHandler]
    public class WaitForMultipleTasksWorkflow : WorkflowHandlerBase
    {
        public WaitForMultipleTasksWorkflow(Node parent) : this(parent, null) { }
        public WaitForMultipleTasksWorkflow(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected WaitForMultipleTasksWorkflow(NodeToken nt) : base(nt) { }
        public static readonly string CTD = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ContentType name=""WaitForMultipleTasksWorkflow""
             parentType=""Workflow""
             handler=""SenseNet.ContentRepository.Tests.Workflow.WaitForMultipleTasksWorkflow""
             xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
</ContentType>
";

        private const string TASKS = "Tasks";
        [RepositoryProperty(TASKS, RepositoryDataType.Reference)]
        public IEnumerable<Node> Tasks
        {
            get { return base.GetReferences(TASKS); }
            set { base.SetReferences(TASKS, value); }
        }

        private const string WAITFORALLTRUE = "WaitForAllTrue";
        [RepositoryProperty(WAITFORALLTRUE, RepositoryDataType.Int)]
        public bool WaitForAll
        {
            get { return base.GetProperty<int>(WAITFORALLTRUE) != 0; }
            set { base.SetProperty(WAITFORALLTRUE, value ? 1 : 0); }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case TASKS:
                    return Tasks;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case TASKS:
                    Tasks = (IEnumerable<Node>)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }
        public override Dictionary<string, object> CreateParameters()
        {
            var @params = base.CreateParameters();
            @params.Add(TASKS, this.Tasks.Select(n => new WfContent(n)).ToArray());
            @params.Add(WAITFORALLTRUE, this.WaitForAll);
            return @params;
        }
    }

    [ContentHandler]
    public class DelayTestWorkflow : WorkflowHandlerBase
    {
        public DelayTestWorkflow(Node parent) : this(parent, null) { }
        public DelayTestWorkflow(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected DelayTestWorkflow(NodeToken nt) : base(nt) { }
        public static readonly string CTD = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ContentType name=""DelayTestWorkflow""
             parentType=""Workflow""
             handler=""SenseNet.ContentRepository.Tests.Workflow.DelayTestWorkflow""
             xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
</ContentType>
";

        private const string TIMEOUT = "Timeout";
        [RepositoryProperty(TIMEOUT, RepositoryDataType.String)]
        public TimeSpan Timeout
        {
            get { return ParseTimeSpan(base.GetProperty<string>(TIMEOUT)); }
            set { base.SetProperty(TIMEOUT, TimeSpanToString(value)); }
        }

        private const string TESTINSTANCEID = "TestInstanceId";
        [RepositoryProperty(TESTINSTANCEID, RepositoryDataType.String)]
        public string TestInstanceId
        {
            get { return base.GetProperty<string>(TESTINSTANCEID); }
            set { base.SetProperty(TESTINSTANCEID, value); }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case TIMEOUT:
                    return Timeout;
                case TESTINSTANCEID:
                    return TestInstanceId;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case TIMEOUT:
                    Timeout = (TimeSpan)value;
                    break;
                case TESTINSTANCEID:
                    TestInstanceId = (string)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        private object TimeSpanToString(TimeSpan value)
        {
            return value.Days.ToString("00.") + value.ToString();
        }
        private TimeSpan ParseTimeSpan(string p)
        {
            try
            {
                return TimeSpan.Parse(p);
            }
            catch
            {
                return default(TimeSpan);
            }
        }

        //public override Dictionary<string, object> CreateParameters()
        //{
        //    var @params = base.CreateParameters();
        //    @params.Add("Workspace", new WfContent(this.Parent));
        //    return @params;
        //}
    }


    [TestClass]
    public class WorkflowTests : TestBase
    {
        #region test infrastructure
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
        private static string _testRootName = "_WorkflowTests";
        private static string _testRootPath = String.Concat("/Root/", _testRootName);
        /// <summary>
        /// Do not use. Instead of TestRoot property
        /// </summary>
        private Node _testRoot;
        public Node TestRoot
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
        [ClassInitialize]
        public static void InitializeWorkflows(TestContext testContext)
        {
            ContentTypeInstaller.InstallContentType(WaitForMultipleContentWorkflow.CTD, WaitForMultipleTasksWorkflow.CTD,
                DelayTestWorkflow.CTD);

            var wftrootpath = "/Root/System/Workflows";
            var wftroot = Node.LoadNode(wftrootpath);
            if (!Node.Exists(wftrootpath))
            {
                var f = Content.CreateNew("Folder", Repository.SystemFolder, "Workflows");
                f.Save();
                wftroot = f.ContentHandler;
            }
            //TODO: WFTest Use relative paths (deploy to out directory)
            InitializeWorkflow(wftroot, typeof(WaitForMultipleContentWorkflow),   @"C:\Dev10\SenseNet\Development\Budapest\Source\SenseNet\TestWorkflows\XamlFiles\WaitForMultipleTestActivity.xaml");
            InitializeWorkflow(wftroot, typeof(WaitForMultipleTasksWorkflow),     @"C:\Dev10\SenseNet\Development\Budapest\Source\SenseNet\TestWorkflows\XamlFiles\WaitForMultipleTasksWorkflow.xaml");
            InitializeWorkflow(wftroot, typeof(DelayTestWorkflow),                @"C:\Dev10\SenseNet\Development\Budapest\Source\SenseNet\TestWorkflows\XamlFiles\DelayTest.xaml");
        }
        private static void InitializeWorkflow(Node wftroot, Type wfHandlerType, string xamlPath)
        {
            if (!System.IO.File.Exists(xamlPath))
                throw new System.IO.FileNotFoundException("XAML was not found: ", xamlPath);
            var zaml = String.Empty;
            using (var reader = new System.IO.StreamReader(xamlPath))
            {
                zaml = reader.ReadToEnd();
            }
            var path = RepositoryPath.Combine(wftroot.Path, wfHandlerType.Name);
            if (Node.Exists(path))
                Node.ForceDelete(path);
            var wftcontent = Content.CreateNew("WorkflowDefinition", wftroot, wfHandlerType.Name);
            var wftfile = (File)wftcontent.ContentHandler;
            wftfile.Binary.SetStream(Tools.GetStreamFromString(zaml));
            wftcontent.Save();
        }
        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.ForceDelete(_testRootPath);
            ContentType ct;
            ct = ContentType.GetByName(typeof(WaitForMultipleContentWorkflow).Name);
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
            ct = ContentType.GetByName(typeof(WaitForMultipleTasksWorkflow).Name);
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
            ct = ContentType.GetByName(typeof(DelayTestWorkflow).Name);
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
        }
        #endregion

        [TestMethod]
        public void WF_WaitForMultipleContent_One()
        {
            Content content;
            var paths = new string[3];
            var nodes = new Node[3];
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = RepositoryPath.Combine(TestRoot.Path, "Car" + i);
                if (Node.Exists(paths[i]))
                    Node.ForceDelete(paths[i]);
                content = Content.CreateNew("Car", TestRoot, "Car" + i); content.Save();
                nodes[i] = content.ContentHandler;
            }

            var wfContent = new WaitForMultipleContentWorkflow(TestRoot);
            wfContent.Triggers = nodes;
            wfContent.WaitForAll = false;
            wfContent.Save();
            wfContent = Node.Load<WaitForMultipleContentWorkflow>(wfContent.Id);

            InstanceManager.Start(wfContent);

            var car = Node.LoadNode(paths[1]);
            car.Index++;
            car.Save();

            string msg;
            if (!WfWatcher.WaitForFinished(out msg))
                Assert.Inconclusive("#2");
            Assert.IsTrue(msg == "Finished", String.Concat("Received message: '", msg, "'. Expected: 'Finished'"));
        }
        [TestMethod]
        public void WF_WaitForMultipleContent_All()
        {
            Content content;
            var paths = new string[3];
            var nodes = new Node[3];
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = RepositoryPath.Combine(TestRoot.Path, "Car" + i);
                if (Node.Exists(paths[i]))
                    Node.ForceDelete(paths[i]);
                content = Content.CreateNew("Car", TestRoot, "Car" + i); content.Save();
                nodes[i] = content.ContentHandler;
            }

            var wfContent = new WaitForMultipleContentWorkflow(TestRoot);
            wfContent.Triggers = nodes;
            wfContent.WaitForAll = true;
            wfContent.Save();
            wfContent = Node.Load<WaitForMultipleContentWorkflow>(wfContent.Id);

            InstanceManager.Start(wfContent);

            for (int i = 0; i < paths.Length; i++)
            {
                var car = Node.LoadNode(paths[i]);
                car.Index++;
                car.Save();
            }

            string msg;
            if (!WfWatcher.WaitForFinished(out msg))
                Assert.Inconclusive("#2");
            Assert.IsTrue(msg == "Finished", String.Concat("Received message: '", msg, "'. Expected: 'Finished'"));
        }

        [TestMethod]
        public void WF_WaitForMultipleTasks_One_No()
        {
            WaitForMultipleTasksTest(3, false, "False", "-", "no");
        }
        [TestMethod]
        public void WF_WaitForMultipleTasks_One_Yes()
        {
            WaitForMultipleTasksTest(3, false, "True", "-", "yes");
        }
        [TestMethod]
        public void WF_WaitForMultipleTasks_Multi_No()
        {
            WaitForMultipleTasksTest(3, true, "False", "no");
        }
        [TestMethod]
        public void WF_WaitForMultipleTasks_Multi_YesNo()
        {
            WaitForMultipleTasksTest(3, true, "False", "yes", "no");
        }
        [TestMethod]
        public void WF_WaitForMultipleTasks_Multi_YesYesNo()
        {
            WaitForMultipleTasksTest(3, true, "False", "yes", "yes", "no");
        }
        [TestMethod]
        public void WF_WaitForMultipleTasks_Multi_YesYesYes()
        {
            WaitForMultipleTasksTest(3, true, "True", "yes", "yes", "yes");
        }
        private void WaitForMultipleTasksTest(int taskCount, bool waitForAll, string expectedMessage, params string[] taskResults)
        {
            Content content;
            var paths = new string[taskCount];
            var tasks = new Node[taskCount];
            for (int i = 0; i < paths.Length; i++)
            {
                var name = Guid.NewGuid().ToString();
                paths[i] = RepositoryPath.Combine(TestRoot.Path, name);
                content = Content.CreateNew("ApprovalWorkflowTask", TestRoot, name); content.Save();
                tasks[i] = content.ContentHandler;
            }

            var wfContent = new WaitForMultipleTasksWorkflow(TestRoot);
            wfContent.Tasks = tasks;
            wfContent.WaitForAll = waitForAll;
            wfContent.Save();
            wfContent = Node.Load<WaitForMultipleTasksWorkflow>(wfContent.Id);

            InstanceManager.Start(wfContent);

            for (int i = 0; i < taskResults.Length; i++)
            {
                if (taskResults[i] != "no" && taskResults[i] != "yes")
                    continue;
                var task = Node.LoadNode(paths[i]);
                task["Result"] = taskResults[i];
                task.Save();
            }

            string msg;
            if (!WfWatcher.WaitForFinished(out msg))
                Assert.Inconclusive("Workflow message was not received");
            Assert.IsTrue(msg == expectedMessage, String.Concat("Received message: '", msg, "'. Expected: '", expectedMessage, "'"));
        }

        [TestMethod]
        public void WF_Delay()
        {
            //if (!Node.Exists("/Root/System/WorkflowProtoTypes/DelayTest"))
            //{
            //    var prototypesFolder = Node.LoadNode("/Root/System/WorkflowProtoTypes");
            //    if (prototypesFolder == null)
            //    {
            //        prototypesFolder = Content.CreateNew("Folder", Repository.SystemFolder, "WorkflowProtoTypes").ContentHandler;
            //        prototypesFolder.Save();
            //    }
            //    var wfProto = new DelayTestWorkflow(prototypesFolder);
            //    wfProto.Name = "DelayTest";
            //    wfProto.Save();
            //}

            DelayTestWorkflow wfContent1, wfContent2;

            //-- creating workflow state contents
            wfContent1 = new DelayTestWorkflow(TestRoot);
            wfContent1.Name = "WF1";
            wfContent1.TestInstanceId = "WF1";
            wfContent1.Timeout = TimeSpan.Parse("00:00:00:10");
            wfContent1.Save();
            wfContent1 = Node.Load<DelayTestWorkflow>(wfContent1.Id);

            wfContent2 = new DelayTestWorkflow(TestRoot);
            wfContent2.Name = "WF2";
            wfContent2.TestInstanceId = "WF2";
            wfContent2.Timeout = TimeSpan.Parse("00:00:00:10");
            wfContent2.Save();
            wfContent2 = Node.Load<DelayTestWorkflow>(wfContent2.Id);

            //-- starting the workflow
            InstanceManager.Start(wfContent1);
            Thread.Sleep(10);
            InstanceManager.Start(wfContent2);

            Thread.Sleep(25 * 1000);

            wfContent1 = Node.Load<DelayTestWorkflow>(wfContent1.Id);
            Debug.WriteLine("##WF> @@@@ WorkflowStatus: " + wfContent1.WorkflowStatus);
            wfContent2 = Node.Load<DelayTestWorkflow>(wfContent2.Id);
            Debug.WriteLine("##WF> @@@@ WorkflowStatus: " + wfContent2.WorkflowStatus);

            // var result = SenseNet.Search.ContentQuery.Query("WorkflowStarted:yes .AUTOFILTERS:OFF");

            //-- checking final status
            var expectedMessage = "Finished";
            string msg;
            if (!WfWatcher.WaitForFinished(out msg))
                Assert.Inconclusive("Workflow message was not received");
            Assert.IsTrue(msg == expectedMessage, String.Concat("Received message: '", msg, "'. Expected: '", expectedMessage, "'"));
        }
        [TestMethod]
        public void WF_Delay1()
        {
            DelayTestWorkflow wfContent1;

            //-- creating workflow state contents
            wfContent1 = new DelayTestWorkflow(TestRoot);
            wfContent1.Name = "WF1";
            wfContent1.TestInstanceId = "WF1";
            wfContent1.Timeout = TimeSpan.Parse("00:00:00:10");
            wfContent1.Save();
            wfContent1 = Node.Load<DelayTestWorkflow>(wfContent1.Id);

            //-- starting the workflow
            InstanceManager.Start(wfContent1);

            InstanceManager._Poll();

            InstanceManager._Poll();

            wfContent1 = Node.Load<DelayTestWorkflow>(wfContent1.Id);
            Debug.WriteLine("##WF> @@@@ WorkflowStatus: " + wfContent1.WorkflowStatus);

            // var result = SenseNet.Search.ContentQuery.Query("WorkflowStarted:yes .AUTOFILTERS:OFF");

            //-- checking final status
            Assert.IsTrue(wfContent1.WorkflowStatus == WorkflowStatusEnum.Completed,
                String.Concat("WorkflowStatus: ", wfContent1.WorkflowStatus, ". Expected: ", WorkflowStatusEnum.Completed));
        }
    }
}
