using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;
using SenseNet.Search.Indexing;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using SenseNet.Diagnostics;

namespace SenseNet.Search.Indexing.Activities
{
    internal class CommitNowActivity : DistributedLuceneActivity
    {
        internal override void Execute()
        {
            LuceneManager.ApplyChanges();
        }
    }
}
