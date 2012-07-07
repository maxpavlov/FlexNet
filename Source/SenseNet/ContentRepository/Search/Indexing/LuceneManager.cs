using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Utilities;
using SenseNet.ContentRepository.Storage.Data;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage;
using System.Threading;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using SenseNet.Diagnostics;
using SenseNet.Communication.Messaging;
//using SenseNet.ContentRepository;
using SenseNet.Search.Indexing.Activities;
using SenseNet.Search.Indexing.Configuration;
using Lucene.Net.Util;

namespace SenseNet.Search.Indexing
{
    public enum CommitHint { AddNew, AddNewVersion, Update, Rename, Move, Delete } ;

    public static class LuceneManager
    {
        public static readonly Lucene.Net.Util.Version LuceneVersion = Lucene.Net.Util.Version.LUCENE_29;

        internal static IndexingHistory _history = new IndexingHistory();

        /* =========================================================================== TEST COMMIT */
        // This is a code for commit testing. 
        // Put <add key="CommitLogPath" value="c:\\commitlog-1-{0}.csv"/> in the webconfig, name should reflect the actual web node (use "1" for 1st node, "2" for 2nd node, etc.)
        private static bool? _commitLogEnabled;
        private static object CommitLogEnabledSync = new object();
        private static bool CommitLogEnabled
        {
            get
            {
                if (!_commitLogEnabled.HasValue)
                {
                    lock (CommitLogEnabledSync)
                    {
                        if (!_commitLogEnabled.HasValue)
                        {
                            _commitLogEnabled = StartCommitLog();
                        }
                    }
                }
                return _commitLogEnabled.Value;
            }
        }
        private static object CommitLogSync = new object();
        private static System.Timers.Timer CommitLogTimer;
        private static Stopwatch _commitStopper;
        private static string CommitLogPath;
        private static string CommitLog;
        private static void WriteCommitLog(string status, int? gapSize = null, int? gapSizePeak = null, int? activities = null, int? delaycycle = null, string gapString = null, int? maxActivityId = null)
        {
            if (!CommitLogEnabled)
                return;

            long elapsed = 0;
            if (status == "Start" && _commitStopper != null)
                _commitStopper.Restart();
            if ((status == "Committed" || status == "Stop") && _commitStopper != null)
            {
                elapsed = _commitStopper.ElapsedMilliseconds;
                _commitStopper.Restart();
            }

            var line = DateTime.Now.Ticks.ToString() + ";" + DateTime.Now.Hour.ToString() + ";" + DateTime.Now.Minute.ToString() + ";" + DateTime.Now.Second.ToString() + ";";
            if (status == "Info")
            {
                line += status + ";0;" + gapSize.Value.ToString() + ";" + gapSizePeak.Value.ToString() + ";" + activities.Value.ToString() + ";" + delaycycle.Value.ToString() + ";;;" + Environment.NewLine;
            }
            else
            {
                line += status + ";" + elapsed + ";0;0;0;0;" + (maxActivityId.HasValue ? maxActivityId.Value.ToString() : string.Empty) + ";" + gapString + ";" + Environment.NewLine;
            }
            lock (CommitLogSync)
            {
                CommitLog += line;
            }
        }
        private static bool StartCommitLog()
        {
            // eg  "c:\\commitlog-1-{0}.csv"
            var appSettingsCommitLogPath = System.Configuration.ConfigurationManager.AppSettings["CommitLogPath"];
            if (string.IsNullOrEmpty(appSettingsCommitLogPath))
                return false;

            CommitLogPath = string.Format(appSettingsCommitLogPath, DateTime.Now.ToString("yyyyMMddhhmmss"));
            using (var fs = new System.IO.FileStream(CommitLogPath, System.IO.FileMode.Create))
            {
                using (var wr = new System.IO.StreamWriter(fs))
                {
                    wr.WriteLine("ticks;hour;minute;second;status;elapsed;gapsize;gapsizepeak;activities;delaycycle;maxActivityId;gapString");
                }
            }
            _commitStopper = Stopwatch.StartNew();
            CommitLog = string.Empty;
            CommitLogTimer = new System.Timers.Timer(10000.0);
            CommitLogTimer.Elapsed += new System.Timers.ElapsedEventHandler(CommitLogTimer_Elapsed);
            CommitLogTimer.Start();
            return true;
        }
        private static void CommitLogTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (CommitLogSync)
            {
                using (var writer = new System.IO.StreamWriter(CommitLogPath, true))
                {
                    writer.Write(CommitLog);
                }
                CommitLog = string.Empty;
            }
        }
        // =========================================================================== TEST COMMIT END


        public static readonly string KeyFieldName = "VersionId";
        internal static IndexWriter _writer;
        internal static IndexReader _reader;

