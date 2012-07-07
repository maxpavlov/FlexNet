/// <depends path="$skin/scripts/sn/SN.Util.js" />
/// <depends path="$skin/scripts/jquery/plugins/themeswitchertool.js" />

$(function () {

    // Init Theme switcher
    /*
    $("body").append("<div id='themeswitcher' style='position:fixed; left: 10px; top: 10px;'></div>");
    $("#themeswitcher").themeswitcher({
    height: 400
    });
    */

    // Init UI Interface
    SN.Util.CreateUIInterface($(".sn-layout-container"));

    // Custom menu
    // first level
    $(".custommenu1 > li").hover(
    // over
        function () { $(this).children("ul:first").slideDown(200); },
    // out
        function () { $(this).children("ul:first").stop(true, true).slideUp(200); });
    // second level
    $(".custommenu1 > li > ul > li").hover(
    // over
        function () { $(this).children("ul:first").fadeIn(200); },
    // out
        function () { $(this).children("ul:first").stop(true, true).fadeOut(200); });



    $(".sn-demo-features .desc").each(function () {
        $(this).hover(function () {
            $(this).stop().animate({ opacity: 1.0 }, 250);
        },
           function () {
               $(this).stop().animate({ opacity: 0.0 }, 250);
           });
    });

    var agentStr = navigator.userAgent;
    var mode;
    if ($.browser.msie) {
        if (agentStr.indexOf("Trident/5.0") > -1) {
            if (agentStr.indexOf("MSIE 7.0") > -1)
                mode = "ie9comp";
            else
                mode = "ie9";
        }
        else if (agentStr.indexOf("Trident/4.0") > -1) {
            if (agentStr.indexOf("MSIE 7.0") > -1)
                mode = "ie8comp";
            else
                mode = "ie8";
        }
        else
            mode = "ie7";
    }
    $('.sn-body').addClass(mode);

    if (((1280 >= screen.width) && (mode = "ie8")), (($.browser.msie) && (mode = "ie9comp") && (1280 >= screen.width))) {
        $(".sn-body").addClass("ie81024");
    }

    if (($.browser.msie) && (1280 >= document.documentElement.clientWidth))
    {
        $(".sn-body").addClass("ie81024");
    }

    if ($('body').hasClass('.ie9comp')) {
        $('.sn-column-half').last().addClass('secondcolumn');
    }

    $(".sn-index-header-button").mouseover(
        function () {
            $(this).addClass('hover');
        });

    $(".sn-index-header-button").mouseout(
        function () {
            $(this).removeClass('hover');
        });

    $(".sn-head-pager-right").click(
        function () {
            if ($('.sn-index-header1').hasClass('active')) {
                $('.sn-index-header1').removeClass('active');
                $('.sn-index-header1').fadeOut('2000');
                $('.sn-index-header1').hide();
                $('.sn-index-header2').addClass('active');
                $('.sn-index-header2').fadeIn('2000');
                $('.sn-index-header2').show();
                $(this).css('background', '#349e03');
                $(".sn-head-pager-left").css('background', '#349e03');
                if (($.browser.msie) && ($.browser.version == '8.0')) {
                    $(".sn-head-pager-left").css('background', 'url(/Root/Skins/sensenet/images/arrow_left_green_ie8.png) no-repeat');
                    $(".sn-head-pager-right").css('background', 'url(/Root/Skins/sensenet/images/arrow_right_green_ie8.png) no-repeat');
                }
                if (($.browser.msie) && (mode = "ie9comp")) {
                    $(".sn-head-pager-left").css('background', 'url(/Root/Skins/sensenet/images/arrow_left_green_ie8.png) no-repeat');
                    $(".sn-head-pager-right").css('background', 'url(/Root/Skins/sensenet/images/arrow_right_green_ie8.png) no-repeat');
                }
            }
            else {
                $('.sn-index-header2').removeClass('active');
                $('.sn-index-header2').fadeOut('2000');
                $('.sn-index-header2').hide();
                $('.sn-index-header1').addClass('active');
                $('.sn-index-header1').fadeIn('2000');
                $('.sn-index-header1').show();
                $(this).css('background', '#007dc6');
                $(".sn-head-pager-left").css('background', '#007dc6');
                if (($.browser.msie) && ($.browser.version == '8.0')) {
                    $(".sn-head-pager-left").css('background', 'url(/Root/Skins/sensenet/images/arrow_left_blue_ie8.png) no-repeat');
                    $(".sn-head-pager-right").css('background', 'url(/Root/Skins/sensenet/images/arrow_right_blue_ie8.png) no-repeat');
                }
                if (($.browser.msie) && (mode = "ie9comp")) {
                    $(".sn-head-pager-left").css('background', 'url(/Root/Skins/sensenet/images/arrow_left_blue_ie8.png) no-repeat');
                    $(".sn-head-pager-right").css('background', 'url(/Root/Skins/sensenet/images/arrow_right_blue_ie8.png) no-repeat');
                }
            }
        });

    $(".sn-head-pager-left").click(
        function () {
            if ($('.sn-index-header1').hasClass('active')) {
                $('.sn-index-header1').removeClass('active');
                $('.sn-index-header1').fadeOut('2000');
                $('.sn-index-header1').hide();
                $('.sn-index-header2').addClass('active');
                $('.sn-index-header2').fadeIn('2000');
                $('.sn-index-header2').show();
                $(this).css('background', '#349e03');
                $(".sn-head-pager-right").css('background', '#349e03');
                if (($.browser.msie) && ($.browser.version == '8.0')) {
                    $(".sn-head-pager-right").css('background', 'url(/Root/Skins/sensenet/images/arrow_right_green_ie8.png) no-repeat');
                    $(".sn-head-pager-left").css('background', 'url(/Root/Skins/sensenet/images/arrow_left_green_ie8.png) no-repeat');
                }
                if (($.browser.msie) && (mode = "ie9comp")) {
                    $(".sn-head-pager-right").css('background', 'url(/Root/Skins/sensenet/images/arrow_right_green_ie8.png) no-repeat');
                    $(".sn-head-pager-left").css('background', 'url(/Root/Skins/sensenet/images/arrow_left_green_ie8.png) no-repeat');
                }
            }
            else {
                $('.sn-index-header2').removeClass('active');
                $('.sn-index-header2').fadeOut('2000');
                $('.sn-index-header2').hide();
                $('.sn-index-header1').addClass('active');
                $('.sn-index-header1').fadeIn('2000');
                $('.sn-index-header1').show();
                $(this).css('background', '#007dc6');
                $(".sn-head-pager-right").css('background', '#007dc6');
                if (($.browser.msie) && ($.browser.version == '8.0')) {
                    $(".sn-head-pager-right").css('background', 'url(/Root/Skins/sensenet/images/arrow_right_blue_ie8.png) no-repeat');
                    $(".sn-head-pager-left").css('background', 'url(/Root/Skins/sensenet/images/arrow_left_blue_ie8.png) no-repeat');
                }
                if (($.browser.msie) && (mode = "ie9comp")) {

                    $(".sn-head-pager-right").css('background', 'url(/Root/Skins/sensenet/images/arrow_right_blue_ie8.png) no-repeat');
                    $(".sn-head-pager-left").css('background', 'url(/Root/Skins/sensenet/images/arrow_left_blue_ie8.png) no-repeat');
                }
            }



        });

    // init sn-submit button's special submit behavior
    SN.Util.InitSubmitButtonDisable();
});
