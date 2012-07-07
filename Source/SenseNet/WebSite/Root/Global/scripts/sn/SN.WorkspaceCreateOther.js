// using $skin/scripts/sn/SN.js
// using $skin/scripts/jquery/jquery.js

SN.ns('SN.WorkspaceCreateOther');

SN.WorkspaceCreateOther = {
    init: function() {
        var $typelist = $("#sn-createitem-types");

        if ($typelist.length != 0) {

            var $typegroup = $("li", $typelist); // array of content type groups
            var maxheight = 0; //for maximum height of list items

            // create the common info panel
            $typelist.before("<div id=\"sn-createitem-info\"><div id=\"sn-createitem-info-inner\" class=\"sn-group-0\"><h2>" + $("li:first > h2", $typelist).text() + "</h2><p>" + $("li:first > p", $typelist).text() + "</p></div></div>");

            var $infopanel = $("#sn-createitem-info-inner");
            var $infotitle = $("h2", $infopanel);
            var $infodesc = $("p", $infopanel);
            var $oldtitle = "";
            var $olddesc = "";

            $typegroup.each(function() {
                var $this = $(this);
                var $links = $("dl > dt > a", $this);

                //search the maximum height of list items
                if ($this.height() > maxheight) maxheight = $this.height();

                // switch active group on mouseover
                $this.hover(function() {
                    //over
                    var $li = $(this);
                    var idx = $typegroup.index($li); // get the index of the active group

                    if (!$typegroup.eq(idx).hasClass("sn-active")) {

                        $infopanel.fadeOut(50, function() {
                            $typegroup.removeClass("sn-active");

                            $li.addClass("sn-active");
                            // infopanel setup
                            $infopanel.attr("class", "sn-group-" + idx);

                            $oldtitle = $("h2", $this).text();
                            $olddesc = $("p", $this).text();

                            $infotitle.html($oldtitle);
                            $infodesc.html($olddesc);

                            $infopanel.fadeIn(100);
                        });
                    }
                }, function() {
                    //out
                })

                // set info text on mouseover
                $links.hover(function() {
                    //over
                    var $title = $(this).text();
                    var $desc = $(this).parent().next("dd").text();
                    $infotitle.html($title);
                    $infodesc.html($desc);
                }, function() {
                    //out
                    $infotitle.html($oldtitle);
                    $infodesc.html($olddesc);
                });

            });
            $typegroup.height(maxheight); //set the same height for every column

        }

    }
}

$(document).ready(function() {
    SN.WorkspaceCreateOther.init();
});

