using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.AppModel;
using SenseNet.ContentRepository.Storage;
using System.Reflection;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository
{
    internal class ApplicationCache : IApplicationCache
    {
        //========================================================== IApplicationCache Members

        public IEnumerable<string> GetPaths(string appTypeName)
        {
            if (locked)
                return empty;
            if (ActiveSchema.NodeTypes["ApplicationCacheFile"] == null)
                return empty;

            var file = LoadCacheFile(appTypeName);
            return file.CachedData;
        }
        public void Invalidate(string appTypeName, string path)
        {
            if (locked)
                return;
            Logger.WriteVerbose("ApplicationCache is invalidated.", Logger.EmptyCategoryList, new Dictionary<string, object> { { "AppTypeName", appTypeName }, { "Path", path } });
            var cachePath = RepositoryPath.Combine(PersistentAppCacheFolderPath, appTypeName);
            var cacheFile = Node.Load<ApplicationCacheFile>(cachePath);
            if (cacheFile != null)
            {
                locked = true;
                cacheFile.ForceDelete();
                locked = false;
            }
        }

        //==========================================================

        private static readonly string PersistentAppCacheFolderPath;
        private static readonly string PersistentAppCacheFolderName;
        private bool locked;
        string[] empty = new string[0];

        static ApplicationCache()
        {
            PersistentAppCacheFolderName = "AppCache";
            PersistentAppCacheFolderPath = "/Root/System/AppCache";
        }

        internal ApplicationCacheFile LoadCacheFile(string appTypeName)
        {
            var cachePath = RepositoryPath.Combine(PersistentAppCacheFolderPath, appTypeName);
            var cacheFile = Node.Load<ApplicationCacheFile>(cachePath);
            if (cacheFile != null)
                return cacheFile;
            locked = true;
            cacheFile = CreateApplicationCache(appTypeName);
            locked = false;
            return cacheFile;
        }
        private ApplicationCacheFile CreateApplicationCache(string appTypeName)
        {
            var cacheFile = new ApplicationCacheFile(GetAppCacheRoot());
            cacheFile.Name = appTypeName;
            cacheFile.Binary.SetStream(Tools.GetStreamFromString(BuildCacheData(appTypeName)));
            cacheFile.Save();
            cacheFile = Node.Load<ApplicationCacheFile>(cacheFile.Id);
            return cacheFile;
        }
        private Node GetAppCacheRoot()
        {
            var node = Node.LoadNode(PersistentAppCacheFolderPath);
            if (node != null)
                return node;
            var folder = new SystemFolder(Repository.SystemFolder);
            folder.Name = PersistentAppCacheFolderName;
            //folder.AddReference("ContentTypes", ContentType.GetByName("ApplicationCacheFile"));
            folder.Save();
            return folder;
        }
        private string BuildCacheData(string appTypeName)
        {
            var data = new StringBuilder();
            foreach (string path in SearchData(appTypeName))
                data.AppendLine(path);
            return data.ToString();
        }
        private List<string> SearchData(string appTypeName)
        {
            //CONDITIONAL EXECUTE
            NodeQueryResult result;
            if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
            {
                var q = new NodeQuery(new TypeExpression(ActiveSchema.NodeTypes["Folder"]),
                    new StringExpression(StringAttribute.Name, StringOperator.Equal, appTypeName));
                result = q.Execute();
            }
            else
            {
                result = NodeQuery.QueryNodesByTypeAndPathAndName(ActiveSchema.NodeTypes["Folder"], false, null, false, appTypeName);
            }
            var data = new List<string>();
            foreach (var node in result.Nodes)
                foreach (var node1 in NodeEnumerator.GetNodes(node.Path).Where(n => n.Id != node.Id).OrderBy(x => x.Name))
                    data.Add(node1.Path);

            Logger.WriteVerbose("ApplicationCache is created.", Logger.EmptyCategoryList, new Dictionary<string, object> { { "AppTypeName", appTypeName }, { "Count", data.Count } });
            return data;
        }
    }
}
