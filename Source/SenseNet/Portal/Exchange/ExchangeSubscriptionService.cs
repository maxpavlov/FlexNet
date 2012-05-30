using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using System.Configuration;
using SenseNet.Search;
using SenseNet.Diagnostics;
using System.Diagnostics;

namespace SenseNet.Portal.Exchange
{
    public class ExchangeSubscriptionService : ISnService
    {
        public bool Start()
        {
            if (!Configuration.SubscriptionServiceEnabled)
                return false;

            // renew subscriptions
            //  1: go through doclibs with email addresses
            var doclibs = ContentQuery.Query("+TypeIs:DocumentLibrary +ListEmail:* -ListEmail:\"\"");
            if (doclibs.Count > 0)
            {
                Logger.WriteInformation("Exchange subscription service enabled, running subscriptions (" + doclibs.Count.ToString() + " found)", ExchangeHelper.ExchangeLogCategory);
                foreach (var doclib in doclibs.Nodes)
                {
                    try
                    {
                        ExchangeHelper.Subscribe(doclib);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex, ExchangeHelper.ExchangeLogCategory);
                    }
                }
            }
            else
            {
                Logger.WriteInformation("Exchange subscription service enabled, no subscriptions found.", ExchangeHelper.ExchangeLogCategory);
            }

            return true;
        }
    }
}
