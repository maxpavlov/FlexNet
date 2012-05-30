using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Portal.Portlets.Controls
{
    public class AuditLogControl : UserControl
    {
        protected Table filterTable;
        protected TextBox startDate;
        protected TextBox endDate;
        protected TextBox userName;
        protected ListBox eventList;
        protected TextBox parameter;
        protected Button filterButton;
        protected SqlDataSource auditlogDataSource;
        protected SqlDataSource eventTypeDataSource;
        protected GridView auditLogGridView;

        public AuditLogControl()
        {
            
        }
        protected override void OnInit(EventArgs e)
        {
            startDate.Text = DateTime.Now.AddDays(-1).ToString();
            base.OnInit(e);
            auditlogDataSource.ConnectionString = RepositoryConfiguration.ConnectionString;
            eventTypeDataSource.ConnectionString = RepositoryConfiguration.ConnectionString;
            FilterBtnOnClick(this, EventArgs.Empty);
        }
        protected void FilterBtnOnClick(object sender, EventArgs e)
        {
            var sDate = new DateTime();
            var eDate = new DateTime();
            if(!string.IsNullOrEmpty(startDate.Text) && !DateTime.TryParse(startDate.Text, out sDate))
            {
                //error msg
                return;
            }
            if (!string.IsNullOrEmpty(endDate.Text) && !DateTime.TryParse(endDate.Text, out eDate))
            {
                //error msg
                return;
            }
            var sDateText = string.IsNullOrEmpty(startDate.Text) ? string.Empty : sDate.ToString();
            var eDateText = string.IsNullOrEmpty(endDate.Text) ? string.Empty : eDate.ToString();

            SetParameterValue("startDate", sDateText);
            SetParameterValue("endDate", eDateText);
            SetParameterValue("usrName", userName.Text);
            SetParameterValue("title", eventList.SelectedValue);
            SetParameterValue("params", parameter.Text);
            auditLogGridView.PageIndex = 0;
        }

        private void SetParameterValue(string paramName, string value)
        {
            var param = auditlogDataSource.SelectParameters[paramName];
            if(param != null)
            {
                param.DefaultValue = value;
            }
        }
    }
}
