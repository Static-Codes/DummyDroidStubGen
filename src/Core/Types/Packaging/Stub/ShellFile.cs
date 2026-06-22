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
public class ShellFile(string FileName, ShellFileContent Contents)
{
    public string FileName { get; init; } = FileName;

    /// <summary>
    public ShellFileContent Contents { get; init; } = Contents; 

    /// <summary> Calls AddLine using "}" </summary>
    public void AddClosingBracket(int tabs = 0) => AddLine("}", tabs);


    /// <summary> Calls AddLine using $"# {comment}" </summary>
    public void AddComment(string comment, int tabs = 0) => AddLine($"# {comment}", tabs);


    /// <summary> Appends a KeyValuePair containing a new line char to the existing Dictionary. </summary>
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
    
    /// <summary> 
    ///     Creates a KeyValuePair to be inserted in JavaFile.Contents. <br/>
    ///     Inserts the KeyValuePair at the end of the Dictionary, or at index (if specified).
    /// </summary>
    public void AddLine(string line, int tabs = 0, int? index = null) 
    {
        // There are 4 spaces in the standard tab.
        string indent = new(' ', tabs * 4);

        var finalIndex = Contents.Length;

        // If the specified index
        if (index != null && index > 0 && index < Contents.Length) {
            finalIndex = (int)index;
        }

        Contents.Add(
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


}
