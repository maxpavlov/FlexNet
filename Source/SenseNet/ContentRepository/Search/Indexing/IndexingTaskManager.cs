using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.Linq;
using System.IO;
using SenseNet.Search.Indexing;
using SenseNet.Search.Indexing.Activities;

namespace SenseNet.Search.Indexing
{
    public sealed class IndexingDataContext : IndexingTasksDataContext
    {
        public IndexingDataContext() 
            : base(ConfigurationManager.ConnectionStrings["SnCrMsSql"].ConnectionString)
        {
            
            if (IndexingTaskManager.Log != null)
            {
                this.Log = IndexingTaskManager.Log;
            }
            
        }
    }
    
    /// <summary>
    /// The IndexingTaskManager is responsible for registering, distributing and executing 
    /// tasks to maintain integrity in the Lucene index.
    /// </summary>
    internal static class IndexingTaskManager
    {
        
        public static TextWriter Log;

        /// <summary>
        /// Registers the task for execution and stores task data to the persistence layer.
        /// </summary>
        /// <param name="task">The task.</param>
        public static void RegisterTask(IndexingTask task)
        {
            using (var context = new IndexingDataContext())
            {
                context.IndexingTasks.InsertOnSubmit(task);
                context.SubmitChanges();
            }
        }

        public static IndexingTask GetTask(int taskId, bool loadActivities)
        {
            using (var context = new IndexingDataContext())
            {
                if (loadActivities) 
                { 
                    var options = new DataLoadOptions();
                    options.LoadWith<IndexingTask>(task => task.IndexingActivities);
                    context.LoadOptions = options;
                };

                return context.IndexingTasks.FirstOrDefault(task => task.IndexingTaskId == taskId);
            }
        }
        public static void UpdateTask(IndexingTask task)
        {
            using (var context = new IndexingDataContext())
            {
                context.IndexingTasks.Attach(task, true);
                context.SubmitChanges();
            }
        }

        public static IndexingTask[] GetUnprocessedTasks(int lastTaskId, IEnumerable<int> missingTasks)
        {
            using (var context = new IndexingDataContext())
            {
                var options = new DataLoadOptions();
                options.LoadWith<IndexingTask>(task => task.IndexingActivities);
                context.LoadOptions = options;
                return context.IndexingTasks.Where(task => task.IndexingTaskId > lastTaskId || missingTasks.Contains(task.IndexingTaskId)).OrderBy(t => t.IndexingTaskId).ToArray();
            }
        }

        internal static  LuceneIndexingActivity CreateLucActivity(IndexingActivity taskActivity)
        {
            switch (taskActivity.ActivityType)
            {
                case IndexingActivityType.AddDocument:
                    return LuceneIndexingActivity.CreateFromTaskActivity<AddDocumentActivity>(taskActivity);
                case IndexingActivityType.AddTree:
                    return LuceneIndexingActivity.CreateFromTaskActivity<AddTreeActivity>(taskActivity);
                case IndexingActivityType.UpdateDocument:
                    return LuceneIndexingActivity.CreateFromTaskActivity<UpdateDocumentActivity>(taskActivity);
                case  IndexingActivityType.RemoveTree:
                    return LuceneIndexingActivity.CreateFromTaskActivity<RemoveTreeActivity>(taskActivity);
                case IndexingActivityType.RemoveDocument:
                    return LuceneIndexingActivity.CreateFromTaskActivity<RemoveDocumentActivity>(taskActivity);
            }
            throw new ArgumentException("Invalid ActivityType value", taskActivity.ActivityType.ToString());
        }

        public static void ExecuteTaskDirect(IndexingTask task)
        {
            var lucActivities = task.GetLuceneActivities();
            foreach (var lucAct in lucActivities)
            {
                lucAct.Prepare();
                lucAct.Execute();
            }
        }

        public static void ExecuteTask(IndexingTask task, bool waitForComplete, bool distribute)
        {
            //create LuceneActivities from IndexActivities
            var lucActivities = task.GetLuceneActivities();

            //execute and wait for them
            //TODO: we need to call waitmultiple here
            foreach (var lucAct in lucActivities)
            {
                if (distribute) 
                    lucAct.Distribute();
                lucAct.Prepare();
                ActivityQueue.AddActivity(lucAct);
                if (waitForComplete) 
                    lucAct.WaitForComplete();
                
            }
        }

        internal static void DeleteAllTasks()
        {
            var proc = SenseNet.ContentRepository.Storage.Data.DataProvider.CreateDataProcedure(@"
BEGIN TRAN
DELETE FROM IndexingActivity
DELETE FROM IndexingTask
COMMIT
");
            proc.CommandType = System.Data.CommandType.Text;
            proc.ExecuteNonQuery();
        }

        internal static int GetLastTaskId()
        {
            return SenseNet.ContentRepository.Storage.Data.DataProvider.Current.GetLastTaskId();
        }
    }
}
