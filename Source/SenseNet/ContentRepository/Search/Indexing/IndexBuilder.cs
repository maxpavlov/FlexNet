//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Lucene.Net.Analysis;
//using Lucene.Net.Documents;
//using Lucene.Net.Index;
//using Lucene.Net.Store;
//using SenseNet.Diagnostics;
//using SenseNet.ContentRepository.Storage;
//using SenseNet.ContentRepository.Storage.Schema;
//using SenseNet.ContentRepository.Storage.Search;
//using Lucene.Net.Search;
//using Lucene.Net.Util;
//using SenseNet.Search.Indexing.Activities;

//namespace SenseNet.Search.Indexing
//{
//    public class DocumentPopulator : IIndexPopulator
//    {
//        internal static bool NewAlgorithm = true;

//        private static IndexingTask CreateTask(string comment)
//        {
//            return new IndexingTask() { Comment = comment };
//        }
//        private static void AddActivity(IndexingTask task, IndexingActivityType type, int nodeId, int versionId)
//        {
//            AddActivity(task, type, nodeId, versionId, null, null, null);
//        }
//        private static void AddActivity(IndexingTask task, IndexingActivityType type, int nodeId, int versionId, bool? isPublic, bool? isLastPublic, bool? isLastDraft)
//        {
//            task.IndexingActivities.Add(new IndexingActivity()
//            {
//                ActivityType = type,
//                NodeId = nodeId,
//                VersionId = versionId,
//                IsPublicValue = isPublic,
//                IsLastPublicValue = isLastPublic,
//                IsLastDraftValue = isLastDraft
//            });
//        }

//        /*================================================================================================================================*/

//        private class DocumentPopulatorData
//        {
//            internal Node Node { get; set; }
//            internal NodeHead NodeHead { get; set; }
//            internal NodeSaveSettings Settings { get; set; }
//            internal string OriginalPath { get; set; }
//            internal string NewPath { get; set; }
//            internal bool IsNewNode { get; set; }
//            //internal bool IsNewVersion { get; set; }
//            //internal bool IsPublic { get; set; }
//            //internal VersionNumber LastMajorVersion { get; set; }
//            //internal VersionNumber LastMinorVersion { get; set; }
//        }
//        private class DeleteVersionPopulatorData
//        {
//            internal Node OldVersion { get; set; }
//            internal Node LastDraftAfterDelete { get; set; }
//        }

//        /*================================================================================================================================*/

//        // caller: CommitPopulateNode
//        private static void CreateNode(Node node)
//        {
//            using (var traceOperation = Logger.TraceOperation("IndexPopulator AddNode"))
//            {
//                var task = CreateTask("CreateNode");
//                AddActivity(task, IndexingActivityType.AddDocument, node.Id, node.VersionId);
//                IndexingTaskManager.RegisterTask(task);
//                IndexingTaskManager.ExecuteTask(task, true, true);
//                traceOperation.IsSuccessful = true;
//            }

//            #region without WriterActivity
//            //using (var traceOperation = Logger.TraceOperation("IndexPopulator AddNode"))
//            //{
//            //    var doc = DocumentBuilder.GetDocument(node);
//            //    ExecuteActivity(new AddDocumentWriterActivity { Doc = doc });
//            //    //-- brand new node
//            //    //zpace               
//            //    //var writer = IndexManager.GetIndexWriter(false);
//            //    var writer = LuceneManager.LuceneManager.GetIndexWriter(CommitHint.AddNew);
//            //    try
//            //    {
//            //        var doc = DocumentBuilder.GetDocument(node);
//            //        writer.AddDocument(doc);
//            //    }
//            //    finally
//            //    {

//            //        LuceneManager.LuceneManager.CommitIndexWriter(writer);
//            //        //writer.Close();
//            //    }
//            //    traceOperation.IsSuccessful = true;
//            //}
//            #endregion
//        }

//        // caller: CommitPopulateNode
//        private static void AddNewVersion(Node newVersion, bool isPublic, VersionNumber lastPublicVersion, VersionNumber lastDraftVersion)
//        {
//            using (var traceOperation = Logger.TraceOperation("IndexPopulator AddNewVersion"))
//            {
//                var newIsMajor = newVersion.Version.IsMajor;
//                var newIsPublic = isPublic;
//                var nodeId = newVersion.Id;
//                var isLastPublic = isPublic;
//                var isLastDraft = true;

//                if (!newIsMajor && isPublic)
//                    throw new InvalidOperationException("Minor version cannot be public");

//                var task = CreateTask("AddNewVersion");

