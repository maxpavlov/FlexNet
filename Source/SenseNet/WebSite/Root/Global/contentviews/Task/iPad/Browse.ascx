<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" EnableViewState="false" %>
<script runat="server">
    Button btnStatusActive = null;
    Button btnPriorityActive = null;
    protected void btnStatusOnClick(object sender, EventArgs e)
    {
        var btn = sender as Button;
        if (btn != null && btn != btnStatusActive)
        {
            var status = btn.ID.Replace("btnSetStatus", "").ToLower();
            this.Content.Fields["Status"].SetData(status);
            this.Content.Save();
            
            // we need to set the css classes here again to represent the right status after click
            btnStatusActive.CssClass = btnStatusActive.CssClass.Replace(" snm-item-active", "");
            btn.CssClass += " snm-item-active";            
        }
    }
    protected void btnPriorityOnClick(object sender, EventArgs e)
    {
        var btn = sender as Button;
        if (btn != null && btn != btnPriorityActive)
        {
            var priority = btn.ID.Replace("btnSetPriority", "").ToLower();
            this.Content.Fields["Priority"].SetData(priority);
            this.Content.Save();

            // we need to set the css classes here again to represent the right status after click
            btnPriorityActive.CssClass = btnPriorityActive.CssClass.Replace(" snm-item-active", "");
            btn.CssClass += " snm-item-active";
        }
    }
    protected override void OnInit(EventArgs e)
    {
        var status = this.Content.ContentHandler["Status"].ToString();
        var priority = this.Content.ContentHandler["Priority"].ToString();
        
        var ph1 = this.FindControl("phStatusButtons") as PlaceHolder;
        foreach (var opt in (this.Content.Fields["Status"].FieldSetting as SenseNet.ContentRepository.Fields.ChoiceFieldSetting).Options)
        {
            var btn = new Button() { ID = "btnSetStatus" + opt.Value, Text = opt.Text, CssClass = "snm-button anim-slidein clickable-yellowbg"};
            if (status == opt.Value)
            {
                btn.CssClass += " snm-item-active";
                btnStatusActive = btn;
            }
            btn.Click += btnStatusOnClick;
            ph1.Controls.Add(btn);
        }
       
        var ph2 = this.FindControl("phPriorityButtons") as PlaceHolder;
        foreach (var opt in (this.Content.Fields["Priority"].FieldSetting as SenseNet.ContentRepository.Fields.ChoiceFieldSetting).Options)
        {
            var btn = new Button() { ID = "btnSetPriority" + opt.Value, Text = opt.Text, CssClass = "snm-button anim-slidein clickable-yellowbg" };
            if (priority == opt.Value)
            {
                btn.CssClass += " snm-item-active";
                btnPriorityActive = btn;
            }
            btn.Click += btnPriorityOnClick;
            ph2.Controls.Add(btn);
        }
        base.OnLoad(e);
    }
</script>
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
        <h1 class="anim-slidein"><%= this.Content["DisplayName"] %></h1>
        <dl class="snm-property-list">          
          <dt>Start date:</dt><dd><%= DateTime.Parse(this.Content["StartDate"].ToString()) == DateTime.MinValue ? "n/a" : DateTime.Parse(this.Content["StartDate"].ToString()).ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern)%></dd>
          <dt>Due date:</dt><dd><%= DateTime.Parse(this.Content["DueDate"].ToString()).ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern) %></dd>                    
          <dt>Description:</dt><dd><%= this.Content["Description"] %></dd>
          <dt>Assigned to:</dt>
          <dd>
            <% 
                var users = this.Content.Fields["AssignedTo"].GetData() as List<SenseNet.ContentRepository.Storage.Node>;
                if (users != null && users.Count > 0)
                {
                    foreach (var user in users)
                    {
                        var tmpUser = user as SenseNet.ContentRepository.User;
                        if (tmpUser != null)
                        {
                            var avatarUrl = String.IsNullOrEmpty(tmpUser.AvatarUrl) ? "/Root/Global/images/default_avatar.png" : tmpUser.AvatarUrl;
                            var fullName = String.IsNullOrEmpty(tmpUser.FullName) ? tmpUser.DisplayName : tmpUser.FullName;                                                      
                            %><a class="snm-avatar-user clickable-yellowbg" style="background-image: url(<%= avatarUrl + "?dynamicThumbnail=1&width=32&height=32" %>);" href="<%= SenseNet.Portal.Helpers.Actions.ActionUrl(SenseNet.ContentRepository.Content.Create(tmpUser), "Profile", true) %>" ><%= fullName %></a><%
                        }
                    }
                }
                else
                {
                    %>&nbsp;<%
                }
            %>
           </dd>
          <dt>Completion:</dt><dd><%= this.Content["TaskCompletion"] %>%</dd>
          <dt></dt><dd></dd>
        </dl>
        <h2>Actions</h2>
        <dl class="snm-property-list">
          <dt>Status:</dt>
          <dd>
            <asp:UpdatePanel ID="updatePnlStatus" runat="server" UpdateMode="Conditional">
              <ContentTemplate>
                <asp:PlaceHolder ID="phStatusButtons" runat="server" />            
              </ContentTemplate>
            </asp:UpdatePanel>          
          </dd>
          <dt>Priority:</dt>
          <dd>
            <asp:UpdatePanel ID="updatePnlPriority" runat="server" UpdateMode="Conditional">
              <ContentTemplate>
                <asp:PlaceHolder ID="phPriorityButtons" runat="server" />              
              </ContentTemplate>
            </asp:UpdatePanel>
          </dd>
        </dl>
      </div>
    </div>
  </div>
</div>     
            