
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

// Rename table
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

// Show column info
function GetColumnInfo(hrefElement) {
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
    var tableName = $(manager.get_txtTable2Id()).val();
    if (!tableName) {
        alert('Table is not selected.')
        return false;
    }

    CfCServiceTester.WEBservice.CfcWebService.EnumerateColumns(serverName, databaseName, tableName,
                                                          onSuccess_EnumerateColumns, onFailure_EnumerateTables);
    return false;
}

function InsertNewColumn() {
    var manager = $find('CfcTestManager');

    var dialog = new Boxy('#ColumnEditor2', 
            { center: true, modal: true, title: "Create new column", draggable: true, fixed: false}); 
    manager.set_columnEditor(dialog);
    dialog.show();
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

// rsult is instance of EnumerateColumnsResponse
function onSuccess_EnumerateColumns(result) {
    var errMessage = $('span#spnGetColumnsError2');
    if (!result.IsSuccess) {
        errMessage.text(result.ErrorMessage);
        errMessage.show();
    }
    else {
        errMessage.hide();

        var aDiv = $('div#DynamicTable2');
        var body = aDiv.find('table tbody');
        body.empty();
        
        var sb = new Sys.StringBuilder();
        Array.forEach(result.Columns, AppendRowToTable, sb);
        var tableBody = sb.toString();
        body.html(tableBody);
        aDiv.show();
    }
}

// Parameters:
//  The element argument is the array element that the function will take action on. Instance of the DataColumnDbo class.
//  The index argument is the index of element, and 
//  The array argument is the array that contains element.
//  Context (this) represents string builder (innerHtl for the body element).
// See http://msdn.microsoft.com/en-us/library/bb397509.aspx
function AppendRowToTable(element, index, array) {
    var className = '';
    if (element.IsPrimaryKey)
        className = 'primaryKeyRow';
    else
        className = index % 2 > 0 ? 'oddRow' : 'eventRow';

    this.append('<tr class="' + className + '"><td>');
    this.append(element.IsPrimaryKey ? '+' : '&nbsp;');
    this.append('</td><td>');
    this.append(element.IsIdentity ? '+' : '&nbsp;');
    this.append('</td><td class="ColumnName">');
    this.append(element.Name);
    this.append('</td><td>');
    this.append(element.SqlDataType);
    this.append('</td><td>');
    this.append(element.MaximumLength > 0 ? element.MaximumLength : '&nbsp;');
    this.append('</td><td>');
    this.append(element.NumericPrecision > 0 ? element.NumericPrecision : '&nbsp;');
    this.append('</td><td>');
    this.append(element.NumericScale > 0 ? element.NumericScale : '&nbsp;');
    this.append('</td><td>');
    this.append(element.IsNullable ? '+' : '&nbsp;');
    this.append('</td></tr>');
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
