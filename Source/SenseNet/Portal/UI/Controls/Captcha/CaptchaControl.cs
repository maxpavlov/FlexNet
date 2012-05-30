using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Specialized;
using System;
using SenseNet.ApplicationModel;
using SenseNet.Portal.Properties;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls.Captcha
{
    [DefaultProperty("Text")]
    public class CaptchaControl : FieldControl, INamingContainer, IValidator
    {
        #region Enums
        public enum Layout
        {
            Horizontal,
            Vertical
        }
        public enum CacheType
        {
            HttpRuntime,
            Session
        }
        #endregion

        #region Member variables

        private int _timeoutSecondsMax = 90;
        private int _timeoutSecondsMin = 3;
        private bool _userValidated = true;
        private string _text = GetSR(SR.EnterCode);
        private string _font = "";
        private CaptchaImage _captcha = new CaptchaImage();
        private Layout _layoutStyle = Layout.Horizontal;
        private string _prevguid;
        private string _errorMessage = GetSR(SR.WrongAnswer);
        private CacheType _cacheStrategy = CacheType.Session;
        private TextBox tbUserEntry;
        #endregion

        #region Properties

        [Browsable(false), Bindable(true), Category("Appearance"), DefaultValue("The text you typed does not match the text in the image."), Description("Message to display in a Validation Summary when the CAPTCHA fails to validate.")]
        string System.Web.UI.IValidator.ErrorMessage {
            get {
                if (!_userValidated)
                {
                    return _errorMessage;
                }
                else
                {
                    return "";
                }
            }
            set {
                _errorMessage = value;
            }
        }

        //[Browsable(false), Bindable(true), Category("Appearance"), DefaultValue("The text you typed does not match the text in the image."), Description("CaptchaHandler.ashx's full path")]
        public string CaptchaHandlerPath
        {
            get;
            set;
        }

        // [Browsable(false), Category("Behavior"), DefaultValue(true), Description("Is Valid"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        bool System.Web.UI.IValidator.IsValid {
            get {
                return _userValidated;
            }
            set {
            }
        }
        public override bool Enabled {
            get {
                return base.Enabled;
            }
            set {
                base.Enabled = value;
                // When a validator is disabled, generally, the intent is not to
                // make the page invalid for that round trip.
                if (!value)
                {
                    _userValidated = true;
                }
            }
        }
        [DefaultValue("Enter the code shown above:"), Description("Instructional text displayed next to CAPTCHA image."), Category("Appearance")]
        public string Text {
            get {
                return _text;
            }
            set {
                _text = value;
            }
        }
        [DefaultValue(typeof(CaptchaControl.Layout), "Horizontal"), Description("Determines if image and input area are displayed horizontally, or vertically."), Category("Captcha")]
        public Layout LayoutStyle {
            get {
                return _layoutStyle;
            }
            set {
                _layoutStyle = value;
            }
        }
        [DefaultValue(typeof(CaptchaControl.CacheType), "HttpRuntime"), Description("Determines if CAPTCHA codes are stored in HttpRuntime (fast, but local to current server) or Session (more portable across web farms)."), Category("Captcha")]
        public CacheType CacheStrategy {
            get {
                return _cacheStrategy;
            }
            set {
                _cacheStrategy = value;
            }
        }
        [Description("Returns True if the user was CAPTCHA validated after a postback."), Category("Captcha")]
        public bool UserValidated {
            get {
                ValidateCaptcha();
                return _userValidated;
            }
        }
        [DefaultValue(""), Description("Font used to render CAPTCHA text. If font name is blank, a random font will be chosen."), Category("Captcha")]
        public string CaptchaFont {
            get {
                return _font;
            }
            set {
                _font = value;
                _captcha.Font = _font;
            }
        }
        [DefaultValue(""), Description("Characters used to render CAPTCHA text. A character will be picked randomly from the string."), Category("Captcha")]
        public string CaptchaChars {
            get {
                return _captcha.TextChars;
            }
            set {
                _captcha.TextChars = value;
            }
        }
        [DefaultValue(5), Description("Number of CaptchaChars used in the CAPTCHA text"), Category("Captcha")]
        public int CaptchaLength {
            get {
                return _captcha.TextLength;
            }
            set {
                _captcha.TextLength = value;
            }
        }
        [DefaultValue(2), Description("Minimum number of seconds CAPTCHA must be displayed before it is valid. If you're too fast, you must be a robot. Set to zero to disable."), Category("Captcha")]
        public int CaptchaMinTimeout {
            get {
                return _timeoutSecondsMin;
            }
            set {
                if (value > 15)
                {
                    throw new ArgumentOutOfRangeException("CaptchaTimeout", "Timeout must be less than 15 seconds. Humans aren't that slow!");
                }
                _timeoutSecondsMin = value;
            }
        }
        [DefaultValue(90), Description("Maximum number of seconds CAPTCHA will be cached and valid. If you're too slow, you may be a CAPTCHA hack attempt. Set to zero to disable."), Category("Captcha")]
        public int CaptchaMaxTimeout {
            get {
                return _timeoutSecondsMax;
            }
            set {
                if (value < 15 & value != 0)
                {
                    throw new ArgumentOutOfRangeException("CaptchaTimeout", "Timeout must be greater than 15 seconds. Humans can't type that fast!");
                }
                _timeoutSecondsMax = value;
            }
        }
        [DefaultValue(50), Description("Height of generated CAPTCHA image."), Category("Captcha")]
        public int CaptchaHeight {
            get {
                return _captcha.Height;
            }
            set {
                _captcha.Height = value;
            }
        }
        [DefaultValue(180), Description("Width of generated CAPTCHA image."), Category("Captcha")]
        public int CaptchaWidth {
            get {
                return _captcha.Width;
            }
            set {
                _captcha.Width = value;
            }
        }
        [DefaultValue(typeof(CaptchaImage.FontWarpFactor), "Low"), Description("Amount of random font warping used on the CAPTCHA text"), Category("Captcha")]
        public CaptchaImage.FontWarpFactor CaptchaFontWarping {
            get {
                return _captcha.FontWarp;
            }
            set {
                _captcha.FontWarp = value;
            }
        }
        [DefaultValue(typeof(CaptchaImage.BackgroundNoiseLevel), "Low"), Description("Amount of background noise to generate in the CAPTCHA image"), Category("Captcha")]
        public CaptchaImage.BackgroundNoiseLevel CaptchaBackgroundNoise {
            get {
                return _captcha.BackgroundNoise;
            }
            set {
                _captcha.BackgroundNoise = value;
            }
        }
        [DefaultValue(typeof(CaptchaImage.LineNoiseLevel), "None"), Description("Add line noise to the CAPTCHA image"), Category("Captcha")]
        public CaptchaImage.LineNoiseLevel CaptchaLineNoise {
            get {
                return _captcha.LineNoise;
            }
            set {
                _captcha.LineNoise = value;
            }
        }
        private System.Drawing.Color _backColor = System.Drawing.Color.White;
        /// <summary>
        /// Background color for the captcha image
        /// </summary>
        public System.Drawing.Color BackColor {
            get { return _backColor; }
            set { _backColor = value; _captcha.BackColor = _backColor; }
        }
		
        private System.Drawing.Color _fontColor = System.Drawing.Color.Black;
        /// <summary>
        /// Color of captcha text
        /// </summary>
        public System.Drawing.Color FontColor 
        {
            get { return _fontColor; }
            set { _fontColor = value; _captcha.FontColor = _fontColor; }
        }
		
        private System.Drawing.Color _noiseColor = System.Drawing.Color.Black;
        /// <summary>
        /// Color for dots in the background noise 
        /// </summary>
        public System.Drawing.Color NoiseColor {
            get { return _noiseColor; }
            set { _noiseColor = value; _captcha.NoiseColor = _noiseColor; }
        }
		
        private System.Drawing.Color _lineColor = System.Drawing.Color.Black;
        /// <summary>
        /// Color for the background lines of the captcha image
        /// </summary>
        public System.Drawing.Color LineColor {
            get { return _lineColor; }
            set { _lineColor = value;  _captcha.LineColor = _lineColor;}
        }
        #endregion

        #region Methods 

        void System.Web.UI.IValidator.Validate()
        {
            //-- a no-op, since we validate in LoadPostData
             
        }
        private CaptchaImage GetCachedCaptcha(string guid)
        {
            CaptchaImage functionReturnValue = null;
            if (_cacheStrategy == CacheType.HttpRuntime)
            {
                return (CaptchaImage)HttpRuntime.Cache.Get(guid);
            }
            else
            {
                return (CaptchaImage)HttpContext.Current.Session[guid];
            }
            return functionReturnValue;
        }
        private void RemoveCachedCaptcha(string guid)
        {
            if (_cacheStrategy == CacheType.HttpRuntime)
            {
                HttpRuntime.Cache.Remove(guid);
            }
            else
            {
                HttpContext.Current.Session.Remove(guid);
            }
        }
        /// <summary>
        /// are we in design mode?
        /// </summary>
        private bool IsDesignMode {
            get {
                return HttpContext.Current == null;
            }
        }
        /// <summary>
        /// Validate the user's text against the CAPTCHA text
        /// </summary>
        public void ValidateCaptcha()
        {
            string userEntry = tbUserEntry.Text;
            if (!Visible | !Enabled)
            {
                _userValidated = true;
                return;
            }
            //-- retrieve the previous captcha from the cache to inspect its properties
            CaptchaImage ci = GetCachedCaptcha(_prevguid);
            if (ci == null)
            {
                SetErrorMessage("The code you typed has expired after " + this.CaptchaMaxTimeout + " seconds.");
                _userValidated = false;
                return;
            }
            //--  was it entered too quickly?
            if (this.CaptchaMinTimeout > 0)
            {
                if ((ci.RenderedAt.AddSeconds(this.CaptchaMinTimeout) > System.DateTime.Now))
                {
                    _userValidated = false;
                    SetErrorMessage("Code was typed too quickly. Wait at least " + this.CaptchaMinTimeout + " seconds.");
                    RemoveCachedCaptcha(_prevguid);
                    return;
                }
            }
            if (string.Compare(userEntry, ci.Text, true) != 0)
            {
                //((System.Web.UI.IValidator) this).ErrorMessage = "The code you typed does not match the code in the image.";
                SetErrorMessage("The code you typed does not match the code in the image.");
                _userValidated = false;
                RemoveCachedCaptcha(_prevguid);
                return;
            }
            _userValidated = true;
            RemoveCachedCaptcha(_prevguid);
        }
        /// <summary>
        /// returns HTML-ized color strings
        /// </summary>
        private string HtmlColor(System.Drawing.Color color)
        {
            string functionReturnValue = null;
            if (color.IsEmpty) return ""; 
            if (color.IsNamedColor)
            {
                return color.ToKnownColor().ToString();
            }
            if (color.IsSystemColor)
            {
                return color.ToString();
            }
            return "#" + color.ToArgb().ToString("x").Substring(2);
            return functionReturnValue;
        }
        /// <summary>
        /// returns css "style=" tag for this control
        /// based on standard control visual properties
        /// </summary>
        private string CssStyle()
        {
            System.Text.StringBuilder functionReturnValue = new System.Text.StringBuilder();
            string strColor;
			
            {
                functionReturnValue.Append(" style='");
                if (BorderWidth.ToString().Length > 0)
                {
                    functionReturnValue.Append("border-width:");
                    functionReturnValue.Append(BorderWidth.ToString());
                    functionReturnValue.Append(";");
                }
                if (BorderStyle != System.Web.UI.WebControls.BorderStyle.NotSet)
                {
                    functionReturnValue.Append("border-style:");
                    functionReturnValue.Append(BorderStyle.ToString());
                    functionReturnValue.Append(";");
                }
                strColor = HtmlColor(BorderColor);
                if (strColor.Length > 0)
                {
                    functionReturnValue.Append("border-color:");
                    functionReturnValue.Append(strColor);
                    functionReturnValue.Append(";");
                }
                strColor = HtmlColor(BackColor);
                if (strColor.Length > 0)
                {
                    functionReturnValue.Append("background-color:" + strColor + ";");
                }
                strColor = HtmlColor(ForeColor);
                if (strColor.Length > 0)
                {
                    functionReturnValue.Append("color:" + strColor + ";");
                }
                if (Font.Bold)
                {
                    functionReturnValue.Append("font-weight:bold;");
                }
                if (Font.Italic)
                {
                    functionReturnValue.Append("font-style:italic;");
                }
                if (Font.Underline)
                {
                    functionReturnValue.Append("text-decoration:underline;");
                }
                if (Font.Strikeout)
                {
                    functionReturnValue.Append("text-decoration:line-through;");
                }
                if (Font.Overline)
                {
                    functionReturnValue.Append("text-decoration:overline;");
                }
                if (Font.Size.ToString().Length > 0)
                {
                    functionReturnValue.Append("font-size:" + Font.Size.ToString() + ";");
                }
                if (Font.Names.Length > 0)
                {
                    // string strFontFamily;
                    functionReturnValue.Append("font-family:");
                    foreach (string strFontFamily in Font.Names) {
                        functionReturnValue.Append(strFontFamily);
                        functionReturnValue.Append(",");
                    }
                    functionReturnValue.Length = functionReturnValue.Length - 1;
                    functionReturnValue.Append(";");
                }
                if (Height.ToString() != "")
                {
                    functionReturnValue.Append("height:" + Height.ToString() + ";");
                }
                if (Width.ToString() != "")
                {
                    functionReturnValue.Append("width:" + Width.ToString() + ";");
                }
                functionReturnValue.Append("'");
            }
            if (functionReturnValue.ToString() == " style=''")
            {
                return "";
            }
            else
            {
                return functionReturnValue.ToString();
            }
            return functionReturnValue.ToString();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Panel pnlContainer = new Panel();


            pnlContainer.CssClass = CssClass; 
            System.Web.UI.WebControls.Image cImage = new System.Web.UI.WebControls.Image();
            //string imageid = string.Format("CaptchaImage.ashx?guid={0}&s=1", _captcha.UniqueId);           
            //string currentPath = PortalContext.Current.RequestedUri.AbsoluteUri;
            //string imageid = string.Format("{0}?Action=GetCaptcha&guid={1}&s=1", currentPath, _captcha.UniqueId);
            cImage.ImageUrl = ActionFramework.GetActionUrl("/Root", "GetCaptcha", string.Empty)+string.Format("&guid={0}&s=1",_captcha.UniqueId);

            cImage.BorderStyle = BorderStyle.None;
            cImage.AlternateText = ToolTip;
            cImage.Width = _captcha.Width;
            cImage.Height = _captcha.Height;
            pnlContainer.Controls.Add(cImage);

            if (this.LayoutStyle == Layout.Vertical)
            {
                Literal lBr = new Literal();
                lBr.Text = "<br />";
                pnlContainer.Controls.Add(lBr);
            }
            else
            {
                Literal lSpace = new Literal();
                lSpace.Text = "&nbsp;&nbsp;";
                pnlContainer.Controls.Add(lSpace);
            }

            Label lblInstruct = new Label();
            lblInstruct.ID = "lblInstructoin";
            lblInstruct.Text = Text;
            lblInstruct.AccessKey = AccessKey;
            pnlContainer.Controls.Add(lblInstruct);

            tbUserEntry = new TextBox();
            tbUserEntry.ID = "tbUserEntry";
            tbUserEntry.AccessKey = AccessKey;
            tbUserEntry.Attributes.Add("size", _captcha.TextLength.ToString());
            tbUserEntry.Attributes.Add("maxlength",_captcha.TextLength.ToString());
            if (!Enabled)
            {
                tbUserEntry.Attributes.Add("disabled","disabled");
            }
            if (TabIndex > 0)
            {
                tbUserEntry.Attributes.Add("tabindex",TabIndex.ToString());
            }
            pnlContainer.Controls.Add(tbUserEntry);

            // Set the style::
            pnlContainer.Attributes.Add("style", this.CssStyle());
            this.Controls.Add(pnlContainer);

            
        }

        /// <summary>
        /// generate a new captcha and store it in the ASP.NET Cache by unique GUID
        /// </summary>
        private void GenerateNewCaptcha()
        {
            if (!IsDesignMode)
            {
                if (_cacheStrategy == CacheType.HttpRuntime)
                {
                    HttpRuntime.Cache.Add(_captcha.UniqueId, _captcha, null, DateTime.Now.AddSeconds(Convert.ToDouble((this.CaptchaMaxTimeout == 0 ? 90 : this.CaptchaMaxTimeout))), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.NotRemovable, null);
                }
                else
                {
                    HttpContext.Current.Session.Add(_captcha.UniqueId, _captcha);
                }
            }
        }

        /// <summary>
        /// Retrieve the user's CAPTCHA input from the posted data
        /// </summary>
        protected override object SaveControlState()
        {
            object functionReturnValue = null;
            return (object)_captcha.UniqueId;
            return functionReturnValue;
        }
        protected override void LoadControlState(object state)
        {
            if (state != null)
            {

                _prevguid = (string)state;
            }
        }

        /// <summary>
        /// Gets object data.
        /// </summary>
        /// <remarks>
        /// Exception handling and displayed is done at ContentView level; FormatExceptions and Exceptions are handled and displayed at this level.
        /// Should you need custom or localized error messages, throw a FieldControlDataException with your own error message.
        /// </remarks>
        /// <returns>Object representing data of the wrapped Field</returns>
        public override object GetData()
        {
            return this.UserValidated;
        }

        /// <summary>
        /// Sets data within the FieldControl
        /// </summary>
        /// <param name="data">Data of the <see cref="SenseNet.ContentRepository.Field">Field</see> wrapped</param>
        public override void SetData(object data)
        {
            //Do nothing, captcha is generated by handler
        }

        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);
            Page.RegisterRequiresControlState(this);
            Page.Validators.Add(this);
        }
        protected override void OnUnload(System.EventArgs e)
        {
            if ((Page != null))
            {
                Page.Validators.Remove(this);
            }
            base.OnUnload(e);
        }
        protected override void OnPreRender(System.EventArgs e)
        {
            if (this.Visible)
            {
                GenerateNewCaptcha();
            }
            base.OnPreRender(e);
        }
        protected static string GetSR(string strResId)
        {
            //TODO SR language?
            
            return Resources.ResourceManager.GetString(strResId);
            //return Language.GetStringValue(Session.Current.GetGlobalVariable("lang"), strResId);
        }

        #endregion



    }
}