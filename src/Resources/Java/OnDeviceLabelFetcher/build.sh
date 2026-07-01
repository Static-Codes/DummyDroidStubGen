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


set -e

if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ] || [ -z "$4" ]; then
    echo "Invalid usage." >&2
    echo "Expected: ./build.sh \"AppName\" \"Package/Directory/Structure" "packageName.apk" "path/to/working/dir" >&2
    echo "Example: ./build.sh \"OnDeviceLabelFetcher\" \"com/staticcodes/odlf\" \"odlf.apk\"" \""path/to/working/dir\"" >&2
    exit 1
fi

# The name of the class that contains the an extension of Android's Activity class.
APP_NAME="$1"
# The project's source directory structure
PKG_DIR_STRUCTURE="$2"
# The filename of the compiled APK
APK_NAME="$3"
# The app data directory for script execution.
APP_DATA_DIR="$4"

# The relative path to where the generated class files will reside.
RELATIVE_CLASS_DIR="$PKG_DIR_STRUCTURE/$APP_NAME"
# The absolute path to the working directory for this script.
WORKING_DIR="$APP_DATA_DIR/Resources"
# The absolute path to the directory containing Java Runtime Executables for the stub.
JAVA_LIBS_DIR="$WORKING_DIR/libs"
# The absolute path to the directory containing ODLF's source.
APP_BUILD_DIR="$WORKING_DIR/$APP_NAME"
# The absolute path to the Java source for the compiled stub.
JAVA_SOURCE_DIR="$APP_BUILD_DIR/src/main"
# The absolute path to the directory containing resource files.
JAVA_RESOURCE_DIR="$JAVA_SOURCE_DIR/res"
# The absolute path to the compiled objects for the compiled stub..
JAVA_OBJECT_DIR="$APP_BUILD_DIR/obj"
# The absolute path to the directory containing the Android Dex classes for the compiled stub.
JAVA_DEXOUT_DIR="$APP_BUILD_DIR/dex_out"

# The absolute path to the Debug keystore file that will be used to sign the compiled APK.
DEBUG_KEYSTORE_PATH="$APP_BUILD_DIR/debug.keystore" 
# The absolute path to the compiled resources that will be included in the compiled stub.
COMPILED_RESOURCES_PATH="$APP_BUILD_DIR/compiled_resources.zip"
# The absolute path to the unaligned APK for the compiled stub.
UNALIGNED_APK_PATH="$APP_BUILD_DIR/unaligned.apk"
# The absolute path to the aligned APK for the compiled stub.
ALIGNED_APK_PATH="$APP_BUILD_DIR/aligned.apk"
# The absolute path to the final aligned APK for the compiled stub.
FINAL_APK_PATH="$APP_BUILD_DIR/$APK_NAME"
# The absolute path to the compiled Android Dex classes.dex.
ANDROID_DEX_CLASS_PATH="$JAVA_DEXOUT_DIR/classes.dex"
# The absolute path to the Android Manifest XML file for the compiled stub.
ANDROID_MANIFEST_PATH="$JAVA_SOURCE_DIR/AndroidManifest.xml"

# The Android 5.0 Lollipop (API 21) Java Runtime
ANDROID_JAR="$JAVA_LIBS_DIR/android-21.jar"
# The D8 (R8) runtime to convert Java ByteCode to Android Dex.
D8_JAR="$JAVA_LIBS_DIR/r8lib.jar"

# The absolute path to the compiled Java class files for the compiled stub.
JAVA_CLASS_FILES_WILDCARD_PATH="$JAVA_OBJECT_DIR/$RELATIVE_CLASS_DIR*.class"
# The absolute path to the directory containing the stub's Java source files and a wildcard to represent the files themselves.
JAVA_FILES_WILDCARD_PATH="$JAVA_SOURCE_DIR/$PKG_DIR_STRUCTURE/*.java"


if [ ! -e "$ANDROID_JAR" ] || [ ! -e "$D8_JAR" ]; then
    echo "One or more of the required Java Runtime Executables were missing from: '$JAVA_LIBS_DIR'" >&2
    exit 1
fi

# cd $APP_BUILD_DIR

mkdir -p $JAVA_OBJECT_DIR $JAVA_DEXOUT_DIR

printf "%b" "\n[3/16] -> Compiling Resources for $APP_NAME...\n"

