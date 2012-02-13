// Another table was selected
function TableListOnChange6(dropDown) {
    var manager = $find('CfcTestManager');

    var tableName = $(dropDown).val();
    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').show();
    $(manager.get_hdnSelectedTable6Id()).val(tableName);
    $('table#FKeyDefinition6 tbody#FKeyInfoFields6 tr td span.SourceFields').text(String.format("Fields ({0})", tableName));
    CfCServiceTester.WEBservice.CfcWebService.EnumerateForeignKeys(tableName, onSuccess_EnumerateForeignKeys, onFailure_EnumerateForeignKeys);

    return false;
}

// Another index was selected
function ForeignKeyListOnChange6(aList) {
    var manager = $find('CfcTestManager');
    var tableName = $(manager.get_hdnSelectedTable6Id()).val();
    var fKeyName = $(manager.get_lstForeignKeyList6Id() + ' option:selected').val();
    $(manager.get_hdnSelectedForeignKey6Id()).val(fKeyName);

    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.GetForeignKeyDescription(tableName, fKeyName, onSuccess_GetForeignKey, onFailure_EnumerateForeignKeys);

    return false;
}

// Rename foreign key
function RenameForeignKey6(button) {
    var manager = $find('CfcTestManager');

    var tableName = $(manager.get_hdnSelectedTable6Id()).val();
    if (!tableName) {
        alert('Table name is not defined.');
        return false;
    }
    var fKeyName = $(manager.get_hdnSelectedForeignKey6Id()).val();
    if (!fKeyName) {
        alert('Foreign key is not selected.');
        return false;
    }
    var newForeignKeyName = $(manager.get_txtNewName6Id()).val();
    if (!newForeignKeyName || newForeignKeyName == fKeyName) {
        alert('New name for the foreign key is not defined.');
        return false;
    }

    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').show();
    var request = {     // Object is instanced of UpdateForeignKeyRequest class
        Operation: 'Rename',
        CFC_DB_Major_Version: $(manager.get_txtMajorDbVersion6Id()).val(),
        CFC_DB_Minor_Version: $(manager.get_txtMinorDbVersion6Id()).val(),
        Table: tableName,
        OldForeignKeyName: fKeyName,
        ForeignKeyName: newForeignKeyName
    };
    CfCServiceTester.WEBservice.CfcWebService.UpdateForeignKey(request, false, onSuccess_RenameForeignKey6, onFailure_EnumerateForeignKeys);
    return false;
}

// Delete foreign key
function DeleteForeignKey(button) {
    var manager = $find('CfcTestManager');

    var tableName = $(manager.get_hdnSelectedTable6Id()).val();
    if (!tableName) {
        alert('Table name is not defined.');
        return false;
    }
    var fKeyName = $(manager.get_hdnSelectedForeignKey6Id()).val();
    if (!fKeyName) {
        alert('Foreign key is not selected.');
        return false;
    }
    if (!confirm('Are you sure to delete foreign key ' + fKeyName + '?'))
        return false;

    var request = {     // Object is instanced of UpdateForeignKeyRequest class
        Operation: 'Delete',
        CFC_DB_Major_Version: $(manager.get_txtMajorDbVersion6Id()).val(),
        CFC_DB_Minor_Version: $(manager.get_txtMinorDbVersion6Id()).val(),
        Table: tableName,
        OldForeignKeyName: fKeyName,
        ForeignKeyName: fKeyName
    };
    CfCServiceTester.WEBservice.CfcWebService.UpdateForeignKey(request, false, onSuccess_DeleteForeignKey, onFailure_EnumerateForeignKeys);
    return false;
}

// Create new foreign key
function CreateForeignKey6(aButton) {
    var manager = $find('CfcTestManager');

    $(manager.get_txtFkeyName7Id()).removeAttr("disabled");
    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.EnumerateTables(onSuccess_EnumerateTables6, onFailure_EnumerateForeignKeys, true);

    var tableName = $(manager.get_hdnSelectedTable6Id()).val();
    $(manager.get_txtFkeyName7Id()).val('');
    $(manager.get_txtSourceTblName7Id()).val(tableName);

    return false;
}

