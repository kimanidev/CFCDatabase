
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
    CfCServiceTester.WEBservice.CfcWebService.EnumerateTables(onSuccess_EnumerateTables, onFailure_EnumerateTables);
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
    var singleUserMode = false; // $(manager.get_chkSingleMode2Id()).attr('checked') == 'checked';
    $('span#SpanSelectTable2 span.Pauser').show();
    var request = {     // instance of the RenameTableRequest class
        Operation: "Rename",
        CFC_DB_Major_Version: $(manager.get_txtMajorDbVersion2Id()).val(),
        CFC_DB_Minor_Version: $(manager.get_txtMinorDbVersion2Id()).val(),
        Table: newName,
        OldName: oldName,
        SingleUserMode: singleUserMode
    };
    CfCServiceTester.WEBservice.CfcWebService.RenameTable(request, onSuccess_RenameTable, onFailure_EnumerateTables);
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

    CfCServiceTester.WEBservice.CfcWebService.EnumerateColumns(tableName, onSuccess_EnumerateColumns, onFailure_EnumerateTables);
    return false;
}

// Create new table
function CreateNewTable(aButton) {
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
        alert('Table name is not defined.')
        return false;
    }
    var request = {     // Instance of the DbModifyRequest class
        Operation: "Insert",
        CFC_DB_Major_Version: $(manager.get_txtMajorDbVersion2Id()).val(),
        CFC_DB_Minor_Version: $(manager.get_txtMinorDbVersion2Id()).val(),
        Table: tableName
    };
    CfCServiceTester.WEBservice.CfcWebService.CreateTable(request, onSuccess_EnumerateColumns, onFailure_EnumerateTables);
    return false;
}

// Delete table
function DeleteTable(aButton) {
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
        alert('Table name is not defined.')
        return false;
    }
    if (confirm("Are you sure to delete table '" + tableName + "'?")) {
        var request = {     // Instance of the DeleteTableRequest class
            Operation: "Delete",
            CFC_DB_Major_Version: $(manager.get_txtMajorDbVersion2Id()).val(),
            CFC_DB_Minor_Version: $(manager.get_txtMinorDbVersion2Id()).val(),
            Table: tableName,
            disableDependencies: true
        };
        CfCServiceTester.WEBservice.CfcWebService.DeleteTable(request, onSuccess_DeleteTable, onFailure_EnumerateTables);
    }
     return false;
}

function InsertNewColumn() {
    var manager = $find('CfcTestManager');

    var dialog = new Boxy('#ColumnEditor2', 
            { center: true, modal: true, title: "Create new column", draggable: true, fixed: false});

    $('#ColumnEditor2 span.InsertColumn3').show();
    $('#ColumnEditor2 span.EditColumn3').hide();
    $('#ColumnEditor2 span.Pauser').hide();
    $(manager.get_chlColumnProperties3Id() + ' input[type=checkbox]:eq(2)').attr('checked', true);

    manager.set_columnEditor(dialog);
    dialog.show();

    return false;
}

// Edit column. Parameter is link that activated Click event.
function EditCurrentColumn(aLink) {
    var manager = $find('CfcTestManager');

    PrepareDialogFields($(aLink));
    var dialog = new Boxy('#ColumnEditor2',
            { center: true, modal: true, title: "Edit column", draggable: true, fixed: false });

    $('#ColumnEditor2 span.InsertColumn3').hide();
    $('#ColumnEditor2 span.EditColumn3').show();
    $('#ColumnEditor2 span.Pauser').hide();
    manager.set_columnEditor(dialog);
    dialog.show();

    return false;
}

