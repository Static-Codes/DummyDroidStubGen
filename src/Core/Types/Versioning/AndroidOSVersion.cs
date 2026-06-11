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
namespace DummyDroidStubGen.Core.Types.Versioning;

/// <summary> 
///     Contains an integer representing the AndroidAPILevel. <br/>
///     Exceptions: <br/>
///     UNKNOWN returns -1 <br/>
///     LESS_THAN_ANDROID_14 returns 0. <br/> 
/// </summary>
public enum AndroidOSVersion
{
    UNKNOWN = -1,
    LESS_THAN_ANDROID_14 = 0,
    // ANDROID_10 = 29,
    // ANDROID_11 = 30,
    // ANDROID_12 = 31,
    // ANDROID_12L = 32,
    // ANDROID_13 = 33,
    ANDROID_14 = 34,
    ANDROID_15 = 35,
    ANDROID_16 = 36,
    ANDROID_17 = 37

}