using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using System.Data;
using System.Web;

namespace SenseNet.Portal.Portlets.Controls
{
    /// <summary>
    /// Controller class of Tag Admin Portlet.
    /// </summary>
    public class TagAdminControl : UserControl
    {
        /// <summary>
        /// Instance of Add tag button.
        /// </summary>
        private Button _btnAddTag;

        /// <summary>
        /// Instance of add text textbox.
        /// </summary>
        private TextBox _tbAddTag;

        /// <summary>
        /// Instance of Syncronize button.
        /// </summary>
        protected Button _btnImport;

        /// <summary>
        /// Instsance of Import label.
        /// </summary>
        protected Label _lblImport;

        /// <summary>
        /// Instance of the listview.
        /// </summary>
        private ListView _lv;

        /// <summary>
        /// Property for path of tags in Content Repository.
        /// </summary>
        /// <remarks>Gets or sets path of tags in Content Repository.</remarks>
        public string TagPath { get; set; }

        /// <summary>
        /// Gets or sets the black list paths.
        /// </summary>
        /// <value>The black list paths.</value>
        public List<string> SearchPaths { get; set; }

        /// <summary>
        /// List for storing tags coming from Lucene.
        /// </summary>
        private List<string> _allTags;

        /// <summary>
        /// List for storing tags in Content Repository.
        /// </summary>
        private List<string> _tagsInRepository;

        /// <summary>
        /// Overridden OnInit method.
        /// </summary>
        /// <param name="e">EventArg parameter.</param>
        /// <remarks>
        /// It initializes _allTags and _tagsInRepository variables.
        /// Sets Syncronization label's text the correct value based on
        /// _allTags and _tagsInRepository variables.
        /// 
        /// It sets instances of Add Tag button and Add Tag textbox.
        /// </remarks>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SetDataSource();

            //comparing tags in lucene & repository
            var tagsToImport = 0;
            var tagsToPurge = 0;
            foreach (var tag in _allTags)
            {
                if (!_tagsInRepository.Contains(tag))
                {
                    tagsToImport += 1;
                }
            }
            foreach (var tag in _tagsInRepository)
            {
                if (!_allTags.Contains(tag))
                {
                    tagsToPurge += 1;
                }
            }

            if (tagsToPurge > 0 || tagsToImport > 0)
            {
                _lblImport.Text = string.Empty;
                if (tagsToImport > 0)
                    _lblImport.Text = tagsToImport + " tag(s) to import";
                if (tagsToPurge > 0 && tagsToImport > 0)
                    _lblImport.Text += " and ";
                if (tagsToPurge > 0)
                    _lblImport.Text += tagsToPurge + " tag(s) to purge";
                _lblImport.Text += ".";
            }
            else
            {
                _lblImport.Text = "No tags to sync.";
            }

            _btnAddTag = FindControl("AddTagButton") as Button;
            if (_btnAddTag != null) _btnAddTag.Click += BtnAddTagClick;

            _tbAddTag = FindControl("NewTagTextBox") as TextBox;
            if (_tbAddTag != null)
            {
                _tbAddTag.Text = String.Empty;
            }
        }

        /// <summary>
        /// Checks if given tag blacklisted is.
        /// </summary>
        /// <param name="tagId">Node ID of given tag.</param>
        /// <remarks>
        /// Used for setting value in the table on content view.
        /// </remarks>
        /// <returns>"Yes" or "No" debending on value of 'IsBlacklisted' field of tag.</returns>
        public string GetIsBlackListed(int tagId)
        {
            var tmpNode = Node.LoadNode(tagId);

            var tag = tmpNode.DisplayName;

            return TagManager.IsBlacklisted(tag, SearchPaths.ToList()) ? "Yes" : "No";
        }

        /// <summary>
        /// Generates link for adding and removing tag to/from blacklist.
        /// </summary>
        /// <remarks>
        /// Used for setting value in the table on content view.
        /// </remarks>
        /// <param name="tagId">Node ID of given tag.</param>
        /// <returns>Correct link for managing blacklist.</returns>
        public string GetBlacklistLink(int tagId)
        {
            return GetActionLink(tagId, "UpdateBlacklist");
        }

