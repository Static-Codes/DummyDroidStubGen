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
namespace DummyDroidStubGen.Core.Types;

using ADB.Wireless;
using CliWrap;
using CliWrap.Buffered;
using Core.Extensions;
using CPU;
using DummyDroidStubGen.Core.Helpers;
using Packaging;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Versioning;


using static ADB.Connection;
using static ADB.Connection.ConnectionMethod;
using static Common.InputValidation;
using static Functions;
using static Global.Constants;
using static Global.Messaging;
using static Helpers.IO.FileHelper;
using static Helpers.IO.InputHelper;
using static Helpers.PSIHelper;
using static Packaging.Stub.Contents.ShellCode;

public class Device
{
    /// <summary> 
    ///     The name of the connected device, as is reported by the Android Debug Bridge.
    /// </summary>
    public string Name { get; set; }


    /// <summary> 
    ///     The version of the Android OS running on the specified device.
    /// </summary>
    public AndroidOSVersion AndroidOSVersion { get; set; } = AndroidOSVersion.UNKNOWN;


    /// <summary> 
    ///     The version of the Android API that was bundled with the version of Android running on the specified device.
    /// </summary>
    public int AndroidAPILevel { get; set; } = (int)AndroidOSVersion.UNKNOWN;


    /// <summary> 
    ///     Holds internal state data that is used to identify if a device is connected to adb. 
    /// </summary>
    public ConnectionStatus ConnectionStatus { get; set; }


    /// <summary> 
    ///     Holds the Device IP, Pairing Port, and Pairing Code. (If WIFI pairing is used.) 
    /// </summary>
    public PairingInfo? WirelessPairingInfo { get; set; }


    /// <summary> 
    ///     A DeviceProperties object containing the results of `adb shell getprops` 
    /// </summary>
    public DeviceProperties? Properties { get; set; }


    /// <summary> The architecture of the current device's CPU. </summary>
    public CPUArchitecture ProcessorArchitecture { get; set; } = CPUArchitecture.UNKNOWN;
    

    /// <summary> The page size supported by the device's CPU </summary>
    private CPUPageSize PageSize { get; set; } = CPUPageSize.UNKNOWN;


    /// <summary> If the device has a CPU that supports 16 kilobyte page sizes. </summary>
    public bool PageSizeIs16KB { get; set; }


    /// <summary> A list containing a list of package objects for non-system services. </summary>
    public List<Package> InstalledThirdPartyPackages { get; set; } = [];

    public PackageRetrievalType RetrievalType { get; set; } = PackageRetrievalType.UNSET;

    /// <summary> 
    ///     The identifier associated with the Android device. <br/> 
    ///     When using USB Pairing, this identifier is the device's serial number. <br/> 
    ///     When using WIFI Pairing, this identifier is the IP:Port used to connect with ADB. <br/> 
    /// </summary>
    public string? ID { get; set; }
    
    /// <summary> The serial number of the connected Android device. </summary>
    public string? SerialNumber { get; set; }


    /// <summary> The internal codename associated with the current Android Device. </summary>
    public string Codename { get; set; }


    /// <summary> This is the normal constructor which will be used for object creation. </summary>
    public Device(string? name = null, AndroidOSVersion? androidOSVersion = null, ConnectionStatus? connectionStatus = null, ConnectionMethod? connectionMethod = null, PairingInfo? wirelessPairingInfo = null, string? id = null, string? serialNumber = null, string? codename = null)
    {
        Name = name ?? "Unknown";
        AndroidOSVersion = androidOSVersion ?? AndroidOSVersion.UNKNOWN;
        
        ConnectionStatus = connectionStatus ?? new(
            Connected: false, 
            Method: connectionMethod ?? AskForConnectionMethod(), 
            Output: null, 
            Result: null
        );
        
        WirelessPairingInfo = wirelessPairingInfo;
        
        ID = id ?? "Unknown";

        SerialNumber = serialNumber ?? "Unknown";
        Codename = codename ?? "Unknown";
    }

    /// <summary>
    ///      This constructor is used when instantiating a new object of the type Device. <br/>
    ///      The capitalized "Name" parameter is used to prevent ambiguity between this constructor, and the normal primary constructor above.
    /// </summary>

