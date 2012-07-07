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
using SenseNet.Communication.Messaging;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Data;

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
            var lastActivityId = IndexingActivityManager.GetLastActivityId();
            var commitData = IndexManager.CreateCommitUserData(lastActivityId);
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
                RepositoryInstance.Instance.ConsoleWrite("  Deleting indexing activities ... ");
                IndexingActivityManager.DeleteAllActivities();
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
            CreateActivityAndExecute(IndexingActivityType.AddTree, path, false, null);
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
        public void CommitPopulateNode(object data, IndexDocumentData indexDocument = null)
        {
            var state = (DocumentPopulatorData)data;
            if (state.OriginalPath.ToLower() != state.NewPath.ToLower())
            {
                DeleteTree(state.OriginalPath, true);
                PopulateTree(state.NewPath);
            }
            else if (state.IsNewNode)
            {
                CreateBrandNewNode(state.Node, indexDocument);
            }
            else if (state.Settings.IsNewVersion())
            {
                AddNewVersion(state.Node, indexDocument);
            }
            else
            {
                UpdateVersion(state, indexDocument);
            }
            OnNodeIndexed(state.Node.Path);
        }
        // caller: CommitPopulateNode (rename), Node.MoveTo, Node.ForceDelete
        public void DeleteTree(string path, bool moveOrRename)
        {
            //-- add new tree
            CreateActivityAndExecute(IndexingActivityType.RemoveTree, path, moveOrRename, null);
        }

        // caller: Node.DeleteMoreInternal
        public void DeleteForest(IEnumerable<Int32> idSet, bool moveOrRename)
        {
            foreach (var head in NodeHead.Get(idSet))
                DeleteTree(head.Path, moveOrRename);
        }
        // caller: Node.MoveMoreInternal
        public void DeleteForest(IEnumerable<string> pathSet, bool moveOrRename)
        {
            foreach (var path in pathSet)
                DeleteTree(path, moveOrRename);
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
                ExecuteActivity(CreateActivity(IndexingActivityType.UpdateDocument, node.Id, node.VersionId, node.VersionTimestamp, null, null));//UNDONE: SingleVersion
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
            // node index is refreshed only on this node and should not be distributed.
            var versionId = node.VersionId;
            if (RepositoryInstance.ContentQueryIsAllowed)
            {
                var activity = CreateActivity(IndexingActivityType.UpdateDocument, node.Id, node.VersionId, node.VersionTimestamp, null, null);
                IndexingActivityManager.ExecuteActivity(activity, true, false);
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
        private static void CreateBrandNewNode(Node node, IndexDocumentData indexDocumentData)
        {
            CreateActivityAndExecute(IndexingActivityType.AddDocument, node.Id, node.VersionId, node.VersionTimestamp, true, indexDocumentData);
        }
        // caller: CommitPopulateNode
        private static void AddNewVersion(Node newVersion, IndexDocumentData indexDocumentData)
        {
            CreateActivityAndExecute(IndexingActivityType.AddDocument, newVersion.Id, newVersion.VersionId, newVersion.VersionTimestamp, null, indexDocumentData); //UNDONE: SingleVersion
        }
        // caller: CommitPopulateNode
        private static void UpdateVersion(DocumentPopulatorData state, IndexDocumentData indexDocumentData)
        {
            foreach (var versionId in state.Settings.DeletableVersionIds)
                ExecuteActivity(CreateActivity(IndexingActivityType.RemoveDocument, state.Node.Id, versionId, 0, null, null));//UNDONE: SingleVersion
            if (!state.Settings.DeletableVersionIds.Contains(state.Node.VersionId))
                ExecuteActivity(CreateActivity(IndexingActivityType.UpdateDocument, state.Node.Id, state.Node.VersionId, state.Node.VersionTimestamp, null, indexDocumentData));//UNDONE: SingleVersion
        }

        // caller: ClearAndPopulateAll, RepopulateTree
        private static IEnumerable<Node> GetVersions(Node node)
        {
            var versionNumbers = Node.GetVersionNumbers(node.Id);
            var versions = from versionNumber in versionNumbers select Node.LoadNode(node.Id, versionNumber);
            return versions.ToArray();
        }
        /*================================================================================================================================*/

        private static IndexingActivity CreateActivity(IndexingActivityType type, int nodeId, int versionId, long versionTimestamp, bool? singleVersion, IndexDocumentData indexDocumentData)
        {
            return new IndexingActivity
            {
                ActivityType = type,
                NodeId = nodeId,
                VersionId = versionId,
                VersionTimestamp = versionTimestamp,
                SingleVersion = singleVersion,
                IndexDocumentData = indexDocumentData
            };
        }
        private static IndexingActivity CreateActivity(IndexingActivityType type, string path, bool moveOrRename, IndexDocumentData indexDocumentData)
        {
            return new IndexingActivity
            {
                ActivityType = type,
                Path = path.ToLower(),
                IndexDocumentData = indexDocumentData,
                MoveOrRename = moveOrRename
            };
        }
        private static void CreateActivityAndExecute(IndexingActivityType type, int nodeId, int versionId, long versionTimestamp, bool? singleVersion, IndexDocumentData indexDocumentData)
        {
            ExecuteActivity(CreateActivity(type, nodeId, versionId, versionTimestamp, singleVersion, indexDocumentData));
        }
        private static void CreateActivityAndExecute(IndexingActivityType type, string path, bool moveOrRename, IndexDocumentData indexDocumentData)
        {
            ExecuteActivity(CreateActivity(type, path, moveOrRename, indexDocumentData));
        }
        private static void ExecuteActivity(IndexingActivity activity)
        {
            IndexingActivityManager.RegisterActivity(activity);
            IndexingActivityManager.ExecuteActivity(activity, true, true);
        }
    }
}
