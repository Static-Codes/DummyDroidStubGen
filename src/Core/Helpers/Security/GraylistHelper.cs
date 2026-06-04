namespace DummyDroidStubGen.Core.Helpers.Security;

using DummyDroidStubGen.Core.Types.Packaging;
using static Global.Constants;

public static class GraylistHelper 
{   
    // Unlike the Blacklist, the Graylist will serve the end-user a warning but not block stub generation.
    //
    // A majority of the applications on the Graylist are just permission hungry, and not explicitly cause for concern.  
    public static readonly string ResourcePath = $"{ApplicationName}.Resources.Security.graylist.json";
    public static bool IsGraylisted(this List<Package> graylistedPackages, string packageName) 
    {
        // Checking if a wildcard indicator is present.
        // If so, then pattern matching is be used, otherwise a direct match is performed.
        return packageName.Contains(".*") switch {
            true => graylistedPackages.Any(p => p.Name.StartsWith(packageName[..^2], StringComparison.OrdinalIgnoreCase)),
            false => graylistedPackages.Any(p => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase))
        };
    }
    

    

    
}