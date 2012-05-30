using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using SenseNet.ContentRepository.Storage;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using SenseNet.Portal.Virtualization;
using SenseNet.Portal.UI.Controls;
using SNC = SenseNet.ContentRepository;
using SNP = SenseNet.Portal;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository;
using System.Linq;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI
{
    /// <summary>
    /// <c>ContentView</c> is a class responsible for enabling various visualizations of the <see cref="SenseNet.ContentRepository.Content ">Content</see> class that it owns.
    /// </summary>
    /// <remarks>
    /// The two most important properties of <c>ContentView</c> is the collection of <see cref="SenseNet.Portal.UI.Controls.FieldControl">FieldControl</see>s and the <see cref="SenseNet.ContentRepository.Content ">Content</see> it references.
    /// These two parts are closely related as every <see cref="SenseNet.Portal.UI.Controls.FieldControl">FieldControl</see> wraps a #Field# defined within its <see cref="SenseNet.ContentRepository.Content ">Content</see>.
    /// 
    /// Basically the <c>ContentView</c> is a visualizer of the <see cref="SenseNet.ContentRepository.Content ">Content</see> assigned to it when created. The <see cref="SenseNet.Portal.UI.Controls.FieldControl">FieldControl</see>s of this ContentView
    /// each visualize a #Field# defined within this <see cref="SenseNet.ContentRepository.Content ">Content</see> (but not necessary all of them). 
    /// 
    /// Another important role of the <c>ContentView</c> is automatically utilizing the pre-defined <see cref="SenseNet.Portal.UI.ViewMode">ViewMode</see>s 
    /// It renders itself and all of its <see cref="SenseNet.Portal.UI.Controls.FieldControl">FieldControl</see>s according to the <see cref="SenseNet.Portal.UI.ViewMode">ViewMode</see> assigned to it when created.
    /// </remarks>
    /// <example>
    /// 
    /// The following code shows a method that loads a ContentView using a Browse view defined in a file located elsewhere than the default location
    /// <code>
    /// public ContentView LoadCustomBrowseView(string nodePath, string customBrowseViewPath)
    /// {
    ///     Node node = Content.Load(nodePath);
    ///     if(node != null)
    ///     {
    ///         ContentView contentView = ContentView.Create(node,this.Page,ViewMode.Browse,customBrowseViewPath);
    ///         return(contentView);
    ///     }
    ///     else
    ///     {
    ///         // TODO: throw exception indicating that nodePath either does not define a node or permission to view it is denied
    ///         throw new NullReferenceException();
    ///     }
    /// </code>
    /// </example>
    public abstract class ContentView : UserControl, INamingContainer
    {
        /// <summary>
        /// EventHandler for the UserAction. Use it in the following cases:
        ///  - defaultbuttons is contained in contentview (obsolete!)
        ///  - a button with OnClick="Click" property is contained in a SingleContentView
        /// Use CommandButtonsAction when using the CommandButtons control in the contentview!
        /// </summary>
        public event EventHandler<UserActionEventArgs> UserAction;

        /// <summary>
        /// EventHandler for CommandButtons control. When a button in the CommandButtons control is clicked, this event is fired.
        /// </summary>
        public event EventHandler<CommandButtonsEventArgs> CommandButtonsAction;


        private List<ErrorControl> _errorControls;

        /// <summary>
        /// Gets the owned Content object
        /// </summary>
        public SNC.Content Content { get; private set; }

        /// <summary>
        /// Gets the <see cref="SenseNet.Portal.UI.ViewMode">ViewMode</see> in which the content should be viewed. This value determines the DefaultControlRenderMode property.
        /// </summary>
        public ViewMode ViewMode { get; private set; }

        /// <summary>
        /// Gets the <see cref="SenseNet.Portal.UI.Controls.FieldControlRenderMode">FieldControlRenderMode</see> which will be the default render mode for <see cref="SenseNet.Portal.UI.Controls.FieldControl">FieldControl</see>s. This value is derermined by the ViewMode property.
        /// </summary>
        [Obsolete("Use ViewControlMode and ViewControlFrameMode instead")]
        public FieldControlRenderMode DefaultControlRenderMode { get; set; }

        private FieldControlControlMode _viewControlMode = FieldControlControlMode.None;
        public FieldControlControlMode ViewControlMode
        {
            get
            {
                // backward compatibility
                if (_viewControlMode == FieldControlControlMode.None) 
                {
                    switch (this.DefaultControlRenderMode)
                    {
                        case FieldControlRenderMode.Browse:
                            return FieldControlControlMode.Browse;
                        case FieldControlRenderMode.Default:
                        case FieldControlRenderMode.Edit:
                        case FieldControlRenderMode.InlineEdit:
                            return FieldControlControlMode.Edit;
                    }
                }

                return _viewControlMode;
            }
            set
            {
                _viewControlMode = value;
            }
        }
        private FieldControlFrameMode _viewControlFrameMode;
        public FieldControlFrameMode ViewControlFrameMode
        {
            get
            {
                // backward compatibility
                if (_viewControlFrameMode == FieldControlFrameMode.None)
                {
                    switch (this.DefaultControlRenderMode)
                    {
                        case FieldControlRenderMode.Browse:
                        case FieldControlRenderMode.InlineEdit:
                            return FieldControlFrameMode.NoFrame;
                        case FieldControlRenderMode.Default:
                        case FieldControlRenderMode.Edit:
                            return FieldControlFrameMode.ShowFrame;
                    }
                    return FieldControlFrameMode.ShowFrame;
                }
                return _viewControlFrameMode;
            }
            set
            {
                _viewControlFrameMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the friendly name of the view. By default it is the title of the <see cref="SenseNet.ContentRepository.Content"> it wraps.</see>
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the  description of the view. By default it is the description of the <see cref="SenseNet.ContentRepository.Content"> it wraps.</see>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon of the view. By default it is the icon of the <see cref="SenseNet.ContentRepository.Content"> it wraps.</see>
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets the list of owned <see cref="SenseNet.Portal.UI.Controls.FieldControl">FieldControl</see>s. ContentView is build around this collection.
        /// </summary>
        public List<FieldControl> FieldControls { get; private set; }

        /// <summary>
        /// Gets or sets the ContentException. If not null this exception is displayed within the view.
        /// </summary>
        public Exception ContentException { get; set; }
        public bool NeedToValidate { get; set; }
        public bool IsUserInputValid { get; private set; }

        #region Shortcuts
        /// <summary>
        /// Gets the Path of the ContentHandler in a safe way. (This ContentHandler is wrapped by the Content which is owned by this ContentView) (Is a shortcut)
        /// </summary>
        public string ParentPath
        {
            get
            {
                if (this.Content == null || this.Content.ContentHandler == null || this.Content.ContentHandler.ParentId == 0)
                    return String.Empty;
                return this.Content.ContentHandler.ParentPath;
            }
        }

        /// <summary>
        /// Gets the <see cref="SenseNet.ContentRepository.Storage.Node">ContentHandler</see> wrapped by the <see cref="SenseNet.ContentRepository.Content ">Content</see> which is owned by this ContentView. (Is a shortcut)
        /// </summary>
        public Node ContentHandler
        {
            get { return this.Content.ContentHandler; }
        }

        /// <summary>
        /// Gets the ContentType property of the <see cref="SenseNet.ContentRepository.Content ">Content</see> owned by this ContentView. (Is a shortcut)
        /// </summary>
        public ContentType ContentType
        {
            get { return this.Content.ContentType; }
        }

        /// <summary>
        /// Gets the Name of the Content owned by this ContentView in a safe way. (Is a shortcut)
        /// </summary>
        public string ContentName
        {
            get
            {
                if (this.Content == null)
                    return String.Empty;
                return this.Content.Name;
            }
        }

        /// <summary>
        /// Gets the Name of the ContentType of the <see cref="SenseNet.ContentRepository.Content ">Content</see> owned by this ContentView in a safe way. (Is a shortcut)
        /// </summary>
        public string ContentTypeName
        {
            get
            {
                if (this.Content == null || this.Content.ContentType == null)
                    return String.Empty;
                return this.Content.ContentType.Name;
            }
        }

        [Obsolete("This method is obsolete. Please use GetValue instead.", false)]
        protected string GetProperty(string name)
        {
            return (GetValue(name));
        }

        protected string GetValue(string name)
        {
            try
            {
                return (GetValue(name, this.Content));
            }
            catch (Exception ex) //logged
            {
                Logger.WriteException(ex);
                ContentException = ex;
                return ("");
            }
        }

        /// <summary>
        /// Gets a the specified property belonging to the Content of the View in a safe way.
        /// </summary>
        /// <param name="name">The property name. Can be hierarchical.</param>
        /// <returns>String value of the property specified</returns>
        internal static string GetValue(string name, SNC.Content parentContent, OutputMethod outputMethod)
        {
            switch (outputMethod)
            {
                case OutputMethod.Default:
                    throw new NotSupportedException("OutputMethod cannot be Default");
                case OutputMethod.Raw:
                    return GetValue(name, parentContent);
                case OutputMethod.Text:
                    return System.Web.HttpUtility.HtmlEncode(GetValue(name, parentContent));
                case OutputMethod.Html:
                    return SenseNet.Portal.Security.Sanitize(GetValue(name, parentContent));
                default:
                    throw new NotImplementedException("Unknown OutputMethod: " + outputMethod);
            }
        }
        internal static string GetValue(string name, SNC.Content parentContent)
        {
            string[] parts = name.Split(new char[] { '.' });
            if (parts.Length == 0)
                return "";

            string currPart = "";
            #region special values

            if (parts[0].ToLower() == "current" && parts.Length > 1)
            {
                switch (parts[1].ToLower())
                {
                    case "url":
                        if (parts.Length > 2)
                        {
                            switch (parts[2].ToLower())
                            {
                                case "hostname":
                                    foreach (string url in SNP.Page.Current.Site.UrlList.Keys)
                                    {
                                        if (SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.ToString().IndexOf(string.Concat(SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.Scheme, "://", url, "/")) == 0)
                                        {
                                            return (url);
                                        }
                                    }
                                    return ("");
                                case "host":
                                    foreach (string url in SNP.Page.Current.Site.UrlList.Keys)
                                    {
                                        if (SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.ToString().IndexOf(string.Concat(SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.Scheme, "://", url, "/")) == 0)
                                        {
                                            return (string.Concat(SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.Scheme, "://", url));
                                        }
                                    }
                                    return ("");
                                case "name":
                                    return (SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.OriginalString);
                                default:
                                    return ("");
                            }
                        }
                        else
                            return ("");
                    case "user":
                        if (User.Current != null && parts.Length > 1)
                        {
                            switch (parts[2].ToLower())
                            {
                                case "isauthenticated":
                                    return (User.Current.IsAuthenticated ? "1" : "0");
                                default:
                                    return (((User)User.Current).GetProperty(parts[2]).ToString());
                            }
                        }
                        else
                            return ("");
                    case "site":
                        if (PortalContext.Current.Site != null)
                        {
                            return PortalContext.Current.Site.GetProperty(parts[2]).ToString();
                        }
                        else
                            return ("");
                    case "page":
                        if (PortalContext.Current.Page != null)
                        {
                            return PortalContext.Current.Page.GetProperty(parts[2]).ToString();
                        }
                        else
                            return ("");
                }
            }

            #endregion

            object obj = null;
            object previousListObj = null; // Needed because of IList fields with no indexing
            foreach (string _currPart in parts)
            {
                currPart = _currPart;
                int index = 0;
                //bool isIndexed = false;

                #region Custom properties

                // Check if current part is indexed and if it is get the index and take it off the current part
                if (IsIndexedField(currPart))
                {
                    index = GetIndexedFieldValue(currPart);
                    //isIndexed = true;
                    currPart = StripIndex(currPart);
                }

                if (currPart == "Count")
                {
                    if (previousListObj is ICollection)
                        return (((ICollection)previousListObj).Count.ToString());
                    else if (obj is ICollection)
                        return (((ICollection)obj).Count.ToString());
                }

                if (currPart == "AsList()" || Regex.IsMatch(currPart, "AsList[(](['\"](.*)['\"])[)]"))
                {
                    if (previousListObj is IList)
                    {
                        Match firstMatch = Regex.Match(currPart, "AsList[(](['\"](.*)['\"])[)]");
                        string separatorString = firstMatch.Groups[1].Value;
                        if (separatorString == String.Empty)
                            separatorString = ", "; // default
                        List<string> elements = new List<string>();
                        foreach (object currObj in previousListObj as IList)
                            elements.Add(currObj.ToString());
                        return string.Join(separatorString, elements.ToArray());
                    }
                }

                if (_currPart == "Load()" && obj is string)
                {
                    obj = Node.Load<Node>(obj as string);
                    if (!(obj is Node))
                        return "";
                    else
                        parentContent = SNC.Content.Load(((Node)obj).Id);
                    continue;
                }

                if (currPart == "SiteRelativePath")
                {
                    if (SNP.Page.Current.Site != null && ((Node)parentContent.ContentHandler).Path.StartsWith(SNP.Page.Current.Site.Path))
                        return (((Node)parentContent.ContentHandler).Path.Substring(SNP.Page.Current.Site.Path.Length));
                    else
                        currPart = "Path";
                }

                if (currPart == "PageRelativePath")
                {
                    if (SNP.Page.Current != null && ((Node)parentContent.ContentHandler).Path.StartsWith(SNP.Page.Current.Path))
                        return (((Node)parentContent.ContentHandler).Path.Substring(String.Concat(SNP.Page.Current.Path, "/").Length));
                    else
                        currPart = "Path";
                }

                if (currPart == "Children" && obj is IFolder)
                {
                    IFolder f = obj as IFolder;
                    previousListObj = f.Children;
                    obj = f.Children;
                    if (index < f.ChildCount)
                    {
                        obj = f.Children.ToArray<Node>()[index];
                        parentContent = SNC.Content.Load(((Node)obj).Id);
                    }
                    continue;
                }

                // Check for 'string' parts
                if (Regex.IsMatch(_currPart, @"'(.+)'"))
                {
                    if (obj == null || obj is string)
                    {
                        Match firstMatch = Regex.Match(_currPart, @"'(.+)'");
                        obj += firstMatch.Groups[1].Value;
                        continue;
                    }
                    else
                        return "";
                }

                #region Custom Field property mappings

                if (obj is SenseNet.ContentRepository.Fields.HyperLinkField.HyperlinkData)
                {
                    var hyperlinkObj = (SenseNet.ContentRepository.Fields.HyperLinkField.HyperlinkData)obj;
                    if (currPart == "Href")
                        obj = hyperlinkObj.Href;
                    if (currPart == "Text")
                        obj = hyperlinkObj.Text;
                    if (currPart == "Title")
                        obj = hyperlinkObj.Title;
                    if (currPart == "Target")
                        obj = hyperlinkObj.Target;
                    continue;
                }

                #endregion

                #endregion

                // If parent content is empty next part can not be resolved. Later on however something could be returned if 'string' or Load() part is present
                if (parentContent == null)
                {
                    obj = null;
                    continue;
                }

                // Try to get the property of the current part
                try
                {
                    obj = parentContent[_currPart];//((GenericContent)parentContent.ContentHandler).GetProperty(currPart);
                }
                catch (Exception e) //logged
                {
                    Logger.WriteException(e);
                    obj = ((GenericContent)parentContent.ContentHandler).GetProperty(currPart);
                }

                #region Check the type of the obj and deal with it and set previousListObj, parentContent and obj accordingly
                if (obj is IList)
                {
                    IList list = obj as IList;
                    obj = list;
                    previousListObj = list;
                    if (index < list.Count)
                    {
                        obj = list[index];
                        Node node = obj as Node;
                        if (node != null)
                            parentContent = SNC.Content.Load(node.Id);
                        else
                            parentContent = null;
                    }
                    else
                        parentContent = null;
                    continue;
                }
                else if (obj is SenseNet.ContentRepository.Fields.HyperLinkField.HyperlinkData)
                {
                    continue;
                }
                else if (obj is Node)//Not link and not HyperlinkData
                {
                    parentContent = SNC.Content.Load(((Node)obj).Id);
                    continue;
                }
                else // If the object was not of the above then carry it on to the next iteration
                {
                    continue;
                }
                #endregion
            }
            if (obj != null) // No more parts left
                return (obj.ToString());
            else
            {
                return ("");
                //throw new Exception(String.Format("{0} could not be resolved at {1}.", name, currPart));
            }
        }

        protected static bool IsIndexedField(string text)
        {
            string pattern = @"\[(.+)\]";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(text);
            return (m.Success);
        }

        protected static string StripIndex(string text)
        {
            string pattern = @"(.+)\[.+\]";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            text = r.Replace(text, "$1");
            return (text);
        }

        private static int GetIndexedFieldValue(string text)
        {
            Regex regex = new Regex(@".+\[(.+)\]");
            Match match = regex.Match(text);
            if (match.Success)
            {
                int propertyIndex = Convert.ToInt32(match.Groups[1].Value);
                return (propertyIndex);
            }
            else
                return 0;
        }

        #endregion

        /// <summary>
        /// Creates the ContenView for the <see cref="SenseNet.ContentRepository.Content ">Content</see>.
        /// </summary>
        /// <param name="content">The Content belonging to the the ContentView</param>
        /// <param name="aspNetPage"></param>
        /// <param name="mode">The ViewMode in which FieldControls of the ContentView will be rendered</param>
        public static ContentView Create(SNC.Content content, System.Web.UI.Page aspNetPage, ViewMode mode)
        {
            return CreateFromViewRoot(content, aspNetPage, mode, Repository.ContentViewFolderName);
        }

        /// <summary>
        /// Creates the ContenView for the <see cref="SenseNet.ContentRepository.Content ">Content</see>.
        /// </summary>
        /// <param name="content">The Content belonging to the the ContentView</param>
        /// <param name="aspNetPage"></param>
        /// <param name="mode">The ViewMode in which FieldControls of the ContentView will be rendered</param>
        /// <param name="viewPath">The location of the file defining the ContentView</param>
        public static ContentView Create(SNC.Content content, System.Web.UI.Page aspNetPage, ViewMode mode, string viewPath)
        {
            // ways to call this function:
            // - absolute actual: "/Root/Global/contentviews/Book/Edit.ascx"
            // - relative actual: "$skin/contentviews/Book/Edit.ascx"
            // - absolute root:   "/Root/Global/contentviews"
            // - relative root:   "$skin/contentviews"
            // - empty string:    ""

            var resolvedPath = ResolveContentViewPath(content, mode, viewPath);

            if (string.IsNullOrEmpty(resolvedPath))
            {
                if (viewPath.ToLower().EndsWith(".ascx"))
                    throw new ApplicationException(String.Concat("ContentView was not found: ", viewPath));
                else
                    throw new ApplicationException(String.Concat("ViewRoot was not found: ", viewPath));
            }
            return CreateFromActualPath(content, aspNetPage, mode, resolvedPath);
        }
        private static string GetTypeDependentPath(SNC.Content content, ViewMode mode, string viewRoot)
        {
            // "$skin/contentviews/Book/Edit.ascx"
            return RepositoryPath.Combine(viewRoot, String.Concat("/", content.ContentType.Name, "/", mode, ".ascx"));
        }
        private static string GetGenericPath(SNC.Content content, ViewMode mode, string viewRoot)
        {
            // "$skin/contentviews/Edit.ascx"
            return RepositoryPath.Combine(viewRoot, String.Concat(mode, ".ascx"));
        }
        private static string ResolveContentViewPath(SNC.Content content, ViewMode mode, string path)
        {
            // ways to call this function:
            // - absolute actual: "/Root/Global/contentviews/Book/Edit.ascx"
            // - relative actual: "$skin/contentviews/Book/Edit.ascx"
            // - absolute root:   "/Root/Global/contentviews"
            // - relative root:   "$skin/contentviews"
            // - empty string:    ""

            if (string.IsNullOrEmpty(path))
                path = Repository.ContentViewFolderName;

            var isActual = path.EndsWith(".ascx");

            // - relative with custom name --> convert to relative actual
            if (isActual && !path.Contains(RepositoryPath.PathSeparator))
                path = string.Format("{0}/{1}/{2}", Repository.ContentViewFolderName, content.ContentType.Name, path);

            var isAbsolute = SkinManager.IsNotSkinRelativePath(path);

            // - absolute actual: "/Root/Global/contentviews/Book/Edit.ascx"
            if (isAbsolute && isActual)
            {
                return path;
                // "/Root/Global/contentviews/Book/Edit.ascx"
            }

            // - relative actual: "$skin/contentviews/Book/Edit.ascx"
            if (!isAbsolute && isActual)
            {
                return SkinManager.Resolve(path);
                // "/Root/{skin/global}/contentviews/Book/Edit.ascx"
            }

            // - absolute root:   "/Root/Global/contentviews"
            if (isAbsolute && !isActual)
            {
                var probePath = GetTypeDependentPath(content, mode, path);
                var node = Node.LoadNode(probePath);
                if (node != null)
                    return probePath;
                    // "/Root/Global/contentviews/Book/Edit.ascx"
                
                probePath = GetGenericPath(content, mode, path);
                node = Node.LoadNode(probePath);
                if (node != null)
                    return probePath;
                    // "/Root/Global/contentviews/Edit.ascx"

                return string.Empty;
            }

            // - relative root:   "$skin/contentviews"
            if (!isAbsolute && !isActual)
            {
                var typedPath = GetTypeDependentPath(content, mode, path);
                var resolvedPath = string.Empty;
                if (SkinManager.TryResolve(typedPath, out resolvedPath))
                    return resolvedPath;
                    // "/Root/{skin}/contentviews/Book/Edit.ascx"

                var genericPath = GetGenericPath(content, mode, path);
                if (SkinManager.TryResolve(genericPath, out resolvedPath))
                    return resolvedPath;
                    // "/Root/{skin}/contentviews/Edit.ascx"

                var probePath = SkinManager.Resolve(typedPath);
                var node = Node.LoadNode(probePath);
                if (node != null)
                    return probePath;
                    // "/Root/Global/contentviews/Book/Edit.ascx"

                probePath = SkinManager.Resolve(genericPath);
                node = Node.LoadNode(probePath);
                if (node != null)
                    return probePath;
                    // "/Root/Global/contentviews/Edit.ascx"

                return string.Empty;
            }
            return string.Empty;
        }
        private static ContentView CreateFromViewRoot(SNC.Content content, System.Web.UI.Page aspNetPage, ViewMode mode, string viewRoot)
        {
            // ways to call this function:
            // - absolute root:   "/Root/Global/contentviews"
            // - relative root:   "$skin/contentviews"
            // - empty string:    ""

            string resolvedPath = ResolveContentViewPath(content, mode, viewRoot);
            return CreateFromActualPath(content, aspNetPage, mode, resolvedPath);
        }
        private static ContentView CreateFromActualPath(SNC.Content content, System.Web.UI.Page aspNetPage, ViewMode viewMode, string viewPath)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            if (aspNetPage == null)
                throw new ArgumentNullException("aspNetPage");
            if (viewPath == null)
                throw new ArgumentNullException("viewPath");
            if (viewPath.Length == 0)
                throw new ArgumentOutOfRangeException("viewPath", "Parameter 'viewPath' cannot be empty");
            if (viewMode == ViewMode.None)
                throw new ArgumentOutOfRangeException("viewMode", "Parameter 'viewMode' cannot be ViewMode.None");

            string path = String.Concat("~", viewPath);

            ContentView view = GetContentViewFromCache(path);

            // if not in request cache
            if (view == null)
            {
                var addToCache = false;
                try
                {
                    view = aspNetPage.LoadControl(path) as ContentView;
                    addToCache = true;
                }
                catch (Exception e) //logged
                {
                    Logger.WriteException(e);
                    var errorContentViewPath = RepositoryPath.Combine(Repository.ContentViewFolderName, "Error.ascx");
                    var resolvedErrorContentViewPath = SkinManager.Resolve(errorContentViewPath);
                    path = String.Concat("~", resolvedErrorContentViewPath);
                    view = aspNetPage.LoadControl(path) as ContentView;
                    view.ContentException = e;
                }
                if (view == null)
                    throw new ApplicationException(string.Format("ContentView instantiation via LoadControl for path '{0}' failed.", path));
                if (addToCache)
                    AddContentViewToCache(path, view);
            }

            view.Initialize(content, viewMode);
            return view;
        }
        private static ContentView GetContentViewFromCache(string path)
        {
            System.Web.HttpContext currentContext = System.Web.HttpContext.Current;
            if (currentContext == null)
                return null;

            Dictionary<string, ConstructorInfo> constructorCache;

            constructorCache = currentContext.Items["ContextBasedContentViewCache"] as Dictionary<string, ConstructorInfo>;

            if (constructorCache == null)
                return null;

            if (!constructorCache.ContainsKey(path))
                return null;

            ConstructorInfo ci = constructorCache[path];

            return ci.Invoke(new object[0]) as ContentView;
        }
        private static void AddContentViewToCache(string path, ContentView contentView)
        {
            System.Web.HttpContext currentContext = System.Web.HttpContext.Current;

            if (currentContext == null)
                return;

            Dictionary<string, ConstructorInfo> constructorCache;

            constructorCache = currentContext.Items["ContextBasedContentViewCache"] as Dictionary<string, ConstructorInfo>;

            if (constructorCache == null)
            {
                constructorCache = new Dictionary<string, ConstructorInfo>();
                currentContext.Items.Add("ContextBasedContentViewCache", constructorCache);
            }

            constructorCache[path] = contentView.GetType().GetConstructor(Type.EmptyTypes);
        }

        internal static ContentView RegisterControl(ViewControlBase control)
        {
            //-- #1: cast
            ErrorControl errorControl = null;
            FieldControl fieldControl = control as FieldControl;
            if (fieldControl == null)
                errorControl = control as ErrorControl;

            //-- #2: get ContentView
            Control parent = control.Parent;
            while (parent != null && !(parent is ContentView))
                parent = parent.Parent;
            ContentView view = (parent as ContentView);

            //-- #3: there is not a ContentView in the parent axis: exception
            if (view == null)
            {
                if (fieldControl != null)
                    throw new ApplicationException(String.Concat("Control did not find the ContentView. Control type: ", control.GetType().FullName, ", Id: '", control.ID, "', FieldName: '", fieldControl.FieldName, "'"));
                throw new ApplicationException(String.Concat("Control did not find the ContentView. Control type: ", control.GetType().FullName, ", Id: '", control.ID));
            }

            //-- #4 finish if GenericFieldControl or others
            if (fieldControl == null && errorControl == null)
                return view;

            //-- #5 register by type
            if (errorControl != null)
                view.RegisterErrorControl(errorControl);
            else
                view.RegisterFieldControl(fieldControl);

            //-- #6 introduce the view to caller
            return view;
        }

        internal void Initialize(SNC.Content content, ViewMode viewMode)
        {
            this.Content = content;
            this.FieldControls = new List<FieldControl>();
            _errorControls = new List<ErrorControl>();
            this.DisplayName = content.DisplayName;
            this.Description = content.Description;
            this.Icon = content.Icon;
            string prefixId = String.Concat("ContentView", "_", viewMode, "_");
            this.ID = String.Concat(prefixId, content.ContentHandler.Id.ToString());
            this.ViewMode = viewMode;
            this.DefaultControlRenderMode = GetDefaultControlRenderMode(viewMode);
            OnViewInitialize();
        }
        private FieldControlRenderMode GetDefaultControlRenderMode(ViewMode viewMode)
        {
            switch (viewMode)
            {
                default:
                case ViewMode.None:
                case ViewMode.Query:
                case ViewMode.InlineNew:
                case ViewMode.InlineEdit:
                case ViewMode.New:
                case ViewMode.Edit: return FieldControlRenderMode.Edit;
                case ViewMode.Grid:
                case ViewMode.Browse: return FieldControlRenderMode.Browse;
            }
        }
        protected virtual void OnViewInitialize()
        {
        }

        /// <summary>
        /// Fires all EventHandlers registered for UserAction.
        /// </summary>
        /// <param name="sender">Object from where the call is originating from</param>
        /// <param name="actionName">Name of the action needed to be handled</param>
        /// <param name="eventName">Name of the event needed to be handled</param>
        public void OnUserAction(object sender, string actionName, string eventName)
        {
            if (UserAction != null)
            {
                NeedToValidate = true;
                UserAction(sender, new UserActionEventArgs(actionName, eventName, this));
            }
        }
        public void OnCommandButtonsAction(object sender, CommandButtonType commandButtonType, out bool cancelled)
        {
            OnCommandButtonsAction(sender, commandButtonType, null, out cancelled);
        }
        public void OnCommandButtonsAction(object sender, string customCommand, out bool cancelled)
        {
            OnCommandButtonsAction(sender, CommandButtonType.None, customCommand, out cancelled);
        }
        public void OnCommandButtonsAction(object sender, CommandButtonType commandButtonType, string customCommand, out bool cancelled)
        {
            cancelled = false;
            if (CommandButtonsAction != null)
            {
                NeedToValidate = true;
                var args = new CommandButtonsEventArgs(commandButtonType, this, customCommand);
                CommandButtonsAction(sender, args);
                if (args.Cancel)
                    cancelled = true;
            }
        }

        internal void RegisterFieldControl(FieldControl fieldControl)
        {
            SetFieldInControl(fieldControl);
            this.FieldControls.Add(fieldControl);
        }
        internal void RegisterErrorControl(ErrorControl errorControl)
        {
            _errorControls.Add(errorControl);
        }
        private void SetFieldInControl(FieldControl fieldControl)
        {
            Field field = null;
            if (!this.Content.Fields.TryGetValue(fieldControl.FieldName, out field))
                throw new ApplicationException(String.Concat("ContentType '", this.Content.ContentHandler.Name, "' does not contain Field: ", fieldControl.FieldName));
            fieldControl.Field = field;
        }

        /// <summary>
        /// Updates and validates data of <see cref="SenseNet.Portal.UI.Controls.FieldControl">FieldControl</see>s owned by this ContentView
        /// </summary>
        public void UpdateContent()
        {
            UpdateContent(true);
        }

        /// <summary>
        /// Updates data of <see cref="SenseNet.Portal.UI.Controls.FieldControl">FieldControl</see>s owned by this ContentView
        /// </summary>
        /// <param name="withValidate">Whether the Updates data of <see cref="SenseNet.ContentRepository.Content">Content</see>s owned by this ContentView data should be validated after updating. When <c>true</c> content validation errors will be displayed (if any), when <c>false</c>, these errors will not be visible.</param>
        public void UpdateContent(bool withValidate)
        {
            using (var traceOperation = Logger.TraceOperation("ContentView.UpdateContent"))
            {
                this.IsUserInputValid = true;
                this.ContentException = null;
                foreach (IFieldControl fieldControl in this.FieldControls)
                {
                    if (fieldControl.ReadOnly)
                        continue;
                    Field field = fieldControl.Field;
                    try
                    {
                        field.SetData(fieldControl.GetData());
                    }
                    catch (FieldControlDataException fcex) //logged
                    {
                        Logger.WriteException(fcex);
                        this.IsUserInputValid = false;
                        var message = GetString("FieldControlErrors", fcex.ResourceStringKey ?? "InvalidData");
                        fieldControl.SetErrorMessage(message ?? "Invalid data");
                        this.SetContentViewFieldError();
                    }
                    catch (FormatException fex) //logged
                    {
                        Logger.WriteException(fex);
                        this.IsUserInputValid = false;
                        var message = GetString("InvalidFormat");
                        fieldControl.SetErrorMessage(message ?? "Invalid format");
                        this.SetContentViewFieldError();
                    }
                    catch (Exception ex) //logged
                    {
                        Logger.WriteException(ex);
                        this.IsUserInputValid = false;
                        var message = GetString("InvalidData");
                        fieldControl.SetErrorMessage(message ?? "Invalid data");
                        this.SetContentViewFieldError();
                    }
                }
                if (this.IsUserInputValid && withValidate)
                {
                    this.Content.Validate();
                    foreach (IFieldControl fieldControl in this.FieldControls)
                    {
                        if (fieldControl == null)
                            continue;
                        if (fieldControl.ReadOnly)
                            continue;

                        if (!this.NeedToValidate || fieldControl.Field.IsValid)
                            fieldControl.ClearError();
                        else
                        {
                            fieldControl.SetErrorMessage(FieldControl.ResolveValidationResult(fieldControl.Field));
                            this.SetContentViewFieldError();
                        }
                    }
                }
                traceOperation.IsSuccessful = true;
            }
        }
        /// <summary>
        /// Renders the view.
        /// </summary>
        /// <remarks>
        /// Should an error occur (ContentException being set) and there are no error controls registered then the error message will rendered in the top of the view.
        /// Otherwise errors (if any) are viewed within the registered error control(s).
        /// </remarks>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.ContentException != null && _errorControls.Count == 0)
                ErrorView.RenderContentError(writer, this.ContentException);

            try
            {
                base.Render(writer);
            }
            catch (Exception e) //logged
            {
                Logger.WriteException(e);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-view-main");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-view-body");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                ErrorView.RenderContentError(writer, e);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        private string GetString(string name)
        {
            return GetString("ContentView", name);
        }
        private string GetString(string className, string name)
        {
            return SenseNetResourceManager.Current.GetStringOrNull(className, name, System.Globalization.CultureInfo.CurrentUICulture);
        }

        private void SetContentViewFieldError()
        {
            if (this.ContentException == null)
                this.ContentException = new InvalidOperationException("Invalid data. See detailed error messages below.");
        }
    }
}
