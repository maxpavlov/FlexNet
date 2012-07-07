using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.Virtualization;
using System.IO;
using System.Text.RegularExpressions;
using SenseNet.ContentRepository.Storage.Events;
using System.Configuration;
using SenseNet.Diagnostics;
using System.Threading;
using SenseNet.ContentRepository.Storage.Security;
using System.Web.Hosting;

namespace SenseNet.Portal.UI
{
    internal class SNScriptDependencyCache
    {
        private SortedDictionary<string, IEnumerable<string>> _depCache;
        private ReaderWriterLockSlim _depCacheLock;

        private static readonly string _usingStr = "using";
        private static readonly string _resourceStr = "resource";
        private static readonly string _resourcePath = "/Resources.ashx?class=";


        public IEnumerable<string> GetDependencies(string path)
        {
            if (ConfigurationManager.AppSettings["UseScriptDependencyCache"] == "true")
            {
                try
                {
                    _depCacheLock.TryEnterUpgradeableReadLock(LockHandler.DefaultLockTimeOut);
                    if (!_depCache.ContainsKey(path))
                    {
                        var deps = ReadDependencies(path) ?? new List<string>();

                        try
                        {
                            _depCacheLock.TryEnterWriteLock(LockHandler.DefaultLockTimeOut);
                            _depCache.Add(path, deps);
                        }
                        finally
                        {
                            if (_depCacheLock.IsWriteLockHeld)
                                _depCacheLock.ExitWriteLock();
                        }
                    }
                    if (_depCache.ContainsKey(path))
                        return _depCache[path];
                }
                finally
                {
                    if (_depCacheLock.IsUpgradeableReadLockHeld)
                        _depCacheLock.ExitUpgradeableReadLock();
                }
            }

            return ReadDependencies(path) ?? new List<string>();
        }

        private static IEnumerable<string> ReadDependencies(string path)
        {
            // read dependencies for .js files only
            if (!path.ToLower().EndsWith(".js"))
                return new List<string>();

            try
            {
                var deps = new List<string>();
                using (var str = VirtualPathProvider.OpenFile(path))
                using (var r = new StreamReader(str))
                {
                    var l = r.ReadLine();
                    var parsedDependency = ParseDependency(l);
                    while (parsedDependency != null)
                    {
                        deps.Add(parsedDependency);
                        l = r.ReadLine();
                        parsedDependency = ParseDependency(l);
                    }
                }
                return deps;
            }
            catch (Exception e)
            {
                Logger.WriteException(e);
            }

            return null;
        }

        private static string ParseDependency(string line)
        {
            string path = null;

            if (line == null)
                return null;
            
            if (line.StartsWith("/// <depends"))
            {
                // old way: /// <depends path="$skin/scripts/jquery/jquery.js" />
                var startidx = line.IndexOf('"');
                var endidx = line.LastIndexOf('"');
                path = line.Substring(startidx + 1, endidx - startidx - 1);
            }
            else if (line.StartsWith("//"))
            {
                // new way:
                var linePart = line.Substring(2).Trim();
                if (linePart.StartsWith(_usingStr))
                {
                    // // using $skin/scripts/jquery/jquery.js
                    path = linePart.Substring(_usingStr.Length).Trim();
                }
                else if (linePart.StartsWith(_resourceStr))
                {
                    // // resource UserBrowse
                    var className = linePart.Substring(_resourceStr.Length).Trim();
                    path = _resourcePath + className;
                }
            }

            return path;
        }

        #region Singleton instantiation

        private SNScriptDependencyCache()
        {
            _depCache = new SortedDictionary<string, IEnumerable<string>>();
            _depCacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        private static readonly SNScriptDependencyCache _instance = new SNScriptDependencyCache();

        public static SNScriptDependencyCache Instance
        {
            get { return _instance; }
        }

        #endregion

        #region nodeobserver handlers

        internal void RemovePath(string path)
        {
            try
            {
                _depCacheLock.TryEnterWriteLock(LockHandler.DefaultLockTimeOut);
                _depCache.Remove(path);
            }
            finally
            {
                if (_depCacheLock.IsWriteLockHeld)
                    _depCacheLock.ExitWriteLock();
            }
        }

        internal void UpdateDeps(string path)
        {
            try
            {
                _depCacheLock.TryEnterUpgradeableReadLock(LockHandler.DefaultLockTimeOut);
                if (_depCache.ContainsKey(path))
                {
                    try
                    {
                        _depCacheLock.TryEnterWriteLock(LockHandler.DefaultLockTimeOut);
                        _depCache[path] = ReadDependencies(path);
                    }
                    finally
                    {
                        if (_depCacheLock.IsWriteLockHeld)
                            _depCacheLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                if (_depCacheLock.IsUpgradeableReadLockHeld)
                    _depCacheLock.ExitUpgradeableReadLock();
            }
        }

        #endregion
    }

    //FIXME
    //Do not forget to prime the nodeobserver before framework activation!

    internal class ScriptDependencyObserver : NodeObserver
    {
        protected override void OnNodeModified(object sender, NodeEventArgs e)
        {
            //renamed?
            if (!string.Equals(e.OriginalSourcePath, e.SourceNode.Path, StringComparison.InvariantCulture))
                SNScriptDependencyCache.Instance.RemovePath(e.OriginalSourcePath);
            else
                SNScriptDependencyCache.Instance.UpdateDeps(e.SourceNode.Path);
        }

        protected override void OnNodeMoved(object sender, NodeOperationEventArgs e)
        {
            SNScriptDependencyCache.Instance.RemovePath(e.OriginalSourcePath);
        }

        protected override void OnNodeDeleted(object sender, NodeEventArgs e)
        {
            SNScriptDependencyCache.Instance.RemovePath(e.SourceNode.Path);
        }
    }
}
