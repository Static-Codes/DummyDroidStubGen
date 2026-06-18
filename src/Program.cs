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
using static DummyDroidStubGen.Core.Common.InstallationChecks;
using static DummyDroidStubGen.Core.Helpers.IO.FileHelper;
using static DummyDroidStubGen.Core.Types.ADB.Connection;
using static DummyDroidStubGen.Functions;
using static DummyDroidStubGen.Global.Messaging;


// using System.Reflection;
// using DummyDroidStubGen.Core.Types.Packaging.Stub.Contents;
// using DummyDroidStubGen.Core.Types.Packaging;
// using DummyDroidStubGen.Core.Types.Packaging.Stub;
// var sfs = StubFileStructure.Create("/home/nerdy/.config/DummyDroidStubGen/Resources/", "my.test.package", "Resources/Android/DrawableVectors/cyclone.xml");
// Console.WriteLine(sfs.JavaCodeDir);
// var package = new Package("my.test.package", PackageCategory.Commercial, "MyTestPackage");

// AndroidManifest manifest = new(sfs, package);
// manifest.Write();

// Environment.Exit(1);

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

// Has the user choose a package retrievel method, either:
// 1. PackageRetrievalType.APP_NAME
// 2. PackageRetrievalType.PACKAGE_NAME
// The selected retrieval method is used to return a list of installed packages.
device.SetInstalledPackages();



WriteInformation("Preparing build info menu..");
Thread.Sleep(1500);

// foreach (var package in device.InstalledThirdPartyPackages) {
//     Console.WriteLine(package.Name);
// }

// var desiredPackage = new Package(desiredPackageName, PackageCategory.Application, desiredPackageLabel);
