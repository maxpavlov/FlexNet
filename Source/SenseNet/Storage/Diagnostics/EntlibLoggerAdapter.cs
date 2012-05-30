using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace SenseNet.Diagnostics
{
    public class EntLibLoggerAdapter : ILoggerAdapter
    {
        public void Write(object message, ICollection<string> categories, int priority, int eventId, System.Diagnostics.TraceEventType severity, string title, IDictionary<string, object> properties)
        {
            var props = Utility.CollectAutoProperties(properties);
            Microsoft.Practices.EnterpriseLibrary.Logging.Logger.Write(
                message ?? String.Empty, categories ?? Logger.EmptyCategoryList, 
                priority, eventId, severity, title ?? string.Empty, props);
        }

        public void Write<T>(object message, ICollection<string> categories, int priority, int eventId,
            TraceEventType severity, string title, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            var properties = getPropertiesCallback(callbackArg);
            Write(message, categories, priority, eventId, severity, title, properties);
        }
    }

    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class OneLineTraceListener : CustomTraceListener
    {
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (data is LogEntry && this.Formatter != null)
            {
                this.WriteLine(this.Formatter.Format(data as LogEntry));
            }
            else
            {
                this.WriteLine(data.ToString());
            }
        }
        public override void Write(string message)
        {
            Debug.Write(message);
        }
        public override void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }
    }

}
