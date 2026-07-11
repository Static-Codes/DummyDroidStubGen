namespace DummyDroidStubGen.Core.Helpers.IO;

using static Global.Messaging;
using static Global.Constants;
using System.IO.Compression;
using System.Threading.Tasks;
using static System.Environment;

public static class FileHelper 
{
    /// <summary> 
    ///     The path to the current user's application data directory.
    /// </summary>
    private static readonly string AppDataDirectory = GetSystemAppDataDirectory();

    /// <summary>
    ///     The path to the subdirectory (inside AppDataDirectory) where DDSG will write data to.
    /// </summary>
    private static readonly string AppDataSubDirectory = Path.Combine(AppDataDirectory, ApplicationName);
    
    /// <summary>
    ///     The path to the subdirectory (inside AppDataSubDirectory) where DDSG will cache builds. <br/>
    /// 
    ///     This is disabled by default and can be enabled by passing the "--backup" argument.
    /// </summary>
    public static readonly string BuildHistorySubDirectory = Path.Combine(AppDataSubDirectory, "BuildHistory");

    /// <summary>
    ///     The path to the subdirectory inside AppDataSubDirectory where DDSG will extract embedded resources.
    /// </summary>
    public static readonly string ResourcesSubDirectory = Path.Combine(AppDataSubDirectory, "Resources");
    
    /// <summary>    
    ///     The extraction path to the Zip Archive embedded within the DDSG binary, it contains the required Java Libraries for stub generation.
    /// </summary>
    private static readonly string LibrariesExtractionPath = Path.Combine(ResourcesSubDirectory, "libs.zip");
    
    /// <summary>
    ///     The extraction path for the Zip Archive embedded within the DDSG binary. DDSG will extract the optional OnDeviceLabelFetcher Zip archive.
    /// </summary>
    private static readonly string ODLFExtractionPath = Path.Combine(ResourcesSubDirectory, "odlf.zip");

    /// <summary>
    ///     The subdirectory where DDSG will extract the required Java Libraries.
    /// </summary>
    public static readonly string LibrariesSubDirectory = Path.Combine(ResourcesSubDirectory, "libs");

    /// <summary>
    ///     The subdirectory where DDSG will write the optional VDTool binary.
    /// </summary>
    public static readonly string VDToolSubDirectory = Path.Combine(ResourcesSubDirectory, "vdtool");

    /// <summary> The path to the zip file containing the VDTool binary. </summary>
    public static readonly string VDToolZipPath = Path.Combine(VDToolSubDirectory, "vdtool.zip");


    /// <summary> The absolute path to the VDTool binary. </summary>
    public static readonly string VDToolBinaryPath = Path.Combine(VDToolSubDirectory, "vdtool-wrapper");

    /// <summary>
    ///     The path to the Android SDK that will be written to disk during the compilation process. <br/>
    ///     This JAR will then be called by aapt2 link during the execution of build.sh
    /// </summary> 
    public static readonly string AndroidSDKJarPath = Path.Combine(LibrariesSubDirectory, "android-21.jar");


    /// <summary>
    ///     The path to the Android R8 Java runtime that will be written to disk during the compilation process. <br/>
    ///     This JAR will then be converted to Android Dex code and will be aligned within the APK in following commands.
    /// </summary> 
    public static readonly string AndroidR8JarPath = Path.Combine(LibrariesSubDirectory, "r8lib.jar");

    /// <summary>
    ///     The path to inside ResourcesSubDirectory where DDSG will extract the optional OnDeviceLabelFetcher.
    /// </summary>
    public static readonly string ODLFSubDirectory = Path.Combine(ResourcesSubDirectory, "OnDeviceLabelFetcher");


    public static async Task CreateRequiredDirectories() 
    {
        CreateAppDataSubDirectory();
        CreateBuildHistorySubDirectory();
        CreateEmbeddedResourcesSubDirectory();
        await ExtractEmbeddedArchives();
    }


