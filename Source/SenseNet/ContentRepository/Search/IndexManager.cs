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

namespace SenseNet.Search
{
    internal class CommitUserData
    {
        public int LastTaskId;
        public List<int> Gap;
    }

    internal class IndexManager
    {
        internal static readonly string LastTaskIdKey = "LastTaskId";
        internal static readonly string MissingTasksKey = "MissingTasks";

        static IndexManager()
        {
            IndexWriter.SetDefaultWriteLockTimeout(20 * 60 * 1000); // 20 minutes
        }

        internal static IndexWriter GetIndexWriter(bool createNew)
        {
            Directory dir = FSDirectory.GetDirectory(SenseNet.ContentRepository.Storage.IndexDirectory.CurrentOrDefaultDirectory, createNew);
            return new IndexWriter(dir, GetAnalyzer(), createNew, IndexWriter.MaxFieldLength.UNLIMITED);
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
            return masterAnalyzer;
        }

        internal static CommitUserData ReadCommitUserData(IndexReader reader)
        {
            int lastTaskId = 0;
            var gap = new List<int>();

            var cud = reader.GetCommitUserData();
            if (cud != null)
            {
                if (cud.ContainsKey(IndexManager.LastTaskIdKey))
                {
                    var lastID = cud[IndexManager.LastTaskIdKey];
                    if (!string.IsNullOrEmpty(lastID))
                        int.TryParse(lastID, out lastTaskId);
                }
                if (cud.ContainsKey(IndexManager.MissingTasksKey))
                {
                    var gapstring = cud[IndexManager.MissingTasksKey];
                    int g;
                    if (!string.IsNullOrEmpty(gapstring))
                        foreach (var s in gapstring.Split(','))
                            if (Int32.TryParse(s, out g))
                                gap.Add(g);
                }
            }
            return new CommitUserData { LastTaskId = lastTaskId, Gap = gap };
        }
        internal static Dictionary<string, string> CreateCommitUserData(int lastTaskId)
        {
            return CreateCommitUserData(lastTaskId, null);
        }
        internal static Dictionary<string, string> CreateCommitUserData(int lastTaskId, string gapString)
        {
            var d = new Dictionary<string, string>();
            d.Add(IndexManager.LastTaskIdKey, lastTaskId.ToString());
            if(gapString != null)
                d.Add(IndexManager.MissingTasksKey, gapString);
            return d;
        }

    }

}
