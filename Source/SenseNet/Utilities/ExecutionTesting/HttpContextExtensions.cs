using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SenseNet.Utilities.ExecutionTesting
{
    public static class HttpContextExtensions
    {
        public static HostedJobWorkerRequest 
            GetWorkerRequest(this HttpContext context)
        {
            return 
                HostedJobWorkerRequest.GetWorkerRequest(context);
        }
        
    }
}
