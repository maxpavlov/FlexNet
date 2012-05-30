using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.Search.Indexing;
using System.Diagnostics;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    public abstract class LuceneDocumentActivity : LuceneIndexingActivity
    {
        private bool _documentIsCreated;
        private Document _document;
        public Document Document
        {
            get
            {
                if (!_documentIsCreated)
                {
                    _document = CreateDocument();
                    _documentIsCreated = true;
                }
                return _document;
            }
        }

        public virtual Document CreateDocument()
        {
            using (var optrace = new OperationTrace("CreateDocument"))
            {
                var doc = IndexDocumentInfo.GetDocument(this.VersionId);
                optrace.IsSuccessful = true;
                return doc;
            }
        }

        public override string ToString()
        {
            return String.Format("{0}: [{1}/{2}], {3}", this.GetType().Name, this.NodeId, this.VersionId, this.Path);
        }
    }

}