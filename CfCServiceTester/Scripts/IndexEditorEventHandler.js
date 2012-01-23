function SaveIndex(button) {
    var dbo = GetIndexDefinition();
    var errMessage = ValidateIndexForm(dbo);
    if (errMessage != '') {
        alert(errMessage);
    }
    else {
        var request = GenerateIndexModifyRequest(dbo);
        $('#CreateIndexDialogPauser').show();
        CfCServiceTester.WEBservice.CfcWebService.UpdateIndex(request, false, onSuccess_UpdateIndex, onFailure_UpdateIndex);
    }
    return false;
}

// Function returns IndexDbo object
function GetIndexDefinition() {
    var manager = $find('CfcTestManager');
    var dbo = {};

    dbo.CompactLargeObjects = $(manager.get_chkCompactLargeObjects5Id()).attr('checked') == 'checked';
    dbo.DisallowPageLocks = $(manager.get_chkDisallowPageLocks5Id()).attr('checked') == 'checked';
    dbo.DisallowRowLocks = $(manager.get_chkDisallowRowLocks5Id()).attr('checked') == 'checked';
    dbo.FillFactor = $(manager.get_txtFillFactor5Id()).val();
    dbo.FilterDefinition = $(manager.get_txtFilterDefinition5Id()).val();
    dbo.IgnoreDuplicateKeys = $(manager.get_chkIgnoreDuplicateKeys5Id()).attr('checked') == 'checked';
    
    dbo.IndexedColumns = [];
    $('#IndexFieldList input:checked').each(function (i) {
        dbo.IndexedColumns[i] = $(this).attr('value');
    });
    dbo.IndexKeyType = $(manager.get_ddlIndexKeyType5Id()).val();

    dbo.IsClustered = $(manager.get_chkIsClustered5Id()).attr('checked') == 'checked';
//    dbo.IsDisabled = $(manager.get_chkIsDisabled5Id()).attr('checked') == 'checked';
    dbo.IsUnique = $(manager.get_chkIsUnique5Id()).attr('checked') == 'checked';
    dbo.Name = $(manager.get_txtName5Id()).val();

    return dbo;
}

// The function displays index editor. rsp is instance GetIndexResponse class.
function ShowIndexEditor(manager, rsp) {
    $(manager.get_hdIndexOperation5Id()).val('Modify');
    var indexName = $(manager.get_txtName5Id());
    indexName.val(rsp.Dbo.Name);
    indexName.attr('readonly','readonly');

    ShowIndexFields(manager, rsp);
    ShowIndexCharacteristics5(rsp.Dbo);

    var dialog = new Boxy('#IndexEditor4',
            { center: true, modal: true, title: "Modify index", draggable: true, fixed: false });
    manager.set_columnEditor(dialog);
    dialog.show();
}
function ShowIndexFields(manager, rsp) {
    var checkBoxList = $('#IndexFieldList');
    checkBoxList.empty();
    var sb = new Sys.StringBuilder();
    Array.forEach(rsp.AllFields, AppendCheckBoxToList5, sb);
    var aList = sb.toString();
    checkBoxList.html(aList);
}
// Parameters:
//  The element argument is the array element that the function will take action on. Instance of the TableField class.
//  The index argument is the index of element, and 
//  The array argument is the array that contains element.
//  Context (this) represents string builder (innerHtl for the body element).
function AppendCheckBoxToList5(element, index, array) {
    var checkedState = element.IsIncluded ? 'checked="checked"' : '';
    var s = String.format('<input type="checkbox" value="{0}" {1}>{0}<br>', element.Name, checkedState);
    this.append(s);
}

// dbo is instance of IndexDbo object
function ShowIndexCharacteristics5(dbo) {
    var manager = $find('CfcTestManager');

    $(manager.get_chkCompactLargeObjects5Id()).attr('checked', dbo.CompactLargeObjects);
    $(manager.get_chkDisallowPageLocks5Id()).attr('checked', dbo.DisallowPageLocks);
    $(manager.get_chkDisallowRowLocks5Id()).attr('checked', dbo.DisallowRowLocks);
    $(manager.get_txtFillFactor5Id()).val(dbo.FillFactor);
    $(manager.get_txtFilterDefinition5Id()).val(dbo.FilterDefinition);
    $(manager.get_chkIgnoreDuplicateKeys5Id()).attr('checked', dbo.IgnoreDuplicateKeys);

    $(manager.get_ddlIndexKeyType5Id() + ' option[selected]').removeAttr('selected');
    $(manager.get_ddlIndexKeyType5Id() + ' option[value="' + dbo.IndexKeyType + '"]').attr('selected', true);

    $(manager.get_chkIsClustered5Id()).attr('checked', dbo.IsClustered);
    $(manager.get_chkIsUnique5Id()).attr('checked', dbo.IsUnique);
}

// dbo is instance of IndexDbo object
function ValidateIndexForm(dbo) {
    if (!dbo.Name)
        return 'Name of the index is not defined.';
    if (dbo.IndexedColumns.length < 1)
        return 'Index contains no fields.';

    if (!dbo.FillFactor)
        return '';

    var pattern = /^\d+$/;
    if (pattern.test(dbo.FillFactor) && dbo.FillFactor >= 0 && dbo.FillFactor <= 100)
        return '';
    else 
        return 'Invalid fill factor.';
}

// indexDbo is instance of IndexDbo class.
function GenerateIndexModifyRequest(indexDbo) {
    var manager = $find('CfcTestManager');
    var rzlt = {};  // rzlt is instance of UpdateIndexRequest object

    rzlt.OperationType = $(manager.get_hdIndexOperation5Id()).val();
    rzlt.TableName = $(manager.get_hdnSelectedTable4Id()).val();
    rzlt.OldIndexName = rzlt.IndexName = indexDbo.Name;
    rzlt.IndexDescriptor = indexDbo;

    return rzlt
}

// result is instance of UpdateIndexResponse class.
function onSuccess_UpdateIndex(result) {
    var manager = $find('CfcTestManager');
    $('#CreateIndexDialogPauser').hide();

    if (result.IsSuccess) {
        var operation = $(manager.get_hdIndexOperation5Id()).val();
        var boxy = manager.get_columnEditor();
        boxy.hide();

        if (operation == "Insert") {
            var option = String.format('<option value="{0}" selected="selected" >{0}</option>', result.Dbo.Name);
            $(manager.get_lstIndexList4Id() + ' option:selected').removeAttr('selected');
            $(manager.get_lstIndexList4Id()).append(option);
        }
        ProcessIndex(manager, result.Dbo);
        $(manager.get_hdnSelectedIndex4Id()).val(result.Dbo.Name);
    } else {
        alert(result.ErrorMessage);
    }
}

function onFailure_UpdateIndex(result) {
    $('#CreateIndexDialogPauser').hide();
    alert(result.get_message());
}

