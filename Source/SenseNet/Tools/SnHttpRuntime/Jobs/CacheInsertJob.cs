using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Utilities.ExecutionTesting;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.ContentRepository;
using System.IO;

namespace ConcurrencyTester
{
    [Serializable]
    public class CacheInsertJob : Job
    {
        public CacheInsertJob(string name, TextWriter output)
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
                    

                    var key = r.Next(1, 100000);
                    var id = r.Next(1, 100);
                    PortletDependency pd = new PortletDependency(id.ToString());
                    NodeIdDependency nd = new NodeIdDependency(id);
                    System.Web.Caching.AggregateCacheDependency acd = new System.Web.Caching.AggregateCacheDependency();
                    acd.Add(pd, nd);
                    DistributedApplication.Cache.Insert(key.ToString(), key, acd);
                    if (context.IterationCount % 100 == 0)
                    {
                        Console.WriteLine(this.Name + ":" + context.IterationCount.ToString());
                    }
                    //Console.WriteLine("Putting cache item: " + key.ToString());
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
