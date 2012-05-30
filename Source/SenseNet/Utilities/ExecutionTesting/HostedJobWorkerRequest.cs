using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.IO;
using System.Web;
using System.Reflection;

namespace SenseNet.Utilities.ExecutionTesting
{
    /// <summary>
    /// HostJobWorkerRequest will be our request into the web appdomain
    /// </summary>
    public class HostedJobWorkerRequest : SimpleWorkerRequest
    {
        public Job Job { get; internal set;}

        public HostedJobWorkerRequest(string page, string query, TextWriter output) :
            base(page, query, output)
        {
            
        }

        internal static HostedJobWorkerRequest GetWorkerRequest(HttpContext context)
        {
            var workerRequestProperty = typeof(HttpContext).GetProperty("WorkerRequest",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.Instance);
            if (workerRequestProperty == null)
                throw new InvalidOperationException("get_WorkerRequest not found");
            var wr = (HostedJobWorkerRequest)workerRequestProperty.GetValue(context, null);
            return wr;
        }

    }
}
