using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;

namespace SenseNet.Workflow
{
    public class Configuration
    {
        private const string SECTIONNAME = "sensenet/workflow";

        private const string TIMERINTERVALKEY = "TimerInterval";

        private const double DefaultTimerInterval = 10.0;

        private static double? _timerInterval;

        public static double TimerInterval
        {
            get
            {
                if (_timerInterval == null)
                    Parse();
                return _timerInterval.Value;
            }
        }

        private static void Parse()
        {
            var collection = System.Configuration.ConfigurationManager.GetSection(SECTIONNAME) as NameValueCollection;
            if (collection == null)
            {
                _timerInterval = DefaultTimerInterval;
                return;
            }
            var setting = collection.Get(TIMERINTERVALKEY);
            double @double;
            _timerInterval =
                double.TryParse(setting, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out @double)
                ?
                @double
                :
                DefaultTimerInterval;
Debug.WriteLine("@> _timerInterval: " + _timerInterval);
        }
    }
}
