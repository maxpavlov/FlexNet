using System;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Search;
using System.Web;
using System.Threading;

namespace SenseNet.ContentRepository.Security
{
    public class UserAccessProvider : AccessProvider
    {
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
                Thread.CurrentPrincipal = new PortalPrincipal(value);
            }
        }

        public override IUser GetCurrentUser()
        {
			if (HttpContext.Current == null)
			{
				//return GetDesktopUser();
                return this.CurrentUser;
            }
			else
			{
				IUser currentUser = null;
				if (HttpContext.Current.User != null)
					currentUser = HttpContext.Current.User.Identity as IUser;

				if (currentUser == null)
				{
					SetCurrentUser(this.StartupUser);
                    currentUser = this.StartupUser;
				}

				return currentUser;
			}
        }

        public override void SetCurrentUser(IUser user)
        {
			if (HttpContext.Current == null)
                //SetDesktopUser(user);
                this.CurrentUser = user;
            else
				HttpContext.Current.User = new PortalPrincipal(user);
        }

        //bool _initialized;
        //IUser _currentDesktopUser;

        //private IUser GetDesktopUser()
        //{
        //    if (!_initialized)
        //    {
        //        _initialized = true;
        //        DesktopAccessProvider.ChangeToSystemAccount();
        //        _currentDesktopUser = User.Administrator;
        //    }

        //    return _currentDesktopUser;
        //}

        //private void SetDesktopUser(IUser user)
        //{
        //    this.CurrentUser = user;
        //}

        public override bool IsAuthenticated
        {
            get
            {
                if (HttpContext.Current == null)
                    return false;

                System.Security.Principal.IPrincipal currentPrincipal =
                    HttpContext.Current.User;

                if (currentPrincipal == null || currentPrincipal.Identity == null || currentPrincipal.Identity.IsAuthenticated == false)
                    return false;

                return true;
            }
        }
    }
}