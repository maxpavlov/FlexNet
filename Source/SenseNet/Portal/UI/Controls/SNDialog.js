// Register the namespace for the control.
Type.registerNamespace('SenseNet.Portal.UI.Control.SNDialog');

//
// Define the control properties.
//
SenseNet.Portal.UI.Control.SNDialog = function(element){
    SenseNet.Portal.UI.Control.SNDialog.initializeBase(this, [element]);
    
    this._headerText = null;
    this._parentNodeId = null;
    this._modal = null;
    this._width = null;
    this._height = null;
    this.config = {};
    
}
//
// Create the prototype for the control.
//
SenseNet.Portal.UI.Control.SNDialog.prototype = {

    initialize: function(){
        SenseNet.Portal.UI.Control.SNDialog.callBaseMethod(this, 'initialize');
        
        var containerDialogElement = this.get_element();
        this._parentNodeId = this._element.parentNode.id;
        
        
        //
        //  Ez at kell irni, hogy valodi propertyk legyenek.
        //
        this.config.autoTabs = false;
        this.config.width = this._width;
        this.config.height = this._height;
        this.config.shadow = true;
        //        this.config.minWidth = this._width;
        //        this.config.minHeight = this._height;
        this.config.maskTarget = document.forms[0];
        
        if (this._modal == "true") {
            this.config.modal = true;
        }
        else {
            this.config.modal = false;
        }
        
        var _dialogConfig = this.config;
        
        Ext.onReady(function(){
            var basicDialog = new SN.Controls.BasicDialog(containerDialogElement.id, _dialogConfig);
            basicDialog.addKeyListener(27, basicDialog.hide, basicDialog);
            
            SN.addComponent(basicDialog);           
        }); // end Ext.onReady 
    },
    
    dispose: function(){
        SenseNet.Portal.UI.Control.SNDialog.callBaseMethod(this, 'dispose');
    },
    
    get_modal: function(){
        return this._modal;
    },
    set_modal: function(value){
        if (this._modal !== value) {
            this._modal = value;
            this.raisePropertyChanged("modal");
        }
    },
    
    get_width: function(){
        return this._width;
    },
    set_width: function(value){
        if (this._width !== value) {
            this._width = value;
            this.raisePropertyChanged("width");
        }
    },
    
    get_height: function(){
        return this._height;
    },
    set_height: function(value){
        if (this._height !== value) {
            this._height = value;
            this.raisePropertyChanged("height");
        }
    },
    get_headerText: function(){
        return this._headerText;
    },
    set_headerText: function(value){
        if (this._headerText !== value) {
            this._headerText = value;
            this.raisePropertyChanged("headerText");
        }
    }
}

// Register the class as a type that inherits from Sys.UI.Control.
SenseNet.Portal.UI.Control.SNDialog.registerClass('SenseNet.Portal.UI.Control.SNDialog', Sys.UI.Control);
if (typeof(Sys) !== 'undefined') 
    Sys.Application.notifyScriptLoaded();