using System;
using System.Collections.Generic;
using System.Linq;

using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Tests.ContentHandlers
{
	[ContentHandler]
	public class NonGenericHandler : Node
	{
		public NonGenericHandler(Node parent) : base(parent) { }
		public NonGenericHandler(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected NonGenericHandler(NodeToken token) : base(token) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty("TestString")]
		public string TestString
		{
			get { return this.GetProperty<string>("TestString"); }
			set { this["TestString"] = value; }
		}

		public const string ContentTypeDefinition = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='NonGeneric' handler='SenseNet.ContentRepository.Tests.ContentHandlers.NonGenericHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<DisplayName>NonGeneric [demo]</DisplayName>
	<Description>This is a demo NonGeneric node definition</Description>
	<Icon>icon.gif</Icon>
	<Fields>
		<Field name='Index' type='Integer' />
		<Field name='TestString' type='ShortText' />
	</Fields>
</ContentType>
";

	}
}