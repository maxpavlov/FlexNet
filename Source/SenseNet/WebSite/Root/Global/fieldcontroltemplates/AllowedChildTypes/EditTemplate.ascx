<%@  Language="C#" %>
<asp:TextBox ID="InnerData" runat="server" style="display:none;" class="sn-allowedct-innerdata" />


<div id="sn-allowedct-inherit" style="padding-bottom:10px;margin-bottom:10px;border-bottom:1px solid #DDD;display:none;height:25px;" class='ui-helper-clearfix'>
    <div style="float:left; padding:4px;">
    Custom setting (not inherited from CTD)
    </div> 
    <input type="submit" value="Inherit from CTD" class="sn-button sn-submit" onclick="$('.sn-allowedct-innerdata').val('');" style="float:right;" />
</div>

<div id="sn-allowedct-container"></div>

<div class='ui-helper-clearfix' style="padding-top: 10px;margin-top:10px;border-top: 1px solid #DDD;">
    <input class="sn-dropbox-createother sn-unfocused-postbox" onfocus="SN.ACT.onfocusPostBox($(this));" onblur="SN.ACT.onblurPostBox($(this));" onkeydown="if (event.keyCode == 13) return false;" onkeyup="SN.ACT.onchange(event, $(this)); return false;" type="text" value="Start typing..."/>
    <a style="vertical-align:baseline;" class='sn-dropbox-createothers-showalllink' tabindex='-1' href='javascript:' title='Show all types' onclick='SN.ACT.showAll();'><img class='sn-dropbox-createothers-showallimg' src='/Root/Global/images/actionmenu_down.png'/></a>
    <input class="sn-allowedct-createother-value" type="hidden" />
    <input class="sn-allowedct-createother-path" type="hidden" />
    <input class="sn-allowedct-createother-icon" type="hidden" />
    <input type="button" class="sn-dropbox-createother-submit sn-submit sn-button sn-notdisabled" value="Add" onclick="SN.ACT.addType();" /><input type="button" class="sn-dropbox-createother-submitfake sn-submit sn-button sn-notdisabled sn-disabled" disabled="disabled" value="Add" />
    <div class="sn-dropbox-createother-autocomplete"></div>
</div>
