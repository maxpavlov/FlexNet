using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using System.IO;
using System.Reflection;
using System.Globalization;
using Ionic.Zip;
using SenseNet.ContentRepository.Storage.Data;
using System.Net;
using System.Configuration;
using System.Diagnostics;
using SenseNet.Communication.Messaging;
using SenseNet.Search.Indexing;
using System.Threading;

namespace Backup
{
    internal enum BackupLevel { Index, Database, Full }

    class BackupProgram
    {
        private const string BACKUPCONTAINERNAME = "Backups";
        private const string ARG_INDEX = "INDEX";
        private const string ARG_DATABASE = "DATABASE";
        private const string ARG_FULL = "FULL";
        private const string ARG_TARGET = "TARGET";
        private const string ARG_WEB = "WEB";
        private const string ARG_KEEPDAYS = "KEEPDAYS";
        private const string ARG_CONFIRM = "CONFIRM";
        private const string ARG_WAIT = "WAIT";
        private const string ARG_HELP = "HELP";
        private const string ARG_FORCERESTORE = "FORCERESTORE";

        private const string CONFIG_INDEXBACKUPSTARTEDTIMEOUT_KEY = "IndexBackupStartedTimeOut";
        private const string CONFIG_INDEXBACKUPFINISHEDTIMEOUT_KEY = "IndexBackupFinishedTimeOut";
        private const string CONFIG_RESTOREINDEXTIMEOUT_KEY = "IndexRestoringTimeOut";
        private const string CONFIG_EXCLUSIONS_KEY = "WebDirectoryExclusions";
        private const string CONFIG_BACKUPDIRECORYPREFIX_KEY = "BackupDirectoryPrefix";
        private const string CONFIG_WEBZIPPREFIX_KEY = "WebZipPrefix";
        private const string CONFIG_DBBACKUPPREFIX_KEY = "DatabaseBackupPrefix";

        private const string CONFIG_CLUSTERCHANNELPROVIDER_KEY = "ClusterChannelProvider";
        private const string CONFIG_MSMQCHANNELQUEUENAME_KEY = "MsmqChannelQueueName";
        private const string CONFIG_INDEXBACKUPCREATORID_KEY = "IndexBackupCreatorId";

        private static string BackupPrefix = "SnBackup-";
        private static string WebZipPrefix = "WebSite-";
        private static string DbBackupPrefix = "SenseNetContentRepository-";

        private static string[] SupportedDataProviders = new[] { "SenseNet.ContentRepository.Storage.Data.SqlClient.SqlProvider" };

        private const int DEFAULTINDEXBACKUPSTARTEDTIMEOUT = 5;
        private const int DEFAULTINDEXBACKUPFINISHEDTIMEOUT = 20;
        private const double DEFAULTRESTOREINDEXTIMEOUT = 20;

        private static string CR = Environment.NewLine;

        #region Usage screen
        private static string UsageScreen = String.Concat(
            //   0         1         2         3         4         5         6         7        |
            //   0123456789012345678901234567890123456789012345678901234567890123456789012345678|
                "", CR,
                "Sense/Net Backup tool Usage:", CR,
                CR,
                "Backup -? | -HELP", CR,
                "Backup -INDEX | -DATABASE | -FULL [-TARGET <target>] [-WEB <webfolder>]", CR,
                "       [-KEEPDAYS <days>] [-CONFIRM]", CR,
                CR,
                "Parameters:", CR,
                "-INDEX:        Optimizing and saving Lucene index to database.", CR,
                "-FORCERESTORE: Restoring index immediatelly on every running appdomain.", CR,
                "               Real goal is the runtime index optimizing.", CR,
                "-DATABASE:     Saving Lucene index to database and database backup to the", CR,
                "               file system.", CR,
                "-FULL:         Saving Lucene index to database, database and web folder", CR,
                "               backup to the file system.", CR,
                "-TARGET:       Backups container.", CR,
                "<target>:      Full path of a directory that will contain all backups.", CR,
                "-WEB:          Required if the level is FULL.", CR,
                "<webfolder>:   Full path of the webfolder that will be backed up.", CR,
                "-KEEPDAYS:     Saving disk space.", CR,
                "<days>:        Number of days at which older material have to be deleted.", CR,
                "-CONFIRM       The Backup.exe displays the interpreted parameters and waits for", CR,
                "               your confirmation before executes the tasks.", CR,
                CR,
                "Comments:", CR,
                "- The TARGET is irrelevant if the level is INDEX.", CR,
                "- The WEB is irrelevant if the level is not FULL.", CR,
                "- The <target> and <webfolder> paths can be valid local or network", CR,
                "  filesystem path.", CR,
                "- Default <target>: '", BACKUPCONTAINERNAME, "' directory in the application", CR,
                "  working directory.", CR,
                "- Every backup is a directory. The passed <target> is the container of the", CR,
                "  backup directories.", CR,
                CR
            );
        #endregion

