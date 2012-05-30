using System;
using System.Collections.Generic;
using System.Text;
using SenseNet.ContentRepository.Storage;
using  SenseNet.ContentRepository.Schema;
using SenseNet.Portal.Virtualization;
using System.Xml;
using System.Linq;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Workspaces;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal
{
    /// <summary>
    ///     <para>Represents a web site in the Sense/Net Portal.</para>
    /// </summary>
    /// <remarks>
    ///     <para>In the ECMS (Enterprise Content Management) systems all the data and all the objects are handled as contents. Everything (like the web contents, web pages, but the portal users, your business data as well) is an enterprise content, and can be stored in the Sense/Net Content Repository. The Site class represents a web site that is stored in the content repository.</para>
    /// </remarks>
	[ContentHandler]
    public class Site : Workspace
    {
        #region class UrlDictionary
        //public class UrlDictionary : IDictionary<string, string>
        //{
        //    private Dictionary<string, string> _urls = new Dictionary<string, string>();

        //    public event EventHandler Changed;

        //    private void FireChanged()
        //    {
        //        if (Changed != null)
        //            Changed(this, EventArgs.Empty);
        //    }

        //    //======================================================================= IDictionary<string,string> Members

        //    public void Add(string key, string value)
        //    {
        //        _urls.Add(key, value);
        //        FireChanged();
        //    }
        //    public bool ContainsKey(string key)
        //    {
        //        return _urls.ContainsKey(key);
        //    }
        //    public ICollection<string> Keys
        //    {
        //        get { return _urls.Keys; }
        //    }
        //    public bool Remove(string key)
        //    {
        //        var b = _urls.Remove(key);
        //        FireChanged();
        //        return b;
        //    }
        //    public bool TryGetValue(string key, out string value)
        //    {
        //        return _urls.TryGetValue(key, out value);
        //    }
        //    public ICollection<string> Values
        //    {
        //        get { return _urls.Values; }
        //    }
        //    public string this[string key]
        //    {
        //        get
        //        {
        //            return _urls[key];
        //        }
        //        set
        //        {
        //            _urls[key] = value;
        //            FireChanged();
        //        }
        //    }

        //    //======================================================================= ICollection<KeyValuePair<string,string>> Members

        //    public void Add(KeyValuePair<string, string> item)
        //    {
        //        ((ICollection<KeyValuePair<string, string>>)_urls).Add(item);
        //        FireChanged();
        //    }
        //    public void Clear()
        //    {
        //        _urls.Clear();
        //        FireChanged();
        //    }
        //    public bool Contains(KeyValuePair<string, string> item)
        //    {
        //        return ((ICollection<KeyValuePair<string, string>>)_urls).Contains(item);
        //    }
        //    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        //    {
        //        ((ICollection<KeyValuePair<string, string>>)_urls).CopyTo(array, arrayIndex);
        //    }
        //    public int Count
        //    {
        //        get { return _urls.Count; }
        //    }
        //    public bool IsReadOnly
        //    {
        //        get { return ((ICollection<KeyValuePair<string, string>>)_urls).IsReadOnly; }
        //    }
        //    public bool Remove(KeyValuePair<string, string> item)
        //    {
        //        var b = ((ICollection<KeyValuePair<string, string>>)_urls).Remove(item);
        //        FireChanged();
        //        return b;
        //    }

        //    //======================================================================= IEnumerable<KeyValuePair<string,string>> Members

        //    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        //    {
        //        return ((ICollection<KeyValuePair<string, string>>)_urls).GetEnumerator();
        //    }

        //    //======================================================================= IEnumerable Members

        //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //    {
        //        return ((System.Collections.IEnumerable)_urls).GetEnumerator();
        //    }
        //}
        #endregion

        [Obsolete("Use typeof(Site).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(Site).Name;

		private IDictionary<string, string> _urlList;

        public Site(Node parent) : this(parent, null) { }
		public Site(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected Site(NodeToken nt) : base(nt) { }

		[RepositoryProperty("Description", RepositoryDataType.Text)]
		public string Description
		{
			get { return this.GetProperty<string>("Description"); }
			set { this["Description"] = value; }
		}
		[RepositoryProperty("PendingUserLang")]
		public string PendingUserLang
        {
			get { return this.GetProperty<string>("PendingUserLang"); }
			set { this["PendingUserLang"] = value; }
		}
		[RepositoryProperty("Language")]
		public string Language
        {
			get { return this.GetProperty<string>("Language"); }
			set { this["Language"] = value; }
		}

		[RepositoryProperty("UrlList", RepositoryDataType.Text)]
		public IDictionary<string, string> UrlList
		{
		    get { return _urlList ?? (_urlList = ParseUrlList(this.GetProperty<string>("UrlList"))); }
		    set
		    {
                this["UrlList"] = UrlListToString(value);
                _urlList = null;
            }
		}

		[RepositoryProperty("StartPage", RepositoryDataType.Reference)]
		public Page StartPage
        {
			get { return this.GetReference<Page>("StartPage"); }
			set { this.SetReference("StartPage", value); }
		}
		[RepositoryProperty("LoginPage", RepositoryDataType.Reference)]
		public Page LoginPage
        {
			get { return this.GetReference<Page>("LoginPage"); }
			set { this.SetReference("LoginPage", value); }
        }

        [RepositoryProperty("SiteSkin", RepositoryDataType.Reference)]
        public Node SiteSkin
        {
            get { return this.GetReference<Node>("SiteSkin"); }
            set { this.SetReference("SiteSkin", value); }
        }

        private const string DENYCROSSSITEACCESSPROPERTY = "DenyCrossSiteAccess";
        [RepositoryProperty(DENYCROSSSITEACCESSPROPERTY, RepositoryDataType.Int)]
        public bool DenyCrossSiteAccess
        {
            get { return base.GetProperty<int>(DENYCROSSSITEACCESSPROPERTY) != 0; }
            set { base.SetProperty(DENYCROSSSITEACCESSPROPERTY, value ? 1 : 0); }
        }

        //[RepositoryProperty("StatisticalLog", RepositoryDataType.Int)]
        //public bool StatisticalLog
        //{
        //    get { return this.GetProperty<int>("StatisticalLog") != 0; }
        //    set { this["StatisticalLog"] = value ? 1 : 0; }
        //}
        //[RepositoryProperty("AuditLog", RepositoryDataType.Int)]
        //public bool AuditLog
        //{
        //    get { return this.GetProperty<int>("AuditLog") != 0; }
        //    set { this["AuditLog"] = value ? 1 : 0; }
        //}

        public new static Site Current
        {
            get { return PortalContext.Current.Site; }
        }

		//////////////////////////////////////// Methods //////////////////////////////////////////////

        /// <summary>
        /// <para>Saves the Site instance.</para>
        /// <para>This is an overriden method, which calls its base implementation first to persist the <c>Site</c> instance, and then rebuilds the site list and sends an application message to the other applications forcing them to do that too.</para>
        /// </summary>
		public override void Save()
		{
            RefreshUrlList();

            if (this.CopyInProgress)
            {
                //we need to reset these values to avoid 
                //conflict with the source site
                this.UrlList = new Dictionary<string, string>();
                this.StartPage = null;
            }
            else
            {
                ValidateUrlList();
            }

			base.Save();
            
            var action = new PortalContext.ReloadSiteListDistributedAction();
            action.Execute();
		}

        public override void Save(SavingMode mode)
        {
            RefreshUrlList();

            if (this.CopyInProgress)
            {
                //we need to reset these values to avoid 
                //conflict with the source site
                this.UrlList = new Dictionary<string, string>();
                this.StartPage = null;
            }
            else
            {
                ValidateUrlList();
            }

            base.Save(mode);

            var action = new PortalContext.ReloadSiteListDistributedAction();
            action.Execute();
        }

        public override void Delete()
        {
            base.Delete();

            var action = new PortalContext.ReloadSiteListDistributedAction();
            action.Execute();
        }

        public override void ForceDelete()
        {
            base.ForceDelete();

            var action = new PortalContext.ReloadSiteListDistributedAction();
            action.Execute();
        }

        private void RefreshUrlList()
        {
            var originalUrls = this.GetProperty<string>("UrlList");
            var currentUrls = UrlListToString(this.UrlList);
            if (originalUrls != currentUrls)
                this["UrlList"] = currentUrls;
        }

        private void ValidateUrlList()
        {
            //if another site already uses one of our urls, throw an exception
            foreach (var url in UrlList.Keys.Where(url => PortalContext.Sites.Keys.Count(k => k == url && PortalContext.Sites[k].Id != this.Id) > 0))
            {
                throw new ApplicationException(string.Format("The url {0} is already used by the site {1}", url, PortalContext.Sites[url].DisplayName));
            }
        }

        public static Site GetSiteByNode(Node source)
		{
            return GetSiteByNodePath(source.Path);
		}

        public static Site GetSiteByNodePath(string path)
        {
            return PortalContext.GetSiteByNodePath(path);
        }

		public string GetAuthenticationType(Uri uri)
		{
			string url = uri.GetComponents(UriComponents.HostAndPort | UriComponents.Path, UriFormat.Unescaped);
			foreach (string siteUrl in UrlList.Keys)
			{
				if (url.StartsWith(siteUrl))
					return UrlList[siteUrl];
			}
			return null;
		}
		public static IDictionary<string, string> ParseUrlList(string urlSrc)
		{
			//-- For exmple:
			//   <Url authType="Forms">localhost:1315/</Url>
			//   <Url authType="Windows">name.server.xy</Url>

            var urlList = new Dictionary<string, string>();
			if (String.IsNullOrEmpty(urlSrc))
				return urlList;

			var doc = new XmlDocument();
			doc.LoadXml(String.Concat("<root>", urlSrc, "</root>"));
			foreach (XmlNode node in doc.SelectNodes("//Url"))
			{
				var attr = node.Attributes["authType"];
				string authType = attr == null ? "" : attr.Value;
                var url = node.InnerText.Trim();

                if (!string.IsNullOrEmpty(url) && !urlList.ContainsKey(url))
                    urlList.Add(url, authType);
			}
			return urlList;
		}
		public static string UrlListToString(IDictionary<string, string> urlList)
		{
            if (urlList == null)
                throw new ApplicationException("Please give at least one site url.");

			var sb = new StringBuilder();
			foreach (string key in urlList.Keys)
			{
				string auth = urlList[key];
				sb.Append("<Url");
				if(!String.IsNullOrEmpty(auth))
					sb.Append(" authType='").Append(auth).Append("'");
				sb.Append(">");
				sb.Append(key);
				sb.Append("</Url>");
			}
			return sb.ToString();
		}

		public static string GetUrlByRepositoryPath(string url, string repositoryPath)
		{
            return PortalContext.GetUrlByRepositoryPath(url, repositoryPath);
		}

        public static IEnumerable<ChoiceOption> GetAllLanguages()
        {
            var languageFieldSetting = ContentType.GetByName("Site").GetFieldSettingByName("Language") as ChoiceFieldSetting;
            return languageFieldSetting.Options;
        }

		//================================================================================= Generic Property handling

		public override object GetProperty(string name)
		{
			switch (name)
			{
				case "Description":
					return this.Description;
				case "PendingUserLang":
					return this.PendingUserLang;
				case "Language":
					return this.Language;
				case "UrlList":
					return this.UrlList;
				case "StartPage":
					return this.StartPage;
				case "LoginPage":
					return this.LoginPage;
                case "SiteSkin":
                    return this.SiteSkin;
                case DENYCROSSSITEACCESSPROPERTY:
                    return this.DenyCrossSiteAccess;
                //case "StatisticalLog":
                //    return this.StatisticalLog;
                //case "AuditLog":
                //    return this.AuditLog;
				default:
					return base.GetProperty(name);
			}
		}
		public override void SetProperty(string name, object value)
		{
			switch (name)
			{
				case "Description":
					this.Description = (string)value;
					break;
				case "PendingUserLang":
					this.PendingUserLang = (string)value;
					break;
				case "Language":
					this.Language = (string)value;
					break;
				case "UrlList":
					this.UrlList = (Dictionary<string, string>)value;
					break;
				case "StartPage":
					this.StartPage = (Page)value;
					break;
				case "LoginPage":
					this.LoginPage = (Page)value;
					break;
                case "SiteSkin":
                    this.SiteSkin = (Node)value;
                    break;
                case DENYCROSSSITEACCESSPROPERTY:
                    this.DenyCrossSiteAccess = (bool)value;
                    break;
                //case "StatisticalLog":
                //    this.StatisticalLog = (bool)value;
                //    break;
                //case "AuditLog":
                //    this.AuditLog = (bool)value;
                //    break;
				default:
					base.SetProperty(name, value);
					break;
			}
		}

		public bool IsRequested(Uri uri)
		{
			string url = uri.GetComponents(UriComponents.HostAndPort | UriComponents.Path, UriFormat.Unescaped);
			foreach (string key in UrlList.Keys)
				if (url.StartsWith(key))
					return true;
			return false;
		}

	}
}