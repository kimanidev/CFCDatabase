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
    this._txtCurrentDatabaseName1Id = '';   // Text box with name of current database (Backup/Restore tab)
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
    this._txtDefaultValue3Id = '';          // Default value (Column Edit window);

    this._hdnSelectedTable4Id = '';         // Selected table
    this._hdnSelectedIndex4Id = '';         // Selected index
    this._lstTableList4Id = '';             // Selected table name
    this._lstIndexList4Id = '';             // Selected index name
    this._lstFieldList4Id = '';             // Fields in the index
    this._chkCompactLargeObjects4Id = '';   // CompactLargeObjects
    this._chkDisallowPageLocks4Id = '';     // Disallow Page Locks
    this._chkDisallowRowLocks4Id = '';      // Disallow Row Locks
    this._txtFillFactor4Id = '';            // Fill Factor
    this._txtFilterDefinition4Id = '';      // Filter definition
    this._chkIgnoreDuplicateKeys4Id = '';   // Ignore dublicate keys
    this._ddlIndexKeyType4Id = ''           // Key type
    this._chkIsClustered4Id = ''            // Key is clustered
    this._chkIsDisabled4Id = ''             // Key is disabled
    this._chkIsUnique4Id = ''               // Key is unique
    this._txtNewName4Id = '';               // New index name

    this._txtName5Id = '';                  // Index name (Index edit box)
    this._chkCompactLargeObjects5Id = '';   // Compact large objects
    this._chkDisallowPageLocks5Id = '';     // Disallow Page Locks
    this._chkDisallowRowLocks5Id = '';      // Disallow Row Locks
    this._txtFillFactor5Id = '';            // Fill Factor
    this._txtFilterDefinition5Id = '';      // Filter definition
    this._chkIgnoreDuplicateKeys5Id = '';   // Ignore dublicate keys
    this._ddlIndexKeyType5Id = '';          // Key type
    this._chkIsClustered5Id = '';           // Key is clustered
    this._chkIsUnique5Id = '';              // Key is unique
    this._hdIndexOperation5Id = '';         // Index operation code

    this._hdnSelectedTable6Id = '';         // Current table
    this._hdnSelectedForeignKey6Id = '';    // Selected foreign key
    this._lstTableList6Id = '';             // Selected table
    this._lstForeignKeyList6Id = '';        // Selected foreign key
    this._lstSourceColumnList6Id = '';      // Source columns
    this._lstTargetColumnList6Id = '';      // Target columns
    this._btnRenameFkey6Id = '';            // Rename button
    this._btnCreateFkey6Id = '';            // Create button
    this._btnModifyFkey6Id = '';            // Modify button
    this._btnDeleteFkey6Id = '';            // Delete button
    this._txtNewName6Id = '';               // New name for the foreign key

    this._txtFkeyName7Id = ''               // Foreign key name
    this._txtSourceTblName7Id = '';         // Source table name
    this._ddlTargetTblName7Id = '';         // Target table name
    this._lbxSourceColumns7Id = '';         // Source columns
    this._lbxTargetColumns7Id = '';         // Target columns
    this._ddlSourceColumns7Id = '';         // Source columns
    this._ddlTargetColumns7Id = '';         // Target columns

    this._localServersOnly = true;          // true - look for local servers only, false - all available SQL servers
    this._accessibleDatabasesOnly = true;   // true - enumerate accessable databases only, false - all databases on the server

    this._rsaExponent = '';         // RSA parameters: exponent
    this._rsaModulus = '';          //                 Modulus

    this._columnEditor = null;      // Column editor (boxy dialog, object)
    this._userRoles = [];

    this._suppressEvents = false;   // Set this field to true for suppressing child events (when content of the list is changed).
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

    get_txtCurrentDatabaseName1Id: function () {
        return this._txtCurrentDatabaseName1Id;
    },
    set_txtCurrentDatabaseName1Id: function (value) {
        this._txtCurrentDatabaseName1Id = value;
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

    get_txtDefaultValue3Id: function () {
        return this._txtDefaultValue3Id;
    },
    set_txtDefaultValue3Id: function (value) {
        this._txtDefaultValue3Id = value;
    },

    get_hdnSelectedTable4Id: function () {
        return this._hdnSelectedTable4Id;
    },
    set_hdnSelectedTable4Id: function (value) {
        this._hdnSelectedTable4Id = value;
    },

    get_hdnSelectedIndex4Id: function () {
        return this._hdnSelectedIndex4Id;
    },
    set_hdnSelectedIndex4Id: function (value) {
        this._hdnSelectedIndex4Id = value;
    },

    get_lstTableList4Id: function () {
        return this._lstTableList4Id;
    },
    set_lstTableList4Id: function (value) {
        this._lstTableList4Id = value;
    },

    get_lstIndexList4Id: function () {
        return this._lstIndexList4Id;
    },
    set_lstIndexList4Id: function (value) {
        this._lstIndexList4Id = value;
    },

    get_lstFieldList4Id: function () {
        return this._lstFieldList4Id;
    },
    set_lstFieldList4Id: function (value) {
        this._lstFieldList4Id = value;
    },

    get_chkCompactLargeObjects4Id: function () {
        return this._chkCompactLargeObjects4Id;
    },
    set_chkCompactLargeObjects4Id: function (value) {
        this._chkCompactLargeObjects4Id = value;
    },

    get_chkDisallowPageLocks4Id: function () {
        return this._chkDisallowPageLocks4Id;
    },
    set_chkDisallowPageLocks4Id: function (value) {
        this._chkDisallowPageLocks4Id = value;
    },

    get_chkDisallowRowLocks4Id: function () {
        return this._chkDisallowRowLocks4Id;
    },
    set_chkDisallowRowLocks4Id: function (value) {
        this._chkDisallowRowLocks4Id = value;
    },

    get_txtFillFactor4Id: function () {
        return this._txtFillFactor4Id;
    },
    set_txtFillFactor4Id: function (value) {
        this._txtFillFactor4Id = value;
    },

    get_txtFilterDefinition4Id: function () {
        return this._txtFilterDefinition4Id;
    },
    set_txtFilterDefinition4Id: function (value) {
        this._txtFilterDefinition4Id = value;
    },

    get_chkIgnoreDuplicateKeys4Id: function () {
        return this._chkIgnoreDuplicateKeys4Id;
    },
    set_chkIgnoreDuplicateKeys4Id: function (value) {
        this._chkIgnoreDuplicateKeys4Id = value;
    },

    get_ddlIndexKeyType4Id: function () {
        return this._ddlIndexKeyType4Id;
    },
    set_ddlIndexKeyType4Id: function (value) {
        this._ddlIndexKeyType4Id = value;
    },

    get_chkIsClustered4Id: function () {
        return this._chkIsClustered4Id;
    },
    set_chkIsClustered4Id: function (value) {
        this._chkIsClustered4Id = value;
    },

    get_chkIsDisabled4Id: function () {
        return this._chkIsDisabled4Id;
    },
    set_chkIsDisabled4Id: function (value) {
        this._chkIsDisabled4Id = value;
    },

    get_chkIsUnique4Id: function () {
        return this._chkIsUnique4Id;
    },
    set_chkIsUnique4Id: function (value) {
        this._chkIsUnique4Id = value;
    },

    get_txtNewName4Id: function () {
        return this._txtNewName4Id;
    },
    set_txtNewName4Id: function (value) {
        this._txtNewName4Id = value;
    },


    get_txtName5Id: function () {
        return this._txtName5Id;
    },
    set_txtName5Id: function (value) {
        this._txtName5Id = value;
    },

    get_chkCompactLargeObjects5Id: function () {
        return this._chkCompactLargeObjects5Id;
    },
    set_chkCompactLargeObjects5Id: function (value) {
        this._chkCompactLargeObjects5Id = value;
    },

    get_chkDisallowPageLocks5Id: function () {
        return this._chkDisallowPageLocks5Id;
    },
    set_chkDisallowPageLocks5Id: function (value) {
        this._chkDisallowPageLocks5Id = value;
    },
    get_chkDisallowRowLocks5Id: function () {
        return this._chkDisallowRowLocks5Id;
    },
    set_chkDisallowRowLocks5Id: function (value) {
        this._chkDisallowRowLocks5Id = value;
    },
    get_txtFillFactor5Id: function () {
        return this._txtFillFactor5Id;
    },
    set_txtFillFactor5Id: function (value) {
        this._txtFillFactor5Id = value;
    },
    get_txtFilterDefinition5Id: function () {
        return this._txtFilterDefinition5Id;
    },
    set_txtFilterDefinition5Id: function (value) {
        this._txtFilterDefinition5Id = value;
    },

    get_chkIgnoreDuplicateKeys5Id: function () {
        return this._chkIgnoreDuplicateKeys5Id;
    },
    set_chkIgnoreDuplicateKeys5Id: function (value) {
        this._chkIgnoreDuplicateKeys5Id = value;
    },

    get_ddlIndexKeyType5Id: function () {
        return this._ddlIndexKeyType5Id;
    },
    set_ddlIndexKeyType5Id: function (value) {
        this._ddlIndexKeyType5Id = value;
    },

    get_chkIsClustered5Id: function () {
        return this._chkIsClustered5Id;
    },
    set_chkIsClustered5Id: function (value) {
        this._chkIsClustered5Id = value;
    },
    /*
    get_chkIsDisabled5Id: function () {
    return this._chkIsDisabled5Id;
    },
    set_chkIsDisabled5Id: function (value) {
    this._chkIsDisabled5Id = value;
    },
    */
    get_chkIsUnique5Id: function () {
        return this._chkIsUnique5Id;
    },
    set_chkIsUnique5Id: function (value) {
        this._chkIsUnique5Id = value;
    },

    get_hdIndexOperation5Id: function () {
        return this._hdIndexOperation5Id;
    },
    set_hdIndexOperation5Id: function (value) {
        this._hdIndexOperation5Id = value;
    },

    get_hdnSelectedTable6Id: function () {
        return this._hdnSelectedTable6Id;
    },
    set_hdnSelectedTable6Id: function (value) {
        this._hdnSelectedTable6Id = value;
    },

    get_hdnSelectedForeignKey6Id: function () {
        return this._hdnSelectedForeignKey6Id;
    },
    set_hdnSelectedForeignKey6Id: function (value) {
        this._hdnSelectedForeignKey6Id = value;
    },

    get_lstTableList6Id: function () {
        return this._lstTableList6Id;
    },
    set_lstTableList6Id: function (value) {
        this._lstTableList6Id = value;
    },

    get_lstForeignKeyList6Id: function () {
        return this._lstForeignKeyList6Id;
    },
    set_lstForeignKeyList6Id: function (value) {
        this._lstForeignKeyList6Id = value;
    },

    get_lstSourceColumnList6Id: function () {
        return this._lstSourceColumnList6Id;
    },
    set_lstSourceColumnList6Id: function (value) {
        this._lstSourceColumnList6Id = value;
    },

    get_lstTargetColumnList6Id: function () {
        return this._lstTargetColumnList6Id;
    },
    set_lstTargetColumnList6Id: function (value) {
        this._lstTargetColumnList6Id = value;
    },

    get_btnRenameFkey6Id: function () {
        return this._btnRenameFkey6Id;
    },
    set_btnRenameFkey6Id: function (value) {
        this._btnRenameFkey6Id = value;
    },

    get_btnCreateFkey6Id: function () {
        return this._btnCreateFkey6Id;
    },
    set_btnCreateFkey6Id: function (value) {
        this._btnCreateFkey6Id = value;
    },

    get_btnModifyFkey6Id: function () {
        return this._btnModifyFkey6Id;
    },
    set_btnModifyFkey6Id: function (value) {
        this._btnModifyFkey6Id = value;
    },

    get_btnDeleteFkey6Id: function () {
        return this._btnDeleteFkey6Id;
    },
    set_btnDeleteFkey6Id: function (value) {
        this._btnDeleteFkey6Id = value;
    },

    get_txtNewName6Id: function () {
        return this._txtNewName6Id;
    },
    set_txtNewName6Id: function (value) {
        this._txtNewName6Id = value;
    },

    get_txtFkeyName7Id: function () {
        return this._txtFkeyName7Id;
    },
    set_txtFkeyName7Id: function (value) {
        this._txtFkeyName7Id = value;
    },

    get_txtSourceTblName7Id: function() {
        return this._txtSourceTblName7Id;
    },
    set_txtSourceTblName7Id: function(value) {
        this._txtSourceTblName7Id = value;
    },

    get_ddlTargetTblName7Id: function() {
        return this._ddlTargetTblName7Id;
    },
    set_ddlTargetTblName7Id: function(value) {
        this._ddlTargetTblName7Id = value;
    },

    get_lbxSourceColumns7Id: function() {
        return this._lbxSourceColumns7Id;
    },
    set_lbxSourceColumns7Id: function(value) {
        this._lbxSourceColumns7Id = value;
    },

    get_lbxTargetColumns7Id: function() {
        return this._lbxTargetColumns7Id;
    },
    set_lbxTargetColumns7Id: function(value) {
        this._lbxTargetColumns7Id = value;
    },

    get_ddlSourceColumns7Id: function() {
        return this._ddlSourceColumns7Id;
    },
    set_ddlSourceColumns7Id: function(value) {
        this._ddlSourceColumns7Id = value;
    },

    get_ddlTargetColumns7Id: function() {
        return this._ddlTargetColumns7Id;
    },
    set_ddlTargetColumns7Id: function(value) {
        this._ddlTargetColumns7Id = value;
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