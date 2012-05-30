using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SenseNet.Search.Indexing.Activities;

namespace SenseNet.Search.Indexing
{
    public partial class IndexingTask : INotifyPropertyChanging, INotifyPropertyChanged
    {
        public IEnumerable<LuceneIndexingActivity> GetLuceneActivities()
        {
            var result = this.IndexingActivities.ToArray().Select(act => IndexingTaskManager.CreateLucActivity(act)).ToArray();
            var lastActivity = result.LastOrDefault();
            if (lastActivity != null)
                lastActivity.LastActivity = true;
            return result;
        }
    }
}
