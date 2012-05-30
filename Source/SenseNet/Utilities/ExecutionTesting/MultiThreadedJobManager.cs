using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SenseNet.Utilities.ExecutionTesting
{
    public class MultiThreadedJobManager : JobManager
    {
        List<Thread> Threads = new List<Thread>();

        public override void Start()
        {
            Running = true;
            this.GetJobs().ToList().ForEach(
                job =>
                {
                    Thread t = new Thread(new ParameterizedThreadStart(JobStarter));
                    Threads.Add(t);
                    t.Start(job);
                });
        }

        private void JobStarter(object job)
        {
            Job j = (Job)job;
            j.Execute();
        }
    }
}
