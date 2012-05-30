using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SenseNet.Diagnostics
{
    
    public class OperationTrace : IDisposable
    {
        private string _operationName;
        private string _title;
        private List<string> _categories;

        public readonly Guid OperationId = Guid.NewGuid();

        public bool IsSuccessful { get; set; }
        public object AdditionalObject { get; set; }

        public OperationTrace(string operationName) : this(operationName, string.Empty, Logger.EmptyCategoryList) { }
        public OperationTrace(string operationName, string title) : this(operationName, title, Logger.EmptyCategoryList) { }
        public OperationTrace(string operationName, string title, IEnumerable<string> categories)
        {
            _categories = categories.ToList();
            if (Tracing.Enabled)
            {
                _operationName = operationName;
                _title = title;
                if (GetActivityId().Equals(Guid.Empty))
                {
                    SetActivityId(Guid.NewGuid());
                }
                Start();
            }
        }
        //public OperationTrace(string operationName, Guid operationId)
        //{
        //    if (Tracing.Enabled)
        //    {
        //        _operationName = operationName;
        //        SetActivityId(operationId);
        //        Start();
        //    }
        //}

        private System.Diagnostics.Stopwatch sw;

        private string GetCallerMethodName()
        {
            var method = Utility.GetOriginalCaller(this);
            if (method == null) return "Unknown";
            return method.ToString();
        }

        long startTicks;

        public void Start()
        {
            if (Tracing.Enabled)
            {
                StartLogicalOperation(_operationName);
                sw = Stopwatch.StartNew();
                startTicks = Stopwatch.GetTimestamp();

                WriteStartLog();
            }
        }

        bool finished = false;

        public void Finish()
        {
            if (Tracing.Enabled)
            {
                if (!finished)
                {
                    finished = true;
                    WriteFinishLog();
                    StopLogicalOperation();
                }
            }
            //throw new NotImplementedException();
        }

        private void WriteFinishLog()
        {
            long finisTicks = Stopwatch.GetTimestamp();
            decimal secondsElapsed = GetSecondsElapsed(sw.ElapsedMilliseconds);
            string methodName = GetCallerMethodName();
            var id =  GetActivityId();

            var additionalMessage = String.Empty;
            if (AdditionalObject != null)
            {
                try { additionalMessage = ", additional info: " + AdditionalObject; }
                catch { }
            }

            string message = string.Format("Finish {0}: {1}, id: {2}, method:{3}, ticks: {4}, seconds:{5}{6}", 
                IsSuccessful ? "successful" : "UNSUCCESSFUL",
                PeekLogicalOperation() as string, id, methodName, finisTicks, secondsElapsed, additionalMessage);
            Tracing.OnOperationEnd(message, IsSuccessful, this.OperationId, finisTicks, secondsElapsed);

            Logger.Write(message, _categories, TraceEventType.Stop);
        }
        private decimal GetSecondsElapsed(long milliseconds)
        {
            decimal result = Convert.ToDecimal(milliseconds) / 1000m;
            return Math.Round(result, 6);
        }

        private void WriteStartLog()
        {
            var id = GetActivityId();
            var caller = GetCallerMethodName();
            //string message = string.Format(Resources.Culture, Resources.Tracer_StartMessageFormat, GetActivityId(), methodName, tracingStartTicks);
            string message = string.Format("Start: {0}, id: {1}, method:{2}, ticks:{3}",
                PeekLogicalOperation() as string, id, caller, startTicks);
            Tracing.OnOperationStart(this.OperationId, _operationName, _title, message, caller, startTicks);
            Logger.Write(message, _categories, TraceEventType.Start);
        }



        public Guid GetActivityId()
        {
            return Trace.CorrelationManager.ActivityId;
        }
        public void SetActivityId(Guid guid)
        {
            Trace.CorrelationManager.ActivityId = guid;
        }
        public void StartLogicalOperation(string opname)
        {
            Trace.CorrelationManager.StartLogicalOperation(opname);
        }
        public object PeekLogicalOperation()
        {
            return Trace.CorrelationManager.LogicalOperationStack.Peek();
        }
        public void StopLogicalOperation()
        {
            Trace.CorrelationManager.StopLogicalOperation();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool disposed = false;

        public void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                Finish();
            }
        }

        ~OperationTrace()
        {
            Dispose(false);
        }
        #endregion
    }

    
}
