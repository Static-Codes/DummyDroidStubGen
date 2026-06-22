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
namespace DummyDroidStubGen.Core.Types.Packaging.Stub.Contents;

using Extensions;
using static AndroidManifest;
using static Extensions.ShellTypeExtension;
using static Global.Messaging;
using static Helpers.IO.FileHelper;
using static Helpers.ShellTypeHelper;


/// <summary> Contains the functions and members used to generate the stub's build and installation scripts. </summary>
public class ShellCode 
{
    /// <summary> A semantic representation of an empty JavaFileContent list. </summary>
    public static readonly ShellFileContent Empty = new([]);    
    public const string BuildFileName = "build.sh";
    public const string InstallFileName = "run.sh";

    /// <summary> The shell files to be used for the compilation and installation of the generated stub. </summary>
    private static readonly List<ShellFile> ShellFiles =
    [
        new ShellFile(BuildFileName, Empty),
        new ShellFile(InstallFileName, Empty),
    ];

    /// <summary> The current linux shell in use. </summary>
    public static readonly ShellType CurrentShellType = FindCurrent();

    /// <summary> The current linux shell in use. </summary>
    public static readonly string ShebangOperator = CurrentShellType.ToShebangOperator();



    /// <summary> 
    ///     Uses the total number of lines in a script to locate the highest line number. <br/>
    ///     The length of the located line number is then compared to length of the current line number. <br/>
    ///     This function returns a string containing X whitespace chars, where X is the difference in these lengths.
    /// </summary> 
    private static string GetDebugLinePadding(int currentLineNumber, int totalLines) 
    {
        int lineLength = currentLineNumber.ToString().Length;

        int highestDigitCount = totalLines.ToString().Length;
        
        int spacesNeeded = highestDigitCount - lineLength;
        
        return new(' ', Math.Max(0, spacesNeeded));
    }


