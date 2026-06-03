namespace DummyDroidStubGen.Core.Common;

using System.Reflection;
using DummyDroidStubGen.Core.Types;
using static Global.Constants;
using static Global.Messaging;


public class InstallationChecks 
{
    private readonly static MethodInfo[] ResolvedMethods = typeof(InstallationChecks).GetMethods(_privateStaticFlag);
    public static InstallationCheck[] CheckForRequiredBinaries() 
    {
        // Invoking all resolved methods
        var checkFunctions = ResolvedMethods.Select(a => {
            object? checkResult = null;
            
            try {
                checkResult = a.Invoke(a, null);
                ArgumentNullException.ThrowIfNull(checkResult);
            }
            
            catch (Exception ex) {
                WriteWarningMessage($"Unable to execute: {a.Name}() in InstallationChecks.GetChecks()");
                WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
            }

            return (InstallationCheck)checkResult;
        });

        // Hardcoded fallbacks if dynamic resolution fails.
        if (checkFunctions.Any()) {
            return [
                new(BinaryName: "aapt2", BinaryPath: AAPT2Path, Check: AAPT2Installed ),
                new(BinaryName: "adb",   BinaryPath: ADBPath, Check: ADBInstalled ),
                new(BinaryName: "apksigner",   BinaryPath: APKSignerPath, APKSignerInstalled ), 
                new(BinaryName: "java",  BinaryPath: JavaPath, JavaInstalled ),
                new(BinaryName: "javac", BinaryPath: JavaCompilerPath, Check: JavaCompilerInstalled ),
                new(BinaryName: "unzip", BinaryPath: UnzipPath, Check: UnzipInstalled ),
                new(BinaryName: "zip",   BinaryPath: ZipPath, Check: ZipInstalled ),
            ];
        }

        // Returning the
        return [..checkFunctions];
    }

    /// <summary> Checks for the existence of the AAPT2 binary at /usr/bin/aapt2 </summary>
    private static bool AAPT2Installed() { 
        return File.Exists(AAPT2Path);
    }

    /// <summary> Checks for the existence of the ADB binary at /usr/bin/adb </summary>
    private static bool ADBInstalled() {
        return File.Exists(ADBPath);
    }

    /// <summary> Checks for the existence of the ADB binary at /usr/bin/apksigner </summary>
    private static bool APKSignerInstalled() {
        return File.Exists(APKSignerPath);
    }

    /// <summary> Checks for the existence of the Java binary at /usr/bin/java </summary>
    private static bool JavaInstalled()  {
        return File.Exists(JavaPath);
    }

    /// <summary> Checks for the existence of the Java Compiler binary at /usr/bin/javac </summary>
    private static bool JavaCompilerInstalled() {
        return File.Exists(JavaCompilerPath);
    }

    /// <summary> Checks for the existence of the zip binary at /usr/bin/unzip </summary>
    private static bool UnzipInstalled() {
        return File.Exists(UnzipPath);
    }
    
    /// <summary> Checks for the existence of the unzip binary at /usr/bin/zip </summary>
    private static bool ZipInstalled() {
        return File.Exists(ZipPath);
    }

}