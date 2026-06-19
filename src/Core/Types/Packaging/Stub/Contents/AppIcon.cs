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
namespace DummyDroidStubGen.Core.Types.Packaging.Stub.Contents;

using static DefaultDrawables;
using static Global.Messaging;
using static Helpers.IO.FileHelper;
public class AppIcon 
{
    private static bool TryWriteIcon(StubInfo stubInfo) 
    {   
        if (stubInfo.StubStructure.Icon.IconBuffer.Length == 0) 
        {
            WriteErrorMessage(
                message: "stubInfo.SetBuffer() failed to write the required icon buffer.", 
                exit: true, 
                exitCode: 1
            );
        }

        if (string.IsNullOrEmpty(stubInfo.StubStructure.Icon.OutputFilePath)) {
            WriteErrorMessage("stubInfo.StubStructure.IconInfo.OutputFilePath is null or empty.");
            return false;
        }

        try {
            File.WriteAllBytes(
                stubInfo.StubStructure.Icon.OutputFilePath, 
                stubInfo.StubStructure.Icon.IconBuffer
            );
        }

        catch (Exception ex) {
            WriteWarningMessage($"An error occurred when trying to write the contents of: {stubInfo.StubStructure.Icon.OutputFilePath}");
            WriteErrorMessage(ex.Message);
            return false;
        }

        return true;
    }


    public static bool Write(StubInfo stubInfo)
    {
        if (TryWriteIcon(stubInfo)) {
            return true;
        }

        WriteInformation("Falling back to a predefined DrawableVector.");
        var fallbackPath = GetPsuedoRandomChoice().ResourcePath;
        
        if (TryGetManifestResourceStream(fallbackPath, out var stream) && stream != null)
        {
            // Note to my future self to save my sanity:
            // Since the stream was returned as an output parameter, it cannot be defined again using:
            // using var stream = ...
            using (stream) 
            {
                byte[] buffer = new byte[stream.Length];
                stream.ReadExactly(buffer);
                return TryWriteIcon(stubInfo);
            }
        }

        return false;
    }
}