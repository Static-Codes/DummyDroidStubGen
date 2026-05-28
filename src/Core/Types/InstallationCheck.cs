namespace DummyDroidStubGen.Core.Types;

public class InstallationCheck(string BinaryName, string BinaryPath, Func<bool> Check)
{
    public string BinaryName { get; set; } = BinaryName;
    public string BinaryPath { get; set; } = BinaryPath;
    public bool IsInstalled { get; set; } = Check.Invoke();
}