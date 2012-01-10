// Insert new Column
function InsertColumn3(insertButton) {
    var manager = $find('CfcTestManager');

    if (!ValidateColumnName(true))
        return false;
    if (!ValidateNumericValues($(insertButton).parents('table.FormattedTableNoBorder').find('tbody')))
        return false;

    var insertRequest = CreateUpdateRequest('Insert');
    if (insertRequest.Column.IsNullable && insertRequest.Column.IsPrimaryKey)
        alert('Nullable columns cannot be included into primary key.');
    else
        CfCServiceTester.WEBservice.CfcWebService.InsertColumn(insertRequest, onSuccess_InsertColumn, onFailure_InsertColumn);

    return false;
}

// Rename the column
function RenameColumn3(renameButton) {
    var manager = $find('CfcTestManager');

    var oldName = $(manager.get_hdnOldFieldName3Id()).val();
    var newName = $(manager.get_txtColumnName3Id()).val();
    if (ValidateColumnName(true)) {
        var renameRequest = CreateUpdateRequest('Rename');
        CfCServiceTester.WEBservice.CfcWebService.RenameColumn(renameRequest, onSuccess_RenameColumn, onFailure_InsertColumn);
    }
    return false;
}

// Delete the column
function DeleteColumn3(deleteButton) {
    var manager = $find('CfcTestManager');

    var confirmMessage = String.format("Are you sure to delete the '{0}' column?", $(manager.get_txtColumnName3Id()).val());
    if (!confirm(confirmMessage))
        return false;

    if (ValidateColumnName(false, "Delete column")) {
        var deleteRequest = CreateUpdateRequest('Delete');
        CfCServiceTester.WEBservice.CfcWebService.DeleteColumn(deleteRequest, onSuccess_DeleteColumn, onFailure_InsertColumn);
    }
    return false;
}

// Change type of the column
function EditColumn(editButton) {
    var manager = $find('CfcTestManager');

    if (ValidateColumnName(false, "Update column")) {
        var updateRequest = CreateUpdateRequest('Modify');
        if (updateRequest.Column.IsNullable && updateRequest.Column.IsPrimaryKey)
            alert('Nullable columns cannot be included into primary key.');
        else
            CfCServiceTester.WEBservice.CfcWebService.UpdateColumn(updateRequest, onSuccess_UpdateColumn, onFailure_InsertColumn);
    }
    return false;
}

// Column name is mandatory field and must be unique for new column. 
function ValidateColumnName(insertMode, operation) {
    var manager = $find('CfcTestManager');

    var oldName = $(manager.get_hdnOldFieldName3Id()).val();
    var columnNameControl = $(manager.get_txtColumnName3Id());
    var columnName = columnNameControl.val();
    if (!columnName) {
        alert('Column name is not defined.');
        columnNameControl.focus();
        return false;
    }
    if (insertMode) {
        if (oldName != '' && columnName == oldName) {
            alert('Column was not renamed.');
            columnNameControl.focus();
            return false;
        }
    } else {
        if (oldName != columnName) {
            var errorMessage = String.format("Column name cannot be changed for the '{0}' operation.", operation);
            alert(errorMessage);
            columnNameControl.focus();
            return false;
        }
    }


    // Regex validator for column name: http://stackoverflow.com/questions/4977898/check-for-valid-sql-column-name
    var rg = /^[a-z][a-z\d_]*$/i;
    if (!rg.test(columnName)) {
        alert('Column name contains invalid characters.');
        columnNameControl.focus();
        return false;
    }

    var ColumnNames = $.map($('div#DynamicTable2 table tbody tr td.ColumnName'),
                            function (e) { return $(e).text().toUpperCase(); });
    if (jQuery.inArray(columnName.toUpperCase(), ColumnNames) > -1) {
        if (insertMode) {
            alert('Table contains column "' + columnName + '"');
            columnNameControl.focus();
            return false;
        }
    } else {
        if (!insertMode) {
            alert('Table has no column "' + columnName + '"');
            columnNameControl.focus();
            return false;
        }
    }

    return true;
}

function ValidateNumericValues(tbody) {
    var re = /^\d+$/;
    var rzlt = true;
    
    tbody.find('tr td input.NumericValue').each(function (index) {
        var valueElement = $(this);
        var value = valueElement.val();
        if (value) {
            if (!re.test(value)) {
                rzlt = false;
                var header = valueElement.parents('tr').find('td:first').text();
                alert(header + ' must be numeric value without sign.');
                valueElement.focus();
                return false;
            }
        }
    });

    return rzlt;
}

// The function creates UpdateColumnRequest object
function CreateUpdateRequest(updateMode) {
    var manager = $find('CfcTestManager');

    var column = {};
    column.Name = $(manager.get_txtColumnName3Id()).val();
    column.SqlDataType = $(manager.get_ddlDatatype3Id() + ' :selected').text();
    column.MaximumLength = $(manager.get_txtMaximumLength3Id()).val();
    column.NumericPrecision = $(manager.get_txtNumericPrecision3Id()).val();
    column.NumericScale = $(manager.get_txtNumericScale3Id()).val();
    
    column.IsNullable = false;
    column.IsIdentity = false;
    column.IsPrimaryKey = false;
    $(manager.get_chlColumnProperties3Id() + ' input[type=checkbox]').each(function (index) {
        if (this.checked) {
            switch (index) {
                case 0: 
                    column.IsPrimaryKey = true;
                    break;
                case 1:
                    column.IsIdentity = true;
                    break;
                default:
                    column.IsNullable = true;
                    break;
            }
        }
    });
    column.Default = $(manager.get_txtDefaultValue3Id()).val();

    var rzlt = {};
    rzlt.Operation = updateMode;
    rzlt.Table = $(manager.get_txtTable2Id()).val();
    rzlt.OldColumnName = $(manager.get_hdnOldFieldName3Id()).val();
    rzlt.Column = column;
    rzlt.SingleUserMode = $(manager.get_chkSingleMode2Id()).attr('checked') == 'checked';
    rzlt.DisableDependencies = $('#ColumnEditor2 input#chkDisableDependencies').attr('checked') == 'checked';
    
    $('#ColumnEditor2 span.Pauser').show();
    return rzlt;
}

