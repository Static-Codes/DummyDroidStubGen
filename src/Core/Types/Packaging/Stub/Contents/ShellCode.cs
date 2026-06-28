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
using static Global.Constants;
using static Global.Messaging;
using static Helpers.IO.FileHelper;
using static Helpers.ShellTypeHelper;


/// <summary> Contains the functions and members used to generate the stub's build and installation scripts. </summary>
public class ShellCode 
{
    /// <summary> A semantic representation of an empty JavaFileContent list. </summary>
    public static readonly ShellFileContent Empty = new([]);    
    public const string BuildFileName = "build.sh";
    public const string RunFileName = "run.sh";

    /// <summary> The shell files to be used for the compilation and installation of the generated stub. </summary>
    private static readonly List<ShellFile> ShellFiles =
    [
        new ShellFile(BuildFileName, Empty),
        new ShellFile(RunFileName, Empty),
    ];

    /// <summary> The current linux shell in use. </summary>
    public static readonly ShellType CurrentShellType = FindCurrent();

    /// <summary> The path to the file being used as the current linux shell. </summary>
    public static readonly string CurrentShellPath = CurrentShellType.ToPath();

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
    private static void PopulateBuildScript()
    {
        if (!TryGetShellFile(BuildFileName, out var shellFile) || shellFile == null) {
            Environment.Exit(1);
        }

        int tabs = 0;

        WriteBuildScriptHeaderBlock(shellFile, tabs);
        
        WriteBuildScriptResourcesBlock(shellFile, tabs);

        WriteBuildScriptManifestLinkBlock(shellFile, tabs);

        WriteBuildScriptCompilationBlock(shellFile, tabs);

        WriteBuildScriptConversionBlock(shellFile, tabs);

        WriteBuildScriptPackagingBlock(shellFile, tabs);

        WriteBuildScriptAlignmentBlock(shellFile, ref tabs);

        WriteBuildScriptConfirmationBlock(shellFile, ref tabs);

        WriteBuildScriptSigningBlock(shellFile, ref tabs);

        WriteBuildScriptCleanupBlock(shellFile, ref tabs);

        RunDebugIfActive(shellFile);
        Environment.Exit(1);
    }


    /// <summary> Populates the contents of BuildFileName </summary>
    private static void PopulateRunScript(string profileID = "0")
    {
        if (!TryGetShellFile(BuildFileName, out var shellFile) || shellFile == null) {
            Environment.Exit(1);
        }

        int tabs = 0;


        // Adding the shebang operator, license source notice, and usage check block.
        WriteRunScriptHeaderBlock(shellFile, ref tabs);
        shellFile.AddEmptyLine();


        // Declaring the $PROFILE_ID
        shellFile.AddVariableDeclaration(
            variableName: "PROFILE_ID", 
            value: profileID, 
            comment: "Change this to \"10\" if you want to install the stub to a sandboxed or work profile."
        );


        // Removing leftover build artifacts
        shellFile.AddCommand(command: "rm", [
            new(Flag: "-rf", Value: "\"$2\" \"$2.idsig\" unaligned.apk compiled_resources.zip dex_out obj")
        ], tabs);
        shellFile.AddEmptyLine();


        // Attempting to uninstall the stub (if previously installed.)
        shellFile.AddEchoCommand(text: "Attempting to uninstall package: $1", escaped: false, tabs);
        shellFile.AddCommand(command: ADBPath, [
            new(Flag: "shell", Value: "pm uninstall --user $PROFILE_ID \"$1\"")
        ]);
        shellFile.AddSleepCommand(seconds: 1, tabs);
        shellFile.AddEmptyLine();

        // Building the stub
        shellFile.AddEchoCommand(text: "Compiling dummy stub for source...", escaped: false, tabs);
        shellFile.AddCommand(command: $"./{BuildFileName}", arguments: null, tabs);
        shellFile.AddSleepCommand(seconds: 1, tabs);


        RunDebugIfActive(shellFile);
        Environment.Exit(1);
    }


