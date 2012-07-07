using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.Diagnostics;
using System.Diagnostics;
using SenseNet.Search.Indexing;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Search
{
    internal class CommitUserData
    {
        public int LastActivityId;
        public List<int> Gap;
    }

    internal class IndexManager
    {
        internal static readonly string LastActivityIdKey = "LastActivityId";
        internal static readonly string MissingActivitiesKey = "MissingActivities";

        static IndexManager()
        {
            IndexWriter.SetDefaultWriteLockTimeout(20 * 60 * 1000); // 20 minutes
        }

        internal static IndexWriter GetIndexWriter(bool createNew)
        {
            Directory dir = FSDirectory.GetDirectory(SenseNet.ContentRepository.Storage.IndexDirectory.CurrentOrDefaultDirectory, createNew);
            return new IndexWriter(dir, GetAnalyzer(), createNew, IndexWriter.MaxFieldLength.UNLIMITED);
            //return LuceneManager.CreateIndexWriter(FSDirectory.Open(new System.IO.DirectoryInfo(IndexDirectory.CurrentOrDefaultDirectory)), createNew);
        }

        internal static Analyzer GetAnalyzer()
        {
            //var masterAnalyzer = new PerFieldAnalyzerWrapper(new KeywordAnalyzer());
            ////TODO: Lucene_FullText2 is failed with new WhitespaceAnalyzer
            ////masterAnalyzer.AddAnalyzer(LucObject.FieldName.AllText, new WhitespaceAnalyzer());
            //masterAnalyzer.AddAnalyzer(LucObject.FieldName.AllText, new StandardAnalyzer());
            //return masterAnalyzer;

            //  Field          Analyzer
            //  -----------------------------------------------------------------
            //  Name           Lucene.Net.Analysis.KeywordAnalyzer
            //  Path           Lucene.Net.Analysis.KeywordAnalyzer
            //  Keywords       Lucene.Net.Analysis.StopAnalyzer
            //  _Text          Lucene.Net.Analysis.Standard.StandardAnalyzer
            //  -----------------------------------------------------------------
            //  Default        Lucene.Net.Analysis.WhitespaceAnalyzer

            var masterAnalyzer = new PerFieldAnalyzerWrapper(new KeywordAnalyzer());
            foreach (var item in SenseNet.ContentRepository.Storage.StorageContext.Search.SearchEngine.GetAnalyzers())
                masterAnalyzer.AddAnalyzer(item.Key, (Analyzer)Activator.CreateInstance(item.Value));
            masterAnalyzer.AddAnalyzer(LucObject.FieldName.AllText, new StandardAnalyzer());
            //masterAnalyzer.AddAnalyzer(LucObject.FieldName.AllText, new StandardAnalyzer(SenseNet.Search.Indexing.LuceneManager.LuceneVersion));
            return masterAnalyzer;
        }

        internal static CommitUserData ReadCommitUserData(IndexReader reader)
        {
            int lastActivityId = 0;
            var gap = new List<int>();

            var cud = reader.GetCommitUserData();
            if (cud != null)
            {
                if (cud.ContainsKey(IndexManager.LastActivityIdKey))
                {
                    var lastID = cud[IndexManager.LastActivityIdKey];
                    if (!string.IsNullOrEmpty(lastID))
                        int.TryParse(lastID, out lastActivityId);
                }
                if (cud.ContainsKey(IndexManager.MissingActivitiesKey))
                {
                    var gapstring = cud[IndexManager.MissingActivitiesKey];
                    int g;
                    if (!string.IsNullOrEmpty(gapstring))
                        foreach (var s in gapstring.Split(','))
                            if (Int32.TryParse(s, out g))
                                gap.Add(g);
                }
            }
            return new CommitUserData { LastActivityId = lastActivityId, Gap = gap };
        }
        internal static Dictionary<string, string> CreateCommitUserData(int lastActivityId)
        {
            return CreateCommitUserData(lastActivityId, null);
        }
        internal static Dictionary<string, string> CreateCommitUserData(int lastActivityId, string gapString)
        {
            var d = new Dictionary<string, string>();
            d.Add(IndexManager.LastActivityIdKey, lastActivityId.ToString());
            if(gapString != null)
                d.Add(IndexManager.MissingActivitiesKey, gapString);
            return d;
        }

    }

}
