using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
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
    public abstract class FeedContent
    {
        public static readonly string XML_ELEMENTNAME_REGEX = @"^(?!(xml|[_\d\W]))[^ \s\W]+$";

        public Stream GetXml()
        {
            return GetXml(false);
        }
        [Obsolete("ActionLinkResolvers are deprecated, overloads containing it may be removed in the future.")]
        public Stream GetXml(object ActionLinkResolver, bool withChildren)
        {
            return GetXml(withChildren);
        }
        [Obsolete("ActionLinkResolvers are deprecated, overloads containing it may be removed in the future.")]
        public Stream GetXml(object ActionLinkResolver)
        {
            return GetXml(false);
        }

        public Stream GetXml(bool withChildren, SerializationOptions options = null)
        {
            return GetXml(withChildren, null, null, options);
        }
        public Stream GetXml(string queryFilter, QuerySettings querySettings, SerializationOptions options = null)
        {
            return GetXml(true, queryFilter, querySettings, options);
        }
        private Stream GetXml(bool withChildren, string queryFilter, QuerySettings querySettings, SerializationOptions options)
        {
            if (options == null)
                options = SerializationOptions.Default;

            var stream = new MemoryStream();
            var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true });
            writer.WriteStartDocument();

            //if a filter or query settings is present, we have to filter children
            if (!string.IsNullOrEmpty(queryFilter) || querySettings != null)
                WriteXml(writer, queryFilter, querySettings, options);
            else
                WriteXml(writer, withChildren, options);

            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        [Obsolete("ActionLinkResolvers are deprecated, overloads containing it may be removed in the future.")]
        public Stream GetXml(object ActionLinkResolver, string referenceMemberName)
        {
            return GetXml(referenceMemberName);
        }
        public Stream GetXml(string referenceMemberName, SerializationOptions options = null)
        {
            var stream = new MemoryStream();
            var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true });
            writer.WriteStartDocument();

            WriteXml(writer, referenceMemberName, options);

            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        protected void WriteXml(IEnumerable<Node> childNodes, XmlWriter writer, SerializationOptions options)
        {
            foreach (var content in (from node in childNodes select Content.Create(node)).ToList())
                content.WriteXml(writer, false, options);
        }
        protected void WriteXml(IEnumerable<Content> childNodes, XmlWriter writer, SerializationOptions options)
        {
            foreach (var content in childNodes)
                content.WriteXml(writer, false, options);
        }
        protected abstract void WriteXml(XmlWriter writer, bool withChildren, SerializationOptions options);
        protected abstract void WriteXml(XmlWriter writer, string queryFilter, QuerySettings querySettings, SerializationOptions options);
        protected abstract void WriteXml(XmlWriter writer, string referenceMemberName, SerializationOptions options);

        protected void WriteHead(XmlWriter writer, string contentTypeName, string contentName, string iconName, string path, bool isFolder)
        {
            WriteHead(writer, contentTypeName, contentTypeName, contentName, iconName, path, isFolder);
        }

        protected void WriteHead(XmlWriter writer, string contentTypeName, string contentTypeTitle, string contentName, string iconName, string path, bool isFolder)
        {
            var ct = string.IsNullOrEmpty(contentTypeName) ? null : ContentType.GetByName(contentTypeName);

            writer.WriteElementString("ContentType", contentTypeName);
            writer.WriteElementString("ContentTypePath", ct == null ? string.Empty : ct.Path);
            writer.WriteElementString("ContentTypeTitle", contentTypeTitle);
            writer.WriteElementString("ContentName", contentName);
            writer.WriteElementString("Icon", iconName);
            writer.WriteElementString("SelfLink", path);
            writer.WriteElementString("IsFolder", isFolder.ToString().ToLowerInvariant());
        }
        protected void WriteActions(XmlWriter writer, string path, IEnumerable<ActionBase> actions)
        {
            if (actions == null)
                return;
            if (actions.Count() == 0)
                return;

            writer.WriteStartElement("Actions");
            foreach (var action in actions)
            {
                try
                {
                    if (action.Active)
                    {
                        //if the name does not follow the standard xml element name rules, we must log this and skip it
                        if (!Regex.IsMatch(action.Name, XML_ELEMENTNAME_REGEX))
                        {
                            Logger.WriteWarning("Content actions serialization error: invalid xml element. Path: "+ path + ". Action name: " + action.Name);
                            continue;
                        }

                        if (action.IncludeBackUrl)
                        {
                            writer.WriteElementString(action.Name, action.Uri);
                        }
                        else
                        {
                            var actionUrl = action.Uri;
                            var urlSeparator = (actionUrl != null && actionUrl.Contains("?")) ? "&" : "?";
                            var back = string.Format("{0}{1}={2}", urlSeparator, ActionBase.BackUrlParameterName, action.BackUri);

                            writer.WriteStartElement(action.Name);
                            writer.WriteAttributeString(ActionBase.BackUrlParameterName, back);
                            writer.WriteString(actionUrl);
                            writer.WriteEndElement();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //log exception, but continue writing actions
                    Logger.WriteException(ex);

                    //no point in continuing the process if the writer is closed
                    if (writer.WriteState == WriteState.Error || writer.WriteState == WriteState.Closed)
                        break;
                }
            }

            writer.WriteEndElement();
        }
    }
}
