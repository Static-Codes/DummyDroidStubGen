// ADD METHODS TO WRITE STUB using STUBINFO, 

namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

using System.Reflection;
using DummyDroidStubGen.Core.Types.Packaging.Stub.Contents;
using DummyDroidStubGen.Core.Types.Packaging;
using DummyDroidStubGen.Core.Types.Packaging.Stub;
        
public class StubCreator 
{
    public static void GenerateStub() 
    {
        // It should then write the icon to the path <mainSourceDir>/res/drawable/icon.{filetype}
        var sfs = StubFileStructure.New("/home/nerdy/.config/DummyDroidStubGen/Resources/", "my.test.package", "Resources/Android/DrawableVectors/cyclone.xml");
        

        Console.WriteLine(sfs.ProjectDirectory);
        var package = new Package("my.test.package", PackageCategory.Commercial, "MyTestPackage");

        AndroidManifest manifest = new(sfs, package);
        manifest.Write();

        Environment.Exit(1);
    }
}
