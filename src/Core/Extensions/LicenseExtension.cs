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

        var commentChar = true switch
        {
            _ when file is JavaFile => "//",
            _ when file is ShellFile => "#",
            _ => throw new Exception("Invalid object type provided to switch statement in WriteLicenseNotice.")
        };

        // According to MS docs, a dynamic cast should resolve any issues with subsequent calls to AddLine below.
        // https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/interop/using-type-dynamic
        // TODO: Monitor this.
        var fileTarget = (dynamic)file;

        fileTarget.AddEmptyLine();
        fileTarget.AddLine($"{commentChar} This program is free software: you can redistribute it and/or modify");
        fileTarget.AddLine($"{commentChar} the Free Software Foundation, either version 3 of the License, or");
        fileTarget.AddLine($"{commentChar} (at your option) any later version.");
        
        fileTarget.AddEmptyLine();
        fileTarget.AddLine($"{commentChar} This program is distributed in the hope that it will be useful,");
        fileTarget.AddLine($"{commentChar} but WITHOUT ANY WARRANTY; without even the implied warranty of");
        fileTarget.AddLine($"{commentChar} MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the");
        fileTarget.AddLine($"{commentChar} GNU General Public License for more details.");
        
        fileTarget.AddEmptyLine();
        fileTarget.AddLine($"{commentChar} You should have received a copy of the GNU General Public License");
        fileTarget.AddLine($"{commentChar} along with this program.  If not, see <https://www.gnu.org/licenses/>.");
        fileTarget.AddEmptyLine();
    }
}
    
 