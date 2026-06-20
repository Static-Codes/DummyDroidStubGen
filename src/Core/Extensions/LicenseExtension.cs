namespace DummyDroidStubGen.Core.Extensions;

using Types.Packaging.Stub;
using static Global.Messaging;

public static class LicenseExtension 
{
    /// <summary> Writes the GNUv3 License Notice at the beginning of the shell script files. </summary>
    public static void WriteLicenseNotice(this object file) 
    {
        if (file is not JavaFile and not ShellFile){
            WriteErrorMessage("Invalid object type provided to WriteLicenseNotice.", exit: true, exitCode: 1);
        }

        // According to MS docs, a dynamic cast should resolve any issues with subsequent calls to AddLine below.
        // https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/interop/using-type-dynamic
        // TODO: Monitor this.
        var fileTarget = (dynamic)file;


        fileTarget.AddLine("/*");
        fileTarget.AddLine("*");
        fileTarget.AddLine("* This program is free software: you can redistribute it and/or modify");
        fileTarget.AddLine("* the Free Software Foundation, either version 3 of the License, or");
        fileTarget.AddLine("* (at your option) any later version.");
        fileTarget.AddLine("*");
        fileTarget.AddLine("* This program is distributed in the hope that it will be useful,");
        fileTarget.AddLine("* but WITHOUT ANY WARRANTY; without even the implied warranty of");
        fileTarget.AddLine("* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the");
        fileTarget.AddLine("* GNU General Public License for more details.");
        fileTarget.AddLine("*");
        fileTarget.AddLine("* You should have received a copy of the GNU General Public License");
        fileTarget.AddLine("* along with this program.  If not, see <https://www.gnu.org/licenses/>.");
        fileTarget.AddLine("*/");
    }
}
    
 