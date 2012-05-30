using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SenseNet.Utilities;
using System.Threading;
using System.Diagnostics;

namespace SenseNet.Utilities.ExecutionTesting
{
    /// <summary>
    /// A Job represents work to be executed during StressTests.
    /// Each job will be executed on a separate thread inside the AppDomain of it's JobManager.
    /// </summary>
    [Serializable]
    public class Job
    {
        //Action that the Job will do repeatedly untill MaxIterationCount is reached
        public virtual Action<JobExecutionContext> Action
        {
            get; set;
        }

        public Job(string name, TextWriter output)
        {
            MaxIterationCount = 1000;
            Name = name;
            Console = output;
            Context = JobExecutionContext.Create(this);
        }

        //a set ki lett nyitva, ezt okosabban kell megcsinalni
        public TextWriter Console
        {
            get; set;
        }
        public JobExecutionContext Context { get; internal set; }
        public JobManager JobManager
        {
            get; internal set;
        }

        public bool StopOnException { get; set; }
        public bool StopAllOnException { get; set; }
        public int SleepTime { get; set; }
        public int MaxIterationCount { get; set;}
        public int? WarmupTime { get; set; }

        private bool _running = true;

        

        public void Execute()
        {
            var stopwatch = new Stopwatch();
            
            if (WarmupTime.HasValue && WarmupTime.Value > 0)
            {
                WriteLine("Warmup starting: {0}".FillTemplate(Name));
                Thread.Sleep(WarmupTime.Value);
                WriteLine("Warmup finished: {0}".FillTemplate(Name));
            }

            WriteLine("Job starting: {0}".FillTemplate(Name));
            while (ShouldExecute)
            {
                try
                {
                    ExecuteActionWithTimer(stopwatch);
                }
                catch (Exception e)
                {
                    //throw e;
                    HandleException(e);
                }
                finally
                {
                    if (SleepTime > 0) Thread.Sleep(SleepTime);
                }
            }
            WriteLine("Job finished: {0}".FillTemplate(this.Name));
            
            this.JobManager.JobFinished(this);
        }

        private void HandleException(Exception e)
        {
            Context.AddIterationException(e);
            WriteLine("*******************************************************");
            WriteLine("In job {0} the following exception occured:".FillTemplate(this.Name));
            WriteLine(e.ToString());
            WriteLine("*******************************************************");                                       
            
            
            if (StopAllOnException)
            {
                JobManager.Stop();
                StopOnException = true;
            }
            if (StopOnException)
                _running = false;
        }

        private void ExecuteActionWithTimer(Stopwatch stopwatch)
        {
            stopwatch.Reset();
            stopwatch.Start();
            Action(Context);
            Context.IterationCompleted(stopwatch.ElapsedMilliseconds);
        }


        private bool ShouldExecute
        {
            get
            {
                return 
                    _running &&
                    (Context.IterationCount < MaxIterationCount) && 
                    this.JobManager.Running;
            }
        }

        public void WriteLine(string message)
        {
           if (this.Console != null)
               Console.WriteLine(message);
        }

        public string Name { get; private set; }

        public string Domain { get; set; }

        public string UserName { get; set; }
    }
}
