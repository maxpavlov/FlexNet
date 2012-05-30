<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>

<div class="sn-survey-evaluation">

    <p class="sn-iu-title"><%=GetGlobalResourceObject("Survey", "TypeEvaluation")%></p>

    <sn:LongText ID="EvaluationText" runat="server" FieldName="Evaluation" RenderMode="InlineEdit">
        <EditTemplate>
            <asp:TextBox CssClass="sn-ctrl sn-ctrl-text" ID="InnerControl" runat="server" TextMode="MultiLine" Rows="10" Width="728"></asp:TextBox>
        </EditTemplate>
    </sn:LongText>

    <div class="sn-panel sn-buttons">
        <sn:CommandButtons runat="server" ID="CommandButtons1" />
    </div>

    <hr />

    <div class="sn-content">
        <h3 class="sn-content-title"><%= GetValue("DisplayName")%></h3>
        <div class="sn-lead"><%= GetValue("Description")%></div>
    </div>

    <sn:GenericFieldControl ID="GenericField1" runat="server" ContentListFieldsOnly="True" />

</div>