    /// <summary> Populates the contents of the 2 required Shell script files for the generated stub. </summmary>
    public static void PopulateShellFiles(string profileID = "0") 
    {
        PopulateBuildScript();
        PopulateRunScript(profileID);
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


    /// <summary> Writes the APK alignment block to the provided shellFile. </summary>
    private static void WriteBuildScriptAlignmentBlock(ShellFile shellFile, ref int tabs) 
    {
        // Notifying the user of the alignment process that is about to occur and documenting the different flags used.
        shellFile.AddEchoCommand("\\n[8/16] -> Aligning APK...\\n");
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

        tabs++; // Increasing tabs 0 -> 1
        shellFile.AddEchoCommand("\\nUnable to align compiled APK, please try again.\\n", escaped: true, tabs);
        shellFile.AddExitCommand(status: 1, tabs);

        tabs--; // Decreasing tabs 1 -> 0
        shellFile.AddLine("fi");
        shellFile.AddEmptyLine();
    }


    /// <summary> Writes the APK alignment confirmation block to the provided shellFile. </summary>
    private static void WriteBuildScriptConfirmationBlock(ShellFile shellFile, ref int tabs) 
    {
        // Notifying the user of the confirmation process that is about to occur and documenting the different flags used.
        shellFile.AddEchoCommand("\\n[9/16] -> Confirming Alignment...\\n");
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
        tabs++; // Increasing tabs 0 -> 1

        shellFile.AddEchoCommand("\\nAlignment Confirmed!", escaped: true, tabs);
        
        tabs--; // Decreasing tabs 1 -> 0
        shellFile.AddLine("else", tabs);
        
        tabs++; // Increasing tabs 0 -> 1
        shellFile.AddEchoCommand("\\nIncorrect Alignment Detected!", escaped: true, tabs);
        shellFile.AddExitCommand(status: 1, tabs);
        
        tabs--; // Decreasing tabs 1 -> 0
        shellFile.AddLine("fi", tabs);
        shellFile.AddEmptyLine();

        // Notifying the user that the unaligned APK is being removed
        shellFile.AddEchoCommand("\\n[10/16] -> Removing Unaligned APK...");
        shellFile.AddCommand(command: "rm", [ 
            new ShellCommandArgument(Flag: "-f", Value: "unaligned.apk")
        ]);
        shellFile.AddEmptyLine();
    }


    /// <summary> Writes the APK compilation block to the provided shellFile. </summary>
    private static void WriteBuildScriptCompilationBlock(ShellFile shellFile, int tabs) 
    {
        shellFile.AddEchoCommand("\\n[5/16] -> Compiling stub from source using javac...\\n", escaped: true, tabs);
        // Compiling the stub's Java source files.
        // javac -d obj --release 8 -classpath {AndroidSDKJarPath} src/main/\"$PKG_DIR_STRUCTURE\"/*.java
        shellFile.AddCommand(command: "javac", arguments: [
            new ShellCommandArgument(Flag: "-d", Value: "obj"),
            new ShellCommandArgument(Flag: "--release", Value: "8"), // The Java API to be used for compilation
            new ShellCommandArgument(Flag: "-classpath", Value: AndroidSDKJarPath),
            new ShellCommandArgument(Flag: "src/main/\"$PKG_DIR_STRUCTURE\"/*.java")
        ]);
        shellFile.AddEmptyLine();
    }


    /// <summary> Writes the Java byte code conversion block to the provided shellFile. </summary>
    private static void WriteBuildScriptConversionBlock(ShellFile shellFile, int tabs) 
    {
        shellFile.AddEchoCommand("\\n[6/16] -> Converting Java ByteCode to Android Dex...\\n", escaped: true, tabs);
        // java -cp {AndroidR8JarPath} // 
        //      com.android.tools.r8.D8 //
        //      --lib {AndroidSDKJarPath} //
        //      --release //
        //      --output dex_out/ obj/\"$CLASS_DIR\"*.class
        // TODO: Make an alternative function called AddJavaCopyLine
        // Converting the Java ByteCode to Android Dex.
        shellFile.AddCommand(command: "java", [
            new ShellCommandArgument(Flag: "-cp", Value: AndroidR8JarPath), 
            new ShellCommandArgument(Flag: "com.android.tools.r8.D8"), // The class name for the D8 package.
            new ShellCommandArgument(Flag: "--lib", Value: AndroidSDKJarPath),
            // Indicating D8 should compile using the specified Java API version.
            // TODO: Run tests with Value: "8"
            new ShellCommandArgument(Flag: "--release"),
            new ShellCommandArgument(Flag: "--output", "dex_out/ obj/\"$CLASS_DIR\"*.class")
        ]);
        shellFile.AddEmptyLine();
    }


    /// <summary> 
    /// Writes the build script's header including the shebang operator, license source notice, and argument confirmation.
    /// </summary>
    private static void WriteBuildScriptHeaderBlock(ShellFile shellFile, int tabs) 
    {
        // Adding the shebang operator.
        // shellFile.AddLine("#!/usr/bin/env bash");
        shellFile.AddLine(ShebangOperator);

        // Writing the GNUv3 license notice to the current shell file.
        shellFile.WriteLicenseNotice();
        
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
        WriteBuildScriptVariablesBlock(shellFile, tabs);        
        shellFile.AddEmptyLines(2);
    }


    /// <summary> Writes the Android Manifest linking block to the provided shellFile. </summary>
    private static void WriteBuildScriptManifestLinkBlock(ShellFile shellFile, int tabs) 
    {
        // echo -e \"\\n[4/16] -> Linking XML Manifest using aapt2...\\n\"
        shellFile.AddEchoCommand("\\n[4/16] -> Linking XML Manifest using aapt2...\\n", escaped: true, tabs);

        // Linking the project's resources
        // aapt2 link --auto-add-overlay \\
        //       --manifest {RelativeManifestPath} \\
        //       -I {AndroidSDKJarPath} \\
        //       -R compiled_resources.zip \\
        //       -o unaligned.apk
        shellFile.AddCommand($"{AAPT2Path}", [
            new ShellCommandArgument(Flag: "link",       Value: "--auto-add-overlay"),
            new ShellCommandArgument(Flag: "--manifest", Value: RelativeManifestPath),
            new ShellCommandArgument(Flag: "-I",         Value: AndroidSDKJarPath),
            new ShellCommandArgument(Flag: "-R",         Value: "compiled_resources.zip"),
            new ShellCommandArgument(Flag: "-o",         Value: "unaligned.apk")
        ], tabs);
        shellFile.AddEmptyLine();
    }


    /// <summary> Writes the Android Dex class packaging block to the specified shellFile. </summary>
    private static void WriteBuildScriptPackagingBlock(ShellFile shellFile, int tabs) 
    {
        shellFile.AddEchoCommand("\\n[7/16] -> Packaging Output Dex Classes...\\n", escaped: true, tabs);
        
        // Packaging the dex output class
        // zip -uj unaligned.apk dex_out/classes.dex
        shellFile.AddCommand(command: "zip", [
            new ShellCommandArgument(
                Flag: "-uj", 
                Value: "unaligned.apk dex_out/classes.dex"
            )
        ]);
        shellFile.AddEmptyLine();
    }


    /// <summary> Writes the Stub's resource compilation block to the specified shellFile. </summary>
    private static void WriteBuildScriptResourcesBlock(ShellFile shellFile, int tabs) 
    {
        // mkdir -p obj dex_out
        shellFile.AddMkdirCommand(directories: ["obj", "dex_out"], createParents: true);
        shellFile.AddEmptyLine();

        // echo -e \"\\n[3/16] -> Compiling Resources for $APP_NAME...\\n\"
        shellFile.AddEchoCommand("\\n[3/16] -> Compiling Resources for $APP_NAME...\\n", escaped: true, tabs);

        // Compiling the stub's resources to compiled_resources.zip
        // aapt2 compile --dir \"$PROJECT_ROOT/src/main/res\" -o \"$PROJECT_ROOT/compiled_resources.zip\"
        shellFile.AddCommand($"{AAPT2Path} compile", [
            new ShellCommandArgument(Flag: "--dir", Value: "$PROJECT_ROOT/src/main/res"),
            new ShellCommandArgument(Flag: "-o", Value: "$PROJECT_ROOT/compiled_resources.zip")
        ]);
        shellFile.AddEmptyLine();
    }


    /// <summary> Writes the APK signing block to the specified shellFile. </summary>
    private static void WriteBuildScriptSigningBlock(ShellFile shellFile, ref int tabs) 
    {
        // Notifying the user that the aligned APK is going to be signed.
        shellFile.AddEchoCommand("\\n[11/16] -> Signing APK...\\n", escaped: true, tabs);

        // Checking if the debug.keystore file exists, if not it is generated.
        shellFile.AddLine("if [ ! -f \"debug.keystore\" ]; then", tabs);

        tabs++; // Increasing tabs 0 -> 1
        shellFile.AddEchoCommand("File debug.keystore not found, generating now.", escaped: true, tabs);
        
        shellFile.AddCommand(command: "keytool", [
            new(Flag: "-genkey"),
            new(Flag: "-v"), // Verbose output
            new(Flag: "-keystore", Value: "debug.keystore"), // Specifying the keystore file.
            new(Flag: "-storepass", Value: "android"),
            new(Flag: "-alias", Value: "androiddebugkey"),
            new(Flag: "-keypass", Value: "android"),
            new(Flag: "-keyalg", Value: "RSA"), // Using RSA encryption for the signing process.
            new(Flag: "-keysize", Value: "2048"),
            // TODO: Change this to something more reasonable.
            new(Flag: "-validity", Value: "10000"), // Setting the number of days the signature is valid for.
            new(Flag: "-dname", Value: "CN=Android Debug,O=Android,C=US")
        ], tabs);

        tabs--; // Decreasing tabs 1 -> 0
        shellFile.AddLine("fi");
        shellFile.AddEmptyLine();
        shellFile.AddEmptyLine();

        // Performing a self signature on the Aligned APK using the debug.keystore file from above.
        shellFile.AddLine(
            line: "if ! apksigner sign --ks debug.keystore --ks-pass pass:android --out \"$APK_NAME\" aligned.apk; then", 
            tabs
        );
        
        tabs++; // Increasing tabs 0 -> 1
        shellFile.AddEchoCommand("Unable to sign the APK..", escaped: false, tabs);
        shellFile.AddExitCommand(status: 1, tabs);
        
        tabs--; // Decreasing tabs 1 -> 0
        shellFile.AddLine("fi", tabs);
        shellFile.AddEmptyLine();
    }


    /// <summary> 
    ///     Writes the final block containing cleanup operations to the specified shellFile. 
    /// </summary>
    private static void WriteBuildScriptCleanupBlock(ShellFile shellFile, ref int tabs) 
    {
        // Notifying the user that leftover build artifacts will be removed.
        shellFile.AddEchoCommand("\\n[12/16] -> Removing leftover build artifacts...\\n", escaped: true, tabs);
        
        // Removes the leftover build artifacts, exits if this fails.
        shellFile.AddLine("if ! rm -rf \"$APK_NAME.idsig\" aligned.apk unaligned.apk compiled_resources.zip dex_out obj; then");
        
        tabs++; // Increasing tabs 0 -> 1
        shellFile.AddEchoCommand("Unable to clean all leftover build artifacts..", escaped: true, tabs);
        shellFile.AddExitCommand(status: 1, tabs);

        tabs--; // Decreasing tabs 1 -> 0
        shellFile.AddLine("fi", tabs);
        shellFile.AddEmptyLine();
        
        // Notifies the user that the build process has been completed.
        shellFile.AddEchoCommand("Build finalized for $APP_NAME!\\n", escaped: true, tabs);
    }


    /// <summary> Writes the variable definition block to the specified shellFile. </summary>
    private static void WriteBuildScriptVariablesBlock(ShellFile shellFile, int tabs = 0) 
    {
        shellFile.AddVariableDeclaration(
            variableName: "APP_NAME", 
            value: "$1", 
            comment: "The name of the class that extends Android's Activity class.",
            tabs
        );
        // shellFile.AddLine("APP_NAME=\"$1\" # The name of the class that contains the an extension of Activity.");
        
        shellFile.AddVariableDeclaration(
            variableName: "PKG_DIR_STRUCTURE", 
            value: "$2", 
            comment: "The project's source directory structure",
            tabs
        );
        // shellFile.AddLine("PKG_DIR_STRUCTURE=\"$2\" # The project's source directory structure");
        
        shellFile.AddVariableDeclaration(
            variableName: "APK_NAME", 
            value: "$3",     
            comment: "The filename of the compiled APK",
            tabs
        );
        // shellFile.AddLine("APK_NAME=\"$3\" # The filename of the compiled APK");
        
        shellFile.AddVariableDeclaration(
            variableName: "CLASS_DIR", 
            value: "$PKG_DIR_STRUCTURE/$APP_NAME",
            comment: "The path directory containing the *.class files generated during compilation",
            tabs
        );
        // shellFile.AddLine("CLASS_DIR=\"$PKG_DIR_STRUCTURE/$APP_NAME\"");
        
        shellFile.AddVariableDeclaration(
            variableName: "PROJECT_ROOT", 
            value: "$(pwd)",
            comment: "The directory this current is being executed from.",
            tabs
        );
        // shellFile.AddLine("PROJECT_ROOT=\"$(pwd)\"");
    }


    /// <summary> 
    /// Writes the build script's header including the shebang operator, license source notice, and argument confirmation.
    /// </summary>
    private static void WriteRunScriptHeaderBlock(ShellFile shellFile, ref int tabs) 
    {
        // Adding the shebang operator.
        // shellFile.AddLine("#!/usr/bin/env bash");
        shellFile.AddLine(ShebangOperator);

        // Writing the GNUv3 license notice to the current shell file.
        shellFile.WriteLicenseNotice();
        shellFile.AddEmptyLine();

        // Inserting the invalid usage block.
        shellFile.AddInvalidUsageBlock([
            new("packageName", "com.yourname.yourapp"),
            new("apkFileName", "yourapp.apk"),
        ], ref tabs);  
    }


    /// <summary> Writes the 2 required shell script files to the specified project directory. </summary>
    public static void WriteShellFiles(string projectDirectory) 
    {
        foreach (var shellFile in ShellFiles) 
        {
            if (shellFile.Content.Length == 0) {
                WriteWarningMessage($"Unable to write required shell script file:\n\t -> {shellFile.FileName}");
                WriteErrorMessage("Current file contents is empty.", exit: true, exitCode: 1);
            }

            var filePath = Path.Combine(projectDirectory, shellFile.FileName);
            try {
                File.WriteAllLines(filePath, shellFile.Content.GetLines());   
            }
            catch (Exception ex) 
            {
                WriteWarningMessage($"Unable to write required shell script file:\n\t -> {shellFile.FileName}");
                WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
            }

        }
    }
}