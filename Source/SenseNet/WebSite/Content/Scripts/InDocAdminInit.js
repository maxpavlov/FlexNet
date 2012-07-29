$(function () {

    function Naming(name, nameBlocks, paramBlocks) {
        var self = this;
        self.TypeName = name;
        self.NameBlocks = ko.observableArray(nameBlocks);
        self.ParamBlocks = ko.observableArray(paramBlocks);

        self.addBlock = function () {
            $("#dialog").dialog();
            alert("I am to add a block to this particular naming!");
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