using System;
using SenseNet.DirectoryServices;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository;

namespace ADSync
{
    class Program
    {
        static void Main(string[] args)
        {
            var startSettings = new RepositoryStartSettings
            {
                Console = Console.Out,
                StartLuceneManager = true
            };
            using (Repository.Start(startSettings))
            {
                using (var traceOperation = Logger.TraceOperation("SyncPortal2AD", string.Empty, AdLog.AdSyncLogCategory))
                {
                    ADProvider.RetryAllFailedActions();

                    traceOperation.IsSuccessful = true;
                }
            }
        }
    }
}
