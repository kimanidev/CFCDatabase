function TableListOnChange6(dropDown) {
    var manager = $find('CfcTestManager');

    var tableName = $(dropDown).val();
    $('table#FKeyDefinition6 tbody tr.Pauser td span.Pauser').show();
    $(manager.get_hdnSelectedTable4Id()).val(tableName);

    CfCServiceTester.WEBservice.CfcWebService.EnumerateIndexes(tableName, onSuccess_EnumerateIndexes, onFailure_GetIndex);

    return false;
}