    [JsonConstructor]
    public Device(string? Name, ConnectionStatus? ConnectionStatus = null, AndroidOSVersion? androidOSVersion = null, ConnectionMethod? ConnectionMethod = null, PairingInfo? WirelessPairingInfo = null, string? ID = null, string? SerialNumber = null, string? Codename = null)
    {
        var defaultValue = "Unknown";
        
        this.Name = Name ?? defaultValue;
        AndroidOSVersion = androidOSVersion ?? AndroidOSVersion.UNKNOWN;
        
        this.ConnectionStatus = ConnectionStatus ?? new(
            Connected: false, 
            Method: ConnectionMethod ?? AskForConnectionMethod(), 
            Output: null, 
            Result: null
        );
        
        this.WirelessPairingInfo = WirelessPairingInfo;
        
        this.ID = ID ?? defaultValue;
        this.SerialNumber = SerialNumber ?? defaultValue;

        this.Codename = Codename ?? defaultValue;
    }


    /// <summary> Uses a StringBuilder to create the command that will be executed in GetPackagesAsync() </summary>
    private static string BuildRetrievalCommand() 
    {
        return new StringBuilder()
            // Start of Outer Loop -> Querying and iterating through each user handle on the current device.
            .AppendLine("for user in $(pm list users | cut -f2 -d'{' | cut -f1 -d':'); do")
            
            // Start of Inner Loop -> Querying and iterating through each third party package in the current user handle.
            .AppendLine("for pkg in $(pm list packages -3 --user $user | cut -d: -f2); do", spaces: 4)
            
            // Parsing the base.apk codepath of the current package in the iteration.
            .AppendLine("path=$(pm path --user $user $pkg | grep 'base.apk$' | cut -d: -f2)", spaces: 8)
            
            // Ensuring the base.apk path stored in $path is set prior to echoing the result.
            .AppendLine("[ ! -z \"$path\" ] && echo \"$pkg|$path\"", spaces: 8)
            
            // End of Inner Loop
            .AppendLine("done", spaces: 4)
            
            // End of Outer Loop
            .AppendLine("done")
            
            .ToString();
    }



    /// <summary>
    ///     Attempts to connect a paired device to the host machine over the Debug Service Port. <br/>
    ///     If this connection fails, execution haults immediately. <br/> 
    ///     Modifies the associated Device object following successful execution.
    /// </summary>
    private async Task ConnectPairedDeviceOverWifi() 
    {
        string deviceIP;
        string debugPort;
        
        // If a pre-existing device connection is present, that information is used to make the bridge connection.
        // Otherwise the user is prompted for a debug service port to make the connection via ADB.
        (deviceIP, debugPort) = (ConnectionStatus.Connected && ID != null) switch
        {
            true => (
                ID!.Split(':').ElementAt(0), 
                ID!.Split(':').ElementAt(1)
            ),
            false => (
                WirelessPairingInfo?.IP ?? AskForInput("Device IP: "), 
                PromptForConnectionInfo(WirelessPairingInfo?.IP ?? AskForInput("Device IP: ")).port
            )
        };

        var deviceName = ConnectionStatus.Output?.DeviceName ?? "device";
        WriteInformation(
            $"Attempting to connect the current system to the {deviceName} at {deviceIP}:{debugPort}"
        );

        var connectionProcess = await RunProcessAsync(
            psi: DeviceConnectionPSI(deviceIP, debugPort)
        );

        if (connectionProcess.ExitCode != 0) {
            WriteErrorMessage("Unable to connect the current system to the device at the specified address.");
            WriteErrorMessage(
                message: $"The process returned a non-zero status code of {connectionProcess.ExitCode}.",
                exit: true,
                exitCode: 1
            );
        }

        WriteInformation("Waiting five seconds for any remaining ADB requests to process.");
        await Task.Delay(5000);
        
        // Checking `adb -d
        var connectionStatus = await CheckForDeviceConnection();

        if (!connectionStatus.Connected) 
        {
            WriteWarningMessage("Unable to connect the current system to the device at the specified address.");
            WriteErrorMessage(
                message: "ADB couldnt detect any connected devices, please clear the pairing and try again.",
                exit: true,
                exitCode: 1
            );
        }

        ID = connectionStatus.Identifier;
        
        ConnectionStatus = connectionStatus;


        WriteSuccessMessage($"Connected to a {connectionStatus.Output?.DeviceName ?? "device"} at address: {deviceIP}:{debugPort}");
        WriteWarningMessage(
            "If you didn't receive a notification that wireless debugging was connected, please clear the pairing and try again."
        );

        WriteSuccessMessage("Waiting two seconds for any remaining ADB requests to process.");
        await Task.Delay(2000);
    }