        public string GetActionLink(int tagId, string action)
        {
            var tmpNode = Node.LoadNode(tagId);

            var searchPathParam = SearchPaths.Aggregate(string.Empty, (current, searchPath) => string.Concat(current, searchPath, ","));

            searchPathParam = searchPathParam.TrimEnd(',');

            switch (action)
            {
                case "UpdateBlacklist":
                    if (TagManager.IsBlacklisted(tmpNode.DisplayName, SearchPaths))
                    {
                        return "<a href=\"" + tmpNode.Path + "?Action=UpdateBlacklist&amp;Do=Remove&amp;Paths=" + HttpUtility.UrlEncode(searchPathParam) + "&amp;back=" + String.Format("{0}?{1}", Portal.Page.Current.Path, Request.QueryString) + "\">Remove from blacklist</a>";
                    }

                    return "<a href=\"" + tmpNode.Path + "?Action=UpdateBlacklist&amp;Do=Add&amp;Paths=" + HttpUtility.UrlEncode(searchPathParam) + "&amp;back=" + String.Format("{0}?{1}", Portal.Page.Current.Path, Request.QueryString) + "\">Add to blacklist</a>";
                case "Edit":
                    return
                        string.Format(
                            "<a href=\"{0}?Action=Edit&amp;Paths={1}&amp;back={2}?{3}\">Edit</a>", tmpNode.Path, HttpUtility.UrlEncode(searchPathParam), Portal.Page.Current.Path, Request.QueryString);
                case "Delete":
                    return string.Format(
                            "<a href=\"{0}?Action=Delete&amp;Paths={1}&amp;back={2}?{3}\">Delete</a>", tmpNode.Path, HttpUtility.UrlEncode(searchPathParam), Portal.Page.Current.Path, Request.QueryString); ;
                default:
                    return "<a href=\"#\"></a>";
            }

        }

        /// <summary>
        /// Event handler of Add tag button.
        /// </summary>
        /// <param name="sender">Sender parameter</param>
        /// <param name="e">EventArg parameter</param>
        /// <remarks>
        /// It adds tags are in Add tag textbox separated by spaces.
        /// </remarks>
        void BtnAddTagClick(object sender, EventArgs e)
        {
            var names = _tbAddTag.Text.Trim(',', ' ').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var name in names)
            {
                TagManager.AddToRepository(name, TagPath);
            }
            Response.Redirect(String.Format("{0}?{1}", Portal.Page.Current.Path, Request.QueryString));
        }

        /// <summary>
        /// Sets datasource of listview.
        /// </summary>
        /// <remarks>
        /// Loads tags form Content Repository and adds the following properties of them to a datatable:
        /// DisplayName, Created By, Creation Date, Modification Date, Reference Count, Path, Is Blacklisted an Node ID.
        /// Sets this datatable as datasource to the listview.
        /// </remarks>
        private void SetDataSource()
        {
            var refCounts = TagManager.GetTagOccurrencies();

            var exprList = new ExpressionList(ChainOperator.And);

            exprList.Add(new TypeExpression(ActiveSchema.NodeTypes["Tag"], true));
            exprList.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, TagPath));

            var nq = new NodeQuery(exprList);

            var result = nq.Execute();

            var dt = new DataTable();
            _tagsInRepository = new List<string>();

            dt.Columns.AddRange(new[] { 
                                        new DataColumn("DisplayName", typeof(String)),
                                        new DataColumn("CreatedBy", typeof(String)), 
                                        new DataColumn("CreationDate", typeof(DateTime)), 
                                        new DataColumn("ModificationDate", typeof(DateTime)), 
                                        new DataColumn("RefCount", typeof(Int32)), 
                                        new DataColumn("Path", typeof(String)),
                                        new DataColumn("IsBlackListed", typeof(String)),
                                        new DataColumn("ID", typeof(Int32))
                                });

            foreach (var item in result.Nodes.ToList())
            {
                dt.Rows.Add(new object[]
                                {
                                    item.DisplayName,
                                    item.CreatedBy,
                                    item.CreationDate,
                                    item.ModificationDate,
                                    refCounts.ContainsKey(item.DisplayName) ? refCounts[item.DisplayName] : 0,
                                    item.Path, GetIsBlackListed(item.Id),
                                    Convert.ToInt32(item.Id)
                                });
                if (GetIsBlackListed(item.Id) == "No")
                    _tagsInRepository.Add(item.DisplayName);
            }

            _allTags = TagManager.GetAllTags(null, SearchPaths);

            dt.DefaultView.Sort = !String.IsNullOrEmpty(Request.QueryString["OrderBy"]) ? String.Concat(Request.QueryString["OrderBy"], " " + Request.QueryString["Direction"]) : "DisplayName ASC";

            _lv = FindControl("LVTags") as ListView;
            if (_lv != null)
            {
                _lv.DataSource = dt.DefaultView;
                _lv.DataBind();
            }
        }

        /// <summary>
        /// Imports tags are not in Content Repository but in Lucene index
        /// and removes tag entities from Content Repository  are not in Lucene index and are not blacklisted.
        /// </summary>
        /// <param name="sender">Sender parameter</param>
        /// <param name="e">EventArgs parameter</param>
        protected void BtnImportClick(object sender, EventArgs e)
        {
            TagManager.SynchronizeTags(TagPath, SearchPaths);
            Response.Redirect(String.Format("{0}?{1}", Portal.Page.Current.Path, Request.QueryString));
        }

    }
}