//                if (newIsPublic)
//                {
//                    if ((lastPublicVersion != null) && (lastPublicVersion != lastDraftVersion))
//                    {
//                        var lastPublicNode = Node.LoadNode(nodeId, lastPublicVersion);
//                        if (lastPublicNode != null)
//                        {
//                            AddActivity(task, IndexingActivityType.UpdateDocument, lastPublicNode.Id, lastPublicNode.VersionId, true, true, false);
//                        }
//                    }
//                    var lastDraftNode = Node.LoadNode(nodeId, lastDraftVersion);
//                    if (lastDraftNode != null)
//                    {
//                        var lastDraftIsPublic = lastDraftNode.Version.Status == VersionStatus.Approved;
//                        AddActivity(task, IndexingActivityType.UpdateDocument, lastDraftNode.Id, lastDraftNode.VersionId, lastDraftIsPublic, false, false);
//                    }
//                }
//                else
//                {
//                    var lastDraftIsPublic = (lastPublicVersion != null) && (lastPublicVersion == lastDraftVersion);
//                    var lastDraftNode = Node.LoadNode(nodeId, lastDraftVersion);
//                    if (lastDraftNode != null)
//                    {
//                        var lastDraftNodeIsPublic = lastDraftNode.Version.Status == VersionStatus.Approved;
//                        AddActivity(task, IndexingActivityType.UpdateDocument, lastDraftNode.Id, lastDraftNode.VersionId, lastDraftNodeIsPublic, lastDraftIsPublic, false);
//                    }
//                }
//                AddActivity(task, IndexingActivityType.AddDocument, newVersion.Id, newVersion.VersionId, isPublic, isLastPublic, isLastDraft);

//                IndexingTaskManager.RegisterTask(task);
//                IndexingTaskManager.ExecuteTask(task, true, true);

//                traceOperation.IsSuccessful = true;
//            }

//            #region without WriterActivity
//            //using (var traceOperation = Logger.TraceOperation("IndexPopulator AddNewVersion"))
//            //{
//            //    var newIsMajor = newVersion.Version.IsMajor;
//            //    var newIsPublic = isPublic;
//            //    var nodeId = newVersion.Id;
//            //    var isLastPublic = isPublic;
//            //    var isLastDraft = true;

//            //    if (!newIsMajor && isPublic)
//            //        throw new InvalidOperationException("Minor version cannot be public");

//            //    //zpace
//            //    //var writer = IndexManager.GetIndexWriter(false);
//            //    var writer = LuceneManager.LuceneManager.GetIndexWriter(CommitHint.Update);
//            //    try
//            //    {
//            //        if (newIsPublic)
//            //        {
//            //            if ((lastPublicVersion != null) && (lastPublicVersion != lastDraftVersion))
//            //            {
//            //                var lastPublicNode = Node.LoadNode(nodeId, lastPublicVersion);
//            //                if (lastPublicNode != null)
//            //                {
//            //                    var lastPublicDoc = DocumentBuilder.GetDocument(lastPublicNode, true, false, false);
//            //                    //zpace
//            //                    //UpdateNode(lastPublicNode.VersionId, lastPublicDoc, writer);
//            //                    writer.UpdateDocument(writer.GetIdTerm(lastPublicDoc), lastPublicDoc);
//            //                }
//            //            }
//            //            var lastDraftNode = Node.LoadNode(nodeId, lastDraftVersion);
//            //            if (lastDraftNode != null)
//            //            {
//            //                var lastDraftIsPublic = lastDraftNode.Version.Status == VersionStatus.Approved;
//            //                var lastDraftDoc = DocumentBuilder.GetDocument(lastDraftNode, lastDraftIsPublic, false, false);

//            //                //zpace
//            //                writer.UpdateDocument(writer.GetIdTerm(lastDraftDoc), lastDraftDoc);
//            //                //AddNode(lastDraftDoc, writer);
//            //            }
//            //        }
//            //        else
//            //        {
//            //            //var lastIsPublic = (lastPublicVersion != null) && (lastPublicVersion != lastDraftVersion);
//            //            var lastDraftIsPublic = (lastPublicVersion != null) && (lastPublicVersion == lastDraftVersion);
//            //            var lastDraftNode = Node.LoadNode(nodeId, lastDraftVersion);
//            //            if (lastDraftNode != null)
//            //            {
//            //                var lastDraftNodeIsPublic = lastDraftNode.Version.Status == VersionStatus.Approved;
//            //                var lastDraftDoc = DocumentBuilder.GetDocument(lastDraftNode, lastDraftNodeIsPublic, lastDraftIsPublic, false);
//            //                //zpace
//            //                //UpdateNode(lastDraftNode.VersionId, lastDraftDoc, writer);
//            //                writer.UpdateDocument(writer.GetIdTerm(lastDraftDoc), lastDraftDoc);
//            //                //writer.AddDocument(lastDraftDoc);
//            //            }
//            //        }
//            //        var newDoc = DocumentBuilder.GetDocument(newVersion, isPublic, isLastPublic, isLastDraft);

