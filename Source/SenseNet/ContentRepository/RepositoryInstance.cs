using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using System.Configuration;
using System.Reflection;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Data;
using System.Diagnostics;
using System.Threading;

namespace SenseNet.ContentRepository
{
    /// <summary>
    /// Represents a running Repository. There is always one instance in any appdomain.
    /// Repository will be stopped when the instance is disposing.
    /// </summary>
    public sealed class RepositoryInstance : IDisposable
    {
        /// <summary>
        /// Provides some information about the boot sequence
        /// </summary>
        public class StartupInfo
        {
            /// <summary>
            /// Name of the assemblies thats are loaded before startup sequence begins.
            /// </summary>
            public string[] AssembliesBeforeStart { get; internal set; }
            /// <summary>
            /// Name of the assemblies thats are loaded from the appdomain's working directory.
            /// </summary>
            public string[] ReferencedAssemblies { get; internal set; }
            /// <summary>
            /// Name of the assemblies thats are loaded from an additional path (if there is).
            /// </summary>
            public string[] Plugins { get; internal set; }
            /// <summary>
            /// True if the index was read only before startup. Means: there was writer.lock file in the configured index directory.
            /// </summary>
            public bool IndexWasReadOnly { get; internal set; }
            /// <summary>
            /// Moment of the start before executing the startup sequence.
            /// </summary>
            public DateTime Starting { get; internal set; }
            /// <summary>
            /// Moment of the start after executing the startup sequence.
            /// </summary>
            public DateTime Started { get; internal set; }
        }

        private StartupInfo _startupInfo;
        private RepositoryStartSettings.ImmutableRepositoryStartSettings _settings;
        private static RepositoryInstance _instance;
        private static object _startupSync = new Object();

        /// <summary>
        /// Gets a <see cref="StartupInfo"/> instance that provides some information about the boot sequence.
        /// </summary>
        public StartupInfo StartupTrace { get { return _startupInfo; } }
        /// <summary>
        /// Gets the startup control information.
        /// </summary>
        public RepositoryStartSettings.ImmutableRepositoryStartSettings StartSettings
        {
            get { return _settings; }
        }
        /// <summary>
        /// Gets the started up instance or null.
        /// </summary>
        public static RepositoryInstance Instance { get { return _instance; } }

        private RepositoryInstance()
        {
            _startupInfo = new StartupInfo { Starting = DateTime.Now };
        }

        private static bool _started;
        internal static RepositoryInstance Start(RepositoryStartSettings settings)
        {
            if (!_started)
            {
                lock (_startupSync)
                {
                    if (!_started)
                    {
                        var instance = new RepositoryInstance();
                        instance._settings = new RepositoryStartSettings.ImmutableRepositoryStartSettings(settings);
                        _instance = instance;
                        try
                        {
                            instance.DoStart();
                        }
                        catch(Exception)
                        {
                            _instance = null;
                            throw;
                        }
                        _started = true;
                    }
                }
            }
            return _instance;
        }
        internal void DoStart()
        {
            ConsoleWriteLine();
            ConsoleWriteLine("Starting Repository...");
            ConsoleWriteLine();

            var x = Lucene.Net.Documents.Field.Index.NO;
            var y = Lucene.Net.Documents.Field.Store.NO;
            var z = Lucene.Net.Documents.Field.TermVector.NO;

            CounterManager.Start();

            RegisterAppdomainEventHandlers();

            if (_settings.IndexPath != null)
                StorageContext.Search.SetIndexDirectoryPath(_settings.IndexPath);
            RemoveIndexWriterLockFile();

            LoadAssemblies();
            StartManagers();

            ConsoleWriteLine();
            ConsoleWriteLine("Repository has started.");
            ConsoleWriteLine();

            _startupInfo.Started = DateTime.Now;
        }
        /// <summary>
        /// Starts Lucene if it is not running.
        /// </summary>
        public void StartLucene()
        {
            if (LuceneManagerIsRunning)
            {
                ConsoleWrite("LuceneManager has already started.");
                return;
            }
            ConsoleWriteLine("Starting LuceneManager:");

            //var x = Lucene.Net.Documents.Field.Index.NO;
            //var y = Lucene.Net.Documents.Field.Store.NO;
            //var z = Lucene.Net.Documents.Field.TermVector.NO;
            SenseNet.Search.Indexing.LuceneManager.Start(_settings.Console);

            ConsoleWriteLine("LuceneManager has started.");
        }
        /// <summary>
        /// Starts workflow engine if it is not running.
        /// </summary>
        public void StartWorkflowEngine()
        {
            if (_workflowEngineIsRunning)
            {
                ConsoleWrite("Workflow engine has already started.");
                return;
            }
            ConsoleWrite("Starting Workflow subsystem ... ");
            var t = TypeHandler.GetType("SenseNet.Workflow.InstanceManager");
            if (t != null)
            {
                var m = t.GetMethod("StartWorkflowSystem", BindingFlags.Static | BindingFlags.Public);
                m.Invoke(null, new object[0]);
                _workflowEngineIsRunning = true;
                ConsoleWriteLine("ok.");
            }
            else
            {
                ConsoleWriteLine("NOT STARTED");
            }
        }
        
