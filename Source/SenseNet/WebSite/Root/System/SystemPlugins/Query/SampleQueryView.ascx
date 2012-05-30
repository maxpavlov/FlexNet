<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.Controls.QueryView" %>
<%@ Register Src="/Root/System/SystemPlugins/Query/PagerControl.ascx" TagName="PagerControl" TagPrefix="sn" %>
<%-- 
Query example:

<?xml version="1.0" encoding="utf-8"?>
<SearchExpression xmlns="http://schemas.sensenet.com/SenseNet/ContentRepository/SearchExpression">
  <And>
    <Type nodeType="WebContentDemo" />
  </And>
</SearchExpression>

--%>

<div class="sn-contentlist">	
	
	<asp:UpdatePanel id="updSampleQuery" UpdateMode="Conditional" runat="server">
	    <ContentTemplate>
	    
	    <sn:PagerControl id="pagerHeader" runat="server" ShowUnusedLinks="True" MorePagesSkipCount="3" VisiblePageCount="4" EnableSettingPageSize="False"/>
	    
		<sn:RepeaterView ID="RepeaterView1" runat="server">
			<ItemTemplate>

				<div class="sn-content sn-contentlist-item">
						
						    <h1 class="sn-content-title"><a href="/features/News/Article?Content=<%# GetValue(Container.DataItem, "Path") %>"><%# GetValue(Container.DataItem, "DisplayName")%></a></h1>

						<div class="sn-content-header">
    						<%# GetValue(Container.DataItem, "Header") %>
    					</div>

					    <div class="sn-more">
						    <a class="sn-link" href="/features/News/Article?Content=<%# GetValue(Container.DataItem, "Path") %>">more &raquo;</a>
					    </div>
					    
				</div>
	
			</ItemTemplate>
		</sn:RepeaterView>

        <sn:PagerControl id="pagerFooter" runat="server" ShowUnusedLinks="True" MorePagesSkipCount="3" VisiblePageCount="4"/>
        
        </ContentTemplate>
    </asp:UpdatePanel>
</div>