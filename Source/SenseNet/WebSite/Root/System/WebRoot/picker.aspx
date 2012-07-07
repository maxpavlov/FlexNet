<%@ Page Language="C#" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.ContentRepository.Schema" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>

<%
    // allowed content types
    var targetPath = Request["targetPath"];
    var targetField = Request["targetField"];
    var allowedContentTypes = Request["allowedTypes"];
    var contentTypes = new List<ContentType>();
    if (!string.IsNullOrEmpty(allowedContentTypes))
    {
        var allowedContentTypesArray = allowedContentTypes.Split(',',';');
        contentTypes = allowedContentTypesArray.Select(c => SenseNet.ContentRepository.Schema.ContentType.GetByName(c)).Where(ct => ct != null).ToList();
    }
    else
    {
        if (!string.IsNullOrEmpty(targetPath))
        {
            var targetNode = Node.LoadNode(targetPath) as SenseNet.ContentRepository.GenericContent;
            if (targetNode != null)
            {
                if (string.IsNullOrEmpty(targetField))
                {
                    // allowed contenttypes come from the node (folder etc.)
                    contentTypes.AddRange(targetNode.GetAllowedChildTypes());
                }
                else
                {
                    // allowed contenttypes come from field setings (reference field etc.)
                    var targetContent = SenseNet.ContentRepository.Content.Create(targetNode);
                    bool found;
                    var fieldInfoObj = targetContent.Fields[targetField].FieldSetting.GetProperty("AllowedTypes", out found);
                    var allowedTypes = fieldInfoObj as List<ContentType>;
                    if (allowedTypes != null)
                        contentTypes.AddRange(allowedTypes);
                }
            }
        }
    }
    var allTypes = SenseNet.ContentRepository.Schema.ContentType.GetContentTypes();
    if (contentTypes.Count == 0)
        contentTypes.AddRange(allTypes);
    
    contentTypes = contentTypes.OrderBy(n => string.IsNullOrEmpty(n.DisplayName) ? n.Name : n.DisplayName).ToList();
    var contentTypesText = string.Join(",", contentTypes.Select(t => t.Name).ToArray());
	
	// rownum
	var rowNum = ConfigurationManager.AppSettings["SNPickerRowNum"] ?? "0";
%>

