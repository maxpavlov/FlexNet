using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.i18n;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    /// <summary>
    /// Display Facebook like button
    /// </summary>
    public class FacebookLikePortlet : ContextBoundPortlet
    {
        #region Enums
        [System.ComponentModel.TypeConverter(typeof(LocalizedEnumConverter))]
        public enum LayoutStyleEnum
        {
            [LocalizedStringValue("FacebookLikeButton", "LayoutStyleEnum_Standard")]
            standard = 0,
            [LocalizedStringValue("FacebookLikeButton", "LayoutStyleEnum_ButtonCount")]
            button_count
        }

        [System.ComponentModel.TypeConverter(typeof(LocalizedEnumConverter))]
        public enum ActionEnum
        {
            [LocalizedStringValue("FacebookLikeButton", "ActionEnum_Like")]
            like,
            [LocalizedStringValue("FacebookLikeButton", "ActionEnum_Recommend")]
            recommend
        }

        [System.ComponentModel.TypeConverter(typeof(LocalizedEnumConverter))]
        public enum FontsEnum
        {
            [LocalizedStringValue("Arial")]
            arial,
            [LocalizedStringValue("Lucida")]
            lucida,
            [LocalizedStringValue("Grande")]
            grande,
            [LocalizedStringValue("Segoe")]
            segoe,
            [LocalizedStringValue("Ui")]
            ui,
            [LocalizedStringValue("Tahoma")]
            tahoma,
            [LocalizedStringValue("Trebuchet")]
            trebuchet,
            [LocalizedStringValue("Ms")]
            ms,
            [LocalizedStringValue("Verdana")]
            verdana
        }

        [System.ComponentModel.TypeConverter(typeof(LocalizedEnumConverter))]
        public enum ColorSchemaEnum
        {
            [LocalizedStringValue("FacebookLikeButton", "ColorSchemaEnum_Light")]
            light,
            [LocalizedStringValue("FacebookLikeButton", "ColorSchemaEnum_Dark")]
            dark
        } 
        #endregion

        private int _buttonWidth = 450;

        private LayoutStyleEnum _layoutStyle;

        /// <summary>
        /// Get or set the like button layout style.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [LocalizedWebDisplayName("FacebookLikeButton", "LayoutStyleTitle"), LocalizedWebDescription("FacebookLikeButton", "LayoutStyleDescription")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory("FacebookLikeButton", "CategoryTitle", 5), WebOrder(20)]
        public LayoutStyleEnum LayoutStyle
        {
            get { return _layoutStyle; }
            set
            {
                if (value < LayoutStyleEnum.standard || value > LayoutStyleEnum.button_count)
                    throw new ArgumentOutOfRangeException();
                _layoutStyle = value;
            }
        }
        /// <summary>
        /// You can specifies whether to display profile photos below the button.
        /// It works only standard layout!
        /// </summary>
        [LocalizedWebDisplayName("FacebookLikeButton", "ShowFacesTitle"), LocalizedWebDescription("FacebookLikeButton", "ShowFacesDescription")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory("FacebookLikeButton", "CategoryTitle", 5), WebOrder(30)]
        public bool ShowFaces { get; set; }

        /// <summary>
        /// Get or set the width of facebook like button
        /// </summary>
        [LocalizedWebDisplayName("FacebookLikeButton", "ButtonWidthTitle"), LocalizedWebDescription("FacebookLikeButton", "ButtonWidthDescription")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory("FacebookLikeButton", "CategoryTitle", 5), WebOrder(40)]
        public int ButtonWidth
        {
            get { return _buttonWidth; }
            set { _buttonWidth = value; }
        }

        /// <summary>
        /// The verb to display on the button. Two possible options are 'like' and 'recommend'.
        /// Default value is 'like'.
        /// </summary>
        [LocalizedWebDisplayName("FacebookLikeButton", "ActionTitle"), LocalizedWebDescription("FacebookLikeButton", "ActionDescription")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory("FacebookLikeButton", "CategoryTitle", 5), WebOrder(50)]
        public ActionEnum Action { get; set; }

        /// <summary>
        /// You can choose which font to display in the button.
        /// </summary>
        [LocalizedWebDisplayName("FacebookLikeButton", "ButtonFontTitle"), LocalizedWebDescription("FacebookLikeButton", "ButtonFontDescription")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory("FacebookLikeButton", "CategoryTitle", 5), WebOrder(60)]
        public FontsEnum ButtonFont { get; set; }

        /// <summary>
        /// You can choose which font to display in the button.
        /// </summary>
        [LocalizedWebDisplayName("FacebookLikeButton", "ColorSchemaTitle"), LocalizedWebDescription("FacebookLikeButton", "ColorSchemaDescription")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory("FacebookLikeButton", "CategoryTitle", 5), WebOrder(70)]
        public ColorSchemaEnum ColorSchema { get; set; }

        [WebBrowsable(false)]
        public override string Renderer
        {
            get
            {
                return base.Renderer;
            }
            set
            {
                base.Renderer = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the FacebookLikePortlet class.
        /// </summary>
        public FacebookLikePortlet()
        {
            this.Name = SenseNetResourceManager.Current.GetString("FacebookLikeButton", "PortletTitle");
            this.Description = SenseNetResourceManager.Current.GetString("FacebookLikeButton", "PortletDescription");
            this.Category = new PortletCategory(PortletCategoryType.Portal);
        }

       /// <summary>
       /// Renders the control to the specified HTML writer.
       /// </summary>
       /// <param name="writer">The System.Web.UI.HtmlTextWriter object that receives the control content.</param>
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write(GenerateLikeButton());
        }

        /// <summary>
        /// Generate IFrame with specified params.
        /// </summary>
        /// <returns>An System.String which contains the IFrame html fragment with specified parameters</returns>
        protected virtual string GenerateLikeButton()
        {
            var likebtn = GetFacebookLikeButtonHtmlFragment;
            if (!string.IsNullOrEmpty(likebtn))
            {
                var iFrame = ReplaceParams(likebtn);
                return iFrame;
            }

            return SenseNetResourceManager.Current.GetString("FacebookLikeButton", "Error_HtmlFragment");
        }

        /// <summary>
        /// It replaces the placeholders to specified parameters in the raw IFrame
        /// </summary>
        /// <param name="rawIFrame">The raw Iframe with placeholders</param>
        /// <returns>An System.String contains generated Iframe</returns>
        protected virtual string ReplaceParams(string rawIFrame)
        {
            try
            {
                return string.Format(rawIFrame,
                                     PortalContext.GetUrlByRepositoryPath(HttpContext.Current.Request.Url.Host,
                                                                          ContextNode.Path),
                                     LayoutStyle, ShowFaces, ButtonWidth, Action, ButtonFont, ColorSchema,
                                     ComputeLikeButtonHeight);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                return SenseNetResourceManager.Current.GetString("FacebookLikeButton", "Error_ReplaceParams");
            }
        }

        /// <summary>
        /// Get Like button minimal height
        /// </summary>
        protected virtual int ComputeLikeButtonHeight
        {
            get { return LayoutStyle == LayoutStyleEnum.button_count ? 20 : ShowFaces ? 80 : 35; }
        }

        /// <summary>
        /// An System.String contains raw Iframe.
        /// </summary>
        protected virtual string GetFacebookLikeButtonHtmlFragment
        {
            get
            {
                return
                    "<iframe src=\"http://www.facebook.com/plugins/like.php?href={0}&amp;layout={1}&amp;show_faces={2}&amp;width={3}&amp;action={4}&amp;font={5}&amp;colorscheme={6}&amp;height={7}\" scrolling=\"no\" frameborder=\"0\" style=\"border:none; overflow:hidden; width:{3}px; height:{7}px;\" allowTransparency=\"true\"></iframe>";
            }
        }
    }
}