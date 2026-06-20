namespace DummyDroidStubGen.Core.Extensions;

using Core.Types.Packaging.Stub;
using static Global.Messaging;

public static class FileStructureExtension
{
    private static void CreateProjectDirectories(ProjectDirectories projectDirectories) 
    {
        var directoryPairsEnumerable = projectDirectories.AsEnumerable();

        foreach (var directoryPair in directoryPairsEnumerable) 
        {
            if (string.IsNullOrEmpty(directoryPair.Value)) 
            {
                WriteErrorMessage(
                    message: $"Unable to created required project directory \n\t-> {directoryPair.Value}",
                    exit: true,
                    exitCode: 1
                );   
            }

            try {
                Directory.CreateDirectory(directoryPair.Value);
            }

            catch (Exception ex) {
                WriteWarningMessage(
                    $"Unable to created required project directory \n\t-> {directoryPair.Value}"
                );
                WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
            }
        }
    }

    public static void Write(this ProjectDirectories stubProjectDirectories) 
    {
        CreateProjectDirectories(stubProjectDirectories);
    }
}