using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.Tools.EventLogCreator
{
    class Program
    {
        private static readonly string DEFAULT_LOG_NAME = "SenseNet";
        private static readonly string DEFAULT_MACHINE_NAME = ".";
        private static readonly string DEFAULT_SOURCES = "Common,SenseNet.ContentRepository,SenseNet.Storage,SenseNet.Portal,SenseNet.CorePortlets,SenseNet.Services,SenseNet.WebSite";


        static void Main(string[] args)
        {
            var arguments = new CommandLineArguments(args);

            Console.WriteLine("Sense/Net Content Repository Tools - EventLog creator");
            Console.WriteLine("This tool creates a log and attached sources in the eventlog.");
            Console.WriteLine("-------------------------------------------------------------");

            if (!string.IsNullOrEmpty(arguments["getlogbysource"]))
            {
                string source = arguments["getlogbysource"];
                string registeredInLog = System.Diagnostics.EventLog.LogNameFromSourceName(source, ".");
                Console.WriteLine("Source \"{0}\" registered to log \"{1}\".", source, registeredInLog);
                return;
            }

            if (!string.IsNullOrEmpty(arguments["h"]) || !string.IsNullOrEmpty(arguments["help"]))
            {
                Console.WriteLine();
                Console.WriteLine("use: EventLogCreator [-machine MachineName] [-logname LogName] [-sources CommaSeparatedSources] [-delete]");
                Console.WriteLine();
                Console.WriteLine("-machine : On which machine you want to create the log. Default: \"{0}\"", DEFAULT_MACHINE_NAME);
                Console.WriteLine("-logname : The name of the log (eg. Application, Security...). Default: \"{0}\"", DEFAULT_LOG_NAME);
                Console.WriteLine("-sources : The name of the sources will be registered for the log. Default: \"{0}\"", DEFAULT_SOURCES);
                Console.WriteLine("-delete  : Deletes the log and the registrated sources rather than create it.");
                return;
            }

            Console.WriteLine();

            string machineName = arguments["machine"];
            if (string.IsNullOrEmpty(machineName))
                machineName = DEFAULT_MACHINE_NAME;
            Console.WriteLine("Machine name: \"{0}\"", machineName);

            string logName = arguments["logname"];
            if (string.IsNullOrEmpty(logName))
                logName = DEFAULT_LOG_NAME;
            Console.WriteLine("Log name: \"{0}\"", logName);

            string sources = arguments["sources"];
            if (string.IsNullOrEmpty(sources))
                sources = DEFAULT_SOURCES;
            Console.WriteLine("Sources: \"{0}\"", sources);

            bool delete = false;
            string deleteParam = arguments["delete"];
            if (deleteParam == "true")
                delete = true;
            Console.WriteLine("Create/Delete: {0}", delete ? "Delete" : "Create");

            Console.WriteLine();

            EventLogDeployer deployer = new EventLogDeployer(logName, sources.Split(",".ToCharArray()));

            if (delete)
                deployer.Delete();
            else
                deployer.Create();
            Console.Read();
        }
    }
}