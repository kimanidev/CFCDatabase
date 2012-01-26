<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ModifyForeignKeys.ascx.cs" Inherits="CfCServiceTester.CustomControls.ModifyForeignKeys" %>

<asp:HiddenField runat="server" ID="hdnSelectedTable6" Value="Employees" />
<asp:HiddenField runat="server" ID="hdnSelectedForeignKey6" Value="" />

<table style="width:45em;" id="FKeyDefinition6">
    <colgroup>
        <col style="width:20%;" />
        <col style="width:30%" />
        <col style="width:50%" />
    </colgroup>
    <tbody id="FKeyInfoFields6">
        <tr>
            <td>Server/Database</td>
            <td>
                <asp:TextBox ID="txtServerName6" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
            </td>
            <td>
                <asp:TextBox ID="txtDatabaseName6" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
            </td>
        </tr>
        <tr>
            <td>Table</td>
            <td>
                <asp:DropDownList ID="lstTableList6" runat="server" OnDataBound="TableList6_OnDataBound" 
                        onchange='return TableListOnChange6(this);'/>
            </td>
            <td>
                <asp:Label ID="lblErrorMessage6" runat="server" Text="" CssClass="ErrorMessage" />
            </td>
        </tr>
        <tr>
            <td colspan="3"><hr /></td>
        </tr>
        <tr>
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">Foreign keys</td>
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">Fields (source)</td>
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">Fields (target)</td>
        </tr>
        <tr>
            <td style="vertical-align: top; padding-left: 5px; padding-top: 5px;">
                <asp:ListBox ID="lstForeignKeyList6" runat="server" OnDataBound="lstIndexList6_OnDataBound" 
                    onchange='return IndexListOnChange(this);' Rows="10" Width="18em" />
            </td>
            <td>
                <asp:ListBox ID="lstSourceColumnList6" runat="server" Rows="10" Width="18em" />
            </td>
            <td>
                <asp:ListBox ID="lstTargetColumnList6" runat="server" Rows="10" Width="18em" />
            </td>
        </tr>
        <tr class="Pauser">
            <td colspan="2">&nbsp;</td>
            <td>
                <span class="Pauser" style="display:none;">
                    <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>
            </td>
        </tr>
    </tbody>
</table>