        public static int IndexCount { get { return 1; } }
        public static int IndexedDocumentCount
        {
            get
            {
                using (var readerFrame = LuceneManager.GetIndexReaderFrame())
                {
                    var idxReader = readerFrame.IndexReader;
                    return idxReader.NumDocs();
                }
            }
        }

        //UNDONE: xxx delete this and all usage
        internal static ReaderWriterLockSlim _writerRestartLock = new ReaderWriterLockSlim();    // protects against writer.close
        private const int REOPENRETRYMAX = 2;
        internal static IndexReader IndexReader
        {
            get
            {
                //_writerRestartLock.EnterReadLock();
                //try
                //{
                //    if (!_reader.IsCurrent())
                //        ReopenReader();
                //}
                //finally
                //{
                //    _writerRestartLock.ExitReadLock();
                //}
                using (var wrFrame = IndexWriterFrame.Get(false)) // // IndexReader getter
                {
                    if (!_reader.IsCurrent())
                        ReopenReader();
                }
                return _reader;
            }
        }
        public static IndexReaderFrame GetIndexReaderFrame()
        {
            return new IndexReaderFrame(IndexReader);
        }

        private static object _startSync = new object();
        private static bool _running;
        public static bool Running
        {
            get { return _running; }
        }

        private static bool _paused;
        internal static bool Paused
        {
            get { return _paused; }
        }

        internal static void PauseIndexing()
        {
            if (!_running)
                _paused = true;
        }
        internal static void ContinueIndexing()
        {
            if (!_running)
                _paused = false;
        }


        [Obsolete("Use Start(System.IO.TextWriter) instead.")]
        public static void Start()
        {
            Start(null);
        }
        public static void Start(System.IO.TextWriter consoleOut)
        {
            if (!_running)
            {
                lock (_startSync)
                {
                    if (!_running)
                    {
                        Startup(consoleOut);
                        _running = true;
                    }
                }
            }
        }
        private static void Startup(System.IO.TextWriter consoleOut)
        {
            try
            {
                //We can't handle cache invalidation as it would call into LuceneManager
                //we only process lucene messages
                var safeMessageTypes = new List<Type>();
                safeMessageTypes.Add(typeof(DistributedLuceneActivity.LuceneActivityDistributor));
                ClusterChannel.ProcessedMessageTypes = safeMessageTypes;

                //we positively start the message cluster
                int dummy = SenseNet.ContentRepository.DistributedApplication.Cache.Count;
                var dummy2 = SenseNet.ContentRepository.DistributedApplication.ClusterChannel;

                if (SenseNet.ContentRepository.RepositoryInstance.RestoreIndexOnStartup())
                    BackupTools.RestoreIndex(false, consoleOut);

                CreateWriterAndReader();

                using (new SystemAccount())
                    ExecuteUnprocessedIndexingActivities(consoleOut);

                Warmup();

                var commitStart = new ThreadStart(CommitWorker);
                new Thread(commitStart).Start();
            }
            finally
            {
                ClusterChannel.ProcessedMessageTypes = null;
            }
        }

        private static void CreateWriterAndReader()
        {
            var directory = FSDirectory.Open(new System.IO.DirectoryInfo(IndexDirectory.CurrentDirectory));

            _writer = new IndexWriter(directory, IndexManager.GetAnalyzer(), false);//UNDONE: xx: ez obsolete, a következő sor kellene, de akkor elpattan ez a teszt: Msmq_SendLargeIndexDocument: "Assert.IsTrue failed. Lucene activity was not processed within 3 seconds"
            //_writer = new IndexWriter(directory, IndexManager.GetAnalyzer(), false, IndexWriter.MaxFieldLength.UNLIMITED);

            _writer.SetMaxMergeDocs(RepositoryConfiguration.LuceneMaxMergeDocs);
            _writer.SetMergeFactor(RepositoryConfiguration.LuceneMergeFactor);
            _writer.SetRAMBufferSizeMB(RepositoryConfiguration.LuceneRAMBufferSizeMB);
            _reader = _writer.GetReader();
        }

        //========================================================================================== Start, Restart, Shutdown, Warmup

