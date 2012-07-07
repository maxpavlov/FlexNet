
    <%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.ContentListViews.ListView" %>
    <%@ Import Namespace="SNCR=SenseNet.ContentRepository" %>
    <%@ Import Namespace="SenseNet.Portal.UI.ContentListViews" %>
    <%@ Import Namespace="System.Linq" %>
    <%@ Import Namespace="SCR=SenseNet.ContentRepository.Fields" %>
    
    <sn:CssRequest ID="gallerycss" runat="server" CSSPath="/Root/Global/styles/slides.css" />
     <sn:CssRequest ID="gallerycss2" runat="server" CSSPath="/Root/Global/styles/prettyPhoto.css" />
    <sn:ScriptRequest runat="server" Path="/Root/Global/scripts/jquery/plugins/jquery.easing.1.3.js" />
    <sn:ScriptRequest runat="server" Path="/Root/Global/scripts/jquery/plugins/slides.min.jquery.js" />
    <sn:ScriptRequest runat="server" Path="$skin/scripts/jquery/plugins/jquery.prettyPhoto.js" />
    
    <script>
        $(function ()
        {
            $('#slides').slides({
                preload: true,
                preloadImage: '/Root/Global/images/slides/loading.gif',
                play: 5000,
                pause: 2500,
                autoHeight: true,
                hoverPause: true
            });
        });
        $(document).ready(function ()
        {
            $(".slides_container a[rel^='prettyPhoto']").prettyPhoto({
                theme: 'facebook',
                deeplinking: false,
                overlay_gallery: false
            });
        });
    </script>
    <div id="slides">
        
                 <div class="slides_container">
     
    <asp:ListView ID="ViewBody"
                  DataSourceID="ViewDatasource"
                  runat="server">
      

      <LayoutTemplate>      
      
          <div runat="server" id="itemPlaceHolder" />
          
          
   
    </LayoutTemplate>
    <ItemTemplate>
                <div class='<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? "sn-hide" : "cr-content-container" %>'>
          <a href="<%# Eval("Path") %>" rel="<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? " " : "prettyphoto[pp_gal]" %>" class="<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? "sn-folder-img" : "sn-gallery-img" %>" title="<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? Eval("DisplayName") : Eval("DisplayName") %>"><img src='<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? "/Root/Global/images/icons/folder-icon-125.jpg" : Eval("Path") %>' title="<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? Eval("DisplayName") : Eval("DisplayName") %>" /></a>

          </div>
    </ItemTemplate>
    <EmptyDataTemplate>
    </EmptyDataTemplate>

  
      
      
    </asp:ListView>
    
          </div>
          <a href="#" class="prev"><img src="/Root/Global/images/slides/arrow-prev.png" width="24" height="43" alt="Arrow Prev"></a>
        <a href="#" class="next"><img src="/Root/Global/images/slides/arrow-next.png" width="24" height="43" alt="Arrow Next"></a>
          <div class="cr-thumbs">
          </div>
    </div>
    <asp:Literal runat="server" id="ViewScript" />
    <sn:SenseNetDataSource ID="ViewDatasource" runat="server" />
  