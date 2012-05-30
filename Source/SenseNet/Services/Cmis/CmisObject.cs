using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace SenseNet.Services.Cmis
{
	[XmlRoot(ElementName = "object", Namespace = CmisService.CmisNamespace)]
	public class CmisObject
	{
		[XmlArray(ElementName = "properties", Namespace = CmisService.CmisNamespace)]
		[XmlArrayItem(ElementName = "propertyBoolean", Type = typeof(CmisBooleanProperty))]
		[XmlArrayItem(ElementName = "propertyId", Type = typeof(CmisIdProperty))]
		[XmlArrayItem(ElementName = "propertyInteger", Type = typeof(CmisIntegerProperty))]
		[XmlArrayItem(ElementName = "propertyDateTime", Type = typeof(CmisDateTimeProperty))]
		[XmlArrayItem(ElementName = "propertyDecimal", Type = typeof(CmisDecimalProperty))]
		[XmlArrayItem(ElementName = "propertyHtml", Type = typeof(CmisHtmlProperty))]
		[XmlArrayItem(ElementName = "propertyString", Type = typeof(CmisStringProperty))]
		[XmlArrayItem(ElementName = "propertyUri", Type = typeof(CmisUriProperty))]
		[XmlArrayItem(ElementName = "propertyXml", Type = typeof(CmisXmlProperty))]
		public List<CmisProperty> Properties { get; set; }

		public CmisAllowableActions AllowableActions { get; set; }

	}
}