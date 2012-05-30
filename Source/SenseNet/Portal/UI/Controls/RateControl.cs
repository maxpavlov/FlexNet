using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;

[assembly: WebResource("SenseNet.Portal.UI.Controls.RateControl.js", "application/x-javascript")]

namespace SenseNet.Portal.UI.Controls
{
    [DefaultProperty("Number")]
    [ToolboxData("<{0}:RateControl ID=\"RateControl\" runat=\"server\"></{0}:RateControl>")]
    public class RateControl : Panel, IScriptControl, INamingContainer
    {
        #region RateControlMode enum

        public enum RateControlMode
        {
            Normal,
            Admin,
            ReadOnly
        }

        #endregion

        private int? _contentId;
        private string _contentPath;
        private Control _hover;
        private CheckBox _reset;
        private RadioButtonList _stars;
        private VoteData _voteData = VoteData.CreateVoteData(5, 1);

        [PersistenceMode(PersistenceMode.Attribute)]
        public int ContentId
        {
            get
            {
                if (_contentId.HasValue)
                    return _contentId.Value;
                if (string.IsNullOrEmpty(_contentPath))
                    return 0;
                var n = NodeHead.Get(_contentPath);
                _contentId = n.Id;
                return _contentId.Value;
            }
            set
            {
                _contentId = value;
                _contentPath = null;
            }
        }

