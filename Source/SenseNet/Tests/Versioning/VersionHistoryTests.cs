using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Portal;

namespace SenseNet.ContentRepository.Tests.Versioning
{
    [TestClass]
    public class VersionHistoryTests : TestBase
    {
        private TestContext testContextInstance;
        public override TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        //==================================================================== Test prepare and cleanup

        private const string TestRootName = "_GenericContentTests";
        private static readonly string TestRootPath = String.Concat("/Root/", TestRootName);

        private Node _testRoot;
        public Node TestRoot
        {
            get
            {
                if (_testRoot == null)
                {
                    _testRoot = Node.LoadNode(TestRootPath);

                    if (_testRoot == null)
                    {
                        var node = NodeType.CreateInstance("Folder", Node.LoadNode("/Root"));
                        node.Name = TestRootName;
                        node.Save();

                        _testRoot = Node.LoadNode(TestRootPath);
                    }
                }

                return _testRoot;
            }
        }

        [ClassCleanup]
        public static void DestroyPlayground()
        {
            var tr = Node.LoadNode(TestRootPath);

            if (tr != null)
                tr.ForceDelete();
        }

        //==================================================================== Tests

        [TestMethod]
        public void CreateLongHistory_Test1()
        {
            var test = CreatePage("GCSaveTest");

            //------------ Approving: False, Versioning: None
            test.VersioningMode = VersioningType.None;
            test.ApprovingMode = ApprovingType.False;

            test.Description = "Init_Value";
            test.Save();
            AssertVersionHistory(test, "V1.0.A", "#1");
            AssertPropertyValues(test, false, "Init_Value", "#1");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.L", "#2");

            test.Description = "After-CheckOut";
            test.TrashDisabled = true;

            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.L", "#3");
            AssertPropertyValues(test, true, "After-CheckOut", "#3");

            test.CheckIn();
            AssertVersionHistory(test, "V1.0.A", "#4");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.L", "#5");
            
            test.Description = "Before-Undo";
            test.TrashDisabled = false;
            
            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.L", "#6");
            AssertPropertyValues(test, false, "Before-Undo", "#6");

            test.UndoCheckOut();
            AssertVersionHistory(test, "V1.0.A", "#7");
            AssertPropertyValues(test, true, "After-CheckOut", "#7");

            //------------ Approving: False, Versioning: Major
            test.VersioningMode = VersioningType.MajorOnly;
            test.ApprovingMode = ApprovingType.False;
            test.SaveSameVersion();
            AssertVersionHistory(test, "V1.0.A", "#8");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.L", "#9");

            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.L", "#10");

            test.CheckIn();
            AssertVersionHistory(test, "V1.0.A V2.0.A", "#11");
            
            test.Description = "OFF-Major-BeforeCheckOut";
            test.TrashDisabled = false;

            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A", "#12");
            AssertPropertyValues(test, false, "OFF-Major-BeforeCheckOut", "#12");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V4.0.L", "#13");

            test.Description = "OFF-Major-BeforeUndo";
            test.TrashDisabled = true;

            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V4.0.L", "#14");
            AssertPropertyValues(test, true, "OFF-Major-BeforeUndo", "#14");

            test.UndoCheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A", "#15");
            AssertPropertyValues(test, false, "OFF-Major-BeforeCheckOut", "#15");

            //------------ Approving: False, Versioning: MajorAndMinor
            test.VersioningMode = VersioningType.MajorAndMinor;
            test.ApprovingMode = ApprovingType.False;
            test.SaveSameVersion();

            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A", "#16");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.L", "#17");

            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.L", "#18");

            test.CheckIn();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D", "#19");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.L", "#20");

            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.L", "#21");

            test.UndoCheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D", "#22");

            test.Save(SavingMode.RaiseVersionAndLock);
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.L", "#23");

            test.CheckIn();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D", "#24");

            test.Save(SavingMode.RaiseVersionAndLock);
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D V3.3.L", "#25");

            test.UndoCheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D", "#26");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D V3.3.L", "#27");

            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D V3.3.L", "#28");

            test.CheckIn();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D V3.3.D", "#29");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D V3.3.D V3.4.L", "#30");

            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D V3.3.D V3.4.L", "#31");

            test.CheckIn();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D V3.3.D V3.4.D", "#32");

            //------------ Approving: True, Versioning: None
            test.VersioningMode = VersioningType.None;
            test.ApprovingMode = ApprovingType.True;
            test.SaveSameVersion();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D V3.3.D V3.4.D", "#33");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D V3.3.D V3.4.D V4.0.L", "#34");
            
            test.Description = "ON-None-Before-CheckOut";
            test.TrashDisabled = false;

            test.CheckIn();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V4.0.P", "#35");
            AssertPropertyValues(test, false, "ON-None-Before-CheckOut", "#35");

            test.Reject();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V4.0.R", "#36");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V4.0.R V5.0.L", "#37");

            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V4.0.R V5.0.L", "#38");

            test.CheckIn();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V4.0.R V5.0.P", "#39");

            test.Approve();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A", "#40");

            //------------ Approving: True, Versioning: MajorAndMinor
            test.VersioningMode = VersioningType.MajorAndMinor;
            test.ApprovingMode = ApprovingType.True;
            test.Save();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D", "#41");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.L", "#42");

            test.Description = "ON-MajorMinor-BeforeCheckIn";
            test.TrashDisabled = true;

            test.CheckIn();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.D", "#43");
            AssertPropertyValues(test, true, "ON-MajorMinor-BeforeCheckIn", "#43");

            test.Publish();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.P", "#44");
            AssertPropertyValues(test, true, "ON-MajorMinor-BeforeCheckIn", "#44");

            test.Reject();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.R", "#45");

            test.CheckOut();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.R V3.3.L", "#46");

            test.Publish();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.R V3.3.P", "#47");

            test.Approve();
            AssertVersionHistory(test, "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.R V4.0.A", "#48");

            //this must never change
            const string oldHistory = "V1.0.A V2.0.A V3.0.A V3.1.D V3.2.R ";

            test.CheckOut();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.L", "#49");

            test.CheckIn();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D", "#50");

            test.CheckOut();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.L", "#51");

            test.Publish();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.P", "#52");

            test.Reject();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.R", "#53");

            test.Publish();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.R V4.3.P", "#54");

            test.Reject();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.R V4.3.R", "#54");

            test.CheckOut();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.R V4.3.R V4.4.L", "#55");

            //------------ Approving: True, Versioning: Major
            test.VersioningMode = VersioningType.MajorOnly;
            test.ApprovingMode = ApprovingType.True;

            test.CheckIn();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.R V4.3.R V4.4.P", "#56");

            test.Reject();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.R V4.3.R V4.4.R", "#57");

            test.CheckOut();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.R V4.3.R V4.4.R V5.0.L", "#58");

            test.Description = "ON-Major-Before-CheckIn";
            test.TrashDisabled = false;

            test.CheckIn();
            AssertVersionHistory(test, oldHistory + "V4.0.A V4.1.D V4.2.R V4.3.R V4.4.R V5.0.P", "#59");
            AssertPropertyValues(test, false, "ON-Major-Before-CheckIn", "#59");

            test.Approve();
            AssertVersionHistory(test, oldHistory + "V4.0.A V5.0.A", "#60");
            AssertPropertyValues(test, false, "ON-Major-Before-CheckIn", "#60");
        }
 