    /// <summary> Populates the contents of BuildFileName </summary>
    public static void PopulateBuildScript()
    {
        if (!TryGetShellFile(BuildFileName, out var shellFile) || shellFile == null) {
            Environment.Exit(1);
        }

        // Adding the shebang operator.
        // shellFile.AddLine("#!/usr/bin/env bash");
        shellFile.AddLine(ShebangOperator);

        // Writing the GNUv3 license notice to the current shell file.
        shellFile.WriteLicenseNotice();

        int tabs = 0;
        
        // Error handling and parameter validation
        shellFile.AddLine("set -e", tabs);
        shellFile.AddEmptyLine();

        // Inserting the invalid usage block.
        shellFile.AddInvalidUsageBlock([
            new("AppName", "OnDeviceLabelFetcher"),
            new("Package/Directory/Structure", "com/staticcodes/odlf"),
            new("packageName.apk", "odlf.apk")
        ], ref tabs);

        // Declaring the build script's variables.
        shellFile.AddLine("APP_NAME=\"$1\" # The name of the class that contains the an extension of Activity.");
        shellFile.AddLine("PKG_DIR_STRUCTURE=\"$2\" # The project's source directory structure");
        shellFile.AddLine("APK_NAME=\"$3\" # The filename of the compiled APK");
        shellFile.AddLine("CLASS_DIR=\"$PKG_DIR_STRUCTURE/$APP_NAME\"");
        shellFile.AddLine("PROJECT_ROOT=\"$(pwd)\"");
        shellFile.AddEmptyLine();
        shellFile.AddEmptyLine();
        shellFile.AddLine("mkdir -p obj dex_out");
        shellFile.AddEmptyLine();

        // Compiling the stub's resources to compiled_resources.zip
        shellFile.AddLine("echo -e \"\\n[3/16] -> Compiling Resources for $APP_NAME...\\n\"");
        shellFile.AddLine("aapt2 compile --dir \"$PROJECT_ROOT/src/main/res\" -o \"$PROJECT_ROOT/compiled_resources.zip\"");
        shellFile.AddEmptyLine();

        // Linking the project's resources 
        shellFile.AddLine("echo -e \"\\n[4/16] -> Linking XML Manifest using aapt2...\\n\"");
        shellFile.AddLine("aapt2 link --auto-add-overlay \\");
        shellFile.AddLine($"--manifest {RelativeManifestPath} \\", 1);
        shellFile.AddLine($"-I {AndroidSDKJarPath} \\", 1);
        shellFile.AddLine("-R compiled_resources.zip \\", 1);
        shellFile.AddLine("-o unaligned.apk", 1);
        shellFile.AddEmptyLine();

        // Compiling the stub's Java source files. 
        shellFile.AddLine("echo -e \"\\n[5/16] -> Compiling Java to ByteCode using javac...\\n\"");
        shellFile.AddLine($"javac -d obj --release 8 -classpath {AndroidSDKJarPath} src/main/\"$PKG_DIR_STRUCTURE\"/*.java");
        shellFile.AddEmptyLine();

        // Converting the Java ByteCode to Android Dex.
        shellFile.AddLine("echo -e \"\\n[6/16] -> Converting Java ByteCode to Android Dex...\\n\"");

        // TODO: Make an alternative function called AddJavaCopyLine
        shellFile.AddLine($"java -cp {AndroidR8JarPath} com.android.tools.r8.D8 --lib {AndroidSDKJarPath} --release --output dex_out/ obj/\"$CLASS_DIR\"*.class");
        shellFile.AddEmptyLine();

        // Packaging the dex output class
        shellFile.AddLine("echo -e \"\\n[7/16] -> Packaging Output Dex Classes...\\n\"");
        shellFile.AddLine("zip -uj unaligned.apk dex_out/classes.dex");
        shellFile.AddEmptyLine();


        // Notifying the user of the alignment process that is about to occur and documenting the different flags used.
        shellFile.AddLine("echo -e \"\\n[8/16] -> Aligning APK...\\n\"");
        shellFile.AddComment("-P | Aligns uncompressed .so libraries page size to X KiB chunks.");
        shellFile.AddComment("16 | Specifies the \"X\" in the \"X KiB\" above.");
        shellFile.AddComment("-f | Forces an overwright of aligned.apk (if a previous build failed)");
        shellFile.AddComment("-v | Verbose output of the aligning process.");
        shellFile.AddComment("4  | Aligning optional APK resources into 4 byte chunks");
        shellFile.AddComment("Docs: https://developer.android.com/tools/zipalign");
        shellFile.AddEmptyLine();

        // Adding a maintainers note for the apk aligning functionality.
        shellFile.AddComment("Maintainers Note:");
        shellFile.AddComment("If this project ever requires .so libraries:");
        shellFile.AddComment("The next line should be uncommented, and the line below that should commented out.");
        shellFile.AddComment("if ! zipalign -P 16 -v 4 unaligned.apk aligned.apk");

        // Aligning the APK at aligned.apk
        shellFile.AddLine("if ! zipalign -f -v 4 unaligned.apk aligned.apk; then");
        shellFile.AddLine("echo -e \"\\nUnable to align compiled APK, please try again.\\n\"", 1);
        shellFile.AddLine("exit 1", 1);
        shellFile.AddLine("fi");
        shellFile.AddEmptyLine();

        // Notifying the user of the confirmation process that is about to occur and documenting the different flags used.
        shellFile.AddLine("echo -e \"\\n[9/16] -> Confirming Alignment...\\n\"");
        shellFile.AddComment(" -c | Instructs zipalign to perform a confirmation instead of an overwrite");
        shellFile.AddComment(" -P | Aligns uncompressed .so libraries page size to X KiB chunks.");
        shellFile.AddComment(" 16 | Specifies the \"X\" in the \"X KiB\" above.");
        shellFile.AddComment(" -v | Verbose output of the aligning process.");
        shellFile.AddComment(" 4  | Aligning optional APK resources into 4 byte chunks (extra headers)");
        shellFile.AddComment(" Docs: https://developer.android.com/tools/zipalign");
        
        // Adding a maintainers note
        shellFile.AddEmptyLine();
        shellFile.AddComment(" Maintainers Note:");
        shellFile.AddComment(" If this project ever requires .so libraries:");
        shellFile.AddComment(" The next line should be uncommented, and the line below that should commented out.");
        shellFile.AddComment(" if zipalign -c -P 16 -v 4 aligned.apk; then");
        
        
        // Confirming the alignment of aligned.apk
        shellFile.AddLine("if zipalign -c -v 4 aligned.apk; then");
        shellFile.AddLine("echo -e \"\\nAlignment Confirmed!\"", 1);
        shellFile.AddLine("else", 0);
        shellFile.AddLine("echo -e \"\\nIncorrect Alignment Detected!\"", 1);
        shellFile.AddLine("exit 1", 1);
        shellFile.AddLine("fi");
        shellFile.AddEmptyLine();

        // Notifying the user that the unaligned APK is being removed
        shellFile.AddLine("echo -e \"\\n[10/16] -> Removing Unaligned APK...\"");
        shellFile.AddLine("rm -f unaligned.apk");
        shellFile.AddEmptyLine();

        // Notifying the user that the aligned APK is going to be signed.
        shellFile.AddLine("echo -e \"\\n[11/16] -> Signing APK...\\n\"");

        // Checking if the debug.keystore file exists, if not it is generated.
        shellFile.AddLine("if [ ! -f \"debug.keystore\" ]; then");
        shellFile.AddLine("echo \"File debug.keystore not found, generating now.\"", 1);
        shellFile.AddLine("keytool -genkey -v -keystore debug.keystore -storepass android -alias androiddebugkey \\", 1);
        shellFile.AddLine("-keypass android -keyalg RSA -keysize 2048 -validity 10000 \\", 2);
        shellFile.AddLine("-dname \"CN=Android Debug,O=Android,C=US\"", 2);
        shellFile.AddLine("fi");
        shellFile.AddEmptyLine();
        shellFile.AddEmptyLine();

        // Performing a self signature on the Aligned APK using the debug.keystore file from above.
        shellFile.AddLine("if ! apksigner sign --ks debug.keystore --ks-pass pass:android --out \"$APK_NAME\" aligned.apk; then");
        shellFile.AddLine("echo \"Unable to sign the APK..\"", 1);
        shellFile.AddLine("exit 1", 1);
        shellFile.AddLine("fi");
        shellFile.AddEmptyLine();

        // Notifying the user that leftover build artifacts will be removed.
        shellFile.AddLine("echo -e \"\\n[12/16] -> Removing leftover build artifacts...\\n\"");
        
        // Removes the leftover build artifacts, exits if this fails.
        shellFile.AddLine("if ! rm -rf \"$APK_NAME.idsig\" aligned.apk unaligned.apk compiled_resources.zip dex_out obj; then");
        shellFile.AddLine("echo \"Unable to clean all leftover build artifacts..\"", 1);
        shellFile.AddLine("exit 1", 1);
        shellFile.AddLine("fi");
        shellFile.AddEmptyLine();

        // Notifies the user that the build process has been completed.
        shellFile.AddLine("echo -e \"Build finalized for $APP_NAME!\\n\"");
    }


    /// <summary> If the application is being run via "dotnet run", the contents of the JavaFile is displayed. </summary>
    private static void RunDebugIfActive(ShellFile shellFile) 
    {
        #if DEBUG    
            foreach (var line in shellFile.Content.Get()) 
            {
                string indent = GetDebugLinePadding(
                    currentLineNumber: line.Number, 
                    totalLines: shellFile.Content.Length
                );

                WriteDebugMessage($"Line {indent}{line.Number}: {line.Content}");
            }
        #endif
    }
    

    /// <summary> 
    ///     Attempts to return a JavaFile object from JavaFiles using the provided fileName. <br/>
    ///     If successful, file will be assigned this resolved JavaFile object, and returns true. <br/>
    ///     Otherwise, file will be assigned a null value, and false will be returned.
    /// </summary>
    public static bool TryGetShellFile(string fileName, out ShellFile? file) 
    {
        file = null;
        try {
            file = ShellFiles.Where(f => f.FileName == fileName).FirstOrDefault();
        }
        catch (Exception ex) {
            WriteWarningMessage($"Unable to lookup local stub source file: {fileName}");
            WriteErrorMessage(ex.Message);
            return false;
        }
        return file != null;
    }
}