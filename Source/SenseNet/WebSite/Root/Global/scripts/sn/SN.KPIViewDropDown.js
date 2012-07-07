// using $skin/scripts/sn/SN.js
// using $skin/scripts/jquery/jquery.js

SN.KPIViewDropDown = {
    views: null,
    sourcedd: null,
    targetdd: null,
    targettb: null,
    setDropDownOptions: function(options) {
        SN.KPIViewDropDown.targetdd.children().remove().end().append(options);
    },
    populateDropDown: function(sourceName) {
        var options = '<option value="">Select a view...</option>';
        $.each(SN.KPIViewDropDown.views, function(i, item) {
            if (item.sourceName == sourceName)
                options += '<option value="' + item.viewName + '">' + item.viewName + '</option>';
        });
        if (sourceName == '') {
            options = '<option value="">Select a KPI datasource first</option>';
        }
        SN.KPIViewDropDown.setDropDownOptions(options);
    },
    init: function(sourcedropdowncss, targetdropdowncss, initialviews) {
        SN.KPIViewDropDown.views = initialviews;

        SN.KPIViewDropDown.sourcedd = $('.' + sourcedropdowncss + ' select');
        SN.KPIViewDropDown.targetdd = $('.' + targetdropdowncss + ' select');
        SN.KPIViewDropDown.targettb = $('.' + targetdropdowncss + ' input');

        var sourceName = SN.KPIViewDropDown.sourcedd.val();
        if (sourceName == '')
            SN.KPIViewDropDown.setDropDownOptions('<option value="">Select a KPI datasource first</option>');
        else {
            // populate dropdown initially
            SN.KPIViewDropDown.populateDropDown(sourceName);

            // set initial value
            var viewName = SN.KPIViewDropDown.targettb.val();
            SN.KPIViewDropDown.targetdd.val(viewName);
        }

        if (SN.KPIViewDropDown.sourcedd) {
            SN.KPIViewDropDown.sourcedd.live('change', function() {
                var sourceName = SN.KPIViewDropDown.sourcedd.val();
                SN.KPIViewDropDown.populateDropDown(sourceName);
                SN.KPIViewDropDown.targettb.val(SN.KPIViewDropDown.targetdd.val());
            });
        }
        if (SN.KPIViewDropDown.targetdd) {
            SN.KPIViewDropDown.targetdd.live('change', function() {
                SN.KPIViewDropDown.targettb.val(SN.KPIViewDropDown.targetdd.val());
            });
        }
    }
}
