using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.ContentRepository.Tests
{
    /// <summary>
    /// Summary description for TrashTest
    /// </summary>
    [TestClass]
    public class TrashTest : TestBase
    {
        // --- TestContext
        // ---------------
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
        
        // --- PlayGround
        // --------------
        
        private const string _testRootName = "TrashTestContext";
        private static readonly string _testRootPath = String.Concat("/Root/", _testRootName);

        private const string testFieldValueForT1 = "ContentList1Field3 test";
        private const decimal testFieldValueForT2 = 10;

        private enum Check { Signature, FieldValuesKept, FieldValuesNotKept }

        private static string _testTrashName = "Trash";
        private static string _testTrashPath = String.Concat("/Root/", _testTrashName);

        //This will be the default configuration for the trash, which is valid when each test is starting to run
        private const int _minRetentionTime = 10;
        private const int _bagCapacity = 10;
        private const int _sizeQuota = 100;
        private const bool _isActive = true;

        [ClassInitialize]
        public static void Initialize(TestContext tc)
        {
            var node = NodeType.CreateInstance("Folder", Node.LoadNode("/Root"));
            node.Name = _testRootName;
            node.Save();

            //node.Security.SetPermission(Visitor, true, PermissionType.See, PermissionValue.Allow);
            //node.Security.SetPermission(Visitor, true, PermissionType.SeePermissions, PermissionValue.Allow);
            //node.Save();
        }

        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.LoadNode(_testRootPath).ForceDelete();

            if (Node.Exists(_testTrashPath))
                Node.LoadNode(_testTrashPath).ForceDelete();
        }

        [TestInitialize]
        public void PrepareTest()
        {
            var testRoot = Node.Load<Folder>(_testRootPath);

            if (testRoot != null)
            {
                foreach (var node in testRoot.Children)
                {
                    node.ForceDelete();
                }
            }

            var trashbin = Node.LoadNode(_testTrashPath);

            if (trashbin != null)
            {
                foreach (var node in ((Folder)trashbin).Children)
                {
                    var trashBag = node as TrashBag;

                    if (trashBag != null)
                    {
                        trashBag.KeepUntil = DateTime.Today.AddDays(-1);
                        trashBag.ForceDelete();
                    }
                    else
                    {
                        node.ForceDelete();
                    }
                }

                ConfigureTrash(_minRetentionTime, _sizeQuota, _bagCapacity, _isActive);
            }
            else
            {
                CreateTrash("/Root", _testTrashName, _minRetentionTime, _sizeQuota, _bagCapacity, _isActive);
            }

        }
        

        #region ListDefs
        private const string _listDef1 = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<DisplayName>Cars title</DisplayName>
	<Description>Cars description</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<ContentListField name='#ContentList1Field1' type='ShortText'>
			<DisplayName>ContentList1Field1</DisplayName>
			<Description>ContentList1Field1 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ContentList1Field2' type='WhoAndWhen'>
			<DisplayName>ContentList1Field2</DisplayName>
			<Description>ContentList1Field2 Description</Description>
			<Icon>icon.gif</Icon>
			<Configuration>
			</Configuration>
		</ContentListField>
		<ContentListField name='#ContentList1Field3' type='ShortText'>
			<DisplayName>ContentList1Field3</DisplayName>
			<Description>ContentList1Field3 Description</Description>
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
		<ContentListField name='#ContentList2Field1' type='Integer' />
		<ContentListField name='#ContentList2Field2' type='Number' />
	</Fields>
</ContentListDefinition>
";
        #endregion

        // --- Tests
        // ---------
        
        #region simple move tests
        [TestMethod]
        public void Move_ListItem_ToOtherLocation()
        {
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/OtherLocation");

            MoveNode("[TestRoot]/SourceContentListT1/SourceContentListItem1", "[TestRoot]/OtherLocation");
            
            CheckSimpleNode("[TestRoot]/OtherLocation/SourceContentListItem1");
        }
        
        [TestMethod]
        public void Move_ListItem_ToOtherContentListOftheSameType()
        {
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TargetContentListT1");

            MoveNode("[TestRoot]/SourceContentListT1/SourceContentListItem1", "[TestRoot]/TargetContentListT1");

            CheckContentListItem1("[TestRoot]/TargetContentListT1/SourceContentListItem1",Check.FieldValuesNotKept);
        }

        #endregion
        
        #region ContentListItem and TrashBag tests

        [TestMethod]
        public void Move_ListItemToTrashBag()
        {
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TrashBag1");

            MoveNode("[TestRoot]/SourceContentListT1/SourceContentListItem1", "[TestRoot]/TrashBag1");

            CheckContentListItem1("[TestRoot]/TrashBag1/SourceContentListItem1",Check.FieldValuesKept);
        }
        
        [TestMethod]
        public void Move_ListItem_ToTrashBag_ToOtherLocation()
        {
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TrashBag1");
            EnsureNode("[TestRoot]/OtherLocation");

            MoveNode("[TestRoot]/SourceContentListT1/SourceContentListItem1", "[TestRoot]/TrashBag1");
            MoveNode("[TestRoot]/TrashBag1/SourceContentListItem1", "[TestRoot]/OtherLocation");

            CheckSimpleNode("[TestRoot]/OtherLocation/SourceContentListItem1");
         }

        [TestMethod]
        public void Move_ListItem_ToTrashBag_ToOriginalLocation()
        {
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TrashBag1");
            
            MoveNode("[TestRoot]/SourceContentListT1/SourceContentListItem1", "[TestRoot]/TrashBag1");
            MoveNode("[TestRoot]/TrashBag1/SourceContentListItem1", "[TestRoot]/SourceContentListT1");

            CheckContentListItem1("[TestRoot]/SourceContentListT1/SourceContentListItem1",Check.FieldValuesKept);
        }

        [TestMethod]
        public void Move_ListItem_ToTrashBag_ToContentListOfAnotherType()
        {
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TrashBag1");
            EnsureNode("[TestRoot]/TargetContentListT2");

            MoveNode("[TestRoot]/SourceContentListT1/SourceContentListItem1", "[TestRoot]/TrashBag1");
            MoveNode("[TestRoot]/TrashBag1/SourceContentListItem1", "[TestRoot]/TargetContentListT2");

            CheckContentListItem2("[TestRoot]/TargetContentListT2/SourceContentListItem1",Check.FieldValuesNotKept);
        }

        [TestMethod]
        public void Move_ListItem_ToTrashBag_ToAnotherContentListOfTheSameType()
        {
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TrashBag1");
            EnsureNode("[TestRoot]/TargetContentListT1");

            MoveNode("[TestRoot]/SourceContentListT1/SourceContentListItem1", "[TestRoot]/TrashBag1");
            MoveNode("[TestRoot]/TrashBag1/SourceContentListItem1", "[TestRoot]/TargetContentListT1");

            CheckContentListItem1("[TestRoot]/TargetContentListT1/SourceContentListItem1",Check.FieldValuesNotKept);
        }

        #endregion

        #region ContentList and Trashbag tests

        [TestMethod]
        public void Move_ContentList_ToTrashBag_ToOriginalLocation()
        {
           
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TrashBag1");
            
            MoveNode("[TestRoot]/SourceContentListT1", "[TestRoot]/TrashBag1");
            MoveNode("[TestRoot]/TrashBag1/SourceContentListT1", "[TestRoot]");

            CheckContentList1("[TestRoot]/SourceContentListT1");
            CheckContentListItem1("[TestRoot]/SourceContentListT1/SourceContentListItem1",Check.FieldValuesKept);
            
        }

        [TestMethod]
        public void Move_ContentList_ToTrashBag_ToOtherLocation()
        {
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TrashBag1");
            EnsureNode("[TestRoot]/TargetFolder");

            MoveNode("[TestRoot]/SourceContentListT1", "[TestRoot]/TrashBag1");
            MoveNode("[TestRoot]/TrashBag1/SourceContentListT1", "[TestRoot]/TargetFolder");

            CheckContentList1("[TestRoot]/TargetFolder/SourceContentListT1");
            CheckContentListItem1("[TestRoot]/TargetFolder/SourceContentListT1/SourceContentListItem1",Check.FieldValuesKept);
            
        }

        [TestMethod]
        public void Move_ContentListUnderAFolder_ToTrashBag_ToOtherLocation()
        {
           
            EnsureNode("[TestRoot]/SourceFolder/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TrashBag1");
            EnsureNode("[TestRoot]/TargetFolder");

            MoveNode("[TestRoot]/SourceFolder", "[TestRoot]/TrashBag1");
            MoveNode("[TestRoot]/TrashBag1/SourceFolder", "[TestRoot]/TargetFolder");

            CheckContentList1("[TestRoot]/TargetFolder/SourceFolder/SourceContentListT1");
            CheckContentListItem1("[TestRoot]/TargetFolder/SourceFolder/SourceContentListT1/SourceContentListItem1", Check.FieldValuesKept);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Move_ContentList_ToTrashBag_ToUnderAContantList()
        {
            EnsureNode("[TestRoot]/SourceContentListT1/SourceContentListItem1");
            EnsureNode("[TestRoot]/TrashBag1");
            EnsureNode("[TestRoot]/TargetContentListT1");

            MoveNode("[TestRoot]/SourceContentListT1", "[TestRoot]/TrashBag1");
            MoveNode("[TestRoot]/TrashBag1/SourceContentListT1", "[TestRoot]/TargetContentListT1");

            CheckContentList1("[TestRoot]/TargetFolder/SourceContentListT1");
            CheckContentListItem1("[TestRoot]/TargetFolder/SourceContentListT1/SourceContentListItem1",Check.FieldValuesKept);

        }

        #endregion
        
        #region TrashBin singleton tests

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TrashBin_CreateNotTheSpecifiedLocation()
        {
            EnsureNode("[TestRoot]/Trash");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TrashBin_CreateNotTheSpecifiedName()
        {
            EnsureNode("/Root/TrashBin");
        }

        [TestMethod]
        [ExpectedException(typeof(NodeAlreadyExistsException))]
        public void TrashBin_CreateAnotherTrashToTheSameLocation()
        {
            CreateTrash("/Root", "Trash", 10, 10, 10, true);
        }

        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        //public void TrashBin_DeleteTrash_WithDelete()
        //{
        //    DeleteWithDelete("/Root/Trash");
        //}

        //[TestMethod]
        //[ExpectedException(typeof(InvalidContentException))]
        //public void TrashBin_WithInvalidBagCapacity()
        //{
        //    Content cnt = Content.Load(_testTrashPath);

        //    cnt["BagCapacity"] = -1;

        //    cnt.Save();
        //}

        //[TestMethod]
        //[ExpectedException(typeof(InvalidContentException))]
        //public void TrashBin_WithInvalidSizeQuota()
        //{
        //    Content cnt = Content.Load(_testTrashPath);

        //    cnt["SizeQuota"] = -1;

        //    cnt.Save();
        //}

        [TestMethod]
        [ExpectedException(typeof(InvalidContentException))]
        public void TrashBin_WithInvalidMinRetentionTime()
        {
            var cnt = Content.Load(_testTrashPath);

            cnt["MinRetentionTime"] = -1;

            cnt.Save();
        }

        #endregion

        #region delete test without trash

        [TestMethod]
        public void Delete_UsingDeleteNode_WithoutTrashBin()
        {
            DeleteMainTrash();

            var sampleContent = EnsureNode("[TestRoot]/SampleContent");

            DeleteWithDeleteNode("[TestRoot]/SampleContent");

            CheckDeletedEventual("[TestRoot]/SampleContent", sampleContent);
        }


        #endregion

        #region delete tests

        [TestMethod]
        public void Delete_UsingDeleteNode_WithTrashBin()
        {
            var sampleContentId = EnsureNode("[TestRoot]/SampleContent");

            DeleteWithDeleteNode("[TestRoot]/SampleContent");

            CheckMovedToTrash("[TestRoot]/SampleContent", sampleContentId);
            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Delete_UsingDelete_WithTrashBin()
        {
            var sampleContentId = EnsureNode("[TestRoot]/SampleContent");

            DeleteWithDelete("[TestRoot]/SampleContent");

            CheckMovedToTrash("[TestRoot]/SampleContent", sampleContentId);
            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Delete_UsingDelete_NodeWithTrashDisabledTrueInParent()
        {
            var sampleContent = EnsureNode("[TestRoot]/Folder/SampleContent");
            DisabelTrashForThisNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder/SampleContent");

            CheckDeletedEventual("[TestRoot]/Folder/SampleContent", sampleContent);
            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Delete_UsingDelete_NodeWithTrashDisabledTrueInContent()
        {
            var folderContent = EnsureNode("[TestRoot]/Folder");
            DisabelTrashForThisNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            CheckDeletedEventual("[TestRoot]/Folder", folderContent);
            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Delete_UsingDelete_NodeWithTrashDisabledTrueInParentsParent()
        {
            var sampleContent = EnsureNode("[TestRoot]/Folder/Folder/SampleContent");
            DisabelTrashForThisNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder/Folder/SampleContent");

            CheckContentInTrash(sampleContent);
            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Delete_UsingDeleteNode_WithInactiveTrashBin()
        {
            ConfigureTrash(10, 10, 10, false);
            
            var sampleContent = EnsureNode("[TestRoot]/SampleContent");

            DeleteWithDeleteNode("[TestRoot]/SampleContent");

            CheckDeletedEventual("[TestRoot]/SampleContent", sampleContent);
            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Delete_UsingDelete_WithInactiveTrashBin()
        {
            ConfigureTrash(10, 10, 10, false);

            var sampleContent = EnsureNode("[TestRoot]/SampleContent");

            DeleteWithDelete("[TestRoot]/SampleContent");

            CheckDeletedEventual("[TestRoot]/SampleContent",sampleContent);
            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Delete_UsingForceDelete_WithTrashBin()
        {
            var sampleContent = EnsureNode("[TestRoot]/SampleContent");

            DeleteWithForceDelete("[TestRoot]/SampleContent");

            CheckDeletedEventual("[TestRoot]/SampleContent", sampleContent);
            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void DeleteMoreThanOneItem_UsingDelete()
        {
            var sc1Id = EnsureNode("[TestRoot]/SampleContent1");
            var sc2Id = EnsureNode("[TestRoot]/SampleContent2");
            var sc3Id = EnsureNode("[TestRoot]/SampleContent3");
            var folderId = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/SampleContent1");
            DeleteWithDelete("[TestRoot]/SampleContent2");
            DeleteWithDelete("[TestRoot]/SampleContent3");
            DeleteWithDelete("[TestRoot]/Folder");

            CheckMovedToTrash("[TestRoot]/SampleContent1", sc1Id);
            CheckMovedToTrash("[TestRoot]/SampleContent2", sc2Id);
            CheckMovedToTrash("[TestRoot]/SampleContent3", sc3Id);
            CheckMovedToTrash("[TestRoot]/Folder", folderId);
            CheckNumberOfItemsInTrash(4);
        }

        [TestMethod]
        public void DeleteMoreThanOneItem_WithTheSameNameAndTitle()
        {
            var sc1Id = EnsureNode("[TestRoot]/SampleContent1");
            var sc2Id = EnsureNode("[TestRoot]/Folder/SampleContent1");

            DeleteWithDelete("[TestRoot]/SampleContent1");
            DeleteWithDelete("[TestRoot]/Folder/SampleContent1");
            
            CheckMovedToTrash("[TestRoot]/SampleContent1", sc1Id);
            CheckMovedToTrash("[TestRoot]/Folder/SampleContent2", sc2Id);
            CheckNumberOfItemsInTrash(2);
        }

        #endregion

        #region tests with bag limit
        [TestMethod]
        public void Delete_UsingDeleteNode_ReachBagLimit()
        {
            ConfigureTrash(10, 10, 2, true);

            var folderId = EnsureNode("[TestRoot]/Folder");
            EnsureNode("[TestRoot]/Folder/Sample1");
            
            DeleteWithDeleteNode("[TestRoot]/Folder");

            CheckMovedToTrash("[TestRoot]/Folder",folderId);
            CheckNumberOfItemsInTrash(1);
        }
        
        [TestMethod]
        public void Delete_UsingDelete_ReachBagLimit()
        {

            ConfigureTrash(10, 10, 2, true);

            var folderId = EnsureNode("[TestRoot]/Folder");
            EnsureNode("[TestRoot]/Folder/Sample1");

            DeleteWithDelete("[TestRoot]/Folder");

            CheckMovedToTrash("[TestRoot]/Folder",folderId);
            CheckNumberOfItemsInTrash(1);
        }
        
        [TestMethod]
        public void Delete_UsingDeleteNode_ExceedBagLimit()
        {
            var exceptionThrown = false;

            ConfigureTrash(10, 10, 2, true);

            EnsureNode("[TestRoot]/Folder/Sample1");
            EnsureNode("[TestRoot]/Folder/Sample2");

            try
            {
                DeleteWithDeleteNode("[TestRoot]/Folder");
            }
            catch (ApplicationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "ApplicationException exception should be thrown.");
            CheckNumberOfItemsInTrash(0);
            
        }
        
        [TestMethod]
        public void Delete_UsingDelete_ExceedBagLimit()
        {
            var exceptionThrown = false;

            ConfigureTrash(10, 10, 2, true);

            EnsureNode("[TestRoot]/Folder/Sample1");
            EnsureNode("[TestRoot]/Folder/Sample2");

            try
            {
                DeleteWithDelete("[TestRoot]/Folder");
            }
            catch (ApplicationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "ApplicationException exception should be thrown.");
            CheckNumberOfItemsInTrash(0);
        }

        //TODO: ezt at kell irni attol fuggoen, hogy a trash disabled hogyan valtozik
        //akkor lehet ezt visszatenni, ha TrashDisabled attributum vizsgalata nem csak az eppen torolt
        //contentre, hanem a content childrenjeire is vizsgalva lesz
        //[TestMethod]
        //public void Delete_UsingDeleteNode_ReachBagLimit_OneNodeWithTrashDisabledTrue()
        //{
        
        //    CreateTrash("/Root", "Trash", 10, 10, 2, true);

        //    EnsureNode("[TestRoot]/Folder/Sample1");
        //    EnsureNode("[TestRoot]/Folder/Sample2");

        //    DisabelTrashForThisNode("[TestRoot]/Folder/Sample2");

        //    DeleteWithDeleteNode("[TestRoot]/Folder");
        //}
        #endregion

        #region delete trashbag

        [TestMethod]
        public void DeleteFromTrash_MinRetentionTimeNotReached()
        {

            var exceptionThrown = false;
            
            ConfigureTrash(1, 10, 2, true);

            var sampleFolder = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            try
            {
                DeleteFromTrash(sampleFolder);
            }
            catch (ApplicationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "ApplicationException exception should be thrown.");
            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void DeleteFromTrash_MinRetentionChanged()
        {

            var exceptionThrown = false;
            
            ConfigureTrash(1, 10, 2, true);

            var sampleFolder = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            ConfigureTrash(0, null, null, null);

            try
            {
                DeleteFromTrash(sampleFolder);
            }
            catch (ApplicationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "ApplicationException exception should be thrown.");
            CheckNumberOfItemsInTrash(1);
        }

        #endregion

        #region Purge

        [TestMethod]
        public void Purge_MinRetentionTimeNotReached()
        {

            ConfigureTrash(1, 10, 2, true);

            var sc1Id = EnsureNode("[TestRoot]/SampleContent1");
            var sc2Id = EnsureNode("[TestRoot]/SampleContent2");
            var sc3Id = EnsureNode("[TestRoot]/SampleContent3");
            var folderId = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/SampleContent1");
            DeleteWithDelete("[TestRoot]/SampleContent2");
            DeleteWithDelete("[TestRoot]/SampleContent3");
            DeleteWithDelete("[TestRoot]/Folder");

            TrashBin.Purge();

            CheckMovedToTrash("[TestRoot]/SampleContent1", sc1Id);
            CheckMovedToTrash("[TestRoot]/SampleContent2", sc2Id);
            CheckMovedToTrash("[TestRoot]/SampleContent3", sc3Id);
            CheckMovedToTrash("[TestRoot]/Folder", folderId);

            CheckNumberOfItemsInTrash(4);
        }

        [TestMethod]
        public void Purge_MinRetentionTimeSomeNotReached()
        {

            ConfigureTrash(1, 10, 2, true);

            var sc1Id = EnsureNode("[TestRoot]/SampleContent1");
            var sc2Id = EnsureNode("[TestRoot]/SampleContent2");
            var sc3Id = EnsureNode("[TestRoot]/SampleContent3");
            var folderId = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/SampleContent1");
            DeleteWithDelete("[TestRoot]/SampleContent2");


            ConfigureTrash(0, null, null, null);
            DeleteWithDelete("[TestRoot]/SampleContent3");
            DeleteWithDelete("[TestRoot]/Folder");

            TrashBin.Purge();

            CheckMovedToTrash("[TestRoot]/SampleContent1", sc1Id);
            CheckMovedToTrash("[TestRoot]/SampleContent2", sc2Id);
            CheckDeletedEventual("[TestRoot]/SampleContent3", sc3Id);
            CheckDeletedEventual("[TestRoot]/Folder", folderId);

            CheckNumberOfItemsInTrash(2);

        }

        [TestMethod]
        public void Purge_WithOtherUserWithoutDeletePermission()
        {

            ConfigureTrash(0, null, null, null);
            
            var folderId = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            using (new TestEquipment.ContextSimulator(User.Visitor))
            {
                TrashBin.Purge();  
            }

            Assert.IsTrue(CheckContentInTrash(folderId), "Trashbag shouldn't be deleted!");
            CheckNumberOfItemsInTrash(1);
            
        }

        //[TestMethod]
        //public void Purge_WithOtherUserWithDeletePermissionWithoutSeePermission()
        //{

        //    ConfigureTrash(0, null, null, null);
        //    var folderId = EnsureNode("[TestRoot]/Folder");

        //    var pdescriptors = new List<TestEquipment.PermissionDescriptor>
        //      {
        //          new TestEquipment.PermissionDescriptor
        //              {
        //                  AffectedPath = DecodePath("[TestRoot]/Folder"),
        //                  AffectedUser = User.Visitor,
        //                  PType = PermissionType.See,
        //                  NewValue = PermissionValue.Deny
        //              },
        //              new TestEquipment.PermissionDescriptor
        //              {
        //                  AffectedPath = DecodePath("[TestRoot]/Folder"),
        //                  AffectedUser = User.Visitor,
        //                  PType = PermissionType.Delete,
        //                  NewValue = PermissionValue.Allow
        //              }
        //      };

        //    TestEquipment.SetPermissions(pdescriptors);

        //    DeleteWithDelete("[TestRoot]/Folder");
            
        //    using (new TestEquipment.ContextSimulator(User.Visitor))
        //    {
        //        TrashBin.Purge();
        //    }

        //    Assert.IsTrue(CheckContentInTrash(folderId), "Trashbag shouldn't be deleted!");
        //    CheckNumberOfItemsInTrash(1);
      
        //}

        
        [TestMethod]
        public void Purge_WithOtherUserWithoutDeletePermission2()
        {

            ConfigureTrash(0, null, null, null);

            var folderId = EnsureNode("[TestRoot]/Folder");
            EnsureNode("[TestRoot]/Folder/SampleContent1");
            
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/Folder"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Delete,
                          NewValue = PermissionValue.Allow
                      },
                      new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/Folder/SampleContent1"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Delete,
                          NewValue = PermissionValue.Deny
                      }
              };

            TestEquipment.SetPermissions(pdescriptors);

            DeleteWithDelete("[TestRoot]/Folder");

            using (new TestEquipment.ContextSimulator(User.Visitor))
            {
                TrashBin.Purge();
            }

            Assert.IsTrue(CheckContentInTrash(folderId), "Trashbag shouldn't be deleted!");
            CheckNumberOfItemsInTrash(1);

        }

        #endregion

        #region permissions

        [TestMethod]
        public void Delete_WithDelete_WithoutDeletePermissions()
        {
            var exceptionThrown = false;
            
            var sampleContent = EnsureNode("[TestRoot]/SampleContent");
            
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/SampleContent"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Delete,
                          NewValue = PermissionValue.Deny
                      }
              };

            TestEquipment.SetPermissions(pdescriptors);
            
            try
            {
                using (new TestEquipment.ContextSimulator(User.Visitor))
                {
                    DeleteWithDelete("[TestRoot]/SampleContent"); 
                }
                
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            CheckNotDeleted("[TestRoot]/SampleContent", sampleContent);
            Assert.IsTrue(exceptionThrown, "SenseNetSecurity exception should be thrown.");
            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Delete_WithForceDelete_WithoutDeletePermissions()
        {
            var exceptionThrown = false;
            
            var sampleContent = EnsureNode("[TestRoot]/SampleContent");
            
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/SampleContent"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Delete,
                          NewValue = PermissionValue.Deny
                      }
              };

            TestEquipment.SetPermissions(pdescriptors);
            
            try
            {
                using (new TestEquipment.ContextSimulator(User.Visitor))
                {
                    DeleteWithForceDelete("[TestRoot]/SampleContent");
                }
            }
            catch (SenseNetSecurityException)
            {
                exceptionThrown = true;
            }

            CheckNotDeleted("[TestRoot]/SampleContent",sampleContent);
            Assert.IsTrue(exceptionThrown,"InvalidOperation exception should be thrown.");
            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Delete_WithDelete_WithoutDeletePermissionsToSubContent()
        {
            var exceptionThrown = false;
            
            var sampleFolder = EnsureNode("[TestRoot]/Folder");
            var sampleContent = EnsureNode("[TestRoot]/Folder/SampleContent");
            
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/Folder"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Delete,
                          NewValue = PermissionValue.Allow
                      },
                      new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/Folder/SampleContent"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Delete,
                          NewValue = PermissionValue.Deny
                      }
              };

            TestEquipment.SetPermissions(pdescriptors);
            
            try
            {
                using (new TestEquipment.ContextSimulator(User.Visitor))
                {
                    DeleteWithDelete("[TestRoot]/Folder");
                }
                
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            CheckNotDeleted("[TestRoot]/Folder", sampleFolder);
            CheckNotDeleted("[TestRoot]/Folder/SampleContent", sampleContent);
            Assert.IsTrue(exceptionThrown, "InvalidOperation exception should be thrown.");
            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Delete_WithForceDelete_WithoutDeletePermissionsToSubContent()
        {
            var exceptionThrown = false;

            var sampleFolder = EnsureNode("[TestRoot]/Folder");
            var sampleContent = EnsureNode("[TestRoot]/Folder/SampleContent");
            
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/Folder"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Delete,
                          NewValue = PermissionValue.Allow
                      },
                      new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/Folder/SampleContent"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Delete,
                          NewValue = PermissionValue.Deny
                      }
              };

            TestEquipment.SetPermissions(pdescriptors);
            
            try
            {
                using (new TestEquipment.ContextSimulator(User.Visitor))
                {
                    DeleteWithForceDelete("[TestRoot]/Folder");
                }
            }
            catch (SenseNetSecurityException)
            {
                exceptionThrown = true;
            }
            
            CheckNotDeleted("[TestRoot]/Folder", sampleFolder);
            CheckNotDeleted("[TestRoot]/Folder/SampleContent", sampleContent);
            Assert.IsTrue(exceptionThrown, "InvalidOperation exception should be thrown.");
            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Delete_WithDelete_LookingAtTrashWithAntherUser()
        {
            var sampleContent = EnsureNode("[TestRoot]/SampleContent");
            
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/SampleContent"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.See,
                          NewValue = PermissionValue.Deny
                      }
              };

            TestEquipment.SetPermissions(pdescriptors);
            
            DeleteWithDelete("[TestRoot]/SampleContent");

            using (new TestEquipment.ContextSimulator(User.Visitor))
            {
                Assert.IsFalse(CheckContentInTrash(sampleContent), "Visitor shouldn't see sampleContent in trash.");
                CheckNumberOfItemsInTrash(0);
            }
            
        }

        #endregion

        #region Restore

        [TestMethod]
        public void Restore_ToEmptyLocation()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            RestoreFromTrash(sampleFolder, "", false, RestoreResultType.Nonedefined);

            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Restore_ToNotValidLocation()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            RestoreFromTrash(sampleFolder, "hgfjghgfjkdhgdf154564678gdfas", false, RestoreResultType.NoParent);

            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Restore_EmptyTrashbag()
        {
            var sampleFolder = EnsureNode("/Root/Trash/TrashBag1");

            var tb = Node.Load<TrashBag>(sampleFolder);

            TrashBin.Restore(tb, DecodePath("[TestRoot]/"));
        }
        
        [TestMethod]
        public void Restore_ToTheSameLocation()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            RestoreFromTrash(sampleFolder, null, false, RestoreResultType.Success);

            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Restore_ToTheSameLocationButSameNameExist()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            EnsureNode("[TestRoot]/Folder");

            RestoreFromTrash(sampleFolder, null, false, RestoreResultType.ExistingName);

            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Restore_ToTheSameLocationButSameNameExistNewNameEnabled()
        {
            
            var sampleFolder = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            EnsureNode("[TestRoot]/Folder");

            RestoreFromTrash(sampleFolder, null, true, RestoreResultType.Success);

            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Restore_ToTheSameLocationButParentNotExist()
        {
            var sampleContent1 = EnsureNode("[TestRoot]/Folder/SampleContent1");

            DeleteWithDelete("[TestRoot]/Folder/SampleContent1");
            DeleteWithDelete("[TestRoot]/Folder");
            
            RestoreFromTrash(sampleContent1, null, false, RestoreResultType.NoParent);

            CheckNumberOfItemsInTrash(2);
        }

        //TODO: lehet, hogy folosleges
        [TestMethod]
        public void Restore_ToTheSameLocationButSameFolderExist()
        {
            var mainFolder = EnsureNode("[TestRoot]/Folder");
            var sampleContent1 = EnsureNode("[TestRoot]/Folder/Folder/SampleContent1");

            DeleteWithDelete("[TestRoot]/Folder");

            var newFolder = EnsureNode("[TestRoot]/Folder");

            RestoreFromTrash(mainFolder, null, false, RestoreResultType.ExistingName);

            CheckNumberOfItemsInTrash(1);
            
        }

        //TODO: lehet, hogy folosleges
        [TestMethod]
        public void Restore_ToTheSameLocationButSameFolderExistNewNameEnabled()
        {
            var mainFolder = EnsureNode("[TestRoot]/Folder");
            var sampleContent1 = EnsureNode("[TestRoot]/Folder/Folder/SampleContent1");

            DeleteWithDelete("[TestRoot]/Folder");

            EnsureNode("[TestRoot]/Folder");

            RestoreFromTrash(mainFolder, null, true, RestoreResultType.Success);

            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Restore_ToAnotherExistingLocation()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");
            EnsureNode("[TestRoot]/OtherLocation");

            DeleteWithDelete("[TestRoot]/Folder");

            RestoreFromTrash(sampleFolder, "[TestRoot]/OtherLocation", false, RestoreResultType.Success);

            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Restore_ToAnotherNotExistingLocation()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");
            
            DeleteWithDelete("[TestRoot]/Folder");

            RestoreFromTrash(sampleFolder, "[TestRoot]/OtherLocation", false, RestoreResultType.NoParent);

            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Restore_ToAnotherLocationUnderANonFolderLocation()
        {

            var sampleFolder = EnsureNode("[TestRoot]/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            EnsureNode("[TestRoot]/Car");

            RestoreFromTrash(sampleFolder, "[TestRoot]/Car", false, RestoreResultType.ForbiddenContentType);

            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Restore_ToAnotherExistingLocationButSameNameExists()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");
            EnsureNode("[TestRoot]/OtherLocation/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            RestoreFromTrash(sampleFolder, "[TestRoot]/OtherLocation", false, RestoreResultType.ExistingName);

            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Restore_ToAnotherExistingLocationButSameNameExistsNewNameEnabled()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");
            EnsureNode("[TestRoot]/OtherLocation/Folder");

            DeleteWithDelete("[TestRoot]/Folder");

            RestoreFromTrash(sampleFolder, "[TestRoot]/OtherLocation", true, RestoreResultType.Success);

            CheckNumberOfItemsInTrash(0);
        }

        [TestMethod]
        public void Restore_ToAnotherExistingLocationWithoutOpenPermission()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");
            EnsureNode("[TestRoot]/OtherLocation");

            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/OtherLocation"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Open,
                          NewValue = PermissionValue.Deny
                      }
              };

            TestEquipment.SetPermissions(pdescriptors);
            
            DeleteWithDelete("[TestRoot]/Folder");

            using (new TestEquipment.ContextSimulator(User.Visitor))
            {
                RestoreFromTrash(sampleFolder, "[TestRoot]/OtherLocation", false, RestoreResultType.PermissionError);    
            }
            
            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Restore_ToAnotherExistingLocationWithoutAddNewPermission()
        {
            var sampleFolder = EnsureNode("[TestRoot]/Folder");
            EnsureNode("[TestRoot]/OtherLocation");

            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/OtherLocation"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.AddNew,
                          NewValue = PermissionValue.Deny
                      }
              };

            TestEquipment.SetPermissions(pdescriptors);
            
            DeleteWithDelete("[TestRoot]/Folder");

            using (new TestEquipment.ContextSimulator(User.Visitor))
            {
                RestoreFromTrash(sampleFolder, "[TestRoot]/OtherLocation", false, RestoreResultType.PermissionError); 
            }
            
            CheckNumberOfItemsInTrash(1);
        }

        [TestMethod]
        public void Restore_ToTheSameLocationButParentTypeChanged()
        {

            EnsureNode("/Root/Trash");

            var sampleContent1 = EnsureNode("[TestRoot]/Folder/SampleContent1");

            DeleteWithDelete("[TestRoot]/Folder/SampleContent1");
            DeleteWithDelete("[TestRoot]/Folder");

            //FakeFolder breaks the logic of Ensure node: jump to EnsureNode function and take a look at the hack
            EnsureNode("[TestRoot]/FakeFolder");

            RestoreFromTrash(sampleContent1, null, false, RestoreResultType.ForbiddenContentType);
            CheckNumberOfItemsInTrash(2);
        }
        
        #endregion

        #region abnormal scenarios

        //[TestMethod]
        //public void Move_AnItemToTrashWithMove()
        //{
        //    var sample1 = EnsureNode("[TestRoot]/Sample1");
            
        //    MoveNode("[TestRoot]/Sample1", "/Root/Trash");

        //    var trashbin = LoadTrash();

        //    Assert.IsTrue(trashbin.ChildCount == 0, "Trash should be empty!");
        //}

        //[TestMethod]
        //public void Copy_AnItemToTrashWithCopy()
        //{
        //    var sample1 = EnsureNode("[TestRoot]/Sample1");

        //    Node.Copy(DecodePath("[TestRoot]/Sample1"), "/Root/Trash");

        //    var trashbin = LoadTrash();

        //    Assert.IsTrue(trashbin.ChildCount == 0, "Trash should be empty!");
        //}

        #endregion

        #region Multiple delete tests

        //[TestMethod]
        //public void MultipleDelete_UsingDelete_WithTrashBin()
        //{
        //    int itemCount = 10;

        //    int[] itemIds = new int[itemCount];
        //    string[] itemPaths = new string[itemCount];

        //    for (int i = 0; i < itemCount; i++)
        //    {
        //        var itemPath = String.Concat("[TestRoot]/Car", i);

        //        itemPaths[i] = itemPath;
        //        itemIds[i] = EnsureNode(itemPath);
        //    }
            
        //    //TODO: ide kell majd betenni a batch deletet
        //    for (int i = 0; i < itemCount; i++)
        //    {
        //        DeleteWithDelete(itemPaths[i]);
        //        Thread.Sleep(20);
        //    }
        //    //TODO: eddig

        //    for (int i = 0; i < itemCount; i++)
        //    {
        //        CheckMovedToTrash(itemPaths[i], itemIds[i]);
        //    }
            
        //    CheckNumberOfItemsInTrash(itemCount);
        //}

        //TODO: refactor
        //[TestMethod]
        //public void MultipleDelete_UsingDelete_WithoutTrashBinWithSufficientPermissions()
        //{
        //    DeleteMainTrash();
            
        //    int itemCount = 50;

        //    int[] itemIds = new int[itemCount];
        //    string[] itemPaths = new string[itemCount];
            
        //    for (int i = 0; i < itemCount; i++)
        //    {
        //        var itemPath = String.Concat("[TestRoot]/Car", i);

        //        itemPaths[i] = itemPath;
        //        itemIds[i] = EnsureNode(itemPath);
        //    }

        //    List<Exception> errors = new List<Exception>();

        //    Node.Delete(itemIds.ToList(), ref errors);

        //    //Asserts
        //    for (int i = 0; i < itemCount; i++)
        //    {
        //        CheckDeletedEventual(itemPaths[i], itemIds[i]);
        //    }

        //    Assert.AreEqual(0, errors.Count, "The number of errors during the process should be 0");

        //    CheckNumberOfItemsInTrash(0);
        //}

        //[TestMethod]
        //public void MultipleDelete_UsingDelete_WithoutTrashBinWithSufficientPermissionsWithCustomStructure()
        //{
        //    DeleteMainTrash();
            
        //    int folderDepth = 3;

        //    //the ids and paths of all stakeholders of this test
        //    List<ContentDescriptor> items = new List<ContentDescriptor>();

        //    //the roots of the tree building process
        //    List<string> folders = new List<string>();
        //    folders.Add("[TestRoot]");

        //    //Build the tree
        //    BuildTree(ref folders,folderDepth,ref items);

        //    //the ids of the contents to be deleted
        //    List<int> itemsToDelete = new List<int>();
        //    Node folder1 = LoadNode("[TestRoot]/Folder1");
        //    Node folder2 = LoadNode("[TestRoot]/Folder2");
        //    itemsToDelete.Add(folder1.Id);
        //    itemsToDelete.Add(folder2.Id);

        //    //the ids of the errors
        //    List<Exception> errors = new List<Exception>();
            
        //    Node.Delete(itemsToDelete, ref errors);

        //    foreach (ContentDescriptor item in items)
        //    {
        //        CheckDeletedEventual(item.Path,item.Id);
        //    }

        //    Assert.AreEqual(0, errors.Count, "The number of errors during the process should be 0");

        //    CheckNumberOfItemsInTrash(0);
        //}

        #endregion

        
        
        // ---- Move helper
        // ----------------

        private static void MoveNode(string encodedSourcePath, string encodedTargetPath)
        {
            string sourcePath = DecodePath(encodedSourcePath);
            string targetPath = DecodePath(encodedTargetPath);
            int sourceId = Node.LoadNode(sourcePath).Id;
            int targetId = Node.LoadNode(targetPath).Id;

            Node.Move(sourcePath, targetPath);

            Node parentNode = Node.LoadNode(targetId);
            Node childNode = Node.LoadNode(sourceId);
            Assert.IsTrue(childNode.ParentId == parentNode.Id, "Source was not moved.");
        }
        
        // ---- Validate results helper
        // ----------------------------

        private static void CheckSimpleNode(string encodedPath)
        {
            var node = LoadNode(encodedPath);
            var content = Content.Create(node);

            Assert.IsTrue(node.ContentListId == 0, "ContentListId is not 0");
            Assert.IsNull(node.ContentListType, "ContentListType is not null");
            Assert.IsFalse(content.Fields.ContainsKey("#ContentList1Field1"), "This content shouldn't contain #ContentList1Field1 field.");
            Assert.IsFalse(content.Fields.ContainsKey("#ContentList1Field2"), "This content shouldn't contain #ContentList1Field2 field.");
            Assert.IsFalse(content.Fields.ContainsKey("#ContentList1Field3"), "This content shouldn't contain #ContentList1Field3 field.");
            Assert.IsFalse(content.Fields.ContainsKey("#ContentList2Field1"), "This content shouldn't contain #ContentList2Field1 field.");
            Assert.IsFalse(content.Fields.ContainsKey("#ContentList2Field2"), "This content shouldn't contain #ContentList2Field2 field.");
        }
        
        private static void CheckContentList1(string encodedPath)
        {
            var contentlist = Node.Load<ContentList>(DecodePath(encodedPath));
            Assert.IsTrue(contentlist.ContentListId == 0, "ContentListId is not 0");
            Assert.IsNotNull(contentlist.ContentListType, "ContentListType is null");
            Assert.IsTrue(contentlist.ContentListDefinition == _listDef1);
        }
        
        //private void CheckContentList2(string encodedPath)
        //{
        //    var contentlist = Node.Load<ContentList>(DecodePath(encodedPath));
        //    Assert.IsTrue(contentlist.ContentListId == 0, "ContentListId is not 0");
        //    Assert.IsNotNull(contentlist.ContentListType, "ContentListType is null");
        //    Assert.IsTrue(contentlist.ContentListDefinition == _listDef2);
        //}
        
        private static void CheckContentListItem1(string encodedPath, Check check)
        {
            var node = LoadNode(encodedPath);
            var content = Content.Create(node);

            var fields = content.Fields;

            Assert.IsNotNull(node.ContentListType, "ContentListItem ContentListType == null");
            Assert.IsTrue(node.ContentListId > 0, "ContentListItem ContentListId == 0");
            Assert.IsNotNull(fields["#ContentList1Field1"], "This content should contain #ContentList1Field1 field.");
            Assert.IsNotNull(fields["#ContentList1Field2"], "This content should contain #ContentList1Field2 field.");
            Assert.IsNotNull(fields["#ContentList1Field3"], "This content should contain #ContentList1Field3 field.");
            Assert.IsFalse(fields.ContainsKey("#ContentList2Field1"), "This content shouldn't contain #ContentList2Field1 field.");
            Assert.IsFalse(fields.ContainsKey("#ContentList2Field2"), "This content shouldn't contain #ContentList2Field2 field.");

            if (fields["#ContentList1Field3"] != null)
            {
                var field = fields["#ContentList1Field3"];

                switch (check)
                {
                    case Check.Signature:
                        break;
                    case Check.FieldValuesKept:
                        Assert.IsTrue(field.HasValue() && (field.OriginalValue.ToString() == testFieldValueForT1), "#ContentList1Field3 doesn't contain the proper value.");
                        break;
                    case Check.FieldValuesNotKept:
                        Assert.IsTrue(field.HasValue() == false, "#ContentList1Field3 shouldn't contain any value.");
                        break;
                    default:
                        throw new ArgumentException("You have to assign a valid value to the check parameter!");
                }
            }
            
        }
        
        private static void CheckContentListItem2(string encodedPath, Check check)
        {
            var node = LoadNode(encodedPath);
            var content = Content.Create(node);

            var fields = content.Fields;

            Assert.IsNotNull(node.ContentListType, "ContentListItem ContentListType == null");
            Assert.IsTrue(node.ContentListId > 0, "ContentListItem ContentListId == 0");
            Assert.IsNotNull(content.Fields["#ContentList2Field1"], "This content should contain #ContentList2Field1 field.");
            Assert.IsNotNull(content.Fields["#ContentList2Field2"], "This content should contain #ContentList2Field2 field.");
            Assert.IsFalse(content.Fields.ContainsKey("#ContentList1Field1"), "This content shouldn't contain #ContentList1Field1 field.");
            Assert.IsFalse(content.Fields.ContainsKey("#ContentList1Field2"), "This content shouldn't contain #ContentList1Field2 field.");
            Assert.IsFalse(content.Fields.ContainsKey("#ContentList1Field3"), "This content shouldn't contain #ContentList1Field3 field.");

            if (fields["#ContentList2Field2"] != null)
            {
                var field = fields["#ContentList2Field2"];

                switch (check)
                {
                    case Check.Signature:
                        break;
                    case Check.FieldValuesKept:
                        Assert.IsTrue(field.HasValue() && ((decimal)field.OriginalValue == testFieldValueForT2), "#ContentList2Field2 doesn't contain the proper value.");
                        break;
                    case Check.FieldValuesNotKept:
                        //Assert.IsTrue(field.HasValue() == false, "#ContentList2Field2 shouldn't contain any value.");
                        Assert.IsTrue((decimal)field.OriginalValue == 0, "#ContentList2Field2 shouldn't contain any value.");
                        break;
                    default:
                        throw new ArgumentException("You have to assign a valid value to the check parameter!");
                }
            }
            
        }
        
        private static void CheckDeletedEventual(string encodedContentPath, int id)
        {
            var deletedNode = LoadNode(encodedContentPath);

            Assert.IsNull(deletedNode, "Deleted node should not exist!");
            Assert.IsFalse(CheckContentInTrash(id), "Trashbag for this node should not exist!");
        }

        private static void CheckMovedToTrash(string encodedContentPath, int id)
        {
            var parentPath = RepositoryPath.GetParentPath(DecodePath(encodedContentPath));
            
            var deletedNode = LoadNode(encodedContentPath);

            var trashbin = LoadTrash();

            if (trashbin == null)
                throw new Exception("TrashBin doesn't exist!!!");

            var selectedTrashbag = trashbin.Children.OfType<TrashBag>().SingleOrDefault(p => p.DeletedContent.Id == id);

            if(selectedTrashbag == null)
                throw new Exception("TrashBag doesn't exist!!!");
            
            Assert.IsTrue(selectedTrashbag.OriginalPath == parentPath, "Original path attribute has bad value!");
            Assert.IsNull(deletedNode, "Deleted node should not exist!");
        }

        private static bool CheckContentInTrash(int id)
        {
            var trashbin = LoadTrash();

            if(trashbin == null)
                return false;

            //var selectedTrashbag = trashbin.Children.SingleOrDefault(p => (p as TrashBag).IsAlive ? (p as TrashBag).LinkedContent.Id == id : false);
            //var selectedTrashbag = trashbin.Children.OfType<TrashBag>().SingleOrDefault(p => p.IsAlive ? p.LinkedContent.Id == id : false);
            var selectedTrashbag = trashbin.Children.OfType<TrashBag>().SingleOrDefault(p => p.DeletedContent.Id == id);
            //SingleOrDefault(p => (p as TrashBag).IsAlive ? (p as TrashBag).LinkedContent.Id == id : false);

            return (selectedTrashbag == null) ? false : true; 
        }

        private static void CheckNumberOfItemsInTrash(int expectedNumberOfTrashbinItems)
        {
            var trashbin = LoadTrash();

            if (trashbin == null)
            {
                Assert.IsTrue(true, "Trashbin doesn't exist!");
            }
            else
            {
                var numberOfTrashbinContents = trashbin.Children.OfType<TrashBag>().Count();
                Assert.AreEqual(expectedNumberOfTrashbinItems,numberOfTrashbinContents, "The number of trashbag items in trash doesn't match the expected value");
            }
        }

        private static void CheckNotDeleted(string encodedPath, int id)
        {
            var node = Node.LoadNode(id);

            if (node == null)
            {
                Assert.IsTrue(true,"Content shouldn't be deleted!");
            }
            else
            {
                var decodedPath = DecodePath(encodedPath);
                var contentName = RepositoryPath.GetFileName(decodedPath);

                Assert.IsTrue(node.Path == decodedPath, String.Format("Content {0} can not found in its original location ({1}).", contentName, decodedPath));
                Assert.IsFalse(CheckContentInTrash(id), "Content shouldn't be in trash.");
            }
            
        }
        
        // --- Structure building helpers
        // ------------------------------
        
        //if the path exists EnsureNode returnes -1
        //if the path doesn't exists EnsureNode returns the id of the content in the last path segment
        private static int EnsureNode(string encodedPath)
        {
            var path = DecodePath(encodedPath);
            if (Node.Exists(path))
                return -1;

            var name = RepositoryPath.GetFileName(path);
            var parentPath = RepositoryPath.GetParentPath(path);
            EnsureNode(parentPath);

            switch (name)
            {
                case "ContentList":
                case "SourceContentListT1":
                    CreateContentList(parentPath, name, _listDef1);
                    break;
                case "SourceContentListT2":
                    CreateContentList(parentPath, name, _listDef2);
                    break;
                case "TargetContentListT1":
                    CreateContentList(parentPath, name, _listDef1);
                    break;
                case "TargetContentListT2":
                    CreateContentList(parentPath, name, _listDef2);
                    break;
                case "SourceFolder":
                case "SourceItemFolder":
                case "SourceItemFolder1":
                case "SourceItemFolder2":
                case "TargetFolder":
                case "OtherLocation":
                case "Folder":
                case "Folder1":
                case "Folder2":
                case "Folder3":
                    CreateNode(parentPath, name, "Folder");
                    break;
                case "FakeFolder":
                    CreateNode(parentPath, "Folder", "Car");
                    path = String.Concat(parentPath, "/", "Folder");
                    break;
                case "TargetItemFolder":
                    CreateNode(parentPath, name, "Folder");
                    break;
                case "SourceContentListItem1":
                    CreateContentListItem(parentPath, name, "T1");
                    break;
                case "SourceContentListItem2":
                    CreateContentListItem(parentPath, name, "T2");
                    break;
                case "SourceNode":
                    CreateNode(parentPath, name, "Car");
                    break;
                case "TrashBag1":
                    CreateNode(parentPath, name, "TrashBag");
                    break;
                case "TrashBin":
                case "Trash":
                    CreateTrash(parentPath, name, 10, 100, 10, true);
                    break;
                case "Page1":
                case "Page2":
                case "Page":
                    CreateNode(parentPath, name, "Page");
                    break;
                default:
                    CreateNode(parentPath, name, "Car");
                    break;
            }

            //in real life Trash scenarios, titles are requiered for content visualization
            var node = LoadNode(path);
            var content = Content.Create(node);
            content["DisplayName"] = name;

            return node.Id;
        }
        
        private static Node LoadNode(string encodedPath)
        {
            return Node.LoadNode(DecodePath(encodedPath));
        }
        
        private static TrashBin LoadTrash()
        {
            return Node.Load<TrashBin>(_testTrashPath);
        }
       
        private static void CreateContentList(string parentPath, string name, string listDef)
        {
            var parent = Node.LoadNode(parentPath);
            var contentlist = new ContentList(parent)
                                  {
                                      Name = name,
                                      ContentListDefinition = listDef
                                  };
            
            contentlist.Save();
        }
        
        private static void CreateNode(string parentPath, string name, string typeName)
        {
            var parent = Content.Load(parentPath);
            var content = Content.CreateNew(typeName, parent.ContentHandler, name);

            if (typeName == "Car") content["Model"] = "Mitshubishi";

            content.Save();
        }
        
        private static void CreateContentListItem(string parentPath, string name, string type)
        {
            var parent = Node.LoadNode(parentPath);
            var task = new Task(parent)
                           {
                               Name = name
                           };
            
            task.Save();

            var content = Content.Create(task);

            switch (type)
            {
                case "T1":
                    content["#ContentList1Field3"] = testFieldValueForT1;
                    break;
                case "T2":
                    content["#ContentList2Field2"] = testFieldValueForT2;
                    break;
                default:
                    throw new ArgumentException("You have to assign a valid value to the type parameter!"); 
            }

            content.Save();            
        }
        
        private static void CreateTrash(string parentPath, string name, int minRetentionTime, int sizeQuota, int bagCapacity, bool isAct)
        {
            var parent = Node.LoadNode(parentPath);
            var tb = new TrashBin(parent)
                         {
                             Name = name,
                             IsActive = isAct,
                             BagCapacity = bagCapacity,
                             MinRetentionTime = minRetentionTime,
                             SizeQuota = sizeQuota
                         };
            tb.Save();
            
        }
        
        private static void ConfigureTrash(int? minRetentionTime, int? sizeQuota, int? bagCapacity, bool? isAct)
        {
            var trashbin = LoadTrash();

            if(minRetentionTime.HasValue)
                trashbin.MinRetentionTime = minRetentionTime.Value;
            if(sizeQuota.HasValue)
                trashbin.SizeQuota = sizeQuota.Value;
            if(bagCapacity.HasValue)
                trashbin.BagCapacity = bagCapacity.Value;
            if(isAct.HasValue)
                trashbin.IsActive = isAct.Value;

            trashbin.Save();
        }

        private static void DeleteMainTrash()
        {
            var node = Node.LoadNode(_testTrashPath);

            if (node == null)
                throw new ArgumentException("Trash doesn't exist!");

            node.Delete();
        }
        
        private static void DeleteWithDeleteNode(string encodedContentLinkPath)
        {
            var node = Node.Load<GenericContent>(DecodePath(encodedContentLinkPath));

            if (node == null)
                throw new ArgumentException("Content with the given path doesn't exist!");

            TrashBin.DeleteNode(node);
        }
        
        private static void DeleteWithForceDelete(string encodedContentLinkPath)
        {
            var node = Node.Load<GenericContent>(DecodePath(encodedContentLinkPath));

            if (node == null)
                throw new ArgumentException("Content with the given path doesn't exist!");
            
            TrashBin.ForceDelete(node);
        }
        
        private static void DeleteWithDelete(string encodedContentLinkPath)
        {
            var gc = Node.Load<GenericContent>(DecodePath(encodedContentLinkPath));

            if (gc == null)
                throw new ArgumentException("Content with the given path doesn't exist!");

            gc.Delete();
        }
        
        private static void DeleteFromTrash(int id)
        {
            var trashbin = LoadTrash();

            var selectedTrashbag = trashbin.Children.OfType<TrashBag>().Single(p => p.DeletedContent.Id == id);

            selectedTrashbag.Delete();
        }

        private static void RestoreFromTrash(int id, string encodedTargetPath, bool addNewName, RestoreResultType expectedRestoreResult)
        {
            var trashbin = LoadTrash();

            var tb = trashbin.Children.OfType<TrashBag>().SingleOrDefault(p => p.DeletedContent.Id == id);

            if(tb == null)
                throw new Exception("TrashBag missing.");

            var decodedTargetPath = (encodedTargetPath != null) ? DecodePath(encodedTargetPath) : tb.OriginalPath;
            var originalContentName = tb.DeletedContent.Name;
            
            
            var trashResult = RestoreResultType.Success;
            
            try
            {
                TrashBin.Restore(tb, decodedTargetPath, addNewName);
            }
            catch (RestoreException rex)
            {
                trashResult = rex.ResultType;
            }

            Assert.IsTrue(trashResult == expectedRestoreResult, String.Format("The return value of the restore method should match the expected value. The returned value of restore was: {0}", trashResult));
            
            var itemInTargetLocation = LoadNode(String.Concat(decodedTargetPath,"/",originalContentName));

            AccessProvider.Current.SetCurrentUser(User.Administrator);
            
            switch (expectedRestoreResult)
                {
                    case RestoreResultType.Success:
                        if (addNewName)
                        {
                            var restoredItemWithNewName = Node.LoadNode(id);

                            Assert.IsTrue(decodedTargetPath == restoredItemWithNewName.ParentPath,
                                          "Renamed content should be in its target directory");
                            Assert.IsTrue(itemInTargetLocation.Id != id,
                                          "The id of the content in the target location should not be identical to the id of the restored content.");
                        }
                        else
                        {
                            Assert.IsNotNull(itemInTargetLocation,
                                         "Restored item should be exist in the target location.");
                            Assert.IsTrue(itemInTargetLocation.Id == id,
                                          "The id of the content in the target location should be identical to the id of the restored content.");
                        }

                        Assert.IsFalse(CheckContentInTrash(id), "This content should get out of trash.");
                        break;

                    case RestoreResultType.ExistingName:
                        Assert.IsNotNull(itemInTargetLocation,
                                         "There must be an item in the target location.");
                        Assert.IsTrue(itemInTargetLocation.Id != id,
                                      "There must be an item in the target location with the same name as the one to be restored.");
                        Assert.IsTrue(CheckContentInTrash(id), "This content should stay at trash.");
                        break;
                    case RestoreResultType.NoParent:
                        var parentInTargetLocation = Node.LoadNode(decodedTargetPath);
                        Assert.IsNull(parentInTargetLocation,
                                      "Content parent should not exist in the original location.");
                        Assert.IsTrue(CheckContentInTrash(id), "This content should stay at trash.");
                        break;
                    case RestoreResultType.Nonedefined:
                    case RestoreResultType.PermissionError:
                    case RestoreResultType.UnknownError:
                        Assert.IsNull(itemInTargetLocation, "Restored item should not exist in the original location.");
                        Assert.IsTrue(CheckContentInTrash(id), "This content should stay at trash.");
                        break;
                    //TODO: atnezni
                    case RestoreResultType.ForbiddenContentType:
                        var parent = Node.LoadNode(decodedTargetPath);
                        var tbContentType = ((GenericContent)parent).GetAllowedChildTypes().Where(ct => ct.Name == tb.NodeType.Name);
                        
                        Assert.IsTrue(!(parent is IFolder) || (tbContentType.Count() == 0) ,
                                      "Content parent should not exist in the original location or it is not a folder.");
                        Assert.IsTrue(CheckContentInTrash(id), "This content should stay at trash.");
                        break;
                    default:
                        throw new NotImplementedException();
                        
                }
            
        }

        private static void DisabelTrashForThisNode(string encodedPath)
        {
            var node = Node.Load<GenericContent>(DecodePath(encodedPath));
            node.TrashDisabled = true;
            node.Save();
        }

        private static string DecodePath(string encodedPath)
        {
            return encodedPath.Replace("[TestRoot]", _testRootPath);
        }
    }

}
