<%@ Import Namespace="SenseNet.ContentRepository"%>
<%@ Import Namespace="SenseNet.ContentRepository.Storage"%>
<%@ Import Namespace="SenseNet.ContentRepository.Storage.Data" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage.Security" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization"%>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.Controls.PermissionEditor" %>
<sn:ScriptRequest ID="ScriptRequest1" runat="server" Path="$skin/scripts/jquery/jquery.js" />

<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true" ID="PermissionContext" />

  <div style="float:left"><%= SenseNet.Portal.UI.IconHelper.RenderIconTag("security", null, 32) %></div>
  <h2 class="sn-view-title"><%= PortalContext.Current.ContextNode.DisplayName %></h2>
  <strong>Path:</strong> <%= PortalContext.Current.ContextNodePath %>
  <br/><br/>

  <asp:Panel ID="InheritanceIndicator" runat="server">
    <div class="sn-pt-body" style="background: none; background-color: #FFFACD">
        <div style="float:left;">
            <asp:Panel ID="BreakedPermission" runat="server" >
                <div class="sn-breadcrumb">
                   <%= String.Format((string)HttpContext.GetGlobalResourceObject("Portal", "PermEditor_InheritPermissionBegin"), PortalContext.Current.ContextNode.DisplayName) %>
                   <asp:HyperLink ID="ParentLink" runat="server" CssClass="sn-link"></asp:HyperLink>
                   <%=HttpContext.GetGlobalResourceObject("Portal", "PermEditor_InheritPermissionEnd")%>
                </div>
            </asp:Panel>            
            <asp:Panel ID="InheritedPermission" runat="server" Enable="false">
                <div>
                    <div class="sn-breadcrumb" style="float:left;">
                        <%= String.Format((string)HttpContext.GetGlobalResourceObject("Portal","PermEditor_HasOwnPermission"), PortalContext.Current.ContextNode.DisplayName) %>                    
                    </div>
                </div>
            </asp:Panel>
        </div>
        <div style="text-align:right; float:right">
            <asp:Button CssClass="sn-submit" ID="ButtonRemoveBreak" runat="server" OnClick="ButtonRemoveBreak_Click" Text="<%$ Resources: Portal, PermEditor_RemoveBreak %>" />
            <asp:Button CssClass="sn-submit" ID="ButtonBreak" runat="server" OnClick="ButtonBreak_Click" Text="<%$ Resources: Portal, PermEditor_BreakInheritance %>" />
            <%--<asp:LinkButton ID="ButtonBreak" runat="server" OnClick="ButtonBreak_Click" Text="<%$ Resources: Portal, PermEditor_BreakInheritance %>" />
            <asp:LinkButton ID="ButtonRemoveBreak" runat="server" OnClick="ButtonRemoveBreak_Click" Text="<%$ Resources: Portal, PermEditor_RemoveBreak %>" />--%>

        </div>        
        
        <% if (HasCustomPermissions())
           { %>    
           <br />   
            <div style="float:left;">
                <asp:Label runat="server" ID="LabelExplicitPermissions"><%= HttpContext.GetGlobalResourceObject("Portal", "PermEditor_HasExtended") %></asp:Label>        
            </div>    
        <% } %>
    </div>
    <br/>
</asp:Panel>

<div id="sn-permissions-edit">

<asp:UpdatePanel id="updPermissionEditor" UpdateMode="Conditional" runat="server">
   <ContentTemplate>

<asp:Panel ID="PanelError" runat="server" Visible="false" CssClass="sn-error" />

<asp:Button CssClass="sn-submit" ID="ButtonAddEntry" runat="server" OnClick="ButtonAddEntry_Click" Text="<%$ Resources: Portal, PermEditor_AddNewEntry %>" />

