namespace DummyDroidStubGen.Core.Types.Packaging;

using DummyDroidStubGen.Core.Extensions;
using System.Net;
using static DummyDroidStubGen.Core.Helpers.InputHelper;
using static DummyDroidStubGen.Global.Messaging;

public class Package(string Name, PackageCategory Category, string? Label = null)
{
    public string? Label { get; set; } = Label;
    public string Name = Name;
    public PackageCategory Category = Category;
    private static readonly HttpClientHandler handler = new() {
        AutomaticDecompression = DecompressionMethods.All
    };

    private static readonly HttpClient client = new(handler);
    
    public string? GetFriendlyName() 
    {
        if (Name == "android") {
            return "System Image (android)";
        }

        // Skipping packages of the type PackageCategory.System
        if (Category == PackageCategory.System) {
            return $"System Package ({Name})";
        }

        string? labelText = IconPatterns.Where(pattern => pattern.Name == Name)
                                        .FirstOrDefault()?
                                        .Label;

        if (labelText != null) {
            return labelText;
        }

        // Since some of the package names in IconPatterns, contain "com.android" this conditional is separated.
        // This ensures that if the package is present in IconPatterns, the known Label of that Package is used.
        if (Name.StartsWithAny(["com.android.", "com.google.android", "com.google.euiccpixel", ])) {
            return $"System Package ({Name})";
        }

        return null;


    }

    public async Task<bool> IsAlreadyPublished() 
    {
        HttpRequestMessage request = new(
            method: HttpMethod.Get, 
            requestUri: $"https://play.google.com/store/apps/details?id={Name}"
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
            response = await client.SendAsync(request);
        }
        catch (Exception ex) {
            WriteWarningMessage($"Unable to query 'play.google.com' for the package '{Name}'"); 
            WriteErrorMessage(ex.Message);
            WriteErrorMessage("Continuing may have unintended consequences on the wellbeing of your device!");
            WriteWarningMessage("If you use the name of a package that is already published on the Play Store:\n");

            WriteInformation(
                coloredText: "\t- You will not be able to download OR open the REAL application associated with that name."
            );
            
            WriteInformation(
                coloredText: "\t- You may cause a UI loop that will render your device stuck, requiring a reboot to fix."
            );

            WriteErrorMessage(
                message: "\t- You may cause general system instability, requiring a full system reinstallation."
            );

            var selection = AskForSelection(
                message: "Would you like to continue, given the circumstances?", 
                options: ["Yes, I understand the risks.", "No, I don't wish to continue"]
            );

            if (selection == "No, I don't wish to continue") {
                selection = ExitChoice;
            }

            UserExitStatusCheck(selection);

            WriteSuccessMessage("Continuing...");
        }

        try 
        {
            if (response == null) {
                WriteErrorMessage("Variable 'response' has a null value in Package.IsAlreadyPublished()");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }

        catch {
            return false;
        }
    }


    private readonly Package[] IconPatterns = [
        new(Name: "com.android.contacts", PackageCategory.Commercial, Label: "Contacts"),
        new(Name: "com.android.gallery3d", PackageCategory.Commercial, Label: "Gallery"),
        new(Name: "com.android.messaging", PackageCategory.Commercial, Label: "Messaging"),
        new(Name: "com.android.phone", PackageCategory.Commercial, Label: "Phone"),
        new(Name: "com.android.settings", PackageCategory.Commercial, Label: "Settings"),
    ];

    [Obsolete("Unused but left for reference.")]
    private readonly string[] NonIconPatterns = [
        "app.*.carrierconfig2",
        "app.*.AppCompatConfig",
        "app.*.config",
        "app.*.networklocation",
        "com.*.imsservice",
        "app.seamlessupdate.client",
        "android",
    ];
}