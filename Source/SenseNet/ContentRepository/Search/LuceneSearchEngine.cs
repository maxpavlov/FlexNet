using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Search;
using Lucene.Net.Documents;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search.Parser;
using System.Diagnostics;
using SenseNet.Search.Indexing;
using SenseNet.Search.Indexing.Activities;

namespace SenseNet.Search
{
    public class LuceneSearchEngine : ISearchEngine
    {
        static LuceneSearchEngine()
        {
            Lucene.Net.Search.BooleanQuery.SetMaxClauseCount(100000);
        }

        public bool IndexingPaused
        {
            get { return SenseNet.ContentRepository.RepositoryInstance.IndexingPaused; }
        }


        public IIndexPopulator GetPopulator()
        {
            return new DocumentPopulator();
        }
        //public object GetIndexDocumentInfo(Node node)
        //{
        //    return IndexDocumentInfo.Create(node);
        //}
        public IEnumerable<int> Execute(NodeQuery nodeQuery)
        {
            var query = LucQuery.Create(nodeQuery);
            var lucObjects = query.Execute();
            return from lucObject in lucObjects select lucObject.NodeId;
            //return from lucObject in lucObjects select lucObject.VersionId;
        }
        public IEnumerable<int> Execute(string lucQuery)
        {
            var query = LucQuery.Parse(lucQuery);
            var lucObjects = query.Execute();
            return from lucObject in lucObjects select lucObject.NodeId;
        }

        private IDictionary<string, Type> _analyzers = new Dictionary<string, Type>();
        public IDictionary<string, Type> GetAnalyzers()
        {
            return _analyzers;
        }

        public static IEnumerable<LucObject> GetAllDocumentVersionsByNodeId(int nodeId)
        {
            var queryText = String.Concat(LucObject.FieldName.NodeId, ":", nodeId, " .AUTOFILTERS:OFF");
            var query = LucQuery.Parse(queryText);
            var result = query.Execute(true);
            return result;
        }

        public void SetIndexingInfo(object indexingInfo)
        {
            var allInfo = (Dictionary<string, PerFieldIndexingInfo>)indexingInfo;
            var analyzerTypes = new Dictionary<string, Type>();

            foreach (var item in allInfo)
            {
                var fieldName = item.Key;
                var fieldInfo = item.Value;
                if (fieldInfo.Analyzer != null)
                {
                    var analyzerType = TypeHandler.GetType(fieldInfo.Analyzer);
                    if (analyzerType == null)
                        throw new InvalidOperationException(String.Concat("Unknown analyzer: ", fieldInfo.Analyzer, ". Field: ", fieldName));
                    analyzerTypes.Add(fieldName, analyzerType);
                }
                _analyzers = analyzerTypes;
            }
            NotifyIndexingInfoChanged();
        }

        private void NotifyIndexingInfoChanged()
        {
            if (!LuceneManager.Running)
                return;
            var act = new WriterRestartActivity();
            act.Distribute();
            act.InternalExecute();
            //act.WaitForComplete();
        }

        public object DeserializeIndexDocumentInfo(byte[] indexDocumentInfoBytes)
        {
            if (indexDocumentInfoBytes == null)
                return null;
            if (indexDocumentInfoBytes.Length == 0)
                return null;

            var docStream = new System.IO.MemoryStream(indexDocumentInfoBytes);
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var info = (IndexDocumentInfo)formatter.Deserialize(docStream);
            return info;
        }
    }
}
