using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Packaging
{
	[global::System.Serializable]
	public class InstallerException : Exception
	{
		public InstallerException() { }
		public InstallerException(string message) : base(message) { }
		public InstallerException(string message, Exception inner) : base(message, inner) { }
		protected InstallerException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
	[global::System.Serializable]
	public class InvalidManifestException : InstallerException
	{
		public InvalidManifestException() { }
		public InvalidManifestException(string message) : base(message) { }
		public InvalidManifestException(string message, Exception inner) : base(message, inner) { }
		protected InvalidManifestException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
