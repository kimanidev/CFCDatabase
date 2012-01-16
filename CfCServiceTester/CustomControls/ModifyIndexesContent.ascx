<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ModifyIndexesContent.ascx.cs" Inherits="CfCServiceTester.CustomControls.ModifyIndexesContent" %>

<asp:HiddenField runat="server" ID="hdnSelectedTable4" Value="" />
<asp:HiddenField runat="server" ID="hdnSelectedIndex4" Value="" />

<table style="width:45em;" id="IndexDefinition4">
    <colgroup>
        <col style="width:20%;" />
        <col style="width:30%" />
        <col style="width:50%" />
    </colgroup>
    <tbody>
        <tr>
            <td>Server/Database</td>
            <td>
                <asp:TextBox ID="txtServerName4" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
            </td>
            <td>
                <asp:TextBox ID="txtDatabaseName4" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
            </td>
        </tr>
        <tr>
            <td>Table</td>
            <td>
                <asp:DropDownList ID="lstTableList4" runat="server" OnDataBound="TableList4_OnDataBound" 
                        onchange='return TableListOnChange(this);'/>
            </td>
            <td>
                <asp:Label ID="lblErrorMessage4" runat="server" Text="" CssClass="ErrorMessage" />
            </td>
        </tr>
        <tr>
            <td colspan="3"><hr /></td>
        </tr>

        <tr>
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">Indexes</td>
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">Fields</td>
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">Characteristics</td>
        </tr>

        <tr>
            <td rowspan="7" style="vertical-align: top; padding-left: 5px; padding-top: 5px;">
                <asp:ListBox ID="lstIndexList4" runat="server" OnDataBound="lstIndexList4_OnDataBound" 
                    onchange='return IndexListOnChange(this);' Rows="10" />
            </td>
            <td rowspan="7" style="vertical-align: top; padding-left: 5px; padding-top: 5px;">
                <asp:ListBox ID="lstFieldList4" runat="server" Rows="10" />
            </td>
            <td>
                <asp:CheckBox ID="chkCompactLargeObjects4" runat="server" ToolTip="Specifies whether to compact the large object (LOB) data in the index." 
                        Text="Compact large object" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="chkDisallowPageLocks4" runat="server" ToolTip="Specifies whether the index allows page locks." 
                        Text="Allows page locks" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="chkDisallowRowLocks4" runat="server" ToolTip="Specifies whether the index allows row locks." 
                        Text="Allows page locks" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="txtFillFactor4" runat="server" ToolTip="Percentage of an index page to fill when the index is created or re-created." 
                        Text="" />
                <asp:Label ID="lblFillFactor4" runat="server" AssociatedControlID="txtFillFactor4" Text="Fill factor" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="txtFilterDefinition4" runat="server" ToolTip="String value that contains the definition for the filter." 
                        Text="" />
                <asp:Label ID="lblFilterDefinition4" runat="server" AssociatedControlID="txtFilterDefinition4" Text="Filter definition" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="chkIgnoreDuplicateKeys4" runat="server" ToolTip="Specifies whether the index ignores duplicate keys." 
                        Text="Ignore duplicate keys" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:DropDownList ID="ddlIndexKeyType4" runat="server" ToolTip="index key type.">
                    <asp:ListItem Text="None" Value="None" />
                    <asp:ListItem Text="Primary key" Value="DriPrimaryKey" />
                    <asp:ListItem Text="Unique key" Value="DriUniqueKey" />
                </asp:DropDownList>
            </td>
        </tr>
        <tr class="Pauser">
            <td rowspan="3" colspan="2" style="vertical-align: top; padding-left: 5px; padding-top: 5px;">
                <span class="Pauser" style="display:none;">
                    <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>
            </td>
            <td>
                <asp:CheckBox ID="chkIsClustered4" runat="server" ToolTip="Specifies whether the index is clustered." 
                        Text="Is clustered" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="chkIsDisabled4" runat="server" ToolTip="Specifies whether the index is disabled." 
                        Text="Is disabled" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="chkIsUnique4" runat="server" ToolTip="Specifies whether the index is unique or not." 
                        Text="Is unique" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td colspan="3"><hr /></td>
        </tr>
        <tr>
            <td>New name</td>
            <td>
                <asp:TextBox ID="txtNewName4" runat="server" ToolTip="Enter new name for selected index." Text="" />
            </td>
            <td>
                <asp:Button ID="btnRenameIndex4" runat="server" Text="Rename index" 
                        ToolTip="Click the button for renaming selected index."
                        CssClass="MpsButton" OnClientClick="return RenameIndex(this);" />
            </td>
        </tr>
    </tbody>
</table>
