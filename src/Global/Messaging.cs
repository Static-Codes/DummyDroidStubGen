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
namespace DummyDroidStubGen.Global;


// A <PackageReference> to Spectre.Console must be included prior to Referencing Messaging.cs in a .csproj
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;
using static Global.Constants;

internal static class Messaging
{
    public static void WriteDebugMessage(string message) {
        AnsiConsole.MarkupLine($"[{OrangeHex}]{DebugTag}[/] {message.EscapeMarkup()}");
    }
    public static void WriteErrorMessage(string message, int? exitCode = null, [DoesNotReturnIf(true)] bool exit = false)
    {
        AnsiConsole.MarkupLine($"[red]{ErrorTag}[/] {message.EscapeMarkup()}");
        if (exit) {
            Environment.Exit(exitCode ?? 1);
        }
    }

    /// <summary> Writes an information message to the active terminal. </summary>
    /// 
    /// <param name="reverse">If set to true, the colored text is rendered first. </param>
    public static void WriteInformation(string whiteText = "", string coloredText = "", string textColor = "blue", string tagName = InfoTag, string tagNameColor = "blue", bool reverse = false) 
    {
        string[] validColors = ["blue", "purple", "orange", "red"];

        if (!validColors.Contains(textColor)) {
            WriteErrorMessage("Invalid textColor passed to WriteInformation.");
            WriteWarningMessage("Supported Colors:");
            WriteInformation(coloredText: "\t blue", textColor: "blue");
            WriteInformation(coloredText: "\t purple", textColor: "purple");
            WriteInformation(coloredText: "\t orange", textColor: "orange");
            WriteInformation(coloredText: "\t red", textColor: "red");
            return;
        }

        textColor = textColor == "orange" ? OrangeHex : textColor;
        tagNameColor = tagNameColor == "orange" ? OrangeHex : tagNameColor;

        // If reverse is true, the white text comes after the blue text.
        string message = reverse switch {
            true => $"[{tagNameColor}]{tagName}[/] {whiteText.EscapeMarkup()}[{textColor}]{coloredText.EscapeMarkup()}[/]",
            false => $"[{tagNameColor}]{tagName}[/] [{textColor}]{coloredText.EscapeMarkup()}[/] {whiteText.EscapeMarkup()}"
            
        }; 
        AnsiConsole.MarkupLine(message);
    }

    public static void WriteStateMessage(string message) => AnsiConsole.MarkupLine($"[blue]{message}[/]");

    public static void WriteSuccessMessage(string message) => AnsiConsole.MarkupLine($"[green]{SuccessTag}[/] {message.EscapeMarkup()}");

    public static void WriteWarningMessage(string message) => AnsiConsole.MarkupLine($"[yellow]{WarningTag}[/] {message.EscapeMarkup()}");

}