        internal static void Restart()
        {
            Debug.WriteLine("##> LUCENEMANAGER RESTART");

            //_writerRestartLock.EnterWriteLock();
            //try
            //{
            //    _writer.Close();
            //    CreateWriterAndReader();
            //}
            //finally
            //{
            //    _writerRestartLock.ExitWriteLock();
            //}
            using (var wrFrame = IndexWriterFrame.Get(true)) // // Restart
            {
                _writer.Close();
                CreateWriterAndReader();
            }
        }
        public static void ShutDown()
        {
            ShutDown(true);
        }
        private static void ShutDown(bool log)
        {
            if (!_running)
                return;

            Debug.WriteLine("##> LUCENEMANAGER SHUTDOWN");

            if (_writer != null)
            {
                _stopCommitWorker = true;
                lock (_commitLock)
                    Commit(false);

                //_writerRestartLock.EnterWriteLock();
                //try
                //{
                //    if(_reader != null)
                //        _reader.Close();
                //    if (_writer != null)
                //        _writer.Close();
                //}
                //finally
                //{
                //    _running = false;
                //    _writerRestartLock.ExitWriteLock();
                //}
                using (var wrFrame = IndexWriterFrame.Get(true)) // // ShutDown
                {
                    if (_reader != null)
                        _reader.Close();
                    if (_writer != null)
                        _writer.Close();
                    _running = false;
                }
            }

            if (log)
                Logger.WriteInformation("LuceneManager has stopped. Max task id: " + MissingActivityHandler.MaxActivityId);
        }
        public static void Backup()
        {
            BackupTools.SynchronousBackupIndex();
        }
        public static void BackupAndShutDown()
        {
            ShutDown();
            BackupTools.BackupIndexImmediatelly();
        }

        private static readonly int ACTIVITIESFRAGMENTSIZE = 100;
        internal static object _executingUnprocessedIndexingActivitiesLock = new object();
        private static bool _executingUnprocessedIndexingActivities;
        //---- Caller: Startup, ForceRestore
        internal static void ExecuteUnprocessedIndexingActivities(System.IO.TextWriter consoleOut)
        {
            lock (_executingUnprocessedIndexingActivitiesLock)
            {
                try
                {
                    _executingUnprocessedIndexingActivities = true;

                    CommitUserData cud;
                    using (var readerFrame = LuceneManager.GetIndexReaderFrame())
                    {
                        cud = IndexManager.ReadCommitUserData(readerFrame.IndexReader);
                    }
                    MissingActivityHandler.MaxActivityId = cud.LastActivityId;
                    MissingActivityHandler.SetGap(cud.Gap);

                    var logProps = new Dictionary<string, object> { { "LastActivityID", cud.LastActivityId }, { "Size of gap", cud.Gap.Count } };
                    Logger.WriteInformation("Executing unprocessed indexing activities from the stored commit point.",
                                            Logger.EmptyCategoryList, logProps);

                    var i = 0;
                    var sumCount = 0;

                    // This loop was created to avoid loading too many activities at once that are present in the gap.
                    while (i * ACTIVITIESFRAGMENTSIZE <= cud.Gap.Count)
                    {
                        // get activities from the DB that are in the current gap fragment
                        var gapSegment = cud.Gap.Skip(i * ACTIVITIESFRAGMENTSIZE).Take(ACTIVITIESFRAGMENTSIZE).ToArray();
                        var activities = IndexingActivityManager.GetUnprocessedActivities(gapSegment);
                        ProcessTasks(activities, consoleOut);
                        sumCount += activities.Length;
                        i++;
                    }

                    // Execute activities where activity id is bigger than than our last (activity) task id
                    var maxIdInDb = 0;
                    var newtasks = IndexingActivityManager.GetUnprocessedActivities(MissingActivityHandler.MaxActivityId, out maxIdInDb, ACTIVITIESFRAGMENTSIZE);
                    while (newtasks.Length > 0)
                    {
                        ProcessTasks(newtasks, consoleOut);
                        sumCount += newtasks.Length;

                        //load the remaining activities, but only if they were created before this operation started
                        var tempMax = 0;
                        newtasks = IndexingActivityManager.GetUnprocessedActivities(MissingActivityHandler.MaxActivityId, out tempMax, ACTIVITIESFRAGMENTSIZE, maxIdInDb);
                    }

                    if (consoleOut != null)
                        consoleOut.WriteLine("ok.");

                    logProps.Add("Processed tasks", sumCount);

                    //write the latest max activity id and gap size to log
                    logProps["LastActivityID"] = MissingActivityHandler.MaxActivityId;
                    logProps["Size of gap"] = MissingActivityHandler.GetGap().Count;

                    Logger.WriteInformation("Executing unprocessed tasks is finished.", Logger.EmptyCategoryList, logProps);
                }
                finally
                {
                    _executingUnprocessedIndexingActivities = false;
                }
            }
        }
        private static void ProcessTasks(IndexingActivity[] activities, System.IO.TextWriter consoleOut)
        {
            if (consoleOut != null && activities.Length != 0)
                consoleOut.Write("    Executing {0} unprocessed tasks ...", activities.Length);

            foreach (var activity in activities)
            {
                activity.FromHealthMonitor = true;  // make sure adddocument will delete first
                IndexingActivityManager.ExecuteActivity(activity, false, false);
            }
        }
        internal static void ExecuteLostIndexingActivities()
        {
            lock (_executingUnprocessedIndexingActivitiesLock)
            {
                var gap = MissingActivityHandler.GetOldestGapAndMoveToNext();

                var i = 0;
                while (i * ACTIVITIESFRAGMENTSIZE < gap.Length)
                {
                    var gapSegment = gap.Skip(i * ACTIVITIESFRAGMENTSIZE).Take(ACTIVITIESFRAGMENTSIZE).ToArray();

                    var activities = IndexingActivityManager.GetUnprocessedActivities(gapSegment);
                    if (activities.Length > 0)
                    {
                        foreach (var act in activities)
                        {
                            act.FromHealthMonitor = true;
                            IndexingActivityManager.ExecuteActivity(act, false, false);
                        }
                    }
                    i++;
                }
            }
        }

