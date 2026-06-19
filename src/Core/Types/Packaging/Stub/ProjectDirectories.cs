namespace DummyDroidStubGen.Core.Types.Packaging.Stub;

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using static Global.Messaging;
public class ProjectDirectories(
    [Required] string projectParentDir,
    [Required] string mainSourceDir,
    [Required] string resourceDir,
    [Required] string drawableDir,
    [Required] string javaCodeDir
)
{
    /// <summary> The directory where all project files will be written to. </summary>
    public string ProjectParent { get; } = projectParentDir;

    /// <summary> The sub directory where all the stub's source will be written. </summary>
    public string MainSource { get; } = mainSourceDir;

    /// <summary> The path to the subdirectory that will store all asset files used by the stub. </summary>
    public string Resources { get; } = resourceDir;

    /// <summary> The path to the subdirectory inside Resources that will hold the drawable vectors. </summary>
    public string Drawables { get; } = drawableDir;

    /// <summary> The path to the directory containing the stub's .java class files </summary>
    public string JavaCode { get; } = javaCodeDir;

    /// <summary>
    ///     Resolves all properties of a StubProjectDirectories object. <br/>
    ///     Returns an IEnumerable containing KeyValuePair objects. <br/>
    ///     Each KeyValuePair contains the following: <br/>
    ///     
    ///     Key: The name of the property. <br/>
    ///     Value: The value of the property.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string?>> AsEnumerable() 
    {
        PropertyInfo[]? Properties = null;
        
        try {    
            Properties = typeof(ProjectDirectories).GetProperties();
        }
        
        catch (Exception ex)
        {
            WriteErrorMessage("Unable to cast StubProjectDirectory to IEnumerable<string?>");
            WriteErrorMessage(ex.Message);
        }

        var isNull = Properties == null;

        if (isNull || Properties!.Length == 0)
        {
            var status = isNull ? "null" : "empty";
            WriteErrorMessage($"Unable to cast StubProjectDirectory to string array, Properties is {status}.");
            return [];
        }
        
        return Properties.Select
        (
            Property => KeyValuePair.Create(
                Property.Name, 
                (string?)Property.GetValue(this)
            )
        ) ?? [];
    }
}