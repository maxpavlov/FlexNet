using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;


using SenseNet.ContentRepository.Fields;
using System.ServiceModel.Syndication;
using System.Text;
using System.IO;
using System.Xml;
using SenseNet.ContentRepository;
using SNC = SenseNet.ContentRepository;

namespace SenseNet.Services.Cmis
{
	internal class CmisFeedBuilder
	{
		//=============== Workspace service

		internal static AtomPub10ServiceDocumentFormatter BuildWorkspace(string repositoryId)
		{
			//var xmlns = new XmlSerializerNamespaces();
			//xmlns.Add("app", Workspace.APPNAMESPACE);
			//xmlns.Add("atom", Workspace.ATOMNAMESPACE);
			//xmlns.Add("cmis", Workspace.CMISNAMESPACE);

			var baseUri = GetBaseUri();

			var workspace = new Workspace("Main Repository", GetResourceCollections(baseUri, repositoryId));
			var repInfo =  new RepositoryInfo
			{
				Id = repositoryId,
				Name = "MainRep",
				Relationship = enumRepositoryRelationship.self,
				Description = "Main Repository",
				VendorName = "Sense/Net Ltd.",
				ProductName = "SenseNet Content Repository Prototype",
				ProductVersion = "0.01",
				RootFolderId = "2",
				Capabilities = new RepositoryCapabilities
				{
					Multifiling = false,
					Unfiling = true,
					VersionSpecificFiling = false,
					PWCUpdateable = false,
					AllVersionsSearchable = false,
					Join = enumCapabilityJoin.nojoin,
					FullText = enumCapabilityFullText.none
				},
				CmisVersionsSupported = "0.5"
			};
			workspace.ElementExtensions.Add(repInfo, new XmlSerializer(typeof(RepositoryInfo)));

			var serviceDoc = new ServiceDocument(new Workspace[] { workspace });
			var formatter = new AtomPub10ServiceDocumentFormatter(serviceDoc);
			return formatter;
		}
		//-- Workspace service tools
		private static List<ResourceCollectionInfo> GetResourceCollections(string baseUri, string repositoryId)
		{
			List<ResourceCollectionInfo> collections = new List<ResourceCollectionInfo>();
			var collectionTitle = "root collection";
			var collectionUri = new Uri(GetRootActionUri(baseUri, repositoryId, "getRoot"));
			var collection = new ResourceCollectionInfo(collectionTitle, collectionUri);
			collection.AttributeExtensions.Add(new XmlQualifiedName("collectionType", CmisService.CmisNamespace), "root-children");
			collections.Add(collection);
			return collections;
		}

		//=============== Object service

		internal static Atom10FeedFormatter BuildChildren(string repositoryId, string folderId)
		{
			if (repositoryId.ToLower() != "mainrep")
				throw new ArgumentException("Unknown repository identifier: " + repositoryId);

			int id;
			if (!Int32.TryParse(folderId, out id))
				throw new ArgumentException("Unknown object identifier: " + folderId);

			SNC.Content folder;
			var contentList = Store.GetChildren(id, repositoryId, out folder);

			//----

			var baseUri = GetBaseUri();

			var title = folder.Name;
			var description = "{Children of " + folder.Name + "}";
			var lastUpdateTime = new DateTimeOffset(folder.ContentHandler.ModificationDate);
			var linkHref = GetEntryActionUri(baseUri, repositoryId, folder.Id, "getEntry");
			var feedAlternateLink = new Uri(linkHref);
			var feedId = linkHref;

			List<SyndicationItem> items = new List<SyndicationItem>();
			foreach (var content in contentList)
			{
				CmisObject cmisObject = new CmisObject { Properties = BuildCmisProperties(content), AllowableActions = GetAllowableActions(content) };
				var itemTitle = content.Name;
				var itemContent = "{Content of " + content.Name + "}";
				var action = content.ContentHandler is IFolder ? "getChildren" : "getEntry";
				var itemAlternateLink = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, action));
				var itemId = GetEntryActionUri(baseUri, repositoryId, content.Id, "getEntry");
				var itemLastUpdates = new DateTimeOffset(content.ContentHandler.ModificationDate);
				var item = new SyndicationItem(itemTitle, itemContent, itemAlternateLink, itemId, itemLastUpdates);
				item.ElementExtensions.Add(cmisObject, new XmlSerializer(typeof(CmisObject)));
				foreach (var link in BuildLinks(content, baseUri, repositoryId))
					item.Links.Add(link);

				items.Add(item);
			}
			SyndicationFeed feed = new SyndicationFeed(title, description, feedAlternateLink, feedId, lastUpdateTime, items);
			foreach (var link in BuildLinks(folder, baseUri, repositoryId))
				feed.Links.Add(link);