        internal static void ForceRestore()
        {
            try
            {
                PauseIndexing();
                ApplyChanges();
                BackupTools.RestoreIndex(true, null);

                var directory = FSDirectory.Open(new System.IO.DirectoryInfo(IndexDirectory.CurrentDirectory));
                //_writerRestartLock.EnterWriteLock();
                //try
                //{
                //    _writer = new IndexWriter(directory, IndexManager.GetAnalyzer(), false);
                //    _reader = _writer.GetReader();
                //}
                //finally
                //{
                //    _writerRestartLock.ExitWriteLock();
                //}
                using (var wrFrame = IndexWriterFrame.Get(true)) // // ForceRestore
                {
                    _writer = new IndexWriter(directory, IndexManager.GetAnalyzer(), false);
                    _reader = _writer.GetReader();
                }

                ExecuteUnprocessedIndexingActivities(null);
            }
            finally
            {
                ContinueIndexing();
            }
        }

        internal static void Warmup()
        {
            var idList = ((LuceneSearchEngine) StorageContext.Search.SearchEngine).Execute("+Id:1");
        }

        /*========================================================================================== Commit */

        internal static void ApplyChanges(int activityId)
        {
            MissingActivityHandler.RemoveActivityAndAddGap(activityId);
            ApplyChanges();
        }

        internal static void ApplyChanges()
        {
            //compiler warning here is not a problem, Interlocked 
            //class can work with a volatile variable
            Interlocked.Increment(ref _activities);
        }
        internal static void CommitOrDelay()
        {
            // set gapsize perfcounters
            MissingActivityHandler.SetGapSizeCounter();

            var act = _activities;
            if (act == 0 && _delayCycle == 0)
                return;

            if (act < 2)
            {
                WriteCommitLog("Start");
                Commit();
                WriteCommitLog("Stop");
            }
            else
            {
                _delayCycle++;
                if (_delayCycle > RepositoryConfiguration.DelayedCommitCycleMaxCount)
                {
                    WriteCommitLog("Start");
                    Commit();
                    WriteCommitLog("Stop");
                }
            }

            Interlocked.Exchange(ref _activities, 0);
        }

        internal static void Commit(bool reopenReader = true)
        {
            var gapData = MissingActivityHandler.GetGapString();
            var gapString = gapData.Item1;
            var maxActivityId = gapData.Item2;

            //_writerRestartLock.EnterReadLock();
            //try
            //{
            //    _writer.Commit(IndexManager.CreateCommitUserData(maxActivityId, gapString));
            //    if (reopenReader)
            //        ReopenReader();
            //}
            //finally
            //{
            //    _writerRestartLock.ExitReadLock();
            //}
            using (var wrFrame = IndexWriterFrame.Get(false)) // // Commit
            {
                _writer.Commit(IndexManager.CreateCommitUserData(maxActivityId, gapString));
                if (reopenReader)
                    ReopenReader();
            }

            //in case of shutdown, reopen is not needed
            WriteCommitLog("Committed", null, null, null, null, gapString, maxActivityId);

            Interlocked.Exchange(ref _activities, 0);
            _delayCycle = 0;
        }
        internal static void ReopenReader()
        {
            var retry = 0;
            Exception e = null;
            while (retry++ < REOPENRETRYMAX)
            {
                try
                {
                    Debug.WriteLine("##> REOPEN " + (retry > 1 ? retry.ToString() : ""));

                    _reader = _writer.GetReader();
                    return;
                }
                catch (AlreadyClosedException ace)
                {
                    e = ace;
                    Thread.Sleep(100);
                }
            }
            if (e != null)
                throw new ApplicationException(String.Concat("Indexwriter is closed after ", retry, " attempt."), e);
        }

        private static volatile int _activities;          //committer thread sets 0 other threads increment
        private static volatile int _delayCycle;          //committer thread uses

