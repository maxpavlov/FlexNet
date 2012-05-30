using System;
using System.Web.Caching;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Caching.DistributedActions;
using System.Threading;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.ContentRepository.Storage.Caching.Dependency
{
    /// <summary>
    /// Creates a dependency that is notified when the portlet changes to invalidate
    /// the related cache item
    /// </summary>
    public class PortletDependency : CacheDependency
    {
        public static object _eventSync = new object();
        private string _portletID;
        //public static event EventHandler<EventArgs<string>> Changed;
        private static EventServer<string> Changed = new EventServer<string>(RepositoryConfiguration.PortletDependencyEventPartitions);

        public PortletDependency(string portletId)
        {
            try
            {
                this._portletID = portletId;
                lock (_eventSync)
                {
                    //PortletDependency.Changed += PortletDependency_Changed;
                    Changed.TheEvent += PortletDependency_Changed;
                }
            }
            finally
            {
                this.FinishInit();
            }
        }

        void PortletDependency_Changed(object sender, EventArgs<string> e)
        {
            if (e.Data == _portletID)
            {
                this.NotifyDependencyChanged(this, e);
            }
        }

        protected override void DependencyDispose()
        {
            lock (_eventSync)
            {
                //if (PortletDependency.Changed != null)
                //    PortletDependency.Changed -= PortletDependency_Changed;
                Changed.TheEvent -= PortletDependency_Changed;
            }
        }

        public static void NotifyChange(string portletId)
        {
            new PortletChangedAction(portletId).Execute();
        }
        public static void FireChanged(string portletId)
        {
            //lock (_eventSync)
            //{
            //    if (Changed != null)
            //    {
            //        try
            //        {
            //            //Console.WriteLine("!!!!!!!!!!!!Portlet Changed:" + portletId);
            //            //Console.WriteLine("PD Notify about: " + changed.GetInvocationList().Length);
            //            Changed(null, new EventArgs<string>(portletId));
            //            //Console.WriteLine("New client count: " + changed.GetInvocationList().Length);
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.WriteLine("Error in invoke Changed: " + ex);
            //        }
            //    }
            //    else
            //    {
            //        Debug.WriteLine("!!! List is empty");
            //    }
            //}

            lock (_eventSync)
            {
                Changed.Fire(null, portletId);
            }
        }
    }
}
