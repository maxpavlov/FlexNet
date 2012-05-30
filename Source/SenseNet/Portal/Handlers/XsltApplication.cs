using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Handlers
{
    [ContentHandler]
    public class XsltApplication : Application, IFile, IHttpHandler
    {
        // ================================================================ Required construction
        public XsltApplication(Node parent) : this(parent, null) 
        { 
        }
        public XsltApplication(Node parent, string nodeTypeName) : base(parent, nodeTypeName)
        {
        }
        protected XsltApplication(NodeToken nt) : base(nt)
        {
        }


        // ================================================================ Properties
        [RepositoryProperty("Cacheable", RepositoryDataType.Int)]
        public virtual bool Cacheable
        {
            get { return (this.GetProperty<int>("Cacheable") != 0); }
            set { this["Cacheable"] = value ? 1 : 0; }
        }

        private const string CACHEABLEFORLOGGEDINUSER = "CacheableForLoggedInUser";
        [RepositoryProperty(CACHEABLEFORLOGGEDINUSER, RepositoryDataType.Int)]
        public virtual bool CacheableForLoggedInUser
        {
            get { return (this.GetProperty<int>(CACHEABLEFORLOGGEDINUSER) != 0); }
            set { this[CACHEABLEFORLOGGEDINUSER] = value ? 1 : 0; }
        }

        [RepositoryProperty("CacheByPath", RepositoryDataType.Int)]
        public virtual bool CacheByPath
        {
            get { return (this.GetProperty<int>("CacheByPath") != 0); }
            set { this["CacheByPath"] = value ? 1 : 0; }
        }

        [RepositoryProperty("CacheByParams", RepositoryDataType.Int)]
        public virtual bool CacheByParams
        {
            get { return (this.GetProperty<int>("CacheByParams") != 0); }
            set { this["CacheByParams"] = value ? 1 : 0; }
        }

        [RepositoryProperty("SlidingExpirationMinutes", RepositoryDataType.Int)]
        public virtual int SlidingExpirationMinutes
        {
            get { return this.GetProperty<int>("SlidingExpirationMinutes"); }
            set { this["SlidingExpirationMinutes"] = value; }
        }

        [RepositoryProperty("AbsoluteExpiration", RepositoryDataType.Int)]
        public virtual int AbsoluteExpiration
        {
            get { return this.GetProperty<int>("AbsoluteExpiration"); }
            set { this["AbsoluteExpiration"] = value; }
        }

        [RepositoryProperty("CustomCacheKey", RepositoryDataType.String)]
        public virtual string CustomCacheKey
        {
            get { return this.GetProperty<string>("CustomCacheKey"); }
            set { this["CustomCacheKey"] = value; }
        }

        [RepositoryProperty("MimeType", RepositoryDataType.String)]
        public virtual string MimeType
        {
            get { return this.GetProperty<string>("MimeType"); }
            set { this["MimeType"] = value; }
        }

        [RepositoryProperty("OmitXmlDeclaration", RepositoryDataType.Int)]
        public virtual bool OmitXmlDeclaration
        {
            get { return (this.GetProperty<int>("OmitXmlDeclaration") != 0); }
            set { this["OmitXmlDeclaration"] = value ? 1 : 0; }
        }

        [RepositoryProperty("ResponseEncoding", RepositoryDataType.String)]
        public virtual string ResponseEncoding
        {
            get { return this.GetProperty<string>("ResponseEncoding"); }
            set { this["ResponseEncoding"] = value; }
        }

        [RepositoryProperty("WithChildren", RepositoryDataType.Int)]
        public virtual bool WithChildren
        {
            get { return (this.GetProperty<int>("WithChildren") != 0); }
            set { this["WithChildren"] = value ? 1 : 0; }
        }

        [RepositoryProperty("Binary", RepositoryDataType.Binary)]
        public virtual BinaryData Binary
        {
            get { return this.GetBinary("Binary"); }
            set { this.SetBinary("Binary", value); }
        }


        // ================================================================ GetProperty - SetProperty
        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "AbsoluteExpiration":
                    return this.AbsoluteExpiration;
                case "Binary":
                    return this.Binary;
                case "CacheByPath":
                    return this.CacheByPath;
                case "CustomCacheKey":
                    return this.CustomCacheKey;
                case "CacheByParams":
                    return this.CacheByParams;
                case "Cacheable":
                    return this.Cacheable;
                case CACHEABLEFORLOGGEDINUSER:
                    return this.CacheableForLoggedInUser;
                case "ContentType":
                    return this.ContentType;
                case "OmitXmlDeclaration":
                    return this.OmitXmlDeclaration;
                case "ResponseEncoding":
                    return this.ResponseEncoding;
                case "SlidingExpirationMinutes":
                    return this.SlidingExpirationMinutes;
                case "WithChildren":
                    return this.WithChildren;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "AbsoluteExpiration":
                    this.AbsoluteExpiration = (int)value;
                    break;
                case "Binary":
                    this.Binary = (BinaryData)value;
                    break;
                case "CacheByParams":
                    this.CacheByParams = (bool)value;
                    break;
                case "CacheByPath":
                    this.CacheByPath = (bool)value;
                    break;
                case "CustomCacheKey":
                    this.CustomCacheKey = value.ToString();
                    break;
                case "Cacheable":
                    this.Cacheable = (bool)value;
                    break;
                case CACHEABLEFORLOGGEDINUSER:
                    this.CacheableForLoggedInUser = (bool)value;
                    break;
                case "MimeType":
                    this.MimeType = value.ToString();
                    break;
                case "OmitXmlDeclaration":
                    this.OmitXmlDeclaration = (bool)value;
                    break;
                case "ResponseEncoding":
                    this.ResponseEncoding = value.ToString();
                    break;
                case "SlidingExpirationMinutes":
                    this.SlidingExpirationMinutes = (int)value;
                    break;
                case "WithChildren":
                    this.WithChildren = (bool)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        
        // ================================================================ IFile members
        public long Size
        {
            get { return Binary.Size; }
        }
        public long FullSize
        {
            get { return Binary.Size; }
        }


        // ================================================================ IHttpHandler members
        public bool IsReusable
        {
            get { return false; }
        }
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Clear();
            context.Response.ContentType = string.IsNullOrEmpty(MimeType) ? "application/xml" : MimeType;
            context.Response.ContentEncoding = GetEncoding();

            var withChildrenParam = context.Request.Params["withchildren"];
            bool withChildren = string.IsNullOrEmpty(withChildrenParam)
                                    ? this.WithChildren
                                    : withChildrenParam.ToLower() == "true";



            if (!CanCache || !Cacheable)
            {
                //render
                Render(context.Response.Output, withChildren);
            }
            else if (IsInCache)
            {
                context.Response.Write(GetCachedOutput());
            }
            else
            {
                using (var sw = new StringWriter())
                {
                    Render(sw, withChildren);
                    var output = sw.ToString();
                    InsertOutputIntoCache(output);
                    context.Response.Write(output);
                }
            }


            context.Response.End();
        }


        // ================================================================ Cache
        protected virtual bool CanCache
        {
            get { return SenseNet.Portal.UI.OutputCache.CanCache(this.CacheableForLoggedInUser); }
        }
        protected virtual bool IsInCache
        {
            get { return !string.IsNullOrEmpty(GetCachedOutput()); }
        }
        /// <summary>
        /// Specifies cache key of the application
        /// </summary>
        /// <returns>Returns a cache key</returns>
        protected virtual string GetCacheKey()
        {
            var path = PortalContext.Current.ContextNodePath;
            return SenseNet.Portal.UI.OutputCache.GetCacheKey(this.CustomCacheKey, path, null, this.CacheByPath, this.CacheByParams);
        }
        /// <summary>
        /// Retrieves the cached item by cache key
        /// </summary>
        /// <returns>The retrieved cache item, or null if the key is not found.</returns>
        protected virtual string GetCachedOutput()
        {
            return SenseNet.Portal.UI.OutputCache.GetCachedOutput(this.GetCacheKey());
        }
        protected virtual void InsertOutputIntoCache(string output)
        {
            var dep = new AggregateCacheDependency();
            dep.Add(new NodeIdDependency(this.Id)); //Add application Id
            dep.Add(new NodeIdDependency(PortalContext.Current.ContextNodeHead.Id)); //Add contextNode
            if (WithChildren)
            {
                foreach (var node in PortalContext.Current.ContextNode.PhysicalChildArray)
                {
                    dep.Add(new NodeIdDependency(node.Id));
                }
            }

            SenseNet.Portal.UI.OutputCache.InsertOutputIntoCache(AbsoluteExpiration, SlidingExpirationMinutes, this.GetCacheKey(), output, dep, CacheItemPriority.Normal);
        }


        // ================================================================ Misc
        protected virtual Encoding GetEncoding()
        {
            Encoding encoding = Encoding.UTF8;
            if (string.IsNullOrEmpty(this.ResponseEncoding))
                return encoding;

            try
            {
                encoding = Encoding.GetEncoding(this.ResponseEncoding);
            }
            catch (ArgumentException ex)
            {
                encoding = Encoding.UTF8;
                Logger.WriteException(ex);
            }
            return encoding;
        }
        protected virtual XsltArgumentList GetXsltArgumentList()
        {
            var arguments = new XsltArgumentList();
            arguments.AddExtensionObject("sn://SenseNet.Portal.UI.ContentTools", new ContentTools());
            arguments.AddExtensionObject("sn://SenseNet.Portal.UI.XmlFormatTools", new XmlFormatTools());
            return arguments;
        }
        protected virtual void Render(TextWriter outputFileName, bool withChildren)
        {
            var writer = XmlWriter.Create(outputFileName, new XmlWriterSettings
            {
                Indent = true,
                Encoding = GetEncoding(),
                OmitXmlDeclaration = this.OmitXmlDeclaration
            });

            var content = Content.Create(PortalContext.Current.ContextNode);
            var xml = new XPathDocument(content.GetXml(withChildren));
            var xslt = Xslt.GetXslt(this.Path, true);

            var xsltArguments = GetXsltArgumentList();

            xslt.Transform(xml, xsltArguments, writer);


            writer.Close();

        }
    }
}