        private static bool _stopCommitWorker;
        private static object _commitLock = new object();
        private static void CommitWorker()
        {
            int wait = (int)(RepositoryConfiguration.CommitDelayInSeconds * 1000.0);
            for (; ; )
            {
                // check if commit worker instructed to stop
                if (_stopCommitWorker)
                {
                    _stopCommitWorker = false;
                    return;
                }

                try
                {
                    lock (_commitLock)
                    {
                        CommitOrDelay();
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex);
                }
                Thread.Sleep(wait);
            }
        }

        /*==================================================================== Document operations */

        internal static void RefreshDocument(int versionId)
        {
            // the optimistic overlapping detection algorithm here is tested in IndexingHistory_Fix**Overlap tests. change tests if this algorithm is changed.

            var node = Node.LoadNodeByVersionId(versionId);
            
            // if node exists, refresh index from db
            // if node does not exist, remove from index
            if (node != null)
            {
                // delete from indexhistory first, because we are trying to refresh to the last timestamp
                // also check if there is a more fresh indexing pending which would surely correct the index, and we don't need this here
                if (!_history.RemoveIfLast(versionId, node.NodeTimestamp))
                    return;

                StorageContext.Search.SearchEngine.GetPopulator().RefreshIndex(node, false);
            }
            else
            {
                var delTerm = new Term(LuceneManager.KeyFieldName, NumericUtils.IntToPrefixCoded(versionId));
                LuceneManager.DeleteDocuments(new[] { delTerm }, false);
            }
        }

        /*-----------------------------------------------------------------------------------------*/

        internal static void AddCompleteDocument(Document document)
        {
            // the optimistic overlapping detection algorithm here is tested in IndexingHistory_Fix**Overlap tests. change tests if this algorithm is changed.

            var versionId = _history.GetVersionId(document);
            var timestamp = _history.GetTimestamp(document);

            if (!_history.CheckForAdd(versionId, timestamp))
                return;
            //_writerRestartLock.EnterReadLock();
            //try
            //{
            //    //in case the system was shut down in the meantime
            //    if (!LuceneManager.Running)
            //        return;
            //    if (_executingUnprocessedIndexingActivities)
            //        _writer.DeleteDocuments(GetVersionIdTerm(versionId));
            //    _writer.AddDocument(document);
            //}
            //finally
            //{
            //    _writerRestartLock.ExitReadLock();
            //}
            using (var wrFrame = IndexWriterFrame.Get(false)) // // AddCompleteDocument
            {
                if (!LuceneManager.Running)
                    return;
                if (_executingUnprocessedIndexingActivities)
                    wrFrame.IndexWriter.DeleteDocuments(GetVersionIdTerm(versionId));
                wrFrame.IndexWriter.AddDocument(document);
            }

            // check if indexing has interfered with other indexing activity for the same versionid
            if (_history.CheckHistoryChange(versionId, timestamp))
                RefreshDocument(versionId);
        }
        internal static void AddDocument(Document document)
        {
            // the optimistic overlapping detection algorithm here is tested in IndexingHistory_Fix**Overlap tests. change tests if this algorithm is changed.

            var versionId = _history.GetVersionId(document);
            var timestamp = _history.GetTimestamp(document);

            if (!_history.CheckForAdd(versionId, timestamp))
                return;

            SetFlagsForAdd(document);
            //_writerRestartLock.EnterReadLock();
            //try
            //{
            //    //in case the system was shut down in the meantime
            //    if (!LuceneManager.Running)
            //        return;
            //    if (_executingUnprocessedIndexingActivities)
            //        _writer.DeleteDocuments(GetVersionIdTerm(versionId));
            //    _writer.AddDocument(document);
            //}
            //finally
            //{
            //    _writerRestartLock.ExitReadLock();
            //}
            using (var wrFrame = IndexWriterFrame.Get(false)) // // AddDocument
            {
                if (!LuceneManager.Running)
                    return;
                if (_executingUnprocessedIndexingActivities)
                    _writer.DeleteDocuments(GetVersionIdTerm(versionId));
                _writer.AddDocument(document);
            }

            // check if indexing has interfered with other indexing activity for the same versionid
            if (_history.CheckHistoryChange(versionId, timestamp))
                RefreshDocument(versionId);
        }
        internal static void UpdateDocument(Term updateTerm, Document document)
        {
            // the optimistic overlapping detection algorithm here is tested in IndexingHistory_Fix**Overlap tests. change tests if this algorithm is changed.

            var versionId = _history.GetVersionId(document);
            var timestamp = _history.GetTimestamp(document);

            if (!_history.CheckForUpdate(versionId, timestamp))
                return;

            SetFlagsForUpdate(document);
            //_writerRestartLock.EnterReadLock();
            //try
            //{
            //    //in case the system was shut down in the meantime
            //    if (!LuceneManager.Running)
            //        return;

            //    _writer.UpdateDocument(updateTerm, document);
            //}
            //finally
            //{
            //    _writerRestartLock.ExitReadLock();
            //}
            using (var wrFrame = IndexWriterFrame.Get(false)) // // UpdateDocument
            {
                //in case the system was shut down in the meantime
                if (!LuceneManager.Running)
                    return;
                _writer.UpdateDocument(updateTerm, document);
            }

            // check if indexing has interfered with other indexing activity for the same versionid
            if (_history.CheckHistoryChange(versionId, timestamp))
                RefreshDocument(versionId);
        }
        internal static void DeleteDocuments(Term[] deleteTerms, bool moveOrRename)
        {
            // the optimistic overlapping detection algorithm here is tested in IndexingHistory_Fix**Overlap tests. change tests if this algorithm is changed.

            if (moveOrRename)
                _history.Remove(deleteTerms);
            else
                _history.ProcessDelete(deleteTerms);

            SetFlagsForDelete(deleteTerms);
            //_writerRestartLock.EnterReadLock();
            //try
            //{
            //    //in case the system was shut down in the meantime
            //    if (!LuceneManager.Running)
            //        return;

            //    _writer.DeleteDocuments(deleteTerms);
            //}
            //finally
            //{
            //    _writerRestartLock.ExitReadLock();
            //}
            using (var wrFrame = IndexWriterFrame.Get(false)) // // DeleteDocuments
            {
                //in case the system was shut down in the meantime
                if (!LuceneManager.Running)
                    return;

                _writer.DeleteDocuments(deleteTerms);
            }

            // don't need to check if indexing interfered here. If it did, change is detected in overlapped adddocument/updatedocument, and refresh (re-delete) is called there.
            // deletedocuments will never detect change in index, since it sets timestamp in indexhistory to maxvalue.
        }

