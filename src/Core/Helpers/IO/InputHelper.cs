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
namespace DummyDroidStubGen.Core.Helpers.IO;

using Spectre.Console;
using System.Diagnostics.CodeAnalysis;
using static Global.Constants;
using static Global.Messaging;
using static Types.ADB.Connection;

public static class InputHelper 
{   
    public const string ExitChoice = "Exit";
    private static readonly Style _style = new(decoration: Decoration.Bold);


    /// <summary> Prompts the user for input via a simple text prompt, and returns said input. </summary>    
    public static string AskForInput(string message) => AnsiConsole.Prompt(MakeTextPrompt(message));


    /// <summary> Prompts the user to select a ConnectionMethod, either USB or WIFI. </summary>
    public static ConnectionMethod AskForConnectionMethod()
    {
        // Dynamically resolving the members of ConnectionMethod and casting each member as an object.
        var connectionOptions = Enum.GetValues<ConnectionMethod>().Cast<object>();

        var promptResponse = AskForSelection(
            message: "Please select your desired connection method for your android device.", 
            options: connectionOptions
        );
        return Enum.Parse<ConnectionMethod>(promptResponse);
    }


    /// <summary> Prompts the user to select a choice from a CLI menu of options. </summary>
    public static string AskForSelection(string message, IEnumerable<object> options, int? pageSize = null)
    {
        var sanitizedMessage = $"[{OrangeHex}]{InputTag}[/] {message}";

        var promptResponse = AnsiConsole.Prompt(
            MakeSelectionPrompt(sanitizedMessage, [.. options], pageSize)
        );

        UserExitStatusCheck(promptResponse);

        return promptResponse;
    }
    
    /// <summary> If the current choice equals, "Exit" </summary>
    public static bool IsExitOption(this string choice) => choice.Equals(ExitChoice);


    /// <summary> 
    ///     Appends the choice "Exit" to the given IEnumerable of the type string. 
    /// </summary>
    public static IEnumerable<string> MakeInputMenu(IEnumerable<string> options) {
        return options.Append(ExitChoice); 
    }


    private static SelectionPrompt<string> MakeSelectionPrompt(string message, object[] options, int? pageSize, bool search = false) 
    {
        if (options.Length == 0) {
            WriteWarningMessage("Unable to create selection menu");
            WriteErrorMessage(
                message: "Parameter 'options' is empty in InputHelper.MakeSelectionPrompt()", 
                exitCode: 1, 
                exit: true
            );
        }


        var sanitizedOptions = 
            options.Select(
                opt => (
                    opt.GetType() != typeof(string) ? 
                    opt.ToString().EscapeMarkup() : // Handling non string options
                    (string)opt // Handling string options
                )
            );
        
        // Adding the "Exit" menu option.
        var finalOptions = MakeInputMenu(sanitizedOptions);
        
        int minimumSize = 3;

        // If the page size is provided:
        //  - Its value is increased by one to account for the addition of the "Exit" option.
        //  - This new size is compared to the minimum size of 3.
        //
        // If the page size is not provided:
        //  - The number of options is compared to the minimum size of 3.
        var safePageSize =
            pageSize != null ? 
            Math.Max((int)pageSize + 1, minimumSize) : 
            Math.Max(finalOptions.Count(), minimumSize);

        var prompt = new SelectionPrompt<string>()
            .HighlightStyle(_style)
            .Title(message)
            .PageSize(safePageSize)
            .AddChoices(finalOptions);

        if (search) {
            prompt.EnableSearch();
        }

        return prompt;
    }

    // Adding an orange [INPUT] tag to match the logic in MakeSelectionPrompt
    private static TextPrompt<string> MakeTextPrompt(string message) => new($"[{OrangeHex}]{InputTag}[/] {message}");


    /// <summary> Ends program execution if the provided inputString equals "Exit" </summary>
    public static void UserExitStatusCheck(string inputString) 
    {
        if (inputString.IsExitOption()) {
            WriteInformation("Operation cancelled by user.");
            WriteInformation("Exiting.");
            Environment.Exit(1);
        }
    }

    // The [NotNull] attribute tells roslyn, if this method returns:
    // The argument 'input' is guaranteed not to be null.
    public static void CheckForNullInput([NotNull] object? input)
    {
        if (input is null) {
            throw new ArgumentNullException(nameof(input), "Selection cannot be null.");
        }
    }
}
