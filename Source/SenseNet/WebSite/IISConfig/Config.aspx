<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Config.aspx.cs" Inherits="SenseNet.Portal.Setup.IISConfig.Config" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"> 

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Sense/Net 6.0 - Installation Wizard</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="imagetoolbar" content="no" />
    <link rel="stylesheet" href="/Install/install.css" type="text/css" />
    <%--<script type="text/javascript" src="/Install/Root/Global/scripts/jquery/jquery.js" />--%>
</head>

<body>
    <form id="form1" runat="server" name="form1" >
    
    <script type="text/javascript">
        
        function ShowPanels2(radioObj) {
            if(radioObj.value=='1') {
                document.getElementById('panelFormsId').style.display = 'block';
                document.getElementById('panelWindowsId').style.display = 'none';
            }
            else {
                document.getElementById('panelFormsId').style.display = 'none';
                document.getElementById('panelWindowsId').style.display = 'block';
            }            
        }

    </script>
    
    <div style="">  
    
    </div>
    <asp:TextBox ID="InvisibleTextBox" runat="server" Style="visibility:hidden;display:none;" /> 
    
    <table>
        <tr>
            <td>
                <div class="sn-portlet">
                    <img class="toplogo" src="../Install/installer-header.png" alt="Sense/Net 6.0" />
                            <asp:Image ID="imgMain" runat="server" CssClass="toppic" ImageUrl="~/Install/picWinLogin.png"
                                AlternateText="Congratulations" />
                            <div class="sn-pt-body-box">
                            <asp:PlaceHolder ID="plcMain" runat="server" >
                            <table> 
                                <tr>
                                    <td style="width:50%">
                                        <div style="margin: 40px 50px 50px 10px;" class="sn-infotext" >
                                            Please select the authentication type for your new site: <br /><br />
                                            <input type="radio" name="myRadioGroup" id="rdbFormsId" value="1" onclick="ShowPanels2(this)" checked="checked" />Forms<br />
                                            <input type="radio" name="myRadioGroup" id="rdbWindowsId" value="2" onclick="ShowPanels2(this)" />Windows
                                        </div>
                                    </td>
                                    <td style="width:50%">
                                        <div id="panelFormsId" class="sn-infotext" style="margin: 40px 10px 10px 10px; display:block;" runat="server">
                                            The portal will be installed with forms authentication. <br/>
                                            After the install process, you can log in with the username and password <br />
                                            <span style="color:Red;">Administrator/administrator</span>. <br/> <br/>
                                            <asp:Button runat="server" ID="buttonForms" CssClass="" Text="Ok" class="submiton" />

                                        </div>                                
                                        <div id="panelWindowsId" style="display:none;" runat="server">
                                            <asp:Login ID="Login1" runat="server" 
                                                BackColor="#EFF3FB" BorderColor="#B5C7DE" 
                                                BorderPadding="4" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" 
                                                Font-Size="1.1em" ForeColor="#333333" Width="300px" DisplayRememberMe="False" 
                                                Orientation="Vertical" TitleText="Please log in with your domain user">
                                                    <TextBoxStyle Font-Size="1.1em" />
                                                    <LoginButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" 
                                                        Font-Names="Verdana" Font-Size="1.1em" ForeColor="#007DC2" />
                                                    <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
                                                    <TitleTextStyle BackColor="#007DC2" Font-Bold="True" Font-Size="1.1em" ForeColor="White" />
                                            </asp:Login>
                                        </div>
                                    </td>
                                </tr>
                            </table>                                
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="plcFinish" runat="server" Visible="false">
                <div class="sn-pt-header">
                                <div class="sn-pt-arrow"></div>
                                <span style="color:White;font-weight:bold;font-size:1.2em" >You logged in successfully. Press Next to continue.</span>
                            </div>
                <div class="sn-pt-body-box">
                                <div class="sn-pt-body-inner">
                                    <div style="overflow: hidden; zoom:1;">                                            
                                    <a href="/" class="submiton" style="float: right">Next</a>
        </div>
                                </div>
                            </div>
    </asp:PlaceHolder>
                            </div>
                    <div class="sn-pt-footer">
                    </div>
                </div>
            </td>
        </tr>
    </table>
    </form>
    
    
</body>
</html>
