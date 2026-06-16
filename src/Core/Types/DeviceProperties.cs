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
namespace DummyDroidStubGen.Core.Types;


using Core.Types.CPU;
using Core.Types.Versioning;
using System.Threading.Tasks;
using static Core.Helpers.PSIHelper;
using static Core.Types.ADB.Connection.ConnectionMethod;
using static Functions;
using DummyDroidStubGen.Core.Extensions;

public class DeviceProperties 
{
    // The key of this dictionary holds the property's name and the dictionary value, holds the property's value.
    private Dictionary<string, string> _properties = [];

    public async Task LoadAsync(Device device) 
    {
        var psi = DevicePropertiesPSI(device.ConnectionStatus.Method == USB);

        var processResult = await RunProcessAsync(psi);

        if (processResult.Exception != null) {
            throw processResult.Exception;
        }

        // # if DEBUG
        //     foreach (var line in processResult.Output) { WriteDebugMessage(line); }
        //     foreach (var line in processResult.Error) { WriteDebugMessage(line); }
        // #endif

        _properties = ParseDeviceProperties(processResult.Output);
    }

    public Dictionary<string, string> GetAllProperties() => _properties;

    public CPUArchitecture GetCPUArchitecture() 
    {
        var rawArchitecture = TryGetValueOfProperty("ro.product.cpu.abi") ?? "UNKNOWN";
        
        // Converting the rawArchitecture string to a CPUArchitecture object.
        var parsedArchitecture = rawArchitecture.FromABIString();

        return parsedArchitecture;
    }

    public CPUPageSize GetCPUPageSize() 
    {
        var fallbackValue = CPUPageSize.UNKNOWN;

        // int.TryParse will return false for "-1", which is the intended fallback.
        var rawPageSize = TryGetValueOfProperty("ro.product.cpu.pagesize.max") ?? fallbackValue.ToString();
        
        // Ensuring the rawPageSize is a valid 32 bit signed integer thats greater than 0.
        if (int.TryParse(rawPageSize, out int pageSize) && pageSize > (int)fallbackValue) {
            return (CPUPageSize)pageSize;
        }
        
        return fallbackValue;
    }

    public AndroidOSVersion GetOSVersion()
    {
        var rawBuildVersion = TryGetValueOfProperty("ro.build.version.release");

        if (Enum.TryParse<AndroidOSVersion>(rawBuildVersion, ignoreCase: true, out var result)) {
            return result;
        }

        return AndroidOSVersion.UNKNOWN;
    }

    public string? GetSDKVersion() => TryGetValueOfProperty("ro.build.version.sdk");

    public string? GetSerialNumber() => TryGetValueOfProperty("ro.serialno");
    
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