<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage.Schema" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>

<asp:TextBox runat="server" ID="ContextNodePath" Visible="false" Enabled="false"></asp:TextBox>

<script runat="server">
  
    private Node GetUserNode()
    {
        return Node.LoadNode(ContextNodePath.Text);
    }

    protected void UpdateMemberships(object sender, EventArgs e)
    {
      
        System.Threading.Thread.Sleep(800);
        var user = GetUserNode();
        var groups = GroupList.DataSource as List<Node>;
        foreach (var group in groups)
        {
            var checkbox = GroupList.Items[groups.IndexOf(group)].FindControl("MemberOfGroup") as CheckBox;
            //FindControl("MemberOF_" + group.Id) as CheckBox;
            if (checkbox.Checked && !group.HasReference("Members", user) && !Security.IsUserInRole(group.Path, user.Path))
            {
                group.AddReference("Members", user);
                group.Save();
            }
            else if (!checkbox.Checked && group.HasReference("Members", user))
            {
                group.RemoveReference("Members", user);
                group.Save();
            }
        }
  
    }

    protected bool HasSavePermission(string groupPath)
    {
        var groupNode = SenseNet.ContentRepository.Content.Load(Eval("Path").ToString());
        return groupNode.Security.GetPermission(PermissionType.Save) == SenseNet.ContentRepository.Storage.Security.PermissionValue.Allow;
    }
    
    private string GetGroupNameText(SenseNet.ContentRepository.Group group)
    {
        var text = group.Domain == null ? group.Name : group.Domain.Name + "\\" + group.Name;
        if (group.Workspace != null)
            text += string.Format(" ({0})", group.Workspace.Name);

        return text;
    }
   
</script>

<asp:UpdatePanel ID="GroupManagerUpdatePanel" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:ListView ID="GroupList" runat="server" EnableViewState="false" DataMember="Path">
            <LayoutTemplate>
                <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />
            </LayoutTemplate>
            <ItemTemplate>
                <asp:CheckBox runat="server" Text='<%#GetGroupNameText(Container.DataItem as SenseNet.ContentRepository.Group)%>'
                    ID="MemberOfGroup" Enabled='<%# HasSavePermission(Eval("Path").ToString()) %>'
                    Checked='<%#Security.IsUserInRole(Eval("Path").ToString(), ContextNodePath.Text) %>' />
                <br />
            </ItemTemplate>
        </asp:ListView>
        <table>
            <tr style="vertical-align:middle;">
                <td style="padding:8px 10px 8px 0;">
                    <asp:Button CssClass="sn-floatleft" runat="server" ID="btnUpdate" OnClick="UpdateMemberships" Text="Update memberships" />
                </td>
                <td>
                    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                        <ProgressTemplate>
                            <asp:Image ID="Image1" runat="server" ImageUrl="/root/global/images/loading.gif" />
                        </ProgressTemplate>
                    </asp:UpdateProgress>
                </td>
            </tr>
        </table>
    </ContentTemplate>
</asp:UpdatePanel>
