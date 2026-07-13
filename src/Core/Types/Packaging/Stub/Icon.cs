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

using DummyDroidStubGen.Core.Extensions;

public enum IconFileType { UNSET = 0, XML = 1, WEBP = 2, SVG = 3 }
public class Icon
{
    /// <summary> 
    ///     A byte array containing the XML contents of the Android DrawableVector to be used as a logo. <br/>
    ///     This logo will be saved at -> "path/to/Java/PackageName/src/res/icon.OutputFileType"
    /// </summary>
    public byte[] IconBuffer = [];
    public string? InputFilePath;
    public string? OutputFilePath;
    public IconFileType InputFileType = IconFileType.UNSET;
    public IconFileType OutputFileType = IconFileType.UNSET;

    public Icon(string inputIconPath, string outputIconPath) 
    {
        InputFileType = inputIconPath.ToIconFileType();
        OutputFileType = outputIconPath.ToIconFileType();

        if (OutputFileType == IconFileType.UNSET || OutputFileType == IconFileType.SVG) {
            throw new ArgumentException("Invalid IconFileType provided to StubIconInfo constructor, expected WEBP or XML");
        }

        InputFilePath = inputIconPath;
        OutputFilePath = outputIconPath;
        this.SetBuffer();
    }
}

