using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace SenseNet.Tools.IntegrityChecker
{
    class CheckIntegrity
    {
        private static string CR = Environment.NewLine;
        #region Usage screen
        private static string UsageScreen = String.Concat(
            //0         1         2         3         4         5         6         7      |
            //01234567890123456789012345678901234567890123456789012345678901234567890123456|
            "",
            "Sense/Net Content Repository Index integrity checker tool Usage:", CR,
            CR,
            "CheckIntegrity [-?] [-HELP]", CR,
            "CheckIntegrity -INDEX <indexpath> [-DATABASE <connection>] [-PATH <path>]", CR,
            "               [-NORECURSE]", CR,
            CR,
            "Parameters:", CR,
            "<indexpath>:    Main index directory (grandparent of files).", CR,
            "<connection>:   Database connection string if it is different from", CR,
            "                the configured.", CR,
            "<path>:         Checking scope (default: /Root)", CR,
            "NORECURSE:      Checking only one node.", CR,
            CR
        );
        #endregion

        internal static List<string> ArgNames = new List<string>(new string[] { "INDEX", "DATABASE", "PATH", "NORECURSE", "WAIT" });
        internal static bool ParseParameters(string[] args, List<string> argNames, out Dictionary<string, string> parameters, out string message)
        {
            message = null;
            parameters = new Dictionary<string, string>();
            if (args.Length == 0)
                return false;

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

        static void Main(string[] args)
        {
            Dictionary<string, string> parameters;
            string message;
            if (!ParseParameters(args, ArgNames, out parameters, out message))
            {
                Usage(message);
                return;
            }

            string indexPath = parameters.ContainsKey("INDEX") ? parameters["INDEX"] : null;
            string cnstr = parameters.ContainsKey("DATABASE") ? parameters["DATABASE"] : null;
            string repoPath = parameters.ContainsKey("PATH") ? parameters["PATH"] : null;
            bool recurse = !parameters.ContainsKey("NORECURSE");
            bool waitForAttach = parameters.ContainsKey("WAIT");

            //-- Path existence checks
            StringBuilder errorSb = new StringBuilder();
            if (indexPath != null && !Directory.Exists(indexPath))
                errorSb.Append("Path does not exist: -INDEX \"").Append(indexPath).Append("\"").Append(CR);
            if (errorSb.Length > 0)
            {
                Usage(errorSb.ToString());
                return;
            }

            try
            {
                if (waitForAttach)
                {
                    Console.WriteLine("Running in wait mode - now you can attach to the process with a debugger.");
                    Console.WriteLine("Press ENTER to continue.");
                    Console.ReadLine();
                }
                Run(indexPath, cnstr, repoPath, recurse);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Checking ends with error:");
                PrintException(e, null);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("<press enter to exit>");
                Console.ReadLine();
            }
        }

        private static void Run(string indexPath, string connectionString, string repoPath, bool recurse)
        {
            Console.WriteLine();
            Console.WriteLine("================= Sense/Net Index integrity checker Tool ====================");
            Console.WriteLine();

            var whole = repoPath == null && !recurse;
            if (!whole && repoPath == null)
                repoPath = Checker.RootPath;

            IEnumerable<Difference> diff;

            try
            {
                diff = whole ? Checker.Check(indexPath, connectionString) : Checker.Check(indexPath, connectionString, repoPath, recurse);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during index integrity check: {0}", ex.Message);
                return;
            }

            

            if (diff.Count() == 0)
            {
                Console.WriteLine("No differences.");
            }
            else
            {
                var count = 0;
                foreach (var d in diff)
                {
                    WriteDiff(d);
                    if (++count >= Checker.MAXRESULT)
                        break;
                }
                Console.WriteLine("{0} differences found.", diff.Count());
            }
            Console.WriteLine();
        }
        private static void WriteDiff(Difference d)
        {
            Console.WriteLine(d);
            Console.WriteLine();
        }

        private static void PrintException(Exception e, string path)
        {
            Console.WriteLine("========== Exception:");
            if (!String.IsNullOrEmpty(path))
                Console.WriteLine("Path: ", path);
            Console.Write(e.GetType().Name);
            Console.Write(": ");
            Console.WriteLine(e.Message);
            PrintTypeLoadError(e as ReflectionTypeLoadException);
            Console.WriteLine(e.StackTrace);
            while ((e = e.InnerException) != null)
            {
                Console.WriteLine("---- Inner Exception:");
                Console.Write(e.GetType().Name);
                Console.Write(": ");
                Console.WriteLine(e.Message);
                PrintTypeLoadError(e as ReflectionTypeLoadException);
                Console.WriteLine(e.StackTrace);
            }
            Console.WriteLine("=====================");
        }
        private static void PrintTypeLoadError(ReflectionTypeLoadException exc)
        {
            if (exc == null)
                return;
            Console.WriteLine("LoaderExceptions:");
            foreach (var e in exc.LoaderExceptions)
            {
                Console.Write("-- ");
                Console.Write(e.GetType().FullName);
                Console.Write(": ");
                Console.WriteLine(e.Message);

                var fileNotFoundException = e as FileNotFoundException;
                if (fileNotFoundException != null)
                {
                    Console.WriteLine("FUSION LOG:");
                    Console.WriteLine(fileNotFoundException.FusionLog);
                }
            }
        }

    }
}
