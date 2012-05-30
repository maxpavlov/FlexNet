(function () {
    var cursor_position = -1;
    var selectedNode;

    function insert(resultData) {
        if (!resultData)
            return;

        var ed = tinymce.EditorManager.activeEditor;

        // Fixes crash in Safari  
        if (tinymce.isWebKit)
            ed.getWin().focus();

        var args = {};

        // use later
        tinymce.extend(args, {
            src: resultData[0].Path
        });

        el = ed.selection.getNode();

        // this expression will be always false, because the clicking in the contentpicker changes the focus.
        if (selectedNode && selectedNode.nodeName == 'IMG' && el && el.nodeName == 'IMG') {
            // it is not working in Google Chrome and Safari, should have something like this:
            // $(selectedNode).replaceWith('<img id="__mce_tmp" src="' + resultData[0].Path + '" />');
            ed.dom.setAttribs(el, args);
        }
        else {
            if (cursor_position != -1) {
                var editor_full_text = ed.getContent();
                var sub_text = editor_full_text.substring(0, cursor_position);
                var sub_text2 = editor_full_text.substring(cursor_position + selected_length);
                ed.setContent(sub_text + '<img id="__mce_tmp" src="' + resultData[0].Path + '" />' + sub_text2);
            } else {
                ed.execCommand('mceInsertContent', false, '<img id="__mce_tmp" />', {
                    skip_undo: 1
                });
                ed.dom.setAttribs('__mce_tmp', args);
                ed.dom.setAttrib('__mce_tmp', 'id', '');
                ed.undoManager.add();
            }
        }
    }

    tinymce.create('tinymce.plugins.SenseNetContentPickerPlugin', {
        init: function (ed, url) {

            // Register commands
            ed.addCommand('mceContentPicker', function () {
                var selectedPath = null;
                selectedNode = ed.selection.getNode();
                if (selectedNode.nodeName == 'IMG') {
                    selectedPath = $(selectedNode).attr("_mce_src");
                }

                se = ed.selection;
                var selected = se.getContent();
                selected_length = selected.length;

                // get cursor position (hack: this method should work in all browsers)
                var save_full_text = ed.getContent();
                ed.execCommand('mceInsertContent', false, "#cc#", {
                    skip_undo: 1
                });
                cursor_position = ed.getContent().indexOf("#cc#");
                var editor_full_text = ed.getContent().replace("#cc#", selected);
                ed.setContent(editor_full_text);

                var treeRoots = SN.tinymceimagepickerparams == null ? null : SN.tinymceimagepickerparams.TreeRoots;
                var defaultPath = SN.tinymceimagepickerparams == null ? null : SN.tinymceimagepickerparams.DefaultPath;
                SN.PickerApplication.open({ MultiSelectMode: 'none', TreeRoots: treeRoots, DefaultPath: defaultPath, SelectedNodePath: selectedPath, callBack: insert, AllowedContentTypes: ['Image', 'File'] });
            });

            // Register buttons
            ed.addButton('snimage', { //image2
                title: 'insert image from Portal File Sytem',
                cmd: 'mceContentPicker'
            });
        },

        insert: insert,

        getInfo: function () {
            return {
                longname: 'Sense/Net 6.0 Content picker plugin for tinymce',
                author: 'sn',
                authorurl: 'http://www.sensenet.com',
                infourl: 'http://www.sensenet.com',
                version: '0.2'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('sncontentpicker', tinymce.plugins.SenseNetContentPickerPlugin);
})();