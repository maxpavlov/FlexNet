using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Services.IdentityManagement;
using System.Web;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Web;
using SenseNet.ContentRepository;

namespace SenseNet.Services.IdentityManagement
{
    public class IdentityManagementService : IIdentityManagementService
    {
        #region IIdentityManagementService Members

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        private string GetSiteUrl()
        {
            string url = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri.ToString();
            return url;
        }

        public SenseNet.Services.IdentityManagement.GetServiceUrlsResult GetServiceUrls(string OriginalUrl)
        {
            string encodedOriginalUrl = HttpUtility.UrlEncode(OriginalUrl);
            return new GetServiceUrlsResult() 
            {
                SignOnUrl = "http://samplehost/login?OriginalUrl=" + encodedOriginalUrl,
                SignOffUrl = "http://samplehost/login?Command=SignOff&OriginalUrl=" + encodedOriginalUrl,
                RegistrationUrl = "http://samplehost/Home/Registration"
            };
        }

        public SenseNet.Services.IdentityManagement.LookupIdentityResult LookupIdentity(string identityToken)
        {
            string username = string.Empty;
            try
            {
                username = CryptoApi.Decrypt(identityToken, "sensenet60", "SenseNetContentRepository");
            }
            catch //rethrow
            {
                throw new InvalidTokenException();
            }

            Debug.WriteLine("Token resolved to:" + username);
            string[] userParts = username.Split('\\');
            User user = null;
            try
            {
                user = User.Load(userParts[0], userParts[1]);
            }
            catch(Exception e) //rethrow
            {
                throw new SecurityException(e.Message, e);
            }

            if (user == null)
            {
                throw new InvalidParameterException(username);
            }

            return new LookupIdentityResult() 
            { 
                Identity = new Identity(user),
            };
        }

        public SenseNet.Services.IdentityManagement.LookupIdentityResult LookupIdentityEx(SenseNet.Services.IdentityManagement.LookupIdentityParameters parameters)
        {
            throw new NotImplementedException();
        }


        #endregion
    }


}