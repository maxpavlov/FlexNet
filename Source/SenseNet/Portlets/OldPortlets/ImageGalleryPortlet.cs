using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage;
using System.Web;
using SenseNet.ContentRepository.Storage.Search;
using System.Web.UI.WebControls;
using System.Linq;
using SenseNet.Portal.Portlets.ContentHandlers;
using SenseNet.Portal.UI.PortletFramework;
using System.Security;
using SenseNet.Portal.UI;
using SenseNet.ContentRepository;
using System.IO;
using SenseNet.Diagnostics;


namespace SenseNet.Portal.Portlets
{
    public class ImageGalleryPortlet : CacheablePortlet
	{
        // Members /////////////////////////////////////////////////////////////////

		string _imageGalleryPath = string.Empty;
		string _javaScriptPath = "/Root/System/SystemPlugins/Portlets/ImageGallery/slide.js";
		string _cssPath = "/Root/System/SystemPlugins/Portlets/ImageGallery/style.css";
		bool _slideShow = false;
		int _slideShowDelay = 5;
        private string _urlQueryKey = "Gallery";
        private bool _useUrlPath = true;

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Use URL ContentPath")]
        [WebDescription("Indicates that portlet loads Content from the proper part of the URL.")]
        [WebCategory(EditorCategory.ImageGallery, EditorCategory.ImageGallery_Order)]
        public bool UseUrlPath
        {
            get { return _useUrlPath; }
            set { _useUrlPath = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("URL query key")]
        [WebDescription("Sets the parameter to use when loading contents from URL")]
        [WebCategory(EditorCategory.ImageGallery, EditorCategory.ImageGallery_Order)]
        public string UrlQueryKey
        {
            get { return _urlQueryKey; }
            set { _urlQueryKey = value; }
        }

		private string ImageGalleryPathQuery
		{
			get 
            {
                if (UseUrlPath)
                {
                    string urlQuery = HttpContext.Current.Request.Url.Query;
                    if (HttpContext.Current.Request.Params[UrlQueryKey] != null)
                    {
                        return(HttpContext.Current.Request.Params[UrlQueryKey].ToString());
                    }
                    else if (urlQuery.StartsWith("?"))
                        return(urlQuery.Replace("?", "").Replace("%20", " "));
                }
                return HttpContext.Current.Request.Params["ImageGalleryPath"];
            }
		}

		[WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("ImageGallery Path")]
        [WebDescription("The path of the ImageGallery displayed")]
        [WebCategory(EditorCategory.ImageGallery, EditorCategory.ImageGallery_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions()]
        public string ImageGalleryPath
		{
			get { return _imageGalleryPath; }
			set { _imageGalleryPath = value; }
		}

		[WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Javascript path")]
        [WebDescription("The path of the javascript used within the portlet")]
        [WebCategory(EditorCategory.ImageGallery, EditorCategory.ImageGallery_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Js)]
        public string JavaScriptPath
		{
			get { return _javaScriptPath; }
			set { _javaScriptPath = value; }
		}

		[WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Css path")]
        [WebDescription("The path of the CSS file used by the portlet")]
        [WebCategory(EditorCategory.ImageGallery, EditorCategory.ImageGallery_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Css)]
        public string CssPath
		{
			get { return _cssPath; }
			set { _cssPath = value; }
		}

		[WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Is slideshow")]
        [WebDescription("Wether the gallery should be displayed as a slideshow")]
        [WebCategory(EditorCategory.ImageGallery, EditorCategory.ImageGallery_Order)]
        public bool SlideShow
		{
			get { return _slideShow; }
			set { _slideShow = value; }
		}

		[WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Delay of slideshow")]
        [WebDescription("The number of seconds between slideshow changes")]
        [WebCategory(EditorCategory.ImageGallery, EditorCategory.ImageGallery_Order)]
        public int SlideShowDelay
		{
			get { return _slideShowDelay; }
			set { _slideShowDelay = value; }
		}

		//[WebBrowsable(true), Personalizable(true)]
		//public string ImageQuery
		//{
		//    get { return _imageQuery; }
		//    set { _imageQuery = value; }
		//}

        //-- Constructor -------------------------------------------------
        public ImageGalleryPortlet()
        {
            this.Name = "Image gallery";
            this.Description = "Show pictures of an Image Gallery list.";
            this.Category = new PortletCategory(PortletCategoryType.Application);
        }
        
        // Events /////////////////////////////////////////////////////////////////

        protected override void OnLoad(EventArgs e)
        {
            RegisterJS();
            base.OnLoad(e);
        }

        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            Controls.Clear();
            CreateControls();
			if (!UseUrlPath && ImageGalleryPath == String.Empty)
            {
                Label l = new Label();
                l.Text = "Please set the Image Gallery Path in the porlet properties";
                Controls.Add(l);
            }
            ChildControlsCreated = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!CanCache)
            {
                WebPartManager wpm = WebPartManager.GetCurrentWebPartManager(this.Page);
                if (wpm.DisplayMode == WebPartManager.EditDisplayMode ||
                    wpm.DisplayMode == WebPartManager.CatalogDisplayMode)
                    CreateChildControls();
            }
            base.OnPreRender(e);
        }

        // Internals /////////////////////////////////////////////////////////////

		private static string GenerateUrl(string nodePath)
		{
			if (SenseNet.Portal.Page.Current != null && SenseNet.Portal.Page.Current.Site != null && nodePath.IndexOf(SenseNet.Portal.Page.Current.Site.Path) > -1)
			{
				return string.Concat(
					SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.Scheme,
					"://",
					GetUrl(),
					nodePath.Substring(SenseNet.Portal.Page.Current.Site.Path.Length));
			}
			return nodePath;
		}

		private static string GetUrl()
		{
			foreach (string url in SenseNet.Portal.Page.Current.Site.UrlList.Keys)
			{
				if (SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.ToString().IndexOf(string.Concat(SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.Scheme, "://", url, "/")) == 0)
				{
					return url;
				}
			}
			return null;
		}

		private static string GetApplicationPath()
		{
			string appPath = HttpContext.Current.Request.ApplicationPath;
			return appPath == "/" ? string.Empty : appPath;
		}

        private void RegisterJS()
        {
            if (!string.IsNullOrEmpty(JavaScriptPath) && !string.IsNullOrEmpty(ImageGalleryPath))
            {
                UITools.AddScript(JavaScriptPath);
                UITools.RegisterStartupScript("startGallery", @"slideShow.init(); slideShow.lim();", Page);
            }
        }

        private void CreateControls()
        {
            ImageGallery ig = null;

            if (!string.IsNullOrEmpty(ImageGalleryPathQuery))
            {
                ig = Node.LoadNode(ImageGalleryPathQuery) as ImageGallery;
            }
            else if (!string.IsNullOrEmpty(ImageGalleryPath))
            {
                ig = Node.LoadNode(ImageGalleryPath) as ImageGallery;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(@"
<script type='text/javascript'>
var snGalleryImageId = 'sn-gallery-img';
var snGalleryThumbId = 'snGalleryThumbs';
var snGalleryImageArray = new Array();
var snGalleryTitleArray = new Array();
var snGalleryDescArray = new Array();");

            //
            if (SlideShow)
            {
                sb.Append("var auto = true; var autodelay = ").Append(SlideShowDelay).Append(";");
            }
            else
            {
                sb.Append("var auto = false;");
            }

            IList<SenseNet.ContentRepository.Image> imageList = null;
            string titleId = string.Empty;
            if (ig != null)
            {
                titleId = ig.GetPropertySingleId("#DisplayName");
                string descId = ig.GetPropertySingleId("#Description2");

                imageList = ig.GetImages();

                for (int j = 1; j <= imageList.Count; j++)
                {
                    sb.Append(string.Concat("snGalleryImageArray[", j, "] = '", imageList[j - 1].Path, "';"));

                    // HACK! Refactor later.
                    var a = imageList[j - 1][titleId];
                    var astr = string.Empty;
                    if (a != null)
                        astr = a.ToString();
                    var b = imageList[j - 1][descId];
                    var bstr = string.Empty;
                    if (b != null)
                        bstr = b.ToString();

                    // Escaping to make it xml safe
                    sb.Append(string.Concat("snGalleryTitleArray[", j, "] = '", astr.Replace("\\", "\\\\").Replace("'", "\\'"), "';"));
                    sb.Append(string.Concat("snGalleryDescArray[", j, "] = '", bstr.Replace("\\", "\\\\").Replace("'", "\\'"), "';"));
                }
            }

            sb.Append("</script>");

            sb.Append("<link rel='stylesheet' type='text/css' href='").Append(GetApplicationPath()).Append(CssPath).Append("' />");

            sb.Append(@"
<div id='sn-gallery'>
<div id='snGalleryImageArea'>
    <div id='sn-gallery-img'>
        <a href='javascript:slideShow.nav(-1)' id='snGalleryPrevImg' class='snGalleryImageNav'></a>
        <a href='javascript:slideShow.nav(1)' id='snGalleryNextImg' class='snGalleryImageNav'></a>
    </div>
</div>
<div id='sn-gallery-description'></div>
<div id='snGalleryThumbContainer'>
<div id='snGalleryThumbArea'>
<ul id='snGalleryThumbs'>");

            int i = 1;
            if (imageList != null)
            {
                foreach (Node image in imageList)
                {
                    sb.Append("<li value='").Append(i).Append("'><img src='").Append(GenerateUrl(image.Path)).Append("?NodeProperty=$Binary_1").Append("' alt='' title='").Append(image[titleId]).Append("' /></li>");
                    i++;
                }
            }

            sb.Append(@"</ul></div></div></div>");
            Controls.Add(new LiteralControl(sb.ToString()));
        }

        private string GetContentListPropertyId(ImageGallery ig, string p)
        {
            if (ig.ContentListBindings[p] != null)
            {
                return ig.ContentListBindings[p][0];
            }

            return string.Empty;
        }
    }
}
