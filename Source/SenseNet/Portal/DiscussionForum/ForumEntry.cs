using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Virtualization;
using SenseNet.Search;
using System.Diagnostics;

namespace SenseNet.Portal.DiscussionForum
{
    [ContentHandler]
    public class ForumEntry : GenericContent
    {
        public ForumEntry(Node parent) : this(parent, null) { }
		public ForumEntry(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected ForumEntry(NodeToken nt) : base(nt) { }

        [RepositoryProperty("ReplyTo", RepositoryDataType.Reference)]
        public virtual Node ReplyTo
        {
            get { return this.GetReference<Node>("ReplyTo"); }
            set { this.SetReference("ReplyTo", value); }
        }

        [RepositoryProperty("PostedBy", RepositoryDataType.Reference)]
        public virtual Node PostedBy
        {
            get { return this.GetReference<Node>("PostedBy"); }
            set { this.SetReference("PostedBy", value); }
        }

        [RepositoryProperty("SerialNo")]
        public virtual int SerialNo
        {
            get { return (int)base.GetProperty("SerialNo"); }
            set { this["SerialNo"] = value; }
        }

        private int? _replyToNo;
        public virtual int ReplyToNo
        {
            get
            {
                if (_replyToNo == null)
                {
                    if (ReplyTo != null)
                        _replyToNo = ((ForumEntry)ReplyTo).SerialNo;
                }

                return _replyToNo ?? -1;
            }
        }

        //private string _replyToUrl;
        //public string ReplyToUrl
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(_replyToUrl))
        //        {
        //            var ContextElement = Content.Create(this.Parent);

        //            var a = SenseNet.ApplicationModel.ActionFramework.GetAction("Add",
        //                ContextElement, PortalContext.Current.RequestedUri.PathAndQuery,
        //                new { ReplyTo = this.Path });

        //            _replyToUrl = a.Uri;
        //        }

        //        return _replyToUrl;
        //    }
        //}

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "ReplyTo":
                    return this.ReplyTo;
                case "PostedBy":
                    return this.PostedBy;
                case "ReplyToNo":
                    return this.ReplyToNo;
                case "SerialNo":
                    return this.SerialNo;
                //case "ReplyToUrl":
                //    return this.ReplyToUrl;
                default:
                    return base.GetProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "ReplyTo":
                    this.ReplyTo = (Node)value;
                    break;
                case "PostedBy":
                    this.PostedBy = (Node)value;
                    break;
                case "SerialNo":
                    this.SerialNo = (int)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        protected override void OnCreating(object sender, SenseNet.ContentRepository.Storage.Events.CancellableNodeEventArgs e)
        {
            base.OnCreating(sender, e);

            this.PostedBy = this.CreatedBy;
            if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
            {
                try
                {
                    var querystring = string.Format("+InTree:'{0}' +Type:'ForumEntry' +CreationDate:<'{1}' .COUNTONLY", this.ParentPath, this.CreationDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    var q = ContentQuery.Query(querystring, new QuerySettings { Top = int.MaxValue });
                    this.SerialNo = q.Count;
                }
                catch
                {
                    Trace.Write("Lucene query failed on node " + this.Path);
                }
            }
        }

        public IEnumerable<ForumEntry> GetReplies()
        {
            var q = new NodeQuery();
            q.Add(new IntExpression(IntAttribute.ParentId, ValueOperator.Equal, this.ParentId));
            q.Add(new ReferenceExpression(PropertyType.GetByName("ReplyTo"), new IntExpression(IntAttribute.Id, ValueOperator.Equal, this.Id)));

            var res = q.Execute();

            if (res.Count == 0)
                return new List<ForumEntry>();

            return res.Nodes.OfType<ForumEntry>();
        }
    }
}
