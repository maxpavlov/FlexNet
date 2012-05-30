/// <depends path="$skin/scripts/jquery/jquery.js" />

SN.ListGrid = {

    init: function (id) {
        var $listGrid = $("#" + id + " > table");

        //map listgrid elements to variables
        var Cols = $("thead th", $listGrid);
        var GroupColIndex = Cols.index($("th.sn-lg-col-groupby", $listGrid).eq(0));
        var $checkboxes = $("td.sn-lg-cbcol input", $listGrid);
        var $checkboxToggle = $("th.sn-lg-cbcol input", $listGrid);

        // init batch buttons        
        var $batchButtons = $("#" + id).closest(".sn-listview").find(".sn-batchaction");
        SN.ListGrid.disableBatchActions($batchButtons);

        if ($checkboxes.length > 0) {
            $checkboxToggle.change(function () {
                if ($(this).is(":checked")) {
                    $checkboxes.attr("checked", "checked");
                    SN.ListGrid.enableBatchActions($batchButtons);
                } else {
                    $checkboxes.removeAttr("checked");
                    SN.ListGrid.disableBatchActions($batchButtons);
                }
            });
        } else {
            $checkboxToggle.attr("disabled",true);
        }
        
        $checkboxes.change(function () {
            var checked = $checkboxes.filter(":checked").length;
            if (checked > 0) {
                SN.ListGrid.enableBatchActions($batchButtons);
                if (checked != $checkboxes.length) $checkboxToggle.removeAttr("checked"); else $checkboxToggle.attr("checked", "checked");
            } else {
                SN.ListGrid.disableBatchActions($batchButtons);
            }
        });

        if (GroupColIndex != -1) {
            var groupName = null;
            var groupId = -1;
            var Rows = $("tr:gt(0)", $listGrid);
            var RowGroups = [];

            Rows.each(function (index, element) {
                var $this = $(this);
                var GroupCell = $("td:eq(" + GroupColIndex + ")", this);
                var GroupCellValue = (GroupCell.text() == "" && GroupCell.has(".sn-actionlinkbutton")) ? GroupCell.find(".sn-actionlinkbutton").first().attr("title") : GroupCell.text();

                if (groupName !== GroupCellValue) {
                    groupId++;
                    groupName = GroupCellValue;
                    $this.before('<tbody class="sn-lg-groupheader"><tr class="sn-lg-row-group ui-state-default ui-widget-content"><td colspan="' + Cols.length + '"><span class="ui-icon ui-icon-triangle-1-e"></span>' + groupName + '</td></tr></tbody>');
                    var groupRow = $this.prev("tbody");
                    groupRow.attr("data-groupid", groupId);
                    groupRow.show();
                    groupRow.click(function () {
                        $(".ui-icon", this).toggleClass("ui-icon-triangle-1-s");
                        $(RowGroups[$(this).attr("data-groupid")]).toggle();
                    });
                    groupRow.find("tr").hover(function () { $(this).toggleClass("ui-state-hover"); });
                    $this.before('<tbody class="sn-lg-grouppanel"></tbody>');
                    RowGroups[groupId] = $this.prev("tbody");
                }
                RowGroups[groupId].append($this);

            });

            //strip default tbody from the table
            $listGrid.children("tbody").replaceWith(function () {
                return $(this).contents();
            });

        }

    },

    getSelectedIds: function (portletId) {
        var portlet = $("#" + portletId);
        var checkedItems = $("td.sn-lg-cbcol input:checked", portlet);
        var ids = "";
        $.each(checkedItems, function () {
            ids = ids + $(this).val() + ",";
        });
        ids = ids.substring(0, ids.length - 1);
        return ids;
    },

    redirectWithIds: function (portletId, actionName, paramName) {
        var idlist = SN.ListGrid.getSelectedIds(portletId);
        var requestPath = '?action=' + actionName + '&' + paramName + '=' + idlist + '&back=' + escape(location.href);
        location = requestPath;
    },

    disableBatchActions: function (buttons) {
        buttons.each(function () {
            var $this = $(this);
            $this.addClass("sn-disabled");
            $this.attr("disabled", "disabled");
        });
    },
    enableBatchActions: function (buttons) {
        buttons.each(function () {
            var $this = $(this);
            $this.removeClass("sn-disabled");
            $this.removeAttr("disabled");
        });
    }

}
