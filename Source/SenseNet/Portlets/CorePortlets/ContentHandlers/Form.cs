using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
	[ContentHandler]
	public class Form : ContentList
	{
        public Form(Node parent) : this(parent, null) { }
		public Form(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected Form(NodeToken nt) : base(nt)	{ }

		[RepositoryProperty("EmailList", RepositoryDataType.Text)]
		public string EmailList
		{
			get { return this.GetProperty<string>("EmailList"); }
			set { this["EmailList"] = value; }
		}

        [RepositoryProperty("TitleSubmitter", RepositoryDataType.String)]
        public string TitleSubmitter
        {
            get { return this.GetProperty<string>("TitleSubmitter"); }
            set { this["TitleSubmitter"] = value; }
        }

		[RepositoryProperty("Description", RepositoryDataType.Text)]
		public string Description
		{
			get { return this.GetProperty<string>("Description"); }
			set { this["Description"] = value; }
		}

		[RepositoryProperty("AfterSubmitText", RepositoryDataType.Text)]
		public string AfterSubmitText
		{
			get { return this.GetProperty<string>("AfterSubmitText"); }
			set { this["AfterSubmitText"] = value; }
		}

        [RepositoryProperty("EmailFrom", RepositoryDataType.String)]
        public string EmailFrom
        {
			get { return this.GetProperty<string>("EmailFrom"); }
			set { this["EmailFrom"] = value; }
		}

        [RepositoryProperty("EmailFromSubmitter", RepositoryDataType.String)]
        public string EmailFromSubmitter
        {
            get { return this.GetProperty<string>("EmailFromSubmitter"); }
            set { this["EmailFromSubmitter"] = value; }
        }

        [RepositoryProperty("EmailField", RepositoryDataType.String)]
        public string EmailField
        {
			get { return this.GetProperty<string>("EmailField"); }
			set { this["EmailField"] = value; }
		}

        [RepositoryProperty("EmailTemplate", RepositoryDataType.Text)]
        public string EmailTemplate
        {
			get { return this.GetProperty<string>("EmailTemplate"); }
			set { this["EmailTemplate"] = value; }
		}

        [RepositoryProperty("EmailTemplateSubmitter", RepositoryDataType.Text)]
        public string EmailTemplateSubmitter
        {
            get { return this.GetProperty<string>("EmailTemplateSubmitter"); }
            set { this["EmailTemplateSubmitter"] = value; }
        }
	}
}