// Update existing foreign key
function EditForeignKey6(aButton) {
    var manager = $find('CfcTestManager');

    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.EnumerateTables(onSuccess_EnumerateTables6, onFailure_EnumerateForeignKeys, false);

    var tableName = $(manager.get_hdnSelectedTable6Id()).val();
    var fKeyName = $(manager.get_hdnSelectedForeignKey6Id()).val();
    $(manager.get_txtFkeyName7Id()).val(fKeyName);
    $(manager.get_txtSourceTblName7Id()).val(tableName);

    return false;
}

// result is instance of DataTableListDbo class.
// context: true - create new foreign key, false - edit existing one
function onSuccess_EnumerateTables6(result, context, methodName) {
    var manager = $find('CfcTestManager');
    var targetTables = $(manager.get_ddlTargetTblName7Id());
    targetTables.empty();
    if (result.IsSuccess)
    {
        var sb = new Sys.StringBuilder();
        sb.append('<option value="">Select target table</option>');
        Array.forEach(result.TableNames, AppendTableToList, sb);
        targetTables.html(sb.toString());
        var tableName = $(manager.get_hdnSelectedTable6Id()).val();
        CfCServiceTester.WEBservice.CfcWebService.EnumerateColumns(tableName, onSuccess_EnumerateColumns6,
                                                                   onFailure_EnumerateForeignKeys, context);
    } else {
        alert(result.ErrorMessage);
    }
}
function AppendTableToList(element, index, array) {
    var s = String.format('<option value="{0}">{0}</option>', element);
    this.append(s);
}

// result is instance of EnumerateColumnsResponse
// context: true - create new foreign key, false - edit existing one
function onSuccess_EnumerateColumns6(result, context, methodName) {
    var manager = $find('CfcTestManager');
    var sourceFieldList = $(manager.get_ddlSourceColumns7Id());
    $(manager.get_ddlTargetColumns7Id()).empty();

    if (result.IsSuccess) {
        sourceFieldList.empty();
        var sb = new Sys.StringBuilder();
        Array.forEach(result.Columns, AppendFieldToList, sb);
        sourceFieldList.html(sb.toString());
        if (context) {
            $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();
            $(manager.get_hdnOperationType7Id()).val('Insert');
            $(manager.get_btnCreateForeignKey7Id()).attr('value', 'Create key');
            var dialog = new Boxy('#ForeignKeyEditor6',
                { center: true, modal: true, title: "Create new foreign key", draggable: true, fixed: false });
            manager.set_columnEditor(dialog);
            dialog.show();
        } else {
            $(manager.get_hdnOperationType7Id()).val('Modify');
            var tableName = $(manager.get_hdnSelectedTable6Id()).val();
            var fKeyName = $(manager.get_hdnSelectedForeignKey6Id()).val();
            CfCServiceTester.WEBservice.CfcWebService.GetForeignKeyDescription(
                                                tableName, fKeyName, onSuccess_GetForeignKeyDescription6, onFailure_EnumerateForeignKeys);
        }
    } else {
        $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();
        alert(result.ErrorMessage);
    }
}

