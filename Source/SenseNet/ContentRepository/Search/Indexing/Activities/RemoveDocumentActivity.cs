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
    public class RemoveDocumentActivity : LuceneDocumentActivity
    {
        public override void Execute()
        {
            using (var optrace = new OperationTrace("RemoveDocumentActivity Execute"))
            {
                var delTerm = new Term(LuceneManager.KeyFieldName, NumericUtils.IntToPrefixCoded(this.VersionId));
                LuceneManager.DeleteDocuments(new[] { delTerm });
                base.Execute();
                optrace.IsSuccessful = true;
            }
        }

        public override Lucene.Net.Documents.Document CreateDocument()
        {
            throw new InvalidOperationException();
        }

    }


}