<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IndexEditorBox.ascx.cs" Inherits="CfCServiceTester.CustomControls.IndexEditorBox" %>

<asp:HiddenField runat="server" ID="hdIndexOperation5" Value="" />

<table class="FormattedTableNoBorder" style="width:100%; padding:5px;" >
    <colgroup>
        <col style="width: 40%;" />
        <col style="width: 60%;" />
    </colgroup>
    <thead>
        <tr>
            <td style="text-align:right; padding-right: 1em;">Index name</td>
            <td>
                <asp:TextBox runat="server" ID="txtName5" Width="98%" ToolTip="Name of the index." />
            </td>
        </tr>
        <tr>
            <td colspan="3"><hr /></td>
        </tr>
        <tr>
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">Fields</td>
            <td style="text-align: left; padding-left: 10px; font-weight:bolder;">Characteristics</td>
        </tr>
        <tr>
            <td colspan="2"><hr /></td>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td rowspan="9" style="vertical-align: top; padding-left: 1em; padding-top: 1ex;">
                <div id="IndexFieldList" class="FieldList">
                    <input type="checkbox" value="fld1" />fld1<br />
                    <input type="checkbox" value="fld2" />fld2<br />
                </div>
            </td>
            <td>
                <asp:CheckBox ID="chkCompactLargeObjects5" runat="server" ToolTip="Specifies whether to compact the large object (LOB) data in the index." 
                        Text="Compact large object" TextAlign="Right" />
            </td>            
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="chkDisallowPageLocks5" runat="server" ToolTip="Specifies whether the index allows page locks." 
                        Text="Forbids page locks" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="chkDisallowRowLocks5" runat="server" ToolTip="Specifies whether the index allows row locks." 
                        Text="Forbids row locks" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="txtFillFactor5" runat="server" ToolTip="Percentage of an index page to fill when the index is created or re-created." 
                        Text="" />
                <asp:Label ID="lblFillFactor5" runat="server" AssociatedControlID="txtFillFactor5" Text="Fill factor" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="txtFilterDefinition5" runat="server" ToolTip="String value that contains the definition for the filter." 
                        Text="" />
                <asp:Label ID="lblFilterDefinition5" runat="server" AssociatedControlID="txtFilterDefinition5" Text="Filter definition" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="chkIgnoreDuplicateKeys5" runat="server" ToolTip="Specifies whether the index ignores duplicate keys." 
                        Text="Ignore duplicate keys" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:DropDownList ID="ddlIndexKeyType5" runat="server" ToolTip="index key type.">
                    <asp:ListItem Text="None" Value="None" />
                    <asp:ListItem Text="Primary key" Value="DriPrimaryKey" />
                    <asp:ListItem Text="Unique key" Value="DriUniqueKey" />
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="chkIsClustered5" runat="server" ToolTip="Specifies whether the index is clustered." 
                        Text="Is clustered" TextAlign="Right" />
            </td>
        </tr>
<%--
        <tr>
            <td>
                <asp:CheckBox ID="chkIsDisabled5" runat="server" ToolTip="Specifies whether the index is disabled." 
                        Text="Is disabled" TextAlign="Right" />
            </td>
        </tr>
--%>        
        <tr>
            <td>
                <asp:CheckBox ID="chkIsUnique5" runat="server" ToolTip="Specifies whether the index is unique or not." 
                        Text="Is unique" TextAlign="Right" />
            </td>
        </tr>
        <tr>
            <td colspan="3"><hr /></td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:Button ID="btnSaveIndex5" runat="server" Text="Save" 
                        ToolTip="Click the button for creating new index."
                        CssClass="MpsButton" OnClientClick="return SaveIndex(this);" />
                <span id="CreateIndexDialogPauser" class="Pauser" style="display:none; padding-left: 3em;" >
                    <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                </span>

            </td>
        </tr>
    </tbody>
</table>
