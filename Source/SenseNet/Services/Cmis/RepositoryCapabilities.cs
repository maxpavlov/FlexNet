using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace SenseNet.Services.Cmis
{
	[DataContract]
	public class RepositoryCapabilities
	{
		[XmlElement(ElementName = "capabilityMultifiling", Order = 0)]
		public bool Multifiling { get; set; }

		[XmlElement(ElementName = "capabilityUnfiling", Order = 1)]
		public bool Unfiling { get; set; }

		[XmlElement(ElementName = "capabilityVersionSpecificFiling", Order = 2)]
		public bool VersionSpecificFiling { get; set; }

		[XmlElement(ElementName = "capabilityPWCUpdateable", Order = 3)]
		public bool PWCUpdateable { get; set; }

		[XmlElement(ElementName = "capabilityAllVersionsSearchable", Order = 4)]
		public bool AllVersionsSearchable { get; set; }

		[XmlElement(ElementName = "capabilityJoin", Order = 5)]
		public enumCapabilityJoin Join { get; set; }

		[XmlElement(ElementName = "capabilityFullText", Order = 6)]
		public enumCapabilityFullText FullText { get; set; }
	}
}