/// <depends path="$skin/scripts/sn/SN.js" />
/// <depends path="$skin/scripts/jquery/jquery.js" />
/// <depends path="$skin/scripts/jqueryui/minified/jquery-ui.min.js" />

SN.Util = {

    //
    // JQuery UI Related Utility functions
    //

    // Create UI INTERFACE from JQuery collections with default skin classes
    // example: SN.Util.CreateUIInterface();

    CreateUIInterface: function($scope) {
        if (!$scope) { $scope = $("body"); }
        SN.Util.CreateUIButton($(".sn-button", $scope));
        SN.Util.CreateUIAccordion($(".sn-accordion", $scope));
        SN.Util.CreateUIPager($(".sn-pager", $scope));
    },


    // Create UI BUTTON from JQuery collections (option parameter is optional)
    // example: SN.Util.CreateUIButton($(".sn-button"),{disabled:true})

    CreateUIButton: function($elements, options) {
        if ($elements.length != 0) {
            $elements.button(options);
        }
    },

    // Create UI ACCORDION from JQuery collections (option parameter is optional)
    // example: SN.Util.CreateUIAccordion($(".sn-accordion"),{autoHeight:false})

    CreateUIAccordion: function($elements, options) {
        if ($elements.length != 0) {
            $elements.accordion(options);
        }
    },


    // Create UI PAGER from JQuery collections (option parameter is optional)
    // The collection have to contains the predefined css classes for buttons
    // example: SN.Util.CreateUIPager($(".sn-pager"))

    CreateUIPager: function($elements, options) {
        if ($elements.length != 0) {
            $elements.each(function() {
                $(".sn-pager-first", this).button({
                    text: false,
                    icons: {
                        primary: 'ui-icon-seek-start'
                    }
                });
                $(".sn-pager-prev", this).button({
                    text: false,
                    icons: {
                        primary: 'ui-icon-seek-prev'
                    }
                });
                $(".sn-pager-next", this).button({
                    text: false,
                    icons: {
                        primary: 'ui-icon-seek-next'
                    }
                });
                $(".sn-pager-last", this).button({
                    text: false,
                    icons: {
                        primary: 'ui-icon-seek-end'
                    }
                });
                $(".sn-pager-item", this).button();
                $(".sn-pager-active", this).button("disable");
                $(".sn-pager-active", this).toggleClass("ui-state-disabled ui-state-active");
            });
        }
    },

    // Create Admin UI dialog

    CreateAdminUIDialog: function($element, options) {

        // Initialize admin dialog

        // add default css class for the dialog
        options.dialogClass = (options.dialogClass === undefined) ? "sn-admin sn-admindialog" : "sn-admin sn-admindialog " + options.dialogClass;

        // Setup default open functionality
        $element.bind("dialogopen", function(event, ui) {

            var dialog = $(this).parent(".ui-dialog");
            var overlay = dialog.prev(".ui-widget-overlay");

            if (!overlay.hasClass("sn-adminoverlay")) overlay.addClass("sn-adminoverlay");
            if (overlay.parent("body").length > 0) overlay.appendTo($("body > form"));

            if (!dialog.hasClass("sn-admindialog")) dialog.addClass("sn-admin sn-admindialog");
            if (dialog.parent("body").length > 0) dialog.appendTo($("body > form"));

        });
        var el = $element.dialog(options);
        return el;
    },

    // Create UI dialog

    CreateUIDialog: function($element, options) {

        // Initialize dialog

        // add default css class for the dialog
        options.dialogClass = (options.dialogClass === undefined) ? "sn-dialog" : "sn-dialog " + options.dialogClass;

        // setup default open function
        $element.bind("dialogopen", function(event, ui) {
            var dialog = $(this).parent(".ui-dialog");
            var overlay = dialog.prev(".ui-widget-overlay");

            if (!overlay.hasClass("sn-overlay")) overlay.addClass("sn-overlay");
            if (overlay.parent("body").length > 0) overlay.appendTo($("body > form"));

            if (!dialog.hasClass("sn-dialog")) dialog.addClass("sn-dialog");
            if (dialog.parent("body").length > 0) dialog.appendTo($("body > form"));

        });
        var el = $element.dialog(options);
        return el;
    },

    //
    // Other Utility functions
    //

    //Togggle visibility of advanced fields panel on content views

    ToggleAdvancedPanel: function(showId, hideId, advancedPanelId) {
        $('#' + showId).toggle();
        $('#' + hideId).toggle();
        $('#' + advancedPanelId).toggle();
    },

    // init sn-submit button's special submit behavior
    InitSubmitButtonDisable: function() {
        var buttons = $(".sn-submit:not(.sn-notdisabled)");
        $.each(buttons, function() {
            $(this).click(function() {
                var $element = $(this);
                var newButton = $element.clone().removeAttr('name').removeAttr('id').addClass('sn-submit-disabled').attr('disabled', true);
                $element.after(newButton);
                $element.hide();
            });
        });
    },

    CheckComment: function () { 
            if ($('.sn-checkincompulsory').val()) { 
                $('#CheckInErrorPanel').hide(); 
                return true; 
            } else { 
                $('#CheckInErrorPanel').show(); 
                return false; 
            } 
    }
}
