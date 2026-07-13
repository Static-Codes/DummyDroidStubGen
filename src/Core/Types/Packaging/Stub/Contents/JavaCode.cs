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

using System;
using System.Collections.Generic;
using DummyDroidStubGen.Core.Extensions;
using static Global.Messaging;


public record JavaExtension(bool Required, string ClassName);
public record JavaImport(bool Static, string Name);

/// <summary> Contains the functions and members used to generate the stub's Java source files. </summary>
public class JavaCode
{
    /// <summary> A semantic representation of an empty JavaFileContent list. </summary>
    private static readonly JavaFileContent Empty = new([]);

    /// <summary> A List of JavaFile objects to be called by PopulateSourceFiles and WriteSourceFiles. </summary>
    private static readonly List<JavaFile> JavaFiles =
    [
        new JavaFile(fileName: "Blacklist.java", content: Empty),
        new JavaFile(fileName: "MainActivity.java", content: Empty),
        new JavaFile(fileName: "PackageResult.java", content: Empty),
    ];

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

    /// <summary> 
    ///     Attempts to return a JavaFile object from JavaFiles using the provided fileName. <br/>
    ///     If successful, file will be assigned this resolved JavaFile object, and returns true. <br/>
    ///     Otherwise, file will be assigned a null value, and false will be returned.
    /// </summary>
    public static bool TryGetJavaFile(string fileName, out JavaFile? file) 
    {
        file = null;
        try {
            file = JavaFiles.Where(f => f.FileName == fileName).FirstOrDefault();
        }
        catch (Exception ex) {
            WriteWarningMessage($"Unable to lookup local stub source file: {fileName}");
            WriteErrorMessage(ex.Message);
            return false;
        }
        return file != null;
    }


