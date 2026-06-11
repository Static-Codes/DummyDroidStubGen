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
using System.Net;

namespace DummyDroidStubGen.Core.Helpers;

public static class NetworkHelper {
    private static readonly HttpClient _clientInstance = new(
        new HttpClientHandler() {
            AutomaticDecompression = DecompressionMethods.All
        }
    );

    /// <summary>
    ///     A static instance of the HttpClient that will be created once, and used for the entire runtime execution.
    /// </summary>
    public static HttpClient ClientInstance => _clientInstance;
}