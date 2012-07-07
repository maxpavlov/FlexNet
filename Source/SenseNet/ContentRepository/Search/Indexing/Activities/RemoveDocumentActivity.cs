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
    internal class RemoveDocumentActivity : LuceneDocumentActivity
    {
        internal override void Execute()
        {
            using (var optrace = new OperationTrace("RemoveDocumentActivity Execute"))
            {
                var delTerm = new Term(LuceneManager.KeyFieldName, NumericUtils.IntToPrefixCoded(this.VersionId));
                LuceneManager.DeleteDocuments(new[] { delTerm }, MoveOrRename);
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