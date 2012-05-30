using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;
using Lucene.Net.Index;
using SenseNet.Diagnostics;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage;
using Lucene.Net.Documents;
using Lucene.Net.Util;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    public class UpdateDocumentActivity : LuceneDocumentActivity
    {
        private Term GetIdTerm(Document document)
        {
            var versionID =
                 NumericUtils.IntToPrefixCoded(Int32.Parse((string)document.GetFieldable(LuceneManager.KeyFieldName).StringValue()));
            if (string.IsNullOrEmpty(versionID))
                throw new ApplicationException("VersionID field missing");

            var term = new Term(LuceneManager.KeyFieldName, versionID);
            return term;
        }

        public override void Execute()
        {
            using (var optrace = new OperationTrace("UpdateDocumentActivity Execute"))
            {
                if (Document != null)
                    LuceneManager.UpdateDocument(GetIdTerm(Document), Document);
                base.Execute();
                optrace.IsSuccessful = true;
            }
        }
    }
}