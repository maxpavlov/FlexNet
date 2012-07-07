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
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    internal abstract class LuceneDocumentActivity : LuceneIndexingActivity
    {
        private bool _documentIsCreated;

        private IndexDocumentData _indexDocumentData;
        public IndexDocumentData IndexDocumentData
        {
            get { return _indexDocumentData; }
            set { _indexDocumentData = value; }
        }

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
                if (_indexDocumentData != null)
                {
                    //Trace.WriteLine("###I> create document from indexdocumentdata");
                    // create document from indexdocumentdata if it has been supplied (eg via MSMQ if it was small enough to send it over)
                    var docInfo = _indexDocumentData.IndexDocumentInfo as IndexDocumentInfo;
                    var doc = IndexDocumentInfo.CreateDocument(docInfo, _indexDocumentData);
                    optrace.IsSuccessful = true;
                    return doc;
                }
                else
                {
                    //Trace.WriteLine("###I> get document from db");
                    // create document via loading it from db (eg when indexdocumentdata was too large to send over MSMQ)
                    var doc = IndexDocumentInfo.GetDocument(this.VersionId);
                    optrace.IsSuccessful = true;
                    return doc;
                }
            }
        }

        public void SetDocument(Document document)
        {
            _document = document;
            _documentIsCreated = true;
        }

        public override string ToString()
        {
            return String.Format("{0}: [{1}/{2}], {3}", this.GetType().Name, this.NodeId, this.VersionId, this.Path);
        }

        public override void Distribute()
        {
            // check doc size before distributing
            var sendDocOverMSMQ = _indexDocumentData != null && _indexDocumentData.IndexDocumentInfoSize.HasValue && _indexDocumentData.IndexDocumentInfoSize.Value < RepositoryConfiguration.MsmqIndexDocumentSizeLimit;

            if (sendDocOverMSMQ)
            {
                // document is small to send over MSMQ
                //Trace.WriteLine("###I> DOCUMENT IS SMALL");
                base.Distribute();
            }
            else
            {
                // document is too large, send activity without the document
                //Trace.WriteLine("###I> DOCUMENT IS LARGE");
                var docData = _indexDocumentData;
                _indexDocumentData = null;

                base.Distribute();

                // restore indexdocument after activity is sent
                _indexDocumentData = docData;
            }
        }
    }
}