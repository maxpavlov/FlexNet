<%@ Page Language="C#" AutoEventWireup="true" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" style="height: 100%;">
<head>
    
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <script src="/Root/Global/scripts/sn/SN.js" type="text/javascript"></script>
    <script src="/Root/Global/scripts/jquery/jquery.js" type="text/javascript"></script>
    <script src="/Root/Global/scripts/jqueryui/minified/jquery-ui.min.js" type="text/javascript"></script>
    <script src="/Root/Global/scripts/jquery/plugins/tree/jquery.tree.js" type="text/javascript"></script>
    <script src="/Root/Global/scripts/sn/SN.ExploreTree.js" type="text/javascript"></script>
    <link href="/Root/Global/styles/widgets/jqueryui/jquery-ui.css" rel="Stylesheet" type="text/css" media="all" />
    <link href="/Root/Global/styles/widgets.css" rel="Stylesheet" type="text/css" media="all" />
    <link href="/Root/Global/scripts/jquery/plugins/tree/themes/sn/style.css" rel="Stylesheet" type="text/css" media="all" />
</head>

<body class="sn-exploretree-body sn-admin">
        <form id="form1" runat="server">
            <div id="sn-exploretree-controls" class="ui-helper-clearfix">
                <span id="sn-exploretree-currentsite" style="display:none;"><%= (SenseNet.Portal.Virtualization.PortalContext.Current != null && SenseNet.Portal.Virtualization.PortalContext.Current.Site != null) ? SenseNet.Portal.Virtualization.PortalContext.Current.Site.Path : string.Empty %></span>
                <a href="javascript:;" id="sn-exploretree-closebutton" class="sn-button sn-submit" onclick="parent.location = parent.frames['ExploreFrame'].location;">Close</a>
                <div id="sn-exploretree-treeactions">
                    <input type="radio" id="sn-treeaction-explore" name="sn-exploretree-treeactions" value="Explore" checked="checked" /><label for="sn-treeaction-explore">Explore</label> 
                    <input type="radio" id="sn-treeaction-browse" name="sn-exploretree-treeactions" value="Browse" /><label for="sn-treeaction-browse">Browse</label> 
                    <input type="radio" id="sn-treeaction-edit" name="sn-exploretree-treeactions" value="Edit" /><label for="sn-treeaction-edit">Edit</label>
                </div>
                <input type="text" id="sn-exploretree-urlbox" class="ui-widget-content ui-corner-all" />
                <dl class="sn-exploretree-info sn-admin ui-widget ui-helper-reset ui-helper-clearfix">
                    <dt>Content:</dt><dd><input type="text" readonly="readonly" id="sn-exploretree-contentname" /></dd>
                    <dt>Action:</dt><dd><span id="sn-exploretree-actionname"></span></dd>
                </dl>
                <div id="sn-exploretree-search">
                    <input type="text" id="sn-exploretree-searchbox" class="ui-widget-content ui-corner-all" onkeypress="SubmitSearch(event);" />
                    <input id="sn-exploretree-searchbutton" class="sn-button sn-submit" type="button" value="Search" onclick="ExploreSearch();" />
                </div>
            </div>            
            <div id="sn-exploretree-treediv" class="ui-widget ui-widget-content ui-corner-all">
                <div id="sn-exploretree-treerootdiv">
                    Tree root:
                    <span id="sn-exploretree-treeroottextdiv"></span>
                    <span id="sn-exploretree-treerootselectdiv">
                        <select id="sn-exploretree-treeroot" onchange="SN.ExploreTree.SelectTreeRoot();">
                        </select>
                    </span>
                </div>
                <div id="sn-exploretree-treerootshowall">
                    <label for="sn-exploretree-treeshowall"><input id="sn-exploretree-treeshowall" type="checkbox" onclick="SN.ExploreTree.ToggleHidden(this.checked)" checked="checked" />Show system files</label>
                </div>
                <div id="sn-exploretree-treecontainer">
                    <div id="sn-exploretree-tree"></div>
                </div>
            </div>
        </form>

<script type="text/javascript" language="javascript">

    function GetQueryParam(url, name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
        var r = url.search.substr(1).match(reg);
        if (r != null) return unescape(r[2]); return null;
    }

    function ExploreSearch() {
        var searchText = encodeURIComponent($('#sn-exploretree-searchbox').val());

        if (searchText != "") {
            var contentPath = $('#sn-exploretree-contentname').val();
            var exploreFrameUrl = parent.frames['ExploreFrame'].location;

            // Looking for existing back url parameter
            if (GetQueryParam(exploreFrameUrl, "action").toLowerCase() == "exploresearch") {
                var back = GetQueryParam(exploreFrameUrl, "back");
            } else {
                var back = encodeURIComponent(exploreFrameUrl);
            }

            var mode = $("#sn-exploretree-treeactions input:checked").val();
            parent.frames['ExploreFrame'].location = contentPath + '?action=ExploreSearch&text=' + searchText + "&back=" + back + "&mode=" + mode;
        }
    }

    function SubmitSearch(e) {
        if (e.keyCode == 13) {
            ExploreSearch();
        }
    } 
</script>
</body>
</html>
