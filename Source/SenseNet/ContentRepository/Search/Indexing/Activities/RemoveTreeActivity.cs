using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using SenseNet.Diagnostics;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage;
using Lucene.Net.Util;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    internal class RemoveTreeActivity : LuceneTreeActivity
    {
        internal override void Execute()
        {
            using (var optrace = new OperationTrace("RemoveTreeActivity Execute"))
            {
                var terms = new[] { new Term("InTree", TreeRoot), new Term("Path", TreeRoot) };
                LuceneManager.DeleteDocuments(terms, MoveOrRename);
                base.Execute();

                optrace.IsSuccessful = true;
            }
        }
    }
}