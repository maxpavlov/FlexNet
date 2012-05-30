using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace SenseNet.Diagnostics
{
    public static class Tracing
    {
        public const string OperationTraceDataSlotName = "OperationTraceDataSlot";
        private static bool _enabled;
        private static ILoggerAdapter _defaultLoggerAdapter;

        static Tracing()
        {
            _enabled = true;
            var traceValue = WebConfigurationManager.AppSettings["OperationTrace"];
            if (!string.IsNullOrEmpty(traceValue) && traceValue.ToLower().Equals("false"))
                _enabled = false;
            _defaultLoggerAdapter = new DebugWriteLoggerAdapter();
        }

        public static bool Enabled
        {
            get 
            {
                return _enabled; 
            }
        }

        public static ILoggerAdapter DefaultLoggerAdapter
        {
            get
            {
                return _defaultLoggerAdapter;
            }
        }

        internal static void OnOperationStart(Guid id, string name, string title, string message, string methodName, long startTicks)
        {
            var collector = GetCollector();
            if (collector == null)
                return;
            collector.StartOperation(id, name, title, message, methodName, startTicks);
        }
        internal static void OnOperationEnd(string message, bool successful, Guid id, long finisTicks, decimal secondsElapsed)
        {
            var collector = GetCollector();
            if (collector == null)
                return;
            collector.EndOperation(message, successful, id, finisTicks, secondsElapsed);
        }
        public static string GetOperationTrace()
        {
            var collector = GetCollector();
            if (collector == null)
                return null;
            return collector.ToXml();
        }
        public static void ClearOperationTrace()
        {
            var slot = System.Threading.Thread.GetNamedDataSlot(Tracing.OperationTraceDataSlotName);
            System.Threading.Thread.SetData(slot, null);
        }

        private static OperationTraceCollector GetCollector()
        {
            var slot = System.Threading.Thread.GetNamedDataSlot(Tracing.OperationTraceDataSlotName);
            var data = System.Threading.Thread.GetData(slot);
            return data as OperationTraceCollector;
        }
    }

    public class OperationTraceCollector
    {
        [DebuggerDisplay("{Message} {MethodName}")]
        public class OpNode
        {
            [XmlAttribute("id")]
            public Guid Id;
            [XmlAttribute("name")]
            public string Name;
            [XmlAttribute("title")]
            public string Title;
            [XmlAttribute("message")]
            public string Message;
            [XmlAttribute("methodName")]
            public string MethodName;
            [XmlAttribute("startTicks")]
            public long StartTicks;
            [XmlAttribute("finishTicks")]
            public long FinishTicks;
            [XmlAttribute("duration")]
            public decimal Duration;
            [XmlAttribute("successful")]
            public bool Successful;
            public List<OpNode> Children=new List<OpNode>();
            [XmlIgnore]
            public OpNode Parent;
        }

        private OpNode _root;
        private OpNode _currentNode;

        public OperationTraceCollector()
        {
            _root = new OpNode { Id = Guid.NewGuid() };
            _currentNode = _root;
        }

        internal void StartOperation(Guid id, string name, string title, string message, string methodName, long startTicks)
        {
            var node = new OpNode { Id = id, Name = name, Title = title, Message = message, MethodName = methodName, StartTicks = startTicks };
            node.Parent = _currentNode;
            node.Parent.Children.Add(node);
            _currentNode = node;
        }
        internal void EndOperation(string message, bool successful, Guid id, long finishTicks, decimal secondsElapsed)
        {
            var node = _currentNode;
            node.Message = message;
            node.Successful = successful;
            node.FinishTicks = finishTicks;
            node.Duration = secondsElapsed;
            _currentNode = node.Parent;
        }
        public string ToXml()
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            var serializer = new XmlSerializer(typeof(OpNode));
            serializer.Serialize(writer, _root);
            return sb.ToString();
        }
    }
}