        private static Term GetVersionIdTerm(Document doc)
        {
            return GetVersionIdTerm(Int32.Parse(doc.Get(LucObject.FieldName.VersionId)));
        }
        private static Term GetVersionIdTerm(int versionId)
        {
            return new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId));
        }

        //-------------------------------------------------------------------- flag setting

        private class DocumentVersionComparer : IComparer<Document>
        {
            public int Compare(Document x, Document y)
            {
                var vx = x.Get("Version").Substring(1);
                var vxa = vx.Split('.');
                var vy = y.Get("Version").Substring(1);
                var vya = vy.Split('.');

                var vxma = Int32.Parse(vxa[0]);
                var vyma = Int32.Parse(vya[0]);
                var dxma = vxma.CompareTo(vyma);
                if (dxma != 0)
                    return dxma;

                var vxmi = Int32.Parse(vxa[1]);
                var vymi = Int32.Parse(vya[1]);
                var dxmi = vxmi.CompareTo(vymi);
                if (dxmi != 0)
                    return dxmi;
                return vxa[2].CompareTo(vya[2]);
            }
        }
        private class VersionInfo
        {
            public Document Document;
            public bool IsActualDocument;
            public string Version;
            public int VersionId;
            public bool OriginalIsMajor;
            public bool OriginalIsPublic;
            public bool OriginalIsLastDraft;
            public bool OriginalIsLastPublic;
            public bool ExpectedIsMajor;
            public bool ExpectedIsPublic;
            public bool ExpectedIsLastDraft;
            public bool ExpectedIsLastPublic;
        }

        private static void SetFlagsForAdd(Document document)
        {
            VersionInfo currentInfo;
            var infoList = GetAllVersionInfo(document, out currentInfo);
            UpdateDirtyDocuments(infoList);
            SetDocumentFlags(currentInfo);
        }
        internal static void SetFlagsForUpdate(Document document)
        {
            VersionInfo currentInfo;
            var infoList = GetAllVersionInfo(document, out currentInfo);
            UpdateDirtyDocuments(infoList);
            SetDocumentFlags(currentInfo);
        }
        private static void SetFlagsForDelete(Term[] deleteTerms)
        {
            foreach (var deleteTerm in deleteTerms)
                SetFlagsForDelete(deleteTerm);
        }
        internal static void SetFlagsForDelete(Term deleteTerm)
        {
            if(deleteTerm.Field() != LucObject.FieldName.VersionId)
                return;

            var versionId = NumericUtils.PrefixCodedToInt(deleteTerm.Text());
            var infoList = GetAllVersionInfoAfterDeleteVersion(versionId);
            UpdateDirtyDocuments(infoList);
        }

        private static List<VersionInfo> GetAllVersionInfo(Document document, out VersionInfo currentInfo)
        {
            //-- create current VersionInfo
            var versionstring = document.Get(LucObject.FieldName.Version);
            var version = VersionNumber.Parse(versionstring);
            var isPublic = version.Status == VersionStatus.Approved;
            currentInfo = new VersionInfo
            {
                Document = document,
                IsActualDocument = true,
                Version = versionstring,
                VersionId = Int32.Parse(document.Get(LucObject.FieldName.VersionId)),
                ExpectedIsMajor = version.IsMajor,
                ExpectedIsPublic = isPublic,
            };

            //-- create original list
            var infoList = GetOriginalVersionInfoList(document);

            //-- search existing VersionInfo
            var existingIndex = -1;
            VersionInfo existingInfo = null;
            for (int i = 0; i < infoList.Count; i++)
            {
                if (infoList[i].VersionId == currentInfo.VersionId)
                {
                    existingIndex = i;
                    existingInfo = infoList[i];
                    break;
                }
            }

            //-- positioning the current info
            if (existingInfo == null)
            {
                infoList.Add(currentInfo);
            }
            else
            {
                infoList[existingIndex] = currentInfo;
                currentInfo.OriginalIsMajor = existingInfo.OriginalIsMajor;
                currentInfo.OriginalIsPublic = existingInfo.OriginalIsPublic;
                currentInfo.OriginalIsLastPublic = existingInfo.OriginalIsLastPublic;
                currentInfo.OriginalIsLastDraft = existingInfo.OriginalIsLastDraft;
            }

            //-- set expected flags
            SetExpectedFlags(infoList);

            return infoList;
        }
        private static List<VersionInfo> GetAllVersionInfoAfterDeleteVersion(int versionId)
        {
            var d = GetDocumentByVersionId(versionId);
            if (d.Count == 0)
                //throw new ArgumentException("Lucene Index does not contain any documents by versionId " + versionId);
                return new List<VersionInfo>(0);

            var document = d[0];

            //-- create original list
            var infoList = GetOriginalVersionInfoList(document);

            //-- remove the existing VersionInfo
            var existingIndex = -1;
            for (int i = 0; i < infoList.Count; i++)
            {
                if (infoList[i].VersionId == versionId)
                {
                    existingIndex = i;
                    break;
                }
            }
            infoList.RemoveAt(existingIndex);

            //-- set expected flags
            SetExpectedFlags(infoList);

            return infoList;
        }
        private static List<VersionInfo> GetOriginalVersionInfoList(Document document)
        {
            var nodeId = document.Get(LucObject.FieldName.NodeId);
            var docs = GetDocumentsByNodeId(nodeId);
            var infoArray = new VersionInfo[docs.Count];
            for (int i = 0; i < docs.Count; i++)
            {
                var doc = docs[i];
                var versionstring = doc.Get(LucObject.FieldName.Version);
                var version = VersionNumber.Parse(versionstring);
                var isPublic = version.Status == VersionStatus.Approved;
                var info = new VersionInfo
                {
                    Document = doc,
                    IsActualDocument = false,
                    Version = versionstring,
                    VersionId = Int32.Parse(doc.Get(LucObject.FieldName.VersionId)),
                    OriginalIsMajor = doc.Get(LucObject.FieldName.IsMajor) == BooleanIndexHandler.YES,
                    OriginalIsPublic = doc.Get(LucObject.FieldName.IsPublic) == BooleanIndexHandler.YES,
                    OriginalIsLastPublic = doc.Get(LucObject.FieldName.IsLastPublic) == BooleanIndexHandler.YES,
                    OriginalIsLastDraft = doc.Get(LucObject.FieldName.IsLastDraft) == BooleanIndexHandler.YES,
                    ExpectedIsMajor = version.IsMajor,
                    ExpectedIsPublic = isPublic,
                };
                infoArray[i] = info;
            }
            return infoArray.ToList();
        }
        private static void SetExpectedFlags(List<VersionInfo> infoList)
        {
            if (infoList.Count == 0)
                return;

            //-- reset ExpectedIsLastDraft, ExpectedIsLastPublic flags
            foreach (var info in infoList)
            {
                info.ExpectedIsLastDraft = false;
                info.ExpectedIsLastPublic = false;
            }

            //-- set ExpectedIsLastDraft flag
            infoList.Last().ExpectedIsLastDraft = true;

            //-- set ExpectedIsLastPublic flag
            for (int i = infoList.Count - 1; i >= 0; i--)
            {
                var info = infoList[i];
                if (info.ExpectedIsPublic)
                {
                    info.ExpectedIsLastPublic = true;
                    break;
                }
            }
        }

        private static void SetDocumentFlags(VersionInfo info)
        {
            var doc = info.Document;
            doc.RemoveField(LucObject.FieldName.IsMajor);
            doc.RemoveField(LucObject.FieldName.IsPublic);
            doc.RemoveField(LucObject.FieldName.IsLastPublic);
            doc.RemoveField(LucObject.FieldName.IsLastDraft);
            SetDocumentFlag(doc, LucObject.FieldName.IsMajor, info.ExpectedIsMajor);
            SetDocumentFlag(doc, LucObject.FieldName.IsPublic, info.ExpectedIsPublic);
            SetDocumentFlag(doc, LucObject.FieldName.IsLastPublic, info.ExpectedIsLastPublic);
            SetDocumentFlag(doc, LucObject.FieldName.IsLastDraft, info.ExpectedIsLastDraft);
        }
        internal static void SetDocumentFlag(Document doc, string fieldName, bool value)
        {
            doc.Add(new Field(fieldName, value ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
        }
        private static void UpdateDirtyDocuments(List<VersionInfo> infoList)
        {
            //-- select dirty documents
            var dirtyVersions = infoList.Where(i => !i.IsActualDocument &&
                    (i.OriginalIsPublic != i.ExpectedIsPublic || i.OriginalIsMajor != i.ExpectedIsMajor ||
                    i.OriginalIsLastDraft != i.ExpectedIsLastDraft || i.OriginalIsLastPublic != i.ExpectedIsLastPublic)).ToArray();

            //-- play dirty documents 
            var docs = IndexDocumentInfo.GetDocuments(dirtyVersions.Select(d => d.VersionId));
            foreach (var doc in docs)
            {
                var versionId = Int32.Parse(doc.Get(LucObject.FieldName.VersionId));
                foreach (var dirtyVersion in dirtyVersions)
                {
                    if (dirtyVersion.VersionId == versionId)
                    {
                        dirtyVersion.Document = doc;
                        SetDocumentFlags(dirtyVersion);
                        var delTerm = new Term(KeyFieldName, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(dirtyVersion.VersionId));

                        //_writerRestartLock.EnterReadLock();
                        //try
                        //{
                        //    //in case the system was shut down in the meantime
                        //    if (!LuceneManager.Running)
                        //        return;

                        //    _writer.UpdateDocument(delTerm, dirtyVersion.Document);
                        //}
                        //finally
                        //{
                        //    _writerRestartLock.ExitReadLock();
                        //}
                        using (var wrFrame = IndexWriterFrame.Get(false)) // // UpdateDirtyDocuments
                        {
                            //in case the system was shut down in the meantime
                            if (!LuceneManager.Running)
                                return;

                            _writer.UpdateDocument(delTerm, dirtyVersion.Document);
                        }

                        break;
                    }
                }
            }
        }

        public static List<Document> GetDocumentsByNodeId(int nodeId)
        {
            using (var readerFrame = LuceneManager.GetIndexReaderFrame())
            {
                var termDocs = readerFrame.IndexReader.TermDocs(new Term(LucObject.FieldName.NodeId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(nodeId)));
                return GetDocumentsFromTermDocs(termDocs, readerFrame);
            }
        }
        private static List<Document> GetDocumentsByNodeId(string nodeId)
        {
            using (var readerFrame = LuceneManager.GetIndexReaderFrame())
            {
                var termDocs = readerFrame.IndexReader.TermDocs(new Term(LucObject.FieldName.NodeId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(Int32.Parse(nodeId))));
                return GetDocumentsFromTermDocs(termDocs, readerFrame);
            }
        }
        internal static List<Document> GetDocumentByVersionId(int versionId)
        {
            using (var readerFrame = LuceneManager.GetIndexReaderFrame())
            {
                var termDocs = readerFrame.IndexReader.TermDocs(new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId)));
                return GetDocumentsFromTermDocs(termDocs, readerFrame);
            }
        }
        private static List<Document> GetDocumentsFromTermDocs(TermDocs termDocs, IndexReaderFrame readerFrame)
        {
            var docs = new List<Document>();
            while (termDocs.Next())
                docs.Add(readerFrame.IndexReader.Document(termDocs.Doc()));
            docs.Sort(new DocumentVersionComparer());
            return docs;
        }

        internal static string TraceDoc(Document doc)
        {
            return string.Format("{0}, {1}, {2}, LD:{3}, LP:{4}, M:{5}, P:{6}",
                doc.Get(LucObject.FieldName.Path),
                doc.Get(LucObject.FieldName.Version),
                doc.Get(LucObject.FieldName.VersionId),
                doc.Get(LucObject.FieldName.IsLastDraft) ?? "[null]",
                doc.Get(LucObject.FieldName.IsLastPublic) ?? "[null]",
                doc.Get(LucObject.FieldName.IsMajor) ?? "[null]",
                doc.Get(LucObject.FieldName.IsPublic) ?? "[null]");
        }
    }
}
