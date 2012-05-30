<%@  Language="C#" %>
<sn:ScriptRequest runat="server" Path="/Root/Global/scripts/jquery/plugins/password_strength_plugin.js" />

<asp:TextBox CssClass="sn-ctrl sn-ctrl-text sn-ctrl-password" ID="InnerPassword1" runat="server" TextMode="Password" /><br />
<asp:TextBox CssClass="sn-ctrl sn-ctrl-text sn-ctrl-password2" ID="InnerPassword2" runat="server" TextMode="Password" />
<sn:InlineScript runat="server">
<script type="text/javascript">
	$.fn.shortPass = 'Too short';
	$.fn.badPass = 'Weak';
	$.fn.goodPass = 'Good';
	$.fn.strongPass = 'Strong';
	$.fn.samePassword = 'Username and Password identical.';

    $(function () {
        $("input.sn-ctrl-password, input.sn-ctrl-password2").passStrength({ userid: ".sn-ctrl-username" });
    });
</script>
</sn:InlineScript>
