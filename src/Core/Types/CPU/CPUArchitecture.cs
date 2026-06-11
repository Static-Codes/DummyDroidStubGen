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
namespace DummyDroidStubGen.Core.Types.CPU;

public enum CPUArchitecture 
{
    UNKNOWN = 0,
    Arm64V8a = 1,   // DummyDroidStubGen.Resources.arm64-v8a.aapt2
    ArmEabiV7a = 2, // DummyDroidStubGen.Resources.armeabi-v7a.aapt2
    X86 = 3,        // DummyDroidStubGen.Resources.x86.aapt2
    X86_64 = 4      // DummyDroidStubGen.Resources.x86-64.aapt2
}