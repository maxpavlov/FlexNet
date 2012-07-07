using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Index;
using System.Data.Common;
using SenseNet.Search;
using SenseNet.Search.Indexing;
using System.Data.SqlClient;
using System.Configuration;

namespace SenseNet.Tools.IntegrityChecker
{
    internal class Checker : IDisposable
    {
        public const string RootPath = "/Root";
        private const string LastTaskIdKey = "LastActivityId";
        private const string MissingTasksKey = "MissingActivities";

        public static IEnumerable<Difference> Check(string indexPath, string connectionString)
        {
            return Check(indexPath, connectionString, null, true);
        }
        public static IEnumerable<Difference> Check(string indexPath, string connectionString, string repoPath, bool recurse)
        {
            if (recurse)
            {
                if (repoPath != null)
                    if (repoPath.ToLower() == RootPath.ToLower())
                        repoPath = null;
                using (var checker = new Checker(indexPath, connectionString))
                {
                    checker.CheckIndexState();
                    checker.CheckDuplicates(repoPath);
                    return checker.CheckRecurse(repoPath);
                }
            }
            using (var checker = new Checker(indexPath, connectionString))
            {
                checker.CheckIndexState();
                checker.CheckDuplicates(repoPath);
                return checker.CheckNode(repoPath ?? RootPath);
            }
        }

        /*==================================================================================== Instance part */

        private Checker(string indexPath, string connectionString)
        {
            _ixPath = GetCurrentDirectory(indexPath);
            _connectionString = connectionString;
        }
        private string GetCurrentDirectory(string root)
        {
            var rootExists = System.IO.Directory.Exists(root);
            string path = null;
            if (rootExists)
            {
                path = System.IO.Directory.GetDirectories(root)
                    .Where(a => Char.IsDigit(System.IO.Path.GetFileName(a)[0]))
                    .OrderBy(s => s)
                    .LastOrDefault();
            }
            Console.WriteLine(String.Format("Current directory: {0}", (path ?? "[null]")));
            Console.WriteLine();
            return path;
        }

        public const int MAXRESULT = 20;
        private string _ixPath;
        private string _connectionString;
        private IndexReader __ixReader;
        private IndexReader GetIndexReader()
        {
            if (__ixReader == null)
            {
                var directory = Lucene.Net.Store.FSDirectory.GetDirectory(_ixPath, false);
                __ixReader = IndexReader.Open(directory);
            }
            return __ixReader;
        }
        private void CloseReader()
        {
            if (__ixReader != null)
                __ixReader.Close();
        }

        private IEnumerable<Difference> CheckNode(string path)
        {
            var result = new List<Difference>();
            var ixreader = GetIndexReader();
            var docids = new List<int>();

            //---

            string checkNodeSql = "SELECT V.VersionId, CONVERT(bigint, n.timestamp) NodeTimestamp, CONVERT(bigint, v.timestamp) VersionTimestamp from Versions V join Nodes N on V.NodeId = N.NodeId WHERE N.Path = '{0}'";
            var sql = String.Format(checkNodeSql, path);

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                using (var proc = new SqlCommand())
                {
                    proc.Connection = connection;
                    proc.CommandText = sql;
                    proc.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    using (var dbreader = proc.ExecuteReader())
                    {
                        while (dbreader.Read())
                        {
                            var docid = CheckDbAndIndex(dbreader, ixreader, result);
                            if (docid >= 0)
                                docids.Add(docid);
                        }
                    }
                }
            }

            //---

