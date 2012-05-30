using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Portal.Workspaces
{
    public class WorkspaceJournalsDataContext : WsJournalDataContext
    {
        public WorkspaceJournalsDataContext() : base(ConfigurationManager.ConnectionStrings["SnCrMsSql"].ConnectionString) { }
    }

    public static class Journals
    {

        public static void Add(string what, string wherewith, string who, DateTime when, int nodeId, string displayName, string nodeTypeName, string sourcePath, string targetPath, string targetDisplayName, string details, bool hidden)
        {
            var db = new WorkspaceJournalsDataContext();
            db.JournalItems.InsertOnSubmit(
                new JournalItem
                {
                    What = what,
                    When = when,
                    Wherewith = wherewith,
                    Who = who,
                    NodeId = nodeId,
                    DisplayName = displayName,
                    NodeTypeName = nodeTypeName,
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    TargetDisplayName = targetDisplayName,
                    Details = details,
                    Hidden = hidden
                });
            db.SubmitChanges();
        }

        public static JournalItem GetSingleItem(int id)
        {
            var db = new WorkspaceJournalsDataContext();

            return (from item in db.JournalItems
                    where item.Id == id
                    select item).FirstOrDefault();
        }

        public static IEnumerable<JournalItem> GetItemsForUser(IUser user)
        {
            var db = new WorkspaceJournalsDataContext();

            return (from item in db.JournalItems
                    where item.Who == user.Name && !item.Wherewith.Contains("/Like-") && !item.Wherewith.Contains("/Post-") && !item.Wherewith.Contains("/Comment-") && !item.Wherewith.EndsWith("/Likes") && !item.Wherewith.EndsWith("/Posts") && !item.Wherewith.EndsWith("/Comments")
                    orderby item.When descending
                    select item);
        }

        public static IEnumerable<JournalItem> GetItemsForWorkspace(string path)
        {
            var db = new WorkspaceJournalsDataContext();

            return (from item in db.JournalItems
                    where (item.Wherewith == path || item.Wherewith.StartsWith(path + "/")) && !item.Wherewith.Contains("/Like-") && !item.Wherewith.Contains("/Post-") && !item.Wherewith.Contains("/Comment-") && !item.Wherewith.EndsWith("/Likes") && !item.Wherewith.EndsWith("/Posts") && !item.Wherewith.EndsWith("/Comments")
                    orderby item.When descending
                    select item);
        }

        public static IEnumerable<JournalItem> GetItemsForContent(int nodeid)
        {
            var db = new WorkspaceJournalsDataContext();

            return (from item in db.JournalItems
                    where item.NodeId == nodeid && !item.Wherewith.Contains("/Like-") && !item.Wherewith.Contains("/Post-") && !item.Wherewith.Contains("/Comment-") && !item.Wherewith.EndsWith("/Likes") && !item.Wherewith.EndsWith("/Posts") && !item.Wherewith.EndsWith("/Comments")
                    orderby item.When descending
                    select item);
        }

        public static IEnumerable<JournalItem> Get(string path, int maxCount)
        {
            return Get(path, maxCount, 0);
        }

        public static IEnumerable<JournalItem> Get(string path, int maxCount, int skip)
        {
            return Get(path, maxCount, skip, false);
        }

        public static IEnumerable<JournalItem> Get(string path, int maxCount, int skip, bool descending)
        {
            var db = new WorkspaceJournalsDataContext();

            if (descending)
            {
                return (from item in db.JournalItems
                        where item.Wherewith == path || item.Wherewith.StartsWith(path + "/")
                        orderby item.When descending
                        select item).Skip(skip).Take(maxCount);
            }
            else
            {
                return (from item in db.JournalItems
                        where item.Wherewith == path || item.Wherewith.StartsWith(path + "/")
                        select item).Skip(skip).Take(maxCount);
            }
        }
    }
}