<asp:Panel ID="PlcAddEntry" CssClass="sn-permissions-addentry" runat="server" Visible="false">

        <div class="sn-inputunit">
            <div class="sn-iu-label">
		        <label class="sn-iu-title"><%= HttpContext.GetGlobalResourceObject("Portal", "PermEditor_Type") %></label>
		    </div>
            <div class="sn-iu-control">
                <asp:RadioButtonList CssClass="sn-radiogroup sn-radiogroup-h" RepeatLayout="Flow" RepeatDirection="Horizontal" ID="RbListIdentityType" runat="server">
                    <asp:ListItem Value="User" Text="User" Selected="true" />
                    <asp:ListItem Value="Group" Text="Group" />
                    <asp:ListItem Value="OrganizationalUnit" Text="Organizational unit" />
                </asp:RadioButtonList>
            </div>
        </div>

        <div class="sn-inputunit">
            <div class="sn-iu-label">
		        <label class="sn-iu-title"><%= HttpContext.GetGlobalResourceObject("Portal", "PermEditor_SearchName") %></label>
		    </div>
            <div class="sn-iu-control">
                <asp:TextBox CssClass="sn-ctrl sn-ctrl-text" ID="SearchText" runat="server" />
                <asp:Button CssClass="sn-submit" ID="ButtonSearchIdentity" runat="server" Text="<%$ Resources: Portal, PermEditor_BtnSearch %>" OnClick="ButtonSearchId_Click" />
            </div>
        </div>

        <div class="sn-inputunit">
            <div class="sn-iu-label">
		        <label class="sn-iu-title"><%= HttpContext.GetGlobalResourceObject("Portal", "PermEditor_ChooseEntry")%></label>
		    </div>
            <div class="sn-iu-control">
                <asp:ListBox CssClass="sn-ctrl sn-ctrl-select" ID="ListEntries" runat="server" Rows="5" SelectionMode="Multiple" Width="100%" />
		    </div>
        </div>
        
        <div class="sn-panel sn-buttons">
            <asp:Button CssClass="sn-submit" ID="ButtonAddSelectedItem" runat="server" OnClick="ButtonAddSelected_Click" Text="<%$ Resources: Portal, PermEditor_BtnAdd %>" /> 
            <asp:Button CssClass="sn-submit" ID="ButtonCancelAddIdentity" runat="server" OnClick="ButtonCancelAddId_Click" Text="<%$ Resources: Portal, PermEditor_BtnCancel %>" />
		</div>
    
</asp:Panel>

