// Radio button state
function BackupActionChanged(button) {
    if (button.checked) {
        if (button.value == 'backup') {
            $('#backupBody').css('display', 'table-row-group');
            $('#restoreBody').css('display', 'none');
        } else if (button.value == 'restore') {
            $('#backupBody').css('display', 'none');
            $('#restoreBody').css('display', 'table-row-group');
        }
    }
    return true;
}

// Backup content of the database
function BackupDatabase(overwrite) {
    var manager = $find('CfcTestManager');

    var overWriteMode = $('#' + overwrite).attr('checked') == 'checked';
    var singleUserMode = $(manager.get_chkSingleModeId()).attr('checked') == 'checked';
    var directory = $(manager.get_txtBackupDirectoryId()).val();
    var file = $(manager.get_txtBackupFileNameId()).val();

    CfCServiceTester.WEBservice.CfcWebService.BackupDatabase(directory, file,
                        overWriteMode, singleUserMode, onSuccess_BackupDatabase, onFailure_BackupDatabase);
    return false;
}

// Get list of available databases on the selected server
function PickDatabases1() {
    var manager = $find('CfcTestManager');

    var serverName = $(manager.get_txtServerName1Id()).val();
    if (!serverName) {
        alert('Server is not selected.')
        return false;
    }
    var template = $(manager.get_txtDatabaseName1Id()).val();
    $('span#SpanSelectDatabase1 span.Magnifier').hide();
    $('span#SpanSelectDatabase1 span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.EnumerateDatabases(
                serverName, template, manager.get_accessibleDatabasesOnly(), onSuccess_PickDatabases1, onFailure_PickDatabases1);
    return false;
}

// Get list of available Files in the backup directory
function SelectBackupFiles1() {
    var manager = $find('CfcTestManager');

    var backupDirectory = $(manager.get_txtBackupDirectoryId()).val();
    if (!backupDirectory) {
        alert('Backup directory not defined.')
        return false;
    }
    var template = $(manager.get_txtBackupFileNameId()).val();
    $('span#SpanSelectBackupFile1 span.Magnifier').hide();
    $('span#SpanSelectBackupFile1 span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.EnumerateBackupFiles(
                backupDirectory, template, onSuccess_EnumerateBackupFiles1, onFailure_EnumerateBackupFiles1);
    return false;
}

// Get list of available backup files.
function PickBackupFiles1() {
    var manager = $find('CfcTestManager');

    var backupDirectory = $(manager.get_txtBackupDirectoryId()).val();
    if (!backupDirectory) {
        alert('Backup directory not defined.')
        return false;
    }
    var template = $(manager.get_txtFileName1Id()).val();
    $('span#SpanSelectFile1 span.Magnifier').hide();
    $('span#SpanSelectFile1 span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.EnumerateBackupFiles(
                backupDirectory, template, onSuccess_EnumerateBackupFiles, onFailure_EnumerateBackupFiles);
    return false;
}

// Restore database
function RestoreDatabase(chkOverwriteId) {
    var manager = $find('CfcTestManager');

    var currentDatabase = $(manager.get_txtCurrentDatabaseName1Id()).val();
    var dbName = $(manager.get_txtDatabaseName1Id()).val();
    if (dbName == currentDatabase) {
        var errMessage = "Active database '" + currentDatabase + "' and restored database '" + dbName + "' must be different.";
        alert(errMessage);
        return false;
    }

    var directory = $(manager.get_txtBackupDirectoryId()).val();
    var fileName = $(manager.get_txtFileName1Id()).val();
    var overWriteMode = $('#' + chkOverwriteId).attr('checked') == 'checked';
    var singleUserMode = $(manager.get_chkSingleModeId()).attr('checked') == 'checked';
    var switchDatabase = $('#chkSwitchDatabase').attr('checked') == 'checked';
    var killUserProcedure = $(manager.get_hdnKillUserProcedureId()).val();

    if (!dbName) {
        alert('Data base name is not defined.');
        return false;
    }
    if (!directory) {
        alert('Backup directory is not defined.');
        return false;
    }
    if (!fileName) {
        alert('Backup file is not defined.');
        return false;
    }

    CfCServiceTester.WEBservice.CfcWebService.RestoreDatabase(dbName, directory, fileName,
                overWriteMode, killUserProcedure, singleUserMode, switchDatabase, onSuccess_RestoreDatabase, onFailure_PickDatabases);

    $('span#RestoreDatabase1 span.Pauser').show();
    return false;
}

function dbSelectionChanged1(selectElement) {
    var manager = $find('CfcTestManager');

    $(manager.get_txtDatabaseName1Id()).val(selectElement.value);
    $('span#DatabaseSelector1').hide();
    $('span#SpanSelectDatabase1 span.Magnifier').show();
}
function dbFileSelectionChanged1(selectElement) {
    var manager = $find('CfcTestManager');

    $(manager.get_txtFileName1Id()).val(selectElement.value);
    $('span#FileSelector1').hide();
    $('span#SpanSelectFile1 span.Magnifier').show();
}
function fileSelectionChanged1a(selectElement) {
    var manager = $find('CfcTestManager');

    $(manager.get_txtBackupFileNameId()).val(selectElement.value);
    $('span#BackupFileSelector1').hide();
    $('span#SpanSelectBackupFile1 span.Magnifier').show();
}

