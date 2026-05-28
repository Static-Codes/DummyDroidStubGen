namespace DummyDroidStubGen.Core.Common;

using System.Text.RegularExpressions;

public static partial class RegexPatterns 
{
    [GeneratedRegex(@"Successfully\spaired\sto\s(?<ip>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}):(?<port>[0-9]{1,5})\s+\[guid=(?<device_id>[^\]]+)")]
    public static partial Regex PairingRegex();

    [GeneratedRegex(@"([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})")] 
    public static partial Regex ValidateAddressIPV4();

    [GeneratedRegex(@"(?<USBDeviceID>[0-9A-Z]{14})\s{1,}device\s{1,}usb:[0-9]{1,}-[0-9]{1,}\s{1,}product:\w{1,}\s{1,}model:(?<USBDeviceName>\w{1,})\s{1,}device:(?<USBDeviceCodename>\w{1,})\s{1,}transport_id:(?<USBTransportID>\w{1,})|(?<IP>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}):(?<Port>[0-9]{1,5})\s{1,5}device\s{1,}product:\w{1,}\s{1,}model:(?<WIFIDeviceName>\w{1,})\s{1,}device:(?<WIFIDeviceCodename>\w{1,})\s{1,}transport_id:(?<WIFITransportID>\w{1,})")]
    public static partial Regex ConnectionRegex();
}