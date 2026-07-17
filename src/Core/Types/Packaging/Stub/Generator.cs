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
namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

using Stub.Contents;
using static Global.Messaging;
using static Stub.Contents.ShellCode;
using static Stub.Contents.JavaCode;
using static Stub.Contents.ProjectFileCode;

/// <summary> Contains the publicly exposed method to generate the current stub </summary>
public class Generator 
{
    public static void GenerateStub(StubInfo stubInfo) 
    {
        #if DEBUG
            WriteDebugMessage($"IconBuffer Length in bytes: {stubInfo.StubStructure.Icon.IconBuffer.Length}");
        #endif

        var stubLabelName = $"{stubInfo.TargetPackage.Label}Launcher";

        // Populating then writing the two Shell script files to disk.
        PopulateShellFiles(stubInfo);
        WriteShellFiles(stubInfo.StubStructure.Directories.ProjectParent);

        // Writing the App Icon and AndroidManifest.xml to the project's parent directory.
        var manifest = new AndroidManifest(stubInfo);
        AppIcon.Write(stubInfo);
        manifest.Write();

        // Writing the .classpath file to the project's parent directory.
        CreateClassPathFile(stubInfo.StubStructure.Directories.ProjectParent);

        // Writing the .project file to the project's parent directory.
        CreateProjectFile(
            parentDirectory: stubInfo.StubStructure.Directories.ProjectParent, 
            projectName: stubLabelName
        );

        // Populating then writing the three Java source files to disk.
        PopulateSourceFiles(stubInfo.StubPackage);
        WriteSourceFiles(stubInfo.StubStructure.Directories.JavaCode);

        WriteSuccessMessage($"Compiled {stubInfo.TargetPackage.Label}Launcher.");
        WriteInformation($"The compiled stub will launch the package: {stubInfo.TargetPackage.Name}");
    }
}


