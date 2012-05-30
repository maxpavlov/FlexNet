/// <depends path="$skin/scripts/sn/SN.js" />
/// <depends path="$skin/scripts/jquery/jquery.js" />
/// <depends path="$skin/scripts/codemirror/js/codemirror.js" />

SN.BinaryFieldControl = {
    initHighlightTextbox: function(extension) {
        var parserfile = "parsexml.js";
        var stylesheet = "/Root/Global/scripts/codemirror/css/xmlcolors.css";
        if (extension == ".js") {
            parserfile = ["tokenizejavascript.js", "parsejavascript.js"];
            stylesheet = "/Root/Global/scripts/codemirror/css/jscolors.css";
        }
        if (extension == ".css") {
            parserfile = "parsecss.js";
            stylesheet = "/Root/Global/scripts/codemirror/css/csscolors.css";
        }
        $(".sn-highlighteditor").each(function() {
            var $textfield = $(this);
            var editor = CodeMirror.fromTextArea($textfield.attr("id"), {
                height: "600px",
                parserfile: parserfile,
                stylesheet: stylesheet,
                path: "/Root/Global/scripts/codemirror/js/",
                continuousScanning: 500,
                lineNumbers: true
            });
        });
    },
    initZoomWindow: function() {
        $(".sn-zoomtext").each(function() {
            var $zoombtn = $(this).after("<button class='sn-zoomtext-btn' style='vertical-align:top'>Open in dialog</button>").next();
            $zoombtn.button({
                icons: { primary: 'ui-icon-newwin' },
                text: false
            });

            $zoombtn.click(function() {
                var $zoombtn = $(this);
                var $textfield = $zoombtn.prev();
                var $textfieldDialog = $textfield.after("<div><textarea id='" + $textfield.attr("id") + "_clone' style='width:100%; height:100%; padding:0; margin:0; border:0;'>" + $textfield.val() + "</textarea></div>").next();

                var dlgtitle = $textfield.parent().prev().children(".sn-iu-title").text();
                if (dlgtitle == "") dlgtitle = "Edit text";

                $textfieldDialog.dialog({
                    title: dlgtitle,
                    minWidth: 400,
                    minHeight: 300,
                    width: 700,
                    height: 500,
                    modal: true,
                    close: function() {
                        var newtext = $("textarea", $textfieldDialog).val();
                        $textfield.val(newtext);
                        $textfieldDialog.dialog("destroy");
                    }
                });
                return false; //prevent submit
            });
        });
    }
}
