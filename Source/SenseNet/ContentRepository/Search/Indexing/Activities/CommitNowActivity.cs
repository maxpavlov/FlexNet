using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;
using SenseNet.Search.Indexing;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using SenseNet.Search.Indexing;
using SenseNet.Diagnostics;

namespace SenseNet.Search.Indexing.Activities
{
    public class CommitNowActivity : DistributedLuceneActivity
    {
        public override void Execute()
        {
            using (var optrace = new OperationTrace("Executing CommitNow activity. Changes: " + LuceneManager._unCommitedChanges))
            {
                LuceneManager.CommitChanges();
                LuceneManager.RefreshReader();
                optrace.IsSuccessful = true;
            }
        }
    }
}
