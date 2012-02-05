<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForeignKeyEditor.ascx.cs" Inherits="CfCServiceTester.CustomControls.ForeignKeyEditor" %>

<asp:HiddenField ID="hdnOperationType7" runat="server" Value="" />

<table class="FormattedTableNoBorder" style="width:100%; padding:5px;" >
    <colgroup>
        <col style="width: 40%;" />
        <col style="width: 30%;" />
        <col style="width: 30%;" />
    </colgroup>
    <thead>
        <tr>
            <th>Foreign key name</th>
            <th>Source table</th>
            <th>Target table</th>
        </tr>
        <tr>
            <td>
                <asp:TextBox runat="server" ID="txtFkeyName7" Width="98%" ToolTip="Name of the foreign key." />
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtSourceTblName7" Width="98%"  ToolTip="Name of source table." Enabled="False" />
            </td>
            <td>
                <asp:DropDownList ID="ddlTargetTblName7" runat="server" Width="98%" ToolTip="Name of target table."
                                  onchange='return TargetTableListOnChange7(this);' />
            </td>
        </tr>
        <tr>
            <td style="text-align:right; padding-right:1em;">Update and delete action</td>
            <td colspan="2">
                <select id="ddlUpdateAction7" title="Action that is performed on target table after updating source value">
                    <option value="NoAction">No action</option>
                    <option value="Cascade">Cascade updating</option>
                    <option value="SetNull">Set NULL</option>
                    <option value="SetDefault">Set default value</option>
                </select>
                <select id="ddlDeleteAction7" title="Action that is performed on target table after deleting source value">
                    <option value="NoAction">No action</option>
                    <option value="Cascade">Cascade updating</option>
                    <option value="SetNull">Set NULL</option>
                    <option value="SetDefault">Set default value</option>
                </select>
            </td>
        </tr>
        <tr>
            <td rowspan="2">
                <span id="spnAddFkeyColumn7" style="white-space:nowrap" >
                    <asp:Button ID="btnAddFkeyColumn7" runat="server" Text="Add column" CausesValidation="false"
                            ToolTip="Click the button for including this column into the foreign key." Width="9em"
                            CssClass="MpsButton" OnClientClick="return AddColumn7(this);" />
                    <span class="Pauser" style="display:none; padding-left:2ex;">
                        <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                    </span>
                </span>
            </td>
            <td colspan="2">Select source column, target column and click 'Add column'</td>
        </tr>
        <tr>
            <td>
                <asp:DropDownList ID="ddlSourceColumns7" runat="server" Width="98%" />
            </td>
            <td>
                <asp:DropDownList ID="ddlTargetColumns7" runat="server" Width="98%" />
            </td>
        </tr>
        <tr>
            <td colspan="3"><hr /></td>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td rowspan="2" style="vertical-align:bottom;">
                <asp:Button ID="spnRemoveFkeyColumn7" runat="server" Text="Delete column" CausesValidation="false"
                        ToolTip="Click the button for removing selected column." Width="9em"
                        CssClass="MpsButton" OnClientClick="return RemoveColumn7(this);" />
            </td>
            <th>Source columns</th>
            <th>Target columns</th>
        </tr>
        <tr>
            <td>
                <asp:ListBox ID="lbxSourceColumns7" runat="server" Width="98%" Height="12ex" 
                             onchange="return SelectionChanged7(this);" />
            </td>
            <td>
                <asp:ListBox ID="lbxTargetColumns7" runat="server" Width="98%" Height="12ex"
                             onchange="return SelectionChanged7(this);" />
            </td>
        </tr>
    </tbody>
    <tfoot>
        <tr>
            <td colspan="3"><hr /></td>
        </tr>
        <tr>
            <td>
                <span style="white-space:nowrap;">
                    <input type="checkbox" id="chkCheckAfterConstruction" checked="checked" 
                           title="Verify values in the source table after constructing the key." />
                    <label for="chkCheckAfterConstruction">Check values after construction</label>
                </span>
            </td>
            <td colspan="2" style="text-align:right;">
                <asp:Button ID="btnCreateForeignKey" runat="server" Text="Create key" CausesValidation="false"
                        ToolTip="Click the button for creating foreign key." Width="9em"
                        CssClass="MpsButton" OnClientClick="return CreateThisForeignKey7(this);" />
            </td>
        </tr>
    </tfoot>
</table>
