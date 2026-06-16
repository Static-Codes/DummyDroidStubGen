#!/bin/bash
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
    echo "Invalid usage."
    echo "Expected: ./run.sh \"AppName\" \"PackageName\""
    echo "Example: ./run.sh \"OnDeviceLabelFetcher\" \"com.staticcodes.odlf\"" 
    exit 1
fi


APP_NAME="$1" # The name of the Android Application
PKG_ID="$2" # The package identifer associated with the Application


# Splitting $PKG_ID by '.' as the delimeter.
readarray -d "." -t PKG_ID_PARTS <<< "$PKG_ID"

# -le is the bash equivalent to <= (Less than or equals.)
NUM_OF_PARTS="${#PKG_ID_PARTS[@]}"
if [ "$NUM_OF_PARTS" -le "1" ]; then
    echo "Invalid PackageName provided."
    echo -e "Expected Formats:\n"
    echo -e "1.\tcom.yourname"
    echo -e "2.\tcom.yourname.yourpackage"
    exit 1;
fi


# Joins an array by the provided delimiter, while also removing whitespace.
function join_by_without_spaces { 
    local IFS="$1"
    shift
    var="$*"
    read -rd '' var <<< "$var"
    echo "$var"
}

# Joins an array by the provided delimiter, while keeping any whitespace
function join_by { local IFS="$1"; shift; echo "$*"; }

# The actual packagename from the package ID
# Example: "com.devname.mycoolapp" returns "mycoolapp"
PKG_NAME=${PKG_ID_PARTS[-1]}

# The source directory structure associated with the Java for Android Project.
PKG_DIR_STRUCTURE=$(join_by '/' "${PKG_ID_PARTS[@]}")

# The filename of the compiled APK
APK_NAME=$(join_by_without_spaces '' "$PKG_NAME").apk

# Installing the compiled APK to a personal/non-sandboxed profile.
# If you are using PROFILE_ID="10", please comment out this line.
PROFILE_ID="0" 

# Uncomment this line to install the compiled APK to a sandboxed/work profile.
# PROFILE_ID="0"

echo -e "[1/16] -> Attempting to uninstall package: $PKG_ID"  

# Capturing both STDOut and STDIn using "2>&1"
# This is required as adb throws an exit code of 1 for both a failed uninstallatation and a missing device.
UNINSTALL_RESULT=$(adb shell pm uninstall --user $PROFILE_ID "$PKG_ID" 2>&1)

if [ "$UNINSTALL_RESULT" = "adb: no devices/emulators found" ]; then 
    echo -e "Unable to locate any devices connected to adb, please try again.\n"
    exit 1
fi

sleep 1

echo -e "\n[2/16] -> Compiling $APP_NAME from source..."
if ! ./build.sh "$APP_NAME" "$PKG_DIR_STRUCTURE" "$APK_NAME"; then
    exit 1
fi

sleep 1

echo -e "[13/16] -> Installing $APP_NAME ($APK_NAME) to device via adb..."
if ! adb install --user $PROFILE_ID "$APK_NAME"; then
    exit 1
fi
sleep 1

echo -e "\n[14/16] -> Executing $APP_NAME on device via adb..."
if ! adb shell am start -n "$PKG_ID/$PKG_ID.$APP_NAME"; then
    exit 1
fi
sleep 0.5

echo -e "\n[15/16] -> Reading output from $APP_NAME on device via adb..."


OUTPUT=$(adb shell run-as "$PKG_ID" cat "/data/data/$PKG_ID/files/list.txt")
if [ -z "$OUTPUT" ]; then
    echo "Unable to process packages on the device."
    exit 1
fi
sleep 0.5

echo -e "\n[16/16] -> Removing $APP_NAME ($APK_NAME) from device via adb..."  
if ! adb shell pm uninstall --user "$PROFILE_ID" "$PKG_ID"; then
    exit 1
fi

echo -e "\n$OUTPUT"
exit 1