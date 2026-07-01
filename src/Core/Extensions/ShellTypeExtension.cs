
namespace DummyDroidStubGen.Core.Extensions;

using Types;

public static class ShellTypeExtension 
{
    /// <summary> Converts the current string to it's equivalent ShellType member. </summary>
    public static ShellType FromShellPath(this string shellPath) 
    {
        return shellPath switch 
        {
            "/bin/bash" => ShellType.BASH,
            "/bin/zsh"  => ShellType.ZSH,
            "/bin/fish" => ShellType.FISH,
            "/bin/sh"   => ShellType.SH,
            _ => ShellType.UNKNOWN
        };
    }

    /// <summary> Converts the current ShellType to it's equivalent filepath. </summary>
    public static string ToPath(this ShellType shellType) 
    {
        return shellType switch {
            ShellType.BASH => "/bin/bash",
            ShellType.FISH => "/bin/fish",
            ShellType.SH => "/bin/sh",
            ShellType.ZSH => "/bin/zsh",
            _ => "#!/bin/sh"
        };
    }
}
