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
        <td>
            <asp:TextBox ID="txtDatabaseName2" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
        </td>
        <td>
            <span style="white-space: nowrap" >
                <span style="padding-right: 5px" >Version:</span>
                <span style="padding-right: 1em" >
                    <asp:TextBox ID="txtMajorDbVersion2" runat="server" Text='0' Width="4em" ReadOnly="true" 
                                 ToolTip="DB version number: major value" />
                </span>
                <asp:TextBox ID="txtMinorDbVersion2" runat="server" Text='0' Width="4em" ReadOnly="true"
                             ToolTip="DB version number: minor value" />
            </span>
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
        <td colspan="3">
            <span id="spnGetColumnsError2" class="ErrorMessage" style="display: none;" />
        </td>
    </tr>
    <tr>
        <td colspan="3" style="white-space: nowrap;">
            <asp:Button ID="btnCreateTable" runat="server" Text="Create table" ToolTip="Create new data table and modify it."
                        CssClass="MpsButton" OnClientClick="return CreateNewTable(this);" />
            <asp:Button ID="btnEditTable" runat="server" Text="Edit table" ToolTip="Display structure of the table and modify it."
                        CssClass="MpsButton" OnClientClick="return GetColumnInfo(this);" />
            <asp:Button ID="btnDeleteTable" runat="server" Text="Delete table" ToolTip="Delete the table."
                        CssClass="MpsButton" OnClientClick="return DeleteTable(this);" />
        </td>
    </tr>
    <tr>
        <td colspan="3">
            <div id="DynamicTable2" style="display:none; width: 100%;" >
                <table style="width: 100%;" class="FormattedTable" >
                    <colgroup>
                        <col style="width:8%" />
                        <col style="width:8%" />
                        <col style="width:23%" />
                        <col style="width:13%" />
                        <col style="width:10%" />
                        <col style="width:8%" />
                        <col style="width:8%" />
                        <col style="width:8%" />
                        <col style="width:14%" />
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
                            <th>Default<br />Value</th>
                        </tr>
                    </thead>
                    <tbody>
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="9">
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

<script type="text/javascript">
// <![CDATA[
    var min_majorVerion1 = $('#<%= txtMajorDbVersion2.ClientID %>').val();
    var min_minorVersion1 = $('#<%= txtMinorDbVersion2.ClientID %>').val();
    $('#<%= txtMajorDbVersion2.ClientID %>').spinner({ min: min_majorVerion1, max: 10000, increment: 'fast' });
    $('#<%= txtMinorDbVersion2.ClientID %>').spinner({ min: min_minorVersion1, max: 10000, increment: 'fast' });
// ]]>
</script>
