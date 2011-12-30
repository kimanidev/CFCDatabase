<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DatabaseBackupContent.ascx.cs" Inherits="CfCServiceTester.CustomControls.DatabaseBackupContent" %>

<table style="width:45em;">
    <colgroup>
        <col style="width:20%;" />
        <col style="width:50%" />
        <col style="width:30%" />
    </colgroup>
    <tr>
        <td>Backup directory</td>
        <td colspan="2">
            <asp:TextBox ID="txtBackupDirectory" runat="server" Text="<%$ appSettings:BackupDirectory %>" ReadOnly="true" Width="98%" 
                 BackColor="#EAF7FB" />
        </td>
    </tr>
    <tr>
        <td>Selected Server</td>
        <td colspan="2">
            <asp:TextBox ID="txtServerName1" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
        </td>
    </tr>
    <tr>
        <td>&nbsp;</td>
        <td colspan="2">
            <asp:CheckBox ID="chkSingleMode" runat="server" Checked="true" Text="Single user mode" 
                          ToolTip="Check this control for switching to single user mode." />
        </td>
    </tr>
    <tr>
        <td colspan="3" style="padding-bottom: 2ex;"><hr /></td>
    </tr>
    <tr>
        <th colspan="3">Backup</th>
    </tr>
    <tr>
        <td>File name</td>
        <td colspan="2">
            <asp:TextBox ID="txtBackupFileName" runat="server" Text="" Width="98%" />
        </td>
    </tr>
    <tr>
        <td>
            <a href="#" onclick='return BackupDatabase("<%= chkOverwrite.ClientID %>");' title="Backup database">Backup database</a>
        </td>
        <td colspan="2">
            <asp:CheckBox ID="chkOverwrite" runat="server" Checked="true" Text="Overwrite existing file." 
                ToolTip="The service will delete file with the same name before performing operation." />
        </td>
    </tr>
    <tr>
        <td colspan="3">
            <span runat="server" id="spnBackupError" class="ErrorMessage" style="display: none;" />
            <span runat="server" id="spnBackupOK" class="OkMessage" enableviewstate="false" style="display:none;">
                Backup is done.
            </span>
        </td>
    </tr>
    <tr>
        <td colspan="3"><hr /></td>
    </tr>
    <tr>
        <th colspan="3">Restore</th>
    </tr>
    <tr>
        <td>Select Database</td>
        <td>
            <span class="Nowrap" id="SpanSelectDatabase1">
                <asp:TextBox runat="server" ID="txtDatabaseName1" Width="85%" ToolTip="Restoring may be performed to another database." />
                <span class="Magnifier" style="display:inline-block">
                    <asp:ImageButton ID="btnSelectDatabase" ImageUrl="~/Images/Magnifier-24.png"
                        OnClientClick='return PickDatabases1();'
                        runat="server" ToolTip="Choose Database name from the list" />
                </span>
                <span class="Pauser" style="display:none;">
                    <asp:Image ID="WaitImage1" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>
            </span>
        </td>
        <td>
            <span id="DatabaseSelector1" style="display:none;">
                <select onchange='javascript:dbSelectionChanged1(this);'/>
            </span>
        </td>
    </tr>
    <tr>
        <td>Restore from file</td>
        <td>
            <span class="Nowrap" id="SpanSelectFile1">
                <asp:TextBox runat="server" ID="txtFileName1" Width="85%" ToolTip="Select backup file for restoring." />
                <span class="Magnifier" style="display:inline-block">
                    <asp:ImageButton ID="ImageButton1" ImageUrl="~/Images/Magnifier-24.png"
                        OnClientClick='return PickBackupFiles1();'
                        runat="server" ToolTip="Choose backup file from the list" />
                </span>
                <span class="Pauser" style="display:none;">
                    <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>
            </span>
        </td>
        <td>
            <span id="FileSelector1" style="display:none;">
                <select onchange='javascript:dbFileSelectionChanged1(this);'/>
            </span>
        </td>
    </tr>

    <tr>
        <td>
            <a href="#" onclick='return RestoreDatabase("<%= chkOverwriteDb1.ClientID %>");' title="Restore database">Restore database</a>
        </td>
        <td colspan="2">
            <span class="Nowrap" id="RestoreDatabase1">
                <asp:CheckBox ID="chkOverwriteDb1" runat="server" Checked="true" Text="Overwrite existing file." 
                              ToolTip="The SQL server will overwrite database with the same name." />
                <span class="Pauser" style="display:none;">
                    <asp:Image ID="Image2" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>
            </span>
        </td>
    </tr>
    <tr>
        <td colspan="3">
            <span runat="server" id="spnRestoreError1" class="ErrorMessage" style="display: none;" />
            <span runat="server" id="spnRestoreOK1" class="OkMessage" enableviewstate="false" style="display:none;">
                Database is restored.
            </span>
        </td>
    </tr>

</table>
