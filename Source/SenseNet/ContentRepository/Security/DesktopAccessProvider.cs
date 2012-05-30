using System;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Search;
using System.Web;
using System.Security.Principal;
using System.Threading;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Security
{
    public class SystemPrincipal : IPrincipal
    {
        IUser user;

        public SystemPrincipal(IUser user)
        {
            this.user = user;
        }

        //========================================================== IPrincipal Members

        public IIdentity Identity { get { return user; } }
        public bool IsInRole(string role) { return false; }
    }

    public class DesktopAccessProvider : AccessProvider
    {
        private bool _initialized;

        private IUser CurrentUser
        {
            get
            {
                var user = Thread.CurrentPrincipal.Identity as IUser;
                if (user != null)
                    return user;

                CurrentUser = StartupUser;
                user = User.Administrator;
                CurrentUser = user;
                return user;
            }
            set
            {
                Thread.CurrentPrincipal = new SystemPrincipal(value);
            }
        }

        public override IUser GetCurrentUser()
        {
            if (!_initialized)
            {
                _initialized = true;
                DesktopAccessProvider.ChangeToSystemAccount();
                CurrentUser = User.Administrator;
            }
            return CurrentUser;
        }

        public override void SetCurrentUser(IUser user)
        {
            CurrentUser = user;
        }

        public override bool IsAuthenticated
        {
            get { return true; }
        }
    }
}