aapt2 compile --dir "$JAVA_RESOURCE_DIR" -o "$COMPILED_RESOURCES_PATH"

printf "%b" "\n[4/16] -> Linking XML Manifest using aapt2...\n"
aapt2 link --auto-add-overlay \
           --manifest "$ANDROID_MANIFEST_PATH" \
           -I "$ANDROID_JAR" \
           -R "$COMPILED_RESOURCES_PATH" \
           -o "$UNALIGNED_APK_PATH"

printf "%b" "\n[5/16] -> Compiling Java to ByteCode using javac...\n"
javac -d obj --release 8 -classpath $ANDROID_JAR $JAVA_FILES_WILDCARD_PATH
   
printf "%b" "\n[6/16] -> Converting ByteCode to Android Dex...\n"
java -cp $D8_JAR com.android.tools.r8.D8 --lib $ANDROID_JAR --release --output $JAVA_DEXOUT_DIR $JAVA_CLASS_FILES_WILDCARD_PATH
  
printf "%b" "\n[7/16] -> Packaging Output Dex Classes...\n"
zip -uj "$UNALIGNED_APK_PATH" "$ANDROID_DEX_CLASS_PATH"

printf "%b" "\n[8/16] -> Aligning APK...\n"
# -P | Aligns uncompressed .so libraries page size to X KiB chunks.
# 16 | Specifies the "X" in the "X KiB" above.
# -f | Forces an overwright of aligned.apk (if a previous build failed)
# -v | Verbose output of the aligning process.
# 4  | Aligning optional APK resources into 4 byte chunks 
# Docs: https://developer.android.com/tools/zipalign

# Maintainers Note:
# If this project ever requires .so libraries:
# The next line should be uncommented, and the line below that should commented out.
# if ! zipalign -P 16 -v 4 "$UNALIGNED_APK_PATH" "$ALIGNED_APK_PATH"
if ! zipalign -f -v 4 "$UNALIGNED_APK_PATH" "$ALIGNED_APK_PATH"; then
    printf "%b" "\nUnable to align compiled APK, please try again.\n" >&2
    exit 1
fi


printf "%b" "\n[9/16] -> Confirming Alignment...\n"
# -c | Instructs zipalign to perform a confirmation instead of an overwrite
# -P | Aligns uncompressed .so libraries page size to X KiB chunks.
# 16 | Specifies the "X" in the "X KiB" above.
# -v | Verbose output of the aligning process.
# 4  | Aligning optional APK resources into 4 byte chunks (extra headers) 
# Docs: https://developer.android.com/tools/zipalign

# Maintainers Note:
# If this project ever requires .so libraries:
# The next line should be uncommented, and the line below that should commented out.
# if zipalign -c -P 16 -v 4 "$ALIGNED_APK_PATH"; then
if zipalign -c -v 4 "$ALIGNED_APK_PATH"; then
    printf "%b" "\nAlignment Confirmed!"
else
    printf "%b" "\nIncorrect Alignment Detected!"
    exit 1
fi

printf "%b" "\n[10/16] -> Removing Unaligned APK..."
rm -f "$UNALIGNED_APK_PATH"

printf "%b" "\n[11/16] -> Signing APK...\n"
if [ ! -f "$DEBUG_KEYSTORE_PATH" ]; then
    echo "File debug.keystore not found, generating now." >&2
    keytool -genkey -v -keystore "$DEBUG_KEYSTORE_PATH" \
            -storepass android -alias androiddebugkey \
            -keypass android -keyalg RSA -keysize 2048 -validity 10000 -dname "CN=Android Debug,O=Android,C=US"
fi


if ! apksigner sign --ks "$DEBUG_KEYSTORE_PATH" --ks-pass pass:android --in $ALIGNED_APK_PATH --out "$APK_NAME"; then
    echo "Unable to sign the APK.." >&2
    exit 1
fi

printf "%b" "\n[12/16] -> Removing leftover build artifacts...\n"
if ! rm -rf "$FINAL_APK_PATH.idsig" "$ALIGNED_APK_PATH" "$UNALIGNED_APK_PATH" "$COMPILED_RESOURCES_PATH" "$JAVA_DEXOUT_DIR" "$JAVA_OBJECT_DIR"; then
    echo "Unable to clean all leftover build artifacts.." >&2
    exit 1
fi
   
printf "%b" "Build finalized for $APP_NAME!\n"