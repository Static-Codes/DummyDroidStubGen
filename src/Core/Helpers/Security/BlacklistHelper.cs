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
namespace DummyDroidStubGen.Core.Helpers.Security;

using DummyDroidStubGen.Core.Types.Packaging;
using static Global.Constants;

public static class BlacklistHelper 
{   
    // This blacklist is in no means "impenetrable". 
    // It serves as a rudamentary attempt to prevent the user from generating a stub that links to a sensitive application.
    //
    // These applications include categories such as:
    //
    // - Core System Utilities (Which can cause general instability at best and compromises at worst.)
    // - Common Commercial Targets (Such as banking apps, crypto wallets, etc)
    // - Private messaging apps, such as Signal, Simplex, Telegram 
    public static readonly string ResourcePath = $"{ApplicationName}.Resources.Security.blacklist.json";
    public static bool IsBlacklisted(this List<Package> blacklistedPackages, string packageName) 
    {
        // Checking if a wildcard indicator is present.
        // If so, then pattern matching is be used, otherwise a direct match is performed.
        return packageName.Contains(".*") switch {
            true => blacklistedPackages.Any(p => p.Name.StartsWith(packageName[..^2], StringComparison.OrdinalIgnoreCase)),
            false => blacklistedPackages.Any(p => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase))
        };
    }
    

    

    
}