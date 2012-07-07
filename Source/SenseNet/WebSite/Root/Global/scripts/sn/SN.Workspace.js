// using $skin/scripts/sn/SN.js
// using $skin/scripts/sn/SN.Util.js
// using $skin/scripts/jquery/jquery.js
// using $skin/scripts/jqueryui/minified/jquery-ui.min.js
// using $skin/scripts/sn/SN.Picker.js
// using $skin/scripts/sn/SN.LabelPicker.js


SN.WorkspaceMembers = {
    userPickDefaultText: 'Start typing...',
    removeConfirmDialogConf: {
        title: 'Remove item',
        modal: true,
        zIndex: 10000,
        width: 320,
        minWidth: 320,
        height: 'auto',
        minHeight: 0,
        maxHeight: 350,
        resizable: false,
        autoOpen: true,
        close: function () { $(this).dialog("destroy") }
    },
    addUserDialogConf: {
        title: 'Add users to workspace',
        modal: true,
        zIndex: 10000,
        width: 520,
        minWidth: 520,
        height: 'auto',
        minHeight: 0,
        maxHeight: 550,
        resizable: false,
        autoOpen: true,
        close: function () { $(this).dialog("destroy") }
    },
    showAll: function ($this) {
        $parent = $this.closest('.sn-workspacemembers');
        $('.sn-workspacemembers-showall', $parent).hide();
        $('.sn-workspacemembers-item-hidden', $parent).show();
    },
    remove: function ($this, id, path, displayname, groupId, groupName) {
        SN.Util.CreateUIDialog($('#sn-workspacemembers-removeconfirm'), SN.WorkspaceMembers.removeConfirmDialogConf);
        var user = $('#sn-workspacemembers-removeconfirm-name');
        user.text(displayname);
        user.attr('href', path);
        var group = $('#sn-workspacemembers-removeconfirm-groupname');
        group.text(groupName);
        $userid = $('#sn-workspacemembers-removeconfirm-userid');
        $groupid = $('#sn-workspacemembers-removeconfirm-groupid');
        $userid.val(id);
        $groupid.val(groupId);
    },
    okRemove: function () {
        $userid = $('#sn-workspacemembers-removeconfirm-userid');
        $groupid = $('#sn-workspacemembers-removeconfirm-groupid');
        $.getJSON('/Workspace.mvc/RemoveMember',
			{
			    groupId: $groupid.val(),
			    memberId: $userid.val(),
			    rnd: Math.random()
			},
			SN.WorkspaceMembers.membersAdded);
    },
    cancelRemove: function () {
        $('#sn-workspacemembers-removeconfirm').dialog("destroy");
    },
    add: function () {
        SN.Util.CreateUIDialog($('#sn-workspacemembers-adduser'), SN.WorkspaceMembers.addUserDialogConf);

        var labelpickers = $('.sn-workspacemembers-userpicker');
        $.each(labelpickers, function () {
            SN.LabelPicker.create({ container: $(this), autocompleteFunc: null, searchRoot: '/Root/IMS', contentTypes: 'User,Group', minLength: 2, createLink: true });
        });
    },
    addUserPick: function ($pickbutton) {
        SN.PickerApplication.open({ TreeRoots: ['/Root/IMS', '/Root'], AllowedContentTypes: ['User', 'Group'], callBack: SN.WorkspaceMembers.createAddMemberCallback($pickbutton) });
    },
    createAddMemberCallback: function ($pickbutton) {
        return function (resultData) {
            if (!resultData)
                return;

            $.each(resultData, function () {
                SN.LabelPicker.addLabel($pickbutton.prev(), this.Id, this.DisplayName, this.Path, true);
            });
        }
    },
    okAdd: function () {
        var labelpickers = $('.sn-workspacemembers-userpicker');
        var data = [];
        for (var i = 0; i < labelpickers.length; i++) {
            var labelIds = SN.LabelPicker.getLabelIds(labelpickers[i]);

            var ids = '';
            $.each(labelIds, function () {
                ids = ids + this + ',';
            });
            if (ids.length > 0)
                ids = ids.substring(0, ids.length - 1);

            var groupidinput = $(labelpickers[i]).prev();
            data[i] = { groupId: groupidinput.val(), ids: ids };
        }
        var dataStr = '[';
        for (var i = 0; i < data.length; i++) {
            if (i > 0)
                dataStr += ',';
            dataStr += SN.WorkspaceMembers.stringify(data[i]);
        }
        dataStr += ']';
        $.getJSON('/Workspace.mvc/AddMembers',
			{
			    //data: JSON.stringify(data), -> doesn't work in IE7
			    data: dataStr,
			    rnd: Math.random()
			},
			SN.WorkspaceMembers.membersAdded);
    },
    cancelAdd: function () {
        $('#sn-workspacemembers-adduser').dialog("destroy");
    },
    membersAdded: function () {
        location = location.href;
    },
    stringify: function (jsonData) {
        var strJsonData = '{';
        var itemCount = 0;
        for (var item in jsonData) {
            if (itemCount > 0) {
                strJsonData += ',';
            }
            temp = jsonData[item];
            if (typeof (temp) == 'object') {
                s = SN.Util.Stringify(temp);
            } else {
                s = '"' + temp + '"';
            }
            strJsonData += '"' + item + '":' + s;
            itemCount++;
        }
        strJsonData += '}';
        return strJsonData;
    }
}