    /// <summary> 
    ///     Creates an instance of a Device object using the specified name and connection status.
    /// </summary>
    public static Device CreateDevice(string deviceName, ConnectionStatus connectionStatus) 
    {
        if (!connectionStatus.Connected) {
            return new Device(name: deviceName);
        }

        return new Device(
            Name: deviceName,
            ConnectionStatus: connectionStatus
        );
    }


    /// <summary>
    ///     Uses the current device's PackageManager to return a list of packages and their base APK code paths. <br/>
    ///     
    ///     Due to limitations with PackageManager, the app name for each of these packages must be queried separately.
    /// </summary>
    private async Task<List<Package>> GetPackagesWithoutLabelsAsync() 
    {
        string retrievalCommand = BuildRetrievalCommand();

        List<Package> packages = [];

        try 
        {
            // Streaming the command's bytes to a PipeSource for use below.
            var commandBytes = Encoding.UTF8.GetBytes(retrievalCommand);
            var inputPipe = PipeSource.FromBytes(commandBytes);

            var result = await Cli.Wrap(ADBPath)
                .WithArguments([
                    GetStartingADBArgument(), 
                    "shell", 
                    "sh" // The piped input failed to properly execute without chaining '/system/bin/sh'.
                ])
                .WithStandardInputPipe(inputPipe)
                // .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync(Encoding.UTF8);

            if (result.ExitCode != 0) {
                throw new Exception(
                    $"The process associated with GetPackagesWithoutLabelsAsync() returned a non-zero status code: {result.ExitCode}"
                );
            }
            
            #if DEBUG
                WriteDebugMessage(result.StandardOutput); 
            #endif

            var sanitizedResults = result.StandardOutput.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in sanitizedResults) 
            {
                var parts = line.Split('|');
                if (parts.Length >= 2) 
                {
                    packages.Add(new Package(
                        name: parts[0], 
                        category: PackageCategory.Commercial,
                        baseCodePath: parts[1]
                    ));
                }
            }
        }

        catch (Exception ex) {
            WriteWarningMessage("Unable to retrieve a list of installed third party packages on the current device.");
            WriteErrorMessage(ex.Message);
        }

