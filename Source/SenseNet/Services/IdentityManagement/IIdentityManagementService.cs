using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using SenseNet.Services.IdentityManagement;
using System.ServiceModel.Web;

namespace SenseNet.Services.IdentityManagement
{
    [ServiceContract(Namespace="http://schemas.sensenet.com/services/ims")]
    public interface IIdentityManagementService
    {
        [OperationContract]
        [WebGet(UriTemplate="/GetServiceUrls?OriginalUrl={OriginalUrl}")]
        GetServiceUrlsResult GetServiceUrls(string OriginalUrl);

        [OperationContract]
        [WebGet(UriTemplate="/LookupIdentity?Token={identityToken}")]
        LookupIdentityResult LookupIdentity(string identityToken);

        [OperationContract]
        LookupIdentityResult LookupIdentityEx(LookupIdentityParameters parameters);
    }

	public class IdentityManagementServiceFactory : MultiHostServiceFactory<IdentityManagementService>
	{
	}

}