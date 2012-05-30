using System;

namespace SenseNet.ContentRepository.Storage.Events
{
	public class NodeOperationEventArgs : NodeEventArgs
	{
		private Node _targetNode;
		public Node TargetNode { get { return _targetNode; } }

		public NodeOperationEventArgs(Node sourceNode, Node targetNode, NodeEvent eventType)
            : base(sourceNode, eventType)
		{
			_targetNode = targetNode;
		}
        public NodeOperationEventArgs(Node sourceNode, Node targetNode, NodeEvent eventType, string originalSourcePath)
            : base(sourceNode, eventType, originalSourcePath)
        {
            _targetNode = targetNode;
        }
    }

}