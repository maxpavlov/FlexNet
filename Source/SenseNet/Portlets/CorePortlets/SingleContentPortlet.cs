using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SNC = SenseNet.ContentRepository;
using System.Linq;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Portlets
{   
    public class SingleContentPortlet : CacheablePortlet
    {
        #region Validity checking
        public enum ValidationOption
        {
            /// <summary>
            /// Do not check validity.
            /// </summary>
            DontCheckValidity,
            /// <summary>
            /// Show all valid contents.
            /// </summary>
            ShowAllValid,
            /// <summary>
            /// Show valid but archived contents.
            /// </summary>
            ShowValidButArchived,
            /// <summary>
            /// Show valid contents that are not archived.
            /// </summary>
            ShowValidButNotArchived
        }

        internal class ValiditySetting
        {

            private string _validFromPropertyName = "ValidFrom";
            private string _reviewDatePropertyName = "ReviewDate";
            private string _archiveDatePropertyName = "ArchiveDate";
            private string _validTillPropertyName = "ValidTill";
            private DateTime _validFrom;
            private DateTime _reviewDate;
            private DateTime _archiveDate;
            private DateTime _validTill;
            private bool _isValid;
            private DateTime? _currentDateTime;

            public string ValidFromPropertyName
            {
                get { return _validFromPropertyName; }
                set { _validFromPropertyName = value; }
            }
            public string ReviewDatePropertyName
            {
                get { return _reviewDatePropertyName; }
                set { _reviewDatePropertyName = value; }
            }
            public string ArchiveDatePropertyName
            {
                get { return _archiveDatePropertyName; }
                set { _archiveDatePropertyName = value; }
            }
            public string ValidTillPropertyName
            {
                get { return _validTillPropertyName; }
                set { _validTillPropertyName = value; }
            }
            public DateTime ValidFrom
            {
                get { return _validFrom; }
                set { _validFrom = value; }
            }
            public DateTime ReviewDate
            {
                get { return _reviewDate; }
                set { _reviewDate = value; }
            }
            public DateTime ArchiveDate
            {
                get { return _archiveDate; }
                set { _archiveDate = value; }
            }
            public DateTime ValidTill
            {
                get { return _validTill; }
                set { _validTill = value; }
            }
            public bool IsValid
            {
                get { return _isValid; }
            }
            public DateTime? CurrentDateTime
            {
                get { return _currentDateTime; }
                set { _currentDateTime = value; }
            }

            public ValiditySetting() { }
            public ValiditySetting(DateTime validFrom, DateTime reviewDate, DateTime archiveDate, DateTime validTill)
            {
                _validFrom = validFrom;
                _reviewDate = reviewDate;
                _archiveDate = archiveDate;
                _validTill = validTill;
            }

            public bool ShowAllValidContent
            {
                get
                {
                    //ValidFrom < now < ValidTill
                    //if (DateTime.Compare(_validFrom, DateTime.MinValue) == 0 || DateTime.Compare(_validTill, DateTime.MinValue) == 0)
                    //    return true;

                    int isValidFromEmpty = DateTime.Compare(_validFrom, ActiveSchema.DateTimeMinValue);
                    int isValidTillEmpty = DateTime.Compare(_validTill, ActiveSchema.DateTimeMinValue);

                    if (isValidFromEmpty == -1 && isValidTillEmpty == -1)
                        return true;

                    if (isValidFromEmpty == -1 && isValidTillEmpty > 0)
                        return (DateTime.Compare(_currentDateTime.HasValue ? _currentDateTime.Value : DateTime.Now, _validTill) <= 0);

                    if (isValidFromEmpty > 0 && isValidTillEmpty == -1)
                        return (DateTime.Compare(_validFrom, _currentDateTime.HasValue ? _currentDateTime.Value : DateTime.Now) <= 0);

                    return (DateTime.Compare(_validFrom, _currentDateTime.HasValue ? _currentDateTime.Value : DateTime.Now) <= 0 && DateTime.Compare(_currentDateTime.HasValue ? _currentDateTime.Value : DateTime.Now, _validTill) <= 0);
                }
            }
            public bool ShowValidAndArchived
            {
                get
                {
                    // ArchiveDate < now < ValidTill
                    //if (DateTime.Compare(_archiveDate, DateTime.MinValue) == 0 || DateTime.Compare(_validTill, DateTime.MinValue) == 0)
                    //    return true;
                    return (DateTime.Compare(_archiveDate, _currentDateTime.HasValue ? _currentDateTime.Value : DateTime.Now) <= 0 && DateTime.Compare( _currentDateTime.HasValue ? _currentDateTime.Value : DateTime.Now,_validTill) <= 0);
                }
            }
            public bool ShowValidAndNotArchived
            {
                get
                {
                    //ValidFrom < now < ArchiveDate
                    //if (DateTime.Compare(_validFrom, DateTime.MinValue) == 0 || DateTime.Compare(_archiveDate, DateTime.MinValue) == 0)
                    //    return true;
                    return (DateTime.Compare(_validFrom, _currentDateTime.HasValue ? _currentDateTime.Value : DateTime.Now) <= 0 && DateTime.Compare(_currentDateTime.HasValue ? _currentDateTime.Value : DateTime.Now, _archiveDate) <= 0);
                }
            }
            public void Fill(SNC.Content content)
            {
                _isValid = true;

                DateTimeField validFromField = content.Fields[_validFromPropertyName] as DateTimeField;
                DateTimeField reviewDateField = content.Fields[_reviewDatePropertyName] as DateTimeField;
                DateTimeField archiveDateField = content.Fields[_archiveDatePropertyName] as DateTimeField;
                DateTimeField validTillField = content.Fields[_validTillPropertyName] as DateTimeField;
                
                if (_isValid)
                {
                    _validFrom = Convert.ToDateTime(validFromField.GetData());
                    _reviewDate = Convert.ToDateTime(reviewDateField.GetData());
                    _archiveDate = Convert.ToDateTime(archiveDateField.GetData());
                    _validTill = Convert.ToDateTime(validTillField.GetData());


                }
            }

        }
        #endregion

        #region Members
        private string _urlQueryKey = "Content";

        private const string ContainerPanelId = "_viewpanel";           // default asp.net container id postfix
        private const string SelectNewContentTypeMode = "SelectNew";    // select new contenttype function name

        private PlaceHolder _container; // container control which holds the child controls.

		private SNC.Content _content;           // Content instance
		private ContentView _contentView;   // ContentView which is linked with the Repository content.

		private string _contentPath;            // Repository path of the content
        private string _urlContentPath;         // contentpath comes from url
        private string _displayMode;            // stores the portlet displaymode
        private bool _recreateNewContentView;   // flag for recreating new contentview the next cycle
        private bool _recreateEditContentView;  // flag for recreating edit contentview the next cycle
        private string _errorMessage;

        private Button _newContentButton;
        private DropDownList _contentTypeNames;

		private string _contentPathOld = null;

        private ValiditySetting _validitySetting;

        //----------------------------------------------------------- Properties

        protected bool HasErrorInternal
        {
            get { return !String.IsNullOrEmpty(_errorMessage); }
        }
        /// <summary>
        /// Gets the global error text which will be displayed to the end user when some error(s) occures.
        /// </summary>
        protected string ErrorMessage
        {
            get { return _errorMessage; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Content type name")]
        [WebDescription("Default content type used in 'New content' and 'Edit content' scenarios")]
        [WebCategory(EditorCategory.SingleContentPortlet, EditorCategory.SingleContentPortlet_Order)]
        [WebOrder(10)]
        [Editor(typeof(DropDownPartField), typeof(IEditorPartField))]
        [DropDownPartOptions(DropDownCommonType.ContentTypeDropdown)]
        public string UsedContentTypeName { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Content path")]
        [WebDescription("Path of the content to be presented")]
        [WebCategory(EditorCategory.SingleContentPortlet, EditorCategory.SingleContentPortlet_Order)]
        [WebOrder(20)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions()]
        public string ContentPath
        {
            get { return _contentPath; }
			set
			{
				_contentPath = value;
				if (_contentPathOld == null)
				{
					_contentPathOld = value;
				}
			}
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Browse contentview path")]
        [WebDescription("Path of presented content's browse contentview. Leave empty to use default contentview resolution logic")]
        [WebCategory(EditorCategory.SingleContentPortlet, EditorCategory.SingleContentPortlet_Order)]
        [WebOrder(30)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
        public string ContentViewPath { get; set; }

        [WebBrowsable(true)] 
        [Personalizable(true)]
        [WebDisplayName("Edit contentview path")]
        [WebDescription("Path of presented content's edit contentview. Leave empty to use default contentview resolution logic")]
        [WebCategory(EditorCategory.SingleContentPortlet, EditorCategory.SingleContentPortlet_Order)]
        [WebOrder(40)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
        public string ContentViewEditPath { get; set; }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        [WebBrowsable(true)] 
        [Personalizable(true)]
        [WebDisplayName("Use url params to resolve content")]
        [WebDescription("If set to true content path can be given with 'Content' url query key")]
        [WebCategory(EditorCategory.SingleContentPortlet, EditorCategory.SingleContentPortlet_Order)]
        [WebOrder(50)]       
        public bool UseUrlPath { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("ContentPath url query key name")]
        [WebDescription("The url query key name that will be used when Content is resolved using url params")]
        [WebCategory(EditorCategory.SingleContentPortlet, EditorCategory.SingleContentPortlet_Order)]
        [WebOrder(55)]
        public string UrlQueryKey
        {
            get { return _urlQueryKey; }
            set { _urlQueryKey = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Content validity check")]
        [WebDescription("Validation check uses content's 'ValidFrom' and 'ValidTill' properties. Archive check uses 'ArchiveDate' property")]
        [WebCategory(EditorCategory.SingleContentPortlet, EditorCategory.SingleContentPortlet_Order)]
        [WebOrder(60)]       
        public ValidationOption ValidationSetting { get; set; }

        protected string RelativeContentPath
        {
            get
            {
                if (_urlContentPath != null)
                    return (_urlContentPath);
                else if (_contentPath != null)
                    return (_contentPath);
                else return ("");
            }
        }
        protected string AbsoulteContentPath
        {
            get
            {
                if (_urlContentPath != null)
                {
                    if (_urlContentPath.StartsWith("/"))
                        return (_urlContentPath);
                    else if (SenseNet.Portal.Page.Current != null)
                        return (String.Concat(SenseNet.Portal.Page.Current.Path, "/", _urlContentPath));
                    else return ("");
                }
                else if (_contentPath != null)
                {
                    if (_contentPath.StartsWith("/"))
                        return (_contentPath);
                    else if (SenseNet.Portal.Page.Current != null)
                        return (String.Concat(SenseNet.Portal.Page.Current.Path, "/", _contentPath));
                    else return ("");
                }
                else
                    return ("");
            }
        }
        protected bool IsUrlAbsoulte
        {
            get
            {
                if (_urlContentPath != null)
                {
                    return (_urlContentPath.StartsWith("/"));
                }
                else if (_contentPath != null)
                {
                    return (_contentPath.StartsWith("/"));
                }
                else return (true);
            }
        }

        #endregion

        // Consturtor /////////////////////////////////////////////////////////////
        public SingleContentPortlet() 
        {

            this.Name = "Single content";
            this.Description = "An easy way to display a piece of Content using a Content View.";
            this.Category = new PortletCategory(PortletCategoryType.Content);

            this._recreateNewContentView = false;
            this._recreateEditContentView = false;
            this._displayMode = GetViewModeName(ViewMode.Browse);
        }

        // Events /////////////////////////////////////////////////////////////////

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }
        protected override object SaveControlState()
        {
            object[] state = new object[4];
            state[0] = base.SaveControlState();
            state[1] = this._recreateNewContentView;
            state[2] = this._recreateEditContentView;
            state[3] = this._displayMode;

            return state;
        }
        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                var state = savedState as object[];
                if (state != null && state.Length == 4)
                {
                    base.LoadControlState(state[0]);

                    SetControlCreationFlags(state);

                    CreateSelectNewControlsIfNeed();
                        
                    RecreateNewContentViewIfNeed();

                    RecreateEditContentViewIfNeed();

                }
            } else
                base.LoadControlState(savedState);
        }

        //public override void NotifyCheckin()
        //{
        //    base.NotifyCheckin();
        //    if (_content != null && _content.ContentHandler != null &&_content.ContentHandler.Path != null)
        //    {
        //        PathDependency.FireChanged(_content.ContentHandler.Path);
        //    }
        //}
        public override void AddPortletDependency()
        {
            base.AddPortletDependency();

            if (_contentPath != null)
            {
                var nodeHead = NodeHead.Get(_contentPath);
                if (nodeHead != null)
                {
                    var nodeDependency = CacheDependencyFactory.CreateNodeDependency(nodeHead);
                    Dependencies.Add(nodeDependency);
                }
            }
        }
        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            using (var traceOperation = Logger.TraceOperation("SingleContentPortlet.CreateChildControls", this.Name))
            {
                Controls.Clear();

                if (this.HasErrorInternal)
                {
                    ShowSimpleErrorMessage();
                    return;
                }

                try
                {
                    if (this.UseUrlPath)
                        ProcessUrl();
                    else
                        TryLoadFirstContent();  // return value is not used.

                    if (IsCreateSelectNewControls())
                        this._displayMode = SelectNewContentTypeMode;

                    if (!String.IsNullOrEmpty(this._contentPath))
                    {
                        ShowInlineNewContentViewIfNeed();
                        ShowInlineEditContentViewIfNeed();
                        ShowBrowseContentViewIfNeed();
                    }
                    else
                    {
                        ShowBrowseContentView();
                        ShowInlineNewContentView();
                        CreateSelectNewControlsIfNeed();
                    }

                    if (_container != null)
                        this.Controls.Add(_container);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                    this._errorMessage = ex.Message;
                }

                ChildControlsCreated = true;
                traceOperation.IsSuccessful = true;
            }
        }
        
        public void _contentView_UserAction(object sender, UserActionEventArgs e)
        {
            if (e.ActionName == "save")
            {
				e.ContentView.UpdateContent();
				if (e.ContentView.IsUserInputValid && _content.IsValid)
                {
                    string t = RepositoryPath.Combine(_content.ContentHandler.ParentPath, _content.Fields["Name"].GetData() as string);
                    //PathInfoRemove:
                    //if (RepositoryPathInfo.GetPathInfo(t) == null)
                    if (NodeHead.Get(t) == null)
                    {
                        _content.Save();
                        ProcessUserAction();
                    }
                    else
                    {
                        if (_content.ContentHandler.Id > 0)
                        {
                            _content.Save();
                            ProcessUserAction();
                        }
                        else
                        {
                            this._recreateEditContentView = false;
                            this._recreateNewContentView = false;
                            this._displayMode = GetViewModeName(ViewMode.Browse);
                            this._errorMessage = HttpContext.GetGlobalResourceObject("SingleContentPortlet", "ContentNameAlreadyExists") as string;
                        }
                    }
                }
                else
                {
                    this._recreateNewContentView = true;
                    _content.DontSave();
                }

            }
            else if (e.ActionName == "cancel")
            {
                _recreateNewContentView = false;
                _recreateEditContentView = false;
                _displayMode = GetViewModeName(ViewMode.Browse);
                _container.Controls.Clear();
                CreateChildControls();
            }
            else
            {
                this._recreateNewContentView = false;
                this._recreateEditContentView = false;
                _container.Controls.Clear();
            }
        }
        public void _newContentButton_Click(object sender, EventArgs e)
        {
            // indicates that contentview recreation process is required in the next cycle           
            this.UsedContentTypeName= _contentTypeNames.SelectedValue;
            ProcessNewContent();
        }        
        
        // Internals /////////////////////////////////////////////////////////////////
        private void ShowBrowseContentViewIfNeed()
        {
            if (this._displayMode != WebPartManager.BrowseDisplayMode.Name) 
                return;
            BuildContentView(ViewMode.Browse);
        }
        private void ShowBrowseContentView()
        {
            if (this._displayMode != GetViewModeName(ViewMode.Browse) || String.IsNullOrEmpty(this._urlContentPath))
                return;
            BuildContentView(ViewMode.Browse);
        }
        private void ShowInlineEditContentViewIfNeed()
        {
            if (this._displayMode != WebPartManager.EditDisplayMode.Name) 
                return;
            BuildContentView(ViewMode.InlineEdit);
        }
        private void ShowInlineNewContentViewIfNeed()
        {
            if (this._displayMode != GetViewModeName(ViewMode.InlineNew) ||
                String.IsNullOrEmpty(this.UsedContentTypeName)) 
                return;
            this._contentPath = string.Empty;
            this.ContentViewEditPath = string.Empty;
            this.ContentViewPath = string.Empty;
            BuildContentView(this.UsedContentTypeName, SenseNet.Portal.Page.Current, ViewMode.InlineNew);
        }
        private void ShowInlineNewContentView()
        {
            if (this._displayMode != GetViewModeName(ViewMode.InlineNew)) 
                return;
            if (String.IsNullOrEmpty(this.UsedContentTypeName) || this._recreateNewContentView) 
                return;
            BuildContentView(this.UsedContentTypeName, SenseNet.Portal.Page.Current, ViewMode.InlineNew);
        }
        private void RecreateEditContentViewIfNeed()
        {
            if (!this._recreateEditContentView) 
                return;
            this._displayMode = GetViewModeName(ViewMode.Edit);
            this._recreateEditContentView = false;
            CreateChildControls();
        }
        private void RecreateNewContentViewIfNeed()
        {
            if (!this._recreateNewContentView)
                return;
            this._displayMode = GetViewModeName(ViewMode.InlineNew);
            this._recreateNewContentView = false;
            CreateChildControls();
        }
        private void SetControlCreationFlags(object[] state)
        {
            if (state[1] != null)
                this._recreateNewContentView = (bool)state[1];

            if (state[2] != null)
                this._recreateEditContentView = (bool)state[2];

            if (state[3] != null)
                this._displayMode = (string)state[3];
        }        
        private bool IsCreateSelectNewControls()
        {
            return (String.IsNullOrEmpty(this._contentPath) 
                && String.IsNullOrEmpty(this.UsedContentTypeName) 
                && this._displayMode != GetViewModeName(ViewMode.Browse) 
                && this._displayMode != GetViewModeName(ViewMode.Edit));
        }
        private void CreateSelectNewControlsIfNeed()
        {
            if (this._displayMode != SelectNewContentTypeMode)
                return;

            if (this._container == null)
                CreateContainer();
                
            _contentTypeNames = new DropDownList();
            _contentTypeNames.ID = "ContentTypeNames";

            var result = DropDownPartField.GetWebContentTypeList();
            if (result.Count > 0)
            {
                IEnumerable<Node> nodes = result.Nodes;
                foreach(var node in nodes)
                {
                    var contentTypeName = node.Name;
                    _contentTypeNames.Items.Add(new ListItem(contentTypeName, contentTypeName));
                }
            }

            _newContentButton = new Button();
            _newContentButton.ID = "NewContentButton";
            _newContentButton.Text = HttpContext.GetGlobalResourceObject("SingleContentPortlet", "CreateNewContent") as string;;
            _newContentButton.Click += new EventHandler(_newContentButton_Click);

            _container.Controls.Clear();
            _container.Controls.Add(_contentTypeNames);
            _container.Controls.Add(_newContentButton);
        }

        public static SingleContentPortlet GetContainingSingleContentPortlet(Control child)
        {
            SingleContentPortlet ancestor = null;

            while ((child != null) && ((ancestor = child as SingleContentPortlet) == null))
            {
                child = child.Parent;
            }

            return ancestor;
        }

        public static Node GetContextNodeForControl(Control c)
        {
            var ancestor = GetContainingSingleContentPortlet(c);

            if (ancestor != null)
            {
                var path = ancestor.ContentPath;
                if (!string.IsNullOrEmpty(path))
                    return Node.LoadNode(path);
            }
            
            return null;
        }
        
        private void CreateContainer()
        {
            _container = new PlaceHolder {ID = ContainerPanelId};
            this.Controls.Add(_container);
        }
        private bool TryLoadFirstContent()
        {
            var result = false;
            
            if (String.IsNullOrEmpty(this._contentPath) && 
                !String.IsNullOrEmpty(this.UsedContentTypeName) && 
                this._displayMode != GetViewModeName(ViewMode.InlineNew))
            {
                try
                {
					//string currentRepositoryPath = SenseNet.Portal.Page.Current.Path;
                    var query = new NodeQuery();
                    query.Add(new TypeExpression(ActiveSchema.NodeTypes[this.UsedContentTypeName], true));
                    query.Add(new IntExpression(IntAttribute.ParentId, ValueOperator.Equal, SenseNet.Portal.Page.Current.Id));
                    
                    var queryResult = query.Execute().Nodes.ToList();

                    if (queryResult != null && queryResult.Count > 0)
                    {
                        var nodeComparer = new NodeComparer<Node>();
                        queryResult.Sort(nodeComparer);

                        this._contentPath = queryResult[0].Path;
                        this._displayMode = GetViewModeName(ViewMode.Browse);

                        result = true;
                    }
                }
                catch (Exception exc) //logged
                {
                    Logger.WriteException(exc);
                }
            }
            return result;
        }
        private void BuildContentView(ViewMode viewMode)
        {
            if (viewMode == ViewMode.InlineNew)
                throw new InvalidOperationException("You cannot use New mode without giving contenttype name and parent node parameters.");

            BuildContentView(string.Empty, null, viewMode);
        }
        private void BuildContentView(string contentTypeName, Node parent, ViewMode viewMode)
        {
            var viewPath = string.Empty;

            #region creates container)
            if (_container == null)
                CreateContainer();
            else
            {
                _container.Controls.Clear();
                //this.Controls.Remove(_container);
            }
            
            if (this._container.Parent == null) this.Controls.Add(this._container);
            #endregion

            #region creates new content or loads an existing one
            // when we are in InlineNew creates an empty content
            if (viewMode == ViewMode.InlineNew)
                _content = SNC.Content.CreateNew(contentTypeName, parent, string.Empty);

            // when the portlet is in browse, edit, inlineedit states, loads the content
            if (viewMode == ViewMode.Browse ||
                viewMode == ViewMode.Edit ||
                viewMode == ViewMode.InlineEdit)
            {
                
                Node node = Node.LoadNode(this.AbsoulteContentPath);

                // check if can select a single node (ie smartfolderex)
                ICustomSingleNode singleNode = node as ICustomSingleNode;

                // select single node
                if (singleNode != null)
                    node = singleNode.SelectedItem();

                if (node == null)
                    _content = null;
                else
                    _content = SNC.Content.Create(node);

                //_content = SNC.Content.Load(this.AbsoulteContentPath);
            }

            #endregion

            if (viewMode == ViewMode.InlineEdit || viewMode == ViewMode.Edit)
                this._displayMode = GetViewModeName(ViewMode.Edit);

            // if content does not exist stop creating controls.
            if (_content == null)
            {               
               this._errorMessage = String.Format(HttpContext.GetGlobalResourceObject("SingleContentPortlet", "PathNotFound") as string,AbsoulteContentPath);
               return;
            }

            #region checks validity
            // if content is not valid, exit the method, and an empty contol will be shown to the user.

            if (User.Current.Id == User.Visitor.Id)
            {
                if (this.ValidationSetting != ValidationOption.DontCheckValidity)
                {
                    _validitySetting = GetValiditySetting(_content);
                    if (_validitySetting != null)
                    {
                        // User has been set the ValidationSetting,
                        // and checks the content will be displayed or not.
                        // if the content is not visible, just return (and don't do anything else)
                        // otherwise the content processing will be going on.
                        switch (this.ValidationSetting)
                        {
                            case ValidationOption.ShowAllValid:
                                if (!_validitySetting.ShowAllValidContent)
                                    return;
                                break;
                            case ValidationOption.ShowValidButArchived:
                                if (!_validitySetting.ShowValidAndArchived)
                                    return;
                                break;
                            case ValidationOption.ShowValidButNotArchived:
                                if (!_validitySetting.ShowValidAndNotArchived)
                                    return;
                                break;
                            case ValidationOption.DontCheckValidity:    // not used
                            default:
                                break;
                        }


                    }
                }
            }
            #endregion

            
            viewPath = GetViewPath();


            // try to create ContentView which contains all webcontrols of the content
            try
            {
                if (this.UseUrlPath )
                    _contentView = ContentView.Create(_content, this.Page, viewMode, String.IsNullOrEmpty(this.ContentViewPath) ? viewPath : this.ContentViewPath);
                else
                    _contentView = ContentView.Create(_content, this.Page, viewMode, viewPath);
                
                _container.Controls.Remove(_contentView);
            }
            catch (Exception e) //logged
            {
                Logger.WriteException(e);
                this._errorMessage = String.Format("Message: {0} {1} Source: {2} {3} StackTrace: {4} {5}", e.Message, Environment.NewLine, e.Source, Environment.NewLine, e.StackTrace, Environment.NewLine);
                return;
            }

            _contentView.UserAction += new EventHandler<UserActionEventArgs>(_contentView_UserAction);
            _container.Controls.Add(_contentView);

        }
        private string GetViewPath()
        {
            // creates the ContentViewPath based upon the portlet displaymode setting.
            string viewPath;
            if (this._displayMode == WebPartManager.EditDisplayMode.Name)
                viewPath = GetViewRepositoryPath(GetViewModeName(ViewMode.InlineEdit));
            else if (this._displayMode == GetViewModeName(ViewMode.Browse))
                viewPath = GetViewRepositoryPath(GetViewModeName(ViewMode.Browse));
            else if (this._displayMode == GetViewModeName(ViewMode.InlineNew))
                viewPath = GetViewRepositoryPath(GetViewModeName(ViewMode.InlineNew));
            else
                viewPath = GetViewRepositoryPath(GetViewModeName(ViewMode.Browse));
            return viewPath;
        }
		private ValiditySetting GetValiditySetting(SNC.Content content)
		{
			ValiditySetting result = new ValiditySetting();

			//if (!String.IsNullOrEmpty(this.ValidFromPropertyName))
			//    result.ValidFromPropertyName = this.ValidFromPropertyName;
			//if (!String.IsNullOrEmpty(this.ValidTillPropertyName))
			//    result.ValidTillPropertyName = this.ValidTillPropertyName;

			// try to fetch PreviewDate value
			if (content.Security.HasPermission(SenseNet.ContentRepository.Storage.Schema.PermissionType.OpenMinor))
				result.CurrentDateTime = GetPreviewDate();

			// reads property values
			result.Fill(content);

			// if error has been occured while parsing property values, returns null.
			if (!result.IsValid)
				return null;

			return result;
		}
        private DateTime? GetPreviewDate()
        {
            DateTime? result = null;

            string urlCurrentDateTime = HttpContext.Current.Request.Params["TimeMachineDate"] as String;
            if (String.IsNullOrEmpty(urlCurrentDateTime))
                return null;

            DateTime currDate = DateTime.MinValue;
            if (DateTime.TryParse(urlCurrentDateTime, out currDate))
                result = currDate;

            return result;
        }
        private string GetViewModeName(ViewMode viewMode)
        {
            switch (viewMode)
            {
                case ViewMode.None:
                    return Enum.GetName(typeof(ViewMode), ViewMode.None);
                case ViewMode.Edit:
                    return Enum.GetName(typeof(ViewMode), ViewMode.Edit);
                case ViewMode.New:
                    return Enum.GetName(typeof(ViewMode), ViewMode.New);
                case ViewMode.Browse:
                    return Enum.GetName(typeof(ViewMode), ViewMode.Browse);
                case ViewMode.InlineEdit:
					return Enum.GetName(typeof(ViewMode), ViewMode.InlineEdit);
                case ViewMode.Grid:
                    return Enum.GetName(typeof(ViewMode), ViewMode.Grid);
                case ViewMode.Query:
                    return Enum.GetName(typeof(ViewMode), ViewMode.Query);
                case ViewMode.InlineNew:
                    return Enum.GetName(typeof(ViewMode), ViewMode.InlineNew);
                default:
                    return string.Empty;
            }
        }      
        private string GetViewRepositoryPath(string viewName)
        {
            string result = string.Empty;

            if (viewName == "InlineEdit")
                result = this.ContentViewEditPath;
            else if (viewName == "Browse")
                result = this.ContentViewPath;

            if (String.IsNullOrEmpty(result))
            {
                // this is a skin-relative path, contentview.create will resolve it to skin and type
                result = Repository.ContentViewFolderName;
            }

            return result;
        }
        private void ShowSimpleErrorMessage()
        {
            this.Controls.Clear();
            var l = new Label {Text = _errorMessage};
            this.Controls.Add(l);
        }
        private void ProcessUserAction()
        {
            this._contentPath = _content.ContentHandler.Path;
            this.ContentViewPath = GetViewRepositoryPath(GetViewModeName(ViewMode.Browse));
            this.ContentViewEditPath = GetViewRepositoryPath(GetViewModeName(ViewMode.InlineEdit));
            this._displayMode = GetViewModeName(ViewMode.Browse);

            CreateChildControls();

            this._recreateEditContentView = false;
            this._recreateNewContentView = false;
        }
        private void ProcessUrl()
        {
            string urlQuery = HttpContext.Current.Request.Url.Query;
            if (HttpContext.Current.Request.Params[_urlQueryKey] != null)
            {
                this._urlContentPath = HttpContext.Current.Request.Params[_urlQueryKey].ToString();
            }
            else if (urlQuery.StartsWith("?"))
                this._urlContentPath = urlQuery.Replace("?", "").Replace("%20", " ");
        }
        private void ProcessNewContent()
        {
            if (!String.IsNullOrEmpty(this.UsedContentTypeName))
            {
                this._displayMode = GetViewModeName(ViewMode.InlineNew);
                this.CreateChildControls();
                this._recreateEditContentView = false;
                this._recreateNewContentView = true;
            }
            else
            {
                this._displayMode = SelectNewContentTypeMode;
                this.CreateChildControls();
            }
        }
        
        // Verbs /////////////////////////////////////////////////////////////////
        
        public override WebPartVerbCollection Verbs
        {
            get
            {
                if (String.IsNullOrEmpty(this._displayMode))
                    this._displayMode = GetViewModeName(ViewMode.Browse);

                WebPartVerb editBrowseVerb = null;
                WebPartVerb newContentVerb = null;

                if (this._displayMode == GetViewModeName(ViewMode.Browse) ||
                    this._displayMode == GetViewModeName(ViewMode.InlineNew) ||
                    this._displayMode == SelectNewContentTypeMode)
                    editBrowseVerb = CreateSwitchToEditModeVerb();

                if (this._displayMode == GetViewModeName(ViewMode.Edit) ||
                    this._displayMode == GetViewModeName(ViewMode.InlineNew) ||
                    this._displayMode == SelectNewContentTypeMode)
                    editBrowseVerb = CreateSwitchToBrowseModeVerb();

                newContentVerb = CreateNewContentVerb();

                return new WebPartVerbCollection(new WebPartVerb[] { editBrowseVerb, newContentVerb });
            }
        }
        private WebPartVerb CreateSwitchToEditModeVerb()
        {
            WebPartVerb result = new WebPartVerb("SwitchToEditMode", new WebPartEventHandler(OnSwitchToEditAction));
            result.Text = HttpContext.GetGlobalResourceObject("SingleContentPortlet", "EditContent") as string;

            return result;
        }
        private WebPartVerb CreateSwitchToBrowseModeVerb()
        {
            WebPartVerb result = new WebPartVerb("SwitchToBrowseMode", new WebPartEventHandler(OnSwitchToBrowseAction));
            result.Text = HttpContext.GetGlobalResourceObject("SingleContentPortlet", "BrowseContent") as string;

            return result;
        }
        private WebPartVerb CreateNewContentVerb()
        {
            WebPartVerb result = new WebPartVerb("NewContent", new WebPartEventHandler(OnNewContent));
            result.Text = HttpContext.GetGlobalResourceObject("SingleContentPortlet", "NewContent") as string;
            
            return result;
        }
        protected void OnSwitchToBrowseAction(object sender, WebPartEventArgs e)
        {
            this._displayMode = GetViewModeName(ViewMode.Browse);
            this.CreateChildControls();
            this._recreateEditContentView = false;
            this._recreateNewContentView = false;
        }
        protected void OnSwitchToEditAction(object sender, WebPartEventArgs e)
        {
            this._displayMode = WebPartManager.EditDisplayMode.Name;
            this.CreateChildControls();
            this._recreateEditContentView = true;
            this._recreateNewContentView = false;
        }
        protected void OnNewContent(object sender, WebPartEventArgs e)
        {
            ProcessNewContent();
        }
    }
}
