namespace DummyDroidStubGen.Core.Types.ADB;

using static Global.Messaging;
using static Helpers.InputHelper;
using static Helpers.FileHelper;
using System.Text.Json;

[Obsolete("Currently Whitelist is non-functional and should not be used in public releases.")]
public class Whitelist
{
    private readonly JsonSerializerOptions serializerOptions = new() { 
        WriteIndented = true, 
        IncludeFields = true 
    };

    private readonly string WhitelistFilePath = GetWhitelistFilePath();
    private readonly string WhitelistBackupFilePath = GetWhitelistBackupFilePath();
    private List<AuthorizedDevice> AuthorizedDevices { get; set; } = [];
    
    public Whitelist() {
        AuthorizedDevices = LoadWhitelist();
    }

    public Whitelist(List<AuthorizedDevice> Devices) {
        AuthorizedDevices = Devices.Count > 0 ? Devices : LoadWhitelist();
    }


    /// <summary> 
    ///     Attempts to add the passed device to AuthorizedDevices. <br/> 
    ///     Returns a boolean representing the status of this request.
    /// </summary>
    public bool AddDevice(object device) 
    {
        if (device is Device or AuthorizedDevice) {
            return ProcessDevice(ref device, NeedsAuthorization: device is Device);
        }

        WriteWarningMessage("Invalid object type passed to Whitelist.AddDevice.");
        WriteInformation("The expected object must of the type Device or AuthorizedDevice");
        return false;
    }


    /// <summary> 
    ///     Performing a semantic cast from the type Device to the type AuthorizedDevice. 
    /// </summary>
    private static AuthorizedDevice AuthorizeDevice(object device) => new((Device)device);


    /// <summary> 
    ///     Creates an empty file at $HOME/.config/DummyDroidStubGen/whitelist.json <br/>
    ///     Returns null if success or an exception if any occur in the process of this file's creation.
    /// </summary>
    private Exception? CreateEmptyWhitelist() {
        // If windows support is ever added, this needs to be dynamically initialized in a variable.
        // Currently, Carriage Returns are not required as this is a Unix-Only application.
        try {
            File.WriteAllText(WhitelistFilePath, "[\n]\n");
            return null;
        }
        catch (Exception ex) {
            return ex;
        }
    }


    /// <summary> 
    ///     Performs a cast on AuthorizedDevices, going from List of objects to an IEnumerable of AuthorizedDevice.
    /// </summary>
    public IEnumerable<AuthorizedDevice> GetAuthorizedDevices() => AuthorizedDevices;


    /// <summary> 
    ///     Returns a boolean representing the whitelist status of the current device. 
    /// </summary>
    public bool IsWhitelistedDevice(object device) 
    {
        if (device is AuthorizedDevice authorized)
        {
            return AuthorizedDevices.Any(d => d.Device != null && d.Device.ID == authorized.Device.ID);
        }
        return false;
    }


    /// <summary>
    ///     Loads the Whitelist file if it exists, and if not, an empty whitelsit file is created. <br/>
    ///     There is an optional paramater "list", if: <br/>
    ///     "list" is passed a non-null value, that value is used to update then return the whitelist. <br/>
    ///     "list is passed a null value, the contents of the whitelist file is returned.
    /// </summary>
    private List<AuthorizedDevice> LoadWhitelist(List<AuthorizedDevice>? list = null) 
    {
        list ??= [];
        
        if (File.Exists(WhitelistFilePath)) 
        {
            try {
                using FileStream stream = File.OpenRead(WhitelistFilePath);
                var items = JsonSerializer.Deserialize<List<AuthorizedDevice>>(stream) ?? []; 
                
                // This check prevents duplicate entries from being populated on startup.
                foreach (var deviceObj in items) 
                {
                    if (!list.Any(d => d.Device.ID == deviceObj.Device.ID)) {
                        list.Add(deviceObj);
                    }
                }
            }
            catch (Exception ex) {
                WriteWarningMessage("An exception has occurred while attempting to load the device whitelist.");
                var exc = ex;
                throw exc;
            }
        }
        else {
            CreateEmptyWhitelist();
        }

        return list;
    }


