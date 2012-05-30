using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using SenseNet.Services.Cmis;
using System.ServiceModel.Activation;

namespace SenseNet.Services
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class CmisService : ICmisService
	{
		public const string AppNamespace = "http://www.w3.org/2007/app";
		public const string AtomNamespace = "http://www.w3.org/2005/Atom";
		public const string CmisNamespace = "http://www.cmis.org/2008/05";
		public const string CmisSnExtNamespace = "http://schemas.sensenet.com/services/cmis";

		public AtomPub10ServiceDocumentFormatter GetRepositories()
		{
			SetResponseHeaders();
			return Cmis.CmisFeedBuilder.BuildWorkspace("MainRep");
		}
		public AtomPub10ServiceDocumentFormatter GetRepositoryInfo(string repositoryId)
		{
			SetResponseHeaders();
			return Cmis.CmisFeedBuilder.BuildWorkspace("MainRep");
		}
		public Atom10FeedFormatter GetRoot(string repositoryId)
		{
			SetResponseHeaders();
			return GetChildren(repositoryId, "2");
		}
		public Atom10FeedFormatter GetChildren(string repositoryId, string folderId)
		{
			SetResponseHeaders();
			return Cmis.CmisFeedBuilder.BuildChildren(repositoryId, folderId);
		}
		public Atom10FeedFormatter GetEntry(string repositoryId, string entryId)
		{
			SetResponseHeaders();
			return Cmis.CmisFeedBuilder.GetEntry(repositoryId, entryId);
		}
		public Atom10FeedFormatter GetParents(string repositoryId, string entryId)
		{
			return GetParent(repositoryId, entryId);
		}
		public Atom10FeedFormatter GetParent(string repositoryId, string entryId)
		{
			SetResponseHeaders();
			return Cmis.CmisFeedBuilder.GetParent(repositoryId, entryId);
		}

		private void SetResponseHeaders()
		{
			if (WebOperationContext.Current != null)
			{
				//-- this is a REST context
				//WebOperationContext.Current.OutgoingResponse.Headers.Add(System.Net.HttpResponseHeader.ContentType, "application/atom+xml;type=feed");
			}
			else
			{
				//-- this is a SOAP context
			}
		}
	}
}