        private void LoadAssemblies()
        {
            string[] asmNames;
            _startupInfo.AssembliesBeforeStart = GetLoadedAsmNames().ToArray();
            var localBin = AppDomain.CurrentDomain.BaseDirectory;
            var pluginsPath = _settings.PluginsPath ?? localBin;

            if (System.Web.HttpContext.Current != null)
            {
                ConsoleWrite("Getting referenced assemblies ... ");
                System.Web.Compilation.BuildManager.GetReferencedAssemblies();
                ConsoleWriteLine("Ok.");
            }
            else
            {
                //Assembly.GetExecutingAssembly().GetReferencedAssemblies();
                ConsoleWriteLine("Loading Assemblies from ", localBin, ":");
                asmNames = SenseNet.ContentRepository.Storage.TypeHandler.LoadAssembliesFrom(localBin);
                foreach (string name in asmNames)
                    ConsoleWriteLine("  ", name);
            }
            _startupInfo.ReferencedAssemblies = GetLoadedAsmNames().Except(_startupInfo.AssembliesBeforeStart).ToArray();


            ConsoleWriteLine("Loading Assemblies from ", pluginsPath, ":");
            asmNames = SenseNet.ContentRepository.Storage.TypeHandler.LoadAssembliesFrom(pluginsPath);
            _startupInfo.Plugins = GetLoadedAsmNames().Except(_startupInfo.AssembliesBeforeStart).Except(_startupInfo.ReferencedAssemblies).ToArray();

            if (_settings.Console == null)
                return;

            foreach (string name in asmNames)
                ConsoleWriteLine("  ", name);
            ConsoleWriteLine("Ok.");
            ConsoleWriteLine();
        }
        private IEnumerable<string> GetLoadedAsmNames()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(a => a.FullName).ToArray();
        }
        private void StartManagers()
        {
            object dummy;

            ConsoleWrite("Initializing cache ... ");
            dummy = SenseNet.ContentRepository.DistributedApplication.Cache.Count;
            ConsoleWriteLine("ok.");

            ConsoleWrite("Starting message channel ... ");
            var channel = SenseNet.ContentRepository.DistributedApplication.ClusterChannel;
            ConsoleWriteLine("ok.");

            ConsoleWrite("Starting NodeType system ... ");
            dummy = ActiveSchema.NodeTypes[0];
            ConsoleWriteLine("ok.");

            ConsoleWrite("Starting ContentType system ... ");
            dummy = SenseNet.ContentRepository.Schema.ContentType.GetByName("GenericContent");
            ConsoleWriteLine("ok.");

            ConsoleWrite("Starting AccessProvider ... ");
            dummy = User.Current;
            ConsoleWriteLine("ok.");

            if (_settings.StartLuceneManager)
                StartLucene();
            else
                ConsoleWriteLine("LuceneManager is not started.");

            //switch on message processing after LuceneManager was started
            channel.AllowMessageProcessing = true;

            SenseNet.Search.Indexing.IndexHealthMonitor.Start(_settings.Console);

            if (_settings.StartWorkflowEngine)
                StartWorkflowEngine();
            else
                ConsoleWriteLine("Workflow subsystem is not started.");

            foreach (var serviceType in TypeHandler.GetTypesByInterface(typeof(ISnService)))
            {
                var service = (ISnService)Activator.CreateInstance(serviceType);
                service.Start();
            }
        }

