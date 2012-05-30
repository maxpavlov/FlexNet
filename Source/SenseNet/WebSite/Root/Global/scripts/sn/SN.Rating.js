/// <depends path="$skin/scripts/sn/SN.js" />
/// <depends path="$skin/scripts/jquery/jquery.js" />
/// <depends path="$skin/scripts/jquery/plugins/jquery.rating.js" />


SN.RatingControl = {

    rating: function(contentId, starsId, hoverPanelId, isReadOnly, rateValue) {
        // enable rating on all content
        $("#" + starsId + " input[type=radio]").rating('enable');

        // if readOnly disable rating 
        /*if (isReadOnly) {
        $("#" + starsId + " input[type=radio]").rating('disable');
        }*/
        if (hoverPanelId != "") {
            // hover effect
            $("#" + starsId).attr("hoverPanelId", hoverPanelId);
            $("#" + starsId).hover(function() {
                SN.RatingControl.hoverEffectOn($(this).attr("hoverPanelId"))
            },
            function() {
                SN.RatingControl.hoverEffectOff($(this).attr("hoverPanelId"))
            });

            SN.RatingControl.updateHoverPanel(hoverPanelId, rateValue);
        }
    },
    hoverEffectOn: function(hoverPanelId) {
        $("#" + hoverPanelId).show();
    },
    hoverEffectOff: function(hoverPanelId) {
        $("#" + hoverPanelId).hide();
    },
    updateHoverPanel: function(hoverPanelId, rateValue) {
        // Setting the All and Avg values
        $("#" + hoverPanelId + " #rating-avg").html(rateValue.AverageRate);

        var i = 0;
        for (i = 0; i < rateValue.HoverPanelData.length; i++) {
            // It does the scaling in the graph
            $("#" + hoverPanelId).find("#rating-scale-" + (i + 1)).css("width", SN.RatingControl.toInt(rateValue.HoverPanelData[i].Value) + "%");

            // This sets the specific value for the item
            $("#" + hoverPanelId).find("#rating-value-" + (i + 1)).html("(" + rateValue.HoverPanelData[i].Value + "%)");
        }
    },
    toInt: function(n) { return Math.round(Number(n)); },
    initialize: function(contentId, starsId, hoverPanelId, isReadOnly, rateValue) {

        // when document is ready
        $(document).ready(function() {
            SN.RatingControl.rating(contentId, starsId, hoverPanelId, isReadOnly, rateValue);

            // GUI fix for IE
            $.each($.browser, function(i) {
                if ($.browser.msie) {
                    $("#" + hoverPanelId).each(function() {
                        $(this).find("div.rating-inside").css("background", "none");
                    });
                }
            });
        });

        // select average default
        var average = SN.RatingControl.toInt(rateValue.AverageRate * rateValue.Split);
        $("#" + starsId + " input:radio[value=" + average + "]").attr("checked", true);

        $("#" + starsId + " input[type=radio]").rating({
            // parameter to split the stars into parts, default: 1
            split: rateValue.Split,

            // callback
            callback: function(value, link) {
                $.ajax({ url: window.location.protocol + '//' + window.location.host + '/StarVotes.mvc/Rate?id=' + contentId + '&vote=' + value + '&isgrouping=' + rateValue.EnableGrouping,
                    beforeSend: function(a) { $("#" + starsId + " input[type=radio]").rating('disable'); },
                    context: hoverPanelId,
                    success: function(arg) {
                        SN.RatingControl.updateHoverPanel(hoverPanelId, arg);

                        if (!arg.Success) {
                            var error = arg.ErrorMessage;
                            if (error == null) {
                                error = "Error has occured!";
                            }
                            alert(error);
                            return false;
                        }
                    },
                    error: function(XMLHttpRequest, textStatus, errorThrown) {
                        alert("Unexcepted error!");
                    }
                });
            }
        });
        // hide labels
        var $starsId = $("#" + starsId);

        $($starsId).find("label").each(function() {
            $(this).hide();
            $starsId.show();
        });
    }
}

/* Rating Search Portlet field's value checking */
$(document).ready(function() {
    /* Checks if number were entered in the search fields on Rated Search Portlet */
    $('.sn-rating-search-btn').click(function() {

        if (isNaN($(".sn-rating-search-from").val())) {
            alert("From field: Only numbers can be entered in the field!");
            return false;
        } else if ($(".sn-rating-search-from").val() == "") {
            alert("From field: Please enter a value!");
            return false;
        } else if ($(".sn-rating-search-from").val() < 1 || $(".sn-rating-search-from").val() > 5) {
            alert("From field: Value must be between 1 and 5!");
            return false;
        } if (isNaN($(".sn-rating-search-to").val())) {
            alert("To field: Only numbers can be entered in the field!");
            return false;
        } else if ($(".sn-rating-search-to").val() == "") {
            alert("To field: Please enter a value!");
            return false;
        } else if ($(".sn-rating-search-to").val() < 1 || $(".sn-rating-search-to").val() > 5) {
            alert("To field: Value must be between 1 and 5!");
            return false;
        } else if ($(".sn-rating-search-from").val() > $(".sn-rating-search-to").val()) {
            alert("From field's value must be lower than To field's value!");
            return false;
        } else {
            return true;
        }
    });
});