// result is instance of InsertColumnResponse class
function onSuccess_UpdateColumn(result) {
    var manager = $find('CfcTestManager');
    var whiteSpace = ' ';
    $('#ColumnEditor2 span.Pauser').hide();
    if (result.IsSuccess) {
        var queryString = String.format("div#DynamicTable2 table tbody tr td.ColumnName:contains({0})", result.Column.Name);
        //var currentRow = $(queryString).parent();
        $(queryString).siblings().each(function (index) {
            var curentElement = $(this);
            switch (index) {
                case 0:     // Is primary Key
                    curentElement.text(result.Column.IsPrimaryKey ? '+' : whiteSpace);
                    if (result.Column.IsPrimaryKey)
                        curentElement.parent().attr("class", "primaryKeyRow");
                    break;
                case 1:     // Is identity
                    curentElement.text(result.Column.IsIdentity ? '+' : whiteSpace);
                    break;
                case 2:     // Data type
                    curentElement.text(result.Column.SqlDataType);
                    break;
                case 3:     // Maximum length
                    curentElement.text(result.Column.MaximumLength);
                    break;
                case 4:     // Numeric precision
                    curentElement.text(result.Column.NumericPrecision);
                    break;
                case 5:     // Numeric scale
                    curentElement.text(result.Column.NumericScale);
                    break;
                case 6:     // Is NULL
                    curentElement.text(result.Column.IsNullable ? '+' : whiteSpace);
                    break;
                case 7:     // Default value
                    curentElement.text(result.Column.Default);
                    break;
            }
        });
        // Recreate 'Zebra';
        $('div#DynamicTable2 table tbody tr').each(function (index) {
            var className = index % 2 > 0 ? 'oddRow' : 'eventRow';
            var jqThis = $(this);
            if (jqThis.attr('class') != 'primaryKeyRow')
                jqThis.attr('class', className);
        });

        var boxy = manager.get_columnEditor()
        boxy.hide();
    } else {
        alert(result.ErrorMessage);
        $(manager.get_txtColumnName3Id()).focus();
    }
}

function onSuccess_DeleteColumn(result) {
    var manager = $find('CfcTestManager');
    $('#ColumnEditor2 span.Pauser').hide();

    if (result.IsSuccess) {
        var oldColumnName = $(manager.get_hdnOldFieldName3Id()).val().trim();
        var queryString = String.format("div#DynamicTable2 table tbody tr td.ColumnName:contains({0})", oldColumnName);
        $(queryString).parent().remove();
        
        // Recreate 'Zebra';
        $('div#DynamicTable2 table tbody tr').each(function (index) {
            var className = index % 2 > 0 ? 'oddRow' : 'eventRow';
            var jqThis = $(this);
            if (jqThis.attr('class') != 'primaryKeyRow')
                jqThis.attr('class', className);
        });

        var boxy = manager.get_columnEditor()
        boxy.hide();
    }
    else {
        alert(result.ErrorMessage);
        $(manager.get_txtColumnName3Id()).focus();
    }
}

// result is instance of RenameColumnResponse class.
function onSuccess_RenameColumn(result) {
    var manager = $find('CfcTestManager');
    $('#ColumnEditor2 span.Pauser').hide();

    if (result.IsSuccess) {
        var oldColumnName = $(manager.get_hdnOldFieldName3Id()).val().trim().toUpperCase();
        $('div#DynamicTable2 table tbody tr td.ColumnName a').filter(function () {
            var currentName = $(this).text().trim().toUpperCase();
            return oldColumnName == currentName;
        }).text(result.Column.Name);

        var boxy = manager.get_columnEditor()
        boxy.hide();
    }
    else {
        alert(result.ErrorMessage);
        $(manager.get_txtColumnName3Id()).focus();
    }
}

// result is instance of the InsertColumnResponse class
function onSuccess_InsertColumn(result) {
    var manager = $find('CfcTestManager');
    $('#ColumnEditor2 span.Pauser').hide();

    if (result.IsSuccess) {
        var boxy = manager.get_columnEditor()
        boxy.hide();
        //GetColumnInfo(null);

        var sb = new Sys.StringBuilder();
        var AppendRowToTableDelegate = Function.createDelegate(sb, AppendRowToTable);
        var rowNumber = $('div#DynamicTable2 table tbody tr').length;
        AppendRowToTableDelegate(result.Column, rowNumber, []);
        $('div#DynamicTable2 table tbody:last').append(sb.toString());
    }
    else {
        alert(result.ErrorMessage);
        $(manager.get_txtColumnName3Id()).focus();
    }
}

function onFailure_InsertColumn(result) {
    var manager = $find('CfcTestManager');
    $('#ColumnEditor2 span.Pauser').hide();

    alert(result.get_message());
    $(manager.get_txtColumnName3Id()).focus();
}

