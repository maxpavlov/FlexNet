/// <depends path="$skin/scripts/sn/SN.js" />
/// <depends path="$skin/scripts/jquery/jquery.js" />

SN.ContentName = {
    invalidChars: '',
    placeHolderSymbol: '',
    InitUrlNameControl: function (textboxId, extensionTextId, labelId, editbuttonId, cancelbuttonId, displayNameAvailableControlId, editable, isNewContent) {
        var $textbox = $('#' + textboxId);
        var $label = $('#' + labelId);
        var $editbutton = $('#' + editbuttonId);
        var $cancelbutton = $('#' + cancelbuttonId);
        var $extensionText = extensionTextId == '' ? null : $('#' + extensionTextId);
        var $displayNameAvailableControl = $('#' + displayNameAvailableControlId);

        var $container = SN.ContentName.GetCommonContainer($textbox);
        var $titleBox = $('.sn-urlname-name', $container);

        // set state of displayNameAvailableControl indicating the presence of displayname field
        if ($displayNameAvailableControl.length > 0) {
            $displayNameAvailableControl.val($titleBox.length);
        }

        // new content: editable if no displayname field present
        // old content: editable if 'editable' is true
        if (($titleBox.length == 0 && isNewContent == 'true') || (editable == 'true')) {
            // this control is automatically editable
            if ($textbox.length > 0 && $label.length > 0 && $editbutton.length > 0 && $cancelbutton.length > 0) {
                $textbox.show();
                if ($extensionText && $extensionText.length > 0)
                    $extensionText.show();
                $label.hide();
                $editbutton.hide();
                $cancelbutton.hide();
            }
        }
    },
    InitNameControl: function (nameAvailableControlId, invalidChars, placeHolderSymbol) {
        SN.ContentName.invalidChars = invalidChars;
        SN.ContentName.placeHolderSymbol = placeHolderSymbol;

        var $nameAvailableControl = $('#' + nameAvailableControlId);

        var $container = SN.ContentName.GetCommonContainer($nameAvailableControl);
        var $titleBox = $('.sn-urlname-control', $container);

        // set state of nameAvailableControl indicating the presence of name field
        if ($nameAvailableControl.length > 0) {
            $nameAvailableControl.val($titleBox.length);
        }
    },
    EditUrlName: function (textboxId, extensionTextId, labelId, editbuttonId, cancelbuttonId) {
        var $textbox = $('#' + textboxId);
        var $label = $('#' + labelId);
        var $editbutton = $('#' + editbuttonId);
        var $cancelbutton = $('#' + cancelbuttonId);
        var $extensionText = extensionTextId == '' ? null : $('#' + extensionTextId);
        if ($textbox.length > 0 && $label.length > 0 && $editbutton.length > 0 && $cancelbutton.length > 0) {
            $textbox.show();
            if ($extensionText && $extensionText.length > 0)
                $extensionText.show();
            $label.hide();
            $editbutton.hide();
            $cancelbutton.show();
        }
    },
    CancelEditingUrlName: function (textboxId, extensionTextId, labelId, editbuttonId, cancelbuttonId) {
        var $textbox = $('#' + textboxId);
        var $label = $('#' + labelId);
        var $editbutton = $('#' + editbuttonId);
        var $cancelbutton = $('#' + cancelbuttonId);
        var $extensionText = extensionTextId == '' ? null : $('#' + extensionTextId);
        if ($textbox.length > 0 && $label.length > 0 && $editbutton.length > 0 && $cancelbutton.length > 0) {
            var urlname = $label.text();

            // extensiontext is present $textbox should only contain filename
            if ($extensionText && $extensionText.length > 0) {
                var fileNameAndExtension = SN.ContentName.GetFileAndExtension(urlname);
                $textbox.val(fileNameAndExtension.fileName);
                $extensionText.val(fileNameAndExtension.extension);
                $label.text(urlname);
            } else {
                $textbox.val(urlname);
                $label.text(urlname);
            }
            $textbox.hide();
            if ($extensionText && $extensionText.length > 0)
                $extensionText.hide();
            $label.show();
            $editbutton.show();
            $cancelbutton.hide();
        }
    },
    GetCommonContainer: function ($control) {
        return $control.closest('.sn-portlet');
    },
    GetTitleForUrlTextbox: function ($textbox) {
        var $container = SN.ContentName.GetCommonContainer($textbox);
        var $titleBox = $('.sn-urlname-name', $container);
        if ($titleBox.length > 0)
            return $titleBox.val();
        return "";
    },
    TextEnter: function (textboxId, originalName) {
        var $titleBox = $('#' + textboxId);
        if ($titleBox) {
            var $container = SN.ContentName.GetCommonContainer($titleBox);
            var $nameBox = $('.sn-urlname-control', $container);
            var $nameLabel = $('.sn-urlname-label', $container);
            var $nameExtensionLabel = $('.sn-urlname-extensionlabel', $container);
            if ($container.length > 0 && $nameBox.length > 0 && $nameLabel.length > 0 && $nameExtensionLabel.length > 0) {
                var title = $titleBox.val();

                // check if title ends with extension
                var extension = $nameExtensionLabel.val();
                var nameLength = title.length - extension.length;
                if (nameLength >= 0 && title.indexOf(extension, nameLength) == nameLength && extension.length > 0) {
                    title = title.substring(0, nameLength - 1);
                }

                var validName = SN.ContentName.RemoveInvalidCharacters(title);
                if (validName.length == 0)
                    validName = originalName;

                // name may not end neither with '.' nor with '-'
                while (validName.charAt(validName.length - 1) == '.' || validName.charAt(validName.length - 1) == SN.ContentName.placeHolderSymbol)
                    validName = validName.substring(0, validName.length - 1);

                // add extension
                fullName = validName;
                var ext = $nameExtensionLabel.val();
                if (ext.length > 0)
                    fullName = validName + '.' + ext;


                $nameLabel.text(fullName);
                if ($nameBox.is(':visible'))
                    return;
                $nameBox.val(validName);
            }
        }
    },
    RemoveInvalidCharacters: function (s) {
        // NOTE: changing this code requires a change in RemoveInvalidCharacters, GetNoAccents functions in ContentNamingHelper.cs, in order that these work in accordance
        var noaccents = SN.ContentName.GetNoAccents(s);
        noaccents = noaccents.replace(new RegExp(SN.ContentName.invalidChars, 'g'), SN.ContentName.placeHolderSymbol);
        // space is also removed
        noaccents = noaccents.replace(new RegExp(' ', 'g'), SN.ContentName.placeHolderSymbol);
        // '-' should not be followed by another '-'
        noaccents = noaccents.replace(new RegExp(SN.ContentName.placeHolderSymbol + "{2,}", 'g'), SN.ContentName.placeHolderSymbol);
        // trim placeholder from start and end
        noaccents = noaccents.replace(new RegExp("^[" + SN.ContentName.placeHolderSymbol + "]+", "g"), "");
        noaccents = noaccents.replace(new RegExp("[" + SN.ContentName.placeHolderSymbol + "]+$", "g"), "");
        return noaccents;
    },
    GetNoAccents: function (r) {
        // NOTE: changing this code requires a change in RemoveInvalidCharacters, GetNoAccents functions in ContentNamingHelper.cs, in order that these work in accordance
        r = r.replace(new RegExp("[àáâãäå]", 'g'), "a");
        r = r.replace(new RegExp("[ÀÁÂÃÄÅ]", 'g'), "A");
        r = r.replace(new RegExp("æ", 'g'), "ae");
        r = r.replace(new RegExp("Æ", 'g'), "AE");
        r = r.replace(new RegExp("ç", 'g'), "c");
        r = r.replace(new RegExp("Ç", 'g'), "C");
        r = r.replace(new RegExp("[èéêë]", 'g'), "e");
        r = r.replace(new RegExp("[ÈÉÊË]", 'g'), "E");
        r = r.replace(new RegExp("[ìíîï]", 'g'), "i");
        r = r.replace(new RegExp("[ÌÍÎÏ]", 'g'), "I");
        r = r.replace(new RegExp("ñ", 'g'), "n");
        r = r.replace(new RegExp("Ñ", 'g'), "N");
        r = r.replace(new RegExp("[òóôõöőø]", 'g'), "o");
        r = r.replace(new RegExp("[ÒÓÔÕÖŐØ]", 'g'), "O");
        r = r.replace(new RegExp("œ", 'g'), "oe");
        r = r.replace(new RegExp("Œ", 'g'), "OE");
        r = r.replace(new RegExp("ð", 'g'), "d");
        r = r.replace(new RegExp("Ð", 'g'), "D");
        r = r.replace(new RegExp("ß", 'g'), "s");
        r = r.replace(new RegExp("[ùúûüű]", 'g'), "u");
        r = r.replace(new RegExp("[ÙÚÛÜŰ]", 'g'), "U");
        r = r.replace(new RegExp("[ýÿ]", 'g'), "y");
        r = r.replace(new RegExp("[ÝŸ]", 'g'), "Y");
        return r;
    },
    GetFileAndExtension: function (fullName) {
        var extension = '';
        var index = fullName.lastIndexOf('.');
        if (index != -1 && fullName.length > index + 1)
            extension = fullName.substring(index + 1);
        var filename = fullName.substring(0, fullName.length - extension.length - 1);
        return { fileName: filename, extension: extension };
    }
}

