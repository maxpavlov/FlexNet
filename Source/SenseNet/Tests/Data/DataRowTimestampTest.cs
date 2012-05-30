using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Versioning;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Search.Indexing;
using SenseNet.Search;
using Lucene.Net.Index;
using LucField = Lucene.Net.Documents.Field;
using System.Data.Linq;
using SenseNet.ContentRepository.Storage.Data;
using System.Data;
using Lucene.Net.Util;
using Lucene.Net.Documents;
using System.Threading;
using System.Reflection;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Tests.Data
{
    [TestClass]
    public class DataRowTimestampTest : TestBase
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
        //Use ClassInitialize to run code before running the first test in the class
        #endregion
        #region Playground
        private static string __testRootName = "_DataRowTimestampTest";
        private static string _testRootPath = String.Concat("/Root/", __testRootName);
        private static string _fakeIndexingTaskComment = "Test:Indexing_ActivitesWithMissingVersion";
        private static int _fakeId = 987654321;
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
                        node.Name = __testRootName;
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

        [TestMethod]
        [ExpectedException(typeof(NodeIsOutOfDateException))]
        public void Timestamp_CannotSaveObsolete()
        {
            var content = Content.CreateNew("Car", TestRoot, "Car");
            content.Save();
            var id = content.Id;

            var node1 = Node.LoadNode(id);
            var node2 = Node.LoadNode(id);
            node1.Index = 111;
            node2.Index = 112;
            node1.Save();
            node2.Save();
        }

        [TestMethod]
        public void Timestamp_Growing()
        {
            var content = Content.CreateNew("Car", TestRoot, "Car");
            var handler = (GenericContent)content.ContentHandler;
            handler.VersioningMode = VersioningType.MajorAndMinor;
            content.Save();
            var id = content.Id;
            var timestamp = content.ContentHandler.NodeTimestamp;

            content.ContentHandler.Index++;
            content.Save();
            Assert.IsTrue(content.ContentHandler.NodeTimestamp > timestamp, "Timestamp is not greater after Save");
            timestamp = content.ContentHandler.NodeTimestamp;

            content.CheckOut();
            Assert.IsTrue(content.ContentHandler.NodeTimestamp > timestamp, "Timestamp is not greater after CheckOut");
            timestamp = content.ContentHandler.NodeTimestamp;

            content.UndoCheckOut();
            Assert.IsTrue(content.ContentHandler.NodeTimestamp > timestamp, "Timestamp is not greater after UndoCheckOut");
            timestamp = content.ContentHandler.NodeTimestamp;

            content.CheckOut();
            Assert.IsTrue(content.ContentHandler.NodeTimestamp > timestamp, "Timestamp is not greater after CheckOut #2");
            timestamp = content.ContentHandler.NodeTimestamp;

            content.ContentHandler.Index++;
            content.Save();
            Assert.IsTrue(content.ContentHandler.NodeTimestamp > timestamp, "Timestamp is not greater after Save #2");
            timestamp = content.ContentHandler.NodeTimestamp;

            content.CheckIn();
            Assert.IsTrue(content.ContentHandler.NodeTimestamp > timestamp, "Timestamp is not greater after CheckIn");
            timestamp = content.ContentHandler.NodeTimestamp;

            content.Publish();
            Assert.IsTrue(content.ContentHandler.NodeTimestamp > timestamp, "Timestamp is not greater after Publish");
            timestamp = content.ContentHandler.NodeTimestamp;
        }

        [TestMethod]
        public void Indexing_SavingRepairsDuplicatedNodes()
        {
            var content = Content.CreateNew("Car", TestRoot, "CarBadIndex");
            var handler = (GenericContent)content.ContentHandler;
            handler.VersioningMode = VersioningType.MajorAndMinor;
            content.Save();
            var id = content.Id;
            var versionIdList = new List<int>();
            versionIdList.Add(content.ContentHandler.VersionId);

            for (int i = 0; i < 4; i++)
            {
                content.ContentHandler.Index++;
                content.Save();
                versionIdList.Add(content.ContentHandler.VersionId);
            }

            var term = new Term(LucObject.FieldName.NodeId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(id));
            LuceneManager._writer.DeleteDocuments(term);
            LuceneManager._writer.Commit();
            LuceneManager._reader = LuceneManager._writer.GetReader();

            foreach (var versionId in versionIdList)
            {
                var node = Node.LoadNodeByVersionId(versionId);
                var doc = IndexDocumentInfo.CreateDocument(node);
                doc.RemoveField(LucObject.FieldName.IsLastDraft);
                doc.Add(new LucField(LucObject.FieldName.IsLastDraft, BooleanIndexHandler.YES, LucField.Store.YES, LucField.Index.NOT_ANALYZED, LucField.TermVector.NO));
                LuceneManager._writer.AddDocument(doc);
            }
            LuceneManager._writer.Commit();
            LuceneManager._reader = LuceneManager._writer.GetReader();

            var children1 = ((IFolder)TestRoot).Children.ToArray();
            var result1 = ContentQuery.Query("Name:CarBadIndex", new QuerySettings { EnableAutofilters = false, EnableLifespanFilter = false });

            content.ContentHandler.Index++;
            content.Save();

            var children2 = ((IFolder)TestRoot).Children.ToArray();
            var result2 = ContentQuery.Query("Name:CarBadIndex", new QuerySettings { EnableAutofilters = false, EnableLifespanFilter = false });

            if (result1.Identifiers.Count() <= result2.Identifiers.Count())
                Assert.Inconclusive("Identifier counts are equal. We did nothing.");
            Assert.IsTrue(result2.Identifiers.Count() == 1, String.Concat("Identifiers.Count is ", result2.Identifiers.Count(), ". Expected: 1"));
        }

        [TestMethod]
        [Description("There is no task that is contains more than one activity with same VersionId")]
        public void Indexing_DuplicatedActivites()
        {
            var content = Content.CreateNew("Car", TestRoot, "Car_Indexing_SavingRepairsDuplicatedNodes");
            var handler = (GenericContent)content.ContentHandler;
            handler.VersioningMode = VersioningType.None;
            content.Save();
            var id = content.Id;

            content = Content.Load(id);
            content.CheckOut();
            Assert.IsTrue(CheckActivityCountWithSameVersionAndNodeId(id), "#1 There are duplicated activities after CheckOut");

            content = Content.Load(id);
            content.Save();
            Assert.IsTrue(CheckActivityCountWithSameVersionAndNodeId(id), "#2 There are duplicated activities after Save");

            content = Content.Load(id);
            content.CheckIn();
            Assert.IsTrue(CheckActivityCountWithSameVersionAndNodeId(id), "#3 There are duplicated activities after CheckIn");

            content = Content.Load(id);
            content.CheckOut();
            Assert.IsTrue(CheckActivityCountWithSameVersionAndNodeId(id), "#4 There are duplicated activities after CheckOut");

            content = Content.Load(id);
            content.UndoCheckOut();
            Assert.IsTrue(CheckActivityCountWithSameVersionAndNodeId(id), "#5 There are duplicated activities after UndoCheckOut");
        }
        bool CheckActivityCountWithSameVersionAndNodeId(int nodeId)
        {
            using (var context = new IndexingDataContext())
            {
                var y = from task in context.IndexingTasks
                        join activity in context.IndexingActivities on task.IndexingTaskId equals activity.IndexingTaskId
                        group activity by new { TId = activity.IndexingTaskId, VId = activity.VersionId, NId = activity.NodeId } into x
                        where x.Count() > 1
                        select x;
                return y.Count() == 0;
            }
        }

        [TestMethod]
        [Description("A task execution with update activity after delete activity not throws any exception.")]
        public void Indexing_ActivitesWithMissingVersion()
        {
            var content = Content.CreateNew("Car", TestRoot, "Car_Indexing_ActivitesWithMissingVersion");
            var handler = (GenericContent)content.ContentHandler;
            //handler.VersioningMode = VersioningType.None;
            content.Save();
            var id = content.Id;

            LuceneManager.CommitChanges();
            LuceneManager.RefreshReader();

            var task = new IndexingTask { Comment = _fakeIndexingTaskComment };
            task.IndexingActivities.Add(new IndexingActivity
            {
                ActivityType = IndexingActivityType.RemoveDocument,
                NodeId = _fakeId,
                VersionId = _fakeId
            });
            task.IndexingActivities.Add(new IndexingActivity
            {
                ActivityType = IndexingActivityType.UpdateDocument,
                NodeId = _fakeId,
                VersionId = _fakeId
            });
            task.IndexingActivities.Add(new IndexingActivity
            {
                ActivityType = IndexingActivityType.AddDocument,
                NodeId = _fakeId,
                VersionId = _fakeId
            });

            try
            {
                using (var context = new IndexingDataContext())
                {
                    context.IndexingTasks.InsertOnSubmit(task);
                    context.SubmitChanges();

                }

                var tasks = IndexingTaskManager.GetUnprocessedTasks(task.IndexingTaskId - 1, new int[0]);
                foreach (var t in tasks)
                    IndexingTaskManager.ExecuteTaskDirect(t);
            }
            finally
            {
                RemoveFakeTestTask();
            }

        }
        private static void RemoveFakeTestTask()
        {
            using (var context = new IndexingDataContext())
            {
                var sql =
                    @"DECLARE @FakeTaskId INT
                    SELECT @FakeTaskId = IndexingTaskId FROM IndexingTask WHERE Comment = @FakeComment

                    DELETE FROM IndexingActivity WHERE IndexingTaskId = @FakeTaskId
                    DELETE FROM IndexingTask WHERE IndexingTaskId = @FakeTaskId";
                var proc = DataProvider.CreateDataProcedure(sql);
                proc.CommandType = System.Data.CommandType.Text;
                proc.Parameters.Add(new System.Data.SqlClient.SqlParameter("@FakeComment", SqlDbType.NVarChar));
                proc.Parameters["@FakeComment"].Value = _fakeIndexingTaskComment;
                proc.ExecuteNonQuery();
            }
        }

        [TestMethod]
        [Description("A task execution with update activity after delete activity not throws any exception.")]
        public void Indexing_WritingGapAndGettingUnprocessedTasksWithGap()
        {
            var content = Content.CreateNew("Car", TestRoot, "Car_Indexing_WritingGapAndGettingUnprocessedTasksWithGap");
            var handler = (GenericContent)content.ContentHandler;
            content.Save();
            var id = content.Id;
            for (int i = 0; i < 10; i++)
            {
                handler.Index++;
                handler.Save();
            }

            var maxTaskIdSave = LuceneManager._maxTaskId;
            var savedGap = MissingTaskHandler.GetGap();
            try
            {
                LuceneManager._maxTaskId -= 2;
                MissingTaskHandler.SetGap(new List<int>(new[] { maxTaskIdSave - 4, maxTaskIdSave - 5, maxTaskIdSave - 7 }));
                EnsureWriterHasChanges();
                LuceneManager.CommitChanges();
                LuceneManager.RefreshReader();

                var cud = LuceneManager._reader.GetCommitUserData();
                var lastIdStr = cud[IndexManager.LastTaskIdKey];
                var missingStr = cud[IndexManager.MissingTasksKey];

                // [0]: {[LastTaskId, 10]}
                // [1]: {[MissingTasks, 8,7,5]}
                Assert.IsTrue(cud[IndexManager.LastTaskIdKey] == LuceneManager._maxTaskId.ToString(), "#1");
                Assert.IsTrue(cud[IndexManager.MissingTasksKey] == String.Join(",", new[] { maxTaskIdSave - 4, maxTaskIdSave - 5, maxTaskIdSave - 7 }), "#1");

                var mid = LuceneManager._maxTaskId;
                var tasks = IndexingTaskManager.GetUnprocessedTasks(mid, new[] { mid - 2, mid - 3, mid - 5 });
                var exp = String.Join(",", new[] { mid - 5, mid - 3, mid - 2, mid + 1, mid + 2 });
                var cur = String.Join(",", tasks.Select(t => t.IndexingTaskId));
                Assert.AreEqual(exp, cur);
            }
            finally
            {
                MissingTaskHandler.SetGap(savedGap);
                LuceneManager._maxTaskId = maxTaskIdSave;
            }
        }
        private void EnsureWriterHasChanges()
        {
            var doc = new Lucene.Net.Documents.Document();
            var field = new Lucene.Net.Documents.Field("Path", "/root/indexing_writinggapandgettingunprocessedtaskswithgap/fake", LucField.Store.YES, LucField.Index.NOT_ANALYZED, LucField.TermVector.NO);
            doc.Add(field);
            LuceneManager._writer.AddDocument(doc);
        }

        [TestMethod]
        [Description("This test works well but DO NOT RUN IT WITH ANOTHER TEST.")]
        public void Indexing_HealthMonitor()
        {
            var content = Content.CreateNew("Car", TestRoot, "Car_Indexing_HealthMonitor");
            var handler = (GenericContent)content.ContentHandler;
            content.Save();
            var id = content.Id;
            for (int i = 0; i < 10; i++)
            {
                handler.Index++;
                handler.Save();
            }
            //Thread.Sleep(2000);
            var maxTaskIdSave = LuceneManager._maxTaskId;
            var savedGap = MissingTaskHandler.GetGap();
            var missingTaskHandlerAcc = new PrivateType(typeof(MissingTaskHandler));
            var activityQueueAcc = new PrivateObject(ActivityQueue.Instance);
            var activityQueue_checkGapDivider = (int)activityQueueAcc.GetField("_checkGapDivider", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                LuceneManager._maxTaskId -= 2;
                var mid = LuceneManager._maxTaskId;
                MissingTaskHandler.SetGap(new List<int>(new[] { mid - 2, mid - 3, mid - 4 }));

                activityQueueAcc.SetField("_checkGapDivider", BindingFlags.NonPublic | BindingFlags.Instance, 400);
                var y = (int)activityQueueAcc.GetField("_checkGapDivider", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsTrue(y == 400, String.Format("_checkGapDivider is [{0}]. Expected: 400", y));

                var gapAfter = MissingTaskHandler.GetGap();
                for (int i = 0; i < 20; i++) // 10 seconds timeout
                {
                    Thread.Sleep(500);
                    gapAfter = MissingTaskHandler.GetGap();
                    if (gapAfter.Count == 0)
                        break;
                }

                var exp = String.Join(",", new[] { mid - 2, mid - 3 });
                var cur = String.Join(",", gapAfter);
                Assert.IsTrue(gapAfter.Count == 0, String.Format("GapAfter is [{0}]. Expected: empty", cur));
                Assert.IsTrue(LuceneManager._maxTaskId == maxTaskIdSave, String.Format("maxTaskId is {0}. Expected: {1}", LuceneManager._maxTaskId, maxTaskIdSave));
            }
            finally
            {
                MissingTaskHandler.SetGap(savedGap);
                LuceneManager._maxTaskId = maxTaskIdSave;
                activityQueueAcc.SetField("_checkGapDivider", BindingFlags.NonPublic | BindingFlags.Instance, activityQueue_checkGapDivider);
            }
        }

        [TestMethod]
        public void Indexing_VersionTimestampIsSequential()
        {
            var content = Content.CreateNew("Car", TestRoot, "Car_Indexing_VersionTimestampInActivity");
            var handler = (GenericContent)content.ContentHandler;
            var count = 4;
            var ts_versionBefore = new long[count];
            var ts_versionAfter = new long[count];
            var ts_activityBefore = new long[count];
            var ts_activityAfter = new long[count];
            for (int i = 0; i < 4; i++)
            {
                handler.Index++;
                ts_versionBefore[i] = handler.VersionTimestamp;
                ts_activityBefore[i] = GetLastTimestamp(handler.VersionId);
                Assert.IsTrue(ts_versionBefore[i] == ts_activityBefore[i], String.Format("Version and Activity timestamps are not equal (before). Version ts: {0}. Activity ts: {1}, i: {2}", ts_versionBefore[i], ts_activityBefore[i], i));
                content.Save();
                ts_versionAfter[i] = handler.VersionTimestamp;
                ts_activityAfter[i] = GetLastTimestamp(handler.VersionId);
                Assert.IsTrue(ts_versionAfter[i] == ts_activityAfter[i], String.Format("Version and Activity timestamps are not equal (after). Version ts: {0}. Activity ts: {1}, i: {2}", ts_versionAfter[i], ts_activityAfter[i], i));
            }
            for (int i = 0; i < count; i++)
                Assert.IsTrue(ts_versionAfter[i] > ts_versionBefore[i], String.Format("Timestamp after ({0}) is not greater than timestamp before ({1}). Index: {2}", ts_versionAfter[i], ts_versionBefore[i], i));
            for (int i = 0; i < count - 1; i++)
                Assert.IsTrue(ts_versionAfter[i] < ts_versionAfter[i + 1], String.Format("Timestamp #{0} ({1}) is not greater than #{2} ({3})", i + 1, ts_versionAfter[i + 1], i, ts_versionAfter[i]));
        }
        [TestMethod]
        public void Indexing_VersionTimestampInIndex()
        {
            var content = Content.CreateNew("Car", TestRoot, "Car_Indexing_VersionTimestampInIndex");
            var handler = (GenericContent)content.ContentHandler;
            var count = 4;
            var ts_versionBefore = new long[count];
            var ts_versionAfter = new long[count];
            var ts_activityBefore = new long[count];
            var ts_activityAfter = new long[count];
            var ts_index = new long[count];
            for (int i = 0; i < 4; i++)
            {
                handler.Index++;
                ts_versionBefore[i] = handler.VersionTimestamp;
                ts_activityBefore[i] = GetLastTimestamp(handler.VersionId);
                content.Save();
                ts_versionAfter[i] = handler.VersionTimestamp;
                ts_activityAfter[i] = GetLastTimestamp(handler.VersionId);
                ts_index[i] = GetLastTimestampFromIndex(handler.VersionId);
            }
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(ts_versionBefore[i] == ts_activityBefore[i], String.Format("Version and Activity timestamps are not equal (before). Version ts: {0}. Activity ts: {1}, i: {2}", ts_versionBefore[i], ts_activityBefore[i], i));
                Assert.IsTrue(ts_versionAfter[i] == ts_activityAfter[i], String.Format("Version and Activity timestamps are not equal (after). Version ts: {0}. Activity ts: {1}, i: {2}", ts_versionAfter[i], ts_activityAfter[i], i));
                Assert.IsTrue(ts_versionAfter[i] == ts_index[i], String.Format("Version and Index timestamps are not equal (after). Version ts: {0}. Index ts: {1}, i: {2}", ts_versionAfter[i], ts_index[i], i));
            }
            for (int i = 0; i < count; i++)
                Assert.IsTrue(ts_versionAfter[i] > ts_versionBefore[i], String.Format("Timestamp after ({0}) is not greater than timestamp before ({1}). Index: {2}", ts_versionAfter[i], ts_versionBefore[i], i));
            for (int i = 0; i < count - 1; i++)
                Assert.IsTrue(ts_versionAfter[i] < ts_versionAfter[i + 1], String.Format("Timestamp #{0} ({1}) is not greater than #{2} ({3})", i + 1, ts_versionAfter[i + 1], i, ts_versionAfter[i]));
        }
        private long GetLastTimestamp(int versionId)
        {
            using (var context = new IndexingDataContext())
            {
                var activity = context.IndexingActivities.Where(a => a.VersionId == versionId).OrderByDescending(a => a.IndexingActivityId).FirstOrDefault();
                if (activity == null)
                    return 0;
                return activity.VersionTimestamp ?? 0;
            }

        }
        private long GetLastTimestampFromIndex(int versionId)
        {
            var document = LuceneManager.GetDocumentByVersionId(versionId)[0];
            var vt = document.Get(LucObject.FieldName.VersionTimestamp);
            var versionTimestamp = Convert.ToInt64(vt);
            return versionTimestamp;
        }

        [TestMethod]
        [Description("This test works well but DO NOT RUN IT WITH ANOTHER TEST.")]
        public void __Indexing_IndexIntegrityCheck()
        {
            var content = Content.CreateNew("Folder", TestRoot, "1");
            var folder = (Folder)content.ContentHandler;
            folder.InheritableVersioningMode = InheritableVersioningType.MajorOnly;
            content.Save();


            content = Content.CreateNew("Car", TestRoot, "Car_Indexing_IntegrityCheck_1");
            content.Save();
            var nodeId_1 = content.Id;
            var versionId_1 = content.ContentHandler.VersionId;
            content = Content.CreateNew("Car", TestRoot, "Car_Indexing_IntegrityCheck_2");
            content.Save();
            var nodeId_2 = content.Id;
            var versionId_2 = content.ContentHandler.VersionId;
            content = Content.CreateNew("Car", folder, "Car_Indexing_IntegrityCheck_3");
            content.Save();
            var nodeId_3 = content.Id;
            var versionId_3 = content.ContentHandler.VersionId;
            content = Content.CreateNew("Car", folder, "Car_Indexing_IntegrityCheck_4");
            content.Save();
            var gc = (GenericContent)content.ContentHandler;
            var nodeId_4 = content.Id;
            var versionId_4 = content.ContentHandler.VersionId;
            var path_4 = gc.Path;
            gc.Index++;
            gc.Save();
            var versionId_5 = content.ContentHandler.VersionId;
            gc.Index++;
            gc.Save();
            var versionId_6 = content.ContentHandler.VersionId;


            var diff0 = IntegrityChecker.Check();
            if(diff0.Count() > 0)
                Assert.Inconclusive("Cannot use this test if the initial state is inconsistent.");

            AddFakeDocument(nodeId_1);
            DeleteDocument(versionId_2);
            Exec(String.Format("UPDATE Nodes SET [Index] = [Index] WHERE NodeId = {0}", nodeId_3));
            Exec(String.Format("UPDATE Versions SET [Status] = [Status] WHERE VersionId = {0}", versionId_4));

            //---- Total 
            var diff1 = IntegrityChecker.Check().ToArray();

            Assert.IsTrue(diff1.Length == 4, String.Format("Count is {0}, expected: 4", diff1.Length));

            Assert.IsTrue(diff1[0].Kind == IndexDifferenceKind.NotInIndex, String.Format("diff1[0].Kind is {0}, expected: NotInIndex", diff1[0].Kind));
            Assert.IsTrue(diff1[0].VersionId == versionId_2, String.Format("diff1[0].Kind is {0}, expected: {1}", diff1[0].VersionId, versionId_2));

            Assert.IsTrue(diff1[1].Kind == IndexDifferenceKind.DifferentNodeTimestamp, String.Format("diff1[1].Kind is {0}, expected: NotInIndex", diff1[1].Kind));
            Assert.IsTrue(diff1[1].VersionId == versionId_3, String.Format("diff1[1].VersionId is {0}, expected: {1}", diff1[1].VersionId, versionId_3));
            Assert.IsTrue(diff1[1].DbNodeTimestamp != diff1[1].IxNodeTimestamp, "diff1[1].DbNodeTimestamp == diff1[1].IxNodeTimestamp, expected: different.");
            Assert.IsTrue(diff1[1].DbVersionTimestamp == diff1[1].IxVersionTimestamp, "diff1[1].DbVersionTimestamp != diff1[1].IxVersionTimestamp, expected: equal.");

            Assert.IsTrue(diff1[2].Kind == IndexDifferenceKind.DifferentVersionTimestamp, String.Format("diff1[2].Kind is {0}, expected: NotInIndex", diff1[2].Kind));
            Assert.IsTrue(diff1[2].VersionId == versionId_4, String.Format("diff1[2].VersionId is {0}, expected: {1}", diff1[2].VersionId, versionId_4));
            Assert.IsTrue(diff1[2].DbNodeTimestamp != diff1[2].IxNodeTimestamp, "diff1[2].DbNodeTimestamp == diff1[2].IxNodeTimestamp, expected: different."); // older version !
            Assert.IsTrue(diff1[2].DbVersionTimestamp != diff1[2].IxVersionTimestamp, "diff1[2].DbVersionTimestamp == diff1[2].IxVersionTimestamp, expected: different.");

            Assert.IsTrue(diff1[3].Kind == IndexDifferenceKind.NotInDatabase, String.Format("diff1[3].Kind is {0}, expected: NotInDatabase", diff1[3].Kind));
            Assert.IsTrue(diff1[3].VersionId == 99999, String.Format("diff1[3].VersionId is {0}, expected: 99999", diff1[3].VersionId));
            Assert.IsTrue(diff1[3].NodeId == 99999, String.Format("diff1[3].NodeId is {0}, expected: 99999", diff1[3].NodeId));
            Assert.IsTrue(diff1[3].Path == "/root/fakedocument", String.Format("diff1[3].Path is {0}, expected: /root/fakedocument", diff1[3].Path));

            //---- Subtree
            var diff2 = IntegrityChecker.Check(folder.Path, true).ToArray();

            Assert.IsTrue(diff2[0].Kind == IndexDifferenceKind.DifferentNodeTimestamp, String.Format("diff3[0].Kind is {0}, expected: NotInIndex", diff2[0].Kind));
            Assert.IsTrue(diff2[0].VersionId == versionId_3, String.Format("diff3[0].VersionId is {0}, expected: {1}", diff2[0].VersionId, versionId_3));
            Assert.IsTrue(diff2[0].DbNodeTimestamp != diff2[0].IxNodeTimestamp, "diff3[0].DbNodeTimestamp == diff3[0].IxNodeTimestamp, expected: different.");
            Assert.IsTrue(diff2[0].DbVersionTimestamp == diff2[0].IxVersionTimestamp, "diff3[0].DbVersionTimestamp != diff3[0].IxVersionTimestamp, expected: equal.");

            Assert.IsTrue(diff2[1].Kind == IndexDifferenceKind.DifferentVersionTimestamp, String.Format("diff3[1].Kind is {0}, expected: NotInIndex", diff2[1].Kind));
            Assert.IsTrue(diff2[1].VersionId == versionId_4, String.Format("diff3[1].VersionId is {0}, expected: {1}", diff2[1].VersionId, versionId_4));
            Assert.IsTrue(diff2[1].DbNodeTimestamp != diff2[1].IxNodeTimestamp, "diff3[1].DbNodeTimestamp == diff3[1].IxNodeTimestamp, expected: different."); // older version
            Assert.IsTrue(diff2[1].DbVersionTimestamp != diff2[1].IxVersionTimestamp, "diff3[1].DbVersionTimestamp == diff3[1].IxVersionTimestamp, expected: different.");

            //---- Node not recursive not changed
            var diff3 = IntegrityChecker.Check(folder.Path, false).ToArray();
            Assert.IsTrue(diff3.Length == 0, String.Format("diff3.Length is {0}, expected: 0.", diff3.Length));

            //---- Node not recursive an old version changed
            var diff4 = IntegrityChecker.Check(path_4, false).ToArray();
            Assert.IsTrue(diff4.Length == 1, String.Format("diff4.Length is {0}, expected: 1.", diff3.Length));

            Assert.IsTrue(diff4[0].Kind == IndexDifferenceKind.DifferentVersionTimestamp, String.Format("diff4[0].Kind is {0}, expected: NotInIndex", diff4[0].Kind));
            Assert.IsTrue(diff4[0].VersionId == versionId_4, String.Format("diff4[0].VersionId is {0}, expected: {1}", diff4[0].VersionId, versionId_4));
            Assert.IsTrue(diff4[0].DbNodeTimestamp != diff4[0].IxNodeTimestamp, "diff4[0].DbNodeTimestamp == diff4[0].IxNodeTimestamp, expected: different."); // older version
            Assert.IsTrue(diff4[0].DbVersionTimestamp != diff4[0].IxVersionTimestamp, "diff4[0].DbVersionTimestamp == diff4[0].IxVersionTimestamp, expected: different.");

        }
        private void AddFakeDocument(int fromNodeId)
        {
            //minimal fakeobject: NodeId, VersionId, Path, Version, NodeTimestamp, VersionTimestamp

            var node = Node.LoadNode(fromNodeId);
            var doc = IndexDocumentInfo.CreateDocument(node);
            doc.RemoveField(LucObject.FieldName.NodeId);
            doc.RemoveField(LucObject.FieldName.VersionId);
            doc.RemoveField(LucObject.FieldName.Name);
            doc.RemoveField(LucObject.FieldName.Path);

            var nf = new NumericField(LucObject.FieldName.NodeId, LucField.Store.YES, true);
            nf.SetIntValue(99999);
            doc.Add(nf);
            nf = new NumericField(LucObject.FieldName.VersionId, LucField.Store.YES, true);
            nf.SetIntValue(99999);
            doc.Add(nf);
            doc.Add(new LucField(LucObject.FieldName.Name, "fakedocument", LucField.Store.YES, LucField.Index.NOT_ANALYZED, LucField.TermVector.NO));
            doc.Add(new LucField(LucObject.FieldName.Path, "/root/fakedocument", LucField.Store.YES, LucField.Index.NOT_ANALYZED, LucField.TermVector.NO));

            LuceneManager.AddCompleteDocument(doc);
            LuceneManager.CommitChanges();
            LuceneManager.RefreshReader();
        }
        private void DeleteDocument(int versionId)
        {
            var term = new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId));
            LuceneManager._writer.DeleteDocuments(term);
            LuceneManager.CommitChanges();
            LuceneManager.RefreshReader();
        }
        private void Exec(string sql)
        {
            var proc = DataProvider.CreateDataProcedure(sql);
            proc.CommandType = CommandType.Text;
            proc.ExecuteNonQuery();
        }
    }
}
