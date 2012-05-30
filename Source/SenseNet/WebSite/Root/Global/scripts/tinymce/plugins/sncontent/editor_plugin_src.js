var SnContentHelper = {
    getGeneratedHtml: function(contentPath, displayPath, selected_node) {
        // get html response of portlet
        var response = $.ajax({
            url: '/portlet-preview.aspx?portlettype=contentcollectionportlet&customrootpath=' + contentPath + '&renderer=' + displayPath,
            dataType: "html",
            async: false
        });

        // get original alignment and size
        var originalAlignStr = '';
        var originalHeightStr = '';
        var originalWidthStr = '';
        var originalStyleStr = '';
        if (selected_node && $(selected_node).attr("sncontentintext")) {
            originalAlignStr = 'align="' + $(selected_node).attr("align") + '"';
            originalHeightStr = 'height="' + $(selected_node).attr("height") + '"';
            originalWidthStr = 'width="' + $(selected_node).attr("width") + '"';
            originalStyleStr = 'style="' + $(selected_node).attr("style") + '"';
        }

        var markerStartString = '<!-- rendered content start -->';
        var markerEndString = '<!-- rendered content end -->';
        var markerStartIndex = response.responseText.indexOf(markerStartString);
        var markerEndIndex = response.responseText.indexOf(markerEndString);
        var markerStartLength = markerStartString.length;
        var portletOutput = response.responseText.substring(markerStartIndex + markerStartLength, markerEndIndex);
        var nonEditableClass = tinyMCE.activeEditor.getParam("noneditable_noneditable_class", "mceNonEditable");
        var generatedHtml = '<table sncontentintext="' + contentPath + ';' + displayPath + '" border="0" ' + originalAlignStr + originalHeightStr + originalWidthStr + originalStyleStr + '><tbody><tr><td><div class="' + nonEditableClass + '">' + portletOutput + '</div></td></tr></tbody></table>';
        return generatedHtml;
    }
};

(function() {
    function importScript(url) {
        var tag = document.createElement("script");
        tag.type = "text/javascript";
        tag.src = url;
        document.body.appendChild(tag);
    }


    tinymce.create('tinymce.plugins.SnContentPlugin', {
        init: function(ed, url) {

            function open() {
                tinyMCE.activeEditor.windowManager.open({
                    file: url + '/sncontent.htm',
                    width: 540,
                    height: 140,
                    inline: 1,
                    resizable: 0
                }, {
            });
        };

        // Register commands
        ed.addCommand('mceSnContent', function() {
            open();
        });

        // Register buttons
        ed.addButton('sncontent', {
            title: 'Insert content from Content Repository',
            cmd: 'mceSnContent',
            image: ed.baseURI.toAbsolute('') + 'plugins/sncontent/img/sncontent.png'
        });

        // Add a node change handler, selects the button in the UI when a image is selected
        ed.onNodeChange.add(function(ed, cm, n) {
            var se = ed.selection;
            var closestContent = $(se.getNode()).closest("table[sncontentintext]");
            if (closestContent.length != 0) {
                cm.setActive('sncontent', true);
            } else {
                cm.setActive('sncontent', false);
            }
        });

        ed.onInit.add(function(ed) {
            // refresh snapshots!
            $.each($("table[sncontentintext]", ed.dom.getRoot()), function() {
                var selected_node = $(this).get();
                var contentAttr = $(this).attr("sncontentintext");

                var props = contentAttr.split(';');
                var contentPath = props[0];
                var displayPath = props[1];

                var generatedHtml = SnContentHelper.getGeneratedHtml(contentPath, displayPath, selected_node);

                $(this).replaceWith(generatedHtml);
            });
        });

    },

    getInfo: function() {
        return {
            longname: 'Sense/Net 6.0 Content picker plugin for tinymce',
            author: 'sn',
            authorurl: 'http://www.sensenet.com',
            infourl: 'http://www.sensenet.com',
            version: '0.2'
        };
    }
});


// Register plugin
tinymce.PluginManager.add('sncontent', tinymce.plugins.SnContentPlugin);
})();
