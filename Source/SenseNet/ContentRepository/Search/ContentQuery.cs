using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using System.Xml;
using System.Text;

namespace SenseNet.Search
{
    public class ContentQuery : IContentQuery
    {
        public static readonly string EmptyText = "$##$EMPTY$##$";
        internal static readonly string EmptyInnerQueryText = "$##$EMPTYINNERQUERY$##$";

        private static readonly string[] QuerySettingParts = new[] { "SKIP", "TOP", "SORT", "REVERSESORT", "AUTOFILTERS", "LIFESPAN", "COUNTONLY" };

        public static ContentQuery CreateQuery(string text)
        {
            return new ContentQuery { Text = text };
        }
        public static ContentQuery CreateQuery(string text, QuerySettings settings)
        {
            return new ContentQuery { Text = text, Settings = settings };
        }
        public static QueryResult Query(string text)
        {
            return CreateQuery(text).Execute(ExecutionHint.None);
        }
        public static QueryResult Query(string text, QuerySettings settings)
        {
            return CreateQuery(text, settings).Execute(ExecutionHint.None);
        }

        //================================================================== IContentQuery Members

        public string Text { get; set; }
        public int TotalCount { get; private set; }

        private QuerySettings _settings;
        public QuerySettings Settings
        {
            get { return _settings ?? (_settings = new QuerySettings()); }
            set { _settings = value; }
        }

        public bool IsNodeQuery
        {
            get { return !string.IsNullOrEmpty(Text) && Text.StartsWith("<"); }
        }
        public bool IsContentQuery
        {
            get { return !string.IsNullOrEmpty(Text) && !Text.StartsWith("<"); }
        }

        public void AddClause(string text)
        {
            AddClause(text, ChainOperator.And);
        }
        public void AddClause(string text, ChainOperator chainOp)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (text.Length == 0)
                throw new ArgumentException("Clause cannot be empty", "text");

            if (string.IsNullOrEmpty(this.Text))
                this.Text = text;
            else
            {
                switch (chainOp)
                {
                    case ChainOperator.And:
                        this.Text = MoveSettingsToTheEnd(string.Format("+({0}) +({1})", Text, text)).Trim();
                        break;
                    case ChainOperator.Or:
                        this.Text = MoveSettingsToTheEnd(string.Format("({0}) {1}", Text, text));
                        break;
                }
            }
        }

        public static string AddClause(string originalText, string addition, ChainOperator chainOp)
        {
            if (addition == null)
                throw new ArgumentNullException("addition");
            if (addition.Length == 0)
                throw new ArgumentException("Clause cannot be empty", "addition");

            if (string.IsNullOrEmpty(originalText))
                return addition;

            var queryText = string.Empty;

            switch (chainOp)
            {
                case ChainOperator.And:
                    queryText = MoveSettingsToTheEnd(string.Format("+({0}) +({1})", originalText, addition)).Trim();
                    break;
                case ChainOperator.Or:
                    queryText = MoveSettingsToTheEnd(string.Format("({0}) {1}", originalText, addition));
                    break;
            }

            return queryText;
        }

        public void AddClause(Expression expression)
        {
            AddClause(expression, ChainOperator.And);
        }
        public void AddClause(Expression expression, ChainOperator chainOp)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            ExpressionList finalExpList;
            var origExpList = expression as ExpressionList;
            
            if (origExpList != null)
            {
                finalExpList = origExpList;
            }
            else
            {
                finalExpList = new ExpressionList(chainOp);
                finalExpList.Add(expression);
            }

