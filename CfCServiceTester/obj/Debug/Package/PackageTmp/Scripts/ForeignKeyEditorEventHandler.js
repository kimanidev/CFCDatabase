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
    var tdSibling = $(aList).closest('td').siblings();
    tdSibling.find('select option[selected]').removeAttr('selected');
    var argument = String.format('select option:nth-child({0})', index + 1);
    tdSibling.find(argument).attr('selected', 'selected');
    return false;
}

// Store information about new foreign key in the database
function CreateThisForeignKey7(aButton) {
    var manager = $find('CfcTestManager');
    var fKeyName = $(manager.get_txtFkeyName7Id()).val();
    if (!ValidateName(fKeyName))
        return false;
    var request = PrepareUpdateForeignKeyRequest(fKeyName);

    $('span#spnAddFkeyColumn7 span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.UpdateForeignKey(request, false, onSuccess_CreateForeignKey7, onFailure_GetTargetColumns7);
    
    return false;
}
function PrepareUpdateForeignKeyRequest(fKeyName) {
    var manager = $find('CfcTestManager');

    var rzlt = {    // rzlt is instance of UpdateForeignKeyRequest class
        Operation: $(manager.get_hdnOperationType7Id()).val(),
        CFC_DB_Major_Version: $(manager.get_txtMajorDbVersion6Id()).val(),
        CFC_DB_Minor_Version: $(manager.get_txtMinorDbVersion6Id()).val(),
        Table: $(manager.get_txtSourceTblName7Id()).val(),
        OldForeignKeyName: fKeyName,
        ForeignKeyName: fKeyName,
        Dbo: {      // Dbo is instance of ForeignKeyDbo class
            Name: fKeyName,
            Columns: GetForeignKeyColumns7(manager.get_lbxSourceColumns7Id(), manager.get_ddlTargetColumns7Id()),
            DeleteAction: $('#ddlDeleteAction7').val(),
            UpdateAction: $('#ddlUpdateAction7').val(),
            IsChecked: $('#chkCheckAfterConstruction').is(':checked'),
            ReferencedTable: $(manager.get_ddlTargetTblName7Id()).val()
        }
    };
    return rzlt;
}
function GetForeignKeyColumns7(sourceNames, targetNames) {
    var listSource = $(String.format("{0} option", sourceNames));
    var listTarget = $(String.format("{0} option", targetNames));
    var rzlt = new Array(listSource.length);

    listSource.each(function (index) {
        rzlt[index] = { // Each element in the array is instance of ForeignKeyColumnDbo object
            Name: this.value,
            ReferencedColumn: listTarget.get(index).value // Both lists are of the same length
        }
    });
    return rzlt;
}

// Returns false if name for the foreign key is invalid.
function ValidateName(fKeyName) {
    var manager = $find('CfcTestManager');
    var fKeyNameControl = $(manager.get_txtFkeyName7Id());

    if (!fKeyName) {
        alert("Name of the foreign key is not defined.");
        fKeyNameControl.focus();
        return false;
    }
    // Regex validator for column name: http://stackoverflow.com/questions/4977898/check-for-valid-sql-column-name
    var rg = /^[a-z][a-z\d_]*$/i;
    if (!rg.test(fKeyName)) {
        alert('Name of the foreign key contains invalid characters.');
        fKeyNameControl.focus();
        return false;
    }

    var rzlt = true;
    if ($(manager.get_hdnOperationType7Id()).val() == 'Insert') {
        var fKeyNameUpperCase = fKeyName.toUpperCase();
        $(manager.get_lstForeignKeyList6Id() + ' option').each(function (index) {
            var foreignKeyName = this.value.toUpperCase();
            if (foreignKeyName == fKeyNameUpperCase) {
                var message = String.format("Table {0} contains foreign key {1}.", $(manager.get_txtSourceTblName7Id()).val(), fKeyName);
                alert(message);
                rzlt = false;
                fKeyNameControl.focus();
                return false;
            }
        });
    }
    return rzlt;
}

// Remove selected column from the list
function RemoveColumn7(aButton) {
    var manager = $find('CfcTestManager');
    var sourceColumn = $(manager.get_lbxSourceColumns7Id()).get(0);
    var targetColumn = $(manager.get_lbxTargetColumns7Id()).get(0);
    
    RemoveDomElement(sourceColumn) && RemoveDomElement(targetColumn);       // Don't remove target column once source deleting failed
    return false;
}
function RemoveDomElement(element) {
    var index = element.selectedIndex;
    if (index < 0) {
        alert("Select the column clicking on it's name in source or target list.");
        return false;
    }
    var argument = String.format('option:nth-child({0})', index + 1);
    $(element).find(argument).remove();
    return true;
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

// result is instance of UpdateForeignKeyResponse class.
function onSuccess_CreateForeignKey7(result) {
    var manager = $find('CfcTestManager');
    $('span#spnAddFkeyColumn7 span.Pauser').hide();

    if (result.IsSuccess) {
        if ($(manager.get_hdnOperationType7Id()).val() == 'Insert') {
            AddForeignKeyDescription(manager, result.Dbo);
        }
        ShowFkeyColumns6(manager, result.Dbo);
        manager.get_columnEditor().hide();
    } else {
        alert(result.ErrorMessage);
    }
}
// dbo is instance of ForeignKeyDbo class
function AddForeignKeyDescription(manager, dbo)
{
    var fKeylist = $(manager.get_lstForeignKeyList6Id());
    fKeylist.find('option:selected').removeAttr('selected');
    fKeylist.append($("<option/>", { value: dbo.Name, text: dbo.Name, selected: "selected" }));
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