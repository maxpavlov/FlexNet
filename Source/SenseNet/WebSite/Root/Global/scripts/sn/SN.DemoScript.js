$(function() {

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

});