        return packages;
    }

    /// <summary>
    ///     Uses the current device's PackageManager to return a list of packages and their base APK code paths. <br/>
    ///     
    ///     Due to limitations with PackageManager, the app name for each of these packages must be queried separately.
    /// </summary>
    private static async Task<List<Package>> GetPackagesWithLabelsAsync() 
    {
        var appName = "OnDeviceLabelFetcher";
        var packageName = "com.staticcodes.odlf";

        var runScriptPath = Path.Combine(ODLFSubDirectory, RunFileName);
        var buildScriptPath = Path.Combine(ODLFSubDirectory, BuildFileName);

        var buildFileNameSet = PermissionHelper.TrySetExecutablePermissions(buildScriptPath);
        var runFileNameSet = PermissionHelper.TrySetExecutablePermissions(runScriptPath);

        List<Package> packages = [];

        try 
        {
            var result = await Cli.Wrap(runScriptPath)
                      .WithArguments([appName, packageName])
                      .WithWorkingDirectory(ODLFSubDirectory)
                      .WithEnvironmentVariables(env => env.Set("HOME", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)))
                      // .WithValidation(CommandResultValidation.None)
                      .ExecuteBufferedAsync(Encoding.UTF8);

            if (result.ExitCode != 0) {
                WriteDebugMessage(result.StandardOutput); 
                throw new Exception(
                    $"The process associated with GetPackagesWithLabelsAsync() returned a non-zero status code: {result.ExitCode}"
                );
            }
            
            #if DEBUG
                WriteDebugMessage(result.StandardOutput); 
            #endif

            var sanitizedResults = result.StandardOutput.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in sanitizedResults) 
            {
                var parts = line.Split('|');
                if (parts.Length >= 2) 
                {
                    packages.Add(new Package(
                        name: parts[0], 
                        category: PackageCategory.Commercial,
                        baseCodePath: parts[1]
                    ));
                }
            }
        }

        catch (Exception ex) {
            WriteWarningMessage("Unable to retrieve a list of installed third party packages on the current device.");
            WriteErrorMessage(ex.Message);
        }

        return packages;
    }


    /// <summary> 
    ///     Returns "-d" if the device is connected over USB or string.Empty otherwise. 
    /// </summary>
    private string GetStartingADBArgument() {
        return (ConnectionStatus.Method == USB) ? "-d" : string.Empty;
    }


    /// <summary> 
    ///     Returns true if the device is currently paired over ADB using WIFI, otherwise false.
    /// </summary>
    private bool IsPairedOverWIFI() 
    {
        return 
            !string.IsNullOrEmpty(SerialNumber) && 
            ID != null && 
            ID.Contains(':');
    }

    
    /// <summary> Calls PairDeviceOverWifi() and ConnectPairedDeviceOverWifi() </summary
    private async void PairAndConnectDeviceOverWifi()
    {
        // Pairing the device to adb over TCP.
        await PairDeviceOverWifi();

        // Connecting to the paired device over adb.
        await ConnectPairedDeviceOverWifi();
    }


    /// <summary>
    ///     Pairs a device over WiFi (if it is not already paired). <br/>
    ///     If this pairing fails, execution haults immediately. <br/> 
    ///     Returns a modified Device object following successful execution.
    /// </summary>
    private async Task PairDeviceOverWifi() 
    {
        if (IsPairedOverWIFI()) {
            return;
        }

        // If pairing is required the user is prompted for the device IP.
        string deviceIP = AskForInput("Device IP: "); 
        PairingInfo pairingInfo = PromptForPairingInfo(deviceIP);
        
        // Validating the provided input.
        DoPortValidation(ref pairingInfo);
        DoCodeValidation(ref pairingInfo);
            
        // Assigning the validated input (assuming execution didnt hault due to invalid input).
        WirelessPairingInfo = pairingInfo;

        var pairingResult = await RunProcessAsync
        (
            psi: DevicePairingPSI(
                WirelessPairingInfo.IP, 
                WirelessPairingInfo.Port, 
                WirelessPairingInfo.Code
            )
        );

        if (pairingResult.ExitCode != 0) 
        {
            WriteWarningMessage("Unable to pair the current system to the device at the specified address.");
            WriteErrorMessage(
                message: $"The process returned a non-zero status code of {pairingResult.ExitCode}.",
                exit: true,
                exitCode: 1
            );
        }

        // Adding a 2 second delay
        WriteSuccessMessage("Pairing successful, waiting two seconds for on-device processing.");
        await Task.Delay(2000);

    }


    /// <summary> 
    ///     Updates the values for Device.PageSize and Device.PageSizeIs16KB 
    /// </summary>
    private void SetCPUArchitecture() 
    {
        // Since UpdateProperties() is called prior to this function, Properties is guaranteed to not be null here.
        ProcessorArchitecture = Properties!.GetCPUArchitecture();
    }


    /// <summary> 
    ///     Attempts to parse the Android OS Version from the provided DeviceProperties object. <br/>
    ///     Also updates the AndroidAPILevel based on the associated Android OS Version. 
    /// </summary>
    private void SetOSVersion() 
    {
        // Since UpdateProperties() is called prior to this function, Properties is guaranteed to not be null here.
        AndroidOSVersion = Properties!.GetOSVersion();
        AndroidAPILevel = (int)AndroidOSVersion;
    }
    

    /// <summary> 
    ///     Instructs the user to select a package resolution method, then updates Device object's internal package list.
    /// </summary> 
    public async Task SetInstalledPackagesAsync() 
    {
        WriteInformation("DDSG provides two methods to select your desired package.");
        WriteInformation(
            coloredText: "Using a package name (com.developer.packagename)", 
            textColor: "purple",
            tagName: "[[METHOD 1]]:",
            tagNameColor: "orange",
            reverse: true
        );

        WriteInformation(
            coloredText: "Using an app name (Requires sideloading OnDeviceLabelFetcher)", 
            textColor: "purple",
            tagName: "[[METHOD 2]]:",
            tagNameColor: "orange",
            reverse: true
        );

        var selection = AskForSelection(
            message: "Please select a method to continue", 
            options: ["Select using an app name (Recommended)", "Select using a package name"]
        );

        // Exits if the user has chosen "Exit"
        UserExitStatusCheck(selection);

        // Handles both APP_NAME and PACKAGE_NAME
        SetRetrievalType
        (
            selection switch {
                "Select using an app name (Recommended)" => PackageRetrievalType.APP_NAME,
                _ => PackageRetrievalType.PACKAGE_NAME
            }
        );
        
        WriteWarningMessage("Please implement logic to retrieve packages by label name in Device.cs");

        await UpdateInstalledPackages(usingLabels: RetrievalType == PackageRetrievalType.APP_NAME);
    }


    /// <summary> 
    ///     Updates the values for Device.PageSize and Device.PageSizeIs16KB 
    /// </summary>
    private void SetPageSize() 
    {
        // Since UpdateProperties() is called prior to this function, Properties is guaranteed to not be null here.
        PageSize = Properties!.GetCPUPageSize();
        PageSizeIs16KB = PageSize == CPUPageSize._16KB;
    }


    /// <summary> 
    /// Updates the Device object's internal PackageRetrievalType (if the provided value differs from the current one).
    /// </summary>
    private void SetRetrievalType(PackageRetrievalType retrievalType) 
    {
        if (RetrievalType != retrievalType) {
            RetrievalType = retrievalType;
        }
    }


    /// <summary> 
    ///     Updates the current Device object's Serial Number using the value returned by GetProperties().
    /// </summary>
    private void SetSerialNumber() {
        SerialNumber = Properties!.GetSerialNumber();
    }


    /// <summary>
    ///     Sets InstalledThirdPartyPackages for the current device object. <br/>
    ///     
    ///     This will be called by SetInstalledPackages() when the user selects "Select using a package name".
    /// </summary>
    private async Task UpdateInstalledPackages(bool usingLabels = false)
    {
        var isUSB = ConnectionStatus.Method == USB;

        if (isUSB && !ConnectionStatus.Connected) {
            WriteErrorMessage(
                message: $"No connected devices were detected.\n\n{ConnectionSection}",
                exit: true,
                exitCode: 1
            );
        }

        // If the current device is connected, the method is guaranteed to be set.
        // If the current device is connected over USB, then ID becomes the device's serial number.
        if (isUSB) {
            ID = SerialNumber; 
        }
        else {
            PairAndConnectDeviceOverWifi();
        }

        WriteInformation("Starting retrieval operations, please wait...");

        // Updating the device object's internal state to track the parsed packages.
        InstalledThirdPartyPackages = usingLabels switch {
            true => await GetPackagesWithLabelsAsync(),
            false => await GetPackagesWithoutLabelsAsync()
        };
    }


    /// <summary> 
    ///     Attempts to parse the Android OS Version from the provided DeviceProperties object. <br/>
    ///     
    ///     Also updates the AndroidAPILevel based on the associated Android OS Version. <br/>
    ///     
    ///     Finally, the memory associated with the reference parameter "properties" is freed.
    /// </summary>
    public void UpdateProperties(ref DeviceProperties? properties) 
    {
        
        Properties = properties;
        properties = null;

        if (Properties == null) {
            WriteWarningMessage("Unable to access the required properties from the connected device.");
            WriteErrorMessage("Properties is null in UpdateProperties()", exit: true, exitCode: 1);
        }

        SetOSVersion();
        SetCPUArchitecture();
        SetPageSize();
        SetSerialNumber();
        
    }
}