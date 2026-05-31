namespace DummyDroidStubGen.Core.Helpers;

using System.Diagnostics;
using static Core.Helpers.FileHelper;
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