// Selected table changed
function TableListOnChange(dropDown) {
    var manager = $find('CfcTestManager');

    var tableName = $(dropDown).val();
    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').show();
    $(manager.get_hdnSelectedTable4Id()).val(tableName);

    CfCServiceTester.WEBservice.CfcWebService.EnumerateIndexes(tableName, onSuccess_EnumerateIndexes, onFailure_GetIndex);

    return false;
}

// Selected index changed
function IndexListOnChange(dropDown) {
    var manager = $find('CfcTestManager');
    if (manager._suppressEvents == true)
        return false;

    var indexName = $(dropDown).val();
    var tableName = $(manager.get_lstTableList4Id()).val();
    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').show();
    $(manager.get_hdnSelectedIndex4Id()).val(indexName);
    $(manager.get_hdnSelectedTable4Id()).val(tableName);
    CfCServiceTester.WEBservice.CfcWebService.GetIndex(tableName, indexName, false, onSuccess_GetIndex, onFailure_GetIndex);
    return false;
}

// Create new index
function CreateIndex(aButton) {
    var manager = $find('CfcTestManager');
    var tableName = $(manager.get_hdnSelectedTable4Id()).val();
    $(manager.get_txtFillFactor5Id()).val('50');

    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.EnumerateColumns(tableName, onSuccess_EnumerateColumns4, onFailure_GetIndex);

    return false;
}

function EditIndex(aButton) {
    var manager = $find('CfcTestManager');
    var tableName = $(manager.get_hdnSelectedTable4Id()).val();
    var indexName = $(manager.get_hdnSelectedIndex4Id()).val();

    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.GetIndex(tableName, indexName, true, onSuccess_GetIndex4, onFailure_GetIndex);

    return false;
}

function DeleteIndex(aButton) {
    var manager = $find('CfcTestManager');
    var indexName = $(manager.get_hdnSelectedIndex4Id()).val();
    if (!confirm("Are you sure to delete '" + indexName + "' index?"))
        return false;

    // Instance of the UpdateIndexRequest class. Delete operation does not require IndexDescriptor.
    var request = { OperationType: 'Delete' };
    request.DisableDependencies = $('#chkDisableDependencies4').attr('checked') == 'checked';
    request.TableName = $(manager.get_hdnSelectedTable4Id()).val();
    request.OldIndexName = request.IndexName = indexName;
    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.UpdateIndex(request, false, onSuccess_DeleteIndex4, onFailure_GetIndex);

    return false;
}

// Rename selected index
function RenameIndex(aButton) {
    var manager = $find('CfcTestManager');

    var tableName = $(manager.get_lstTableList4Id()).val();
    if (!tableName)
    {
        alert('Table name is not defined.');
        return false;
    }
    var indexName = $(manager.get_lstIndexList4Id()).val();
    if (!indexName)
    {
        alert('Index is not selected.');
        return false;
    }
    var newIndexName = $(manager.get_txtNewName4Id()).val();
    if (!newIndexName || newIndexName == indexName)
    {
        alert('New name for index is not defined.');
        return false;
    }

    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').show();
    var request = {
        OperationType: 'Rename',
        TableName: tableName,
        OldIndexName: indexName,
        IndexName: newIndexName
    };
    CfCServiceTester.WEBservice.CfcWebService.UpdateIndex(request, false, onSuccess_UpdateIndex, onFailure_GetIndex);
    return false;
}

// result is instance of UpdateIndexResponse
function onSuccess_UpdateIndex(result)
{
    var manager = $find('CfcTestManager');

    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').hide();
    if (result.IsSuccess) {
        var selectedIndex = $(manager.get_lstIndexList4Id() + ' option:selected');
        var newName = result.Dbo.Name;
        selectedIndex.attr('value', newName);
        selectedIndex.html(newName);
    } else {
        alert(result.ErrorMessage); 
    }
}

// result is instance of GetIndexResponse class
function onSuccess_GetIndex(result) {
    var manager = $find('CfcTestManager');

    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').hide();
    if (result.IsSuccess) {
        ProcessIndex(manager, result.Dbo);
    } else {
        alert(result.ErrorMessage);
    }
}
// result is instance of GetIndexResponse class
function onSuccess_GetIndex4(result) {
    var manager = $find('CfcTestManager');

    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').hide();
    if (result.IsSuccess) {
        ShowIndexEditor(manager, result);
    } else {
        alert(result.ErrorMessage);
    }
}
function ProcessIndex(manager, dbo) {
    var fldList = $(manager.get_lstFieldList4Id());
    fldList.empty();
    if (dbo) {
        var sb = new Sys.StringBuilder();
        $.each(dbo.IndexedColumns, function (index, value) {
            var attrSelected = index == 0 ? 'selected="selected"' : '';
            var s = String.format('<option value="{0}" {1}>{0}</option>', value, attrSelected);
            sb.append(s);
        });
        fldList.html(sb.toString());
        ShowIndexCharacteristics(dbo)
    }
}

