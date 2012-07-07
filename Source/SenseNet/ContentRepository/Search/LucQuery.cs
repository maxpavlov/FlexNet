using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Search;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using System.Collections;
using SenseNet.ContentRepository.Storage.Security;
using Lucene.Net.Index;
using Lucene.Net.Util;
using SenseNet.Diagnostics;
using System.Diagnostics;
using SenseNet.Search.Parser;
using SenseNet.Search.Indexing;

namespace SenseNet.Search
{
    public class LucQuery
    {
        public QueryTraceInfo TraceInfo { get; private set; }

        private Query __query;
        public Query Query
        {
            get { return __query; }
            private set
            {
                __query = value;
                TraceInfo.Query = value;
            }
        }
        public string QueryText { get { return QueryToString(Query); } }

        public IUser User { get; set; }
        public SortField[] SortFields { get; set; }
        public string Projection { get; private set; }

        [Obsolete("Use Skip instead. Be aware that StartIndex is 1-based but Skip is 0-based.")]
        public int StartIndex
        {
            get
            {
                return Skip + 1;
            }
            set
            {
                Skip = Math.Max(0, value - 1);
            }
        }
        public int Skip { get; set; }
        public int PageSize { get; set; }
        public int Top { get; set; }
        public bool CountOnly { get; set; }
        public bool EnableAutofilters { get; set; }
        public bool EnableLifespanFilter { get; set; }

        public int TotalCount { get; private set; }

        private Query _autoFilterQuery;
        internal Query AutoFilterQuery
        {
            get
            {
                if (_autoFilterQuery == null)
                {
                    var parser = new SnLucParser();
                    _autoFilterQuery = parser.Parse("IsSystemContent:no");
                }
                return _autoFilterQuery;
            }
        }

        private Query _lifespanQuery;
        internal Query LifespanQuery
        {
            get
            {
                if (_lifespanQuery == null)
                {
                    var parser = new SnLucParser();
                    var lfspText = LucQueryTemplateReplacer.ReplaceTemplates("EnableLifespan:no OR (+ValidFrom:<@@CurrentTime@@ +ValidTill:>@@CurrentTime@@)");

                    _lifespanQuery = parser.Parse(lfspText);
                }
                return _lifespanQuery;
            }
        }

        private LucQuery()
        {
            EnableAutofilters = true;
            EnableLifespanFilter = false;
            TraceInfo = new QueryTraceInfo();
        }

        public static LucQuery Create(NodeQuery nodeQuery)
        {
            NodeQueryParameter[] parameters;
            var result = new LucQuery();
            result.TraceInfo.BeginCrossCompilingTime();

            SortField[] sortFields;
            string oldQueryText;
            try
            {
                var compiler = new SnLucCompiler();
                var compiledQueryText = compiler.Compile(nodeQuery, out parameters);

                sortFields = (from order in nodeQuery.Orders
                              select new SortField(
                                  GetFieldNameByPropertyName(order.PropertyName),
                                  GetSortType(order.PropertyName), //SortField.STRING,
                                  order.Direction == OrderDirection.Desc)).ToArray();

                oldQueryText = compiler.CompiledQuery.ToString();
                oldQueryText = oldQueryText.Replace("[*", "[ ").Replace("*]", " ]").Replace("{*", "{ ").Replace("*}", " }");
                result.TraceInfo.InputText = oldQueryText;
            }
            finally
            {
                result.TraceInfo.FinishCrossCompilingTime();
            }
            result.TraceInfo.BeginParsingTime();
            Query newQuery;
            try
            {
                newQuery = new SnLucParser().Parse(oldQueryText);
            }
            finally
            {
                result.TraceInfo.FinishParsingTime();
            }
            result.Query = newQuery; // compiler.CompiledQuery,
            result.User = nodeQuery.User;
            result.SortFields = sortFields;
            result.StartIndex = nodeQuery.Skip;
            result.PageSize = nodeQuery.PageSize;
            result.Top = nodeQuery.Top;
            result.EnableAutofilters = false;
            result.EnableLifespanFilter = false;

            return result;
        }
        private static string GetFieldNameByPropertyName(string propertyName)
        {
            if (propertyName == "NodeId") return "Id";
            return propertyName;
        }
        private static int GetSortType(string propertyName)
        {
            var x = SenseNet.ContentRepository.Schema.ContentTypeManager.GetPerFieldIndexingInfo(GetFieldNameByPropertyName(propertyName));
            if (x != null)
                return x.IndexFieldHandler.SortingType;
            return SortField.STRING;
        }
        public static LucQuery Create(Query luceneQuery)
        {
            Logger.WriteVerbose("Query creating from luceneQuery");
            var query = new LucQuery { Query = luceneQuery, SortFields = new SortField[0] };
            query.TraceInfo.InputText = "";
            return query;
        }

