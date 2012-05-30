<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %> 
<%@ Import Namespace="SenseNet.ContentRepository" %> 
<%@ Import Namespace="SenseNet.ContentRepository.Workspaces" %> 
<%@ Import Namespace="SenseNet.Search" %> 
<article class="snm-tile bg-zero" id="reload">
    <a href="javascript:location.reload(true)" class="snm-link-tile bg-zero clr-text">
        <span class="snm-lowertext snm-fontsize3">Refresh</span>
    </a>
</article>
<div id="snm-container">
    <div id="page1" class="snm-page">
        <div class="snm-pagecontent">
       	    <div class="snm-col snm-col-wide6">
    		    <h1>Welcome <%= User.Current.FullName %>!</h1>
            </div>
    	    <div class="snm-col snm-col-wide3">
                <article class="snm-tile snm-tile-wide3 snm-right snm-profile">
                    <section class="snm-tile snm-tile-wide2">
                        <span class="snm-middletext">
                            <span class="snm-fontsize2 snm-name"><%= User.Current.FullName %></span><br />
                            <asp:LoginStatus LogoutText="<%$ Resources:LoginPortlet,Logout %>" LogoutPageUrl="/" LogoutAction="Redirect" runat="server" CssClass="clr-text snm-fontsize3" />
                        </span>
                    </section>
                    <section class="snm-tile">
                        <span class="snm-background"><img src="<%= SenseNet.Portal.UI.UITools.GetAvatarUrl() %>?dynamicThumbnail=1&width=80&height=80" alt="" title="<%= SenseNet.ContentRepository.User.Current.FullName %>" /></span>
                    </section>
                </article>
            </div>
    	    <div class="snm-col">
                <div class="snm-panel">
                    <p>Pellentesque vel nisi quis elit sollicitudin porta. In faucibus semper nibh, sit amet porttitor elit interdum vel. Aliquam tincidunt, diam nec pulvinar tincidunt, ipsum urna tempus nibh, in vestibulum elit nisi vel ipsum. Vivamus ac diam ac tortor semper ornare eget ac ipsum. Sed id augue in mi varius elementum. Sed placerat lectus sed lectus condimentum sed lacinia nunc porttitor. Maecenas elementum dolor vitae augue porttitor sit amet luctus velit congue. Duis fringilla fringilla nisi ut interdum. Curabitur mollis nulla vel nisi bibendum varius. Phasellus non lacus mauris, sed interdum lorem. Etiam a porttitor dui. Integer luctus est at nisl euismod sit amet vestibulum erat ultricies. Morbi tincidunt viverra eros eget adipiscing. Vestibulum vel sem nec magna tempor feugiat. Pellentesque elementum facilisis porta. </p>
                </div>
                <article class="snm-tile snm-tile-wide3 bg-primary">
                    <a href="/workspaces" class="snm-link-tile snm-middletext snm-center clr-text"><span class="snm-fontsize2"><span class="snm-icon snm-icon-folder"></span> Workspaces</span></a>
                </article>
            </div>
        </div>
    </div>
</div>
