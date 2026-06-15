#!/bin/bash
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

echo -e "[1/13] -> Attempting to uninstall package: $PKG_ID"  
adb shell pm uninstall --user $PROFILE_ID "$PKG_ID"
sleep 1

echo -e "\n[2/13] -> Compiling $APP_NAME from source..."
./build.sh "$APP_NAME" "$PKG_DIR_STRUCTURE" "$APK_NAME"
sleep 1

echo -e "[13/13] -> Installing $APP_NAME ($APK_NAME) to device via adb...\n"
adb install --user $PROFILE_ID "$APK_NAME"
sleep 1

