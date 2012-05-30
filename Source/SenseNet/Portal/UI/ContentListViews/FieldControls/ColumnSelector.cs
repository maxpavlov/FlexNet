using System;
using System.Collections.Generic;
using System.Linq;
using SenseNet.Portal.UI.Controls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using System.Web.UI.WebControls;
using Handler = SenseNet.Portal.UI.ContentListViews.Handlers.ListView;
using WebControls = System.Web.UI.WebControls;
using SenseNet.Portal.UI.ContentListViews.Handlers;

namespace SenseNet.Portal.UI.ContentListViews.FieldControls
{
    public class ColumnSelector : FieldControl
    {
        private WebControls.ListView _columnListView;
        protected WebControls.ListView ColumnListView
        {
            get
            {
                if (_columnListView == null && this.Controls.Count > 0)
                {
                    _columnListView =
                        this.Controls[0].FindControl("InnerListView") as WebControls.ListView;

                    if (_columnListView != null)
                        _columnListView.ItemDataBound += ColumnList_ItemDataBound;
                }

                return _columnListView;
            }
        }

        private WebControls.ListView _columnListViewAdvanced;
        protected WebControls.ListView ColumnListViewAdvanced
        {
            get
            {
                if (_columnListViewAdvanced == null && this.Controls.Count > 0)
                {
                    _columnListViewAdvanced =
                        this.Controls[0].FindControl("InnerListViewAdvanced") as WebControls.ListView;

                    if (_columnListViewAdvanced != null)
                        _columnListViewAdvanced.ItemDataBound += ColumnList_ItemDataBound;
                }

                return _columnListViewAdvanced;
            }
        }

        private Panel _advancedPanel;
        protected Panel AdvancedPanel
        {
            get
            {
                if (_advancedPanel == null && this.Controls.Count > 0)
                {
                    _advancedPanel = this.Controls[0].FindControl("AdvancedPanel") as Panel;
                }

                return _advancedPanel;
            }
        }

        private AdvancedPanelButton _advancedPanelButton;
        protected AdvancedPanelButton AdvancedPanelButton
        {
            get
            {
                if (_advancedPanelButton == null && this.Controls.Count > 0)
                {
                    _advancedPanelButton =
                        this.Controls[0].FindControl("AdvancedFieldsButton") as AdvancedPanelButton;
                }

                return _advancedPanelButton;
            }
        }

        private List<Column> _columns;
        private List<Column> _columnsAdvanced;

        private GenericContent _systemContext;
        public GenericContent SystemContext
        {
            get
            {
                if (_systemContext == null)
                {
                    var node = Content.ContentHandler as GenericContent;
                    _systemContext = (node != null) ? node.MostRelevantSystemContext : null;
                }

                return _systemContext;
            }
        }

        private List<string> _advancedFieldNames;

        private List<FieldSetting> _availableFields;
        public IEnumerable<FieldSetting> AvailableFields
        {
            get
            {
                if (_availableFields == null)
                {
                    _availableFields = new List<FieldSetting>();
                    _advancedFieldNames = new List<string>();

                    //get leaf settings to determine visibility using the most granted mode
                    var leafFieldSettings = SystemContext.GetAvailableFields(false);

                    foreach (var fieldSetting in leafFieldSettings)
                    {
                        var fs = fieldSetting;
                        var displayableFound = false;
                        var advanced = true;
                        var fieldName = string.Empty;

                        while (fs != null)
                        {
                            if (!displayableFound &&
                                (fs.VisibleBrowse != FieldVisibility.Hide || 
                                fs.VisibleEdit != FieldVisibility.Hide || 
                                fs.VisibleNew != FieldVisibility.Hide))
                            {
                                //get the root field setting and add if it was not added before
                                var rootFs = FieldSetting.GetRoot(fs);
                                if (!_availableFields.Contains(rootFs))
                                    _availableFields.Add(rootFs);

                                //store the root name of the fieldsetting
                                fieldName = rootFs.BindingName;
                                displayableFound = true;
                            }

                            if (fs.VisibleBrowse == FieldVisibility.Show ||
                                fs.VisibleEdit == FieldVisibility.Show ||
                                fs.VisibleNew == FieldVisibility.Show)
                            {
                                advanced = false;
                            }

                            if (displayableFound && !advanced)
                                break;

                            fs = fs.ParentFieldSetting;
                        }

                        if (advanced && !string.IsNullOrEmpty(fieldName))
                            _advancedFieldNames.Add(fieldName);
                    }
                }

                return _availableFields;
            }
        }

