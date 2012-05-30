using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Search;
using System.Diagnostics;
using SenseNet.Search.Parser;

namespace SenseNet.Search
{
    public class QueryTraceInfo
    {
        public string InputText { get; internal set; }
        public Query Query { get; internal set; }
        public long ParsingTime { get; internal set; }
        public long CrossCompilingTime { get; internal set; }
        public long KernelTime { get; internal set; }
        public long CollectingTime { get; internal set; }
        public long PagingTime { get; internal set; }
        public long FullExecutingTime { get; internal set; }
        public int Searches { get; internal set; }

        public string ParsedText { get { return Query == null ? string.Empty : Query.ToString(); } } // GetSafeString(Query.ToString())
        public string ObjectTree { get { return Query == null ? string.Empty : ObjectDump(Query); } }

        public static string ObjectDump(Query query)
        {
            var v = new DumpVisitor();
            v.Visit(query);
            return v.ToString();
        }

        //private Stopwatch timer;
        //internal void BeginFullExecutingTime()
        //{
        //    timer = Stopwatch.StartNew();
        //}
        //internal void FinishFullExecutingTime()
        //{
        //    if (timer == null)
        //        return;
        //    FullExecutingTime = timer.ElapsedTicks;
        //    timer.Stop();
        //}

        //private long kernelStart;
        //internal void BeginKernelTime()
        //{
        //    kernelStart = timer.ElapsedTicks;
        //}
        //internal void FinishKernelTime()
        //{
        //    if (timer == null)
        //        return;
        //    KernelTime = timer.ElapsedTicks - kernelStart;
        //}

        //private long collectingStart;
        //internal void BeginCollectingTime()
        //{
        //    collectingStart = timer.ElapsedTicks;
        //}
        //internal void FinishCollectingTime()
        //{
        //    if (timer == null)
        //        return;
        //    CollectingTime = timer.ElapsedTicks - collectingStart;
        //}

        //private long pagingStart;
        //public void BeginPagingTime()
        //{
        //    pagingStart = timer.ElapsedTicks;
        //}
        //public void FinishPagingTime()
        //{
        //    if (timer == null)
        //        return;
        //    PagingTime = timer.ElapsedTicks - pagingStart;
        //}

        private Stopwatch parsingTimer;
        internal void BeginParsingTime()
        {
            parsingTimer = Stopwatch.StartNew();
        }
        internal void FinishParsingTime()
        {
            if (parsingTimer == null)
                return;
            ParsingTime = parsingTimer.ElapsedTicks;
            parsingTimer.Stop();
        }

        private Stopwatch crossCompilingTimer;
        internal void BeginCrossCompilingTime()
        {
            crossCompilingTimer = Stopwatch.StartNew();
        }
        internal void FinishCrossCompilingTime()
        {
            if (crossCompilingTimer == null)
                return;
            CrossCompilingTime = crossCompilingTimer.ElapsedTicks;
            crossCompilingTimer.Stop();
        }

        public override string ToString()
        {
            return ToString(false);
        }
        public string ToString(bool full)
        {
            if (full)
                return String.Format("FullExecutingTime: {0}, Searches: {1}, ParsingTime: {2}, CrossCompilingTime: {3}, KernelTime: {4}, CollectingTime: {5}, PagingTime: {6}, InputText: {7}, ParsedText: {8}, ObjectTree: {9}.",
                    this.FullExecutingTime,
                    this.Searches,
                    this.ParsingTime,
                    this.CrossCompilingTime,
                    this.KernelTime,
                    this.CollectingTime,
                    this.PagingTime,
                    this.InputText,
                    this.ParsedText,
                    this.ObjectTree
                    );
            return String.Format("FullExecutingTime: {0}, Searches: {1}, KernelTime: {2}, CollectingTime: {3}, PagingTime: {4}.",
                this.FullExecutingTime,
                this.Searches,
                this.KernelTime,
                this.CollectingTime,
                this.PagingTime
                );
        }
        //private string GetSafeString(string s)
        //{
        //    var chars = s.ToCharArray();
        //    for (int i = 0; i < chars.Length; i++)
        //        if (chars[i] < ' ')
        //            chars[i] = '.';
        //    return new String(chars);
        //}
    }
}
