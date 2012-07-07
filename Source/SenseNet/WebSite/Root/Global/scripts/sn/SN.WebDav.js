// using $skin/scripts/sn/SN.js
// using $skin/scripts/jquery/jquery.js

SN.ns('SN.WebDav');

SN.WebDav = {
    RefreshPage: function () {
        //location.reload();
    },
    RefreshPageOnNextFocus: function () {
        //window.onfocus = SN.WebDav.RefreshPage;
    },
    GetSPSObject: function (objectid) {
        if ($.browser.msie) {
            var activexobject = null;
            if (window.ActiveXObject) {
                try {
                    activexobject = new ActiveXObject(objectid);
                }
                catch (e) {
                    activexobject = null;
                }
            }
            return activexobject;
        }
        else {
            try {
                var x = document.getElementById("winFirefoxPlugin");
                if (!x && navigator.mimeTypes && navigator.mimeTypes["application/x-sharepoint"] && navigator.mimeTypes["application/x-sharepoint"].enabledPlugin) {
                    var t = document.createElement("object");
                    t.id = "winFirefoxPlugin";
                    t.type = "application/x-sharepoint";
                    t.width = 0;
                    t.height = 0;
                    t.style.setProperty("visibility", "hidden", "");
                    document.body.appendChild(t);
                    x = document.getElementById("winFirefoxPlugin")
                }
            }
            catch (e)
      { x = null }
            return x
        }
    },
    OpenDocument: function (path) {


        var spobjid = "SharePoint.OpenDocuments";
        if (path.charAt(0) == "/" || path.substr(0, 3).toLowerCase() == "%2f")
            path = document.location.protocol + "//" + document.location.host + path;

        var res = SN.WebDav.OpenDocumentWithObject(0, path);
        if (res)
            return;
        res = SN.WebDav.OpenDocumentWithObject(1, path);
        if (res)
            return;
        res = SN.WebDav.OpenDocumentWithObject(2, path);
        if (!res)
            alert("The document could not be opened.");
    },
    OpenDocumentWithObject: function (mode, path) {

        // mode 0: SharePoint.OpenDocuments.3, EditDocument3
        // mode 1: SharePoint.OpenDocuments, EditDocument2
        // mode 2: SharePoint.OpenDocuments, EditDocument, ppt
        var objId = (mode == 0) ? "SharePoint.OpenDocuments.3" : "SharePoint.OpenDocuments";
        try {
            var spobj = SN.WebDav.GetSPSObject(objId);
            if (spobj) {
                var res = false;
                if (mode == 0)
                    res = spobj.EditDocument3(window, path, false, '');
                if (mode == 1)
                    res = spobj.EditDocument2(window, path, '');
                if (mode == 2) {
                    var extension = path.substr(path.length - 4, path.length - 1);
                    var param = (extension == ".ppt") ? "PowerPoint.Slide" : '';
                    res = spobj.EditDocument(path, param);
                }
                if (!res)
                    return false;
                if (mode == 0) {
                    if (spobj.PromptedOnLastOpen()) {
                        window.onfocus = SN.WebDav.RefreshPageOnNextFocus;
                    } else {
                        window.onfocus = SN.WebDav.RefreshPage;
                    }
                } else if (mode == 1) {
                    window.onfocus = SN.WebDav.RefreshPageOnNextFocus;
                } else {
                    window.onfocus = SN.WebDav.RefreshPage;
                }
                if (window.event) {
                    window.event.cancelBubble = false;
                    window.event.returnValue = false;
                }
                return true;
            }
        }
        catch (e) {
            alert("An error occurred.");
        }
        return false;
    },
    CreateDocument: function (url, template) {
        try {
            EditDocumentButton = new ActiveXObject("SharePoint.OpenDocuments.2");
            if (EditDocumentButton) {
                var template = "http://localhost:1315/Home/Sample_Document_Workspace/DemoDoclib1/Folder0/boci/UzletiKovetelmenyek.doc";
                EditDocumentButton.CreateNewDocument2(window, template, "http://localhost:1315/Home/Sample_Document_Workspace/DemoDoclib1/x.doc");
                event.cancelBubble = false;
                event.returnValue = false;
            }
        }
        catch (e) {
        }
    },
    BrowseFolder: function (src) {
        if (!$.browser.msie) {
            alert("This feature only works in Internet Explorer!");
            return;
        }

        var webFolderTarget = null;
        var webFolderSsrc = null;
        var webFolderDiv = null;
        var urlPattern = "http://[a-zA-Z0-9\-\.]+(:80)?/";
        var urlPatternRegexp = new RegExp(urlPattern, 'gi');
        var target = '_blank';

        if (src.charAt(0) == '/') src = "http://" + document.location.host + src;
        webFolderSrc = src;
        webFolderTarget = target;

        if (webFolderDiv == null) {
            webFolderDiv = document.createElement('div');
            document.body.appendChild(webFolderDiv);
            webFolderDiv.onreadystatechange = SN.WebDav.BrowseFolder;
            webFolderDiv.addBehavior('#default#httpFolder');
        }
        if (webFolderDiv.readyState == "complete") {
            webFolderDiv.onreadystatechange = null;
            var success = false;
            var targetFrame = null;

            try {
                targetFrame = document.frames.item(webFolderTarget);
                if (targetFrame != null) targetFrame.document.body.innerText = 'WebFolder not found';
            } catch (e) { }

            try {
                var ret = webFolderDiv.navigateFrame(webFolderSrc, webFolderTarget);
                if (ret == "OK") success = true;
            }
            catch (e) { }

            if (!success && webFolderSrc.search(urlPattern) == 0) {
                var sUrl = webFolderSrc.replace(urlPatternRegexp, "//$1/").replace('/', "\\");
                if (targetFrame != null) {
                    try {
                        targetFrame.onload = null;
                        targetFrame.document.location.href = sUrl;
                        success = true;
                    }
                    catch (e) { }
                }
            }

            if (!success) alert('Error');
        }
    }
}