            this.Text = AddFilterToNodeQuery(Text, finalExpList.ToXml());
        }

        public QueryResult Execute()
        {
            return Execute(ExecutionHint.None);
        }
        public QueryResult Execute(ExecutionHint hint)
        {
            return new QueryResult(GetIdResults(hint), TotalCount);
        }
        public IEnumerable<int> ExecuteToIds()
        {
            return ExecuteToIds(ExecutionHint.None);
        }
        public IEnumerable<int> ExecuteToIds(ExecutionHint hint)
        {
            //We need to get the pure id list for one single query.
            //If you run Execute, it returns a NodeList that loads
            //all result ids, not only the page you specified.
            return GetIdResults(hint);
        }

        //------------------------------------------------------------------

        public IEnumerable<QueryTraceInfo> TraceInfo { get; private set; }
        public string GetTraceInfo()
        {
            var sb = new StringBuilder();
            long sumParsingTime = 0;
            long sumCrossCompilingTime = 0;
            long sumKernelTime = 0;
            long sumCollectingTime = 0;
            long sumPagingTime = 0;
            long sumFullExecutingTime = 0;
            var pass = 0;
            foreach (var info in TraceInfo)
            {
                sumParsingTime += info.ParsingTime;
                sumCrossCompilingTime += info.CrossCompilingTime;
                sumKernelTime += info.KernelTime;
                sumCollectingTime += info.CollectingTime;
                sumPagingTime += info.PagingTime;
                sumFullExecutingTime += info.FullExecutingTime;
                sb.Append("Pass#").Append(++pass).AppendLine();
                sb.Append("  Input text:\t").AppendLine(info.InputText);
                sb.Append("  Parsed text:\t").AppendLine(info.ParsedText);
                sb.Append("  Full executing time:\t").AppendLine(new TimeSpan(info.FullExecutingTime).ToString());
                sb.Append("  Parsing time:\t").AppendLine(new TimeSpan(info.ParsingTime).ToString());
                sb.Append("  Cross compiling time:\t").AppendLine(new TimeSpan(info.CrossCompilingTime).ToString());
                sb.Append("  Kernel time:\t").AppendLine(new TimeSpan(info.KernelTime).ToString());
                sb.Append("  Collecting time:\t").AppendLine(new TimeSpan(info.CollectingTime).ToString());
                sb.Append("  Paging time:\t").AppendLine(new TimeSpan(info.PagingTime).ToString());
                sb.Append("  Object tree:\t").AppendLine(info.ObjectTree);
            }
            if (TraceInfo.Count() > 1)
            {
                sb.AppendLine("=================");
                sb.AppendLine("Summarize: ");
                sb.Append("  Full executing time:\t").AppendLine(new TimeSpan(sumFullExecutingTime).ToString());
                sb.Append("  Cross compiling time:\t").AppendLine(new TimeSpan(sumCrossCompilingTime).ToString());
                sb.Append("  Kernel time:\t").AppendLine(new TimeSpan(sumKernelTime).ToString());
                sb.Append("  Collecting time:\t").AppendLine(new TimeSpan(sumCollectingTime).ToString());
                sb.Append("  Paging time:\t").AppendLine(new TimeSpan(sumPagingTime).ToString());
            }
            return sb.Replace('\0','.').ToString();
        }

        //================================================================== Get result ids

        private IEnumerable<int> GetIdResults(ExecutionHint hint)
        {
            return GetIdResults(hint, Settings.Top, Settings.Skip, Settings.Sort, 
                Settings.EnableAutofilters, Settings.EnableLifespanFilter);
        }
        private IEnumerable<int> GetIdResults(ExecutionHint hint, int top, int skip, IEnumerable<SortInfo> sort, 
            bool? enableAutofilters, bool? enableLifespanFilter)
        {
            if (IsNodeQuery)
                return GetIdResultsWithNodeQuery(hint, top, skip, sort, enableAutofilters, enableLifespanFilter);
            if (IsContentQuery)
                return GetIdResultsWithLucQuery(top, skip, sort, enableAutofilters, enableLifespanFilter);

            throw new InvalidOperationException("Cannot execute query with null or empty Text");
        }
        private IEnumerable<int> GetIdResultsWithLucQuery(int top, int skip, IEnumerable<SortInfo> sort,
            bool? enableAutofilters, bool? enableLifespanFilter)
        {
            var queryText = Text;

            if (!queryText.Contains("}}"))
            {
                var query = LucQuery.Parse(queryText);
                if (skip != 0)
                    query.Skip = skip;

                query.Top = System.Math.Min(top == 0 ? int.MaxValue : top, query.Top == 0 ? int.MaxValue : query.Top);
                if (query.Top == 0)
                    query.Top = GetDefaultMaxResults();

                query.PageSize = query.Top;

                if (sort != null && sort.Count() > 0)
                    query.SetSort(sort);

                if (enableAutofilters.HasValue)
                    query.EnableAutofilters = enableAutofilters.Value;
                if (enableLifespanFilter.HasValue)
                    query.EnableLifespanFilter = enableLifespanFilter.Value;

                //Re-set settings values. This is important for NodeList that
                //uses the paging info written into the query text.
                this.Settings.Top = query.PageSize;
                this.Settings.Skip = query.Skip;
                //this.Settings.Sort = we don't need this

                this.TraceInfo = new[] { query.TraceInfo };

                var lucObjects = query.Execute().ToList();

                TotalCount = query.TotalCount;

                return (from luco in lucObjects
                        select luco.NodeId).ToList();
            }
            else
            {
                List<string> log;
                int count;
                IEnumerable<QueryTraceInfo> traceInfo;
                var result = RecursiveExecutor.ExecuteRecursive(queryText, top, skip,
                            sort, enableAutofilters, enableLifespanFilter, this.Settings, out count, out log, out traceInfo);

                TotalCount = count;

                this.TraceInfo = traceInfo;

                return result;
            }
        }
        private IEnumerable<int> GetIdResultsWithNodeQuery(ExecutionHint hint, int top, int skip, IEnumerable<SortInfo> sort,
            bool? enableAutofilters, bool? enableLifespanFilter)
        {
            var queryText = enableAutofilters.GetValueOrDefault() ? AddAutofilterToNodeQuery(Text) : Text;
            if (enableLifespanFilter.GetValueOrDefault())
                queryText = AddLifespanFilterToNodeQuery(queryText, GetLifespanFilterForNodeQuery());

            var query = NodeQuery.Parse(queryText);
            if(skip != 0)
                query.Skip = skip;

            if (top != 0)
                query.Top = top;
            else
                if (query.Top == 0)
                    query.Top = GetDefaultMaxResults();

            query.PageSize = query.Top;

            if (sort != null && sort.Count() > 0)
                throw new NotSupportedException("Sorting override is not allowed on NodeQuery");

            var result = query.Execute(hint);
            TotalCount = result.Count;

            return result.Identifiers.ToList();
        }

        //================================================================== Filter methods

        public static string AddAutofilterToNodeQuery(string originalText)
        {
            return AddFilterToNodeQuery(originalText, GetAutofilterForNodeQuery());
        }

        public static string AddFilterToNodeQuery(string originalText, string filterText)
        {
            if (string.IsNullOrEmpty(filterText))
                return originalText;

            var filterXml = new XmlDocument();
            filterXml.LoadXml(filterText);
            var filterTopLogicalElement = (XmlElement)filterXml.SelectSingleNode("/*/*[1]");
            if (filterTopLogicalElement == null)
                return originalText;
            var filterInnerXml = filterTopLogicalElement.InnerXml;
            if(String.IsNullOrEmpty(filterInnerXml))
                return originalText;

            var originalXml = new XmlDocument();
            originalXml.LoadXml(originalText);
            var originalTopLogicalElement = (XmlElement)originalXml.SelectSingleNode("/*/*[1]");
            if (originalTopLogicalElement == null)
                return originalText;
            var originalOuterXml = originalTopLogicalElement.OuterXml;
            if (String.IsNullOrEmpty(originalOuterXml))
                return originalText;

            filterTopLogicalElement.InnerXml = String.Concat(filterInnerXml, originalOuterXml);

            return filterXml.OuterXml;
        }

        public static string AddLifespanFilterToNodeQuery(string originalText, string filterText)
        {
            if (string.IsNullOrEmpty(filterText))
                return originalText;

            return originalText;
        }

        /// <summary>
        /// Moves .TOP and .SKIP clauses from inside to the end of the query
        /// </summary>
        /// <param name="queryText">Original query text</param>
        /// <returns>Query text with skip and top values moved to the end</returns>
        private static string MoveSettingsToTheEnd(string queryText)
        {
            var backParts = string.Empty;

            foreach (var settingPart in QuerySettingParts)
            {
                var hasValue = settingPart != "COUNTONLY";
                var templatePattern = hasValue ? string.Format("\\.{0}[ ]*:[ ]*[^) ]+", settingPart) : string.Format("(?<key>\\.{0})[) ]+", settingPart);
                var index = 0;
                var regex = new Regex(templatePattern, RegexOptions.IgnoreCase);

                while (true)
                {
                    var match = regex.Match(queryText, index);
                    if (!match.Success)
                        break;

                    if (hasValue)
                    {
                        queryText = queryText.Remove(match.Index, match.Length);
                        index = match.Index + match.Length;
                        backParts += " " + match.Value;
                    }
                    else
                    {
                        var group = match.Groups["key"];
                        queryText = queryText.Remove(group.Index, group.Length);
                        index = group.Index + group.Length;
                        backParts += " " + group.Value;
                    }

                    if (index >= queryText.Length)
                        break;
                }
            }

            return string.Concat(queryText, backParts);
        }

        private static int GetDefaultMaxResults()
        {
            return int.MaxValue;
        }
        private static string GetAutofilterForNodeQuery()
        {
            return "";
        }
        private static string GetLifespanFilterForNodeQuery()
        {
            return "";
        }

        //================================================================== Recursive executor class

        private static class RecursiveExecutor
        {
            private class InnerQueryResult
            {
                internal bool IsIntArray;
                internal string[] StringArray;
                internal int[] IntArray;
            }

            public static IEnumerable<int> ExecuteRecursive(string queryText, int top, int skip, 
                IEnumerable<SortInfo> sort, bool? enableAutofilters, bool? enableLifespanFilter, 
                QuerySettings settings, out int count, out List<string> log, out IEnumerable<QueryTraceInfo> traceInfo)
            {
                log = new List<string>();
                IEnumerable<int> result = new int[0];
                var src = queryText;
                log.Add(src);
                var control = GetControlString(src);
                var trace = new List<QueryTraceInfo>();

                while (true)
                {
                    int start;
                    var sss = GetInnerScript(src, control, out start);
                    var end = sss == String.Empty;
                    QueryTraceInfo traceItem;

                    if (!end)
                    {
                        src = src.Remove(start, sss.Length);
                        control = control.Remove(start, sss.Length);

                        int innerCount;
                        var innerResult = ExecuteInnerScript(sss.Substring(2, sss.Length - 4), 0, 0,
                            sort, enableAutofilters, enableLifespanFilter, null, true, out innerCount,
                            out traceItem).StringArray;

                        switch (innerResult.Length)
                        {
                            case 0:
                                sss = EmptyInnerQueryText;
                                break;
                            case 1:
                                sss = innerResult[0];
                                break;
                            default:
                                sss = String.Join(" ", innerResult);
                                sss = "(" + sss + ")";
                                break;
                        }
                        src = src.Insert(start, sss);
                        control = control.Insert(start, sss);
                        trace.Add(traceItem);
                        log.Add(src);
                    }
                    else
                    {
                        result = ExecuteInnerScript(src, top, skip, sort, enableAutofilters, enableLifespanFilter, 
                            settings, false, out count, out traceItem).IntArray;

                        trace.Add(traceItem);
                        log.Add(String.Join(" ", result.Select(i => i.ToString()).ToArray()));
                        break;
                    }
                }
                traceInfo = trace;
                return result;
            }
            private static string GetControlString(string src)
            {
                var s = src.Replace("\\'", "__").Replace("\\\"", "__");
                var @out = new StringBuilder(s.Length);
                var instr = false;
                var strlimit = '\0';
                var esc = false;
                foreach (var c in s)
                {
                    if (c == '\\')
                    {
                        esc = true;
                        @out.Append('_');
                    }
                    else
                    {
                        if (esc)
                        {
                            esc = false;
                            @out.Append('_');
                        }
                        else
                        {
                            if (instr)
                            {
                                if (c == strlimit)
                                    instr = !instr;
                                @out.Append('_');
                            }
                            else
                            {
                                if (c == '\'' || c == '"')
                                {
                                    instr = !instr;
                                    strlimit = c;
                                    @out.Append('_');
                                }
                                else
                                {
                                    @out.Append(c);
                                }
                            }
                        }
                    }
                }

                var l0 = src.Length;
                var l1 = @out.Length;

                return @out.ToString();
            }
            private static string GetInnerScript(string src, string control, out int start)
            {
                start = 0;
                var p1 = control.IndexOf("}}");
                if (p1 < 0)
                    return String.Empty;
                var p0 = control.LastIndexOf("{{", p1);
                if (p0 < 0)
                    return String.Empty;
                start = p0;
                var ss = src.Substring(p0, p1 - p0 + 2);
                return ss;
            }

            private static InnerQueryResult ExecuteInnerScript(string src, int top, int skip,
                IEnumerable<SortInfo> sort, bool? enableAutofilters, bool? enableLifespanFilter, 
                QuerySettings settings, bool enableProjection, out int count, out QueryTraceInfo traceInfo)
            {
                var query = LucQuery.Parse(src);

                var projection = query.Projection;
                if (projection != null)
                    if (!enableProjection)
                        throw new ApplicationException(String.Format("Projection in top level query is not allowed ({0}:{1})", Parser.SnLucLexer.Keywords.Select, projection));

                if (skip != 0)
                    query.Skip = skip;

                if (top != 0)
                    query.PageSize = top;
                else
                    if (query.PageSize == 0)
                        query.PageSize = GetDefaultMaxResults();

                if (sort != null && sort.Count() > 0)
                    query.SetSort(sort);

                if (enableAutofilters.HasValue)
                    query.EnableAutofilters = enableAutofilters.Value;
                if (enableLifespanFilter.HasValue)
                    query.EnableLifespanFilter = enableLifespanFilter.Value;

                //Re-set settings values. This is important for NodeList that
                //uses the paging info written into the query text.
                if (settings != null)
                {
                    settings.Top = query.PageSize;
                    settings.Skip = query.Skip;
                }

                InnerQueryResult result;

                var qresult = query.Execute().ToList();
                if (projection == null || !enableProjection)
                {
                    var idResult = qresult.Select(o => o.NodeId).ToArray();
                    result = new InnerQueryResult { IsIntArray = true, IntArray = idResult, StringArray = idResult.Select(i => i.ToString()).ToArray() };
                }
                else
                {
                    var stringResult = qresult.Select(o => o[projection, false]).Where(r => !String.IsNullOrEmpty(r));
                    var escaped = new List<string>();
                    foreach (var s in stringResult)
                        escaped.Add(EscapeForQuery(s));
                    result = new InnerQueryResult { IsIntArray = false, StringArray = escaped.ToArray() };
                }

                traceInfo = query.TraceInfo;
                count = query.TotalCount;

                return result;
            }

            private static object __escaperRegexSync = new object();
            private static Regex __escaperRegex;
            private static Regex EscaperRegex
            {
                get
                {
                    if (__escaperRegex == null)
                    {
                        lock (__escaperRegexSync)
                        {
                            if (__escaperRegex == null)
                            {
                                var pattern = new StringBuilder("[");
                                foreach (var c in SenseNet.Search.Parser.SnLucLexer.STRINGTERMINATORCHARS.ToCharArray())
                                    pattern.Append("\\" + c);
                                pattern.Append("]");
                                __escaperRegex = new Regex(pattern.ToString());
                            }
                        }
                    }
                    return __escaperRegex;
                }
            }

            public static string EscapeForQuery(string value)
            {
                if (EscaperRegex.IsMatch(value))
                    return String.Concat("'", value, "'");
                return value;
            }
        }

    }
}
