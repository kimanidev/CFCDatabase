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

    CfCServiceTester.WEBservice.CfcWebService.GetIndex(tableName, indexName, onSuccess_GetIndex, onFailure_GetIndex);

    return false;
}

// result is instance of GetIndexResponse class
function onSuccess_GetIndex(result) {
    var manager = $find('CfcTestManager');

    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').hide();
    if (result.IsSuccess) {
        ProcessIndex(manager, result.Dbo);
//        var fldList = $(manager.get_lstFieldList4Id());
//        fldList.empty();
//        var sb = new Sys.StringBuilder();
//        $.each(result.Dbo.IndexedColumns, function (index, value) {
//            var attrSelected = index == 0 ? 'selected="selected"' : '';
//            var s = String.format('<option value="{0}" {1}>{0}</option>', value, attrSelected);
//            sb.append(s);
//        });
//        fldList.html(sb.toString());
    }
//    ShowIndexCharacteristics(result.Dbo)
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
    //    alert('IndexListOnChange: as jau cia: ');
}

function onFailure_GetIndex(result) {
    $('table#IndexDefinition4 tbody tr.Pauser td span.Pauser').hide();
    alert(result.get_message());
}
