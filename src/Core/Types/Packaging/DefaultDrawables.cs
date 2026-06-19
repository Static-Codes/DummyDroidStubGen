/*
 * Copyright (C) 2026 Static Codes
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace DummyDroidStubGen.Core.Types.Packaging;
using System.Reflection;
using static Global.Messaging;

/// <summary> 
///     Represents the built-in drawables that bundled with the Android 34 SDK. <br/>
/// 
///     A drawable is a vector image (similar to SVGs) that is stored in an XML-like format.
///     
///     If the user does not want to use a custom app icon, DDSG will pseudorandomly choose one of these enum members.
/// </summary> 

public record DefaultDrawable(int Index, string ResourcePath);
public static class DefaultDrawables
{
    public static DefaultDrawable CHESS_KNIGHT { get; } = new(
        Index: 0, 
        ResourcePath: "DummyDroidStubGen.Resources.Android.DrawableVectors.chess_knight.xml"
    );

    public static DefaultDrawable CYCLONE { get; } = new (
        Index: 1, 
        ResourcePath: "DummyDroidStubGen.Resources.Android.DrawableVectors.cyclone.xml"
    );
    
    public static DefaultDrawable DPAD { get; } = new(
        Index: 2, 
        ResourcePath: "DummyDroidStubGen.Resources.Android.DrawableVectors.dpad.xml"
    );
    
    public static DefaultDrawable FOOTBALL { get; } = new(
        Index: 3, 
        ResourcePath: "DummyDroidStubGen.Resources.Android.DrawableVectors.football.xml"
    );
    
    public static DefaultDrawable HAND { get; } = new(
        Index: 4, 
        ResourcePath: "DummyDroidStubGen.Resources.Android.DrawableVectors.hand.xml"
    );
    
    public static DefaultDrawable MAP { get; } = new(
        Index: 5, 
        ResourcePath: "DummyDroidStubGen.Resources.Android.DrawableVectors.map.xml"
    );
    
    public static DefaultDrawable SHOPPING { get; } = new(
        Index: 6, 
        ResourcePath: "DummyDroidStubGen.Resources.Android.DrawableVectors.shopping.xml"
    );

    /// <summary> 
    ///     Resolves all DrawableVectors that are embedded within DDSG, then chooses and returns one at random.
    /// </summary>
    public static DefaultDrawable GetPsuedoRandomChoice() 
    {
        try 
        {
            var rawOptions = typeof(DefaultDrawables).GetProperties();

            if (rawOptions == null || rawOptions.Length == 0) {
                WriteErrorMessage("Variable 'rawOptions' returned null in GetPsuedoRandomChoice()");
                WriteInformation("Defaulting to CYCLONE");
                return CYCLONE;
            }
            
            int pseudoRandomIndex = new Random().Next(0, rawOptions.Length - 1);

            PropertyInfo pseudoRandomChoice = rawOptions.ElementAt(pseudoRandomIndex);
            
            // Returns either the DefaultDrawable associated with the choice or defaults to CYCLONE if reflection fails.
            return (DefaultDrawable?)pseudoRandomChoice.GetValue(null) ?? CYCLONE;
        }
        
        catch (Exception ex) {
            WriteWarningMessage("An exception has occured while attempting to randomly choice an App Icon.");
            WriteErrorMessage(ex.Message);
            WriteInformation("Defaulting to CYCLONE");
            return CYCLONE;
        }
        
    }
}