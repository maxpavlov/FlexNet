using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ContentRepository.Tests.Data
{
    /// <summary>
    ///This is a test class for SenseNet.ContentRepository.Storage.Data.TransactionScope and is intended
    ///to contain all SenseNet.ContentRepository.Storage.Data.TransactionScope Unit Tests
    ///</summary>
    [TestClass()]
    public class TransactionScopeTest : TestBase
    {


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
        public void TransactionScope_IsActive_PassiveTest()
        {
            Assert.AreEqual(false, TransactionScope.IsActive);
        }

        [TestMethod()]
        public void TransactionScope_IsolationLevel_PassiveTest()
        {
            Assert.AreEqual(IsolationLevel.Unspecified, TransactionScope.IsolationLevel);
        }

        [TestMethod()]
        public void TransactionScope_IsActive()
        {
            TransactionScope.Begin();
            Assert.AreEqual(true, TransactionScope.IsActive);
            TransactionScope.Rollback();
        }

        [TestMethod()]
        public void TransactionScope_IsolationLevel()
        {
            TransactionScope.Begin();
            Assert.AreEqual(IsolationLevel.ReadCommitted, TransactionScope.IsolationLevel);
            TransactionScope.Rollback();
        }

        [TestMethod()]
        public void TransactionScope_IsolationLevel_ValueTest1()
        {
            TransactionScope.Begin(IsolationLevel.Serializable);
            Assert.AreEqual(IsolationLevel.Serializable, TransactionScope.IsolationLevel);
            TransactionScope.Rollback();
        }

        [TestMethod()]
        public void TransactionScope_Commit_UseCase1()
        {
            string name = "CommitTest-" + Guid.NewGuid().ToString();
			AddPathToDelete("/Root/" + name);

            TransactionScope.Begin();

            var folder = new Folder(Repository.Root);
            folder.Name = name;
            folder.Save();

            TransactionScope.Commit();

			folder = (Folder)Node.LoadNode(folder.Id);
            Assert.IsNotNull(folder);
            Assert.AreEqual(name, folder.Name);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TransactionScope_Begin_AlreadyActive()
        {
            try
            {
                TransactionScope.Begin();
                TransactionScope.Begin();
            }
            finally
            {
                TransactionScope.Rollback();
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TransactionScope_Commit_NonActive()
        {
            TransactionScope.Commit();
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TransactionScope_Rollback_NonActive()
        {
            TransactionScope.Rollback();
        }

        [TestMethod()]
        public void TransactionScope_Rollback_UseCase1()
        {
            string name = "RollbackTest-" + Guid.NewGuid().ToString();
			AddPathToDelete("/Root/" + name);

            var folder = new Folder(Repository.Root);
            folder.Name = name;
            TransactionScope.Begin();

            folder.Save();
            int id = folder.Id;
            Assert.AreNotEqual(0, id);

            TransactionScope.Rollback();

            Assert.AreEqual(0, folder.Id, "Node.Id must be set back to 0 after a rollback event.");
			folder = (Folder)Node.LoadNode(id);
            Assert.IsNull(folder);
        }
    }
}