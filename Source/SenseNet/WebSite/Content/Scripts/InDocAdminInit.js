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

    function CodeBlock(nameBlock, paramBlock, parent) {
        var self = this;
        self.Name = nameBlock;
        self.Param = paramBlock;
        self.CellWidth = ko.computed(function () {
            return parent.CellWidth();
        });
    }

    function SpecialCode(code, hasValue) {
        var self = this;
        self.Code = code;
        self.HasValue = hasValue;
    };

    function Naming(name, codeBlocks) {
        var self = this;
        self.TypeName = name;
        self.BlocksCount = ko.observable(codeBlocks.length);

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
                return {
                    Type: 'Custom',
                    Code: ko.utils.unwrapObservable(self.dialogSelectedCode),
                    Value: ko.utils.unwrapObservable(self.dialogSelectedCodeValue)
                };
            } else {
                return {
                    Type: 'Text',
                    Value: ko.utils.unwrapObservable(self.dialogSelectedCodeValue)
                };
            }
        });

        self.dialogOptions = {
            autoOpen: false,
            modal: true,
            buttons: {
                'Добавить': function () {
                    alert('adding block ' + ko.toJSON(self.dialogDataToAdd));
                    var toAdd = ko.utils.unwrapObservable(self.dialogDataToAdd);
                    if (toAdd.Type == "Custom") {
                        if (toAdd.Code.HasValue) {
                            self.CodeBlocks.push(new CodeBlock(toAdd.Code.Code, toAdd.Value, self));
                        } else {
                            self.CodeBlocks.push(new CodeBlock(toAdd.Code.Code, '', self));
                        }
                    } else {
                        self.CodeBlocks.push(new CodeBlock(toAdd.Value, '', self));
                    }
                    $(this).dialog('close');
                },
                'Отменить': function () { $(this).dialog('close'); }
            }
        };

        self.addBlock = function () {
            self.isDialogOpen(true);
        };

        self.CodeBlocks = ko.observableArray();

        self.CellWidth = ko.computed(function () {
            return (95 / (self.BlocksCount()) | 0) + '%';
        });

        var codeBlockModelsArray = jQuery.map(codeBlocks, function (val, i) {
            return (new CodeBlock(val.Code, val.Param, self));
        });

        self.CodeBlocks = ko.observableArray(codeBlockModelsArray);

        self.CodeBlocks.subscribe(function (newValue) {
            self.BlocksCount(newValue.length);
        });
    };

    function ExistingNamingsViewModel(initialData) {
        var self = this;

        var namings = $.makeArray(initialData.Namings);
        var specialCodes = $.makeArray(initialData.Codes);

        var namingModelsArray = jQuery.map(namings, function (val, i) {
            return (new Naming(val.TypeName, val.CodeBlocks));
        });

        var codesModelsArray = jQuery.map(specialCodes, function (val, i) {
            return (new SpecialCode(val.Code, val.HasValue));
        });

        self.ExistingNamings = ko.observableArray(namingModelsArray);
        self.NamingCodes = ko.observableArray(codesModelsArray);
    };

    var pageVM = new ExistingNamingsViewModel($.parseJSON($('#initial-namings-data').val()));

    ko.applyBindings(pageVM, document.getElementById("namings-control"));
});