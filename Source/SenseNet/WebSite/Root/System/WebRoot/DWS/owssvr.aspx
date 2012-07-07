<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server" type="text/C#">
    public string GetFilter()
    {
        var filter = HttpUtility.ParseQueryString(SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri.Query).Get("FileDialogFilterValue");
        if (String.IsNullOrEmpty(filter)) return String.Empty;
        return String.Concat(" +(", String.Join(" OR ", filter.Split(';').Select(f => "Name:" + f)), ")");
    }
</script>
<% 
    //if (SenseNet.Portal.Dws.DwsHelper.CheckVisitor())
    //    return;

    var mv = this.FindControl("mvFileDialog") as MultiView;
    var uri = SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri;
    if (mv == null)
    {
        HttpContext.Current.Response.StatusCode = 404;
        HttpContext.Current.Response.End();
        return;
    }
    var location = HttpUtility.ParseQueryString(uri.Query).Get("location");
        if (!String.IsNullOrEmpty(location)) location = "/" + location;
    
    var reqContentPath = uri.AbsolutePath.Replace("/_vti_bin/owssvr.dll", String.Empty) + location;
    if (reqContentPath.StartsWith("/Root"))
    {
        mv.ActiveViewIndex = 1;
    }
    else
    {
        // main screen
        mv.ActiveViewIndex = 0;
    }
    
%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <meta http-equiv="Expires" content="0">
    <title id="onetidTitle">File Properties</title>
    <style type="text/css">
        html,body { margin:0;padding:0; min-height: 100% }
        body {
            font: 14px/1.2 Arial, Helvetica, sans-serif;
            color: #222;
            padding: 0 0 10px;
            background: url(/Root/Global/images/sn_bg.jpg) repeat-x 0 200px;
            cursor: default;
            -webkit-user-select: none;
            -khtml-user-select: none;
            -moz-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        #header {
            background:url(/Root/Global/images/sensenetlogo.png) no-repeat 14px 10px #007dc2;
            height: 60px;
            overflow: hidden; text-indent: -3000em;
            position: fixed; left:0px; right: 0px;
        }
        h2 { font-size: 18px; font-weight: normal; margin: 0 0 5px 0; padding: 10px 10px 5px 10px; border-bottom: 1px solid #ccc;}
        #content { padding: 67px 10px 0; }
        table {
            margin: 0; padding: 5px;
            border:1px solid transparent; border-collapse:collapse;
            width: 100%;
            font: 12px/1.2 Arial, Helvetica, sans-serif;
            cursor: default;
            empty-cells: show;
        }
        table tr.odd { background-color: #f9f9f9; }
        table tr.even { background-color: #fff; }
        table td, table th { text-align: left; padding: 4px;}
        table thead td { padding-left: 0; padding-right: 0; }
        table thead th { padding:8px 4px; border-bottom: 1px solid #ccc; }
        table tbody td:first-child, table th:first-child, table tfoot td:first-child { padding-left: 10px;}
        table tbody tr:hover { background-color: #f0f0f0; }
        table tfoot td { padding-top: 15px; border-top: 1px solid #ccc; }
        .sn-icon { vertical-align: middle;}
        .sn-icon32 { margin: 0 5px 0 0; } 
        .sn-icon-back { margin: 0 5px 0 0; } 
    </style>
</head>
<body servertype="OWS" doclibslist="1" onselectstart='return false;'>
    <div id="header">Sense/Net</div>
    <div id="content">
    <asp:MultiView runat="server" ID="mvFileDialog">
        <asp:View runat="server" ID="viewBrowseRoot">
            <% var host = SenseNet.Portal.Dws.DwsHelper.GetHostStr(); %>
            <% var site = SenseNet.Portal.Virtualization.PortalContext.Current.Site; %>
            <table id="FileDialogViewTable">
                <thead>
                <tr>
                    <td>
                        <h2><%=site.DisplayName%></h2>
                    </td>
                </tr>
                </thead>
                <tbody>
                <tr fileattribute="folder" id="<%=host%><%=site.Path%>">
                    <td><img src="/Root/Global/images/icons/32/search.png" class="sn-icon sn-icon32" /> Browse Content Repository</td>
                </tr>
                </tbody>
                <thead>
                <tr>
                    <td>
                        <h2>Document Workspaces</h2>
                    </td>
                </tr>
                </thead>
                <tbody>
                <% foreach (var ws in SenseNet.Search.ContentQuery.Query("TypeIs:DocumentWorkspace").Nodes)
                   { %>
                <tr fileattribute="folder" id="<%=host%><%=ws.Path%>">
                    <td><img src="/Root/Global/images/icons/32/workspace-document.png" class="sn-icon sn-icon32" /> <%= ws.DisplayName %></td>
                </tr>
                <%  } %>
                </tbody>
            </table>
        </asp:View>
        <asp:View runat="server" ID="viewBrowseWorkspace">
            <% var host = SenseNet.Portal.Dws.DwsHelper.GetHostStr(); %>
            <% var site = SenseNet.Portal.Virtualization.PortalContext.Current.Site; %>
            <% var uri = SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri; %>
            <% var location = HttpUtility.ParseQueryString(uri.Query).Get("location");
               if (!String.IsNullOrEmpty(location)) location = "/" + location;
            %>
            <% var reqContentPath = uri.AbsolutePath.Replace("/_vti_bin/owssvr.dll", String.Empty) + location; %>
            <% var reqContentHead = SenseNet.ContentRepository.Storage.NodeHead.Get(reqContentPath); %>
            
            <table id="FileDialogViewTable">
                <thead>
                <tr>
                    <td colspan="4">
                        <h2><%= reqContentHead.DisplayName%></h2>
                    </td>
                </tr>
                <tr>
                    <th colspan="2">Name</th>
                    <th>Modified by</th>
                    <th>Modification date</th>
                </tr>
                </thead>
                <% var idx = 0;
                   var Nodes = SenseNet.Search.ContentQuery.Query(String.Format("(+TypeIs:Folder +InFolder:{0}) OR (+TypeIs:File +InFolder:{0} {1}) .SORT:DisplayName", reqContentPath, GetFilter())).Nodes;
                   if (Nodes.LongCount() > 0) { %>
                <tbody>
                <% foreach (var cnt in Nodes.Select(node => SenseNet.ContentRepository.Content.Create(node))) { %>
                <tr fileattribute="<%= cnt.ContentHandler is SenseNet.ContentRepository.IFile ? "file" : "folder" %>" id="<%=host%><%=cnt.Path%>" class='<%=idx++%2==0?"odd":"even"%>'>
                    <td style="width:16px;" align="center">
                        <img src="/Root/Global/images/icons/16/<%= cnt.ContentHandler is SenseNet.ContentRepository.IFile ? cnt.Icon : "folder" %>.png" />
                    </td>
                    <td><%= cnt.DisplayName%></td>
                    <td><%= (cnt.Fields["ModifiedBy"].GetData() as SenseNet.ContentRepository.User).FullName%></td>
                    <td><%= (cnt.Fields["ModificationDate"].GetData()).ToString()%></td>
                </tr>
                <% } %>
                </tbody>                   
                <% } %>
                <tfoot>
                <tr fileattribute="folder" id="<%=host%>">
                    <td align="center" colspan="4"><img src="/Root/Global/images/icons/16/reply.png" class="sn-icon sn-icon-back" /> Back to Workspaces</td>
                </tr>
                </tfoot>
            </table>
        </asp:View>
    </asp:MultiView>
    </div>
</body>
</html>
