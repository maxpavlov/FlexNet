// This is a patch for Sense/Net Portal Custom WebPartZone drag and drop functionality.
function Zone_GetWebPartIndex(location) {
    var x = location.x;
    var y = location.y;

    //
    //  If we use custom html rendering, this expression always returns -1. It caused that drag and drop doesn't work properly,
    //  Couldn't find html table tag.
    
    //if ((x < this.webPartTableLeft) || (x > this.webPartTableRight) ||
    //    (y < this.webPartTableTop) || (y > this.webPartTableBottom)) {
    //    return -1;
    //}
 
    var vertical = this.isVertical;
    var webParts = this.webParts;
    var webPartsCount = webParts.length;
    for (var i = 0; i < webPartsCount; i++) {
        var webPart = webParts[i];
        if (vertical) {
            if (y < webPart.middleY) {
                return i;
            }
        }
        else {
            if (x < webPart.middleX) {
                return i;
            }
        }
    }
    return webPartsCount;
}


function Zone(zoneElement, zoneIndex, uniqueID, isVertical, allowLayoutChange, highlightColor) {
		    	var webPartTable = null;
				//	
			    if (zoneElement.rows != null)
			    {
			        if (zoneElement.rows.length == 1)
			            webPartTableContainer = zoneElement.rows[0].cells[0];
			        else
			            webPartTableContainer = zoneElement.rows[1].cells[0];   
			    } 
			    else
			    {
			        webPartTableContainer = zoneElement;
			    }
				//
			    var i;
			    for (i = 0; i < webPartTableContainer.childNodes.length; i++) {
			        var node = webPartTableContainer.childNodes[i];
					//				
					if ((node.tagName == "DIV") || (node.tagName == "TABLE"))
                    {
                        webPartTable = node;
                        break
                    }
					//
			    }
			    this.zoneElement = zoneElement;
			    this.zoneIndex = zoneIndex;
			    this.webParts = new Array();
			    this.uniqueID = uniqueID;
			    this.isVertical = isVertical;
			    this.allowLayoutChange = allowLayoutChange;
			    this.allowDrop = false;
			    this.webPartTable = webPartTable;
			    this.highlightColor = highlightColor;
			    this.savedBorderColor = (webPartTable != null) ? webPartTable.style.borderColor : null;
			    this.dropCueElements = new Array();
			    if (webPartTable != null) {
			        if (isVertical) {

						if (node.tagName == "TABLE") {
			            	for (i = 0; i < webPartTable.rows.length; i += 2) 
			                	this.dropCueElements[i / 2] = webPartTable.rows[i].cells[0].childNodes[0];
						}
						if (node.tagName == "DIV") {
                            a = 0;
                            for (var i = 0; i < webPartTable.childNodes.length; i++) {
                              // document.ELEMENT_NODE = 1
                              if (webPartTable.childNodes[i].nodeType == 1) {
                                    if (!(a % 2))
                                        this.dropCueElements[a/2] = webPartTable.childNodes[i];

                                    a = a + 1;
                               }
                            }       
						}
			        }
			        else {
						
						if (node.tagName == "TABLE") {
							for (i = 0; i < webPartTable.rows[0].cells.length; i += 2) 
			                	this.dropCueElements[i / 2] = webPartTable.rows[0].cells[i].childNodes[0];
						}
						
						if (node.tagName == "DIV") {
                            for (a = 0; a < webPartTable.childNodes.getElementsByTagName('div').length; a += 2)
                                this.dropCueElements[a / 2] = webPartTable.childNodes[a];  
                        }
						

			        }
			    }
			    this.AddWebPart = Zone_AddWebPart;
			    this.GetWebPartIndex = Zone_GetWebPartIndex;
			    this.ToggleDropCues = Zone_ToggleDropCues;
			    this.UpdatePosition = Zone_UpdatePosition;
			    this.Dispose = Zone_Dispose;
			    webPartTable.__zone = this;
        }