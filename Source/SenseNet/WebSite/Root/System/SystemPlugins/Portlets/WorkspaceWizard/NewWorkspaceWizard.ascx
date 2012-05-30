<%@  Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<asp:UpdatePanel ID="WizardUpdatePanel" runat="server" ChildrenAsTriggers="true">
    <contenttemplate>
    <asp:Label ID="ErrorMessage" runat="server" CssClass="sn-error-msg"></asp:Label>
    <input type="hidden" id="_settings" runat="server" runat="server" />
                <asp:Wizard ID="Wizard1" runat="server" EnableViewState="true" DisplaySideBar="false" BorderStyle="None" BorderWidth="0" CellPadding="0" CellSpacing="0" CssClass="snWsWizard">
                    <HeaderTemplate>
                        <div class="snWsWizardHeader">
                            <h1>
                                Sense/Net 6.0<br />
                                New Workspace Wizard</h1>
                            <div class="sn-logo">
                                SenseNet 6.0</div>
                        </div>
                    </HeaderTemplate>
                    <SideBarTemplate>
                        <asp:Label ID="SideBarInfo" runat="server" />
                        <asp:DataList ID="SideBarList" RepeatDirection="Horizontal" RepeatLayout="Flow" Style="display: none;"
                            runat="server">
                            <ItemTemplate>
                                <asp:LinkButton ID="SideBarButton" Visible="false" runat="server"></asp:LinkButton>
                            </ItemTemplate>
                            <SelectedItemStyle Font-Bold="True" CssClass="Wizard-Sidebar-Selected" />
                        </asp:DataList>
                    </SideBarTemplate>
                    <WizardSteps>
                        <asp:TemplatedWizardStep  ID="ChooseWorkspaceStep" runat="server" AllowReturn="true"
                            StepType="Start">
                            <ContentTemplate>
                                <div class="snWsWizardMain">
                                    <div class="snWsWizardMainLeft snWsWizardRoundedTable">
                                      <div class="nw">
                                        <div class="ne">
                                          <div class="n"></div>
                                        </div>
                                      </div>
                                      <div class="w">
                                        <div class="e">
                                          <div class="c">
                                            <asp:RadioButtonList ID="WorkspaceList" EnableViewState="true" CssClass="snWsWizardRbList" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow">
                                            </asp:RadioButtonList>
                                          </div>
                                        </div>
                                      </div>                    
                                      <div class="sw">
                                        <div class="se">
                                          <div class="s"></div>
                                        </div>
                                      </div>                                      
                                    </div>
                                    <div class="snWsWizardMainRight"></div>
                                </div>
                            </ContentTemplate>
                            <CustomNavigationTemplate>
                              <div class="sn-pt-header">
                                <div class="sn-pt-header-tl"></div>
                                <div class="sn-pt-header-center">
                                  <div class="sn-pt-title">Create a Workspace in three easy steps in just a few seconds</div>
                                </div>
                              </div>
                              <div class="sn-pt-body-border ui-widget-content">
                                <div class="sn-pt-body>
                                  <div class="snWsWizardFooter">
                                      <ol class="snWsWizardProgButtons">
                                          <li class="snWsWizardActive_1">
                                              Choose the type of Workspace
                                              <br/><span class="snWsWizardSubscript">you are at this step</span>
                                          </li>
                                          <li class="snWsWizardAvailable_2">
                                              <asp:LinkButton UseSubmitBehavior="True" ID="StepNextButton" runat="server" CommandName="MoveNext" Text="Name your workspace" />
                                          </li>
                                          <li class="snWsWizardUnavailable_3">Let the wizard create you a workspace in a few seconds</li>
                                      </ol>
                                  </div>
                                </div>  
                              </div>
                            </CustomNavigationTemplate>
                        </asp:TemplatedWizardStep>
                        <asp:TemplatedWizardStep ID="WorkspaceFormStep" runat="server" AllowReturn="True"
                            StepType="Step">
                            <ContentTemplate>
                                <div class="snWsWizardMain">
                                    <div class="snWsWizardForm snWsWizardRoundedTable">
                                      <div class="nw">
                                        <div class="ne">
                                          <div class="n"></div>
                                        </div>
                                      </div>
                                      <div class="w">
                                        <div class="e">
                                          <div class="c">
                                            <h2>
                                            Please name and describe your new <asp:Label ID="NewWorkspaceTypeName" runat="server" ForeColor="Red" /> Workspace</h2>
                                            <asp:Label ID="WorkspaceNameLabel" runat="server" Text="Workspace name" AssociatedControlID="WorkspaceNameText"></asp:Label><br />
                                            <asp:Label ID="WorkspaceNameDescLabel" runat="server" AssociatedControlID="WorkspaceNameText" Text="The name will also show up in the URL of your workspace."></asp:Label>
                                            <asp:TextBox ID="WorkspaceNameText" runat="server" Columns="40"></asp:TextBox>
                                            <br />
                                            <asp:Label ID="WorkspaceDescLabel" runat="server" Text="Brief description" AssociatedControlID="WorkspaceDescText"></asp:Label><br />
                                            <asp:Label ID="WorkspaceDescDescLabel" runat="server" AssociatedControlID="WorkspaceDescText" Text="The name will also show up in the URL of your workspace."></asp:Label>
                                            <asp:TextBox ID="WorkspaceDescText" runat="server" Rows="5" Columns="40" TextMode="MultiLine"></asp:TextBox>                                        
                                          </div>
                                        </div>
                                      </div>                    
                                      <div class="sw">
                                        <div class="se">
                                          <div class="s"></div>
                                        </div>
                                      </div>                                      
                                    </div>
                                </div>
                            </ContentTemplate>
                            <CustomNavigationTemplate>
                              <div class="sn-pt-header">
                                <div class="sn-pt-header-tl"></div>
                                <div class="sn-pt-header-center">
                                  <div class="sn-pt-title">Create a Workspace in three easy steps in just a few seconds</div>
                                </div>
                              </div>
                              <div class="sn-pt-body-border ui-widget-content">
                                <div class="sn-pt-body>
                                  <div class="snWsWizardFooter">
                                      <ol class="snWsWizardProgButtons">
                                        <li class="snWsWizardAvailable_1">
                                            <asp:LinkButton UseSubmitBehavior="False" ID="MovePrevious" runat="server" CommandName="MovePrevious" Text="Choose the type of Workspace" />
                                        </li>
                                        <li class="snWsWizardActive_2">
                                            Name your workspace
                                            <br/><span class="snWsWizardSubscript">you are at this step</span>
                                        </li>
                                        <li class="snWsWizardAvailable_3">
                                            <asp:LinkButton UseSubmitBehavior="True" ID="MoveNext" runat="server" CommandName="MoveNext" Text="Let the wizard create you a workspace in a few seconds" />
                                        </li>
                                      </ol>
                                  </div>
                                </div>  
                              </div>
                            </CustomNavigationTemplate>
                        </asp:TemplatedWizardStep>
                        <asp:TemplatedWizardStep ID="Progress" runat="server">
                            <ContentTemplate>
                                <div class="snWsWizardMain">
                                    <div class="snWsWizardMainLeft snWsWizardRoundedTable">
                                      <div class="nw">
                                        <div class="ne">
                                          <div class="n"></div>
                                        </div>
                                      </div>
                                      <div class="w">
                                        <div class="e">
                                          <div class="c">
                                              <h2><asp:Label ID="ProgressHeaderLabel" runat="server" ></asp:Label></h2>                                    
                                              <asp:LinkButton UseSubmitBehavior="False" ID="CreateWorkspaceNowButton" runat="server" CommandName="MoveNext" Text="Create workspace now..." />
                                          </div>
                                        </div>
                                      </div>                    
                                      <div class="sw">
                                        <div class="se">
                                          <div class="s"></div>
                                        </div>
                                      </div>                                      
                                    </div>
                                    <div class="snWsWizardMainRight"></div>
                              </div>
                            </ContentTemplate>
                            <CustomNavigationTemplate>
                              <div class="sn-pt-header">
                                <div class="sn-pt-header-tl"></div>
                                <div class="sn-pt-header-center">
                                  <div class="sn-pt-title">Create a Workspace in three easy steps in just a few seconds</div>
                                </div>
                              </div>
                              <div class="sn-pt-body-border ui-widget-content">
                                <div class="sn-pt-body>
                                  <div class="snWsWizardFooter">
                                      <ol class="snWsWizardProgButtons">
                                        <li class="snWsWizardUnavailable_1">Choose the type of Workspace</li>
                                        <li class="snWsWizardAvailable_2">
                                            <asp:LinkButton UseSubmitBehavior="False" ID="MovePrevious" runat="server" CommandName="MovePrevious" Text="Name your workspace" />
                                        </li>
                                        <li class="snWsWizardActive_3">
                                            Let the wizard create you a workspace in a few seconds
                                            <br/><span class="snWsWizardSubscript">you are at this step</span>
                                        </li>
                                      </ol>
                                  </div>
                                </div>  
                              </div>
                            </CustomNavigationTemplate>
                        </asp:TemplatedWizardStep>
                        <asp:TemplatedWizardStep ID="Complete" runat="server">
                            <ContentTemplate>
                                <div class="snWsWizardMain">
                                <div class="snWsWizardMainLeft">
                                    <h2>Your workspace is ready to use</h2>
                                </div>
                                <div class="snWsWizardMainRight"></div>
                                </div>
                            </ContentTemplate>
                            <CustomNavigationTemplate>
                                <div class="snWsWizardFooter">
                                    <div class="snWsWizardReadyButton">
                                        Create and jump to your new Workspace: <asp:Label ID="NewWorkspaceName" runat="server" ForeColor="Red"></asp:Label>
                                        <a href="" id="NewWorkspaceLink" runat="server">GO</a>
                                    </div>
                                </div>
                            </CustomNavigationTemplate>
                        </asp:TemplatedWizardStep>
                    </WizardSteps>
                </asp:Wizard>
            </contenttemplate>
</asp:UpdatePanel>
<asp:UpdateProgress ID="UpdateProgress1" AssociatedUpdatePanelID="WizardUpdatePanel"
    runat="server" DisplayAfter="0" DynamicLayout="true">
    <progresstemplate>
        <div class="snWsWizardWait">
                work in progress...
        </div>
    </progresstemplate>
</asp:UpdateProgress>
