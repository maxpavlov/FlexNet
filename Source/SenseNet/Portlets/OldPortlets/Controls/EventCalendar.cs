using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using Content = SenseNet.ContentRepository.Content;
using SenseNet.Search;
using SenseNet.Portal.UI;

namespace SenseNet.Portal.Portlets.Controls
{
    public class EventCalendar : UserControl
    {
        protected Calendar MyCalendar;
        public IEnumerable<Node> CalendarEvents;
        protected Panel panelUpcomings;
        protected Label lblUpcomingTitle;
        protected ListView CalendarEventResult;

        /// <summary>
        /// Gets or sets the calendar path.
        /// </summary>
        /// <value>The calendar path.</value>
        public string CalendarPath { get; set; }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            MyCalendar.FirstDayOfWeek = FirstDayOfWeek.Monday;
            MyCalendar.NextPrevFormat = NextPrevFormat.ShortMonth;
            MyCalendar.TitleFormat = TitleFormat.MonthYear;
            MyCalendar.ShowGridLines = true;
            MyCalendar.TodaysDate = DateTime.Now;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            UITools.AddStyleSheetToHeader(UITools.GetHeader(), "$skin/styles/sn-calendar.css");

            var myNodeList = new List<MyCalendarEvent>();

            foreach (DateTime selectedDate in MyCalendar.SelectedDates)
            {
                foreach (var item in CalendarEvents)
                {
                    var myNewEvent = new MyCalendarEvent
                    {
                        EventText = "",
                        Registering = 2,
                        BrowseContentPath = item.Path
                    };

                    var eventStartDate = Convert.ToDateTime(item["StartDate"]);
                    var eventEndDate = Convert.ToDateTime(item["EndDate"]);
                    DateTime startDate;
                    DateTime endDate;

                    if (!(eventStartDate <= selectedDate))
                    {
                        startDate = Convert.ToDateTime(item["StartDate"]);
                        startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);

                        endDate = Convert.ToDateTime(item["EndDate"]);
                        endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

                        eventStartDate = startDate;
                        eventEndDate = endDate;
                    }

                    if (eventStartDate <= selectedDate && selectedDate <= eventEndDate)
                    {
                        var numParticipants = GetNumberOfParticipants(item);
                        var maxParticipants = Convert.ToInt32(item["MaxParticipants"]);
                        string regItemPath;
                        var isSubscribed = IsUserAlreadySubscribed(item, out regItemPath);

                        if (Convert.ToBoolean(item["RequiresRegistration"]))
                        {
                            // Registration needed
                            // Participant text defines the avalaible free spaces for the event
                            var partText = String.Format(SenseNetResourceManager.Current.GetString("EventCalendar", "ParticipantsText"),
                                        maxParticipants - numParticipants,
                                        maxParticipants);

                            myNewEvent.ParticipantText = partText;

                            if (maxParticipants > numParticipants)
                            {
                                if (isSubscribed)
                                {
                                    myNewEvent.Registering = 1;
                                    myNewEvent.EventText = SenseNetResourceManager.Current.GetString("EventCalendar", "AlreadyApplied");

                                    //Unregister URL
                                    myNewEvent.BrowseContentPath = regItemPath;
                                }
                                else
                                {
                                    myNewEvent.Registering = 0;
                                    myNewEvent.EventText = SenseNetResourceManager.Current.GetString("EventCalendar", "ApplyText");

                                    //Register URL
                                    myNewEvent.BrowseContentPath = ((CalendarEvent) item).RegistrationForm.Path;
                                }
                            }
                            else if (numParticipants == maxParticipants)
                            {
                                myNewEvent.Registering = 3;
                                myNewEvent.EventText = SenseNetResourceManager.Current.GetString("EventCalendar", "FullEventText");
                                if (isSubscribed)
                                {
                                    myNewEvent.Visible = true;

                                    //Unregister URL
                                    myNewEvent.BrowseContentPath = regItemPath;
                                }
                                else
                                {
                                    myNewEvent.Visible = false;
                                }
                            }
                        }
                        else
                        {
                            // Don't need registration
                            myNewEvent.EventText = SenseNetResourceManager.Current.GetString("EventCalendar", "DontNeedRegistration");
                        }
                        if (!(myNodeList.Any(i => i.RelatedNode.Id == item.Id)))
                        {
                            myNewEvent.RelatedNode = item;
                            myNodeList.Add(myNewEvent);
                        }
                    }
                }
            }

