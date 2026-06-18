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

using Core.Types.Packaging;
using System.Text.Json;
using static DummyDroidStubGen.Core.Helpers.IO.FileHelper;
using static DummyDroidStubGen.Global.Messaging;

public class PackageHelpers 
{

    /// <summary>
    ///     Takes a package label, package name, and package category as input parameters. <br/>
    ///        
    ///     Possible Outcomes: <br/>
    /// 
    ///     1. If the package label is a non-null and non-empty string, it's value is returned. <br/>
    ///     
    ///     2. If the package name equals "android", the value returned is "System Image (android)" <br/>
    ///     
    ///     3. If the package category is PackageCategory.System, the value returned is "System Package (name)" <br/>
    /// 
    ///     4. Otherwise, returns "Unknown"
    /// </summary>
    public static string GetFriendlyName(string? label, string name, PackageCategory category) 
    {
        // Directly returning the label if provided.
        if (!string.IsNullOrWhiteSpace(label)) {
            return label;
        }

        if (name == "android") {
            return "System Image (android)";
        }

        // Skipping packages of the type PackageCategory.System
        if (category == PackageCategory.System) {
            return $"System Package ({name})";
        }

        return "Unknown";

    }
    

    /// <summary> 
    ///     Parses the embedded package lists for both the blacklist and graylist JSON files.
    /// </summary>
    public static List<Package> ParseEmbeddedResourceToPackageList(string resourcePath) 
    {
        if (!TryGetManifestResourceStream(resourcePath, out Stream? stream)) {
            Environment.Exit(1);
        }

        List<Package> packageList;

        try {
            packageList = JsonSerializer.Deserialize<List<Package>>(stream!) ?? [];
        } 

        catch (Exception ex) {
            WriteWarningMessage($"Unable to deserialize the contents of embedded resource at path: {resourcePath}");
            WriteErrorMessage(ex.Message);
            packageList = [];
        }

        return packageList;
    }
}