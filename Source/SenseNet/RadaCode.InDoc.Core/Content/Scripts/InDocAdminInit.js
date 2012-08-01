$(function () {

    ko.bindingHandlers.jqButton = {
        init: function (element) {
            $(element).button();
        }
    };

    ko.bindingHandlers.dialog = {
        init: function (element, valueAccessor, allBindingsAccessor) {
            var options = ko.utils.unwrapObservable(valueAccessor()) || {};
            //do in a setTimeout, so the applyBindings doesn't bind twice from element being copied and moved to bottom
            setTimeout(function () {
                options.close = function () {
                    allBindingsAccessor().dialogVisible(false);
                };

                $(element).dialog(options);
            }, 0);

            //handle disposal (not strictly necessary in this scenario)
            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                $(element).dialog("destroy");
            });
        },
        update: function (element, valueAccessor, allBindingsAccessor) {
            var shouldBeOpen = ko.utils.unwrapObservable(allBindingsAccessor().dialogVisible);
            $(element).dialog(shouldBeOpen ? "open" : "close");
        }
    };

    function SpecialCode(code, hasValue) {
        var self = this;
        self.Code = code;
        self.HasValue = hasValue;
    };

    function Naming(name, nameBlocks, paramBlocks) {
        var self = this;
        self.TypeName = name;
        self.NameBlocks = ko.observableArray(nameBlocks);
        self.ParamBlocks = ko.observableArray(paramBlocks);
        self.isDialogOpen = ko.observable(false);
        self.dialogSelectedCode = ko.observable();
        self.dialogRadioValue = ko.observable('custom');
        self.dialogSelectedCodeValue = ko.observable();
        self.dialogSelectedCodeHasValue = ko.computed(function () {
            var selectedCode = ko.utils.unwrapObservable(self.dialogSelectedCode);
            return selectedCode && selectedCode.HasValue;
        });
        self.dialogDataToAdd = ko.computed(function () {
            var radioValue = ko.utils.unwrapObservable(self.dialogRadioValue);
            if (radioValue == 'custom') {
                return new {
                    Type: 'Custom',
                    Code: ko.utils.unwrapObservable(self.dialogSelectedCode),
                    Value: ko.utils.unwrapObservable(self.dialogSelectedCodeValue)
                };
            } else {
                return new {
                    Type: 'Text',
                    Value: ko.utils.unwrapObservable(self.dialogSelectedCodeValue)
                };
            }
        });

        self.addBlock = function () {
            self.isDialogOpen(true);
        };
    };

    function ExistingNamingsViewModel(initialData) {
        var self = this;

        var namings = $.makeArray(initialData.Namings);
        var specialCodes = $.makeArray(initialData.Codes);

        var namingModelsArray = jQuery.map(namings, function (val, i) {
            return (new Naming(val.TypeName, val.NameBlocks, val.ParamBlocks));
        });

        var codesModelsArray = jQuery.map(specialCodes, function (val, i) {
            return (new SpecialCode(val.Code, val.HasValue));
        });

        self.ExistingNamings = ko.observableArray(namingModelsArray);
        self.NamingCodes = ko.observableArray(codesModelsArray);
    };

    var initialDataObject = $.parseJSON($('#initial-namings-data').val());
    ko.applyBindings(new ExistingNamingsViewModel(initialDataObject), document.getElementById("namings-control"));
});