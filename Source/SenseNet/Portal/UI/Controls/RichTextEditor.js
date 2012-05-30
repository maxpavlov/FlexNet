Type.registerNamespace('SenseNet.Portal.UI.Controls.RichTextEditor');

SenseNet.Portal.UI.Controls.RichTextEditor = function(element) {
    SenseNet.Portal.UI.Controls.RichTextEditor.initializeBase(this, [element]);
    // internals	
    this._config = null;
    this.externalToolbarId = null;
    this._draggableToolbar = null;
}


SenseNet.Portal.UI.Controls.RichTextEditor.prototype = {

  _onFormSubmitHandler: function() {
    try {
      tinyMCE.triggerSave(true, false);
    } catch (e) {
      // TODO : repeated action cause unexpected behaviour in ff and ie
    }
    return true; // go on
  },

  initialize: function() {
    Sys.WebForms.PageRequestManager.getInstance()._onsubmit = this._onFormSubmitHandler;
    SenseNet.Portal.UI.Controls.RichTextEditor.callBaseMethod(this, 'initialize');

    this.externalToolbarId = 'snExternalToolbar';
    this._config = {};

    if (this.options !== null) {
      // we r happy
      this._config = eval('(' + this.options + ')'); // this._config = eval('({' + this.options + '})');
    } else {
      // backward compatibility
      var t = this.theme;
      if (t !== null) {
        this._config.theme = "advanced";
        this._config.plugins = this.plugins;
        this._config.theme_advanced_buttons1 = this.theme_advanced_buttons1;
        this._config.theme_advanced_buttons2 = this.theme_advanced_buttons2;
        this._config.theme_advanced_buttons3 = this.theme_advanced_buttons3;
        this._config.theme_advanced_buttons4 = this.theme_advanced_buttons4;
        this._config.theme_advanced_toolbar_location = this.theme_advanced_toolbar_location; // external
        this._config.theme_advanced_toolbar_align = this.theme_advanced_toolbar_align; // left
        this._config.theme_advanced_statusbar_location = this.theme_advanced_statusbar_location; // bottom
        this._config.theme_advanced_path_location = this.theme_advanced_path_location; //"bottom";
        this._config.theme_advanced_resizing = this.theme_advanced_resizing; // "false";
        this._config.theme_advanced_resize_horizontal = this.theme_advanced_resize_horizontal; //"true";
        this._config.auto_resize = this.auto_resize; // default false
        this._config.table_styles = this.table_styles;
        this._config.table_cell_styles = this.table_cell_styles;
        this._config.table_row_styles = this.table_row_styles;
        this._config.theme_advanced_styles = this.theme_advanced_styles;
        this._config.content_css = this.content_css;
        this._config.font_size_style_values = this.font_size_style_values;
      }
    }
    // TODO: handle setting 
    this._config.inline = true;
    this._config.relative_urls = false;
    this._config.add_form_submit_trigger = false;
    this._config.submit_patch = false;

    this._config.mode = "exact";
    this._config.setup = this._editorSetup;
    this._config.elements = '' + this.get_element().id;

    if (typeof (tinyMCE_GZ) !== 'undefined') {
      //createTinyMCE_GZ();
      tinyMCE_GZ.init(this._config);
    }
    tinyMCE.init(this._config);
    //	disabled --> $addHandlers(this.get_element(), {'save': this._onSave}, this);	

  },

  _editorSetup: function(ed) {
    var component = $find(ed.id);
    if (component) ed.onPostRender.add(component._onEditorPostRender);
  },

  _createExternalToobarPlaceholder: function(externalToolbarId) {

    var externalWrapper = $get(externalToolbarId);
    var createToolbarPanel = function() {

      externalWrapper = document.createElement('div');
      externalWrapper.id = externalToolbarId;

      // set skin classes for toolbar div
      var cssclass = "mceEditor " + tinyMCE.activeEditor.getParam('skin') + 'Skin';
      if (v = tinyMCE.activeEditor.getParam('skin_variant'))
        cssclass += ' ' + v.substring(0, 1).toUpperCase() + v.substring(1);
      externalWrapper.className = cssclass;

      document.forms[0].appendChild(externalWrapper);
      return externalWrapper;
    };

    // if custom wrapper toolbar element does not exists, create one.
    if (typeof (externalWrapper) === 'object') {
      return createToolbarPanel();
    }
    return externalWrapper;
  },


  _onEditorPostRender: function(editor, cm) {
    var cmp = $find(editor.id);
    var t = $get(editor.id + '_external');
    var w = editor.getWin();

    if (t == null) // external toolbar is disabled.
      return;

    tinymce.dom.Event.add(editor.getWin(), 'focus', function(event) {
    $(".snTinyMCEActiveEditor").removeClass("snTinyMCEActiveEditor");
    $('#' + tinyMCE.activeEditor.editorId + '_parent').addClass("snTinyMCEActiveEditor");
    });

    tinymce.dom.Event.add(document.body, 'mousedown', function(event) {
    $('#' + tinyMCE.activeEditor.editorId + '_parent').removeClass("snTinyMCEActiveEditor");
    });

    tinymce.dom.Event.add(t.dom, 'mousedown', function(event) {
    $(".snTinyMCEActiveEditor").removeClass("snTinyMCEActiveEditor");
    $('#' + tinyMCE.activeEditor.editorId + '_parent').addClass("snTinyMCEActiveEditor");
      if (event.preventDefault) {
        event.preventDefault();
        event.stopPropagation();
      } else {
        event.returnValue = false;
        event.cancelBubble = true;
      }
    });

    //
    //  internal function for creating toolbar.
    //
    var buildToolbarPanel = function(component) {

      //  #1 create external toolbar placeholder
      var isPlaceHolderCreated = false;
      var externalToolbarId = component.externalToolbarId;
      var toolbarPanel = $get(externalToolbarId);
      if (toolbarPanel === null) {
        isPlaceHolderCreated = true;
        toolbarPanel = component._createExternalToobarPlaceholder(externalToolbarId);
      }

      //  #2 append the external toolbar instance of rte to ToolbarPlaceHolder
      var editorToolbar = $get(editor.id + '_external');
      toolbarPanel.appendChild(editorToolbar);

      //  #3 display it? need it, really?
      //component._showDragPanel(editor);

      //  #4 get bounds coordinates
      var dragToolbarBounds = Sys.UI.DomElement.getBounds(toolbarPanel);

      // Create toolbar height offset for BODY

      if (isPlaceHolderCreated) {
        setTimeout(function() {
          var toolbarHeight = Number($('#' + externalToolbarId).outerHeight()) + Number($("body").css("marginTop").slice(0, -2));
          $("body").css("marginTop", toolbarHeight + "px");
        }, 0);
      }
    };

    buildToolbarPanel(cmp);
  },

  _showDragPanel: function(ed) {

    var p = $get('snExternalToolbar');
    p.style.display = "block";
  },

  //    _hideDragPanel: function(ed) {
  //        var p = $get('snExternalToolbar');
  //        p.style.display = "none";
  //    },

  _save: function(element_id, html, body) {
    return html;
  },

  dispose: function() {
    this._destoryEditorControl();
    SenseNet.Portal.UI.Controls.RichTextEditor.callBaseMethod(this, 'dispose');
  },
  //
  //  Removes the tinymce instance
  //
  _destoryEditorControl: function() {
    var ed = document.getElementById(this.get_element().id);
    if (tinyMCE.getInstanceById(ed) !== null) {

      // pre remove for custom toolbar
      var editorToolbar = $get(ed.id + '_external');
      if (editorToolbar !== null) {
        editorToolbar.parentNode.removeChild(editorToolbar);
      }

      //tinyMCE.execCommand('mceFocus', false, ed.id);
      tinyMCE.activeEditor = {};
      tinyMCE.selectedInstance = {};

      try {
        tinyMCE.execCommand('mceRemoveControl', true, ed.id);
      } catch (exc) {
        // IE7 'SOMETIMES' throws a 'this.getDoc().body is undefined' message.
        // it seems, swallow this exception doesn't break the normal workflow.
      }

    }
  },

  // -------------------------------------------------------------------- Events
  add_save: function(handler) {
    this.get_events().addHandler('save', handler);
  },
  remove_save: function(handler) {
    this.get_events().removeHandler('save', handler);
  },
  _onSave: function(e) {
    // empty handler    
  }

}

SenseNet.Portal.UI.Controls.RichTextEditor.registerClass('SenseNet.Portal.UI.Controls.RichTextEditor',Sys.UI.Control);
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();