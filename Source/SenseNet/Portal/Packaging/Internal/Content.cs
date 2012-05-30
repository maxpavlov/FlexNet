using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SenseNet.Packaging.Internal
{
	internal class Content
	{
		public string Name { get; set; }
		public string ContentType { get; set; }
		public string Path { get; set; }
		public string Data { get; set; }
		public Attachment[] Attachments { get; set; }
		public bool IsNewContent { get; set; }
		public bool HasReference { get; set; }
	}
	internal class Attachment
	{
		public string FieldName { get; set; }
		public string FileName { get; set; }
		public IManifest Manifest { get; set; }
	}
}
