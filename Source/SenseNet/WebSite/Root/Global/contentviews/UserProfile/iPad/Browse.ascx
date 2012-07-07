<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" EnableViewState="false" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>

<%
    var userProfile = this.Content.ContentHandler as SenseNet.ContentRepository.UserProfile;
    var email = userProfile.User.Email;
    var phone = userProfile.User["Phone"] as string;
    var managerRefList = this.Content["Manager"] as IEnumerable<SenseNet.ContentRepository.Storage.Node>;
    var manager = managerRefList == null ? null : managerRefList.FirstOrDefault() as SenseNet.ContentRepository.User;
    var department = userProfile.User["Department"] as string;
    var languages = userProfile.User["Languages"] as string;
    var education = userProfile.User["Education"] as string;
    
    var gender = userProfile.User["Gender"] as string;    
    DateTime dob;
    var dobOkay = DateTime.TryParse(userProfile.User["BirthDate"].ToString(), out dob);
    var ageGender = String.Format("{0}{1}", !dobOkay || dob == DateTime.MinValue ? String.Empty : Convert.ToInt32((DateTime.Now - dob).TotalDays / 365.242199).ToString(), String.IsNullOrEmpty(gender) ? String.Empty : "/" + gender.ToLower()[0]).TrimStart('/');

    var maritialStatus = userProfile.User["MaritalStatus"] as string;

    var twitter = userProfile.User["TwitterAccount"] as string;
    var facebook = userProfile.User["FacebookURL"] as string;
    var linkedin = userProfile.User["LinkedInURL"] as string;
    
    var username = SenseNet.ContentRepository.User.Current.Name;
    var profileUsername = userProfile.User.Username;

    bool showEducationAndWork = !string.IsNullOrEmpty(department) && !string.IsNullOrEmpty(languages) && !SenseNet.Portal.Helpers.UserProfiles.IsEducationEmpty(education);
    bool showSocialNetworks = !string.IsNullOrEmpty(twitter) && !string.IsNullOrEmpty(facebook) && !string.IsNullOrEmpty(linkedin);    
%>
<article class="snm-tile bg-zero" id="reload">
  <a href="javascript:location.reload(true)" class="snm-link-tile bg-zero clr-text">
    <span class="snm-lowertext snm-fontsize3">Refresh</span>
  </a>
</article>
<article class="snm-tile" id="backtile">
  <a href="javascript:window.history.back()" class="snm-link-tile bg-semitransparent clr-text">
    <span class="snm-lowertext snm-fontsize3">Back</span>
  </a>
</article>
<div id="snm-container">
  <div id="page1" class="snm-page">
    <div class="snm-pagecontent">
      <div class="snm-col">
        <h1 class="anim-slidein"><%= String.Format("{0}{1}", userProfile.User.FullName, String.IsNullOrEmpty(ageGender) ? String.Empty : String.Format(" ({0})", ageGender)) %></h1>
        <div id="snm-userprofile-centercol">
          <div id="snm-userprofile-leftcol">
            <a href='<%= userProfile.User.AvatarUrl %>' title='<%= userProfile.User.FullName %>' rel="sexylightbox">
              <img src='<%= userProfile.User.AvatarUrl + "?dynamicThumbnail=1&width=150&height=150" %>' alt="Missing User Image" class="sn-avatar nice_img" />
            </a>
            <div id="snm-userprofile-nav">
              <ul>            
                <li><a id="snm-userprofile-nav-info" class="snm-button clickable-yellowbg snm-item-active" href="#">Info</a></li>
                <li><a id="snm-userprofile-nav-wall" class="snm-button clickable-yellowbg" href="#">Wall</a></li>
                <li><a id="snm-userprofile-nav-photos" class="snm-button clickable-yellowbg" href="#">Photos</a></li>
              </ul>
            </div>
          </div>
          <div id="snm-userprofile-content">
              <div id="snm-userprofile-info" class="snm-tab-active">
                  <h2>Personal</h2>
                  <dl class="snm-property-list">                    
                      <dt>username</dt><dd><%= profileUsername %></dd>
                      <% if (!string.IsNullOrEmpty(email)) { %><dt>e-mail</dt><dd><a href='mailto:<%= email%>'><%= email %></a></dd>  <% } %>
                      <% if (dob != DateTime.MinValue) { %>
                          <dt>Date of birth:</dt><dd><%= dob.ToShortDateString() %></dd>
                      <% } %>
                      <% if (!string.IsNullOrEmpty(maritialStatus)){ %>
                          <dt>Maritial status:</dt><dd><%= maritialStatus  %></dd>
                      <% } %>
                      <% if (!string.IsNullOrEmpty(phone)) { %><dt>phone</dt><dd><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "PhoneLabel") %>: <%= phone %></dd>  <% } %>
                      <% if (manager != null) { %>
                          <dt><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "ManagerLabel")%>:</dt>
                          <dd><a href='<%= Actions.ActionUrl(SenseNet.ContentRepository.Content.Create(manager), "Profile") %>' title="[manager]"><%= manager.FullName %></a></dd>
                      <% } %>
                  </dl>
                  <% if(showEducationAndWork){ %>
                      <h2>Education and work</h2>
                      <dl class="snm-property-list">
                          <% if (!string.IsNullOrEmpty(department)) { %>
                              <dt><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "DepartmentLabel") %>:</dt><dd><%= department%></dd>
                          <% } %>
                          <% if (!string.IsNullOrEmpty(languages)) { %>
                              <dt><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "LanguagesLabel")%>:</dt><dd><%= languages%></dd>
                          <% } %>
                          <% if (!SenseNet.Portal.Helpers.UserProfiles.IsEducationEmpty(education)) { %>
                              <dt>Education:</dt><dd><%= education %></dd>
                          <% } %>                    
                      </dl>
                  <%} %>
                  <% if(showSocialNetworks){ %>
                      <h2>Social networks</h2>
                      <dl class="snm-property-list">
                          <% if (!string.IsNullOrEmpty(twitter)){ %>
                              <dt>Twitter:</dt><dd>@<%= twitter%></dd>
                          <% } %>
                          <% if (!string.IsNullOrEmpty(facebook)){ %>
                              <dt>Facebook:</dt><dd><%= facebook%></dd>
                          <% } %>
                          <% if (!string.IsNullOrEmpty(linkedin)){ %>
                              <dt>LinkedIn:</dt><dd><%= linkedin%></dd>
                          <% } %>
                      </dl>
                  <%} %>
              </div>
              <div id="snm-userprofile-wall" class="snm-tab-hidden">
                <h2>Wall</h2>
              </div>
              <div id="snm-userprofile-photos" class="snm-tab-hidden">
                <h2>Photos</h2>
              </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>        
          