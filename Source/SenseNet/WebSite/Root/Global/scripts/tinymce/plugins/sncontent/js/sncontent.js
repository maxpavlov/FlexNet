var cursor_position = -1;
var selected_node;
var selected_length = 0;

var SnContentDialog = {
    init: function(ed) {
        var ed = tinyMCEPopup.editor, se = ed.selection;
        var selected = se.getContent();
        selected_length = selected.length;

        // get first table parent having custom sncontentintext attribute
        var closestContent = $(se.getNode()).closest("table[sncontentintext]");
        if (closestContent.length != 0) {
            // existing sncontent
            selected_node = closestContent.get();
            $("#insertBtn").val("Update");

            // var contentAttr = se.getNode().className;
            var contentAttr = $(selected_node).attr("sncontentintext");

            var props = contentAttr.split(';');
            var contentPath = props[0];
            var displayPath = props[1];

            $("#tinyMceSnContent_ContentPath").val(contentPath);
            $("#tinyMceSnContent_DisplayPath").val(displayPath);
        }
        else {
            // new sncontent
            // get cursor position (hack: this method should work in all browsers)
            var save_full_text = ed.getContent();
            ed.execCommand('mceInsertContent', 0, "#cc#", {
                skip_undo: 1
            });
            cursor_position = ed.getContent().indexOf("#cc#");
            var editor_full_text = ed.getContent().replace("#cc#", selected);
            ed.setContent(editor_full_text);
        }
    },

    validateForm: function(contentPath, displayPath) {
        $("#tinymce_sncontent_content_errordiv").hide();
        $("#tinymce_sncontent_display_errordiv").hide();

        var error = 0;
        if (contentPath === '') {
            $("#tinymce_sncontent_content_errordiv").show();
            error = 1;
        }
        if (displayPath === '') {
            $("#tinymce_sncontent_display_errordiv").show();
            error = 1;
        }
        return error;
    },

    insertSnContent: function() {
        var contentPath = $("#tinyMceSnContent_ContentPath").val();
        var displayPath = $("#tinyMceSnContent_DisplayPath").val();

        // validation
        if (SnContentDialog.validateForm(contentPath, displayPath) == 1)
            return;

        var generatedHtml = SnContentHelper.getGeneratedHtml(contentPath, displayPath, selected_node);

        var ed = tinymce.EditorManager.activeEditor;

        // Fixes crash in Safari  
        if (tinymce.isWebKit)
            ed.getWin().focus();

        if (selected_node && $(selected_node).attr("sncontentintext")) {
            // refresh view
            $(selected_node).replaceWith(generatedHtml);
        }
        else {
            // insert new content to cursor position
            if (cursor_position != -1) {
                var editor_full_text = ed.getContent();
                var sub_text = editor_full_text.substring(0, cursor_position);
                var sub_text2 = editor_full_text.substring(cursor_position + selected_length);
                ed.setContent(sub_text + generatedHtml + sub_text2);
            } else {
                ed.execCommand('mceInsertContent', false, generatedHtml, {
                    skip_undo: 1
                });
                ed.undoManager.add();
            }
        }
        tinyMCEPopup.close();
    },

    showPopup: function(w) {
        $("#" + tinyMCEPopup.id, top.document).css("z-index", 1001);
        $("#mceModalBlocker", top.document).css("z-index", 1000);
    },

    log: function(message) {
        alert(message);
    },

    openContentPicker: function(targetId) {
        SnContentDialog.showPopup(window.parent);

        var selectedPath = $("#" + targetId).val();
        window.parent.SN.PickerApplication.open({ MultiSelectMode: 'none', SelectedNodePath: selectedPath, callBack: function(resultData) { if (!resultData) return; $("#" + targetId).val(resultData[0].Path); } });
    }
};

tinyMCEPopup.onInit.add(SnContentDialog.init, SnContentDialog);
