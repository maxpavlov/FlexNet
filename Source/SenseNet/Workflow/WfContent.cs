using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using Repo = SenseNet.ContentRepository;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Workflow
{
    [Serializable]
    public class WfContent
    {
        public WfContent() { }
        public WfContent(Node n) { Path = n.Path; Id = n.Id; }
        public WfContent(string path) { Path = path; }

        public string Path { get; set; }

        [NonSerialized]
        private int __id;
        public int Id
        {
            get
            {
                if (__id == 0)
                {
                    var nodeHead = NodeHead.Get(Path);
                    if (nodeHead != null)
                        __id = nodeHead.Id;
                }
                return __id;
            }
            set
            {
                __id = value;
            }
        }

        public WfReference Reference
        {
            get
            {
                return new WfReference(Path);
            }
        }

        public WfContentCollection References(string fieldName)
        {
            return new WfContentCollection(Path, fieldName);
        }

        public object this[string fieldName]
        {
            get
            {
                var content = Repo.Content.Load(Path);
                if(content == null)
                    throw new ApplicationException(String.Concat("Content not found: ", Path));

                Field field;
                if(content.Fields.TryGetValue(fieldName, out field))
                {
                    var value = content[fieldName];
                    var listofstring = value as IEnumerable<string>;
                    if (listofstring != null)
                        value = string.Join(",", listofstring);
                    return value;
                }

                var gcontent = content.ContentHandler as GenericContent;
                if (gcontent != null)
                    return gcontent.GetProperty(fieldName);

                throw new ApplicationException(String.Format("Field '{0}' not found in a {1} content: {2} ", fieldName, content.ContentType.Name, content.Path));
            }
            set
            {
                var content = Repo.Content.Load(Path);
                content[fieldName] = value;
                content.ContentHandler.DisableObserver(typeof(WorkflowNotificationObserver));
                content.Save();
                //TODO: WF: Write back the timestamp (if the content is the relatedContent)
            }
        }

        public string ActionUrl(string ActionName)
        {
            var node = Node.LoadNode(Path);
            var content = Repo.Content.Create(node);
            return ActionFramework.GetAction(ActionName, content, null, null).Uri;
        }

        public override string ToString()
        {
            return Path;
        }

        public object GetField(string fieldName)
        {
            return this[fieldName];
        }

        public void Delete()
        {
            Node.ForceDelete(this.Id);
        }
        public void SetPermission(IUser user, PermissionType permissionType, PermissionValue permissionValue)
        {
            var node = Node.LoadNode(Path);
            if(node != null)
                node.Security.SetPermission(user, true, permissionType, permissionValue);
        }

    }
}
