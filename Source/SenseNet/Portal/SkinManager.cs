using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.IO;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.Portal.Virtualization;
using System.Web.Configuration;
using System.Threading;
using SenseNet.ContentRepository.Storage.Security;
using System.Diagnostics;

namespace SenseNet.Portal
{
    public sealed class SkinManager
    {
        /* ================================================================== Members */
        // slightly higher memory consumption in favor of extendability and query speed
        private SortedDictionary<string, SortedDictionary<string, string>> _skinMap;
        private ReaderWriterLockSlim _skinMapLock;

        private static SkinManager _instance;
        public static readonly string skinPrefix = "$skin/";


        /* ================================================================== Static Methods */
        public static Node GetCurrentSkin()
        {
            if (PortalContext.Current != null)
            {
                if (PortalContext.Current.Page != null)
                {
                    if (PortalContext.Current.Page.PageSkin != null)
                        return PortalContext.Current.Page.PageSkin;
                    if (PortalContext.Current.ContextWorkspace != null && PortalContext.Current.ContextWorkspace.WorkspaceSkin != null)
                        return PortalContext.Current.ContextWorkspace.WorkspaceSkin;
                    if (PortalContext.Current.Site.SiteSkin != null)
                        return PortalContext.Current.Site.SiteSkin;
                }
            }

            var path = RepositoryPath.Combine(Repository.SkinRootFolderPath, ConfigurationManager.AppSettings["DefaultSkinName"]);
            return Node.LoadNode(path);
        }
        public static string GetCurrentSkinName()
        {
            var skin = GetCurrentSkin();
            if (skin == null)
                return string.Empty;

            return skin.Name;
        }
        public static string Resolve(string relpath)
        {
            return Instance.ResolvePath(relpath, GetCurrentSkinName());
        }
        public static bool TryResolve(string relpath, out string resolvedpath)
        {
            return Instance.TryResolvePath(relpath, GetCurrentSkinName(), out resolvedpath);
        }
        public static bool IsNotSkinRelativePath(string path)
        {
            return !(path.StartsWith(skinPrefix));
        }
        public static string TrimSkinPrefix(string relPath)
        {
            return relPath.Remove(0, skinPrefix.Length);
        }


        /* ================================================================== Methods */
        public string ResolvePath(string relpath, string skinname)
        {
            return ResolvePath(relpath, skinname, true);
        }
        public bool TryResolvePath(string relpath, string skinname, out string resolvedpath)
        {
            resolvedpath = ResolvePath(relpath, skinname, false);
            return !string.IsNullOrEmpty(resolvedpath);
        }


        /* ================================================================== Private Methods */
        private string ResolvePath(string relpath, string skinname, bool fallbackToRoot)
        {
            // absolute path is given: no fallback, no check
            if (IsNotSkinRelativePath(relpath))
                return relpath;

            var skinRelPath = TrimSkinPrefix(relpath);
            if (!string.IsNullOrEmpty(skinname))
            {
                try
                {
                    _skinMapLock.TryEnterReadLock(LockHandler.DefaultLockTimeOut);
                    if (_skinMap.ContainsKey(skinname))
                    {
                        var current = _skinMap[skinname];

                        if (current.ContainsKey(skinRelPath))
                        {
                            var resolved = current[skinRelPath];

                            if (!string.IsNullOrEmpty(resolved))
                                return resolved;
                        }
                    }
                }
                finally
                {
                    if (_skinMapLock.IsReadLockHeld)
                        _skinMapLock.ExitReadLock();
                }
            }

            // if fallback to root is not requested
            if (!fallbackToRoot)
                return string.Empty;

            return RepositoryPath.Combine(Repository.SkinGlobalFolderPath, skinRelPath);
        }
        private void ReadSkinStructure()
        {
            //CONDITIONAL EXECUTE
            Node[] nodes;
            if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
            {
                var query = new NodeQuery();
                query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, Repository.SkinRootFolderPath));
                query.Add(new TypeExpression(NodeType.GetByName("Skin")));
                nodes = query.Execute().Nodes.ToArray();
            }
            else
            {
                nodes = NodeQuery.QueryNodesByTypeAndPath(NodeType.GetByName("Skin"), false, Repository.SkinRootFolderPath, false).Nodes.ToArray();
            }

