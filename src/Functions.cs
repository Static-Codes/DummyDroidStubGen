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

namespace DummyDroidStubGen;

using Core.Helpers;
using Core.Types;
using Core.Types.ADB.Wireless;
using Core.Types.Packaging.Stub;
using System.Diagnostics;
using System.Threading.Tasks;
using static Core.Helpers.IO.FileHelper;
using static Core.Helpers.IO.InputHelper;
using static Core.Types.ADB.Connection;
using static Core.Types.ADB.Connection.ConnectionMethod;
using static Core.Types.Packaging.Stub.Contents.ShellCode;
using static Global.Constants;
using static Global.Messaging;

/// <summary> Contains the core functions that are used by DummyDroidStubGen (DDSG). </summary>
public class Functions 
{   
    /// <summary> 
    ///     Prompts the user to confirm if they wish to proceed with the connected device. 
    /// </summary>
    public static void AskForDeviceConfirmation(ConnectionStatus connectionStatus, string message) 
    {
        var confirmationSelection = AskForSelection(message, options: ["Yes", "No"]);

        UserExitStatusCheck(confirmationSelection);

        if (confirmationSelection == "No") {
            WriteInformation("Operation cancelled by user.");
            WriteInformation("Exiting.");
            Environment.Exit(1);
        }

        if (connectionStatus.Result?.Output != null) {
            foreach (var line in connectionStatus.Result.Output) { WriteDebugMessage(line); }
        }
    }

    /// <summary> Creates an instance of a Device object using the specified name and connection status. </summary>
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
    ///     Returns a string representing the name of the current connected device.
    /// </summary>
    public static string GetNameOfConnectedDevice(ConnectionStatus connectionStatus) 
    {
        return connectionStatus switch
        {
            { Connected: false} => "Unknown", // Device is unknown at this point

            { Connected: true, Method: USB } => $"Android Device (USB) @ {connectionStatus.Identifier}",


            { Connected: true, Output: { DeviceName: null, DeviceID: not null } } => 
                $"Android Device (WiFi) @ {connectionStatus.Identifier}",

            { Connected: true, Output: { DeviceName: not null, DeviceID: not null } } => 
                $"{connectionStatus.Output.DeviceName} (WiFi) @ {connectionStatus.Identifier}",

            _ => $"Android Device (WiFi) @ {connectionStatus.Identifier}"   
        };
    }

    /// <summary>
    ///     Processes the ConnectionStatus associated with the device passed as a parameter. <br/>
    ///     Returns a tuple with: <br/>
    ///     - bool isError: Indicates if a connection error is present. <br/>
    ///     - string message: Holding the contents of the response. <br/>
    ///     
    ///     If isError is false, then message is used when requesting user input. <br/>
    ///     If isError is true, then message is used as an error message.
    /// </summary>
    public static (bool isError, bool shouldExit, string message) HandleConnectionStatus(Device device) 
    {
        return device.ConnectionStatus switch 
        {
             { Connected: true, Method: USB, } => (
                isError: false, 
                shouldExit: false,
                message: $"Would you like to use the " +
                         $"{device.ConnectionStatus.Output?.DeviceName ?? "device"} " +
                         $"({device.ConnectionStatus.Identifier}) " +
                         "connected via USB?"
            ), 

            { Connected: true, Method: WIFI, } => (
                isError: false, 
                shouldExit: false,
                message: $"Would you like to use the " +
                         $"{device.ConnectionStatus.Output?.DeviceName ?? "device"} at " + 
                         $"{device.ConnectionStatus.Identifier}"
            ), 

            { Connected: false, Method: USB } => (
                isError: true,
                shouldExit: true, 
                message: "No Android device was detected over USB.\nPlease ensure USB Debugging is enabled."
            ),

            // Unlike USB connection, which does not require additional pairing, 
            // WIFI pairing will require additional user input.    
            { Connected: false, Method: WIFI } => (
                isError: true, 
                shouldExit: false, 
                message: "No device was connected via WIFI.\nPlease ensure WIFI Debugging is enabled."
            ),

            _ => throw new Exception("An exception has occured in HandleConnectionStatus() due to a missing pattern match clause.")
        };
    }