//            //        writer.AddDocument(newDoc);
//            //    }
//            //    finally
//            //    {
//            //        //zpace
//            //        //writer.Close();
//            //        LuceneManager.LuceneManager.CommitIndexWriter(writer);
//            //    }
//            //    traceOperation.IsSuccessful = true;
//            //}
//            #endregion
//        }
//        private static void AddNewVersionNEW(Node newVersion)
//        {
//            using (var traceOperation = Logger.TraceOperation("IndexPopulator AddNewVersion"))
//            {
//                var task = CreateTask("AddNewVersion");
//                AddActivity(task, IndexingActivityType.AddDocument, newVersion.Id, newVersion.VersionId, null, null, null);

//                IndexingTaskManager.RegisterTask(task);
//                IndexingTaskManager.ExecuteTask(task, true, true);

//                traceOperation.IsSuccessful = true;
//            }
//        }

//        // caller: CommitPopulateNode
//        private static void UpdateVersion(DocumentPopulatorData state)
//        {
//            // curVer = 5.0.P, curId = 105, expVer = 3.0.A, expId = 102
//            // deletableVersions: 105, 104, 103
//            // verId:  100,   101,   102,   103,   104,   105
//            // ver:   1.0.A, 2.0.A, 3.0.R, 4.0.R, 5.0.R, 6.0.P

//            // before: lastPub: 101, lastDraft: 105
//            // after:  lastPub: 102, lastDraft: 102

//            //  Before:
//            //  VerId    Ver isLastPub isLastDraft isPub  action           Ver isLastPub isLastDraft isPub
//            //   100   1.0.A    false     false    true   -              1.0.A    false     false    true  
//            //   101   2.0.A    true      false    true   backupdate     2.0.A    false     false    true 
//            //   102   3.0.R    false     false    false  update         3.0.A    true      true     true 
//            //   103   4.0.R    false     false    false  delete           -
//            //   104   5.0.R    false     false    false  delete           -
//            //   105   6.0.P    false     true     false  delete           -

//            var nodeHeadBefore = state.NodeHead;
//            var nodeBefore = state.Node;
//            var nodeHeadAfter = NodeHead.Get(nodeBefore.Id);
//            var nodeAfter = Node.LoadNode(nodeBefore.Id);
//            var historyAfter = nodeHeadAfter.Versions;

//            var delete = state.Settings.DeletableVersionIds;
//            var addVersion = false;

//            var nodeId = state.Node.Id;
//            var lastMajorBefore = historyAfter.Where(v => v.VersionId == nodeHeadBefore.LastMajorVersionId).FirstOrDefault();
//            var lastMajorAfter = historyAfter.Where(v => v.VersionId == nodeHeadAfter.LastMajorVersionId).FirstOrDefault();
//            var lastMinorBefore = historyAfter.Where(v => v.VersionId == nodeHeadBefore.LastMinorVersionId).FirstOrDefault();
//            var lastMinorAfter = historyAfter.Where(v => v.VersionId == nodeHeadAfter.LastMinorVersionId).FirstOrDefault();
//            var lastMajorIdBefore = nodeHeadBefore.LastMajorVersionId;
//            var lastMajorIdAfter = nodeHeadAfter.LastMajorVersionId;
//            var lastMinorIdBefore = nodeHeadBefore.LastMinorVersionId;
//            var lastMinorIdAfter = nodeHeadAfter.LastMinorVersionId;
//            var lastMajorIdBeforeDeleted = delete.Contains(nodeHeadBefore.LastMajorVersionId);
//            var lastMinorIdBeforeDeleted = delete.Contains(nodeHeadBefore.LastMinorVersionId);

//            var task = CreateTask("UpdateVersion");

//            //-- set/reset last public/draft flags
//            if (lastMajorIdBefore != 0)
//                if (lastMajorIdBefore != lastMajorIdAfter && !lastMajorIdBeforeDeleted)
//                    resetLastPublicOn(nodeId, lastMajorBefore, task);
//            if (lastMinorIdBefore != 0)
//                if (lastMinorIdBefore != lastMinorIdAfter && !lastMinorIdBeforeDeleted)
//                    resetLastDraftOn(nodeId, lastMinorBefore, task);
//            if (lastMajorIdAfter != 0)
//                if (lastMajorIdBefore != lastMajorIdAfter && lastMajorIdAfter != nodeAfter.VersionId)
//                    setLastPublicOn(nodeId, lastMajorAfter, task);
//            if (lastMinorIdAfter != 0)
//                if (lastMinorIdBefore != lastMinorIdAfter && lastMinorIdAfter != nodeAfter.VersionId)
//                    setLastDraftOn(nodeId, lastMinorAfter, task);

