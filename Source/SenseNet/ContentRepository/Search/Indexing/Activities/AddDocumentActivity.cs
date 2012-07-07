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

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    internal class AddDocumentActivity : LuceneDocumentActivity
    {
        internal override void Execute()
        {
            using (var optrace = new OperationTrace("AddDocumentActivity Execute"))
            {
                if (Document != null)
                {
                    if (true == this.SingleVersion)
                        LuceneManager.AddCompleteDocument(Document);
                    else
                        LuceneManager.AddDocument(Document);
                }
                base.Execute();
                optrace.IsSuccessful = true;
            }
        }

    }

}