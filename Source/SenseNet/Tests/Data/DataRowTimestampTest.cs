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
                // LuceneManager._isDirtyReader = true; //??/
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
        [Description("An activity execution with update activity after delete activity not throws any exception.")] // ??
        public void Indexing_ActivitesWithMissingVersion()
        {
            var content = Content.CreateNew("Car", TestRoot, "Car_Indexing_ActivitesWithMissingVersion");
            var handler = (GenericContent)content.ContentHandler;
            //handler.VersioningMode = VersioningType.None;
            content.Save();
            var id = content.Id;

            LuceneManager.ApplyChanges();
            IndexingActivity[] act = new IndexingActivity[3];
            act[0] = new IndexingActivity
            {
                ActivityType = IndexingActivityType.RemoveDocument,
                NodeId = _fakeId,
                VersionId = _fakeId
            };
            act[1] = new IndexingActivity
            {
                ActivityType = IndexingActivityType.UpdateDocument,
                NodeId = _fakeId,
                VersionId = _fakeId
            };
            act[2] = new IndexingActivity
            {
                ActivityType = IndexingActivityType.AddDocument,
                NodeId = _fakeId,
                VersionId = _fakeId
            };

            try
            {
                using (var context = new IndexingDataContext())
                {
                    foreach (var a in act)
                    {
                        context.IndexingActivities.InsertOnSubmit(a);
                        context.SubmitChanges();
                    }
                }

                var max = 0;
                var activities = IndexingActivityManager.GetUnprocessedActivities(act[2].IndexingActivityId - 1, out max);
                foreach (var a in activities)
                    IndexingActivityManager.ExecuteActivityDirect(a);
            }
            finally
            {
                RemoveFakeTestActivity();
            }

        }
        private static void RemoveFakeTestActivity()
        {
            using (var context = new IndexingDataContext())
            {
                var sql = @"DELETE FROM IndexingActivity WHERE VersionId = @FakeId";
                var proc = DataProvider.CreateDataProcedure(sql);
                proc.CommandType = System.Data.CommandType.Text;
                proc.Parameters.Add(new System.Data.SqlClient.SqlParameter("@FakeId", SqlDbType.Int));
                proc.Parameters["@FakeId"].Value = _fakeId;
                proc.ExecuteNonQuery();
            }
        }

        [TestMethod]
        [Description("An activity execution with update activity after delete activity not throws any exception.")] // ??
        public void Indexing_WritingGapAndGettingUnprocessedActivitiesWithGap()
        {
            var content = Content.CreateNew("Car", TestRoot, "Indexing_WritingGapAndGettingUnprocessedActivitiesWithGap");
            var handler = (GenericContent)content.ContentHandler;
            content.Save();
            var id = content.Id;
            for (int i = 0; i < 10; i++)
            {
                handler.Index++;
                handler.Save();
            }

            var maxActivityIdSave = MissingActivityHandler.MaxActivityId;
            var savedGap = MissingActivityHandler.GetGap();
            try
            {
                MissingActivityHandler.MaxActivityId -= 2;
                MissingActivityHandler.SetGap(new List<int>(new[] { maxActivityIdSave - 4, maxActivityIdSave - 5, maxActivityIdSave - 7 }));
                EnsureWriterHasChanges();

                //LuceneManager.ApplyChanges();
                var luceneManagerAcc = new PrivateType(typeof(LuceneManager));
                luceneManagerAcc.InvokeStatic("Commit", BindingFlags.Static | BindingFlags.NonPublic, new object[] { true });

                IDictionary<string, string> cud;
                using (var readerFrame = LuceneManager.GetIndexReaderFrame())
                {
                    cud = readerFrame.IndexReader.GetCommitUserData();
                }

                var lastIdStr = cud[IndexManager.LastActivityIdKey];
                var missingStr = cud[IndexManager.MissingActivitiesKey];

                // [0]: {[LastActivityId, 10]}
                // [1]: {[MissingActivities, 8,7,5]}
                Assert.IsTrue(cud[IndexManager.LastActivityIdKey] == MissingActivityHandler.MaxActivityId.ToString(), "#1");
                Assert.IsTrue(cud[IndexManager.MissingActivitiesKey] == String.Join(",", new[] { maxActivityIdSave - 4, maxActivityIdSave - 5, maxActivityIdSave - 7 }), "#2");

                var mid = MissingActivityHandler.MaxActivityId;
                var activities = IndexingActivityManager.GetUnprocessedActivities(new[] { mid - 2, mid - 3, mid - 5 });
                var exp = String.Join(",", new[] { mid - 5, mid - 3, mid - 2 });
                var cur = String.Join(",", activities.Select(t => t.IndexingActivityId));
                Assert.AreEqual(exp, cur);

                var max = 0;
                activities = IndexingActivityManager.GetUnprocessedActivities(mid, out max);
                exp = String.Join(",", new[] { mid + 1, mid + 2 });
                cur = String.Join(",", activities.Select(t => t.IndexingActivityId));
                Assert.AreEqual(exp, cur);
            }
            finally
            {
                MissingActivityHandler.SetGap(savedGap);
                MissingActivityHandler.MaxActivityId = maxActivityIdSave;
            }
        }
        private void EnsureWriterHasChanges()
        {
            var doc = new Lucene.Net.Documents.Document();
            var field = new Lucene.Net.Documents.Field("Path", "/root/indexing_writinggapandgettingunprocessedactivitiesswithgap/fake", LucField.Store.YES, LucField.Index.NOT_ANALYZED, LucField.TermVector.NO);
            doc.Add(field);
            LuceneManager._writer.AddDocument(doc);
        }

        [TestMethod]
        public void Indexing_PlayMissingactivitiesTwoTimes()
        {
            var content = Content.CreateNew("Car", TestRoot, "Indexing_PlayMissingactivitiesTwoTimes");
            var handler = (GenericContent)content.ContentHandler;
            content.Save();
            var id = content.Id;
            for (int i = 0; i < 10; i++)
            {
                handler.Index++;
                handler.Save();
            }
            var maxActivityIdSave = MissingActivityHandler.MaxActivityId;

            try
            {
                MissingActivityHandler.MaxActivityId -= 3;
                MissingActivityHandler.SetGap(new List<int>(new[] { maxActivityIdSave - 4, maxActivityIdSave - 5, maxActivityIdSave - 6, maxActivityIdSave - 7 }));
                LuceneManager.ExecuteUnprocessedIndexingActivities(null);

                MissingActivityHandler.MaxActivityId -= 3;
                MissingActivityHandler.SetGap(new List<int>(new[] { maxActivityIdSave - 4, maxActivityIdSave - 5, maxActivityIdSave - 6, maxActivityIdSave - 7 }));
                LuceneManager.ExecuteUnprocessedIndexingActivities(null);

                Assert.IsTrue(maxActivityIdSave == MissingActivityHandler.MaxActivityId, string.Format("MissingActivityHandler.MaxActivityId is {0}, expected {1}", maxActivityIdSave, MissingActivityHandler.MaxActivityId));
                var gap = MissingActivityHandler.GetGap();
                var gapString = string.Join(", ", gap);
                Assert.IsTrue(gap.Count == 0, string.Format("The gap is {0}, expected: empty", gapString));
            }
            finally
            {
                MissingActivityHandler.MaxActivityId = maxActivityIdSave;
            }

            var result = ContentQuery.Query("Name:Indexing_PlayMissingactivitiesTwoTimes .AUTOFILTERS:OFF");
            Assert.IsTrue(result.Count == 1, string.Format("The result count is {0}, expected: 1", result.Count));
        }

        [TestMethod]
        public void Indexing_ExecuteUnprocessedIndexingActivities()
        {
            lock (LuceneManager._executingUnprocessedIndexingActivitiesLock)    // make sure indexhealthmonitor will not overlap
            {
                var initInfo = InitCarsForUnprocessedTests();
                var carlist = initInfo.Item1;
                var lastActivityId = initInfo.Item2;
                var expectedLastActivityId = initInfo.Item3;

                // generate a gap and delete corresponding documents from index
                var activities = GetCarActivities(lastActivityId);
                var activitiesToDelete = new IndexingActivity[] { activities[0], activities[2], activities[5], activities[7], activities[8], activities[9] };

                MissingActivityHandler.SetGap(activitiesToDelete.Take(3).Select(a => a.IndexingActivityId).ToList());   // 0,2,5 will be missing
                MissingActivityHandler.MaxActivityId = activities[6].IndexingActivityId;                                // 7,8,9 will be missing
                
                // commit gap and maxactivityid to the index
                EnsureWriterHasChanges();
                LuceneManager.Commit(true);

                foreach (var activity in activitiesToDelete)
                {
                    DeleteVersionFromIndex(activity.VersionId);
                    DeleteVersionIdFromIndexingHistory(activity.VersionId);
                }


                // check: cars deleted can NOT be found in the index
                for (var i = 0; i < carlist.Count; i++)
                {
                    var id = carlist[i].Id;
                    if (activitiesToDelete.Select(a => a.NodeId).Contains(id))
                        Assert.IsFalse(CheckCarInIndex(id), "Deleted car can still be found in the index.");    // deleted car should not be in index
                    else
                        Assert.IsTrue(CheckCarInIndex(id), "Untouched car can not be found in the index.");     // untouched car should still be in index
                }


                // execute unprocessed indexing tasks
                LuceneManager.ExecuteUnprocessedIndexingActivities(null);


                // check: all cars can be found in the index again
                for (var i = 0; i < carlist.Count; i++)
                {
                    Assert.IsTrue(CheckCarInIndex(carlist[i].Id), "ExecuteUnprocessedIndexingActivities did not repair lost document.");
                }

                Assert.AreEqual(expectedLastActivityId, MissingActivityHandler.MaxActivityId, "Maxtaskid was not updated correctly.");
                Assert.AreEqual(0, MissingActivityHandler.GetGap().Count, "Gap size is not 0.");
            }
        }

        [TestMethod]
        public void Indexing_ExecuteLostIndexingActivities()
        {
            lock (LuceneManager._executingUnprocessedIndexingActivitiesLock)    // make sure indexhealthmonitor will not overlap
            {
                var initInfo = InitCarsForUnprocessedTests();
                var carlist = initInfo.Item1;
                var lastActivityId = initInfo.Item2;
                var expectedLastActivityId = initInfo.Item3;

                // generate a gap and delete corresponding documents from index
                var activities = GetCarActivities(lastActivityId);
                var activitiesToDelete = new IndexingActivity[] { activities[0], activities[2], activities[5], activities[7], activities[8] };

                MissingActivityHandler.SetGap(activitiesToDelete.Select(a => a.IndexingActivityId).ToList());   // 0,2,5,7,8 will be missing

                // commit gap and maxactivityid to the index
                EnsureWriterHasChanges();
                LuceneManager.Commit(true);

                foreach (var activity in activitiesToDelete)
                {
                    DeleteVersionFromIndex(activity.VersionId);
                    DeleteVersionIdFromIndexingHistory(activity.VersionId);
                }


                // check: cars deleted can NOT be found in the index
                for (var i = 0; i < carlist.Count; i++)
                {
                    var id = carlist[i].Id;
                    if (activitiesToDelete.Select(a => a.NodeId).Contains(id))
                        Assert.IsFalse(CheckCarInIndex(id), "Deleted car can still be found in the index.");    // deleted car should not be in index
                    else
                        Assert.IsTrue(CheckCarInIndex(id), "Untouched car can not be found in the index.");     // untouched car should still be in index
                }


                // make sure to move current gap to oldest gap, since always the oldest gap will get processed
                for (var i = 0; i < MissingActivityHandler.GapSegments - 1; i++)
                    MissingActivityHandler.GetOldestGapAndMoveToNext();

                // execute unprocessed indexing tasks
                LuceneManager.ExecuteLostIndexingActivities();


                // check: all cars can be found in the index again
                for (var i = 0; i < carlist.Count; i++)
                {
                    Assert.IsTrue(CheckCarInIndex(carlist[i].Id), "ExecuteLostIndexingActivities did not repair lost document.");
                }

                Assert.AreEqual(expectedLastActivityId, MissingActivityHandler.MaxActivityId, "Maxtaskid is not what is expected.");
                Assert.AreEqual(0, MissingActivityHandler.GetGap().Count, "Gap size is not 0.");
            }
        }
        private IndexingActivity[] GetCarActivities(int lastActivityId)
        {
            using (var context = new IndexingDataContext())
            {
                context.CommandTimeout = RepositoryConfiguration.SqlCommandTimeout;
                return context.IndexingActivities.Where(a => a.IndexingActivityId > lastActivityId).OrderBy(b => b.IndexingActivityId).ToArray();
            }
        }
        private Tuple<List<GenericContent>, int, int> InitCarsForUnprocessedTests()
        {
            // init: create some cars
            var container = new Folder(TestRoot);
            container.Name = "unprocessedtest-" + Guid.NewGuid().ToString();
            container.Save();

            var lastActivityId = IndexingActivityManager.GetLastActivityId();

            var carlist = new List<GenericContent>();
            for (var i = 0; i < 10; i++)
            {
                var car = new GenericContent(container, "Car");
                car.Name = "testcar" + i.ToString();
                car.Save();
                carlist.Add(car);
            }

            var expectedLastActivityId = IndexingActivityManager.GetLastActivityId();

            // check1: cars can be found in the index
            for (var i = 0; i < carlist.Count; i++)
            {
                Assert.IsTrue(CheckCarInIndex(carlist[i].Id), "Car cannot be found in index after init.");
            }

            return new Tuple<List<GenericContent>, int, int>(carlist, lastActivityId, expectedLastActivityId);
        }
        private bool CheckCarInIndex(int versionid)
        {
            return ContentQuery.Query("Id:" + versionid, new QuerySettings { EnableAutofilters = false }).Count == 1;
        }
        internal static void DeleteVersionFromIndex(int versionid)
        {
            var delTerm = new Term(LuceneManager.KeyFieldName, NumericUtils.IntToPrefixCoded(versionid));
            LuceneManager.DeleteDocuments(new[] { delTerm }, false);
        }
        internal static void DeleteVersionIdFromIndexingHistory(int versionId)
        {
            var history = LuceneManager._history;
            var historyAcc = new PrivateObject(history);
            var storage = (Dictionary<int, long>)historyAcc.GetField("_storage");
            storage.Remove(versionId);
        }

        [TestMethod]
        public void Indexing_ExecutingUnprocessedDoesNotDuplicate()
        {
            var contentName = "Indexing_ExecutingUnprocessedDoesNotDuplicate";
            var query = ContentQuery.CreateQuery(".COUNTONLY Name:" + contentName, new QuerySettings { EnableAutofilters = false, EnableLifespanFilter = false });

            var count = query.Execute().Count;
            if (count > 0)
                Assert.Inconclusive();

            var lastActivityId = MissingActivityHandler.MaxActivityId;

            var content = Content.CreateNew("Car", TestRoot, contentName);
            content.Save();

            content.ContentHandler.Index++;
            content.Save();

            content.ContentHandler.Index++;
            content.Save();

            count = query.Execute().Count;
            Assert.IsTrue(count == 1, String.Format("Before executing unprocessed activities found {0}, expected: 1.", count));

            MissingActivityHandler.MaxActivityId -= 3;
            LuceneManager.ExecuteUnprocessedIndexingActivities(null);

            count = query.Execute().Count;
            Assert.IsTrue(count == 1, String.Format("After executing unprocessed activities #1 found {0}, expected: 1.", count));

            count = ContentQuery.Query(".COUNTONLY +Index:2 +Name:" + contentName, new QuerySettings { EnableAutofilters = false, EnableLifespanFilter = false }).Count;
            Assert.IsTrue(count == 1, String.Format("After executing unprocessed activities #2 found {0}, expected: 1.", count));
        }

        [TestMethod]
        public void Indexing_HandlingGap()
        {
            //if (MissingActivityHandler.GetGap().Count != 0)
            //    Assert.Inconclusive("This test cannot run correctly with any gap.");

            MissingActivityHandler.SetGap(new List<int>());
            var savedMaxActivityId = MissingActivityHandler.MaxActivityId;
            MissingActivityHandler.MaxActivityId = 0;

            try
            {
                for (int i = 1; i <= MissingActivityHandler.GapSegments * 2; i++)
                {
                    var currentActivityId = i * 3;
                    MissingActivityHandler.RemoveActivityAndAddGap(currentActivityId);
                    Assert.IsTrue(MissingActivityHandler.MaxActivityId == currentActivityId, String.Format("MaxActivityId is {0}, expected: {1}", MissingActivityHandler.MaxActivityId, currentActivityId));

                    if (i % 2 == 0)
                        MissingActivityHandler.GetOldestGapAndMoveToNext();
                }

                var expectedGapSize = MissingActivityHandler.GetGap().Count;
                for (int i = 1; i <= MissingActivityHandler.MaxActivityId; i += 3)
                {
                    MissingActivityHandler.RemoveActivityAndAddGap(i);
                    var gapSize =  MissingActivityHandler.GetGap().Count;
                    --expectedGapSize;
                    Assert.IsTrue(gapSize == expectedGapSize, String.Format("Gap size is {0}, expected: {1}", gapSize, expectedGapSize));
                }

                var gap = MissingActivityHandler.GetGap();
                Assert.IsTrue(gap.Count == MissingActivityHandler.GapSegments*2, "");
                gap.Sort();
                var expectedGap = "2,5,8,11,14,17,20,23,26,29,32,35,38,41,44,47,50,53,56,59";
                var gapstring = String.Join(",", gap);
                Assert.IsTrue(gapstring == expectedGap, String.Format("Gap size is {0}, expected: {1}", gapstring, expectedGap));

            }
            finally
            {
                MissingActivityHandler.SetGap(new List<int>());
                MissingActivityHandler.MaxActivityId = savedMaxActivityId;
            }
        }

        [TestMethod]
        public void Indexing_RareCommitRareReopen()
        {
            //rare commit: 1 adddoc, aztán vársz, új readered kell legyen. több addoc, aztán vársz, nem kell új reader, csak később.
            //rare reopen: több adddoc után nincs új reader. több addoc és 1 olvasás után van új reader. commit közben ne történjen, mert az bekavar.

            var delay1 = Convert.ToInt32(RepositoryConfiguration.CommitDelayInSeconds * 1000.0);
            var delay4 = delay1 * 4;
            var delay033 = delay1 / 3;

            var writerInstance = LuceneManager._writer;
            var readerInstance = LuceneManager._reader;

            //-- one import (means: one activity in "delay1" period).
            var content = Content.CreateNew("Car", TestRoot, null);
            content.Save();
            if (!object.ReferenceEquals(readerInstance, LuceneManager._reader)) // ha a határon volt és épp most volt commit
            {
                readerInstance = LuceneManager._reader;
                content = Content.CreateNew("Car", TestRoot, null);
                content.Save();
            }
            Assert.IsTrue(object.ReferenceEquals(readerInstance, LuceneManager._reader), "Reader is changed after first save.");

            Thread.Sleep(delay4);
            Assert.IsFalse(object.ReferenceEquals(readerInstance, LuceneManager._reader), "Reader isn't changed after first save and a 2 sec delay.");

            //-- batch import (means: more activity in "delay1" period).
            content = Content.CreateNew("Car", TestRoot, null);
            content.Save();
            Thread.Sleep(delay033);
            readerInstance = LuceneManager._reader;
            var expectedMaxChanges = 2;
            var changes = 0;
            for (int i = 0; i < 10; i++)
            {
                content = Content.CreateNew("Car", TestRoot, null);
                content.Save();
                Debug.WriteLine("##> Batch save #" + i);
                if (!object.ReferenceEquals(readerInstance, LuceneManager._reader))
                    changes++;

                if (i == 5)
                {
                    Debug.WriteLine("##> **** GetIndexReaderFrame");
                    LuceneManager.GetIndexReaderFrame(); //-- ensure reopen
                    Assert.IsFalse(object.ReferenceEquals(readerInstance, LuceneManager._reader), "Reader isn't changed after calling GetIndexReaderFrame.");
                    readerInstance = LuceneManager._reader;
                }
                else
                {
                    Thread.Sleep(delay033);
                }
            }
            Assert.IsTrue(changes <= expectedMaxChanges, String.Format("Reader is changed {0} times, allowed max: {1}", changes, expectedMaxChanges));

            //-- release and overrun last batch
            Thread.Sleep(delay1 * 2);
            Debug.WriteLine("##> *********** Last check");
            Assert.IsFalse(object.ReferenceEquals(readerInstance, LuceneManager._reader), "Reader isn't changed after last delay.");
        }


        [TestMethod]
        [Description("This test works well but DO NOT RUN IT WITH ANOTHER TEST.")]
        public void __Indexing_HealthMonitor()
        {
            Assert.Inconclusive();
            /*
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
            var maxActivityIdSave = MissingActivityHandler.MaxActivityId;
            var savedGap = MissingActivityHandler.GetGap();
            var MissingActivityHandlerAcc = new PrivateType(typeof(MissingActivityHandler));
            var activityQueueAcc = new PrivateObject(ActivityQueue.Instance);
            var activityQueue_checkGapDivider = (int)activityQueueAcc.GetField("_checkGapDivider", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                MissingActivityHandler.MaxActivityId -= 2;
                var mid = MissingActivityHandler.MaxActivityId;
                MissingActivityHandler.SetGap(new List<int>(new[] { mid - 2, mid - 3, mid - 4 }));

                activityQueueAcc.SetField("_checkGapDivider", BindingFlags.NonPublic | BindingFlags.Instance, 400);
                var y = (int)activityQueueAcc.GetField("_checkGapDivider", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsTrue(y == 400, String.Format("_checkGapDivider is [{0}]. Expected: 400", y));

                var gapAfter = MissingActivityHandler.GetGap();
                for (int i = 0; i < 20; i++) // 10 seconds timeout
                {
                    Thread.Sleep(500);
                    gapAfter = MissingActivityHandler.GetGap();
                    if (gapAfter.Count == 0)
                        break;
                }

                var exp = String.Join(",", new[] { mid - 2, mid - 3 });
                var cur = String.Join(",", gapAfter);
                Assert.IsTrue(gapAfter.Count == 0, String.Format("GapAfter is [{0}]. Expected: empty", cur));
                Assert.IsTrue(MissingActivityHandler.MaxActivityId == maxActivityIdSave, String.Format("maxActivityId is {0}. Expected: {1}", MissingActivityHandler.MaxActivityId, maxActivityIdSave));
            }
            finally
            {
                MissingActivityHandler.SetGap(savedGap);
                MissingActivityHandler.MaxActivityId = maxActivityIdSave;
                activityQueueAcc.SetField("_checkGapDivider", BindingFlags.NonPublic | BindingFlags.Instance, activityQueue_checkGapDivider);
            }
            */
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
            LuceneManager.ApplyChanges();
        }
        private void DeleteDocument(int versionId)
        {
            var term = new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId));
            LuceneManager._writer.DeleteDocuments(term);
            LuceneManager.ApplyChanges();
        }
        private void Exec(string sql)
        {
            var proc = DataProvider.CreateDataProcedure(sql);
            proc.CommandType = CommandType.Text;
            proc.ExecuteNonQuery();
        }
    }
}