            var scoredocs = GetDocsUnderTree(path, false);
            foreach (var scoredoc in scoredocs)
            {
                var docid = scoredoc.Doc;
                var doc = ixreader.Document(docid);
                if (!docids.Contains(docid))
                {
                    result.Add(new Difference(IndexDifferenceKind.NotInDatabase)
                    {
                        DocId = scoredoc.Doc,
                        VersionId = ParseInt(doc.Get(LucObject.FieldName.VersionId)),
                        NodeId = ParseInt(doc.Get(LucObject.FieldName.NodeId)),
                        Path = path,
                        Version = doc.Get(LucObject.FieldName.Version),
                        IxNodeTimestamp = ParseLong(doc.Get(LucObject.FieldName.NodeTimestamp)),
                        IxVersionTimestamp = ParseLong(doc.Get(LucObject.FieldName.VersionTimestamp))
                    });
                }
            }
            return result;
        }

        int intsize = sizeof(int) * 8;
        private int numdocs;

        private IEnumerable<Difference> CheckRecurse(string path)
        {
            Console.WriteLine("Comparing database and index");
            var result = new List<Difference>();

            var ixreader = GetIndexReader();
            numdocs = ixreader.NumDocs() + ixreader.NumDeletedDocs();
            var docbits = GetDocBits(path);

            //---

            string sql;
            if (path == null)
                sql = "SELECT V.VersionId, CONVERT(bigint, n.timestamp) NodeTimestamp, CONVERT(bigint, v.timestamp) VersionTimestamp from Versions V join Nodes N on V.NodeId = N.NodeId";
            else
                sql = String.Format("SELECT V.VersionId, CONVERT(bigint, n.timestamp) NodeTimestamp, CONVERT(bigint, v.timestamp) VersionTimestamp from Versions V join Nodes N on V.NodeId = N.NodeId WHERE N.Path = '{0}' OR N.Path LIKE '{0}/%'", path);

            var checks = 0;
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                using (var proc = new SqlCommand())
                {
                    proc.Connection = connection;
                    proc.CommandText = sql;
                    proc.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    using (var dbreader = proc.ExecuteReader())
                    {
                        while (dbreader.Read())
                        {
                            var docid = CheckDbAndIndex(dbreader, ixreader, result);
                            if (docid > -1)
                                docbits[docid / intsize] |= 1 << docid % intsize;
                            checks++;
                        }
                    }
                }
            }
            Console.WriteLine("    {0} node versions are checked in the database and index", checks);

