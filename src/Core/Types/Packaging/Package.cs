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
namespace DummyDroidStubGen.Core.Types.Packaging;

using System.ComponentModel;
using System.Text.Json.Serialization;
using DummyDroidStubGen.Core.Helpers.Security;
using static DummyDroidStubGen.Core.Helpers.IO.InputHelper;
using static DummyDroidStubGen.Core.Helpers.NetworkHelper;
using static DummyDroidStubGen.Core.Helpers.PackageHelpers;
using static DummyDroidStubGen.Global.Messaging;

public class Package(string name, PackageCategory category, string? label = null, string? baseCodePath = null)
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = name;

    
    [JsonPropertyName("category")]

    // Handles the conversion of a category string to its enum member.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    
    // Setting the default value if the conversion fails.
    [DefaultValue(PackageCategory.Other)]
    public PackageCategory Category { get; init; } = category;

    /// <summary> The Base Code Path to the APK associated with the current package. </summary>
    public string? BaseCodePath { get; init; }= baseCodePath;



    [JsonPropertyName("label")]
    public string Label { get; init; } = GetFriendlyName(label, name, category);


    /// <summary>
    ///     Returns a List of blacklisted packages that will be used in GetWhitelistedPackages().
    /// </summary>
    private static List<Package> GetBlacklistedPackages() 
    {
        return ParseEmbeddedResourceToPackageList(BlacklistHelper.ResourcePath);
    }

    public static List<Package> GetWhitelistedPackages(Dictionary<PackageCategory, List<string>> packageCategoryGroups) 
    {
        if (packageCategoryGroups == null || packageCategoryGroups.Count == 0) 
        {
            WriteWarningMessage("No eligible packages were located while trying to generate a stub.");
            WriteErrorMessage("packageCategoryGroups is null or empty in Package.GetWhitelistedPackages()");
            return [];
        }

        var blacklistedPackages = GetBlacklistedPackages();

        // Excluding Blacklisted and system packages.
        // Mapping package names to Package objects.
        return [.. packageCategoryGroups
            .Where(pair => pair.Key != PackageCategory.System)
            .Where(pair => pair.Value.Any(name => !blacklistedPackages.Any(a => a.Name.Contains(name))))
            .SelectMany(pair => pair.Value.Select(
                packageName =>  {
                    return new Package(name: packageName, category: pair.Key);
                }
            ))
        ];
    }
    
    /// <summary>
    ///     Makes a query to play.google.com using the current package's name. <br/>
    ///     This function returns true if the query has a 200 status code, otherwise, it returns false.
    /// </summary>
    public async Task<bool> IsAlreadyPublished() 
    {
        HttpRequestMessage request = new(
            method: HttpMethod.Get, 
            requestUri: $"https://play.google.com/store/apps/details?id={Name}"
        );

        request.Headers.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:151.0) Gecko/20100101 Firefox/151.0");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        request.Headers.Add("Sec-GPC", "1");
        request.Headers.Add("Alt-Used", "play.google.com");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("Sec-Fetch-Dest", "document");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "none");
        request.Headers.Add("Sec-Fetch-User", "?1");
        request.Headers.Add("Priority", "u=0, i");
        request.Headers.Add("TE", "trailers");

        HttpResponseMessage? response = null;

        try {
            response = await ClientInstance.SendAsync(request);
        }
        catch (Exception ex) {
            WriteWarningMessage($"Unable to query 'play.google.com' for the package '{Name}'"); 
            WriteErrorMessage(ex.Message);
            WriteErrorMessage("Continuing may have unintended consequences on the wellbeing of your device!");
            WriteWarningMessage("If you use the name of a package that is already published on the Play Store:\n");

            WriteInformation(
                coloredText: "\t- You will not be able to download OR open the REAL application associated with that name."
            );
            
            WriteInformation(
                coloredText: "\t- You may cause a UI loop that will render your device stuck, requiring a reboot to fix."
            );

            WriteErrorMessage(
                message: "\t- You may cause general system instability, requiring a full system reinstallation."
            );

            var selection = AskForSelection(
                message: "Would you like to continue, given the circumstances?", 
                options: ["Yes, I understand the risks.", "No, I don't wish to continue"]
            );

            if (selection == "No, I don't wish to continue") {
                selection = ExitChoice;
            }

            UserExitStatusCheck(selection);

            WriteSuccessMessage("Continuing...");
        }

        try 
        {
            if (response == null) {
                WriteErrorMessage("Variable 'response' has a null value in Package.IsAlreadyPublished()");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }

        catch {
            return false;
        }
    }
}