    /// <summary> 
    ///     Creates a subdirectory "DummyDroidStubGen" inside the current user's app data directory. 
    /// </summary> 
    private static void CreateAppDataSubDirectory() 
    {
        try 
        {
            if (!Directory.Exists(AppDataSubDirectory)) {
                Directory.CreateDirectory(AppDataSubDirectory);
            }
        }

        catch (Exception ex) {
            WriteWarningMessage($"Failed to create required directory:\n\t\t{ApplicationName}\n");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }
    }


    /// <summary> 
    ///     Creates a subdirectory "BuildHistory" inside the "DummyDroidStubGen" subdirectory. 
    /// </summary> 
    private static void CreateBuildHistorySubDirectory() 
    {
        try 
        {
            if (!Directory.Exists(BuildHistorySubDirectory)) {
                Directory.CreateDirectory(BuildHistorySubDirectory);
            }
        }

        catch (Exception ex) {
            WriteWarningMessage($"Failed to create required directory:\n\t\t{BuildHistorySubDirectory}\n");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }

    }


    /// <summary>
    ///     Creates a subdirectory "Resources" inside the "DummyDroidStubGen" subdirectory.
    /// </summary>
    private static void CreateEmbeddedResourcesSubDirectory() 
    {
        try 
        {
            if (!Directory.Exists(ResourcesSubDirectory)) {
                Directory.CreateDirectory(ResourcesSubDirectory);
            }
        }

        catch (Exception ex) {
            WriteWarningMessage($"Failed to create required directory:\n\t\t{ResourcesSubDirectory}\n");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }
    }


    /// <summary>
    ///     Creates a subdirectory with the packageName's value inside the "DummyDroidStubGen" subdirectory.
    /// </summary>
    public static bool CreateProjectBuildSubDirectory(string packageName, out string directoryPath) 
    {
        var projectBuildDir = GetPackageBuildDirectory(packageName);

        try 
        {
            if (!Directory.Exists(projectBuildDir)) {
                Directory.CreateDirectory(projectBuildDir);
            }
        }
        catch (Exception ex) {
            WriteWarningMessage("Failed to create required directory:");
            WriteInformation($"\n\t{projectBuildDir}\n");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }

        directoryPath = projectBuildDir;

        return true;
    }


    /// <summary> An InputArchive object is used in ExtractEmbeddedArchiveAsync() </summary>
    /// <param name="EmbeddedResourcePath">The path to the Zip Archive embedded in the DDSG binary. </param>
    /// <param name="ExtractionPath">The path the Zip Archive will be extracted to prior to decompression. </param>
    /// <param name="OutputSubDirectory">The path the Zip Archive will be decompressed to. </param>
    private record InputArchive(string EmbeddedResourcePath, string ExtractionPath, string OutputSubDirectory);

    /// <summary> Extracts the embedded zip archives that are required for DDSG to generate stubs. </summary>
    private static async Task ExtractEmbeddedArchives() 
    {
        // The embedded resource path is the key, and its extraction path is the value.
        var inputArchives = new List<InputArchive>() {
            new( JavaLibsResourcePath, LibrariesExtractionPath, LibrariesSubDirectory ),
            new( ODLFResourcePath, ODLFExtractionPath, ODLFSubDirectory )
        };

        foreach (var inputArchive in inputArchives) 
        {
            if (Directory.Exists(inputArchive.OutputSubDirectory)) {
                WriteInformation($"{inputArchive.OutputSubDirectory} already exists, skipping.");
                continue;
            }

            if (!await ExtractEmbeddedArchiveAsync(inputArchive)) 
            {
                WriteInformation(
                    whiteText: $"If this persists, please delete the Resources directory at:",
                    coloredText: ResourcesSubDirectory,
                    textColor: "orange",
                    reverse: true
                );

                Exit(1);
            }

            WriteSuccessMessage($"Extracted archive to: {inputArchive.OutputSubDirectory}");
        }
    }


