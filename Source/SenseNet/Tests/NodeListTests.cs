using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Search;
using System.Diagnostics;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Tests.Security;

namespace SenseNet.ContentRepository.Tests
{
	[TestClass]
    public class NodeListTests : TestBase
	{
        Node _userFolder;
        User _testUser;

        User TestUser
        {
            get { return _testUser ?? (_testUser = PermissionTest.LoadOrCreateUser("testuser", "John Smith", UserFolder)); }
        }

        Node UserFolder
        {
            get
            {
                if (_userFolder == null)
                {
                    var userFolder = Node.LoadNode(RepositoryPath.GetParentPath(User.Administrator.Path));
                    if (userFolder == null)
                        throw new ApplicationException("UserFolder cannot be found.");

                    _userFolder = userFolder;
                }
                return _userFolder;
            }
        }

        private TestContext testContextInstance;
        public override TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

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
        [ClassInitialize]
        public static void InitializePlayground(TestContext testContext)
        {
            Repository.Root.Security.SetPermission(Group.Everyone, true, PermissionType.Open, PermissionValue.NonDefined);
        }

        [ClassCleanup]
        public static void DestroyPlayground()
        {
            foreach (var path in _pathsToDelete)
            {
                try
                {
                    var n = Node.LoadNode(path);
                    if (n != null)
                        Node.ForceDelete(path);
                }
                catch
                {
                    throw;
                }
            }
        }


        [TestMethod]
        public void NodeList_ReplaceEumerableExtensions()
        {
            var firstId = 1001;
            var skip = 10;
            var take = 15;

            var baseList = new NodeList<Node>(Enumerable.Range(firstId, 42));
            var skipedList = baseList.Skip(skip);
            var takenList = skipedList.Take(take);
            var castList = takenList.Cast<ContentType>();

            Assert.IsInstanceOfType(skipedList, typeof(NodeList<Node>), "List#1 is not an instance of NodeList<Node> but " + skipedList.GetType().FullName);
            Assert.IsInstanceOfType(takenList, typeof(NodeList<Node>), "List#2 is not an instance of NodeList<Node> but " + takenList.GetType().FullName);
            Assert.IsInstanceOfType(castList, typeof(NodeList<ContentType>), "List#3 is not an instance of NodeList<Node> but " + castList.GetType().FullName);
            Assert.IsTrue(takenList.First().Id == firstId + skip, String.Concat("First id is:", takenList.First().Id, ".Expected: ", firstId + skip));
            Assert.IsTrue(takenList.Last().Id == firstId + skip + take - 1, String.Concat("Last id is:", takenList.Last().Id, ".Expected: ", firstId + skip + take - 1));
        }

		[TestMethod]
		public void NodeList_Remove()
		{
			var list = new NodeList<Node>();
			var node = Node.LoadNode(2);

			list.Add(node);
			list.Remove(node);
			Assert.IsTrue(list.Count == 0, "#1");

			list.Add(Node.LoadNode(2));
			list.Remove(Node.LoadNode(2));
			Assert.IsTrue(list.Count == 0, "#2");

			var n1 = Node.LoadNode(2);
			var n2 = Node.LoadNode(2);
			list.Add(n1);
			list.Remove(n2);
			Assert.IsTrue(list.Count == 0, "#3");
		}

		[TestMethod]
        public void NodeList_BrokenReference()
        {
            var intList = new int[] { 1, 2, Int32.MaxValue, 4, Int32.MaxValue - 1, 6 };
            var nodeList = (NodeList<Node>)new PrivateObject(typeof(NodeList<Node>), intList).Target;
            var nodes = (IEnumerable<Node>)nodeList;

            Assert.IsTrue(nodeList.Count == 6, "#1");

            //Do not use ToList() here, because it creates 
            //an array that uses the first, wrong size. The result
            //will be a few 'null' nodes at the end of the list...
            //var list = nodes.ToList();

            var list = new List<Node>();
            foreach (var node in nodes)
                list.Add(node);

		    Assert.IsTrue(list.Count == 4, "#2");
        }

