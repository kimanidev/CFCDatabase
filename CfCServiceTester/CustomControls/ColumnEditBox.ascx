<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ColumnEditBox.ascx.cs" Inherits="CfCServiceTester.CustomControls.ColumnEditBox" %>

<table class="FormattedTableNoBorder">
    <colgroup>
        <col style="width:30%" />
        <col style="width:40%" />
        <col style="width:30%" />
    </colgroup>
<%--
    <thead>
        <tr>
            <th colspan="3">Create new column</th>
        </tr>
    </thead>
--%>    
    <tbody>
        <tr>
            <td>Name</td>
            <td>
                <asp:TextBox ID="txtColumnName3" runat="server" ToolTip="Column name" Text="" Width="98%" />
            </td>
            <td rowspan="2">
                <asp:CheckBoxList ID="chlColumnProperties3" runat="server" ToolTip="Check listed boxes for defining column type" >
                    <asp:ListItem Text="Is Primary Key" Value="1" />
                    <asp:ListItem Text="Is Identity" Value="2" />
                    <asp:ListItem Text="Is Nullable" Value="4" />
                </asp:CheckBoxList>
            </td>
        </tr>
        <tr>
            <td>SQL data type</td>
            <td>
                <asp:DropDownList ID="ddlDatatype3" runat="server" ToolTip="Sql data type">
                    <asp:ListItem Text="BigInt" Value="BigInt" />
                    <asp:ListItem Text="Binary" Value="Binary" />
                    <asp:ListItem Text="Bit" Value="Bit" />
                    <asp:ListItem Text="Char" Value="Char" />
                    <asp:ListItem Text="Date" Value="Date" />
                    <asp:ListItem Text="DateTime" Value="DateTime" />
                    <asp:ListItem Text="Decimal" Value="Decimal" />
                    <asp:ListItem Text="Float" Value="Float" />
                    <asp:ListItem Text="Image" Value="Image" />
                    <asp:ListItem Text="Int" Value="Int" Selected="true" />
                    <asp:ListItem Text="Money" Value="Money" />
                    <asp:ListItem Text="NChar" Value="NChar" />
                    <asp:ListItem Text="NText" Value="NText" />
                    <asp:ListItem Text="NVarChar" Value="NVarChar" />
                    <asp:ListItem Text="NVarCharMax" Value="NVarCharMax" />
                    <asp:ListItem Text="Real" Value="Real" />
                    <asp:ListItem Text="SmallDateTime" Value="SmallDateTime" />
                    <asp:ListItem Text="SmallInt" Value="SmallInt" />
                    <asp:ListItem Text="SmallMoney" Value="SmallMoney" />
                    <asp:ListItem Text="Text" Value="Text" />
                    <asp:ListItem Text="Timestamp" Value="Timestamp" />
                    <asp:ListItem Text="TinyInt" Value="TinyInt" />
                    <asp:ListItem Text="UniqueIdentifier" Value="UniqueIdentifier" />
                    <asp:ListItem Text="VarBinary" Value="UniqueIdentifier" />
                    <asp:ListItem Text="VarBinaryMax" Value="VarBinaryMax" />
                    <asp:ListItem Text="VarChar" Value="VarBinaryMax" />
                    <asp:ListItem Text="VarCharMax" Value="VarCharMax" />
                    <asp:ListItem Text="Variant" Value="Variant" />
                    <asp:ListItem Text="Xml" Value="Xml" />
                    <asp:ListItem Text="SysName" Value="SysName" />
                    <asp:ListItem Text="Numeric" Value="Numeric" />
                    <asp:ListItem Text="Time" Value="Time" />
                    <asp:ListItem Text="DateTimeOffset" Value="DateTimeOffset" />
                    <asp:ListItem Text="DateTime2" Value="DateTime2" />
                    <asp:ListItem Text="Geometry" Value="Geometry" />
                    <asp:ListItem Text="Geography" Value="Geography" />
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Maximum Length</td>
            <td colspan="2">
                <asp:TextBox ID="txtMaximumLength3" runat="server" ToolTip="Gets the maximum length of the data type." 
                             Text="" CssClass="NumericValue" />
            </td>
        </tr>
        <tr>
            <td>Numeric Precision</td>
            <td colspan="2">
                <asp:TextBox ID="txtNumericPrecision3" runat="server" ToolTip="Gets the maximum length of the data type." 
                             Text="" CssClass="NumericValue" />
            </td>
        </tr>
        <tr>
            <td>Numeric Scale</td>
            <td colspan="2">
                <asp:TextBox ID="txtNumericScale3" runat="server" ToolTip="Gets or sets the numeric scale of the data type." 
                             Text="" CssClass="NumericValue" />
            </td>
        </tr>
        <tr>
            <td>Default value</td>
            <td colspan="2">
                <asp:TextBox ID="txtDefaultValue3" runat="server" ToolTip="Default value of the column." Text="" Width="98%" />
            </td>
        </tr>
    </tbody>
    <tfoot>
        <tr>
            <td colspan="3">
                <span class="InsertColumn3" style="display:inline-block;" >
                    <asp:Button ID="btnInsertColumn3" runat="server" OnClientClick="return InsertColumn3(this);" Text="Insert" 
                                ToolTip="Insert new column" CausesValidation="true" />
                </span>
                <span class="EditColumn3" style="display:none; white-space:nowrap;" >
                    <asp:Button ID="btnDeleteColumn3" runat="server" OnClientClick="return DeleteColumn(this);" Text="Delete" 
                                ToolTip="Delete the column" />
                    <asp:Button ID="btnEditColumn3" runat="server" OnClientClick="return EditColumn(this);" Text="Update" 
                                ToolTip="Update the column" />
                </span>
            </td>
        </tr>
    </tfoot>
</table>