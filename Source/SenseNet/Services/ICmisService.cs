using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Syndication;
using SenseNet.Services.Cmis;

namespace SenseNet.Services
{
	[ServiceContract(Namespace = CmisService.CmisSnExtNamespace)]
	public interface ICmisService
	{
		[OperationContract]
		[WebGet(UriTemplate = "/getRepositories")]
		AtomPub10ServiceDocumentFormatter GetRepositories();

		[OperationContract]
		[WebGet(UriTemplate = "/getRepositoryInfo/{repositoryId}")]
		AtomPub10ServiceDocumentFormatter GetRepositoryInfo(string repositoryId);

		[OperationContract]
		[WebGet(UriTemplate = "/getRoot/{repositoryId}")]
		Atom10FeedFormatter GetRoot(string repositoryId);

		[OperationContract]
		[WebGet(UriTemplate = "/getChildren/{repositoryId}/{folderId}")]
		Atom10FeedFormatter GetChildren(string repositoryId, string folderId);

		[OperationContract]
		[WebGet(UriTemplate = "/getEntry/{repositoryId}/{entryId}")]
		Atom10FeedFormatter GetEntry(string repositoryId, string entryId);

		[OperationContract]
		[WebGet(UriTemplate = "/getParent/{repositoryId}/{entryId}")]
		Atom10FeedFormatter GetParent(string repositoryId, string entryId);

		[OperationContract]
		[WebGet(UriTemplate = "/getParents/{repositoryId}/{entryId}")]
		Atom10FeedFormatter GetParents(string repositoryId, string entryId);
	}
}