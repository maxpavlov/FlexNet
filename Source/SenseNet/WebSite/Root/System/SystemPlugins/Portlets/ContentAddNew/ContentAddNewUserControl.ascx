<%@ Language="C#" AutoEventWireup="true" %>

<style type="text/css">
    .sn-contentTypesColumn {float:left; margin-right: 10px; width:210px;}
    .sn-contentTypesColumnInitial {font-size:16px; margin-top:5px;}
</style>

<asp:Panel ID="AdvancedPanel" runat="server">
    <% 
        var portlet = this.Parent as SenseNet.Portal.Portlets.ContentAddNewPortlet;
        var contextContent = SenseNet.ContentRepository.Content.Create(portlet.ContextNode);
        var groups = portlet.ContentTypes.GroupBy(a => a.Text[0].ToString().ToLower());
        var maxColumnCount = (portlet.ContentTypes.Count() + groups.Count() * 2) / 4;
        var contentTypesStringList = portlet.ContentTypes.Select(c => "{ text: \"" + c.Text + "\", link:\"" + portlet.GetRedirUrl(c.Value) + "\"}");
        var contentTypesString = "["+String.Join(",", contentTypesStringList)+"]";
    %>
    <div style="margin-bottom:10px;">
        <input id="contenttypesearch" type="text" style="width:400px;" />
        <asp:Button id="contenttypeaddnew" runat="server" class="contenttypeaddnew sn-submit" value="Add new" style="display:none;" />
        <asp:Button ID="CancelSelectContentTypeButton2" CssClass="sn-submit" runat="server" Text="Cancel" />
    </div>
    <div id="sn-contentTypes-all">
        <div class="sn-contentTypesColumn">
            <% 
               var actCount = 0;
               foreach (var group in groups)
               {
                   actCount+=2;
                   %><div class="sn-contentTypesColumnInitial"><%=group.First().Text[0]%></div><%
                   foreach (var item in group)
                   {
                       %><a href="<%=portlet.GetRedirUrl(item.Value) %>"><%=item.Text%></a><br /><%
                       actCount++;
                       if (actCount > maxColumnCount)
                       {
                           actCount = 0;
                           %></div><div class="sn-contentTypesColumn"><%
                       }
                   }
               }
            %>
        </div>
        <div style="clear:both;"></div>
    </div>
        <script type="text/javascript">
            var ctypesOriginalHtml = $('#sn-contentTypes-all').html();
            var snanctcontenttypes = <%=contentTypesString %>;
            var snanctmaxcolumn = <%=maxColumnCount %>;
            var contenttypelink = -1;
            $('.contenttypeaddnew').click(function() {  
                if (contenttypelink == -1)
                    return false;

                window.location = $('#contenttypelink0').attr('href');
                return false;
            });
            $('#contenttypesearch').keyup(function(event) {
                if (event.keyCode == 13) {
                    if (contenttypelink == -1)
                        return false;

                    window.location = $('#contenttypelink0').attr('href');
                    return false;
                }

                var newTypes = [];
                var val = $('#contenttypesearch').val();
                var exactResult = -1;
                var matchers = val.toLowerCase().split(' ');
                for(var i=0;i<snanctcontenttypes.length;i++) {
                    var match = true;
                    for (var j=0;j<matchers.length;j++) {
                        if (snanctcontenttypes[i].text.toLowerCase().indexOf(matchers[j].toLowerCase()) == -1) {
                            match = false;
                            break;
                        }
                    }
                    if (match) {
                        newTypes.push(snanctcontenttypes[i]);
                        if (snanctcontenttypes[i].text.toLowerCase() == val.toLowerCase()) {
                            exactResult = newTypes.length - 1;
                        }
                    }
                }
                if (val == '' || newTypes.length == snanctcontenttypes.length) {
                    $('#sn-contentTypes-all').html(ctypesOriginalHtml);
                } else {
                    var html='<div class="sn-contentTypesColumn">';
                    var maxColumnCount = snanctmaxcolumn;
                    var actCount = 0;
                    var prevInitial = '';
                    for (var i=0;i<newTypes.length;i++) {
                        if (prevInitial.toLowerCase() != newTypes[i].text.charAt(0).toLowerCase()) {
                            prevInitial = newTypes[i].text.charAt(0).toUpperCase();
                            html += '<div class="sn-contentTypesColumnInitial">'+prevInitial+'</div>';
                            actCount += 2;
                        }
                        html += '<a id="contenttypelink'+i+'" href="'+newTypes[i].link+'">'+newTypes[i].text+'</a><br />';
                        actCount++;
                        if (actCount > maxColumnCount) {
                            actCount = 0;
                            html += '</div><div class="sn-contentTypesColumn">';
                        }
                    }
                    html += '</div><div style="clear:both;"></div>';
                    $('#sn-contentTypes-all').html(html);
                }

                // show/hide add new button
                var $btn = $('.contenttypeaddnew');
                if (newTypes.length == 1 || exactResult != -1) {
                    $btn.val('Add new '+newTypes[0].text);
                    if (exactResult != -1)
                        contenttypelink = exactResult;
                    else 
                        contenttypelink = 0;
                    $btn.show();
                } else {
                    $btn.hide();
                }
            });
            $('#contenttypesearch').focus();
        </script>
</asp:Panel>

<asp:Label ID="ErrorMessage" runat="server" CssClass="sn-error"></asp:Label>
<asp:DropDownList ID="ContentTypeList" runat="server"></asp:DropDownList>
<asp:Button ID="SelectContentTypeButton" CssClass="sn-submit" runat="server" Text="Select" />
<asp:Button ID="CancelSelectContentTypeButton" CssClass="sn-submit" runat="server" Text="Cancel" />
<asp:PlaceHolder ID="ContentViewPlaceHolder" runat="server"></asp:PlaceHolder>