        public static LucQuery Parse(string luceneQueryText)
        {
            var result = new LucQuery();
            result.TraceInfo.InputText = luceneQueryText;

            result.TraceInfo.BeginParsingTime();
            var parser = new SnLucParser();
            Query query;
            try
            {
                var replacedText = LucQueryTemplateReplacer.ReplaceTemplates(luceneQueryText);
                query = parser.Parse(replacedText);
            }
            finally
            {
                result.TraceInfo.FinishParsingTime();
            }
            //Run EmptyTermVisitor if the parser created empty query term.
            if (parser.ParseEmptyQuery)
            {
                var visitor = new EmptyTermVisitor();
                result.Query = visitor.Visit(query);
            }
            else
            {
                result.Query = query;
            }

            var sortFields = new List<SortField>();
            foreach (var control in parser.Controls)
            {
                switch (control.Name)
                {
                    case SnLucLexer.Keywords.Select:
                        result.Projection = control.Value;
                        break;
                    case SnLucLexer.Keywords.Top:
                        result.Top = Convert.ToInt32(control.Value);
                        break;
                    case SnLucLexer.Keywords.Skip:
                        result.Skip = Convert.ToInt32(control.Value);
                        break;
                    case SnLucLexer.Keywords.Sort:
                        sortFields.Add(CreateSortField(control.Value, false));
                        break;
                    case SnLucLexer.Keywords.ReverseSort:
                        sortFields.Add(CreateSortField(control.Value, true));
                        break;
                    case SnLucLexer.Keywords.Autofilters:
                        result.EnableAutofilters = control.Value == SnLucLexer.Keywords.On;
                        break;
                    case SnLucLexer.Keywords.Lifespan:
                        result.EnableLifespanFilter = control.Value == SnLucLexer.Keywords.On;
                        break;
                    case SnLucLexer.Keywords.CountOnly:
                        result.CountOnly = true;
                        break;
                }
            }
            result.SortFields = sortFields.ToArray();

            return result;
        }
        private static SortField CreateSortField(string fieldName, bool reverse)
        {
            var info = SenseNet.ContentRepository.Schema.ContentTypeManager.GetPerFieldIndexingInfo(fieldName);
            var sortType = SortField.STRING;
            if (info != null)
                sortType = info.IndexFieldHandler.SortingType;
            if (sortType == SortField.STRING)
                return new SortField(fieldName, System.Threading.Thread.CurrentThread.CurrentCulture, reverse);
            return new SortField(fieldName, sortType, reverse);
        }

        //========================================================================================

        public IEnumerable<LucObject> Execute()
        {
            return Execute(false);
        }
        public IEnumerable<LucObject> Execute(bool allVersions)
        {
            if (CountOnly)
                return Execute(allVersions, new QueryExecutor20100701CountOnly());
            //return Execute(allVersions, new QueryExecutor20100701());
            return Execute(allVersions, new QueryExecutor20110503());
        }
        internal IEnumerable<LucObject> Execute(IQueryExecutor executor)
        {
            return Execute(false, executor);
        }
        internal IEnumerable<LucObject> Execute(bool allVersions, IQueryExecutor executor)
        {
            var result = executor.Execute(this, allVersions);
            TotalCount = executor.TotalCount;
            return result;
        }

        public override string ToString()
        {
            var result = new StringBuilder(QueryText);
            if (CountOnly)
                result.Append(" ").Append(SnLucLexer.Keywords.CountOnly);
            if (Top != 0)
                result.Append(" ").Append(SnLucLexer.Keywords.Top).Append(":").Append(Top);
            if (Skip != 0)
                result.Append(" ").Append(SnLucLexer.Keywords.Skip).Append(":").Append(Skip);
            foreach (var sortField in this.SortFields)
                if (sortField.GetReverse())
                    result.Append(" ").Append(SnLucLexer.Keywords.ReverseSort).Append(":").Append(sortField.GetField());
                else
                    result.Append(" ").Append(SnLucLexer.Keywords.Sort).Append(":").Append(sortField.GetField());
            if (!EnableAutofilters)
                result.Append(" ").Append(SnLucLexer.Keywords.Autofilters).Append(":").Append(SnLucLexer.Keywords.Off);
            if (EnableLifespanFilter)
                result.Append(" ").Append(SnLucLexer.Keywords.Lifespan).Append(":").Append(SnLucLexer.Keywords.On);
            return result.ToString();
        }
        private string QueryToString(Query query)
        {
            try
            {
                var visitor = new ToStringVisitor();
                visitor.Visit(query);
                return visitor.ToString();
            }
            catch (Exception e)
            {
                Logger.WriteException(e);

                var c = query.ToString().ToCharArray();
                for (int i = 0; i < c.Length; i++)
                    if (c[i] < ' ')
                        c[i] = '.';
                return new String(c);
            }
        }