// Edit column. Parameter is wrapped link that activated Click event.
function PrepareDialogFields(aLink) {
    var manager = $find('CfcTestManager');

    var columnName = aLink.text();
    $(manager.get_hdnOldFieldName3Id()).val(columnName);    // siblings() shows all elements in the row exept the link
    $(manager.get_txtColumnName3Id()).val(columnName);

    var rowData = [];
    aLink.parent().siblings().each(function (index) {
        rowData[index] = $(this).text().trim(); 
    });

    // CheckboxList
    var query = manager.get_chlColumnProperties3Id() + ' input[type=checkbox]:';
    $(query + 'checked').removeAttr('checked');
    $(query + 'eq(0)').attr('checked', rowData[0] == '+');
    $(query + 'eq(1)').attr('checked', rowData[1] == '+');
    $(query + 'eq(2)').attr('checked', rowData[6] == '+');
/*
    if (rowData[0] == '+' || rowData[1] == '+') {
        $('#spnModifyColumnType3').hide();
    } else {
        $('#spnModifyColumnType3').show();
    }
*/
    // DropDown List
    query = manager.get_ddlDatatype3Id() + ' option';
    $(query + '[selected]').removeAttr('selected');
    var regex = new RegExp('^' + rowData[2] + '$', "i");
    $(query).filter(function () {
        var value = $(this).attr('value');
        return regex.test(value);
    }).attr('selected', true);

    $(manager.get_txtMaximumLength3Id()).val(rowData[3]);       // Maximum length
    $(manager.get_txtNumericPrecision3Id()).val(rowData[4]);    // Precision
    $(manager.get_txtNumericScale3Id()).val(rowData[5]);        // Scale
    $(manager.get_txtDefaultValue3Id()).val(rowData[7]);        // Default value
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
    $('span#SpanSelectTable2 span.Pauser').hide();

    if (result.IsSuccess) {
        $(manager.get_spnRenameTableError2Id()).hide();
        var newName = $(manager.get_txtNewTable2Id()).val();
        $(manager.get_txtTable2Id()).val(newName);
        
        var message = $(manager.get_spnRenameTableOK2Id());
        message.text(String.format("New name of the table is '{0}'. Total count of records in the table: {1}.", newName, result.RecordCount));
        message.show();
    }
    else {
        var errorSpan = $(manager.get_spnRenameTableError2Id());
        errorSpan.text(result.ErrorMessage);
        errorSpan.show();
        $(manager.get_spnRenameTableOK2Id()).hide();
    }
}

// result is instance of DeleteTableResponse
function onSuccess_DeleteTable(result) {
    var manager = $find('CfcTestManager');
    var errMessage = $('span#spnGetColumnsError2');

    if (!result.IsSuccess) {
        errMessage.text(result.ErrorMessage);
        errMessage.show();
    } else {
        alert(result.ErrorMessage);     // ErrorMessage says: Table 'xxx' is deleted.
        $(manager.get_txtTable2Id()).val('');
        $(manager.get_txtNewTable2Id()).val('');
        $('#DynamicTable2').hide();
    }
}

// result is instance of EnumerateColumnsResponse
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
// Modify function PrepareDialogFields after changing order of elements in the row.
function AppendRowToTable(element, index, array) {
    var className = '';
    if (element.IsPrimaryKey)
        className = 'primaryKeyRow';
    else
        className = index % 2 > 0 ? 'oddRow' : 'eventRow';

    this.append('<tr class="' + className + '"><td>');      // 1 - Primary key flag
    this.append(element.IsPrimaryKey ? '+' : '&nbsp;');
    this.append('</td><td>');                               // 2 - Identity flag
    this.append(element.IsIdentity ? '+' : '&nbsp;');
    this.append('</td><td class="ColumnName">');            // 3 - Column name
    this.append('<a href="#" onclick="return EditCurrentColumn(this);" title="Click the link for editing column">');
    this.append(element.Name);
    this.append('</a></td><td>');                           // 4 - Sql datatype
    this.append(element.SqlDataType);
    this.append('</td><td>');                               // 5 - Max length
    this.append(element.MaximumLength > 0 ? element.MaximumLength : '&nbsp;');
    this.append('</td><td>');                               // 6 - Numeric precision
    this.append(element.NumericPrecision > 0 ? element.NumericPrecision : '&nbsp;');
    this.append('</td><td>');                               // 7 - Numeric scale
    this.append(element.NumericScale > 0 ? element.NumericScale : '&nbsp;');
    this.append('</td><td>');                               // 8 - I nullable flag
    this.append(element.IsNullable ? '+' : '&nbsp;');
    this.append('</td><td>');                               // 9 - Default value
    this.append(element.Default);
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
    $('span#SpanSelectTable2 span.Pauser').hide();
    alert(result.get_message());
}
