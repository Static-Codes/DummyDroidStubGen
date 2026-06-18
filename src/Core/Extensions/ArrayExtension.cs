namespace DummyDroidStubGen.Core.Extensions;

using static Global.Messaging;

public static class ArrayExtensions
{
    private static void WriteMaintainersNote() 
    {
        WriteInformation(
            coloredText: "The following is a note for maintainers/developers of DDSG only!",
            textColor: "red",
            tagName: "[[NOTICE]]:",
            tagNameColor: "red"
        );

        WriteWarningMessage("Currently, ArrayExtensions.Deconstruct<T> only supports a T[] with a length of 3 or less.");
        WriteInformation("If you need to deconstruct more than 3 variables, please consider a different approach first.");
        WriteInformation("If you find that deconstruction is the only way forward, see:");

        WriteInformation(
            coloredText: "/path/to/DummyDroidStubGen/src/Core/Extensions/ArrayExtensions.cs",
            textColor: "blue",
            tagName: "[[FILE]]:",
            tagNameColor: "red"
        );
    }

    /// <summary> 
    ///     A simple implementation of Deconstruct to allow for Array Deconstruction. <br/>
    ///     
    ///     Note: This implementation only handles T[] with size of 0 to 3. <br/>
    /// 
    ///     If a future maintainer requires an additional variable support, for example: <br/>
    ///     
    ///     var (var1, var2, var3, var4, var5) = CallToSomeFunction(); <br/>
    /// 
    ///     Then this method may be used as a basis for elaboration.
    /// </summary>
    public static void Deconstruct<T>(this T[] array, out T var1, out T var2, out T var3)
    {
        if (array.Length > 3) {
            WriteMaintainersNote();
        }
        
        var1 = array.Length > 0 ? array[0] : default!;
        var2 = array.Length > 1 ? array[1] : default!;
        var3 = array.Length > 2 ? array[2] : default!;
    }
}