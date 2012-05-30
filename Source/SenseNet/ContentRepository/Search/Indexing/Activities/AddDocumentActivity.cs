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
    public class AddDocumentActivity : LuceneDocumentActivity
    {
        public override void Execute()
        {
            using (var optrace = new OperationTrace("AddDocumentActivity Execute"))
            {
                if(Document != null)
                    LuceneManager.AddDocument(Document);
                base.Execute();
                optrace.IsSuccessful = true;
            }
        }
    }

}