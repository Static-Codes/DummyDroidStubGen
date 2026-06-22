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
using static Helpers.IO.FileHelper;
using static Global.Messaging;


/// <summary> Contains the functions and members used to generate the stub's Java project files. </summary>
public class ProjectFileCode 
{
    const string licenseText =
        @"/*
        *
        * This program is free software: you can redistribute it and/or modify
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
        */";

    /// <summary> Writes the .classpath file for the current Java project. </summary>
    public static void CreateClassPathFile(string parentDirectory)
    {
        var expectedAndroidJarPath = Path.Combine(LibrariesSubDirectory, "android-21.jar");

        if (!File.Exists(expectedAndroidJarPath)) {
            WriteWarningMessage("Unable to locate the required Java Runtime for Android 5.0+ (Android SDK 21)");
            WriteErrorMessage($"JAR expected at: {expectedAndroidJarPath}", exit: true, exitCode: 1);
        }

        if (!Directory.Exists(parentDirectory)) {
            WriteWarningMessage("Unable to locate the provided project directory.");
            WriteErrorMessage($"Directory not found at:\n\t -> {parentDirectory}", exit: true, exitCode: 1);
        }

        XDocument doc = new
        (
            new XDeclaration(version: "1.0", encoding: "UTF-8", standalone: null),
            new XComment(licenseText),
            new XElement("classpath",
                new XElement("classpathentry", 
                    new XAttribute("kind", "con"), 
                    new XAttribute("path", "org.eclipse.jdt.launching.JRE_CONTAINER")
                ),
                new XElement("classpathentry", 
                    new XAttribute("kind", "src"), 
                    new XAttribute("path", "src/main")
                ),
                new XElement("classpathentry", 
                    new XAttribute("kind", "lib"), 
                    new XAttribute("path", "../Resources/lib/android-21.jar")
                ),
                new XElement(
                    "classpathentry", 
                    new XAttribute("kind", "output"), 
                    new XAttribute("path", "obj")
                )
            )
        );


        var classFilePath = Path.Combine(parentDirectory, ".classpath");

        doc.Save(classFilePath);
    }

    /// <summary> Writes the .project file for the current Java project. </summary>
    public static void CreateProjectFile(string parentDirectory, string projectName) 
    {
        string projectIDTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        XDocument doc = new
        (
            new XDeclaration(version: "1.0", encoding: "UTF-8", standalone: null),
            new XComment(licenseText),
            new XElement("projectDescription",
                new XElement("name", projectName),
                new XElement("comment"),
                new XElement("projects"),
                new XElement("buildSpec",
                    new XElement("buildCommand",
                        new XElement("name", "org.eclipse.jdt.core.javabuilder"),
                        new XElement("arguments")
                    )
                ),
                new XElement("natures",
                    new XElement("nature", "org.eclipse.jdt.core.javanature")
                ),
                new XElement("filteredResources",
                    new XElement("filter",
                        new XElement("id", projectIDTimeStamp),
                        new XElement("name"),
                        new XElement("type", "30"),
                        new XElement("matcher",
                            new XElement("id", "org.eclipse.core.resources.regexFilterMatcher"),
                            new XElement("arguments", "node_modules|\\.git|__CREATED_BY_JAVA_LANGUAGE_SERVER__")
                        )
                    )
                )
            )
        );

        var projectFilePath = Path.Combine(parentDirectory, ".project");
        doc.Save(projectFilePath);

    }
}