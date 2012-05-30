using System;
using System.Collections.Generic;
using System.Text;
using SenseNet.ContentRepository.Storage;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository;

namespace SenseNet.Portal
{
	[ContentHandler]
	public class MasterPage : File
	{
        [Obsolete("Use typeof(MasterPage).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(MasterPage).Name;

		//================================================================================= Construction

        public MasterPage(Node parent) : this(parent, null) { }
		public MasterPage(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected MasterPage(NodeToken nt) : base(nt) { }

		//================================================================================= Generic Property handling

		public override object GetProperty(string name)
		{
			switch (name)
			{
				default:
					return base.GetProperty(name);
			}
		}
		public override void SetProperty(string name, object value)
		{
			switch (name)
			{
				default:
					base.SetProperty(name, value);
					break;
			}
		}
	}
}