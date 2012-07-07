// using $skin/scripts/sn/SN.js
// using $skin/scripts/sn/SN.Util.js
// using $skin/scripts/jquery/jquery.js
// using $skin/scripts/jqueryui/minified/jquery-ui.min.js

SN.LabelPicker = {

    // public functions
    create: function (options) {
        if (options.createLink == null)
            options.createLink = true;

        var $container = options.container;

        // check if markup already created
        if ($container.hasClass('sn-labelpicker'))
            return;

        $container.addClass('sn-labelpicker');

        // setup layout
        var markup = '<span class="sn-labelpicker-labels"></span><span class="sn-labelpicker-inputcontainer">' +
            '<input type="text" class="sn-labelpicker-inputbox sn-labelpicker-unfocused" value="Start typing..." onfocus="SN.LabelPicker.onfocusInputBox($(this));" onblur="SN.LabelPicker.onblurInputBox($(this));" />' +
            '</span>';

        $container.append(markup);
        $container.bind('click', SN.LabelPicker.containerClicked);

        var $container = options.container;
        var $inputbox = $('.sn-labelpicker-inputbox', $container);

        $inputbox.keydown(SN.LabelPicker.createkeydown($container, $inputbox));
        $inputbox.keyup(SN.LabelPicker.createkeyup($container, $inputbox));

        // setup autocomplete for adduser
        if (options.autocompleteFunc)
            options.autocompleteFunc(options, $container, $inputbox);
        else
            SN.LabelPicker.defineAutocomplete(options, $container, $inputbox);
    },
    addLabel: function ($container, id, displayname, path, createLink) {
        var innerHtml;
        if (createLink)
            innerHtml = '<a href="' + path + '">' + displayname + '</a>';
        else
            innerHtml = displayname;

        var labelHtml = '<span class="sn-labelpicker-label" title="' + path + '">' + innerHtml + '<a href="javascript:" onclick="SN.LabelPicker.removeItem($(this));" class="ui-icon-close ui-icon sn-labelpicker-labelclose"></a><input class="sn-labelpicker-label-id" type="hidden" value="' + id + '" /></span>';
        $('.sn-labelpicker-labels', $container).append(labelHtml);
    },
    getLabelIds: function ($container) {
        var ids = [];
        $.each($('.sn-labelpicker-label-id', $container), function () {
            var id = $(this).val();
            ids.push(id);
        });
        return ids;
    },


    // private functions
    inputDefaultText: 'Start typing...',
    createkeydown: function ($container, $inputbox) {
        return function (event) {
            var enter = event.which == 13;
            var backsp = event.which == 8;
            var space = event.which == 32;
            if (enter || space) {
                // check if nothing is selected
                var au = $('.ui-autocomplete');
                var selected = $('#ui-active-menuitem', au);

                // nothing selected : enter selects first
                if (selected.length == 0 && enter) {
                    // trigger click event
                    var link = $('a', $('.ui-menu-item:first', au));
                    link.trigger('mouseenter').click();
                }

                // something selected : enter, tab and space selects selected element
                if (selected.length != 0) {
                    selected.trigger('mouseenter').click();
                }
            }
            if (backsp) {
                // remove last label if input text is empty
                if ($inputbox.val() == '') {
                    $('.sn-labelpicker-label:last', $container).remove();
                }
            }
        }
    },
    createkeyup: function ($container, $inputbox) {
        return function (event) {
            // entered space should be erased
            if ($inputbox.val() == ' ')
                $inputbox.val('');
        }
    },
    defineAutocomplete: function (options, $container, $inputbox) {
        $inputbox.autocomplete({
            source: function (request, response) {
                if (request.term.length > 0 && request.term.substring(0, 1) == '/') {

                    var term = request.term;

                    if (term == '/') {
                        // if there is no search root, '/' means '/root', else it is the searchroot
                        if (!options.searchRoot)
                            term = '/Root';
                        else
                            term = options.searchRoot;
                    }

                    // remove trailing '/'
                    if (term.substring(term.length - 1) == '/')
                        term = term.substring(0, request.term.length - 1);

                    $.ajax({
                        url: '/ContentStore.mvc/GetChildren',
                        data: {
                            parentPath: term,
                            rnd: Math.random()
                        },
                        success: function (data) {
                            response($.map(data, function (item) {
                                return {
                                    label: item.Name,
                                    value: item.Path,
                                    Id: item.Id,
                                    Path: item.Path,
                                    DisplayName: item.DisplayName,
                                    IsPathSelect: true,
                                    Name: item.Name,
                                    ContentTypeName: item.ContentTypeName
                                }
                            }));
                        }
                    });
                } else {
                    if (request.term.length >= options.minLength) {
                        $.ajax({
                            url: '/ContentStore.mvc/Search',
                            data: {
                                searchStr: request.term,
                                searchRoot: options.searchRoot ? options.searchRoot : '/Root',
                                contentTypes: options.contentTypes,
                                rnd: Math.random()
                            },
                            success: function (data) {
                                response($.map(data, function (item) {
                                    return {
                                        label: item.DisplayName + ' (' + item.Name + ')',
                                        value: item.DisplayName,
                                        Id: item.Id,
                                        Path: item.Path,
                                        DisplayName: item.DisplayName
                                    }
                                }));
                            }
                        });
                    }
                }
            },
            minLength: 0, // options.minLength ? options.minLength : 2,
            select: function (event, ui) {
                if (!ui.item)
                    return;

                if (ui.item.IsPathSelect && options.contentTypes.indexOf(ui.item.ContentTypeName) == -1) {
                    // if we are selecting a path, we need to put the cursor at the end of the textbox, but otherwise do nothing
                    $('.sn-labelpicker-inputbox', $container).blur();
                    $('.sn-labelpicker-inputbox', $container).focus();
                    return;
                }

                if (options.addLabel)
                    options.addLabel($container, ui.item.Id, ui.item.DisplayName, ui.item.Path, options.createLink);
                else
                    SN.LabelPicker.addLabel($container, ui.item.Id, ui.item.DisplayName, ui.item.Path, options.createLink);

                ui.item.label = '';
                ui.item.value = '';
            }
        });
        $inputbox.autocomplete("widget").addClass('sn-labelpicker-autocomplete');
    },
    removeItem: function ($this) {
        $this.closest('.sn-labelpicker-label').remove();
    },
    containerClicked: function () {
        $('input', $(this)).focus();
    },
    onfocusInputBox: function ($box) {
        if ($box.val() == SN.LabelPicker.inputDefaultText) {
            $box.val('');
            $box.removeClass('sn-labelpicker-unfocused');
        }
    },
    onblurInputBox: function ($box) {
        if ($box.val().length == 0) {
            $box.val(SN.LabelPicker.inputDefaultText);
            $box.addClass('sn-labelpicker-unfocused');
        }
    }
}