// using $skin/scripts/sn/SN.js
// using $skin/scripts/sn/SN.Util.js
// using $skin/scripts/jquery/plugins/jquery.cookie.js
// using $skin/scripts/sn/SN.Picker.js

/// <reference path="jquery/jquery.vsdoc.js"/>
SN.PortalRemoteControl = {
    PRCToolbarId: "snGeneratedID",   // do not modify, its value comes from server-side
    PRCIconId: "snGeneratedID",      // do not modify, its value comes from server-side
    PRCDialog: null,
    PRCInitialize: function(ctrl) {
        var prcShowCookie = $.cookie('prc-show');
        var prcx = $.cookie('prc-x');
        var prcy = $.cookie('prc-y');
        if (prcx == null || prcx == 'undefined')
            prcx = 100;
        if (prcy == null || prcy == 'undefined')
            prcy = 100;

        var config = {
            autoOpen: false,
            resizable: false,
            width: 231,
            position: [parseInt(prcx), parseInt(prcy)],
            closeOnEscape: false,
            dragStop: function(event, ui) {
                $.cookie('prc-x', ui.position.left, { path: '/', expires: 3000 });
                $.cookie('prc-y', ui.position.top, { path: '/', expires: 3000 });
            },
            close: function(event, ui) {
                SN.PortalRemoteControl.PRCClose();
            }
        };
        this.PRCDialog = SN.Util.CreateAdminUIDialog($('#' + ctrl), config);
        this.PRCDialog.dialog("widget").css("position", "fixed");
        // Restrict dragging to the viewport only
        //this.PRCDialog.dialog("widget").draggable("option", "containment", "window");

        // set hover for prc toolbar
        $('#' + this.PRCToolbarId).hover(function(a) {
            $(this).addClass("sn-prc-toolbar-hover");
        }, function(a) {
            $(this).removeClass("sn-prc-toolbar-hover");
        });
        // bind prc open for prc icon and toolbar buttons
        $('#' + this.PRCToolbarId + ', #' + this.PRCIconId).click(function() {
            if (SN.PortalRemoteControl.PRCDialog.dialog("isOpen")) {
                SN.PortalRemoteControl.PRCDialog.dialog("close");
            } else {
                SN.PortalRemoteControl.PRCOpen();
            }
        });

        if (prcShowCookie == 'true') {
            this.PRCOpen();
        } else {
            this.PRCClose();
        }

        // hide / show explore link
        var $exploreLink = $("#sn-prc-explorelink");
        if (window.parent.frames['ExploreTree'])
            $exploreLink.hide();
        else
            $exploreLink.show();

        // init statusbar
        $("#sn-prc-actions .sn-prc-button").hover(
            function() { $("#sn-prc-statusbar-text").html($(this).text()); },
            function() { $("#sn-prc-statusbar-text").html(""); }
            );
        $("#sn-prc-states .sn-prc-button").hover(
            function() { $("#sn-prc-statusbar-text").html($(this).attr("title")); },
            function() { $("#sn-prc-statusbar-text").html(""); }
            );
        
    },
    PRCOpen: function() {
        SN.PortalRemoteControl.PRCDialog.dialog("open");
        $("#" + this.PRCIconId).hide();
        $("#" + this.PRCToolbarId).removeClass("sn-prc-toolbar-open");
        $("#" + this.PRCToolbarId).addClass("sn-prc-toolbar-close");
        $.cookie('prc-show', true, { path: '/', expires: 3000 });
        return false;
    },
    PRCClose: function() {
        $("#" + this.PRCIconId).show();
        $("#" + this.PRCToolbarId).addClass("sn-prc-toolbar-open");
        $("#" + this.PRCToolbarId).removeClass("sn-prc-toolbar-close");
        $.cookie('prc-show', false, { path: '/', expires: 3000 });
    },

    // Edit portlet dialog
    showDialog: function(id, options) {
        SN.Util.CreateAdminUIDialog($('#' + id), options);
    },
    InitPortletEditorAccordion: function() {
        $("#ptEditAccordion").accordion({ header: "h3.sn-accordion-title", fillSpace: true });
    },
    ResizePortletEditorAccordion: function() {
        $("#ptEditAccordion").accordion("resize");
    }

};
