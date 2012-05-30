using System;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.Portal.UI.ContentListViews
{
    public class ListHeaderCell : TableHeaderCell
    {
        public string FieldName
        {
            get;
            set;
        }

        private System.Web.UI.WebControls.ListView _currentListView;
        private System.Web.UI.WebControls.ListView CurrentListView
        {
            get
            {
                if (_currentListView == null)
                {
                    var parent = this.Parent;
                    while (parent != null && _currentListView == null)
                    {
                        _currentListView = parent as System.Web.UI.WebControls.ListView;
                        parent = parent.Parent;
                    }
                }

                return _currentListView;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (CurrentListView == null || this.FieldName == null)
                return;

            var sortDir = CurrentListView.SortDirection;
            var sortField = CurrentListView.SortExpression ?? string.Empty;

            if (string.IsNullOrEmpty(sortField))
            {
                var listGrid = CurrentListView as ListGrid;
                if (listGrid != null)
                {
                    sortField = listGrid.DefaulSortExpression ?? string.Empty;
                    sortDir = listGrid.DefaulSortDirection;
                }
                else
                {
                    sortField = string.Empty;
                    sortDir = SortDirection.Ascending;
                }
            }

            if (!SortFieldMatches(sortField))
                return;

            this.Attributes["class"] = this.Attributes["class"] + (sortDir == SortDirection.Descending ? " sn-lg-col-desc" : " sn-lg-col-asc");
        }

        private bool SortFieldMatches(string sortField)
        {
            if (string.IsNullOrEmpty(sortField) || string.IsNullOrEmpty(this.FieldName))
                return false;

            if (string.Compare(sortField, this.FieldName) == 0)
                return true;

            if (sortField.EndsWith("." + this.FieldName) || this.FieldName.EndsWith("." + sortField))
                return true;

            return false;
        }
    }
}