        internal void SetSort(IEnumerable<SortInfo> sort)
        {
            var sortFields = new List<SortField>();
            foreach (var field in sort)
                sortFields.Add(CreateSortField(field.FieldName, field.Reverse));
            this.SortFields = sortFields.ToArray();
        }

        public void AddAndClause(LucQuery q2)
        {
            var boolQ = new BooleanQuery();
            boolQ.Add(Query, BooleanClause.Occur.MUST);
            boolQ.Add(q2.Query, BooleanClause.Occur.MUST);
            Query = boolQ;
        }
        public void AddOrClause(LucQuery q2)
        {
            var boolQ = new BooleanQuery();
            boolQ.Add(Query, BooleanClause.Occur.SHOULD);
            boolQ.Add(q2.Query, BooleanClause.Occur.SHOULD);
            Query = boolQ;
        }
    }

    public interface IQueryExecutor
    {
        int TotalCount { get; }
        IEnumerable<LucObject> Execute(LucQuery lucQuery, bool allVersions);
    }

    internal abstract class QueryExecutor : IQueryExecutor
    {
        protected static readonly IEnumerable<LucObject> EmptyResult = new LucObject[0];

        protected LucQuery LucQuery { get; private set; }
        internal long FullExecutionTime { get; private set; }
        internal long KernelTime { get; private set; }
        internal long CollectingTime { get; private set; }
        internal long PagingTime { get; private set; }
        internal long FullExecutingTime { get; private set; }
        public int TotalCount { get; private set; }

        private Stopwatch timer;
        protected void BeginFullExecutingTime()
        {
            timer = Stopwatch.StartNew();
        }
        protected void FinishFullExecutingTime()
        {
            if (timer == null)
                return;
            FullExecutingTime = timer.ElapsedTicks;
            timer.Stop();
        }

        private long kernelStart;
        protected void BeginKernelTime()
        {
            kernelStart = timer.ElapsedTicks;
        }
        protected void FinishKernelTime()
        {
            if (timer == null)
                return;
            KernelTime = timer.ElapsedTicks - kernelStart;
        }

        private long collectingStart;
        protected void BeginCollectingTime()
        {
            collectingStart = timer.ElapsedTicks;
        }
        protected void FinishCollectingTime()
        {
            if (timer == null)
                return;
            CollectingTime = timer.ElapsedTicks - collectingStart;
        }

        private long pagingStart;
        protected void BeginPagingTime()
        {
            pagingStart = timer.ElapsedTicks;
        }
        protected void FinishPagingTime()
        {
            if (timer == null)
                return;
            PagingTime = timer.ElapsedTicks - pagingStart;
        }

        public IEnumerable<LucObject> Execute(LucQuery lucQuery, bool allVersions)
        {
            this.LucQuery = lucQuery;
            using (var traceOperation = Logger.TraceOperation("Query execution", "Query: " + this.LucQuery.QueryText))
            {
                Query currentQuery;

                if (this.LucQuery.EnableAutofilters || this.LucQuery.EnableLifespanFilter)
                {
                    var fullQuery = new BooleanQuery();
                    fullQuery.Add(new BooleanClause(this.LucQuery.Query, BooleanClause.Occur.MUST));

                    if (this.LucQuery.EnableAutofilters)
                        fullQuery.Add(new BooleanClause(this.LucQuery.AutoFilterQuery, BooleanClause.Occur.MUST));
                    if (this.LucQuery.EnableLifespanFilter && this.LucQuery.LifespanQuery != null)
                        fullQuery.Add(new BooleanClause(this.LucQuery.LifespanQuery, BooleanClause.Occur.MUST));

                    currentQuery = fullQuery;
                }
                else
                {
                    currentQuery = this.LucQuery.Query;
                }

                int totalCount;
                IEnumerable<LucObject> result;
                using (var readerFrame = LuceneManager.GetIndexReaderFrame())
                {
                    var idxReader = readerFrame.IndexReader;
                    result = DoExecute(currentQuery, allVersions, idxReader, out totalCount);
                }

                TotalCount = totalCount;

                var trace = lucQuery.TraceInfo;
                trace.KernelTime = KernelTime;
                trace.CollectingTime = CollectingTime;
                trace.PagingTime = PagingTime;
                trace.FullExecutingTime = FullExecutingTime;

                traceOperation.AdditionalObject = trace;
                traceOperation.IsSuccessful = true;
                return result;
            }
        }

