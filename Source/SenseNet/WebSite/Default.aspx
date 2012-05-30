<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SenseNet.Portal.Setup.Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"> 
<html> 
<head>
    <title>Sense/Net 6.0 - Installation Wizard</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="imagetoolbar" content="no" />
    <!--[if lt IE 7.]>
<script defer type="text/javascript" src="pngfix.js"></script>
<![endif]-->
    <style type="text/css">
        html, body, form
        {
            margin: 0;
            padding: 0;
            height: 100%;
        }
        body
        {
            background: url(Install/sn_bg.jpg) repeat-x 0 100%;
            color: #aaa;
            font-size: 12px;
            font-family: Arial, Helvetica, sans-serif;
        }
        .copyright
        {
            font-size: 10px;
            color: #aaa;
            font-family: Arial, Helvetica, sans-serif;
            float: right;
        }
        table, tr, td
        {
            margin: 0 auto;
            width: 100%;
            height: 100%;
            padding: 0;
            vertical-align: middle;
            text-align: center;
        }
        a
        {
            text-decoration: none;
            padding: 2px 5px;
        }
        .sn-portlet
        {
            width: 708px;
            margin: 0 auto;
            text-align: left;
            position:relative;
            zoom:1;
        }
        
        .sn-pt-header
        {
            background: #007DC2;
            padding: 10px;
            position: relative;
            color: #fff;
            margin-bottom: 5px;
            z-index: 100;
            zoom:1;
        }
        .sn-pt-arrow
        {
            position: absolute;
            left: 73px;
            bottom: -11px;
            width: 18px;
            height: 11px;
            background: url(Install/pt_head_arrow.gif) no-repeat 0 0;
            z-index:100;
        }
        .sn-pt-body-border
        {
            border: 1px solid #000;
        }
        .sn-pt-body-box
        {
            background: #fff;
            border: 1px solid #ccc;
            margin: 0;
            padding: 5px;
            position:releative;
            z-index: 100;

        }
        .sn-pt-body-box .sn-pt-body-inner
        {
            background: transparent url(Install/ptbg_inner.gif) repeat-x 0 100%;
            padding: 7px 13px 10px;
        }
        .sn-infotext
        {
            font-weight: bold;
            color: #001144;
        }        
        .sn-countinfo
        {
            font-weight: bold;
            color: #3322DD;
        }  
        .submiton
        {
            border: 1px solid #ccc;
            background: #fff;
            color: #222;
        }
        .submitoff
        {
            display: none;
        }
        .sn-pt-footer
        {
            padding-top: 7px;
        }
        .toppic
        {
            margin-bottom: 10px;
            display: block;
        }
        .toplogo
        {
            margin-bottom: 10px;
            display: block;
        }
        .sn-progressbar
        {
            border:1px solid #ccc;
            background: #ddd;
            position: relative;
            height: 22px;
        }

        .sn-bar {
            background: url(Install/progress.gif) repeat-x 0 0;
            height: 22px;
            float:left;
        }

        .sn-progressbar strong 
        {
            position: absolute;
            left: 50%; top: 50%;
            display: block;
            z-index: 100;
            line-height: 22px;
            width: 200px; height: 22px;
            margin: -11px 0 0 -100px;
            text-align: center;
            font-size: 16px;
            font-weight: bold;
            text-shadow: 1px 1px 1px #222;
            color:#fff;
        }
        
        .sn-errorpanel
        {
            background-color:Red;
            color:White;
        }
        
        .sn-errorpanel a
        {
            text-decoration: none;
            color:White;
        }
        
        .sn-errorpanel a:hover
        {
            text-decoration: underline;
        }
        
    </style>
</head>
<body>
    <form id="main" runat="server">
    <input type="hidden" id="MaxPictures" runat="server" name="MaxPictures" value="2" />
    <asp:ScriptManager ID="ScriptManager1" runat="server" ScriptMode="Release" EnablePartialRendering="true"
        AsyncPostBackTimeout="180" LoadScriptsBeforeUI="false">
    </asp:ScriptManager>
    <table>
        <tr>
            <td>
                <asp:Panel runat="server" ID="InstallPanel" class="sn-portlet">
                    <img class="toplogo" src="Install/installer-header.png" alt="Sense/Net 6.0" />
                    <asp:UpdatePanel ID="updSensenetMain" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Timer ID="timerMain" Interval="8000" runat="server" OnTick="Timer_OnTick" />
                            <asp:Image ID="imgMain" runat="server" CssClass="toppic" ImageUrl="Install/picFirst.png"
                                AlternateText="Congratulations" />                                
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <asp:UpdatePanel ID="updSensenetFooter" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="sn-pt-header">
                                <div class="sn-pt-arrow"></div>
                                <asp:Image id="imgMessage" ImageUrl="Install/installer-portlet-caption-text-1.png" runat="server" AlternateText="Sense/Net 6.0 Install" title="" />
                            </div>
                            <div class="sn-pt-body-box">
                                <div class="sn-pt-body-inner">                                
                                    <div style="overflow: hidden; zoom:1;">
                                        <asp:Panel runat="server" ID="plcProgressBar" CssClass="sn-progressbar">
                                            <asp:Panel ID="panelBar" CssClass="sn-bar" runat="server" Width="1%"></asp:Panel>
                                            <strong><asp:Label runat="server" ID="labelProgressPercent" Text="0.15%" /></strong>
                                        </asp:Panel>
                                        <div>
                                            <asp:Label runat="server" id="finish" CssClass="submiton" style="float: right" Visible="false">Please wait while the portal loads</asp:Label>
                                        </div>
                                        <asp:Panel ID="ErrorPanel" runat="server" Visible="false" CssClass="sn-errorpanel">
                                            <strong>Install finished with error: </strong><asp:Label ID="labelError" runat="server" /> <br />
                                           You can check the error log or visit the community forum at <strong><a href="http://forum.sensenet.com">forum.sensenet.com</a></strong>
                                        </asp:Panel>
                                    </div>
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div class="sn-pt-footer">
                    </div>
                </asp:Panel>
                <asp:Panel ID="AppNameErrorPanel" runat="server" class="sn-portlet" Visible="false">
                    <div class="sn-pt-header">
                        <div class="sn-pt-arrow"></div>
                        <asp:Image id="Image1" ImageUrl="Install/installer-portlet-caption-text-2.png" runat="server" AlternateText="Sense/Net 6.0 Install" title="" />
                    </div>
                     <div class="sn-pt-body-box">
                                <div class="sn-pt-body-inner">                                
                                    <div style="overflow: hidden; zoom:1; color:Black">
                                        <asp:Panel ID="Panel3" runat="server" >
                                            <strong>
                                           You have tried to install Sense/Net into a non-root virtual application folder. <br />
We apologize for the inconvenience, but Sense/Net needs to be installed into the web site root to run correctly. Please re-install Sense/Net into the web site root. <br />
Thank you for your patience.</strong>
                                        </asp:Panel>
                                    </div>
                                </div>
                            </div>
                </asp:Panel>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
