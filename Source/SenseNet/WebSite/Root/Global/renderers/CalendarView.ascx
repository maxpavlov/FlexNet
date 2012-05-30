<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>

<div style="display:none">
    <sn:ActionMenu runat="server" Text="hello" NodePath="/root" Scenario="ListItem"></sn:ActionMenu>
</div>
    
<script runat="server">
        
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RefreshMonthEvents(DateTime.Today);
    }
    
    protected void CalendarControl_DayRender(object sender, DayRenderEventArgs e)
    {
        var scm = ScriptManager.GetCurrent(Page);
        if (scm != null && scm.IsInAsyncPostBack)
            return;

        if (HasEvent(e.Day.Date))
        {
            e.Day.IsSelectable = true;
            e.Cell.CssClass = String.Concat(e.Cell.CssClass," sn-cal-eventday");
        }
        else {
            e.Day.IsSelectable = false;
        }
        
        var addLink = new ActionLinkButton
                          {
                              ActionName = "Add", 
                              NodePath = this.Model.Content.Path,
                              ParameterString = "StartDate=" + e.Day.Date,
                              IconVisible = false,
                              Text = "Add",
                              ToolTip = "Add new event for " + e.Day.Date.ToString("MMMM dd")
                          };

        e.Cell.Controls.Add(addLink);
    }

    protected void CalendarControl_SelectionChanged(object sender, EventArgs e)
    {
        var calendar = sender as Calendar;
        if (calendar == null)
            return;

        if (DetailsControl == null)
            return;

        DetailsControl.DataSource = GetEventsForDay(calendar.SelectedDate);
        DetailsControl.DataBind();

        DateLabel.Text = CalendarControl.SelectedDate.ToString("MMMM dd, yyyy");
    }

    protected void CalendarControl_VisibleMonthChanged(object sender, MonthChangedEventArgs e)
    {
        var calendar = sender as Calendar;
        if (calendar == null)
            return;

        RefreshMonthEvents(e.NewDate);
    }
    
    private void RefreshMonthEvents(DateTime matchDate)
    {
        if (DetailsControl == null)
            return;

        DetailsControl.DataSource = GetEventsForMonth(matchDate);
        DetailsControl.DataBind();

        DateLabel.Text = matchDate.ToString("Y");
    }

    private bool HasEvent(DateTime currentDate)
    {
        return GetEventsForDay(currentDate).Count() > 0;
    }

    private IEnumerable<SenseNet.ContentRepository.Content> GetEventsForDay(DateTime matchDate)
    {
        return (from content in this.Model.Items
                    let mds = Convert.ToDateTime(content["StartDate"])
                    let mde = Convert.ToDateTime(content["EndDate"])
                    where (DateTime.Compare(mds.Date, matchDate.Date) <= 0 && DateTime.Compare(mde.Date, matchDate.Date) >= 0)
                    select content).ToList();
    }

    private IEnumerable<SenseNet.ContentRepository.Content> GetEventsForMonth(DateTime matchDate)
    {
        return (from content in this.Model.Items
                    let mds = Convert.ToDateTime(content["StartDate"])
                    let mde = Convert.ToDateTime(content["EndDate"])
                    where ((mds.Year == matchDate.Year && mds.Month == matchDate.Month) || (mde.Year == matchDate.Year && mde.Month == matchDate.Month))
                    select content).ToList();
    }
    
</script>
    
    <asp:Calendar ID="CalendarControl" runat="server"    
        CssClass="sn-calendar" 
        DayStyle-CssClass="sn-cal-day" 
        NextPrevStyle-CssClass="sn-cal-nextprev" 
        OtherMonthDayStyle-CssClass="sn-cal-day sn-cal-othermonth" 
        SelectedDayStyle-CssClass="sn-cal-day sn-cal-selectedday" 
        SelectorStyle-CssClass="sn-cal-selector" 
        TitleStyle-CssClass="sn-cal-title" 
        TodayDayStyle-CssClass="sn-cal-day sn-cal-today" 
        WeekendDayStyle-CssClass="sn-cal-day sn-cal-weekend"

        CellPadding="0" 
        CellSpacing="0" 
        SelectionMode="Day"
        ShowDayHeader="true" 
        ShowGridLines="false" 
        ShowTitle="true"
        ShowNextPrevMonth="true"
        OnDayRender="CalendarControl_DayRender"
        OnSelectionChanged="CalendarControl_SelectionChanged"
        OnVisibleMonthChanged="CalendarControl_VisibleMonthChanged"
        >
    </asp:Calendar>
   

<div class="sn-eventlist">
    <div class="inner">
        <h1 class="sn-eventlist-title"><asp:Label ID="DateLabel" runat="server" /></h1>

        <asp:Repeater ID="DetailsControl" runat="server" >
            <ItemTemplate>  
                
                <div class="sn-event ui-helper-clearfix">
                    <% if (Security.IsInRole("Editors")) { %>
                    <sn:ActionMenu NodePath='<%# Eval("Path") %>' runat="server" WrapperCssClass="sn-floatright" Scenario="ListItem" Text="Manage event" />
                    <% } %>

                    <div class="sn-event-schedule">
                        <span class="sn-event-date sn-event-start">
                            <small class="sn-event-year"><%# ((DateTime)Eval("StartDate")).Year %></small> 
                            <small class="sn-event-month"><%# ((DateTime)Eval("StartDate")).ToString("MMM") %></small> 
                            <big class="sn-event-day"><%# ((DateTime)Eval("StartDate")).Day %></big> 
                            <%# (bool)Eval("AllDay") ? "" : "<small class=\"sn-event-time\">" + ((DateTime)Eval("StartDate")).ToString("HH:mm") + "</small>" %>
                        </span>
                        <span class="sn-event-date-sep"> - </span>
                        <span class="sn-event-date sn-event-end">
                            <%# ((DateTime)Eval("StartDate")).Year == ((DateTime)Eval("EndDate")).Year ? "" : "<small class=\"sn-event-year\">" + ((DateTime)Eval("EndDate")).Year + "</small>"%>
                            <small class="sn-event-month"><%# ((DateTime)Eval("EndDate")).ToString("MMM") %></small>
                            <big class="sn-event-day"><%# ((DateTime)Eval("EndDate")).Day %></big>
                            <%# (bool)Eval("AllDay") ? "" : "<small class=\"sn-event-time\">" + ((DateTime)Eval("EndDate")).ToString("HH:mm") + "</small>" %>
                        </span>
                    </div>
                    
                    <%# String.IsNullOrEmpty(Eval("Location").ToString()) ? "" : "<div class=\"sn-event-location\">" + Eval("Location") + "</div>" %>
                    <h2 class="sn-event-title"><%# Actions.BrowseAction(Eval("Path").ToString(), true) %></h2>
                    <div class="sn-event-lead"><%# Eval("Lead") %></div>

                </div>
            </ItemTemplate>
            <SeparatorTemplate>
                <hr />
            </SeparatorTemplate>
        </asp:Repeater>

    </div>
</div>
    




