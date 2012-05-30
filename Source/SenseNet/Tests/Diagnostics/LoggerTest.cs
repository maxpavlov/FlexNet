using SenseNet.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Tests
{
    /// <summary>
    ///This is a test class for LoggerTest and is intended
    ///to contain all LoggerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LoggerTest : TestBase
    {
        private TestContext testContextInstance;
        public override TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
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

        ///// <summary>
        /////A test for TraceOperation
        /////</summary>
        //[TestMethod()]
        //public void TraceOperationTest()
        //{
        //    using (var traceOperation = Logger.TraceOperation("MyOperation"))
        //    //alternative: using (new OperationTrace("MyOperation"))
        //    {
        //        //do operation
        //    }
        //    //string name = string.Empty; // TODO: Initialize to an appropriate value
        //    //OperationTrace expected = null; // TODO: Initialize to an appropriate value
        //    //OperationTrace actual;
        //    //actual = Logger.TraceOperation(name);
        //    //Assert.AreEqual(expected, actual);
        //    //Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        //[TestMethod]
        //public void TraceOperationWithTryFinally()
        //{
        //    var opTrace = new OperationTrace("MyOp");
        //    try
        //    {
        //        //do op
        //    }
        //    finally
        //    {
        //        opTrace.Finish();
        //    }
        //}

        ///// <summary>
        /////A test for Write
        /////</summary>
        //[TestMethod()]
        //public void WriteTest5()
        //{
        //    object message = "message"; // TODO: Initialize to an appropriate value
        //    Logger.Write(message);
        //    //Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Write
        /////</summary>
        //[TestMethod()]
        //public void WriteTest4()
        //{
        //    object message = "Null value encountered"; // TODO: Initialize to an appropriate value
        //    ICollection<string> categories = new[] {"Publish","Storage" }; // TODO: Initialize to an appropriate value
        //    TraceEventType severity = TraceEventType.Critical; // TODO: Initialize to an appropriate value
        //    Logger.Write(message, categories, severity);
        //}

        ///// <summary>
        /////A test for Write
        /////</summary>
        //[TestMethod()]
        //public void WriteTest3()
        //{
        //    object message = null; // TODO: Initialize to an appropriate value
        //    string category = string.Empty; // TODO: Initialize to an appropriate value
        //    Logger.Write(message, category);
        //}

        ///// <summary>
        /////A test for Write
        /////</summary>
        //[TestMethod()]
        //public void WriteTest2()
        //{
        //    object message = null; // TODO: Initialize to an appropriate value
        //    ICollection<string> categories = null; // TODO: Initialize to an appropriate value
        //    Logger.Write(message, categories);
        //}

        ///// <summary>
        /////A test for Write
        /////</summary>
        //[TestMethod()]
        //public void WriteTest1()
        //{
        //    object message = "message"; // TODO: Initialize to an appropriate value
        //    ICollection<string> categories = Logger.EmptyCategoryList; // TODO: Initialize to an appropriate value
        //    int priority = Logger.DefaultPriority; // TODO: Initialize to an appropriate value
        //    int eventId = Logger.DefaultEventId; // TODO: Initialize to an appropriate value
        //    TraceEventType severity = Logger.DefaultSeverity; // TODO: Initialize to an appropriate value
        //    string title = string.Empty; // TODO: Initialize to an appropriate value
        //    IDictionary<string, object> properties = null; // TODO: Initialize to an appropriate value
        //    Logger.Write(message, categories, priority, eventId, severity, title, properties);
        //}

        ///// <summary>
        /////A test for Write
        /////</summary>
        //[TestMethod()]
        //public void WriteException()
        //{
        //    Exception exception = null; // TODO: Initialize to an appropriate value
        //    Logger.Write(exception);
        //}
    }
}