        private void RemoveIndexWriterLockFile()
        {
            //delete write.lock if necessary
            var lockFilePath = StorageContext.Search.IndexLockFilePath;
            if (lockFilePath == null)
                return;
            if (System.IO.File.Exists(lockFilePath))
            {
                _startupInfo.IndexWasReadOnly = true;
                var endRetry = DateTime.Now.AddSeconds(RepositoryConfiguration.LuceneLockDeleteRetryInterval);

                //retry write.lock for a given period of time
                while (true)
                {
                    try
                    {
                        System.IO.File.Delete(lockFilePath);
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Threading.Thread.Sleep(5000);
                        if (DateTime.Now > endRetry)
                            throw new System.IO.IOException("Cannot remove the index lock: " + ex.Message, ex);
                    }
                }
            }
            else
            {
                _startupInfo.IndexWasReadOnly = false;
                ConsoleWriteLine("Index directory is read/write.");
            }
        }
        private void RegisterAppdomainEventHandlers()
        {
            AppDomain appDomain = AppDomain.CurrentDomain;
            //appDomain.AssemblyLoad += new AssemblyLoadEventHandler(Domain_AssemblyLoad);
            //appDomain.AssemblyResolve += new ResolveEventHandler(Domain_AssemblyResolve);
            //appDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(Domain_ReflectionOnlyAssemblyResolve);
            //appDomain.ResourceResolve += new ResolveEventHandler(Domain_ResourceResolve);
            //appDomain.TypeResolve += new ResolveEventHandler(Domain_TypeResolve);
            appDomain.UnhandledException += new UnhandledExceptionEventHandler(Domain_UnhandledException);
        }