        protected abstract IEnumerable<LucObject> DoExecute(Query query, bool allVersions, IndexReader idxReader, out int totalCount);

        protected bool IsPermitted(Document doc, IUser user, bool isCurrentUser)
        {
            var path = doc.Get(LucObject.FieldName.Path);

            var createdById = IntegerIndexHandler.ConvertBack(doc.Get(LucObject.FieldName.CreatedById));
            var lastModifiedById = IntegerIndexHandler.ConvertBack(doc.Get(LucObject.FieldName.ModifiedById));
            var isLastPublic = BooleanIndexHandler.ConvertBack(doc.Get(LucObject.FieldName.IsLastPublic));
            var isLastDraft = BooleanIndexHandler.ConvertBack(doc.Get(LucObject.FieldName.IsLastDraft));
            var level = isCurrentUser
                ? SecurityHandler.GetPermittedLevel(path, createdById, lastModifiedById)
                : SecurityHandler.GetPermittedLevel(path, createdById, lastModifiedById, user);
            switch (level)
            {
                case PermittedLevel.None:
                    return false;
                case PermittedLevel.HeadOnly:
                    return isLastPublic;
                case PermittedLevel.PublicOnly:
                    return isLastPublic;
                case PermittedLevel.All:
                    return isLastDraft;
                default:
                    throw new NotImplementedException();
            }
        }
    }
    internal class QueryExecutor20100630 : QueryExecutor
    {
        protected override IEnumerable<LucObject> DoExecute(Query query, bool allVersions, IndexReader idxReader, out int totalCount)
        {
            Searcher searcher = null;

            BeginFullExecutingTime();
            try
            {
                IEnumerable<LucObject> result;
                searcher = new IndexSearcher(idxReader);
                TopDocs topDocs;
                ScoreDoc[] hits;
                var top = 100000; // 501; //idxReader.MaxDoc();
                if (this.LucQuery.SortFields.Length > 0)
                {
                    BeginKernelTime();
                    TopFieldDocCollector collector = new TopFieldDocCollector(idxReader, new Sort(this.LucQuery.SortFields), top);
                    searcher.Search(query, collector);
                    FinishKernelTime();
                    BeginCollectingTime();
                    topDocs = collector.TopDocs();
                    totalCount = topDocs.TotalHits;
                    hits = topDocs.ScoreDocs;
                    FinishCollectingTime();
                }
                else
                {
                    BeginKernelTime();
                    TopDocCollector collector = new TopDocCollector(top);
                    searcher.Search(query, collector);
                    FinishKernelTime();
                    BeginCollectingTime();
                    topDocs = collector.TopDocs();
                    totalCount = topDocs.TotalHits;
                    hits = topDocs.ScoreDocs;
                    FinishCollectingTime();
                }
                BeginPagingTime();
                result = GetResultPage(hits, searcher, allVersions);
                FinishPagingTime();


                return result;
            }
            catch
            {
                FinishKernelTime();
                FinishCollectingTime();
                FinishPagingTime();
                throw;
            }
            finally
            {
                FinishFullExecutingTime();
                if (searcher != null)
                {
                    searcher.Close();
                    searcher = null;
                }
            }
        }
        private IEnumerable<LucObject> GetResultPage(ScoreDoc[] hits, Searcher searcher, bool allVersions)
        {
            Logger.Write(this.LucQuery.QueryText);
            //using (var traceOperation = Logger.TraceOperation(allVersions ? "Query paging" : "Query paging and security"))
            //{
            var startIndex = this.LucQuery.StartIndex;
            var pageSize = this.LucQuery.PageSize;
            if (pageSize == 0)
                pageSize = Int32.MaxValue;
            var top = this.LucQuery.Top;
            if (top == 0)
                top = Int32.MaxValue;
            if (top < pageSize)
                pageSize = top;
            var count = 0;
            var countInPage = 0;
            var result = new List<LucObject>();

            var user = this.LucQuery.User;
            var currentUser = AccessProvider.Current.GetCurrentUser();
            if (user == null)
                user = currentUser;
            var isCurrentUser = user.Id == currentUser.Id;

            foreach (var hit in hits)
            {
                Document doc = searcher.Doc(hit.Doc);
                if (allVersions || IsPermitted(doc, user, isCurrentUser))
                {
                    if (++count >= startIndex)
                    {
                        if (countInPage++ >= pageSize)
                            break;
                        result.Add(new LucObject(doc));
                    }
                }
            }
            //traceOperation.IsSuccessful = true;
            return result;
            //}
        }
    }
    internal class QueryExecutor20100701 : QueryExecutor
    {
        protected override IEnumerable<LucObject> DoExecute(Query query, bool allVersions, IndexReader idxReader, out int totalCount)
        {
            Searcher searcher = null;
            IEnumerable<LucObject> result;
            totalCount = 0;

            BeginFullExecutingTime();
            searcher = new IndexSearcher(idxReader);

            var numDocs = idxReader.NumDocs();

            var start = this.LucQuery.Skip;

            var maxtop = numDocs - start;
            if (maxtop < 1)
                return EmptyResult;

            int top = this.LucQuery.Top != 0 ? this.LucQuery.Top : this.LucQuery.PageSize;

            if (top == 0)
                top = 100000;
            var howMany = (top < int.MaxValue / 2) ? top * 2 : int.MaxValue; // numDocs; // * 4; // * 2;

            bool noMorePage = false;
            if ((long)howMany > maxtop)
            {
                howMany = maxtop - start;
                noMorePage = true;
            }

            var numHits = howMany + start;
            if (numHits > numDocs)
                numHits = numDocs;

            try
            {
                //====================================================
                var collector = CreateCollector(numHits);
                BeginKernelTime();
                searcher.Search(query, collector);
                FinishKernelTime();
                BeginCollectingTime();
                //var topDocs = collector.TopDocs(start, howMany);
                var topDocs = (this.LucQuery.SortFields.Length > 0) ?
                    ((TopFieldCollector)collector).TopDocs(start, howMany) :
                    ((TopScoreDocCollector)collector).TopDocs(start, howMany);

                totalCount = topDocs.TotalHits;
                var hits = topDocs.ScoreDocs;
                FinishCollectingTime();
                //====================================================

                BeginPagingTime();
                bool noMoreHits;
                result = GetResultPage(hits, searcher, top, allVersions, out noMoreHits);
                FinishPagingTime();
                if (result.Count() < top && !noMorePage /*&& !noMoreHits*/)
                {
                    //re-search
                    numHits = numDocs - start;
                    collector = CreateCollector(numHits);
                    searcher.Search(query, collector);
                    //topDocs = collector.TopDocs(start);
                    topDocs = (this.LucQuery.SortFields.Length > 0) ?
                        ((TopFieldCollector)collector).TopDocs(start, howMany) :
                        ((TopScoreDocCollector)collector).TopDocs(start, howMany);

                    hits = topDocs.ScoreDocs;
                    result = GetResultPage(hits, searcher, top, allVersions, out noMoreHits);
                }

                return result;
            }
            catch
            {
                FinishKernelTime();
                FinishCollectingTime();
                FinishPagingTime();

                throw;
            }
            finally
            {
                if (searcher != null)
                    searcher.Close();
                searcher = null;
                FinishFullExecutingTime();
            }
        }
        protected Collector CreateCollector(int numHits)
        {
            var docsScoredInOrder = false;
            if (this.LucQuery.SortFields.Length > 0)
            {
                var sort = new Sort(this.LucQuery.SortFields);
                var fillFields = false;
                var trackDocScores = true;
                var trackMaxScore = false;
                return TopFieldCollector.Create(sort, numHits, fillFields, trackDocScores, trackMaxScore, docsScoredInOrder);
            }
            return TopScoreDocCollector.Create(numHits, docsScoredInOrder);
        }
        protected IEnumerable<LucObject> GetResultPage(ScoreDoc[] hits, Searcher searcher, int howMany, bool allVersions, out bool noMoreHits)
        {
            var result = new List<LucObject>();
            noMoreHits = false;
            if (hits.Length == 0)
                return result;

            var user = this.LucQuery.User;
            var currentUser = AccessProvider.Current.GetCurrentUser();
            if (user == null)
                user = currentUser;
            var isCurrentUser = user.Id == currentUser.Id;

            var upperBound = hits.Length;
            var index = 0;
            while (true)
            {
                Document doc = searcher.Doc(hits[index].Doc);
                if (allVersions || IsPermitted(doc, user, isCurrentUser))
                {
                    result.Add(new LucObject(doc));
                    if (result.Count == howMany)
                    {
                        noMoreHits = false;
                        break;
                    }
                }
                if (++index >= upperBound)
                {
                    noMoreHits = true;
                    break;
                }
            }
            return result;

            //foreach (var hit in hits)
            //{
            //    Document doc = searcher.Doc(hit.doc);
            //    if (allVersions || IsPermitted(doc, user, isCurrentUser))
            //    {
            //        result.Add(new LucObject(doc));
            //        if (result.Count == howMany)
            //            break;
            //    }
            //}
            //return result;

            /*
            Logger.Write(this.LucQuery.QueryText);
            //var startIndex = this.StartIndex;
            var pageSize = this.LucQuery.PageSize;
            if (pageSize == 0)
                pageSize = Int32.MaxValue;
            var top = this.LucQuery.Top;
            if (top == 0)
                top = Int32.MaxValue;
            if (top < pageSize)
                pageSize = top;
            var countInPage = 0;
            var result = new List<LucObject>();

            var user = this.LucQuery.User;
            var currentUser = AccessProvider.Current.GetCurrentUser();
            if (user == null)
                user = currentUser;
            var isCurrentUser = user.Id == currentUser.Id;

            foreach (var hit in hits)
            {
                Document doc = searcher.Doc(hit.doc);
                if (allVersions || IsPermitted(doc, user, isCurrentUser))
                {
                    if (countInPage++ >= pageSize)
                        break;
                    result.Add(new LucObject(doc));
                }
            }
            return result;
            */
        }
    }
    internal class QueryExecutor20100701CountOnly : QueryExecutor20100701
    {
        protected override IEnumerable<LucObject> DoExecute(Query query, bool allVersions, IndexReader idxReader, out int totalCount)
        {
            Searcher searcher = null;
            totalCount = 0;

            BeginFullExecutingTime();
            searcher = new IndexSearcher(idxReader);

            try
            {
                //====================================================
                var collector = CreateCollector(1);
                BeginKernelTime();
                searcher.Search(query, collector);
                FinishKernelTime();
                BeginCollectingTime();
                //totalCount = collector.GetTotalHits();
                totalCount = (this.LucQuery.SortFields.Length > 0) ?
                    ((TopFieldCollector)collector).GetTotalHits() :
                    ((TopScoreDocCollector)collector).GetTotalHits();

                FinishCollectingTime();
                //====================================================

                BeginPagingTime();
                FinishPagingTime();

                return EmptyResult;
            }
            catch
            {
                FinishKernelTime();
                FinishCollectingTime();
                FinishPagingTime();

                throw;
            }
            finally
            {
                if (searcher != null)
                    searcher.Close();
                searcher = null;
                FinishFullExecutingTime();
            }
        }
    }

