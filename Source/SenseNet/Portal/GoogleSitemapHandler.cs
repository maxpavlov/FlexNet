using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Serialization;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Virtualization;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Mail;

namespace SenseNet.Services
{
    public class GoogleSitemapHandler : IHttpHandler
    {

        #region Members
        #endregion

        #region Methods

        /// <summary>
        /// Returns actual URL of a Node using PortalContext.Current
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public string getPageUrl(Node node, string siteUrl)
        {
            string localPath = node.Path;
            string sitePath = PortalContext.Current.Site.Path;
            if (localPath.StartsWith(sitePath))
            {
                localPath = localPath.Remove(0, sitePath.Length);
            }
            return "http://" + siteUrl + localPath;
        }

        #endregion

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            // requested page path without the handled extension
            string requestPath = PortalContext.Current.RepositoryPath;
            string sitemapNodePath = requestPath.Substring(0, requestPath.LastIndexOf(".map"));
            requestPath = requestPath.Substring(0, requestPath.LastIndexOf('/'));

            NodeQuery query;
            bool listHidden = false;
            string siteUrl = "";


            // get sitemap configuration node at requested path
            Node sitemapNode = Node.LoadNode(sitemapNodePath);
            if (sitemapNode == null)
            {
                context.Response.StatusCode = 404;
                throw new HttpException(404, "Sitemap configuration node not found");
            }

            var propQuery = "Query";
            var propListHidden = "ListHidden";
            var propHidden = "Hidden";
            var propSiteUrl = "SiteUrl";
            
            // invalid sitemap configuration node
			if(!(sitemapNode.HasProperty(propQuery) && sitemapNode.HasProperty(propListHidden) && sitemapNode.HasProperty(propSiteUrl)))
            {
                context.Response.StatusCode = 400;
                throw new HttpException(400, "Invalid sitemap configuration node (Query, ListHidden or SiteUrl properties missing, update CTD)");
            }

			var queryString = sitemapNode.GetProperty<string>(propQuery);

            if ((queryString == null) || (queryString == ""))
            {
                // default query
                query = new NodeQuery();
                query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, requestPath));
                query.Add(new TypeExpression(ActiveSchema.NodeTypes["Page"]));
            }
            else
            {
                try
                {
                    query = NodeQuery.Parse(queryString);
                }
                catch (Exception) //rethrow
                {
                    context.Response.StatusCode = 400;
                    throw new HttpException(400, "Invalid query string");
                }
            }

			listHidden = sitemapNode.GetProperty<int>(propListHidden) == 1;
			siteUrl = sitemapNode.GetProperty<string>(propSiteUrl);
            if (String.IsNullOrEmpty(siteUrl))
				siteUrl = PortalContext.Current.SiteUrl;


            // query nodes of "Page" type under the requested page
            var pages = query.Execute().Nodes;
            Node[] goodPages;

            // exclude pages under hidden pages from list
            if (!listHidden)
            {
                string[] InvalidPaths =
					pages.Where(page => page.GetProperty<int>(propHidden) == 1).
                    Select(page => page.Path).ToArray();

                goodPages =
                     pages.Where(page => InvalidPaths.Where(ip => page.Path.StartsWith(ip)).Count() == 0).
                     Select(page => page).ToArray();
            }
            else
            {
                goodPages = pages.ToArray();
            }


            // set up object model for sitemap
            Urlset urlset = new Urlset();

            urlset.Urls =
                goodPages.Select(page => new Urlset.Url() { loc = getPageUrl(page, siteUrl) }).ToList();

            // output sitemap XML to http response
            context.Response.ContentType = "text/xml";

            XmlSerializer ser = new XmlSerializer(typeof(Urlset));
            ser.Serialize(context.Response.OutputStream, urlset);

            context.Response.OutputStream.Flush();
        }

        #endregion
    }

    #region Sitemap Object Model

    [XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9", IsNullable = false)]
    public class Urlset
    {
        [XmlAttribute("schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string _urlsetSchemaLocAttr = "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd";
        
        [XmlElement("url")]
        public List<Url> Urls { get; set; }


        public class Url
        {
            public string loc { get; set; }
            //public string lastmod { get; set; }
        }

        public Urlset() 
        { 
            Urls = new List<Urlset.Url>(); 
        }
    }

    #endregion

}
