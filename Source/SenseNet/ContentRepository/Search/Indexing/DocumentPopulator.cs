using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using Lucene.Net.Search;
using Lucene.Net.Util;
using SenseNet.Search.Indexing.Activities;
using SenseNet.ContentRepository;

namespace SenseNet.Search.Indexing
{
    public class DocumentPopulator : IIndexPopulator
    {
        private class DocumentPopulatorData
        {
            internal Node Node { get; set; }
            internal NodeHead NodeHead { get; set; }
            internal NodeSaveSettings Settings { get; set; }
            internal string OriginalPath { get; set; }
            internal string NewPath { get; set; }
            internal bool IsNewNode { get; set; }
        }
        private class DeleteVersionPopulatorData
        {
            internal Node OldVersion { get; set; }
            internal Node LastDraftAfterDelete { get; set; }
        }

        /*======================================================================================================= IIndexPopulator Members */

        // caller: IndexPopulator.Populator, Import.Importer, Tests.Initializer, RunOnce
        public void ClearAndPopulateAll()
        {
            var lastTaskId = IndexingTaskManager.GetLastTaskId();
            var commitData = IndexManager.CreateCommitUserData(lastTaskId);
            using (var traceOperation = Logger.TraceOperation("IndexPopulator ClearAndPopulateAll"))
            {
                //-- recreate
                var writer = IndexManager.GetIndexWriter(true);
                try
                {
                    foreach (var docData in StorageContext.Search.LoadIndexDocumentsByPath("/Root"))
                    {
                        var doc = IndexDocumentInfo.GetDocument(docData);
                        writer.AddDocument(doc);
                        OnNodeIndexed(docData.Path);
                    }
                    RepositoryInstance.Instance.ConsoleWrite("  Commiting ... ");
                    writer.Commit(commitData);
                    RepositoryInstance.Instance.ConsoleWriteLine("ok");
                    RepositoryInstance.Instance.ConsoleWrite("  Optimizing ... ");
                    writer.Optimize();
                    RepositoryInstance.Instance.ConsoleWriteLine("ok");
                }
                finally
                {
                    writer.Close();
                }
                RepositoryInstance.Instance.ConsoleWrite("  Deleting indexing tasks ... ");
                IndexingTaskManager.DeleteAllTasks();
                RepositoryInstance.Instance.ConsoleWriteLine("ok");
                RepositoryInstance.Instance.ConsoleWrite("  Making backup ... ");
                BackupTools.BackupIndexImmediatelly();
                RepositoryInstance.Instance.ConsoleWriteLine("ok");
                traceOperation.IsSuccessful = true;
            }
        }

        // caller: IndexPopulator.Populator
        public void RepopulateTree(string path)
        {
            using (var traceOperation = Logger.TraceOperation("IndexPopulator RepopulateTree"))
            {
                var writer = IndexManager.GetIndexWriter(false);
                writer.DeleteDocuments(new Term(LucObject.FieldName.InTree, path.ToLower()));
                try
                {
                    foreach (var docData in StorageContext.Search.LoadIndexDocumentsByPath(path))
                    {
                        var doc = IndexDocumentInfo.GetDocument(docData);
                        writer.AddDocument(doc);
                        OnNodeIndexed(docData.Path);
                    }
                    writer.Optimize();
                }
                finally
                {
                    writer.Close();
                }
                traceOperation.IsSuccessful = true;
            }
        }

        // caller: CommitPopulateNode (rename), Node.MoveTo, Node.MoveMoreInternal
        public void PopulateTree(string path)
        {
            //-- add new tree
            //var task = CreateTask("PopulateTree");
            //AddActivity(task, IndexingActivityType.AddTree, path);
            //IndexingTaskManager.RegisterTask(task);
            //IndexingTaskManager.ExecuteTask(task, true, true);

            CreateTaskAndExecute("PopulateTree", IndexingActivityType.AddTree, path);
        }

        // caller: Node.Save, Node.SaveCopied
        public object BeginPopulateNode(Node node, NodeSaveSettings settings, string originalPath, string newPath)
        {
            var populatorData = new DocumentPopulatorData
            {
                Node = node,
                Settings = settings,
                OriginalPath = originalPath,
                NewPath = newPath,
                NodeHead = settings.NodeHead,
                IsNewNode = node.Id == 0,
            };
            return populatorData;
        }
        public void CommitPopulateNode(object data)
        {
            var state = (DocumentPopulatorData)data;
            if (state.OriginalPath.ToLower() != state.NewPath.ToLower())
            {
                DeleteTree(state.OriginalPath);
                PopulateTree(state.NewPath);
            }
            else if (state.IsNewNode)
            {
                CreateBrandNewNode(state.Node);
            }
            else if (state.Settings.IsNewVersion())
            {
                AddNewVersion(state.Node);
            }
            else
            {
                UpdateVersion(state);
            }
            OnNodeIndexed(state.Node.Path);
        }
        // caller: CommitPopulateNode (rename), Node.MoveTo, Node.ForceDelete
        public void DeleteTree(string path)
        {
            //-- add new tree
            //var task = CreateTask("DeleteTree");
            //AddActivity(task, IndexingActivityType.RemoveTree, path);
            //IndexingTaskManager.RegisterTask(task);
            //IndexingTaskManager.ExecuteTask(task, true, true);

            CreateTaskAndExecute("DeleteTree", IndexingActivityType.RemoveTree, path);
        }

