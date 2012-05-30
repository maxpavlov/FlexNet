using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
//using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
//using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
//using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
//using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace SenseNet.Diagnostics
{
    public class DebugWriteLoggerAdapter: ILoggerAdapter
    {
        public void Write(object message, ICollection<string> categories, int priority, int eventId, 
            TraceEventType severity, string title, IDictionary<string, object> properties)
        {
            var props = Utility.CollectAutoProperties(properties);
            string msg = string.Format(@"Message: {0}; Categories:{1}; Priority:{2}; EventId:{3}; " +
                "Severity: {4}; Title: {5}; Properties: {6}",
                message, string.Join(",", categories.ToArray()), priority, eventId, severity, title,
                props == null ? "" : String.Join(", ",
                    (from item in props select String.Concat(item.Key, ":", item.Value)).ToArray()));

            Debug.WriteLine(msg);
        }

        public void Write<T>(object message, ICollection<string> categories, int priority, int eventId,
            TraceEventType severity, string title, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Write(message, categories, priority, eventId, severity, title,
                getPropertiesCallback(callbackArg));
        }

    }

    //public class DebugWriteListener : TraceListener
    //{
    //    public DebugWriteListener(string init)
    //    {
    //        int q = 1;
    //    }
    //    public override void Write(string message)
    //    {
    //        Debug.Write(message);
    //    }

    //    public override void WriteLine(string message)
    //    {
    //        Debug.WriteLine(message.Replace("\r", "\n").Replace("\n\n", "\n").Replace("\n", ";"));
    //    }
    //}
}
