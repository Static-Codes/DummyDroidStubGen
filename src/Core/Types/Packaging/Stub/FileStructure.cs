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
namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

using Core.Extensions;
using DummyDroidStubGen.Core.Helpers;
using System.Diagnostics;
using System.Text;

using static Functions;
using static Global.Constants;
using static Global.Messaging;
using static Helpers.IO.FileHelper;
using static Helpers.IO.InputHelper;
using static FileStructure.NonNativeIconConversionStatus;

public class FileStructure 
{
    /// <summary> The project directory in which the stub's contents will be generated. </summary>
    public required ProjectDirectories Directories { get; set; }

    /// <summary> The path to the icon file that will be created and used for the compiled stub, either an XML or WEBP file. </summary>
    public required Icon Icon { get; set; }
    
    /// <summary> The path to the AndroidManifest.xml file, which will be used during compilation. </summary>
    public required string ManifestFilePath { get; set; }
    
    public record NonNativeIconConversion(bool Required, bool Occurred, string Path) {
        public string? GetPathFileExtension() => System.IO.Path.GetExtension(Path);
    };

    public enum NonNativeIconConversionStatus { UNUSED = -1, FAILURE = 0, SUCCESS = 1  }

    public record NonNativeIconResponse(NonNativeIconConversion Conversion, NonNativeIconConversionStatus Status) 
    {
        /// <summary> If the Conversion was successful (or didnt occur). </summary>
        /// <returns>A bool representing this status.</returns>
        public bool Success() => Status is not FAILURE;
        public bool Skipped() => Status is UNUSED;
    }

    /// <summary>
    /// Will exit if iconFileType is UNSET or the svg conversion fails <br/>
    /// 
    /// Returns: null
    /// A boolean detailing if the file contents was converted. <br/> 
    /// A nullable string that will either be null or contain the path to the converted file.
    /// </summary>
    private static async Task<NonNativeIconResponse> HandleSVGIcons(IconFileType iconFileType, string inputFilePath) 
    {   
        if (iconFileType == IconFileType.UNSET) {
            WriteWarningMessage("Unable to create the internal directory structure for the stub.");
            WriteErrorMessage(
                message: "Unsupported file format provided, supported formats include: '.svg', '.webp', and '.xml'", 
                exit: true, 
                exitCode: 1
            );
        }

        else if (iconFileType == IconFileType.SVG) 
        {
            var outputFilePath = await ProcessSVGConversion(inputFilePath);

            if (outputFilePath == null) {
                return new NonNativeIconResponse(
                    Conversion: new NonNativeIconConversion( Required: true, Occurred: true, Path: inputFilePath),
                    Status: FAILURE
                ); 
            }

            return new NonNativeIconResponse(
                Conversion: new NonNativeIconConversion( Required: true, Occurred: true, Path: outputFilePath),
                Status: SUCCESS
            );
        }

        return new NonNativeIconResponse(
            Conversion: new NonNativeIconConversion(Required: false, Occurred: false, Path: inputFilePath), 
            Status: UNUSED
        );
    }

    /// <summary> Creates a new instance of FileStructure for the current stub. </summary>
    public static async Task<FileStructure> New(string ProjectDirectory, string PackageName, string InputIconPath) 
    {

        if (!Directory.Exists(ProjectDirectory)) {
            WriteWarningMessage("Unable to create the internal directory structure for the stub.");
            WriteErrorMessage($"The specifed directory does not exist at: {ProjectDirectory}", exit: true, exitCode: 1);
        }

        var numberOfPeriods = MemoryExtensions.Count(PackageName, '.');

        if (numberOfPeriods != 2) {
            WriteWarningMessage("Unable to create the internal directory structure for the stub.");
            WriteErrorMessage($"Expected package name in the format: 'com.yourname.yourapp'", exit: true, exitCode: 1);
        }

        string? inputIconFileExt = null;

        var inputIconFileType = InputIconPath.ToIconFileType(ref inputIconFileExt);

        var iconResponse = await HandleSVGIcons(inputIconFileType, InputIconPath); 
        
        if (!iconResponse.Success()) {
            WriteWarningMessage("Icon file conversion failed.");
            WriteErrorMessage("The conversion either didnt occur, or failed.", exit: true, exitCode: 1);
        }
    
        // The output icon defaults to ".xml", this will be changed
        string? outputIconFileExt = Path.GetExtension(iconResponse.Conversion.Path);
        

        // At this point the file will either be .webp or .xml and can be natively embedded as the Stub's icon.
        var iconFileName = $"icon{outputIconFileExt}";

        // PackageNameParts' constructor ensures execution will not continue if the PackageName is invalid.
        PackageNameParts packageParts = new(PackageName.Split('.'));

        // Defining the main source dir for improved readability
        var mainSourceDir = Path.Combine(ProjectDirectory, "src", "main");


        var stubProjectDirectories = new ProjectDirectories
        (
            projectParentDir: ProjectDirectory,

            // Creating <ProjectDirectory>/src/main/
            mainSourceDir: mainSourceDir,
            
            // Creating <ProjectDirectory>/src/main/res/
            resourceDir: Path.Combine(mainSourceDir, "res"),

            drawableDir: Path.Combine(mainSourceDir, "res", "drawable"),

            // Creating: <ProjectDirectory>/src/main/<packageType>/<developerName>/<appName>
            javaCodeDir: Path.Combine(
                mainSourceDir,
                packageParts.PackageType,
                packageParts.DeveloperName,
                packageParts.AppName
            )
        );

        // Writing the required project directories.
        stubProjectDirectories.Write();
        
        // Creating: <ParentDirectory>/src/main/AndroidManifest.xml
        var manifestFilePath = Path.Combine(mainSourceDir, "AndroidManifest.xml");
    
        // Creating <ProjectDirectory>/src/main/res/<icon>.xml
        var iconFilePath = Path.Join(stubProjectDirectories.Drawables, iconFileName);          
        var iconInfo = new Icon(iconResponse.Conversion.Path, iconFilePath);

        return new() {
            Directories = stubProjectDirectories,
            Icon = iconInfo,
            ManifestFilePath = manifestFilePath,
        };
    }

