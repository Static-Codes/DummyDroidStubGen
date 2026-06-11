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
using static Core.Helpers.IO.FileHelper;
using static Global.Constants;
using static Global.Messaging;

public class PSIHelper 
{
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
        return new() {
            FileName = ADBPath,
            Arguments = $"connect {ipAddress}:{port}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
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
    public static ProcessStartInfo DevicePropertiesPSI(bool isUSB) 
    {
        return new() {
            FileName = ADBPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = isUSB switch
            {
                true => "-d shell getprop",
                false => "shell getprop"
            }
        };
    }

    /// <summary> 
    ///     Executes 3 commands in batch with the same adb shell command.over adb shell. <br/>
    ///     
    ///     Checking for the superuser binary -> [ -f /system/bin/su ] <br/>
    ///     Checking for read-write permissions on the read-only partition -> mount | grep ' / ' | grep 'rw' <br/>
    ///     Checking for the most common rooting tool magisk -> pm list packages | grep -q 'magisk'
    ///     
    /// </summary>
    [Obsolete("No longer required, but left for reference.")]
    public static ProcessStartInfo DeviceRootCheckPSI() 
    {   
        var combinedCommands = @"
            [ -f /system/bin/su ] && echo 'SU_INSTALLED:TRUE' || echo 'SU_INSTALLED:FALSE';
            mount | grep -q ' / ' | grep -q 'rw' && echo 'SYS_WRITE_ACCESS:TRUE' || echo 'SYS_WRITE_ACCESS:FALSE';
            pm list packages | grep -q 'com.topjohnwu.magisk' && echo 'MAGISK_INSTALLED:TRUE' || echo 'MAGISK_INSTALLED:FALSE';"
            // Wrapping the commands so only one call to adb shell is required for all 3 commands.
            .Replace("\"", "\\\"")
            .Replace("\r\n", " ");

        return new() {
            FileName = ADBPath,
            Arguments = combinedCommands,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
    }


    /// <summary> Executes: adb pull "/data/app/{codePath}/base.apk" "{tempDir}/{appName}.apk" </summary>
    public static ProcessStartInfo ExtractBaseBinaryPSI(string packageName, string appName, string codePath, bool isUSB) 
    {
        // Retrieving a temporary sub-directory within the current users temporary directory.
        var tempDir = GetTemporaryBinaryDirectory(appName);

        try 
        {
            // Deleting the directory if it already exists.
            if (Directory.Exists(tempDir)) {
                Directory.Delete(tempDir, recursive: true);
            }

            Directory.CreateDirectory(tempDir);
        }

        catch (Exception ex) {
            WriteWarningMessage($"Unable to extract the base.apk from '{packageName}'");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }
        
        return new() {
            FileName = ADBPath,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = isUSB switch {
                true => $"-d pull {codePath}/base.apk \"{tempDir}/{appName}.apk\"",
                false => $"pull {codePath}/base.apk \"{tempDir}/{appName}.apk\"",
            }
        };
    }


    /// <summary> Executes: adb shell dumpsys package {packageName} | grep codePath= </summary>
    public static ProcessStartInfo GetCodePathOfPackagePSI(string packageName, bool isUSB) 
    {
        return new() {
            FileName = "/bin/bash",
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = isUSB switch {
                true => $"-c \"adb -d shell dumpsys package {packageName} | grep codePath=\"",
                false => $"-c \"adb shell dumpsys package {packageName} | grep codePath=\""
            }
        };
    }
    
    /// <summary> Executes: adb shell pm list packages </summary>
    public static ProcessStartInfo PackageListPSI(bool isUSB = true) 
    {
        return new()
        {
            FileName = ADBPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = isUSB switch
            {
                true => "-d shell pm list packages",
                false => "shell pm list packages"
            }
        };
    }
}