//            //-- DeleteVersions;
//            foreach (var versionId in delete)
//            {
//                task.IndexingActivities.Add(new IndexingActivity { ActivityType = IndexingActivityType.RemoveDocument, VersionId = versionId, });
//            }

//            //-- Write
//            var type = addVersion ? IndexingActivityType.AddDocument : IndexingActivityType.UpdateDocument;
//            if (state.Settings.IsPublic())
//                AddActivity(task, type, nodeAfter.Id, nodeAfter.VersionId, true, true, true);
//            else
//                AddActivity(task, type, nodeAfter.Id, nodeAfter.VersionId);


//            IndexingTaskManager.RegisterTask(task);
//            IndexingTaskManager.ExecuteTask(task, true, true);

//            #region without WriterActivity

//            //var nodeHeadBefore = state.NodeHead;
//            //var nodeBefore = state.Node;
//            //var nodeHeadAfter = NodeHead.Get(nodeBefore.Id);
//            //var nodeAfter = Node.LoadNode(nodeBefore.Id);
//            //var historyAfter = nodeHeadAfter.Versions;

//            //var delete = state.Settings.DeletableVersionIds;
//            //var addVersion = false;
//            ////if (state.Settings.ExpectedVersionId != 0 && !delete.Contains(nodeAfter.VersionId))
//            ////{
//            ////    delete.Add(state.Settings.ExpectedVersionId);
//            ////    addVersion = true;
//            ////}

//            //var nodeId = state.Node.Id;
//            //var lastMajorBefore = historyAfter.Where(v => v.VersionId == nodeHeadBefore.LastMajorVersionId).FirstOrDefault();
//            //var lastMajorAfter = historyAfter.Where(v => v.VersionId == nodeHeadAfter.LastMajorVersionId).FirstOrDefault();
//            //var lastMinorBefore = historyAfter.Where(v => v.VersionId == nodeHeadBefore.LastMinorVersionId).FirstOrDefault();
//            //var lastMinorAfter = historyAfter.Where(v => v.VersionId == nodeHeadAfter.LastMinorVersionId).FirstOrDefault();
//            //var lastMajorIdBefore = nodeHeadBefore.LastMajorVersionId;
//            //var lastMajorIdAfter = nodeHeadAfter.LastMajorVersionId;
//            //var lastMinorIdBefore = nodeHeadBefore.LastMinorVersionId;
//            //var lastMinorIdAfter = nodeHeadAfter.LastMinorVersionId;
//            //var lastMajorIdBeforeDeleted = delete.Contains(nodeHeadBefore.LastMajorVersionId);
//            //var lastMinorIdBeforeDeleted = delete.Contains(nodeHeadBefore.LastMinorVersionId);


//            ////zpace
//            ////var writer = IndexManager.GetIndexWriter(false);
//            //var writer = LuceneManager.LuceneManager.GetIndexWriter(CommitHint.Update);
//            //try
//            //{
//            //    //-- set/reset last public/draft flags
//            //    if (lastMajorIdBefore != 0)
//            //        if (lastMajorIdBefore != lastMajorIdAfter && !lastMajorIdBeforeDeleted)
//            //            resetLastPublicOn(nodeId, lastMajorBefore, writer);
//            //    if (lastMinorIdBefore != 0)
//            //        if (lastMinorIdBefore != lastMinorIdAfter && !lastMinorIdBeforeDeleted)
//            //            resetLastDraftOn(nodeId, lastMinorBefore, writer);
//            //    if (lastMajorIdAfter != 0)
//            //        if (lastMajorIdBefore != lastMajorIdAfter && lastMajorIdAfter != nodeAfter.VersionId)
//            //            setLastPublicOn(nodeId, lastMajorAfter, writer);
//            //    if (lastMinorIdAfter != 0)
//            //        if (lastMinorIdBefore != lastMinorIdAfter && lastMinorIdAfter != nodeAfter.VersionId)
//            //            setLastDraftOn(nodeId, lastMinorAfter, writer);

//            //    //-- DeleteVersions;
//            //    // zpace
//            //    foreach (var versionId in delete)
//            //    {
//            //        var term = new Term(LuceneManager.LuceneManager.KeyFieldName, NumericUtils.IntToPrefixCoded(versionId));
//            //        writer.DeleteDocuments(term);
//            //    }

//            //    //-- Write
//            //    Document doc;
//            //    if (state.Settings.IsPublic())
//            //        doc = DocumentBuilder.GetDocument(nodeAfter, true, true, true);
//            //    else
//            //        doc = DocumentBuilder.GetDocument(nodeAfter);
//            //    if (addVersion)
//            //        writer.AddDocument(doc);
//            //    //AddNode(doc, writer);
//            //    else
//            //        //delete it will be by zpace
//            //        //UpdateNode(nodeAfter.VersionId, doc, writer);
//            //        writer.UpdateDocument(writer.GetIdTerm(doc), doc);
//            //    //AddNode(doc, writer);
//            //}
//            //finally
//            //{
//            //    //zpace
//            //    //writer.Close();
//            //    LuceneManager.LuceneManager.CommitIndexWriter(writer);
//            //}
//            #endregion
//        }
//        private static void UpdateVersionNEW(DocumentPopulatorData state)
//        {
//            var nodeBefore = state.Node;

