using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Workspaces;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Helpers;
using SenseNet.Search;

namespace SenseNet.Portal.Workspaces
{
    public class WorkspaceHelper
    {
        // ======================================================================================== helper classes
        public class WorkspaceGroup
        {
            public GenericContent Workspace { get; set; }
            public Group Group { get; set; }
        }
        public class WorkspaceGroupList
        {
            public GenericContent Workspace { get; set; }
            public IGrouping<GenericContent, WorkspaceGroup> Groups { get; set; }
        }
        public class ManagerData 
        {
            public string ManagerName { get; set; }
            public string ManagerUrl { get; set; }
            public string ManagerImgPath { get; set; }
        }


        // ======================================================================================== static methods
        public static IEnumerable<WorkspaceGroupList> GetWorkspaceGroupLists(User user)
        {
            // 1. query groups under workspaces
            var settings = new QuerySettings { EnableAutofilters = false };
            var groups = SenseNet.Search.ContentQuery.Query("+TypeIs:Group +Workspace:*", settings).Nodes;

            // 2. select groups in which the current user is a member (Owner, Editor)
            var wsGroups = groups.Select(g => new WorkspaceGroup { Workspace = (g as SenseNet.ContentRepository.Group).Workspace, Group = (g as SenseNet.ContentRepository.Group) });
            wsGroups = wsGroups.Where(wsg => user.IsInGroup(wsg.Group));

            // 3. group by workspaces
            var wsGroupLists = from wsg in wsGroups
                               orderby wsg.Workspace.DisplayName
                               group wsg by wsg.Workspace into w
                               select new WorkspaceGroupList { Workspace = w.Key, Groups = w };
            
            return wsGroupLists;
        }
        public static IEnumerable<Node> GetViaGroups(User user, WorkspaceGroup groupInfo)
        {
            var principals = user.GetPrincipals();

            return groupInfo.Group.Members.Where(m => principals.Contains(m.Id)).OrderBy(m => m.DisplayName);
        }
        public static ManagerData GetManagerData(User manager)
        {
            var imgSrc = "/Root/Global/images/orgc-missinguser.png?dynamicThumbnail=1&width=48&height=48";
            var managerName = "No manager associated";
            var manUrl = string.Empty;

            if (manager != null)
            {
                managerName = manager.FullName;
                var managerC = SenseNet.ContentRepository.Content.Create(manager);
                manUrl = Actions.ActionUrl(managerC, "Profile");

                var imgField = managerC.Fields["Avatar"] as SenseNet.ContentRepository.Fields.ImageField;
                imgField.GetData(); // initialize image field
                var param = SenseNet.ContentRepository.Fields.ImageField.GetSizeUrlParams(imgField.ImageMode, 48, 48);
                if (!string.IsNullOrEmpty(imgField.ImageUrl))
                    imgSrc = imgField.ImageUrl + param;
            }

            return new ManagerData { ManagerName = managerName, ManagerUrl = manUrl, ManagerImgPath = imgSrc };
        }
    }
}