        internal static List<string> ArgNames = new List<string>(new string[] { ARG_INDEX, ARG_DATABASE, ARG_FULL, ARG_TARGET, ARG_WEB, ARG_KEEPDAYS, ARG_CONFIRM, ARG_WAIT, ARG_FORCERESTORE });
        internal static bool ParseParameters(string[] args, List<string> argNames, out Dictionary<string, string> parameters, out string message)
        {
            message = null;
            parameters = new Dictionary<string, string>();

            int argIndex = -1;
            int paramIndex = -1;
            string paramToken = null;
            while (++argIndex < args.Length)
            {
                string arg = args[argIndex];
                if (arg.StartsWith("-"))
                {
                    paramToken = arg.Substring(1).ToUpper();

                    if (paramToken == "?" || paramToken == ARG_HELP)
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
                Console.WriteLine("----------------------------------------");
                Console.WriteLine(message);
                Console.WriteLine("----------------------------------------");
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

            bool index = parameters.ContainsKey(ARG_INDEX);
            bool database = parameters.ContainsKey(ARG_DATABASE);
            bool full = parameters.ContainsKey(ARG_FULL);
            string backupPath = parameters.ContainsKey(ARG_TARGET) ? parameters[ARG_TARGET] : null;
            string webFolderPath = parameters.ContainsKey(ARG_WEB) ? parameters[ARG_WEB] : null;
            string keep = parameters.ContainsKey(ARG_KEEPDAYS) ? String.Empty + parameters[ARG_KEEPDAYS] : null;
            bool confirm = parameters.ContainsKey(ARG_CONFIRM);
            bool waitForAttach = parameters.ContainsKey(ARG_WAIT);
            bool forceRestore = parameters.ContainsKey(ARG_FORCERESTORE);

            BackupLevel? backupLevel = null;
            int q = (index ? 1 : 0) + (database ? 1 : 0) + (full ? 1 : 0);
            if (q == 0)
            {
                Usage(String.Concat("Missing level parameter (-", ARG_INDEX, ", -", ARG_DATABASE, " or -", ARG_FULL, ")."));
                return;
            }
            if (q > 1)
            {
                Usage(String.Concat("Only one level parameter can be used (-", ARG_INDEX, ", -", ARG_DATABASE, " or -", ARG_FULL, ")."));
                return;
            }

            if (index)
                backupLevel = BackupLevel.Index;
            if (database)
                backupLevel = BackupLevel.Database;
            if (full)
                backupLevel = BackupLevel.Full;

            //----------------
            double keepDays = 0;
            if (keep != null)
            {
                keep = keep.Replace(",", ".");
                if (!double.TryParse(keep, NumberStyles.Any, CultureInfo.InvariantCulture, out keepDays))
                {
                    Usage(String.Concat("Invalid ", ARG_KEEPDAYS, ": ", keep, ". Valid value is a non-negative number"));
                    return;
                }
                if (keepDays < 0)
                {
                    Usage(String.Concat("Invalid ", ARG_KEEPDAYS, ": ", keep, ". Valid value is a non-negative number"));
                    return;
                }
            }
            if (backupLevel == BackupLevel.Full)
            {
                if (webFolderPath == null)
                {
                    Usage(String.Concat("Missing ", ARG_WEB, " parameter."));
                    return;
                }
                if (!Directory.Exists(webFolderPath))
                {
                    Usage(String.Concat("Invalid ", ARG_WEB, " parameter: the directory does not exist: ", webFolderPath));
                    return;
                }
            }

            if (string.IsNullOrEmpty(backupPath))
                backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BACKUPCONTAINERNAME);

            if (waitForAttach)
            {
                Console.WriteLine("Running in wait mode - now you can attach to the process with a debugger.");
                Console.WriteLine("Press ENTER to continue.");
                Console.ReadLine();
            }

            //if (forceRestore)
            //{
            //    Console.WriteLine("{0} is not implemented. Backup will be executed but restore do not.", ARG_FORCERESTORE);
            //    forceRestore = false;
            //}

            try
            {
                Run(backupLevel.Value, backupPath, webFolderPath, keepDays, confirm, waitForAttach, forceRestore);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Backup ends with error:");
                PrintException(e, null);
                Console.WriteLine();
            }
            if (Debugger.IsAttached)
            {
                Console.WriteLine("<press ENTER to exit>");
                Console.ReadLine();
            }
        }
        internal static byte[] CreateHash(byte operation, int versionId, long versionTimestamp, byte[] hash) // 1000000 --> 510 sec
        {
            using (var hasher = System.Security.Cryptography.MD5.Create("MD5"))
            {
                var bb = new byte[13 + hash.Length];
                bb[bb.Length - 1] = operation;
                var b = BitConverter.GetBytes(versionId);
                for (var i = 0; i < b.Length; i++)
                    bb[bb.Length - 2 - i] = b[i];
                b = BitConverter.GetBytes(versionTimestamp);
                for (var i = 0; i < b.Length; i++)
                    bb[bb.Length - 6 - i] = b[i];
                for (var i = 0; i < hash.Length; i++)
                    bb[i] = hash[i];
                var h = hasher.ComputeHash(bb);
                return h;
            }
        }


        private static void Run(BackupLevel backupLevel, string backupPath, string webFolderPath, double keepDays, bool confirm, bool waitForAttach, bool forceRestore)
        {
            var timer = Stopwatch.StartNew();

            var s = ConfigurationManager.AppSettings[CONFIG_BACKUPDIRECORYPREFIX_KEY];
            if (!String.IsNullOrEmpty(s))
                BackupPrefix = s;

            var now = DateTime.Now;
            var keepDate = keepDays == 0 ? (DateTime?)null :
                (Math.Floor(keepDays) == keepDays ? new DateTime(now.Year, now.Month, now.Day) : now).AddDays(-keepDays);
            //var keepDate = keepDays == 0 ? (DateTime?)null : new DateTime(now.Year, now.Month, now.Day).AddDays(-keepDays);
            //var keepDate = keepDays == 0 ? (DateTime?)null : now.AddDays(-keepDays);
            var timeSuffix = now.ToString("yyyyMMdd-HHmmss");
            var newBackupDirectory = Path.Combine(backupPath, BackupPrefix + timeSuffix);

            Console.WriteLine();
            Console.WriteLine("======================== Sense/Net Backup Tool ==============================");
            Console.WriteLine("  BACKUP LEVEL:     {0}", backupLevel);
            if (backupLevel != BackupLevel.Index)
                Console.WriteLine("  BACKUP DIRECTORY: {0}", newBackupDirectory);
            if (backupLevel == BackupLevel.Full)
                Console.WriteLine("  WEBFOLDER:        {0}", webFolderPath);
            if (keepDate != null && backupLevel != BackupLevel.Index)
                Console.WriteLine("  CLEANING:         The backups will be deleted that are created before {0}", keepDate);
            if(forceRestore)
                Console.WriteLine("  All active instances will restore the index");
            else
                Console.WriteLine("  Active instances will not restore the index");
            Console.WriteLine("=============================================================================");
            if (confirm)
            {
                bool? ok = null;
                do
                {
                    Console.Write("Are you sure you want to continue with this parameters (<Y>: yes, <N>: no) ? ");
                    var key = Console.ReadLine();
                    if (key.ToUpper() == "Y")
                        ok = true;
                    if (key.ToUpper() == "N")
                        ok = false;
                } while (ok == null);
                Console.WriteLine("=============================================================================");
                if (ok == false)
                {
                    Console.WriteLine();
                    Console.WriteLine("Backup is not executed.");
                    Console.WriteLine();
                    return;
                }
            }
            Console.WriteLine("Executing backup...");

            var successful = Run(backupLevel, newBackupDirectory, webFolderPath, timeSuffix, keepDate, waitForAttach, forceRestore);
            timer.Stop();
            if (successful)
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Backup is successfully finished.");
                Console.WriteLine("Total executing time: " + timer.Elapsed);
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        private static bool Run(BackupLevel backupLevel, string newBackupDirectory, string webFolderPath, string timeSuffix, DateTime? keepDate, bool waitForAttach, bool forceRestore)
        {
            bool ok = false;
            if (!(ok = BackupIndex(forceRestore)))
                return false;
            if (forceRestore)
                ForceRestoreStart();
            if (backupLevel != BackupLevel.Index)
            {
                EnsureEmptyDirectory(newBackupDirectory);
                if (backupLevel == BackupLevel.Full)
                    if (!BackupWebfolder(webFolderPath, newBackupDirectory, timeSuffix))
                        return false;
                if (!BackupDatabase(newBackupDirectory, timeSuffix))
                    return false;
                if (keepDate != null)
                    Clean(Path.GetDirectoryName(newBackupDirectory), keepDate.Value);
            }
            if (forceRestore)
                return WaitForRestored();
            return true;
        }

        private static bool BackupIndex(bool forceRestore)
        {
            var clusterchannelprovider = ConfigurationManager.AppSettings[CONFIG_CLUSTERCHANNELPROVIDER_KEY];
            if (String.IsNullOrEmpty(clusterchannelprovider))
            {
                Console.WriteLine("Invalid configuration: Missing " + CONFIG_CLUSTERCHANNELPROVIDER_KEY);
                return false;
            }
            var msmqchannelqueuename = ConfigurationManager.AppSettings[CONFIG_MSMQCHANNELQUEUENAME_KEY];
            if (String.IsNullOrEmpty(msmqchannelqueuename))
            {
                Console.WriteLine("Invalid configuration: Missing " + CONFIG_MSMQCHANNELQUEUENAME_KEY);
                return false;
            }
            var indexbackupcreatorid = ConfigurationManager.AppSettings[CONFIG_INDEXBACKUPCREATORID_KEY];
            if (String.IsNullOrEmpty(indexbackupcreatorid))
            {
                Console.WriteLine("Invalid configuration: Missing " + CONFIG_INDEXBACKUPCREATORID_KEY);
                return false;
            }

            var timer = Stopwatch.StartNew();
            Console.Write("Connecting ... ");

            int startTimeOut;
            if (!Int32.TryParse(ConfigurationManager.AppSettings[CONFIG_INDEXBACKUPSTARTEDTIMEOUT_KEY], out startTimeOut))
                startTimeOut = DEFAULTINDEXBACKUPSTARTEDTIMEOUT;
            startTimeOut = startTimeOut * 10;

            int finishTimeOut;
            if (!Int32.TryParse(ConfigurationManager.AppSettings[CONFIG_INDEXBACKUPFINISHEDTIMEOUT_KEY], out finishTimeOut))
                finishTimeOut = DEFAULTINDEXBACKUPFINISHEDTIMEOUT;
            finishTimeOut = finishTimeOut * 10;

            DistributedApplication.ClusterChannel.MessageReceived += new MessageReceivedEventHandler(ClusterChannel_MessageReceived);

            var msg = new RequestBackupIndexMessage(Environment.MachineName, RepositoryConfiguration.IndexBackupCreatorId);
            msg.Send();

            var count = 0;
            while (!_indexBackupStarted)
            {
                Thread.Sleep(100);
                if (++count > startTimeOut)
                {
                    Console.WriteLine("TIMED OUT");
                    Console.WriteLine("Backup is not executed.");
                    Console.WriteLine();
                    Console.WriteLine("Check the requirements:");
                    Console.WriteLine("- There is an application that responds the configured cluster channel.");
                    Console.WriteLine("    Expected provider: " + clusterchannelprovider);
                    Console.WriteLine("    Expected channel name: " + msmqchannelqueuename);
                    Console.WriteLine("- There is an application that have the same 'IndexBackupCreatorId'.");
                    Console.WriteLine("    Expected IndexBackupCreatorId: " + indexbackupcreatorid);
                    return false;
                }
            }
            Console.WriteLine("ok.");

            //Console.WriteLine("Wait for saving the index ... ");
            while (!_indexBackupFinished)
            {
                Thread.Sleep(100);
                if (++count > finishTimeOut)
                {
                    Console.WriteLine("TIMED OUT");
                    Console.WriteLine("Backup is not executed.");
                    return false;
                }
            }

            timer.Stop();
            Console.WriteLine("ok. Executing time: " + timer.Elapsed);

            return true;
        }
        private static void ForceRestoreStart()
        {
            new RestoreIndexRequestMessage().Send();
            Console.WriteLine("Index restoring request is sent."); 
            _restoreRequestSent = DateTime.Now;
        }
        private static bool WaitForRestored()
        {
            double x;
            if (!Double.TryParse(ConfigurationManager.AppSettings[CONFIG_RESTOREINDEXTIMEOUT_KEY], out x))
                x = DEFAULTRESTOREINDEXTIMEOUT;
            DateTime finishTime = _restoreRequestSent.AddSeconds(x);
            while (true)
            {
                if (_restoringMessages.Count > 0)
                {
                    lock (_sync)
                    {
                        foreach (var msg in _restoringMessages)
                            Console.WriteLine(msg);
                        _restoringMessages.Clear();
                    }
                }
                if (_restoredMessages.Count > 0)
                {
                    lock (_sync)
                    {
                        foreach (var msg in _restoredMessages)
                            Console.WriteLine(msg);
                        _restoredMessages.Clear();
                    }
                }
                if ((_restoringInstances > 0 && _restoredInstances == _restoringInstances) || DateTime.Now > finishTime)
                    return _restoringErrors == 0;
                Thread.Sleep(100);
            }
        }

        private static bool _indexBackupStarted;
        private static bool _indexBackupFinished;
        private static object _sync = new object();
        private static int _restoringInstances = 0;
        private static int _restoredInstances = 0;
        private static List<string> _restoringMessages = new List<string>();
        private static List<string> _restoredMessages = new List<string>();
        private static DateTime _restoreRequestSent;
        private static int _restoringErrors;
        private static void ClusterChannel_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (args.Message is IndexBackupStartedMessage)
            {
                _indexBackupStarted = true;
                return;
            }

            var backupFinishedMessage = args.Message as IndexBackupFinishedMessage;
            if (backupFinishedMessage != null)
            {
                Console.WriteLine();
                Console.WriteLine("--------");
                Console.WriteLine("Summary:");
                //Console.WriteLine("Storing message count: " + _storingMessages);
                Console.WriteLine(backupFinishedMessage.Message);
                _indexBackupFinished = true;
                return;
            }

            var backupProgressMsg = args.Message as IndexBackupProgressMessage;
            if (backupProgressMsg != null)
            {
                DisplayProgress(backupProgressMsg);
                return;
            }

            var restoringStarteddMsg = args.Message as IndexRestoringStartedMessage;
            if (restoringStarteddMsg != null)
            {
                lock (_sync)
                {
                    _restoringInstances++;
                    _restoringMessages.Add(String.Concat("Index restoring started on ", restoringStarteddMsg.Machine, " | ", restoringStarteddMsg.InstanceId, " | ", _restoringInstances));
                }
            }
            var restoringFinishedMsg = args.Message as IndexRestoringFinishedMessage;
            if (restoringFinishedMsg != null)
            {
                lock (_sync)
                {
                    _restoredInstances++;
                    _restoredMessages.Add(String.Concat("Index restored on ", restoringFinishedMsg.Machine, " | ", restoringFinishedMsg.InstanceId, " | executing time: ", DateTime.Now - _restoreRequestSent));
                }
            }
            var errorMsg = args.Message as IndexRestoringErrorMessage;
            if (errorMsg != null)
            {
                lock (_sync)
                {
                    _restoredMessages.Add(String.Concat("Index restoring error:", CR, errorMsg.Exception.Message, CR, errorMsg.Exception.StackTrace));
                    _restoringErrors++;
                }
            }
        }
        private static int _lastPercent;
        //private static int _storingMessages;
        private static void DisplayProgress(IndexBackupProgressMessage msg)
        {
            if (msg.Type != IndexBackupProgressType.Storing)
            {
                if (msg.MaxValue == 1)
                {
                    if (msg.Value == 0)
                        Console.Write("{0} ... ", msg.Message);
                    if (msg.Value == msg.MaxValue)
                        Console.WriteLine("ok.");
                }
                else
                {
                    Console.WriteLine("{0} [{1}/{2}]", msg.Message, msg.Value, msg.MaxValue);
                }
                return;
            }

            //_storingMessages++;
            if (msg.Value == 0 && msg.MaxValue == 1)
            {
                Console.WriteLine("Storing backup to database");
                Console.WriteLine("__________________________________________________");
                _lastPercent = 0;
            }
            else if (msg.Value == msg.MaxValue)
            {
                for (int i = _lastPercent; i < 50; i++)
                    Console.Write("|");
                Console.WriteLine();
            }
            else
            {
                var percent = Convert.ToInt32(msg.Value * 100 / msg.MaxValue / 2);
                if (percent != _lastPercent)
                {
                    for (int i = _lastPercent; i < percent; i++)
                        Console.Write("|");
                    _lastPercent = percent;
                }
            }
        }

        private static bool BackupDatabase(string newBackupDirectory, string timeSuffix)
        {
            var timer = Stopwatch.StartNew();

            var providerName = DataProvider.Current.GetType().FullName;
            if (!SupportedDataProviders.Contains(providerName))
            {
                Console.WriteLine("{0}Cannot backup the database. This data provider is not supported: {1}.", CR, providerName);
                return false;
            }

            Console.Write("Backing up the database ... ");

            var s = ConfigurationManager.AppSettings[CONFIG_DBBACKUPPREFIX_KEY];
            if (!String.IsNullOrEmpty(s))
                DbBackupPrefix = s;

            var bakName = String.Concat(DbBackupPrefix, timeSuffix, ".bak");
            var bakFilePath = Path.Combine(newBackupDirectory, bakName); // C:\Program Files\Microsoft SQL Server\MSSQL.1\MSSQL\Backup\SenseNetContentRepository.bak
            var scripts = DataProvider.Current.GetScriptsForDatabaseBackup();

            var dbName = DataProvider.Current.DatabaseName;
            foreach (var script in scripts)
            {
                var sql = script
                    .Replace("{DatabaseName}", dbName)
                    .Replace("{BackupFilePath}", bakFilePath);

                using (var proc = DataProvider.CreateDataProcedure(sql))
                {
                    proc.CommandType = System.Data.CommandType.Text;
                    proc.ExecuteNonQuery();
                }
            }
            timer.Stop();

            Console.WriteLine("ok. Executing time: " + timer.Elapsed);
            return true;
        }
        private static bool BackupWebfolder(string webFolderPath, string newBackupDirectory, string timeSuffix)
        {
            Console.Write("Saving the webfolder... ");
            var timer = Stopwatch.StartNew();

            var s = ConfigurationManager.AppSettings[CONFIG_WEBZIPPREFIX_KEY];
            if (!String.IsNullOrEmpty(s))
                WebZipPrefix = s;

            var zipFilePath = Path.Combine(newBackupDirectory, String.Concat(WebZipPrefix, timeSuffix, ".zip"));
            var exclusions = GetExclusions(webFolderPath);
            if (newBackupDirectory.ToLower().StartsWith(webFolderPath.ToLower()))
            {
                s = newBackupDirectory.Substring(webFolderPath.Length + 1);
                var p = s.IndexOf("\\");
                if (p < 0)
                {
                    exclusions.Add(newBackupDirectory);
                }
                else
                {
                    s = s.Substring(0, p);
                    exclusions.Add(Path.Combine(webFolderPath, s));
                }
            }

            var dirs = Directory.GetDirectories(webFolderPath);
            var files = System.IO.Directory.GetFiles(webFolderPath);
            using (ZipFile zip = new ZipFile())
            {
                foreach (var dir in dirs)
                    if (!exclusions.Contains(dir))
                        zip.AddDirectory(dir, Path.GetFileName(dir));
                foreach (var file in files)
                    if (!exclusions.Contains(file))
                        zip.AddFile(file, ".");
                zip.Save(zipFilePath);
            }
            timer.Stop();

            Console.WriteLine("ok. Executing time: " + timer.Elapsed);
            return true;
        }

        private static List<string> GetExclusions(string webFolderPath)
        {
            var exclusions = ConfigurationManager.AppSettings[CONFIG_EXCLUSIONS_KEY];
            if (String.IsNullOrEmpty(exclusions))
                return new List<string>();
            var entries = exclusions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < entries.Length; i++)
                entries[i] = Path.Combine(webFolderPath, entries[i].Trim());
            return entries.ToList();
        }
        private static void Clean(string backupPath, DateTime dateTime)
        {
            Console.WriteLine("Cleaning the backups directory ... ");

            foreach (var dirPath in Directory.GetDirectories(backupPath))
            {
                var dirInfo = new DirectoryInfo(dirPath);
                if (dirInfo.CreationTime < dateTime)
                {
                    Console.WriteLine("  Delete: {0}", Path.GetFileName(dirPath));
                    Directory.Delete(dirPath, true);
                }
            }

            Console.WriteLine("ok.");
        }

        private static void EnsureEmptyDirectory(string path)
        {
            Directory.CreateDirectory(path);
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
        private static void PrintFieldErrors(Content content, string path)
        {
            Console.WriteLine("---------- Field Errors (path: ", path, "):");
            foreach (string fieldName in content.Fields.Keys)
            {
                Field field = content.Fields[fieldName];
                if (!field.IsValid)
                {
                    Console.Write(field.Name);
                    Console.Write(": ");
                    Console.WriteLine(field.GetValidationMessage());
                }
            }
            Console.WriteLine("------------------------");
        }
    }
}
