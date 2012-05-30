using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Utilities.ExecutionTesting;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.ContentRepository;
using System.IO;
using System.Threading;

namespace ConcurrencyTester
{
    [Serializable]
    public class NotifyPortletChangeJob: Job
    {
        public NotifyPortletChangeJob(string name, TextWriter output)
            : base(name, output)
        {
        }
        private static Random r = new Random();
        public override Action<JobExecutionContext> Action
        {
            get
            {
                return context =>
                {
                    //if (context.IterationCount == 0)
                    //{
                    //    Console.WriteLine("not now");
                    //    Thread.Sleep(5000);
                    //    return;
                    //}
                    for (int i = 0; i++ < 100; )
                    {
                        PortletDependency.FireChanged(i.ToString());
                    }
                };
            }
            set
            {
                throw new InvalidOperationException();
                //base.Action = value;
            }
        }
    }
}
