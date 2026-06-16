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
namespace DummyDroidStubGen.Core.Types.ADB;


using static Common.RegexPatterns;
using static Connection.ConnectionMethod;
using static Helpers.PSIHelper;
using static Functions;
using static Global.Messaging;

public class Connection 
{
    public enum ConnectionMethod { USB, WIFI }
    
    /// <summary> An object representing key values that will be pulled from calls to adb. <br/>
    /// Index 0 -> Full line associated with the match. <br/>
    /// Index 1 -> "device_id" <br/>
    /// Index 2 -> "ip <br/>
    /// Index 3 -> "port" <br/>
    /// Index 4 -> "device_name" <br/>
    /// Index 5 -> "codename" <br/>
    /// Index 6 -> "transport_id" <br/>
    /// </summary>
    public class ConnectionOutput(
        string? DeviceID = null, 
        string? DeviceIP = null, 
        string? DebugPort = null, 
        string? DeviceName = null, 
        string? Codename = null, 
        string? TransportID = null
    ) 
    { 
        // Index 0 -> Full line associated with the match. 
        // Index 1 -> "device_id"
        // Index 2 -> "ip
        // Index 3 -> "port"
        // Index 4 -> "device_name"
        // Index 5 -> "codename"
        // Index 6 -> "transport_id"

        public string DeviceID { get; set; } = DeviceID ?? "Unknown";
        public string IP { get; set; } =  DeviceIP ?? "";
        public string Port { get; set; } = DebugPort ?? "";
        public string DeviceName { get; set; } = DeviceName ?? "Unknown Android Device";
        public string Codename { get; set; } = Codename ?? "Unknown";
        public string TransportID { get; set; } = TransportID ?? "Unknown";

    }

    public class ConnectionStatus(bool Connected, ConnectionMethod? Method, ConnectionOutput? Output, ProcessResult? Result, string? Identifier = null) 
    {
        /// <summary> If a device is currently connected </summary>
        public bool Connected { get; set; } = Connected;

        /// <summary> The current connection method (if a device is connected) </summary>
        public ConnectionMethod? Method { get; set; } = Method;


        /// <summary> 
        ///     The sanitized output from the StandardOutput of the process associated with the call to "adb connect 'ID'"
        /// </summary>
        public ConnectionOutput? Output { get; set; } = Output;

        /// <summary> The raw ProcessResult object associated with the connecting process. </summary>
        public ProcessResult? Result { get; set; } = Result;

        /// <summary> The Identifier associated with the connected device (either a GUID or Device Address) </summary>
        public string? Identifier { get; set; } = Identifier;

    }
    
    public enum ConnectionType { Existing, New }
    
    
    /// <summary> 
    ///     Performs a device connection check by executing "/usr/bin/adb devices -l". <br/>
    /// 
    ///     The output from this command is parsed with a call to DoDeviceConnectionRegex(). <br/> 
    ///
    ///     The result of the call to DoDeviceConnectionRegex() will be of the type ConnectionStatus.
    /// </summary>
    public static async Task<ConnectionStatus> CheckForDeviceConnection() 
    {
        WriteInformation(
            coloredText: "Verifying device connection status over ADB..\n", 
            tagNameColor: "purple", 
            reverse: true
        );

        var psi = DeviceConnectionCheckPSI();

        var processResult = await RunProcessAsync(psi);

        if (processResult.Exception != null) {
            throw processResult.Exception;
        }

        # if DEBUG
            foreach (var line in processResult.Output) { WriteDebugMessage(line); }
            foreach (var line in processResult.Error) { WriteDebugMessage(line); }
        #endif

        return DoDeviceConnectionRegex(processResult);
    }


    /// <summary>
    ///     Performs a match check using a regex pattern that will handle both WIFI and USB connections through ADB. <br/>
    /// 
    ///     Will exit if more than one device is connected. <br/>
    ///     
    ///     Returns a ConnectionStatus object, regardless of connection status (unless the condition above is met first).
    /// </summary> 
    private static ConnectionStatus DoDeviceConnectionRegex(ProcessResult connectionResult) 
    {   
        var matches = connectionResult.Output
            .Where(line => !string.IsNullOrWhiteSpace(line) && line != "List of devices attached")
            .Select(line => ConnectionRegex().Match(line))
            .Where(match => match.Success)
            .FirstOrDefault();

        if (matches == null ) {
            WriteWarningMessage("No connected devices were located over ADB..");
            return new ConnectionStatus(
                Connected: false, 
                Method: null, 
                Output: null, 
                Result: connectionResult
            );
        }

        var usingUSB = matches.Groups["USBDeviceID"].Success;
        var usingWIFI = matches.Groups["IP"].Success;

        if (usingUSB)
        {
            var connectionOutput = new ConnectionOutput(
                DeviceID: matches.Groups["USBDeviceID"].Value,
                DeviceName: matches.Groups["USBDeviceName"].Value.Replace("_", " "),
                Codename: matches.Groups["USBDeviceCodename"].Value,
                TransportID: matches.Groups["USBTransportID"].Value
            );
            
            return new ConnectionStatus(
                Connected: true, 
                Method: USB, 
                Output: connectionOutput, 
                Result: connectionResult, 
                Identifier: connectionOutput.DeviceID
            );
        }

        else if (usingWIFI)
        {
            var output = new ConnectionOutput(
                DeviceIP: matches.Groups["IP"].Value,
                DebugPort: matches.Groups["Port"].Value,
                DeviceName: matches.Groups["DeviceName"].Value.Replace("_", " "),
                Codename: matches.Groups["WIFIDeviceCodename"].Value,
                TransportID: matches.Groups["TransportID"].Value
            );

            return new ConnectionStatus(
                Connected: true, 
                Method: WIFI, 
                Output: output, 
                Result: connectionResult, 
                Identifier: $"{output.IP}:{output.Port}"
            );
        }

        return new ConnectionStatus(
            Connected: false, 
            Method: null, 
            Output: null, 
            Result: connectionResult
        );
    }
    
}