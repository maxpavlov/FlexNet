$(function () {

    $("#add-block-dialog").dialog({
        autoOpen: false,
        modal: true,
        buttons: {
            "Добавить": function () {
                alert("adding block");
            },
            "Отменить": function () {
                $(this).dialog("close");
            }
        },
        close: function (event, ui) { $(this).dialog('destroy').remove();}
    });

    function Naming(name, nameBlocks, paramBlocks) {
        var self = this;
        self.TypeName = name;
        self.NameBlocks = ko.observableArray(nameBlocks);
        self.ParamBlocks = ko.observableArray(paramBlocks);

        self.addBlock = function () {
            $("#add-block-dialog").dialog("open");
        };
    };

    function ExistingNamingsViewModel(namings) {
        var self = this;

        var namingModelsArray = jQuery.map(namings, function (val, i) {
            return (new Naming(val.TypeName, val.NameBlocks, val.ParamBlocks));
        });

        self.ExistingNamings = ko.observableArray(namingModelsArray);
    };


    ko.bindingHandlers.jqButton = {
        init: function (element) {
            $(element).button();
        }
    };

    var initialData = $('#initial-namings-data').val();
    var initialDataObject = $.parseJSON(initialData);
    var realArrayInitialData = $.makeArray(initialDataObject);
    ko.applyBindings(new ExistingNamingsViewModel(realArrayInitialData), document.getElementById("namings-control"));
});