    /// <summary>
    ///     Attempts to retrieve a stream object by calling GetManifestResourceStream() using the specified resourcePath. <br/>
    ///     Checks if the stream object is a valid, non-corrupt ZipArchive. <br/>
    ///     If so, it is extracted to absoluteOutputDir. <br/>
    ///     Returns true if the operation succeeded, otherwise false.
    /// </summary>   
    private static async Task<bool> ExtractEmbeddedArchiveAsync(InputArchive archive) 
    {
        Stream? stream;
        try 
        {
            stream = _assembly.GetManifestResourceStream(archive.EmbeddedResourcePath);
            if (stream == null) 
            {
                WriteWarningMessage($"Unable to parse the embedded resource at the path: '{archive.EmbeddedResourcePath}'");
                WriteErrorMessage("The stream object used to parse the resourcePath returned null.");
                WriteInformation("This indicates the resource path provided was incorrect.");
                return false;
            }
            
        }

        catch (Exception ex) 
        {
            WriteWarningMessage($"Unable to parse the embedded resource at the path: '{archive.EmbeddedResourcePath}'");
            WriteErrorMessage(ex.Message);
            return false;
        }


        try 
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
            await ZipFile.ExtractToDirectoryAsync(
                source: stream, 
                destinationDirectoryName: archive.OutputSubDirectory, 
                overwriteFiles: true, 
                cancellationToken: cts.Token
            );
        }

        catch (Exception ex) 
        {
            WriteWarningMessage($"Unable to decompress Zip Archive to: '{archive.OutputSubDirectory}'");
            WriteErrorMessage(ex.Message);
            return false;
        }

        return true; 
    }



    /// <summary> Returns BuildHistorySubDirectory/packageName </summary>
    private static string GetPackageBuildDirectory(string packageName) {
        return Path.Combine(BuildHistorySubDirectory, packageName);
    }


    /// <summary> 
    ///     Returns the path to the user appdata directory on the current system. 
    /// </summary> 
    private static string GetSystemAppDataDirectory() 
    {
        string? appDataPath;
        try {
            appDataPath = GetFolderPath(SpecialFolder.ApplicationData);
        }
        catch (Exception ex) {
            WriteWarningMessage("An exception has occured while attempting to retrieve the AppData directory on the current system.");
            var exc = ex;
            throw exc;
        }
        return appDataPath;
    }

    public static string GetUserProfileDirectory() => GetFolderPath(SpecialFolder.UserProfile);

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


    /// <summary> 
    ///     Attempts to serialize the provided stream object into the output variable bytes. </br>
    ///     Returns true if this operation succeeds, otherwise false.
    /// </summary>
    public static bool TrySerializePathToByteArray(string? path, out byte[] bytes) 
    {

        try {
            ArgumentException.ThrowIfNullOrEmpty(path);
            bytes = File.ReadAllBytes(path);
        }

        catch (Exception ex) 
        {
            WriteWarningMessage("Unable to serialize the provided icon content stream.");
            WriteErrorMessage(ex.Message);
            bytes = [];
            return false;
        }
        
        return true;

    }


    /// <summary> 
    ///     Attempts to serialize the provided stream object into the output variable bytes. </br>
    ///     Returns true if this operation succeeds, otherwise false.
    /// </summary>
    public static bool TrySerializeStreamToByteArray(Stream stream, out byte[] bytes) 
    {
        try 
        {
            using MemoryStream memoryStream = new();
            stream.CopyTo(memoryStream);
            bytes = memoryStream.ToArray();
            return true;
        }
        
        catch (Exception ex) 
        {
            WriteWarningMessage("Unable to serialize the provided icon content stream.");
            WriteErrorMessage(ex.Message);
            bytes = [];
            return false;
        }
    }

    public static bool VDToolIsDownloaded()
    {
        string[] files = [ 
            "libawt.so", "libawt_headless.so", "libawt_xawt.so", "libfontmanager.so", "libjava.so",
            "libjavajpeg.so", "libjvm.so", "liblcms.so", "libmlib_image.so", "vdtool-wrapper" 
        ];

        static string GetAbsolutePath(string file) => Path.Combine(VDToolSubDirectory, file); 

        foreach (var file in files) 
        {
            if (!Path.Exists(GetAbsolutePath(file))){
                return false;
            }
        }

        return true;
    }
}