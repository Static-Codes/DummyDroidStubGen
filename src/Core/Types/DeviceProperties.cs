namespace DummyDroidStubGen.Core.Types;

using System.Threading.Tasks;
using static Core.Helpers.PSIHelper;
using static Global.Messaging;
using static Functions;

public class DeviceProperties 
{
    // The key represents the property name, and the value represents the property name.
    private Dictionary<string, string> _properties = [];

    public async Task LoadAsync(bool isUSB) 
    {
        var psi = GetDevicePropertiesPSI(isUSB);

        var processResult = await RunProcessAsync(psi);

        if (processResult.exception != null) {
            throw processResult.exception;
        }

        # if DEBUG
            foreach (var line in processResult.output) { WriteDebugMessage(line); }
            foreach (var line in processResult.error) { WriteDebugMessage(line); }
        #endif

        _properties = ParseDeviceProperties(processResult.output);
    }

    public Dictionary<string, string> GetAllProperties() => _properties;

    public string? GetAndroidOSVersion() {
        
        return TryGetValueOfProperty("ro.build.version.release");
    }

    public string? GetAndroidSDKVersion() {
        
        return TryGetValueOfProperty("ro.build.version.sdk");
    }

    public string? TryGetValueOfProperty(string? propertyName) 
    {
        if (propertyName == null) {
            return null;
        }

        return _properties.Where(p => p.Key == propertyName)?
                          .Select(prop => prop.Value)
                          .FirstOrDefault();
    }

}