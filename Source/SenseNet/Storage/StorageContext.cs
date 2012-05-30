using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Data;
using System.Configuration;
using System.Diagnostics;
using SenseNet.Diagnostics;

namespace SenseNet.ContentRepository.Storage
{
    public interface IL2Cache
    {
        object Get(string key);
        void Set(string key, object value);
        void Clear();
    }
    internal class NullL2Cache : IL2Cache
    {
        public object Get(string key) { return null; }
        public void Set(string key, object value) { return; }
        public void Clear()
        {
            // Do nothing
        }
    }
    public class IndexDirectory
    {
        private static readonly string DEFAULTDIRECTORYNAME = "0";

        public static bool Exists
        {
            get { return CurrentDirectory != null; }
        }
        public static string CurrentOrDefaultDirectory
        {
            get
            {
                if (CurrentDirectory != null)
                    return CurrentDirectory;
                var path = System.IO.Path.Combine(StorageContext.Search.IndexDirectoryPath, DEFAULTDIRECTORYNAME);
                System.IO.Directory.CreateDirectory(path);
                Reset();
                return CurrentDirectory;
            }
        }
        public static string CurrentDirectory
        {
            get { return Instance.CurrentDirectoryPrivate; }
        }
        public static string CreateNew()
        {
            var name = DateTime.Now.ToString("yyyyMMddHHmmss");
            var path = System.IO.Path.Combine(StorageContext.Search.IndexDirectoryPath, name);
            System.IO.Directory.CreateDirectory(path);
Debug.WriteLine(String.Format("@> {0} -------- new index directory: {1}", AppDomain.CurrentDomain.FriendlyName, path));
            return path;
        }
        public static void Reset()
        {
Debug.WriteLine(String.Format("@> {0} -------- IndexDirectory reset", AppDomain.CurrentDomain.FriendlyName));
            Instance._currentDirDone = false;
            Instance._currentDirectory = null;
        }
        public static void RemoveUnnecessaryDirectories()
        {
            var root = StorageContext.Search.IndexDirectoryPath;
            if (!System.IO.Directory.Exists(root))
                return;
            var unnecessaryDirs = System.IO.Directory.GetDirectories(root)
                .Where(a => Char.IsDigit(System.IO.Path.GetFileName(a)[0]))
                .OrderByDescending(s => s)
                .Skip(2).Where(x => Deletable(x));
            foreach (var dir in unnecessaryDirs)
            {
                try
                {
                    System.IO.Directory.Delete(dir, true);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(String.Concat("Cannot delete the directory: ", dir, ", ", e.Message));
                    Logger.WriteWarning("Cannot delete the directory: " + dir, Logger.EmptyCategoryList, new Dictionary<string, object> { { "Reason", e.Message }, { "StackTrace", e.StackTrace } });
                }
            }
        }
        private static bool Deletable(string path)
        {
            var time = new System.IO.DirectoryInfo(path).CreationTime;
            if (time.AddMinutes(10) < DateTime.Now)
                return true;
            return false;
        }

        //==================================================================================

