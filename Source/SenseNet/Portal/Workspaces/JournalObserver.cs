using System;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.Diagnostics;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.Workspaces
{
    public class JournalObserver : NodeObserver
    {
        private static bool? _createJournalItems = null;
        public static bool CreateJournalItems
        {
            get
            {
                if (!_createJournalItems.HasValue)
                {
                    bool result;
                    if (bool.TryParse(ConfigurationManager.AppSettings["CreateJournalItems"], out result))
                        _createJournalItems = result;
                    else
                        _createJournalItems = true;
                }
                return _createJournalItems.Value;
            }
        }

        protected override void OnNodeCreated(object sender, NodeEventArgs e)
        {
            // copy is logged separately
            if (!e.SourceNode.CopyInProgress)
                Log(e);
        }
        protected override void OnNodeModified(object sender, NodeEventArgs e)
        {
            Log(e);
        }
        protected override void OnNodeDeleted(object sender, NodeEventArgs e)
        {
            Log(e);
        }
        protected override void OnNodeDeletedPhysically(object sender, NodeEventArgs e)
        {
            Log(e);
        }

        protected override void OnNodeMoved(object sender, NodeOperationEventArgs e)
        {
            Log(e, e.OriginalSourcePath, e.TargetNode);
        }

        protected override void OnNodeCopied(object sender, NodeOperationEventArgs e)
        {
            Log(e, null, e.TargetNode);
        }

        private void Log(NodeEventArgs e)
        {
            Log(e, null, null);
        }

        private void Log(NodeEventArgs e, string sourcePath, Node target)
        {
            if (!CreateJournalItems)
                return;

            var path = "[null]";
            if (e.SourceNode == null)
                return;

            if (e.SourceNode.NodeOperation == NodeOperation.TemplateChildCopy || e.SourceNode.NodeOperation == NodeOperation.HiddenJournal)
                return;

            try
            {
                path = e.SourceNode.Path;
            }
            catch(Exception ee) //logged
            {
                Logger.WriteException(ee);
                path = "[error]";
            }

            var userName = "[nobody]";
            try
            {
                if (e.User != null)
                    userName = e.User.Name;
            }
            catch(Exception eee) //logged
            {
                Logger.WriteException(eee);
                userName = "[error]";
            }
            string info = null;
            if (e.ChangedData != null)
            {
                var sb = new StringBuilder();
                foreach (var changedData in e.ChangedData)
                {
                    if (changedData.Name == "NodeModificationDate" ||
                        changedData.Name == "NodeModifiedById" ||
                        changedData.Name == "NodeModifiedBy" ||
                        changedData.Name == "ModificationDate" ||
                        changedData.Name == "ModifiedById" ||
                        changedData.Name == "ModifiedBy")
                        continue;

                    sb.Append(changedData.Name + ", ");
                }
                info = "Changed Fields: " + sb.ToString().TrimEnd(',',' ');
            }
            
            var displayName = string.IsNullOrEmpty(e.SourceNode.DisplayName) ? e.SourceNode.Name : e.SourceNode.DisplayName;
            var targetPath = target == null ? null : target.Path;
            var targetDisplayName = target == null ? null : string.IsNullOrEmpty(target.DisplayName) ? target.Name : target.DisplayName;

            Journals.Add(e.EventType.ToString(), path, userName, e.Time, e.SourceNode.Id, displayName, e.SourceNode.NodeType.Name, sourcePath, targetPath, targetDisplayName, info, false);
        }
    }
}
