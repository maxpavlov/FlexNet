/// <depends path="$skin/scripts/sn/SN.Picker.js" />

SN.ReferenceGrid = {
    init: function (displayAreaId, outputTextareaId, addButtonId, changeButtonId, initialSelection, readOnly, isMultiSelect, pagerdivid, rownum) {
        var height = 160;
        var sortable = true;
        var resizable = true;
        if (!isMultiSelect) {
            height = 20;
            sortable = false;
            resizable = false;
            $("#" + displayAreaId).parent().addClass("sn-referencegrid-singleselect");
        }
        var colNames = ['', '', '', 'DisplayName', 'Path', 'NodeType', ''];
        var colModel = [
              { name: 'Id', index: 'Id', hidden: true },
              { name: 'IconPath', index: 'IconPath', hidden: true },
              { name: 'Icon', index: 'Icon', formatter: SN.ReferenceGrid.IconFormatter, width: 16, fixed: true, resizable: false, sortable: false, align: "center" },
              { name: 'DisplayName', index: 'DisplayName', width: 150, sortable: sortable, resizable: resizable },
              { name: 'Path', index: 'Path', width: 200, sortable: sortable, resizable: resizable },
              { name: 'NodeTypeTitle', index: 'NodeTypeTitle', width: 80, sortable: sortable, resizable: resizable },
              { name: '', index: 'Id', width: 16, formatter: SN.ReferenceGrid.removeButtonFormatter, formatoptions: { displayAreaId: displayAreaId, outputTextareaId: outputTextareaId }, sortable: sortable, resizable: false }
              ];
        if (readOnly) {
            colNames = ['', '', '', 'DisplayName', 'Path', 'NodeType'];
            colModel = [
              { name: 'Id', index: 'Id', hidden: true },
              { name: 'IconPath', index: 'IconPath', hidden: true },
              { name: 'Icon', index: 'Icon', formatter: SN.ReferenceGrid.IconFormatter, width: 16, fixed: true, resizable: false, sortable: false, align: "center" },
              { name: 'DisplayName', index: 'DisplayName', width: 150, sortable: sortable, resizable: resizable },
              { name: 'Path', index: 'Path', width: 200, sortable: sortable, resizable: resizable },
              { name: 'NodeTypeTitle', index: 'NodeTypeTitle', width: 80, sortable: sortable, resizable: resizable }
              ];
        }

        $("#" + displayAreaId).attr("outputTextArea", outputTextareaId);
        $("#" + displayAreaId).attr("addButtonId", addButtonId);
        $("#" + displayAreaId).attr("changeButtonId", changeButtonId);

        // initial button visibility
        if (!isMultiSelect && initialSelection != null && initialSelection.length > 0) {
            SN.ReferenceGrid.getAddButton(displayAreaId).hide();
            SN.ReferenceGrid.getChangeButton(displayAreaId).show();
        } else {
            SN.ReferenceGrid.getAddButton(displayAreaId).show();
            SN.ReferenceGrid.getChangeButton(displayAreaId).hide();
        }

        var $grid = $("#" + displayAreaId);
        $grid.jqGrid({
            datatype: 'local',
            data: initialSelection,
            height: height,
            width: 505,
            shrinkToFit: false,
            colNames: colNames,
            colModel: colModel,
            sortname: 'DisplayName',
            gridview: true,
            rowNum: rownum == 0 ? 1000000 : rownum,
            pager: isMultiSelect ? (rownum == 0 ? null : '#' + pagerdivid) : null
        });
    },
    IconFormatter: function (index, cellvalue, dataItem) {
        return "<img src='" + dataItem.IconPath + "'/>";
    },
    addToGrid: function (displayAreaId, selection) {
        for (var i = 0; i < selection.length; i++) $("#" + displayAreaId).jqGrid('addRowData', selection[i].Id, selection[i], 'last');
        SN.ReferenceGrid.dataChanged(displayAreaId);
    },
    callBackMultiSelect: function (displayAreaId, resultData) {
        if (!resultData)
            return;
        SN.ReferenceGrid.addToGrid(displayAreaId, resultData);
    },
    callBackSingleSelect: function (displayAreaId, resultData) {
        if (!resultData)
            return;

        // clear grid
        SN.ReferenceGrid.clearButtonHandler(displayAreaId);
        SN.ReferenceGrid.addToGrid(displayAreaId, resultData);

        // show change button, hide add button
        SN.ReferenceGrid.getChangeButton(displayAreaId).show();
        SN.ReferenceGrid.getAddButton(displayAreaId).hide();
    },
    addButtonHandler: function (displayAreaId, treeRoots, defaultPath, multiSelectmode, allowedContentTypes, defaultContentTypes) {
        var _selected = SN.ReferenceGrid.rowPaths(displayAreaId);

        var callBackFunction = multiSelectmode == 'none' ? this.callBackSingleSelect : this.callBackMultiSelect;

        SN.PickerApplication.open({
            TreeRoots: treeRoots,
            DefaultPath: defaultPath,
            MultiSelectMode: multiSelectmode,
            AllowedContentTypes: allowedContentTypes,
            SelectedNodePaths: _selected,
            DefaultContentTypes: defaultContentTypes,
            callBack: function (resultData) { callBackFunction(displayAreaId, resultData); }
        });
    },
    removeButtonHandler: function (id, displayAreaId) {
        var success = $("#" + displayAreaId).jqGrid('delRowData', id);
        SN.ReferenceGrid.dataChanged(displayAreaId);
        if (!success) {
            alert('Could not find item with rowIndex: ' + id);
        }
    },
    clearButtonHandler: function (displayAreaId) {
        var outputTextareaId = $("#" + displayAreaId).attr("outputTextArea");

        $("#" + displayAreaId).jqGrid('clearGridData');
        $("#" + outputTextareaId).val("");

        // hide change button, show add button
        SN.ReferenceGrid.getChangeButton(displayAreaId).hide();
        SN.ReferenceGrid.getAddButton(displayAreaId).show();
    },
    removeButtonFormatter: function (cellvalue, cell, node) {
        var displayAreaId = cell.colModel.formatoptions.displayAreaId;

        return "<a class='sn-icon-small sn-icon-button snIconSmall_Delete' id='" + displayAreaId + "_removes_" + node.Id +
            "' style='height:20px;width:20px' type='button' onclick='SN.ReferenceGrid.removeButtonHandler(\"" + cell.rowId +
            "\", \"" + displayAreaId + "\");'></a>";
    },
    dataChanged: function (displayAreaId) {
        var outputTextareaId = $("#" + displayAreaId).attr("outputTextArea");

        var paths = SN.ReferenceGrid.rowPaths(displayAreaId);

        var pathString = paths.join("\; ");
        $("#" + outputTextareaId).val(pathString);

        // check if grid is empty
        if (pathString.length == 0) {
            // show add and hide change buttons
            SN.ReferenceGrid.getChangeButton(displayAreaId).hide();
            SN.ReferenceGrid.getAddButton(displayAreaId).show();
        }
    },
    rowPaths: function (displayAreaId) {
        var data = $("#" + displayAreaId).jqGrid('getGridParam', 'data');   // getrowdata returns visible rows only: data contains whole grid data
        return jQuery.map(data, function (row, index) { return row.Path; });
    },
    getAddButton: function (displayAreaId) {
        var addButtonId = $("#" + displayAreaId).attr("addButtonID");
        return $("#" + addButtonId);
    },
    getChangeButton: function (displayAreaId) {
        var changeButtonId = $("#" + displayAreaId).attr("changeButtonID");
        return $("#" + changeButtonId);
    }
}