    internal abstract class QueryExecutor2 : IQueryExecutor
    {
        protected LucQuery LucQuery { get; private set; }
        internal long FullExecutingTime { get; private set; }
        public int TotalCount { get; private set; }

        private Stopwatch timer;
        protected void BeginFullExecutingTime()
        {
            timer = Stopwatch.StartNew();
        }
        protected void FinishFullExecutingTime()
        {
            if (timer == null)
                return;
            FullExecutingTime = timer.ElapsedTicks;
            timer.Stop();
        }

        public IEnumerable<LucObject> Execute(LucQuery lucQuery, bool allVersions)
        {
            this.LucQuery = lucQuery;
            using (var traceOperation = Logger.TraceOperation("Query execution", "Query: " + this.LucQuery.QueryText))
            {
                Query currentQuery;

                if (this.LucQuery.EnableAutofilters || this.LucQuery.EnableLifespanFilter)
                {
                    var fullQuery = new BooleanQuery();
                    fullQuery.Add(new BooleanClause(this.LucQuery.Query, BooleanClause.Occur.MUST));

                    if (this.LucQuery.EnableAutofilters)
                        fullQuery.Add(new BooleanClause(this.LucQuery.AutoFilterQuery, BooleanClause.Occur.MUST));
                    if (this.LucQuery.EnableLifespanFilter && this.LucQuery.LifespanQuery != null)
                        fullQuery.Add(new BooleanClause(this.LucQuery.LifespanQuery, BooleanClause.Occur.MUST));

                    currentQuery = fullQuery;
                }
                else
                {
                    currentQuery = this.LucQuery.Query;
                }

                //var idxReader = LuceneManager.IndexReader;
                SearchResult r = null;
                using (var readerFrame = LuceneManager.GetIndexReaderFrame())
                {
                    var idxReader = readerFrame.IndexReader;

                    BeginFullExecutingTime();
                    try
                    {
                        r = DoExecute(currentQuery, allVersions, idxReader, timer);
                    }
                    finally
                    {
                        FinishFullExecutingTime();
                    }
                }
                TotalCount = r.totalCount;

                var searchtimer = r.searchTimer;
                var trace = lucQuery.TraceInfo;
                trace.KernelTime = searchtimer.KernelTime;
                trace.CollectingTime = searchtimer.CollectingTime;
                trace.PagingTime = searchtimer.PagingTime;
                trace.FullExecutingTime = FullExecutingTime;
                trace.Searches = r.searches;

                traceOperation.AdditionalObject = trace;
                traceOperation.IsSuccessful = true;
                return r.result;
            }
        }

