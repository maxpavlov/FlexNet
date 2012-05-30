using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using System.Linq;
using System.IO;
using SenseNet.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using SenseNet.ContentRepository.Fields;
using System.Xml.XPath;
using System.Web.Configuration;
using SenseNet.ApplicationModel;
using SenseNet.Search;

namespace SenseNet.ContentRepository
{
    public class SearchFolder : FeedContent
    {
        private NodeQuery _query;
        private ContentQuery _contentQuery;

        public IEnumerable<Node> Children { get; private set; }

        private SearchFolder() { }

        [Obsolete("Use Create(ContentQuery query) instead.")]
        public static SearchFolder Create(NodeQuery query)
        {
            var folder = new SearchFolder
            {
                _query = query,
                Children = query.Execute().Nodes
            };
            return folder;
        }

        public static SearchFolder Create(ContentQuery query)
        {
            var folder = new SearchFolder
            {
                _contentQuery = query,
                Children = query.Execute().Nodes.ToArray()
            };
            return folder;
        }

        public static SearchFolder Create(IEnumerable<Node> nodes)
        {
            return new SearchFolder { Children = nodes };
        }

        protected override void WriteXml(XmlWriter writer, bool withChildren)
        {
            const string thisName = "SearchFolder";
            const string thisPath = "/Root/SearchFolder";

            writer.WriteStartElement("Content");
            base.WriteHead(writer, thisName, thisName, thisName, thisPath, true);

            if (_query != null)
            {
                writer.WriteStartElement("Query");
                writer.WriteRaw(_query.ToXml());
                writer.WriteEndElement();
            }

            if (withChildren && Children != null)
            {
                writer.WriteStartElement("Children");
                this.WriteXml(Children, writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        protected override void WriteXml(XmlWriter writer, string queryFilter, QuerySettings querySettings)
        {
            WriteXml(writer, false);
        }

        protected override void WriteXml(XmlWriter writer, string referenceMemberName)
        {
            WriteXml(writer, false);
        }
    }

}
