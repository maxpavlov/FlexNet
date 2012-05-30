using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class UserProfileAction : UrlAction
    {
        private string _userProfileUrl;

        public override void Initialize(Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            if (context == null || this.Forbidden)
                return;

            var user = context.ContentHandler as User;
            if (user == null)
                return;

            var s = SerializeParameters(GetParameteres());
            if (!Repository.UserProfilesEnabled)
            {
                _userProfileUrl = context.Path;
            }
            else
            {
                if (!user.IsProfileExist())
                {
                    user.CreateProfile();
                }

                _userProfileUrl = user.GetProfilePath();
            }

            if (!string.IsNullOrEmpty(s))
            {
                _userProfileUrl = ContinueUri(_userProfileUrl);
                _userProfileUrl += s.Substring(1);
            }
        }

        public override string Uri
        {
            get
            {
                if (string.IsNullOrEmpty(_userProfileUrl))
                    return base.Uri;

                var finalUri = _userProfileUrl;

                if (this.IncludeBackUrl && !string.IsNullOrEmpty(this.BackUri))
                {
                    finalUri = ContinueUri(_userProfileUrl);
                    finalUri += string.Format("{0}={1}", PortalContext.BackUrlParamName, System.Uri.EscapeDataString(this.BackUri));
                }

                return finalUri;
            }
        }
    }
}
