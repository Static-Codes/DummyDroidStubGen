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
namespace DummyDroidStubGen.Core.Helpers;

using System.Diagnostics;
using static Global.Constants;

public class PSIHelper 
{
    /// <summary> Executes: adb shell pm list packages -3 and parses the output using cut. </summary>
    private static ProcessStartInfo CreateADBShellCommandPSI(string command, bool isUSB = true)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ADBPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // ADB Shell uses -d for to indicate a direct (USB) connection is in use.
        if (isUSB) {
            startInfo.ArgumentList.Add("-d");
        }

        startInfo.ArgumentList.Add("shell");
        startInfo.ArgumentList.Add(command);

        return startInfo;
    }

    public static ProcessStartInfo DeviceConnectionCheckPSI() 
    {
        var psi = new ProcessStartInfo() {
            FileName = ADBPath,
            Arguments = "devices -l",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Allowing ADB to access the RSA keys stored in $HOME/.adb (likely not needed, but useful for debugging.)
        psi.EnvironmentVariables["HOME"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // Allowing ADB to access other Environment Variables (required on some configurations)
        psi.EnvironmentVariables["PATH"] = Environment.GetEnvironmentVariable("PATH");

        return psi;
    }

    /// <summary> Executes: adb connect {ip}:{port} </summary>
    public static ProcessStartInfo DeviceConnectionPSI(string ipAddress, string port) 
    {
        return CreateADBShellCommandPSI(command: $"connect {ipAddress}:{port}", isUSB: false);
    }


    public static ProcessStartInfo DevicePairingPSI(string ipAddress, string port, string code) 
    {
        return new() {
            FileName = "/bin/bash",
            Arguments = $"-c \"echo {code} | adb pair {ipAddress}:{port}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }


    /// <summary> Executes: adb shell getprop </summary>
    public static ProcessStartInfo DevicePropertiesPSI(bool isUSB) {
        return CreateADBShellCommandPSI(command: "getprop", isUSB);
    }
}