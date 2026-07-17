#!/usr/bin/env sh
# Copyright (C) 2026 Static Codes
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program. If not, see <https://www.gnu.org/licenses/>.

if [ -z "$1" ] || [ -z "$2" ] ; then
    printf "Invalid usage.\n" >&2
    printf "Expected: ./run.sh \"AppName\" \"PackageName\"\n" >&2
    printf "Example: ./run.sh \"OnDeviceLabelFetcher\" \"com.staticcodes.odlf\"\n" >&2
    exit 1
fi

if [ -z "$HOME" ]; then
    printf "DDSG requires '\$HOME' to be set before it is able to sideload %s\n" "$1" >&2
    printf "Use the command below replacing 'username' with the current user's username.\n\n" >&2
    printf "export HOME=\"/home/username\"\n" >&2
    exit 1
fi

# The directory containing all application subdirectories and files. 
APP_DATA_DIR="$HOME/.config/DummyDroidStubGen"
# The absolute path to the directory where this script is stored on disk. 
SCRIPT_DIR="$APP_DATA_DIR/Resources/$1"
# The absolute path to the build script for ODLF
SCRIPT_PATH="$SCRIPT_DIR/build.sh"
# The name of the Android Application 
APP_NAME="$1"
# The package identifer associated with the Application 
PKG_ID="$2"
# The package's internal directory structure
PKG_DIR_STRUCTURE=$(printf '%s' "$PKG_ID" | tr '.' '/')

get_apk_name() {
    package_name_delimiter="."
    old_ifs="$IFS"
    
    IFS="$package_name_delimiter"

    # Since POSIX doesnt support arrays, this is the disgusting alternative.
    set -- $PKG_ID
    IFS="$old_ifs"

    # Iterating through the provided elements to retrieve the last (using -gt [greater than])
    while [ "$#" -gt 1 ]; do shift; done
    printf "%s.apk" "$1"
}

get_instances_of_substring() {
    if [ -z "$1" ] || [ -z "$2" ]; then
        printf "Invalid usage of get_instances_of_substring\n" >&2
        exit 1
    fi

    count=$(printf '%s' "$1" | awk -v s="$2" '
    BEGIN{ n=0 }
    {
      while (1) {
        i = index($0, s)
        if (i == 0) break
        n++
        $0 = substr($0, i + length(s))
      }
    }
    END{ print n }
    ')

    printf "%s" "$count"
}

join_by() { IFS="$1"; shift; printf "%s\n" "$*"; }

SUBSTRING_COUNT=$(get_instances_of_substring "$PKG_ID" ".")

if [ "$SUBSTRING_COUNT" -le "1" ]; then
    printf "Invalid PackageName provided.\n" >&2
    printf "Expected Formats:\n" >&2
    printf "1.\tcom.yourname\n" >&2
    printf "2.\tcom.yourname.yourpackage\n" >&2
    exit 1;
fi

APK_NAME=$(get_apk_name)
PROFILE_ID="0"

printf "%s\n[1/16] -> Attempting to uninstall package: " "$PKG_ID"
UNINSTALL_RESULT=$(adb shell pm uninstall --user "$PROFILE_ID" "$PKG_ID" 2>&1)
if [ "$UNINSTALL_RESULT" = "adb: no devices/emulators found" ]; then 
    printf "Unable to locate any devices connected to adb, please try again.\n" >&2
    exit 1
fi
sleep 1

printf "%s\n[2/16] -> Compiling %s from source..." "$APP_NAME"
if ! "$SCRIPT_PATH" "$APP_NAME" "$PKG_DIR_STRUCTURE" "$APK_NAME" "$APP_DATA_DIR"; then    
    exit 1
fi
sleep 1

printf "%s\n[13/16] -> Installing %s (%s) to device via adb..." "$APP_NAME" "$APK_NAME"
if ! adb install --user "$PROFILE_ID" "$APK_NAME"; then
    exit 1
fi
sleep 1

printf "%s\n[14/16] -> Executing %s on device via adb..." "$APP_NAME"
if ! adb shell am start -n "$PKG_ID/$PKG_ID.$APP_NAME"; then
    exit 1
fi
sleep 0.5

printf "%s\n[15/16] -> Reading output from %s on device via adb..." "$APP_NAME"
OUTPUT=$(adb shell run-as "$PKG_ID" cat "/data/data/$PKG_ID/files/list.txt") || $(echo "error: package list not found" && exit 1)
if [ -z "$OUTPUT" ]; then
    printf "Unable to process packages on the device.\n"
    exit 1
fi
sleep 0.5

printf "%s\n[16/16] -> Removing %s (%s) from device via adb..." "$APP_NAME" "$APK_NAME"
if ! adb shell pm uninstall --user "$PROFILE_ID" "$PKG_ID"; then
    exit 1
fi
printf "%s\n" "$OUTPUT"
exit 0