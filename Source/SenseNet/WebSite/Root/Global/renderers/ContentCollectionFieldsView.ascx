<%@ Control Language="C#" AutoEventWireup="true" %>

<sn:SenseNetDataSource ID="SNDSFields" ContextInfoID="ViewContext" MemberName="AllFieldSettingContents" FieldNames="Name DisplayName ShortName Owner Path" runat="server"  />

<asp:ListView ID="FieldsContentList" DataSourceID="SNDSFields" runat="server" EnableViewState="false" >
       <LayoutTemplate>
            <table class="sn-listgrid ui-widget-content">
                <thead>
                    <tr class="ui-widget-content">
                        <th class="sn-lg-col-1 ui-state-default">Field title</th>
                        <th class="sn-lg-col-2 ui-state-default">Name</th>
                        <th class="sn-lg-col-3 ui-state-default">Defined in</th>
                        <th class="sn-lg-col-4 ui-state-default">Type</th>
                        <th class="sn-lg-col-5 ui-state-default">Description</th>
                    </tr>
                </thead>
                <tbody>
                    <tr runat="server" id="itemPlaceHolder"></tr>
                </tbody>
            </table>               
       </LayoutTemplate>
       <ItemTemplate>
            <tr class="sn-lg-row0 ui-widget-content">
                <td><%# Eval("DisplayName") %></td>
                <td><%# Eval("Name") %></td>
                <td><sn:ActionLinkButton runat="server" ID="ActionLink" ActionName="Explore" NodePath='<%# ((SenseNet.ContentRepository.Storage.Node)((SenseNet.ContentRepository.Content)Container.DataItem)["Owner"]).Path %>' Text='<%# Eval("Owner")%>' IconVisible="false" /></td>
                <td><%# Eval("ShortName")%></td>
                <td><%# ((SenseNet.ContentRepository.Content)Container.DataItem)["Description"]%></td>
            </tr>
       </ItemTemplate>
       <EmptyDataTemplate>
       </EmptyDataTemplate>
    </asp:ListView>   