        [TestMethod]
        public void Bug1308Test()
        {
            const string fileName = "bug1308.xml";
            const string fileBinary = @"<?xml version='1.0' encoding='utf-8'?> <ContentType><Fields /></ContentType>";
            
            
            var test = new File(TestRoot);
            test.Name = fileName;
            test.Binary.SetStream(Tools.GetStreamFromString(fileBinary));
            test.Binary.FileName = fileName;
            test.Save();

            //1. checkout
            test.CheckOut();

            //2. checkin
            var exceptionOccured = false;
            try
            {
                test.UndoCheckOut();
            }
            catch (NullReferenceException)
            {
                exceptionOccured = true;
            }

            //assert
            var errorMessage = String.Concat("Version history should be V1.0.A instead of: ", GetVersionHistoryString(NodeHead.Get(test.Id)));
            Assert.IsFalse(exceptionOccured, String.Concat("An exception occured during execution.", errorMessage));
            AssertVersionHistory(test, "V1.0.A", errorMessage);

        }

        //==================================================================== Helper methods 
        
        private static void AssertVersionHistory(Node node, string expectedHistory, string message)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (string.IsNullOrEmpty(expectedHistory))
                throw new ArgumentNullException("expectedHistory");

            var head = NodeHead.Get(node.Id);
            var actualHistory = GetVersionHistoryString(head);

            Assert.AreEqual(expectedHistory, actualHistory, "Wrong version history " + message);
        }

        private static void AssertPropertyValues(GenericContent gc, bool trashValue, string descValue, string message)
        {
            if (gc == null)
                throw new ArgumentNullException("gc");

            Assert.AreEqual(trashValue, gc.TrashDisabled, "Wrong TrashDisabled value " + message);
            Assert.AreEqual(descValue, gc.Description, "Wrong Description value " + message);
        }

        private static string GetVersionHistoryString(NodeHead head)
        {
            return String.Join(" ", (from version in head.Versions
                                     select version.VersionNumber.ToString()).ToArray());
        }

        public Page CreatePage(string name)
        {
            return CreatePage(name, this.TestRoot);
        }

        public static Page CreatePage(string name, Node parent)
        {
            var page = Node.LoadNode(string.Concat(parent.Path, "/", name)) as Page;
            if (page != null)
                page.ForceDelete();

            page = new Page(parent) { Name = name };
            page.Save();

            return page;
        }
    }
}
