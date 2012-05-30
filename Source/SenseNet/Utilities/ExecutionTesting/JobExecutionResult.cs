using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SenseNet.Utilities.ExecutionTesting
{
    public class JobExecutionResult
    {
        public long MaxDuration { get; set; }

        public long SumDuration { get; set; }

        public decimal AverageDuration { get; set; }

        public int ExceptionCount { get; set; }

        public decimal IterationCount { get; set; }

        public Collection<Exception> Exceptions { get; internal set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Execution Result for: {0}", "");
            sb.AppendFormat("\n\tIterationCount: {0}", IterationCount);
            sb.AppendFormat("\n\tAverageDuration: {0}", AverageDuration);
            sb.AppendFormat("\n\tMaxDuration: {0}", MaxDuration);
            sb.AppendFormat("\n\tExceptionCount:{0}", ExceptionCount);
            if (ExceptionCount > 0)
                foreach(Exception e in Exceptions)
                {
                    sb.AppendFormat("\t\t{0}\n{1}\n", e.Message, e.StackTrace);
                }
            return sb.ToString();
        }
   }
}
