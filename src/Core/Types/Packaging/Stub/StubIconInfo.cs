namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

using System.Text.Json;
using static Core.Helpers.IO.FileHelper;
using static Global.Messaging;

public enum IconFileType { UNSET = 0, XML = 1, WEBP = 2, SVG = 3 }
public class StubIconInfo 
{
    /// <summary> 
    ///     A byte array containing the XML contents of the Android DrawableVector to be used as a logo. <br/>
    ///     This logo will be saved at: path/to/Java/PackageName/src/res/icon.xml
    /// </summary>
    public byte[] IconBuffer = [];
    public IconFileType FileType = IconFileType.UNSET;

    public StubIconInfo(byte[] iconBuffer, IconFileType fileType) 
    {
        IconBuffer = iconBuffer;
        FileType = fileType;
    }

    public StubIconInfo(Stream iconContentStream, IconFileType fileType) 
    {
        // If the original Stream object passed as a parameter is valid, the function ends here.
        if (TrySerializeStreamToByteArray(iconContentStream, out IconBuffer)) {
            FileType = fileType;
            return;
        }
        
        WriteWarningMessage("Unable to serialize the provided icon content stream.");
        WriteInformation("Choosing a predefined Vector for your icon...");
        var choice = DefaultDrawables.GetPsuedoRandomChoice();

        // A safe check on whether the embedded resource can be resolved.
        if (!TryGetManifestResourceStream(choice.ResourcePath, out var stream)) 
        {
            WriteWarningMessage("Unable to retrieve the predefined Vector's content stream.");
            Environment.Exit(1);
        }

        // Assigns the predefined vectors contents to IconBuffer (if successful), otherwise exits.
        if (!TrySerializeStreamToByteArray(stream!, out IconBuffer)) 
        {
            WriteWarningMessage("Unable to serialize the predefined Vector's content stream.");
            Environment.Exit(1);
        }

        FileType = fileType;
    }


}