using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.Virtualization;
using SenseNet.Search;
using SenseNet.ContentRepository;
using System.Web.Script.Serialization;

namespace SenseNet.Portal.Workspaces
{
    public class WorkspaceController : Controller
    {
        internal class GroupData
        {
            public int groupId { get; set; }
            public string ids { get; set; }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AddMembers(string data, string rnd)
        {
            AssertPermission();
            var ser = new JavaScriptSerializer();
            var allGroupData = ser.Deserialize<IEnumerable<GroupData>>(data);

            foreach (var groupData in allGroupData)
            {
                if (string.IsNullOrEmpty(groupData.ids))
                    continue;

                var group = Node.LoadNode(groupData.groupId) as Group;
                if (group == null)
                    continue;

                foreach (var id in groupData.ids.Split(','))
                {
                    var node = Node.LoadNode(id);
                    if (node == null)
                        continue;

                    var iusr = node as IUser;
                    if (iusr != null)
                        group.AddMember(iusr);
                    else
                        group.AddMember(node as IGroup);
                }

                group.Save();
            }

            return null;
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RemoveMember(int groupId, int memberId, string rnd)
        {
            AssertPermission();

            var members = Node.LoadNode(groupId) as Group;
            if (members == null)
                return null;

            var node = Node.LoadNode(memberId);
            var iusr = node as IUser;
            if (iusr != null)
                members.RemoveMember(iusr);
            else
                members.RemoveMember(node as IGroup);

            members.Save();

            return null;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult BrowseProfile(string path, string back)
        {
            path = HttpUtility.UrlDecode(path);
            back = HttpUtility.UrlDecode(back);
            var url = path;
            var user = Node.Load<User>(path);

            if (user != null)
            {
                if (Repository.UserProfilesEnabled)
                {
                    if (!user.IsProfileExist())
                    {
                        user.CreateProfile();
                    }

                    url = user.GetProfilePath();
                }

                if (!string.IsNullOrEmpty(back))
                    url = string.Format("{0}?{1}={2}", url, PortalContext.BackUrlParamName, back);
            }
            else
            {
                url = back;
            }

            return RedirectPermanent(url);
        }

        //===================================================================== Helper methods
        private static readonly string PlaceholderPath = "/Root/System/PermissionPlaceholders/Workspace-mvc";

        private void AssertPermission()
        {
            var permissionContent = Node.LoadNode(PlaceholderPath);
            if (permissionContent == null || !permissionContent.Security.HasPermission(PermissionType.RunApplication))
                throw new SenseNetSecurityException("Access denied for " + PlaceholderPath);
        }
    }
}