        private void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.WriteCritical("Domain_UnhandledException", Logger.GetDefaultProperties, e.ExceptionObject);
        }
        private Assembly Domain_TypeResolve(object sender, ResolveEventArgs args)
        {
            Logger.WriteVerbose("Domain_TypeResolve: " + args.Name);
            return null;
        }
        private Assembly Domain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            Logger.WriteVerbose("Domain_ResourceResolve: " + args.Name);
            return null;
        }
        private Assembly Domain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Logger.WriteVerbose("Domain_ReflectionOnlyAssemblyResolve: " + args.Name);
            return null;
        }
        private Assembly Domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Logger.WriteVerbose("Domain_AssemblyResolve: " + args.Name);
            return null;
        }
        private void Domain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Logger.WriteVerbose("Domain_AssemblyLoad: " + args.LoadedAssembly.FullName);
        }

        internal static void Shutdown()
        {
            _instance.ConsoleWriteLine();

            DistributedApplication.ClusterChannel.ShutDown();

            if (Instance.StartSettings.BackupIndexAtTheEnd)
            {
                if (LuceneManagerIsRunning)
                {
                    _instance.ConsoleWriteLine("Backing up the index...");
                    SenseNet.Search.Indexing.BackupTools.SynchronousBackupIndex();
                    _instance.ConsoleWriteLine("The backup of index is finished.");
                }
                else
                {
                    _instance.ConsoleWriteLine("Backing up index is skipped because Lucene was not started.");
                }
            }

            if (LuceneManagerIsRunning)
                SenseNet.Search.Indexing.LuceneManager.ShutDown();

            WaitForWriterLockFileIsReleased(WaitForLockFileType.OnEnd);

            var t = DateTime.Now - _instance._startupInfo.Starting;
            var msg = String.Format("Repository has stopped. Running time: {0}.{1:d2}:{2:d2}:{3:d2}", t.Days, t.Hours, t.Minutes, t.Seconds);

            _instance.ConsoleWriteLine(msg);
            _instance.ConsoleWriteLine();
            Logger.WriteInformation(msg);
            _instance = null;
        }

        public void ConsoleWrite(params string[] text)
        {
            if (_settings.Console == null)
                return;
            foreach (var s in text)
                _settings.Console.Write(s);
        }
        public void ConsoleWriteLine(params string[] text)
        {
            if (_settings.Console == null)
                return;
            ConsoleWrite(text);
            _settings.Console.WriteLine();
        }

        internal static bool Started()
        {
            return _instance != null;
        }

        //======================================== Wait for write.lock
        private const string WAITINGFORLOCKSTR = "write.lock exists, waiting for removal...";
        private const string WRITELOCKREMOVEERRORSUBJECTSTR = "Error at application start";
        private const string WRITELOCKREMOVEERRORTEMPLATESTR = "Write.lock was present at application start and was not removed within set timeout interval ({0} seconds) - a previous appdomain may use the index. Write.lock deletion and application start is forced. AppDomain friendlyname: {1}, base directory: {2}";
        private const string WRITELOCKREMOVEERRORONENDTEMPLATESTR = "Write.lock was present at shutdown and was not removed within set timeout interval ({0} seconds) - application exit is forced. AppDomain friendlyname: {1}, base directory: {2}";
        private const string WRITELOCKREMOVEEMAILERRORSTR = "Could not send notification email about write.lock removal. Check {0} and {1} settings in web.config!";
        public enum WaitForLockFileType { OnStart = 0, OnEnd }
        /// <summary>
        /// Waits for releasing index writer lock file in the configured index directory. Timeout: configured with IndexLockFileWaitForRemovedTimeout key.
        /// Returns true if the lock was released. Returns false if the time has expired.
        /// </summary>
        /// <returns>Returns true if the lock was released. Returns false if the time has expired.</returns>
        public static bool WaitForWriterLockFileIsReleased()
        {
            //return WaitForWriterLockFileIsReleased(StorageContext.Search.IndexDirectoryPath);
            return WaitForWriterLockFileIsReleased(IndexDirectory.CurrentDirectory);
        }
        /// <summary>
        /// Waits for releasing index writer lock file in the specified directory. Timeout: configured with IndexLockFileWaitForRemovedTimeout key.
        /// Returns true if the lock was released. Returns false if the time has expired.
        /// </summary>
        /// <returns>Returns true if the lock was released. Returns false if the time has expired.</returns>
        public static bool WaitForWriterLockFileIsReleased(string indexDirectory)
        {
            return WaitForWriterLockFileIsReleased(indexDirectory, RepositoryConfiguration.IndexLockFileWaitForRemovedTimeout);
        }
        /// <summary>
        /// Waits for releasing index writer lock file in the specified directory and timeout.
        /// Returns true if the lock was released. Returns false if the time has expired.
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <returns>Returns true if the lock was released. Returns false if the time has expired.</returns>
        public static bool WaitForWriterLockFileIsReleased(string indexDirectory, int timeout)
        {
            if (indexDirectory == null)
                return true;

            var lockFilePath = System.IO.Path.Combine(indexDirectory, Lucene.Net.Index.IndexWriter.WRITE_LOCK_NAME);
            var deadline = DateTime.Now.AddSeconds(timeout);
            while (System.IO.File.Exists(lockFilePath))
            {
                Debug.WriteLine(WAITINGFORLOCKSTR);
                Thread.Sleep(100);
                if (DateTime.Now > deadline)
                    return false;
            }
            return true;
        }        
        /// <summary>
        /// Waits for write.lock to disappear for a configured time interval. Timeout: configured with IndexLockFileWaitForRemovedTimeout key. 
        /// If timeout is exceeded an error is logged and execution continues. For errors at OnStart an email is also sent to a configured address.
        /// </summary>
        /// <param name="waitType">A parameter that influences the logged error message and email template only.</param>
        public static void WaitForWriterLockFileIsReleased(WaitForLockFileType waitType)
        {
            // check if writer.lock is still there -> if yes, wait for other appdomain to quit or lock to disappear - until a given timeout.
            // after timeout is passed, Repository.Start will deliberately attempt to remove lock file on following startup

            if (!WaitForWriterLockFileIsReleased())
            {
                // lock file was not removed by other or current appdomain for the given time interval (onstart: other appdomain might use it, onend: current appdomain did not release it yet)
                // onstart -> notify operator and start repository anyway
                // onend -> log error, and continue
                var template = waitType == WaitForLockFileType.OnEnd ? WRITELOCKREMOVEERRORONENDTEMPLATESTR : WRITELOCKREMOVEERRORTEMPLATESTR;
                Logger.WriteError(string.Format(template,
                    RepositoryConfiguration.IndexLockFileWaitForRemovedTimeout,
                    AppDomain.CurrentDomain.FriendlyName,
                    AppDomain.CurrentDomain.BaseDirectory));

                if (waitType == WaitForLockFileType.OnStart)
                    RepositoryInstance.SendWaitForLockErrorMail();
            }
        }
        private static void SendWaitForLockErrorMail()
        {
            if (!string.IsNullOrEmpty(RepositoryConfiguration.NotificationSender) && !string.IsNullOrEmpty(RepositoryConfiguration.IndexLockFileRemovedNotificationEmail))
            {
                try
                {
                    var smtpClient = new System.Net.Mail.SmtpClient();
                    var msgstr = string.Format(WRITELOCKREMOVEERRORTEMPLATESTR,
                        RepositoryConfiguration.IndexLockFileWaitForRemovedTimeout,
                        AppDomain.CurrentDomain.FriendlyName,
                        AppDomain.CurrentDomain.BaseDirectory);
                    var msg = new System.Net.Mail.MailMessage(
                        RepositoryConfiguration.NotificationSender,
                        RepositoryConfiguration.IndexLockFileRemovedNotificationEmail,
                        WRITELOCKREMOVEERRORSUBJECTSTR,
                        msgstr);
                    smtpClient.Send(msg);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }
            }
            else
            {
                Logger.WriteError(string.Format(WRITELOCKREMOVEEMAILERRORSTR,
                    RepositoryConfiguration.NotificationSenderKey,
                    RepositoryConfiguration.IndexLockFileRemovedNotificationEmailKey));
            }
        }

        //========================================

        private bool _workflowEngineIsRunning;
        private bool _notificationEngineIsRunning;

        //======================================== LuceneManager hooks

        public static bool LuceneManagerIsRunning
        {
            get
            {
                if (_instance == null)
                    throw new NotSupportedException("Querying running state of LuceneManager is not supported when RepositoryInstance is not created.");
                return SenseNet.Search.Indexing.LuceneManager.Running;
            }
        }
        public static bool IndexingPaused
        {
            get
            {
                if (_instance == null)
                    throw new NotSupportedException("Querying pausing state of LuceneManager is not supported when RepositoryInstance is not created.");
                return SenseNet.Search.Indexing.LuceneManager.Paused;
            }
        }

        internal static bool RestoreIndexOnStartup()
        {
            if (_instance == null)
                return true;
            return _instance._settings.RestoreIndex;
        }

        //======================================== Outer search engine

        public static bool ContentQueryIsAllowed
        {
            get
            {
                return StorageContext.Search.IsOuterEngineEnabled &&
                       StorageContext.Search.SearchEngine != InternalSearchEngine.Instance &&
                       RepositoryInstance.LuceneManagerIsRunning;
            }
        }

        //======================================== IDisposable
        bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
                if (disposing)
                    Shutdown();
            _disposed = true;
        }
        ~RepositoryInstance()
        {
            Dispose(false);
        }

    }
}
