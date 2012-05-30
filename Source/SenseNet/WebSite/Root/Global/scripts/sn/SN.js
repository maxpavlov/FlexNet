SN = {
    version: "Sense/Net 6.0 CMS",
    ns: function() {
        var o;
        if (arguments.length > 0) {
            for (var i = 0; i < arguments.length; i++) {
                var n = arguments[i];   // SN.GUI.COMPONENTS
                var db = n.split('.');  // [0]SN [1]GUI [2]COMPONENTS
                o = window[db[0]] = window[db[0]] || {};
                for (var j = 0; j < db.length; j++) {
                    var db2 = db[j];
                    o = o[db2] = o[db2] || {};
                };
            }
        }
    } 
}