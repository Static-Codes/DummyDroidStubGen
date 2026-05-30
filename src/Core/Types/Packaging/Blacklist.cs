namespace DummyDroidStubGen.Core.Types.Packaging;

public class Blacklist 
{   
    // This blacklist is in no means "impenetrable". 
    // It serves as a rudamentary attempt to prevent the user from generating a stub that links to a sensitive application.
    //
    // These applications include categories such as:
    //
    // - Core System Utilities (Which can cause general instability at best and compromises at worst.)
    // - Common Commercial Targets (Such as banking apps, crypto wallets, etc)
    // - Private messaging apps, such as Signal, Simplex, Telegram 
    private static readonly List<Package> BlacklistedPackages = 
    [
        
    ];

    public static bool IsBlacklisted(string packageName) 
    {
        var isPattern = packageName.Contains('*');
        // Add a switch statement here to catch both true and false for isPattern
        // true => Any(p => p.Name.Contains(packageName[^1], StringComparison.OrdinalIgnoreCase))
        // false => Any(p => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase));
        
        return BlacklistedPackages.Any(p => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase));
    }
}