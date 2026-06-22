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

using static Global.Messaging;
using static Helpers.IO.FileHelper;
public static class IconExtension 
{
    public static void SetBuffer(this Icon iconInfo) 
    {
        // If the inputFilePath passed as a parameter is valid, the function ends here.
        if (TrySerializePathToByteArray(iconInfo.InputFilePath, out iconInfo.IconBuffer)) {
            return;
        }
        
        WriteWarningMessage("Unable to serialize the provided icon content stream.");
        WriteInformation("Choosing a predefined vector for your icon...");
        var choice = DefaultDrawables.GetPsuedoRandomChoice();

        // A safe check on whether or not the embedded resource can be resolved.
        if (!TryGetManifestResourceStream(choice.ResourcePath, out var stream)) 
        {
            WriteWarningMessage("Unable to retrieve the predefined vector's content stream.");
            Environment.Exit(1);
        }

        // Assigns the predefined vector's contents to IconBuffer (if successful), otherwise exits.
        if (!TrySerializeStreamToByteArray(stream!, out iconInfo.IconBuffer)) 
        {
            WriteWarningMessage("Unable to serialize the predefined Vector's content stream.");
            Environment.Exit(1);
        }
    }
}