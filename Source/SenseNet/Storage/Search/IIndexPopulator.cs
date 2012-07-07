using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Communication.Messaging;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.ContentRepository.Storage.Search
{
    public interface IIndexPopulator
    {
        void ClearAndPopulateAll();
        void RepopulateTree(string newPath);
        void PopulateTree(string newPath);
        object BeginPopulateNode(Node node, NodeSaveSettings settings, string originalPath, string newPath);
        void CommitPopulateNode(object data, IndexDocumentData indexDocument);
        void DeleteTree(string path, bool moveOrRename);
        void DeleteForest(IEnumerable<int> idSet, bool moveOrRename);
        void DeleteForest(IEnumerable<string> pathSet, bool moveOrRename);

        void RefreshIndexDocumentInfo(IEnumerable<Node> nodes);
        void RefreshIndexDocumentInfo(Node node, bool recursive);
        void RefreshIndex(IEnumerable<Node> nodes);
        void RefreshIndex(Node node, bool recursive);

        event EventHandler<NodeIndexedEvenArgs> NodeIndexed;
    }

    public class NodeIndexedEvenArgs : EventArgs
    {
        public string Path { get; private set; }
        public NodeIndexedEvenArgs(string path) { Path = path; }
    }
    internal class NullPopulator : IIndexPopulator
    {
        public static NullPopulator Instance = new NullPopulator();

        private static readonly object PopulatorData = new object();

        public void ClearAndPopulateAll() { }
        public void RepopulateTree(string newPath) { }
        public void PopulateTree(string newPath) { }
        public object BeginPopulateNode(Node node, NodeSaveSettings settings, string originalPath, string newPath) { return PopulatorData; }
        public void CommitPopulateNode(object data, IndexDocumentData indexDocument) { }
        public void DeleteTree(string path, bool moveOrRename) { }
        public event EventHandler<NodeIndexedEvenArgs> NodeIndexed;
        public void DeleteForest(IEnumerable<int> idSet, bool moveOrRename) { }
        public void DeleteForest(IEnumerable<string> pathSet, bool moveOrRename) { }

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
        }

        public void RefreshIndex(IEnumerable<Node> nodes) { }
        public void RefreshIndex(Node node, bool recursive) { }
    }
}
