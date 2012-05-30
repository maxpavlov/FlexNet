<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.EventCalendar" %>
<asp:UpdatePanel runat="server" ID="UpdatePanel">
    <ContentTemplate>
        <div class="sn-calendar">
            <asp:Calendar ID="MyCalendar" runat="server"
                FirstDayOfWeek="Monday" NextMonthText="Next &gt;" PrevMonthText="&lt; Prev" ShowGridLines="True"
                NextPrevFormat="ShortMonth" OnDayRender="MyCalendar_DayRender" showdescriptionastooltip="True"
                BorderStyle="Solid" ShowTitle="true" SelectionMode="DayWeekMonth">
                <DayStyle HorizontalAlign="Left" VerticalAlign="Top" Wrap="True" />
            </asp:Calendar>
        </div>
        <div class="sn-book-calendar-result">
            <asp:Panel ID="panelUpcomings" runat="server">
                <h3><asp:Label ID="lblUpcomingTitle" runat="server"></asp:Label></h3>
            </asp:Panel>
        </div>

        <div class="sn-book-calendar-result"> 
            <asp:ListView ID="CalendarEventResult" runat="server">
                <LayoutTemplate>
                    <h3>Events:</h3>
                    <asp:PlaceHolder ID="itemPlaceholder" runat="server" >
                    </asp:PlaceHolder>
                </LayoutTemplate>
                <ItemTemplate>
                    <div class="sn-calendar-eventlist">
                        <span class="sn-calendar-event">
                            <sn:ActionLinkButton ID="eventTitle" runat="server" Text='<%# Eval("DisplayName")%>' ActionName="Browse" NodePath='<%# Eval("BrowseContentPath")%>' IconVisible="false" />
                        </span>
                        <br />
                        <span class="sn-calendar-time">Start Date: </span>
                        <asp:Label CssClass="sn-calendar-time" ID="startDate" runat="server" Text='<%# Eval("StartDate")%>' />
                        <br />
                        <span class="sn-calendar-time">Start Date: </span>
                        <asp:Label CssClass="sn-calendar-time" ID="endDate" runat="server" Text='<%# Eval("EndDate")%>' />
                        <br />
                        
                        <asp:MultiView ID="CalendarResultMultiView1" runat="server" ActiveViewIndex='<%# Eval("Registering")%>'>
                            <asp:View ID="NotRegisteredView" runat="server">
                                <asp:Label CssClass="sn-calendar-apply" ID="Label4" runat="server" Text='<%# Eval("ParticipantText")%>' />
                                <br />
                                <span class="sn-calendar-event">
                                    <sn:ActionLinkButton ID="ActionLinkButton1" runat="server" Text="Register" ActionName="Add" NodePath='<%# Eval("BrowseContentPath")%>' IconVisible="false" />
                                </span>
                            </asp:View>
                            
                            <asp:View ID="RegisteredView" runat="server">
                                <asp:Label CssClass="sn-calendar-apply" ID="Label5" runat="server" Text='<%# Eval("ParticipantText")%>' />
                                <br />
                                <asp:Label CssClass="sn-calendar-cannotapply" ID="Label3" runat="server" Text='<%# Eval("EventText")%>' />
                                <br />
                                <span class="sn-calendar-event">
                                    <sn:ActionLinkButton ID="ActionLinkButton2" runat="server" Text="Unregister" ActionName="Unregister" NodePath='<%# Eval("BrowseContentPath")%>' IconVisible="false" />
                                </span>
                            </asp:View>
                            
                            <asp:View ID="HideView" runat="server">
                                <asp:Label CssClass="sn-calendar-apply" ID="Label6" runat="server" Text='<%# Eval("EventText")%>' />
                            </asp:View>
                            
                            <asp:View ID="View1" runat="server">
                                <asp:Label CssClass="sn-calendar-cannotapply" ID="Label1" runat="server" Text='<%# Eval("EventText")%>' />
                                <br />
                                <asp:Label ID="IsRegistered" runat="server" CssClass="sn-calendar-event" Visible='<%# Eval("Visible")%>'>
                                    <sn:ActionLinkButton ID="ActionLinkButton3" runat="server" Text="Unregister" ActionName="Unregister" NodePath='<%# Eval("BrowseContentPath")%>' IconVisible="false" />
                                </asp:Label>
                            </asp:View>
                        </asp:MultiView>
                    </div>
                </ItemTemplate>
            </asp:ListView>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
