using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.Search.Indexing;
using System.Diagnostics;

namespace SenseNet.Tools.Lucene.IndexPopulator
{
    class Populator
    {
        private static string CR = Environment.NewLine;
        private static List<string> ArgNames = new List<string>(new string[] { "SOURCE", "INDEX", "ASM", "WAIT" });
        private static bool ParseParameters(string[] args, List<string> argNames, out Dictionary<string, string> parameters, out string message)
        {
            message = null;
            parameters = new Dictionary<string, string>();
            //if (args.Length == 0)
            //    return false;

            int argIndex = -1;
            int paramIndex = -1;
            string paramToken = null;
            while (++argIndex < args.Length)
            {
                string arg = args[argIndex];
                if (arg.StartsWith("-"))
                {
                    paramToken = arg.Substring(1).ToUpper();

                    if (paramToken == "?" || paramToken == "HELP")
                        return false;

                    paramIndex = ArgNames.IndexOf(paramToken);
                    if (!argNames.Contains(paramToken))
                    {
                        message = "Unknown argument: " + arg;
                        return false;
                    }
                    parameters.Add(paramToken, null);
                }
                else
                {
                    if (paramToken != null)
                    {
                        parameters[paramToken] = arg;
                        paramToken = null;
                    }
                    else
                    {
                        message = String.Concat("Missing parameter name before '", arg, "'");
                        return false;
                    }
                }
            }
            return true;
        }
        private static void Usage(string message)
        {
            if (!String.IsNullOrEmpty(message))
            {
                Console.WriteLine("--------------------");
                Console.WriteLine(message);
                Console.WriteLine("--------------------");
            }
            Console.WriteLine(UsageScreen);
        }
        private static string UsageScreen = String.Concat(
        //   0         1         2         3         4         5         6         7         8
        //   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            CR,
            "Sense/Net Content Repository IndexPopulator tool Usage:", CR,
            "IndexPopulator [-?] [-HELP]", CR,
            "IndexPopulator [-SOURCE <source>] [-INDEX <indexpath>] [-ASM <asm>]", CR,
            CR,
            "Parameters:", CR,
            "<source>: Sense/Net Content Repository path as the export root (default: /Root)", CR,
            "<asm>:    FileSystem folder containig the required assemblies", CR,
            "          (default: location of IndexPopulator.exe)", CR,
            "<indexpath>: Location of Lucene index directory (default: depends on the configuration)", CR
        );

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Dictionary<string, string> parameters;
            string message;
            if (!ParseParameters(args, ArgNames, out parameters, out message))
            {
                Usage(message);
                return;
            }
            string repositoryPath = parameters.ContainsKey("SOURCE") ? parameters["SOURCE"] : null;
            string indexPath = parameters.ContainsKey("INDEX") ? parameters["INDEX"] : null;
            string asmPath = parameters.ContainsKey("ASM") ? parameters["ASM"] : null;
            bool wait = parameters.ContainsKey("WAIT") ? true : false;

            if (wait)
            {
                Console.WriteLine("Press enter to start...");
                Console.ReadLine();
            }

            Run(repositoryPath, indexPath, asmPath);
        }
        private static void Run(string repositoryPath, string indexPath, string asmPath)
        {
            using (var x = SenseNet.ContentRepository.Repository.Start(
                new SenseNet.ContentRepository.RepositoryStartSettings { Console = Console.Out, StartLuceneManager = false, StartWorkflowEngine = false, PluginsPath = asmPath }))
            {
                var y = (SenseNet.ContentRepository.RepositoryInstance)x;
                var z = y.StartupTrace;

                SenseNet.ContentRepository.Storage.Data.RepositoryConfiguration.WorkingModeIsPopulating = true;

                if (indexPath != null)
                    StorageContext.Search.SetIndexDirectoryPath(indexPath);

                _versionCount = SenseNet.ContentRepository.Storage.Data.DataProvider.GetVersionCount(repositoryPath);
                _factor = Convert.ToDouble(_versionCount) / Convert.ToDouble(_progressHead.Length);
                //Console.WriteLine("Progress of populating {0} items:", _versionCount);
                //Console.WriteLine(_progressHead);
                Console.WriteLine("Populating {0} items:", _versionCount);
                Console.Write("Initializing ... ");

                var populator = new DocumentPopulator();
                populator.NodeIndexed += new EventHandler<NodeIndexedEvenArgs>(IndexBuilder_NodeIndexed);
                if (repositoryPath == null)
                    populator.ClearAndPopulateAll();
                else
                    populator.RepopulateTree(repositoryPath);
                populator.NodeIndexed -= new EventHandler<NodeIndexedEvenArgs>(IndexBuilder_NodeIndexed);

                Console.WriteLine();
                Console.WriteLine("Populating has been finished.");
            }
        }

        private static string _progressHead = "________________________________________________________________";
        private static int _versionCount;
        private static int _count;
        private static double _factor;
        private static int _progress;

        static void IndexBuilder_NodeIndexed(object sender, NodeIndexedEvenArgs e)
        {
            if (_count == 0)
            {
                Console.WriteLine("ok.");
                Console.WriteLine("Progress:");
                Console.WriteLine(_progressHead);
                _count++;
            }
            else if (_count < _versionCount - 1)
            {
                var p = ++_count / _factor;
                p = Math.Floor(p);
                var progress = Convert.ToInt32(p);
                if (progress == _progress)
                    return;
                while (_progress < progress)
                {
                    _progress++;
                    Console.Write('|');
                }
            }
            else
            {
                Console.WriteLine('|');
                Console.WriteLine();
                Console.WriteLine("Finalizing");
            }
        }
        private static void LoadAssemblies()
        {
            LoadAssemblies(AppDomain.CurrentDomain.BaseDirectory);
        }
        private static void LoadAssemblies(string localBin)
        {
            string[] names = SenseNet.ContentRepository.Storage.TypeHandler.LoadAssembliesFrom(localBin);
        }

    }
}
