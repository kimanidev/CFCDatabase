// Insert new Column
function InsertColumn3(insertButton) {
    var manager = $find('CfcTestManager');

    if (!ValidateColumnName(true))
        return false;
    if (!ValidateNumericValues($(insertButton).parents('table.FormattedTableNoBorder').find('tbody')))
        return false;

    var insertRequest = CreateUpdateRequest('Insert');
    CfCServiceTester.WEBservice.CfcWebService.InsertColumn(insertRequest, onSuccess_InsertColumn, onFailure_InsertColumn);
    return false;
}

// Column name is mandatory field and must be unique for new column. 
function ValidateColumnName(insertMode) {
    var manager = $find('CfcTestManager');

    var columnNameControl = $(manager.get_txtColumnName3Id());
    var columnName = columnNameControl.val();
    if (!columnName) {
        alert('Column name is not defined.');
        columnNameControl.focus();
        return false;
    }

    var ColumnNames = $.map($('div#DynamicTable2 table tbody tr td.ColumnName'),
                            function (e) { return $(e).text().toUpperCase(); });
    if (jQuery.inArray(columnName.toUpperCase(), ColumnNames) > -1) {
        if (insertMode) {
            alert('Table contains column "' + columnName + '"');
            return false;
        }
    } else {
        // TODO: renaming column requires unique name too.
        if (!insertMode) {
            alert('Table has no column "' + columnName + '"');
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
    rzlt.OldColumnName = '';    // TODO: write column name here for edit operations
    rzlt.Column = column;
    
    return rzlt;
}

// result is instance of the InsertColumnResponse class
function onSuccess_InsertColumn(result) {
    var manager = $find('CfcTestManager');

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
    }
}

function onFailure_InsertColumn(result) {
    alert(result);
}

