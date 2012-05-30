$(document).ready(function() {
    /* The jQuery selector selects input fields with '.tbSearchInput' css class 
    and calls the autcomplete function on these items.
    The '/ContTagManager.mvc/GetTags' uses the TagManagerController class' GetTags function.
    This function returns the matched tags from the system in JSON.  */
    $(".sn-tags-input").autocomplete(window.location.protocol + '//' + window.location.host + '/ContTagManager.mvc/GetTags', {
        dataType: 'json',
        minChars: 0,
        delay: 400,
        max: 50,
        matchContains: true,
        autoFill: false,
        mustMatch: false,
        scrollHeight: 220,
        highlight: function(match, keywords) {
            keywords = keywords.split(' ').join('|');
            return match.replace(new RegExp("(" + keywords + ")", "gi"), '<b>$1</b>');
        }
    });
});