        private List<int> _columnIndexList;

        protected override void OnInit(EventArgs e)
        {
            if (this.ColumnListView == null)
            {
                var c = Page.LoadControl("/Root/System/SystemPlugins/ListView/ColumnSelectorControl.ascx");
                if (c != null)
                {
                    this.Controls.Add(c);

                    if (AdvancedPanelButton != null && AdvancedPanel != null)
                    {
                        AdvancedPanelButton.AdvancedPanelId = AdvancedPanel.ClientID;
                    }
                }
            }

            base.OnInit(e); 
        }

        public override object GetData()
        {
            var columns = new List<Column>();

            columns.AddRange(GetSelectedColumns(ColumnListView));
            columns.AddRange(GetSelectedColumns(ColumnListViewAdvanced));

            return Handler.SerializeColumnXml(columns);
        }

        public override void SetData(object data)
        {
            _columnIndexList = new List<int>();

            var index = 1;
            var unusedCols = new List<Column>();
            var advancedCols = new List<Column>();
            var colXml = data as string;
            var colEnum = Handler.PrepareColumnList(colXml);
            var fieldTitles = new List<string>();
            var duplicatedTitles = new List<string>();

            _columns = colEnum != null ? colEnum.ToList() : new List<Column>();
            _columnsAdvanced = new List<Column>();

            var ind = 1;

            foreach (var column in _columns)
            {
                column.Selected = true;

                //use the display name of the field instead of the value saved in the view
                var afs = AvailableFields.FirstOrDefault(af => af.FullName == column.FullName);
                if (afs != null)
                    column.Title = afs.DisplayName;

                //default index for the selected columns)
                if (this.Field.Content.IsNew)
                    column.Index = ind++;

                //add index numbers for selected columns
                _columnIndexList.Add(index++);
            }

            foreach (var fs in AvailableFields)
            {
                if (fieldTitles.Contains(fs.DisplayName))
                    duplicatedTitles.Add(fs.DisplayName);
                else
                    fieldTitles.Add(fs.DisplayName);

                if (_columns.Any(c => c.BindingName == fs.BindingName)) 
                    continue;

                //add index numbers for remaining columns
                _columnIndexList.Add(index++);

                var col = new Column
                              {
                                  Title = (string.IsNullOrEmpty(fs.DisplayName) ? fs.Name : fs.DisplayName),
                                  FullName = fs.FullName,
                                  BindingName = fs.BindingName,
                                  Index = 1,
                                  Selected = false
                              };

                if (_advancedFieldNames.Contains(fs.BindingName))
                {
                    advancedCols.Add(col);
                }
                else
                {
                    unusedCols.Add(col);
                }
            }

            unusedCols.Sort(CompareColumns);
            advancedCols.Sort(CompareColumns);

            //default column indexes: 1,2,3,...
            ind = _columns.Count + 1;
            foreach (var col in unusedCols)
            {
                col.Index = ind++;
            }

            foreach (var col in advancedCols)
            {
                col.Index = ind++;
            }

            _columns.AddRange(unusedCols); 
            _columnsAdvanced.AddRange(advancedCols);

            foreach (var dupTitle in duplicatedTitles)
            {
                foreach (var column in _columns.Where(cc => cc.Title == dupTitle))
                {
                    var fs = this.AvailableFields.FirstOrDefault(af => af.FullName == column.FullName);
                    if (fs == null)
                        continue;

                    column.Title = column.Title + string.Format(" ({0})", fs.Owner.DisplayName);
                }

                foreach (var column in _columnsAdvanced.Where(cc => cc.Title == dupTitle))
                {
                    var fs = this.AvailableFields.FirstOrDefault(af => af.FullName == column.FullName);
                    if (fs == null)
                        continue;

                    column.Title = column.Title + string.Format(" ({0})", fs.Owner.DisplayName);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ColumnListView.DataSource = _columns;
            ColumnListView.DataBind();

            if (ColumnListViewAdvanced != null)
            {
                ColumnListViewAdvanced.DataSource = _columnsAdvanced;
                ColumnListViewAdvanced.DataBind();
            }
        }

        private IEnumerable<Column> GetSelectedColumns(WebControls.ListView listView)
        {
            var cols = new List<Column>();

            if (listView == null)
                return cols;

            foreach (var c in listView.Items)
            {
                var cb = c.FindControl("cbField") as CheckBox;
                var tb = c.FindControl("tbWidth") as TextBox;
                var ha = c.FindControl("ddHAlign") as DropDownList;
                var wm = c.FindControl("ddWrap") as DropDownList;
                var lblFullName = c.FindControl("lblColumnFullName") as Label;
                var ind = c.FindControl("ddIndex") as DropDownList;

                if ((cb == null) || !cb.Checked || lblFullName == null)
                    continue;

                var query = from FieldSetting f in AvailableFields
                            where f.FullName == lblFullName.Text
                            select f;

                if (query.Count() <= 0)
                    continue;

                var fs = query.First();
                var newcol = new Column
                {
                    Title = (string.IsNullOrEmpty(fs.DisplayName) ? fs.Name : fs.DisplayName),
                    FullName = fs.FullName,
                    BindingName = fs.BindingName,
                    Index = ind != null ? Convert.ToInt32(ind.SelectedValue) : 1
                };

                if (tb != null)
                {
                    var colWidth = 0;
                    if (int.TryParse(tb.Text, out colWidth))
                        newcol.Width = colWidth;
                }

                if (ha != null)
                {
                    newcol.HorizontalAlign = ha.SelectedValue;
                }

                if (wm != null)
                {
                    newcol.Wrap = wm.SelectedValue;
                }

                //TODO: refactor this
                if (fs.BindingName == "GenericContent_DisplayName" ||
                    fs.BindingName == "GenericContent_Title" ||
                    fs.BindingName == "GenericContent_Name" ||
                    fs.BindingName == "ContentType_DisplayName" ||
                    fs.BindingName == "ContentType_Title" ||
                    fs.BindingName == "ContentType_Name")
                {
                    newcol.IsLeadColumn = true;
                    newcol.Icon = fs.Icon;
                }

                cols.Add(newcol);
            }

            return cols;
        }

        protected void ColumnList_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var di = e.Item as ListViewDataItem;

            if (di == null)
                return;

            var col = di.DataItem as Column;
            if (col == null)
                return;

            var dd = di.FindControl("ddIndex") as DropDownList;
            var cb = di.FindControl("cbField") as CheckBox;
            var tb = di.FindControl("tbWidth") as TextBox;
            var ha = di.FindControl("ddHAlign") as DropDownList;
            var wm = di.FindControl("ddWrap") as DropDownList;
            var lblOwner = di.FindControl("lblColumnOwner") as Label;
            var lblType = di.FindControl("lblColumnType") as Label;

            if (dd != null)
            {
                dd.DataSource = _columnIndexList;
                dd.DataBind();

                dd.SelectedIndex = col.Index - 1;
            }

            if (cb != null)
            {
                cb.Checked = col.Selected;
            }

            if (tb != null)
            {
                tb.Text = col.Width > 0 ? col.Width.ToString() : string.Empty;
            }

            if (ha != null)
            {
                var selected = col.HorizontalAlign ?? string.Empty;
                var index = 0;
                ha.Items.Clear();
                ha.Items.Add("");

                foreach (var haName in Enum.GetNames(typeof(HorizontalAlign)))
                {
                    ha.Items.Add(haName);
                    index++;

                    if (haName == selected)
                        ha.SelectedIndex = index;
                }
            }

            if (wm != null)
            {
                var selected = col.Wrap ?? string.Empty;
                var index = 0;
                wm.Items.Clear();
                wm.Items.Add("");

                foreach (var wmName in Enum.GetNames(typeof(WrapMode)))
                {
                    wm.Items.Add(wmName);
                    index++;

                    if (wmName == selected)
                        wm.SelectedIndex = index;
                }
            }

            //find the field and display its metadata
            var afs = AvailableFields.FirstOrDefault(fs => fs.FullName.CompareTo(col.FullName) == 0);
            if (afs != null)
            {
                if (lblOwner != null)
                    lblOwner.Text = afs.Owner.Name;

                if (lblType != null && !string.IsNullOrEmpty(afs.ShortName))
                    lblType.Text = afs.ShortName;
            }
            else if (lblType != null)
            {
                lblType.CssClass += " sn-error";
                lblType.Text = "unknown";
            }
        }

        private static int CompareColumns(Column x, Column y)
        {
            var xTitle = x.Title ?? string.Empty;
            var yTitle = y.Title ?? string.Empty;

            return xTitle.CompareTo(yTitle);
        }
    }
}