//            var task = CreateTask("UpdateVersion");

//            //-- DeleteVersions;
//            foreach (var versionId in state.Settings.DeletableVersionIds)
//                task.IndexingActivities.Add(new IndexingActivity { ActivityType = IndexingActivityType.RemoveDocument, VersionId = versionId, });

//            //-- Write
//            AddActivity(task, IndexingActivityType.UpdateDocument, nodeBefore.Id, nodeBefore.VersionId);

//            IndexingTaskManager.RegisterTask(task);
//            IndexingTaskManager.ExecuteTask(task, true, true);
//        }

//        /*================================================================================================================================*/

//        private static IEnumerable<Node> GetVersions(Node node)
//        {
//            using (var traceOperation = Logger.TraceOperation("IndexPopulator GetVersions"))
//            {
//                var versionNumbers = Node.GetVersionNumbers(node.Id);
//                var versions = from versionNumber in versionNumbers select Node.LoadNode(node.Id, versionNumber);
//                var versionsArray = versions.ToArray();

//                traceOperation.IsSuccessful = true;
//                return versionsArray;
//            }
//        }

//        private static void resetLastPublicOn(int nodeId, NodeHead.NodeVersion nodeVersion, IndexingTask task)
//        {
//            setLastFlags(null, false, null, nodeId, nodeVersion, task);
//        }
//        private static void setLastPublicOn(int nodeId, NodeHead.NodeVersion nodeVersion, IndexingTask task)
//        {
//            setLastFlags(null, true, null, nodeId, nodeVersion, task);
//        }
//        private static void resetLastDraftOn(int nodeId, NodeHead.NodeVersion nodeVersion, IndexingTask task)
//        {
//            setLastFlags(null, null, false, nodeId, nodeVersion, task);
//        }
//        private static void setLastDraftOn(int nodeId, NodeHead.NodeVersion nodeVersion, IndexingTask task)
//        {
//            setLastFlags(null, null, true, nodeId, nodeVersion, task);
//        }
//        private static void setLastFlags(bool? isPublic, bool? isLastPublic, bool? isLastDraft, int nodeId, NodeHead.NodeVersion nodeVersion, IndexingTask task)
//        {
//            var query = new TermQuery(new Term(LucObject.FieldName.VersionId, NumericUtils.IntToPrefixCoded(nodeVersion.VersionId)));
//            var lucQuery = LucQuery.Create(query);
//            lucQuery.EnableAutofilters = false;
//            lucQuery.EnableLifespanFilter = false;

//            var doc = lucQuery.Execute(true).First();
//            var isPublic1 = isPublic ?? doc[LucObject.FieldName.IsPublic] == BooleanIndexHandler.YES;
//            var isLastPublic1 = isLastPublic ?? doc[LucObject.FieldName.IsLastPublic] == BooleanIndexHandler.YES;
//            var isLastDraft1 = isLastDraft ?? doc[LucObject.FieldName.IsLastDraft] == BooleanIndexHandler.YES;
//            AddActivity(task, IndexingActivityType.UpdateDocument, nodeId, nodeVersion.VersionId, isPublic1, isLastPublic1, isLastDraft1);
//        }

//        #region without WriterActivity
//        //private static void resetLastPublicOn(int nodeId, NodeHead.NodeVersion nodeVersion, DistributedIndexWriter writer)
//        //{
//        //    setLastFlags(null, false, null, nodeId, nodeVersion, writer);
//        //}
//        //private static void setLastPublicOn(int nodeId, NodeHead.NodeVersion nodeVersion, DistributedIndexWriter writer)
//        //{
//        //    setLastFlags(null, true, null, nodeId, nodeVersion, writer);
//        //}
//        //private static void resetLastDraftOn(int nodeId, NodeHead.NodeVersion nodeVersion, DistributedIndexWriter writer)
//        //{
//        //    setLastFlags(null, null, false, nodeId, nodeVersion, writer);
//        //}
//        //private static void setLastDraftOn(int nodeId, NodeHead.NodeVersion nodeVersion, DistributedIndexWriter writer)
//        //{
//        //    setLastFlags(null, null, true, nodeId, nodeVersion, writer);
//        //}
//        //private static void setLastFlags(bool? isPublic, bool? isLastPublic, bool? isLastDraft, int nodeId, NodeHead.NodeVersion nodeVersion, DistributedIndexWriter writer)
//        //{
//        //    var query = new TermQuery(new Term(LucObject.FieldName.VersionId, NumericUtils.IntToPrefixCoded(nodeVersion.VersionId)));
//        //    var lucQuery = LucQuery.Create(query);
//        //    lucQuery.EnableAutofilters = false;
//        //    lucQuery.EnableLifespanFilter = false;

