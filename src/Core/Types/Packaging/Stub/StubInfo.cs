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

public class StubInfo(Package targetPackage, Package stubPackage, FileStructure fileStructure, string profileID = "0")
{
    /// <summary> The Package the Stub will target/open. </summary>
    public Package TargetPackage = targetPackage;
    
    /// <summary> The Package object representing the Stub itself. </summary> 
    public Package StubPackage = stubPackage;

    /// <summary> The file structure for the compiled stub. </summary>
    public FileStructure StubStructure = fileStructure;

    /// <summary> 
    ///     The profile ID where the stub will be involved. <br/>
    ///     This will always almost be set to "0" indicating a personal profile. <br/>
    ///     This can be set to "10" to indicate a work/sandboxed profile. <br/>
    /// </summary>
    public string ProfileID = profileID;
}