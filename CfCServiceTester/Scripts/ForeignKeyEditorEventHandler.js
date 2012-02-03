// Selection changed event handler for target table drop down
function TargetTableListOnChange7(dropDownList) {
    var manager = $find('CfcTestManager');

    var tableName = dropDownList.options[dropDownList.selectedIndex].value;
    $(manager.get_lbxSourceColumns7Id()).empty();
    $(manager.get_lbxTargetColumns7Id()).empty();

    $('span#spnAddFkeyColumn7 span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.GetTargetColumns(tableName, false, onSuccess_GetTargetColumns7, onFailure_GetTargetColumns7);
    return false;
}

// Click event handler for 'Add Column' button.
function AddColumn7(aButton) {
    var manager = $find('CfcTestManager');
    var sourceColumn = $(manager.get_ddlSourceColumns7Id()).val();
    var targetColumn = $(manager.get_ddlTargetColumns7Id()).val();
    
    if (!AddOneColumn7(manager.get_lbxSourceColumns7Id(), sourceColumn)) {
        alert('List with source columns contains name "' + sourceColumn + '".');
        return false;
    }
    if (!AddOneColumn7(manager.get_lbxTargetColumns7Id(), targetColumn)) {
        alert('List with target columns contains name "' + targetColumn + '".');
        $(manager.get_lbxSourceColumns7Id() + ' option:last').remove();
        return false;
    }

    return false;
}

// Selection index changed in list with fields (source and target)
function SelectionChanged7(aList) {
    var index = aList.selectedIndex;
    var argument = String.format('select option:nth-child({0})', index + 1);
    $(aList).closest('td').siblings().find(argument).attr('selected', 'selected');
    return false;
}

// The function returns false if list contains option with supplied name
function AddOneColumn7(listId, name) {
    var manager = $find('CfcTestManager');
    var argument = String.format('{0} option[value="{1}"]', listId, name);
    var options = $(argument);
    if (options.length > 0)
        return false;

    $(listId).append($("<option/>", { value: name, text: name }));
    return true;
}


// result is instance of EnumerateBackupFilesResponse class.
function onSuccess_GetTargetColumns7(result) {
    var manager = $find('CfcTestManager');
    var targetColumns = $(manager.get_ddlTargetColumns7Id());
    $('span#spnAddFkeyColumn7 span.Pauser').hide();

    targetColumns.empty();
    if (result.IsSuccess) {
        var sb = new Sys.StringBuilder();
        Array.forEach(result.NameList, AppendFieldToList7, sb);
        targetColumns.html(sb.toString());
    } else {
        alert(result.ErrorMessage);
    }
    function AppendFieldToList7(element, index, array) {
        var s = String.format('<option value="{0}">{0}</option>', element);
        this.append(s);
    }
}


function onFailure_GetTargetColumns7(result, context, operation) {
    $('span#spnAddFkeyColumn7 span.Pauser').hide();
    alert(result.get_message());
}