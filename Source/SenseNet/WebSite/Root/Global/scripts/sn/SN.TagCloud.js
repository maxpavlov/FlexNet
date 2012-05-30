$(document).ready(function() {
    /* The jQuery selector selects all the Tags (anchors) from the Tag Cloud. */
    $(".sn-tags ul li a").each(function() {
        var temp = new Array(10);
        temp = $(this).text();
        var newText = "";

        /* Checks the Tags if they are longer than 10 characters */
        if (temp.length > 9) {
            newText = temp.substring(0, 7) + "...";
            $(this).text(newText);
        }
    });
});