    /// <summary> Populates the contents of Blacklist.java using the provided package object. </summary>
    private static void PopulateBlacklist(Package package) 
    {
        if (!TryGetJavaFile("PackageResult.java", out var javaFile) || javaFile == null) {
            Environment.Exit(1);
        }

        JavaImport[] imports = [
            new(Static: false, Name: "java.util.Arrays"),
            new(Static: false, Name: "java.util.HashSet"),
            new(Static: false, Name: "java.util.Set"),
        ];

        WriteJavaFileHeader(javaFile, package, imports);

        javaFile.AddClassName(className: "Blacklist");
        javaFile.AddOpeningBracket();

        int tabs = 1;

        javaFile.AddLine(
            line: "private static HashSet<String> BLACKLIST = new HashSet<>(", 
            tabs
        );

        // Increasing tabs 1 -> 2
        tabs++;
        javaFile.AddLine(line: "Arrays.asList(", tabs);

        // Increasing tabs 2 -> 3
        tabs++;

        var blacklistedPackages = Package.GetBlacklistedPackages();

        // This is infinitely more efficient than using a hardcoded blacklist on the stub generator, or the stub itself.
        javaFile.AddLines(lines: blacklistedPackages.Select(bp => $"\"{bp.Name}\","), tabs);

        // Decreasing tabs 3 -> 2
        tabs--;
        javaFile.AddLine(line: ")", tabs);

        // Decreasing tabs 2 -> 1
        tabs--;
        javaFile.AddLine(line: ");", tabs);
        javaFile.AddEmptyLine();
        javaFile.AddEmptyLine();

        // javaFile.AddLine("private static final Set<String> BLACKLIST = new HashSet<>(cryptoPackageNames);", tabs);
        javaFile.AddEmptyLine();
        javaFile.AddLine("public static boolean isBlacklisted(String packageName)", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 1 -> 2
        tabs++;
        javaFile.AddLine("if (packageName == null) { return true; }", tabs);
        javaFile.AddEmptyLine();
        javaFile.AddLine("packageName = packageName.toLowerCase();", tabs);
        javaFile.AddEmptyLine();

        javaFile.AddComment("Inclusive block on sensitive package names.", tabs);
        javaFile.AddLine(
            line: "String[] genericKeywords = { \"bank\", \"crypto\", \"installer\", \"security\", \"wallet\"};",
            tabs
        );
        javaFile.AddLine(
            line: "boolean blacklistedPackageFound = Arrays.stream(genericKeywords).anyMatch(packageName::contains);",
            tabs
        );
        javaFile.AddEmptyLine();
        javaFile.AddLine("return blacklistedPackageFound ? false : BLACKLIST.contains(packageName);", tabs);

        tabs--;
        javaFile.AddClosingBracket(tabs);
        
        tabs--;
        javaFile.AddClosingBracket(tabs);
    }


    /// <summary> Populates the contents of MainActivity.java using the provided package object. </summary>
    private static void PopulateMainActivity(Package package) 
    {
        if (!TryGetJavaFile("MainActivity.java", out var javaFile) || javaFile == null) {
            Environment.Exit(1);
        }

        JavaImport[] imports = [
            new(Static: false, Name: "import android.app.Activity"), 
            new(Static: false, Name: "import android.content.ComponentName"), 
            new(Static: false, Name: "android.os.UserHandle"),
            new(Static: false, Name: "android.content.pm.LauncherActivityInfo"),
            new(Static: false, Name: "android.content.pm.LauncherApps"),
            new(Static: false, Name: "android.os.Bundle"),
            new(Static: false, Name: "android.os.Handler"),
            new(Static: false, Name: "android.os.UserHandle"),
            new(Static: false, Name: "android.os.UserManager"),
            new(Static: false, Name: "android.widget.LinearLayout"),
            new(Static: false, Name: "android.widget.TextView"),
            new(Static: false, Name: "java.util.List"),

            new(Static: true, Name: "android.view.Gravity.CENTER"),
            new(Static: true, Name: "android.view.WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON"),
            new(Static: true, Name: "android.widget.Toast.makeText"),
            new(Static: true, Name: "android.widget.Toast.LENGTH_LONG"),
            new(Static: true, Name: "android.widget.Toast.LENGTH_SHORT")
        ];


        WriteJavaFileHeader(javaFile, package, imports);        
        javaFile.AddClassName(
            className: "MainActivity", 
            extension: new JavaExtension(
                Required: true, 
                ClassName: "Activity"
            )
        );
        javaFile.AddOpeningBracket();
        
        int tabs = 1;

        javaFile.AddLine($"private final String appName = {package.Label};", tabs);
        javaFile.AddLine($"private final String packageName = {package.Name};", tabs);
        javaFile.AddEmptyLine();

        javaFile.AddLine("private PackageResult packageResult = null;", tabs);
        javaFile.AddEmptyLine();

        
        WriteMainActivityOnCreate(ref javaFile, ref tabs);
        WriteMainActivityOnResume(ref javaFile, ref tabs);
        WriteMainActivityDelay(ref javaFile, ref tabs);
        WriteMainActivityLocatePackage(ref javaFile, ref tabs);
        WriteMainActivityOpenPackage(ref javaFile, ref tabs);
        WriteMainActivityNotices(ref javaFile, ref tabs);

        // RunDebugIfActive(javaFile);


    }


    /// <summary> Populates the contents of PackageResult.java using the provided package object. </summary>
    private static void PopulatePackageResult(Package package) 
    {
        if (!TryGetJavaFile("PackageResult.java", out var javaFile) || javaFile == null) {
            Environment.Exit(1);
        }

        JavaImport[] imports = [
            new(Static: false, Name: "android.content.ComponentName"), 
            new(Static: false, Name: "android.content.pm.LauncherApps"), 
            new(Static: false, Name: "android.os.UserHandle")
        ];

        WriteJavaFileHeader(javaFile, package, imports);
        
        javaFile.AddEmptyLine();
        javaFile.AddClassName("PackageResult");
        javaFile.AddOpeningBracket();
        
        javaFile.AddComment("If the package was located in any of the device's profiles.", tabs: 1);
        javaFile.AddLine("public boolean PackageFound = false;", tabs: 1);
        javaFile.AddEmptyLine();

        javaFile.AddComment("A UserHandle where the associated package is installed.", tabs: 1);
        javaFile.AddLine("public UserHandle Profile = null;", tabs: 1);
        javaFile.AddEmptyLine();

        javaFile.AddComment("The component name associated with the desired application.", tabs: 1);
        
        javaFile.AddLine("public ComponentName MainActivityName = null;", tabs: 1);
        javaFile.AddEmptyLine();

        javaFile.AddComment(
            "The applications that the stub can directly launch without accessibility services.", 
            tabs: 1
        );

        javaFile.AddLine("public LauncherApps AppsVisibleToStub = null;", tabs: 1);
        javaFile.AddEmptyLine();

        // public PackageResult(boolean packageFound, UserHandle profile, ComponentName mainActivityName, LauncherApps appsVisibleToStub)
        javaFile.AddSecondaryConstructor
        (
            className: "PackageResult",
            args: [
                "boolean packageFound", 
                "UserHandle profile", 
                "ComponentName mainActivityName", 
                "LauncherApps appsVisibleToStub"
            ],
            tabs: 1
        );

        javaFile.AddOpeningBracket(tabs: 1);
        javaFile.AddLine("PackageFound = packageFound;", tabs: 2);
        javaFile.AddLine("Profile = profile;", tabs: 2);
        javaFile.AddLine("MainActivityName = mainActivityName;", tabs: 2);
        javaFile.AddLine("AppsVisibleToStub = appsVisibleToStub;", tabs: 2);
        javaFile.AddClosingBracket(tabs: 1);
        javaFile.AddClosingBracket();
        
        // RunDebugIfActive(javaFile);
        
    }
    

    /// <summary> Populates the contents of the 3 required Java source files for the generated stub. </summmary>
    public static void PopulateSourceFiles(Package package) 
    {
        PopulateBlacklist(package);
        PopulatePackageResult(package);
        PopulateMainActivity(package);
    }
    
    
    /// <summary> If the application is being run via "dotnet run", the contents of the JavaFile is displayed. </summary>
    private static void RunDebugIfActive(JavaFile javaFile) 
    {
        #if DEBUG    
            foreach (var line in javaFile.Content.Get()) 
            {
                string indent = GetDebugLinePadding(
                    currentLineNumber: line.Number, 
                    totalLines: javaFile.Content.Length
                );

                WriteDebugMessage($"Line {indent}{line.Number}: {line.Content}");
            }
        #endif
    }
    

    /// <summary> Writes the GNUv3 license notice, the package name, and the class imports. </summary>
    private static void WriteJavaFileHeader(JavaFile javaFile, Package package, JavaImport[] imports) 
    {
        javaFile.WriteLicenseNotice();
        javaFile.AddPackageName(package);
        javaFile.AddEmptyLine();

        foreach (var import in imports) {
            javaFile.AddImportStatement(import);
        }
        javaFile.AddEmptyLine();
    }


    /// <summary> Writes a closing bracket along with two new line chars to javaFile.Contents </summary>
    private static void WriteJavaBlockEnd(ref JavaFile javaFile, ref int tabs) 
    {
        if (tabs > 0) {
            tabs--;
        }

        // Decreasing tab length, if the length is greater than zero.
        // tabs = tabs > 0 ? tabs-- : 0;
        
        javaFile.AddClosingBracket(tabs);
        javaFile.AddEmptyLine();
        javaFile.AddEmptyLine();
    }


    /// <summary> Creates the function "onCreate(Bundle b)" in MainActivity.java </summary>
    private static void WriteMainActivityOnCreate(ref JavaFile javaFile, ref int tabs) 
    {
        javaFile.AddLine("@Override", tabs);
        javaFile.AddLine("protected void onCreate(Bundle b)", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 1 -> 2.
        tabs++;

        // Creates an instance of the MainActivity
        javaFile.AddLine("super.onCreate(b)", tabs);
        javaFile.AddEmptyLine();

        // Ensuring the connected device's screen doesn't turn off during execution.
        javaFile.AddComment(
            comment: "Ensuring the current device's screen does not timeout during the launch operation.",
            tabs
        );
        javaFile.AddLine("getWindow().addFlags(FLAG_KEEP_SCREEN_ON);", tabs);
        javaFile.AddEmptyLine();
        
        // Creating the UI LinearLayout
        javaFile.AddComment(
            comment: "Creating a new LinearLayout within the current context to center the TextView generated below.",
            tabs
        );
        javaFile.AddLine("LinearLayout layout = new LinearLayout(this);", tabs);
        javaFile.AddLine("layout.setGravity(CENTER);", tabs);
        javaFile.AddEmptyLine();

        // Creating the UI TextView.
        javaFile.AddComment(
            comment: "Creating a TextView within the current context to display a status message to the end-user.",
            tabs
        );
        javaFile.AddLine("TextView textView = new TextView(this);", tabs);
        javaFile.AddLine("textView.setText(String.format(\"Locating %1$s...\", appName));", tabs);
        javaFile.AddEmptyLine();

        // Adding the TextView to the LinearLayout
        javaFile.AddComment(
            comment: "Adding the TextView created above to the LinearLayout created prior.",
            tabs
        );
        javaFile.AddLine("layout.addView(textView);", tabs);
        javaFile.AddEmptyLine();

        // Adding the LinearLayout to the Activity context's view window.
        javaFile.AddComment(
            comment: "Adding the LinearLayout to the current context's view window.",
            tabs
        );
        javaFile.AddLine("setContentView(layout);", tabs);
        javaFile.AddEmptyLine();

        // Handling cases where a blacklisted package was used for the stub's generation.
        javaFile.AddComment(
            comment: "If the end-user has somehow generated a potentially vulnerable stub:",
            tabs
        );
        javaFile.AddComment(
            comment: "The request is blocked and execution is terminated immediately.",
            tabs
        );
        javaFile.AddLine("if (Blacklist.isBlacklisted(packageName))", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 2 -> 3.
        tabs++;

        // Writing the inner block of the blacklist check above.
        javaFile.AddLine("WriteBlacklistedAppNotice();", tabs);
        javaFile.AddLine("finish();", tabs);
        javaFile.AddLine("return;", tabs);
        
        // Decreasing tabs 3 -> 2.
        tabs--;

        // Ending the inner block
        javaFile.AddClosingBracket(tabs);
        javaFile.AddEmptyLine();

        // Launching the desired application.
        javaFile.AddLine("OpenPackage();", tabs);

        // Decreasing tabs 2 -> 1.
        tabs--;

        javaFile.AddClosingBracket(tabs);
    }
    
    
    /// <summary> Creates the function "onResume()" in MainActivity.java </summary>
    private static void WriteMainActivityOnResume(ref JavaFile javaFile, ref int tabs) 
    {
        javaFile.AddEmptyLine();
        javaFile.AddLine("@Override", tabs);
        javaFile.AddLine("protected void onResume()", tabs);
        javaFile.AddOpeningBracket(tabs);
        
        // Increasing tabs 1 -> 2
        tabs++;
        
        javaFile.AddLine("super.onResume();", tabs);
        javaFile.AddLine("finish();", tabs);

        WriteJavaBlockEnd(ref javaFile, ref tabs);

    }
    

    /// <summary> Creates the function "DelayCurrentThreadByMilliseconds()" in MainActivity.java </summary>
    private static void WriteMainActivityDelay(ref JavaFile javaFile, ref int tabs) 
    {
        javaFile.AddLine("public void DelayCurrentThreadByMilliseconds(int delay)", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 1 -> 2
        tabs++;
        javaFile.AddLine("new Handler(getMainLooper()).postDelayed(", tabs);

        // Increasing tabs 2 -> 3
        tabs++;
        javaFile.AddLine("new Runnable() {", tabs);

        // Increasing tabs 3 -> 4
        tabs++;
        javaFile.AddLine("@Override", tabs);
        javaFile.AddLine("public void run() {", tabs);

        // Increasing tabs 4 -> 5
        tabs++;
        javaFile.AddLine("finish();", tabs);


        // Decreasing tabs 5 -> 4
        tabs--;
        javaFile.AddClosingBracket(tabs);

        // Decreasing tabs 4 -> 3
        tabs--;
        javaFile.AddLine("}, delay", tabs);

        // Decreasing tabs 3 -> 2
        tabs--;
        javaFile.AddLine(");", tabs);

        // Decreasing tabs 2 -> 1
        // Closing the function
        tabs--;
        javaFile.AddClosingBracket(tabs);
        javaFile.AddEmptyLine();
        javaFile.AddEmptyLine();


    }
    

    /// <summary> Creates the function "LocatePackage()" in MainActivity.java </summary>
    private static void WriteMainActivityLocatePackage(ref JavaFile javaFile, ref int tabs) 
    {
        javaFile.AddComment(
            comment: "Determines if the package that is to be launched is sandboxed, or in another User Handle (or Cross-Profile)",
            tabs
        );
        javaFile.AddLine("private PackageResult LocatePackage(String packageName)", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 1 -> 2
        tabs++;
        javaFile.AddLine(
            line: "LauncherApps appsVisibleToStubs = (LauncherApps) getSystemService(LAUNCHER_APPS_SERVICE);",
            tabs
        );
        javaFile.AddLine("UserManager userManager = (UserManager) getSystemService(USER_SERVICE);", tabs);
        javaFile.AddLine("List<UserHandle> profiles = userManager.getUserProfiles();", tabs);
        javaFile.AddEmptyLine();

        javaFile.AddComment("Checking each of the UserHandles for the specified package string.", tabs);
        javaFile.AddLine("for (UserHandle profile : profiles)", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 2 -> 3
        tabs++;
        javaFile.AddComment(
            comment: "Attempting to locate the activities in the given profile associated with the desired package name.",
            tabs
        );
        javaFile.AddLine(
            line: "List<LauncherActivityInfo> activities = appsVisibleToStubs.getActivityList(packageName, profile);", 
            tabs
        );
        javaFile.AddEmptyLine();
        javaFile.AddLine("Skipped past failed resolutions.", tabs);
        javaFile.AddLine("if (activities == null || activities.isEmpty()) { continue; }", tabs);
        javaFile.AddEmptyLine();

        javaFile.AddComment(
            comment: "Every android package has a .MainActivity class or equivalent which serves as an entry point.", 
            tabs
        );
        javaFile.AddLine("ComponentName mainActivityName = activities.get(0).getComponentName();", tabs);
        javaFile.AddEmptyLine();

        javaFile.AddLine("return new PackageResult(true, profile, mainActivityName, appsVisibleToStubs);", tabs);

        // Decreasing tabs 3 -> 2
        WriteJavaBlockEnd(ref javaFile, ref tabs);
        javaFile.AddLine("return new PackageResult(false, null, null, null);", tabs);
        

        // Ending the function
        // Decreasing tabs 2 -> 1
        WriteJavaBlockEnd(ref javaFile, ref tabs);
    }


    /// <summary> Creates the function "OpenPackage()" in MainActivity.java </summary>
    private static void WriteMainActivityOpenPackage(ref JavaFile javaFile, ref int tabs) 
    {
        javaFile.AddComment("Attempts to launch the application associated with the stub.", tabs);
        javaFile.AddComment("Handles sandboxed or cross-profile applications as well.", tabs);
        javaFile.AddLine("private void OpenPackage()", tabs);
        javaFile.AddOpeningBracket(tabs);
        
        // Increasing tabs 1 -> 2
        tabs++;
        javaFile.AddLine("packageResult = LocatePackage(packageName);", tabs);
        javaFile.AddEmptyLine();
        javaFile.AddComment("If the desired package is not present:", tabs);
        javaFile.AddComment("The user is notified and the stub's execution is terminated.", tabs);
        javaFile.AddComment("if (!packageResult.PackageFound)", tabs);
        javaFile.AddOpeningBracket(tabs);


        // Increasing tabs 2 -> 3
        tabs++;
        javaFile.AddLine("WriteMissingAppNotice();", tabs);
        javaFile.AddLine("finish();", tabs);
        javaFile.AddLine("return;", tabs);

        // Decreasing tabs 3 -> 2
        tabs--;
        javaFile.AddClosingBracket(tabs);
        javaFile.AddEmptyLine();
        javaFile.AddComment("Firing an intent to open the desired package.", tabs);
        javaFile.AddLine("try", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 2 -> 3
        tabs++;
        javaFile.AddLine("packageResult.AppsVisibleToStub.startMainActivity(", tabs);
        

        // Increasing tabs 3 -> 4
        tabs++;
        javaFile.AddLine("packageResult.MainActivityName,", tabs);
        javaFile.AddLine("packageResult.Profile,", tabs);
        javaFile.AddLine("null,", tabs);
        javaFile.AddLine("null,", tabs);
        

        // Decreasing tabs 4 -> 2
        tabs -= 2;
        javaFile.AddClosingBracket(tabs);
        javaFile.AddEmptyLine();


        // Increasing tabs 3 -> 4
        javaFile.AddLine("catch (Exception ex) {", tabs);

        // Increasing tabs 4 -> 5
        tabs++;
        javaFile.AddLine(
            line: "String toastMessage = String.format(\"Unable to launch: %1$s\", appName);",
            tabs
        );
        javaFile.AddLine("makeText(this, toastMessage, LENGTH_SHORT);", tabs);
        javaFile.AddLine("makeText(this, ex.getMessage(), LENGTH_LONG);", tabs);
        javaFile.AddLine("finish();", tabs);


        // Decreasing tabs 5 -> 4
        tabs--;
        javaFile.AddClosingBracket(tabs);
        javaFile.AddLine("finish();", tabs);
        javaFile.AddEmptyLine();

        // Decreasing tabs 2 -> 1
        WriteJavaBlockEnd(ref javaFile, ref tabs);
    }


    /// <summary> Handles the creation of the activity notices in MainActivity.java </summary>
    private static void WriteMainActivityNotices(ref JavaFile javaFile, ref int tabs) 
    {
        javaFile.AddLine("private void WriteBlacklistedAppNotice()", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 1 -> 2
        tabs++;
        javaFile.AddLine(
            line: "String toastMessage = \"Unable to complete the operation, (Reason: Blacklisted Package Name)\";",
            tabs
        );
        javaFile.AddLine("makeText(this, toastMessage, LENGTH_SHORT);", tabs);
        
        // Ending the function
        // Decreasing tabs 2 -> 1
        WriteJavaBlockEnd(ref javaFile, ref tabs);

        javaFile.AddLine("private void WriteLaunchedAppNotice()", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 1 -> 2
        tabs++;
        javaFile.AddLine("String toastMessage = String.format(\"Launched %1$s\", appName);", tabs);
        javaFile.AddLine("makeText(this, toastMessage, LENGTH_SHORT);", tabs);
        
        // Ending the function
        // Decreasing tabs 2 -> 1
        WriteJavaBlockEnd(ref javaFile, ref tabs);

        javaFile.AddLine("private void WriteMissingAppNotice()", tabs);
        javaFile.AddOpeningBracket(tabs);

        // Increasing tabs 1 -> 2
        tabs++;
        javaFile.AddLine(
            line: "String toastMessage = \"Unable to complete the operation, (Reason: Blacklisted Package Name)\";",
            tabs
        );
        javaFile.AddLine("makeText(this, toastMessage, LENGTH_SHORT);", tabs);

        // Ending the function
        // Decreasing tabs 2 -> 1
        WriteJavaBlockEnd(ref javaFile, ref tabs);


        // Ending the function
        // Decreasing tabs 2 -> 1
        WriteJavaBlockEnd(ref javaFile, ref tabs);
    }


    /// <summary> Writes the 3 required Java source files to the specified source directory. </summary>
    public static void WriteSourceFiles(string JavaSourceDirectory) 
    {
        foreach (var javaFile in JavaFiles) 
        {
            if (javaFile.Content.Length == 0) {
                WriteWarningMessage($"Unable to write required java source file:\n\t -> {javaFile.FileName}");
                WriteErrorMessage("Current file contents is empty.", exit: true, exitCode: 1);
            }

            var filePath = Path.Combine(JavaSourceDirectory, javaFile.FileName);
            try {
                File.WriteAllLines(filePath, javaFile.Content.GetLines());   
            }
            catch (Exception ex) 
            {
                WriteWarningMessage($"Unable to write required java source file:\n\t -> {javaFile.FileName}");
                WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
            }

        }
    }
    
}