        // caller: Node.DeleteMoreInternal
        public void DeleteForest(IEnumerable<Int32> idSet)
        {
            var task = CreateTask("DeleteForest");
            foreach (var head in NodeHead.Get(idSet))
                AddActivity(task, IndexingActivityType.RemoveTree, head.Path);
            IndexingTaskManager.RegisterTask(task);
            IndexingTaskManager.ExecuteTask(task, true, true);
        }
        // caller: Node.MoveMoreInternal
        public void DeleteForest(IEnumerable<string> pathSet)
        {
            var task = CreateTask("DeleteForest");
            foreach (var path in pathSet)
                AddActivity(task, IndexingActivityType.RemoveTree, path);
            IndexingTaskManager.RegisterTask(task);
            IndexingTaskManager.ExecuteTask(task, true, true);
        }

        public void RefreshIndexDocumentInfo(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
                RefreshIndexDocumentInfo(node, false);
        }
        public void RefreshIndexDocumentInfo(Node node, bool recursive)
        {
            if (!recursive)
            {
                RefreshIndexDocumentInfoOneNode(node);
                return;
            }
            using (new SenseNet.ContentRepository.Storage.Security.SystemAccount())
                foreach (var n in NodeEnumerator.GetNodes(node.Path))
                    RefreshIndexDocumentInfoOneNode(n);
        }
        private void RefreshIndexDocumentInfoOneNode(Node node)
        {
            var versionId = node.VersionId;
            DataBackingStore.SaveIndexDocument(node);
            if(RepositoryInstance.ContentQueryIsAllowed)
            {
                var task = CreateTask("UpdateVersion");
                AddActivity(task, IndexingActivityType.UpdateDocument, node.Id, node.VersionId, node.VersionTimestamp);
                ExecuteTask(task);
            }
        }

        public void RefreshIndex(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
                RefreshIndex(node, false);
        }
        public void RefreshIndex(Node node, bool recursive)
        {
            if (!recursive)
            {
                RefreshIndexOneNode(node);
                return;
            }
            using (new SenseNet.ContentRepository.Storage.Security.SystemAccount())
                foreach (var n in NodeEnumerator.GetNodes(node.Path))
                    RefreshIndexOneNode(n);
        }
        private void RefreshIndexOneNode(Node node)
        {
            var versionId = node.VersionId;
            if (RepositoryInstance.ContentQueryIsAllowed)
            {
                var task = CreateTask("UpdateVersion");
                AddActivity(task, IndexingActivityType.UpdateDocument, node.Id, node.VersionId, node.VersionTimestamp);
                ExecuteTask(task);
            }
        }

        public event EventHandler<NodeIndexedEvenArgs> NodeIndexed;
        protected void OnNodeIndexed(string path)
        {
            if (NodeIndexed == null)
                return;
            NodeIndexed(null, new NodeIndexedEvenArgs(path));
        }

        /*================================================================================================================================*/

        // caller: CommitPopulateNode
        private static void CreateBrandNewNode(Node node)
        {
            CreateTaskAndExecute("CreateNode", IndexingActivityType.AddDocument, node.Id, node.VersionId, node.VersionTimestamp);
        }
        // caller: CommitPopulateNode
        private static void AddNewVersion(Node newVersion)
        {
            CreateTaskAndExecute("AddNewVersion", IndexingActivityType.AddDocument, newVersion.Id, newVersion.VersionId, newVersion.VersionTimestamp);
        }
        // caller: CommitPopulateNode
        private static void UpdateVersion(DocumentPopulatorData state)
        {
            var task = CreateTask("UpdateVersion");
            foreach (var versionId in state.Settings.DeletableVersionIds)
                AddActivity(task, IndexingActivityType.RemoveDocument, state.Node.Id, versionId, 0);
            if (!state.Settings.DeletableVersionIds.Contains(state.Node.VersionId))
                AddActivity(task, IndexingActivityType.UpdateDocument, state.Node.Id, state.Node.VersionId, state.Node.VersionTimestamp);
            ExecuteTask(task);
        }

        // caller: ClearAndPopulateAll, RepopulateTree
        private static IEnumerable<Node> GetVersions(Node node)
        {
            var versionNumbers = Node.GetVersionNumbers(node.Id);
            var versions = from versionNumber in versionNumbers select Node.LoadNode(node.Id, versionNumber);
            return versions.ToArray();
        }
        /*================================================================================================================================*/

        private static IndexingTask CreateTask(string comment)
        {
            return new IndexingTask() { Comment = comment };
        }
        private static void AddActivity(IndexingTask task, IndexingActivityType type, int nodeId, int versionId, long versionTimestamp)
        {
            task.IndexingActivities.Add(new IndexingActivity() { ActivityType = type, NodeId = nodeId, VersionId = versionId, VersionTimestamp = versionTimestamp });
        }
        private static void AddActivity(IndexingTask task, IndexingActivityType type, string path)
        {
            task.IndexingActivities.Add(new IndexingActivity { ActivityType = type, Path = path.ToLower() });
        }
        private static void AddActivityAndExecute(IndexingTask task, IndexingActivityType type, int nodeId, int versionId, long versionTimestamp)
        {
            AddActivity(task, type, nodeId, versionId, versionTimestamp);
            ExecuteTask(task);
        }
        private static void CreateTaskAndExecute(string comment, IndexingActivityType type, int nodeId, int versionId, long versionTimestamp)
        {
            var task = CreateTask(comment);
            AddActivity(task, type, nodeId, versionId, versionTimestamp);
            ExecuteTask(task);
        }
        private static void CreateTaskAndExecute(string comment, IndexingActivityType type, string path)
        {
            var task = CreateTask(comment);
            AddActivity(task, type, path);
            ExecuteTask(task);
        }
        private static void ExecuteTask(IndexingTask task)
        {
            IndexingTaskManager.RegisterTask(task);
            IndexingTaskManager.ExecuteTask(task, true, true);
        }
    }
}
