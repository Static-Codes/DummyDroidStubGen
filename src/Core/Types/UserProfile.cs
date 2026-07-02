
namespace DummyDroidStubGen.Core.Types;

using System.Text.RegularExpressions;
using static UserProfile.UserStatus;
using static UserProfile.UserManagementState;

/// <summary> Represents an Android UserProfile. </summary>
public class UserProfile 
{   
    /// <summary> An enum representing the status of the current UserProfile. </summary>
    public enum UserStatus : uint
    {
        /// <summary> Indicates the current profile is running. </summary>
        RUNNING = 0,
        /// <summary> Indicates the current profile is not running or the state is unknown. </summary>
        UNKNOWN = 1
    }
    
    /// <summary> An enum representing the the management state of the current profile. </summary>
    public enum UserManagementState : uint 
    {
        /// <summary> Indicates the current profile is not managed. </summary>
        UNMANAGED = 0, 
        /// <summary> Indicates the current profile is managed but not explicitly sandboxed. </summary>
        MANAGED = 1,
        /// <summary> Indicates the current profile is managed and sandboxed. </summary>
        SANDBOXED = 2, 
    }
    
    /// <summary> The User Identifier associated with the current UserProfile. </summary>
    private uint? UID { get; set; }

    /// <summary> The label representing the name for the current UserProfile. </summary>
    private string? Name { get; set; }
    
    /// <summary> Flags for the current UserProfile (if any). </summary>
    private uint? Flags { get; set; }

    /// <summary> If the current UserProfile is active. </summary>
    private UserStatus ProfileStatus { get; set; }

    /// <summary> Contains 
    private UserManagementState ManagementState { get; set; }
    
    public bool Running() => ProfileStatus is RUNNING;

    /// <summary> If the current UserProfile is managed (or sandboxed) </summary>
    public bool Managed() => ManagementState is MANAGED || ManagementState is SANDBOXED;

    /// <summary> 
    ///     If the current profile was created using legacy tools like Island or Insular. <br/>
    ///     Insular: https://github.com/proletarius101/Insular <br/>
    ///     Island:  https://github.com/oasisfeng/island <br/>
    /// 
    ///     It is highly recommended to use Shelter over Insular/Island as it is actively maintained.
    /// </summary>
    public bool WasConfiguredByIsland => Name?.ToLower() is "island";
    
    /// <summary> 
    ///     If the current profile was created using Shelter. <br/>
    ///     Repository: https://github.com/PeterCxy/Shelter <br/>
    /// 
    ///     It is highly recommended to use Shelter over Insular/Island as it is actively maintained.
    /// </summary>
    public bool WasConfiguredByShelter => Name?.ToLower() is "shelter";
    
    public bool IsMainProfile => Name?.ToLower() is "owner";

    public bool IsGenericWorkProfile => Name?.ToLower() is "work profile";


    /// <summary> Represents an Android UserProfile. </summary>
    /// <param name="UID"> The User Identifer for the profile. </param>
    /// <param name="Name"> The name (label text) for the profile. </param>
    /// <param name="Name"> The flags specified for the profile. </param>
    /// <param name="Status"> The status string for the profile. </param>
    public UserProfile(uint? UID, string? Name, uint? Flags, string Status) {
        SetUID(UID);
        SetName(Name);
        SetFlags(Flags);
        SetStatus(Status);
        SetManagementState();
    }

    public uint? GetFlags() => Flags;
    public string? GetName() => Name;
    public uint? GetUID() => UID;

    public void SetFlags(uint? flags) => Flags ??= flags;
    
    private void SetManagementState()
    {
        ManagementState = true switch {
            _ when WasConfiguredByIsland   => SANDBOXED,
            _ when WasConfiguredByShelter  => SANDBOXED,
            _ when UID >= 10               => MANAGED,
            _ when IsGenericWorkProfile    => MANAGED,
            _ when IsMainProfile           => UNMANAGED,
            _                              => UNMANAGED,
        };
    }

    public void SetName(string? name) => Name ??= name;
    public void SetStatus(string status) => ProfileStatus = status is "running" ? RUNNING : UNKNOWN;
    public void SetUID(uint? uid) => UID ??= uid;

    public static UserProfile New(Match match) 
    {
        // Using base-16 to convert the strings to their raw uint32 representations
        return new UserProfile(
            UID: Convert.ToUInt32(match.Groups["uid"].Value, 16),
            Name: match.Groups["name"].Value,
            Flags: Convert.ToUInt32(match.Groups["flags"].Value, 16),
            Status: match.Groups["status"].Value
        );
    }




    
}