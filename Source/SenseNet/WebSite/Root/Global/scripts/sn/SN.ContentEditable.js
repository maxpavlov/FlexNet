// using $skin/scripts/sn/SN.js
// using $skin/scripts/jquery/jquery.js

SN.ns('SN.ContentEditable');

SN.ContentEditable = {
    setupContentEditableFields: function(ctrlClass) {
        $('[contenteditable=true]').each(function() {
            var $this = $(this);

            $this.keypress(function(event) {
                // event is cancelled if enter is pressed
                return event.which != 13;
            });

            $this.bind('blur keyup', ctrlClass, function() {
                var newValue = $(this).html().trim();

                // save the changes into the control
                var ctrl = $(this).next("." + ctrlClass);
                //alert("ctrl:" + ctrl + " / new content: " + newValue);
                ctrl.val(newValue);

                return true;
            });

        });
    }
}