using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;
using SenseNet.Search.Indexing;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    internal abstract class LuceneIndexingActivity : DistributedLuceneActivity
    {
        public int ActivityId { get; set; }

        public int NodeId { get; set; }
        public int VersionId { get; set; }

        public IndexingActivityType IndexingActivityType { get; set; }

        public Nullable<bool> SingleVersion { get; set; }
        public bool MoveOrRename { get; set; }

        public string Path { get; set; }

        public static T CreateFromIndexingActivity<T>(IndexingActivity activity) where T : LuceneIndexingActivity, new()
        {
            T result = new T();
            result.ActivityId = activity.IndexingActivityId;
            result.IndexingActivityType = activity.ActivityType;
            result.SingleVersion = activity.SingleVersion;
            result.MoveOrRename = activity.MoveOrRename.HasValue ? activity.MoveOrRename.Value : false;
            result.Path = activity.Path;
            result.VersionId = activity.VersionId;
            result.NodeId = activity.NodeId;

            if (activity.IndexDocumentData != null)
            {
                var lucDocAct = result as LuceneDocumentActivity;
                if (lucDocAct != null)
                    lucDocAct.IndexDocumentData = activity.IndexDocumentData;
            }

            return result;
        }

        internal override void Execute()
        {
            LuceneManager.ApplyChanges(this.ActivityId);
        }
    }
}