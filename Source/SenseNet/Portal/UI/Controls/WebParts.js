//----------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//----------------------------------------------------------
// WebParts.js
var __wpm=null;function Point(a,b){this.x=a;this.y=b}function __getOffset(e,a){if(a.offsetY&&a.offsetY)return {x:a.offsetX,y:a.offsetY};var b=Sys.UI.DomElement.getLocation(e),c=window.pageXOffset+a.clientX-b.x,d=window.pageYOffset+a.clientY-b.y;return {x:c,y:d}}function cancelEvent(a){if(a.preventDefault&&a.stopPropagation){a.preventDefault();a.stopPropagation()}else if(window.event){window.event.returnValue=false;window.event.cancelBubble=true}}function __wpTranslateOffset(b,c,a,e,f){while(typeof a!="undefined"&&a!=null&&a!=e){b+=a.offsetLeft;c+=a.offsetTop;var d=a.tagName;if(d!="TABLE"&&d!="BODY"){if(a.clientLeft)b+=a.clientLeft;if(a.clientTop)c+=a.clientTop}if(f&&d!="BODY"){b-=a.scrollLeft;c-=a.scrollTop}a=a.offsetParent}return new Point(b,c)}function __wpGetPageEventLocation(a,c){if(typeof a=="undefined"||a==null)a=window._event;var b=__getOffset(a.target,a.rawEvent);return __wpTranslateOffset(b.x,b.y,a.target,null,c)}function WebPart(b,a,e,d,c){this.webPartElement=b;this.allowZoneChange=c;this.zone=e;this.zoneIndex=d;this.title=typeof a!="undefined"&&a!=null?a.innerText:"";b.__webPart=this;if(typeof a!="undefined"&&a!=null)a.style.cursor="move";this.UpdatePosition=WebPart_UpdatePosition;this.Dispose=WebPart_Dispose}function WebPart_Dispose(){this.webPartElement.__webPart=null}function WebPart_UpdatePosition(){var a=__wpTranslateOffset(0,0,this.webPartElement,null,false);this.middleX=a.x+this.webPartElement.offsetWidth/2;this.middleY=a.y+this.webPartElement.offsetHeight/2}
function Zone(c, h, i, d, f, g)
            {
                var b = null;
                if (c.rows != null)
                {
                    if (c.rows.length == 1)
                        webPartTableContainer = c.rows[0].cells[0];
                    else
                        webPartTableContainer = c.rows[1].cells[0];   
                } 
                else
                {
                    webPartTableContainer = c;
                }
                var a;
                for (a = 0; a < webPartTableContainer.childNodes.length; a++)
                {
                    var e = webPartTableContainer.childNodes[a];
                    //if (e.tagName == "TABLE")
                    if ((e.tagName == "DIV") || (e.tagName == "TABLE"))
                    {
                        b = e;
                        break
                    }
                }
                this.zoneElement = c;
                this.zoneIndex = h;
                this.webParts = new Array;
                this.uniqueID = i;
                this.isVertical = d;
                this.allowLayoutChange = f;
                this.allowDrop = false;
                this.webPartTable = b;
                this.highlightColor = g;
                this.savedBorderColor = b != null ? b.style.borderColor : null;
                this.dropCueElements = new Array;
                if (b != null)
                    if (d)
                    {
                        if (e.tagName == "TABLE")
                        {
                            for (a = 0; a < b.rows.length; a += 2)
                                this.dropCueElements[a / 2] = b.rows[a].cells[0].childNodes[0];                
                        }
                        if (e.tagName == "DIV")
                        {
                            //for (a = 0; a < b.childNodes.length; a += 2)
                            a = 0;
                            for (var i = 0; i < b.childNodes.length; i++) {
                              // document.ELEMENT_NODE = 1
                              if (b.childNodes[i].nodeType == 1) {
                                    if (!(a % 2))
                                    {
                                        this.dropCueElements[a/2] = b.childNodes[i];
                                        
                                    }
                                    a = a + 1;
                               }
                            }          
                         }
                    }
                    else
                    {
                        if (e.tagName == "TABLE")
                        {
                            for (a = 0; a < b.rows[0].cells.length; a += 2)
                                this.dropCueElements[a / 2] =
                                    b.rows[0].cells[a].childNodes[0];            
                        }
                        if (e.tagName == "DIV")
                        {
                            for (a = 0; a < b.childNodes.getElementsByTagName('div').length; a += 2)
                                this.dropCueElements[a / 2] = b.childNodes[a];  
                        }
                    }
                this.AddWebPart = Zone_AddWebPart;
                this.GetWebPartIndex = Zone_GetWebPartIndex;
                this.ToggleDropCues = Zone_ToggleDropCues;
                this.UpdatePosition = Zone_UpdatePosition;
                this.Dispose = Zone_Dispose;
                b.__zone = this
            }
