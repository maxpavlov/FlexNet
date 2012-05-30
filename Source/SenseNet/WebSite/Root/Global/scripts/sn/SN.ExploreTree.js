/// <reference path="../jquery/jquery.vsdoc.js"/>
SN.ExploreTree = {

    _currentConfig: null,

    // PUBLIC FUNCTIONS ///////////////////////////////////////////////////////////////////
    open: function(config) {
        this._currentConfig = config || {};
        this._currentConfig.treeContainerId = "sn-exploretree-tree";
        this._currentConfig.showSystemFiles = $("#sn-exploretree-treeshowall").is(":checked");

        SN.ExploreTree.initLayout();
    },
    NavigateTreeToPath: function(path) {
        var parentPath = path.substring(0, path.lastIndexOf('/'));
        SN.ExploreTree.NavigateTreeToParentPath(parentPath);
    },
    NavigateTreeToParentPath: function(parentPath) {
        this._currentConfig.InNavigation = true;
        this._currentConfig.openedNodes = this.GetOpenedNodesArray(parentPath);
        this._currentConfig.selectedNode = this.GenerateTreeNodeId(parentPath);

        // find out which treeroot does it correspond
        var treeRoots = this._currentConfig["TreeRoots"];
        if (treeRoots) {
            if (treeRoots.length == 1) {
                SN.ExploreTree.setTreeRoot(treeRoots[0]);
            } else {
                var correspondingTreeRoot = "/Root";
                var index = 0;
                for (i = 0; i < treeRoots.length; i++) {
                    if (parentPath.indexOf(treeRoots[i]) == 0) {
                        correspondingTreeRoot = treeRoots[i];
                        index = i;
                        break;
                    }
                }
                $('#sn-exploretree-treeroot').val(index);
                SN.ExploreTree.setTreeRoot(correspondingTreeRoot);
            }
        } else {
            SN.ExploreTree.setTreeRoot('/Root');
        }
    },
    NavigateOpenTreeToParentPath: function(parentPath) {
        this._currentConfig.openedNodes = this.GetOpenedNodesArray(parentPath);
        this._currentConfig.selectedNode = this.GenerateTreeNodeId(parentPath);

        this._currentConfig.index = 0;
        this._currentConfig.InPathOpening = true;
        SN.ExploreTree.OpenNextTreeNode();
    },
    RefreshTargetFolder: function() {
        // refresh target path when opening of current folder finished
        SN.ExploreTree._currentConfig.openingNodesFinished = null;
        SN.ExploreTree._currentConfig.openedNodes = SN.ExploreTree.GetOpenedNodesArray(SN.ExploreTree._currentConfig.previousContent);
        SN.ExploreTree._currentConfig.InPathOpening = true;
        SN.ExploreTree._currentConfig.index = 0;
        SN.ExploreTree._currentConfig.parentRefreshed = false;    // refresh parent only once
        SN.ExploreTree.OpenNextTreeNode();
    },
    OpenNextTreeNodeFinished: function() {
        SN.ExploreTree._currentConfig.InPathOpening = false;

        // invoke callback function when finished opening
        if (SN.ExploreTree._currentConfig.openingNodesFinished != null)
            SN.ExploreTree._currentConfig.openingNodesFinished();
    },
    OpenNextTreeNode: function() {
        var tree = SN.ExploreTree._currentConfig.tree;
        var currentIndex = SN.ExploreTree._currentConfig.index;
        if (currentIndex >= SN.ExploreTree._currentConfig.openedNodes.length) {
            SN.ExploreTree.OpenNextTreeNodeFinished();
            return;
        }

        // check if previous action was an add/move/delete: refresh current node
        if (currentIndex == SN.ExploreTree._currentConfig.openedNodes.length - 1 && SN.ExploreTree._currentConfig.parentRefreshed == false) {
            if (
            (SN.ExploreTree._currentConfig.currentAction != 'Add' && SN.ExploreTree._currentConfig.previousAction == 'Add') ||
            (SN.ExploreTree._currentConfig.currentAction != 'Edit' && SN.ExploreTree._currentConfig.previousAction == 'Edit') ||
            (SN.ExploreTree._currentConfig.currentAction != 'MoveToTarget' && SN.ExploreTree._currentConfig.previousAction == 'MoveToTarget') ||
            (SN.ExploreTree._currentConfig.currentAction != 'CopyToTarget' && SN.ExploreTree._currentConfig.previousAction == 'CopyToTarget') ||
            (SN.ExploreTree._currentConfig.currentAction != 'Delete' && SN.ExploreTree._currentConfig.previousAction == 'Delete') ||
            (SN.ExploreTree._currentConfig.currentAction != 'DeleteBatchTarget' && SN.ExploreTree._currentConfig.previousAction == 'DeleteBatchTarget')
            ) {
                var indexToRefresh = currentIndex;
                // when Edit action is finished, parent folder should be refreshed, not the current folder
                if (SN.ExploreTree._currentConfig.previousAction == 'Edit')
                    indexToRefresh = currentIndex - 1;

                SN.ExploreTree._currentConfig.parentRefreshed = true;
                SN.ExploreTree._currentConfig.InSelection = true;
                tree.close_branch("#" + SN.ExploreTree._currentConfig.openedNodes[indexToRefresh], true);
                SN.ExploreTree._currentConfig.InSelection = false;
                return;
            }
        }

        SN.ExploreTree._currentConfig.index = currentIndex + 1;
        tree.open_branch("#" + SN.ExploreTree._currentConfig.openedNodes[currentIndex], true, null);

        // check if we can select the last node already
        if (currentIndex == SN.ExploreTree._currentConfig.openedNodes.length - 1) {
            SN.ExploreTree._currentConfig.InSelection = true;
            var selectedNode = $("#" + SN.ExploreTree._currentConfig.selectedNode);
            tree.select_branch(selectedNode, false);
            SN.ExploreTree._currentConfig.InSelection = false;
            if (selectedNode.length > 0) {
                var offset = selectedNode.offset();
                if (offset) {
                    var top = selectedNode.offset().top - 100;
                    $(document).scrollTop(top);
                }
                SN.ExploreTree.OpenNextTreeNodeFinished();
                return;
            }
            else {
                if (SN.ExploreTree._currentConfig.nodeNotSelectable == false) {
                    // node cannot be selected: parent should be refreshed - but only once!
                    SN.ExploreTree._currentConfig.nodeNotSelectable = true;
                    SN.ExploreTree._currentConfig.index = currentIndex - 1;
                    SN.ExploreTree._currentConfig.InSelection = true;
                    tree.close_branch("#" + SN.ExploreTree._currentConfig.openedNodes[currentIndex - 1], true);
                    SN.ExploreTree._currentConfig.InSelection = false;
                }
            }
        }
    },
    WindowNavigated: function() {
        // explore frame navigated
        $urlbox = $('#sn-exploretree-urlbox');
        $urlbox.val("unknown url");
        $contentbox = $("#sn-exploretree-contentname");
        $contentbox.val("none");
        $actionbox = $("#sn-exploretree-actionname");
        $actionbox.text("none");

        try {
            var exploreFrame = parent.frames["ExploreFrame"];

            // set url info
            var loc = parent.frames["ExploreFrame"].location;
            var href = loc.href;
            if (href.indexOf("ExploreFrame.html") != -1)
                return;

            var pathname = loc.pathname;
            $urlbox.val(href);

            // check if pathname starts with Root. if not, current site path should be added
            if (pathname.indexOf("/Root") != 0) {
                pathname = $("#sn-exploretree-currentsite").text() + pathname;
            }

            // set content
            contentLastIdx = pathname.indexOf("?");
            var content = "none";
            if (contentLastIdx == -1)
                content = pathname;
            else
                content = href.substring(0, contentLastIdx);
            $contentbox.val(content);

            this._currentConfig.previousContent = this._currentConfig.currentContent;
            this._currentConfig.currentContent = content;

            // set action
            actionIdx = href.indexOf("action=");
            var action = "none";
            if (actionIdx != -1) {
                actionLastIdx = href.indexOf('&', actionIdx);
                if (actionLastIdx == -1)
                    action = href.substring(actionIdx + 7);
                else
                    action = href.substring(actionIdx + 7, actionLastIdx);
            }
            $actionbox.text(action);

            this._currentConfig.previousAction = this._currentConfig.currentAction;
            this._currentConfig.currentAction = action;
            this._currentConfig.parentRefreshed = false;    // refresh parent only once
            SN.ExploreTree._currentConfig.nodeNotSelectable = false;    // refresh parent when node is not selectable only once

            // when explore frame is navigated because we clicked on the tree, ignore this event
            if (this._currentConfig.TreeNavigatesExplore == true) {
                this._currentConfig.TreeNavigatesExplore = false;
                return;
            }

            // when node moved or copied the target folder should be refreshed
            this._currentConfig.openingNodesFinished = null;
            if ((SN.ExploreTree._currentConfig.currentAction != 'MoveToTarget' && SN.ExploreTree._currentConfig.previousAction == 'MoveToTarget') ||
            (SN.ExploreTree._currentConfig.currentAction != 'CopyToTarget' && SN.ExploreTree._currentConfig.previousAction == 'CopyToTarget')) {
                this._currentConfig.openingNodesFinished = SN.ExploreTree.RefreshTargetFolder;
            }

            // otherwise navigate tree to the explore frame's location
            var path = pathname;
            SN.ExploreTree.NavigateOpenTreeToParentPath(path);

            // navigate parent frame to create handy urls
            if (SN.ExploreTree.UrlUpdateSupported())
                parent.location = parent.location.pathname + "#" + path;
        } catch (e) {
            alert("Explore frame has been navigated to an unknown url. Please note that the requested page should be viewed in a separate browser window.");
        };
    },
    ToggleHidden: function(show) {
        this._currentConfig.showSystemFiles = show;
        SN.ExploreTree.ToggleHiddenNodes(show);
    },
    ToggleHiddenNodes: function(show) {
        var systemNodes = $("a[issystem=true]");
        $.each(systemNodes, function() {
            if (show)
                $(this).parent().show();
            else
                $(this).parent().hide();
        });
    },

    // TREE FUNCTIONS ///////////////////////////////////////////////////////////////////
    UrlUpdateSupported: function() {
        return ($.browser.msie || $.browser.mozilla);
    },
    initLayout: function() {
        // default path
        var defaultPath = this._currentConfig["DefaultPath"];
        this._currentConfig.openedNodes = SN.ExploreTree.GetOpenedNodesArray(defaultPath);
        this._currentConfig.selectedNode = SN.ExploreTree.GenerateTreeNodeId(defaultPath);

        // available tree roots
        SN.ExploreTree.InitTreeRoots();
    },
    InitTreeRoots: function() {
        // check if tree root is given in config
        var treeRoots = this._currentConfig["TreeRoots"];
        if (treeRoots) {
            $('#sn-exploretree-treerootdiv').show();
            if (treeRoots.length == 1) {
                $('#sn-exploretree-treeroottextdiv').show();
                $('#sn-exploretree-treerootselectdiv').hide();
                $('#sn-exploretree-treeroottextdiv').html(treeRoots[0]);
            }
            else {
                $('#sn-exploretree-treeroottextdiv').hide();
                $('#sn-exploretree-treerootselectdiv').show();
                var i = 0;
                $.each(treeRoots, function() {
                    var newOption = '<option value=' + i + '>' + this + '</option>';
                    $("#sn-exploretree-treeroot").append(newOption);
                    i++;
                });
            }

            // set tree position depends on the treeRoots panel's visibility
            $('#sn-exploretree-treecontainer').css("top", $('#sn-exploretree-treerootdiv').outerHeight());

            // init tree
            // if no default path is given, init tree with first treeroot, otherwise navigatetree to corresponding treeroot and path
            if (this._currentConfig.DefaultPath == null) {
                SN.ExploreTree.setTreeRoot(treeRoots[0]);
            } else {
                SN.ExploreTree.NavigateTreeToParentPath(this._currentConfig.DefaultPath);
            }
        } else {
            // init tree
            SN.ExploreTree.setTreeRoot('/Root');
        }
    },
    setTreeRoot: function(rootPath) {
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
    GenerateTreeNodeId: function(path) {
        if ((path == null) || (path.length == 0))
            return false;

        return 'ExploreTreeNode_' + path.replace(/\W/g, "_");
    },
    GetOpenedNodesArray: function(path) {
        if ((path == null) || (path.length == 0))
            return [];

        var path2 = path.substring(1); // leading / should be trimmed
        var paths = path2.split('/');
        var openedNodes = [];
        var currentPath = "";
        for (i = 0; i < paths.length; i++) {
            currentPath = currentPath + "/" + paths[i];
            openedNodes[i] = SN.ExploreTree.GenerateTreeNodeId(currentPath);
        }
        return openedNodes;
    },
    getTree: function() {
        return $.tree.reference($('#' + SN.ExploreTree._currentConfig.treeContainerId));
    },
    destroyTree: function() {
        var tree = SN.ExploreTree.getTree();
        if (tree)
            tree.destroy();
        $("#sn-exploretree-treecontainer").html('<div id="sn-exploretree-tree"></div>');
    },
    initTree: function() {
        // destroy if already initialized
        this.destroyTree();

        var treeConfig = {
            callback: {
                beforedata: function(NODE, TREE_OBJ) {
                    var rp = SN.ExploreTree._currentConfig.searchRootPath;
                    if ((typeof rp === "undefined") || (rp == null)) {
                        return { path: $(NODE).find("a:first").attr("path"), rootonly: "0", rnd: Math.random() };
                    } else {
                        SN.ExploreTree._currentConfig.searchRootPath = null;
                        return { path: rp, rootonly: "1", rnd: Math.random() };
                    }
                },
                onselect: function(NODE, TREE_OBJ) {
                    var path = $(NODE).find("a:first").attr("path");
                    SN.ExploreTree.TreeNodeClicked(path);
                },
                onclose: function(NODE, TREE_OBJ) {
                    var childNodes = $(NODE).find("ul:first");
                    childNodes.remove();
                    if (SN.ExploreTree._currentConfig.InPathOpening) {
                        SN.ExploreTree.OpenNextTreeNode();
                    }
                },
                onopen: function(NODE, TREE_OBJ) {
                    if (SN.ExploreTree._currentConfig.InPathOpening) {
                        SN.ExploreTree.OpenNextTreeNode();
                    }
                    // state is altered above
                    if (!SN.ExploreTree._currentConfig.InPathOpening) {
                        SN.ExploreTree.ToggleHiddenNodes(SN.ExploreTree._currentConfig.showSystemFiles);
                    }
                },
                ondata: function(DATA, TREE_OBJ) {
                    var newResult = [];
                    $.each(DATA, function(i, d) {
                        var item = {};
                        item.data = {};
                        item.data.title = d.Name;
                        item.data.icon = d.IconPath;
                        item.data.children = {};
                        item.data.attributes = {
                            href: "#",
                            //id: d.Id,
                            id: SN.ExploreTree.GenerateTreeNodeId(d.Path),
                            path: d.Path,   // TODO: encode special chars
                            issystem: d.IsSystemContent,
                            style: d.IsSystemContent ? "color: Gray;" : "",
                            title: d.DisplayName
                        };
                        item.data.leaf = d.Leaf;
                        if (d.Leaf != true)
                            item.state = "closed";
                        newResult.push(item);
                    });
                    newResult.sort(SN.ExploreTree.SortChildren);
                    return newResult; //return DATA;
                },
                onparse: function(STR, TREE_OBJ) {
                    return STR;
                },
                onload: function(TREE_OBJ) {
                    SN.ExploreTree._currentConfig.InNavigation = false;
                }
            },
            data: {
                async: true,
                type: "json",
                opts: {
                    async: true,
                    method: "GET",
                    url: "/ContentStore.mvc/GetTreeNodeAllChildren"
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
            opened: SN.ExploreTree._currentConfig.openedNodes,
            selected: SN.ExploreTree._currentConfig.selectedNode
        };

        var treeContainer = $('#' + SN.ExploreTree._currentConfig.treeContainerId);
        treeContainer.tree(treeConfig);

        var tree = SN.ExploreTree.getTree();
        this._currentConfig.tree = tree;

        // clear openednodes before next init
        SN.ExploreTree._currentConfig.openedNodes = [];
    },
    SortChildren: function(a, b) {
        if (a.data.title == null)
            return -1;
        if (b.data.title == null)
            return 1;
        if (a.data.leaf == null)
            return -1;
        if (b.data.leaf == null)
            return 1;
        if (a.data.leaf == b.data.leaf)
            return a.data.title.toLowerCase() > b.data.title.toLowerCase() ? 1 : -1;
        return a.data.leaf ? 1 : -1;
    },
    // tree root is selected from dropdown
    SelectTreeRoot: function() {
        // set searchroot
        var selectedRoot = $('#sn-exploretree-treeroot option:selected').text();
        SN.ExploreTree.TreeNodeSelected(selectedRoot);

        // init tree
        SN.ExploreTree.setTreeRoot(selectedRoot);
    },
    TreeNodeClicked: function(path) {
        // when navigating the tree, don't percept selections
        if (this._currentConfig.InNavigation == true)
            return;

        // when selecting a tree node, don't percept selections
        if (this._currentConfig.InSelection == true)
            return;

        // navigate explore frame
        var action = $("#sn-exploretree-treeactions input:checked").val();
        parent.frames["ExploreFrame"].location = path + (action == "Browse" ? "" : "?action=" + action);

        // navigate parent frame to create handy urls
        if (this.UrlUpdateSupported())
            parent.location = parent.location.pathname + "#" + path;

        this._currentConfig.TreeNavigatesExplore = true;
    }
}

$(document).ready(function () {
    var targetPath = parent.location.hash.substring(1);
    SN.ExploreTree.open({ DefaultPath: targetPath });

    // init JQuery UI elements
    $(".sn-button").button();
    $("#sn-exploretree-treeactions").buttonset().change(function () { SN.ExploreTree.TreeNodeClicked($("#sn-exploretree-contentname").val()); });
    $(window).resize(function () {
        var $controls = $("#sn-exploretree-controls");
        var newTop = $controls.offset().top + $controls.outerHeight();
        $("#sn-exploretree-treediv").css("top", newTop + "px");
    });

});