    private static async Task<string?> ProcessSVGConversion(string inputSVGPath) 
    {
        // Checking if VDTool is already downloaded.
        var vdToolDownloaded = VDToolIsDownloaded();

        WriteInformation("You have selected an SVG file, this needs to be converted to Android's VectorDrawable format.");

        if (!vdToolDownloaded) 
        {
            WriteInformation("This process involves downloading a standalone application known as VDToolWrapper.\n");

            var learnMoreChoice = AskForSelection("Would you like to learn more about how this application work?",  ["Yes", "No"]);

            if (learnMoreChoice.IsExitOption()) { Environment.Exit(1); }

            else if (learnMoreChoice is "Yes") {
                WriteInformation("VDToolWrapper was compiled from VectorDrawableCliTool using a fork of Ryan Harter's VDTool.");
                WriteInformation(coloredText: $"VD Tool\n\t{VDToolForkLink}", tagName: "[[LINK]]: ", tagNameColor: "orange");
                WriteInformation(coloredText: $"VD Tool Builder\n\t{VDToolBuilderLink}\n", tagName: "[[LINK]]: ", tagNameColor: "orange");
            }

            WriteInformation("VDTool requires an additional 160MB of disk space for the download and extraction process.");

            var continueChoice = AskForSelection("Would you like to continue?", ["Yes", "No"]);
        
            if (continueChoice.IsExitOption() || continueChoice is "No") { Environment.Exit(1); }

            // If not already downloaded, an attempt is made to download the VDTool archive.
            if (await DownloadHelper.DownloadVDToolArchive()) { vdToolDownloaded = true; }
        }

        // If the archive is still missing, an error will displayed by DownloadVDToolArchive() and execution ends. 
        if (!vdToolDownloaded) { Environment.Exit(1); }
        
        var conversionDirectory = await RunVDToolWrapper(inputSVGPath);

        string? convertedFilePath = null;

        try {
            convertedFilePath = Directory.GetFiles(conversionDirectory).FirstOrDefault();
        }
        catch (Exception ex)
        {
            WriteWarningMessage("An exception has occured while reading the contents of the converted file.");
            #if DEBUG
                WriteErrorMessage(ex.StackTrace ?? ex.Message, exit: true, exitCode: 1);
            #else
                WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
            #endif
        }
        return convertedFilePath;
    }

    /// <summary> Executes the VD Tool Wrapper. </summary>
    /// <param name="inputSVGPath">The absolute path to the SVG file to be converted.</param>
    /// <returns>The absolute path to the temporary directory containing the conversion result. </returns>
    private static async Task<string> RunVDToolWrapper(string inputSVGPath)
    {
        // At this point, the archive will have been extracted and it's contents were verified to be on disk.
        DirectoryInfo? tmpDir = null; 
        
        try { tmpDir = Directory.CreateTempSubdirectory(); }
        catch (Exception ex)
        {
            WriteWarningMessage("An exception has occured while creating a temporary output directory");
            #if DEBUG
                WriteErrorMessage(ex.StackTrace ?? ex.Message, exit: true, exitCode: 1);
            #else
                WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
            #endif
        }

        var psi = new ProcessStartInfo()
        {
            FileName = VDToolBinaryPath,
            // Argument Breakdown
            // -c | Specifies that the wrapper should convert the contents of the input SVG.
            // -in | Specifies that the next argument will be the input SVG.
            // -out | Specifies that the next argument will be the output directory.
            Arguments = $"-c -in \"{inputSVGPath}\" -out \"{tmpDir.FullName}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,

            WorkingDirectory = VDToolSubDirectory,

            // This is likely unneccessary as glibc handles both UTF8 and UTF16 string marshalling.
            StandardOutputEncoding = Encoding.UTF8,
        };

        DataReceivedEventHandler? inputHandler = null;
        DataReceivedEventHandler? outputHandler = null;

        try {
            var process = await RunProcessAsync(psi, inputArg: null, inputHandler, outputHandler);
            
            // If the execution did not fail, no logging is required.
            if (process.ExitCode == 0) { return tmpDir.FullName; }

            // At this point it's confirmed the execution has failed, and the user will be notified why.
            WriteWarningMessage("An error has occured while trying to execute the VDTool-Wrapper.");
            foreach (var error in process.Error) { WriteErrorMessage(error); }
            
            throw process.Exception ?? new("No additional data was reported from the wrapper.");
        }

        catch (Exception ex)
        {
            var exc = ex;
            throw exc;
        }

    }

}


