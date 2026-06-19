// ADD METHODS TO WRITE STUB using STUBINFO, 

namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

using DummyDroidStubGen.Core.Types.Packaging.Stub.Contents;

public class Generator 
{
    public static void GenerateStub(StubInfo stubInfo) 
    {
        Console.WriteLine(stubInfo.StubStructure.Icon.IconBuffer.Length);

        AndroidManifest manifest = new(stubInfo.StubStructure, stubInfo.PackageInfo);
        AppIcon.Write(stubInfo);
        manifest.Write();

        Environment.Exit(1);
    }
}
