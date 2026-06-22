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

using System.Xml.Linq;
using Types.Packaging;
using Types.Packaging.Stub;
using Types.Versioning;
using static Global.Messaging;

public class AndroidManifest(FileStructure fileStructure, Package package) 
{   
    /// <summary> The stub's file structure. This will be accessed by GenerateXDoc() internally. </summary>
    private readonly FileStructure FileStructure = fileStructure;

    /// <summary> The Package object associated with the stub's. This will be accessed by GenerateXDoc() internally. </summary>
    private readonly Package Package = package;
    
    /// <summary> The path the AndroidManifest.xml file will be written to. </summary>
    public string FilePath = fileStructure.ManifestFilePath;

    /// <summary> The minimum supported Android SDK version for the compiled stub. </summary>
    public const int MINIMUM_SDK_VERSION = 21;

    /// <summary> The most recent Android SDK version the stub has been verified to work with. </summary> 
    public const int TARGET_SDK_VERSION = 36;
    
    /// <summary> 
    ///     The minimum supported Android version for the compiled stub. <br/> 
    ///     
    ///     As of 06/21/2026, this is Android 5.0 (Lollipop)
    /// </summary>
    public const AndroidOSVersion MINIMUM_OS_VERSION = (AndroidOSVersion)MINIMUM_SDK_VERSION;

    /// <summary> The required permissions the compiled stub requires to function as intended. </summary>
    private readonly string[] Permissions = ["INTERACT_ACROSS_PROFILES", "QUERY_ALL_PACKAGES"];

    /// <summary> The path of the AndroidManifest.xml file relative to the main build script (build.sh). </summary>
    public const string RelativeManifestPath = "src/main/AndroidManifest.xml";

    /// <summary> Generates an XDocument object containing the contents of the stub's AndroidManifest.xml </summary>
    private XDocument GenerateXDoc() 
    {
        var iconFileName = Path.GetFileNameWithoutExtension(FileStructure.Icon.OutputFilePath);
		var ns = XNamespace.Get("http://schemas.android.com/apk/res/android");
        return new XDocument
        (
			new XDeclaration("1.0", "utf-8", "yes"),
			new XElement("manifest",
			[
				new XAttribute(XNamespace.Xmlns + "android", ns.NamespaceName),
				new XAttribute("package", Package.Name),

				new XComment(
					"""
		                SDK 21 is the minimum compatible version as it introduced CrossProfileApps. 
		                https://developer.android.com/reference/android/content/pm/CrossProfileApps
		            """
				),
				new XElement("uses-sdk",
					new XAttribute(ns + "minSdkVersion", MINIMUM_SDK_VERSION.ToString()),
					new XAttribute(ns + "targetSdkVersion", TARGET_SDK_VERSION.ToString())
				),

				new XComment(
					"""
		                SDK 21 is the minimum compatible version as it introduced CrossProfileApps. 
		                https://developer.android.com/reference/android/content/pm/CrossProfileApps
		            """
				),
				new XElement("uses-sdk",
					new XAttribute(ns + "minSdkVersion", MINIMUM_SDK_VERSION.ToString()),
					new XAttribute(ns + "targetSdkVersion", TARGET_SDK_VERSION.ToString())
				),

				new XComment(
					""" 
		                INTERACT_ACROSS_PROFILES is required for cross-profile application launching.
		                Without the permission, The launcher will throw a SecurityException when launching a cross-profile app.
		                This would prevent the stub from supporting Work Profile Sandboxing through Island, Insular, etc.
		            """
				),
				
                ..Permissions
					.Select(p => new XElement("uses-permission",
						new XAttribute(ns + "name", $"android.permission.{p}")
					)),

				new XComment(
					"""
		                Attribute Guide:
		            
		                supportsRtl=true         | Informs Android that the stub supports Right to left layout alterations.
		                exported=true            | Allows the active launcher to open the stub. 
		                noHistory=true           | Prevents the stub from being cached in Android's activity stack.
		                excludesFromRecents=true | Prevents the stub from being included in Android's Recent Apps menu.
		                launchMode=singleTask    | Specifies the stub may only be used to launch a single external package.
		            """
				),
				new XElement("application",
					new XAttribute(ns + "label", Package.Label),
					new XAttribute(ns + "icon", $"@drawable/{iconFileName}"),
					new XAttribute(ns + "supportsRtl", "true"),
					new XAttribute(ns + "theme", "@android:style/Theme.NoTitleBar"),
				
                    new XElement("activity",
                        new XAttribute(ns + "name", ".MainActivity"),
                        new XAttribute(ns + "excludeFromRecents", "true"),
                        new XAttribute(ns + "exported", "true"),
                        new XAttribute(ns + "launchMode", "singleTask"),
                        new XAttribute(ns + "noHistory", "true"),

                        new XElement("intent-filter",
                            new XElement("action",
                                new XAttribute(ns + "name", "android.intent.action.MAIN")
                            ),
                            new XElement("category",
                                new XAttribute(ns + "name", "android.intent.category.LAUNCHER")
                            ),
                            new XElement("category",
                                new XAttribute(ns + "name", "android.intent.category.DEFAULT")
                            )
                        )
                    )
                )
            ])
		); 
    }

    /// <summary> 
    ///     A runtime safe method that will write the AndroidManifest.xml file to disk. <br/>
    ///     This function will hault the application's execution if an exception is thrown.
    /// </summary>
    public void Write() 
    {
		XDocument doc = GenerateXDoc();

        bool requiresFlush = true;
        try 
        {
            if (File.Exists(FilePath)) {
                File.Delete(FilePath);
            }
        }
        catch (Exception ex) {
            WriteWarningMessage($"An error occurred when trying to delete the old copy of: {FilePath}");
            WriteErrorMessage(ex.Message);
            WriteInformation("Flushing the existing stream..");
            requiresFlush = true;
        }

        try 
        {
            using Stream fileStream = File.Open(FilePath, FileMode.OpenOrCreate);
            if (requiresFlush) {
                fileStream.Flush();
            }

            doc.Save(fileStream);
        }

        catch (Exception ex) {
            WriteWarningMessage($"An error occurred when trying to write: {FilePath}");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }
    }
}