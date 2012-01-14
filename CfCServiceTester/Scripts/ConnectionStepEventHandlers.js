
// Test the service itself
function Button1_onclick() {
//    var service = new CfCServiceNS.CfCService();
//    service.CostOfSandwiches(3, onSuccessButton1_onclick, null, null);
    CfCServiceTester.WEBservice.CfcWebService.HelloWorld('client', onSuccessButton1_click, onFailureButton1_click);
    return false;
}
function onSuccessButton1_click(result) {
    $('#msgServiceWorking').text(result);
}
function onFailureButton1_click(result) {
    var msg = result.get_message();
    var msg1 = result.get_stackTrace();
    var msg2 = result.get_statusCode();
    alert(msg);
}
// **********************************************************************

// Get list of available SQL servers
function PickServers() {
    var manager = $find('CfcTestManager');

    var template = $(manager.get_txtServerNameId()).val();
    $('span#SpanSelectServer span.Magnifier').hide();
    $('span#SpanSelectServer span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.EnumerateSqlServers(
                manager.get_localServersOnly(), template, onSuccess_PickServers, onFailure_PickServers);
    return false;
}
// Get list of available databases on the selected server
function PickDatabases() {
    var manager = $find('CfcTestManager');

    var serverName = $(manager.get_txtServerNameId()).val();
    if (!serverName) {
        alert('Server is not selected.')
        return false;
    }
    var template = $(manager.get_txtDatabaseNameId()).val();
    $('span#SpanSelectDatabase span.Magnifier').hide();
    $('span#SpanSelectDatabase span.Pauser').show();
    CfCServiceTester.WEBservice.CfcWebService.EnumerateDatabases(
                serverName, template, manager.get_accessibleDatabasesOnly(), onSuccess_PickDatabases, onFailure_PickDatabases);
    return false;
}

// Make and verify DB connection. Correct credentials are stored in the session and don't need to be repeated.
function ConnectDatabase() {
    var manager = $find('CfcTestManager');

    var rsa = new RSAKey();
    rsa.setPublic(manager.get_rsaModulus(), manager.get_rsaExponent());
    var serverName = $(manager.get_txtServerNameId()).val();
    var databaseName = $(manager.get_txtDatabaseNameId()).val();
    var uName = $(manager.get_txtLoginNameId()).val();
    var uPasswd = $(manager.get_txtLoginPasswdId()).val();
    var username = rsa.encrypt(uName);
    var pass = rsa.encrypt(uPasswd);

    CfCServiceTester.WEBservice.CfcWebService.CreateDbConnection(
                serverName, databaseName, username, pass, onSuccess_CreateDbConnection, onFailure_CreateDbConnection);
    return false;
}

/* ******************** Utilities ******************** */
function onSuccess_PickServers(result) {
    var manager = $find('CfcTestManager');

    if (result.length < 1) {
        alert("No SQL servers were found.");
        $('span#SpanSelectServer span.Magnifier').show();
        $('span#SpanSelectServer span.Pauser').hide();
    }
     else if (result.length == 1) {
        $(manager.get_txtServerNameId()).val(result[0].Name);
        $('span#SpanSelectServer span.Magnifier').show();
        $('span#SpanSelectServer span.Pauser').hide();
    }
    else {
        FillServerDropDown(result);
    }
}
function onSuccess_PickDatabases(result) {
    var manager = $find('CfcTestManager');

    var serverName = $(manager.get_txtServerNameId()).val();
    if (result.length < 1) {
        alert("SQL server " + serverName + " contains no available databases.");
        $('span#SpanSelectDatabase span.Magnifier').show();
        $('span#SpanSelectDatabase span.Pauser').hide();
    }
    else if (result.length == 1) {
        $(manager.get_txtDatabaseNameId()).val(result[0]);
        $('span#SpanSelectServer span.Magnifier').show();
        $('span#SpanSelectServer span.Pauser').hide();
    }
    else {
        FillDatabaseDropDown(result);
    }
}

// result is instance of CreateDbConnectionResponse class
function onSuccess_CreateDbConnection(result) {
    if (!result.Connected)
        PrintConnectionError(result.ErrorMessage);
    else {
        var manager = $find('CfcTestManager');
        var loginName = $(manager.get_txtLoginNameId());
        var password = $(manager.get_txtLoginPasswdId());

        PrintConnectionInfo(result.Roles, result.CurrentServer, result.CurrentDatabase);
        loginName.val('');
        loginName.attr('disabled', true);
        password.val('');
        password.attr('disabled', true);
    }
}

function FillServerDropDown(result) {
    var dbCombo = $('span#ServerSelector select');
    dbCombo.empty();
    
    var comboHtml = GetOptionString('', 'Select Server');
    for (i = 0; i < result.length; i++) {
        var dbName = result[i].Name;
        comboHtml += GetOptionString(dbName, dbName);
    }
    dbCombo.html(comboHtml);

    $('span#SpanSelectServer span.Pauser').hide();
    $('span#ServerSelector').show();
}
function GetOptionString(value, text) {
    var rzlt = '<option value="' + value + '">' + text + '</option>';
    return rzlt;
}
function FillDatabaseDropDown(result) {
    var dbCombo = $('span#DatabaseSelector select');
    dbCombo.empty();

    var comboHtml = GetOptionString('', 'Select Database');
    for (i = 0; i < result.length; i++) {
        var dbName = result[i].Name;
        comboHtml += GetOptionString(dbName, dbName);
    }
    dbCombo.html(comboHtml);
    $('span#SpanSelectDatabase span.Pauser').hide();
    $('span#DatabaseSelector').show();
}

function onFailure_PickServers(result) {
    alert(result.get_message());
}
function onFailure_PickDatabases(result) {
    var msg = result.get_message();
    var msg1 = result.get_stackTrace();
    var msg2 = result.get_statusCode();
    alert(msg);
}
function onFailure_CreateDbConnection(result) {
    var msg = result.get_message();
    var msg1 = result.get_stackTrace();
    var msg2 = result.get_statusCode();
    alert(msg);
}

function svSelectionChanged(selectElement) {
    var manager = $find('CfcTestManager');

    $(manager.get_txtServerNameId()).val(selectElement.value);
    $('span#ServerSelector').hide();
    $('span#SpanSelectServer span.Magnifier').show();
}
function dbSelectionChanged(selectElement) {
    var manager = $find('CfcTestManager');

    $(manager.get_txtDatabaseNameId()).val(selectElement.value);
    $('span#DatabaseSelector').hide();
    $('span#SpanSelectDatabase span.Magnifier').show();
}

// Connection to database failed
function PrintConnectionError(errorMessage) {
    var manager = $find('CfcTestManager');

    var errMessage = $(manager.get_spnConnectionErrorId());
    errMessage.text(errorMessage);

    $(manager.get_spnConnectionOKId()).hide();
    errMessage.show();
}
// You are connected to database. Parameter contains list of roles.
function PrintConnectionInfo(roles, server, database) {
    var manager = $find('CfcTestManager');
    var tmpArray = new Array();

    var okMessage = $(manager.get_spnConnectionOKId() + ' ul');
    okMessage.children().remove();
    for (var i in roles) {
        okMessage.append('<li>' + roles[i] + '</li>');
        tmpArray[i] = roles[i];
    }

    manager.set_userRoles(tmpArray);
    $(manager.get_divLoginTooltip()).text('Reload the page if you want another connection.');
    $(manager.get_spnConnectionErrorId()).hide();
    okMessage.closest('div').show();
}