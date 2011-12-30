
// Get list of available databases on the selected server
function PickUpTables() {
    var manager = $find('CfcTestManager');

    var serverName = $(manager.get_txtServerName2Id()).val();
    if (!serverName) {
        alert('Server is not selected.')
        return false;
    }
    var databaseName = $(manager.get_txtDatabaseName2Id()).val();
    if (!databaseName) {
        alert('Database is not selected.')
        return false;
    }

    $('span#SpanSelectTable2 span.Magnifier').hide();
    $('span#SpanSelectTable2 span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.EnumerateTables(
                serverName, databaseName, onSuccess_EnumerateTables, onFailure_EnumerateTables);
    return false;
}

function RenameTable(oldNameId, newNameId) {
    var manager = $find('CfcTestManager');

    var oldName = $('#' + oldNameId).val();
    var newName = $('#' + newNameId).val();

    if (!oldName) {
        alert('Table name is not defined.');
        return false;
    }
    if (!newName) {
        alert('New table name is not defined.');
        return false;
    }
    if (oldName == newName) {
        alert('New table name is equal to old one.');
        return false;
    }
    var singleUserMode = $(manager.get_chkSingleMode2Id()).attr('checked') == 'checked';

    CfCServiceTester.WEBservice.CfcWebService.RenameTable(oldName, newName, singleUserMode, 
                                                          onSuccess_RenameTable, onFailure_EnumerateTables);
    return false;
}

// result is instance of DataTableListDbo class.
function onSuccess_EnumerateTables(result) {
    var manager = $find('CfcTestManager');

    var databaseName = $(manager.get_txtDatabaseName2Id()).val();
    if (result.TableNames.length < 1) {
        alert("Data base " + databaseName + " contains no available tables.");
        $('span#SpanSelectTable2 span.Magnifier').show();
        $('span#SpanSelectTable2 span.Pauser').hide();
    }
    else if (result.TableNames.length == 1) {
        $(manager.get_txtTable2Id()).val(result.TableNames[0]);
        $('span#SpanSelectTable2 span.Magnifier').show();
        $('span#SpanSelectTable2 span.Pauser').hide();
    }
    else {
        FillTableDropDown2(result.TableNames);
    }
}

// rsult is instance of RenameTableStatus
function onSuccess_RenameTable(result)
{
    var manager = $find('CfcTestManager');

    if (result.IsSuccess) {
        $(manager.get_spnRenameTableError2Id()).hide();
        var newName = $(manager.get_txtNewTable2Id()).val();
        $(manager.get_txtTable2Id()).val(newName);
        $(manager.get_spnRenameTableOK2Id()).show();
    }
    else {
        var errorSpan = $(manager.get_spnRenameTableError2Id());
        errorSpan.text(result.ErrorMessage);
        errorSpan.show();
        $(manager.get_spnRenameTableOK2Id()).hide();
    }
}
function AfterRenaming() {

}

function FillTableDropDown2(result) {
    var dbCombo = $('span#TableSelector2 select');
    dbCombo.empty();

    var comboHtml = GetOptionString('', 'Select table');
    for (i = 0; i < result.length; i++) {
        var tableName = result[i];
        comboHtml += GetOptionString(tableName, tableName);
    }
    dbCombo.html(comboHtml);
    $('span#SpanSelectTable2 span.Pauser').hide();
    $('span#TableSelector2').show();
}

function tblSelectionChanged2(selectElement) {
    var manager = $find('CfcTestManager');

    $(manager.get_txtTable2Id()).val(selectElement.value);
    var newName = $(manager.get_txtNewTable2Id());
    if (!newName.val())
        newName.val(selectElement.value);
    $('span#TableSelector2').hide();
    $('span#SpanSelectTable2 span.Magnifier').show();
}

function onFailure_EnumerateTables(result) {
    alert(result);
}
