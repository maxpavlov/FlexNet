using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace SenseNet.Search.Indexing
{
    [Serializable]
    public abstract class QueuedActivity
    {

        [NonSerialized]
        private AutoResetEvent _finishSignal = new AutoResetEvent(false);

        public AutoResetEvent FinishSignal
        {
            get
            {
                return _finishSignal;
            }
        }

        internal void InternalExecute(ActivityQueue activityQueue)
        {
            try
            {
                //activityQueue can be null if we are invoked directly without a queue
                //if (activityQueue != null)
                Execute();
                //else
                //Debug.WriteLine("@@##$$> activityQueue parameter is null in QueuedActivity.InternalExecute");
            }
            finally
            {
                if (FinishSignal != null)
                    FinishSignal.Set();
            }
        }

        public abstract void Execute();

        public void WaitForComplete()
        {
            if (Debugger.IsAttached)
            {
                FinishSignal.WaitOne();
            }
            else
            {
                if (!FinishSignal.WaitOne(30000, false))
                {
                    throw new ApplicationException("Activity is not finishing on a timely manner");
                }
            }
        }
    }
}
