using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Diagnostics;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    internal class WriterRestartActivity : DistributedLuceneActivity
    {
        public override void Execute()
        {
            using (var optrace = new OperationTrace("WriterRestartActivity Execute"))
            {
                LuceneManager.Restart();
                optrace.IsSuccessful = true;
            }
        }
    }
}
