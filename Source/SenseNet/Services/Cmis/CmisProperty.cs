using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Diagnostics;

namespace SenseNet.Services.Cmis
{
	[DebuggerDisplay("Name: {Name}, Value: {Value}")]
	public abstract class CmisProperty
	{
		[XmlAttribute(AttributeName = "name", Namespace = CmisService.CmisNamespace)]
		public string Name { get; set; }

		[XmlElement(ElementName = "value")]
		public object Value { get; set; }
	}
	
	public class CmisBooleanProperty  : CmisProperty	{ }
	public class CmisIdProperty  : CmisProperty	{ }
	public class CmisIntegerProperty  : CmisProperty	{ }
	public class CmisDateTimeProperty  : CmisProperty	{ }
	public class CmisDecimalProperty  : CmisProperty	{ }
	public class CmisHtmlProperty  : CmisProperty	{ }
	public class CmisStringProperty  : CmisProperty	{ }
	public class CmisUriProperty  : CmisProperty	{ }
	public class CmisXmlProperty : CmisProperty { }

}