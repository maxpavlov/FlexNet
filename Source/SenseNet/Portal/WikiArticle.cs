using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.Portal.UI;

namespace SenseNet.Portal
{
    [ContentHandler]
    public class WikiArticle : GenericContent
    {
        public WikiArticle(Node parent) : this(parent, null) { }
		public WikiArticle(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected WikiArticle(NodeToken nt) : base(nt) { }

        //==================================================================================== Properties

        public const string WIKIARTICLETEXT = "WikiArticleText";
        [RepositoryProperty(WIKIARTICLETEXT, RepositoryDataType.Text)]
        public string WikiArticleText
        {
            get { return this.GetProperty<string>(WIKIARTICLETEXT); }
            set
            {
                this[WIKIARTICLETEXT] = value;

                //we need to refresh the list of referenced titles in case of the article text changed
                this.ReferencedWikiTitles = WikiTools.GetReferencedTitles(this);
            }
        }

        public const string REFERENCEDWIKITITLES = "ReferencedWikiTitles";
        [RepositoryProperty(REFERENCEDWIKITITLES, RepositoryDataType.Text)]
        public string ReferencedWikiTitles
        {
            get { return this.GetProperty<string>(REFERENCEDWIKITITLES); }
            set { this[REFERENCEDWIKITITLES] = value; }
        }

        //==================================================================================== Overrides

        private string _oldDisplayName;

        protected override void OnLoaded(object sender, NodeEventArgs e)
        {
            base.OnLoaded(sender, e);

            _oldDisplayName = this.DisplayName;
        }

        protected override void OnModifying(object sender, CancellableNodeEventArgs e)
        {
            base.OnModifying(sender, e);

            this.ReferencedWikiTitles = WikiTools.GetReferencedTitles(this);
        }

        protected override void OnModified(object sender, NodeEventArgs e)
        {
            base.OnModified(sender, e);

            var oldName = RepositoryPath.GetFileName(e.OriginalSourcePath);
            if (oldName.CompareTo(e.SourceNode.Name) != 0 || this.DisplayName.CompareTo(_oldDisplayName) != 0)
                WikiTools.RefreshArticlesAsync(this, oldName, _oldDisplayName);
        }

        protected override void OnCreating(object sender, CancellableNodeEventArgs e)
        {
            base.OnCreating(sender, e);

            this.ReferencedWikiTitles = WikiTools.GetReferencedTitles(this);
        }

        protected override void OnCreated(object sender, NodeEventArgs e)
        {
            base.OnCreated(sender, e);
            
            WikiTools.RefreshArticlesAsync(this, WikiArticleAction.Create);
        }

        protected override void OnDeletedPhysically(object sender, NodeEventArgs e)
        {
            base.OnDeletedPhysically(sender, e);

            WikiTools.RefreshArticlesAsync(this, WikiArticleAction.Delete);
        }

        //==================================================================================== Property get/set

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case WIKIARTICLETEXT:
                    return this.WikiArticleText;
                case REFERENCEDWIKITITLES:
                    return this.ReferencedWikiTitles;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case WIKIARTICLETEXT:
                    this.WikiArticleText = (string)value;
                    break;
                case REFERENCEDWIKITITLES:
                    //this is a readonly property, modified only by this handler itself
                    //this.ReferencedWikiTitles = (string)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }
    }
}
