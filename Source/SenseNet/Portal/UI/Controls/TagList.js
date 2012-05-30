// Register the namespace for the control.
Type.registerNamespace('SenseNet.Portal.UI.Controls');

// Define the control properties.
SenseNet.Portal.UI.Controls.TagList = function(element) {
    SenseNet.Portal.UI.Controls.TagList.initializeBase(this, [element]);
    this.ContentId = "";
    this.ListId = "";
    this.BtnId = "";
}

// Create the prototype for the control.
SenseNet.Portal.UI.Controls.TagList.prototype = {
    initialize: function() {
        SenseNet.Portal.UI.Controls.TagList.callBaseMethod(this, 'initialize');

        var currentId = this.ContentId;
        var listId = this.ListId;
        var btnId = this.BtnId;

        $("#" + btnId).click(function() {
            var inputValue = $(".snContentTagContainer input[type=text]").val();
            var temp = window.location.protocol + '//' + window.location.host + '/ContTagManager.mvc/AddTag?id=' + currentId + '&tag=' + inputValue;

            $.ajax({ url: window.location.protocol + '//' + window.location.host + '/ContTagManager.mvc/AddTag?id=' + currentId + '&tag=' + inputValue,
                context: document.body, success: function() {
                    alert("New tag (" + inputValue + ") has been added!");
                }
            });

            if (inputValue != "") {
                $("#" + btnId).parent().parent().find(".snTagList").each(function() {
                    $(this).append('<li><a href="?Action=SearchTag&TagFilter=' + inputValue + '>' + inputValue + '</a></li>');
                });

                // after click, clear new tag's input field
                $(".snContentTagContainer input[type=text]").each(function() {
                    $(this).attr("value", "");
                });
            }
        });
    },

    dispose: function() {
        SenseNet.Portal.UI.Controls.TagList.callBaseMethod(this, 'dispose');
    },
    get_ContentId: function() {
        return this.ContentId;
    },
    set_ContentId: function(value) {
        this.ContentId = value;
    },
    get_ListId: function() {
        return this.ListId;
    },
    set_ListId: function(value) {
        this.ListId = value;
    },
    get_ButtonId: function() {
        return this.ButtonId;
    },
    set_ButtonId: function(value) {
        this.ButtonId = value;
    }
}

// Register the class as a type that inherits from Sys.UI.Control.
SenseNet.Portal.UI.Controls.TagList.registerClass('SenseNet.Portal.UI.Controls.TagList', Sys.UI.Control);


if (typeof (Sys) !== 'undefined')
    Sys.Application.notifyScriptLoaded();