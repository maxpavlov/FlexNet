using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Security;
using System.Diagnostics;
using System.Threading;
using SenseNet.Search;
using SenseNet.ContentRepository.Storage.Search;
using System.Reflection;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        public abstract TestContext TestContext { get; set; }

        //private bool _testContextInstanceResolved;
        //private TestContext __testContextInstance;
        //private TestContext testContextInstance
        //{
        //    get
        //    {
        //        if (!_testContextInstanceResolved)
        //        {
        //            var pi = this.GetType().GetProperty("TestContext");
        //            if(pi != null)
        //                __testContextInstance = (TestContext)pi.GetValue(this, null);
        //            _testContextInstanceResolved = true;
        //        }
        //        return __testContextInstance;
        //    }
        //}

        [TestInitialize]
        public void CheckBeforeTest()
        {
            ////if (testContextInstance != null)
            ////    Trace.WriteLine("@#$Test> TEST START: " + testContextInstance.TestName + ": " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), "T:" + Thread.CurrentThread.ManagedThreadId.ToString());
            ////else
            ////    Trace.WriteLine("@#$Test> TEST START: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), "T:" + Thread.CurrentThread.ManagedThreadId.ToString());

            //Trace.WriteLine("@#$Test> TEST START: " + TestContext.TestName + ": " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), "T:" + Thread.CurrentThread.ManagedThreadId.ToString());
        }
        [TestCleanup]
        public void CheckAfterTest()
        {
            ////if (testContextInstance != null)
            ////    Trace.WriteLine("@#$Test> TEST END: " + testContextInstance.TestName + ": " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), "T:" + Thread.CurrentThread.ManagedThreadId.ToString());
            ////else
            ////    Trace.WriteLine("@#$Test> TEST END: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), "T:" + Thread.CurrentThread.ManagedThreadId.ToString());

            //Trace.WriteLine("@#$Test> TEST END: " + TestContext.TestName + ": " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), "T:" + Thread.CurrentThread.ManagedThreadId.ToString());

            //var user = AccessProvider.Current.GetCurrentUser();
            //Assert.IsNotNull(user);
            //Assert.IsTrue(user.Id == 1, String.Concat("user.Id is ", user.Id, ". Expected: 1."));

            CheckIndexConsistency();
        }

        private void CheckIndexConsistency()
        {
            if (TestContext != null)
                Trace.WriteLine("&> TEST END: " + TestContext.TestName + ": " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
            else
                Trace.WriteLine("&> TEST END: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));

            var diff = SenseNet.Search.Indexing.IntegrityChecker.Check();
            Trace.WriteLine("&> Index integrity check. Count of differences: " + diff.Count());
            foreach (var d in diff)
                Trace.WriteLine("&> DIFF " + d);

            //var q = new NodeQuery(
            //    new IntExpression(IntAttribute.Id, ValueOperator.GreaterThanOrEqual, (int)1));
            //var r1 = q.Execute(ExecutionHint.ForceRelationalEngine);
            //var r2 = q.Execute(ExecutionHint.ForceIndexedEngine);
            //Assert.IsTrue(r1.Count == r2.Count, String.Format("Inconsistent index. Node counts: sql: {0}, luc: {1}", r1.Count, r2.Count));
        }
    }
}
