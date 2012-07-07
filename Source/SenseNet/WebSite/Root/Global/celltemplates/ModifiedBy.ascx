
<sn:ActionLinkButton ID="ModifiedBy" runat='server' NodePath='<%# ListHelper.GetModifierSafely(Container.DataItem) == null ? string.Empty : ListHelper.GetModifierSafely(Container.DataItem).Path%>' ActionName='Profile' IconVisible="false"
    Text='<%# ListHelper.GetModifierSafely(Container.DataItem) == null ? "unknown" : ListHelper.GetModifierSafely(Container.DataItem).FullName %>'
ToolTip='<%# ListHelper.GetModifierSafely(Container.DataItem) == null ? string.Empty : ListHelper.GetModifierSafely(Container.DataItem).Domain + "\\" + ListHelper.GetModifierSafely(Container.DataItem).Name %>'  />