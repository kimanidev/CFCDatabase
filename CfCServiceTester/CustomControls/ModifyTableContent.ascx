<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ModifyTableContent.ascx.cs" Inherits="CfCServiceTester.CustomControls.ModifyTableContent" %>

<table style="width:45em;">
    <colgroup>
        <col style="width:20%;" />
        <col style="width:50%" />
        <col style="width:30%" />
    </colgroup>
    <tr>
        <td>Selected Server</td>
        <td colspan="2">
            <asp:TextBox ID="txtServerName2" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
        </td>
    </tr>
    <tr>
        <td>Selected database</td>
        <td colspan="2">
            <asp:TextBox ID="txtDatabaseName2" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
        </td>
    </tr>
    <tr>
        <td>&nbsp;</td>
        <td colspan="2">
            <asp:CheckBox ID="chkSingleMode2" runat="server" Checked="false" Text="Single user mode" 
                          ToolTip="Check this control for switching to single user mode." />
        </td>
    </tr>
    <tr>
        <td>Table name</td>
        <td>
            <span class="Nowrap" id="SpanSelectTable2">
                <asp:TextBox runat="server" ID="txtTable2" Width="85%" />
                <span class="Magnifier" style="display:inline-block">
                    <asp:ImageButton ID="btnPickImage" ImageUrl="~/Images/Magnifier-24.png"
                        OnClientClick='return PickUpTables();'
                        runat="server" ToolTip="Choose table from the list" />
                </span>
                <span class="Pauser" style="display:none;">
                    <asp:Image ID="WaitImage" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>
            </span>
        </td>
        <td>
            <span id="TableSelector2" style="display:none;">
                <select onchange='javascript:tblSelectionChanged2(this);' />
            </span>
        </td>
    </tr>
    <tr>
        <td>New table name</td>
        <td>
            <asp:TextBox runat="server" ID="txtNewTable2" Width="98%" />
        </td>
        <td>
            <a href="#" onclick='return RenameTable("<%= txtTable2.ClientID %>", "<%= txtNewTable2.ClientID %>");' 
                        title="Rename table">Rename table</a>
        </td>
    </tr>
    <tr>
        <td colspan="3">
            <span runat="server" id="spnRenameTableError2" class="ErrorMessage" style="display: none;" />
            <div runat="server" id="spnRenameTableOK2" class="OkMessage" enableviewstate="false" style="display:none;">
                Table is renamed.
            </div>
        </td>
    </tr>
</table>