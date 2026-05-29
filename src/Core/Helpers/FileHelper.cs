namespace DummyDroidStubGen.Core.Helpers;

using static Global.Messaging;
using static Global.Constants;

public static class FileHelper 
{

    public static readonly string AppDataDirectory = GetSystemAppDataDirectory();
    public static readonly string AppDataSubDirectory = Path.Combine(AppDataDirectory, ApplicationName);
    
    public static void CreateAppDataSubDirectory() 
    {
        if (!Directory.Exists(AppDataSubDirectory)) {
            Directory.CreateDirectory(AppDataSubDirectory);
        }
    }


    /// <summary> Returns the path to the user appdata directory on the current system. </summary> 
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
}