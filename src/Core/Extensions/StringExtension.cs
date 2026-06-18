namespace DummyDroidStubGen.Core.Extensions;

using DummyDroidStubGen.Core.Types.Packaging.Stub;
using System.Text;

using static Global.Messaging;

public static class StringExtension 
{
    /// <summary> 
    ///     Checks if the provided array has any members that start with the provided string. 
    /// </summary>
    public static bool StartsWithAny<T>(this string str, string[] Options) {
        return Options.Any(option => option.StartsWith(str));
    }

    /// <summary> 
    ///     An alternative implementation of StringBuilder.AppendLine() that will prepend the number of spaces specified.
    /// </summary> 
    public static StringBuilder AppendLine(this StringBuilder builder, string line, uint spaces = 0) 
    {
        // Much more efficient alternative to using IEnumerable.Repeat().
        string indentation = new(' ', (int)spaces);
        
        builder.Append(indentation);
        builder.Append(line);
    
        // Note to my future self, StringBuilder.AppendLine() appends the OS SPECIFIC LINE ENDING FOR YOU!
        return builder.AppendLine();
    } 


    /// <summary>
    ///     Parses a file path for it's file extension.
    ///     Assigns the file extension to the reference parameter.
    ///     Returns the IconFileType enum member associated with the extension, or IconFileType.UNSET otherwise.
    /// </summary>
    public static IconFileType ToIconFileType(this string InputIconPath, ref string? iconFileExt) 
    {   
        if (string.IsNullOrEmpty(InputIconPath)) {
            return IconFileType.UNSET;
        }

        try {
            iconFileExt = Path.GetExtension(InputIconPath).ToLower();
        }

        catch (Exception ex) {
            WriteWarningMessage($"Unable to locate the extension for path: {InputIconPath}");
            WriteErrorMessage(ex.Message);
            return IconFileType.UNSET;
        }

        return iconFileExt switch {
            ".svg" => IconFileType.SVG,
            ".webp" => IconFileType.WEBP,
            ".xml" => IconFileType.XML,
            _ => IconFileType.UNSET,
        };
    }
}