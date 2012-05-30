using System;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Data.Linq;
using System.Xml.Linq;
using System.Web.Query.Dynamic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Content=SenseNet.ContentRepository.Content;

namespace SenseNet.Portal.UI.ContentListViews
{

    public class UpcomingEventView : ListView
    {
        private IEnumerable<Content> _contentList;
        private Calendar _calendar;
        private Calendar CalendarControl
        {
            get
            {
                if (_calendar == null)
                    _calendar = FindControl("CalendarControl") as Calendar;
                return _calendar;
            }
        }

        private Repeater _details;
        private Repeater DetailsControl
        {
            get
            {
                if (_details == null)
                {
                    _details = FindControl("DetailsControl") as Repeater;

                    if (_details != null)
                        _details.ItemDataBound += Details_ItemDataBound;
                }

                return _details;
            }
        }

        protected void Details_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Header || CalendarControl == null) 
                return;

            var dateLabel = e.Item.FindControl("DateLabel") as Label;
            if (dateLabel == null) 
                return;

            dateLabel.Text = CalendarControl.SelectedDate.ToShortDateString();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (CalendarControl == null)
                return;
            
            CalendarControl.DayRender += CalendarControl_DayRender;
            CalendarControl.SelectionChanged += CalendarControl_SelectionChanged;
           
            _contentList = ViewDataSource.Select(DataSourceSelectArguments.Empty) ?? new List<Content>();
        }
        
        protected void CalendarControl_SelectionChanged(object sender, EventArgs e)
        {
            var calendar = sender as Calendar;
            if (calendar == null)
                return;

            if (DetailsControl == null)
                return;

            DetailsControl.DataSource = GetEvents(calendar.SelectedDate);
            DetailsControl.DataBind();
        }
        protected void CalendarControl_DayRender(object sender, DayRenderEventArgs e)
        {
            var scm = ScriptManager.GetCurrent(Page);
            
            if (scm != null && scm.IsInAsyncPostBack)
                return;

            e.Day.IsSelectable = HasEvent(e.Day.Date);
        }
        
        private bool HasEvent(DateTime currentDate)
        {
            return GetEvents(currentDate).Count() > 0;
        }

        private IEnumerable<Content> GetEvents(DateTime matchDate)
        {
            return from c in _contentList
                   where DateTime.Compare(Convert.ToDateTime(c.Fields["StartDate"].GetData()).Date, matchDate.Date) == 0
                   select c;
        }
    }
}
