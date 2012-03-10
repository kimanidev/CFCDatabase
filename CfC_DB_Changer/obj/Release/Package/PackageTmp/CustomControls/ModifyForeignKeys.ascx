<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ModifyForeignKeys.ascx.cs" Inherits="CfCServiceTester.CustomControls.ModifyForeignKeys" %>
<%@ Register TagPrefix="con" TagName="fKeyEditor" Src="~/CustomControls/ForeignKeyEditor.ascx" %>

<asp:HiddenField runat="server" ID="hdnSelectedTable6" Value="" />
<asp:HiddenField runat="server" ID="hdnSelectedForeignKey6" Value="" />

<table style="width:45em;" id="FKeyDefinition6">
    <colgroup>
        <col style="width:20%;" />
        <col style="width:30%" />
        <col style="width:50%" />
    </colgroup>
    <tbody id="FKeyInfoFields6">
        <tr>
            <td>Selected Server</td>
            <td colspan="2">
                <asp:TextBox ID="txtServerName6" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
            </td>
            <td>
            </td>
        </tr>
        <tr>
            <td>Selected database</td>
            <td>
                <asp:TextBox ID="txtDatabaseName6" runat="server" Text="" ReadOnly="true" Width="98%" BackColor="#EAF7FB" />
            </td>
            <td>
                <span style="white-space: nowrap" >
                    <span style="padding-right: 5px" >Version:</span>
                    <span style="padding-right: 1em" >
                        <asp:TextBox ID="txtMajorDbVersion6" runat="server" Text='0' Width="4em" ReadOnly="true" 
                                     ToolTip="DB version number: major value" />
                    </span>
                    <asp:TextBox ID="txtMinorDbVersion6" runat="server" Text='0' Width="4em" ReadOnly="true"
                                 ToolTip="DB version number: minor value" />
                </span>
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
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">
                <span runat="server" id="SourceFieldLabel" class="SourceFields">Fields (source)</span>
            </td>
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">
                <span runat="server" id="TargetFieldLabel6" class="TargetFields">Fields (target)</span>
            </td>
        </tr>
        <tr>
            <td style="vertical-align: top; padding-left: 5px; padding-top: 5px;">
                <asp:ListBox ID="lstForeignKeyList6" runat="server" OnDataBound="lstIndexList6_OnDataBound" 
                    onchange='return ForeignKeyListOnChange6(this);' Rows="10" Width="18em" />
            </td>
            <td>
                <asp:ListBox ID="lstSourceColumnList6" runat="server" Rows="10" Width="18em" />
            </td>
            <td>
                <asp:ListBox ID="lstTargetColumnList6" runat="server" Rows="10" Width="18em" />
            </td>
        </tr>
        <tr class="Pauser">
            <td colspan="2">
                <span style="white-space:nowrap;" >
                    <label runat="server" id="lblNewName6" for="txtNewName6">New name</label>
                    <asp:TextBox ID="txtNewName6" runat="server" ToolTip="Enter new name for selected foreign key." 
                                 Text="" Width="19em" />
                    <asp:Button ID="btnRenameFkey6" runat="server" Text="Rename" CausesValidation="false"
                        ToolTip="Click the button for renaming selected foreign key."
                        CssClass="MpsButton" OnClientClick="return RenameForeignKey6(this);" />
               </span>
            </td>
            <td rowspan="2">
                <span class="Pauser" style="display:none;">
                    <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:Button ID="btnCreateFkey6" runat="server" Text="Create" CausesValidation="false"
                        ToolTip="Click the button for creating new foreign key."
                        CssClass="MpsButton" OnClientClick="return CreateForeignKey6(this);" />
                <asp:Button ID="btnModifyFkey6" runat="server" Text="Edit" CausesValidation="false"
                        ToolTip="Click the button for editing selected foreign key."
                        CssClass="MpsButton" OnClientClick="return EditForeignKey6(this);" />
                <asp:Button ID="btnDeleteFkey6" runat="server" Text="Delete" CausesValidation="false"
                        ToolTip="Click the button for deleting selected foreign key."
                        CssClass="MpsButton" OnClientClick="return DeleteForeignKey(this);" />
            </td>
        </tr>
    </tbody>
</table>

<div id="ForeignKeyEditor6" style="display: none; width: 40em; height:auto;" class='boxy' >
    <con:fKeyEditor ID="ForeignKeyEditorBox6" runat="server" />
</div>

<script type="text/javascript">
// <![CDATA[
    var min_majorVerion6 = $('#<%= txtMajorDbVersion6.ClientID %>').val();
    var min_minorVersion6 = $('#<%= txtMinorDbVersion6.ClientID %>').val();
    $('#<%= txtMajorDbVersion6.ClientID %>').spinner({ min: min_majorVerion6, max: 10000, increment: 'fast' });
    $('#<%= txtMinorDbVersion6.ClientID %>').spinner({ min: min_minorVersion6, max: 10000, increment: 'fast' });
// ]]>
</script>
