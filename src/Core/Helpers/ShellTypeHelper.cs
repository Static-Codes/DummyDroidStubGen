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
namespace DummyDroidStubGen.Core.Helpers;

using Extensions;
using Types;
using static Global.Constants;
using static Global.Messaging;
using static Helpers.IO.InputHelper;

/// <summary> Contains functionality to return the current ShellType in use (Linux Only) </summary>
public class ShellTypeHelper 
{
    /// <summary> Returns the contents of "/etc/shells" </summary>
    private static string[] ReadShellFile() 
    {
        try {
            return File.ReadAllLines("/etc/shells");
        }

        catch (Exception ex) 
        {
            WriteWarningMessage("An exception has occured while attempting to read /etc/shells on the current machine.");
            WriteErrorMessage(ex.Message);
        }

        return [];
    }

    /// <summary> Parses the returned contents from ReadShellFile() </summary>
    private static IEnumerable<ShellType> ParseShellFile(string[] shellFileContents) 
    {
        if (shellFileContents.Length == 0) {

            WriteWarningMessage("An exception has occured while attempting to read /etc/shells on the current machine.");
            WriteErrorMessage("Content is empty in ParseShellFile", exit: true, exitCode: 1);
        }
        
        if (!shellFileContents[0].Equals("# /etc/shells: valid login shells")) {
            WriteWarningMessage("A non fatal error has occured | Unexpected contents located on line 1 of /etc/shells");
        }

        return 
            shellFileContents
            .Where(path => File.Exists(path))  // Skipping shells that aren't accessible or installed.
            .Select(path => path.FromShellPath())
            .Where(shell => shell != ShellType.UNKNOWN);
    }

    /// <summary> Returns the current shell in use. If more than one is found, the user is prompted to select one. </summary>
    public static ShellType FindCurrent() 
    {
        var shellFileContents = ReadShellFile();

        if (shellFileContents.Length == 0) {
            WriteWarningMessage("Unable to determine the currently used shell.");
            WriteInformation("Falling back to sh as a default.");
            WriteInformation($"If this causes issues, please make a bug report at {ProjectIssueLink}");
            return ShellType.SH;
        }

        IEnumerable<ShellType> foundShells = ParseShellFile(shellFileContents);
        var foundShellCount = foundShells.Count();

        if (foundShellCount == 0) {
            WriteWarningMessage("Unable to determine the currently used shell.");
            WriteInformation("Falling back to sh as a default.");
            WriteInformation($"If this causes issues, please make a bug report at {ProjectIssueLink}");
            return ShellType.SH;
        }

        else if (foundShellCount == 1) {
            return foundShells.First();
        }

        var choice = AskForSelection(
            message: "Please select your desired shell to use for execution:", 
            options: foundShells.Select(shell => (object)shell)
        );

        UserExitStatusCheck(choice);

        if (Enum.TryParse<ShellType>(choice, ignoreCase: true, out var shellType)) {
            return shellType;
        }

        return ShellType.UNKNOWN;
    }

}