namespace DummyDroidStubGen.Core.Types.Packaging;

public class Blacklist 
{   
    // This blacklist is in no means "impenetrable". 
    // It serves as a rudamentary attempt to prevent the user from generating a stub that links to a sensitive application.
    //
    // These applications include categories such as:
    //
    // - Core System Utilities (Which can cause general instability at best and compromises at worst.)
    // - Common Commercial Targets (Such as banking apps, crypto wallets, etc)
    // - Private messaging apps, such as Signal, Simplex, Telegram 
    private static readonly List<Package> BlacklistedPackages = 
    [
        // --- Core Android System & Services ---
        new Package("android", PackageCategory.System, "Android Framework"),
        new Package("com.android.systemui", PackageCategory.System, "System UI"),
        new Package("com.android.settings", PackageCategory.System, "Settings"),
        new Package("com.android.keyguard", PackageCategory.System, "Keyguard"),
        new Package("com.android.packageinstaller", PackageCategory.System, "Package Installer"),
        new Package("com.android.phone", PackageCategory.System, "Phone Service"),
        new Package("com.android.nfc", PackageCategory.System, "NFC Service"),
        new Package("com.android.bluetooth", PackageCategory.System, "Bluetooth Service"),
        new Package("com.google.android.gms", PackageCategory.System, "Google Play Services"),
        new Package("com.google.android.packageinstaller", PackageCategory.System, "Google Package Installer"),

        // --- Banking & Financial Apps ---
        // These are frequently targeted; preventing stubs here mitigates phishing/malware risks.
        new Package("com.paypal.android.p2pmobile", PackageCategory.Commercial, "PayPal"),
        new Package("net.one97.paytm", PackageCategory.Commercial, "Paytm"),
        new Package("com.google.android.apps.nbu.paisa.user", PackageCategory.Commercial, "Google Pay"),
        new Package("com.phonepe.app", PackageCategory.Commercial, "PhonePe"),
        
        // --- Crypto Wallets ---
        // High-value targets for asset theft.
        new Package("io.metamask", PackageCategory.Commercial, "MetaMask"),
        new Package("piuk.blockchain.android", PackageCategory.Commercial, "Blockchain.com Wallet"),
        new Package("com.coinbase.android", PackageCategory.Commercial, "Coinbase"),
        new Package("org.trustwallet.app", PackageCategory.Commercial, "Trust Wallet"),

        // --- Business Messaging & Social Communication ---
        // Preventing unauthorized interaction with personal communication history.
        new Package("com.whatsapp.w4b", PackageCategory.Commercial, "WhatsApp Business"),
        new Package("org.telegram.messenger", PackageCategory.Commercial, "Telegram"),
        new Package("org.telegram.messenger.web", PackageCategory.Commercial, "Telegram Web"),
        new Package("org.signal.securesms", PackageCategory.Commercial, "Signal"),
    ];

    public static bool IsBlacklisted(string packageName) 
    {
        return BlacklistedPackages.Any(p => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase));
    }
}