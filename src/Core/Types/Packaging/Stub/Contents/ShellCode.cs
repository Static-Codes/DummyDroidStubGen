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
namespace DummyDroidStubGen.Core.Types.Packaging.Stub.Contents;

using static Extensions.ShellTypeExtension;
using static Helpers.ShellTypeHelper;


/// <summary> Contains the functions and members used to generate the stub's build and installation scripts. </summary>
public class ShellCode 
{
    /// <summary> The shell files to be used for the compilation and installation of the generated stub. </summary>
    private static readonly List<ShellFile> ShellFiles =
    [
        new ShellFile(FileName: "build.sh", Contents: []),
        new ShellFile(FileName: "run.sh", Contents: []),
    ];

    /// <summary> The current linux shell in use. </summary>
    public static readonly ShellType CurrentShellType = FindCurrent();

    /// <summary> The current linux shell in use. </summary>
    public static readonly string ShebangOperator = CurrentShellType.ToShebangOperator();
}