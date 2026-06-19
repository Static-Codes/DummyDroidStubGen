namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

using Stub.Contents;
using static Global.Messaging;

public class Generator 
{
    public static void GenerateStub(StubInfo stubInfo) 
    {
        #if DEBUG
            WriteDebugMessage($"IconBuffer Length in bytes: {stubInfo.StubStructure.Icon.IconBuffer.Length}");
        #endif

        AndroidManifest manifest = new(stubInfo.StubStructure, stubInfo.PackageInfo);
        AppIcon.Write(stubInfo);
        manifest.Write();

        Environment.Exit(1);
    }
}
