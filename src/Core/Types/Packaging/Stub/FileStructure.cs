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
using static Global.Messaging;
using static FileStructure.NonNativeIconConversionStatus;

public class FileStructure 
{
    /// <summary> The project directory in which the stub's contents will be generated. </summary>
    public required ProjectDirectories Directories { get; set; }

    /// <summary> The path to the icon file that will be created and used for the compiled stub, either an XML or WEBP file. </summary>
    public required Icon Icon { get; set; }
    
    /// <summary> The path to the AndroidManifest.xml file, which will be used during compilation. </summary>
    public required string ManifestFilePath { get; set; }

    
    private static bool BackupPreviousBuild() { // TODO: IMPLEMENT ME
        throw new NotImplementedException("Please implement me before committing.");
    }
    
    public record NonNativeIconConversion(bool Required, bool Occurred, string? Path = null);

    public enum NonNativeIconConversionStatus { UNUSED = -1, FAILURE = 0, SUCCESS = 1  }

    public record NonNativeIconResponse(NonNativeIconConversion Conversion, NonNativeIconConversionStatus Status) 
    {
        public bool CheckForSuccess() => Status switch
        {
            FAILURE => false,
            _ => true, // Handles both SUCCESS and UNUSED.
        };
    }

    /// <summary>
    /// Will exit if iconFileType is UNSET or the svg conversion fails <br/>
    /// 
    /// Returns: 
    /// A boolean detailing if the file contents was converted. <br/> 
    /// A nullable string that will either be null or contain the path to the converted file.
    /// </summary>
    public static NonNativeIconResponse HandleNonNativeIconFormats(IconFileType iconFileType) 
    {
        WriteWarningMessage("Please implement SVG to Android Drawable Conversion in FileStructure.cs");
        
        if (iconFileType == IconFileType.UNSET) {
            WriteWarningMessage("Unable to create the internal directory structure for the stub.");
            WriteErrorMessage(
                message: "Unsupported file format provided, supported formats include: '.svg', '.webp', and '.xml'", 
                exit: true, 
                exitCode: 1
            );
        }

        else if (iconFileType == IconFileType.SVG) {
            throw new NotImplementedException("Please implement me before committing");
        }

        return new NonNativeIconResponse
        (
            Conversion: new NonNativeIconConversion(
                Required: false,
                Occurred: false
            ), 
            Status: UNUSED
        );
    }


    /// <summary> Creates a new instance of FileStructure for the current stub. </summary>
    public static FileStructure New(string ProjectDirectory, string PackageName, string InputIconPath) 
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

        string? iconFileExt = null;
        var iconFileType = InputIconPath.ToIconFileType(ref iconFileExt);

        (var Conversion, var Status) = HandleNonNativeIconFormats(iconFileType); 
        
        if (Conversion.Required && !Conversion.Occurred || Status == FAILURE) {
            WriteWarningMessage("Icon file conversion failed.");
            WriteErrorMessage("The conversion either didnt occur, or failed.", exit: true, exitCode: 1);
        }

        // At this point the file will either be .webp or .xml and can be natively embedded as the Stub's icon.
        var iconFileName = $"icon{iconFileExt}";

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
        var iconFilePath = Path.Combine(stubProjectDirectories.Drawables, iconFileName);
        var iconInfo = new Icon(InputIconPath, iconFilePath);

        return new() {
            Directories = stubProjectDirectories,
            Icon = iconInfo,
            ManifestFilePath = manifestFilePath,
        };
    }

}