//        //    var doc = lucQuery.Execute(true).First();
//        //    var isPublic1 = isPublic ?? doc[LucObject.FieldName.IsPublic] == BooleanIndexHandler.YES;
//        //    var isLastPublic1 = isLastPublic ?? doc[LucObject.FieldName.IsLastPublic] == BooleanIndexHandler.YES;
//        //    var isLastDraft1 = isLastDraft ?? doc[LucObject.FieldName.IsLastDraft] == BooleanIndexHandler.YES;
//        //    var n = Node.LoadNode(nodeId, nodeVersion.VersionNumber);
//        //    var updatedDoc = DocumentBuilder.GetDocument(n, isPublic1, isLastPublic1, isLastDraft1);
//        //    writer.UpdateDocument(writer.GetIdTerm(updatedDoc), updatedDoc);
//        //    //UpdateNode(nodeVersion.VersionId, updatedDoc, writer);
//        //}
//        #endregion

//        /*======================================================================================================= IIndexPopulator Members */

//        // caller: IndexPopulator.Populator, Import.Importer, Tests.Initializer, RunOnce
//        public void ClearAndPopulateAll()
//        {
//            using (var traceOperation = Logger.TraceOperation("IndexPopulator ClearAndPopulateAll"))
//            {
//                //-- recreate
//                var writer = IndexManager.GetIndexWriter(true);
//                //var writer = LuceneManager.LuceneManager.GetIndexWriter();
//                try
//                {
//                    foreach (var node in NodeEnumerator.GetNodes("/Root", ExecutionHint.ForceRelationalEngine))
//                    {
//                        foreach (var version in GetVersions(node))
//                        {
//                            if (version != null)
//                            {
//                                var doc = DocumentBuilder.GetDocument(version);
//                                writer.AddDocument(doc);
//                                OnNodeIndexed(version);
//                            }
//                        }
//                    }

//                    writer.Optimize();
//                }
//                finally
//                {
//                    writer.Close();
//                }
//                IndexingTaskManager.DeleteAllTasks();
//                traceOperation.IsSuccessful = true;
//            }
//        }
//        // caller: IndexPopulator.Populator
//        public void RepopulateTree(string path)
//        {
//            using (var traceOperation = Logger.TraceOperation("IndexPopulator RepopulateTree"))
//            {
//                var writer = IndexManager.GetIndexWriter(false);
//                writer.DeleteDocuments(new Term(LucObject.FieldName.InTree, path.ToLower()));
//                try
//                {
//                    foreach (var node in NodeEnumerator.GetNodes(path, ExecutionHint.ForceRelationalEngine))
//                    {
//                        foreach (var version in GetVersions(node))
//                        {
//                            if (version != null)
//                            {
//                                var doc = DocumentBuilder.GetDocument(version);
//                                writer.AddDocument(doc);
//                                OnNodeIndexed(version);
//                            }
//                        }
//                    }
//                    writer.Optimize();
//                }
//                finally
//                {
//                    writer.Close();
//                }
//                traceOperation.IsSuccessful = true;
//            }
//        }

//        // caller: CommitPopulateNode (rename), Node.MoveTo, Node.MoveMoreInternal
//        public void PopulateTree(string path)
//        {
//            //-- add new tree
//            var task = CreateTask("PopulateTree");
//            task.IndexingActivities.Add
//            (
//                new IndexingActivity
//                {
//                    ActivityType = IndexingActivityType.AddTree,
//                    Path = path.ToLower()
//                }
//            );
//            IndexingTaskManager.RegisterTask(task);
//            IndexingTaskManager.ExecuteTask(task, true, true);
//        }

//        // caller: Node.Save, Node.SaveCopied
//        public object BeginPopulateNode(Node node, NodeSaveSettings settings, string originalPath, string newPath)
//        {
//            //VersionNumber lastMajorVersion = null;
//            //VersionNumber lastMinorVersion = null;
//            //if (node.Id != 0)
//            //{
//            //    var head = settings.NodeHead;
//            //    var lastMajorVersionId = head.LastMajorVersionId;
//            //    var lastMinorVersionId = head.LastMinorVersionId;
//            //    var lastVersions = head.Versions.Where(v => v.VersionId == lastMajorVersionId || v.VersionId == lastMinorVersionId);
//            //    lastMajorVersion = lastVersions.Where(v => v.VersionId == lastMajorVersionId).Select(v => v.VersionNumber).FirstOrDefault();
//            //    lastMinorVersion = lastVersions.Where(v => v.VersionId == lastMinorVersionId).Select(v => v.VersionNumber).FirstOrDefault();
//            //}

