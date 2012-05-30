using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;
using Lucene.Net.Index;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage;
using Lucene.Net.Documents;
using SenseNet.ContentRepository.Storage.Search;
using System.Threading;
using SenseNet.Search.Indexing;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    public class AddTreeActivity : LuceneTreeActivity
    {
        private Document[] Documents { get; set; }

        private IEnumerable<Node> GetVersions(Node node)
        {
            using (var traceOperation = Logger.TraceOperation("IndexPopulator GetVersions"))
            {
                var versionNumbers = Node.GetVersionNumbers(node.Id);
                var versions = from versionNumber in versionNumbers select Node.LoadNode(node.Id, versionNumber);
                var versionsArray = versions.ToArray();
                traceOperation.IsSuccessful = true;
                return versionsArray;
            }
        }

        public override void Execute()
        {
            using (var optrace = new OperationTrace("AddTreeActivity Execute"))
            {
                var count = 0;
                foreach (var docData in StorageContext.Search.LoadIndexDocumentsByPath(TreeRoot))
                {
                    var doc = IndexDocumentInfo.GetDocument(docData);
                    LuceneManager.AddCompleteDocument(doc);
                    count++;
                }

                Logger.WriteInformation(String.Concat("AddTreeActivity: ", count, " item added"));
                base.Execute();
                optrace.IsSuccessful = true;
            }
        }
    }
 
}
