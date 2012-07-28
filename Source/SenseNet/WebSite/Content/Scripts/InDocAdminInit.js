$(function () {
    function existingNamingsViewModel() {
        var self = this;
        var initialData = 
        self.ExistingNamings = ko.observableArray(initialData);
    }
    ko.applyBindings(new existingNamingsViewModel(), document.getElementById("namings-control"));
});