function Zone_Dispose(){for(var a=0;a<this.webParts.length;a++)this.webParts[a].Dispose();this.webPartTable.__zone=null}function Zone_AddWebPart(d,e,c){var a=null,b=this.webParts.length;if(this.allowLayoutChange&&__wpm.IsDragDropEnabled())a=new WebPart(d,e,this,b,c);else a=new WebPart(d,null,this,b,c);this.webParts[b]=a;return a}function Zone_ToggleDropCues(g,f,h){if(h==false)this.webPartTable.style.borderColor=g?this.highlightColor:this.savedBorderColor;if(f==-1)return;var a=this.dropCueElements[f];if(a&&a.style){if(a.style.height=="100%"&&!a.webPartZoneHorizontalCueResized){var c=a.parentNode.clientHeight,e=c-10;a.style.height=e+"px";var b=a.getElementsByTagName("DIV")[0];if(b&&b.style){b.style.height=a.style.height;var d=a.parentNode.clientHeight-c;if(d){a.style.height=e-d+"px";b.style.height=a.style.height}}a.webPartZoneHorizontalCueResized=true}a.style.visibility=g?"visible":"hidden"}}function Zone_GetWebPartIndex(e){var b=e.x,c=e.y;if(b<this.webPartTableLeft||b>this.webPartTableRight||c<this.webPartTableTop||c>this.webPartTableBottom)return -1;var h=this.isVertical,f=this.webParts,d=f.length;for(var a=0;a<d;a++){var g=f[a];if(h){if(c<g.middleY)return a}else if(b<g.middleX)return a}return d}function Zone_UpdatePosition(){var a=__wpTranslateOffset(0,0,this.webPartTable,null,false);this.webPartTableLeft=a.x;this.webPartTableTop=a.y;this.webPartTableRight=this.webPartTable!=null?a.x+this.webPartTable.offsetWidth:a.x;this.webPartTableBottom=this.webPartTable!=null?a.y+this.webPartTable.offsetHeight:a.y;for(var b=0;b<this.webParts.length;b++)this.webParts[b].UpdatePosition()}function WebPartMenu(b,a,c){this.menuLabelElement=b;this.menuDropDownElement=a;this.menuElement=c;this.menuLabelElement.__menu=this;this.menuLabelElement.attachEvent("onclick",WebPartMenu_OnClick);this.menuLabelElement.attachEvent("onkeypress",WebPartMenu_OnKeyPress);this.menuLabelElement.attachEvent("onmouseenter",WebPartMenu_OnMouseEnter);this.menuLabelElement.attachEvent("onmouseleave",WebPartMenu_OnMouseLeave);if(typeof this.menuDropDownElement!="undefined"&&this.menuDropDownElement!=null)this.menuDropDownElement.__menu=this;this.menuItemStyle="";this.menuItemHoverStyle="";this.popup=null;this.hoverClassName="";this.hoverColor="";this.oldColor=this.menuLabelElement.style.color;this.oldTextDecoration=this.menuLabelElement.style.textDecoration;this.oldClassName=this.menuLabelElement.className;this.Show=WebPartMenu_Show;this.Hide=WebPartMenu_Hide;this.Hover=WebPartMenu_Hover;this.Unhover=WebPartMenu_Unhover;this.Dispose=WebPartMenu_Dispose;var d=this;Sys.Application.add_unload(function(){d.Dispose()})}function WebPartMenu_Dispose(){this.menuLabelElement.__menu=null;this.menuDropDownElement.__menu=null}function WebPartMenu_Show(){if(typeof __wpm.menu!="undefined"&&__wpm.menu!=null)__wpm.menu.Hide();var e="<html><head><style>"+"a.menuItem, a.menuItem:Link { display: block; padding: 1px; text-decoration: none; "+this.itemStyle+" }"+"a.menuItem:Hover { "+this.itemHoverStyle+" }"+'</style><body scroll="no" style="border: none; margin: 0; padding: 0;" ondragstart="window.event.returnValue=false;" onclick="popup.hide()">'+this.menuElement.innerHTML+"<body></html>",b=16,c=16;this.popup=window.createPopup();__wpm.menu=this;var d=this.popup.document;d.write(e);this.popup.show(0,0,b,c);var a=d.body;b=a.scrollWidth;c=a.scrollHeight;if(b<this.menuLabelElement.offsetWidth)b=this.menuLabelElement.offsetWidth+16;if(this.menuElement.innerHTML.indexOf("progid:DXImageTransform.Microsoft.Shadow")!=-1)a.style.paddingRight="4px";a.__wpm=__wpm;a.__wpmDeleteWarning=__wpmDeleteWarning;a.__wpmCloseProviderWarning=__wpmCloseProviderWarning;a.popup=this.popup;this.popup.hide();this.popup.show(0,this.menuLabelElement.offsetHeight,b,c,this.menuLabelElement)}function WebPartMenu_Hide(){if(__wpm.menu==this){__wpm.menu=null;if(typeof this.popup!="undefined"&&this.popup!=null){this.popup.hide();this.popup=null}}}function WebPartMenu_Hover(){if(this.labelHoverClassName!="")this.menuLabelElement.className=this.menuLabelElement.className+" "+this.labelHoverClassName;if(this.labelHoverColor!="")this.menuLabelElement.style.color=this.labelHoverColor}function WebPartMenu_Unhover(){if(this.labelHoverClassName!=""){this.menuLabelElement.style.textDecoration=this.oldTextDecoration;this.menuLabelElement.className=this.oldClassName}if(this.labelHoverColor!="")this.menuLabelElement.style.color=this.oldColor}function WebPartMenu_OnClick(){var a=window.event.srcElement.__menu;if(typeof a!="undefined"&&a!=null){cancelEvent(window.event);a.Show()}}function WebPartMenu_OnKeyPress(){if(window.event.keyCode==13){var a=window.event.srcElement.__menu;if(typeof a!="undefined"&&a!=null){cancelEvent(window.event);a.Show()}}}function WebPartMenu_OnMouseEnter(){var a=window.event.srcElement.__menu;if(typeof a!="undefined"&&a!=null)a.Hover()}function WebPartMenu_OnMouseLeave(){var a=window.event.srcElement.__menu;if(typeof a!="undefined"&&a!=null)a.Unhover()}function WebPartManager(){this.overlayContainerElement=null;this.zones=new Array;this.menu=null;this.draggedWebPart=null;this.AddZone=WebPartManager_AddZone;this.IsDragDropEnabled=WebPartManager_IsDragDropEnabled;this.ShowHelp=WebPartManager_ShowHelp;this.Execute=WebPartManager_Execute;this.SubmitPage=WebPartManager_SubmitPage;this.UpdatePositions=WebPartManager_UpdatePositions;Sys.Application.add_unload(WebPartManager_Dispose)}function WebPartManager_Dispose(){for(var a=0;a<__wpm.zones.length;a++)__wpm.zones[a].Dispose()}function WebPartManager_AddZone(e,g,f,c,d){var a=this.zones.length,b=new Zone(e,a,g,f,c,d);this.zones[a]=b;return b}function WebPartManager_IsDragDropEnabled(){return typeof this.overlayContainerElement!="undefined"&&this.overlayContainerElement!=null}function WebPartManager_Execute(b){if(this.menu)this.menu.Hide();var a=new Function(b);return a()!=false}function WebPartManager_ShowHelp(b,a){if(typeof this.menu!="undefined"&&this.menu!=null)this.menu.Hide();if(a==0||a==1)if(a==0){var c="edge: Sunken; center: yes; help: no; resizable: yes; status: no";window.showModalDialog(b,null,c)}else window.open(b,null,"scrollbars=yes,resizable=yes,status=no,toolbar=no,menubar=no,location=no");else if(a==2)window.location=b}function WebPartManager_UpdatePositions(){for(var a=0;a<this.zones.length;a++)this.zones[a].UpdatePosition()}function WebPartManager_SubmitPage(b,a){if(typeof this.menu!="undefined"&&this.menu!=null)this.menu.Hide();__doPostBack(b,a)}if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();

