<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<sn:ShortText runat="server" ID="ShortTextField" FieldName="ShortTextField" FrameMode="ShowFrame" />
<sn:LongText runat="server" ID="LongTextField" FieldName="LongTextField" FrameMode="ShowFrame" />
<sn:Number runat="server" ID="NumberField" FieldName="NumberField" FrameMode="ShowFrame" />
<sn:WholeNumber runat="server" ID="IntegerField" FieldName="IntegerField" FrameMode="ShowFrame" />
<sn:Boolean runat="server" ID="BooleanField" FieldName="BooleanField" FrameMode="ShowFrame" />
<sn:DropDown runat="server" ID="ChoiceField" FieldName="ChoiceField" FrameMode="ShowFrame" />
<sn:RadioButtonGroup runat="server" ID="ChoiceField2" FieldName="ChoiceField" FrameMode="ShowFrame" />
<sn:DatePicker runat="server" ID="DateTimeField" FieldName="DateTimeField" FrameMode="ShowFrame" />
<sn:ReferenceGrid runat="server" ID="ReferenceField" FieldName="ReferenceField" FrameMode="ShowFrame" />
<sn:Binary runat="server" ID="BinaryField" FieldName="BinaryField" FrameMode="ShowFrame" />

<!-- or use
  ShortTextField: <%= GetValue("ShortTextField") %>
-->

<%-- template example:
<sn:ShortText runat="server" ID="ShortTextField" FieldName="ShortTextField">
  <EditTemplate>
    <asp:TextBox ID="InnerShortText" runat="server"></asp:TextBox>
  </EditTemplate>
</sn:ShortText>
--%>

<%-- generic field control:  
<sn:GenericFieldControl runat="server" ID="GenericFieldControl1" ExcludedFields="Field1 Field2" />
--%>

<div class="sn-panel sn-buttons">
    <sn:BackButton CssClass="sn-submit" Text="Done" ID="BackButton1" runat="server" />
</div>