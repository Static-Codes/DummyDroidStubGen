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