        protected abstract SearchResult DoExecute(Query query, bool allVersions, IndexReader idxReader, Stopwatch timer);

        protected bool IsPermitted(Document doc, IUser user, bool isCurrentUser)
        {
            var path = doc.Get(LucObject.FieldName.Path);

            var createdById = IntegerIndexHandler.ConvertBack(doc.Get(LucObject.FieldName.CreatedById));
            var lastModifiedById = IntegerIndexHandler.ConvertBack(doc.Get(LucObject.FieldName.ModifiedById));
            var isLastPublic = BooleanIndexHandler.ConvertBack(doc.Get(LucObject.FieldName.IsLastPublic));
            var isLastDraft = BooleanIndexHandler.ConvertBack(doc.Get(LucObject.FieldName.IsLastDraft));
            var level = isCurrentUser
                ? SecurityHandler.GetPermittedLevel(path, createdById, lastModifiedById)
                : SecurityHandler.GetPermittedLevel(path, createdById, lastModifiedById, user);
            switch (level)
            {
                case PermittedLevel.None:
                    return false;
                case PermittedLevel.HeadOnly:
                    return isLastPublic;
                case PermittedLevel.PublicOnly:
                    return isLastPublic;
                case PermittedLevel.All:
                    return isLastDraft;
                default:
                    throw new NotImplementedException();
            }
        }
    }
    internal class QueryExecutor20110503 : QueryExecutor2
    {
        protected override SearchResult DoExecute(Query query, bool allVersions, IndexReader idxReader, Stopwatch timer)
        {
            var numDocs = idxReader.NumDocs();

            var start = this.LucQuery.Skip;

            var maxtop = numDocs - start;
            if (maxtop < 1)
                return SearchResult.Empty;

            var user = this.LucQuery.User;
            var currentUser = AccessProvider.Current.GetCurrentUser();
            if (user == null)
                user = currentUser;
            var isCurrentUser = user.Id == currentUser.Id;

            int top = this.LucQuery.Top != 0 ? this.LucQuery.Top : this.LucQuery.PageSize;
            if (top == 0)
                top = int.MaxValue;

            var searcher = new IndexSearcher(idxReader);

            var p = new SearchParams
            {
                query = query,
                allVersions = allVersions,
                searcher = searcher,
                user = user,
                isCurrentUser = isCurrentUser,
                skip = start,
                timer = timer,
                top = top
            };

            SearchResult r = null;
            SearchResult r1 = null;
            try
            {
                var defaultTops = SenseNet.ContentRepository.Storage.StorageContext.Search.DefaultTopAndGrowth;
                var howManyList = new List<int>(defaultTops);
                if (howManyList[howManyList.Count - 1] == 0)
                    howManyList[howManyList.Count - 1] = int.MaxValue;

                if (top < int.MaxValue)
                {
                    var howMany = (top < int.MaxValue / 2) ? top * 2 : int.MaxValue; // numDocs; // * 4; // * 2;
                    if ((long)howMany > maxtop)
                        howMany = maxtop - start;
                    while (howManyList.Count > 0)
                    {
                        if (howMany < howManyList[0])
                            break;
                        howManyList.RemoveAt(0);
                    }
                    howManyList.Insert(0, howMany);
                }

                for (var i = 0; i < howManyList.Count; i++)
                {
                    var defaultTop = howManyList[i];
                    if (defaultTop == 0)
                        defaultTop = numDocs;

                    p.howMany = defaultTop;
                    p.useHowMany = i < howManyList.Count - 1;
                    var maxSize = i == 0 ? numDocs : r.totalCount;
                    p.collectorSize = Math.Min(defaultTop, maxSize - p.skip) + p.skip;

                    r1 = Search(p);

                    if (i == 0)
                        r = r1;
                    else
                        r.Add(r1);
                    p.skip += r.nextIndex;
                    p.top = top - r.result.Count;

                    if (r.result.Count >= top || r.result.Count >= r.totalCount)
                        break;
                }
                p.timer.Stop();
                return r;
            }
            finally
            {
                if (searcher != null)
                {
                    searcher.Close();
                    searcher = null;
                }
            }
        }
        private SearchResult Search(SearchParams p)
        {
            var r = new SearchResult(p.timer);
            var t = r.searchTimer;

            t.BeginKernelTime();
            var collector = CreateCollector(p.collectorSize);
            p.searcher.Search(p.query, collector);
            t.FinishKernelTime();

            t.BeginCollectingTime();
            //var topDocs = p.useHowMany ? collector.TopDocs(p.skip, p.howMany) : collector.TopDocs(p.skip);
            TopDocs topDocs = null;
            if (this.LucQuery.SortFields.Length > 0)
            {
                topDocs = p.useHowMany ? ((TopFieldCollector)collector).TopDocs(p.skip, p.howMany) : ((TopFieldCollector)collector).TopDocs(p.skip);
            }
            else
            {
                topDocs = p.useHowMany ? ((TopScoreDocCollector)collector).TopDocs(p.skip, p.howMany) : ((TopScoreDocCollector)collector).TopDocs(p.skip);
            }
            r.totalCount = topDocs.TotalHits;
            var hits = topDocs.ScoreDocs;

            t.FinishCollectingTime();

            t.BeginPagingTime();
            GetResultPage(hits, p, r);
            t.FinishPagingTime();

            return r;
        }
        private Collector CreateCollector(int size)
        {
            var docsScoredInOrder = false;
            if (this.LucQuery.SortFields.Length > 0)
            {
                var sort = new Sort(this.LucQuery.SortFields);
                var fillFields = false;
                var trackDocScores = true;
                var trackMaxScore = false;
                return TopFieldCollector.Create(sort, size, fillFields, trackDocScores, trackMaxScore, docsScoredInOrder);
            }
            return TopScoreDocCollector.Create(size, docsScoredInOrder);
        }
        private void GetResultPage(ScoreDoc[] hits, SearchParams p, SearchResult r)
        {
            var result = new List<LucObject>();
            if (hits.Length == 0)
            {
                r.result = result;
                return;
            }

            var upperBound = hits.Length;
            var index = 0;
            while (true)
            {
                Document doc = p.searcher.Doc(hits[index].Doc);
                if (p.allVersions || IsPermitted(doc, p.user, p.isCurrentUser))
                {
                    result.Add(new LucObject(doc));
                    if (result.Count == p.top)
                    {
                        index++;
                        break;
                    }
                }
                if (++index >= upperBound)
                    break;
            }
            r.nextIndex = index;
            r.result = result;
        }

