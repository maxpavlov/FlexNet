using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository.Tests
{
    /// <summary>
    /// Summary description for ContentLinkTest
    /// </summary>
    [TestClass]
    public class ContentLinkTest : TestBase
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
        private const string TestRootName = "ContentLinkTestContext";
        private static readonly string TestRootPath = String.Concat("/Root/", TestRootName);
        

        [ClassInitialize]
        public static void Initialize(TestContext tc)
        {
            var node = Node.LoadNode(TestRootPath);
            if (node == null)
            {
                node = NodeType.CreateInstance("SystemFolder", Node.LoadNode("/Root"));
                node.Name = TestRootName;
                node.Save();
            }
        }

        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (!Node.Exists(TestRootPath)) return;

            var testRoot = Node.LoadNode(TestRootPath);
            testRoot.ForceDelete();
        }

        // --- Tests
        // ---------

        [TestMethod]
        public void ContentLink_WithOneChildren()
        {
            EnsureNode("[TestRoot]/ContentLink1");
            EnsureNode("[TestRoot]/ContentLink1/Car1");

            CheckInactiveContentLink("[TestRoot]/ContentLink1");
        }

        [TestMethod]
        public void ContentLink_WithOneReference()
        {
            EnsureNode("[TestRoot]/ContentLink2");
            EnsureNode("[TestRoot]/GenericContent2");

            SetContentLink("[TestRoot]/ContentLink2", LoadNode("[TestRoot]/GenericContent2"));

            CheckContentLink("[TestRoot]/ContentLink2", "[TestRoot]/GenericContent2");
        }

        [TestMethod]
        public void ContentLink_WithOneReferenceAndOneChildren()
        {
            EnsureNode("[TestRoot]/ContentLink3/Car3");
            EnsureNode("[TestRoot]/GenericContent3");

            SetContentLink("[TestRoot]/ContentLink3", LoadNode("[TestRoot]/GenericContent3"));

            CheckContentLink("[TestRoot]/ContentLink3", "[TestRoot]/GenericContent3");
        }

        [TestMethod]
        public void ContentLink_WithDeletedReference_WithoutChild()
        {
            var node = LoadNode("[TestRoot]/GenericContent4");
            if (node != null)
                node.ForceDelete();

            EnsureNode("[TestRoot]/ContentLink4");
            var refContent = EnsureNode("[TestRoot]/GenericContent4");

            SetContentLink("[TestRoot]/ContentLink4", LoadNode("[TestRoot]/GenericContent4"));

            Node.ForceDelete(refContent);

            CheckInactiveContentLink("[TestRoot]/ContentLink4");
        }
        [TestMethod]
        public void ContentLink_WithDeletedReference_WithChild()
        {
            var node = LoadNode("[TestRoot]/GenericContent4a");
            if (node != null)
                node.ForceDelete();

            EnsureNode("[TestRoot]/ContentLink4a/Car4a");
            var refContent = EnsureNode("[TestRoot]/GenericContent4a");

            SetContentLink("[TestRoot]/ContentLink4a", LoadNode("[TestRoot]/GenericContent4a"));

            Node.ForceDelete(refContent);

            CheckInactiveContentLink("[TestRoot]/ContentLink4a");
        }

        [TestMethod]
        public void ContentLink_WithMovedReference()
        {
            EnsureNode("[TestRoot]/ContentLink5/Car5");
            EnsureNode("[TestRoot]/GenericContent5");
            EnsureNode("[TestRoot]/Folder5");

            SetContentLink("[TestRoot]/ContentLink5", LoadNode("[TestRoot]/GenericContent5"));

            MoveNode("[TestRoot]/GenericContent5", "[TestRoot]/Folder5");

            CheckContentLink("[TestRoot]/ContentLink5", "[TestRoot]/Folder5/GenericContent5");
        }

        [TestMethod]
        public void ContentLink_WithoutChildrenWithDeletedReference()
        {
            EnsureNode("[TestRoot]/ContentLink6");
            var refContent = EnsureNode("[TestRoot]/Car6");

            SetContentLink("[TestRoot]/ContentLink6", LoadNode("[TestRoot]/Car6"));

            Node.ForceDelete(refContent);

            CheckInactiveContentLink("[TestRoot]/ContentLink6");
        }

        [TestMethod]
        public void ContentLink_GetProperty_ComputedProperty()
        {
            decimal referenceValue = -1;

            EnsureNode("[TestRoot]/ContentLink7");
            EnsureNode("[TestRoot]/File7");
            SetContentLink("[TestRoot]/ContentLink7", LoadNode("[TestRoot]/File7"));

            CheckLinkedContentPropertyValue("[TestRoot]/ContentLink7", "FullSize", referenceValue);
        }

        [TestMethod]
        public void ContentLink_GetProperty_RepositoryProperty()
        {
            var referenceValue = "sample make";

            EnsureNode("[TestRoot]/ContentLink8");
            EnsureNode("[TestRoot]/Car8");
            SetContentLink("[TestRoot]/ContentLink8", LoadNode("[TestRoot]/Car8"));

            var car = LoadNode("[TestRoot]/Car8");
            car["Make"] = referenceValue;
            car.Save();

            CheckLinkedContentPropertyValue("[TestRoot]/ContentLink8", "Make", referenceValue);
        }

        [TestMethod]
        public void ContentLink_GetProperty_FieldProperty()
        {
            var referenceValue = "sample keywords";

            EnsureNode("[TestRoot]/ContentLink9");
            EnsureNode("[TestRoot]/Image9");
            SetContentLink("[TestRoot]/ContentLink9", LoadNode("[TestRoot]/Image9"));

            var image = LoadNode("[TestRoot]/Image9");
            image["Keywords"] = referenceValue;
            image.Save();

            CheckLinkedContentPropertyValue("[TestRoot]/ContentLink9", "Keywords", referenceValue);
        }

        [TestMethod]
        public void ContentLink_Title()
        {
            var linkDisplayName = "ContentLink1";
            var targetDisplayName = "TargetTitle1";
            var linkDisplayName2 = "ContentLink2";
            var targetDisplayName2 = "TargetTitle2";

            EnsureNode("[TestRoot]/ContentLink11");
            EnsureNode("[TestRoot]/Car11");

            var car = (GenericContent)LoadNode("[TestRoot]/Car11");
            car.DisplayName = targetDisplayName;
            car.Save();

            var link = (ContentLink)LoadNode("[TestRoot]/ContentLink11");
            link.DisplayName = linkDisplayName;
            link.Link = car;
            link.Save();

            var carContent = LoadContent("[TestRoot]/Car11");
            Assert.IsTrue((string)carContent["DisplayName"] == targetDisplayName, String.Concat("DisplayName of target is ", carContent["DisplayName"], ". Expected: ", targetDisplayName));
            var linkContent = LoadContent("[TestRoot]/ContentLink11");
            Assert.IsTrue((string)linkContent["DisplayName"] == targetDisplayName, String.Concat("DisplayName of target is ", linkContent["DisplayName"], ". Expected: ", targetDisplayName));

            car.DisplayName = targetDisplayName2;
            car.Save();

            carContent = LoadContent("[TestRoot]/Car11");
            Assert.IsTrue((string)carContent["DisplayName"] == targetDisplayName2, String.Concat("DisplayName of target is ", carContent["DisplayName"], ". Expected: ", targetDisplayName2));
            linkContent = LoadContent("[TestRoot]/ContentLink11");
            Assert.IsTrue((string)linkContent["DisplayName"] == targetDisplayName2, String.Concat("DisplayName of target is ", linkContent["DisplayName"], ". Expected: ", targetDisplayName2));
        }

        [TestMethod]
        public void ContentLink_WithOneChildren_WithoutSeePermission()
        {

            EnsureNode("[TestRoot]/ContentLink13/Car13");
            
            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/ContentLink13/Car13"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.See,
                          NewValue = PermissionValue.Deny
                      }
              };

            
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                CheckInactiveContentLink("[TestRoot]/ContentLink13");
            }
        }

        [TestMethod]
        public void ContentLink_WithOneReference_WithoutSeePermissions()
        {
            EnsureNode("[TestRoot]/ContentLink12");
            EnsureNode("[TestRoot]/Car12");

            SetContentLink("[TestRoot]/ContentLink12", LoadNode("[TestRoot]/Car12"));

            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/Car12"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.See,
                          NewValue = PermissionValue.Deny
                      }
              };

            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                CheckInactiveContentLink("[TestRoot]/ContentLink12");
            }
        }

        [TestMethod]
        public void ContentLink_WithOneChildren_WithoutOpenPermissions()
        {

            EnsureNode("[TestRoot]/ContentLink13/Page13");

            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/ContentLink13/Page13"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Open,
                          NewValue = PermissionValue.Deny
                      }
              };

            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                CheckInactiveContentLink("[TestRoot]/ContentLink13");
            }
            

        }

        [TestMethod]
        public void ContentLink_WithOneReference_WithoutOpenPermissions()
        {
            EnsureNode("[TestRoot]/ContentLink14");
            EnsureNode("[TestRoot]/Image14");

            SetContentLink("[TestRoot]/ContentLink14", LoadNode("[TestRoot]/Image14"));

            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = DecodePath("[TestRoot]/Image14"),
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Open,
                          NewValue = PermissionValue.Deny
                      }
              };

            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                CheckInactiveContentLink("[TestRoot]/ContentLink14");
            }

        }
 

        // --- Helper methods
        // ------------------

        private static void CheckContentLink(string contentLinkPath, string assumedContentPath)
        {
            var clink = Node.Load<ContentLink>(DecodePath(contentLinkPath));
            var linkedContent = clink.LinkedContent;

            var assumedContent = Node.LoadNode(DecodePath(assumedContentPath));

            Assert.IsTrue(clink.IsAlive, "The ContentLink should be alive.");
            Assert.IsTrue(linkedContent.Id == assumedContent.Id, "Not the assumed content were returned.");
        }

        private static void CheckInactiveContentLink(string encodedContentLinkPath)
        {
            var contentLinkPath = DecodePath(encodedContentLinkPath);
            
            var clink = Node.Load<ContentLink>(contentLinkPath);

            Assert.IsFalse(clink.IsAlive, "The ContentLink should be alive.");
        }

        private static void CheckLinkedContentPropertyValue<T>(string encodedContentLinkPath, string propertyName, T assumedPropertyValue)
        {
            var contentLinkPath = DecodePath(encodedContentLinkPath);
            var clink = Content.Load(contentLinkPath);
            //var clink = Node.Load<ContentLink>(contentLinkPath);

            var propertyValue = (T) clink[propertyName];

            Assert.IsTrue(propertyValue.Equals(assumedPropertyValue), String.Format("The value of {0} property doesn't match expected value: {1}", propertyName, assumedPropertyValue));

        }

        private static void SetContentLink(string encodedContentLinkPath, Node link)
        {
            var contentLinkPath = DecodePath(encodedContentLinkPath);
            var clink = Node.Load<ContentLink>(contentLinkPath);
            clink.Link = link;
            clink.Save();
        }
        
        private static int EnsureNode(string encodedPath)
        {
            return TestEquipment.EnsureNode(DecodePath(encodedPath));
        }

        private static Node LoadNode(string encodedPath)
        {
            return Node.LoadNode(DecodePath(encodedPath));
        }
        private static Content LoadContent(string encodedPath)
        {
            return Content.Load(DecodePath(encodedPath));
        }

        private static void MoveNode(string encodedSourcePath, string encodedTargetPath)
        {
            var sourcePath = DecodePath(encodedSourcePath);
            var targetPath = DecodePath(encodedTargetPath);
            
            Node.Move(sourcePath, targetPath);
        }

        private static string DecodePath(string encodedPath)
        {
            return encodedPath.Replace("[TestRoot]", TestRootPath);
        }

    }
}
