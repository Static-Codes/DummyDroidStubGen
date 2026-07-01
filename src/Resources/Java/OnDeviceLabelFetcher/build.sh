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
CLASS_DIR="$PKG_DIR_STRUCTURE/$APP_NAME"
# The absolute path to the working directory for this script.
WORKING_DIR="$APP_DATA_DIR/Resources"
# The absolute path to the directory containing Java Runtime Executables for the stub.
JAVA_LIBS_DIR="$WORKING_DIR/libs"
# The absolute path to the directory containing ODLF's source.
APP_BUILD_DIR="$WORKING_DIR/$APP_NAME"
# The absolute path to the Java source for the compiled stub.
JAVA_SOURCE_DIR="$APP_BUILD_DIR/src/main"
# The Android 5.0 Lollipop (API 21) Java Runtime
ANDROID_JAR="$JAVA_LIBS_DIR/android-21.jar"
# The D8 (R8) runtime to convert Java ByteCode to Android Dex.
D8_JAR="$JAVA_LIBS_DIR/r8lib.jar"


if [ ! -e "$ANDROID_JAR" ] || [ ! -e "$D8_JAR" ]; then
    echo "One or more of the required Java Runtime Executables were missing from: '$JAVA_LIBS_DIR'" >&2
    exit 1
fi

cd $APP_BUILD_DIR

mkdir -p obj dex_out

printf "%b" "\n[3/16] -> Compiling Resources for $APP_NAME...\n"

aapt2 compile --dir "$JAVA_SOURCE_DIR/res" -o "compiled_resources.zip"

printf "%b" "\n[4/16] -> Linking XML Manifest using aapt2...\n"
aapt2 link --auto-add-overlay \
           --manifest $JAVA_SOURCE_DIR/AndroidManifest.xml \
           -I "$ANDROID_JAR" \
           -R "compiled_resources.zip" \
           -o "unaligned.apk"

printf "%b" "\n[5/16] -> Compiling Java to ByteCode using javac...\n"
javac -d obj --release 8 -classpath $ANDROID_JAR $JAVA_SOURCE_DIR/"$PKG_DIR_STRUCTURE"/*.java
   
printf "%b" "\n[6/16] -> Converting ByteCode to Android Dex...\n"
java -cp $D8_JAR com.android.tools.r8.D8 --lib $ANDROID_JAR --release --output dex_out/ obj/$CLASS_DIR*.class
  
printf "%b" "\n[7/16] -> Packaging Output Dex Classes...\n"
zip -uj unaligned.apk dex_out/classes.dex

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
# if ! zipalign -P 16 -v 4 unaligned.apk aligned.apk
if ! zipalign -f -v 4 "unaligned.apk" "aligned.apk"; then
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
# if zipalign -c -P 16 -v 4 aligned.apk; then
if zipalign -c -v 4 "aligned.apk"; then
    printf "%b" "\nAlignment Confirmed!"
else
    printf "%b" "\nIncorrect Alignment Detected!"
    exit 1
fi

printf "%b" "\n[10/16] -> Removing Unaligned APK..."
rm -f $APP_BUILD_DIR/unaligned.apk

printf "%b" "\n[11/16] -> Signing APK...\n"
if [ ! -f "$APP_BUILD_DIR/debug.keystore" ]; then
    echo "File debug.keystore not found, generating now." >&2
    keytool -genkey -v -keystore "$APP_BUILD_DIR/debug.keystore" -storepass android -alias androiddebugkey \
            -keypass android -keyalg RSA -keysize 2048 -validity 10000 \
            -dname "CN=Android Debug,O=Android,C=US"
fi


if ! apksigner sign --ks "$APP_BUILD_DIR/debug.keystore" --ks-pass pass:android --in $APP_BUILD_DIR/aligned.apk --out "$APK_NAME"; then
    echo "Unable to sign the APK.." >&2
    exit 1
fi

printf "%b" "\n[12/16] -> Removing leftover build artifacts...\n"
if ! cd $APP_BUILD_DIR && rm -rf "$APK_NAME.idsig" aligned.apk unaligned.apk compiled_resources.zip dex_out obj; then
    echo "Unable to clean all leftover build artifacts.." >&2
    exit 1
fi
   
printf "%b" "Build finalized for $APP_NAME!\n"