namespace DummyDroidStubGen.Core.Extensions;

using DummyDroidStubGen.Core.Types.CPU;
using DummyDroidStubGen.Core.Types.Packaging;
using static DummyDroidStubGen.Core.Types.CPU.CPUArchitecture;

public static class EnumExtension 
{
    /// <summary> 
    ///     Maps the ABI string returned by the Android Environment to an internal CPUArchitecture enum value.
    /// </summary>
    public static CPUArchitecture FromABIString(this string abiString) 
    {
        return abiString switch
        {
            "arm64-v8a"   => Arm64V8a,
            "armeabi-v7a" => ArmEabiV7a,
            "x86_64"      => X86_64,
            "x86"         => X86,
            _             => UNKNOWN
        };
    }
}