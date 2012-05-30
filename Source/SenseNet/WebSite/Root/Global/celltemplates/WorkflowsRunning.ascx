<sn:ActionLinkButton ID="ActionWorkflows" runat='server' NodePath='<%# Eval("Path") %>' ActionName='Workflows' Text="" Visible='<%# Eval("WorkflowsRunning") %>' 
    ToolTip='<%# SenseNet.Portal.UI.ContentListViews.ListHelper.GetRunningWorkflowsText(((SNCR.Content)Container.DataItem).ContentHandler) %>'  />    