        private IndexDirectory() { }
        private static object _sync = new object();
        private static IndexDirectory _instance;
        private static IndexDirectory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                            _instance = new IndexDirectory();
                    }
                }
                return _instance;
            }
        }

        //==================================================================================

        private string _currentDirectory;
        private bool _currentDirDone;
        private string CurrentDirectoryPrivate
        {
            get
            {
                if (!_currentDirDone)
                {
                    _currentDirectory = GetCurrentDirectory();
                    _currentDirDone = true;
                }
                return _currentDirectory;
            }
        }
        private string GetCurrentDirectory()
        {
            var root = StorageContext.Search.IndexDirectoryPath;
            var rootExists = System.IO.Directory.Exists(root);
            string path = null;
            if (rootExists)
            {
                EnsureFirstDirectory(root);
                path = System.IO.Directory.GetDirectories(root)
                    .Where(a => Char.IsDigit(System.IO.Path.GetFileName(a)[0]))
                    .OrderBy(s => s)
                    .LastOrDefault();
            }
            Debug.WriteLine(String.Format("@> {0} -------- GetCurrentDirectory: {1}", AppDomain.CurrentDomain.FriendlyName, (path ?? "[null]")));
            return path;
        }
        private void EnsureFirstDirectory(string root)
        {
            // backward compatibility: move files to new subdirectory (name = '0')
            var files = System.IO.Directory.GetFiles(root);
            if (files.Length == 0)
                return;
            var firstDir = System.IO.Path.Combine(root, DEFAULTDIRECTORYNAME);
            Debug.WriteLine("@> new index directory: " + firstDir + " copy files.");
            System.IO.Directory.CreateDirectory(firstDir);
            foreach (var file in files)
                System.IO.File.Move(file, System.IO.Path.Combine(firstDir, System.IO.Path.GetFileName(file)));
        }
    }

    public class StorageContext
    {
        public static class Search
        {
            public static ISearchEngine SearchEngine
            {
                get { return Instance.GetSearchEnginePrivate(); }
            }

            public static bool IsOuterEngineEnabled
            {
                get { return Instance.IsOuterEngineEnabled; }
            }
            public static string IndexDirectoryPath
            {
                get { return Instance.IndexDirectoryPath; }
            }
            public static string IndexDirectoryBackupPath
            {
                get { return Instance.IndexDirectoryBackupPath; }
            }
            public static string IndexLockFilePath
            {
                //get { return System.IO.Path.Combine(StorageContext.Search.IndexDirectoryPath, "write.lock"); }
                get { return IndexDirectory.Exists ? System.IO.Path.Combine(IndexDirectory.CurrentDirectory, "write.lock") : null; }
            }
            public static void EnableOuterEngine()
            {
                if (false == RepositoryConfiguration.IsOuterSearchEngineEnabled)
                    throw new InvalidOperationException("Indexing is not allowed in the configuration");
                Instance.IsOuterEngineEnabled = true;
            }
            public static void DisableOuterEngine()
            {
                Instance.IsOuterEngineEnabled = false;
            }

            public static void SetIndexDirectoryPath(string path)
            {
                Instance.IndexDirectoryPath = path;
            }

            public static IndexDocumentData LoadIndexDocumentByVersionId(int versionId)
            {
                return DataProvider.LoadIndexDocument(versionId);
            }
            public static IEnumerable<IndexDocumentData> LoadIndexDocumentByVersionId(IEnumerable<int> versionId)
            {
                return DataProvider.LoadIndexDocument(versionId);
            }
            public static IEnumerable<IndexDocumentData> LoadIndexDocumentsByPath(string path)
            {
                return DataProvider.LoadIndexDocumentsByPath(path);
            }

            public static int[] DefaultTopAndGrowth
            {
                get { return Instance.DefaultTopAndGrowth; }
                internal set { Instance.DefaultTopAndGrowth = value; } // for tests
            }
        }

        private static IL2Cache _l2Cache = new NullL2Cache();
        public static IL2Cache L2Cache
        {
            get { return _l2Cache; }
            set { _l2Cache = value; }
        }


        //========================================================================== Singleton model

        private static StorageContext instance;
        private static object instanceLock=new object();

        private static StorageContext Instance
        {
            get
            {
                if (instance != null)
                    return instance;
                lock (instanceLock)
                {
                    if (instance != null)
                        return instance;
                    instance = new StorageContext();
                    instance.Initialize();
                    return instance;
                }
            }
        }

        private StorageContext() { }

        //========================================================================== Thread safe initialization block

        private void Initialize()
        {
            _searchEngine = CreateSearchEnginePrivate();
        }
        private ISearchEngine CreateSearchEnginePrivate()
        {
            return TypeHandler.ResolveProvider<ISearchEngine>() ?? InternalSearchEngine.Instance;
        }

        //========================================================================== Private interface

        private bool? __isOuterEngineEnabled;
        private bool IsOuterEngineEnabled
        {
            get
            {
                if (__isOuterEngineEnabled == null)
                    __isOuterEngineEnabled = RepositoryConfiguration.IsOuterSearchEngineEnabled;
                return (__isOuterEngineEnabled.Value);
            }
            set
            {
                __isOuterEngineEnabled = value;
            }
        }

        private string __indexDirectoryPath;
        private string IndexDirectoryPath
        {
            get
            {
                if (__indexDirectoryPath == null)
                    __indexDirectoryPath = RepositoryConfiguration.IndexDirectoryPath;
                return __indexDirectoryPath;
            }
            set
            {
                __indexDirectoryPath = value;
            }
        }

        private string __indexDirectoryBackupPath;
        private string IndexDirectoryBackupPath
        {
            get
            {
                if (__indexDirectoryBackupPath == null)
                    __indexDirectoryBackupPath = RepositoryConfiguration.IndexDirectoryBackupPath;
                return __indexDirectoryBackupPath;
            }
            set
            {
                __indexDirectoryBackupPath = value;
            }
        }

        private ISearchEngine _searchEngine;
        private ISearchEngine GetSearchEnginePrivate()
        {
            if (IsOuterEngineEnabled)
                return _searchEngine;
            return InternalSearchEngine.Instance;
        }

        private static readonly string DefaultTopAndGrowthKey = "DefaultTopAndGrowth";
        private static int[] _defaultDefaultTopAndGrowth = new[] { 100, 1000, 10000, 0 };
        private int[] _defaultTopAndGrowth;
        private int[] DefaultTopAndGrowth
        {
            get
            {
                if (_defaultTopAndGrowth == null)
                {
                    var value = ConfigurationManager.AppSettings[DefaultTopAndGrowthKey];
                    _defaultTopAndGrowth = String.IsNullOrEmpty(value) ? _defaultDefaultTopAndGrowth : ParseDefaultTopAndGrowth(value);
                }
                return _defaultTopAndGrowth;
            }
            set
            {
                _defaultTopAndGrowth = value;
            }
        }
        private int[] ParseDefaultTopAndGrowth(string value)
        {
            var items = value.Split(',');
            var values = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                var last = i == items.Length - 1;
                var parsedInt = 0;
                if (Int32.TryParse(items[i], out parsedInt))
                    values[i] = parsedInt;
                else
                    throw new SenseNet.ContentRepository.Storage.Data.ConfigurationException("Invalid sequence in the value of 'DefaultTopAndGrowth'. Every value can be positive integer except last, it can be positive integer or zero.");

                if (parsedInt < 0)
                    throw new SenseNet.ContentRepository.Storage.Data.ConfigurationException("Invalid sequence in the value of 'DefaultTopAndGrowth'. A value cannot less than 0.");

                if (parsedInt == 0)
                {
                    if(!last)
                        throw new SenseNet.ContentRepository.Storage.Data.ConfigurationException("Invalid sequence in the value of 'DefaultTopAndGrowth'. Only the last value can be 0.");
                }
                else
                {
                    if (i > 0 && parsedInt <= values[i - 1])
                        throw new SenseNet.ContentRepository.Storage.Data.ConfigurationException("Invalid sequence in the value of 'DefaultTopAndGrowth'. The sequence must be monotonically increasing. Last value can be greater than any other or zero.");
                }
            }
            return values;
        }
    }
}
