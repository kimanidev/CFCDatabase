<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CfCServiceTester._Default" %>

<%@ Register TagPrefix="con" TagName="StartPageContent" Src="~/CustomControls/StartPageContent.ascx" %>
<%@ Register TagPrefix="con" TagName="BackupPageContent" Src="~/CustomControls/DatabaseBackupContent.ascx" %>
<%@ Register TagPrefix="con" TagName="ModifyTablePageContent" Src="~/CustomControls/ModifyTableContent.ascx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>CfC Service Tester</title>
    <link href="Styles/Styles.css" rel="stylesheet" type="text/css"/>
    <link href="Styles/boxy.css" rel="stylesheet" type="text/css"/>
    <link href="Styles/FormattedTable.css" rel="stylesheet" type="text/css"/>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server">
            <Services>
                <asp:ServiceReference Path="~/WEBservice/CfcWebService.asmx" />
            </Services>

            <Scripts>
                <asp:ScriptReference Path="~/Scripts/jquery-1.7.1.min.js" />
                <asp:ScriptReference Path="~/Scripts/ConnectionStepEventHandlers.js" />
                <asp:ScriptReference Path="~/Scripts/BackupStepEventHandlers.js" />
                <asp:ScriptReference Path="~/Scripts/ModifyTableStepEventHandlers.js" />
                <asp:ScriptReference Path="~/Scripts/ColumnEditBoxEventHandler.js" />
                <asp:ScriptReference Path="~/Scripts/jquery.boxy.js" />
                <asp:ScriptReference Path="~/Scripts/RSA.min.js" />

                <asp:ScriptReference Path="~/Scripts/CfcServiceTestManager.js" />
            </Scripts>
        </asp:ScriptManager>
        <asp:Wizard ID="Wizard1" runat="server" HeaderText="Database Processor" Width="60em" 
                    OnActiveStepChanged="Wizard1_OnActiveStepChanged" >
            <WizardSteps>
                <asp:WizardStep runat="server" title="Prepare Connection" ID="ConnectionStep" StepType="Start">
                    <con:StartPageContent ID="StartPageContent" runat="server" />
                </asp:WizardStep>
                <asp:WizardStep runat="server" title="Backup and Restore">
                    <con:BackupPageContent ID="BackupPageContent" runat="server" />
                </asp:WizardStep>
                <asp:WizardStep runat="server" title="Modify table">
                    <con:ModifyTablePageContent ID="ModifyTablePageContent" runat="server" />
                </asp:WizardStep>
            </WizardSteps>
            <navigationbuttonstyle borderwidth="1" width="80" borderstyle="Solid" backcolor="lightgray" /> 
            <headerstyle horizontalalign="Right" font-bold="true" font-size="120%" /> 
            <sidebarstyle backcolor="" borderwidth="0" font-names="Arial" Width="15em" CssClass="WizardSideBar" />
        </asp:Wizard>
    </form>

    <script type="text/javascript">
    // <![CDATA[
        Sys.Application.add_init(pageInit);

        function pageInit() {
            $create(CfcServiceTestManager.CfcComponent,
                    { 'id': 'CfcTestManager',
                        'txtServerNameId': '#<%= (GetFirstPageControlId("txtServerName")) %>',
                        'txtDatabaseNameId': '#<%= (GetFirstPageControlId("txtDatabaseName")) %>',
                        'txtLoginNameId': '#<%= (GetFirstPageControlId("txtLoginName")) %>',
                        'txtLoginPasswdId': '#<%= (GetFirstPageControlId("txtLoginPasswd")) %>',

                        'spnConnectionErrorId': '#<%= (GetFirstPageControlId("spnConnectionError")) %>',
                        'spnConnectionOKId': '#<%= (GetFirstPageControlId("spnConnectionOK")) %>',
                        'divLoginTooltip': '#<%= (GetFirstPageControlId("LoginTooltip")) %>',

                        'txtBackupDirectoryId': '#<%= (GetSecondPageControlId("txtBackupDirectory")) %>',
                        'txtBackupFileNameId': '#<%= (GetSecondPageControlId("txtBackupFileName")) %>',
                        'spnBackupErrorId': '#<%= (GetSecondPageControlId("spnBackupError")) %>',
                        'spnBackupOkId': '#<%= (GetSecondPageControlId("spnBackupOK")) %>',
                        'txtServerName1Id': '#<%= (GetSecondPageControlId("txtServerName1")) %>',
                        'txtDatabaseName1Id': '#<%= (GetSecondPageControlId("txtDatabaseName1")) %>',
                        'txtFileName1Id': '#<%= (GetSecondPageControlId("txtFileName1")) %>',
                        'spnRestoreOK1Id': '#<%= (GetSecondPageControlId("spnRestoreOK1")) %>',
                        'spnRestoreError1Id': '#<%= (GetSecondPageControlId("spnRestoreError1")) %>',
                        'chkSingleModeId': '#<%= (GetSecondPageControlId("chkSingleMode")) %>',

                        'txtServerName2Id': '#<%= (GetThirdPageControlId("txtServerName2")) %>',
                        'txtDatabaseName2Id': '#<%= (GetThirdPageControlId("txtDatabaseName2")) %>',
                        'txtTable2Id': '#<%= (GetThirdPageControlId("txtTable2")) %>',
                        'txtNewTable2Id': '#<%= (GetThirdPageControlId("txtNewTable2")) %>',
                        'spnRenameTableError2Id': '#<%= (GetThirdPageControlId("spnRenameTableError2")) %>',
                        'spnRenameTableOK2Id': '#<%= (GetThirdPageControlId("spnRenameTableOK2")) %>',
                        'chkSingleMode2Id': '#<%= (GetThirdPageControlId("chkSingleMode2")) %>',

                        'txtColumnName3Id': '#<%= (GetEditColumnBoxControlId("txtColumnName3")) %>',
                        'ddlDatatype3Id': '#<%= (GetEditColumnBoxControlId("ddlDatatype3")) %>',
                        'txtMaximumLength3Id': '#<%= (GetEditColumnBoxControlId("txtMaximumLength3")) %>',
                        'txtNumericPrecision3Id': '#<%= (GetEditColumnBoxControlId("txtNumericPrecision3")) %>',
                        'txtNumericScale3Id': '#<%= (GetEditColumnBoxControlId("txtNumericScale3")) %>',
                        'chlColumnProperties3Id': '#<%= (GetEditColumnBoxControlId("chlColumnProperties3")) %>',
                        'txtDefaultValue3Id': '#<%= (GetEditColumnBoxControlId("txtDefaultValue3")) %>',

                        'localServersOnly': "<%= LocalServersOnly %>",
                        'accessibleDatabasesOnly': "<%= AccessibleDatabasesOnly %>",

                        'rsaExponent': "<%= RsaExponent %>",
                        'rsaModulus': "<%= RsaModulus %>"
                    });
        }

    // ]]>
    </script>

    </body>
</html>
