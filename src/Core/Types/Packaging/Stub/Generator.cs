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
using static Stub.Contents.JavaCode;
using static Stub.Contents.ProjectFileCode;

public class Generator 
{
    public static void GenerateStub(StubInfo stubInfo) 
    {
        #if DEBUG
            WriteDebugMessage($"IconBuffer Length in bytes: {stubInfo.StubStructure.Icon.IconBuffer.Length}");
        #endif

        // Writing the App Icon and AndroidManifest.xml to the project's parent directory.
        AndroidManifest manifest = new(stubInfo.StubStructure, stubInfo.PackageInfo);
        AppIcon.Write(stubInfo);
        manifest.Write();

        // Populating then writing the three Java source files to disk.
        PopulateSourceFiles(stubInfo.PackageInfo);
        WriteSourceFiles(stubInfo.StubStructure.Directories.JavaCode);
        
        // Writing the .classpath file to the project's parent directory.
        CreateClassPathFile(stubInfo.StubStructure.Directories.ProjectParent);

        // Writing the .project file to the project's parent directory.
        CreateProjectFile(
            parentDirectory: stubInfo.StubStructure.Directories.ProjectParent, 
            projectName: stubInfo.PackageInfo.Label
        );

        Environment.Exit(1);
    }
}


