//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using SenseNet.Utilities.ExecutionTesting;
//using SenseNet.ContentRepository.Storage.Caching.Dependency;
//using SenseNet.ContentRepository;
//using System.IO;

//namespace ConcurrencyTester
//{
//    [Serializable]
//    public class NotifyNodeChangeJob : Job
//    {
//        public NotifyNodeChangeJob(string name, TextWriter output)
//            : base(name, output)
//        {
//        }
//        private static Random r = new Random();
//        public override Action<JobExecutionContext> Action
//        {
//            get
//            {
//                return context =>
//                {


//                    var id = r.Next(1, 100);
//                    NodeIdDependency.FireChangedPrivate(id);
//                    Console.WriteLine("Notified: " + id.ToString());
//                };
//            }
//            set
//            {
//                throw new InvalidOperationException();
//                //base.Action = value;
//            }
//        }
//    }
//}
