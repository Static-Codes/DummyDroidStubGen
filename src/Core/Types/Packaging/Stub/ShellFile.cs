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

using System.Text;
using static Global.Messaging;
using static Types.Packaging.Stub.Contents.ShellCode;

/// <summary> Represents an argument passed to ShellFile.AddCommand() </summary>
public record ShellCommandArgument(string Flag, string? Value, bool Assigns = false)
{
    public string Render() => (Assigns, Value) switch
    {
        (true, null)         => throw new InvalidOperationException(
                                                $"'{Flag}': Assigns is true, but Value is null."),
        (false, null)        => Flag,
        (false, var value)   => $"{Flag} {value}",
        (true, var value)    => $"{Flag}={value}",
    };
}

// <summary> Represents a command that will be called within a shell function. </summary>
public record ShellFunctionCommand(string Line, int TabCount);

/// <summary> Represents the contents of a single line within a ShellFileContent object. </summary>
public record ShellFileLine(int Number, string Content);

/// <summary> Represents the file contents of the associated ShellFile object. </summary>
public class ShellFileContent(List<ShellFileLine> lines) 
{
    private readonly List<ShellFileLine> Lines = lines;

    public int Length => Lines.Count;
    
    public void Add(ShellFileLine line) => Lines.Add(line);

    public List<ShellFileLine> Get() => Lines;

    public List<string> GetLines() => [.. Lines.Select(line => line.Content)];
}


/// <summary> Contains the functionality to generate a .sh file. </summary>
public class ShellFile(string FileName, ShellFileContent Content)
{
    public string FileName { get; init; } = FileName;

    /// <summary>
    public ShellFileContent Content { get; init; } = Content; 

    /// <summary> Calls AddLine using "}" </summary>
    public void AddClosingBracket(int tabs = 0) => AddLine("}", tabs);


    /// <summary> Calls AddLine using $"# {comment}" </summary>
    public void AddComment(string comment, int tabs = 0) => AddLine($"# {comment}", tabs);

    public void AddCommand(string command, ShellCommandArgument[]? arguments, int tabs = 0)
    {
        if (string.IsNullOrWhiteSpace(command)) {
            throw new ArgumentException(
                message: "command cannot be null or empty.", 
                paramName: nameof(command)
            );
        }
        
        var parts = (arguments ?? []).Select(arg => arg.Render());
        AddLine(string.Join(' ', [command, ..parts]), tabs);
    }

    /// <summary> Appends a ShellFileLine containing a new line char to the existing Dictionary. </summary>
    public void AddEmptyLine() => AddLine("");

    /// <summary> Appends a shell function using the functionName and commands provided. </summary>
    public void AddFunction(string functionName, ShellFunctionCommand[] commands, bool foldCommands = false) 
    {
        int tabs = 0;
        
        // Handling folded commands as a single line.
        if (foldCommands) {
            var foldedCommands = string.Join("; ", commands.Select(command => command.Line));
            AddLine($"function {functionName} {{ {foldedCommands}; }}", tabs);
            return;
        }

        foreach (var command in commands) {
            AddLine(command.Line, command.TabCount);
        }

        AddClosingBracket(tabs);
    }
    

    /// <summary> Represents a shell argument's name and the associated value. </summary>
    public record UsageArgument(string Name, string Value);

    /// <summary> Inserts an invalid usage block using the specified UsageArgument(s). </summary>
    public void AddInvalidUsageBlock(UsageArgument[] arguments, ref int tabs) 
    {
        if (arguments.Length == 0) {
            WriteWarningMessage("arguments is null in AddInvalidUsageBlock");
            return;
        }

        // Escaping the argument names to prevent incorrect insertion.
        var sanitizedNames = arguments.Select(arg => $"\\\"{arg.Name}\\\"");

        // Escaping the argument values to prevent incorrect insertion.
        var sanitizedValues = arguments.Select(arg => $"\\\"{arg.Value}\\\"");

        var numberLineBuilder = new StringBuilder();

        if (arguments.Length == 1) {
            numberLineBuilder.AppendLine("if [ -z \\\"$1\\\" ]; then");
        }

        else 
        {
            var sanitizedNumbers = new string[arguments.Length];

            for (int i = 0; i < arguments.Length; i++ )
            {
                sanitizedNumbers[i] = $"\\\"${i+1}\\\"";

                if (i == 0) {
                    numberLineBuilder.Append($"if [ -z {sanitizedNumbers[i]} ] || ");
                    continue;
                }

                else if (i == arguments.Length - 1) {
                    numberLineBuilder.Append($"[ -z {sanitizedNumbers[i]} ]; then");
                }

                else {
                    numberLineBuilder.Append($"[ -z {sanitizedNumbers[i]} ] || ");
                }

            }
        }

        AddLine(numberLineBuilder.ToString(), tabs);

        // Increasing tabs 0 -> 1
        tabs++;

        AddLine("echo \"Invalid usage.\"", tabs);
        
        AddLine(
            line: $"echo \"Expected: ./{BuildFileName} {string.Join(' ', sanitizedNames)}\"", 
            tabs
        );

        AddLine(
            line: $"echo \"Example: ./{BuildFileName} {string.Join(' ', sanitizedValues)}\"", 
            tabs
        );

        AddLine("exit 1", tabs);

        // Decreasing the reference value tabs 1 -> 0
        tabs--;

        AddLine("fi", tabs);
        AddEmptyLine(); 
    }


    /// <summary> 
    ///     Creates a ShellFileLine to be inserted in ShellFile.Contents. <br/>
    ///     Inserts the ShellFileLine at the end of the Dictionary, or at index (if specified).
    /// </summary>
    public void AddLine(string line, int tabs = 0, int? index = null) 
    {
        // There are 4 spaces in the standard tab.
        string indent = new(' ', tabs * 4);

        var finalIndex = Content.Length;

        // If the specified index
        if (index != null && index > 0 && index < Content.Length) {
            finalIndex = (int)index;
        }

        Content.Add(
            new ShellFileLine(
                Number: finalIndex, 
                Content: $"{indent}{line}"
            )
        ); 
    }


    /// <summary> Calls AddLine using each of lines in the provided array </summary>
    public void AddLines(string[] lines, int tabs = 0) 
    {
        foreach (var line in lines) {
            AddLine(line, tabs);
        }
    }


    /// <summary> Calls AddLine using each of lines in the provided IEnumerable. </summary>
    public void AddLines(IEnumerable<string> lines, int tabs = 0) 
    {
        foreach (var line in lines) {
            AddLine(line, tabs);
        }
    }


    /// <summary> Calls AddLine using "{" </summary>
    public void AddOpeningBracket(int tabs = 0) => AddLine("{", tabs: tabs);


    public void AddVariableDeclaration(string variableName, string value, string? comment = null, int tabs = 0) 
    {
        var builder = new StringBuilder().Append($"{variableName}=\"{value}\"");

        if (comment != null) {
            builder.Append($" # {comment}");
        }

        AddLine(builder.ToString(), tabs);
    }

}
