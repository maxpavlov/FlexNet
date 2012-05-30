<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>

<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true" ID="myContext" />

<div class="sn-article-content">
    
    <div class="sn-content-actions">
        <sn:ActionLinkButton ID="ActionLinkButton1" runat="server" IconUrl="/Root/Global/images/icons/16/edit.png" ActionName="Edit" Text="Edit" ContextInfoID="myContext" /> 
        <sn:ActionLinkButton ID="ActionLinkButton2" runat="server" ActionName="Delete" Text="Delete" ContextInfoID="myContext" IncludeBackUrl="False" ParameterString="backtarget=currentworkspace" /> 
        <sn:ActionLinkButton ID="ActionLinkButton3" runat="server" ActionName="Versions" Text="History" ContextInfoID="myContext" /> 
    </div>
    
    <% if (!String.IsNullOrEmpty(GetValue("DisplayName"))) { %><h1 class="sn-content-title sn-article-title"><%=GetValue("DisplayName") %></h1><% } %>
        

    
    <div class="sn-article-lead sn-richtext">
        <sn:RichText ID="RichText1" FieldName="WikiArticleText" runat="server" />
    </div>

    <div class="sn-inputunit ui-helper-clearfix" style="border-top-color:#e7e7e7;border-top-width:1px;border-top-style:solid;top-marging:3px;" >
        <div>Modified: <b><%=GetValue("ModificationDate") %></b>, <%=GetValue("ModifiedBy") %></div>
     </div>

</div>

    
    
