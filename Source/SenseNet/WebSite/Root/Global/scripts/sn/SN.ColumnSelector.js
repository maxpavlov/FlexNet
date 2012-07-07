// using $skin/scripts/sn/SN.js
// using $skin/scripts/jquery/jquery.js

SN.ns('SN.ColumnSelector');

SN.ColumnSelector = {
    rearrangeSelects: function(selectElement) {
        var s = $('select[id*=ddIndex]');
        var maxSelects = s.length;
        var selectedId = selectElement.id;
        var newIndex = selectElement.selectedIndex + 1;
        var oldIndex;
        var positions = new Array(maxSelects);
        var i;

        for (i = 0; i < maxSelects; i++) {
            positions[i] = 0;
        }

        for (i = 0; i < maxSelects; i++) {
            positions[s[i].selectedIndex] = 1;
        }

        for (i = 0; i < maxSelects; i++) {
            if (positions[i] != 0)
                continue;

            oldIndex = i + 1;
            break;
        }

        var incValue = newIndex > oldIndex ? -1 : 1
        var minIndex = Math.min(newIndex, oldIndex);
        var maxIndex = Math.max(newIndex, oldIndex);

        for (i = 0; i < maxSelects; i++) {
            var currentEl = s[i];

            if (currentEl.id == selectedId)
                continue;

            if (currentEl.selectedIndex + 1 >= minIndex && currentEl.selectedIndex + 1 <= maxIndex)
                currentEl.selectedIndex += incValue;
        }
    }
}
