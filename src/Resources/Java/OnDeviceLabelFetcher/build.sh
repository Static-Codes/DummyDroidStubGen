#!/usr/bin/env bash
set -e

if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
    echo "Invalid usage."
    echo "Expected: ./build.sh \"AppName\" \"Package/Directory/Structure" "packageName.apk"
    echo "Example: ./build.sh \"OnDeviceLabelFetcher\" \"com/staticcodes/odlf\" \"odlf.apk\""
    exit 1
fi


APP_NAME="$1" # The name of the class that contains the an extension of Activity.
PKG_DIR_STRUCTURE="$2" # The project's source directory structure
APK_NAME="$3" # The filename of the compiled APK
CLASS_DIR="$PKG_DIR_STRUCTURE/$APP_NAME"

mkdir -p obj dex_out

PROJECT_ROOT="$(pwd)"

echo -e "\n[3/16] -> Compiling Resources for $APP_NAME...\n"
aapt2 compile --dir "$PROJECT_ROOT/src/main/res" -o "$PROJECT_ROOT/compiled_resources.zip"

echo -e "\n[4/16] -> Linking XML Manifest using aapt2...\n"
aapt2 link --auto-add-overlay \
           --manifest src/main/AndroidManifest.xml \
           -I ../lib/android-21.jar \
           -R compiled_resources.zip \
           -o unaligned.apk

echo -e "\n[5/16] -> Compiling Java to ByteCode using javac...\n"
javac -d obj --release 8 -classpath ../lib/android-21.jar src/main/"$PKG_DIR_STRUCTURE"/*.java
   
echo -e "\n[6/16] -> Converting ByteCode to Android Dex...\n"
java -cp ../lib/r8lib.jar com.android.tools.r8.D8 --lib ../lib/android-21.jar --release --output dex_out/ obj/"$CLASS_DIR"*.class
  
echo -e "\n[7/16] -> Packaging Output Dex Classes...\n"
zip -uj unaligned.apk dex_out/classes.dex

echo -e "\n[8/16] -> Aligning APK...\n"
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
if ! zipalign -f -v 4 unaligned.apk aligned.apk; then
    echo -e "\nUnable to align compiled APK, please try again.\n"
    exit 1
fi


echo -e "\n[9/16] -> Confirming Alignment...\n"
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
if zipalign -c -v 4 aligned.apk; then
    echo -e "\nAlignment Confirmed!"
else
    echo -e "\nIncorrect Alignment Detected!"
    exit 1
fi

echo -e "\n[10/16] -> Removing Unaligned APK..."
rm -f unaligned.apk

echo -e "\n[11/16] -> Signing APK...\n"
if [ ! -f "debug.keystore" ]; then
    echo "File debug.keystore not found, generating now."
    keytool -genkey -v -keystore debug.keystore -storepass android -alias androiddebugkey \
            -keypass android -keyalg RSA -keysize 2048 -validity 10000 \
            -dname "CN=Android Debug,O=Android,C=US"
fi


if ! apksigner sign --ks debug.keystore --ks-pass pass:android --out "$APK_NAME" aligned.apk; then
    echo "Unable to sign the APK.."
    exit 1
fi

echo -e "\n[12/16] -> Removing leftover build artifacts...\n"
if ! rm -rf "$APK_NAME.idsig" aligned.apk unaligned.apk compiled_resources.zip dex_out obj; then
    echo "Unable to clean all leftover build artifacts.."
    exit 1
fi
   
echo -e "Build finalized for $APP_NAME!\n"