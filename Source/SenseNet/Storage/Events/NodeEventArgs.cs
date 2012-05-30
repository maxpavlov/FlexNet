using System;
using SenseNet.ContentRepository.Storage.Security;
using System.Collections.Generic;

namespace SenseNet.ContentRepository.Storage.Events
{
	public class NodeEventArgs : EventArgs, INodeEventArgs
	{
		public Node SourceNode { get; private set; }
		public IUser User { get; private set; }
		public DateTime Time { get; private set; }
		public NodeEvent EventType { get; private set; }
        public string OriginalSourcePath { get; private set; }
        public IEnumerable<ChangedData> ChangedData { get; private set; }

		internal NodeEventArgs(Node node, NodeEvent eventType) : this(node, eventType, node.Path) { }

        internal NodeEventArgs(Node node, NodeEvent eventType, string originalSourcePath) : this(node, eventType, originalSourcePath, null) { }

        internal NodeEventArgs(Node node, NodeEvent eventType, string originalSourcePath, IEnumerable<ChangedData> changedData)
        {
            this.SourceNode = node;
            this.User = AccessProvider.Current.GetCurrentUser();
            this.Time = DateTime.Now;
            this.EventType = eventType;
            this.OriginalSourcePath = originalSourcePath;
            this.ChangedData = changedData;
        }
    }
}