<asp:ListView ID="ListViewAcl" runat="server" EnableViewState="false" >
   <LayoutTemplate>
        <div style="margin-top:10px">
          <div class="sn-permissions-entry" runat="server" id="itemPlaceHolder"></div>
        </div>      
   </LayoutTemplate>
   <ItemTemplate>

         <h2 class="sn-permissions-title" style="clear:both">
            <div style="float:left">
                <asp:Label ID="LabelHiddenAce" runat="server" Visible="false" />
                <asp:Button runat="server" ID="ButtonVisibleAcePanel" Text="Show/Hide" OnClick="ButtonAcePanelVisible_Click" />
                <asp:LinkButton runat="server" id="LinkIdentityName" OnClick="ButtonAcePanelVisible_Click">
                <asp:Label CssClass="sn-icon-button sn-icon-big" id="LabelIcon" runat="server"></asp:Label><asp:Label ID="LabelIdentityName" runat="server" />
                </asp:LinkButton>
                <asp:PlaceHolder runat="server" ID="LabelInherited" Visible='<%# !this.CustomEntryIds.Contains((Container.DataItem as SnAccessControlEntry).Identity.NodeId) %>'> <span style="background-color:#FFFACD;color:Black;font-size:small;font-weight:normal">inherited</span></asp:PlaceHolder>
            </div>
            <div style="float:right;">            
                <sn:ActionLinkButton ID="ActionLinkButtonEditMembers" runat="server" ActionName="Edit" 
                    NodePath='<%# (Container.DataItem as SnAccessControlEntry).Identity.Path %>' Text="<%$ Resources: Portal, PermEditor_EditMembers %>" IconName="group"
                    Visible='<%# Node.LoadNode((Container.DataItem as SnAccessControlEntry).Identity.Path).NodeType.IsInstaceOfOrDerivedFrom("Group") && !RepositoryConfiguration.SpecialGroupNames.Contains((Container.DataItem as SnAccessControlEntry).Identity.Name) %>' />

            </div>
         </h2>

         <asp:PlaceHolder ID="PanelAce" runat="server">
         <div class="sn-inputunit">
            
            <div class="sn-iu-label">
                <asp:Label ID="LabelTitle1" runat="server" CssClass="sn-iu-title" Text="<%$ Resources: Portal, PermEditor_PermSettings %>" />
                <br />
			    <label class="sn-iu-desc"></label>
			</div>
           
           <div class="sn-iu-control">
               <asp:ListView ID="ListViewAce" runat="server" EnableViewState="false" >
                   <LayoutTemplate>      
                       <table class="sn-permissions">
                         <tr>
                             <th style="width:400px"><asp:Label ID="LabelHeader1" runat="server" Text="<%$ Resources: Portal, PermEditor_Permission %>" /></th>
                             <th class="center" style="width:50px">
                                    <asp:CheckBox id="cbToggle1" runat="server" CssClass="sn-checkbox sn-checkbox-toggleall" ToolTip="<%$ Resources: Portal, PermEditor_ToggleAll %>" /><br />
                                    <asp:Label ID="LabelHeader3" runat="server" Text="<%$ Resources: Portal, PermEditor_Allow %>" /></th>
                             <th style="width:175px"><asp:Label ID="LabelHeader4" runat="server" Text="<%$ Resources: Portal, PermEditor_InheritsFrom %>" /></th>
                             <th class="center" style="width:50px">
                                    <asp:CheckBox id="cbToggle2" runat="server" CssClass="sn-checkbox sn-checkbox-toggleall" ToolTip="<%$ Resources: Portal, PermEditor_ToggleAll %>" /><br />
                                    <asp:Label ID="LabelHeader5" runat="server" Text="<%$ Resources: Portal, PermEditor_Deny %>" /></th>
                             <th style="width:175px"><asp:Label ID="LabelHeader6" runat="server" Text="<%$ Resources: Portal, PermEditor_InheritsFrom %>" /></th>
                             <th style="width:50px"><asp:Label ID="LabelHeader7" runat="server" Text="<%$ Resources: Portal, PermEditor_Effective %>" /></th>
                         </tr>
                         <tr runat="server" id="itemPlaceHolder" />
                       </table>
                   </LayoutTemplate>
                   <ItemTemplate>
                       <tr class="<%# Container.DisplayIndex % 2 == 0 ? "row0" : "row1" %>">      		
                         <td><asp:Label ID="LabelPermissionName" runat="server" Text='<%# HttpContext.GetGlobalResourceObject("Portal", "Permission_" + Eval("Name")) as string %>' />
                             <asp:Label ID="LabelHidden" runat="server" Visible="false" />
                         </td>
                         <td class="center"><asp:CheckBox ID="CbPermissionAllow" runat="server" Checked='<%# Eval("Allow") %>' Enabled='<%# Eval("AllowEnabled") %>' OnCheckedChanged="CbAllow_CheckedChanged" /></td>
                         <td><asp:Label CssClass="sn-path" ID="LabelAllowInheritsFrom" runat="server" Text='<%# Eval("AllowFrom") == null ? string.Empty : Eval("AllowFrom").ToString().Substring(Eval("AllowFrom").ToString().LastIndexOf("/") + 1) %>' ToolTip='<%# Eval("AllowFrom") %>' /></td>    	                   
                         <td class="center"><asp:CheckBox ID="CbPermissionDeny" runat="server" Checked='<%# Eval("Deny") %>' Enabled='<%# Eval("DenyEnabled") %>' OnCheckedChanged="CbDeny_CheckedChanged" /></td>
                         <td><asp:Label CssClass="sn-path" ID="LabelDenyInheritsFrom" runat="server" Text='<%# Eval("DenyFrom") == null ? string.Empty : Eval("DenyFrom").ToString().Substring(Eval("DenyFrom").ToString().LastIndexOf("/") + 1) %>' ToolTip='<%# Eval("DenyFrom") %>' /></td>    	                   
                         <td class="center"><span class="ui-icon ui-icon-check" title="Allow"></span><span class="ui-icon ui-icon-closethick" title="Deny"></span></td>
                       </tr>
                   </ItemTemplate>
              </asp:ListView>   
           </div>
         
         </div>
         </asp:PlaceHolder>   
   
   </ItemTemplate>
</asp:ListView>   

