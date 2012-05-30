using System;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.Controls;
using SenseNet.ContentRepository;
using System.Web.UI.WebControls;
using SenseNet.Search;
using Content = SenseNet.ContentRepository.Content;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.Portal.Portlets
{
    public class VotingPortlet : ContextBoundPortlet
    {
        #region Enums

        /// <summary>
        /// The list of the portlet modes.
        /// </summary>
        public enum PortletMode
        {
            ResultOnly, ResultAndVoting, VotingOnly, VoteAndGotoResult
        }

        /// <summary>
        /// The view orders in the ResultAndVoting porletmode.
        /// </summary>
        public enum ViewOrders
        {
            VotingViewFirst, ResutlViewFirst
        }

        #endregion

        #region Private variables
        /// <summary>
        /// Storing the current state of the portlet.
        /// </summary>
        private String _myState = "VotingView";

        /// <summary>
        /// The current content.
        /// </summary>
        private Content _currentContent;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets a value indicating whether this instance is result avalaible before.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is result avalaible before; otherwise, <c>false</c>.
        /// </value>
        public bool IsResultAvalaibleBefore
        {
            get
            {
                if (ContextNode.HasProperty("IsResultVisibleBefore"))
                {
                    return Convert.ToBoolean(ContextNode["IsResultVisibleBefore"]);
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [more filing is enabled].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [more filing is enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool MoreFilingIsEnabled
        {
            get
            {
                if (ContextNode.HasProperty("EnableMoreFilling"))
                {
                    return Convert.ToBoolean(ContextNode["EnableMoreFilling"]);
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the User has voted or not.
        /// </summary>
        /// <value><c>true</c> if [user voted]; otherwise, <c>false</c>.</value>
        private bool UserVoted
        {
            get
            {
                return ContentQuery.Query(string.Format("+Type:votingitem +CreatedById:{0} +InTree:\"{1}\" .AUTOFILTERS:OFF .COUNTONLY", User.Current.Id, ContextNode.Path)).Count > 0;
            }
        }

        #endregion

        #region Portlet properties

        /// <summary>
        /// Gets or sets the content view path.
        /// </summary>
        /// <value>The content view path.</value>
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebDisplayName("ContentView path")]
        [WebDescription("Enter the path of the ContentView")]
        [WebOrder(50)]
        public string ContentViewPath { get; set; }

        /// <summary>
        /// Gets or sets the voting portlet mode.
        /// </summary>
        /// <value>The voting portlet mode.</value>
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebDisplayName("Portlet mode")]
        [WebDescription("Select mode of the portlet")]
        [WebOrder(50)]
        public PortletMode VotingPortletMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable life span].
        /// </summary>
        /// <value><c>true</c> if [enable life span]; otherwise, <c>false</c>.</value>
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebDisplayName("Enable life span")]
        [WebDescription("Defines whether the portlet should check ValidFrom and ValidTill properties")]
        [WebOrder(50)]
        public bool EnableLifeSpan { get; set; }

        /// <summary>
        /// Gets or sets the views order.
        /// </summary>
        /// <value>The views order.</value>
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebDisplayName("Order of the views")]
        [WebDescription("Select which view should be displayed first in VoteAndResult mode")]
        [WebOrder(50)]
        public ViewOrders ViewsOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [foldering enabled].
        /// </summary>
        /// <value><c>true</c> if [foldering enabled]; otherwise, <c>false</c>.</value>
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebDisplayName("Enable foldering")]
        [WebDescription("Enables Voting Items' storings in folders")]
        [WebOrder(50)]
        public bool FolderingEnabled { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebDisplayName("Number of decimals in result")]
        [WebDescription("Defines the number of decimals that the result percentages will contain (it must be between 0 and 5, otherwise it will be 0)")]
        [WebOrder(50)]
        public int DecimalsInResult { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the VotingPortlet class and sets default values.
        /// </summary>
        public VotingPortlet()
        {
            this.Name = SenseNetResourceManager.Current.GetString("Voting", "PortletTitle");
            this.Description = SenseNetResourceManager.Current.GetString("Voting", "Description");
            this.Category = new PortletCategory(PortletCategoryType.Content);
            this.EnableLifeSpan = false;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Raises the PreRender event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            EnsureChildControls();
            base.OnPreRender(e);
        }

        /// <summary>
        /// Raises the Init event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        /// <summary>
        /// Restores control-state information from a previous page request that was saved by the SaveControlState method.
        /// </summary>
        protected override void LoadControlState(object savedState)
        {
            var mySavedState = savedState as object[];
            if (mySavedState != null)
            {
                _myState = mySavedState[1].ToString();
                base.LoadControlState(mySavedState[0]);
            }
        }

        /// <summary>
        /// Saves any server control state changes that have occurred since the time the page was posted back to the server.
        /// </summary>
        /// <returns>
        /// Returns the server control's current state. If there is no state associated with the control, this method returns null.
        /// </returns>
        protected override object SaveControlState()
        {
            return new[]
            {
                base.SaveControlState(),
                _myState
            };
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            try
            {
                var ctn = ContextNode;
            }
            catch (SenseNetSecurityException)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContextNodePermissionError") });
                return;
            }

            // Check ContextNode is not null
            if ((ContextNode as Voting) == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContextNodeError") });
                return;
            }

            try
            {
                _currentContent = Content.Create(ContextNode);
            }
            catch (Exception)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "BindingError") });
                return;
            }

            // Check portlets' field to wheter to check validTill and validFrom fields
            if (this.EnableLifeSpan)
            {
                DateTime validFrom = ContextNode.GetProperty<DateTime>("ValidFrom");
                DateTime validTill = ContextNode.GetProperty<DateTime>("ValidTill");

                if (validTill == DateTime.MinValue) validTill = DateTime.MaxValue;

                // Check if the voting is valid to fill it
                if (DateTime.Now < validFrom || DateTime.Now >= validTill)
                {
                    AddInvalidView();
                    return;
                }
            }

            // ResultOnly PorletMode
            if (VotingPortletMode == PortletMode.ResultOnly)
            {
                AddResultView();
            }
            // VotingOnly PorletMode
            else if (VotingPortletMode == PortletMode.VotingOnly)
            {
                switch (_myState)
                {
                    case "VotingView":
                        AddVotingView();
                        break;
                    case "ThankYouView":
                        AddThankYouView();
                        break;
                    default:
                        Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "VotingOnlyError") });
                        break;
                }
            }
            // VoteAndGotoResult PorletMode
            else if (VotingPortletMode == PortletMode.VoteAndGotoResult)
            {
                switch (_myState)
                {
                    case "VotingView":
                        if (MoreFilingIsEnabled || !UserVoted)
                        {
                            AddVotingView();
                        }
                        else
                        {
                            AddResultView();   
                        }

                        break;
                    case "ResultView":
                        AddResultView();
                        break;
                    case "ThankYouView":
                        AddThankYouView();
                        AddResultView();
                        break;
                    default:
                        Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "VoteAndGotoResultError") });
                        break;
                }
            }
            // ResultAndVoting PorletMode
            else if (VotingPortletMode == PortletMode.ResultAndVoting)
            {
                switch (_myState)
                {
                    case "VotingView":
                        if (ViewsOrder == ViewOrders.VotingViewFirst)
                        {
                            AddVotingView();
                            AddResultView();
                        }
                        else
                        {
                            AddResultView();
                            AddVotingView();
                        }
                        
                        break;

                    case "ThankYouView":
                        AddThankYouView();
                        break;

                    case "ResultView":
                        AddResultView();
                        break;

                    default:
                        Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ResultAndVotingError") });
                        break;
                }
            }
            
            ChildControlsCreated = true;
        }

        #endregion

        #region Methods

        // Voting View
        private void AddVotingView()
        {
            var votingNode = ContextNode as Voting;
            if (votingNode == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContextNodeError") });
                return;
            }

            if (votingNode.IsVotingAvailable)
            {
                if (votingNode.VotingPageContentView != null)
                {
                    var contentView = ContentView.Create(_currentContent, Page, ViewMode.Browse, votingNode.VotingPageContentView.Path);
                    if (contentView == null)
                    {
                        Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContentViewCreateError") });
                        return;
                    }

                    //add ContentView to the Controls collection
                    Controls.Add(contentView);

                    var qph = contentView.FindControlRecursive("QuestionPlaceHolder") as PlaceHolder;
                    if (qph == null)
                    {
                        Controls.Clear();
                        Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "PlaceHolderError") });
                        return;
                    }

                    var args = new object[0];

                    // Checks if Foldering is enabled and sets the parent depending on it
                    var parent = FolderingEnabled ? GetFolder(ContextNode) : ContextNode;
                    var votingItem = Content.CreateNew("VotingItem", parent, null, args);

                    // If there is no question in the Voting (question is avalaible on the Voting Items under the Voting)
                    var isQuestionExist = votingItem.Fields.Any(a => a.Value.Name.StartsWith("#"));
                    if (!isQuestionExist)
                    {
                        Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "NoQuestion") });
                        return;
                    }

                    var votingItemNewContentView = ContentView.Create(votingItem, Page, ViewMode.InlineNew);
                    if (votingItemNewContentView == null)
                    {
                        Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContentViewCreateError") });
                        return;
                    }
                    votingItemNewContentView.ID = "VotingItenContentView";
                    votingItemNewContentView.UserAction += VotingItemNewContentViewUserAction;

                    qph.Controls.Add(votingItemNewContentView);

                    if (!SecurityHandler.HasPermission(votingItemNewContentView.ContentHandler, PermissionType.AddNew))
                    {
                        var btn = votingItemNewContentView.FindControlRecursive("ActButton1") as Button;
                        if (btn != null) btn.Visible = false;
                    }

                    if (MoreFilingIsEnabled && IsResultAvalaibleBefore && VotingPortletMode == PortletMode.VoteAndGotoResult)
                    {
                        var lb = contentView.FindControl("VotingAndResult") as LinkButton;
                        if (lb == null)
                        {
                            Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "CannotFindLink") });
                            return;
                        }

                        lb.Text = SenseNetResourceManager.Current.GetString("Voting", "GoToResultLink");
                        lb.Click += LbClick;
                        lb.Visible = true;
                    }
                    return;
                }
                // No content view is set
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "NoContentView") });
            }
            else
            {
                // When the user can only Vote once
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "OnlyOneVoteError") });
            }
        }

        // Returns the folder (parent) as container of the VotingItems
        private Node GetFolder(Node contextNode)
        {
            var folderName = DateTime.Now.ToString("yyyy_MM_dd");
            var folderPath = RepositoryPath.Combine(ContextNode.Path, folderName);

            using (new SystemAccount())
            {
                // If Folder doesn't exists
                if (!Node.Exists(folderPath))
                {
                    var newFolder = Content.CreateNew("Folder", contextNode, folderName);
                    newFolder.Fields["DisplayName"].SetData(folderName);
                    newFolder.Fields["Name"].SetData(folderName);
                    newFolder.Save();
                    return newFolder.ContentHandler;
                }
                // Return existing Folder
                return Node.LoadNode(folderPath);
            }
        }

        // Result View
        private void AddResultView()
        {
            var votingNode = ContextNode as Voting;
            if (votingNode == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContextNodeError") });
                return;
            }

            if (!IsResultAvalaibleBefore && VotingPortletMode == PortletMode.ResultOnly)
            {
                var c = Content.Create(votingNode.CannotSeeResultContentView);
                if (c == null)
                {
                    Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContentCreateError") });
                    return;
                }

                var contentView = ContentView.Create(c, this.Page, ViewMode.Browse);
                if (contentView == null)
                {
                    Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContentViewCreateError") });
                    return;
                }
                Controls.Add(contentView);
            }
            else
            {
                if (votingNode.ResultPageContentView != null)
                {
                    var contentView = ContentView.Create(_currentContent, Page, ViewMode.InlineNew, votingNode.ResultPageContentView.Path) as VotingResultContentView;
                    
                    if (contentView == null)
                    {
                        Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContentViewCreateError") });
                        return;
                    }
                    
                    contentView.DecimalsInResult = this.DecimalsInResult;
                    Controls.Add(contentView);

                    if (MoreFilingIsEnabled && UserVoted && VotingPortletMode == PortletMode.VoteAndGotoResult)
                    {
                        var lb = contentView.FindControl("VotingAndResult") as LinkButton;
                        if (lb == null)
                        {
                            Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "CannotFindLink") });
                            return;
                        }
                        lb.Text = SenseNetResourceManager.Current.GetString("Voting", "GoToVotingLink");
                        lb.Click += MyLinkBClick;
                        lb.Visible = true;
                    }
                    return;
                }
                // When there is no result to show
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "NoResult") });
            }
        }

        // Thank You View
        private void AddThankYouView()
        {
            var votingNode = ContextNode as Voting;
            if (votingNode == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContextNodeError") });
                return;
            }

            if (votingNode.LandingPage == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "LandingPageError") });
                return;
            }
            var landingContent = Content.Create(votingNode.LandingPage);
            if (landingContent == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContentCreateError") });
                return;
            }

            var landingCv = ContentView.Create(landingContent, Page, ViewMode.Browse, votingNode.GetReference<Node>("LandingPageContentView") != null
                                                                                          ? votingNode.GetReference<Node>("LandingPageContentView").Path
                                                                                          : "/Root/Global/contentviews/WebContentDemo/Browse.ascx");
            if (landingCv == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContentViewCreateError") });
                return;
            }

            Controls.Add(landingCv);

            //if (MoreFilingIsEnabled && UserVoted && VotingPortletMode == PortletMode.VoteAndGotoResult)
            //{
            //    var lb = landingCv.FindControl("VotingAndResult") as LinkButton;
            //    if (lb == null)
            //    {
            //        Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "CannotFindLink") });
            //        return;
            //    }

            //    lb.Text = SenseNetResourceManager.Current.GetString("Voting", "GoToResultLink");
            //    lb.Click += LbClick;
            //    lb.Visible = true;
            //}
            
        }

        // Invalid View
        private void AddInvalidView()
        {
            var votingNode = ContextNode as Voting;
            if (votingNode == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContextNodeError") });
                return;
            }

            if (votingNode.InvalidSurveyPage == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "InvalidSurveyPageError") });
                return;
            }
            var landingContent = Content.Create(votingNode.InvalidSurveyPage);

            var landingCv = ContentView.Create(landingContent, Page, ViewMode.Browse, "/Root/Global/contentviews/WebContentDemo/Browse.ascx");
            if (landingCv == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Voting", "ContentViewCreateError") });
                return;
            }

            Controls.Add(landingCv);
        }

        #endregion

        #region EventHandlers

        // Adding voting view on VotingAndResult linkbutton click
        void MyLinkBClick(object sender, EventArgs e)
        {
            Controls.Clear();
            ChildControlsCreated = false;
            _myState = "VotingView";
        }

        void VotingItemNewContentViewUserAction(object sender, UserActionEventArgs e)
        {
            // If the button's action is not Save
            if (e.ActionName != "Save") return;

            if (!SecurityHandler.HasPermission(e.ContentView.ContentHandler, PermissionType.AddNew))
            {
                e.ContentView.ContentException = new Exception("You do not have the appropriate permissions to answer this question.");
                return;
            }

            e.ContentView.UpdateContent();
            if (e.ContentView.Content.IsValid)
            {
                e.ContentView.Content.Save();
                Controls.Clear();
                ChildControlsCreated = false;
                _myState = "ThankYouView";
            }
        }

        // Adding result view on VotingAndResult linkbutton click
        void LbClick(object sender, EventArgs e)
        {
            Controls.Clear();
            ChildControlsCreated = false;
            _myState = "ResultView";
        }

        #endregion
    }
}