            //---
            checks = 0;
            for (int i = 0; i < docbits.Length; i++)
            {
                if (docbits[i] == -1)
                {
                    checks += intsize;
                }
                else
                {
                    var bits = docbits[i];
                    for (int j = 0; j < intsize; j++)
                    {
                        var docid = i * intsize + j;
                        if (docid >= numdocs)
                            break;
                        checks++;
                        if ((bits & (1 << j)) == 0)
                        {
                            if (!ixreader.IsDeleted(docid))
                            {
                                var doc = ixreader.Document(docid);
                                result.Add(new Difference(IndexDifferenceKind.NotInDatabase)
                                {
                                    DocId = docid,
                                    VersionId = ParseInt(doc.Get(LucObject.FieldName.VersionId)),
                                    NodeId = ParseInt(doc.Get(LucObject.FieldName.NodeId)),
                                    Path = doc.Get(LucObject.FieldName.Path),
                                    Version = doc.Get(LucObject.FieldName.Version),
                                    IxNodeTimestamp = ParseLong(doc.Get(LucObject.FieldName.NodeTimestamp)),
                                    IxVersionTimestamp = ParseLong(doc.Get(LucObject.FieldName.VersionTimestamp))
                                });
                            }
                        }
                    }
                }
            }
            Console.WriteLine("    {0} documents are checked ({1} deleted)", checks, ixreader.NumDeletedDocs());
            return result.ToArray();
        }
        private int CheckDbAndIndex(DbDataReader dbreader, IndexReader ixreader, List<Difference> result)
        {
            var versionId = dbreader.GetInt32(0);
            var dbNodeTimestamp = dbreader.GetInt64(1);
            var dbVersionTimestamp = dbreader.GetInt64(2);

            var termDocs = ixreader.TermDocs(new Lucene.Net.Index.Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId)));
            Lucene.Net.Documents.Document doc = null;
            int docid = -1;
            if (termDocs.Next())
            {
                docid = termDocs.Doc();
                doc = ixreader.Document(docid);
                var indexNodeTimestamp = ParseLong(doc.Get(LucObject.FieldName.NodeTimestamp));
                var indexVersionTimestamp = ParseLong(doc.Get(LucObject.FieldName.VersionTimestamp));
                var nodeId = ParseInt(doc.Get(LucObject.FieldName.NodeId));
                var version = doc.Get(LucObject.FieldName.Version);
                var p = doc.Get(LucObject.FieldName.Path);
                if (termDocs.Next())
                {
                    result.Add(new Difference(IndexDifferenceKind.MoreDocument)
                    {
                        DocId = docid,
                        NodeId = nodeId,
                        VersionId = versionId,
                        Version = version,
                        Path = p,
                        DbNodeTimestamp = dbNodeTimestamp,
                        DbVersionTimestamp = dbVersionTimestamp,
                        IxNodeTimestamp = indexNodeTimestamp,
                        IxVersionTimestamp = indexVersionTimestamp,
                    });
                }
                if (dbVersionTimestamp != indexVersionTimestamp)
                {
                    result.Add(new Difference(IndexDifferenceKind.DifferentVersionTimestamp)
                    {
                        DocId = docid,
                        VersionId = versionId,
                        DbNodeTimestamp = dbNodeTimestamp,
                        DbVersionTimestamp = dbVersionTimestamp,
                        IxNodeTimestamp = indexNodeTimestamp,
                        IxVersionTimestamp = indexVersionTimestamp,
                        NodeId = nodeId,
                        Version = version,
                        Path = p
                    });
                }
                if (dbNodeTimestamp != indexNodeTimestamp)
                {
                    var ok = false;
                    var isLastDraft = doc.Get(LucObject.FieldName.IsLastDraft);
                    if (isLastDraft != BooleanIndexHandler.YES)
                    {
                        var latestDocs = ixreader.TermDocs(new Lucene.Net.Index.Term(LucObject.FieldName.NodeId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(nodeId)));
                        Lucene.Net.Documents.Document latestDoc = null;
                        while (latestDocs.Next())
                        {
                            var latestdocid = latestDocs.Doc();
                            var d = ixreader.Document(latestdocid);
                            if (d.Get(LucObject.FieldName.IsLastDraft) != BooleanIndexHandler.YES)
                                continue;
                            latestDoc = d;
                            break;
                        }
                        var latestPath = latestDoc.Get(LucObject.FieldName.Path);
                        if (latestPath == p)
                            ok = true;
                    }
                    if (!ok)
                    {
                        result.Add(new Difference(IndexDifferenceKind.DifferentNodeTimestamp)
                        {
                            DocId = docid,
                            VersionId = versionId,
                            DbNodeTimestamp = dbNodeTimestamp,
                            DbVersionTimestamp = dbVersionTimestamp,
                            IxNodeTimestamp = indexNodeTimestamp,
                            IxVersionTimestamp = indexVersionTimestamp,
                            NodeId = nodeId,
                            Version = version,
                            Path = p
                        });
                    }
                }
            }
            else
            {
                result.Add(new Difference(IndexDifferenceKind.NotInIndex)
                {
                    DocId = docid,
                    VersionId = versionId,
                    DbNodeTimestamp = dbNodeTimestamp,
                    DbVersionTimestamp = dbVersionTimestamp,
                });
            }
            return docid;
        }
        private ScoreDoc[] GetDocsUnderTree(string path, bool recurse)
        {
            var field = recurse ? "InTree" : "Path";
            var lq = LucQuery.Parse(String.Format("{0}:'{1}'", path, path.ToLower()));

            var idxReader = GetIndexReader();
            var searcher = new IndexSearcher(idxReader);
            var numDocs = idxReader.NumDocs();
            try
            {
                var collector = TopScoreDocCollector.Create(numDocs, false);
                searcher.Search(lq.Query, collector);
                var topDocs = collector.TopDocs(0, numDocs);
                return topDocs.ScoreDocs;
            }
            finally
            {
                if (searcher != null)
                    searcher.Close();
                searcher = null;
            }
        }

        private static int ParseInt(string data)
        {
            Int32 result;
            if (Int32.TryParse(data, out result))
                return result;
            return -1;
        }
        private static long ParseLong(string data)
        {
            Int64 result;
            if (Int64.TryParse(data, out result))
                return result;
            return -1;
        }

        private string GetConnectionString()
        {
            return _connectionString ?? ConfigurationManager.ConnectionStrings["SnCrMsSql"].ConnectionString;
        }

        
        private int GetLastTaskIdFromDb()
        {
            const string sql = "SELECT CASE WHEN i.last_value IS NULL THEN 0 ELSE CONVERT(int, i.last_value) END last_value FROM sys.identity_columns i JOIN sys.tables t ON i.object_id = t.object_id WHERE t.name = 'IndexingActivity'";
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                using (var proc = new SqlCommand())
                {
                    proc.Connection = connection;
                    proc.CommandText = sql;
                    proc.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    
                    var x = proc.ExecuteScalar();
                    if (x == DBNull.Value)
                        return 0;
                    return Convert.ToInt32(x);
                }
            }
        }
        private int GetLastTaskIdFromIndex(IDictionary<string,string> cud)
        {
            if (cud == null || !cud.ContainsKey(LastTaskIdKey))
                throw new Exception("Commit user data is not valid (missing LastActivityId).");

            int lastTaskId = 0;
            
            var lastID = cud[LastTaskIdKey];
            if (!string.IsNullOrEmpty(lastID))
                int.TryParse(lastID, out lastTaskId);

            return lastTaskId;
        }
        private string GetGapsInIndex(IDictionary<string, string> cud)
        {
            if (cud == null || !cud.ContainsKey(MissingTasksKey))
                throw new Exception("Commit user data is not valid (missing MissingTasks).");

            return cud[MissingTasksKey];
            
        }

        //===========================================================================

        private void CheckIndexState()
        {
            var ixreader = GetIndexReader();
            var cud = ixreader.GetCommitUserData();
            
            var gapsInIndex = GetGapsInIndex(cud);
            if (!String.IsNullOrEmpty(gapsInIndex))
                throw new Exception(String.Format("There are tasks to be executed (gaps: {0})", gapsInIndex));
            
            var lastIdFromDb = GetLastTaskIdFromDb();
            var lastIdFromIndex = GetLastTaskIdFromIndex(cud);

            if (lastIdFromDb != lastIdFromIndex)
                throw new Exception(String.Format("There are tasks to be executed (Last task ids don't match! db:{0} and index:{1}", lastIdFromDb, lastIdFromIndex));

            Console.WriteLine("LastTaskId:{0}", lastIdFromDb);
        }
        private void CheckDuplicates(string path)
        {
            CheckDuplicatesByVersionId(path);
            //CheckDuplicatesByPath(path);
        }
        private void CheckDuplicatesByVersionId(string path)
        {
            var ixreader = GetIndexReader();
            numdocs = ixreader.NumDocs() + ixreader.NumDeletedDocs();
            Console.WriteLine("Checking duplicates by VersionId: docs: {0} deleted: {1}", ixreader.NumDocs(), ixreader.NumDeletedDocs());
            var docbits = GetDocBits(path);
            var duplicatesFound = 0;
            for (var docid = 0; docid < numdocs; docid++)
            {
                var bit = docbits[docid / intsize] & (1 << (docid % intsize));
                if (bit != 0)
                    continue;
                if (!ixreader.IsDeleted(docid))
                {
                    var doc = ixreader.Document(docid);
                    var versionId = ParseInt(doc.Get(LucObject.FieldName.VersionId));
                    var p = doc.Get(LucObject.FieldName.Path);

                    var termDocs = ixreader.TermDocs(new Lucene.Net.Index.Term(LucObject.FieldName.VersionId, Lucene.Net.Util.NumericUtils.IntToPrefixCoded(versionId)));
                    while (termDocs.Next())
                    {
                        var docid1 = termDocs.Doc();
                        if (docid1 != docid)
                        {
                            if (!ixreader.IsDeleted(docid1))
                            {
                                duplicatesFound++;
                                docbits[docid1 / intsize] |= 1 << docid1 % intsize;
                                if (duplicatesFound <= MAXRESULT)
                                    Console.WriteLine("DUPLICATED VERSIONID: {0}. Docid: {1}, {2}", versionId, docid, docid1);
                            }
                        }
                    }
                }
                docbits[docid / intsize] |= 1 << docid % intsize;
            }
            if (duplicatesFound == 0)
                Console.WriteLine("No duplicates");
            else
                Console.WriteLine("{0} duplicates found.");
            Console.WriteLine();
        }
        //private void CheckDuplicatesByPath(string path)
        //{
        //    var ixreader = GetIndexReader();
        //    numdocs = ixreader.NumDocs() + ixreader.NumDeletedDocs();
        //    Console.WriteLine("Checking duplicates by Path: docs: {0} deleted: {1}", ixreader.NumDocs(), ixreader.NumDeletedDocs());
        //    var docbits = GetDocBits(path);
        //    var duplicatesFound = 0;
        //    for (var docid = 0; docid < numdocs; docid++)
        //    {
        //        var bit = docbits[docid / intsize] & (1 << (docid % intsize));
        //        if (bit != 0)
        //            continue;
        //        if (!ixreader.IsDeleted(docid))
        //        {
        //            var doc = ixreader.Document(docid);
        //            var p = doc.Get(LucObject.FieldName.Path);

        //            var termDocs = ixreader.TermDocs(new Lucene.Net.Index.Term(LucObject.FieldName.Path, p));
        //            while (termDocs.Next())
        //            {
        //                var docid1 = termDocs.Doc();
        //                if (docid1 != docid)
        //                {
        //                    if (!ixreader.IsDeleted(docid1))
        //                    {
        //                        duplicatesFound++;
        //                        docbits[docid1 / intsize] |= 1 << docid1 % intsize;
        //                        if (duplicatesFound <= MAXRESULT)
        //                            Console.WriteLine("DUPLICATED PATH: {0}. Docid: {1}, {2}", p, docid, docid1);
        //                    }
        //                }
        //            }
        //        }
        //        docbits[docid / intsize] |= 1 << docid % intsize;
        //    }
        //    if (duplicatesFound == 0)
        //        Console.WriteLine("No duplicates");
        //    else
        //        Console.WriteLine("{0} duplicates found.");
        //    Console.WriteLine();
        //}

        private int[] GetDocBits(string path)
        {
            var x = numdocs / intsize;
            var y = numdocs % intsize;
            var docbits = new int[x + (y > 0 ? 1 : 0)];
            if (path == null)
            {
                if (y > 0)
                {
                    var q = 0;
                    for (int i = 0; i < y; i++)
                        q += 1 << i;
                    docbits[docbits.Length - 1] = q ^ (-1);
                }
            }
            else
            {
                for (int i = 0; i < docbits.Length; i++)
                    docbits[i] = -1;
                var scoredocs = GetDocsUnderTree(path, true);
                for (int i = 0; i < scoredocs.Length; i++)
                {
                    var docid = scoredocs[i].Doc;
                    docbits[docid / intsize] ^= 1 << docid % intsize;
                }
            }
            return docbits;
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
                    CloseReader();
            _disposed = true;
        }
        ~Checker()
        {
            Dispose(false);
        }
    }
}
