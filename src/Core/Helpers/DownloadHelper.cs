namespace DummyDroidStubGen.Core.Helpers;

using static Helpers.IO.FileHelper;
using static Helpers.NetworkHelper;
using static Global.Messaging;
using System.IO.Compression;

public class DownloadHelper
{
    public static async Task<bool> DownloadVDToolArchive() 
    {
        HttpRequestMessage request = new(
            method: HttpMethod.Get, 
            requestUri: $"https://github.com/Static-Codes/vd-tool-builder/releases/download/v1.0/vd-tool-wrapper.zip"
        );

        request.Headers.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:151.0) Gecko/20100101 Firefox/151.0");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        request.Headers.Add("Sec-GPC", "1");
        request.Headers.Add("Alt-Used", "play.google.com");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("Sec-Fetch-Dest", "document");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "none");
        request.Headers.Add("Sec-Fetch-User", "?1");
        request.Headers.Add("Priority", "u=0, i");
        request.Headers.Add("TE", "trailers");

        HttpResponseMessage? response = null;

        try {
            response = await ClientInstance.SendAsync(request);
        }
        catch (Exception ex) {
            WriteWarningMessage($"Unable to download the latest release of VDTool."); 
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }

        FileStream? fileStream;

        try 
        {
            if (!Directory.Exists(VDToolSubDirectory)) {
                Directory.CreateDirectory(VDToolSubDirectory);
            }

            if (response == null || response.Content == null) {
                WriteErrorMessage("Variable 'response' has a null value in DownloadVDToolArchive()");
                return false;
            }

            response.EnsureSuccessStatusCode();
            
            fileStream = File.Open(VDToolZipPath, FileMode.CreateNew);

            await response.Content.CopyToAsync(fileStream);

            if (fileStream.Length == 0) {
                WriteErrorMessage("fileStream.Length is 0 in DownloadVDToolArchive()");
                return false;
            }

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            await ZipFile.ExtractToDirectoryAsync(fileStream, VDToolSubDirectory, cts.Token);

            await fileStream.DisposeAsync();

            File.Delete(VDToolZipPath);

            return true;
        }

        catch (Exception ex) 
        {
            WriteWarningMessage("An error has occurred while attempting to download the VDTool archive.");
            #if DEBUG
                WriteErrorMessage(ex.StackTrace ?? ex.Message);
            #else
                WriteErrorMessage(ex.Message);
            #endif

            return false;
        }
    }
}