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

    public class MissingTaskHandler
    {
        private static object _gapSync = new object();
        private static List<int> _gap = new List<int>();
        public static bool HasChanges { get; private set; }

        internal static bool RemoveTask(int taskId)
        {
            lock (_gapSync)
            {
                if (_gap.Remove(taskId))
                {
                    Debug.WriteLine(String.Format("@> {0} REMOVE FROM GAP: {1}, | {1}", AppDomain.CurrentDomain.FriendlyName, taskId));
                    HasChanges = true;
                    return true;
                }
                return false;
            }
        }
        internal static void AddTasks(int lastTaskId, int currentTaskId)
        {
            lock (_gapSync)
            {
                for (var i = lastTaskId + 1; i < currentTaskId; i++)
                    if (!_gap.Contains(i))
                        _gap.Add(i);
                if(lastTaskId + 1 < currentTaskId)
                    HasChanges = true;
            }
            Debug.WriteLine(String.Format("@> {0} Add  : task: {1}, max: {2} | {3} | gap: [{4}]", AppDomain.CurrentDomain.FriendlyName, currentTaskId, lastTaskId, currentTaskId - 1 == lastTaskId ? "ok  " : "DIFF", GetGapString()));
        }

        internal static string GetGapString()
        {
            lock (_gapSync)
                return String.Join(",", _gap);
        }
        internal static List<int> GetGap()
        {
            lock (_gapSync)
            {
Debug.WriteLine(String.Format("@> {0} GetGap. Count: {1}", AppDomain.CurrentDomain.FriendlyName, _gap.Count));
                return new List<int>(_gap);
            }
        }
        internal static void SetGap(List<int> gap)
        {
            lock (_gapSync)
            {
                _gap.Clear();
                foreach (var taskId in gap)
                {
                    if (!_gap.Contains(taskId))
                    {
                        _gap.Add(taskId);
                        HasChanges = true;
                    }
                }
            }
        }

        internal static void Committed()
        {
            lock (_gapSync)
                HasChanges = false;
        }
    }

    public static class LuceneManager
    {
        public static readonly string KeyFieldName = "VersionId";
        internal static IndexWriter _writer;
        internal static IndexReader _reader;
        internal static int _unCommitedChanges;
        internal static int _maxTaskId;

        public static int IndexCount { get { return 1; } }
        public static int IndexedDocumentCount { get { return IndexReader.NumDocs(); } }
        public static IndexReader IndexReader { get { return _reader; } }

        private static object _startSync = new object();
        private static bool _running;
        public static bool Running
        {
            get { return _running; }
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
                ActivityQueue.Instance = new ActivityQueue();
                //We can't handle cache invalidation as it would call into LuceneManager
                //we only process lucene messages
                var safeMessageTypes = new List<Type>();
                safeMessageTypes.Add(typeof(DistributedLuceneActivity.LuceneActivityDistributor));
                ClusterChannel.ProcessedMessageTypes = safeMessageTypes;

                //we positively start the message cluser
                int dummy = SenseNet.ContentRepository.DistributedApplication.Cache.Count;
                var dummy2 = SenseNet.ContentRepository.DistributedApplication.ClusterChannel;

                AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);

                if (SenseNet.ContentRepository.RepositoryInstance.RestoreIndexOnStartup())
                    BackupTools.RestoreIndex(false, consoleOut);

                CreateWriterAndReader();

                using (new SystemAccount())
                {
                    ExecuteUnprocessedIndexTasks(consoleOut);
                }

                ActivityQueue.Instance.Start();
            }
            finally
            {
                ClusterChannel.ProcessedMessageTypes = null;
            }
            //ScheduleMerger();            
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            ShutDown(false);
        }
        private static void CreateWriterAndReader()
        {
            //var directory = FSDirectory.GetDirectory(StorageContext.Search.IndexDirectoryPath, false);
            var directory = FSDirectory.GetDirectory(IndexDirectory.CurrentDirectory, false);
            _writer = new IndexWriter(directory, IndexManager.GetAnalyzer(), false);
            _reader = _writer.GetReader();
        }
        internal static void Restart()
        {
            _writer.Close();
            CreateWriterAndReader();
        }
        // Flushes and shuts down the distributed Lucene manager and disconnect index from the cluster
        public static void ShutDown()
        {
            ShutDown(true);
        }
        private static void ShutDown(bool log)
        {
            if (_reader != null)
                _reader.Close();
            if (_writer != null)
                _writer.Close();
            if(ActivityQueue.Instance != null)
                ActivityQueue.Instance.Stop();
            if (log)
                SenseNet.Diagnostics.Logger.WriteInformation("LuceneManager has stopped.");
            _running = false;
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

        internal static bool _executingUnprocessedIndexTasks = false;
        //---- Caller: Startup, ForceRestore
        private static void ExecuteUnprocessedIndexTasks(System.IO.TextWriter consoleOut)
        {
            Debug.WriteLine(String.Format("#> {0} -------- Executing unprocessed tasks.", AppDomain.CurrentDomain.FriendlyName));
            var cud = IndexManager.ReadCommitUserData(_reader);

            _maxTaskId = cud.LastTaskId;
            MissingTaskHandler.SetGap(cud.Gap);

            var tasks = IndexingTaskManager.GetUnprocessedTasks(cud.LastTaskId, cud.Gap);
            var logProps = new Dictionary<string, object> { { "LastTaskID", cud.LastTaskId }, { "Count of tasks", tasks.Count() } };
            Logger.WriteInformation("Executing unprocessed indexing tasks from the stored commit point.", Logger.EmptyCategoryList, logProps);
            if (consoleOut != null)
                consoleOut.Write("    Executing {0} unprocessed tasks ...", tasks.Count());

            if (tasks.Count() > 0)
            {
                _executingUnprocessedIndexTasks = true;
                foreach (var task in tasks)
                    IndexingTaskManager.ExecuteTaskDirect(task);
                CommitChanges();
                RefreshReader();
                _executingUnprocessedIndexTasks = false;
            }

            if (consoleOut != null)
                consoleOut.WriteLine("ok.");
            Logger.WriteInformation("Executing unprocessed tasks is finished.", Logger.EmptyCategoryList, logProps);
            Debug.WriteLine(String.Format("#> {0} -------- Executing unprocessed tasks is finished.", AppDomain.CurrentDomain.FriendlyName));
        }
        internal static void ExecuteLostIndexTasks()
        {
            Debug.WriteLine(String.Format("#> {0} -------- Executing lost tasks.", AppDomain.CurrentDomain.FriendlyName));
            var oldMaxTaskId = _maxTaskId;
            var tasks = IndexingTaskManager.GetUnprocessedTasks(_maxTaskId, MissingTaskHandler.GetGap());
            if (tasks.Count() > 0)
            {
                if (_executingUnprocessedIndexTasks) // ??
                    return;
                _executingUnprocessedIndexTasks = true;
                foreach (var task in tasks)
                    IndexingTaskManager.ExecuteTask(task, false, false);
                _executingUnprocessedIndexTasks = false;
            }
            Debug.WriteLine(String.Format("#> {0} -------- Executing lost tasks is finished. LastTaskID: {1}, LastTaskID: {2}, Count of tasks: {3}", AppDomain.CurrentDomain.FriendlyName, oldMaxTaskId, _maxTaskId, tasks.Count()));
            var logProps = new Dictionary<string, object> { { "OldLastTaskID", oldMaxTaskId }, { "LastTaskID", _maxTaskId }, { "Count of tasks", tasks.Count() } };
            Logger.WriteInformation("Executing expired indexing tasks.", Logger.EmptyCategoryList, logProps);
        }

        internal static void ForceRestore()
        {
            try
            {
                PauseIndexing();
                CommitChanges();
                //TODO: xx readert hasznalo szalakat kezelni kell!
                //if(_reader != null)
                //    _reader.Close();
                //TODO: xx writert hasznalo szalakat kezelni kell!
                //if (_writer != null)
                //    _writer.Close();
                BackupTools.RestoreIndex(true, null);
                //var directory = FSDirectory.GetDirectory(StorageContext.Search.IndexDirectoryPath, false);
                var directory = FSDirectory.GetDirectory(IndexDirectory.CurrentDirectory, false);
                _writer = new IndexWriter(directory, IndexManager.GetAnalyzer(), false);
                _reader = _writer.GetReader();
                ExecuteUnprocessedIndexTasks(null);
            }
            finally
            {
                ContinueIndexing();
            }
        }

        internal static bool IndexingPaused
        {
            get { return ActivityQueue.Instance.Paused; }
        }
        internal static void PauseIndexing()
        {
            ActivityQueue.Instance.Pause();
        }
        internal static void ContinueIndexing()
        {
            ActivityQueue.Instance.Continue();
        }

        internal static void ApplyChanges(int taskId, bool lastActivity)
        {
            if (lastActivity)
                _maxTaskId = Math.Max(_maxTaskId, taskId);

            if (MissingTaskHandler.RemoveTask(taskId))
                _unCommitedChanges++;
            if (_unCommitedChanges > 0)
            {
                CommitChanges();
                RefreshReader();
            }

            if (lastActivity)
                Debug.WriteLine(String.Format("#> {0} Apply: task: {1}, max: {2} | {3} | gap: [{4}]", AppDomain.CurrentDomain.FriendlyName, taskId, _maxTaskId, taskId == _maxTaskId ? "ok  " : "DIFF", MissingTaskHandler.GetGapString()));
        }
        internal static void CommitChanges()
        {
            using (var optrace = new OperationTrace("Commit index writer"))
            {
                if (_unCommitedChanges == 0 && MissingTaskHandler.HasChanges)
                    EnsureWriterChanged();
                _writer.Commit(IndexManager.CreateCommitUserData(_maxTaskId, MissingTaskHandler.GetGapString()));
                _unCommitedChanges = 0;
                MissingTaskHandler.Committed();
                optrace.IsSuccessful = true;
            }
        }
        private const string COMMITFIELDNAME = "$#COMMIT";
        private const string COMMITDATEFIELDNAME = "$#DATE";
        private static void EnsureWriterChanged()
        {
            var value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
            var doc = new Lucene.Net.Documents.Document();
            doc.Add(new Lucene.Net.Documents.Field(COMMITFIELDNAME, COMMITFIELDNAME, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Lucene.Net.Documents.Field(COMMITDATEFIELDNAME, value, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            LuceneManager._writer.UpdateDocument(new Term(COMMITFIELDNAME, COMMITFIELDNAME), doc);
        }

        internal static void RefreshReader()
        {
            using (var optrace = new OperationTrace("Refresh reader"))
            {
                //var reader = _reader;
                _reader = _writer.GetReader();
                //if(reader != null)
                //    reader.Close();
                optrace.IsSuccessful = true;
            }
        }
        public static void Flush(bool distributed, bool waitForComplete)
        {
            var commitNow = new CommitNowActivity();
            if (distributed)
                commitNow.Distribute();
            ActivityQueue.AddActivity(commitNow);
            if (waitForComplete)
                commitNow.WaitForComplete();
        }

        /*==================================================================== Document operations */

        internal static void AddCompleteDocument(Document document)
        {
            //SetFlagsForAdd(document);
            _writer.DeleteDocuments(GetVersionIdTerm(document));
            _writer.AddDocument(document);
            _unCommitedChanges++;
        }
        internal static void AddDocument(Document document)
        {
            SetFlagsForAdd(document);
            _writer.DeleteDocuments(GetVersionIdTerm(document));
            _writer.AddDocument(document);
            _unCommitedChanges++;
        }
        internal static void UpdateDocument(Term updateTerm, Document document)
        {
            // check if document has already been deleted (by another appdomain)
            //  - NOTE: UpdateDocumentActivity will never be executed after a RemoveDocumentActivity, since Document will be null (check Indexing_ActivitesWithMissingVersion.cs/Indexing_ActivitesWithMissingVersion)
            //  - NOTE: after uncommenting: check UpdateDocument-Commit-Refreshreader-UpdateDocument sequence, and check if second UpdateDocument works well
            //var termdocs = _reader.TermDocs(updateTerm);
            //var docexists = termdocs.Next();
            //if (!docexists)
            //    return;
            //else
            //    if (_reader.IsDeleted(termdocs.Doc()))
            //        return;

            SetFlagsForUpdate(document);
            _writer.UpdateDocument(updateTerm, document);
            _unCommitedChanges++;
        }
        internal static void DeleteDocuments(Term[] deleteTerms)
        {
            SetFlagsForDelete(deleteTerms);
            _writer.DeleteDocuments(deleteTerms);
            _unCommitedChanges++;
        }

        private static Term GetVersionIdTerm(Document doc)
        {
            var versionId = Int32.Parse(doc.Get(LucObject.FieldName.VersionId));
            return new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId));
        }

        //-------------------------------------------------------------------- flag setting

        internal class DocumentVersionComparer : IComparer<Document>
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
        private static void SetFlagsForUpdate(Document document)
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
        private static void SetFlagsForDelete(Term deleteTerm)
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
                        _writer.UpdateDocument(delTerm, dirtyVersion.Document);
                        _unCommitedChanges++;
                        break;
                    }
                }
            }
        }

        public static List<Document> GetDocumentsByNodeId(int nodeId)
        {
            var termDocs = IndexReader.TermDocs(new Term(LucObject.FieldName.NodeId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(nodeId)));
            return GetDocumentsFromTermDocs(termDocs);
        }
        private static List<Document> GetDocumentsByNodeId(string nodeId)
        {
            var termDocs = IndexReader.TermDocs(new Term(LucObject.FieldName.NodeId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(Int32.Parse(nodeId))));
            return GetDocumentsFromTermDocs(termDocs);
        }
        internal static List<Document> GetDocumentByVersionId(int versionId)
        {
            var termDocs = IndexReader.TermDocs(new Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId)));
            return GetDocumentsFromTermDocs(termDocs);
        }
        private static List<Document> GetDocumentsFromTermDocs(TermDocs termDocs)
        {
            var docs = new List<Document>();
            var reader = IndexReader;
            while (termDocs.Next())
                docs.Add(reader.Document(termDocs.Doc()));
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
        private static void TraceInfoList(List<VersionInfo> infoList)
        {
            Trace.WriteLine("@#$>InfoList. Count: " + infoList.Count);
            Trace.WriteLine("@#$>   Actual\tVer\tVid\t|\tLD\tLP\tM\tP\t|\tLD\tLP\tM\tP");
            Trace.WriteLine("@#$>   -----------------");
            foreach (var info in infoList)
            {
                Trace.WriteLine(String.Format("@#$>   {0}\t{1}\t{2}\t|\t{3}\t{4}\t{5}\t{6}\t|\t{7}\t{8}\t{9}\t{10}",
                    info.IsActualDocument,
                    info.Version,
                    info.VersionId,
                    info.OriginalIsLastDraft,
                    info.OriginalIsLastPublic,
                    info.OriginalIsMajor,
                    info.OriginalIsPublic,
                    info.ExpectedIsLastDraft,
                    info.ExpectedIsLastPublic,
                    info.ExpectedIsMajor,
                    info.ExpectedIsPublic));
            }
        }
    }
}