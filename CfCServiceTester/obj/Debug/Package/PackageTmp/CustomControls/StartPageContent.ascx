<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StartPageContent.ascx.cs" Inherits="CfCServiceTester.CustomControls.StartPageContent" %>

<table style="width:45em;">
    <colgroup>
        <col style="width:20%;" />
        <col style="width:50%" />
        <col style="width:30%" />
    </colgroup>
    <tr>
        <td>
            <a href="#" onclick="return Button1_onclick();" >Check service</a>
        </td>
        <td><span id="msgServiceWorking"></span></td>
    </tr>
    <tr>
        <td>Select SQL server</td>
        <td>
            <span class="Nowrap" id="SpanSelectServer">
                <asp:TextBox runat="server" ID="txtServerName" Width="85%" />
                <span class="Magnifier" style="display:inline-block">
                    <asp:ImageButton ID="btnPickImage" ImageUrl="~/Images/Magnifier-24.png"
                        OnClientClick='return PickServers();'
                        runat="server" ToolTip="Choose SQL Server from the list" 
                TabIndex="10" />
                </span>
                <span class="Pauser" style="display:none;">
                    <asp:Image ID="WaitImage" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>
            </span>
        </td>
        <td>
            <span id="ServerSelector" style="display:none;">
                <select onchange='javascript:svSelectionChanged(this);' />
            </span>
        </td>
    </tr>
    <tr>
        <td>Login name/password</td>
        <td>
            <asp:TextBox runat="server" ID="txtLoginName" Width="85%" 
                ToolTip="Login name" />
        </td>
        <td>
            <asp:TextBox runat="server" ID="txtLoginPasswd" Width="85%" ToolTip="Password" 
                TextMode="Password" />
        </td>
    </tr>
    <tr>
        <td>Select Database</td>
        <td>
            <span class="Nowrap" id="SpanSelectDatabase">
                <asp:TextBox runat="server" ID="txtDatabaseName" Width="85%" />
                <span class="Magnifier" style="display:inline-block">
                    <asp:ImageButton ID="btnSelectDatabase" ImageUrl="~/Images/Magnifier-24.png"
                        OnClientClick='return PickDatabases();'
                        runat="server" ToolTip="Choose Database name from the list" 
                TabIndex="11" />
                </span>
                <span class="Pauser" style="display:none;">
                    <asp:Image ID="WaitImage1" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>
            </span>
        </td>
        <td>
            <span id="DatabaseSelector" style="display:none;">
                <select onchange='javascript:dbSelectionChanged(this);'/>
            </span>
        </td>
    </tr>
    <tr>
        <td style="vertical-align:top;">
            <a href="#" onclick="return ConnectDatabase();" 
                title="Connect client to database" tabindex="0">Connect to database</a>
        </td>
        <td colspan="2" style="vertical-align:top;" >
            <span runat="server" id="spnConnectionError" class="ErrorMessage" style="display: none;" />
            <div runat="server" id="spnConnectionOK" enableviewstate="false" style="display:none;">
                <p class="OkMessage">You are connected to database now. SQL server grants you roles:</p>
                <ul id="UserRolesList" runat="server"></ul>
                <p id="LoginTooltip" runat="server" enableviewstate="false">
                    Reload the page if you want another connection.
                </p>
            </div>
        </td>
    </tr>
</table>