//            var populatorData = new DocumentPopulatorData
//            {
//                Node = node,
//                Settings = settings,
//                OriginalPath = originalPath,
//                NewPath = newPath,
//                NodeHead = settings.NodeHead,
//                IsNewNode = node.Id == 0,
//                //IsNewVersion = settings.IsNewVersion(), // raising != VersionRaising.None,
//                //IsPublic = settings.IsPublic(), //status != VersionStatus.Draft && status != VersionStatus.Locked,
//                //LastMajorVersion = lastMajorVersion,
//                //LastMinorVersion = lastMinorVersion,
//            };
//            return populatorData;
//        }
//        public void CommitPopulateNode(object data)
//        {
//            if (DocumentPopulator.NewAlgorithm)
//            {
//                CommitPopulateNodeNEW(data);
//                return;
//            }

//            using (var traceOperation = Logger.TraceOperation("IndexPopulator CommitPopulateNode"))
//            {
//                var state = (DocumentPopulatorData)data;
//                if (state.OriginalPath.ToLower() != state.NewPath.ToLower())
//                {
//                    DeleteTree(state.OriginalPath);
//                    PopulateTree(state.NewPath);
//                }
//                else if (state.IsNewNode)
//                {
//                    CreateNode(state.Node);
//                }
//                else if (state.Settings.IsNewVersion())
//                {
//                    if (state.NodeHead == null)
//                    {
//                        AddNewVersion(state.Node, state.Settings.IsPublic(), null, null);
//                    }
//                    else
//                    {
//                        var lastMajor = state.NodeHead.GetLastMajorVersion();
//                        var lastMinor = state.NodeHead.GetLastMinorVersion();
//                        var lastMajorVer = lastMajor == null ? null : lastMajor.VersionNumber;
//                        var lastMinorVer = lastMinor == null ? null : lastMinor.VersionNumber;
//                        AddNewVersion(state.Node, state.Settings.IsPublic(), lastMajorVer, lastMinorVer);
//                    }
//                }
//                else
//                {
//                    UpdateVersion(state);
//                }
//                OnNodeIndexed(state.Node);

//                traceOperation.IsSuccessful = true;
//            }
//        }
//        public void CommitPopulateNodeNEW(object data)
//        {
//            using (var traceOperation = Logger.TraceOperation("IndexPopulator CommitPopulateNode"))
//            {
//                var state = (DocumentPopulatorData)data;
//                if (state.OriginalPath.ToLower() != state.NewPath.ToLower())
//                {
//                    DeleteTree(state.OriginalPath);
//                    PopulateTree(state.NewPath);
//                }
//                else if (state.IsNewNode)
//                {
//                    CreateNode(state.Node);
//                }
//                else if (state.Settings.IsNewVersion())
//                {
//                    AddNewVersionNEW(state.Node);
//                }
//                else
//                {
//                    UpdateVersionNEW(state);
//                }
//                OnNodeIndexed(state.Node);

//                traceOperation.IsSuccessful = true;
//            }
//        }

//        // caller: CommitPopulateNode (rename), Node.MoveTo, Node.ForceDelete
//        public void DeleteTree(string path)
//        {
//            //-- add new tree
//            var task = CreateTask("DeleteTree");
//            task.IndexingActivities.Add
//            (
//                new IndexingActivity
//                {
//                    ActivityType = IndexingActivityType.RemoveTree,
//                    Path = path.ToLower()
//                }
//            );
//            IndexingTaskManager.RegisterTask(task);
//            IndexingTaskManager.ExecuteTask(task, true, true);
//        }

//        // caller: Node.DeleteMoreInternal
//        public void DeleteForest(IEnumerable<Int32> idSet)
//        {
//            var task = CreateTask("DeleteForest");
//            foreach (var head in NodeHead.Get(idSet))
//            {
//                task.IndexingActivities.Add
//                (
//                    new IndexingActivity
//                    {
//                        ActivityType = IndexingActivityType.RemoveTree,
//                        Path = head.Path.ToLower()
//                    }
//                );
//                IndexingTaskManager.RegisterTask(task);
//                IndexingTaskManager.ExecuteTask(task, true, true);
//            }
//        }
//        // caller: Node.MoveMoreInternal
//        public void DeleteForest(IEnumerable<string> pathSet)
//        {
//            var task = CreateTask("DeleteForest");
//            foreach (var path in pathSet)
//            {
//                task.IndexingActivities.Add
//                (
//                    new IndexingActivity
//                    {
//                        ActivityType = IndexingActivityType.RemoveTree,
//                        Path = path.ToLower()
//                    }
//                );
//                IndexingTaskManager.RegisterTask(task);
//                IndexingTaskManager.ExecuteTask(task, true, true);
//            }
//        }

