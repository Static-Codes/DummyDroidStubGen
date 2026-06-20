namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

public record FunctionCommand(string Line, int TabCount);

public class ShellFile(string FileName, Dictionary<int, string> Contents)
{
    public string FileName { get; init; } = FileName;
    public Dictionary<int, string> Contents { get; init; } = Contents; 

    /// <summary> Calls AddLine using "}" </summary>
    public void AddClosingBracket(int tabs = 0) => AddLine("}", tabs);

    /// <summary> Calls AddLine using $"# {comment}" </summary>
    public void AddComment(string comment, int tabs = 0) => AddLine($"# {comment}", tabs);

    /// <summary> Appends a KeyValuePair containing a new line char to the existing Dictionary. </summary>
    public void AddEmptyLine() => AddLine("");

    /// <summary> Appends a shell function using the functionName and commands provided. </summary>
    public void AddFunction(string functionName, FunctionCommand[] commands, bool foldCommands = false) 
    {
        int tabs = 0;
        AddLine($"function {functionName} {{", tabs);
        
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

        var finalIndex = Contents.Count;

        // If the specified index
        if (index != null && index > 0 && index < Contents.Count) {
            finalIndex = (int)index;
        }

        Contents.Add(finalIndex, $"{indent}{line}"); 
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



public enum ShellType { UNKNOWN = 0, BASH = 1, FISH = 2, ZSH = 3 };

public static class ShellTypeExtension 
{
    public static string? ToShebangOperator(this ShellType shellType) 
    {
        return shellType switch {
            ShellType.BASH => "#!/bin/bash",
            ShellType.FISH => "#!/bin/fish",
            ShellType.ZSH => "#!/bin/zsh",
            _ => null
        };
    }
}