			var formatter = new Atom10FeedFormatter(feed);
			return formatter;
		}
		internal static Atom10FeedFormatter GetEntry(string repositoryId, string entryId)
		{
			if (repositoryId.ToLower() != "mainrep")
				throw new ArgumentException("Unknown repository identifier: " + repositoryId);

			int id;
			if (!Int32.TryParse(entryId, out id))
				throw new ArgumentException("Unknown object identifier: " + entryId);

			var content = Store.GetContent(id, repositoryId);
			if (content == null)
				throw new ArgumentException("Entry is not exist: " + entryId);

			return BuildEntry(content, repositoryId);
		}
		internal static Atom10FeedFormatter GetParent(string repositoryId, string entryId)
		{
			if (repositoryId.ToLower() != "mainrep")
				throw new ArgumentException("Unknown repository identifier: " + repositoryId);

			int id;
			if (!Int32.TryParse(entryId, out id))
				throw new ArgumentException("Unknown object identifier: " + entryId);

			var content = Store.GetContent(id, repositoryId);
			if (content == null)
				throw new ArgumentException("Entry is not exist: " + entryId);

			var parent = content.ContentHandler.Parent;
			if(parent == null)
				throw new NotSupportedException("Entry is a root object: " + entryId);
			if(parent.NodeType.Name == "")
				throw new ArgumentException("Entry is a root object: " + entryId);

			var parentContent = SNC.Content.Create(parent);
			return BuildEntry(parentContent, repositoryId);
		}
		//-- Object service tools
		private static Atom10FeedFormatter BuildEntry(SNC.Content content, string repositoryId)
		{
			var baseUri = GetBaseUri();

			var title = content.Name;
			var description = GenerateAtomContent(content);
			var lastUpdateTime = new DateTimeOffset(content.ContentHandler.ModificationDate);
			var linkHref = GetEntryActionUri(baseUri, repositoryId, content.Id, "getEntry");
			var feedAlternateLink = new Uri(linkHref);
			var feedId = linkHref;

			CmisObject cmisObject = new CmisObject { Properties = BuildCmisProperties(content), AllowableActions = GetAllowableActions(content) };

			SyndicationFeed feed = new SyndicationFeed(title, description, feedAlternateLink, feedId, lastUpdateTime);
			feed.ElementExtensions.Add(cmisObject, new XmlSerializer(typeof(CmisObject)));
			foreach (var link in BuildLinks(content, baseUri, repositoryId))
				feed.Links.Add(link);

			var formatter = new Atom10FeedFormatter(feed);
			return formatter;
		}
		private static List<SyndicationLink> BuildLinks(SNC.Content content, string baseUri, string repositoryId)
		{
			var links = new List<SyndicationLink>();

			//-- atom links
			links.Add(new SyndicationLink { RelationshipType = "self", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "getEntry")) });
			//links.Add(new SyndicationLink { RelationshipType = "edit", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });
			//-- CMIS links
			//links.Add(new SyndicationLink { RelationshipType = "cmis-allowableactions", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });
			//links.Add(new SyndicationLink { RelationshipType = "cmis-relationships", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });
			//links.Add(new SyndicationLink { RelationshipType = "cmis-type", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });

			if (content.ContentHandler is IFolder)
			{
				//-- atom links
				links.Add(new SyndicationLink { RelationshipType = "cmis-parent", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "getParent")) });
				links.Add(new SyndicationLink { RelationshipType = "cmis-children", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "getChildren")) });
			}
			else
			{
				//-- atom links
				//links.Add(new SyndicationLink { RelationshipType = "edit-media", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });
				//links.Add(new SyndicationLink { RelationshipType = "enclosure", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });
				//links.Add(new SyndicationLink { RelationshipType = "alternate", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });
				//-- CMIS links
				links.Add(new SyndicationLink { RelationshipType = "cmis-parents", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "getParents")) });
				//links.Add(new SyndicationLink { RelationshipType = "cmis-allversions", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });
				//links.Add(new SyndicationLink { RelationshipType = "cmis-latestversion", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });
				//links.Add(new SyndicationLink { RelationshipType = "cmis-stream", Uri = new Uri(GetEntryActionUri(baseUri, repositoryId, content.Id, "__????__")) });
			}

			return links;
		}
		private static List<CmisProperty> BuildCmisProperties(SNC.Content content)
		{
			var cmisProps = new List<CmisProperty>();
			foreach (var fieldName in content.Fields.Keys)
			{
				var field = content.Fields[fieldName];
				var fieldValue = field.GetData();
				if (fieldValue == null)
					continue;
				if (fieldName == "NodeType" || fieldName == "Path" || fieldName == "Password")
					continue;
				if (fieldName == "Index")
				{
					cmisProps.Add(new CmisIntegerProperty { Name = fieldName, Value = Convert.ToInt32(fieldValue) });
				}
				else if (fieldName == "Id")
				{
					cmisProps.Add(new CmisIdProperty { Name = fieldName, Value = fieldValue });
				}
				else if (fieldValue is int)
				{
					cmisProps.Add(new CmisIntegerProperty { Name = fieldName, Value = fieldValue });
				}
				else if (fieldValue is decimal)
				{
					cmisProps.Add(new CmisDecimalProperty { Name = fieldName, Value = fieldValue });
				}
				else if (fieldValue is Boolean)
				{
					cmisProps.Add(new CmisBooleanProperty { Name = fieldName, Value = fieldValue });
				}
				else if (fieldValue is WhoAndWhenField.WhoAndWhenData)
				{
					var whoAndWhenValue = (WhoAndWhenField.WhoAndWhenData)fieldValue;
					cmisProps.Add(new CmisDateTimeProperty { Name = fieldName + "_When", Value = whoAndWhenValue.When });
					cmisProps.Add(new CmisStringProperty { Name = fieldName + "_Who", Value = whoAndWhenValue.Who.Name });
				}
				else if (fieldValue is HyperLinkField.HyperlinkData)
				{
					var hyperlinkData = (HyperLinkField.HyperlinkData)fieldValue;
					cmisProps.Add(new CmisUriProperty { Name = fieldName, Value = hyperlinkData.Href });
				}
				else if (fieldValue is List<string>)
				{
					cmisProps.Add(new CmisStringProperty { Name = fieldName, Value = String.Join(",", ((List<string>)fieldValue).ToArray()) });
				}
				else
				{
					cmisProps.Add(new CmisStringProperty { Name = fieldName, Value = fieldValue.ToString() });
				}
			}
			return cmisProps;
		}
		private static CmisAllowableActions GetAllowableActions(SNC.Content content)
		{
			if (content.ContentHandler is IFolder)
			{
				return new CmisAllowableFolderActions
				{
					CanGetParent = content.ContentHandler.ParentId == 0 ? false : content.ContentHandler.ParentId != Repository.Root.Id,
					CanAddPolicy = false,
					CanGetChildren = true,
					CanDelete = false,
					CanGetDescendants = false,
					CanGetProperties = false,
					CanMove = false,
					CanRemovePolicy = false,
					CanUpdateProperties = false
				};
			}
			else
				return new CmisAllowableDocumentActions
				{
					CanAddPolicy = false,
					CanAddToFolder = false,
					CanCancelCheckout = false,
					CanCheckin = false,
					CanCheckout = false,
					CanDelete = false,
					CanDeleteContent = false,
					CanDeleteVersion = false,
					CanGetAllVersions = false,
					CanGetParents = false,
					CanGetProperties = false,
					CanMove = false,
					CanRemoveFromFolder = false,
					CanRemovePolicy = false,
					CanSetContent = false,
					CanUpdateProperties = false,
					CanViewContent = false
				};
		}
		private static string GenerateAtomContent(SNC.Content content)
		{
			return String.Concat(
				"Name: ", content.Name,
                ", type: ", String.IsNullOrEmpty(content.ContentType.DisplayName) ? content.ContentType.Name : content.ContentType.DisplayName,
				content.ContentHandler is IFolder ? ". This object can contain other objects" : "",
				". ",
				String.IsNullOrEmpty(content.ContentType.Description) ? "" : content.ContentType.Description
				);
		}

		//=============== Uri tools

		private static string GetBaseUri()
		{
			if (WebOperationContext.Current != null)
			{
				//-- this is a REST context
				var v = WebOperationContext.Current.IncomingRequest.UriTemplateMatch;
				return v.BaseUri.OriginalString;

			}
			else
			{
				//-- this is a SOAP context
				var x = OperationContext.Current.Host.BaseAddresses[0].OriginalString;
				return x;
			}
		}
		private static string GetRootActionUri(string baseUri, string repositoryId, string actionName)
		{
			return String.Concat(baseUri, "/", actionName, "/", repositoryId);
		}
		private static string GetEntryActionUri(string baseUri, string repositoryId, int entryId, string actionName)
		{
			return GetEntryActionUri(baseUri, repositoryId, entryId.ToString(), actionName);
		}
		private static string GetEntryActionUri(string baseUri, string repositoryId, string entryId, string actionName)
		{
			if (actionName == null)
				return String.Concat(baseUri, "/", repositoryId, "/", entryId);
			return String.Concat(baseUri, "/", actionName, "/", repositoryId, "/", entryId);
		}

	}
}