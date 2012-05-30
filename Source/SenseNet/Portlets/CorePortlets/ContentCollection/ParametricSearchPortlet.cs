using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Search.Parser;
using System.Linq;
using System.Text.RegularExpressions;
using Content = SenseNet.ContentRepository.Content;
using SenseNet.Portal.Virtualization;
using System.Xml;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.Portal.Portlets.ContentCollection
{
    /// <summary>
    /// Class for representing Parametric Search Portlet. It inherits from ContentCollectionPortlet.
    /// </summary>
    public class ParametricSearchPortlet : ContentCollectionPortlet
    {
        // ============================================================================================================ Consts
        private const string DefaultInputRenderer = @"/Root/System/SystemPlugins/Portlets/ParametricSearch/DefaultSearchForm.ascx";
        private const string EmptyQueryErrorPanelID = "EmptyQueryErrorPanel";

        
        // ============================================================================================================ Members
        private string inputRendererPath;
        private UserControl _inputRenderer;
        private Dictionary<string, string> _searchParams { get; set; }
        private string _queryFilter;
        private bool _invalidQuery;
        private bool _hasUrlInput = false;
        private bool _hasFormInput = false;

        
        // ============================================================================================================ Properties
        /// <summary>
        /// Gets or sets the input renderer.
        /// </summary>
        /// <value>The input renderer. It must be an ascx containing search boxes and a search button.</value>
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Input Renderer")]
        [WebDescription("The form to be rendered for user input")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(1005)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField)), ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string InputRenderer
        {
            get { return string.IsNullOrEmpty(inputRendererPath) ? DefaultInputRenderer : inputRendererPath; }
            set { inputRendererPath = value; }
        }

        private string queryTemplate;
        /// <summary>
        /// Gets or sets the query template.
        /// </summary>
        /// <value>The query template. It contains wildcards according to IDs of textboxes on input renderer.</value>
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Query Template")]
        [WebDescription("Parametric query template. Include parameters with %controlID% markup where controlID is the ID of the corresponding asp.Net control in the ascx defined with Input Renderer property.")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(10)]
        public string QueryTemplate
        {
            get
            {
                if (queryTemplate == null)
                {
                    return String.Empty;
                }
                return queryTemplate;
            }
            set
            {
                queryTemplate = value;
            }
        }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Exact Search")]
        [WebDescription("Leave unchecked to automatically add '*' as a suffix to user inputs.")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(20)]
        public bool ExactSearch { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Allow empty search")]
        [WebDescription("If checked the search will be executed even if the resolved template query is empty.")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(30)]
        public bool AllowEmptySearch { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Query debug")]
        [WebDescription("Check this property to show debug info about resolved query template at the bottom of the portlet's layout.")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(100)]
        public bool QueryTemplateDebug { get; set; }


        // ============================================================================================================ Constructor
        public ParametricSearchPortlet()
        {
            Name = "Parametric search";
            Description = "An all-purpose portlet for various searches";
            Category = new PortletCategory(PortletCategoryType.Search);

            Cacheable = false;   // by default, caching is switched off
            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Cache };

            _searchParams = new Dictionary<string, string>();
        }


        // ============================================================================================================ Methods
        protected override void RenderWithXslt(HtmlTextWriter writer)
        {
            if (_hasFormInput || HttpContext.Current.Request.QueryString.ToString().Contains(Math.Abs((PortalContext.Current.ContextNode.Path + this.ID).GetHashCode()).ToString()) || _hasUrlInput)
            {
                base.RenderWithXslt(writer);
            }
        }
        /// <summary>
        /// Renders the specified writer. It renders input renderer in case of Xslt rendering.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (RenderingMode == RenderMode.Xslt)
            {
                _inputRenderer.RenderControl(writer);
            }
            base.Render(writer);

            if (QueryTemplateDebug)
                writer.Write(string.Concat("Resolved query template: ", GetQueryFilter()));
        }
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event. Parents CreateChildControls() logic moved here for processing 
        /// data of input renderer.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var controIdlList = new List<string>();

            if (!String.IsNullOrEmpty(QueryTemplate))
            {
                foreach (var param in HttpContext.Current.Request.Params.AllKeys)
                {
                    if (this.QueryTemplate.Contains("%" + param + "%"))
                    {
                        _hasUrlInput = true;
                        break;
                    }
                }

                foreach (var key in HttpContext.Current.Request.Form.AllKeys)
                {
                    var controlName = string.Empty;
                    if (key.Contains('$'))
                    {
                        controlName = key.Remove(0, key.LastIndexOf('$') + 1); 
                    }
                    if (QueryTemplate.Contains("%" + controlName + "%"))
                    {
                        _hasFormInput = true;
                        break;
                    }
                } 
            }

            if (_hasFormInput || HttpContext.Current.Request.QueryString.ToString().Contains(Math.Abs((PortalContext.Current.ContextNode.Path + this.ID).GetHashCode()).ToString()) || _hasUrlInput)
            {
                this.GetQueryFilter();  // initialize query filter to see if query is invalid for empty search
                var errorPanel = this.FindControlRecursive(EmptyQueryErrorPanelID);
                if (errorPanel != null)
                    errorPanel.Visible = _invalidQuery;
                if (_invalidQuery)
                    return;

                Content modelData;
                try
                {
                    modelData = GetModel() as Content;

                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                    Controls.Clear();
                    Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
                    return;
                }

                var rootContent = modelData as Content;
                if (rootContent != null) rootContent.AllChildren = AllChildren;

                var model = new ParametricSearchViewModel() { State = State };

                var searchParamList = new List<SearchParameter>();

                foreach (var searchParam in _searchParams)
                {
                    searchParamList.Add(new SearchParameter() { Name = searchParam.Key, Value = searchParam.Value });
                }

                model.SearchParameters = searchParamList.ToArray();

                try
                {
                    var childCount = rootContent != null ? rootContent.ChildCount : 0;
                    var pm = new PagerModel(childCount, State, string.Empty);
                    model.Pager = pm;

                    if (RenderingMode == RenderMode.Xslt)
                    {
                        try
                        {
                            XmlModelData = model.ToXPathNavigator();
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(ex.ToString());
                            Logger.WriteException(ex);
                        }
                    }
                    else if (RenderingMode == RenderMode.Ascx || RenderingMode == RenderMode.Native)
                    {
                        model.Content = rootContent;
                        if (CanCache && Cacheable && IsInCache)
                            return;

                        var viewPath = RenderingMode == RenderMode.Native
                                           ? "/root/Global/Renderers/ContentCollectionView.ascx"
                                           : Renderer;

                        var presenter = Page.LoadControl(viewPath);
                        if (presenter is ContentCollectionView)
                        {
                            ((ContentCollectionView)presenter).Model = model;
                        }
                        if (rootContent != null)
                        {
                            var itemlist = presenter.FindControl(ContentListID);
                            if (itemlist != null)
                            {
                                ContentQueryPresenterPortlet.DataBindingHelper.SetDataSourceAndBind(itemlist,
                                                                                                    rootContent.Children);
                            }
                        }

                        var itemPager = presenter.FindControl("ContentListPager");
                        if (itemPager != null)
                        {
                            ContentQueryPresenterPortlet.DataBindingHelper.SetDataSourceAndBind(itemPager,
                                                                                                model.Pager.PagerActions);
                        }

                        Controls.Add(presenter);
                    }

                }
                catch (ParserException ex)
                {
                    var errorText = new LiteralControl { Text = ex.Message };
                    Controls.Add(errorText);
                }
                catch (Exception ex)
                {
                    var errorText = new LiteralControl { Text = ex.ToString() };
                    Controls.Add(errorText);
                }
            }
            ChildControlsCreated = true;
        }
        protected override string GetQueryFilter()
        {
            if (_queryFilter == null)
            {
                var templateQuery = ReplaceTemplateWildCards(@"%\w+%", @"%user\.(\w+)%", QueryTemplate);

                // if template query is resolved to empty search, and empty search is not allowed, this is an invalid query
                if (string.IsNullOrEmpty(templateQuery) && !this.AllowEmptySearch)
                    _invalidQuery = true;

                var res = String.Format("+({0}) +({1})", QueryFilter, templateQuery);
                _queryFilter = res.Replace("+()", "").Trim();
            }

            return _queryFilter;
        }
        /// <summary>
        /// Replaces the template wild cards.
        /// For example: If template contains %wildcard% and input renderer contains a textbox with 'wildcard' ID then %wildcard%
        /// will changed to Text property of corresponding textbox.
        /// </summary>
        /// <param name="pattern">The pattern which determines changing logic.</param>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        protected virtual string ReplaceTemplateWildCards(string pattern, string userPattern, string template)
        {
            var query = String.Empty;
            _searchParams.Clear();

            if (template != null && pattern != null)
            {
                query = template;

                //Replacing user specific query patterns
                var userInputPattern = new Regex(userPattern);
                User user = null;
                if (userInputPattern.IsMatch(template))
                {
                    user = Context.User.Identity as User;
                }
                if (user != null)
                {
                    foreach (var field in userInputPattern.Matches(template))
                    {
                        var fieldName = field.ToString().Trim('%').Replace("user.", string.Empty);
                        var userFieldValue = user[fieldName].ToString();
                        if (string.IsNullOrEmpty(userFieldValue))
                        {
                            userFieldValue = "*";
                        }
                        query = query.Replace(field.ToString(), userFieldValue.ToLower());
                        //Adding to search parameters list
                        if (!_searchParams.ContainsKey(fieldName))
                        {
                            _searchParams.Add(fieldName, userFieldValue);
                        }
                    }
                }

                //Replacing query patterns
                var inputPattern = new Regex(pattern);
                foreach (var inputParam in inputPattern.Matches(template))
                {
                    var inputParamString = inputParam.ToString();
                    var paramName = inputParamString.Trim('%');
                    var inputValue = HttpContext.Current.Request.Params[paramName];
                    string controlId = (_inputRenderer.ClientID + "_" + paramName).Replace('_', '$');

                    // get field name
                    var paramIndex = template.IndexOf(inputParamString);
                    var colonIndex = template.Substring(0, paramIndex).LastIndexOf(':');
                    var spaceIndex = template.Substring(0, colonIndex).LastIndexOf(' ');
                    var fieldStartIndex = spaceIndex == -1 ? 0 : spaceIndex + 1;
                    var fieldName = template.Substring(fieldStartIndex, colonIndex - fieldStartIndex);

                    // check if param is surrounded with quotation marks
                    var containQuotes = template.Substring(colonIndex + 1, paramIndex - colonIndex).Contains('"');
                    var startQuoteIndex = paramIndex - 1;
                    var endQuoteIndex = paramIndex + inputParamString.Length;
                    var surroundingQuotes = template.Length > startQuoteIndex && template.Length > endQuoteIndex && template[startQuoteIndex] == '"' && template[endQuoteIndex] == '"';

                    //if postback occurred or no url parameter is given, then values must come from the controls
                    if (string.IsNullOrEmpty(inputValue) || Page.IsPostBack)
                    {
                        inputValue = Page.Request.Form[controlId];
                    }
                    else
                    {
                        var tbInput = _inputRenderer.FindControl(paramName) as TextBox;
                        if (tbInput != null && !Page.IsPostBack)
                        {
                            tbInput.Text = inputValue;
                        }
                    }

                    // empty query: input value is empty, or consists of '*'-s
                    var emptyQuery = string.IsNullOrEmpty(inputValue) || string.IsNullOrEmpty(inputValue.TrimEnd('*'));
                    if (!emptyQuery)
                    {
                        // substitute input value to parameter if value is not empty

                        // %param% is surrounded with quotes?
                        if (surroundingQuotes)
                        {
                            // _Text:"%param%" -> strip input from qoutes but otherwise leave it as given
                            var vals = inputValue.Replace("\"", "");
                            query = query.Replace(inputParamString, vals);
                        }
                        else
                        {
                            // %param% is not surrounded with quotes
                            if (inputValue.StartsWith("\"") && inputValue.EndsWith("\""))
                            {
                                // if value is between quotes : leave it as given.
                                query = query.Replace(inputParamString, inputValue);
                            }
                            else
                            {
                                // if value is not between quotes : strip quotes, and split input along spaces, add each value as a separate term
                                var vals = inputValue.Replace("\"", "").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                var wildcard = ExactSearch ? String.Empty : "*";
                                var quotes = containQuotes ? string.Empty : "\"";
                                if (vals.Length > 1)
                                {
                                    var val = string.Empty;
                                    foreach (var s in vals)
                                    {
                                        val = string.Concat(val, string.Format("{0}:{1}{2}{3}{4} ", fieldName, quotes, s, wildcard, quotes));
                                    }
                                    val = val.Trim();
                                    var replacePattern = string.Format("{0}:{1}", fieldName, inputParamString);
                                    query = query.Replace(replacePattern, val);
                                }
                                else
                                {
                                    // only one word: substitue it to inputparam
                                    var val = string.Format("{0}{1}{2}{3}", quotes, inputValue, wildcard, quotes);
                                    query = query.Replace(inputParamString, val);
                                }
                            }
                        }
                    }
                    else
                    {
                        // remove whole expressions from query if input value for this parameter is empty
                        var emptySearchPattern = string.Format("\\S*:\\S*{0}\\S*", inputParamString);
                        query = new Regex(emptySearchPattern).Replace(query, string.Empty).Replace("  ", " ").Trim();
                    }

                    //Adding to search parameters list
                    if (!_searchParams.ContainsKey(paramName))
                    {
                        if (!string.IsNullOrEmpty(inputValue))
                            _searchParams.Add(paramName, inputValue);
                    }
                }
            }

            return query.Trim();
        }
        /// <summary>
        /// Creates the child controls. Parents logic moved to OnPreRender(). It renders input renderer.
        /// </summary>
        protected override void CreateChildControls()
        {

            if (!String.IsNullOrEmpty(InputRenderer))
            {
                _inputRenderer = Page.LoadControl(InputRenderer) as UserControl;
                if (_inputRenderer != null)
                {
                    _inputRenderer.ID = "inputRenderer";
                    Controls.Add(_inputRenderer);
                }
            }
        }
    }
}
