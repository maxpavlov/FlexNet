using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace SenseNet.Services.Cmis
{
	[XmlRoot(ElementName = "repositoryInfo", Namespace = CmisService.CmisNamespace)]
	public class RepositoryInfo
	{
		[XmlElement(ElementName = "repositoryId", Order = 0)]
		public string Id { get; set; }

		[XmlElement(ElementName = "repositoryName", Order = 1)]
		public string Name { get; set; }

		[XmlElement(ElementName = "repositoryRelationship", Order = 2)]
		public enumRepositoryRelationship Relationship { get; set; }

		[XmlElement(ElementName = "repositoryDescription", Order = 3)]
		public string Description { get; set; }

		[XmlElement(ElementName = "vendorName", Order = 4)]
		public string VendorName { get; set; }

		[XmlElement(ElementName = "productName", Order = 5)]
		public string ProductName { get; set; }

		[XmlElement(ElementName = "productVersion", Order = 6)]
		public string ProductVersion { get; set; }

		[XmlElement(ElementName = "rootFolderId", Order = 7)]
		public string RootFolderId { get; set; }

		[XmlElement(ElementName = "capabilities", Order = 8)]
		public RepositoryCapabilities Capabilities { get; set; }

		[XmlElement(ElementName = "cmisVersionsSupported", Order = 9)]
		public string CmisVersionsSupported { get; set; }

		//            <cmis:repositorySpecificInformation>
		//                <cmisother:Local>Local Message in vendor specific schema</cmisother:Local>
		//            </cmis:repositorySpecificInformation>
	}
}