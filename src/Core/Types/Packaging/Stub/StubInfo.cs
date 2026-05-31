using DummyDroidStubGen.Core.Types.Versioning;

namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

public class StubInfo (Package packageInfo, byte[] iconBuffer)
{
    public Package PackageInfo = packageInfo;
    public byte[] IconBuffer = iconBuffer;
    public const int TARGET_SDK_VERSION = 34;
    public const AndroidOSVersion MINIMUM_OS_VERSION = (AndroidOSVersion)TARGET_SDK_VERSION;

}