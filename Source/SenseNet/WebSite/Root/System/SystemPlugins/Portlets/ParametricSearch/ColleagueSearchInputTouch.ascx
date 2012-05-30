<%@ Control Language="C#" ClassName="WebUserControl1" %>

<link rel="stylesheet" type="text/css" href="/Root/Global/styles/sn-searchportlets.css" />

<form id="ext-comp-1090" class="x-panel x-form x-fit-item">
<div class="x-panel-body x-scroller-parent" id="ext-gen1109">
    <div id="ext-gen1110" class=" x-scroller">
        <div id="ext-comp-1091" class="x-panel  x-form-fieldset">
            <div id="ext-gen1043" class="x-scroller">
                <h1>
                    Colleague Search
                </h1>
            </div>
            <div class="x-panel-body" id="ext-gen1114">
                <div id="ext-comp-1092" class="x-field x-field-text x-label-align-left">
                    <label for="name">
                        <span>Name</span>
                    </label>
                    <asp:TextBox CssClass="x-input-text" runat="server" ID="name"></asp:TextBox>
                </div>
                <div id="ext-comp-1093" class="x-field x-field-text x-label-align-left">
                    <label for="username" id="ext-gen1134">
                        <span>Username</span>
                    </label>
                    <asp:TextBox CssClass="x-input-text" runat="server" ID="username"></asp:TextBox>
                </div>
                <div id="ext-comp-1094" class="x-field x-field-text x-label-align-left">
                    <label for="email" id="ext-gen1137">
                        <span>Email</span>
                    </label>
                    <input id="email" type="email" name="email" class="x-input-email" placeholder="username@domain.com"
                        autocapitalize="off">
                </div>
                <div id="ext-comp-1094" class="x-field x-field-text x-label-align-left">
                    <label for="phone" id="ext-gen1137">
                        <span>Phone number</span>
                    </label>
                    <input id="phone" type="text" name="tel" class="x-input-tel" placeholder="+36 30 989 3322"
                        autocapitalize="off">
                </div>
                <div id="ext-comp-1096" class="x-field x-field-text x-label-align-left">
                    <label for="language" id="ext-gen1131">
                        <span>Spoken Languages</span>
                    </label>
                    <asp:TextBox CssClass="x-input-text" runat="server" ID="language"></asp:TextBox>
                </div>
                <div id="ext-comp-1097" class="x-field x-field-text x-label-align-left">
                    <label for="manager" id="ext-gen1146">
                        Manager:
                    </label>
                    <asp:TextBox CssClass="x-input-text" runat="server" ID="manager"></asp:TextBox>
                </div>
                <div id="ext-comp-1099" class="x-field x-field-text x-label-align-left">
                    <label for="department" id="ext-gen1151">
                        Department
                    </label>
                    <asp:TextBox CssClass="x-input-text" runat="server" ID="department"></asp:TextBox>
                </div>
            </div>
            <div class="sn-coll-search-button">
                <asp:Button runat="server" ID="btnSearch" Text="Search" CssClass="x-button x-button-action" />
            </div>
        </div>
    </div>
</div>
</form>
