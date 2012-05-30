/// <depends path="$skin/scripts/sn/SN.js" />
/// <depends path="$skin/scripts/sn/SN.Util.js" />
/// <depends path="$skin/scripts/jquery/jquery.js" />
/// <depends path="$skin/scripts/jqueryui/minified/jquery-ui.min.js" />
/// <depends path="$skin/scripts/sn/SN.Picker.js" />

SN.Wall = {
    postDefaultText: 'Post something...',
    shareDefaultText: 'Write something...',
    commentDefaultText: 'Write something...',
    createOtherDefaultText: 'Start typing...',
    likeListDialogConf: {
        title: 'People who like this item',
        modal: true,
        zIndex: 10000,
        width: 320,
        height: 'auto',
        minHeight: 0,
        maxHeight: 350,
        minWidth: 320,
        resizable: false,
        autoOpen: true,
        close: function () { $(this).dialog("destroy") }
    },
    shareDialogConf: {
        title: 'Post this to a wall',
        modal: true,
        zIndex: 10000,
        width: 400,
        height: 'auto',
        minHeight: 0,
        maxHeight: 350,
        minWidth: 400,
        resizable: false,
        autoOpen: true,
        close: function () { $(this).dialog("destroy") }
    },

    // post
    createPost: function (event, $postbox) {
        if (event.keyCode == 13 && event.ctrlKey) {
            $postbox.val($postbox.val() + '\n');
            $postbox[0].scrollTop = 99999;
            return true;
        }
        if (event.keyCode == 13) {
            var workspacePath = $('.sn-posts-workspace-path').val();
            $.getJSON('/Wall.mvc/CreatePost',
			{
			    contextPath: workspacePath,
			    text: $postbox.val(),
			    rnd: Math.random()
			},
			SN.Wall.addPostMarkup);
            $postbox.val('');
            return true;
        }
        return false;
    },
    addPostMarkup: function (o) {
        $('#sn-posts').prepend(o);
    },
    onfocusPostBox: function ($box, share, createOther) {
        var text = share ? SN.Wall.shareDefaultText : SN.Wall.postDefaultText;
        text = createOther ? SN.Wall.createOtherDefaultText : text;
        if ($box.val() == text) {
            $box.val('');
            $box.removeClass('sn-unfocused-postbox');
        }
    },
    onblurPostBox: function ($box, share, createOther) {
        if ($box.val().length == 0) {
            var text = share ? SN.Wall.shareDefaultText : SN.Wall.postDefaultText;
            text = createOther ? SN.Wall.createOtherDefaultText : text;
            $box.val(text);
            $box.addClass('sn-unfocused-postbox');
        }
    },
    smallPostDetails: function ($this) {
        var $postDiv = $this.closest('.sn-postdiv');
        var $detailsDiv = $('.sn-postdiv-details', $postDiv);
        $detailsDiv.toggle();
    },


    // comment
    createComment: function (event, $this) {
        if (event.keyCode == 13) {
            var $postDiv = $this.closest('.sn-postdiv');
            var postId = $('.sn-postid', $postDiv).val();
            var $comments = $('.sn-comments', $postDiv);
            var $commentBox = $('.sn-commentbox', $postDiv);
            var workspacePath = $('.sn-posts-workspace-path').val();
            $.getJSON('/Wall.mvc/CreateComment',
			{
			    postId: postId,
			    contextPath: workspacePath,
			    text: $commentBox.val(),
			    rnd: Math.random()
			},
			SN.Wall.createCommentCallback($comments));
            $commentBox.val('');
            return true;
        }
        return false;
    },
    createCommentCallback: function ($comments) {
        return function (data) {
            $comments.append(data);
        }
    },
    viewAllComments: function ($this) {
        $('.sn-hiddencomments', $this.closest('.sn-postdiv')).show(); $this.parent().hide();
    },
    comment: function ($this) {
        var $postdiv = $this.closest('.sn-postdiv');
        $('.sn-commentboxdiv', $postdiv).show();
        $('.sn-commentbox', $postdiv).focus();
    },
    onfocusCommentBox: function ($box) {
        if ($box.val() == SN.Wall.commentDefaultText) {
            $box.val('');
            $box.removeClass('sn-unfocused-postbox');
            var $parentdiv = $box.closest('.sn-commentboxdiv');
            $parentdiv.removeClass('sn-unfocused-commentbox');
            $parentdiv.addClass('sn-focused-commentbox');
        }
    },
    onblurCommentBox: function ($box) {
        if ($box.val().length == 0) {
            $box.val(SN.Wall.commentDefaultText);
            $box.addClass('sn-unfocused-postbox');
            var $parentdiv = $box.closest('.sn-commentboxdiv');
            $parentdiv.removeClass('sn-focused-commentbox');
            $parentdiv.addClass('sn-unfocused-commentbox');
        }
    },


    // like
    like: function ($this) {
        var $postDiv = $this.closest('.sn-postdiv');
        var postId = $('.sn-postid', $postDiv).val();
        $('.sn-unlike', $postDiv).show();
        $this.hide();
        var $likes = $('.sn-likelabel', $postDiv);
        var $likebox = $('.sn-likebox', $postDiv);
        var workspacePath = $('.sn-posts-workspace-path').val();
        $.getJSON('/Wall.mvc/Like',
			{
			    itemId: postId,
			    contextPath: workspacePath,
			    fullMarkup: true,
			    rnd: Math.random()
			},
			SN.Wall.createLikeCallback($likes, $likebox));
    },
    unlike: function ($this) {
        var $postDiv = $this.closest('.sn-postdiv');
        var postId = $('.sn-postid', $postDiv).val();
        $('.sn-like', $postDiv).show();
        $this.hide();
        var $likes = $('.sn-likelabel', $postDiv);
        var $likebox = $('.sn-likebox', $postDiv);
        $.getJSON('/Wall.mvc/Unlike',
			{
			    itemId: postId,
			    fullMarkup: true,
			    rnd: Math.random()
			},
			SN.Wall.createLikeCallback($likes, $likebox));
    },
    likecomment: function ($this) {
        var $commentDiv = $this.closest('.sn-commentdiv');
        var commentId = $('.sn-commentid', $commentDiv).val();
        $('.sn-commentunlike', $commentDiv).show();
        $this.hide();
        var $likes = $('.sn-commentlikelabel', $commentDiv);
        var $likebox = $('.sn-commentlikebox', $commentDiv);
        $.getJSON('/Wall.mvc/Like',
			{
			    itemId: commentId,
			    fullMarkup: false,
			    rnd: Math.random()
			},
			SN.Wall.createLikeCallback($likes, $likebox));
    },
    unlikecomment: function ($this) {
        var $commentDiv = $this.closest('.sn-commentdiv');
        var commentId = $('.sn-commentid', $commentDiv).val();
        $('.sn-commentlike', $commentDiv).show();
        $this.hide();
        var $likes = $('.sn-commentlikelabel', $commentDiv);
        var $likebox = $('.sn-commentlikebox', $commentDiv);
        $.getJSON('/Wall.mvc/Unlike',
			{
			    itemId: commentId,
			    fullMarkup: false,
			    rnd: Math.random()
			},
			SN.Wall.createLikeCallback($likes, $likebox));
    },
    createLikeCallback: function ($likes, $likebox) {
        return function (data) {
            $likes.html(data);
            if (data.length == 0)
                $likebox.hide();
            else
                $likebox.show();
        }
    },
    showLikeList: function ($this, itemId) {
        var $likelist = $('.sn-likelist');
        var $likelistitems = $('.sn-likelist-items', $likelist);
        $likelistitems.html('Loading...');
        SN.Util.CreateUIDialog($likelist, SN.Wall.likeListDialogConf);
        $.getJSON('/Wall.mvc/GetLikeList',
			{
			    itemId: itemId,
			    rnd: Math.random()
			},
			SN.Wall.createLikeListCallback($likelistitems));
    },
    createLikeListCallback: function ($likelistitems) {
        return function (data) {
            $likelistitems.html(data);
        }
    },
    closeLikeList: function () {
        var $likelist = $('.sn-likelist');
        $likelist.dialog("destroy");
    },


    // share
    openShareDialog: function (contentId) {
        var dialogId = 'sn-sharecontent-' + contentId;
        $('body').append('<div id="' + dialogId + '" class="sn-sharecontent"></div>');
        var el = $('#' + dialogId);
        $(el).load('/Root/Global/renderers/Wall/Share.aspx?contentid=' + contentId + '&rnd=' + Math.random(), function () {
            SN.Util.CreateUIDialog($(this), SN.Wall.shareDialogConf);
        });
    },
    closeShareDialog: function (contentId) {
        var $sharediv = $('#sn-sharecontent-' + contentId);
        $sharediv.dialog("destroy");
    },
    share: function (contentId) {
        var $sharediv = $('#sn-sharecontent-' + contentId);
        var postId = $('.sn-postid', $sharediv).val();
        var $pathlink = $('.sn-sharetarget-path', $sharediv);
        var path = $pathlink.attr('href');
        if (path == '') {
            alert("Please provide a valid workspace path!");
            return;
        }

        var $sharetext = $('.sn-share-text', $sharediv);
        var text = $sharetext.val();
        if (text == SN.Wall.shareDefaultText)
            text = '';

        $.getJSON('/Wall.mvc/Share',
			{
			    itemId: postId,
			    contextPath: path,
			    text: text,
			    rnd: Math.random()
			},
			SN.Wall.createCloseShareDialog(contentId));
    },
    createCloseShareDialog: function (contentId) {
        return function (data) {
            var $sharediv = $('#sn-sharecontent-' + contentId);
            if (data == '403') {
                $('.sn-sharecontent-403', $sharediv).show();
            } else {
                $('.sn-sharebutton', $sharediv).hide();
                $('.sn-sharecontent-maindiv', $sharediv).hide();
                $('.sn-sharecontent-successful', $sharediv).show();
            }
        }
    },
    shareTargetPick: function (contentId) {
        var $sharediv = $('#sn-sharecontent-' + contentId);
        var $pathlink = $('.sn-sharetarget-path', $sharediv);
        var path = $pathlink.attr('href');
        SN.PickerApplication.open({ MultiSelectMode: 'none', SelectedNodePath: path, TreeRoots: ['/Root/Profiles', '/Root'], callBack: SN.Wall.createShareTargetPickCallback($pathlink) });
    },
    createShareTargetPickCallback: function ($pathlink) {
        return function (resultData) {
            if (!resultData)
                return;
            $pathlink.attr('href', resultData[0].Path);
            $pathlink.text(resultData[0].DisplayName);
        }
    },
    setShareTarget: function (contentId, path, name) {
        var $sharediv = $('#sn-sharecontent-' + contentId);
        var $pathlink = $('.sn-sharetarget-path', $sharediv);
        $pathlink.attr('href', path);
        $pathlink.text(name);
    },


    // dropbox
    createContent: function (type, defaultPath) {
        var workspacePath = $('.sn-posts-workspace-path').val();
        var path = workspacePath + defaultPath;
        SN.PickerApplication.open({ TreeRoots: [workspacePath, '/Root'], MultiSelectMode: 'none', DefaultPath: path, callBack: SN.Wall.createContentCreateCallback(type) });
    },
    createContentCreateCallback: function (type) {
        return function (resultData) {
            if (!resultData)
                return;
            var href = resultData[0].Path + '?action=Add&ContentTypeName=' + encodeURI(type) + '&back=' + encodeURI(location.href);
            location = href;
        }
    },
    initDropBox: function (data) {
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
                $(".sn-dropbox-createother-value").val(ui.item.value);
                SN.Wall.enableCreateOtherButton(true);
                return false;
            },
            change: function (event, ui) {
            }
        });

    },
    onchangeCreateOtherBox: function (event, $this) {
        if (event.keyCode == 13) {
            if ($('.sn-dropbox-createother-submit').is(':visible'))
                SN.Wall.createOther();
        } else {
            var source = $this.autocomplete("option", 'source');
            var label = $this.val();
            var found = false;
            for (var i = 0; i < source.length; i++) {
                if (source[i].label == label) {
                    $(".sn-dropbox-createother-value").val(source[i].value);
                    found = true;
                    break;
                }
            }
            SN.Wall.enableCreateOtherButton(found);
        }
    },
    enableCreateOtherButton: function (enable) {
        if (enable) {
            $('.sn-dropbox-createother-submit').show();
            $('.sn-dropbox-createother-submitfake').hide();
        } else {
            $('.sn-dropbox-createother-submit').hide();
            $('.sn-dropbox-createother-submitfake').show();
        }
    },
    createOther: function () {
        SN.Wall.createContent($(".sn-dropbox-createother-value").val(), '');
    },
    createOtherShowAll: function () {
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
    dropboxSelect: function ($this, $div) {
        $('.sn-dropboxdiv').hide();
        $div.show();
        $('.sn-dropbox-buttons a').removeClass('sn-dropbox-buttons-selected');
        $this.addClass('sn-dropbox-buttons-selected');
    },
    dropboxUpload: function () {
        var workspacePath = $('.sn-posts-workspace-path').val();
        SN.PickerApplication.open({ TreeRoots: [workspacePath, '/Root'], MultiSelectMode: 'none', callBack: SN.Wall.uploadCallback });
    },
    uploadCallback: function (resultData) {
        if (!resultData)
            return;
        var href = resultData[0].Path + '?action=Upload' + '&back=' + encodeURI(location.href);
        location = href;
    }
};