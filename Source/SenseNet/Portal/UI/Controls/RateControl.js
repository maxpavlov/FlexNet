/// <depends path="/Root/Global/scripts/sn/SN.Rating.js" />

// Register the namespace for the control.
Type.registerNamespace('SenseNet.Portal.UI.Controls');

// Define the control properties.
SenseNet.Portal.UI.Controls.RateControl = function(element) {
    SenseNet.Portal.UI.Controls.RateControl.initializeBase(this, [element]);
    this.ContentId = null;
    this.StarsId = null;
    this.HoverPanelId = null;
    this.RateValue = null;
    this.IsReadOnly = null;
}

// Create the prototype for the control.
SenseNet.Portal.UI.Controls.RateControl.prototype = {
    initialize: function() {
        SenseNet.Portal.UI.Controls.RateControl.callBaseMethod(this, 'initialize');

        SN.RatingControl.initialize(this.ContentId, this.StarsId, this.HoverPanelId, this.IsReadOnly, this.RateValue);
    },

    dispose: function() {
        SenseNet.Portal.UI.Controls.RateControl.callBaseMethod(this, 'dispose');
    },
    get_ContentId: function() {
        return this.ContentId;
    },
    set_ContentId: function(value) {
        this.ContentId = value;
    },
    get_StarsId: function() {
        return this.StarsId;
    },
    set_StarsId: function(value) {
        this.StarsId = value;
    },
    get_HoverPanelId: function() {
        return this.HoverPanelId;
    },
    set_HoverPanelId: function(value) {
        this.HoverPanelId = value;
    },
    get_RateValue: function() {
        return this.RateValue;
    },
    set_RateValue: function(value) {
        this.RateValue = value;
    },
    get_IsReadOnly: function() {
        return this.IsReadOnly;
    },
    set_IsReadOnly: function(value) {
        this.IsReadOnly = value;
    }
}

// Register the class as a type that inherits from Sys.UI.Control.
SenseNet.Portal.UI.Controls.RateControl.registerClass('SenseNet.Portal.UI.Controls.RateControl', Sys.UI.Control);


if (typeof (Sys) !== 'undefined')
    Sys.Application.notifyScriptLoaded();