    /// <summary> 
    ///     Asks the user if they wish to view the compiled stub's source or proceed with the installation. 
    /// </summary>
    public static async Task HandleInstallationAsync(StubInfo stubInfo) 
    {
        var message = "Would you like to view the source code for the compiled stub before installing?";
        string[] options = ["Yes (Requires manual installation)", "No, continue with the installation"];

        var confirmationSelection = AskForSelection(message, options);

        UserExitStatusCheck(confirmationSelection);

        var parentDirectory = stubInfo.StubStructure.Directories.ProjectParent;
        var profileID = stubInfo.ProfileID;
        var packageName = stubInfo.TargetPackage.Name;
        var apkName = $"{stubInfo.TargetPackage.Label}Launcher.apk";

        if (confirmationSelection.StartsWith("Yes")) {
            var cmd = $"cd {parentDirectory} && adb install --user {profileID} {apkName}";
            WriteInformation("To manually install the compiled stub, please run the following command:");
            WriteInformation(whiteText: cmd, tagName: "[[COMMAND]]:", tagNameColor: "purple");
            await TryOpenDirectoryInDefaultBrowser(parentDirectory);
            Environment.Exit(0);
        }

        await RunStubInstallScriptAsync(parentDirectory, packageName, apkName);
    }

    /// <summary> Parses the contents returned from the command: "adb shell getprops" </summary>
    public static Dictionary<string, string> ParseDeviceProperties(List<string> commandOutput)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in commandOutput)
        {   
            // Ensuring empty lines and lines that don't contain bracket pairs are skipped.
            if (string.IsNullOrWhiteSpace(line) || line.Count(c => c == '[') < 2) {
                continue;
            }

            try 
            {
                // The parts of the line are split by the closing bracket to isolate each section.
                // Example: [Key]: [Value] -> ["", "Key", "Value", ""]
                var parts = line.Split(']');

                if (parts.Length < 3) {
                    continue;
                }

                int keyStart = parts[0].IndexOf('[');
                // Locating the opening bracket in part[0] and the opening bracket in part[1]
                int valStart = parts[1].IndexOf('[');

                if (keyStart != -1 && valStart != -1)
                {
                    string key = parts[0][(keyStart + 1)..];
                    string value = parts[1][(valStart + 1)..];
                    dict[key] = value;
                }
            }

            catch (Exception ex)
            {
                WriteWarningMessage($"Unable to parse device property: '{line}'");
                WriteErrorMessage(ex.Message);
            }
        }

        return dict;
    }


    /// <summary>
    ///     Provided a device IP, the user will be prompted for a debug service port. <br/>
    ///     A Tuple is returned (string deviceIP, string port).
    /// </summary>
    public static (string deviceIP, string port) PromptForConnectionInfo(string deviceIP) 
    {
        WriteInformation("The port to be used for connection will differ from the pairing port.");
        WriteInformation("Please refresh the 'Wireless debugging' tab and enter the updated values below.");

        return (
            deviceIP,
            AskForInput("Debug Service Port: ")
        );
    }


    /// <summary> Processing any error data received by a process spawned in RunProcessAsync() </summary>
    public static void ProcessErrorDataReceived(
        ref Process process,
        List<string> error, 
        TaskCompletionSource<bool> errorDone,
        DataReceivedEventHandler? errorHandler = null
    ) 
    {
        process.ErrorDataReceived += (sender, e) => 
        {
            if (e.Data == null) {
                errorDone.TrySetResult(true);
                return;
            }
            
            lock (error) {
                error.Add(e.Data.Trim());
            }
            errorHandler?.Invoke(sender, e);
        };
    }


    /// <summary> Processing any output data received by a process spawned in RunProcessAsync() </summary>
    public static void ProcessOutputDataReceived(
        ref Process process,
        List<string> output, 
        TaskCompletionSource<bool> outputDone,
        DataReceivedEventHandler? outputHandler = null
    ) 
    {
        process.OutputDataReceived += (sender, e) => 
        {
            if (e.Data == null) {
                outputDone.TrySetResult(true);
                return;
            }
            lock (output) {
                output.Add(e.Data.Trim());
            }
            outputHandler?.Invoke(sender, e);
        };
    }


    /// <summary>
    ///     Provided a device IP, the user will be prompted for a pairing port and pairing code. <br/>
    ///     A PairingInfo object is returned using deviceIP, and the entered pairing port and pairing code.
    /// </summary>
    public static PairingInfo PromptForPairingInfo(string deviceIP) 
    {
        WriteInformation($"To locate your pairing code, please go to:\n{WIFISetting}\n");

        return new(
            deviceIP,
            AskForInput("Pairing Service Port: "),
            AskForInput("Pairing Code: ")
        );
    }


    /// <summary> 
    ///     Provided a device IP and pairing port, the user will be prompted for a pairing code. <br/>
    ///     A PairingInfo object is returned using deviceIP, pairingPort, and the entered pairing code.
    /// </summary>
    public static PairingInfo PromptForPairingCodeOnly(string deviceIP, string pairingPort) 
    {
        WriteInformation($"To locate your pairing code, please go to:\n{WIFISetting}\n");

        return new(
            deviceIP,
            pairingPort,
            AskForInput("Pairing Code: ")
        );
    }


    /// <summary>
    ///     Given a ProcessStartInfo object, a process is spawned and executed. <br/>
    ///     Optionally, if an inputArg is passed, this argument is written out to the process' StandardInput. <br/>
    ///     Finally, Both outputHandler and errorHandler are both optional arguments. <br/>
    ///     Passing either of these may be passed to override the default behavior of StandardError and StandardOutput.
    /// </summary>
    public static async Task<ProcessResult> RunProcessAsync(
        ProcessStartInfo psi, 
        string? inputArg = null,
        DataReceivedEventHandler? outputHandler = null, 
        DataReceivedEventHandler? errorHandler = null
    )
    {
        List<string> output = [];
        List<string> error = [];
        
        Process process = new() { StartInfo = psi };

        var outputDone = new TaskCompletionSource<bool>();
        var errorDone = new TaskCompletionSource<bool>();

        ProcessOutputDataReceived(ref process, output, outputDone, outputHandler);
        ProcessErrorDataReceived(ref process, error, errorDone, errorHandler);

        Exception? exception = null;
        uint exitCode;

        try 
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (inputArg != null) {
                exitCode = await WriteInputToProcessStandardInput(process, inputArg, outputDone, errorDone);
            }

            else {
                await process.WaitForExitAsync();
                exitCode = (uint)process.ExitCode;
            }
        }

        catch (Exception ex) {
            exception = ex;
            exitCode = 1;
            
            // Ensuring the outstanding tasks complete if the process.Start() fails.
            outputDone.TrySetResult(false);
            errorDone.TrySetResult(false);
        }

        return new ProcessResult(output, error, exitCode, exception);
    }


    private static async Task RunStubInstallScriptAsync(string parentDirectory, string packageName, string apkName)
    {
        if (!Directory.Exists(parentDirectory))
        {
            WriteWarningMessage("An exception occured while trying to install the compiled stub.");
            WriteErrorMessage($"Invalid directory: {parentDirectory}", exit: true, exitCode: 1);
        }

        var filePath = Path.Combine(parentDirectory, RunFileName);

        if (!File.Exists(filePath)) {
            WriteWarningMessage("An exception occured while trying to install the compiled stub.");
            WriteErrorMessage($"Could not locate: {filePath}", exit: true, exitCode: 1);
        }

        var psi = new ProcessStartInfo()
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"./{RunFileName} '{packageName}' '{apkName}'\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = parentDirectory
        };

        DataReceivedEventHandler? outputHandler = null;
        DataReceivedEventHandler? errorHandler = null;

        if (!PermissionHelper.TrySetExecutablePermissions(filePath)) {
            WriteErrorMessage("Unable to give executable permissions to the installer script.");
            WriteInformation("To continue, please run the following command, then restart DDSG.");
            WriteInformation($"chmod +x {filePath}", tagName: "[[COMMAND]]:");
            Environment.Exit(1);
        }

        var process = await RunProcessAsync(psi, inputArg: null, outputHandler, errorHandler);

        foreach (var line in process.Output) { Console.WriteLine(line); }
        foreach (var line in process.Error) { Console.WriteLine(line); }
        Console.WriteLine();
        Console.WriteLine("Exit: " + process.ExitCode);

        


    }

    public static bool UserWantsToInstallInWorkProfile() {
        return AskForSelection("Please select a profile", ["Home", "Work/Sandbox"]).Equals("Work/Sandbox");
    }

    /// <summary>
    ///     Writes an input string to the associated process's StandardInput stream. <br/>
    ///     Exits if the stream does not support writing.
    /// </summary>
    private static async Task<uint> WriteInputToProcessStandardInput(
        Process process, 
        string inputArg, 
        TaskCompletionSource<bool> outputDone, 
        TaskCompletionSource<bool> errorDone
    ) 
    {
        using StreamWriter writer = process.StandardInput;

        // Determining whether the calling process' STDInput Stream supports writing in the current context.
        if (!writer.BaseStream.CanWrite) 
        {
            WriteErrorMessage(
                message: "Unable to enter the input provided, please try again over USB.", 
                exitCode: 1, 
                exit: true
            );
        }
        await writer.WriteAsync(inputArg);

        await process.WaitForExitAsync();

        // Allowing all handlers to finish processing before continuing.
        await Task.WhenAll(outputDone.Task, errorDone.Task);

        return (uint)process.ExitCode;
    }
}