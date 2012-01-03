Type.registerNamespace('CfcServiceTestManager');

CfcServiceTestManager.CfcComponent = function () {
    CfcServiceTestManager.CfcComponent.initializeBase(this);

    // Internal state variables
    this._txtServerNameId = '';     // Text box with SQL server's name
    this._txtDatabaseNameId = '';   // Text box with Database name
    this._txtLoginNameId = '';      // Text box with login name
    this._txtLoginPasswdId = '';    // Text box with password

    this._spnConnectionErrorId = '';        // Span with error message
    this._spnConnectionOKId = '';           // Span with message: you are connected ...
    this._divLoginTooltip = '';             // Div with reload message

    this._txtBackupDirectoryId = '';        // Text box with backup directory
    this._txtBackupFileNameId = '';         // Text box with file name
    this._spnBackupErrorId = '';            // Span with error message
    this._spnBackupOkId = '';               // Span with OK message
    this._txtServerName1Id = '';            // Text box with SQL server's name (Backup/Restore tab)
    this._txtDatabaseName1Id = '';          // Text box with Database name (Backup/Restore tab)
    this._txtFileName1Id = '';              // Text box with backup file name (Restore part)
    this._spnRestoreOK1Id = '';             // Span with OK message (Restore part)
    this._spnRestoreError1Id = '';          // Span with error message (Restore part)
    this._chkSingleModeId = '';             // Checkbox for switching to single user mode (Common part)

    this._txtServerName2Id = '';            // Text box with SQL server's name (Modify table tab)
    this._txtDatabaseName2Id = '';          // Text box with Database name (Modify table tab)
    this._txtTable2Id = '';                 // Text box with table name (Modify table tab)
    this._txtNewTable2Id = '';              // Text box with new table name (Modify table tab)
    this._spnRenameTableError2Id = '';      // Span with error message (Modify table part)
    this._spnRenameTableOK2Id = '';         // Span with OK message (Modify table part)
    this._chkSingleMode2Id = '';            // Checkbox for switching to single user mode (Modify table part)

    this._txtColumnName3Id = '';            // Column name (Column Edit window)
    this._ddlDatatype3Id = '';              // SQL data type dropdown (Column Edit window)
    this._txtMaximumLength3Id = '';         // Maximum Length (Column Edit window)
    this._txtNumericPrecision3Id = '';      // Numeric Precision (Column Edit window)
    this._txtNumericScale3Id = '';          // Numeric Scale (Column Edit window)
    this._chlColumnProperties3Id = '';      // Column properties (primary key, is null, identity)
    this._txtDefaultValue3Id = '';          // Default value (Column Edit window)
    this._hdnOldFieldName3Id = '';          // Old field name (Column Edit window in Edit mode).

    this._localServersOnly = true;          // true - look for local servers only, false - all available SQL servers
    this._accessibleDatabasesOnly = true;   // true - enumerate accessable databases only, false - all databases on the server

    this._rsaExponent = '';         // RSA parameters: exponent
    this._rsaModulus = '';          //                 Modulus

    this._columnEditor = null;      // Column editor (boxy dialog, object)
    this._userRoles = [];
}
CfcServiceTestManager.CfcComponent.prototype = {
    initialize: function () {
        CfcServiceTestManager.CfcComponent.callBaseMethod(this, 'initialize');
        //        alert("As jau initializuotas");
    },

    // Properties
    get_txtServerNameId: function () {
        return this._txtServerNameId;
    },
    set_txtServerNameId: function (value) {
        this._txtServerNameId = value;
    },

    get_txtDatabaseNameId: function () {
        return this._txtDatabaseNameId;
    },
    set_txtDatabaseNameId: function (value) {
        this._txtDatabaseNameId = value;
    },

    get_txtLoginNameId: function () {
        return this._txtLoginNameId;
    },
    set_txtLoginNameId: function (value) {
        this._txtLoginNameId = value;
    },

    get_txtLoginPasswdId: function () {
        return this._txtLoginPasswdId;
    },
    set_txtLoginPasswdId: function (value) {
        this._txtLoginPasswdId = value;
    },

    get_spnConnectionErrorId: function () {
        return this._spnConnectionErrorId;
    },
    set_spnConnectionErrorId: function (value) {
        this._spnConnectionErrorId = value;
    },

    get_spnConnectionOKId: function () {
        return this._spnConnectionOKId;
    },
    set_spnConnectionOKId: function (value) {
        this._spnConnectionOKId = value;
    },

    get_divLoginTooltip: function () {
        return this._divLoginTooltip;
    },
    set_divLoginTooltip: function (value) {
        this._divLoginTooltip = value;
    },

    get_columnEditor: function () {
        return this._columnEditor;
    },
    set_columnEditor: function (value) {
        this._columnEditor = value;
    },

    get_userRoles: function () {
        return this._userRoles;
    },
    set_userRoles: function (value) {
        this._userRoles = value;
    },

    get_txtBackupDirectoryId: function () {
        return this._txtBackupDirectoryId;
    },
    set_txtBackupDirectoryId: function (value) {
        this._txtBackupDirectoryId = value;
    },

    get_txtBackupFileNameId: function () {
        return this._txtBackupFileNameId;
    },
    set_txtBackupFileNameId: function (value) {
        this._txtBackupFileNameId = value;
    },

    get_spnBackupErrorId: function () {
        return this._spnBackupErrorId;
    },
    set_spnBackupErrorId: function (value) {
        this._spnBackupErrorId = value;
    },

    get_spnBackupOkId: function () {
        return this._spnBackupOkId;
    },
    set_spnBackupOkId: function (value) {
        this._spnBackupOkId = value;
    },

    get_txtServerName1Id: function () {
        return this._txtServerName1Id;
    },
    set_txtServerName1Id: function (value) {
        this._txtServerName1Id = value;
    },

    get_txtDatabaseName1Id: function () {
        return this._txtDatabaseName1Id;
    },
    set_txtDatabaseName1Id: function (value) {
        this._txtDatabaseName1Id = value;
    },

    get_txtFileName1Id: function () {
        return this._txtFileName1Id;
    },
    set_txtFileName1Id: function (value) {
        this._txtFileName1Id = value;
    },

    get_spnRestoreOK1Id: function () {
        return this._spnRestoreOK1Id;
    },
    set_spnRestoreOK1Id: function (value) {
        this._spnRestoreOK1Id = value;
    },

    get_spnRestoreError1Id: function () {
        return this._spnRestoreError1Id;
    },
    set_spnRestoreError1Id: function (value) {
        this._spnRestoreError1Id = value;
    },

    get_chkSingleModeId: function () {
        return this._chkSingleModeId;
    },
    set_chkSingleModeId: function (value) {
        this._chkSingleModeId = value;
    },

    get_txtServerName2Id: function () {
        return this._txtServerName2Id;
    },
    set_txtServerName2Id: function (value) {
        this._txtServerName2Id = value;
    },

    get_txtDatabaseName2Id: function () {
        return this._txtDatabaseName2Id;
    },
    set_txtDatabaseName2Id: function (value) {
        this._txtDatabaseName2Id = value;
    },

    get_txtTable2Id: function () {
        return this._txtTable2Id;
    },
    set_txtTable2Id: function (value) {
        this._txtTable2Id = value;
    },

    get_txtNewTable2Id: function () {
        return this._txtNewTable2Id;
    },
    set_txtNewTable2Id: function (value) {
        this._txtNewTable2Id = value;
    },

    get_spnRenameTableOK2Id: function () {
        return this._spnRenameTableOK2Id;
    },
    set_spnRenameTableOK2Id: function (value) {
        this._spnRenameTableOK2Id = value;
    },

    get_chkSingleMode2Id: function () {
        return this._chkSingleMode2Id;
    },
    set_chkSingleMode2Id: function (value) {
        this._chkSingleMode2Id = value;
    },

    get_txtColumnName3Id: function () {
        return this._txtColumnName3Id;
    },
    set_txtColumnName3Id: function (value) {
        this._txtColumnName3Id = value;
    },

    get_ddlDatatype3Id: function () {
        return this._ddlDatatype3Id;
    },
    set_ddlDatatype3Id: function (value) {
        this._ddlDatatype3Id = value;
    },

    get_txtMaximumLength3Id: function () {
        return this._txtMaximumLength3Id;
    },
    set_txtMaximumLength3Id: function (value) {
        this._txtMaximumLength3Id = value;
    },

    get_txtNumericPrecision3Id: function () {
        return this._txtNumericPrecision3Id;
    },
    set_txtNumericPrecision3Id: function (value) {
        this._txtNumericPrecision3Id = value;
    },

    get_txtNumericScale3Id: function () {
        return this._txtNumericScale3Id;
    },
    set_txtNumericScale3Id: function (value) {
        this._txtNumericScale3Id = value;
    },

    get_chlColumnProperties3Id: function () {
        return this._chlColumnProperties3Id;
    },
    set_chlColumnProperties3Id: function (value) {
        this._chlColumnProperties3Id = value;
    },

    get_hdnOldFieldName3Id: function () {
        return this._hdnOldFieldName3Id;
    },
    set_hdnOldFieldName3Id: function (value) {
        this._hdnOldFieldName3Id = value;
    },

    get_localServersOnly: function () {
        return this._localServersOnly;
    },
    set_localServersOnly: function (value) {
        this._localServersOnly = value;
    },

    get_txtDefaultValue3Id: function () {
        return this._txtDefaultValue3Id;
    },
    set_txtDefaultValue3Id: function (value) {
        this._txtDefaultValue3Id = value;
    },

    get_spnRenameTableError2Id: function () {
        return this._spnRenameTableError2Id;
    },
    set_spnRenameTableError2Id: function (value) {
        this._spnRenameTableError2Id = value;
    },

    get_accessibleDatabasesOnly: function () {
        return this._accessibleDatabasesOnly;
    },
    set_accessibleDatabasesOnly: function (value) {
        this._accessibleDatabasesOnly = value;
    },

    get_rsaExponent: function () {
        return this._rsaExponent;
    },
    set_rsaExponent: function (value) {
        this._rsaExponent = value;
    },

    get_rsaModulus: function () {
        return this._rsaModulus;
    },
    set_rsaModulus: function (value) {
        this._rsaModulus = value;
    }
}

CfcServiceTestManager.CfcComponent.registerClass('CfcServiceTestManager.CfcComponent', Sys.Component);