// hide toolbar
window.scrollTo(0, 1);

$(function () {

    $(".snm-flip").click(function () {
        $(this).toggleClass("snm-flipped");
        //return false;
    });
    /*
    .swipeleft(function () { $(this).addClass("snm-flipped"); return false; })
    .swiperight(function () { $(this).removeClass("snm-flipped"); return false; });

    $(".snm-flip section").animationComplete(function () { alert("anim complete"); })
    
    $(".ui-page").live("swipeleft", function () {
    var nextPage = $.mobile.activePage.next("div");
    if (nextPage.length > 0) $.mobile.changePage(nextPage, "slide", false, false);
    }).live("swiperight", function () {
    var prevPage = $.mobile.activePage.prev("div");
    if (prevPage.length > 0) $.mobile.changePage(prevPage, "slide", true, false);
    });

    $("body").bind("touchmove", function (event) {
    event.preventDefault();
    });
    */

    $("#snm-container").each(function () {
        var pageScroll = new iScroll(this, { scrollbarClass: 'snm-scrollbar', bounce: false, lockDirection: false });
    });

    //$("[class*='anim-']").addClass("run");

    $(".clickable-yellowbg").each(function () {
        $(this).bind("touchstart", function (e) {
            $(this).addClass("onclick-yellowbg");
            e.preventDefault();
        });
        $(this).bind("touchend", function (e) {
            $(this).removeClass("onclick-yellowbg");
            e.preventDefault();
        });
    });
    $(".clickable-snbluebg").each(function () {
        $(this).bind("touchstart", function (e) {
            $(this).addClass("onclick-snbluebg");
            e.preventDefault();
        });
        $(this).bind("touchend", function (e) {
            $(this).removeClass("onclick-snbluebg");
            e.preventDefault();
        });
    });

    /* USERPROFILE */
    $("#snm-userprofile-nav-info").click(function () {
        userProfileChangeTab("snm-userprofile-info", "snm-userprofile-nav-info");
    });
    $("#snm-userprofile-nav-wall").click(function () {
        userProfileChangeTab("snm-userprofile-wall", "snm-userprofile-nav-wall");
    });
    $("#snm-userprofile-nav-photos").click(function () {
        userProfileChangeTab("snm-userprofile-photos", "snm-userprofile-nav-photos");
    });

    function userProfileChangeTab(selectedTab, selectedMenuItem) {
        $("#snm-userprofile-content div.snm-tab-active").hide('fast').removeClass("snm-tab-active");
        $("#" + selectedTab).addClass("snm-tab-active").show('fast')
        $("#snm-userprofile-nav ul li a.snm-item-active").removeClass("snm-item-active");
        $("#" + selectedMenuItem).addClass("snm-item-active");
    }

    iScroll.prototype.handleEvent = function (e) {
        var that = this,
		hasTouch = 'ontouchstart' in window && !isTouchPad,
		vendor = (/webkit/i).test(navigator.appVersion) ? 'webkit' :
				 (/firefox/i).test(navigator.userAgent) ? 'Moz' :
				 'opera' in window ? 'O' : '',
		RESIZE_EV = 'onorientationchange' in window ? 'orientationchange' : 'resize',
		START_EV = hasTouch ? 'touchstart' : 'mousedown',
		MOVE_EV = hasTouch ? 'touchmove' : 'mousemove',
		END_EV = hasTouch ? 'touchend' : 'mouseup',
		CANCEL_EV = hasTouch ? 'touchcancel' : 'mouseup',
		WHEEL_EV = vendor == 'Moz' ? 'DOMMouseScroll' : 'mousewheel';

        switch (e.type) {
            case START_EV:
                if (that.checkInputs(e.target.tagName)) {
                    return;
                }

                if (!hasTouch && e.button !== 0) return;

                that._start(e);
                break;
            case MOVE_EV:
                that._move(e);
                break;
            case END_EV:
                if (that.checkInputs(e.target.tagName)) {
                    return;
                }
            case CANCEL_EV:
                that._end(e);
                break;
            case RESIZE_EV:
                that._resize();
                break;
            case WHEEL_EV:
                that._wheel(e);
                break;
            case 'mouseout':
                that._mouseout(e);
                break;
            case 'webkitTransitionEnd':
                that._transitionEnd(e);
                break;
        }
    }

    iScroll.prototype.checkInputs = function (tagName) {
        if (tagName === 'INPUT' || tagName === 'TEXTFIELD' || tagName === 'SELECT') {
            return true;
        } else {
            return false;
        }
    }

});