<div id="sn-contentpicker-maindiv" class="ui-widget">
    <div id="sn-contentpicker-rownum" style="display: none;"><%= rowNum %></div>
    <div id="sn-contentpicker-topdiv">
        <div id="sn-contentpicker-treediv" class="ui-widget-content ui-corner-all">
            <div id="sn-contentpicker-treerootdiv" class="ui-widget-header ui-corner-all ui-state-active">
                Tree root:
                <span id="sn-contentpicker-treeroottextdiv"></span>
                <span id="sn-contentpicker-treerootselectdiv">
                    <select id="sn-contentpicker-treeroot" onchange="SN.PickerApplication.SelectTreeRoot()" />
                </span>
            </div>
            <div id="sn-contentpicker-treerootshowall">
                <label for="sn-contentpicker-treeshowall"><input id="sn-contentpicker-treeshowall" type="checkbox" onclick="SN.PickerApplication.ToggleHidden(this.checked)" checked="checked" />Show system files</label>
            </div>
            <div id="sn-contentpicker-treecontainer">
                <div id="sn-contentpicker-tree"></div>
            </div>
        </div>
        <div id="sn-contentpicker-rightdiv">
            <div id="sn-contentpicker-searchheaderdiv">
                <div class="sn-contentpicker-searchmodeswitcher">
                    <a href="javascript:;" onclick="SN.PickerApplication.ToggleSearchDiv()" id="sn-contentpicker-searchheaderdiv_tosearch" class="sn-button">Switch to search mode >></a>
                    <a href="javascript:;" onclick="SN.PickerApplication.ToggleSearchDiv()" id="sn-contentpicker-searchheaderdiv_totree" class="sn-button"><< Switch to tree mode</a>
                </div>
                <div id="sn-contentpicker-searchdiv" class="ui-widget" style="display: none">
                    <div>
                        <input id="sn-contentpicker-searchinput" class=" ui-widget-content ui-corner-all" type="text" style="width: 270px" onkeypress="if(event.keyCode==13){SN.PickerApplication.Search();return false;}" />
                        <input id="sn-contentpicker-searchbutton" class="sn-button" type="button" onclick="SN.PickerApplication.Search()" value="Search" />
                    </div>
                    <p>
                        <small id="sn-contentpicker-islucene" style="display: none;">Freetext and lucene query search (ie. <i>John</i> or <i>_Text:John</i>)</small>
                        <small id="sn-contentpicker-isnotlucene" style="display: none;">Simple freetext search</small>
                    </p>
                    <dl>
                        <dt>
                            Search root
                        </dt>
                        <dd>
                            <span id="sn-contentpicker-searchrootinput"></span>
                        </dd>
                        <dt>
                            Content types                            
                        </dt>
                        <dd id="sn-contentpicker-selectedcontenttypesdiv">
                            <span id="sn-contentpicker-selectedcontenttypesdivtext">
                            </span> <a href="javascript:;" class="sn-button" onclick="SN.PickerApplication.ShowContentTypesDialog()">Change...</a>
                        </dd>
                    </dl>
                </div>
            </div>
            <div id="sn-contentpicker-searchgriddiv" class="ui-widget-content ui-corner-all">
                <table id="sn-contentpicker-grid"></table>
                <div id="pgtoolbar1"></div>
            </div>
         </div>
    </div>
    <div id="sn-contentpicker-selectednodes">
        <table id="sn-contentpicker-selecteditemsgrid" />
    </div>
    <div id="sn-contentpicker-addselected">
        <input type="button" id="sn-contentpicker-addselecteditemsbtn" class="sn-button" value="Add selected..." onclick="SN.PickerApplication.addSelectedItemsToCart()" />
        <input type="button" id="sn-contentpicker-removeallselected" class="sn-button" value="Clear" onclick="SN.PickerApplication.ClearCart()"/>
    </div>
    <div id="sn-contentpicker-dlgbuttons">
        <input type="button" id="closeDialog" class="sn-button" value="Ok" onclick="SN.PickerApplication.closeDialog();return false;" />
        <input type="button" id="cancelDialog" class="sn-button" value="Cancel" onclick="SN.PickerApplication.cancelDialog();return false;" />
    </div>
</div>


<div id="sn-contentpicker-contenttypesdialog" title="Choose Content types" style="display: none;">
    <div id="sn-contentpicker-contenttypes_allcount" style="display: none;"><%= allTypes.Count().ToString() %></div>
    <div id="sn-contentpicker-contenttypes_alltypestext" style="display: none;"><%= contentTypesText %></div>
    <div id="sn-contentpicker-contenttypes_controls">
        <a href="javascript:;" class="sn-button" onclick="SN.PickerApplication.SelectAllContentTypes(true)">Select all</a>
        <a href="javascript:;" class="sn-button" onclick="SN.PickerApplication.SelectAllContentTypes(false)">Deselect all</a><br />
    </div>
    <div id="sn-contentpicker-contenttypeslist" class="ui-widget-content ui-corner-all">
        <% 
           foreach (var contentType in contentTypes)
           {
               var name = contentType.Name;
               var title = contentType.DisplayName;
               if (string.IsNullOrEmpty(title))
                   title = name;
               %><label for='sn-contentpicker-contenttypes_<%= name %>'><input type="checkbox" id='sn-contentpicker-contenttypes_<%= name %>' value='<%= name %>' /><%= title %></label><%
           }
        %>
    </div>
    <div id="sn-contentpicker-contenttypes_buttons">
        <input type="button" class="sn-button" id="sn-contentpicker-contenttypesdialog_ok" value="Ok" onclick="SN.PickerApplication.CloseContentTypeDialog()" />
    </div>
</div>