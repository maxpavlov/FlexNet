using System;
using System.Collections.Generic;
using System.Web.Hosting;
using System.IO;
using System.Diagnostics;

namespace SenseNet.Utilities.ExecutionTesting
{
    /// <summary>
    /// JobManager is the container of jobs, and acts as an AppDomain when you execute StressTests.
    /// Jobs inside a JobManager will be threads belonging to the same AppDomain.
    /// </summary>
    [Serializable]
    public abstract class JobManager
    {
        private List<Job> _jobs = new List<Job>();
        private List<Job> _runningJobs = new List<Job>();

        private bool _running;
        private TextWriter _output;

        public bool Running
        {
            get { return _running;}
            protected set { _running = value;}
        }

        public IEnumerable<Job> GetJobs()
        {
            lock(_jobs)
            {
                return _jobs.ToArray();
            }
        }

        public void AddJob(Job job)
        {

            job.JobManager = this;
            lock (_jobs)
            {
                _jobs.Add(job);
            }
        }

        public virtual void Start()
        {
            throw new NotImplementedException();
            //local thread implementation
            //TBD
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }

        protected internal virtual void StartJob(Job job)
        {
            lock (_runningJobs)
            {
                if (!_runningJobs.Contains(job))
                    _runningJobs.Add(job);
            }
            
            job.Execute();
        }

        internal void JobFinished(Job job)
        {
            int count;
            lock (_runningJobs)
            {
                _runningJobs.Remove(job);
                count = _runningJobs.Count;
            }
            if (count == 0)
            {
                Running = false;
                OnJobManagerFinished();
            }
        }

        protected virtual void OnJobManagerFinished()
        {

        }

    }

    
    //public static class ApplicationHostExtensions
    //{
    //    public static T CreateApplicationHost<T>(string virtualroot, string fileSystemPath)
    //    {
    //        T host = (T)ApplicationHost.CreateApplicationHost(typeof (T), virtualroot, fileSystemPath);
    //        //return
    //    }
    //}
}
