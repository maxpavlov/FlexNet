<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import Namespace="System.Collections.Generic"%>
<%@ Import namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>
<% string user = (SenseNet.ContentRepository.User.Current).ToString(); %>

<script>
    $(function () {
        $('.sn-close').click(function () {
            $('.sn-login-overlay').css('display', 'none');
            $('.sn-login-overlay-window').addClass('.sn-hide').css('display', 'none');
            $('.secondcolumn').css('display', 'block');
            $('.sn-column-half').removeClass('secondcolumn');
        });
    });
</script>

<%if (user == "Visitor")
  {%>
  <div style="position:relative">
   <div class="sn-login-overlay">
   </div>
   <div class="sn-login-overlay-window">
        <div class="sn-pt-body-border ui-widget-content ui-corner-all"> 
        <img class="sn-close" src="/Root/Global/images/icons/16/delete.png" style="margin-left: 170px;cursor:pointer;" />
      <div class="sn-pt-body ui-corner-all">
        <%=GetGlobalResourceObject("Portal", "WSContentList_Visitor")%>
      </div>
    </div>
   </div>
   </div>
<% }%>