<sn:InlineScript ID="InlineScript1" runat="server">
<script type="text/javascript">

    $(function () {

        // Initialize permission tables
        $(".sn-permissions").each(function () {
            var $table = $(this);
            var $allowall = $("tr:first th:nth-child(2) :checkbox", this);
            var $denyall = $("tr:first th:nth-child(4) :checkbox", this);
            var $effectiveallow = $(".ui-icon-check", this);
            var $effectivedeny = $(".ui-icon-closethick", this);
            var $allowcheckboxes = $("tr td:nth-child(2) :checkbox", this);
            var $denycheckboxes = $("tr td:nth-child(4) :checkbox", this);
            var $enabled_allowcheckboxes = $allowcheckboxes;
            var $enabled_denycheckboxes = $denycheckboxes;
            var $required_allowcheckboxes = $allowcheckboxes.filter(":lt(3)"); // select first 3 checkboxes
            var $required_denycheckboxes = $denycheckboxes.filter(":lt(3)"); // select first 3 checkboxes
            var $operation_allowcheckboxes = $allowcheckboxes.filter(":gt(2):lt(8)"); // select 8 checkboxes from the 4th
            var $operation_denycheckboxes = $denycheckboxes.filter(":gt(2):lt(8)"); // select 8 checkboxes from the 4th
            var $permission_allowcheckboxes = $allowcheckboxes.filter(":gt(10):lt(2)"); // select 2 checkboxes from the 12th
            var $permission_denycheckboxes = $denycheckboxes.filter(":gt(10):lt(2)"); // select 2 checkboxes from the 12th
            var $save_add_del_allowcheckboxes = $allowcheckboxes.filter(":eq(3),:eq(6),:eq(8)");
            var $save_add_del_denycheckboxes = $denycheckboxes.filter(":eq(3),:eq(6),:eq(8)");
            var $managelist_allowcheckboxes = $allowcheckboxes.filter(":eq(14)");
            var $managelist_denycheckboxes = $denycheckboxes.filter(":eq(14)");

            function snRefreshPermissionCol() {
                $allowcheckboxes.each(function (idx) {
                    if ($denycheckboxes.eq(idx).is(":checked")) {
                        $effectivedeny.eq(idx).show();
                        $effectiveallow.eq(idx).hide();
                    } else {
                        if ($allowcheckboxes.eq(idx).is(":checked")) {
                            $effectivedeny.eq(idx).hide();
                            $effectiveallow.eq(idx).show();
                        } else {
                            $effectivedeny.eq(idx).show();
                            $effectiveallow.eq(idx).hide();
                        }
                    }
                });
            }

            ($allowcheckboxes.filter(":not(:checked)").length > 0) ? $allowall.removeAttr("checked") : $allowall.attr("checked", "checked");
            ($enabled_allowcheckboxes.length > 0) ? $allowall.removeAttr("disabled") : $allowall.attr("disabled", "disabled");
            ($denycheckboxes.filter(":not(:checked)").length > 0) ? $denyall.removeAttr("checked") : $denyall.attr("checked", "checked");
            ($enabled_denycheckboxes.length > 0) ? $denyall.removeAttr("disabled") : $denyall.attr("disabled", "disabled");
            snRefreshPermissionCol();

            // Permission dependencies
            $required_allowcheckboxes.click(function () {
                var idx = $required_allowcheckboxes.index($(this));
                if ($(this).is(":checked")) {
                    $required_allowcheckboxes.slice(0, idx).attr("checked", "checked");
                    $required_denycheckboxes.slice(0, idx).filter(":enabled").removeAttr("checked");
                } else {
                    $required_allowcheckboxes.slice(idx).add($operation_allowcheckboxes).add($managelist_allowcheckboxes).removeAttr("checked");
                }
            });
            $required_denycheckboxes.click(function () {
                var idx = $required_denycheckboxes.index($(this));
                if ($(this).is(":checked")) {
                    $required_denycheckboxes.slice(idx).add($operation_denycheckboxes).add($managelist_denycheckboxes).attr("checked", "checked");
                    $required_allowcheckboxes.slice(idx).add($operation_allowcheckboxes).add($managelist_allowcheckboxes).filter(":enabled").removeAttr("checked");
                } else {
                    $required_denycheckboxes.slice(0, idx).removeAttr("checked");
                }
            });
            $operation_allowcheckboxes.click(function () {
                if ($(this).is(":checked")) {
                    $required_allowcheckboxes.attr("checked", "checked");
                    $required_denycheckboxes.removeAttr("checked");
                }
            });
            $operation_denycheckboxes.click(function () {
                if (!$(this).is(":checked")) {
                    $required_denycheckboxes.removeAttr("checked");
                }
            });

            $permission_allowcheckboxes.click(function () {
                var idx = $permission_allowcheckboxes.index($(this));
                if ($(this).is(":checked")) {
                    $permission_allowcheckboxes.slice(0, idx).attr("checked", "checked");
                    $permission_denycheckboxes.slice(0, idx).filter(":enabled").removeAttr("checked");
                } else {
                    $permission_allowcheckboxes.slice(idx).removeAttr("checked");
                }
            });
            $permission_denycheckboxes.click(function () {
                var idx = $permission_denycheckboxes.index($(this));
                if ($(this).is(":checked")) {
                    if ($allowall.is(":enabled")) $permission_denycheckboxes.slice(idx).attr("checked", "checked");
                    $permission_allowcheckboxes.slice(idx).filter(":enabled").removeAttr("checked");
                } else {
                    $permission_denycheckboxes.slice(0, idx).removeAttr("checked");
                }
            });

            $managelist_allowcheckboxes.click(function () {
                if ($(this).is(":checked")) {
                    $required_allowcheckboxes.attr("checked", "checked");
                    $save_add_del_allowcheckboxes.attr("checked", "checked");

                    $required_denycheckboxes.filter(":enabled").removeAttr("checked");
                    $save_add_del_denycheckboxes.filter(":enabled").removeAttr("checked");
                }
            });
            $save_add_del_allowcheckboxes.click(function () {
                if (! $(this).is(":checked")) {
                    $managelist_allowcheckboxes.filter(":enabled").removeAttr("checked");
                }
            });
            $save_add_del_denycheckboxes.click(function () {
                if ($(this).is(":checked")) {
                    $managelist_allowcheckboxes.filter(":enabled").removeAttr("checked");
                }
            });
                        

            $allowall.click(function () {
                if ($(this).is(":checked")) {
                    $allowcheckboxes.filter(":enabled").attr("checked", "checked");
                    if ($denyall.is(":enabled")) $denyall.removeAttr("checked");
                    $denycheckboxes.filter(":enabled").removeAttr("checked");
                } else {
                    $allowcheckboxes.removeAttr("checked")
                        .filter("[data-inheritvalue]").attr("disabled", "disabled")
                        .filter("[data-inheritvalue='true']").attr("checked", "checked");
                }
                snRefreshPermissionCol();
            });
            $denyall.click(function () {
                if ($(this).is(":checked")) {
                    $denycheckboxes.attr("checked", "checked");
                    if ($allowall.is(":enabled")) $allowall.removeAttr("checked");
                    $allowcheckboxes.filter(":enabled").removeAttr("checked");
                } else {
                    $denycheckboxes.removeAttr("checked")
                        .filter("[data-inheritvalue]").attr("disabled", "disabled")
                        .filter("[data-inheritvalue='true']").attr("checked", "checked");
                }
                snRefreshPermissionCol();
            });

            $allowcheckboxes.click(function (index) {
                var $this = $(this);
                var idx = $allowcheckboxes.index($this);
                if ($this.is(":checked")) {
                    if ($denycheckboxes.eq(idx).is(":enabled")) $denycheckboxes.eq(idx).removeAttr("checked");
                    if ($denyall.is(":enabled")) $denyall.removeAttr("checked");
                    if ($allowcheckboxes.filter(":not(:checked)").length == 0) $allowall.attr("checked", "checked");
                } else {
                    if ($allowall.is(":enabled")) $allowall.removeAttr("checked");
                }
                snRefreshPermissionCol();
            });

            $denycheckboxes.click(function () {
                var $this = $(this);
                var idx = $denycheckboxes.index($this);
                if ($this.is(":checked")) {
                    if ($allowcheckboxes.eq(idx).is(":enabled")) $allowcheckboxes.eq(idx).removeAttr("checked");
                    if ($allowall.is(":enabled")) $allowall.removeAttr("checked");
                    if ($denycheckboxes.filter(":not(:checked)").length == 0) $denyall.attr("checked", "checked");
                } else {
                    if ($denyall.is(":enabled")) $denyall.removeAttr("checked");
                }
                snRefreshPermissionCol();
            });

        });

    });
</script>
</sn:InlineScript>

</ContentTemplate>
</asp:UpdatePanel>

<div style="clear:both;"></div>

<div class="sn-panel sn-buttons">
    <asp:Button CssClass="sn-submit" ID="ButtonSave" runat="server" Text="<%$ Resources: Portal, PermEditor_BtnSave %>" OnClick="ButtonSave_Click" />
    <sn:BackButton Text="<%$ Resources: Portal, PermEditor_BtnCancel %>" ID="BackButton1" runat="server" CssClass="sn-submit" />
</div>

</div>
