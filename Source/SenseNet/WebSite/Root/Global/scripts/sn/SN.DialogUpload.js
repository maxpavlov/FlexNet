// using $skin/scripts/sn/SN.js
// using $skin/scripts/sn/SN.Util.js
// using $skin/scripts/jquery/jquery.js
// using $skin/scripts/jqueryui/minified/jquery-ui.min.js

SN.DialogUpload = {
    uploadDialogConf: {
        title: 'Select files to upload',
        modal: true,
        zIndex: 10000,
        height: 'auto',
        minHeight: 0,
        maxHeight: 200,
        minWidth: 320,
        resizable: false,
        autoOpen: false,
        close: function () { SN.DialogUpload.closingUploadDialog($(this)); }
    },

    openUploadDialog: function ($control) {
        var $container = $control.closest('.sn-du-container');
        var clientid = $('.sn-du-clientid', $container).val();
        var $dialog = $('.sn-du-uploaddialog-' + clientid);
        var $iframe = $('.sn-du-uploadframe', $dialog);
        var uploadPath = $('.sn-du-uploadpath', $container).val();
        var targetFolderName = $('.sn-du-targetfolder', $container).val();
        $iframe.attr('src', uploadPath + '/?action=IFrameUpload&TargetFolder=' + targetFolderName);

        $dialog.dialog('open');
    },
    closeUploadDialog: function ($control) {
        var $dialog = $control.closest('.sn-du-uploaddialog');
        $dialog.dialog('close');
    },
    closingUploadDialog: function ($dialog) {
        var $iframe = $('.sn-du-uploadframe', $dialog);
        $iframe.attr('src', '');

        var clientid = $('.sn-du-dialogclientid', $dialog).val();
        SN.DialogUpload.loadUploadedFiles(clientid);
    },
    loadUploadedFiles: function (clientid) {
        var $container = $('.sn-du-container-' + clientid);
        var uploadPath = $('.sn-du-uploadpath', $container).val();
        var targetFolderName = $('.sn-du-targetfolder', $container).val();
        var startUploadDate = $('.sn-du-startuploaddate', $container).val();

        $('.sn-du-uploadloading', $container).show();

        // load uploaded contents from repository
        $.ajax({
            url: '/DialogUpload.mvc/GetUserUploads',
            dataType: 'json',
            data: {
                startUploadDate: startUploadDate,
                path: uploadPath + '/' + targetFolderName,
                rnd: Math.random()
            },
            success: function (data) {
                $('.sn-du-uploadloading', $container).hide();
                var markup = '';
                for (var i = 0; i < data.length; i++) {
                    var content = data[i];
                    //markup += '<a href="javascript:" onclick="SN.DialogUpload.addLink($(this),\'' + content.Path + '\',\'' + content.Name + '\');">' + content.Name + '</a><br/>';
                    markup += '<a title="Click to embed in editor!" href="javascript:" onclick="SN.DialogUpload.addLink(\''+clientid+'\',\'' + content.Path + '\',\'' + content.Name + '\');"><img style="width:100px;" src="' + content.Path + '"></img></a>';
                }
                $('.sn-du-uploadlinks', $container).html(markup);
            }
        });
    },
    addLink: function (clientid, path, name) {
        var $container = $('.sn-du-container-' + clientid);
        var $parent = $container.parent();
        for (var i = 0; i < tinyMCE.editors.length; i++) {
            var editor = $(tinyMCE.editors[i].contentAreaContainer);
            if ($parent.find(editor).length == 1) {
                tinyMCE.editors[i].execCommand('mceInsertContent', false, '<a href="' + path + '"><img src="' + path + '" width="200" /></a>');
            }
        }
    }
};

$(document).ready(function () {
    var $dialogs = $('.sn-du-uploaddialog');
    SN.Util.CreateUIDialog($dialogs, SN.DialogUpload.uploadDialogConf);
});
