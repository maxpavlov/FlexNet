using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Utilities.ExecutionTesting;
using System.IO;
using SenseNet.Communication.Messaging;
using System.Threading;
using System.Diagnostics;

namespace ConcurrencyTester
{
    [Serializable]
    public sealed class TestRunFinishedMessage : ClusterMessage
    {
        private string _message;
        public string Message { get { return _message; } }
        public TestRunFinishedMessage(string message)
        {
            _message = message;
        }
    }

    [Serializable]
    public class HostedJobManagerMSMQ : HostedJobManager
    {
        public HostedJobManagerMSMQ(string physicalWebFolderPath, string startPageWebAddress, TextWriter output, string name)
            : base(physicalWebFolderPath, startPageWebAddress, output, name)
        {
            
        }

        protected override void OnJobManagerFinished()
        {
            new TestRunFinishedMessage(this.Name).Send();
        }
    }
}
