using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;
using SenseNet.Search.Indexing;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    public abstract class LuceneIndexingActivity : DistributedLuceneActivity
    {
        public int TaskId { get; set; }
        public int ActivityId { get; set; }
        public bool LastActivity { get; set; }

        public int NodeId { get; set; }
        public int VersionId { get; set; }

        public IndexingActivityType IndexingActivityType { get; set; }

        public Nullable<bool> IsPublicValue { get; set; }
        public Nullable<bool> IsLastPublicValue { get; set; }
        public Nullable<bool> IsLastDraftValue { get; set; }
        public string Path { get; set; }

        public static T CreateFromTaskActivity<T>(IndexingActivity taskActivity) where T : LuceneIndexingActivity, new()
        {
            T result = new T();
            result.TaskId = taskActivity.IndexingTaskId;
            result.ActivityId = taskActivity.IndexingActivityId;
            result.IndexingActivityType = taskActivity.ActivityType;
            result.IsLastPublicValue = taskActivity.IsLastPublicValue;
            result.IsPublicValue = taskActivity.IsPublicValue;
            result.IsLastDraftValue = taskActivity.IsLastDraftValue;
            result.Path = taskActivity.Path;
            result.VersionId = taskActivity.VersionId;
            result.NodeId = taskActivity.NodeId;
            return result;
        }

        public virtual void Prepare()
        {

        }


        public override void Execute()
        {
            LuceneManager.ApplyChanges(this.TaskId, this.LastActivity);
        }
    }

}