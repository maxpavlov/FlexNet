using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SenseNet.Messaging
{
    //WARNING: Do not reorder this
    public enum NotificationFrequency { Immediately, Daily, Weekly, Monthly }

    internal class DataHandler : NotificationsDataContext
    {
        public DataHandler() : base(ConfigurationManager.ConnectionStrings["SnCrMsSql"].ConnectionString) { }
    }
}
