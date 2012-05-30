using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SenseNet.Utilities.ExecutionTesting
{
    [Serializable]
    public class JobExecutionContext
    {
        public JobExecutionContext()
        {
            
        }

        internal static JobExecutionContext Create(Job job)
        {
            var result = new JobExecutionContext();
            result.Job = job;
            return result;
        }

        public Job Job
        {
            get;
            internal set;
        }

        public decimal IterationCount { get; internal set; }

        private long maxDuration;
        private long sumDuration;

        public JobExecutionResult GetResult(bool includeExceptions)
        {
            return new JobExecutionResult()
                       {
                           MaxDuration = maxDuration,
                           SumDuration = sumDuration,
                           AverageDuration = sumDuration / IterationCount,
                           ExceptionCount = _exceptions.Count,
                           IterationCount = IterationCount,
                           Exceptions = includeExceptions ? _exceptions : new Collection<Exception>()
                       };
        }
        internal void IterationCompleted(long duration)
        {
            this.IterationCount++;
            sumDuration += duration;
            maxDuration = (duration > maxDuration) ? duration : maxDuration;

        }


        private readonly Collection<Exception> _exceptions = new Collection<Exception>();


        internal void AddIterationException(Exception e)
        {
            this.IterationCount++;
            lock(_exceptions)
            {
                _exceptions.Add(e);
            }
        }

        public void WriteLine(string message)
        {
            this.Job.WriteLine(message);
        }
    }
}