//        public object BeginDeleteVersion(Node oldVersion)
//        {
//            using (var traceOperation = Logger.TraceOperation("IndexPopulator BeginDeleteVersion"))
//            {
//                var oldVersions = NodeHead.Get(oldVersion.Id).Versions;
//                var lastDraft = oldVersions.Where(v => v.VersionNumber < oldVersion.Version).Max(v => v.VersionNumber);
//                Node lastDraftAfterDelete = null;
//                if (lastDraft != null)
//                    lastDraftAfterDelete = Node.LoadNode(oldVersion.Id, lastDraft);
//                traceOperation.IsSuccessful = true;
//                return new DeleteVersionPopulatorData { OldVersion = oldVersion, LastDraftAfterDelete = lastDraftAfterDelete };
//            }
//        }
//        public void CommitDeleteVersion(object data)
//        {
//            using (var traceOperation = Logger.TraceOperation("IndexPopulator CommitDeleteVersion"))
//            {
//                var task = CreateTask("CommitDeleteVersion");

//                var state = (DeleteVersionPopulatorData)data;
//                Query query;
//                if (state.LastDraftAfterDelete != null)
//                {
//                    //-- set IsLastDraft flag on indexed doc
//                    var versionId = state.LastDraftAfterDelete.VersionId;

//                    query = new TermQuery(new Term(LucObject.FieldName.VersionId, NumericUtils.IntToPrefixCoded(versionId)));
//                    var doc = LucQuery.Create(query).Execute(true).FirstOrDefault();
//                    if (doc == null)
//                        throw new ApplicationException("Lucene index issue: Version was not found: " + versionId);

//                    var isPublic = doc[LucObject.FieldName.IsPublic] == BooleanIndexHandler.YES;
//                    var isLastPublic = doc[LucObject.FieldName.IsLastPublic] == BooleanIndexHandler.YES;
//                    AddActivity(task, IndexingActivityType.UpdateDocument, state.LastDraftAfterDelete.Id, state.LastDraftAfterDelete.VersionId, isPublic, isLastPublic, true);
//                }
//                //-- delete one doc
//                task.IndexingActivities.Add(new IndexingActivity()
//                {
//                    ActivityType = IndexingActivityType.RemoveDocument,
//                    VersionId = state.OldVersion.VersionId,
//                });


//                IndexingTaskManager.RegisterTask(task);
//                IndexingTaskManager.ExecuteTask(task, true, true);

//                traceOperation.IsSuccessful = true;
//            }

//            #region without WriterActivity
//            //using (var traceOperation = Logger.TraceOperation("IndexPopulator CommitDeleteVersion"))
//            //{
//            //    try
//            //    {
//            //        var state = (DeleteVersionPopulatorData)data;
//            //        Query query;
//            //        if (state.LastDraftAfterDelete != null)
//            //        {
//            //            //-- set IsLastDraft flag on indexed doc
//            //            var versionId = state.LastDraftAfterDelete.VersionId;

//            //            query = new TermQuery(new Term(LucObject.FieldName.VersionId, NumericUtils.IntToPrefixCoded(versionId)));
//            //            var doc = LucQuery.Create(query).Execute(true).FirstOrDefault();
//            //            if (doc == null)
//            //                throw new ApplicationException("Lucene index issue: Version was not found: " + versionId);

//            //            var isPublic = doc[LucObject.FieldName.IsPublic] == BooleanIndexHandler.YES;
//            //            var isLastPublic = doc[LucObject.FieldName.IsLastPublic] == BooleanIndexHandler.YES;
//            //            var updatedDoc = DocumentBuilder.GetDocument(state.LastDraftAfterDelete, isPublic, isLastPublic, true);

//            //            //zpace
//            //            //UpdateNode(versionId, updatedDoc, writer);
//            //            writer.UpdateDocument(writer.GetIdTerm(updatedDoc), updatedDoc);
//            //        }
//            //        //-- delete one doc
//            //        var term  = new Term(LucObject.FieldName.VersionId, NumericUtils.IntToPrefixCoded(state.OldVersion.VersionId));
//            //        writer.DeleteDocuments(term);
//            //    }
//            //    finally
//            //    {
//            //        //zpace
//            //        //writer.Close();
//            //        LuceneManager.LuceneManager.CommitIndexWriter(writer);
//            //    }
//            //    traceOperation.IsSuccessful = true;
//            //}
//            #endregion
//        }

//        public event EventHandler<NodeIndexedEvenArgs> NodeIndexed;
//        protected void OnNodeIndexed(Node node)
//        {
//            if (NodeIndexed == null)
//                return;
//            NodeIndexed(null, new NodeIndexedEvenArgs(node));
//        }
//    }
//}
