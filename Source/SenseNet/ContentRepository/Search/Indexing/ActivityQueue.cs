using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using SenseNet.Search.Indexing.Activities;

namespace SenseNet.Search.Indexing
{
    internal class ActivityQueue
    {
        public static ActivityQueue Instance { get; set; }

        public static void AddActivity(DistributedLuceneActivity activity)
        {
            var pAct = activity as LuceneIndexingActivity;
            if (pAct != null)
            {
                var taskId = pAct.TaskId;
                var maxId = LuceneManager._maxTaskId;
                if (taskId - 1 > maxId)
                    if (!LuceneManager._executingUnprocessedIndexTasks) // creating gap
                        MissingTaskHandler.AddTasks(maxId, taskId);
            }
            Instance.AddActivityPrivate(activity);
        }

        internal ActivityQueue()
        {
            _pumpPeriod = 5;
            _checkGapDivider = SenseNet.ContentRepository.Storage.Data.RepositoryConfiguration.IndexHealthMonitorRunningPeriod * 1000 / _pumpPeriod;
        }

        private QueuedActivity AddActivityPrivate(QueuedActivity activity)
        {
            lock (_syncActivities)
            {
                _activities.Add(activity);
            }
            return activity;
        }

        private void AddActivities(QueuedActivity[] activities)
        {
            lock (_syncActivities)
            {
                _activities.AddRange(activities);
            }
        }

        public void Start()
        {
            running = true;
            EnsureRunning();
        }
        public void Stop()
        {
            running = false;
            while(!stopped)
                Thread.Sleep(_pumpPeriod);
            worker.Abort();
        }
        public void Pause()
        {
            pausing = true;
            if (!running)
                paused = true;
        }
        public void Continue()
        {
            pausing = false;
            if (!running)
                paused = false;
        }

        private List<QueuedActivity> _activities = new List<QueuedActivity>();

        private bool running;
        private bool stopped;
        private bool pausing;
        private bool paused;
        public bool Paused
        {
            get { return paused; }
        }

        Thread worker;
        private void EnsureRunning()
        {
            running = true;
            if (worker == null)
            {
                worker = new Thread(new ThreadStart(Pump));
                worker.Start();
            }
        }

        private int _pumpPeriod;
        private int _checkGapDivider;
        private void Pump()
        {
            int i = 0;
            bool checkgapRequest = false;
            while (running)
            {
                try
                {
                    if (pausing != paused)
                    {
                        paused = pausing;
                        Debug.WriteLine(String.Concat("@#> heartbeat: ", i, paused ? " pause" : " continue"), "T:" + Thread.CurrentThread.ManagedThreadId.ToString());
                    }
                    if (!paused)
                    {
                        if (!ProcessActivities())
                            Thread.Sleep(_pumpPeriod);
                    }
                    else
                    {
                        Thread.Sleep(_pumpPeriod);
                    }

                    if (++i % 1000 == 1)
                        Debug.WriteLine(String.Concat("@#> heartbeat: ", i, paused ? " paused" : ""), "T:" + Thread.CurrentThread.ManagedThreadId.ToString());
                    if (i % _checkGapDivider == 0 || checkgapRequest)
                    {
                        if (!paused)
                        {
                            LuceneManager.ExecuteLostIndexTasks();
                            if(checkgapRequest)
                                checkgapRequest = false;
                        }
                        else
                        {
                            checkgapRequest = true;
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    //we cant die just becaise a loosy coder created something brown
                    //Logger would be nice where but its not possible due to some threading issues
                    //TODO: investigate threading issues here when using our EntLib based logger framework
                    Trace.WriteLine("!> Exception in Pump: " + ex);
                    SenseNet.Diagnostics.Logger.WriteException(ex);
                }
            }
            stopped = true;
        }

        private object _syncActivities = new object();

        private bool ProcessActivities()
        {
            List<QueuedActivity> activities = CollectActivities();

            if (activities == null)
                return false;

            activities = Optimize(activities);
            if (activities.Count == 0)
                return false;

            using (new SenseNet.ContentRepository.Storage.Security.SystemAccount())
            {
                foreach (var activity in activities)
                {
                    activity.InternalExecute(this);
                }
            }
            return true;
        }

        private List<QueuedActivity> CollectActivities()
        {
            List<QueuedActivity> activities = null;
            lock (_syncActivities)
            {
                if (_activities.Count > 0)
                {
                    activities = _activities;
                    _activities = new List<QueuedActivity>();
                }
            }
            return activities;
        }

        protected virtual List<QueuedActivity> Optimize(List<QueuedActivity> activities)
        {
            return activities;
        }

        public override string ToString()
        {
            if (_activities.Count == 0)
                return "Activities: Count: 0";
            if (_activities.Count > 10)
                return "Activities: Count: " + _activities.Count;
            return "Activities: " + String.Join(", ", _activities.Select(a => a.ToString()).ToArray());
        }
    }
}
