using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data.Linq;
using System.IO;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.Search.Indexing.Activities;

namespace SenseNet.Search.Indexing
{
    public sealed class IndexingDataContext : IndexingTasksDataContext
    {
        public IndexingDataContext() : base(ConfigurationManager.ConnectionStrings["SnCrMsSql"].ConnectionString) { }
    }
    
    /// <summary>
    /// Responsible for registering, distributing and executing activities to maintain integrity in the Lucene index.
    /// </summary>
    internal static class IndexingActivityManager
    {
        public static void RegisterActivity(IndexingActivity activity)
        {
            using (var context = new IndexingDataContext())
            {
                context.CommandTimeout = RepositoryConfiguration.SqlCommandTimeout;
                context.IndexingActivities.InsertOnSubmit(activity);
                context.SubmitChanges();
            }
        }

        public static IndexingActivity GetActivity(int activityId)
        {
            using (var context = new IndexingDataContext())
            {
                context.CommandTimeout = RepositoryConfiguration.SqlCommandTimeout;
                return context.IndexingActivities.FirstOrDefault(a => a.IndexingActivityId == activityId);
            }
        }
        public static void UpdateActivity(IndexingActivity activity)
        {
            using (var context = new IndexingDataContext())
            {
                context.CommandTimeout = RepositoryConfiguration.SqlCommandTimeout;
                context.IndexingActivities.Attach(activity, true);
                context.SubmitChanges();
            }
        }

        public static IndexingActivity[] GetUnprocessedActivities(IEnumerable<int> missingActivities)
        {
            using (var context = new IndexingDataContext())
            {
                context.CommandTimeout = RepositoryConfiguration.SqlCommandTimeout;
                return context.IndexingActivities.Where(a => missingActivities.Contains(a.IndexingActivityId)).OrderBy(b => b.IndexingActivityId).ToArray();
            }
        }
        public static IndexingActivity[] GetUnprocessedActivities(int lastActivityId, out int maxIdInDb, int top = 0, int max = 0)
        {
            using (var context = new IndexingDataContext())
            {
                context.CommandTimeout = RepositoryConfiguration.SqlCommandTimeout;

                var activityQuery = context.IndexingActivities.Where(a => a.IndexingActivityId > lastActivityId);

                //if last id is given
                if (max > 0)
                {
                    activityQuery = activityQuery.Where(a => a.IndexingActivityId <= max);
                    maxIdInDb = max;
                }
                else
                {
                    //maxIdInDb = context.IndexingActivities.Max(m => m.IndexingActivityId);
                    //we have to format the query this way to handle empty result
                    maxIdInDb = (from ia in context.IndexingActivities
                                 select (int?)ia.IndexingActivityId).Max() ?? 0;
                }

                activityQuery = activityQuery.OrderBy(b => b.IndexingActivityId);
                
                return top > 0 ? activityQuery.Take(top).ToArray() : activityQuery.ToArray();
            }
        }

        internal static  LuceneIndexingActivity CreateLucActivity(IndexingActivity activity)
        {
            switch (activity.ActivityType)
            {
                case IndexingActivityType.AddDocument:
                    return LuceneIndexingActivity.CreateFromIndexingActivity<AddDocumentActivity>(activity);
                case IndexingActivityType.AddTree:
                    return LuceneIndexingActivity.CreateFromIndexingActivity<AddTreeActivity>(activity);
                case IndexingActivityType.UpdateDocument:
                    return LuceneIndexingActivity.CreateFromIndexingActivity<UpdateDocumentActivity>(activity);
                case  IndexingActivityType.RemoveTree:
                    return LuceneIndexingActivity.CreateFromIndexingActivity<RemoveTreeActivity>(activity);
                case IndexingActivityType.RemoveDocument:
                    return LuceneIndexingActivity.CreateFromIndexingActivity<RemoveDocumentActivity>(activity);
            }
            throw new ArgumentException("Invalid ActivityType value", activity.ActivityType.ToString());
        }

        public static void ExecuteActivityDirect(IndexingActivity activity)
        {
            var lucAct = activity.CreateLuceneActivity();
            lucAct.Execute();
        }
        public static void ExecuteActivity(IndexingActivity activity, bool waitForComplete, bool distribute)
        {
            var lucAct = activity.CreateLuceneActivity();
            //TODO: we need to call waitmultiple here
            if (distribute)
                lucAct.Distribute();
            lucAct.InternalExecute();
            if (waitForComplete)
                lucAct.WaitForComplete();
        }

        internal static void DeleteAllActivities()
        {
            var proc = SenseNet.ContentRepository.Storage.Data.DataProvider.CreateDataProcedure(@"DELETE FROM IndexingActivity");
            proc.CommandType = System.Data.CommandType.Text;
            proc.ExecuteNonQuery();
        }

        internal static int GetLastActivityId()
        {
            return SenseNet.ContentRepository.Storage.Data.DataProvider.Current.GetLastActivityId();
        }
    }
}
