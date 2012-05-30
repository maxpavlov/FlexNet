using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.Wall;
using SenseNet.ContentRepository;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets.Wall
{
    public class UserWallPortlet : WallPortlet
    {
        public UserWallPortlet()
        {
            this.Name = "User wall";
            this.Description = "This portlet displays a wall with posts and comments (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Enterprise20);
        }

        protected override IEnumerable<Portal.Wall.PostInfo> GatherPosts()
        {
            var profile = this.ContextNode as UserProfile;
            if (profile == null)
                return null;

            return DataLayer.GetPostsForUser(profile.User, this.ContextNode.Path);
        }
    }
}
