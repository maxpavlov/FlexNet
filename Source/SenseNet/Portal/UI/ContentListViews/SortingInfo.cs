using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.ContentListViews
{
    public class SortingInfo
    {
        public SortingInfo(string sortExpression)
        {
            Initialize(sortExpression);
        }

        public SortingInfo(string fullName, SortDirection direction)
        {
            Initialize(fullName);
            this.Direction = direction;
        }

        public string FieldName { get; set; }
        public string FullName { get; set; }
        public SortDirection Direction { get; set; }

        private void Initialize(string sortExpression)
        {
            FieldName = string.Empty;
            Direction = SortDirection.Ascending;

            if (string.IsNullOrEmpty(sortExpression))
                return;

            var sortExp = sortExpression.Trim();

            if (sortExp.EndsWith(" DESC"))
            {
                sortExp = sortExp.Remove(sortExp.LastIndexOf(" DESC"));
                this.Direction = SortDirection.Descending;
            }
            else if (sortExp.EndsWith(" ASC"))
            {
                sortExp = sortExp.Remove(sortExp.LastIndexOf(" ASC"));
                this.Direction = SortDirection.Ascending;
            }

            this.FullName = sortExp;

            var separatorIndex = sortExp.IndexOf('.');
            if (separatorIndex >= 0)
                sortExp = sortExp.Substring(separatorIndex + 1);

            this.FieldName = sortExp;
        }
    }
}
