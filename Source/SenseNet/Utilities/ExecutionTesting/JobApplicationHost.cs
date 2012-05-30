using System;

namespace SenseNet.Utilities.ExecutionTesting
{
    public class JobApplicationHost : MarshalByRefObject
    {   
        public void StartJobManager(HostedJobManager jobManager)
        {
            jobManager.StartJobRequests(this);
        }
    }
}
