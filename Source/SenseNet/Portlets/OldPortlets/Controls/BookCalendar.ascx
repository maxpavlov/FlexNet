<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.BookCalendar" %>
<asp:UpdatePanel runat="server" id="UpdatePanel">
    <contenttemplate>
    <div class="book-calendar-gotodate">
        <asp:DropDownList ID="ddlYear" runat="server"></asp:DropDownList>
        <asp:DropDownList ID="ddlMonth" runat="server"></asp:DropDownList>
        <asp:Button ID="btnGoToDate" runat="server" Text="Go to date" /> 
    </div>
    <div class="book-calendar">
        <asp:Calendar CssClass="book-calendar-eventcalendar" ID="MyCalendar" runat="server" BackColor="White" Font-Names="Verdana"
            ForeColor="black" FirstDayOfWeek="Monday" NextMonthText="Next &gt;" PrevMonthText="&lt; Prev"
            ShowGridLines="True" NextPrevFormat="ShortMonth"
            OnDayRender="MyCalendar_DayRender" showdescriptionastooltip="True" BorderStyle="Solid"
            ShowTitle="true" BorderColor="black" SelectionMode="DayWeekMonth">
            <SelectedDayStyle CssClass="book-calendar-selectedday ui-state-active" />
            <TodayDayStyle CssClass="book-calendar-today ui-state-active" />
            <SelectorStyle CssClass="book-calendar-selector" />
            <DayStyle CssClass="book-calendar-day ui-state-active" />
            <OtherMonthDayStyle CssClass="book-calendar-othermonth ui-state-active" />
            <NextPrevStyle CssClass="book-calendar-nextprev" />
            <DayHeaderStyle CssClass="book-calendar-dayheader ui-state-active" />
            <TitleStyle CssClass="book-calendar-title" />
            <SelectorStyle CssClass="book-calendar-selector ui-state-active" Width="0" BorderStyle="Solid" BorderWidth="1px" BorderColor="black"/>
        </asp:Calendar>
    </div>
    <div class="book-calendar-result">
        <asp:Panel ID="panelUpcomings" runat="server">
            <h3><asp:Label ID="lblUpcomingTitle" runat="server"></asp:Label></h3>
        </asp:Panel>
    </div>
    </contenttemplate>
</asp:UpdatePanel>