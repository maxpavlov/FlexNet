<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="VotingWebControlCaptcha" Namespace="SenseNet.Toolbox.WebControlCaptcha" TagPrefix="cap" %>
<div class="sn-form ui-helper-clearfix">
     
    <h2 class="sn-form-title">
        <%= (this.Content.ContentHandler.Parent as SenseNet.Portal.Portlets.ContentHandlers.Form).DisplayName%>
    </h2>
    <div class="sn-form-description">
        <%= (this.Content.ContentHandler.Parent as SenseNet.Portal.Portlets.ContentHandlers.Form).Description%>
    </div>
    <div class="sn-form-fields">
        <sn:ErrorView ID="ErrorView1" runat="server" />
        <sn:GenericFieldControl runat=server ID="GenericFieldControl1" ContentListFieldsOnly="true" />
        <br />
        <sn:ShortText runat="server" ID = "Emailcim" FieldName="#Emailcim" />
        <sn:ShortText runat="server" ID = "TeljesNev" FieldName="#TeljesNev" />
        <p>Postacímed, ahova a küldeményt küldhetjük:</p>
        <sn:WholeNumber runat="server" ID = "Iranyitoszam" FieldName="#Iranyitoszam" />
        <sn:ShortText runat="server" ID = "Helyseg" FieldName="#Helyseg" />
        <sn:ShortText runat="server" ID = "Utcahazszam" FieldName="#Utcahazszam" />
        <sn:ShortText runat="server" ID = "Telefonszam" FieldName="#Telefonszam" />
        <p>Ezen tudjuk egyeztetni a nyeremények átadását</p>
        <cap:CaptchaControl ID="Captcha" runat="server" CaptchaHandlerPath="/Root/System/SystemPlugins/Controls/" />
        <p>Kérünk, hogy minden adatot tölts ki, és jelöld be, hogy elfogadod a szabályzatot, mert csak így tudod elküldeni a kérdőívet!</p>
        <p>Adatkezelési nyilvántartási szám: XXXXX-XXXX</p>
        <sn:CheckBoxGroup ID="AcceptTerms" FieldName="#AcceptTerms" runat = "server" />      
    </div>
    <div class="sn-form-comment">
        * compulsory field
    </div>
    <div class="sn-panel sn-buttons sn-form-buttons">
        <asp:Button ID="BtnSend" CssClass="sn-submit" runat="server" CommandName="save" Text="Send" EnableViewState="false" OnClick="Click" />        
    </div>
</div>
