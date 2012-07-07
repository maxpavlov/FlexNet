using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace SenseNet.Search.Indexing
{
    [Serializable]
    internal abstract class LuceneActivity
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

        internal void InternalExecute()
        {
            try
            {
                using (new SenseNet.ContentRepository.Storage.Security.SystemAccount())
                    Execute();
            }
            finally
            {
                if (FinishSignal != null)
                    FinishSignal.Set();
            }
        }

        internal abstract void Execute();

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
