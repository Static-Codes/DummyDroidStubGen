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
namespace DummyDroidStubGen.Global;

using System.Reflection;

internal class Constants 
{
    public const string AAPT2Path = "/usr/bin/aapt2";
    public const string ADBPath = "/usr/bin/adb";
    public const string APKSignerPath = "/usr/bin/apksigner";
    public const string FindPath = "/usr/bin/find";
    public const string JavaPath = "/usr/bin/java";
    public const string JavaCompilerPath = "/usr/bin/javac";
    public const string UnzipPath = "/usr/bin/unzip";
    public const string ZipPath = "/usr/bin/zip";
    
    public const string JavaLibsResourcePath = "Resources.libs.zip";
    public const string ODLFResourcePath = "Resources.odlf.zip";
    
    public static Assembly _assembly = Assembly.GetExecutingAssembly();
    public static string ApplicationName = _assembly.FullName?.Split(',')[0] ?? "DummyDroidStubGen";

    public const BindingFlags _privateFlag = BindingFlags.NonPublic;
    public const BindingFlags _privateStaticFlag = BindingFlags.NonPublic | BindingFlags.Static;
    public const BindingFlags _publicFlag = BindingFlags.Public;
    public const BindingFlags _publicInstanceFlag = _publicFlag | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
    
    public const string ConnectionSection = "Please reference the 'Connecting Your Device' section in the project's repository.";
    public const string USBSetting = "System -> Developer options -> USB Debugging.";
    public const string WIFISetting = "System -> Developer options -> Wireless debugging -> Pair device with pairing code.";
    public const string DebugTag = "[[DEBUG]]";
    public const string ErrorTag = "[[ERROR]]:";
    public const string InfoTag = "[[INFO]]:";
    public const string InputTag = "[[INPUT]]:";
    public const string WarningTag = "[[WARNING]]:";
    public const string SuccessTag = "[[SUCCESS]]:";
    public const string OrangeHex = "#FFAF00";
    public const string ProjectIssueLink = "https://github.com/Static-Codes/DummyDroidStubGen/issues";


}