// result is instance of GetForeignKeysResponse class
function onSuccess_GetForeignKeyDescription6(result) {
    var manager = $find('CfcTestManager');
    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();

    if (result.IsSuccess) {
        ShowForeignKey6($(manager.get_hdnSelectedTable6Id()).val(), result.Dbo);
        var dialog = new Boxy('#ForeignKeyEditor6',
            { center: true, modal: true, title: "Update foreign key", draggable: true, fixed: false });
        manager.set_columnEditor(dialog);
        dialog.show();
    } else {
        alert(result.ErrorMessage);
    }
}
function ShowForeignKey6(tableName, dbo) {
    var manager = $find('CfcTestManager');

    $(manager.get_txtFkeyName7Id()).attr('disabled', 'disabled');

    ShowAction6($(manager.get_ddlTargetTblName7Id()), dbo.ReferencedTable); 
    ShowAction6($('#ddlUpdateAction7'), dbo.UpdateAction); 
    ShowAction6($('#ddlDeleteAction7'), dbo.DeleteAction); 
    
    ShowFkeyColumnsInDialog6(manager, dbo)
    if (dbo.IsChecked)
        $('#chkCheckAfterConstruction').attr('checked', 'checked');
    else
        $('#chkCheckAfterConstruction').removeAttr('checked');
    $(manager.get_btnCreateForeignKey7Id()).attr('value', 'Update key');
}

function ShowAction6(jQuerySelect, action) {
    jQuerySelect.find('option:selected').removeAttr('selected');
    var argument = String.format('option[value="{0}"]', action);
    jQuerySelect.find(argument).attr('selected', 'selected');
}

// Parameters:
//  The element argument is the array element that the function will take action on. Instance of the DataColumnDbo class.
//  The index argument is the index of element, and 
//  The array argument is the array that contains element.
//  Context (this) represents string builder (innerHtl for the body element).
function AppendFieldToList(element, index, array) {
    var selected = index == 0 ? 'selected="selected"' : '';
    var s = String.format('<option value="{0}" {1}>{0}</option>', element.Name, selected);
    this.append(s);
}

// result is instance of UpdateForeignKeyResponse
function onSuccess_DeleteForeignKey(result) {
    var manager = $find('CfcTestManager');

    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();
    if (result.IsSuccess) {
        var selectedForeignKey = $(manager.get_lstForeignKeyList6Id() + ' option:selected').remove();
        var newOption = $(manager.get_lstForeignKeyList6Id() + ' option:first');
        if (newOption.length > 0) {
            newOption.attr("selected", "selected");
            ForeignKeyListOnChange6(newOption.get(0));
        } else {
        $(manager.get_lstSourceColumnList6Id()).empty();
        $(manager.get_lstTargetColumnList6Id()).empty();
    }
    } else {
        alert(result.ErrorMessage);
    }
}


// result is instance of UpdateForeignKeyResponse
function onSuccess_RenameForeignKey6(result) {
    var manager = $find('CfcTestManager');

    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();
    if (result.IsSuccess) {
        var selectedForeignKey = $(manager.get_lstForeignKeyList6Id() + ' option:selected');
        var newName = result.Dbo.Name;
        selectedForeignKey.attr('value', newName);
        selectedForeignKey.html(newName);
        $(manager.get_hdnSelectedForeignKey6Id()).val(newName);
    } else {
        alert(result.ErrorMessage);
    }
}

// Result is instance of EnumerateForeignKeysResponse class
function onSuccess_EnumerateForeignKeys(result, context, operation) {
    var manager = $find('CfcTestManager');
    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();

    if (result.IsSuccess) {
        var fkList = $(manager.get_lstForeignKeyList6Id());
        var sourceColumns = $(manager.get_lstSourceColumnList6Id());
        var targetColumns = $(manager.get_lstTargetColumnList6Id());
        fkList.empty();
        sourceColumns.empty();
        targetColumns.empty();
        var sb = new Sys.StringBuilder();
        $.each(result.ForeignKeys, function (index, value) {
            var attrSelected = index == 0 ? 'selected="selected"' : '';
            var s = String.format('<option value="{0}" {1}>{0}</option>', value.Name, attrSelected);
            sb.append(s);
        });
        fkList.html(sb.toString());

        if (result.ForeignKeys.length > 0) {
            $(manager.get_hdnSelectedForeignKey6Id()).val(result.ForeignKeys[0].Name);
            _ShowForeignKeyFields(manager, result.ForeignKeys[0], sourceColumns, targetColumns);
            $(manager.get_btnRenameFkey6Id()).removeAttr('disabled');
            $(manager.get_btnModifyFkey6Id()).removeAttr('disabled');
            $(manager.get_btnDeleteFkey6Id()).removeAttr('disabled'); 
        } else {
            $(manager.get_hdnSelectedForeignKey6Id()).val('');
            $('table#FKeyDefinition6 tbody#FKeyInfoFields6 tr td span.TargetFields').text('Fields (target)');
            $(manager.get_btnRenameFkey6Id()).attr('disabled', 'disabled');
            $(manager.get_btnModifyFkey6Id()).attr('disabled', 'disabled');
            $(manager.get_btnDeleteFkey6Id()).attr('disabled', 'disabled'); 
        }
    } else {
        alert(result.ErrorMessage);
    }
}

