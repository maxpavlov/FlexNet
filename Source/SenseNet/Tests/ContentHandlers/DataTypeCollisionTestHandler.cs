using System;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ContentRepository.Tests.ContentHandlers
{
	[ContentHandler]
	public class DataTypeCollisionTestHandler : Node
	{
		public DataTypeCollisionTestHandler(Node parent) : base(parent) { }
		public DataTypeCollisionTestHandler(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected DataTypeCollisionTestHandler(NodeToken token) : base(token) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty]
		public string TestString
		{
			get { return this.GetProperty<string>("SenseNet.ContentRepository.Tests.ContentHandlers.DataTypeCollisionTestHandler.TestString"); }
			set { this["SenseNet.ContentRepository.Tests.ContentHandlers.DataTypeCollisionTestHandler.TestString"] = value; }
		}
	}
}
