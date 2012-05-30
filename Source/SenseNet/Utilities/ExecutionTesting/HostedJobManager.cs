using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.IO;
using System.Threading;
using System.Web;
using System.Configuration;

namespace SenseNet.Utilities.ExecutionTesting
{
    [Serializable]
    public class HostedJobManager : JobManager
    {

        //we dont process _address yet
        //the website hosting the tests must accept http://127.0.0.1/ as host
        private string _address;

        private string _name;
        public string Name
        {
            get { return _name;  }
        }
        
        private readonly TextWriter _output;
        private readonly string _path;
        private const string JobHandlerPage = "JobHttpHandlerProxy.ashx";

        private Dictionary<Job, Thread> _threads;
        private Dictionary<Job, HostedJobWorkerRequest> _requests;

        public HostedJobManager() { }
        
        public HostedJobManager(string physicalWebFolderPath, string startPageWebAddress, TextWriter output, string name)
        {
            _path = physicalWebFolderPath;
            _output = output;
            _address = startPageWebAddress;
            _name = name;
        }
        
        public override void Start()
        {
            Running = true;
            //yeah once even I was a kid
            Console.WriteLine("init doom refresh daemon....");
            Console.WriteLine("webfolder path: {0}", _path);
            // this will call us back at StartJobRequests() via remoting
            CreateAppHost(_path).StartJobManager(this); 
        }

        //this method gets called from the ApplicationHost over remoting
        internal void StartJobRequests(JobApplicationHost jobApplicationHost)
        {
            
            _threads = new Dictionary<Job, Thread>();
            _requests = new Dictionary<Job, HostedJobWorkerRequest>();
            
            foreach (var job in GetJobs())
            {
                var hwr = CreateWorkerRequest(job);
                var t = new Thread(ThreadProc);
                _requests.Add(job, hwr);
                _threads.Add(job, t);
            }

            foreach(var item in _threads)
            {
                item.Value.Start(_requests[item.Key]);
            }
                
        }


        //we request the HostedJobHttpHandler or its Proxy via this worker and inject
        //the job instance into the request. HostJobHttpHandler will call
        private HostedJobWorkerRequest CreateWorkerRequest(Job job)
        {
            return new
                HostedJobWorkerRequest(JobHandlerPage, string.Empty, _output) 
                { Job = job };
        }

        private void ThreadProc(object hostedJobWorkerRequest)
        {
            var hjwr = hostedJobWorkerRequest as HostedJobWorkerRequest;
            if (hjwr == null)
                throw new InvalidOperationException("invalid worker request instance");

            _output.WriteLine("Starting worker: " + hjwr.Job.Name);
            HttpRuntime.ProcessRequest(hjwr);
        }

        private static JobApplicationHost CreateAppHost(string path)
        {
            var result = ApplicationHost.CreateApplicationHost(
                                                    typeof(JobApplicationHost),
                                                    "/",
                                                    path);
            return (JobApplicationHost)result;
        }

        public void AddAction(Action<JobExecutionContext> action, int sleepTime)
        {
            AddJob(
                new Job("Anon job", null)
                    {
                        Action = action,
                        SleepTime = sleepTime                        
                    }
                );

        }
    }
}
