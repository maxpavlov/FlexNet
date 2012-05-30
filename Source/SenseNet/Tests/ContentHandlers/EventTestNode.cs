using System;
using SenseNet.ContentRepository.Storage;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.Events;

namespace SenseNet.ContentRepository.Tests.ContentHandlers
{
	[ContentHandler]
	public class EventTestNode : Node
	{
		public static string DefaultNodeTypeName = "RepositoryTest_EventTestNode";
		public static string ContentTypeDefinition = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='RepositoryTest_EventTestNode' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.EventTestNode' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<Fields />
</ContentType>
";

		public EventTestNode(Node parent) : this(parent, DefaultNodeTypeName) { }
		public EventTestNode(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected EventTestNode(NodeToken token) : base(token) { }

		protected override void OnCreating(object sender, CancellableNodeEventArgs e)
		{
			if (e.SourceNode.Index == 0)
			{
				e.CancelMessage = "Index cannot be 0";
				e.Cancel = true;
			}
			base.OnCreating(sender, e);
		}
	}
}