            try
            {
                _skinMapLock.TryEnterWriteLock(LockHandler.DefaultLockTimeOut);
                foreach (Node n in nodes)
                    _skinMap.Add(n.Name, MapSkin(n));
            }
            finally
            {
                if (_skinMapLock.IsWriteLockHeld)
                    _skinMapLock.ExitWriteLock();
            }
        }
        private static SortedDictionary<string, string> MapSkin(Node skin)
        {
            //CONDITIONAL EXECUTE
            NodeQueryResult result;
            if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
            {
                var query = new NodeQuery();
                query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, skin.Path));
                result = query.Execute();
            }
            else
            {
                result = NodeQuery.QueryNodesByPath(skin.Path, false);
            }

            var dict = new SortedDictionary<string, string>();
            foreach (Node n in result.Nodes)
            {
                if (n.Id != skin.Id)
                {
                    var relpath = n.Path.Substring(skin.Path.Length + 1);
                    if (!dict.ContainsKey(relpath))
                        dict.Add(relpath, n.Path);
                }
            }

            return dict;
        }


        /* ================================================================== Singleton instantiation */
        private SkinManager() { }
        private static object _startSync = new object();
        private static bool _initialized;
        public static SkinManager Instance
        {
            get
            {
                if (!_initialized)
                {
                    lock (_startSync)
                    {
                        if (!_initialized)
                        {
                            _instance = new SkinManager();
                            _instance._skinMap = new SortedDictionary<string, SortedDictionary<string, string>>();
                            _instance._skinMapLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
                            _instance.ReadSkinStructure();
                            _initialized = true;
                        }
                    }
                }
                return _instance;
            }
        }


        /* ================================================================== Singleton instantiation */
        internal void AddToMap(string fullPath)
        {
            var s = SplitPath(fullPath);
            if (s == null)
                return;

            try
            {
                _skinMapLock.TryEnterUpgradeableReadLock(LockHandler.DefaultLockTimeOut);
                if (_skinMap.ContainsKey(s[0]))
                {
                    var localskinMap = _skinMap[s[0]];
                    if (localskinMap != null)
                    {
                        if (s.Length > 1 && !string.IsNullOrEmpty(s[1]) && !localskinMap.ContainsKey(s[1]))
                        {
                            try
                            {
                                _skinMapLock.TryEnterWriteLock(LockHandler.DefaultLockTimeOut);
                                localskinMap.Add(s[1], fullPath);
                            }
                            finally
                            {
                                if (_skinMapLock.IsWriteLockHeld)
                                    _skinMapLock.ExitWriteLock();
                            }
                        }
                    }
                }
                else
                {
                    if (s.Length < 2 || string.IsNullOrEmpty(s[1]))
                    {
                        var n = Node.LoadNode(fullPath);
                        if (n.NodeType.IsInstaceOfOrDerivedFrom("Skin"))
                        {
                            try
                            {
                                _skinMapLock.TryEnterWriteLock(LockHandler.DefaultLockTimeOut);
                                _skinMap.Add(s[0], MapSkin(n));
                            }
                            finally
                            {
                                if (_skinMapLock.IsWriteLockHeld)
                                    _skinMapLock.ExitWriteLock();
                            }
                        }
                    }
                }
            }
            finally
            {
                if (_skinMapLock.IsUpgradeableReadLockHeld)
                    _skinMapLock.ExitUpgradeableReadLock();
            }
        }
        internal void RemoveFromMap(string fullPath)
        {
            var s = SplitPath(fullPath);
            if (s == null)
                return;

            try
            {
                _skinMapLock.TryEnterUpgradeableReadLock(LockHandler.DefaultLockTimeOut);
                if (_skinMap.ContainsKey(s[0]))
                {
                    var skinMap = _skinMap[s[0]];
                    if (skinMap != null)
                    {
                        try
                        {
                            _skinMapLock.TryEnterWriteLock(LockHandler.DefaultLockTimeOut);
                            if (!string.IsNullOrEmpty(s[1]))
                                skinMap.Remove(s[1]);
                            else
                                _skinMap.Remove(s[0]);
                        }
                        finally
                        {
                            if (_skinMapLock.IsWriteLockHeld)
                                _skinMapLock.ExitWriteLock();
                        }
                    }
                }
            }
            finally
            {
                if (_skinMapLock.IsUpgradeableReadLockHeld)
                    _skinMapLock.ExitUpgradeableReadLock();
            }
        }
        private static string[] SplitPath(string fullPath)
        {
            if (!fullPath.StartsWith(Repository.SkinRootFolderPath))
                throw new InvalidOperationException("Skin update system called for non-skin path " + fullPath);

            if (fullPath.Length <= Repository.SkinRootFolderPath.Length + 1)
                return null;

            var rippedPath = fullPath.Substring(Repository.SkinRootFolderPath.Length + 1);

            var splitPath = rippedPath.Split(new char[] { '/' }, 2);

            return splitPath;
        }
    }

    internal class SkinObserver : NodeObserver
    {
        public static readonly string SkinStartPath = string.Concat(Repository.SkinRootFolderPath, RepositoryPath.PathSeparator);

        protected override void OnNodeCreated(object sender, NodeEventArgs e)
        {
            base.OnNodeCreated(sender, e);

            if (e.SourceNode.Path.StartsWith(SkinObserver.SkinStartPath))
                SkinManager.Instance.AddToMap(e.SourceNode.Path);
        }
        protected override void OnNodeDeleted(object sender, NodeEventArgs e)
        {
            base.OnNodeDeleted(sender, e);

            if (e.SourceNode.Path.StartsWith(SkinObserver.SkinStartPath))
                SkinManager.Instance.RemoveFromMap(e.SourceNode.Path);
        }
        protected override void OnNodeDeletedPhysically(object sender, NodeEventArgs e)
        {
            base.OnNodeDeletedPhysically(sender, e);

            if (e.SourceNode.Path.StartsWith(SkinObserver.SkinStartPath))
                SkinManager.Instance.RemoveFromMap(e.SourceNode.Path);
        }
        protected override void OnNodeCopied(object sender, NodeOperationEventArgs e)
        {
            base.OnNodeCopied(sender, e);

            if (e.TargetNode.Path.StartsWith(SkinObserver.SkinStartPath))
            {
                var targetPath = RepositoryPath.Combine(e.TargetNode.Path, e.SourceNode.Name);
                SkinManager.Instance.AddToMap(targetPath);
            }
        }
        protected override void OnNodeMoved(object sender, NodeOperationEventArgs e)
        {
            base.OnNodeMoved(sender, e);

            if (e.OriginalSourcePath.StartsWith(SkinObserver.SkinStartPath))
                SkinManager.Instance.RemoveFromMap(e.OriginalSourcePath);
            if (e.TargetNode.Path.StartsWith(SkinObserver.SkinStartPath))
            {
                var targetPath = RepositoryPath.Combine(e.TargetNode.Path, e.SourceNode.Name);
                SkinManager.Instance.AddToMap(targetPath);
            }
        }
        protected override void OnNodeModified(object sender, NodeEventArgs e)
        {
            base.OnNodeModified(sender, e);

            // renamed?
            if (!string.Equals(e.OriginalSourcePath, e.SourceNode.Path, StringComparison.InvariantCulture))
            {
                if (e.OriginalSourcePath.StartsWith(SkinObserver.SkinStartPath))
                    SkinManager.Instance.RemoveFromMap(e.OriginalSourcePath);
                if (e.SourceNode.Path.StartsWith(SkinObserver.SkinStartPath))
                    SkinManager.Instance.AddToMap(e.SourceNode.Path);
            }

        }
    }
}
