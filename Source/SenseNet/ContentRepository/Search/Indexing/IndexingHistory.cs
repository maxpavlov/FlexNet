using System;
using System.Collections.Generic;
using Lucene.Net.Documents;
using Lucene.Net.Util;
using System.Diagnostics;
using Lucene.Net.Index;
using Lucene.Net.Search;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Search.Indexing
{
    internal class IndexingHistory
    {
        private object _sync = new object();

        int _limit;
        Queue<int> _queue;
        Dictionary<int, long> _storage;

        public long Count { get { return _storage.Count; } }

        public IndexingHistory()
        {
            Initialize(RepositoryConfiguration.IndexHistoryItemLimit);
        }
        private void Initialize(int size)
        {
            _limit = size;
            _queue = new Queue<int>(size);
            _storage = new Dictionary<int, long>(size);
        }

        internal int GetVersionId(Document doc)
        {
            return Int32.Parse(doc.Get(LucObject.FieldName.VersionId));
        }
        internal long GetTimestamp(Document doc)
        {
            return Int64.Parse(doc.Get(LucObject.FieldName.NodeTimestamp));
        }
        internal bool CheckForAdd(int versionId, long timestamp)
        {
            Debug.WriteLine(String.Format("##> CheckForUpdate. Id: {0}, time: {1}", versionId, timestamp));
            lock (_sync)
            {
                if (!Exists(versionId))
                {
                    Add(versionId, timestamp);
                    return true;
                }
                return false;
            }
        }
        internal bool CheckForUpdate(int versionId, long timestamp)
        {
            Debug.WriteLine(String.Format("##> CheckForUpdate. Id: {0}, time: {1}", versionId, timestamp));
            long? stored;
            lock (_sync)
            {
                stored = Get(versionId);
                if (stored == null)
                {
                    Add(versionId, timestamp);
                    return true;
                }
                else
                {
                    if (stored.Value >= timestamp)
                        return false;
                    Update(versionId, timestamp);
                    return true;
                }
            }
        }
        internal void ProcessDelete(Term[] deleteTerms)
        {
            Debug.WriteLine("##> ProcessDelete. Count: " + deleteTerms.Length);
            for (int i = 0; i < deleteTerms.Length; i++)
            {
                var term = deleteTerms[i];
                if (term.Field() != LucObject.FieldName.VersionId)
                    return;
                var versionId = NumericUtils.PrefixCodedToInt(term.Text());
                ProcessDelete(versionId);
            }
        }
        internal void ProcessDelete(int versionId)
        {
            lock (_sync)
            {
                if (!Exists(versionId))
                    Add(versionId, long.MaxValue);
                else
                    Update(versionId, long.MaxValue);
            }
        }
        internal void Remove(Term[] deleteTerms)
        {
            lock (_sync)
            {
                foreach (var deleteTerm in deleteTerms)
                {
                    var executor = new QueryExecutor20100701();
                    var q = new TermQuery(deleteTerm);
                    var lucQuery = LucQuery.Create(q);
                    lucQuery.EnableAutofilters = false;
                    var result = executor.Execute(lucQuery, true);
                    foreach (var lucObject in result)
                        _storage.Remove(lucObject.VersionId);
                }
            }
        }
        internal bool RemoveIfLast(int versionId, long? timestamp)
        {
            lock (_sync)
            {
                var last = Get(versionId);
                if (last == timestamp)
                {
                    _storage.Remove(versionId);
                    return true;
                }
                return false;
            }
        }
        internal bool CheckHistoryChange(int versionId, long timestamp)
        {
            lock (_sync)
            {
                var lastTimestamp = Get(versionId);
                return timestamp != lastTimestamp;
            }
        }

        internal bool Exists(int versionId)
        {
            return _storage.ContainsKey(versionId);
        }
        internal long? Get(int versionId)
        {
            long result;
            if (_storage.TryGetValue(versionId, out result))
                return result;
            return null;
        }
        internal void Add(int versionId, long timestamp)
        {
            _storage.Add(versionId, timestamp);
            _queue.Enqueue(versionId);
            if (_queue.Count <= _limit)
                return;
            var k = _queue.Dequeue();
            _storage.Remove(k);
        }
        internal void Update(int versionId, long timestamp)
        {
            _storage[versionId] = timestamp;
        }
    }
}