    /// <summary> 
    ///     Authorizes a device passed via reference parameter (if it's not already authorized). <br/>
    ///     The authorized device is then added it to the whitelist. <br/>
    ///     This is a helper method for Whitelist.AddDevice()
    /// </summary>
    private bool ProcessDevice(ref object device, bool NeedsAuthorization) 
    {
        if (NeedsAuthorization) 
        {
            WriteInformation("You are trying to add an unauthorized device to the application whitelist.");
            
            var selection = AskForSelection(
                message: "Do you wish to continue?",
                options: ["I wish to authorize this device.", "No, I made a mistake."]
            );

            if (selection == "No, I made a mistake.") {
                return false;
            }
        
            device = AuthorizeDevice(device);
        }

        if (device is AuthorizedDevice deviceMatch) 
        {
            // Preventing duplicate entries to AuthorizedDevices upon application startup.
            if (deviceMatch.Device != null && !AuthorizedDevices.Any(
                d => d.Device != null && 
                d.Device.ID == deviceMatch.Device.ID
            ))
            {
                AuthorizedDevices.Add(deviceMatch);
                UpdateWhitelistFile(); 
                return true;
            }
        }

        WriteWarningMessage("Device is already present in the internal whitelist.");
        return false;
    }

    
    /// <summary>
    ///     Copies the contents of WhitelistFilePath to WhitelistBackupFilePath <br/>
    ///     The internal state of AuthorizedDevices is used to update the contents of WhitelistFilePath. <br/>
    ///     Despite being exposed publicly, this should not be called directly. <br/>
    ///     This will be called in the extension method WhitelistExtension.HandleWhitelistSelection(device)
    /// </summary>
    public void UpdateWhitelistFile() 
    {
        byte[] authorizedDeviceBytes;
        
        try {
            // Maps the struct so the JSON serializer sees the internal objects regardless of the current struct state.
            var dataToSerialize = AuthorizedDevices.Select(ad => 
                new {ad.Device}
            ).ToList();

            authorizedDeviceBytes = JsonSerializer.SerializeToUtf8Bytes(dataToSerialize, serializerOptions);
            File.WriteAllBytes("test.json", authorizedDeviceBytes);
        }
        catch (Exception ex) {
            WriteWarningMessage("Unable to update whitelist file.");
            WriteErrorMessage(ex.Message);
            authorizedDeviceBytes = [];
        }

        if (authorizedDeviceBytes.Length == 0) {
            WriteWarningMessage("Unable to update whitelist file.");
            WriteErrorMessage("authorizedDeviceBytes returned a length of 0 bytes.", exit: true, exitCode: 1);
        }

        // Write directly to Backup using FileMode.Create to ensure old bytes are wiped clean
        try {
            using var backupStream = new FileStream(WhitelistBackupFilePath, FileMode.Create, FileAccess.Write);
            backupStream.Write(authorizedDeviceBytes, 0, authorizedDeviceBytes.Length);
        }
        catch (Exception ex) {
            WriteWarningMessage("Unable to update whitelist backup file.");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }

        // Write directly to Whitelist file using FileMode.Create to avoid trailing garbage characters
        try {
            using var finalStream = new FileStream(WhitelistFilePath, FileMode.Create, FileAccess.Write);
            finalStream.Write(authorizedDeviceBytes, 0, authorizedDeviceBytes.Length);
            WriteSuccessMessage("Updated internal device whitelist to include the specified device!");
        }
        catch (Exception ex) {
            WriteWarningMessage("Unable to update whitelist file.");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }
    }
}

[Obsolete("Currently not functional.")]
public static class WhitelistExtension 
{

    public static void HandleWhitelistSelection(this Whitelist whitelist, Device device) 
    {
        var deviceNameForMessage = device.Name != "Unknown" ? device.Name : "device";
        var authorizationStatus = AskForSelection(
            message: $"Would you like to whitelist this {deviceNameForMessage}?", 
            options: ["Yes", "No"]
        );

        UserExitStatusCheck(authorizationStatus);

        if (authorizationStatus == "Yes") {
            whitelist.UpdateWhitelistFile();
        }
    }
}