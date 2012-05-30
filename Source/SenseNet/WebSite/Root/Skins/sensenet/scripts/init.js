/// <depends path="$skin/scripts/sn/SN.Util.js" />
/// <depends path="$skin/scripts/jquery/plugins/themeswitchertool.js" />

$(function() {

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
        function() { $(this).children("ul:first").slideDown(200); },
        // out
        function() { $(this).children("ul:first").stop(true, true).slideUp(200); });
    // second level
    $(".custommenu1 > li > ul > li").hover(
        // over
        function() { $(this).children("ul:first").fadeIn(200); },
        // out
        function() { $(this).children("ul:first").stop(true, true).fadeOut(200); });

    // init sn-submit button's special submit behavior
    SN.Util.InitSubmitButtonDisable();
});
