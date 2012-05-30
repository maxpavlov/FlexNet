using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.UI.Controls
{
	[global::System.Serializable]
	public class FieldControlDataException : ApplicationException
	{
		public string ResourceStringKey { get; private set; }

		public FieldControlDataException(System.Web.UI.Control control, string resourceStringKey) 
		{
			CreateResourceKey(control, resourceStringKey);
		}
		public FieldControlDataException(System.Web.UI.Control control, string resourceStringKey, string message)
			: base(message)
		{
			CreateResourceKey(control, resourceStringKey);
		}
		public FieldControlDataException(System.Web.UI.Control control, string resourceStringKey, string message, Exception inner)
			: base(message, inner)
		{
			CreateResourceKey(control, resourceStringKey);
		}

		protected FieldControlDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

		private void CreateResourceKey(System.Web.UI.Control control, string resourceStringKey)
		{
			ResourceStringKey = String.Concat(control.GetType().Name, "_", resourceStringKey);
		}
	}
}