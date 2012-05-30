<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<sn:ShortText runat="server" ID="ShortTextField" FieldName="ShortTextField" />
<sn:LongText runat="server" ID="LongTextField" FieldName="LongTextField" />
<sn:Number runat="server" ID="NumberField" FieldName="NumberField" />
<sn:WholeNumber runat="server" ID="IntegerField" FieldName="IntegerField" />
<sn:Boolean runat="server" ID="BooleanField" FieldName="BooleanField" />
<sn:DropDown runat="server" ID="ChoiceField" FieldName="ChoiceField" />
<sn:RadioButtonGroup runat="server" ID="ChoiceField2" FieldName="ChoiceField" />
<sn:DatePicker runat="server" ID="DateTimeField" FieldName="DateTimeField" />
<sn:ReferenceGrid runat="server" ID="ReferenceField" FieldName="ReferenceField" />
<sn:Binary runat="server" ID="BinaryField" FieldName="BinaryField" />


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
  <sn:CommandButtons ID="CommandButtons1" runat="server"/>
</div>
