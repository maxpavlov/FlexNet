$(function () {

    function ParamBlocks() {

    }

    function FormatBlocks() {

    }

    function ExistingNamingApproach(name, format, params) {
        var self = this;

        self.typeName = ko.observable(name);
        self.formatBlocks = ko.observable(format);
        self.paramBlocks = ko.observable(params);
    }

    function NamingApproachesViewModel() {
        var self = this;

        self.ExistingNamings = ko.observableArray([]);
    }

    ko.applyBindings(new NamingApproachesViewModel());
})