////////////////////////////////////////////////////////////////////////////////////////
//
// !!!
//
// BEFORE executing the program, copy JobHttpHandlerProxy.ashx to the web folder
// MSMQ must be set for each appdomain and for the controller tool
//
// !!!
//
////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Linq;
using System.Threading;
using SenseNet.Communication.Messaging;
using SenseNet.ContentRepository;

namespace ConcurrencyTester
{
    [Serializable]
    class Program
    {
        public static int ActiveManagerCount = 0;
        public static bool IsTestController { get; set; }

        static void Main(string[] args)
        {
            if (args.Count() != 1 || String.IsNullOrEmpty(args[0]))
                return;

            var testSelector = args[0].ToLower();

            var test = StressTestLibrary.GetTest(testSelector);
            
            DistributedApplication.ClusterChannel.MessageReceived += new MessageReceivedEventHandler(ClusterChannel_MessageReceived);

            ActiveManagerCount = test.GetManagerCount();

            test.Run();
        }

        private static object _sync = new object();
        private static void ClusterChannel_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (args.Message is TestRunFinishedMessage)
            {
                lock (_sync)
                {
                    if (ActiveManagerCount == 0)
                        return;

                    if (ActiveManagerCount == 1)
                        TerminateTestrun();
                    
                    ActiveManagerCount--;
                }
            }
        }
        private static void TerminateTestrun()
        {
            //TODO: ide majd kell talani valami finomabb megoldast
            Thread.Sleep(30000);
            Environment.Exit(0);
        }
    }
}