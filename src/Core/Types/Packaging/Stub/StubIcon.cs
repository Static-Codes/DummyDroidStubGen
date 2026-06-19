namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

using DummyDroidStubGen.Core.Extensions;
using static Core.Helpers.IO.FileHelper;
using static Global.Messaging;

public enum IconFileType { UNSET = 0, XML = 1, WEBP = 2, SVG = 3 }
public class StubIcon 
{
    /// <summary> 
    ///     A byte array containing the XML contents of the Android DrawableVector to be used as a logo. <br/>
    ///     This logo will be saved at: path/to/Java/PackageName/src/res/icon.xml
    /// </summary>
    public byte[] IconBuffer = [];
    public string? InputFilePath;
    public string? OutputFilePath;
    public IconFileType InputFileType = IconFileType.UNSET;
    public IconFileType OutputFileType = IconFileType.UNSET;

    public StubIcon(string inputIconPath, string outputIconPath) 
    {
        InputFileType = inputIconPath.ToIconFileType();
        OutputFileType = outputIconPath.ToIconFileType();

        if (InputFileType != IconFileType.WEBP && InputFileType != IconFileType.XML) {
            throw new ArgumentException("Invalid IconFileType provided to StubIconInfo constructor, expected WEBP or XML");
        }

        InputFilePath = inputIconPath;
        OutputFilePath = outputIconPath;
        this.SetBuffer();
    }
}

public static class StubIconInfoExtension 
{
    public static void SetBuffer(this StubIcon iconInfo) 
    {
        // If the inputFilePath passed as a parameter is valid, the function ends here.
        if (TrySerializePathToByteArray(iconInfo.InputFilePath, out iconInfo.IconBuffer)) {
            return;
        }
        
        WriteWarningMessage("Unable to serialize the provided icon content stream.");
        WriteInformation("Choosing a predefined vector for your icon...");
        var choice = DefaultDrawables.GetPsuedoRandomChoice();

        // A safe check on whether or not the embedded resource can be resolved.
        if (!TryGetManifestResourceStream(choice.ResourcePath, out var stream)) 
        {
            WriteWarningMessage("Unable to retrieve the predefined vector's content stream.");
            Environment.Exit(1);
        }

        // Assigns the predefined vector's contents to IconBuffer (if successful), otherwise exits.
        if (!TrySerializeStreamToByteArray(stream!, out iconInfo.IconBuffer)) 
        {
            WriteWarningMessage("Unable to serialize the predefined Vector's content stream.");
            Environment.Exit(1);
        }
    }
}