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

/// <summary> 
///     Represents the built-in drawables that bundled with the Android 34 SDK. <br/>
/// 
///     A drawable is a vector image (similar to SVGs) that is stored in an XML-like format.
///     
///     If the user does not want to use a custom app icon, DDSG will pseudorandomly choose one of these enum members.
/// </summary> 
public class DefaultDrawable
{
    public const string CYCLONE = "@drawable/cyclone_48";
    public const string ORBIT = "@drawable/orbit_48";
    public const string CHESS_KNIGHT = "@drawable/chess_knight";
}