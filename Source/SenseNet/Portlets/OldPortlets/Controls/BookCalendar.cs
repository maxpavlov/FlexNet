using System;
using System.Drawing;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Portlets.Controls
{
    public class BookCalendar : EventCalendar
    {
        protected DropDownList ddlMonth;
        protected DropDownList ddlYear;
        protected Button btnGoToDate;
        //protected Panel panelUpcomings;
        //protected Label lblUpcomingTitle;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            //base.OnPreRender(e);
            
            foreach (DateTime selectedDate in MyCalendar.SelectedDates)
            {
                foreach (var item in CalendarEvents)
                {
                    var eventDate = (DateTime)item["PublishDate"];
                    if (eventDate.Year == selectedDate.Year && eventDate.Month == selectedDate.Month && eventDate.Day == selectedDate.Day)
                    {
                        var lit = new Literal
                        {
                            Text = String.Format("<div class=\"book-calendar-link\"><a href=\"{0}\">{2}: {1} </a> <div class=\"book-calendar-date\">({3})</div></div>",
                                                                item.Path, item["DisplayName"],
                                                                item["Author"],
                                                                eventDate.ToShortDateString())
                        };
                        panelUpcomings.Controls.Add(lit);
                    }
                }
            }

            if (MyCalendar.SelectedDates.Count == 1)
            {
                lblUpcomingTitle.Text = String.Format("Books to be pulished on {0}", MyCalendar.SelectedDates[0].ToShortDateString());
            }
            else if (MyCalendar.SelectedDates.Count > 1)
            {
                lblUpcomingTitle.Text = String.Format("Books to be pulished between {0} and {1}", MyCalendar.SelectedDates[0].ToShortDateString(), MyCalendar.SelectedDates[MyCalendar.SelectedDates.Count - 1].ToShortDateString());
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                for (var i = DateTime.Now.Year; i < DateTime.Now.Year + 3; i++)
                {
                    var list = new ListItem
                                   {
                                       Text = i.ToString(),
                                       Value = i.ToString()
                                   };
                    ddlYear.Items.Add(list);
                }

                var month = Convert.ToDateTime("1/1/2000");
                for (var i = 0; i < 12; i++)
                {
                    var nextMonth = month.AddMonths(i);
                    var list = new ListItem
                                   {
                                       Text = nextMonth.ToString("MMMM"),
                                       Value = nextMonth.Month.ToString()
                                   };
                    ddlMonth.Items.Add(list);
                }
                ddlYear.SelectedValue = DateTime.Now.Year.ToString();
                ddlMonth.SelectedValue = DateTime.Now.Month.ToString();
            }

            btnGoToDate.Click += btnGoToDate_Click;
        }

        /// <summary>
        /// Handles the Click event of the btnGoToDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void btnGoToDate_Click(object sender, EventArgs e)
        {
            MyCalendar.VisibleDate = new DateTime(Convert.ToInt32(ddlYear.SelectedValue), Convert.ToInt32(ddlMonth.SelectedValue), 1);
        }


        /// <summary>
        /// Handles the DayRender event of the MyCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DayRenderEventArgs"/> instance containing the event data.</param>
        protected override void MyCalendar_DayRender(object sender, DayRenderEventArgs e)
        {
            e.Day.IsSelectable = false;

            foreach (var item in CalendarEvents)
            {
                if ((DateTime)item["PublishDate"] == e.Day.Date)
                {
                    if (!e.Cell.CssClass.Contains(" book-calendar-eventday")) e.Cell.CssClass = String.Concat(e.Cell.CssClass, " book-calendar-eventday ui-state-active");
                    e.Day.IsSelectable = true;
                }
            }
        }
    }
}
