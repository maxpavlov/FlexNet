// <reference name="MicrosoftAjax.js"/>
// <reference name="MicrosoftAjaxTemplates.js"/>

Type.registerNamespace('SenseNet.Portal.UI.Controls.ActionMenu');

SenseNet.Portal.UI.Controls.ActionMenu = function(element) {
  SenseNet.Portal.UI.Controls.ActionMenu.initializeBase(this, [element]);
}

SenseNet.Portal.UI.Controls.ActionMenu.prototype = {

    initialize: function ()
    {

        var clientId = this.get_element().id;
        var popupElementId = clientId + "_popup";
        var resultElementId = clientId + "_result";
        var loadingElementId = clientId + "_loadingText";
        var isLoaded = false;
        var $actionmenu = $("#" + clientId);
        var $actionmenuInner = $(".sn-actionmenu-inner", $actionmenu);

        $actionmenuInner.hover(function ()
        {
            if (!$dropdown || $dropdown.is(":hidden"))
            {
                $(this).addClass("ui-state-hover");
                $(this).removeClass("ui-state-default");
            }
        }, function ()
        {
            if (!$dropdown || $dropdown.is(":hidden"))
            {
                $(this).removeClass("ui-state-hover");
                $(this).addClass("ui-state-default");
            }
        });

        $actionmenuInner.append("<span class='sn-actionmenu-arrow ui-icon ui-icon-triangle-1-s'></span>");
        $(".sn-actionmenu-arrow", $actionmenuInner).hover(function ()
        {
            $actionmenuInner.trigger("mouseenter");
        },
        function ()
        {
            $actionmenuInner.trigger("mouseleave");
        });

        var $dropdown;

        var hideActionMenu = function ()
        {
            $actionmenuInner.removeClass("ui-state-active");
            $actionmenuInner.addClass("ui-state-default");
            $dropdown.slideUp(100, function ()
            {
                $('body').unbind("click", hideActionMenu);
                $(window).unbind("resize", repositionDropDown);
            });
        }
        var showDropDown = function ()
        {
            $actionmenuInner.addClass("ui-state-active");
            $actionmenuInner.removeClass("ui-state-default ui-state-hover");
            $dropdown.slideDown(100, function ()
            {
                $('body').bind("click", hideActionMenu);
                $(window).bind("resize", repositionDropDown);
            });
        }
        var repositionDropDown = function ()
        {

            if ($dropdown && $actionmenu)
            {
                var ddwidth = $dropdown.outerWidth();
                var amtop = $actionmenu.offset().top;
                var amheight = $actionmenu.outerHeight();
                var amleft = $actionmenu.offset().left;
                var ddheight = $dropdown.height();
                if (amleft + ddwidth > $("body").outerWidth()) amleft -= ddwidth - $actionmenu.outerWidth();


                var spaceBottom = true;
                var spaceTop = true;
                var xPos = amtop - $(window).scrollTop();
                if (xPos + amheight + ddheight > $(window).height())
                {
                    spaceBottom = false;
                }
                if (amtop - ddheight < 0)
                {
                    spaceTop = false;
                }

                if ((spaceBottom == false && spaceTop == true))
                {
                    var ntop = parseInt(amtop, 10) - parseInt(ddheight, 10);
                } else
                {
                    amtop = amtop + amheight;
                    var ntop = parseInt(amtop, 10);
                }

                $dropdown.css("left", amleft + "px");
                $dropdown.css("top", ntop + "px");
            }


        }
        var resizeDropDown = function ()
        {
            var minw = parseInt($dropdown.css("min-width"));
            var outerw = $actionmenu.outerWidth();
            var ddwidth = (minw > outerw) ? minw : outerw;

            $dropdown.css("width", ddwidth + "px");
        }

        $actionmenuInner.click(function ()
        {
            if (isLoaded)
            {
                repositionDropDown();
                showDropDown();
                return true;
            }

            // create div
            $('body > form').append('<div id="' + popupElementId + '" class="sn-actionmenu-dd ui-widget ui-helper-hidden" ><div class="ui-widget-content ui-corner-all"><div id="' + loadingElementId + '" class="sn-actionmenu-loading">Loading...</div><ul id="' + resultElementId + '" class="ui-helper-reset" style="display:none;"></ul></div></div>');
            $dropdown = $("#" + popupElementId);

            resizeDropDown();
            repositionDropDown();

            // load data into div
            var jsonRequest = Sys.get("$" + clientId).ServiceUrl + "&jsrfrsh=" + new Date().getUTCMilliseconds();
            $.getJSON(jsonRequest, buildPopup);
        });

        var buildPopup = function (results)
        {
            isLoaded = true;
            $("#" + loadingElementId).remove();

            // load data into div
            var $menuItem = $("#" + resultElementId);
            if (results && results.length > 0)
            {
                $.each(results, function ()
                {
                    var icon = "";
                    var link = "";


                    if (this.IconTag)
                        icon = this.IconTag;


                    if (!($.browser.msie) && navigator.mimeTypes && navigator.mimeTypes["application/x-sharepoint"] && navigator.mimeTypes["application/x-sharepoint"].enabledPlugin)
                    {
                        var msoeditenabled = true;
                    }

                    if (!($.browser.msie) && !msoeditenabled && this.Text == "Edit in Microsoft Office")
                        link = '<div class="sn-actionlink ui-state-default ui-corner-all sn-disabled ' + this.CssClass + '" disabled="disabled">' + icon + this.Text + '</div>';
                    else if (this.Callback)
                        link = '<a href="javascript:" class="sn-actionlink ui-state-default ui-corner-all ' + this.CssClass + '" onclick="' + this.Callback + '">' + icon + this.Text + '</a>';
                    else if (this.Forbidden)
                        link = '<div class="sn-actionlink ui-state-default ui-corner-all sn-disabled ' + this.CssClass + '" disabled="disabled">' + icon + this.Text + '</div>';
                    else
                        link = '<a href="' + this.Uri + '" class="sn-actionlink ui-state-default ui-corner-all ' + this.CssClass + '">' + icon + this.Text + '</a>';

                    $menuItem.append('<li>' + link + '</li>');
                });
            } else
            {
                $menuItem.append('<li>No actions available</li>');
            }

            $menuItem.show();

            // loaded elements hover behavior
            var hoverCssClass = Sys.get("$" + clientId).ItemHoverCssClass;
            if (hoverCssClass === null)
                hoverCssClass = "ui-state-hover";
            var menuItems = $(".sn-actionlink", $menuItem);
            menuItems.hover(
                function ()
                {
                    $(this).addClass(hoverCssClass);
                    $(this).removeClass("ui-state-default");
                },
                function ()
                {
                    $(this).removeClass(hoverCssClass);
                    $(this).addClass("ui-state-default");
                }
            );

            resizeDropDown();
            repositionDropDown();
            showDropDown();
        }

    }
}

SenseNet.Portal.UI.Controls.ActionMenu.registerClass("SenseNet.Portal.UI.Controls.ActionMenu", Sys.UI.Control);
