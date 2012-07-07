// using $skin/scripts/sn/SN.js
// using $skin/scripts/sn/SN.Util.js
// using $skin/scripts/jquery/plugins/tree/jquery.tree.js
// using $skin/scripts/jquery/plugins/grid/i18n/grid.locale-en.js
// using $skin/scripts/jquery/plugins/grid/jquery.jqGrid.min.js

/// <reference path="jquery/jquery.vsdoc.js"/>
var dialogIdCount = 0;
SN.PickerApplication = {

    stringRes_AllContentTypes: "All",
    ContentTypesLabelMaxChar: 100,


    _dialogConfig: { // http://docs.jquery.com/UI/Dialog#options
        modal: true,
        zIndex: 10000,
        width: 850,
        height: 600,
        minHeight: 500,
        minWidth: 650,
        autoOpen: true,
        resize: function (event, ui) {
            SN.PickerApplication.RedrawLayout();
        },
        close: function (event, ui) {
            // destroy tree
            //var tree = $.tree.reference($('#' + SN.PickerApplication._currentConfig.treeContainerId));
            //tree.destroy();
            SN.PickerApplication.destroyTree();

            // destroy grid
            $("#sn-contentpicker-grid").jqGrid('GridDestroy');

            // destroy cart grid
            $("#sn-contentpicker-selecteditemsgrid").jqGrid('GridDestroy');

            // remove contenttypes dialog
            $('#sn-contentpicker-contenttypesdialog').remove();

            // remove main dialog
            $(event.target).remove();

            $(SN.PickerApplication._dialogId).dialog("destroy");
        }
    },
    _contentTypeDialogConfig: {
        modal: true,
        width: 400,
        minHeight: 100,
        maxHeight: 400,
        resizable: false,
        autoOpen: true,
        close: function (event, ui) {
            $('#sn-contentpicker-contenttypesdialog').dialog("destroy");
        }
    },

    _dialogId: null,
    _currentConfig: null,


    // GENERAL FUNCTIONS ///////////////////////////////////////////////////////////////////
    open: function (config) {
        // create div
        dialogIdCount++;
        this._dialogId = 'dialog_' + dialogIdCount;
        $('body').append('<div id="' + this._dialogId + '" title="Content picker" style="display:none"></div>');
        var el = $('#' + this._dialogId);

        this._currentConfig = config || {};
        this._currentConfig.treeContainerId = "sn-contentpicker-tree";

        // single node selection init flag: in singleselect mode node is selected after grid is initialized - but only once
        this._currentConfig.singleSelectionInitialized = false;

        // multiselectmode: "none", "checkbox" or "button" (default is "button")
        if (this._currentConfig.MultiSelectMode == null)
            this._currentConfig.MultiSelectMode = "button";

        // in singleselect mode the nodeid(or path) can be given either in an array or the id(path) itself
        if (this._currentConfig.SelectedNodeIds != null && this._currentConfig.SelectedNodeIds.length == 1)
            this._currentConfig.SelectedNodeId = this._currentConfig.SelectedNodeIds[0];
        if (this._currentConfig.SelectedNodePaths != null && this._currentConfig.SelectedNodePaths.length == 1)
            this._currentConfig.SelectedNodePath = this._currentConfig.SelectedNodePaths[0];

        // in singleselect mode the selected path is enough, default tree path is derived
        if (this._currentConfig.SelectedNodePath != null && this._currentConfig.MultiSelectMode == 'none') {
            var path = this._currentConfig.SelectedNodePath;
            this._currentConfig.DefaultPath = path.substring(0, path.lastIndexOf('/'));
        }

        // target node path
        var targetPath = "";
        if (this._currentConfig.TargetPath != null)
            targetPath = this._currentConfig.TargetPath;

        // target fieldname
        var targetField = "";
        if (this._currentConfig.TargetField != null)
            targetField = this._currentConfig.TargetField;

        // allowed types explicitely given
        var allowedTypes = "";
        if (this._currentConfig.AllowedContentTypes != null && this._currentConfig.AllowedContentTypes.length > 0) {
            for (i = 0; i < this._currentConfig.AllowedContentTypes.length; i++) {
                allowedTypes = allowedTypes + this._currentConfig.AllowedContentTypes[i] + ',';
            }
            allowedTypes = allowedTypes.substring(0, allowedTypes.length - 1);
        }

        $(el).load('/picker.aspx?targetPath=' + targetPath + '&targetField=' + targetField + '&allowedTypes=' + allowedTypes + '&rnd=' + Math.random(), function () {
            SN.PickerApplication._currentConfig.showSystemFiles = $("#sn-contentpicker-treeshowall").is(":checked");

            if (SN.PickerApplication._currentConfig.AdminDialog == "true")
                SN.Util.CreateAdminUIDialog($(this), SN.PickerApplication._dialogConfig);
            else
                SN.Util.CreateUIDialog($(this), SN.PickerApplication._dialogConfig);

            SN.PickerApplication.initLayout();
            SN.PickerApplication.InitCartGrid();
        });
    },
    callBack: function (resultData) {
        // do sumthing with resultData
        this._currentConfig.callBack(resultData);
    },
    initLayout: function () {
        var dialog = $('#' + this._dialogId);

        // multiselect layout
        if (this._currentConfig.MultiSelectMode == "button") {
            $("#sn-contentpicker-addselecteditemsbtn").hide();
        }
        if (this._currentConfig.MultiSelectMode == "none") {
            $("#sn-contentpicker-addselected").hide();
            $("#sn-contentpicker-selectednodes").hide();
            $("#sn-contentpicker-topdiv").css("bottom", $("#sn-contentpicker-dlgbuttons").outerHeight() + "px");
            $("#sn-contentpicker-searchgriddiv").css("bottom", 0);
        }

        // default path
        var defaultPath = this._currentConfig["DefaultPath"];
        this._currentConfig.openedNodes = SN.PickerApplication.GetOpenedNodesArray(defaultPath);
        this._currentConfig.selectedNode = SN.PickerApplication.GenerateTreeNodeId(defaultPath);

        // max rownum
        this._currentConfig.RowNum = $('#sn-contentpicker-rownum').text();

        // all content types count
        this._currentConfig.AllContentTypes = $('#sn-contentpicker-contenttypes_allcount').text();

        // selected contentypes
        SN.PickerApplication.InitContentTypes();

        // available tree roots
        SN.PickerApplication.InitTreeRoots();

        // gui hint: lucenequeryben vagyunk-e
        $.getJSON('/ContentStore.mvc/IsLuceneQuery',
            { rnd: Math.random() },
            function (o) { $(o ? "#sn-contentpicker-islucene" : "#sn-contentpicker-isnotlucene").toggle() });

        SN.Util.CreateUIButton($('.sn-button', dialog));
        $('#sn-contentpicker-searchheaderdiv_totree').hide();
    },

    RedrawLayout: function () {
        SN.PickerApplication.ResetGridSize();
        SN.PickerApplication.ResetCartSize();
    },

    // TREE FUNCTIONS ///////////////////////////////////////////////////////////////////
    destroyTree: function () {
        var tree = $.tree.reference($('#' + SN.PickerApplication._currentConfig.treeContainerId));
        if (tree)
            tree.destroy();
        $("#sn-contentpicker-treecontainer").html('<div id="sn-contentpicker-tree"></div>');
    },
    initTree: function () {
        // destroy if already initialized
        this.destroyTree();

        var treeConfig = {
            callback: {
                beforedata: function (NODE, TREE_OBJ) {
                    var rp = SN.PickerApplication._currentConfig.searchRootPath;
                    if ((typeof rp === "undefined") || (rp == null)) {
                        return { path: $(NODE).find("a:first").attr("path"), rootonly: "0", rnd: Math.random() };
                    } else {
                        SN.PickerApplication._currentConfig.searchRootPath = null;
                        return { path: rp, rootonly: "1", rnd: Math.random() };
                    }
                },
                onselect: function (NODE, TREE_OBJ) {
                    var path = $(NODE).find("a:first").attr("path");
                    SN.PickerApplication.TreeNodeSelected(path);
                },
                onclose: function (NODE, TREE_OBJ) {
                    var childNodes = $(NODE).find("ul:first");
                    childNodes.remove();
                },
                onopen: function (NODE, TREE_OBJ) {
                    SN.PickerApplication.ToggleHiddenNodes(SN.PickerApplication._currentConfig.showSystemFiles);
                },
                ondata: function (DATA, TREE_OBJ) {
                    var newResult = [];
                    $.each(DATA, function (i, d) {
                        var item = {};
                        item.data = {};
                        item.data.title = d.Name;
                        item.data.icon = d.IconPath;
                        item.data.children = {};
                        item.data.attributes = {
                            href: "#",
                            //id: d.Id,
                            id: SN.PickerApplication.GenerateTreeNodeId(d.Path),
                            path: d.Path,
                            nodeid: d.Id,
                            nodetitle: d.DisplayName,
                            nodetypetitle: d.NodeTypeTitle,
                            contenttypename: d.ContentTypeName,
                            issystem: d.IsSystemContent,
                            style: d.IsSystemContent ? "color: Gray;" : "",
                            iconpath: d.IconPath,
                            title: d.DisplayName
                        };
                        item.state = "closed";
                        newResult.push(item);
                    });
                    newResult.sort(SN.PickerApplication.SortChildren);
                    return newResult; //return DATA;
                },
                onparse: function (STR, TREE_OBJ) {
                    return STR;
                }
            },
            data: {
                async: true,
                type: "json",
                opts: {
                    async: true,
                    method: "GET",
                    url: "/ContentStore.mvc/GetTreeNodeChildren"
                }
            },
            ui: {
                theme_name: "sn"
            },
            types: {
                "default": {
                    clickable: true, // can be function
                    renameable: false, // can be function
                    deletable: false, // can be function
                    creatable: false, // can be function
                    draggable: false, // can be function
                    max_children: -1, // -1 - not set, 0 - no children, 1 - one child, etc // can be function
                    max_depth: -1, // -1 - not set, 0 - no children, 1 - one level of children, etc // can be function
                    valid_children: "all", // all, none, array of values // can be function
                    icon: {
                        image: false,
                        position: false
                    }
                }
            },
            opened: SN.PickerApplication._currentConfig.openedNodes,
            selected: SN.PickerApplication._currentConfig.selectedNode
        };

        $('#' + SN.PickerApplication._currentConfig.treeContainerId).tree(treeConfig);

        // clear openednodes before next init
        SN.PickerApplication._currentConfig.openedNodes = [];
    },
    setTreeRoot: function (rootPath) {
        if (this._currentConfig) {
            this._currentConfig.searchRootPath = rootPath;

            // if tree is initialized (and not through NavigateTreeToPath) 
            if (this._currentConfig.openedNodes.length == 0) {
                this._currentConfig.openedNodes = this.GetOpenedNodesArray(rootPath);
                this._currentConfig.selectedNode = this.GenerateTreeNodeId(rootPath);
            }
        }

        this.initTree();
    },
    GenerateTreeNodeId: function (path) {
        if ((path == null) || (path.length == 0))
            return false;

        return 'PickerTreeNode_' + path.replace(/\W/g, "_");
    },
    GetOpenedNodesArray: function (path) {
        if ((path == null) || (path.length == 0))
            return [];

        var path2 = path.substring(1); // leading / should be trimmed
        var paths = path2.split('/');
        var openedNodes = [];
        var currentPath = "";
        for (i = 0; i < paths.length; i++) {
            currentPath = currentPath + "/" + paths[i];
            openedNodes[i] = SN.PickerApplication.GenerateTreeNodeId(currentPath);
        }
        return openedNodes;
    },
    SortChildren: function (a, b) {
        if (a.data.title == null)
            return -1;
        if (b.data.title == null)
            return 1;
        return a.data.title.toLowerCase() > b.data.title.toLowerCase() ? 1 : -1;
    },
    InitTreeRoots: function () {
        // check if tree root is given in config
        var treeRoots = this._currentConfig["TreeRoots"];
        if (treeRoots) {
            $('#sn-contentpicker-treerootdiv').show();
            if (treeRoots.length == 1) {
                $('#sn-contentpicker-treeroottextdiv').show();
                $('#sn-contentpicker-treerootselectdiv').hide();
                $('#sn-contentpicker-treeroottextdiv').html(treeRoots[0]);
            }
            else {
                $('#sn-contentpicker-treeroottextdiv').hide();
                $('#sn-contentpicker-treerootselectdiv').show();
                var i = 0;
                $.each(treeRoots, function () {
                    var newOption = '<option value=' + i + '>' + this + '</option>';
                    $("#sn-contentpicker-treeroot").append(newOption);
                    i++;
                });
            }

            // set tree position depends on the treeRoots panel's visibility
            $('#sn-contentpicker-treecontainer').css("top", $('#sn-contentpicker-treerootdiv').outerHeight() + $("#sn-contentpicker-treerootshowall").outerHeight());

            // init tree
            // if no default path is given, init tree with first treeroot, otherwise navigatetree to corresponding treeroot and path
            if (this._currentConfig.DefaultPath == null) {
                SN.PickerApplication.setTreeRoot(treeRoots[0]);
            } else {
                SN.PickerApplication.NavigateTreeToParentPath(this._currentConfig.DefaultPath);
            }
        } else {
            // set tree position
            $('#sn-contentpicker-treecontainer').css("top", $("#sn-contentpicker-treerootshowall").outerHeight());
            // init tree
            SN.PickerApplication.setTreeRoot('/Root');
        }
    },
    // tree root is selected from dropdown
    SelectTreeRoot: function () {
        // set searchroot
        var selectedRoot = $('#sn-contentpicker-treeroot option:selected').text();
        SN.PickerApplication.TreeNodeSelected(selectedRoot);

        // init tree
        SN.PickerApplication.setTreeRoot(selectedRoot);
    },
    TreeNodeSelected: function (path) {
        // if search panel is visible, set searchroot only
        SN.PickerApplication.SetSearchRoot(path);

        var contentTypes = SN.PickerApplication._currentConfig.SelectedContentTypes;

        // if search panel is not visible, initialize grid
        if (!$("#sn-contentpicker-searchdiv").is(':visible')) {
            $.getJSON('/ContentStore.mvc/GetChildren',
            { parentPath: path, contentTypes: contentTypes, rnd: Math.random() },
            SN.PickerApplication.InitGrid);
        }
    },
    NavigateTreeToPath: function (path) {
        var parentPath = path.substring(0, path.lastIndexOf('/'));
        SN.PickerApplication.NavigateTreeToParentPath(parentPath);
    },
    NavigateTreeToParentPath: function (parentPath) {
        this._currentConfig.openedNodes = this.GetOpenedNodesArray(parentPath);
        this._currentConfig.selectedNode = this.GenerateTreeNodeId(parentPath);

        // find out which treeroot does it correspond
        var treeRoots = this._currentConfig["TreeRoots"];
        if (treeRoots) {
            if (treeRoots.length == 1) {
                SN.PickerApplication.setTreeRoot(treeRoots[0]);
            } else {
                var correspondingTreeRoot = treeRoots[0];
                var index = 0;
                for (i = 0; i < treeRoots.length; i++) {
                    if (parentPath.indexOf(treeRoots[i]) == 0) {
                        correspondingTreeRoot = treeRoots[i];
                        index = i;
                        break;
                    }
                }
                $('#sn-contentpicker-treeroot').val(index);
                SN.PickerApplication.setTreeRoot(correspondingTreeRoot);
            }
        } else {
            SN.PickerApplication.setTreeRoot('/Root');
        }
    },
    ToggleHidden: function (show) {
        this._currentConfig.showSystemFiles = show;
        SN.PickerApplication.ToggleHiddenNodes(show);
        SN.PickerApplication.InitGrid(SN.PickerApplication._currentConfig.lastGridContents);
    },
    ToggleHiddenNodes: function (show) {
        // tree system nodes
        var systemNodes = $("a[issystem=true]");
        $.each(systemNodes, function () {
            if (show)
                $(this).parent().show();
            else
                $(this).parent().hide();
        });
    },
    ToggleGridHiddenNodes: function (show) {
        // grid system nodes
        var $grid = SN.PickerApplication.getGrid();
        systemNodes = $("td[aria-describedby=sn-contentpicker-grid_IsSystemContent][title=true]");
        $.each(systemNodes, function () {
            $(this).parent().attr("style", "color: Gray;");
        });
    },


    // SEARCH FUNCTIONS ///////////////////////////////////////////////////////////////////
    Search: function () {
        var searchStr = $('#sn-contentpicker-searchinput').val();
        var searchRoot = $('#sn-contentpicker-searchrootinput').text();
        var contentTypes = SN.PickerApplication._currentConfig.SelectedContentTypes;

        $.getJSON('/ContentStore.mvc/Search',
            {
                searchStr: searchStr,
                searchRoot: searchRoot,
                contentTypes: contentTypes,
                rnd: Math.random()
            },
            SN.PickerApplication.InitGrid);

        // search started
        $('#sn-contentpicker-searchinput').addClass("SnSearching");
    },
    SetSearchRoot: function (s) {
        $('#sn-contentpicker-searchrootinput').text(s);
    },
    // search pane is toggled
    ToggleSearchDiv: function () {
        $('#sn-contentpicker-searchdiv').toggle();
        $('#sn-contentpicker-searchheaderdiv_tosearch').toggle();
        $('#sn-contentpicker-searchheaderdiv_totree').toggle();
        SN.PickerApplication.ResetGridSize();
    },
    ParseDate: function (v) {
        var msdateRe = /\/Date\((\d+)\)\//; // MS JSON date format
        return new Date(parseInt(v.replace(msdateRe, '$1')));
    },


    // CONTENT TYPES DIALOG FUNCTIONS ///////////////////////////////////////////////////////////////////
    GetAllAllowedContentTypes: function () {
        return $('#sn-contentpicker-contenttypes_alltypestext').text();
    },
    ShowContentTypesDialog: function () {
        if (SN.PickerApplication._currentConfig.AdminDialog == "true")
            SN.Util.CreateAdminUIDialog($('#sn-contentpicker-contenttypesdialog'), SN.PickerApplication._contentTypeDialogConfig);
        else
            SN.Util.CreateUIDialog($('#sn-contentpicker-contenttypesdialog'), SN.PickerApplication._contentTypeDialogConfig);
    },
    InitContentTypes: function () {
        // check if default content types is given in config
        var defaultContentTypes = this._currentConfig["DefaultContentTypes"];
        if (defaultContentTypes) {
            // select default contenttypes in dialog
            $.each(defaultContentTypes, function () {
                var control = $("#sn-contentpicker-contenttypes_" + this, "#sn-contentpicker-contenttypesdialog");
                control.attr({ checked: 'true' });
            });
        } else {
            // select all contenttypes in dialog
            SN.PickerApplication.SelectAllContentTypes(true);
        }
        // set gui hint
        SN.PickerApplication.SetContentTypesGuiHint();
    },
    SetContentTypesGuiHint: function () {
        // get selected content types from content type dialog and create gui hint
        var text = "";
        var valuetext = "";
        var alltext = "";
        var allvaluetext = "";
        var checkboxes = $("#sn-contentpicker-contenttypesdialog input[type=checkbox]");
        var checked = 0;
        $.each(checkboxes, function () {
            var control = $(this);
            if (control.is(':checked')) {
                // text contains the label (title of contenttype)
                text = text + control.parent().text() + ', ';
                // valuetext contains the value of the control (name of contenttype)
                valuetext = valuetext + control.val() + ',';
                checked++;
            }
            alltext = alltext + control.parent().text() + ', ';
            allvaluetext = allvaluetext + control.val() + ',';
        });
        alltext = alltext.substring(0, alltext.length - 2);
        allvaluetext = allvaluetext.substring(0, allvaluetext.length - 1);

        // is any selected?
        if (checked > 0) {
            text = text.substring(0, text.length - 2);
            valuetext = valuetext.substring(0, valuetext.length - 1);

            // all selected
            if (checkboxes.length == checked)
                valuetext = allvaluetext;
        } else {
            // none selected
            text = alltext;
            valuetext = allvaluetext;
        }

        // if list is to long, truncate
        if (text.length > SN.PickerApplication.ContentTypesLabelMaxChar)
            text = text.substring(0, 100) + "...";

        // all contenttypes in the list?
        if (SN.PickerApplication._currentConfig.AllContentTypes == checkboxes.length) {
            // all or none selected?
            if (checkboxes.length == checked || checked == 0) {
                text = SN.PickerApplication.stringRes_AllContentTypes;
                valuetext = "";
            }
        }


        // set gui hint with tooltip
        $('#sn-contentpicker-selectedcontenttypesdivtext').html('<span title="' + valuetext + '">' + text + '</span>');
        SN.PickerApplication._currentConfig.SelectedContentTypes = valuetext;
    },
    CloseContentTypeDialog: function () {
        SN.PickerApplication.SetContentTypesGuiHint();
        $('#sn-contentpicker-contenttypesdialog').dialog("close");

        SN.PickerApplication.ResetGridSize();
    },
    SelectAllContentTypes: function (on) {
        $.each($("#sn-contentpicker-contenttypesdialog input[type=checkbox]"), function () {
            var control = $(this);
            control.attr({ checked: on });
        });
    },


    // SELECTED ITEMS GRID FUNCTIONS ///////////////////////////////////////////////////////////////////
    GetResultDataFromRow: function (rowdata) {
        result = {};
        result.Id = rowdata.Id;
        result.NodeTypeTitle = rowdata.NodeTypeTitle;
        result.IconPath = rowdata.IconPath;
        result.Path = rowdata.Path;
        result.DisplayName = rowdata.DisplayName;
        return result;
    },
    GetCartItems: function () {
        results = [];
        var cartRecordCount = $("#sn-contentpicker-selecteditemsgrid").jqGrid('getGridParam', 'records');
        if (cartRecordCount > 0) {
            var rowdata = jQuery("#sn-contentpicker-selecteditemsgrid").jqGrid('getRowData');
            for (i = 0; i < cartRecordCount; i++) {
                results[i] = this.GetResultDataFromRow(rowdata[i]);
            }
        }
        return results;
    },
    GetSelectedItems: function () {
        var results = this.GetCartItems();
        if (results.length == 0) {
            // check selection of grid
            var selRow = jQuery("#sn-contentpicker-grid").jqGrid('getGridParam', 'selrow');
            if (selRow) {
                var rowdata = jQuery("#sn-contentpicker-grid").jqGrid('getRowData', selRow);
                results[0] = this.GetResultDataFromRow(rowdata);
            } else {
                // check selection of tree
                var tree = $.tree.reference($('#' + SN.PickerApplication._currentConfig.treeContainerId));
                var selItem = $("a", tree.selected);
                // check if this element is an allowed contentype
                if (this.GetAllAllowedContentTypes().indexOf(selItem.attr("contenttypename")) == -1)
                    return null;

                result = {};
                result.Id = selItem.attr("nodeid");
                result.NodeTypeTitle = selItem.attr("nodetypetitle");
                result.Path = selItem.attr("path");
                result.DisplayName = selItem.attr("nodetitle");
                result.IconPath = selItem.attr("iconpath");
                results[0] = result;
            }
        }
        return results;
    },
    removeButtonHandler: function (id) {
        var success = $("#sn-contentpicker-selecteditemsgrid").jqGrid('delRowData', id);
        if (!success) {
            alert('Could not find item with rowIndex: ' + id);
        }
    },
    removeButtonFormatter: function (index, cellvalue, dataItem) {
        return "<a class='sn-icon-small sn-icon-button snIconSmall_Delete' id='removes" + dataItem.Id +
            "' style='height:20px;width:20px' type='button' onclick='SN.PickerApplication.removeButtonHandler(\"" + cellvalue.rowId +
            "\");'></a>";
    },
    // init tree to the parent of the selected node
    CartTreeButtonHandler: function (rowIndex) {
        if (typeof rowIndex === "undefined") {
            throw "addItemToCart: rowIndex is null.";
        }
        var selected = jQuery("#sn-contentpicker-selecteditemsgrid").jqGrid('getRowData', rowIndex);
        if (!selected) {
            alert('Could not find item with rowIndex: ' + rowIndex);
        };

        this.NavigateTreeToPath(selected.Path);
        return false;
    },
    CartTreeButtonFormatter: function (index, cellvalue, dataItem) {
        return "<a id='carttree" + dataItem.Id +
            "' class='sn-icon-small sn-icon-button snIconSmall_Move' href='javascript:;' onclick='SN.PickerApplication.CartTreeButtonHandler(\"" + cellvalue.rowId +
            "\");'></a>";
    },
    InitCartGrid: function () {
        if (SN.PickerApplication._currentConfig.MultiSelectMode == "none")
            return;

        $("#sn-contentpicker-selecteditemsgrid").jqGrid({
            datatype: 'local',
            autowidth: true,
            height: 75,
            shrinkToFit: true,
            colNames: ['Id', '', '', 'DisplayName', 'Path', 'Type', '', ''],
            colModel: [
              { name: 'Id', index: 'Id', hidden: true },
              { name: 'IconPath', index: 'IconPath', hidden: true },
              { name: 'Icon', index: 'Icon', formatter: SN.PickerApplication.IconFormatter, width: 16, fixed: true, resizable: false, sortable: false, align: "center" },
              { name: 'DisplayName', index: 'DisplayName', width: 150, fixed: true },
              { name: 'Path', index: 'Path', width: 200 },
              { name: 'NodeTypeTitle', index: 'NodeTypeTitle', width: 100, fixed: true },
              { name: '', index: 'Id', width: 16, formatter: SN.PickerApplication.removeButtonFormatter, fixed: true, resizable: false, sortable: false, align: "center" },
              { name: '', index: 'Id', width: 16, formatter: SN.PickerApplication.CartTreeButtonFormatter, fixed: true, resizable: false, sortable: false, align: "center" }
              ],
            rowNum: 1000000
        });
    },
    ClearCart: function () {
        $("#sn-contentpicker-selecteditemsgrid").jqGrid('clearGridData');
    },
    ResetCartSize: function () {
        if (SN.PickerApplication._currentConfig.MultiSelectMode == "none")
            return;
        var $GridDiv = $("#sn-contentpicker-selectednodes");
        var $Grid = $("#sn-contentpicker-selecteditemsgrid");
        $Grid.jqGrid('setGridHeight', $GridDiv.height() - 25); //todo: replace fix size with dynamic data
        $Grid.jqGrid('setGridWidth', $GridDiv.width() - 2); //todo: replace fix size with dynamic data
    },

    // DIALOG BUTTON FUNCTIONS /////////////////////////////////////////////////////////////////////////
    closeDialog: function () {
        var results = this.GetSelectedItems();

        var el = $('#' + this._dialogId);
        el.dialog('close');
        this.callBack(results);
    },
    cancelDialog: function () {
        var el = $('#' + this._dialogId);
        el.dialog('close');
    },


    // GRID FUNCTIONS //////////////////////////////////////////////////////////////////////////////////
    addSelectedItemsToCart: function () {
        var cartItems = this.GetCartItems();

        var selectedIds = $("#sn-contentpicker-grid").jqGrid('getGridParam', 'selarrrow');
        $.each(selectedIds, function (item) {
            var id = this;
            var rowData = $("#sn-contentpicker-grid").jqGrid('getRowData', id);
            newItems = SN.PickerApplication.addItemToCartData(rowData, cartItems);

            // successfully added this item, update array
            if (newItems != null)
                cartItems = newItems;
        });
        this.addItemsToCart(newItems);
    },
    addItemToCart: function (source) {
        var cartItems = this.GetCartItems();

        var newItems = this.addItemToCartData(source, cartItems);

        this.addItemsToCart(newItems);
    },
    addItemToCartData: function (source, cartItems) {
        if (typeof source === "undefined") {
            throw "addItemToCartData: source is null.";
        }
        var cartGridId = "sn-contentpicker-selecteditemsgrid";
        var newRow = { Id: source.Id, IconPath: source.IconPath, DisplayName: source.DisplayName, Path: source.Path, NodeTypeTitle: source.NodeTypeTitle };

        // check if row already exists in cart
        var contained = 0;
        for (i = 0; i < cartItems.length; i++) {
            if (cartItems[i].Id == newRow.Id) {
                contained = true;
                break;
            }
        }
        if (contained)
            return null;

        cartItems[cartItems.length] = newRow;
        return cartItems;
    },
    addItemsToCart: function (data) {
        // empty array (maybe all added elements are already contained)
        if (data == null)
            return;

        data.sort(this.SortCartItems);
        this.ClearCart();
        $.each(data, function () {
            jQuery("#sn-contentpicker-selecteditemsgrid").jqGrid('addRowData', "cartitem_" + this.Id, this);
        });
    },
    SortCartItems: function (a, b) {
        if (a.DisplayName == null)
            return -1;
        if (b.DisplayName == null)
            return 1;
        return a.DisplayName.toLowerCase() > b.DisplayName.toLowerCase() ? 1 : -1;
    },
    addButtonHandler: function (rowIndex) {
        if (typeof rowIndex === "undefined") {
            throw "addItemToCart: rowIndex is null.";
        }
        var selected = jQuery("#sn-contentpicker-grid").jqGrid('getRowData', rowIndex);
        if (!selected) {
            alert('Could not find item with rowIndex: ' + rowIndex);
        };

        this.addItemToCart(selected, rowIndex);
    },
    addButtonFormatter: function (index, cellvalue, dataItem) {
        if (SN.PickerApplication.IsContainedNode(dataItem))
            return "";

        return "<a class='sn-icon-small sn-icon-button snIconSmall_Add' id='adds" + dataItem.Id +
            "' style='height:20px;width:20px' type='button' onclick='SN.PickerApplication.addButtonHandler(" + cellvalue.rowId +
            ");'></a>";
    },
    // checks if node is already contained in target's collection
    IsContainedNode: function (dataItem) {
        // if element contained in id list
        var nodeIds = SN.PickerApplication._currentConfig["SelectedNodeIds"];
        if (nodeIds != null) {
            if ($.inArray(dataItem.Id, SN.PickerApplication._currentConfig["SelectedNodeIds"]) > -1)
                return true;
        }

        // if element contained in path list
        var nodePaths = SN.PickerApplication._currentConfig["SelectedNodePaths"];
        if (nodePaths != null) {
            if ($.inArray(dataItem.Path, nodePaths) > -1)
                return true;
        }
        return false;
    },
    TreeButtonHandler: function (rowIndex) {
        // init tree to the parent of the selected node
        if (typeof rowIndex === "undefined") {
            throw "addItemToCart: rowIndex is null.";
        }
        var selected = jQuery("#sn-contentpicker-grid").jqGrid('getRowData', rowIndex);
        if (!selected) {
            alert('Could not find item with rowIndex: ' + rowIndex);
        };

        this.NavigateTreeToPath(selected.Path);
        return false;
    },
    TreeButtonFormatter: function (index, cellvalue, dataItem) {
        return "<a id='gridtree" + dataItem.Id +
            "'  class='sn-icon-small sn-icon-button snIconSmall_Move' href='javascript:;' onclick='SN.PickerApplication.TreeButtonHandler(" + cellvalue.rowId +
            ");'></a>";
    },
    IconFormatter: function (index, cellvalue, dataItem) {
        return "<img src='" + dataItem.IconPath + "'/>";
    },
    getGrid: function () {
        var gridId = "sn-contentpicker-grid";
        var $grid = $("#" + gridId);
        return $grid;
    },
    InitGrid: function (o) {
        SN.PickerApplication._currentConfig.lastGridContents = o;
        var $grid = SN.PickerApplication.getGrid();

        // clear grid
        $grid.jqGrid('GridUnload');
        var $grid = SN.PickerApplication.getGrid();


        var multiSelect;
        if (SN.PickerApplication._currentConfig.MultiSelectMode == "none") {
            multiSelect = false;
        }
        if (SN.PickerApplication._currentConfig.MultiSelectMode == "checkbox") {
            multiSelect = true;
        }
        if (SN.PickerApplication._currentConfig.MultiSelectMode == "button") {
            multiSelect = false;
        }

        var colNames;
        var colModel;

        if (SN.PickerApplication._currentConfig.ColNames) {
            colNames = SN.PickerApplication._currentConfig.ColNames;
            colModel = SN.PickerApplication._currentConfig.ColModel;
        } else {
            if (SN.PickerApplication._currentConfig.MultiSelectMode == "none") {
                multiSelect = false;
                colNames = ['Id', 'IsSystemContent', '', '', 'DisplayName', 'Path', 'Type', ''];
                colModel = [
                  { name: 'Id', index: 'Id', hidden: true },
                  { name: 'IsSystemContent', index: 'IsSystemContent', hidden: true },
                  { name: 'IconPath', index: 'IconPath', hidden: true },
                  { name: 'Icon', index: 'Icon', formatter: SN.PickerApplication.IconFormatter, width: 16, fixed: true, resizable: false, sortable: false, align: "center" },
                  { name: 'DisplayName', index: 'DisplayName', width: 150 },
                  { name: 'Path', index: 'Path', width: 200 },
                  { name: 'NodeTypeTitle', index: 'NodeTypeTitle', width: 100 },
                  { name: '', index: 'Id', width: 16, formatter: SN.PickerApplication.TreeButtonFormatter, fixed: true, resizable: false, sortable: false, align: "center" }
                  ];
            }
            if (SN.PickerApplication._currentConfig.MultiSelectMode == "checkbox") {
                multiSelect = true;
                colNames = ['Id', 'IsSystemContent', '', '', 'DisplayName', 'Path', 'Type', ''];
                colModel = [
                  { name: 'Id', index: 'Id', hidden: true },
                  { name: 'IsSystemContent', index: 'IsSystemContent', hidden: true },
                  { name: 'IconPath', index: 'IconPath', hidden: true },
                  { name: 'Icon', index: 'Icon', formatter: SN.PickerApplication.IconFormatter, width: 16, fixed: true, resizable: false, sortable: false, align: "center" },
                  { name: 'DisplayName', index: 'DisplayName', width: 150 },
                  { name: 'Path', index: 'Path', width: 200 },
                  { name: 'NodeTypeTitle', index: 'NodeTypeTitle', width: 100 },
                  { name: '', index: 'Id', width: 16, formatter: SN.PickerApplication.TreeButtonFormatter, fixed: true, resizable: false, sortable: false, align: "center" }
                  ];
            }
            if (SN.PickerApplication._currentConfig.MultiSelectMode == "button") {
                multiSelect = false;
                colNames = ['Id', 'IsSystemContent', '', '', 'DisplayName', 'Path', 'Type', '', ''];
                colModel = [
                  { name: 'Id', index: 'Id', hidden: true },
                  { name: 'IsSystemContent', index: 'IsSystemContent', hidden: true },
                  { name: 'IconPath', index: 'IconPath', hidden: true },
                  { name: 'Icon', index: 'Icon', formatter: SN.PickerApplication.IconFormatter, width: 16, fixed: true, resizable: false, sortable: false, align: "center" },
                  { name: 'DisplayName', index: 'DisplayName', width: 150 },
                  { name: 'Path', index: 'Path', width: 200 },
                  { name: 'NodeTypeTitle', index: 'NodeTypeTitle', width: 100 },
                  { name: '', index: 'Id', width: 16, formatter: SN.PickerApplication.addButtonFormatter, fixed: true, resizable: false, sortable: false, align: "center" },
                  { name: '', index: 'Id', width: 16, formatter: SN.PickerApplication.TreeButtonFormatter, fixed: true, resizable: false, sortable: false, align: "center" }
                  ];
            }
        }
        SN.PickerApplication._currentConfig.gridSelectedNodeSearch = false;

        // singleselect mode: search selected node (but only when grid is first time initialized)
        var selectedNodeSearch = false;
        if (SN.PickerApplication._currentConfig.MultiSelectMode == "none") {
            if (!SN.PickerApplication._currentConfig.singleSelectionInitialized) {
                if (SN.PickerApplication._currentConfig.SelectedNodeId != null || SN.PickerApplication._currentConfig.SelectedNodePath != null) {
                    selectedNodeSearch = true;
                    SN.PickerApplication._currentConfig.singleSelectionInitialized = true;
                }
            }
        }


        // prepare data for hiding system files
        var o2 = [];
        var index = 0;
        for (var i = 0; i < o.length; i++) {
            if (SN.PickerApplication._currentConfig.showSystemFiles || !o[i].IsSystemContent)
                o2[index++] = o[i];
        }

        SN.PickerApplication._currentConfig.gridSelectedNodeSearch = selectedNodeSearch;

        $grid.jqGrid({
            datatype: 'local',
            data: o2,
            autowidth: true,
            shrinkToFit: true,
            colNames: colNames,
            colModel: colModel,
            multiselect: multiSelect,
            sortname: 'DisplayName',
            sortorder: "asc",
            loadComplete: function (data) {
                // singleselect mode: select node
                if (SN.PickerApplication._currentConfig.gridSelectedNodeSearch) {
                    var selectedIndex = -1;
                    if (data && data.rows) {
                        for (var i = 0; i < data.rows.length; i++) {
                            if (SN.PickerApplication._currentConfig.SelectedNodeId != null && data.rows[i].Id == SN.PickerApplication._currentConfig.SelectedNodeId)
                                selectedIndex = i + 1;
                            if (SN.PickerApplication._currentConfig.SelectedNodePath != null && data.rows[i].Path == SN.PickerApplication._currentConfig.SelectedNodePath)
                                selectedIndex = i + 1;
                        }
                    }
                    $grid.jqGrid('setSelection', selectedIndex);
                }
            },
            rowNum: SN.PickerApplication._currentConfig.RowNum == 0 ? 1000000 : SN.PickerApplication._currentConfig.RowNum,
            pager: SN.PickerApplication._currentConfig.RowNum > o2.length ? null : '#pgtoolbar1',
            gridview: true
        });



        // toggle system files
        SN.PickerApplication.ToggleGridHiddenNodes(SN.PickerApplication._currentConfig.showSystemFiles);

        // search finished
        $('#sn-contentpicker-searchinput').removeClass("SnSearching");

        SN.PickerApplication.ResetGridSize();
    },
    ResetGridSize: function () {
        var $SearchHeader = $('#sn-contentpicker-searchheaderdiv');
        var $SearchGridContainer = $("#sn-contentpicker-searchgriddiv");
        var $Grid = $("#sn-contentpicker-grid");

        var mtop = parseFloat($SearchHeader.css('marginTop').replace(/auto/, 0));
        var mbottom = parseFloat($SearchHeader.css('marginBottom').replace(/auto/, 0));
        var newTop = $SearchHeader.outerHeight() + mtop + mbottom;

        $SearchGridContainer.css("top", newTop + "px");
        var bottomCorrection = SN.PickerApplication._currentConfig.RowNum == 0 ? 23 : 49; //todo: replace fix size with dynamic data
        $Grid.jqGrid('setGridHeight', $SearchGridContainer.height() - bottomCorrection);
        $Grid.jqGrid('setGridWidth', $SearchGridContainer.width()); //todo: replace fix size with dynamic data
    },

    // CUSTOM FUNCTIONS //////////////////////////////////////////////////////////////////////////////////
    openPortletPicker: function (config) {
        colNames = ['Id', 'Description', 'Portlet'];
        colModel = [
            { name: 'Id', index: 'Id', hidden: true },
            { name: 'DescriptionProp', index: 'DescriptionProp', hidden: true },
            { name: 'DisplayName', index: 'DisplayName', formatter: function (index, cellvalue, dataItem)
            { return "<div class='sn-contentpicker-portletrow ui-helper-clearfix'><img class='sn-icon sn-icon32 sn-floatleft' onerror='SN.PickerApplication.PortletPickerImgError(this);' src=/Root/Global/images/icons/32/portlet-" + dataItem.Name + '.png width=32px height=32px /><div style="padding-left:34;"><b>' + dataItem.DisplayName + '</b><br />' + dataItem.DescriptionProp + '</div></div>' }
            }
            ];
        this.open({ AdminDialog: 'true', AllowedContentTypes: ['Portlet'], MultiSelectMode: 'none', TreeRoots: ['/Root/Portlets'], ColNames: colNames, ColModel: colModel, callBack: config.callBack });
    },
    PortletPickerImgError: function (img) {
        img.onerror = null;
        img.src = "/Root/Global/images/icons/32/portlet-Default.png";
    }
}