        [TestMethod]
        public void NodeList_Create_1()
        {
            var list = new NodeList<Node>(new List<int> { 1, 2, 3 });
            var result = String.Join(", ", list.Select(n => n.Id.ToString()).ToArray());
            Assert.IsTrue("1, 2, 3" == result, "Result is ", result, ". Expected: 1, 2, 3");
        }

        [TestMethod]
        public void NodeList_NoPermission()
        {
            //create a folder and switch ON versioning
            var folderPath = RepositoryPath.Combine(Repository.Root.Path, "VersionFolder");
            var folder = LoadOrCreateFolder(folderPath);
            folder.InheritableVersioningMode = InheritableVersioningType.MajorAndMinor;
            folder.Save();

            //grant only open major permission to hide draft versions
            folder.Security.SetPermission(TestUser, true, PermissionType.See, PermissionValue.Allow);
            folder.Security.SetPermission(TestUser, true, PermissionType.Open, PermissionValue.Allow);

            var feladat1Path = RepositoryPath.Combine(folderPath, "Feladat1.txt");
            var feladat2Path = RepositoryPath.Combine(folderPath, "Feladat2.txt");
            var feladat3Path = RepositoryPath.Combine(folderPath, "Feladat3.txt");
            var file1 = CreateBrandNewFile(feladat1Path);
            var file2 = CreateBrandNewFile(feladat2Path);
            var file3 = CreateBrandNewFile(feladat3Path);

            //publish 2 of 3
            file1.Publish();
            file3.Publish();

            var intList = new[] { file1.Id, file2.Id, file3.Id };

            //check with admin rights
            var nodeList = (NodeList<Node>)new PrivateObject(typeof(NodeList<Node>), intList).Target;
            var nodes = (IEnumerable<Node>)nodeList;

            Assert.IsTrue(nodeList.Count == 3, "#1 NodeList.Count");

            //DO NOT USE ToList() here, because it creates 
            //an array that uses the first, wrong size. The result
            //will be a few 'null' nodes at the end of the list...
            //var list = nodes.ToList();
            var list = new List<Node>();
            foreach (var node in nodes)
                list.Add(node);

            Assert.IsTrue(list.Count == 3, "#2 enumerated Count");

            var orig = AccessProvider.Current.GetCurrentUser();

            //switch to test user
            AccessProvider.Current.SetCurrentUser(TestUser);

            nodeList = (NodeList<Node>)new PrivateObject(typeof(NodeList<Node>), intList).Target;
            nodes = (IEnumerable<Node>)nodeList;

            //Count is still the same
            Assert.IsTrue(nodeList.Count == 3, "#3 NodeList.Count with less permissions");

            list = new List<Node>();
            foreach (var node in nodes)
                list.Add(node);

            //enumerated values are one less
            Assert.IsTrue(list.Count == 2, "#4 enumerated Count with less permissions");

            //switch back to the original user
            AccessProvider.Current.SetCurrentUser(orig);
        }

        //=============================================================================================== Helper methods

        private File CreateBrandNewFile(string path)
        {
            if (Node.Exists(path))
            {
                Node.ForceDelete(path);
            }
            return LoadOrCreateFile(path);
        }
        private File LoadOrCreateFile(string path)
        {
            AccessProvider.ChangeToSystemAccount();
            var file = Node.LoadNode(path) as File;
            AccessProvider.RestoreOriginalUser();
            if (file != null)
                return file;

            var parentPath = RepositoryPath.GetParentPath(path);
            var parentFolder = (Folder)Node.LoadNode(parentPath) ?? LoadOrCreateFolder(parentPath);

            file = new File(parentFolder)
                       {
                           Name = RepositoryPath.GetFileName(path),
                           Binary = TestTools.CreateTestBinary()
                       };
            file.Save();
            AddPathToDelete(path);

            return file;
        }

	    private static Folder LoadOrCreateFolder(string path)
	    {
	        var folder = TestTools.LoadOrCreateFolder(path);
            AddPathToDelete(path);

	        return folder;
	    }
	}
}