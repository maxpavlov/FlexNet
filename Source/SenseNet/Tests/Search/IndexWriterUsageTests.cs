using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search.Indexing;
using System.Diagnostics;
using System.Threading;

namespace SenseNet.ContentRepository.Tests.Search
{
    [TestClass]
    public class IndexWriterUsageTests : TestBase
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
        #region Accessors
        private class IndexWriterUsageAccessor : Accessor
        {
            private static PrivateType IndexWriterUsageType = new PrivateType(typeof(IndexWriterUsage));
            public static IndexWriterUsage Instance
            {
                get { return (IndexWriterUsage)IndexWriterUsageType.GetStaticField("_instance"); }
            }
 
            public IndexWriterUsageAccessor(IndexWriterUsage target) : base(target) { }
            public int RefCount { get { return (int)IndexWriterUsageType.GetStaticField("_refCount"); } }
            public bool Waiting { get { return ((IndexWriterUsage)base._target).Waiting; } }
            public AutoResetEvent Signal { get { return (AutoResetEvent)IndexWriterUsageType.GetStaticField("_signal"); } }
        }

        #endregion

        [TestMethod]
        public void IndexWriterUsage_UseFast()
        {
            var fastWriterUsage = IndexWriterUsageAccessor.Instance as FastIndexWriterUsage;
            Assert.IsNotNull(fastWriterUsage, "IndexWriterUsageAccessor.Instance is not FastIndexWriterUsage");

            var indexWriter = LuceneManager._writer;
            var fastWriterUsageAcc = new IndexWriterUsageAccessor(fastWriterUsage);

            var counter = 50;
            while (fastWriterUsageAcc.RefCount > 0 && counter-- > 0)
                System.Threading.Thread.Sleep(100);

            var refCount = fastWriterUsageAcc.RefCount;
            Assert.IsTrue(refCount == 0, String.Format("refCount is {0}, expected: 0", refCount));

            using (var wrf1 = IndexWriterFrame.Get(false))
            {
                refCount = fastWriterUsageAcc.RefCount;
                Assert.IsTrue(refCount == 1, String.Format("refCount is {0}, expected: 1", refCount));
                Assert.ReferenceEquals(indexWriter == wrf1.IndexWriter, "IndexWriter has changed #1");
                using (var wrf2 = IndexWriterFrame.Get(false))
                {
                    refCount = fastWriterUsageAcc.RefCount;
                    Assert.IsTrue(refCount == 2, String.Format("refCount is {0}, expected: 2", refCount));
                    Assert.ReferenceEquals(indexWriter == wrf1.IndexWriter, "IndexWriter has changed #2");
                }
                refCount = fastWriterUsageAcc.RefCount;
                Assert.IsTrue(refCount == 1, String.Format("refCount is {0}, expected: 1", refCount));
            }
            refCount = fastWriterUsageAcc.RefCount;
            Assert.IsTrue(refCount == 0, String.Format("refCount is {0}, expected: 0", refCount));
        }

        [TestMethod]
        public void IndexWriterUsage_GettingSafe()
        {
            var timer = new System.Timers.Timer(1000.0);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(IndexWriterUsage_GettingSafe_TimerElapsed);
            timer.Start();

            IndexWriterFrame safeFrame = null;
            try
            {
                var fastWriterUsage = IndexWriterUsageAccessor.Instance as FastIndexWriterUsage;
                Assert.IsNotNull(fastWriterUsage, "IndexWriterUsageAccessor.Instance is not FastIndexWriterUsage");

                var indexWriter = LuceneManager._writer;
                var fastWriterUsageAcc = new IndexWriterUsageAccessor(fastWriterUsage);

                var counter = 50;
                while (fastWriterUsageAcc.RefCount > 0 && counter-- > 0)
                    System.Threading.Thread.Sleep(100);

                var refCount = fastWriterUsageAcc.RefCount;
                Assert.IsTrue(refCount == 0, String.Format("refCount is {0}, expected: 0", refCount));

                _fastWriterFrames = new List<IndexWriterFrame>();
                _fastWriterFrames.Add(IndexWriterFrame.Get(false));
                _fastWriterFrames.Add(IndexWriterFrame.Get(false));
                _fastWriterFrames.Add(IndexWriterFrame.Get(false));
                _fastWriterFrames.Add(IndexWriterFrame.Get(false));
                safeFrame = IndexWriterFrame.Get(true);

                refCount = fastWriterUsageAcc.RefCount;
                Assert.IsTrue(refCount == 0, String.Format("refCount is {0}, expected: 0", refCount));
            }
            finally
            {
                if (safeFrame != null)
                    safeFrame.Dispose();

                timer.Stop();
                timer.Elapsed -= new System.Timers.ElapsedEventHandler(IndexWriterUsage_GettingSafe_TimerElapsed);
                timer.Dispose();
            }
        }
        List<IndexWriterFrame> _fastWriterFrames;
        void IndexWriterUsage_GettingSafe_TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var usage = IndexWriterUsageAccessor.Instance;
            var usageAcc = new IndexWriterUsageAccessor(usage);
            Trace.WriteLine(String.Format("@#$Test> TimerElapsed before. Usage type: {0}, Fast frame count: {1}, RefCount: {2}, Waiting: {3}",
                usage.GetType().Name, _fastWriterFrames.Count, usageAcc.RefCount, usage.Waiting));

            if (_fastWriterFrames.Count > 0)
            {
                var frame = _fastWriterFrames[0];
                _fastWriterFrames.RemoveAt(0);
                frame.Dispose();
            }
            Trace.WriteLine(String.Format("@#$Test> TimerElapsed  after. Usage type: {0}, Fast frame count: {1}, RefCount: {2}, Waiting: {3}",
                usage.GetType().Name, _fastWriterFrames.Count, usageAcc.RefCount, usage.Waiting));
        }
    }
}