// result is instance of EnumerateIndexesResponse class
function onSuccess_EnumerateIndexes(result) {
    var manager = $find('CfcTestManager');

    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').hide();
    if (result.IsSuccess) {
        manager._suppressEvents = true; // Don' process changed event.

        var indexList = $(manager.get_lstIndexList4Id());
        indexList.empty();
        var sb = new Sys.StringBuilder();
        $.each(result.Indexes, function (index, value) {
            var attrSelected = index == 0 ? 'selected="selected"' : '';
            var s = String.format('<option value="{0}" {1}>{0}</option>', value.Name, attrSelected);
            sb.append(s);
        });
        indexList.html(sb.toString());

        if (result.Indexes.length > 0)
            ProcessIndex(manager, result.Indexes[0]);
        else
            ProcessIndex(manager);

        manager._suppressEvents = false;
    } else {
        alert(result.ErrorMessage);
    }
}

// dbo is instance of IndexDbo object
function ShowIndexCharacteristics(dbo) {
    var manager = $find('CfcTestManager');
    
    $(manager.get_chkCompactLargeObjects4Id()).attr('checked', dbo.CompactLargeObjects);
    $(manager.get_chkDisallowPageLocks4Id()).attr('checked', dbo.DisallowPageLocks);
    $(manager.get_chkDisallowRowLocks4Id()).attr('checked', dbo.DisallowRowLocks);
    $(manager.get_txtFillFactor4Id()).val(dbo.FillFactor);
    $(manager.get_txtFilterDefinition4Id()).val(dbo.FilterDefinition);
    $(manager.get_chkIgnoreDuplicateKeys4Id()).attr('checked', dbo.IgnoreDuplicateKeys);

    $(manager.get_ddlIndexKeyType4Id() + ' option[selected]').removeAttr('selected');
    $(manager.get_ddlIndexKeyType4Id() + ' option[value="' + dbo.IndexKeyType + '"]').attr('selected', true);

    $(manager.get_chkIsClustered4Id()).attr('checked', dbo.IsClustered);
    $(manager.get_chkIsDisabled4Id()).attr('checked', dbo.IsDisabled);
    $(manager.get_chkIsUnique4Id()).attr('checked', dbo.IsUnique);
}

// result is instance of UpdateIndexResponse
function onSuccess_DeleteIndex4(result) {
    var manager = $find('CfcTestManager');
    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').hide();

    if (result.IsSuccess) {
        var indexName = result.Dbo.Name;
        $(manager.get_lstIndexList4Id() + ' option:selected').remove();
        var options = $(manager.get_lstIndexList4Id() + ' option');
        if (options.length < 1)
            ClearTheForm();
        else {
            $(manager.get_lstIndexList4Id()).prop('selectedIndex', 0);
            var dropDown = $(manager.get_lstIndexList4Id()).get(0); // IndexListOnChange expects DOM element.
            IndexListOnChange(dropDown);    
        }
    } else {
        alert(result.ErrorMessage);
    }
}
function ClearTheForm() {
    var manager = $find('CfcTestManager');

    $(manager.get_lstFieldList4Id()).empty();
    $('#IndexInfoFields4 tr td input:checked').removeAttr('checked');
    $('#IndexInfoFields4 tr td input:text').val('');
    $(manager.get_ddlIndexKeyType4Id()).prop('selectedIndex', 0);
}

// result is instance of EnumerateColumnsResponse
function onSuccess_EnumerateColumns4(result) {
    var manager = $find('CfcTestManager');
    var checkBoxList = $('#IndexFieldList');

    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').hide();
    if (result.IsSuccess) {
        checkBoxList.empty();
        var sb = new Sys.StringBuilder();
        Array.forEach(result.Columns, AppendCheckBoxToList, sb);
        var aList = sb.toString();
        checkBoxList.html(aList);

        var indexName = $(manager.get_txtNewName4Id()).val();
        if (indexName)
            $(manager.get_txtName5Id()).val(indexName);
        $(manager.get_hdIndexOperation5Id()).val('Insert');

        var dialog = new Boxy('#IndexEditor4',
            { center: true, modal: true, title: "Create new index", draggable: true, fixed: false });
        manager.set_columnEditor(dialog);
        dialog.show();
    } else {
        alert(result.ErrorMessage);
    }
}
// Parameters:
//  The element argument is the array element that the function will take action on. Instance of the DataColumnDbo class.
//  The index argument is the index of element, and 
//  The array argument is the array that contains element.
//  Context (this) represents string builder (innerHtl for the body element).
function AppendCheckBoxToList(element, index, array) {
    var s = String.format('<input type="checkbox" value="{0}">{0}<br>', element.Name);
    this.append(s);
}


function onFailure_GetIndex(result) {
    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').hide();
    alert(result.get_message());
}