// Parameters:
//  manager - component CfcServiceTestManager,
//  result - instance of ForeignKeyDbo class
function _ShowForeignKeyFields(manager, result, sourceColumns, targetColumns) {
    var sbSource = new Sys.StringBuilder();
    var sbTarget = new Sys.StringBuilder();

    $.each(result.Columns, function (index, value) {
        sbSource.append(String.format('<option value="{0}">{0}</option>', value.Name));
        sbTarget.append(String.format('<option value="{0}">{0}</option>', value.ReferencedColumn));
    });
    sourceColumns.html(sbSource.toString());
    targetColumns.html(sbTarget.toString());

    $('table#FKeyDefinition6 tbody#FKeyInfoFields6 tr td span.TargetFields').text(String.format("Fields ({0})", result.ReferencedTable));
}

// Result is instance of GetForeignKeysResponse class
function onSuccess_GetForeignKey(result, context, operation) {
    var manager = $find('CfcTestManager');
    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();

    if (result.IsSuccess) {
        ShowFkeyColumns6(manager, result.Dbo);
    } else {
        alert(result.ErrorMessage);
    }
}
function ShowFkeyColumns6(manager, dbo) {
    var manager = $find('CfcTestManager');
    var sourceColumns = $(manager.get_lstSourceColumnList6Id());
    var targetColumns = $(manager.get_lstTargetColumnList6Id());
    sourceColumns.empty();
    targetColumns.empty();
    $.each(dbo.Columns, function (index, value) {
        sourceColumns.append($("<option/>", { value: value.Name, text: value.Name }));
        targetColumns.append($("<option/>", { value: value.ReferencedColumn, text: value.ReferencedColumn }));
    });
    $(manager.get_TargetFieldLabel6Id()).text(String.format("Fields ({0})", dbo.ReferencedTable));
}
function ShowFkeyColumnsInDialog6(manager, dbo) {
    var sourceColumns = $(manager.get_lbxSourceColumns7Id());
    var targetColumns = $(manager.get_lbxTargetColumns7Id());
    var availableColumns = $(manager.get_ddlTargetColumns7Id());
    sourceColumns.empty();
    targetColumns.empty();
    availableColumns.empty();
    $.each(dbo.Columns, function (index, value) {
        sourceColumns.append($("<option/>", { value: value.Name, text: value.Name }));
        targetColumns.append($("<option/>", { value: value.ReferencedColumn, text: value.ReferencedColumn }));
    });
    $.each(dbo.AvailableTargetColumns, function (index, colName) {
        var attributes = { value: colName, text: colName };
        if (colName == dbo.Columns[0].ReferencedColumn)
            attributes.selected = 'selected';
        availableColumns.append($("<option/>", attributes));
    });
    var argument = String.format('{0} option[value="{1}"]', manager.get_ddlSourceColumns7Id(), dbo.Columns[0].Name);
    $(argument).attr('selected', 'selected');
}

function onFailure_EnumerateForeignKeys(result, context, operation) {
    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();
    alert(result.get_message());
}