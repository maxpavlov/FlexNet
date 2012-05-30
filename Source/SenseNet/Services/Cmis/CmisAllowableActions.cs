using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SenseNet.Services.Cmis
{
	[XmlRoot(ElementName = "allowableActions", Namespace = CmisService.CmisNamespace)]
	[XmlInclude(typeof(CmisAllowableDocumentActions))]
	[XmlInclude(typeof(CmisAllowableFolderActions))]
	[XmlInclude(typeof(CmisAllowablePolicyActions))]
	[XmlInclude(typeof(CmisRelationshipActions))]
	public abstract class CmisAllowableActions
	{
		[XmlElement(ElementName = "canDelete", Namespace = CmisService.CmisNamespace, Order = 0)]
		public bool CanDelete { get; set; }
		[XmlElement(ElementName = "canGetProperties", Namespace = CmisService.CmisNamespace, Order = 1)]
		public bool CanGetProperties { get; set; }
		[XmlElement(ElementName = "canUpdateProperties", Namespace = CmisService.CmisNamespace, Order = 2)]
		public bool CanUpdateProperties { get; set; }
	}
	[XmlRoot(ElementName = "allowableActions", Namespace = CmisService.CmisNamespace)]
	public class CmisAllowableDocumentActions : CmisAllowableActions
	{
		[XmlElement(ElementName = "canGetParents", Namespace = CmisService.CmisNamespace, Order = 3)]
		public bool CanGetParents { get; set; }
		[XmlElement(ElementName = "canMove", Namespace = CmisService.CmisNamespace, Order = 4)]
		public bool CanMove { get; set; }
		[XmlElement(ElementName = "canDeleteVersion", Namespace = CmisService.CmisNamespace, Order = 5)]
		public bool CanDeleteVersion { get; set; }
		[XmlElement(ElementName = "canDeleteContent", Namespace = CmisService.CmisNamespace, Order = 6)]
		public bool CanDeleteContent { get; set; }
		[XmlElement(ElementName = "canCheckout", Namespace = CmisService.CmisNamespace, Order = 7)]
		public bool CanCheckout { get; set; }
		[XmlElement(ElementName = "canCancelCheckout", Namespace = CmisService.CmisNamespace, Order = 8)]
		public bool CanCancelCheckout { get; set; }
		[XmlElement(ElementName = "canCheckin", Namespace = CmisService.CmisNamespace, Order = 9)]
		public bool CanCheckin { get; set; }
		[XmlElement(ElementName = "canSetContent", Namespace = CmisService.CmisNamespace, Order = 10)]
		public bool CanSetContent { get; set; }
		[XmlElement(ElementName = "canGetAllVersions", Namespace = CmisService.CmisNamespace, Order = 11)]
		public bool CanGetAllVersions { get; set; }
		[XmlElement(ElementName = "canAddToFolder", Namespace = CmisService.CmisNamespace, Order = 12)]
		public bool CanAddToFolder { get; set; }
		[XmlElement(ElementName = "canRemoveFromFolder", Namespace = CmisService.CmisNamespace, Order = 13)]
		public bool CanRemoveFromFolder { get; set; }
		[XmlElement(ElementName = "canViewContent", Namespace = CmisService.CmisNamespace, Order = 14)]
		public bool CanViewContent { get; set; }
		[XmlElement(ElementName = "canAddPolicy", Namespace = CmisService.CmisNamespace, Order = 15)]
		public bool CanAddPolicy { get; set; }
		[XmlElement(ElementName = "canRemovePolicy", Namespace = CmisService.CmisNamespace, Order = 16)]
		public bool CanRemovePolicy { get; set; }
	}
	[XmlRoot(ElementName = "allowableActions", Namespace = CmisService.CmisNamespace)]
	public class CmisAllowableFolderActions : CmisAllowableActions
	{
		[XmlElement(ElementName = "canGetDescendants", Namespace = CmisService.CmisNamespace, Order = 3)]
		public bool CanGetDescendants { get; set; }
		[XmlElement(ElementName = "canMove", Namespace = CmisService.CmisNamespace, Order = 4)]
		public bool CanMove { get; set; }
		[XmlElement(ElementName = "canAddPolicy", Namespace = CmisService.CmisNamespace, Order = 5)]
		public bool CanAddPolicy { get; set; }
		[XmlElement(ElementName = "canRemovePolicy", Namespace = CmisService.CmisNamespace, Order = 6)]
		public bool CanRemovePolicy { get; set; }
		[XmlElement(ElementName = "canGetChildren", Namespace = CmisService.CmisNamespace, Order = 7)]
		public bool CanGetChildren { get; set; }
		[XmlElement(ElementName = "canGetParent", Namespace = CmisService.CmisSnExtNamespace, Order = 8)]
		public bool CanGetParent { get; set; }
	}
	[XmlRoot(ElementName = "allowableActions", Namespace = CmisService.CmisNamespace)]
	public class CmisAllowablePolicyActions : CmisAllowableActions
	{
		[XmlElement(ElementName = "canGetParents", Namespace = CmisService.CmisNamespace, Order = 3)]
		public bool CanGetParents { get; set; }
		[XmlElement(ElementName = "canMove", Namespace = CmisService.CmisNamespace, Order = 4)]
		public bool CanMove { get; set; }
		[XmlElement(ElementName = "canAddPolicy", Namespace = CmisService.CmisNamespace, Order = 5)]
		public bool CanAddPolicy { get; set; }
		[XmlElement(ElementName = "canRemovePolicy", Namespace = CmisService.CmisNamespace, Order = 6)]
		public bool CanRemovePolicy { get; set; }
	}
	[XmlRoot(ElementName = "allowableActions", Namespace = CmisService.CmisNamespace)]
	public class CmisRelationshipActions : CmisAllowableActions
	{
	}
}