using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search.Indexing;
using SenseNet.Search;
using SenseNet.Search.Indexing.Activities;
using SenseNet.ContentRepository.Tests.Data;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace SenseNet.ContentRepository.Tests.Search
{
    [TestClass]
    public class IndexingHistorytests : TestBase
    {
        class IndexingHistoryAccessor : Accessor
        {
            public IndexingHistoryAccessor(IndexingHistory target) : base(target) { }

            public Queue<int> Queue { get { return (Queue<int>)GetPrivateField("_queue"); } }
            public Dictionary<int, long> Storage { get { return (Dictionary<int, long>)GetPrivateField("_storage"); } }

            internal void Initialize(int size)
            {
                CallPrivateMethod("Initialize", size);
            }
        }

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
        #region TestRoot - ClassInitialize - ClassCleanup
        private static string _testRootName = "_IndexingHistorytests";
        private static string __testRootPath = String.Concat("/Root/", _testRootName);
        private Folder __testRoot;
        private Folder TestRoot
        {
            get
            {
                if (__testRoot == null)
                {
                    __testRoot = (Folder)Node.LoadNode(__testRootPath);
                    if (__testRoot == null)
                    {
                        Folder folder = new Folder(Repository.Root);
                        folder.Name = _testRootName;
                        folder.Save();
                        __testRoot = (Folder)Node.LoadNode(__testRootPath);
                    }
                }
                return __testRoot;
            }
        }

        //[ClassInitialize]
        //public static void InitializeEngine(TestContext testContext)
        //{
        //    StorageContext.Search.IsOuterEngineEnabled = true;
        //}
        [ClassCleanup]
        public static void DestroySandBox()
        {
            try
            {
                Node.ForceDelete(__testRootPath);
            }
            catch (Exception e)
            {
                int q = 1;
            }
        }

        #endregion


        //==================================================================

        /* General comment on indexinghistory overlapping tests */
        // The following tests are to test very rare edge cases: the updating of indexinghistory and indexing is done in two separate steps.
        // For this reason it could happen that two overlapping indexing activities both execute indexing, but the earlier update is executed
        // later, and this could result in outdated data being indexed or even ghost documents (in case of delete + add overlapping).
        // Whenever this happens an optimistic approach is used to recover the last index. This approach is implemented in 
        // LuceneManager.AddCompleteDocument, LuceneManager.AddDocument, LuceneManager.UpdateDocument, LuceneManager.DeleteDocuments
        // 
        // A special edge-case of the edge-case is when overlapping occurs in the recovery phase:
        //history timestamp: 6
        //
        //comes in 2 updates:
        //    1 update, check history (timestamp 7)
        //    2 update, check history (timestamp 8)
        //    2 update, index (timestamp 8)
        //    1 update, index (timestamp 7)
        //    1 detect change (my timestamp 7 != last timestamp 8)
        //
        //first thread detects error and recovers:
        //    1 betölti a node-ot (timestamp 8)
        //    1 remove timestamp from history if last
        //    1 update, check history (timestamp 8)
        //    1 update, index (timestamp 8)
        //
        //during recovery a third thread comes in:
        //Case (A)
        //    1 loads node (timestamp 8)
        //    3 update, check history (timestamp 9)
        //    1 remove timestamp from history if last -> does not execute, since 8<9
        //
        //Case (B)
        //    1 loads node (timestamp 8)
        //    1 remove timestamp from history if last
        //    3 update, check history (timestamp 9)
        //    1 update, check history (timestamp 8)	-> does not execute, mert 8<9
        //
        //Case (C)
        //    1 loads node (timestamp 8)
        //    1 remove timestamp from history if last
        //    1 update, check history (timestamp 8)
        //    3 update, check history (timestamp 9)
        //    3 update, index (timestamp 9)
        //    1 update, index (timestamp 8)
        //    1 detect change (my timestamp 8 != last timestamp 9)
        //
        //    first thread detects error, and recovers...
        /**/

        // update / add
        //     1 (update) check indexing history 6
        //     2 (add) check indexing history 5 -> returns
        //     1 (update) indexes document 6
        [TestMethod]
        public void IndexingHistory_FixUpdateAddOverlap()
        {
            // update overlaps with add
            var fieldvalue1 = "IndexingHistoryFixUpdateAddOverlapFirst";
            var fieldvalue2 = "IndexingHistoryFixUpdateAddOverlapSecond";

            var history = LuceneManager._history;

            var car = new GenericContent(TestRoot, "Car");
            car.Name = Guid.NewGuid().ToString();
            car.Save();
            var id = car.Id;

            // init 2
            var node2 = Node.LoadNode(id);
            node2["Description"] = fieldvalue2;
            node2.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node2.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node2.VersionId);

            var docInfo2 = IndexDocumentInfo.Create(node2);
            var docData2 = DataBackingStore.CreateIndexDocumentData(node2, docInfo2, null);
            var document2 = IndexDocumentInfo.CreateDocument(docInfo2, docData2);


            // init 1
            var node1 = Node.LoadNode(id);
            node1["Description"] = fieldvalue1;
            node1.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node1.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node1.VersionId);

            var docInfo1 = IndexDocumentInfo.Create(node1);
            var docData1 = DataBackingStore.CreateIndexDocumentData(node1, docInfo1, null);
            var document1 = IndexDocumentInfo.CreateDocument(docInfo1, docData1);


            // 1 check indexing history
            var versionId1 = history.GetVersionId(document1);
            var timestamp1 = history.GetTimestamp(document1);
            var historyOk = LuceneManager._history.CheckForUpdate(versionId1, timestamp1);

            // timestamp in indexing history should be the newest
            var actTimestamp1 = history.Get(versionId1);
            Assert.AreEqual(timestamp1, actTimestamp1, "Timestamp in indexing history did not change.");
            Assert.IsTrue(historyOk, "History indicates indexing should not be executed, but this is not true.");


            // 2 check indexing history
            var versionId2 = history.GetVersionId(document2);
            var timestamp2 = history.GetTimestamp(document2);
            historyOk = LuceneManager._history.CheckForAdd(versionId2, timestamp2);


            // timestamp in indexing history should NOT change
            var actTimestamp2 = history.Get(versionId2);
            Assert.IsTrue(timestamp2 < actTimestamp2, "Timestamp in indexing history changed, although it should not have.");
            Assert.IsFalse(historyOk, "History indicates indexing can be executed, but this is not true.");

            // 2 does not continue, returns

            // 1 index
            var document = document1;
            var updateTerm = UpdateDocumentActivity.GetIdTerm(document);
            LuceneManager.SetFlagsForUpdate(document);
            LuceneManager._writer.UpdateDocument(updateTerm, document);


            var firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;
            var secondfound = ContentQuery.Query("Description:" + fieldvalue2, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 1 adds values to index
            Assert.AreEqual(1, firstfound);
            Assert.AreEqual(0, secondfound);


            // 1 detects no problem
            var detectChange1 = history.CheckHistoryChange(versionId1, timestamp1);
            Assert.IsFalse(detectChange1, "Thread 1 detected indexing overlapping although it should not have.");


            var node = Node.LoadNode(id);
            node.ForceDelete();
        }

        // add / add
        //     1 (add) check indexing history 5
        //     2 (add) check indexing history 5 -> returns
        //     1 (add) indexes document 6
        [TestMethod]
        public void IndexingHistory_FixAddAddOverlap()
        {
            // add overlaps with add
            var fieldvalue1 = "IndexingHistoryFixAddAddOverlap";

            var history = LuceneManager._history;

            var car = new GenericContent(TestRoot, "Car");
            car.Name = Guid.NewGuid().ToString();
            car.Save();
            var id = car.Id;


            // init 1 & 2
            var node1 = Node.LoadNode(id);
            node1["Description"] = fieldvalue1;
            node1.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node1.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node1.VersionId);

            var docInfo1 = IndexDocumentInfo.Create(node1);
            var docData1 = DataBackingStore.CreateIndexDocumentData(node1, docInfo1, null);
            var document1 = IndexDocumentInfo.CreateDocument(docInfo1, docData1);


            // 1 check indexing history
            var versionId1 = history.GetVersionId(document1);
            var timestamp1 = history.GetTimestamp(document1);
            var historyOk = LuceneManager._history.CheckForAdd(versionId1, timestamp1);

            // timestamp in indexing history should be the newest
            var actTimestamp1 = history.Get(versionId1);
            Assert.AreEqual(timestamp1, actTimestamp1, "Timestamp in indexing history did not change.");
            Assert.IsTrue(historyOk, "History indicates indexing should not be executed, but this is not true.");


            // 2 check indexing history
            var versionId2 = history.GetVersionId(document1);
            var timestamp2 = history.GetTimestamp(document1);
            historyOk = LuceneManager._history.CheckForAdd(versionId2, timestamp2);


            // timestamp in indexing history should not change
            var actTimestamp2 = history.Get(versionId2);
            Assert.AreEqual(timestamp2, actTimestamp2, "Timestamp in indexing history changed, although it should not have.");
            Assert.IsFalse(historyOk, "History indicates indexing can be executed, but this is not true.");

            // 2 does not continue, returns

            // 1 index
            var document = document1;
            LuceneManager._writer.AddDocument(document);


            var firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 1 adds values to index
            Assert.AreEqual(1, firstfound);

            // 1 detects no problem
            var detectChange1 = history.CheckHistoryChange(versionId1, timestamp1);
            Assert.IsFalse(detectChange1, "Thread 1 detected indexing overlapping although it should not have.");


            var node = Node.LoadNode(id);
            node.ForceDelete();
        }

        // delete / add
        //     1 (delete) check indexing history
        //     2 (add) check indexing history 5 -> returns
        //     1 (delete) deletes index document
        [TestMethod]
        public void IndexingHistory_FixDeleteAddOverlap()
        {
            // delete overlaps with add
            var fieldvalue1 = "IndexingHistoryFixDeleteAddOverlap";

            var history = LuceneManager._history;

            var car = new GenericContent(TestRoot, "Car");
            car.Name = Guid.NewGuid().ToString();
            car.Save();
            var id = car.Id;


            // init 2
            var node2 = Node.LoadNode(id);
            node2["Description"] = fieldvalue1;
            node2.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node2.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node2.VersionId);

            var docInfo2 = IndexDocumentInfo.Create(node2);
            var docData2 = DataBackingStore.CreateIndexDocumentData(node2, docInfo2, null);
            var document2 = IndexDocumentInfo.CreateDocument(docInfo2, docData2);


            // init 1
            var node1 = Node.LoadNode(id);
            node1.ForceDelete();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node1.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node1.VersionId);



            // 1 check indexing history
            var versionId1 = node2.VersionId;
            var term = new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId1));
            LuceneManager._history.ProcessDelete(new Term[] { term });


            // timestamp in indexing history should change
            var actTimestamp1 = history.Get(versionId1);
            Assert.AreEqual(long.MaxValue, actTimestamp1, "Timestamp in indexing history did not change.");


            // 2 check indexing history
            var versionId2 = history.GetVersionId(document2);
            var timestamp2 = history.GetTimestamp(document2);
            var historyOk = LuceneManager._history.CheckForAdd(versionId2, timestamp2);


            // timestamp in indexing history should not change
            var actTimestamp2 = history.Get(versionId2);
            Assert.AreNotEqual(timestamp2, actTimestamp2, "Timestamp in indexing history changed, although it should not have.");
            Assert.IsFalse(historyOk, "History indicates indexing can be executed, but this is not true.");

            // 2 does not continue, returns

            // 1 deletes index
            LuceneManager.SetFlagsForDelete(term);
            LuceneManager._writer.DeleteDocuments(term);


            var firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 1 deletes from index
            Assert.AreEqual(0, firstfound);
        }

        // delete / update
        //     1 (delete) check indexing history
        //     2 (update) check indexing history 5 -> returns
        //     1 (delete) deletes index document
        [TestMethod]
        public void IndexingHistory_FixDeleteUpdateOverlap()
        {
            // delete overlaps with update
            var fieldvalue1 = "IndexingHistoryFixDeleteAddOverlap";

            var history = LuceneManager._history;

            var car = new GenericContent(TestRoot, "Car");
            car.Name = Guid.NewGuid().ToString();
            car.Save();
            var id = car.Id;


            // init 2
            var node2 = Node.LoadNode(id);
            node2["Description"] = fieldvalue1;
            node2.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node2.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node2.VersionId);

            var docInfo2 = IndexDocumentInfo.Create(node2);
            var docData2 = DataBackingStore.CreateIndexDocumentData(node2, docInfo2, null);
            var document2 = IndexDocumentInfo.CreateDocument(docInfo2, docData2);


            // init 1
            var node1 = Node.LoadNode(id);
            node1.ForceDelete();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node1.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node1.VersionId);



            // 1 check indexing history
            var versionId1 = node2.VersionId;
            var term = new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId1));
            LuceneManager._history.ProcessDelete(new Term[] { term });


            // timestamp in indexing history should change
            var actTimestamp1 = history.Get(versionId1);
            Assert.AreEqual(long.MaxValue, actTimestamp1, "Timestamp in indexing history did not change.");


            // 2 check indexing history
            var versionId2 = history.GetVersionId(document2);
            var timestamp2 = history.GetTimestamp(document2);
            var historyOk = LuceneManager._history.CheckForUpdate(versionId2, timestamp2);


            // timestamp in indexing history should not change
            var actTimestamp2 = history.Get(versionId2);
            Assert.AreNotEqual(timestamp2, actTimestamp2, "Timestamp in indexing history changed, although it should not have.");
            Assert.IsFalse(historyOk, "History indicates indexing can be executed, but this is not true.");

            // 2 does not continue, returns

            // 1 deletes index
            LuceneManager.SetFlagsForDelete(term);
            LuceneManager._writer.DeleteDocuments(term);


            var firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 1 deletes from index
            Assert.AreEqual(0, firstfound);
        }

        // update / update
        //     1 (update) check indexing history 5
        //     2 (update) check indexing history 6
        //     2 (update) indexes document 6
        //     1 (update) indexes document 5     -> order of updates changed
        //     -> optimistic fix recovers correct index
        [TestMethod]
        public void IndexingHistory_FixUpdateUpdateOverlap()
        {
            // update overlaps with update
            var fieldvalue1 = "IndexingHistoryFixUpdateUpdateOverlapFirst";
            var fieldvalue2 = "IndexingHistoryFixUpdateUpdateOverlapSecond";

            var history = LuceneManager._history;

            var car = new GenericContent(TestRoot, "Car");
            car.Name = Guid.NewGuid().ToString();
            car.Save();
            var id = car.Id;


            // init 1
            var node1 = Node.LoadNode(id);
            node1["Description"] = fieldvalue1;
            node1.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node1.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node1.VersionId);

            var docInfo1 = IndexDocumentInfo.Create(node1);
            var docData1 = DataBackingStore.CreateIndexDocumentData(node1, docInfo1, null);
            var document1 = IndexDocumentInfo.CreateDocument(docInfo1, docData1);


            // init 2
            var node2 = Node.LoadNode(id);
            node2["Description"] = fieldvalue2;
            node2.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node2.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node2.VersionId);

            var docInfo2 = IndexDocumentInfo.Create(node2);
            var docData2 = DataBackingStore.CreateIndexDocumentData(node2, docInfo2, null);
            var document2 = IndexDocumentInfo.CreateDocument(docInfo2, docData2);


            // 1 check indexing history
            var versionId1 = history.GetVersionId(document1);
            var timestamp1 = history.GetTimestamp(document1);
            var historyOk = LuceneManager._history.CheckForUpdate(versionId1, timestamp1);

            // timestamp in indexing history should be the newest
            var actTimestamp1 = history.Get(versionId1);
            Assert.AreEqual(timestamp1, actTimestamp1, "Timestamp in indexing history did not change.");
            Assert.IsTrue(historyOk, "History indicates indexing should not be executed, but this is not true.");


            // 2 check indexing history
            var versionId2 = history.GetVersionId(document2);
            var timestamp2 = history.GetTimestamp(document2);
            historyOk = LuceneManager._history.CheckForUpdate(versionId2, timestamp2);


            // timestamp in indexing history should change
            var actTimestamp2 = history.Get(versionId2);
            Assert.AreEqual(timestamp2, actTimestamp2, "Timestamp in indexing history did not change.");
            Assert.IsTrue(historyOk, "History indicates indexing should not be executed, but this is not true.");


            // 2 index
            var document = document2;
            var updateTerm = UpdateDocumentActivity.GetIdTerm(document);
            LuceneManager.SetFlagsForUpdate(document);
            LuceneManager._writer.UpdateDocument(updateTerm, document);


            var firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;
            var secondfound = ContentQuery.Query("Description:" + fieldvalue2, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 2 writes values in index
            Assert.AreEqual(0, firstfound);
            Assert.AreEqual(1, secondfound);


            // 1 index
            document = document1;
            updateTerm = UpdateDocumentActivity.GetIdTerm(document);
            LuceneManager.SetFlagsForUpdate(document);
            LuceneManager._writer.UpdateDocument(updateTerm, document);


            firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;
            secondfound = ContentQuery.Query("Description:" + fieldvalue2, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 1 updates existing field values
            Assert.AreEqual(1, firstfound);
            Assert.AreEqual(0, secondfound);


            // 1 detects problem
            var detectChange1 = history.CheckHistoryChange(versionId1, timestamp1);
            Assert.IsTrue(detectChange1, "Thread 1 did not detect indexing overlapping although it should have.");

            // 2 detects no problem
            var detectChange2 = history.CheckHistoryChange(versionId2, timestamp2);
            Assert.IsFalse(detectChange2, "Thread 2 detected indexing overlapping although it should not have.");


            // 1 fixes index
            LuceneManager.RefreshDocument(versionId1);

            firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;
            secondfound = ContentQuery.Query("Description:" + fieldvalue2, new QuerySettings { EnableAutofilters = false }).Count;

            Assert.AreEqual(0, firstfound);
            Assert.AreEqual(1, secondfound);

            var node = Node.LoadNode(id);
            node.ForceDelete();
        }

        // add / update
        //     1 (add) check indexing history 5
        //     2 (update) check indexing history 6
        //     2 (update) indexes document 6
        //     1 (add) indexes document 5     -> order of updates changed
        //     -> optimistic fix recovers correct index
        [TestMethod]
        public void IndexingHistory_FixAddUpdateOverlap()
        {
            // add overlaps with update
            var fieldvalue1 = "IndexingHistoryFixAddUpdateOverlapFirst";
            var fieldvalue2 = "IndexingHistoryFixAddUpdateOverlapSecond";

            var history = LuceneManager._history;

            var car = new GenericContent(TestRoot, "Car");
            car.Name = Guid.NewGuid().ToString();
            car.Save();
            var id = car.Id;


            // init 1
            var node1 = Node.LoadNode(id);
            node1["Description"] = fieldvalue1;
            node1.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node1.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node1.VersionId);

            var docInfo1 = IndexDocumentInfo.Create(node1);
            var docData1 = DataBackingStore.CreateIndexDocumentData(node1, docInfo1, null);
            var document1 = IndexDocumentInfo.CreateDocument(docInfo1, docData1);


            // init 2
            var node2 = Node.LoadNode(id);
            node2["Description"] = fieldvalue2;
            node2.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node2.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node2.VersionId);

            var docInfo2 = IndexDocumentInfo.Create(node2);
            var docData2 = DataBackingStore.CreateIndexDocumentData(node2, docInfo2, null);
            var document2 = IndexDocumentInfo.CreateDocument(docInfo2, docData2);


            // 1 check indexing history
            var versionId1 = history.GetVersionId(document1);
            var timestamp1 = history.GetTimestamp(document1);
            var historyOk = LuceneManager._history.CheckForAdd(versionId1, timestamp1);

            // timestamp in indexing history should be the newest
            var actTimestamp1 = history.Get(versionId1);
            Assert.AreEqual(timestamp1, actTimestamp1, "Timestamp in indexing history did not change.");
            Assert.IsTrue(historyOk, "History indicates indexing should not be executed, but this is not true.");


            // 2 check indexing history
            var versionId2 = history.GetVersionId(document2);
            var timestamp2 = history.GetTimestamp(document2);
            historyOk = LuceneManager._history.CheckForUpdate(versionId2, timestamp2);


            // timestamp in indexing history should change
            var actTimestamp2 = history.Get(versionId2);
            Assert.AreEqual(timestamp2, actTimestamp2, "Timestamp in indexing history did not change.");
            Assert.IsTrue(historyOk, "History indicates indexing should not be executed, but this is not true.");


            // 2 index
            var document = document2;
            var updateTerm = UpdateDocumentActivity.GetIdTerm(document);
            LuceneManager.SetFlagsForUpdate(document);
            LuceneManager._writer.UpdateDocument(updateTerm, document);


            var firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;
            var secondfound = ContentQuery.Query("Description:" + fieldvalue2, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 2 writes values in index
            Assert.AreEqual(0, firstfound);
            Assert.AreEqual(1, secondfound);


            // 1 index
            document = document1;
            LuceneManager._writer.AddDocument(document);


            firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;
            secondfound = ContentQuery.Query("Description:" + fieldvalue2, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 1 adds values to index, duplication occurs
            Assert.AreEqual(1, firstfound);
            Assert.AreEqual(1, secondfound);


            // 1 detects problem
            var detectChange1 = history.CheckHistoryChange(versionId1, timestamp1);
            Assert.IsTrue(detectChange1, "Thread 1 did not detect indexing overlapping although it should have.");

            // 2 detects no problem
            var detectChange2 = history.CheckHistoryChange(versionId2, timestamp2);
            Assert.IsFalse(detectChange2, "Thread 2 detected indexing overlapping although it should not have.");


            // 1 fixes index
            LuceneManager.RefreshDocument(versionId1);

            firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;
            secondfound = ContentQuery.Query("Description:" + fieldvalue2, new QuerySettings { EnableAutofilters = false }).Count;

            Assert.AreEqual(0, firstfound);
            Assert.AreEqual(1, secondfound);

            var node = Node.LoadNode(id);
            node.ForceDelete();
        }

        // add / delete
        //     1 (add) check indexing history 5
        //     2 (delete) check indexing history
        //     2 (delete) deletes index document
        //     1 (add) indexes document 5
        //     -> optimistic fix recovers correct index
        [TestMethod]
        public void IndexingHistory_FixAddDeleteOverlap()
        {
            // add overlaps with delete
            var fieldvalue1 = "IndexingHistoryFixAddDeleteOverlap";

            var history = LuceneManager._history;

            var car = new GenericContent(TestRoot, "Car");
            car.Name = Guid.NewGuid().ToString();
            car.Save();
            var id = car.Id;


            // init 1
            var node1 = Node.LoadNode(id);
            node1["Description"] = fieldvalue1;
            node1.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node1.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node1.VersionId);

            var docInfo1 = IndexDocumentInfo.Create(node1);
            var docData1 = DataBackingStore.CreateIndexDocumentData(node1, docInfo1, null);
            var document1 = IndexDocumentInfo.CreateDocument(docInfo1, docData1);


            // init 2
            var node2 = Node.LoadNode(id);
            node2.ForceDelete();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node2.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node2.VersionId);


            // 1 check indexing history
            var versionId1 = history.GetVersionId(document1);
            var timestamp1 = history.GetTimestamp(document1);
            var historyOk = LuceneManager._history.CheckForAdd(versionId1, timestamp1);

            // timestamp in indexing history should be the newest
            var actTimestamp1 = history.Get(versionId1);
            Assert.AreEqual(timestamp1, actTimestamp1, "Timestamp in indexing history did not change.");
            Assert.IsTrue(historyOk, "History indicates indexing should not be executed, but this is not true.");


            // 2 check indexing history
            var versionId2 = node2.VersionId;
            var term = new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId2));
            LuceneManager._history.ProcessDelete(new Term[] { term });


            // timestamp in indexing history should change
            var actTimestamp2 = history.Get(versionId2);
            Assert.AreEqual(long.MaxValue, actTimestamp2, "Timestamp in indexing history did not change.");


            // 2 index
            LuceneManager.SetFlagsForDelete(term);
            LuceneManager._writer.DeleteDocuments(term);


            var firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 2 deletes from index
            Assert.AreEqual(0, firstfound);


            // 1 index
            var document = document1;
            LuceneManager._writer.AddDocument(document);


            firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 1 adds values to index, ghost document is created
            Assert.AreEqual(1, firstfound);


            // 1 detects problem
            var detectChange1 = history.CheckHistoryChange(versionId1, timestamp1);
            Assert.IsTrue(detectChange1, "Thread 1 did not detect indexing overlapping although it should have.");


            // 1 fixes index
            LuceneManager.RefreshDocument(versionId1);

            firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;

            Assert.AreEqual(0, firstfound);
        }

        // update / delete
        //     1 (update) check indexing history 5
        //     2 (delete) check indexing history
        //     2 (delete) deletes index document
        //     1 (update) indexes document 5
        //     -> optimistic fix recovers correct index
        [TestMethod]
        public void IndexingHistory_FixUpdateDeleteOverlap()
        {
            // update overlaps with delete
            var fieldvalue1 = "IndexingHistoryFixUpdateDeleteOverlap";

            var history = LuceneManager._history;

            var car = new GenericContent(TestRoot, "Car");
            car.Name = Guid.NewGuid().ToString();
            car.Save();
            var id = car.Id;


            // init 1
            var node1 = Node.LoadNode(id);
            node1["Description"] = fieldvalue1;
            node1.Save();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node1.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node1.VersionId);

            var docInfo1 = IndexDocumentInfo.Create(node1);
            var docData1 = DataBackingStore.CreateIndexDocumentData(node1, docInfo1, null);
            var document1 = IndexDocumentInfo.CreateDocument(docInfo1, docData1);


            // init 2
            var node2 = Node.LoadNode(id);
            node2.ForceDelete();
            // delete changes from index
            DataRowTimestampTest.DeleteVersionFromIndex(node2.VersionId);
            DataRowTimestampTest.DeleteVersionIdFromIndexingHistory(node2.VersionId);


            // 1 check indexing history
            var versionId1 = history.GetVersionId(document1);
            var timestamp1 = history.GetTimestamp(document1);
            var historyOk = LuceneManager._history.CheckForUpdate(versionId1, timestamp1);

            // timestamp in indexing history should be the newest
            var actTimestamp1 = history.Get(versionId1);
            Assert.AreEqual(timestamp1, actTimestamp1, "Timestamp in indexing history did not change.");
            Assert.IsTrue(historyOk, "History indicates indexing should not be executed, but this is not true.");


            // 2 check indexing history
            var versionId2 = node2.VersionId;
            var term = new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId2));
            LuceneManager._history.ProcessDelete(new Term[] { term });


            // timestamp in indexing history should change
            var actTimestamp2 = history.Get(versionId2);
            Assert.AreEqual(long.MaxValue, actTimestamp2, "Timestamp in indexing history did not change.");


            // 2 index
            LuceneManager.SetFlagsForDelete(term);
            LuceneManager._writer.DeleteDocuments(term);


            var firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 2 deletes from index
            Assert.AreEqual(0, firstfound);


            // 1 index
            var document = document1;
            var updateTerm = UpdateDocumentActivity.GetIdTerm(document);
            LuceneManager.SetFlagsForUpdate(document);
            LuceneManager._writer.UpdateDocument(updateTerm, document);


            firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;

            // check indexing occured correctly
            // thread 1 updates (adds) values to index, ghost document is created
            Assert.AreEqual(1, firstfound);


            // 1 detects problem
            var detectChange1 = history.CheckHistoryChange(versionId1, timestamp1);
            Assert.IsTrue(detectChange1, "Thread 1 did not detect indexing overlapping although it should have.");


            // 1 fixes index
            LuceneManager.RefreshDocument(versionId1);

            firstfound = ContentQuery.Query("Description:" + fieldvalue1, new QuerySettings { EnableAutofilters = false }).Count;

            Assert.AreEqual(0, firstfound);
        }

        //==================================================================

        [TestMethod]
        public void IndexingHistory_Add()
        {
            var history = new IndexingHistory();
            var historyAcc = new IndexingHistoryAccessor(history);

            for (int size = 5; size < 20; size += 5)
            {
                historyAcc.Initialize(size);

                Assert.IsTrue(history.Count == 0, string.Format("history.Count is {0}, expected: 0 (size: {1})", history.Count, size));

                for (int i = 1; i < size + 3; i++)
                    history.Add(i, 100 + i);

                for (int i = 4; i < size + 3; i++)
                {
                    var value = history.Get(i);
                    Assert.IsTrue(value == i + 100, string.Format("storage[{0}] is {1}, expected: {2} (size: {3})", i, value, i + 100, size));
                }

                Assert.IsTrue(history.Count == size, string.Format("history.Count is {0}, expected: {1} (size: {2})", history.Count, size, size));
            }
        }
        [TestMethod]
        public void IndexingHistory_Update()
        {
            var history = new IndexingHistory();
            var historyAcc = new IndexingHistoryAccessor(history);

            for (int size = 5; size < 20; size += 5)
            {
                historyAcc.Initialize(size);

                Assert.IsTrue(history.Count == 0, string.Format("history.Count is {0}, expected: 0 (size: {1})", history.Count, size));

                history.Add(42, 142);
                history.Add(43, 143);

                for (int i = 1111; i < 1122; i+=2)
                {
                    history.Update(42, i);
                    history.Update(43, i + 1);
                    var value42 = history.Get(42);
                    var value43 = history.Get(43);
                    Assert.IsTrue(value42 == i, string.Format("storage[{0}] is {1}, expected: {2} (size: {3})", 42, value42, i, size));
                    Assert.IsTrue(value43 == i + 1, string.Format("storage[{0}] is {1}, expected: {2} (size: {3})", 43, value43, i + 1, size));
                }

                Assert.IsTrue(history.Count == 2, string.Format("history.Count is {0}, expected: 2 (size: {1})", history.Count, size));
            }
        }

        [TestMethod]
        public void IndexingHistory_CheckForAdd()
        {
            var history = new IndexingHistory();
            var historyAcc = new IndexingHistoryAccessor(history);
            var size = 5;
            historyAcc.Initialize(size);
            Assert.IsTrue(history.Count == 0, string.Format("history.Count is {0}, expected: 0 (size: {1})", history.Count, size));

            Assert.IsTrue(history.CheckForAdd(42, 1111), "CheckForAdd(42, 1111) first call returned with false");
            Assert.IsFalse(history.CheckForAdd(42, 1111), "CheckForAdd(42, 1111) second call returned with true");
            Assert.IsFalse(history.CheckForAdd(42, 1110), "CheckForAdd(42, 1110) returned with true");
            Assert.IsFalse(history.CheckForAdd(42, 1112), "CheckForAdd(42, 1112) returned with true");

            Assert.IsTrue(history.CheckForAdd(43, 1111), "CheckForAdd(43, 1111) first call returned with false");
            Assert.IsFalse(history.CheckForAdd(43, 1111), "CheckForAdd(43, 1111) second call returned with true");
            Assert.IsFalse(history.CheckForAdd(43, 1110), "CheckForAdd(43, 1110) returned with true");
            Assert.IsFalse(history.CheckForAdd(43, 1112), "CheckForAdd(43, 1112) returned with true");

            Assert.IsTrue(history.Count == 2, string.Format("history.Count is {0}, expected: 2 (size: {1})", history.Count, size));

            Assert.IsTrue(history.CheckForAdd(44, 1111), "CheckForAdd(44, 1111) returned with false");
            Assert.IsTrue(history.CheckForUpdate(44, 1112), "CheckForUpdate(44, 1111) returned with false");
            Assert.IsFalse(history.CheckForAdd(44, 1113), "CheckForAdd(44, 1113) rreturned with true");
        }
        [TestMethod]
        public void IndexingHistory_CheckForUpdate()
        {
            var history = new IndexingHistory();
            var historyAcc = new IndexingHistoryAccessor(history);
            var size = 5;
            historyAcc.Initialize(size);
            Assert.IsTrue(history.Count == 0, string.Format("history.Count is {0}, expected: 0 (size: {1})", history.Count, size));

            Assert.IsTrue(history.CheckForUpdate(42, 1111), "CheckForAdd(42, 1111) first call returned with false");
            Assert.IsFalse(history.CheckForUpdate(42, 1111), "CheckForUpdate(42, 1111) second call returned with true");
            Assert.IsFalse(history.CheckForUpdate(42, 1110), "CheckForUpdate(42, 1110) returned with true");
            Assert.IsTrue(history.CheckForUpdate(42, 1112), "CheckForUpdate(42, 1112) returned with false");
            Assert.IsTrue(history.CheckForUpdate(42, 1113), "CheckForUpdate(42, 1112) returned with false");

            Assert.IsTrue(history.CheckForUpdate(43, 1111), "CheckForAdd(43, 1111) first call returned with false");
            Assert.IsFalse(history.CheckForUpdate(43, 1111), "CheckForUpdate(43, 1111) second call returned with true");
            Assert.IsFalse(history.CheckForUpdate(43, 1110), "CheckForUpdate(43, 1110) returned with true");
            Assert.IsTrue(history.CheckForUpdate(43, 1112), "CheckForUpdate(43, 1112) returned with false");
            Assert.IsTrue(history.CheckForUpdate(43, 1113), "CheckForUpdate(43, 1112) returned with false");

            Assert.IsTrue(history.Count == 2, string.Format("history.Count is {0}, expected: 2 (size: {1})", history.Count, size));
        }
        [TestMethod]
        public void IndexingHistory_ProcessDelete()
        {
            var history = new IndexingHistory();
            var historyAcc = new IndexingHistoryAccessor(history);
            var size = 5;
            historyAcc.Initialize(size);
            Assert.IsTrue(history.Count == 0, string.Format("history.Count is {0}, expected: 0 (size: {1})", history.Count, size));

            history.ProcessDelete(42);
            Assert.IsFalse(history.CheckForAdd(42, 1111), "CheckForAdd(42, 1111) returned with true");
            Assert.IsFalse(history.CheckForUpdate(42, 1112), "CheckForUpdate(42, 1111) returned with true");

            Assert.IsTrue(history.CheckForUpdate(43, 1111), "CheckForAdd(43, 1111) first call returned with false");
            history.ProcessDelete(43);
            Assert.IsFalse(history.CheckForUpdate(43, 1111), "CheckForUpdate(43, 1111) second call returned with true");

            Assert.IsTrue(history.Count == 2, string.Format("history.Count is {0}, expected: 2 (size: {1})", history.Count, size));
        }
    }
}
