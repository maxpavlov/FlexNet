using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.UI.WebControls.WebParts;

using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using WebPartTools;

namespace SenseNet.Tools.PortletAdministration
{
    internal class Program
    {
        // Properies ///////////////////////////////////////////////////////////////////

        private static IEnumerable<Node> _pages;
        internal static List<string> ArgNames = new List<string>(new[] {"LIST", "WAIT", "PAGE", "MOVE", "RECURSIVE", "DEL", "COPY", "TO"});

        public static IEnumerable<Node> Pages
        {
            get
            {
                if (_pages == null)
                    _pages = GetAllPages();
                return _pages;
            }
        }

        private static IEnumerable<Node> GetAllPages()
        {
            var query = new NodeQuery();
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["Page"], true));
            return query.Execute().Nodes;
        }

        private static void Main(string[] args)
        {
            Dictionary<string, string> parameters;
            string message;


            string[] args1 = Environment.GetCommandLineArgs();
            
            
            if (!ParseParameters(args, ArgNames, out parameters, out message))
            {
                Console.WriteLine("Error parsing parameters.");
                return;
            }

            

            bool waitForAttach = parameters.ContainsKey("WAIT");

            try
            {
                if (waitForAttach)
                {
                    Console.WriteLine("Running in wait mode - now you can attach to the process with a debugger.");
                    Console.WriteLine("Press ENTER to continue.");
                    Console.ReadLine();
                }

                WriteStartTime();

                Console.WriteLine();
                Console.WriteLine("Connecting to Repository ...");
                PortletManager.Current.LoadRepositoryRoot();
                WriteMessage("OK");
                WriteMessage();



                Run(parameters);


                WriteEndTime();
            }
            catch (Exception exc)
            {
                WriteException(exc);
            }


            //Console.Read();
        }
        private static void Run(Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey("LIST"))
                PortletManager.Current.ListWebPartElements(parameters);

            if (parameters.ContainsKey("MOVE"))
                PortletManager.Current.MoveWebParts(parameters);

            if (parameters.ContainsKey("DEL"))
                PortletManager.Current.DeleteWebParts(parameters);
        }
        
        // Parsing arguments (based upon the Importer) /////////////////////////////////
        internal static bool ParseParameters(string[] args, List<string> argNames,
                                             out Dictionary<string, string> parameters, out string message)
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

        // Logging /////////////////////////////////////////////////////////////////////
        private static void WriteStartTime()
        {
            string startDateFormat = @"#Start-Date: {0}";
            Console.WriteLine();
            Console.WriteLine(startDateFormat, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
        }
        private static void WriteEndTime()
        {
            string startDateFormat = @"#End-Date: {0}";
            Console.WriteLine();
            Console.WriteLine(startDateFormat, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
            Console.WriteLine();
        }
        private static void WritePageName(string pageName)
        {
            string pageNameFormat = @"{0}";
            Console.WriteLine();
            Console.WriteLine(pageNameFormat, pageName);
        }
        private static void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }
        private static void WriteMessage()
        {
            Console.WriteLine();
        }
        private static void WriteException(Exception exc)
        {
            Console.WriteLine(@"-----------------------------------------------------");
            Console.WriteLine(@"Message:        {0}", exc.Message);
            Console.WriteLine(@"Source:         {0}", exc.Source);
            Console.WriteLine(@"InnerException: {0}", exc.InnerException != null ? true.ToString() : false.ToString());
            Console.WriteLine(@"StackTrace:     {0}", exc.StackTrace);
            Console.WriteLine(@"-----------------------------------------------------");
        }
    }
}