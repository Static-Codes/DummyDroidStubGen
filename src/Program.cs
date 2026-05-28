using DummyDroidStubGen.Core.Common;
using DummyDroidStubGen.Core.Types;
using static DummyDroidStubGen.Core.Common.InstallationChecks;
using static DummyDroidStubGen.Core.Helpers.FileHelper;
using static DummyDroidStubGen.Core.Helpers.InputHelper;
using static DummyDroidStubGen.Functions;
using static DummyDroidStubGen.Global.Messaging;


var binaryCheckResults = CheckForRequiredBinaries();

foreach (var binaryResult in binaryCheckResults) {
    WriteSuccessMessage($"Located required package: {binaryResult.BinaryName}");
}

CreateAppDataSubDirectory();

var connectionStatus = await CheckForDeviceConnection();
string deviceName = GetNameOfConnectedDevice(connectionStatus);

Device? device;
// var whitelist = new Whitelist();

if (!connectionStatus.Connected) {
    device = new Device(name: deviceName);
}

else 
{
    device = new Device(
        Name: deviceName,
        ConnectionStatus: connectionStatus,
        ID: $"{deviceName ?? "Android Device"} (via {connectionStatus.Method}) @ {connectionStatus.Identifier}"
    );
}

(bool isError, bool shouldExit, string message) = HandleConnectionStatus(device);


if (isError && shouldExit) {
    WriteErrorMessage(message, exit: true);
}


else if (isError && !shouldExit) 
{
    WriteWarningMessage("No paired or connected device was located.");
    WriteInformation("You will be prompted for pairing and connection information for your Android device.");
    
    // The object device is overwritten here, but the old device.ConnectionStatus.Method is used in the reassignment. 
    device = new Device(
        Name: deviceName,
        ConnectionMethod: device.ConnectionStatus.Method
    );
}

else 
{
    var confirmationSelection = AskForSelection(message, options: ["Yes", "No", "I don't know"]);

    UserExitStatusCheck(confirmationSelection);

    var deviceConfirmed = confirmationSelection == "Yes";
    var userIsUnsure = confirmationSelection == "I don't know";

    if (!deviceConfirmed && !userIsUnsure) { 
        Environment.Exit(1); 
    }

    if (userIsUnsure) {
        var sanitizedDeviceName = deviceName != "Unknown" ? deviceName : device.Name != "Unknown" ? device.Name : "device";
        var sanitizedDeviceAddress = device.ConnectionStatus.Identifier != null ? $"at {device.ConnectionStatus.Identifier}" : ""; 
        
        var inputMessage = $"Do you wish to authorize the {sanitizedDeviceName} {sanitizedDeviceAddress}";

        AskForSelection(inputMessage, ["Yes", "No"]);
    }

    if (connectionStatus.Result?.output != null) {
        foreach (var line in connectionStatus.Result.output) { WriteDebugMessage(line); }
    }
}

ProcessResult? packageRetrievalResult;

// Add logic to save a config using the Device object.
(device, packageRetrievalResult) = await RunPackageRetrieval(device);

var packageCategoryInfo = ParsePackageProcessResult(packageRetrievalResult);

var packageNames = packageCategoryInfo.Values;