using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Communication.Messaging;
using System.Diagnostics;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    public abstract class DistributedLuceneActivity : QueuedActivity
    {
        [Serializable]
        public class LuceneActivityDistributor : DistributedAction
        {
            public DistributedLuceneActivity Activity;
            
            public override void DoAction(bool onRemote, bool isFromMe)
            {
                if (ActivityQueue.Instance == null)
                    return;
                
                Trace.WriteLine(String.Format("@#$> T:{0} LuceneActivityDistributor.DoAction: {1}. onRemote: {2}, isFromMe:{3}",
                    System.Threading.Thread.CurrentThread.ManagedThreadId, Activity.GetType().Name, onRemote, isFromMe));

                if (onRemote && !isFromMe)
                {
                    ActivityQueue.AddActivity(this.Activity);
                }
            }
        }

        //public int IndexingTaskId = 0;
        //public int IndexingActivityId = 0;

        public virtual void Distribute()
        {
            var distributor = new LuceneActivityDistributor() { Activity = this };
            distributor.Execute();
        }
    }
}
