using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.Communication.Messaging;
using System.Globalization;

namespace SenseNet.ContentRepository.Storage.Caching.DistributedActions
{
    [Serializable]
    public class CacheCleanAction : DistributedAction
    {

        public override void DoAction(bool onRemote, bool isFromMe)
        {
            //only run on 
            if (onRemote && isFromMe) return;

            List<string> cacheEntryKeys = new List<string>();

            int localCacheCount = DistributedApplication.Cache.Count;
            //int portletClientCount = PortletDependency.ClientCount;


            foreach (DictionaryEntry entry in DistributedApplication.Cache)
                cacheEntryKeys.Add(entry.Key.ToString());

            foreach (string cacheEntryKey in cacheEntryKeys)
                DistributedApplication.Cache.Remove(cacheEntryKey);


            //DebugMessage.Send(String.Format(CultureInfo.InvariantCulture, "Cache flushed. items in cache B/A: {0}/{1}; portlet client count: {2}/{3},", 
            //    cacheEntryKeys.Count, DistributedApplication.Cache.Count, portletClientCount, PortletDependency.ClientCount ));
        }
    }
}