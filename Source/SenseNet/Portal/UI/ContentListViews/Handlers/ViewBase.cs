using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI.PortletFramework;
using System.Xml.XPath;
using System.Web.UI;
using System.IO;
using System.Xml;
using SenseNet.Search;

namespace SenseNet.Portal.UI.ContentListViews.Handlers
{
    [ContentHandler]
    public abstract class ViewBase : SenseNet.ContentRepository.File, IView
    {
        public ViewBase(Node parent) : this(parent, null) { }
		public ViewBase(Node parent, string nodeTypeName) : base(parent, nodeTypeName) {}
        protected ViewBase(NodeToken nt) : base(nt) {}

        #region ContentProperties

        public bool IsDefault
        {
            get
            {
                var clist = ContentList.GetContentListByParentWalk(this);
                return (clist != null && clist.DefaultView == this.Name);
            }
        }

        [RepositoryProperty("Template", RepositoryDataType.Reference)]
        public Node Template
        {
            get { return GetReference<Node>("Template"); }
            set { this.SetReference("Template", value); }
        }

        [RepositoryProperty("FilterXml", RepositoryDataType.Text)]
        public string FilterXml
        {
            get { return GetProperty<string>("FilterXml"); }
            set { this["FilterXml"] = value; }
        }

        [RepositoryProperty("EnableAutofilters", RepositoryDataType.String)]
        public virtual FilterStatus EnableAutofilters
        {
            get
            {
                var enumVal = base.GetProperty<string>("EnableAutofilters");
                if (string.IsNullOrEmpty(enumVal))
                    return FilterStatus.Default;

                return (FilterStatus)Enum.Parse(typeof(FilterStatus), enumVal);
            }
            set
            {
                this["EnableAutofilters"] = Enum.GetName(typeof(FilterStatus), value);
            }
        }

        [RepositoryProperty("EnableLifespanFilter", RepositoryDataType.String)]
        public virtual FilterStatus EnableLifespanFilter
        {
            get
            {
                var enumVal = base.GetProperty<string>("EnableLifespanFilter");
                if (string.IsNullOrEmpty(enumVal))
                    return FilterStatus.Default;

                return (FilterStatus)Enum.Parse(typeof(FilterStatus), enumVal);
            }
            set
            {
                this["EnableLifespanFilter"] = Enum.GetName(typeof(FilterStatus), value);
            }
        }

        public const string VIEWQUERYTOP = "QueryTop";
        [RepositoryProperty(VIEWQUERYTOP)]
        public int QueryTop
        {
            get { return this.GetProperty<int>(VIEWQUERYTOP); }
            set { this[VIEWQUERYTOP] = value; }
        }

        public const string VIEWQUERYSKIP = "QuerySkip";
        [RepositoryProperty(VIEWQUERYSKIP)]
        public int QuerySkip
        {
            get { return this.GetProperty<int>(VIEWQUERYSKIP); }
            set { this[VIEWQUERYSKIP] = value; }
        }

        public bool FilterIsContentQuery
        {
            get
            {
                var filter = FilterXml;

                return string.IsNullOrEmpty(filter) || !filter.StartsWith("<");
            }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "Template":
                    return this.Template;
                case "filterXml":
                    return this.FilterXml;
                case "IsDefault":
                    return this.IsDefault;
                case "EnableAutofilters":
                    return this.EnableAutofilters;
                case "EnableLifespanFilter":
                    return this.EnableLifespanFilter;
                case VIEWQUERYTOP:
                    return this.QueryTop;
                case VIEWQUERYSKIP:
                    return this.QuerySkip;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "Template":
                    this.Template = (Node)value;
                    break;
                case "FilterXml":
                    this.FilterXml = (string)value;
                    break;
                case "EnableAutofilters":
                    this.EnableAutofilters = (FilterStatus)value;
                    break;
                case "EnableLifespanFilter":
                    this.EnableLifespanFilter = (FilterStatus)value;
                    break;
                case VIEWQUERYTOP:
                    this.QueryTop = (int)value;
                    break;
                case VIEWQUERYSKIP:
                    this.QuerySkip = (int)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        #endregion

        public void SetAsDefault()
        {
            var clist = ContentList.GetContentListByParentWalk(this);
            clist.DefaultView = this.Name;
            clist.Save();
        }

        protected virtual IXPathNavigable GetSource()
        {
            var emptyXml = new XmlDocument();
            return emptyXml;
        }

        protected virtual void AddXsltParameters(System.Xml.Xsl.XsltArgumentList argumentList)
        {
            var currentList = ContentList.GetContentListByParentWalk(this);
            var listPath = currentList == null ? string.Empty : currentList.Path;

            argumentList.AddParam("listPath", "", listPath);
        }

        private void SetBinary()
        {
            if (Template == null)
            {
                //throw new InvalidOperationException("Template property is empty.");
                return;
            }

            var xsltTransform = Xslt.GetXslt(Template.Path, false);

            IXPathNavigable viewSourceXml = GetSource();

            if (viewSourceXml == null)
                throw new InvalidOperationException("View description xml bad or missing.");

            var newBinary = this.Binary ?? new BinaryData();
            var ascxStream = new MemoryStream();

            var xsltArgList = new System.Xml.Xsl.XsltArgumentList();

            AddXsltParameters(xsltArgList);

            using (var sw = new StreamWriter(ascxStream, Encoding.UTF8))
            {
                using (var writer = new HtmlTextWriter(sw))
                {
                    xsltTransform.Transform(viewSourceXml, xsltArgList, writer);
                    writer.Flush();
                    this.Binary.SetStream(ascxStream);
                    this.Binary = newBinary;
                    sw.Flush();
                    ascxStream.Flush();
                }
            }
        }

        public override void Save()
        {
            SetBinary();
            base.Save();
        }

        public override void Save(SavingMode mode)
        {
            SetBinary();
            base.Save(mode);
        }

        public override void SaveSameVersion()
        {
            SetBinary();
            base.SaveSameVersion();
        }

        protected override void OnModified(object sender, ContentRepository.Storage.Events.NodeEventArgs e)
        {
            base.OnModified(sender, e);

            var cdName = e.ChangedData.FirstOrDefault(cd => cd.Name == "Name");
            if (cdName != null)
            {
                var cl = ContentList.GetContentListByParentWalk(this);
                if (cl != null && !string.IsNullOrEmpty(cl.DefaultView) && cl.DefaultView.CompareTo(cdName.Original) == 0)
                {
                    cl.DefaultView = this.Name;
                    cl.Save(SavingMode.KeepVersion);
                }
            }
            
        }

        #region IView Members

        public virtual void AddColumn(Column col)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveColumn(string fullName)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<Column> GetColumns()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
