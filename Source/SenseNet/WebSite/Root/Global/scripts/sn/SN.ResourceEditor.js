SN.ResourceEditor = {
    langs: [],
    init: function () {
        SN.ResourceEditor.langs = arguments;
        var styles = "<style>.sn-redit-left {float:left;width:100px;}.sn-redit-right {float:left;width:300px;} .sn-redit-input{width:300px;} .sn-redit-hleft {float:left;width:150px;}.sn-redit-hright {float:left;width:250px;}.sn-redit-headerline{padding:5px 0 5px;} #sn-redit-classname,#sn-redit-name{font-weight:bold;}</style>";

        var header1 = "<div class='sn-redit-headerline'><div class='sn-redit-hleft'>ClassName: </div><div class='sn-redit-hright'><span id='sn-redit-classname'></span></div><div style='clear:both;'></div></div>";
        var header2 = "<div class='sn-redit-headerline'><div class='sn-redit-hleft'>Name: </div><div class='sn-redit-hright'><span id='sn-redit-name'></span></div><div style='clear:both;'></div></div>";

        var header = "<div style='border-bottom:1px solid #DDD; margin-bottom:10px; padding-bottom:10px;'>" + header1 + header2 + "</div>";

        var editors = "";
        for (var i = 0; i < arguments.length; i++) {
            editors += "<div><div class='sn-redit-left'>" + arguments[i] + "</div><div class='sn-redit-right'><input class='sn-redit-input' id='sn-redit-" + arguments[i] + "' type='text' /></div><div style='clear:both;'></div></div>";
        }
        var editor = editors;

        var content = "<div style='padding:10px;'>" + header + editor + "</div>";
        var buttons = "<div style='background-color:#F7F7F7; border:1px solid #DDD; width:auto; height:37px;'><div style='float:right; margin:5px;'><input type='button' class='sn-submit sn-button sn-notdisabled ui-button ui-widget ui-state-default ui-corner-all' value='Save' onclick='SN.ResourceEditor.save();return false;' /><input type='button' class='sn-submit sn-button sn-notdisabled ui-button ui-widget ui-state-default ui-corner-all' value='Cancel' onclick='SN.ResourceEditor.cancel();return false;' /></div></div>";
        var dialogMarkup = styles + "<div id='sn-resourceDialog' style='padding:0px !important;font-size:small;margin:0px;'>" + content + buttons + "</div>";
        $('body').append(dialogMarkup);
        var dialogOptions = { title: 'Edit string resource', modal: true, zIndex: 10000, width: 420, height: 'auto', minHeight: 0, maxHeight: 500, minWidth: 320, resizable: false, autoOpen: false };
        SN.Util.CreateUIDialog($('#sn-resourceDialog'), dialogOptions);
    },
    save: function () {
        var resources = [];
        for (var i = 0; i < SN.ResourceEditor.langs.length; i++) {
            var resource = { Lang: SN.ResourceEditor.langs[i], Value: $('#sn-redit-' + SN.ResourceEditor.langs[i]).val() };
            resources.push(resource);
        }
        $.getJSON('/ResourceEditor.mvc/SaveResource',
			{
			    classname: SN.ResourceEditor.classname,
			    name: SN.ResourceEditor.name,
			    resources: JSON.stringify(resources),
			    rnd: Math.random()
			},
			SN.ResourceEditor.saveCallback);
    },
    saveCallback: function () {
        location = location;
    },
    editResource: function (classname, name) {
        SN.ResourceEditor.classname = classname;
        SN.ResourceEditor.name = name;
        $('#sn-redit-classname').text(classname);
        $('#sn-redit-name').text(name);
        $('#sn-resourceDialog').dialog('open');

        // request resources
        $.getJSON('/ResourceEditor.mvc/GetStringResources',
			{
			    classname: classname,
			    name: name,
			    rnd: Math.random()
			},
			SN.ResourceEditor.getResourcesCallback);
    },
    getResourcesCallback: function (resources) {
        for (var i = 0; i < resources.length; i++) {
            $('#sn-redit-' + resources[i].Key).val(resources[i].Value);
        }
    },
    cancel: function () {
        $('#sn-resourceDialog').dialog('close');
    }
}
