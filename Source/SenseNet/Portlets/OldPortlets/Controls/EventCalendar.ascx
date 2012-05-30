<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.EventCalendar" %>

<asp:UpdatePanel runat="server" id="UpdatePanel">
    <contenttemplate>
    <div class="snBookCalendar">
        <asp:Calendar ID="MyCalendar" runat="server" BackColor="White" Font-Names="Verdana"
            ForeColor="black" FirstDayOfWeek="Monday" NextMonthText="Next &gt;" PrevMonthText="&lt; Prev"
            ShowGridLines="True" NextPrevFormat="ShortMonth"
            OnDayRender="MyCalendar_DayRender" showdescriptionastooltip="True" BorderStyle="Solid"
            ShowTitle="true" BorderColor="black" SelectionMode="DayWeekMonth">
            <SelectedDayStyle BackColor="#2b9bdb" ForeColor="White" />
            <TodayDayStyle BackColor="#ececec" />
            <SelectorStyle BorderColor="#404040" BorderStyle="Solid" />
            <DayStyle HorizontalAlign="Left" VerticalAlign="Top" Wrap="True" />
            <OtherMonthDayStyle ForeColor="#999999" />
            <NextPrevStyle Font-Size="8pt" ForeColor="#333333" Font-Bold="True" VerticalAlign="Bottom" />
            <DayHeaderStyle BorderWidth="1px" Font-Bold="True" Font-Size="8pt" />
            <TitleStyle BorderColor="black" BorderWidth="1px" Font-Bold="True"
                Font-Size="12pt" ForeColor="#333399" HorizontalAlign="Center" VerticalAlign="Middle" BackColor="White" />
            <SelectorStyle Width="0" BorderStyle="Solid" BorderWidth="1px" BorderColor="black"/>
        </asp:Calendar>
    </div>
    <div class="snBookCalendarResult">
        <asp:Panel ID="panelUpcomings" runat="server">
            <h3><asp:Label ID="lblUpcomingTitle" runat="server"></asp:Label></h3>
        </asp:Panel>
    </div>
    </contenttemplate>
</asp:UpdatePanel>​