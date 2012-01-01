<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ModifyTableContent.ascx.cs" Inherits="CfCServiceTester.CustomControls.ModifyTableContent" %>
<%@ Register TagPrefix="con" TagName="ColumnEditor" Src="~/CustomControls/ColumnEditBox.ascx" %>

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
    <tr>
        <td colspan="3"><hr /></td>
    </tr>
    <tr>
        <td>
            <a href="#" title="Get information about columns in the table." onclick="return GetColumnInfo(this);">Get columns</a>
        </td>
        <td colspan="2">
            <span id="spnGetColumnsError2" class="ErrorMessage" style="display: none;" />
        </td>
    </tr>
    <tr>
        <td colspan="3">
            <div id="DynamicTable2" style="display:none; width: 100%;" >
                <table style="width: 100%;" class="FormattedTable" >
                    <colgroup>
                        <col style="width:8%" />
                        <col style="width:8%" />
                        <col style="width:30%" />
                        <col style="width:20%" />
                        <col style="width:10%" />
                        <col style="width:8%" />
                        <col style="width:8%" />
                        <col style="width:8%" />
                    </colgroup>
                    <thead>
                        <tr>
                            <th>Key</th>
                            <th>Iden-<br />tity</th>
                            <th>Name</th>
                            <th>Type</th>
                            <th>Length</th>
                            <th>Preci-<br />sion</th>
                            <th>Scale</th>
                            <th>Null</th>
                        </tr>
                    </thead>
                    <tbody>
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="8">
                                <a href="#" onclick="return InsertNewColumn();" title="Append new column">Add column</a>
                            </td>
                        </tr>
                    </tfoot>
                </table>
            </div>
        </td>
    </tr>
</table>

<div id="ColumnEditor2" style="display: none; width: 26em; height:auto;" class='boxy' >
    <con:ColumnEditor ID="ColumnEditorBox2" runat="server" />
</div>