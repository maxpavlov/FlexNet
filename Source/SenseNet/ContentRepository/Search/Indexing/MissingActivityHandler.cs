using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using System.Threading;

namespace SenseNet.Search.Indexing
{
    public class MissingActivityHandler
    {
        private static int _peakGapSize;
        private static ReaderWriterLockSlim _gapRWSync = new ReaderWriterLockSlim();
        internal static readonly int GapSegments;
        private static HashSet<int>[] _gaps;
        private static int _currentGapIndex = 0;
        private static HashSet<int> _currentGap;

        private static readonly string COUNTERNAME_GAPSIZE = "GapSize";

        private static int _maxActivityId;
        public static int MaxActivityId
        {
            get { return _maxActivityId; }
            set
            {
                _gapRWSync.EnterWriteLock();
                try
                {
                    _maxActivityId = value;
                }
                finally
                {
                    _gapRWSync.ExitWriteLock();
                }
            }
        }

        static MissingActivityHandler()
        {
            try
            {
                CounterManager.Reset(COUNTERNAME_GAPSIZE);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }


            GapSegments = SenseNet.ContentRepository.Storage.Data.RepositoryConfiguration.IndexHealthMonitorGapPartitions;
            _gaps = new HashSet<int>[GapSegments];
            for (int i = 0; i < GapSegments; i++)
                _gaps[i] = new HashSet<int>();
            _currentGap = _gaps[_currentGapIndex];
        }

        //public static Tuple<int, int> GetGapSizeInfo()
        //{
        //    _gapRWSync.EnterReadLock();
        //    try
        //    {
        //        var gapSize = 0;
        //        for (int i = 0; i < GapSegments; i++)
        //            gapSize += _gaps[i].Count;

        //        _peakGapSize = Math.Max(_peakGapSize, gapSize);
        //        return new Tuple<int, int>(gapSize, _peakGapSize);
        //    }
        //    finally
        //    {
        //        _gapRWSync.ExitReadLock();
        //    }
        //}

        public static void SetGapSizeCounter()
        {
            var result = 0;
            for (int i = 0; i < GapSegments; i++)
                result += _gaps[i].Count;

            // set perf counter
            CounterManager.SetRawValue(COUNTERNAME_GAPSIZE, Convert.ToInt64(result));
        }

        internal static void RemoveActivityAndAddGap(int activityId)
        {
            _gapRWSync.EnterWriteLock();
            try
            {
                // remove from gap
                var removed = false;
                for (int i = _currentGapIndex + GapSegments; i > _currentGapIndex; i--)
                {
                    if (_gaps[i % GapSegments].Remove(activityId))
                        removed = true;
                }

                SenseNet.Search.Indexing.Activities.DistributedLuceneActivity.WriteMSMQLog(activityId, SenseNet.Search.Indexing.Activities.DistributedLuceneActivity.MSMQLogStatusType.REMOVED, null);

                // if this activityid was in a gap, then it's preceding activities have already been added to the gap, no need to add them again 
                if (!removed)
                {
                    // add preceding activities to gap if they have not yet been processed
                    if (activityId - 1 > _maxActivityId)
                    {
                        for (var i = _maxActivityId + 1; i < activityId; i++)
                        {
                            _currentGap.Add(i);
                            SenseNet.Search.Indexing.Activities.DistributedLuceneActivity.WriteMSMQLog(i, SenseNet.Search.Indexing.Activities.DistributedLuceneActivity.MSMQLogStatusType.ADDED, activityId);
                        }
                    }
                }
                _maxActivityId = Math.Max(_maxActivityId, activityId);
            }
            finally
            {
                _gapRWSync.ExitWriteLock();
            }
        }
        internal static Tuple<string, int> GetGapString()
        {
            _gapRWSync.EnterReadLock();
            try
            {
                var sb = new StringBuilder();
                for (int i = 0; i < GapSegments; i++)
                {
                    if (_gaps[i].Count == 0)
                        continue;
                    if (sb.Length > 0)
                        sb.Append(',');
                    sb.Append(String.Join(",", _gaps[i]));
                }
                return new Tuple<string, int>(sb.ToString(), _maxActivityId);
            }
            finally
            {
                _gapRWSync.ExitReadLock();
            }
        }
        internal static List<int> GetGap()
        {
            var result = new List<int>();
            _gapRWSync.EnterReadLock();
            try
            {
                for (int i = 0; i < GapSegments; i++)
                    result.AddRange(_gaps[i]);
            }
            finally
            {
                _gapRWSync.ExitReadLock();
            }
            return result;
        }
        internal static int[] GetOldestGapAndMoveToNext()
        {
            _gapRWSync.EnterWriteLock();
            try
            {
                _currentGap = _gaps[_currentGapIndex = ++_currentGapIndex % GapSegments];
                return _currentGap.ToArray();
            }
            finally
            {
                _gapRWSync.ExitWriteLock();
            }
        }
        internal static void SetGap(List<int> gap)
        {
            _gapRWSync.EnterWriteLock();
            try
            {
                for (int i = 0; i < GapSegments; i++)
                    _gaps[i].Clear();

                foreach (var actId in gap)
                    _currentGap.Add(actId);
            }
            finally
            {
                _gapRWSync.ExitWriteLock();
            }
        }
    }
}
