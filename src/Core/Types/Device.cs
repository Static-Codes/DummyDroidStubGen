namespace DummyDroidStubGen.Core.Types;

using ADB.Wireless;
using System.Text.Json.Serialization;
using Versioning;

using static ADB.Connection;
using static Helpers.InputHelper;

public class Device
{
    /// <summary> 
    ///     The name of the connected device, as is reported by the Android Debug Bridge.
    /// </summary>
    public string Name { get; set; }

    /// <summary> 
    ///     The version of the Android OS running on the specified device.
    /// </summary>
    public AndroidOSVersion AndroidOSVersion { get; set; } = AndroidOSVersion.UNKNOWN;

    /// <summary> 
    ///     The version of the Android API that was bundled with the version of Android running on the specified device.
    /// </summary>
    public int AndroidAPILevel => (int)AndroidOSVersion;

    public ConnectionStatus ConnectionStatus { get; set; }

    /// <summary> 
    ///     Holds the Device IP, Pairing Port, and Pairing Code. (If WIFI pairing is used.) 
    /// </summary>
    public PairingInfo? WirelessPairingInfo { get; set; }

    /// <summary> 
    ///     The identifier associated with the Android device. <br/> 
    ///     When using USB Pairing, this identifier is a 14 digit alphanumeric string. <br/> 
    ///     When using WIFI Pairing, this identifier is an IP:Port combination. <br/> 
    /// </summary>
    public string? ID { get; set; }

    public string Codename { get; set; }

    // This is the normal constructor which will be used for object creation.
    public Device(string? name = null, AndroidOSVersion? androidOSVersion = null, ConnectionStatus? connectionStatus = null, ConnectionMethod? connectionMethod = null, PairingInfo? wirelessPairingInfo = null, string? id = null, string? codename = null)
    {
        Name = name ?? "Unknown";
        AndroidOSVersion = androidOSVersion ?? AndroidOSVersion.UNKNOWN;
        
        ConnectionStatus = connectionStatus ?? new(
            Connected: false, 
            Method: connectionMethod ?? AskForConnectionMethod(), 
            Output: null, 
            Result: null
        );
        
        WirelessPairingInfo = wirelessPairingInfo;
        ID = id ?? "Unknown";
        Codename = codename ?? "Unknown";
    }

    // This constructor is used when instantiating a new object of the type Device.
    [JsonConstructor]
    public Device(string? Name, ConnectionStatus? ConnectionStatus = null, AndroidOSVersion? androidOSVersion = null, ConnectionMethod? ConnectionMethod = null, PairingInfo? WirelessPairingInfo = null, string? ID = null, string? Codename = null)
    {
        this.Name = Name ?? "Unknown";
        AndroidOSVersion = androidOSVersion ?? AndroidOSVersion.UNKNOWN;
        
        this.ConnectionStatus = ConnectionStatus ?? new(
            Connected: false, 
            Method: ConnectionMethod ?? AskForConnectionMethod(), 
            Output: null, 
            Result: null
        );
        
        this.WirelessPairingInfo = WirelessPairingInfo;
  
        this.ID = ID ?? "Unknown";
        this.Codename = Codename ?? "Unknown";
    }
}

/// <summary> 
///     AuthorizedDevice serves as a semantic differentiator from the standard Device object. <br/>
///     This struct contains no additional fields, taking only a device object as a parameter. <br/>
///     When a device is "Authorized" in the codebase, the only action that is made is an object cast. <br/>
///     Whitelist.AuthorizeDevice(ref device) returns this casted object. To explicitly show the device is "Authorized". <br/>
/// </summary>
[Obsolete("Until whitelist is reintroduced, this is obsolete.")]
[method: JsonConstructor]
public class AuthorizedDevice(Device device)
{
    [JsonPropertyName("Device")]
    public Device Device { get; set; } = device;
}