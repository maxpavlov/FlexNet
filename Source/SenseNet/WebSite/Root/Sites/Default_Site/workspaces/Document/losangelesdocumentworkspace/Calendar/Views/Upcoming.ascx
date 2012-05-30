<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.ContentListViews.UpcomingEventView" %>
        
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
    >
</asp:Calendar>
<div class="sn-pt-footer"></div>

<asp:Repeater ID="DetailsControl" runat="server" >
    <HeaderTemplate>
        <div class="snEventList">
            <div class="inner">

                <h2 class="snEventListTitle">Events on <asp:Label ID="DateLabel" runat="server" /> :</h2>

    </HeaderTemplate>
    <ItemTemplate>  
                <div class="snEvent">
                    <a href='<%# SenseNet.Portal.Virtualization.PortalContext.Current.GetContentUrl(Container.DataItem) %>'><%# Eval("DisplayName") %></a><br />
                    <small>Start date: <strong><%# Eval("StartDate") %></strong></small>
                </div>
    </ItemTemplate>
    <FooterTemplate>
            </div>
        </div>
        <div></div>
    </FooterTemplate>
    <SeparatorTemplate>
        <hr />
    </SeparatorTemplate>
</asp:Repeater>

<sn:SenseNetDataSource ID="ViewDatasource" runat="server" />
