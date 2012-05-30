using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Diagnostics;
using Lucene.Net.Index;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Threading;
using SenseNet.Utilities;
using SenseNet.Utilities.ExecutionTesting;
using System.Runtime.Remoting.Messaging;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.Search.Indexing;

namespace TestPad
{
    class Program
    {
        static void Main(string[] args)
        {
            //var sw = Stopwatch.StartNew();
            //TypeHandler.LoadAssembliesFrom(@"c:\development\SenseNet\Development\Budapest\Source\SenseNet\WebSite\bin");
            //int dummy = DistributedApplication.Cache.Count;
            //var dummy2 = DistributedApplication.ClusterChannel;

            //while (true)
            //{
            //    Console.WriteLine(LuceneManager.IndexedDocumentCount);
            //    Thread.Sleep(100);
            //}

            ////var lqResult1 = new NodeQueryResult

            ////var n = Node.LoadNode("/root/system");
            ////var p = Repository.Root;
            ////var x = p.Children;

            ////var ss = s;
        }
    }
}
