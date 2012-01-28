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
    var request = {
        OperationType: 'Rename',
        TableName: tableName,
        OldForeignKeyName: fKeyName,
        ForeignKeyName: newForeignKeyName
    };
    CfCServiceTester.WEBservice.CfcWebService.UpdateForeignKey(request, false, onSuccess_RenameForeignKey, onFailure_EnumerateForeignKeys);
    return false;
}

// result is instance of UpdateForeignKeyResponse
function onSuccess_RenameForeignKey(result) {
    var manager = $find('CfcTestManager');

    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();
    if (result.IsSuccess) {
        var selectedForeignKey = $(manager.get_lstForeignKeyList6Id() + ' option:selected');
        var newName = result.Dbo.Name;
        selectedForeignKey.attr('value', newName);
        selectedForeignKey.html(newName);
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
        var sourceColumns = $(manager.get_lstSourceColumnList6Id());
        var targetColumns = $(manager.get_lstTargetColumnList6Id());
        sourceColumns.empty();
        targetColumns.empty();
        _ShowForeignKeyFields(manager, result.Dbo, sourceColumns, targetColumns);
    } else {
        alert(result.ErrorMessage);
    }
}

function onFailure_EnumerateForeignKeys(result, context, operation) {
    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').hide();
    alert(result.get_message());
}