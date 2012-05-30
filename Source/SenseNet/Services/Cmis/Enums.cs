using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SenseNet.Services.Cmis
{
	//public enum enumDecimalPrecision { prec32, prec64 }
	//public enum enumContentStreamAllowed { notallowed, allowed, required }
	//public enum enumCardinality { single, multi }
	//public enum enumUpdateability { @readonly, readwrite, whencheckedout }
	//public enum enumPropertyType { Boolean, id, integer, datetime, @decimal, html, uri, xml }
	//public enum enumCollectionType { root_children, root_descendants, unfiled, checkedout, types_children, types_descendants, query }
	//public enum enumObjectType { document, folder, relationship, policy }
	//public enum enumCapabilityQuery { none, metadataonly, fulltextonly, both }
	public enum enumCapabilityJoin { nojoin, inneronly, innerandouter }
	public enum enumCapabilityFullText { none, fulltextonly, fulltextandstructured }
	public enum enumRepositoryRelationship { self, replica, peer, parent, child, archive }
	//public enum enumTypesOfFileableObjects { documents, folders, policies, any }
	//public enum enumVersioningState { checkedout, minor, major }
	//public enum enumReturnVersion { @this, latest, latestmajor }
	//public enum enumUnfileNonfolderObjects { unfile, deletesinglefiled, delete }
	//public enum enumRelationshipDirection { source, target, both }
	//public enum enumIncludeRelationships { none, source, target, both }

	//public enum PropertyType { String, Decimal, Integer, Boolean, DateTime, URI, ID, XML, HTML }

	public enum LinkRelation
	{
		//http://www.iana.org/assignments/link-relations/
		[EnumMember(Value = "alternate")]    alternate,    // [RFC4287]   
		[EnumMember(Value = "current")]      current,      // [RFC5005] 02 February 2006 
		[EnumMember(Value = "enclosure")]    enclosure,    // [RFC4287]   
		[EnumMember(Value = "edit")]         edit,         // [RFC5023] 27 July 2007 
		[EnumMember(Value = "edit-media")]   editMedia,    // [RFC5023] 27 July 2007 
		[EnumMember(Value = "first")]        first,        // [Nottingham] 02 February 2006 
		[EnumMember(Value = "last")]         last,         // [Nottingham] 02 February 2006 
		[EnumMember(Value = "license")]      license,      // [RFC4946] 14 May 2007 
		[EnumMember(Value = "next")]         next,         // [RFC5005] 02 February 2006 
		[EnumMember(Value = "next-archive")] nextArchive,  // [RFC5005] 13 July 2007 
		[EnumMember(Value = "payment")]      payment,      // [Kinberg],[Sayre] 02 February 2006 
		[EnumMember(Value = "prev-archive")] prevArchive,  // [RFC5005] 13 July 2007 
		[EnumMember(Value = "previous")]     previous,     // [RFC5005] 02 February 2006 
		[EnumMember(Value = "related")]      related,      // [RFC4287]   
		[EnumMember(Value = "replies")]      replies,      // [RFC4685] 28 June 2006 
		[EnumMember(Value = "self")]         self,         // [RFC4287]    
		[EnumMember(Value = "service")]      service,      // [Snell] 20 May 2008 
		[EnumMember(Value = "via")]          via,          // [RFC4287]   

		[EnumMember(Value = "cmis-repository")]       cmisRepository,
		[EnumMember(Value = "cmis-source")]           cmisSource,
		[EnumMember(Value = "cmis-target")]           cmisTarget,
		[EnumMember(Value = "cmis-parent")]           cmisParent,
		[EnumMember(Value = "cmis-parents")]          cmisParents,
		[EnumMember(Value = "cmis-children")]         cmisChildren,
		[EnumMember(Value = "cmis-allowableactions")] cmisAllowableactions,
		[EnumMember(Value = "cmis-relationships")]    cmisRelationships,
		[EnumMember(Value = "cmis-type")]             cmisType,
		[EnumMember(Value = "cmis-descendants")]      cmisDescendants,
		[EnumMember(Value = "cmis-policies")]		  cmisPolicies,
		[EnumMember(Value = "cmis-allversions")]      cmisAllVersions,
		[EnumMember(Value = "cmis-latestversion")]    cmisLatestVersion,
		[EnumMember(Value = "cmis-stream")]           cmisStream

	}
}