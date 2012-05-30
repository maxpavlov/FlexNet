using System;
using System.Reflection;
using System.Web;

namespace SenseNet.Utilities.ExecutionTesting
{
    public class JobHttpHandler : IHttpHandler
    {
        public Action<string, string> AuthenticationAction { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            HostedJobWorkerRequest workerRequest = context.GetWorkerRequest();

            if (workerRequest == null)
               throw new InvalidOperationException("HostedJobWorkerRequest is null");
            if (workerRequest.Job == null || workerRequest.Job.JobManager == null)
                throw new InvalidOperationException("Job or JobManager is null");

            var jobManager = workerRequest.Job.JobManager;
            
            AuthenticateRequest(workerRequest);
            WriteStartMessage(context);           
            jobManager.StartJob(workerRequest.Job);

            WriteResultMessage(workerRequest);
        }

        private static void WriteStartMessage(HttpContext context)
        {
            "Starting job\n".PrintToContext();
            context.Response.Flush();
        }

        private static void WriteResultMessage(HostedJobWorkerRequest wr)
        {
            wr.Job.Context.GetResult(true).ToString().PrintToContext();
            "\nJobFinished\n".PrintToContext();
        }

        private void AuthenticateRequest(HostedJobWorkerRequest wr)
        {
            if (AuthenticationAction == null) return;
            if (wr == null) return;
            if (wr.Job == null) return;

            if ( !string.IsNullOrEmpty(wr.Job.UserName))
            {
                AuthenticationAction(wr.Job.Domain, wr.Job.UserName);
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

}
