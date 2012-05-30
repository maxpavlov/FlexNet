using System;
using System.Collections.Generic;
using asp = System.Web.UI.WebControls;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI.Controls;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.ContentListViews
{
    public class ListView : ViewBase
    {
        protected override IEnumerable<string> GetFieldList()
        {
            Handlers.ListView listDefinition = this.ViewDefinition as Handlers.ListView;
            return listDefinition.GetColumnList();
        }

        private asp.ListView _viewBody;
        public asp.ListView ViewBody
        {
            get { return _viewBody ?? (_viewBody = this.FindControl("ViewBody") as asp.ListView); }
        }

        private Literal _viewScript;
        public Literal ViewScript
        {
            get { return _viewScript ?? (_viewScript = this.FindControl("ViewScript") as Literal); }
        }

        private bool? _showCheckboxes;
        public bool? ShowCheckboxes
        {
            get
            {
                if (!_showCheckboxes.HasValue)
                {
                    _showCheckboxes = false;
                    if (this.OwnerFrame != null)
                    {
                        var listViewPanel = this.OwnerFrame.FindControl("ListViewPanel") as Panel;
                        if (listViewPanel != null)
                            _showCheckboxes = listViewPanel.CssClass.Contains("sn-listview-checkbox");
                    }
                }
                return _showCheckboxes;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            var listDefinition = this.ViewDefinition as Handlers.ListView;

            if (listDefinition != null)
            {
                ViewDataSource.FlattenResults = listDefinition.Flat;
                ViewDataSource.DefaultOrdering = listDefinition.SortBy;
                ViewDataSource.GroupBy = listDefinition.GroupBy;

                try
                {
                    var listGrid = ViewBody as ListGrid;
                    if (listGrid != null)
                    {
                        var sortInfo = new SortingInfo(listDefinition.SortBy);

                        listGrid.DefaulSortExpression = sortInfo.FullName;
                        listGrid.DefaulSortDirection = sortInfo.Direction;
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }
            }

            base.OnLoad(e);
        }
    }
}
