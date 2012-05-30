using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SenseNet.Portal.UI.ContentListViews.Handlers
{
    [ContentHandler]
    public class ListView : ViewBase
    {
        public ListView(Node parent)
            : this(parent, null) 
		{
		}
		public ListView(Node parent, string nodeTypeName) : base(parent, nodeTypeName)
		{
		}
		protected ListView(NodeToken nt) : base(nt)
		{
        }

        #region ContentProperties

        [RepositoryProperty("Columns", RepositoryDataType.Text)]
        public string Columns
        {
            get { return GetProperty<string>("Columns"); }
            set { this["Columns"] = value; }
        }

        [RepositoryProperty("SortBy", RepositoryDataType.String)]
        public string SortBy
        {
            get { return GetProperty<string>("SortBy"); }
            set { this["SortBy"] = value; }
        }

        [RepositoryProperty("GroupBy", RepositoryDataType.String)]
        public string GroupBy
        {
            get { return GetProperty<string>("GroupBy"); }
            set { this["GroupBy"] = value; }
        }

        [RepositoryProperty("Flat", RepositoryDataType.Int)]
        public bool Flat
        {
            get { return GetProperty<int>("Flat") != 0; }
            set { this["Flat"] = value ? 1 : 0; }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "Columns":
                    return this.Columns;
                case "SortBy":
                    return this.SortBy;
                case "GroupBy":
                    return this.GroupBy;
                case "Flat":
                    return this.Flat;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "Columns":
                    this.Columns = (string)value;
                    break;
                case "SortBy":
                    this.SortBy = (string)value;
                    break;
                case "GroupBy":
                    this.GroupBy = (string)value;
                    break;
                case "Flat":
                    this.Flat = (bool)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        #endregion

        #region column_decode

        private static XPathDocument PrepareNavigableColumnXml(string cols)
        {
            return new XPathDocument(new StringReader(cols));
        }

        private static XPathNodeIterator PrepareColumnIterator(string cols)
        {
            XPathDocument doc = PrepareNavigableColumnXml(cols);
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator iter = nav.Select("/Columns/Column");
            return iter;
        }

        public static IEnumerable<Column> PrepareColumnList(string cols)
        {
            if (string.IsNullOrEmpty(cols))
                return null;

            var columns = new List<Column>();
            var xs = new XmlSerializer(typeof(Column));
            var iter = PrepareColumnIterator(cols);

            foreach (XPathNavigator current in iter)
            {
                var xr = new XmlTextReader(current.OuterXml, XmlNodeType.Element, new XmlParserContext(null, null, "", XmlSpace.Default));
                var c = xs.Deserialize(xr) as Column;

                xr.Close();

                if (c != null)
                    columns.Add(c);
            }

            return columns;
        }

        public IEnumerable<string> GetColumnList()
        {
            var fields = new List<string>();

            var iter = PrepareColumnIterator(Columns);

            foreach (XPathNavigator current in iter)
            {
                fields.Add(current.GetAttribute("fullName", ""));
            }

            return fields;
        }

        #endregion

        #region column_encode

        public static IXPathNavigable CreateColumnXml(IEnumerable<Column> cols)
        {
            var doc = new XmlDocument();
            var sortedCols = cols.ToList();
            sortedCols.Sort(CompareColumnsByIndex);

            //set indexes to 1,2,3,... (remove gaps)
            var ind = 1;
            foreach (var col in sortedCols)
            {
                col.Index = ind++;
            }

            var xs = new XmlSerializer(typeof(Column));
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);

            xw.WriteStartDocument();
            xw.WriteStartElement("Columns");

            foreach (var col in sortedCols)
            {
                xs.Serialize(xw, col, ns);
            }

            xw.WriteEndElement();

            doc.LoadXml(sw.ToString());
            xw.Close();

            return doc;
        }

        private static int CompareColumnsByIndex(Column x, Column y)
        {
            return x.Index.CompareTo(y.Index);
        }

        public static string SerializeColumnXml(IEnumerable<Column> cols)
        {
            IXPathNavigable doc = CreateColumnXml(cols);
            XPathNavigator nav = doc.CreateNavigator();

            return nav.OuterXml;
        }

        public void SetColumns(IEnumerable<Column> cols)
        {
            Columns = SerializeColumnXml(cols);
        }

        #endregion

        #region ViewBase_members

        protected override IXPathNavigable GetSource()
        {
            return PrepareNavigableColumnXml(Columns);
        }

        protected override void AddXsltParameters(System.Xml.Xsl.XsltArgumentList argumentList)
        {
            base.AddXsltParameters(argumentList);

            argumentList.AddParam("groupBy", "", this.GroupBy ?? string.Empty);
        }

        #endregion

        #region IView Members

        public override void AddColumn(Column col)
        {
            var cols = PrepareColumnList(Columns).ToList();
            cols.Add(col);
            Columns = SerializeColumnXml(cols);
        }

        public override void RemoveColumn(string fullName)
        {
            var cols = PrepareColumnList(Columns);
            var newcols = from Column c in cols
                          where (c.FullName != fullName)
                          select c;
            Columns = SerializeColumnXml(newcols);
        }

        public override IEnumerable<Column> GetColumns()
        {
            return PrepareColumnList(Columns);
        }

        #endregion
    }
}
