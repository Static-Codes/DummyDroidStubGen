using DummyDroidStubGen.Core.Helpers;
using DummyDroidStubGen.Core.Types;
using DummyDroidStubGen.Core.Types.Packaging;
using static DummyDroidStubGen.Core.Common.InstallationChecks;
using static DummyDroidStubGen.Core.Helpers.FileHelper;
using static DummyDroidStubGen.Core.Types.ADB.Connection.ConnectionMethod;
using static DummyDroidStubGen.Functions;
using static DummyDroidStubGen.Global.Messaging;


var binaryCheckResults = CheckForRequiredBinaries();

foreach (var binaryResult in binaryCheckResults) {
    WriteSuccessMessage($"Located required package: {binaryResult.BinaryName}");
}

CreateAppDataSubDirectory();

var connectionStatus = await CheckForDeviceConnection();
string deviceName = GetNameOfConnectedDevice(connectionStatus);

Device device = CreateDevice(deviceName, connectionStatus);

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

else {
    AskForDeviceConfirmation(device, connectionStatus, deviceName, message);
}

var devicePropertiesObj = new DeviceProperties();
await devicePropertiesObj.LoadAsync(device.ConnectionStatus.Method == USB);

// Create a single function that takes DeviceProperties as a parameter and executes these mutator functions.
device.SetAndroidOSVersion(devicePropertiesObj);


ProcessResult? packageRetrievalResult;

// Add logic to save a config using the Device object.
(device, packageRetrievalResult) = await RunPackageRetrieval(device);

var packageCategoryInfo = ParsePackageProcessResult(packageRetrievalResult);

var packageNames = packageCategoryInfo.Values;


WriteInformation("Preparing build info menu..");
Thread.Sleep(1500);

var desiredPackageName = InputHelper.AskForInput("Please the name of the package you wish to open using this stub: ");
var desiredPackageLabel = InputHelper.AskForInput("Please the name of the app associated with the package above: ");

var desiredPackage = new Package(desiredPackageName, PackageCategory.Application, desiredPackageLabel);

var iconPaths = await GetIcon(desiredPackage.Name, device.ConnectionStatus.Method == USB);

Console.WriteLine(iconPaths);