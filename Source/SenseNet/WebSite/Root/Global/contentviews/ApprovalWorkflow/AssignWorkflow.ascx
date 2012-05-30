<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<div class="sn-content sn-content-inlineview">

    <sn:GenericFieldControl runat="server" ID="GenericFieldcontrol1" FieldsOrder="DisplayName Description FirstLevelApprover FirstLevelTimeFrame SecondLevelApprover SecondLevelTimeFrame WaitForAll" />        

</div>
<asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>

<div class="sn-panel sn-buttons">
  <asp:Button class="sn-submit" ID="AssignWorkflow" runat="server" Text="Assign to list" />
  <sn:BackButton class="sn-submit" ID="Cancel" runat="server" Text="Cancel" />
</div>