        private class SearchParams
        {
            internal int collectorSize;
            internal Searcher searcher;
            internal Query query;
            internal int skip;
            internal int top;
            internal int howMany;
            internal bool useHowMany;
            internal bool allVersions;
            internal IUser user;
            internal bool isCurrentUser;
            internal Stopwatch timer;
        }

    }
    internal class SearchResult
    {
        public static readonly SearchResult Empty;

        static SearchResult()
        {
            Empty = new SearchResult(null) { searches = 0 };
        }

        internal SearchResult(Stopwatch timer)
        {
            searchTimer = new SearchTimer(timer);
        }

        internal SearchTimer searchTimer;
        internal List<LucObject> result;
        internal int totalCount;
        internal int nextIndex;
        internal int searches = 1;

        internal void Add(SearchResult other)
        {
            result.AddRange(other.result);
            nextIndex = other.nextIndex;
            searches += other.searches;

            searchTimer.CollectingTime += other.searchTimer.CollectingTime;
            searchTimer.KernelTime += other.searchTimer.KernelTime;
            searchTimer.PagingTime += other.searchTimer.PagingTime;
        }
    }
    internal class SearchTimer
    {
        public long KernelTime { get; internal set; }
        public long CollectingTime { get; internal set; }
        public long PagingTime { get; internal set; }

        public SearchTimer(Stopwatch timer)
        {
            this.timer = timer;
        }

        private Stopwatch timer;

        private long kernelStart;
        internal void BeginKernelTime()
        {
            kernelStart = timer.ElapsedTicks;
        }
        internal void FinishKernelTime()
        {
            KernelTime = timer.ElapsedTicks - kernelStart;
        }

        private long collectingStart;
        internal void BeginCollectingTime()
        {
            collectingStart = timer.ElapsedTicks;
        }
        internal void FinishCollectingTime()
        {
            CollectingTime = timer.ElapsedTicks - collectingStart;
        }

        private long pagingStart;
        public void BeginPagingTime()
        {
            pagingStart = timer.ElapsedTicks;
        }
        public void FinishPagingTime()
        {
            PagingTime = timer.ElapsedTicks - pagingStart;
        }
    }
}