namespace DummyDroidStubGen.Core.Types.Packaging;

public class Graylist 
{   
    // Unlike the Blacklist, the Graylist will serve the end-user a warning but not block stub generation.
    //
    // A majority of the applications on the Graylist are just permission hungry, and not explicitly cause for concern.
    
    private static readonly List<Package> BlacklistedPackages = 
    [
        
    ];
    
    public static bool IsBlacklisted(string packageName) 
    {
        return BlacklistedPackages.Any(p => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase));
    }
}