        [PersistenceMode(PersistenceMode.Attribute)]
        public string ContentPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_contentPath))
                    return _contentPath;
                if (!_contentId.HasValue)
                    return string.Empty;
                var n = NodeHead.Get(_contentId.Value);
                _contentPath = n.Path;
                return _contentPath;
            }
            set
            {
                _contentPath = value;
                _contentId = null;
            }
        }

        [PersistenceMode(PersistenceMode.Attribute)]
        public RateControlMode Mode { get; set; }

        [PersistenceMode(PersistenceMode.Attribute)]
        public string Votes
        {
            get { return _voteData.Serialize(); }
            set { _voteData = VoteData.CreateVoteData(value); }
        }

        public VoteData VotesArray
        {
            set { _voteData = value; }
            get
            {
                _voteData.SelectedValue = SelectedValue;
                return _voteData;
            }
        }

        public decimal AverageRate
        {
            get { return _voteData.AverageRate; }
        }

        [PersistenceMode(PersistenceMode.Attribute)]
        [DefaultValue(5)]
        public int MaxVotes
        {
            get { return _voteData.MaxVotes; }
            set { _voteData = VoteData.CreateVoteData(value, SplitVote); }
        }

        [PersistenceMode(PersistenceMode.Attribute)]
        [DefaultValue(1)]
        public int SplitVote
        {
            get { return _voteData.Split; }
            set { _voteData = VoteData.CreateVoteData(MaxVotes, value); }
        }

        [PersistenceMode(PersistenceMode.Attribute)]
        [DefaultValue(true)]
        public bool Enable { get; set; }

        [PersistenceMode(PersistenceMode.Attribute)]
        [DefaultValue(true)]
        public bool EnableHoverPanel = true;

        private bool _enableGroupSplit;

        [PersistenceMode(PersistenceMode.Attribute)]
        [DefaultValue(true)]
        public bool EnableGroupSplit
        {
            get { return _enableGroupSplit; }
            set
            {
                _enableGroupSplit = value;
                _voteData.EnableGrouping = _enableGroupSplit;
            }
        }


        public int? SelectedValue
        {
            get
            {
                if (_reset != null && _reset.Checked)
                    return -1;
                if (_stars == null)
                    return null;
                if (string.IsNullOrEmpty(_stars.SelectedValue))
                    return null;
                return Convert.ToInt32(_stars.SelectedValue);
            }
        }

        protected virtual bool IsAdmin
        {
            get { return Group.Administrators.Members.Contains(User.Current as User); }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            UITools.AddStyleSheetToHeader(UITools.GetHeader(), "$skin/styles/SN.Rating.css");
            UITools.AddScript("$skin/scripts/sn/SN.Rating.js");
            if (Enable)
            {
                if (ValidateParams())
                {
                    switch (Mode)
                    {
                        case RateControlMode.Admin:
                            BuildAdminMode();
                            break;
                        case RateControlMode.Normal:
                            BuildNormalMode();
                            break;
                        case RateControlMode.ReadOnly:
                            BuildReadOnlyMode();
                            break;
                        default:
                            throw new NotSupportedException("Not supported this mode");
                    }
                }
                else
                {
                    BuildErrorParams();
                }
            }
            else
            {
                BuildEmptyMode();
            }
            this.ChildControlsCreated = true;
        }

        protected virtual bool ValidateParams()
        {
            return true;
        }

        protected virtual void BuildNormalMode()
        {
            InitStarsControl();
            this.Controls.Add(_stars);

            if (EnableHoverPanel)
            {
                InitHoverPanelControl();
                this.Controls.Add(_hover);
            }
            _stars.Style.Add("display", "none");
        }

        protected virtual void BuildEmptyMode()
        {
            this.Controls.Add(new Label { Text = "Rating not enable!" });
        }

        protected virtual void BuildAdminMode()
        {
            if (IsAdmin)
            {
                InitResetControl();
                InitStarsControl();

                this.Controls.Add(_stars);
                this.Controls.Add(_reset);
            }
            else
            {
                BuildNormalMode();
            }
        }

        protected virtual void BuildErrorParams()
        {
            this.Controls.Add(new Label { Text = "Max rate item changed...." });
            if (IsAdmin)
            {
                InitResetControl();
                this.Controls.Add(_reset);
            }
        }

        protected virtual void BuildReadOnlyMode()
        {
            BuildNormalMode();
        }

        protected void InitStarsControl()
        {
            _stars = new RadioButtonList();
            _stars.RepeatLayout = RepeatLayout.Table;
            _stars.ID = "Stars";

            for (int i = 0; i < _voteData.PercentageVotes.Count; i++)
            {
                _stars.Items.Add(
                    new ListItem(string.Format("{0}: {1} %", i + 1, _voteData.PercentageVotes[i]), (i + 1).ToString()));
            }
        }

        protected void InitResetControl()
        {
            _reset = new CheckBox { Text = "Reset rating" };
        }

        protected void InitHoverPanelControl()
        {
            _hover = Page.LoadControl("/Root/System/SystemPlugins/Rating/RatingHoverPanel.ascx");
            _hover.ID = "hover";

            var rhp = _hover as RatingHoverPanel;
            if (rhp != null)
            {
                rhp.RateDs = _voteData.HoverPanelData;
            }
        }

        #region IScriptControl Renders

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var scriptRef = new ScriptReference { Path = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SenseNet.Portal.UI.Controls.RateControl.js") };
            return new[] { scriptRef };
        }

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            var scriptDescriptor = new ScriptControlDescriptor(typeof(RateControl).FullName, this.ClientID);

            scriptDescriptor.AddProperty("ContentId", this.ContentId);
            scriptDescriptor.AddProperty("StarsId", _stars == null ? string.Empty : _stars.ClientID);
            scriptDescriptor.AddProperty("HoverPanelId", _hover == null ? string.Empty : _hover.ClientID);
            scriptDescriptor.AddProperty("IsReadOnly", Mode == RateControlMode.ReadOnly);
            scriptDescriptor.AddProperty("RateValue", _voteData);

            return new ScriptDescriptor[] { scriptDescriptor };
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!this.DesignMode)
                if (!(IsAdmin && Mode == RateControlMode.Admin))
                    ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!this.DesignMode)
                if (!(IsAdmin && Mode == RateControlMode.Admin))
                    ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
            base.Render(writer);
        }

        #endregion
    }


    public class StarVotesController : System.Web.Mvc.Controller
    {
        public JsonResult Rate(int? id, int? vote, bool isGrouping)
        {
            VoteData newVote = VoteData.CreateVoteData(5, 1);
            try
            {
                bool isOK = false;

                if (id != null && vote != null)
                {
                    using (new SystemAccount())
                    {
                        var content = ContentRepository.Content.Load(id.Value);
                        if (content != null)
                        {
                            newVote.SelectedValue = vote.Value;
                            content.Fields["Rate"].SetData(newVote);
                            if (content.IsValid)
                            {
                                try
                                {
                                    content.Save();
                                    isOK = true;
                                }
                                catch (Exception ex) //logged
                                {
                                    Logger.WriteException(ex);
                                    newVote.ErrorMessage = "You can not vote right now!";
                                }
                            }
                            newVote = content.Fields["Rate"].OriginalValue as VoteData;
                            newVote.EnableGrouping = isGrouping;
                        }
                    }
                }
                newVote.Success = isOK;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                newVote.Success = false;
            }
            return Json(newVote);
        }
    }
}