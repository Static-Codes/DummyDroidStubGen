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

using Contents;
using static Global.Messaging;

public class JavaFile(string FileName, Dictionary<int, string> Contents)
{
    public string FileName { get; init; } = FileName;
    public Dictionary<int, string> Contents { get; init; } = Contents;
    
    /// <summary>
    ///     If the extension.Required set to true, the class written extends extension.ClassName <br/>
    ///      
    /// </summary>
    public void AddClassName(string className, JavaExtension? extension = null, string[]? args = null, int tabs = 0) 
    {
        var line = $"public class {className}";

        var usingArgs = args != null && args.Length > 0;
        var usingExt = extension?.Required ?? false;

        if (usingArgs && usingExt) {
            WriteWarningMessage($"Unable to write class name: {className}");
            WriteErrorMessage(
                message: "You cannot create an extension class while using arguments.",
                exit: true,
                exitCode: 1
            );
        }

        // Writes: public class className(args...)    
        else if (usingArgs) {
            line = $"public class {className}({string.Join(", ", args!)})";
        }

        // Writes: public class {className} extends {extension.ClassName}
        else if (usingExt) {
            line = $"public class {className} extends {extension!.ClassName}";
        }

        AddLine(line, tabs);
    }

    /// <summary> Calls AddLine using "}" </summary>
    public void AddClosingBracket(int tabs = 0) => AddLine("}", tabs: tabs);

    /// <summary> Calls AddLine using $"// {comment}" </summary>
    public void AddComment(string comment, int tabs = 0) => AddLine($"// {comment}", tabs: tabs);

    /// <summary> Appends a KeyValuePair containing a new line char to the existing Dictionary. </summary>
    public void AddEmptyLine() => AddLine("");
    
    /// <summary> 
    ///     If the import is static, calls AddLine using $"import {javaImport.Name};". <br/>
    ///     Otherwise, calls AddLine using $"import static {javaImport.Name};".
    /// </summary>
    public void AddImportStatement(JavaImport javaImport) 
    {
        var statement = javaImport.Static ? 
                        $"import static {javaImport.Name};" : 
                        $"import {javaImport.Name};";
        AddLine(statement);
    }
    

    /// <summary> 
    ///     Creates a KeyValuePair to be inserted in JavaFile.Contents. <br/>
    ///     Inserts the KeyValuePair at the end of the Dictionary, or at index (if specified).
    /// </summary>
    public void AddLine(string line, int tabs = 0, int? index = null) 
    {
        // There are 4 spaces in the standard tab.
        string indent = new(' ', tabs * 4);

        var finalIndex = Contents.Count;

        // If the specified index
        if (index != null && index > 0 && index < Contents.Count) {
            finalIndex = (int)index;
        }

        Contents.Add(finalIndex, $"{indent}{line}"); 
    }

    public void AddLines(string[] lines, int tabs = 0) 
    {
        foreach (var line in lines) {
            AddLine(line, tabs);
        }
    }

    public void AddLines(IEnumerable<string> lines, int tabs = 0) 
    {
        foreach (var line in lines) {
            AddLine(line, tabs);
        }
    }
    
    /// <summary> Calls AddLine using "{" </summary>
    public void AddOpeningBracket(int tabs = 0) => AddLine("{", tabs: tabs);

    /// <summary> Calls AddLine using $"package {package.Name};". </summary>
    public void AddPackageName(Package package) => AddLine($"package {package.Name};");

    /// <summary> Calls AddLine using $"public {className}({string.Join(", ", args)})". </summary>
    public void AddSecondaryConstructor(string className, string[] args, int tabs = 0) {
        AddLine($"public {className}({string.Join(", ", args)})", tabs: tabs);
    }
}

public static class JavaFileExtension 
{
    /// <summary> Writes the GNUv3 License Notice at the beginning of the Java source files. </summary>
    public static void WriteLicenseNotice(this JavaFile file) 
    {
        file.AddLine("/*");
        file.AddLine("*");
        file.AddLine("* This program is free software: you can redistribute it and/or modify");
        file.AddLine("* the Free Software Foundation, either version 3 of the License, or");
        file.AddLine("* (at your option) any later version.");
        file.AddLine("*");
        file.AddLine("* This program is distributed in the hope that it will be useful,");
        file.AddLine("* but WITHOUT ANY WARRANTY; without even the implied warranty of");
        file.AddLine("* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the");
        file.AddLine("* GNU General Public License for more details.");
        file.AddLine("*");
        file.AddLine("* You should have received a copy of the GNU General Public License");
        file.AddLine("* along with this program.  If not, see <https://www.gnu.org/licenses/>.");
        file.AddLine("*/");
    }
} 