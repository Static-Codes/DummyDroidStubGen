namespace DummyDroidStubGen.Core.Types;

public enum CPUArchitecture 
{
    Unknown = 0,
    Arm64V8a = 1,   // DummyDroidStubGen.Resources.arm64-v8a.aapt2
    ArmEabiV7a = 2, // DummyDroidStubGen.Resources.armeabi-v7a.aapt2
    X86 = 3,        // DummyDroidStubGen.Resources.x86.aapt2
    X86_64 = 4      // DummyDroidStubGen.Resources.x86-64.aapt2
}