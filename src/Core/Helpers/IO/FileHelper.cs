namespace DummyDroidStubGen.Core.Helpers.IO;

using static Global.Messaging;
using static Global.Constants;

public static class FileHelper 
{
    

    /// <summary> 
    ///     The path to the current user's application data directory.
    /// </summary>
    public static readonly string AppDataDirectory = GetSystemAppDataDirectory();

    /// <summary>
    ///     The path to the subdirectory (inside AppDataDirectory) where DDSG will write data to.
    /// </summary>
    public static readonly string AppDataSubDirectory = Path.Combine(AppDataDirectory, ApplicationName);

    public static readonly string AppConfigFilePath = Path.Combine(AppDataSubDirectory, "DDSG.config.yaml");
    
    /// <summary>
    ///     The path to the subdirectory (inside AppDataSubDirectory) where DDSG will cache builds. <br/>
    /// 
    ///     This is disabled by default and can be enabled by editing the config at 
    /// </summary>
    public static readonly string BuildHistorySubDirectory = Path.Combine(AppDataSubDirectory, "BuildHistory");
    public static readonly string UserTmpDir = Path.GetTempPath();

    public static void CreateRequiredDirectories() 
    {
        CreateAppDataSubDirectory();
        CreateBuildHistorySubDirectory();
    }

    /// <summary> Creates a 
    private static void CreateAppDataSubDirectory() 
    {
        if (!Directory.Exists(AppDataSubDirectory)) {
            Directory.CreateDirectory(AppDataSubDirectory);
        }
    }

    private static void CreateBuildHistorySubDirectory() 
    {
        if (!Directory.Exists(BuildHistorySubDirectory)) {
            Directory.CreateDirectory(BuildHistorySubDirectory);
        }
    }

    /// <summary> 
    ///     Returns a temporary directory that will be used to hold the decompiled apk contents. 
    /// </summary>
    public static string GetTemporaryBinaryDirectory(string appName) {
        return Path.Combine(UserTmpDir, $"{appName}_files");
    }

    /// <summary> 
    ///     Returns the path to the user appdata directory on the current system. 
    /// </summary> 
    private static string GetSystemAppDataDirectory() 
    {
        string? appDataPath;
        try {
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        catch (Exception ex) {
            WriteWarningMessage("An exception has occured while attempting to retrieve the AppData directory on the current system.");
            var exc = ex;
            throw exc;
        }
        return appDataPath;
    }


    /// <summary>
    ///     Attempts to output a stream object by calling GetManifestResourceStream() using the specified resourcePath.
    /// </summary>   
    public static bool TryGetManifestResourceStream(string resourcePath, out Stream? stream) 
    {
        stream = null;
        try 
        {
            stream = _assembly.GetManifestResourceStream(resourcePath);
            if (stream == null) 
            {
                WriteWarningMessage($"Unable to parse the embedded resource at the path: '{resourcePath}'");
                WriteErrorMessage("The stream object used to parse the resourcePath returned null.");
                WriteInformation("This indicates the resource path provided was incorrect.");
                return false;
            }
        }

        catch (Exception ex) 
        {
            WriteWarningMessage($"Unable to parse the embedded resource at the path: '{resourcePath}'");
            WriteErrorMessage(ex.Message);
            return false;
        }
        return true; 
    }
}