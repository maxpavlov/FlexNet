using System;
using SenseNet.ContentRepository.Storage;
using  SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository.Tests.ContentHandlers
{
	[ContentHandler]
	class TestNodeWithBinaryProperty : Node
	{
		public static string ContentTypeDefinition =
			@"<?xml version='1.0' encoding='utf-8'?>
			<ContentType name='RepositoryTest_TestNodeWithBinaryProperty' handler='SenseNet.ContentRepository.Tests.ContentHandlers.TestNodeWithBinaryProperty' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
				<Fields />
			</ContentType>
			";

        public override bool IsContentType { get { return false; } }

		public TestNodeWithBinaryProperty(Node parent) : base(parent, "RepositoryTest_TestNodeWithBinaryProperty") { }
		protected TestNodeWithBinaryProperty(NodeToken token) : base(token) { }

		[RepositoryProperty]
		public string Note
		{
			get { return this.GetProperty<string>("SenseNet.ContentRepository.Tests.ContentHandlers.TestNodeWithBinaryProperty.Note"); }
			set { this["SenseNet.ContentRepository.Tests.ContentHandlers.TestNodeWithBinaryProperty.Note"] = value; }
		}
		[RepositoryProperty]
		public BinaryData FirstBinary
		{
			get { return this.GetBinary("SenseNet.ContentRepository.Tests.ContentHandlers.TestNodeWithBinaryProperty.FirstBinary"); }
			set { this.SetBinary("SenseNet.ContentRepository.Tests.ContentHandlers.TestNodeWithBinaryProperty.FirstBinary", value); }
		}
		[RepositoryProperty]
		public BinaryData SecondBinary
		{
			get { return this.GetBinary("SenseNet.ContentRepository.Tests.ContentHandlers.TestNodeWithBinaryProperty.SecondBinary"); }
			set { this.SetBinary("SenseNet.ContentRepository.Tests.ContentHandlers.TestNodeWithBinaryProperty.SecondBinary", value); }
		}

		public override string ToString()
		{
			return string.Concat((FirstBinary != null ? FirstBinary.FileName.FullFileName : "FirstBinary is NULL"), "/", (SecondBinary != null ? SecondBinary.FileName.FullFileName : "SecondBinary is NULL"), " (", Note, ")");
		}
	}
}
