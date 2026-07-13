/*
 * Copyright (C) 2026 Static Codes
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using DummyDroidStubGen.Core.Types;
using DummyDroidStubGen.Core.Types.Packaging;
using DummyDroidStubGen.Core.Types.Packaging.Stub;
using System.IO.Compression;
using static DummyDroidStubGen.Core.Common.InstallationChecks;
using static DummyDroidStubGen.Core.Common.RegexPatterns;
using static DummyDroidStubGen.Core.Helpers.IO.FileHelper;
using static DummyDroidStubGen.Core.Helpers.IO.InputHelper;
using static DummyDroidStubGen.Core.Types.ADB.Connection;
using static DummyDroidStubGen.Core.Types.Packaging.PackageRetrievalType;
using static DummyDroidStubGen.Functions;
using static DummyDroidStubGen.Global.Constants;
using static DummyDroidStubGen.Global.Messaging;
using static NativeFileDialogSharp.Dialog;
using static System.OperatingSystem;


if (!IsLinux() && !IsFreeBSD()) {
    WriteWarningMessage("DDSG only supports Linux.");
    WriteInformation("You can run a live environment like Debian or Mint, without installing Linux.");
    WriteInformation($"For more information see: {LiveEnvironmentLink}");
    Environment.Exit(1);
}

var binaryCheckResults = CheckForRequiredBinaries();

foreach (var binaryResult in binaryCheckResults) {
    WriteSuccessMessage($"Located required package:\n\t\t{binaryResult.BinaryName} -> {binaryResult.BinaryPath}\n");
}

await CreateRequiredDirectories();

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
        name: deviceName,
        connectionMethod: device.ConnectionStatus.Method
    );
}

else {
    AskForDeviceConfirmation(connectionStatus, message);
}

var devicePropertiesObj = new DeviceProperties();
await devicePropertiesObj.LoadAsync(device);


// Updates device.Properties
// Calls internal mutator functions
// Frees the memory associated with devicePropertiesObj
device.UpdateProperties(ref devicePropertiesObj);

await device.SetUserProfiles();


// Has the user choose a package retrievel method, either:
// 1. PackageRetrievalType.APP_NAME
// 2. PackageRetrievalType.PACKAGE_NAME
// The selected retrieval method is used to return a list of installed packages.
await device.SetInstalledPackagesAsync();


WriteInformation("Preparing build info menu..");

// #if DEBUG
//     foreach (var thirdPartyPackage in device.InstalledThirdPartyPackages) {
//         Console.WriteLine(thirdPartyPackage.Label);
//     }
// #endif

var usingLabels = device.RetrievalType is APP_NAME;

var packageSelection = AskForSelection(
    message: "Please select the application you wish to be launched by the compiled stub.",
    options: device.GetMenuOptions(usingLabels)
);

Package? package;
string? ProjectName;

var resolutionSuccess = usingLabels switch {
    true  => device.TryGetPackageByLabel(packageSelection, out package),
    false => device.TryGetPackageByName(packageSelection, out package),
};

if (!resolutionSuccess) {
    WriteErrorMessage("Failed to resolve the package you selected.");
    WriteInformation($"This is likely a bug, please make a bug report at: {ProjectIssueLink}");
}

if (package!.Label is null || package.Label is "Unknown") {
    var rawPackageName = AskForInput("Please enter your desired name for the compiled stub: ");
    ProjectName = PackageSanitizationRegex().Replace(input: rawPackageName, replacement: "");
    package.Label = ProjectName;
} else {
    ProjectName = PackageSanitizationRegex().Replace(input: package.Label, replacement: "") + "Launcher";
}

var projectDirectory = Path.Combine(BuildHistorySubDirectory, ProjectName);

try {
    if (Directory.Exists(projectDirectory)) {
        var guid = Guid.NewGuid();
        var zipPath = $"{projectDirectory}-{guid}.zip";
        
        using var fileStream = File.Create(zipPath);
        ZipFile.CreateFromDirectory(projectDirectory, fileStream);
        await fileStream.DisposeAsync();
        
        Directory.Delete(projectDirectory, recursive: true);
        
        WriteSuccessMessage($"Backed up previous build to -> {zipPath}");
    }
}
catch (Exception ex) {
    WriteWarningMessage($"Failed to backup previous build at -> {projectDirectory}");
    WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
}

try {
    Directory.CreateDirectory(projectDirectory);
}
catch (Exception ex) {
    WriteWarningMessage($"Failed to create the current project directory at -> {projectDirectory}");
    WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
}



WriteInformation("DDSG supports SVG, XML, and WEBP files for icon generation");
WriteInformation("Please press O on your keyboard to open the Icon selection Dialog.", tagName: "[[INPUT]]:");
Thread.Sleep(200);

var userPressedKey = false;
while (!userPressedKey) { userPressedKey = Console.ReadKey().Key is ConsoleKey.O; }
Console.Clear();

// https://github.com/mlabbe/nativefiledialog#file-filter-syntax
var res = FileOpen(filterList: "xml,webp,svg");

(bool fileSelected, string? filePath, string? error) = true switch {
    _ when res.IsCancelled => (false, null, "User Cancelled."),
    _ when res.IsError     => (false, null, res.ErrorMessage),
    _ when res.IsOk        => (true,  res.Path, (string?)null),
    _                      => (false, null, "Unhandled case in switch statement in Program.cs")
};

if (!fileSelected || filePath is null) { WriteErrorMessage(error, exit: true, exitCode: 1); }

var sfs = await FileStructure.New(
    ProjectDirectory: projectDirectory, 
    PackageName: package.Name, 
    InputIconPath: filePath
);

var useWorkProfile = device.HasWorkProfileConfigured && UserWantsToInstallInWorkProfile();
var profileID = useWorkProfile ? "10" : "0";

var stubInfo = new StubInfo(package, sfs, profileID);
Generator.GenerateStub(stubInfo);



