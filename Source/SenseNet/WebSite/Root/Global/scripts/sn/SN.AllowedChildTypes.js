// using $skin/scripts/sn/SN.js
// using $skin/scripts/jquery/jquery.js

SN.ACT = {
	createOtherDefaultText: 'Start typing...',
	onfocusPostBox: function ($box) {
		if ($box.val() == SN.ACT.createOtherDefaultText) {
			$box.val('');
			$box.removeClass('sn-unfocused-postbox');
		}
	},
	onblurPostBox: function ($box) {
		if ($box.val().length == 0) {
			$box.val(SN.ACT.createOtherDefaultText);
			$box.addClass('sn-unfocused-postbox');
		}
	},
	showAll: function () {
		var input = $(".sn-dropbox-createother");
		// close if already visible
		if (input.autocomplete("widget").is(":visible")) {
			input.autocomplete("close");
			return;
		}

		// work around a bug (likely same cause as #5265)
		$(this).blur();

		// pass empty string as value to search for, displaying all results
		input.autocomplete("search", "");
		input.focus();
	},
	onchange: function (event, $this) {
		if (event.keyCode == 13) {
			if ($('.sn-dropbox-createother-submit').is(':visible'))
				SN.ACT.addType();
		} else {
			var source = $this.autocomplete("option", 'source');
			var label = $this.val();
			var found = false;
			for (var i = 0; i < source.length; i++) {
				if (source[i].label == label) {
					$(".sn-allowedct-createother-value").val(source[i].value);
					var $path = $(".sn-allowedct-createother-path");
					$path.val(source[i].path);
					var $icon = $(".sn-allowedct-createother-icon");
					$icon.val(source[i].icon);
					found = true;
					break;
				}
			}
			SN.ACT.enableAddButton(found);
		}
	},
	enableAddButton: function (enable) {
		if (enable) {
			$('.sn-dropbox-createother-submit').show();
			$('.sn-dropbox-createother-submitfake').hide();
		} else {
			$('.sn-dropbox-createother-submit').hide();
			$('.sn-dropbox-createother-submitfake').show();
		}
	},
	initTypes: function (data) {
		$('.sn-dropbox-createother-submit').hide();

		$('.sn-dropbox-createother').autocomplete({
			source: data,
			minLength: 0,
			appendTo: '.sn-dropbox-createother-autocomplete',
			focus: function (event, ui) {
				$('.sn-dropbox-createother').val(ui.item.label);
				return false;
			},
			select: function (event, ui) {
				$(".sn-dropbox-createother").val(ui.item.label);
				$(".sn-allowedct-createother-value").val(ui.item.value);
				var $path = $(".sn-allowedct-createother-path");
				$path.val(ui.item.path);
				var $icon = $(".sn-allowedct-createother-icon");
				$icon.val(ui.item.icon);
				SN.ACT.enableAddButton(true);
				return false;
			},
			change: function (event, ui) {
			}
		});
	},
	addType: function () {
		var label = $('.sn-dropbox-createother').val();
		var name = $('.sn-allowedct-createother-value').val();
		var path = $('.sn-allowedct-createother-path').val();
		var icon = $('.sn-allowedct-createother-icon').val();
		$innerData = $('.sn-allowedct-innerdata');
		var names = $innerData.val();
		if ((' ' + names + ' ').indexOf(' ' + name + ' ') != -1)
			return;
		$innerData.val(names + ' ' + name);
		var markup = SN.ACT.getMarkup(path, label, name, icon);
		$('#sn-allowedct-container').append(markup);
		$('#sn-allowedct-inherit').show();
    },
	getMarkup: function (path, label, name, icon) {
		var removelink = '<div style="float:left; width:20px;"><a class="sn-icon-small sn-icon-button snIconSmall_Delete" href="javascript:" onclick="SN.ACT.removeType($(this));"></a></div>';
		var typelink = '<div style="float:left; width: 250px;"><img style="margin-right:5px;" src="/Root/Global/images/icons/16/' + icon + '.png" class="sn-icon-small sn-icon-button" /><a href="' + path + '?action=Explore">' + label + '</a><input class="sn-allowedct-itemname" type="hidden" value="' + name + '" /></div>';
		var markup = '<div class="sn-allowedct-item" style="width:300px;height:22px;">' + typelink + removelink + '<div style="clear:both;"></div></div>';
		return markup;
	},
	init: function (data, inherit) {
		if (inherit)
			$('#sn-allowedct-inherit').show();
		else
			$('#sn-allowedct-inherit').hide();

		SN.ACT.initTypes(data);
		$innerData = $('.sn-allowedct-innerdata');
		var names = $innerData.val().split(' ');
		for (var i = 0; i < names.length; i++) {
			var name = names[i];
			for (var j = 0; j < data.length; j++) {
				var curr = data[j];
				if (curr.value == name) {
					var markup = SN.ACT.getMarkup(curr.path, curr.label, name, curr.icon);
					$('#sn-allowedct-container').append(markup);
					break;
				}
			}
		}
	},
	removeType: function ($this) {
		var $item = $this.closest('.sn-allowedct-item');
		var name = $('.sn-allowedct-itemname', $item).val();
		$item.remove();
		$innerData = $('.sn-allowedct-innerdata');
		var names = (' ' + $innerData.val() + ' ').replace(' ' + name + ' ', ' ');
		// trim
		while (names.substring(0, 1) == ' ') {
			names = names.substring(1, names.length);
		}
		while (names.substring(names.length - 1, names.length) == ' ') {
			names = names.substring(0, names.length - 1);
		}
		$innerData.val(names);
		$('#sn-allowedct-inherit').show();
	}
}