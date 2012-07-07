using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Search;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Tests
{
	[TestClass]
    public class CopyMoveTest : TestBase
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
		private static string _testRootName = "_CopyMoveTests";
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
        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.ForceDelete(_testRootPath);
        }
        #endregion

		#region ListDefs
		private const string _listDef1 = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Cars title</DisplayName>
	<Description>Cars description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ContentListField1' type='ShortText'>
			<DisplayName>ContentListField1</DisplayName>
			<Description>ContentListField1 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ContentListField2' type='WhoAndWhen'>
			<DisplayName>ContentListField2</DisplayName>
			<Description>ContentListField2 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ContentListField3' type='ShortText'>
			<DisplayName>ContentListField3</DisplayName>
			<Description>ContentListField3 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>200</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";
		private const string _listDef2 = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Trucks title</DisplayName>
	<Description>Trucks description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ContentListField1' type='Integer' />
		<!--<ContentListField name='#ContentListField2' type='Number' />-->
	</Fields>
</ContentListDefinition>
";
		#endregion

		#region General Move tests

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Move_SourceIsNotExist()
		{
			Node.Move("/Root/osiejfvchxcidoklg6464783930020398473/iygfevfbvjvdkbu9867513125615", TestRoot.Path);
		}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Move_TargetIsNotExist()
		{
			Node.Move(TestRoot.Path, "/Root/fdgdffgfccxdxdsffcv31945581316942/udjkcmdkeieoeoodoc542364737827");
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Move_MoveTo_Null()
		{
			TestRoot.MoveTo(null);
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Move_NullSourcePath()
		{
			Node.Move(null, TestRoot.Path);
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
		public void Move_InvalidSourcePath()
		{
			Node.Move(string.Empty, TestRoot.Path);
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Move_NullTargetPath()
		{
			Node.Move(TestRoot.Path, null);
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
		public void Move_InvalidTargetPath()
		{
			Node.Move(TestRoot.Path, string.Empty);
		}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Move_ToItsParent()
		{
			MoveNode(TestRoot.Path, TestRoot.ParentPath);
		}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Move_ToItself()
		{
			Node.Move(TestRoot.Path, TestRoot.Path);
		}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Move_ToUnderItself()
		{
			EnsureNode("[TestRoot]/Source/N3");
			MoveNode("[TestRoot]/Source", "[TestRoot]/Source/N3");
		}
		[TestMethod]
		[ExpectedException(typeof(ApplicationException))]
		public void Move_TargetHasSameName()
		{
			EnsureNode("[TestRoot]/Source");
			EnsureNode("[TestRoot]/Target/Source");
			MoveNode("[TestRoot]/Source", "[TestRoot]/Target");
		}
		[TestMethod]
		public void Move_NodeTreeToNode()
		{
			EnsureNode("[TestRoot]/Source/N1/N2");
			EnsureNode("[TestRoot]/Source/N3");
			EnsureNode("[TestRoot]/Target");
			MoveNode("[TestRoot]/Source", "[TestRoot]/Target", true);
			Assert.IsNotNull(LoadNode("[TestRoot]/Target/Source/N1"), "#1");
			Assert.IsNotNull(LoadNode("[TestRoot]/Target/Source/N1/N2"), "#2");
			Assert.IsNotNull(LoadNode("[TestRoot]/Target/Source/N3"), "#3");
		}
		[TestMethod]
		public void Move_SourceIsLockedByAnother()
		{
			IUser originalUser = AccessProvider.Current.GetCurrentUser();
			IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
            TestRoot.Security.SetPermission(visitor, true, PermissionType.Save, PermissionValue.Allow);
            TestRoot.Security.SetPermission(visitor, true, PermissionType.Delete, PermissionValue.Allow);

			EnsureNode("[TestRoot]/Source/N1");
			EnsureNode("[TestRoot]/Source/N2");
			EnsureNode("[TestRoot]/Target");
			Node lockedNode = LoadNode("[TestRoot]/Source");

			AccessProvider.Current.SetCurrentUser(visitor);
            try
            {
                lockedNode.Lock.Lock();
            }
            finally
            {
                AccessProvider.Current.SetCurrentUser(originalUser);
            }

			bool expectedExceptionWasThrown = false;
			Exception thrownException = null;
			try
			{
				MoveNode("[TestRoot]/Source", "[TestRoot]/Target");
			}
			catch (LockedNodeException)
			{
				expectedExceptionWasThrown = true;
			}
			catch (Exception e)
			{
				thrownException = e;
			}

			AccessProvider.Current.SetCurrentUser(visitor);
			lockedNode.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
			AccessProvider.Current.SetCurrentUser(originalUser);

			Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		}
		[TestMethod]
		public void Move_SourceChildIsLockedByAnother()
		{
			IUser originalUser = AccessProvider.Current.GetCurrentUser();
			IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
            TestRoot.Security.SetPermission(visitor, true, PermissionType.Save, PermissionValue.Allow);
            TestRoot.Security.SetPermission(visitor, true, PermissionType.Delete, PermissionValue.Allow);

			EnsureNode("[TestRoot]/Source/N1/N2/N3");
			EnsureNode("[TestRoot]/Source/N1/N4/N5");
			Node lockedNode = LoadNode("[TestRoot]/Source/N1/N4");

			AccessProvider.Current.SetCurrentUser(visitor);
            try
            {
                lockedNode.Lock.Lock();
            }
            finally
            {
                AccessProvider.Current.SetCurrentUser(originalUser);
            }

			bool expectedExceptionWasThrown = false;
			try
			{
				lockedNode.MoveTo(TestRoot);
			}
			catch (LockedNodeException)
			{
				expectedExceptionWasThrown = true;
			}

			AccessProvider.Current.SetCurrentUser(visitor);
			lockedNode.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
			AccessProvider.Current.SetCurrentUser(originalUser);

			Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		}
		[TestMethod]
		public void Move_SourceIsLockedByCurrent()
		{
			EnsureNode("[TestRoot]/Source/N1");
			EnsureNode("[TestRoot]/Source/N2");
			EnsureNode("[TestRoot]/Target");
			var lockedNode = LoadNode("[TestRoot]/Source");
			try
			{
				lockedNode.Lock.Lock();
				MoveNode("[TestRoot]/Source", "[TestRoot]/Target");
			}
			finally
			{
                lockedNode = Node.LoadNode(lockedNode.Id);
				if(lockedNode.Lock.Locked)
					lockedNode.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
			}
			//nem jo az sql hibakod, es nem tudom, hogy kell-e egyaltalan hibat dobni
		}
		[TestMethod]
		public void Move_LockedTarget_SameUser()
		{
			EnsureNode("[TestRoot]/Source/N1/N2/N3");
			EnsureNode("[TestRoot]/Source/N1/N4/N5");
			EnsureNode("[TestRoot]/Target/N6");
			var lockedNode = LoadNode("[TestRoot]/Source/N1/N4");
			try
			{
				lockedNode.Lock.Lock();
				MoveNode("[TestRoot]/Source", "[TestRoot]/Target", true);
			}
			finally
			{
                lockedNode = Node.LoadNode(lockedNode.Id);
                if (lockedNode.Lock.Locked)
					lockedNode.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
			}
			//nem jo az sql hibakod, es nem tudom, hogy kell-e egyaltalan hibat dobni
		}
        [TestMethod]
        public void Move_PathBeforeAfter()
        {
            EnsureNode("[TestRoot]/Source/N1/N2");
            EnsureNode("[TestRoot]/Source/N1/N3");
            EnsureNode("[TestRoot]/Target");
            var n1 = LoadNode("[TestRoot]/Source/N1");
            var pathBefore = n1.Path;

            n1.MoveTo(Node.LoadNode(DecodePath("[TestRoot]/Target")));

            var pathAfter = n1.Path;

            var n2 = LoadNode("[TestRoot]/Target/N1");

            Assert.IsNotNull(n2, "#1");
            Assert.IsTrue(pathBefore != pathAfter, "#2");
            Assert.IsTrue(pathAfter == n2.Path, "#3");
        }

		[TestMethod]
		public void Move_MinimalPermissions()
		{
			IUser originalUser = AccessProvider.Current.GetCurrentUser();
			try
			{
				IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
				EnsureNode("[TestRoot]/Source");
				EnsureNode("[TestRoot]/Target");
				Node sourceNode = LoadNode("[TestRoot]/Source");
				Node targetNode = LoadNode("[TestRoot]/Target");
                sourceNode.Security.SetPermission(visitor, true, PermissionType.OpenMinor, PermissionValue.Allow);
                sourceNode.Security.SetPermission(visitor, true, PermissionType.Delete, PermissionValue.Allow);
                targetNode.Security.SetPermission(visitor, true, PermissionType.AddNew, PermissionValue.Allow);
				AccessProvider.Current.SetCurrentUser(visitor);
				MoveNode("[TestRoot]/Source", "[TestRoot]/Target", true);
			}
			finally
			{
				AccessProvider.Current.SetCurrentUser(originalUser);
			}
		}
		//[TestMethod]
		//public void Move_SourceWithoutOpenMinorPermission()
		//{
		//    IUser originalUser = AccessProvider.Current.GetCurrentUser();
		//    IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
		//    EnsureNode("[TestRoot]/Source");
		//    EnsureNode("[TestRoot]/Target");
		//    Node sourceNode = LoadNode("[TestRoot]/Source");
		//    Node targetNode = LoadNode("[TestRoot]/Target");
		//    sourceNode.Security.SetPermission(visitor, PermissionType.Delete, PermissionValue.Allow);
		//    targetNode.Security.SetPermission(visitor, PermissionType.AddNew, PermissionValue.Allow);
		//    bool expectedExceptionWasThrown = false;
		//    try
		//    {
		//        AccessProvider.Current.SetCurrentUser(visitor);
		//        MoveNode("[TestRoot]/Source", "[TestRoot]/Target");
		//    }
		//    catch (LockedNodeException)
		//    {
		//        expectedExceptionWasThrown = true;
		//    }
		//    finally
		//    {
		//        AccessProvider.Current.SetCurrentUser(originalUser);
		//    }
		//    Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		//}
		//[TestMethod]
		//public void Move_SourceWithoutDeletePermission()
		//{
		//    IUser originalUser = AccessProvider.Current.GetCurrentUser();
		//    IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
		//    EnsureNode("[TestRoot]/Source");
		//    EnsureNode("[TestRoot]/Target");
		//    Node sourceNode = LoadNode("[TestRoot]/Source");
		//    Node targetNode = LoadNode("[TestRoot]/Target");
		//    sourceNode.Security.SetPermission(visitor, PermissionType.OpenMinor, PermissionValue.Allow);
		//    targetNode.Security.SetPermission(visitor, PermissionType.AddNew, PermissionValue.Allow);
		//    bool expectedExceptionWasThrown = false;
		//    try
		//    {
		//        AccessProvider.Current.SetCurrentUser(visitor);
		//        MoveNode("[TestRoot]/Source", "[TestRoot]/Target");
		//    }
		//    catch (LockedNodeException)
		//    {
		//        expectedExceptionWasThrown = true;
		//    }
		//    finally
		//    {
		//        AccessProvider.Current.SetCurrentUser(originalUser);
		//    }
		//    Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		//}
		//[TestMethod]
		//public void Move_TargetWithoutAddNewPermission()
		//{
		//    IUser originalUser = AccessProvider.Current.GetCurrentUser();
		//    IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
		//    EnsureNode("[TestRoot]/Source");
		//    EnsureNode("[TestRoot]/Target");
		//    Node sourceNode = LoadNode("[TestRoot]/Source");
		//    Node targetNode = LoadNode("[TestRoot]/Target");
		//    sourceNode.Security.SetPermission(visitor, PermissionType.OpenMinor, PermissionValue.Allow);
		//    sourceNode.Security.SetPermission(visitor, PermissionType.Delete, PermissionValue.Allow);
		//    bool expectedExceptionWasThrown = false;
		//    try
		//    {
		//        AccessProvider.Current.SetCurrentUser(visitor);
		//        MoveNode("[TestRoot]/Source", "[TestRoot]/Target");
		//    }
		//    catch (LockedNodeException)
		//    {
		//        expectedExceptionWasThrown = true;
		//    }
		//    finally
		//    {
		//        AccessProvider.Current.SetCurrentUser(originalUser);
		//    }
		//    Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		//}
		//[TestMethod]
		//public void Move_SourceTreeWithPartialOpenMinorPermission()
		//{
		//    IUser originalUser = AccessProvider.Current.GetCurrentUser();
		//    IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
		//    EnsureNode("[TestRoot]/Source/N1/N2");
		//    EnsureNode("[TestRoot]/Source/N1/N3");
		//    EnsureNode("[TestRoot]/Source/N4");
		//    EnsureNode("[TestRoot]/Target");
		//    Node source = LoadNode("[TestRoot]/Source");
		//    Node target = LoadNode("[TestRoot]/Target");
		//    source.Security.SetPermission(visitor, PermissionType.OpenMinor, PermissionValue.Allow);
		//    source.Security.SetPermission(visitor, PermissionType.Delete, PermissionValue.Allow);
		//    target.Security.SetPermission(visitor, PermissionType.AddNew, PermissionValue.Allow);
		//    Node blockedNode = LoadNode("[TestRoot]/Source/N1/N3");
		//    blockedNode.Security.SetPermission(visitor, PermissionType.OpenMinor, PermissionValue.NonDefined);
		//    bool expectedExceptionWasThrown = false;
		//    try
		//    {
		//        AccessProvider.Current.SetCurrentUser(visitor);
		//        MoveNode("[TestRoot]/Source", "[TestRoot]/Target");
		//    }
		//    catch (LockedNodeException)
		//    {
		//        expectedExceptionWasThrown = true;
		//    }
		//    finally
		//    {
		//        AccessProvider.Current.SetCurrentUser(originalUser);
		//    }
		//    Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		//}
		//[TestMethod]
		//public void Move_SourceTreeWithPartialDeletePermission()
		//{
		//    IUser originalUser = AccessProvider.Current.GetCurrentUser();
		//    IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
		//    EnsureNode("[TestRoot]/Source/N1/N2");
		//    EnsureNode("[TestRoot]/Source/N1/N3");
		//    EnsureNode("[TestRoot]/Source/N4");
		//    EnsureNode("[TestRoot]/Target");
		//    Node source = LoadNode("[TestRoot]/Source");
		//    Node target = LoadNode("[TestRoot]/Target");
		//    source.Security.SetPermission(visitor, PermissionType.OpenMinor, PermissionValue.Allow);
		//    source.Security.SetPermission(visitor, PermissionType.Delete, PermissionValue.Allow);
		//    target.Security.SetPermission(visitor, PermissionType.AddNew, PermissionValue.Allow);
		//    Node blockedNode = LoadNode("[TestRoot]/Source/N1/N3");
		//    blockedNode.Security.SetPermission(visitor, PermissionType.Delete, PermissionValue.NonDefined);
		//    bool expectedExceptionWasThrown = false;
		//    try
		//    {
		//        AccessProvider.Current.SetCurrentUser(visitor);
		//        MoveNode("[TestRoot]/Source", "[TestRoot]/Target");
		//    }
		//    catch (LockedNodeException)
		//    {
		//        expectedExceptionWasThrown = true;
		//    }
		//    finally
		//    {
		//        AccessProvider.Current.SetCurrentUser(originalUser);
		//    }
		//    Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		//}

		//TODO: Permission upgrade test after move

		#endregion

        [TestMethod]
        public void Move_MoreVersion()
        { 
            EnsureNode("[TestRoot]/Source");
            var node = (GenericContent)LoadNode("[TestRoot]/Source");
            node.InheritableVersioningMode = ContentRepository.Versioning.InheritableVersioningType.MajorAndMinor;
            node.Save();
            EnsureNode("[TestRoot]/Source/M1");
            node = (GenericContent)LoadNode("[TestRoot]/Source/M1");
            var m1NodeId = node.Id;
            node.Index++;
            node.Save();
            node = (GenericContent)LoadNode("[TestRoot]/Source/M1");
            node.Index++;
            node.Save();
            ((GenericContent)LoadNode("[TestRoot]/Source/M1")).Publish();
            ((GenericContent)LoadNode("[TestRoot]/Source/M1")).CheckOut();
            EnsureNode("[TestRoot]/Target");

            MoveNode("[TestRoot]/Source", "[TestRoot]/Target", true);

            var result = ContentQuery.Query(String.Format("InTree:'{0}' .AUTOFILTERS:OFF", DecodePath("[TestRoot]/Target")));
            var paths = result.Nodes.Select(n => n.Path).ToArray();
            Assert.IsTrue(paths.Length == 3, String.Format("Count of paths is {0}, expected {1}", paths.Length, 3));

            var lastMajorVer = Node.LoadNode(DecodePath("/Root/_CopyMoveTests/Target/Source/M1"), VersionNumber.LastMajor).Version.ToString();
            var lastMinorVer = Node.LoadNode(DecodePath("/Root/_CopyMoveTests/Target/Source/M1"), VersionNumber.LastMinor).Version.ToString();

            Assert.IsTrue(lastMajorVer == "V1.0.A", String.Concat("LastMajor version is ", lastMajorVer, ", expected: V1.0.A"));
            Assert.IsTrue(lastMinorVer == "V1.1.L", String.Concat("LastMinor version is ", lastMinorVer, ", expected: V1.1.L"));

            var versionDump = GetVersionDumpByNodeId(m1NodeId);
            Assert.AreEqual("v0.1.d, v0.2.d, v1.0.a, v1.1.l", versionDump);
        }
        private string GetVersionDumpByNodeId(int nodeId)
        {
            var docs = SenseNet.Search.Indexing.LuceneManager.GetDocumentsByNodeId(nodeId);
            return String.Join(", ", docs.Select(d => d.Get(LucObject.FieldName.Version)).ToArray());
        }

        #region ContentList Move Tests
        [TestMethod]
		public void Move_ContentList_LeafNodeToContentList()
		{
			//1: MoveLeafNodeToContentList
			//Create [TestRoot]/SourceNode
			//Create [TestRoot]/TargetContentList
			//Move SourceNode, TargetContentList
			//Check: Node => Item
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceNode");
			EnsureNode("[TestRoot]/TargetContentList");
			MoveNode("[TestRoot]/SourceNode", "[TestRoot]/TargetContentList");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceNode");
		}
		[TestMethod]
		public void Move_ContentList_LeafNodeToContentListItem()
		{
			//2: MoveLeafNodeToContentListItem
			//Create [TestRoot]/SourceNode
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Move SourceNode, TargetItemFolder
			//Check: Node => Item
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceNode");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			MoveNode("[TestRoot]/SourceNode", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceNode");
		}
		[TestMethod]
		public void Move_ContentList_NodeTreeToContentList()
		{
			//3: MoveNodeTreeToContentList
			//Create [TestRoot]/SourceFolder/SourceNode
			//Create [TestRoot]/TargetContentList
			//Move SourceFolder, TargetContentList
			//Check: NodeTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceFolder/SourceNode");
			EnsureNode("[TestRoot]/TargetContentList");
			MoveNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetContentList");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceFolder/SourceNode");
		}
		[TestMethod]
		public void Move_ContentList_NodeTreeToContentListItem()
		{
			//4: MoveNodeTreeToContentListItem
			//Create [TestRoot]/SourceFolder/SourceNode
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Move SourceFolder, TargetItemFolder
			//Check: NodeTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceFolder/SourceNode");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			MoveNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceFolder/SourceNode");
		}
		//[TestMethod]
		//public void Move_ContentList_NodeWithContentListToNode()
		//{
		//    //5: MoveNodeWithContentListToNode
		//    //Create [TestRoot]/SourceFolder/SourceContentList/SourceContentListItem
		//    //Create [TestRoot]/TargetFolder
		//    //Move SourceFolder, TargetFolder
		//    //Check: Unchanged contentlist and item
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceFolder/SourceContentList/SourceContentListItem");
		//    EnsureNode("[TestRoot]/TargetFolder");
		//    MoveNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetFolder");
		//    CheckSimpleNode("[TestRoot]/TargetFolder/SourceFolder");
		//    CheckContentList1("[TestRoot]/TargetFolder/SourceFolder/SourceContentList");
		//    CheckContentListItem1("[TestRoot]/TargetFolder/SourceFolder/SourceContentList/SourceContentListItem");
		//}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
		public void Move_ContentList_NodeWithContentListToContentList()
		{
			//6: MoveNodeWithContentListToContentList
			//Create [TestRoot]/SourceFolder/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetContentList
			//Move SourceFolder, TargetContentList
			//Check: exception
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceFolder/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList");
			MoveNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetContentList");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
		public void Move_ContentList_NodeWithContentListToContentListItem()
		{
			//7: MoveNodeWithContentListToContentListItem
			//Create [TestRoot]/SourceFolder/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Move SourceFolder, TargetItemFolder
			//Check: exception
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceFolder/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			MoveNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetContentList/TargetItemFolder");
		}
		//[TestMethod]
		//public void Move_ContentList_ContentListToNode()
		//{
		//    //8: MoveContentListToNode
		//    //Create [TestRoot]/SourceContentList
		//    //Create [TestRoot]/TargetFolder
		//    //Move SourceContentList, TargetFolder
		//    //Check: ok
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceContentList");
		//    EnsureNode("[TestRoot]/TargetFolder");
		//    MoveNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetFolder");
		//    CheckContentList1("[TestRoot]/TargetFolder/SourceContentList");
		//}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
		public void Move_ContentList_ContentListToContentList()
		{
			//9: MoveContentListToContentList
			//Create [TestRoot]/SourceContentList
			//Create [TestRoot]/TargetContentList
			//Move SourceContentList, TargetContentList
			//Check: exception
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList");
			EnsureNode("[TestRoot]/TargetContentList");
			MoveNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetContentList");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
		public void Move_ContentList_ContentListToContentListItem()
		{
			//10: MoveContentListToContentListItem
			//Create [TestRoot]/SourceContentList
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Move SourceContentList, TargetItemFolder
			//Check: exception
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			MoveNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetContentList/TargetItemFolder");
		}
		//[TestMethod]
		//public void Move_ContentList_ContentListTreeToNode()
		//{
		//    //11: MoveContentListTreeToNode
		//    //Create [TestRoot]/SourceContentList/SourceContentListItem
		//    //Create [TestRoot]/TargetFolder
		//    //Move SourceContentList, TargetFolder
		//    //Check: ok
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
		//    EnsureNode("[TestRoot]/TargetFolder");
		//    MoveNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetFolder");
		//    CheckContentList1("[TestRoot]/TargetFolder/SourceContentList");
		//    CheckContentListItem1("[TestRoot]/TargetFolder/SourceContentList/SourceContentListItem");
		//}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Move_ContentList_ContentListTreeToContentList()
		{
			//12: MoveContentListTreeToContentList
			//Create [TestRoot]/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetContentList
			//Move SourceContentList, TargetContentList
			//Check: exception
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList");
			MoveNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetContentList");
		}
		[TestMethod]
		[ExpectedException(typeof(ApplicationException))]
		public void Move_ContentList_ContentListTreeToContentListItem()
		{
			//13: MoveContentListTreeToContentListItem
			//Create [TestRoot]/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Move SourceContentList, TargetItemFolder
			//Check: exception
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			MoveNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetContentList/TargetItemFolder");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemToNode()
		{
			//14: MoveContentListItemToNode
			//Create [TestRoot]/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetFolder
			//Move SourceContentListItem, TargetFolder
			//Check: Item => Node
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetFolder");
			MoveNode("[TestRoot]/SourceContentList/SourceContentListItem", "[TestRoot]/TargetFolder");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemToContentList()
		{
			//15: MoveContentListItemToContentList
			//Create [TestRoot]/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetContentList
			//Move SourceContentListItem, TargetContentList
			//Check: Item => Item
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList");
			MoveNode("[TestRoot]/SourceContentList/SourceContentListItem", "[TestRoot]/TargetContentList");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemToContentListItem()
		{
			//16: MoveContentListItemToContentListItem
			//Create [TestRoot]/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Move SourceContentListItem, TargetItemFolder
			//Check: Item => Item
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			MoveNode("[TestRoot]/SourceContentList/SourceContentListItem", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemTreeToNode()
		{
			//17: MoveContentListItemTreeToNode
			//Create [TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem
			//Create [TestRoot]/TargetFolder
			//Move SourceItemFolder, TargetFolder
			//Check: ItemTree => NodeTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetFolder");
			MoveNode("[TestRoot]/SourceContentList/SourceItemFolder", "[TestRoot]/TargetFolder");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceItemFolder");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceItemFolder/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemTreeToContentList()
		{
			//18: MoveContentListItemTreeToContentList
			//Create [TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem
			//Create [TestRoot]/TargetContentList
			//Move SourceItemFolder, TargetContentList
			//Check: ItemTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList");
			MoveNode("[TestRoot]/SourceContentList/SourceItemFolder", "[TestRoot]/TargetContentList");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceItemFolder/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemTreeToContentListItem()
		{
			//19: MoveContentListItemTreeToContentListItem
			//Create [TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Move SourceItemFolder, TargetItemFolder
			//Check: ItemTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			MoveNode("[TestRoot]/SourceContentList/SourceItemFolder", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceItemFolder/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemTree2ToNode()
		{
			//20: MoveContentListItemTree2ToNode
			//Create [TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
			//Create [TestRoot]/TargetFolder
			//Move SourceItemFolder2, TargetFolder
			//Check: ItemTree => NodeTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetFolder");
			MoveNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/TargetFolder");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceItemFolder2");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceItemFolder2/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemTree2ToContentList()
		{
			//21: MoveContentListItemTree2ToContentList
			//Create [TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
			//Create [TestRoot]/TargetContentList
			//Move SourceItemFolder2, TargetContentList
			//Check: ItemTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList");
			MoveNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/TargetContentList");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceItemFolder2");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceItemFolder2/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemTree2ToContentListItem()
		{
			//22: MoveContentListItemTree2ToContentListItem
			//Create [TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Move SourceItemFolder2, TargetItemFolder
			//Check: ItemTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			MoveNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceItemFolder2");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceItemFolder2/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemToSameContentList()
		{
			//23: MoveContentListItemToSameContentList
			//Create [TestRoot]/ContentList/SourceItemFolder/SourceContentListItem
			//Create [TestRoot]/ContentList
			//Move SourceContentListItem, SourceContentList
			//Check: 
			//PrepareTest();
			EnsureNode("[TestRoot]/ContentList/SourceItemFolder/SourceContentListItem");
			//EnsureNode("[TestRoot]/ContentList");
            MoveNode("[TestRoot]/ContentList/SourceItemFolder/SourceContentListItem", "[TestRoot]/ContentList");
			CheckContentListItem1("[TestRoot]/ContentList/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemToSameContentListItem()
		{
			//24: MoveContentListItemToSameContentListItem
			//Create [TestRoot]/ContentList/SourceItemFolder/SourceContentListItem
			//Create [TestRoot]/ContentList/TargetItemFolder
			//Move SourceContentListItem, TargetItemFolder
			//Check: 
			//PrepareTest();
			EnsureNode("[TestRoot]/ContentList/SourceItemFolder/SourceContentListItem");
			EnsureNode("[TestRoot]/ContentList/TargetItemFolder");
			MoveNode("[TestRoot]/ContentList/SourceItemFolder/SourceContentListItem", "[TestRoot]/ContentList/TargetItemFolder");
			CheckContentListItem1("[TestRoot]/ContentList/TargetItemFolder/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemTreeToSameContentList()
		{
			//25: MoveContentListItemTreeToSameContentList
			//Create [TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
			//Create [TestRoot]/ContentList
			//Move SourceItemFolder2, SourceContentList
			//Check: 
			//PrepareTest();
			EnsureNode("[TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
			//EnsureNode("[TestRoot]/ContentList");
            MoveNode("[TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/ContentList");
			CheckContentListItem1("[TestRoot]/ContentList/SourceItemFolder2");
			CheckContentListItem1("[TestRoot]/ContentList/SourceItemFolder2/SourceContentListItem");
		}
		[TestMethod]
		public void Move_ContentList_ContentListItemTreeToSameContentListItem()
		{
			//26: MoveContentListItemTreeToSameContentListItem
			//Create [TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
			//Create [TestRoot]/ContentList/TargetItemFolder
			//Move SourceItemFolder2, TargetItemFolder
			//Check: 
			//PrepareTest();
			EnsureNode("[TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
			EnsureNode("[TestRoot]/ContentList/TargetItemFolder");
			MoveNode("[TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/ContentList/TargetItemFolder");
			CheckContentListItem1("[TestRoot]/ContentList/TargetItemFolder/SourceItemFolder2");
			CheckContentListItem1("[TestRoot]/ContentList/TargetItemFolder/SourceItemFolder2/SourceContentListItem");
		}
		#endregion

		#region General Copy tests
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Copy_SourceIsNotExist()
		{
			Node.Copy("/Root/osiejfvchxcidoklg6464783930020398473/iygfevfbvjvdkbu9867513125615", TestRoot.Path);
		}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Copy_TargetIsNotExist()
		{
			Node.Copy(TestRoot.Path, "/Root/fdgdffgfccxdxdsffcv31945581316942/udjkcmdkeieoeoodoc542364737827");
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Copy_CopyTo_Null()
		{
			TestRoot.CopyTo(null);
		}
		[ExpectedException(typeof(ArgumentNullException))]
		public void Copy_NullSourcePath()
		{
			Node.Copy(null, TestRoot.Path);
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
		public void Copy_InvalidSourcePath()
		{
			Node.Copy(string.Empty, TestRoot.Path);
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Copy_NullTargetPath()
		{
			Node.Copy(TestRoot.Path, null);
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
		public void Copy_InvalidTargetPath()
		{
			Node.Copy(TestRoot.Path, string.Empty);
		}
		[TestMethod]
		public void Copy_ToItsParent()
		{
			EnsureNode("[TestRoot]/Target/N1");
			CopyNode("[TestRoot]/Target/N1", "[TestRoot]/Target");
			Assert.IsNotNull(LoadNode("[TestRoot]/Target/Copy of N1"));
		}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Copy_ToItself()
		{
			Node.Copy(TestRoot.Path, TestRoot.Path);
		}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Copy_ToUnderItself()
		{
			EnsureNode("[TestRoot]/Source/N3");
			CopyNode("[TestRoot]/Source", "[TestRoot]/Source/N3");
		}
		[TestMethod]
		[ExpectedException(typeof(ApplicationException))]
		public void Copy_TargetHasSameName()
		{
			EnsureNode("[TestRoot]/Source");
			EnsureNode("[TestRoot]/Target/Source");
			CopyNode("[TestRoot]/Source", "[TestRoot]/Target");
		}
		[TestMethod]
		public void Copy_SourceIsLockedByAnother()
		{
			IUser originalUser = AccessProvider.Current.GetCurrentUser();
			IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
            TestRoot.Security.SetPermission(visitor, true, PermissionType.Save, PermissionValue.Allow);
            TestRoot.Security.SetPermission(visitor, true, PermissionType.Delete, PermissionValue.Allow);

			EnsureNode("[TestRoot]/Source/N1");
			EnsureNode("[TestRoot]/Source/N2");
			EnsureNode("[TestRoot]/Target");
			Node lockedNode = LoadNode("[TestRoot]/Source");

			AccessProvider.Current.SetCurrentUser(visitor);
            try
            {
                lockedNode.Lock.Lock();
            }
            finally
            {
                AccessProvider.Current.SetCurrentUser(originalUser);
            }

			CopyNode("[TestRoot]/Source", "[TestRoot]/Target");

			AccessProvider.Current.SetCurrentUser(visitor);
			lockedNode.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
			AccessProvider.Current.SetCurrentUser(originalUser);
		}
		[TestMethod]
		public void Copy_SourceChildIsLockedByAnother()
		{
			IUser originalUser = AccessProvider.Current.GetCurrentUser();
			IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
            TestRoot.Security.SetPermission(visitor, true, PermissionType.Save, PermissionValue.Allow);
            TestRoot.Security.SetPermission(visitor, true, PermissionType.Delete, PermissionValue.Allow);

			EnsureNode("[TestRoot]/Source/N1/N2/N3");
			EnsureNode("[TestRoot]/Source/N1/N4/N5");
			Node lockedNode = LoadNode("[TestRoot]/Source/N1/N4");

			AccessProvider.Current.SetCurrentUser(visitor);
            try
            {
                lockedNode.Lock.Lock();
            }
            finally
            {
                AccessProvider.Current.SetCurrentUser(originalUser);
            }

			lockedNode.Parent.CopyTo(TestRoot);


			AccessProvider.Current.SetCurrentUser(visitor);
			lockedNode.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
			AccessProvider.Current.SetCurrentUser(originalUser);

            var n4 = LoadNode("[TestRoot]/N1/N4");
            Assert.IsNotNull(n4, "Cannot load a copied node.");
            Assert.IsTrue(n4.LockedById == 0, "LockedById must be 0");
            Assert.IsFalse(n4.Locked, "Node is locked but LockedById is 0. Expected: Locked = false");
            Assert.IsFalse(n4.Version.VersionString.EndsWith("L"), "Wrong version state. Expected: 'P'");
        }
		[TestMethod]
		public void Copy_SourceIsLockedByCurrent()
		{
			EnsureNode("[TestRoot]/Source/N1");
			EnsureNode("[TestRoot]/Source/N2");
			EnsureNode("[TestRoot]/Target");
			var lockedNode = LoadNode("[TestRoot]/Source");
			try
			{
				lockedNode.Lock.Lock();
				CopyNode("[TestRoot]/Source", "[TestRoot]/Target");
			}
			finally
			{
				if (lockedNode.Lock.Locked)
					lockedNode.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
			}
			//nem jo az sql hibakod, es nem tudom, hogy kell-e egyaltalan hibat dobni
		}
		[TestMethod]
		public void Copy_LockedTarget_SameUser()
		{
			EnsureNode("[TestRoot]/Source/N1/N2/N3");
			EnsureNode("[TestRoot]/Source/N1/N4/N5");
			EnsureNode("[TestRoot]/Target/N6");
			var lockedNode = LoadNode("[TestRoot]/Source/N1/N4");
			try
			{
				lockedNode.Lock.Lock();
				CopyNode("[TestRoot]/Source", "[TestRoot]/Target");
			}
			finally
			{
				if (lockedNode.Lock.Locked)
					lockedNode.Lock.Unlock(VersionStatus.Approved, VersionRaising.None);
			}
			//nem jo az sql hibakod, es nem tudom, hogy kell-e egyaltalan hibat dobni
		}

        [TestMethod]
        public void Copy_Streams()
        {
            var sourceText = "Source file content";
            var targetText = "Target file content";

            EnsureNode("[TestRoot]/SourceFolder");
            EnsureNode("[TestRoot]/TargetFolder");
            var srcFolder = LoadNode("[TestRoot]/SourceFolder");
            var file = new File(srcFolder);
            file.Name = "MyFile";
            file.Binary.SetStream(Tools.GetStreamFromString(sourceText));
            file.Save();

            CopyNode("[TestRoot]/SourceFolder/MyFile", "[TestRoot]/TargetFolder");

            file = (File)LoadNode("[TestRoot]/TargetFolder/MyFile");
            file.Binary.SetStream(Tools.GetStreamFromString(targetText));
            file.Save();

            file = (File)LoadNode("[TestRoot]/SourceFolder/MyFile");
            var loadedSourceText = Tools.GetStreamString(file.Binary.GetStream());
            file = (File)LoadNode("[TestRoot]/TargetFolder/MyFile");
            var loadedTargetText = Tools.GetStreamString(file.Binary.GetStream());

            Assert.AreEqual(sourceText, loadedSourceText);
            Assert.AreEqual(targetText, loadedTargetText);
        }
        [TestMethod]
        public void Copy_References()
        {
            // WARNING! Do not use this code in a business app.
            // This is not a valid use case of handling a Group membership but good enough for this test case.

            EnsureNode("[TestRoot]/SourceFolder");
            EnsureNode("[TestRoot]/TargetFolder");
            var srcFolder = LoadNode("[TestRoot]/SourceFolder");
            var group = new Group(srcFolder);
            group.Name = "MyGroup";
            group.AddReference("Members", Node.LoadNode(1));
            group.AddReference("Members", Node.LoadNode(2));
            group.Save();

            CopyNode("[TestRoot]/SourceFolder/MyGroup", "[TestRoot]/TargetFolder");

            group = (Group)LoadNode("[TestRoot]/TargetFolder/MyGroup");
            group.ClearReference("Members");
            group.AddReference("Members", Node.LoadNode(3));
            group.AddReference("Members", Node.LoadNode(4));
            group.AddReference("Members", Node.LoadNode(5));
            group.Save();

            var loadedSourceRefs = String.Join(",", (from r in LoadNode("[TestRoot]/SourceFolder/MyGroup").GetReferences("Members") orderby r.Id select r.Id.ToString()).ToArray());
            var loadedTargetRefs = String.Join(",", (from r in LoadNode("[TestRoot]/TargetFolder/MyGroup").GetReferences("Members") orderby r.Id select r.Id.ToString()).ToArray());

            Assert.AreEqual(loadedSourceRefs, "1,2");
            Assert.AreEqual(loadedTargetRefs, "3,4,5");
        }


		[TestMethod]
		public void Copy_MinimalPermissions()
		{
			IUser originalUser = AccessProvider.Current.GetCurrentUser();
			try
			{
				IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
				EnsureNode("[TestRoot]/Source");
				EnsureNode("[TestRoot]/Target");
				Node sourceNode = LoadNode("[TestRoot]/Source");
				Node targetNode = LoadNode("[TestRoot]/Target");
                sourceNode.Security.SetPermission(visitor, true, PermissionType.Open, PermissionValue.Allow);
                targetNode.Security.SetPermission(visitor, true, PermissionType.AddNew, PermissionValue.Allow);
                targetNode.Security.SetPermission(visitor, true, PermissionType.See, PermissionValue.Allow);
                AccessProvider.Current.SetCurrentUser(visitor);
				CopyNode("[TestRoot]/Source", "[TestRoot]/Target");
			}
			finally
			{
				AccessProvider.Current.SetCurrentUser(originalUser);
			}
		}

		[TestMethod]
        public void Copy_PermissionSet()
        {
            EnsureNode("[TestRoot]/Source/N1/N3/N5");
            EnsureNode("[TestRoot]/Source/N2/N4/N6");
            EnsureNode("[TestRoot]/Target");

            var src = LoadNode("[TestRoot]/Source");
            src.Security.SetPermission(User.Visitor, true, PermissionType.Publish, PermissionValue.Allow);

            var n3 = LoadNode("[TestRoot]/Source/N1/N3");
            n3.Security.SetPermission(User.Visitor, true, PermissionType.SetPermissions, PermissionValue.Allow);
            n3.Security.SetPermission(User.Visitor, true, PermissionType.RecallOldVersion, PermissionValue.Allow);
            var n6 = LoadNode("[TestRoot]/Source/N2/N4/N6");
            n6.Security.SetPermission(User.Visitor, true, PermissionType.SetPermissions, PermissionValue.Allow);
            n6.Security.SetPermission(User.Visitor, true, PermissionType.RecallOldVersion, PermissionValue.Allow);

            CopyNode("[TestRoot]/Source", "[TestRoot]/Target");

            var origUser = AccessProvider.Current.GetCurrentUser();
            AccessProvider.Current.SetCurrentUser(User.Visitor);
            var srcPerm = false;
            var n5perm = false;
            var n6perm = false;
            try
            {
                srcPerm = LoadNode("[TestRoot]/Target/Source").Security.HasPermission(PermissionType.Publish);
                n5perm = LoadNode("[TestRoot]/Target/Source/N1/N3/N5").Security.HasPermission(PermissionType.Publish, PermissionType.SetPermissions, PermissionType.RecallOldVersion);
                n6perm = LoadNode("[TestRoot]/Target/Source/N2/N4/N6").Security.HasPermission(PermissionType.Publish, PermissionType.SetPermissions, PermissionType.RecallOldVersion);
            }
            finally
            {
                AccessProvider.Current.SetCurrentUser(origUser);
            }

            Assert.IsTrue(srcPerm, "#1");
            Assert.IsTrue(n5perm, "#2");
            Assert.IsTrue(n6perm, "#3");
        }

		//[TestMethod]
		//public void Copy_SourceWithoutOpenMinorPermission()
		//{
		//    IUser originalUser = AccessProvider.Current.GetCurrentUser();
		//    IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
		//    EnsureNode("[TestRoot]/Source");
		//    EnsureNode("[TestRoot]/Target");
		//    Node sourceNode = LoadNode("[TestRoot]/Source");
		//    Node targetNode = LoadNode("[TestRoot]/Target");
		//    targetNode.Security.SetPermission(visitor, PermissionType.AddNew, PermissionValue.Allow);
		//    bool expectedExceptionWasThrown = false;
		//    try
		//    {
		//        AccessProvider.Current.SetCurrentUser(visitor);
		//        CopyNode("[TestRoot]/Source", "[TestRoot]/Target");
		//    }
		//    catch (LockedNodeException)
		//    {
		//        expectedExceptionWasThrown = true;
		//    }
		//    finally
		//    {
		//        AccessProvider.Current.SetCurrentUser(originalUser);
		//    }
		//    Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		//}
		//[TestMethod]
		//public void Copy_TargetWithoutAddNewPermission()
		//{
		//    IUser originalUser = AccessProvider.Current.GetCurrentUser();
		//    IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
		//    EnsureNode("[TestRoot]/Source");
		//    EnsureNode("[TestRoot]/Target");
		//    Node sourceNode = LoadNode("[TestRoot]/Source");
		//    Node targetNode = LoadNode("[TestRoot]/Target");
		//    sourceNode.Security.SetPermission(visitor, PermissionType.OpenMinor, PermissionValue.Allow);
		//    bool expectedExceptionWasThrown = false;
		//    try
		//    {
		//        AccessProvider.Current.SetCurrentUser(visitor);
		//        CopyNode("[TestRoot]/Source", "[TestRoot]/Target");
		//    }
		//    catch (LockedNodeException)
		//    {
		//        expectedExceptionWasThrown = true;
		//    }
		//    finally
		//    {
		//        AccessProvider.Current.SetCurrentUser(originalUser);
		//    }
		//    Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		//}
		//[TestMethod]
		//public void Copy_SourceTreeWithPartialOpenMinorPermission()
		//{
		//    IUser originalUser = AccessProvider.Current.GetCurrentUser();
		//    IUser visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId) as IUser;
		//    EnsureNode("[TestRoot]/Source/N1/N2");
		//    EnsureNode("[TestRoot]/Source/N1/N3");
		//    EnsureNode("[TestRoot]/Source/N4");
		//    EnsureNode("[TestRoot]/Target");
		//    Node source = LoadNode("[TestRoot]/Source");
		//    Node target = LoadNode("[TestRoot]/Target");
		//    source.Security.SetPermission(visitor, PermissionType.OpenMinor, PermissionValue.Allow);
		//    source.Security.SetPermission(visitor, PermissionType.Delete, PermissionValue.Allow);
		//    target.Security.SetPermission(visitor, PermissionType.AddNew, PermissionValue.Allow);
		//    Node blockedNode = LoadNode("[TestRoot]/Source/N1/N3");
		//    blockedNode.Security.SetPermission(visitor, PermissionType.OpenMinor, PermissionValue.NonDefined);
		//    bool expectedExceptionWasThrown = false;
		//    try
		//    {
		//        AccessProvider.Current.SetCurrentUser(visitor);
		//        CopyNode("[TestRoot]/Source", "[TestRoot]/Target");
		//    }
		//    catch (LockedNodeException)
		//    {
		//        expectedExceptionWasThrown = true;
		//    }
		//    finally
		//    {
		//        AccessProvider.Current.SetCurrentUser(originalUser);
		//    }
		//    Assert.IsTrue(expectedExceptionWasThrown, "The expected exception was not thrown.");
		//}
		#endregion

        #region ContentList Copy tests
		//[TestMethod]
		//public void Copy_ContentList_NodeWithContentListToNode()
		//{
		//    //5: CopyNodeWithContentListToNode
		//    //Create [TestRoot]/SourceFolder/SourceContentList/SourceContentListItem
		//    //Create [TestRoot]/TargetFolder
		//    //Copy SourceFolder, TargetFolder
		//    //Check: Unchanged contentlist and item
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceFolder/SourceContentList/SourceContentListItem");
		//    EnsureNode("[TestRoot]/TargetFolder");
		//    CopyNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetFolder");
		//    CheckSimpleNode("[TestRoot]/TargetFolder/SourceFolder");
		//    CheckContentList1("[TestRoot]/TargetFolder/SourceFolder/SourceContentList");
		//    CheckContentListItem1("[TestRoot]/TargetFolder/SourceFolder/SourceContentList/SourceContentListItem");
		//}
		//[TestMethod]
		//[ExpectedException(typeof(ApplicationException))]
		//public void Copy_ContentList_NodeWithContentListToContentList()
		//{
		//    //6: CopyNodeWithContentListToContentList
		//    //Create [TestRoot]/SourceFolder/SourceContentList/SourceContentListItem
		//    //Create [TestRoot]/TargetContentList
		//    //Copy SourceFolder, TargetContentList
		//    //Check: exception
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceFolder/SourceContentList/SourceContentListItem");
		//    EnsureNode("[TestRoot]/TargetContentList");
		//    CopyNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetContentList");
		//}
		//[TestMethod]
		//[ExpectedException(typeof(ApplicationException))]
		//public void Copy_ContentList_NodeWithContentListToContentListItem()
		//{
		//    //7: CopyNodeWithContentListToContentListItem
		//    //Create [TestRoot]/SourceFolder/SourceContentList/SourceContentListItem
		//    //Create [TestRoot]/TargetContentList/TargetItemFolder
		//    //Copy SourceFolder, TargetItemFolder
		//    //Check: exception
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceFolder/SourceContentList/SourceContentListItem");
		//    EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
		//    CopyNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetContentList/TargetItemFolder");
		//}
		//[TestMethod]
		//[ExpectedException(typeof(ApplicationException))]
		//public void Copy_ContentList_ContentListToNode()
		//{
		//    //8: CopyContentListToNode
		//    //Create [TestRoot]/SourceContentList
		//    //Create [TestRoot]/TargetFolder
		//    //Copy SourceContentList, TargetFolder
		//    //Check: ok
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceContentList");
		//    EnsureNode("[TestRoot]/TargetFolder");
		//    CopyNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetFolder");
		//}
		//[TestMethod]
		//[ExpectedException(typeof(ApplicationException))]
		//public void Copy_ContentList_ContentListToContentList()
		//{
		//    //9: CopyContentListToContentList
		//    //Create [TestRoot]/SourceContentList
		//    //Create [TestRoot]/TargetContentList
		//    //Copy SourceContentList, TargetContentList
		//    //Check: exception
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceContentList");
		//    EnsureNode("[TestRoot]/TargetContentList");
		//    CopyNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetContentList");
		//}
		//[TestMethod]
		//[ExpectedException(typeof(ApplicationException))]
		//public void Copy_ContentList_ContentListToContentListItem()
		//{
		//    //10: CopyContentListToContentListItem
		//    //Create [TestRoot]/SourceContentList
		//    //Create [TestRoot]/TargetContentList/TargetItemFolder
		//    //Copy SourceContentList, TargetItemFolder
		//    //Check: exception
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceContentList");
		//    EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
		//    CopyNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetContentList/TargetItemFolder");
		//}
		//[TestMethod]
		//[ExpectedException(typeof(ApplicationException))]
		//public void Copy_ContentList_ContentListTreeToNode()
		//{
		//    //11: CopyContentListTreeToNode
		//    //Create [TestRoot]/SourceContentList/SourceContentListItem
		//    //Create [TestRoot]/TargetFolder
		//    //Copy SourceContentList, TargetFolder
		//    //Check: ok
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
		//    EnsureNode("[TestRoot]/TargetFolder");
		//    CopyNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetFolder");
		//    CheckContentList1("[TestRoot]/TargetFolder/SourceContentList");
		//    CheckContentListItem1("[TestRoot]/TargetFolder/SourceContentList/SourceContentListItem");
		//}
		//[TestMethod]
		//[ExpectedException(typeof(ApplicationException))]
		//public void Copy_ContentList_ContentListTreeToContentList()
		//{
		//    //12: CopyContentListTreeToContentList
		//    //Create [TestRoot]/SourceContentList/SourceContentListItem
		//    //Create [TestRoot]/TargetContentList
		//    //Copy SourceContentList, TargetContentList
		//    //Check: exception
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
		//    EnsureNode("[TestRoot]/TargetContentList");
		//    CopyNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetContentList");
		//}
		//[TestMethod]
		//[ExpectedException(typeof(ApplicationException))]
		//public void Copy_ContentList_ContentListTreeToContentListItem()
		//{
		//    //13: CopyContentListTreeToContentListItem
		//    //Create [TestRoot]/SourceContentList/SourceContentListItem
		//    //Create [TestRoot]/TargetContentList/TargetItemFolder
		//    //Copy SourceContentList, TargetItemFolder
		//    //Check: exception
		//    PrepareTest();
		//    EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
		//    EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
		//    CopyNode("[TestRoot]/SourceContentList", "[TestRoot]/TargetContentList/TargetItemFolder");
		//}
		[TestMethod]
		public void Copy_ContentList_ContentListItemToSameContentListItem()
		{
			//24: CopyContentListItemToSameContentListItem
			//Create [TestRoot]/ContentList/SourceItemFolder/SourceContentListItem
			//Create [TestRoot]/ContentList/TargetItemFolder
			//Copy SourceContentListItem, TargetItemFolder
			//Check: 
			//PrepareTest();
			EnsureNode("[TestRoot]/ContentList/SourceItemFolder/SourceContentListItem");
			EnsureNode("[TestRoot]/ContentList/TargetItemFolder");
			CopyNode("[TestRoot]/ContentList/SourceItemFolder/SourceContentListItem", "[TestRoot]/ContentList/TargetItemFolder");
			CheckContentListItem1("[TestRoot]/ContentList/TargetItemFolder/SourceContentListItem");
		}
		[TestMethod]
		public void Copy_ContentList_ContentListItemTreeToSameContentListItem()
		{
			//26: CopyContentListItemTreeToSameContentListItem
			//Create [TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
			//Create [TestRoot]/ContentList/TargetItemFolder
			//Copy SourceItemFolder2, TargetItemFolder
			//Check: 
			//PrepareTest();
			EnsureNode("[TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
			EnsureNode("[TestRoot]/ContentList/TargetItemFolder");
			CopyNode("[TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/ContentList/TargetItemFolder");
			CheckContentListItem1("[TestRoot]/ContentList/TargetItemFolder/SourceItemFolder2");
			CheckContentListItem1("[TestRoot]/ContentList/TargetItemFolder/SourceItemFolder2/SourceContentListItem");
		}
        [TestMethod]
        public void Copy_ContentList_ContentListItemToSameContentList()
        {
            //23: CopyContentListItemToSameContentList
            //Create [TestRoot]/ContentList/SourceItemFolder/SourceContentListItem
            //Create [TestRoot]/ContentList
            //Copy SourceContentListItem, SourceContentList
            //Check: 
            //PrepareTest();
            EnsureNode("[TestRoot]/ContentList/SourceItemFolder/SourceContentListItem");
            //EnsureNode("[TestRoot]/ContentList");
            CopyNode("[TestRoot]/ContentList/SourceItemFolder/SourceContentListItem", "[TestRoot]/ContentList");
            CheckContentListItem1("[TestRoot]/ContentList/SourceContentListItem");
        }
        [TestMethod]
        public void Copy_ContentList_ContentListItemTreeToSameContentList()
        {
            //25: CopyContentListItemTreeToSameContentList
            //Create [TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
            //Create [TestRoot]/ContentList
            //Copy SourceItemFolder2, SourceContentList
            //Check: 
            //PrepareTest();
            EnsureNode("[TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
            //EnsureNode("[TestRoot]/ContentList");
            CopyNode("[TestRoot]/ContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/ContentList");
            CheckContentListItem1("[TestRoot]/ContentList/SourceItemFolder2");
            CheckContentListItem1("[TestRoot]/ContentList/SourceItemFolder2/SourceContentListItem");
        }
        [TestMethod]
        public void Copy_ContentList_InTree()
        {
            //PrepareTest();
            EnsureNode("[TestRoot]/SourceFolder/ContentList/SourceItemFolder1/SourceContentListItem");
            EnsureNode("[TestRoot]/TargetFolder");
            CopyNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetFolder");
            CheckContentList1("[TestRoot]/TargetFolder/SourceFolder/ContentList");
            CheckContentListItem1("[TestRoot]/TargetFolder/SourceFolder/ContentList/SourceItemFolder1");
            CheckContentListItem1("[TestRoot]/TargetFolder/SourceFolder/ContentList/SourceItemFolder1/SourceContentListItem");
        }

        //--- invalid operations

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_LeafNodeToContentList()
		{
			//1: CopyLeafNodeToContentList
			//Create [TestRoot]/SourceNode
			//Create [TestRoot]/TargetContentList
			//Copy SourceNode, TargetContentList
			//Check: Node => Item
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceNode");
			EnsureNode("[TestRoot]/TargetContentList");
            CopyNode("[TestRoot]/SourceNode", "[TestRoot]/TargetContentList");
			//CheckContentListItem2("[TestRoot]/TargetContentList/SourceNode");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_NodeTreeToContentList()
		{
			//3: CopyNodeTreeToContentList
			//Create [TestRoot]/SourceFolder/SourceNode
			//Create [TestRoot]/TargetContentList
			//Copy SourceFolder, TargetContentList
			//Check: NodeTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceFolder/SourceNode");
			EnsureNode("[TestRoot]/TargetContentList");
			CopyNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetContentList");
            //CheckContentListItem2("[TestRoot]/TargetContentList/SourceFolder");
            //CheckContentListItem2("[TestRoot]/TargetContentList/SourceFolder/SourceNode");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_LeafNodeToContentListItem()
		{
			//2: CopyLeafNodeToContentListItem
			//Create [TestRoot]/SourceNode
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Copy SourceNode, TargetItemFolder
			//Check: Node => Item
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceNode");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			CopyNode("[TestRoot]/SourceNode", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceNode");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_NodeTreeToContentListItem()
		{
			//4: CopyNodeTreeToContentListItem
			//Create [TestRoot]/SourceFolder/SourceNode
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Copy SourceFolder, TargetItemFolder
			//Check: NodeTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceFolder/SourceNode");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			CopyNode("[TestRoot]/SourceFolder", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceFolder/SourceNode");
		}
		[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_ContentListItemToNode()
		{
			//14: CopyContentListItemToNode
			//Create [TestRoot]/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetFolder
			//Copy SourceContentListItem, TargetFolder
			//Check: Item => Node
			//PrepareTest();

            //this is a valid operation from now on
			EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetFolder");
			CopyNode("[TestRoot]/SourceContentList/SourceContentListItem", "[TestRoot]/TargetFolder");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceContentListItem");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_ContentListItemToContentList()
		{
			//15: CopyContentListItemToContentList
			//Create [TestRoot]/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetContentList
			//Copy SourceContentListItem, TargetContentList
			//Check: Item => Item
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList");
			CopyNode("[TestRoot]/SourceContentList/SourceContentListItem", "[TestRoot]/TargetContentList");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceContentListItem");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_ContentListItemToContentListItem()
		{
			//16: CopyContentListItemToContentListItem
			//Create [TestRoot]/SourceContentList/SourceContentListItem
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Copy SourceContentListItem, TargetItemFolder
			//Check: Item => Item
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			CopyNode("[TestRoot]/SourceContentList/SourceContentListItem", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceContentListItem");
		}
		[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_ContentListItemTreeToNode()
		{
			//17: CopyContentListItemTreeToNode
			//Create [TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem
			//Create [TestRoot]/TargetFolder
			//Copy SourceItemFolder, TargetFolder
			//Check: ItemTree => NodeTree
			//PrepareTest();

            //this is a valid operation from now on
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetFolder");
			CopyNode("[TestRoot]/SourceContentList/SourceItemFolder", "[TestRoot]/TargetFolder");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceItemFolder");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceItemFolder/SourceContentListItem");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_ContentListItemTreeToContentList()
		{
			//18: CopyContentListItemTreeToContentList
			//Create [TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem
			//Create [TestRoot]/TargetContentList
			//Copy SourceItemFolder, TargetContentList
			//Check: ItemTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList");
			CopyNode("[TestRoot]/SourceContentList/SourceItemFolder", "[TestRoot]/TargetContentList");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceItemFolder/SourceContentListItem");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_ContentListItemTreeToContentListItem()
		{
			//19: CopyContentListItemTreeToContentListItem
			//Create [TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Copy SourceItemFolder, TargetItemFolder
			//Check: ItemTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			CopyNode("[TestRoot]/SourceContentList/SourceItemFolder", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceItemFolder/SourceContentListItem");
		}
		[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_ContentListItemTree2ToNode()
		{
			//20: CopyContentListItemTree2ToNode
			//Create [TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
			//Create [TestRoot]/TargetFolder
			//Copy SourceItemFolder2, TargetFolder
			//Check: ItemTree => NodeTree
			//PrepareTest();

            //this is a valid operation from now on
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetFolder");
			CopyNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/TargetFolder");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceItemFolder2");
			CheckSimpleNode("[TestRoot]/TargetFolder/SourceItemFolder2/SourceContentListItem");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Copy_ContentList_ContentListItemTree2ToContentList()
		{
			//21: CopyContentListItemTree2ToContentList
			//Create [TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
			//Create [TestRoot]/TargetContentList
			//Copy SourceItemFolder2, TargetContentList
			//Check: ItemTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList");
			CopyNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/TargetContentList");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceItemFolder2");
			CheckContentListItem2("[TestRoot]/TargetContentList/SourceItemFolder2/SourceContentListItem");
		}
		[TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
		public void Copy_ContentList_ContentListItemTree2ToContentListItem()
		{
			//22: CopyContentListItemTree2ToContentListItem
			//Create [TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem
			//Create [TestRoot]/TargetContentList/TargetItemFolder
			//Copy SourceItemFolder2, TargetItemFolder
			//Check: ItemTree => ItemTree
			//PrepareTest();
			EnsureNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2/SourceContentListItem");
			EnsureNode("[TestRoot]/TargetContentList/TargetItemFolder");
			CopyNode("[TestRoot]/SourceContentList/SourceItemFolder1/SourceItemFolder2", "[TestRoot]/TargetContentList/TargetItemFolder");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceItemFolder2");
			CheckContentListItem2("[TestRoot]/TargetContentList/TargetItemFolder/SourceItemFolder2/SourceContentListItem");
		}
		#endregion

        #region Rename tests

        [TestMethod]
        public void Rename_FileAndItsParent()
        {
            var fileContent = "FileContent";
            EnsureNode("[TestRoot]/SourceFolder");
            var folder = LoadNode("[TestRoot]/SourceFolder");
            var folderid = folder.Id;
            var file = new File(folder);
            file.Name = "TestFile";
            file.Binary.SetStream(Tools.GetStreamFromString(fileContent));
            file.Save();
            var fileId = file.Id;

            file = Node.Load<File>(fileId);
            file.Name = "RenamedFile";
            file.Save();

            file = Node.Load<File>(fileId);
            var loaded1 = Tools.GetStreamString(file.Binary.GetStream());

            Assert.IsTrue(loaded1 == fileContent);

            folder = Node.Load<Folder>(folderid);
            folder.Name = "RenamedFolder";
            folder.Save();

            file = Node.Load<File>(fileId);
            var loaded2 = Tools.GetStreamString(file.Binary.GetStream());

            Assert.IsTrue(loaded2 == fileContent);

        }
        [TestMethod]
        public void Rename_SubtreeCheck()
        {
            EnsureNode("[TestRoot]/AA/BB/DD");
            EnsureNode("[TestRoot]/AA/BB/EE");
            EnsureNode("[TestRoot]/AA/CC/FF");

            Assert.IsNotNull(LoadNode("[TestRoot]/AA"));
            Assert.IsNotNull(LoadNode("[TestRoot]/AA/BB"));
            Assert.IsNotNull(LoadNode("[TestRoot]/AA/BB/DD"));
            Assert.IsNotNull(LoadNode("[TestRoot]/AA/BB/EE"));
            Assert.IsNotNull(LoadNode("[TestRoot]/AA/CC"));
            Assert.IsNotNull(LoadNode("[TestRoot]/AA/CC/FF"));

            var node = LoadNode("[TestRoot]/AA");
            node.Name = "XX";
            node.Save();

            Assert.IsNull(LoadNode("[TestRoot]/AA"));
            Assert.IsNull(LoadNode("[TestRoot]/AA/BB"));
            Assert.IsNull(LoadNode("[TestRoot]/AA/BB/DD"));
            Assert.IsNull(LoadNode("[TestRoot]/AA/BB/EE"));
            Assert.IsNull(LoadNode("[TestRoot]/AA/CC"));
            Assert.IsNull(LoadNode("[TestRoot]/AA/CC/FF"));

            Assert.IsNotNull(LoadNode("[TestRoot]/XX"));
            Assert.IsNotNull(LoadNode("[TestRoot]/XX/BB"));
            Assert.IsNotNull(LoadNode("[TestRoot]/XX/BB/DD"));
            Assert.IsNotNull(LoadNode("[TestRoot]/XX/BB/EE"));
            Assert.IsNotNull(LoadNode("[TestRoot]/XX/CC"));
            Assert.IsNotNull(LoadNode("[TestRoot]/XX/CC/FF"));

            var q1 = new SenseNet.ContentRepository.Storage.Search.NodeQuery(
                new SenseNet.ContentRepository.Storage.Search.StringExpression(
                    SenseNet.ContentRepository.Storage.Search.StringAttribute.Path,
                    SenseNet.ContentRepository.Storage.Search.StringOperator.StartsWith,
                    String.Concat(_testRootPath, "/AA")));
            var result1 = q1.Execute();
            Assert.IsTrue(result1.Count == 0, "#1");

            var q2 = new SenseNet.ContentRepository.Storage.Search.NodeQuery(
            new SenseNet.ContentRepository.Storage.Search.StringExpression(
                SenseNet.ContentRepository.Storage.Search.StringAttribute.Path,
                SenseNet.ContentRepository.Storage.Search.StringOperator.StartsWith,
                String.Concat(_testRootPath, "/XX")));
            var result2 = q2.Execute();
            Assert.IsTrue(result2.Count == 6, "#2");
        }

        #endregion

        //============================================================================================== Tools

		[TestInitialize]
		public void PrepareTest()
		{
			foreach (Node node in ((Folder)TestRoot).Children)
			{
				int lastUnlockedId = 0;
				do
				{
					try
					{
						node.ForceDelete();
						lastUnlockedId = 0;
					}
					catch (LockedNodeException e)
					{
						lastUnlockedId = node.Id;
						e.LockHandler.Unlock(VersionStatus.Approved, VersionRaising.None);
					}
				} while (lastUnlockedId != 0);
			}
		}

	    private void MoveNode(string encodedSourcePath, string encodedTargetPath, bool clearTarget = false)
		{
			string sourcePath = DecodePath(encodedSourcePath);
			string targetPath = DecodePath(encodedTargetPath);
			int sourceId = Node.LoadNode(sourcePath).Id;
			int targetId = Node.LoadNode(targetPath).Id;

            //make sure target does not contain the source node
            if (clearTarget)
            {
                var sourceName = RepositoryPath.GetFileNameSafe(sourcePath);
                if (!string.IsNullOrEmpty(sourceName))
                {
                    var targetPathWithName = RepositoryPath.Combine(targetPath, sourceName);
                    if (Node.Exists(targetPathWithName))
                        Node.ForceDelete(targetPathWithName);
                }
            }

			Node.Move(sourcePath, targetPath);

			Node parentNode = Node.LoadNode(targetId);
			Node childNode = Node.LoadNode(sourceId);
			Assert.IsTrue(childNode.ParentId == parentNode.Id, "Source was not moved.");
		}
		private void CopyNode(string encodedSourcePath, string encodedTargetPath)
		{
			string sourcePath = DecodePath(encodedSourcePath);
			string targetPath = DecodePath(encodedTargetPath);
			Node srcNode = Node.LoadNode(sourcePath);
			int sourceId = srcNode.Id;
			int sourceParentId = srcNode.ParentId;
			int targetId = Node.LoadNode(targetPath).Id;

			Node.Copy(sourcePath, targetPath);

            AccessProvider.ChangeToSystemAccount();
            try
            {
                Node parentNode = Node.LoadNode(targetId);
                Node sourceNode = Node.LoadNode(sourceId);
                Node copiedNode = Node.LoadNode(RepositoryPath.Combine(parentNode.Path, sourceNode.Name));
			    Assert.IsNotNull(copiedNode, "Copied node is not found.");
			    Assert.IsTrue(sourceNode.Path == sourcePath, "Source node has changed.");
            }
            finally
            {
                AccessProvider.RestoreOriginalUser();
            }
		}

		private void CheckSimpleNode(string encodedPath)
		{
			Node node = LoadNode(encodedPath);
			Assert.IsTrue(node.ContentListId == 0, "ContentListId is not 0");
			Assert.IsNull(node.ContentListType, "ContentListType is not null");
		}
		private void CheckContentList1(string encodedPath)
		{
			ContentList contentlist = Node.Load<ContentList>(DecodePath(encodedPath));
            Assert.IsTrue(contentlist.ContentListId == 0, "ContentListId is not 0");
            Assert.IsNotNull(contentlist.ContentListType, "ContentListType is null");
			Assert.IsTrue(contentlist.ContentListDefinition == _listDef1);
		}
		private void CheckContentList2(string encodedPath)
		{
			ContentList contentlist = Node.Load<ContentList>(DecodePath(encodedPath));
            Assert.IsTrue(contentlist.ContentListId == 0, "ContentListId is not 0");
            Assert.IsNotNull(contentlist.ContentListType, "ContentListType is null");
			Assert.IsTrue(contentlist.ContentListDefinition == _listDef2);
		}
		private void CheckContentListItem1(string encodedPath)
		{
			Node node = LoadNode(encodedPath);
			Assert.IsTrue(node.HasProperty("#String_0"), "ContentListItem has not property: #String_0");
            Assert.IsNotNull(node.ContentListType, "ContentListItem ContentListType == null");
            Assert.IsTrue(node.ContentListId > 0, "ContentListItem ContentListId == 0");
		}
		private void CheckContentListItem2(string encodedPath)
		{
			Node node = LoadNode(encodedPath);
            Assert.IsTrue(node.HasProperty("#Int_0"), "ContentListItem has not property: #Int_0");
            Assert.IsNotNull(node.ContentListType, "ContentListItem ContentListType == null");
            Assert.IsTrue(node.ContentListId > 0, "ContentListItem ContentListId == 0");
		}

		private void EnsureNode(string encodedPath)
		{
			string path = DecodePath(encodedPath);
			if (Node.Exists(path))
				return;

			string name = RepositoryPath.GetFileName(path);
			string parentPath = RepositoryPath.GetParentPath(path);
			EnsureNode(parentPath);

			switch (name)
			{
                case "ContentList":
				case "SourceContentList":
					CreateContentList(parentPath, name, _listDef1);
					break;
				case "TargetContentList":
					CreateContentList(parentPath, name, _listDef2);
					break;
				case "SourceFolder":
				case "SourceItemFolder":
				case "SourceItemFolder1":
				case "SourceItemFolder2":
				case "TargetFolder":
				case "TargetItemFolder":
					CreateNode(parentPath, name, "Folder");
					break;
				case "SourceContentListItem":
				case "SourceNode":
					CreateNode(parentPath, name, "Car");
					break;
				default:
					CreateNode(parentPath, name, "Car");
					break;
			}
		}
		private Node LoadNode(string encodedPath)
		{
			return Node.LoadNode(DecodePath(encodedPath));
		}
		private void CreateContentList(string parentPath, string name, string listDef)
		{
			Node parent = Node.LoadNode(parentPath);
			ContentList contentlist = new ContentList(parent);
			contentlist.Name = name;
			contentlist.ContentListDefinition = listDef;
			contentlist.Save();
		}
		private void CreateNode(string parentPath, string name, string typeName)
		{
			Content parent = Content.Load(parentPath);
            Content content = Content.CreateNew(typeName, parent.ContentHandler, name);
			content.Save();
		}
		private string DecodePath(string encodedPath)
		{
			return encodedPath.Replace("[TestRoot]", this.TestRoot.Path);
		}

	}
}