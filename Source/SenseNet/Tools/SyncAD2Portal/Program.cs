using System;
using SenseNet.DirectoryServices;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository;

namespace ADSync
{
    static class Program
    {
        static void Main(string[] args)
        {
            var startSettings = new RepositoryStartSettings
            {
                Console = Console.Out,
                StartLuceneManager = true
            };
            using (var repo = Repository.Start(startSettings))
            {
                using (var traceOperation = Logger.TraceOperation("SyncAD2Portal", string.Empty, AdLog.AdSyncLogCategory))
                {
                    SyncAD2Portal directoryServices = new SyncAD2Portal();
                    directoryServices.SyncFromAD();

                    traceOperation.IsSuccessful = true;
                }
            }
        }
    }
}