// result is instance of the BackupStatus class
function onSuccess_BackupDatabase(result) {
    var manager = $find('CfcTestManager');

    if (result.IsSuccess) {
        $(manager.get_spnBackupErrorId()).hide();
        $(manager.get_spnBackupOkId()).show();
    }
    else {
        $(manager.get_spnBackupErrorId()).text(result.ErrorMessage);
        $(manager.get_spnBackupErrorId()).show();
        $(manager.get_spnBackupOkId()).hide();
    }
}
function onSuccess_PickDatabases1(result) {
    var manager = $find('CfcTestManager');
    $('span#SpanSelectDatabase1 span.Magnifier').show();
    $('span#SpanSelectDatabase1 span.Pauser').hide();

    var serverName = $(manager.get_txtServerNameId()).val();
    if (result.length < 1) {
        alert("SQL server " + serverName + " contains no available databases.");
    }
    else if (result.length == 1) {
        $(manager.get_txtDatabaseName1Id()).val(result[0]);
    }
    else {
        FillDatabaseDropDown1(result);
    }
}
// result is instance of the EnumerateBackupFilesResponse class.
function onSuccess_EnumerateBackupFiles(result) {
    var manager = $find('CfcTestManager');
    $('span#SpanSelectFile1 span.Magnifier').show();
    $('span#SpanSelectFile1 span.Pauser').hide();

    var backupDirectory = $(manager.get_txtBackupDirectoryId()).val();
    if (result.NameList.length < 1) {
        alert("Directory " + backupDirectory + " contains no available files.");
    }
    else if (result.NameList.length == 1) {
        $(manager.get_txtFileName1Id()).val(result.NameList[0]);
    }
    else {
        FillBackupFilesDropDown1(result.NameList);
    }
}
// result is instance of the EnumerateBackupFilesResponse class.
function onSuccess_EnumerateBackupFiles1(result) {
    var manager = $find('CfcTestManager');
    $('span#SpanSelectBackupFile1 span.Magnifier').show();
    $('span#SpanSelectBackupFile1 span.Pauser').hide();

    var backupDirectory = $(manager.get_txtBackupDirectoryId()).val();
    if (result.NameList.length < 1) {
        alert("Directory " + backupDirectory + " contains no available files.");
    }
    else if (result.NameList.length == 1) {
        $(manager.get_txtBackupFileNameId()).val(result.NameList[0]);
    }
    else {
        FillBackupFilesDropDown1a(result.NameList);
    }
}

// result is instance of the RestoreStatus class
function onSuccess_RestoreDatabase(result) {
    var manager = $find('CfcTestManager');

    $('span#RestoreDatabase1 span.Pauser').hide();
    if (result.IsSuccess) {
        $(manager.get_spnRestoreError1Id()).hide();
        $(manager.get_spnRestoreOK1Id()).show();
        if ($('#chkSwitchDatabase').attr('checked') == 'checked') {
            var newDbName = $(manager.get_txtDatabaseName1Id()).val();
            $(manager.get_txtDatabaseNameId()).val(newDbName);
            $(manager.get_txtCurrentDatabaseName1Id()).val(newDbName);
        }
    }
    else {
        $(manager.get_spnRestoreError1Id()).text(result.ErrorMessage);
        $(manager.get_spnRestoreError1Id()).show();
        $(manager.get_spnRestoreOK1Id()).hide();
    }
}

function FillDatabaseDropDown1(result) {
    var dbCombo = $('span#DatabaseSelector1 select');
    dbCombo.empty();

    var comboHtml = GetOptionString('', 'Select Database');
    for (i = 0; i < result.length; i++) {
        var dbName = result[i].Name;
        comboHtml += GetOptionString(dbName, dbName);
    }
    dbCombo.html(comboHtml);
    $('span#SpanSelectDatabase1 span.Pauser').hide();
    $('span#DatabaseSelector1').show();
}
function FillBackupFilesDropDown1(result) {
    var dbCombo = $('span#FileSelector1 select');
    dbCombo.empty();

    var comboHtml = GetOptionString('', 'Select backup file');
    for (i = 0; i < result.length; i++) {
        var flName = result[i];
        comboHtml += GetOptionString(flName, flName);
    }
    dbCombo.html(comboHtml);
    $('span#FileSelector1').show();
}
function FillBackupFilesDropDown1a(result) {
    var dbCombo = $('span#BackupFileSelector1 select');
    dbCombo.empty();

    var comboHtml = GetOptionString('', 'Select backup file');
    for (i = 0; i < result.length; i++) {
        var flName = result[i];
        comboHtml += GetOptionString(flName, flName);
    }
    dbCombo.html(comboHtml);
    $('span#SpanSelectBackupFile1 span.Magnifier').hide();
    $('span#BackupFileSelector1').show();
}

function onFailure_BackupDatabase(result) {
    alert(result.get_message());
}
function onFailure_PickDatabases1(result) {
    $('span#SpanSelectDatabase1 span.Magnifier').show();
    $('span#SpanSelectDatabase1 span.Pauser').hide();
    alert(result.get_message());
}
function onFailure_EnumerateBackupFiles(result) {
    $('span#SpanSelectDatabase1 span.Magnifier').show();
    $('span#SpanSelectDatabase1 span.Pauser').hide();
    alert(result.get_message());
}
function onFailure_EnumerateBackupFiles1(result) {
    $('span#SpanSelectBackupFile1 span.Magnifier').show();
    $('span#SpanSelectBackupFile1 span.Pauser').hide();
    alert(result.get_message());
}