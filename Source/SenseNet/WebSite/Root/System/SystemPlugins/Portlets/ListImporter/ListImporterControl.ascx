<%@ Control Language="C#" ClassName="WebUserControl1" %>
<%@ Import Namespace="SenseNet.ContentRepository.i18n" %>

<div class="sn-importcsv">
    
    <div class="sn-inputunit ui-helper-clearfix">
        <div class="sn-iu-label">
            <span class="sn-iu-title">
                <asp:Label runat="server" Text='Choose your file:' AssociatedControlID="fuInputFile" />
            </span>
        </div>
        <div class="sn-iu-control">
            <asp:FileUpload CssClass="sn-ctrl sn-ctrl-upload" runat="server" ID="fuInputFile" />
        </div>
    </div>
    <div class="sn-inputunit ui-helper-clearfix">
        <div class="sn-iu-label">
            <span class="sn-iu-title">
                <asp:Label runat="server" Text='<%$Resources: ListImporterPortlet, SelectEncoding%>' AssociatedControlID="ddlEncoding"/>
            </span>
        </div>
        <div class="sn-iu-control">
            <asp:DropDownList CssClass="sn-ctrl sn-dropdown" runat="server" ID="ddlEncoding">
              <asp:ListItem Selected="False" Value="iso-8859-1">iso-8859-1</asp:ListItem>
              <asp:ListItem Selected="True" Value="iso-8859-2">iso-8859-2</asp:ListItem>
              <asp:ListItem Selected="False" Value="us-ascii">us-ascii</asp:ListItem>
              <asp:ListItem Selected="False" Value="utf-7">utf-7</asp:ListItem>
              <asp:ListItem Selected="False" Value="utf-8">utf-8</asp:ListItem>
              <asp:ListItem Selected="False" Value="utf-16">utf-16</asp:ListItem>
            </asp:DropDownList>
        </div>
    </div>
    <div class="sn-inputunit ui-helper-clearfix">
        <div class="sn-iu-label">
            <span class="sn-iu-title">
                <asp:Label runat="server" Text='<%$Resources: ListImporterPortlet, ExcludeFieldsLabel%>' AssociatedControlID="tbExclude"/>
            </span>
        </div>
        <div class="sn-iu-control">
            <asp:TextBox CssClass="sn-ctrl sn-ctrl-text sn-ctrl-long" runat="server" ID="tbExclude"></asp:TextBox>
        </div>
    </div>
    <div class="sn-inputunit ui-helper-clearfix">
        <div class="sn-iu-label">
            <span class="sn-iu-title"><%# SenseNetResourceManager.Current.GetString("ListImporterPortlet", "Options")%></span>
        </div>
        <div class="sn-iu-control">
            <asp:CheckBox runat="server" ID="cbUpdateExisting" Text='<%$Resources: ListImporterPortlet, UpdateExistingLabel%>' /> 
            <asp:CheckBox runat="server" ID="cbImportNew" Text='<%$Resources: ListImporterPortlet, ImportNewLabel%>' /><br />
        </div>
    </div>
   

    <asp:Panel ID="pnlResults" CssClass="sn-panel ui-state-default" runat="server" Visible="false">
        <h3 class="sn-content-subtitle"><%= SenseNetResourceManager.Current.GetString("ListImporterPortlet", "SuccesfullyImportedLabel") %></h3>
        <asp:ListView runat="server" ID="lvImported" >
            <LayoutTemplate>
                <ul class="sn-list">
                    <asp:PlaceHolder runat="server" ID="itemPlaceholder"></asp:PlaceHolder>
                </ul>
            </LayoutTemplate>
            <ItemTemplate>
                    <li><%# Eval("Name") %></li>
            </ItemTemplate>
            <EmptyDataTemplate>
                <p><%= SenseNetResourceManager.Current.GetString("ListImporterPortlet", "NoItems") %></p>
            </EmptyDataTemplate>
        </asp:ListView>

        <h3 class="sn-content-subtitle"><%= SenseNetResourceManager.Current.GetString("ListImporterPortlet", "NotImportedLabel") %></h3>
        <asp:ListView runat="server" ID="lvNotImported">
            <LayoutTemplate>
                <ul class="sn-list">
                    <asp:PlaceHolder runat="server" ID="itemPlaceholder"></asp:PlaceHolder>
                </ul>
            </LayoutTemplate>
            <ItemTemplate>
                    <li><%# Eval("Name") %>: <span class="sn-error"><%# Eval("Error") %></span></li>
            </ItemTemplate>
            <EmptyDataTemplate>
                <p><%= SenseNetResourceManager.Current.GetString("ListImporterPortlet", "NoItems") %></p>
            </EmptyDataTemplate>
        </asp:ListView>
    </asp:Panel>

    <asp:Panel ID="pnlError" CssClass="sn-error-msg" runat="server" Visible="false">
        <asp:Label runat="server" ID="lblError"></asp:Label>
    </asp:Panel>

    <div class="sn-panel sn-buttons">
        <asp:Button runat="server" CssClass="sn-submit" ID="btnImport" Text='<%$Resources: ListImporterPortlet, ImportButtonLabel%>' /> 
        <sn:BackButton runat="server" CssClass="sn-submit" ID="btnDone" Text="Done" /><br />
    </div>

</div>