            CalendarEventResult.DataSource = myNodeList;
            this.DataBind();
        }

        /// <summary>
        /// Determines whether [is user already subscribed].
        /// </summary>
        /// <param name="item">The CalendarEvent item.</param>
        /// <param name="registeredItem">The registered item (output value).</param>
        /// <returns>
        /// 	<c>true</c> if [is user already subscribed]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsUserAlreadySubscribed(Node item, out string registeredItem)
        {
            var regForm = item.GetReference<Node>("RegistrationForm");
            registeredItem = String.Empty;

            if (regForm != null)
            {
                var qt = string.Format("+Type:eventregistrationformitem +ParentId:{0} +CreatedById:{1} .AUTOFILTERS:OFF .LIFESPAN:OFF", regForm.Id, User.Current.Id);
                var result = ContentQuery.Query(qt);

                if (result.Count > 0)
                {
                    registeredItem = NodeHead.Get(result.Identifiers.First()).Path;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the number of participants.
        /// </summary>
        /// <param name="item">The CalendarEvent item.</param>
        /// <returns>Number of approved participants.</returns>
        private static int GetNumberOfParticipants(Node item)
        {
            var regForm = item.GetReference<Node>("RegistrationForm");
            if (regForm != null)
            {
                var qResult =
                    ContentQuery.Query(string.Format("+Type:eventregistrationformitem +ParentId:{0}", regForm.Id), 
                        new QuerySettings { EnableAutofilters = false, EnableLifespanFilter = false } );

                var i = 0;

                foreach (var node in qResult.Nodes)
                {
                    var subs = Content.Create(node);
                    var guests = 0;
                    int.TryParse(subs["GuestNumber"].ToString(), out guests);
                    i += (guests + 1);
                }

                return i;
            }
            return 0;
        }

        /// <summary>
        /// Handles the DayRender event of the MyCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DayRenderEventArgs"/> instance containing the event data.</param>
        protected virtual void MyCalendar_DayRender(object sender, DayRenderEventArgs e)
        {
            e.Day.IsSelectable = false;

            DateTime startDate;
            DateTime endDate;

            foreach (var item in CalendarEvents)
            {
                if (Convert.ToDateTime(item["StartDate"]) <= e.Day.Date && e.Day.Date <= Convert.ToDateTime(item["EndDate"]))
                {
                    e.Cell.CssClass = "sn-calendar-eventday";
                    e.Day.IsSelectable = true;
                }
                else if (!(Convert.ToDateTime(item["StartDate"]) <= e.Day.Date))
                {
                    startDate = Convert.ToDateTime(item["StartDate"]);
                    startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);

                    endDate = Convert.ToDateTime(item["EndDate"]);
                    endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

                    if (startDate <= e.Day.Date && e.Day.Date <= endDate)
                    {
                        e.Cell.CssClass = "sn-calendar-eventday";
                        e.Day.IsSelectable = true;
                    }
                }
            }
        }
    }

    public class MyCalendarEvent
    {
        public Node RelatedNode { get; set; }
        public string DisplayName
        {
            get
            {
                return RelatedNode["DisplayName"].ToString();
            }
        }

        public DateTime StartDate
        {
            get
            {
                return Convert.ToDateTime(RelatedNode["StartDate"]);
            }
        }

        public DateTime EndDate
        {
            get
            {
                return Convert.ToDateTime(RelatedNode["EndDate"]);
            }
        }

        public string ParticipantText { get; set; }

        public string EventText { get; set; }

        public int Registering { get; set; }

        public string BrowseContentPath { get; set; }

        public bool Visible { get; set; }
    }
}
