#!/bin/bash

# Versions and Download URLs
LATEST_AAPT2_VERSION="9.3.0-15703166"
AAPT2_DOWNLOAD_URL="https://dl.google.com/dl/android/maven2/com/android/tools/build/aapt2/$LATEST_AAPT2_VERSION/aapt2-$LATEST_AAPT2_VERSION-linux.jar"
ADB_DOWNLOAD_URL="https://dl.google.com/android/repository/platform-tools-latest-linux.zip"
APKSIG_DOWNLOAD_URL="https://dl.google.com/android/repository/build-tools_r35.0.1_linux.zip"

EPEL_DOWNLOAD_URL="https://dl.fedoraproject.org/pub/epel/epel-release-latest-10.noarch.rpm"

# File Paths
AAPT2_JAR="aapt2-$LATEST_AAPT2_VERSION-linux.jar"
AAPT2_BIN_PATH="/usr/bin/aapt2"
ADB_BIN_PATH="/usr/bin/adb"
ADB_ZIP="platform-tools.zip"
APKSIGN_ZIP="build-tools.zip"
APKSIGN_JAR_DIR="/opt/android-tools"
APKSIGN_JAR_PATH="$APKSIGN_JAR_DIR/apksigner.jar"
APKSIGN_BIN_PATH="/usr/bin/apksigner"
TEMP_INSTALL_DIR="$HOME/tmp"

# Installing the EPEL Repository which contains the Java SDK 21 maintained by OpenSDK.
sudo dnf install -y $EPEL_DOWNLOAD_URL || { echo "Failed to install the CentOS EPEL Repository" && exit 1; }

# Installing OpenJDK 21, Curl, Zip, and Unzip
sudo dnf install -y java-21-openjdk-devel curl zip unzip || { echo "Failed to install core dependencies" && exit 1; }

mkdir -p "$APKSIGN_JAR_DIR" || echo "APK Signer directory already exists."; 
# Creating the temporary installation directory for the downloaded files.
mkdir -p "$TEMP_INSTALL_DIR" || echo "Temporary installation directory already exists"; 
cd "$TEMP_INSTALL_DIR" || { echo "Failed to create a temporary installation directory" && exit 1; }

# Downloading the AAPT2 binary.
if [ ! -e "$AAPT2_BIN_PATH" ]; then
    curl -L $AAPT2_DOWNLOAD_URL --output "$AAPT2_JAR" || { echo "Failed to download AAPT2." && exit 1; }
    unzip "$AAPT2_JAR" aapt2 || { echo "Failed to extract the aapt2 archive." && exit 1; }
    sudo cp aapt2 "$AAPT2_BIN_PATH" || { echo "Failed to copy the extracted the AAPT2 binary." && exit 1; }
    echo "Installed AAPT2 to: $AAPT2_BIN_PATH"
fi

# Downloading the ADB binary.
if [ ! -e "$ADB_BIN_PATH" ]; then
    curl -L $ADB_DOWNLOAD_URL --output "$ADB_ZIP" || { echo "Failed to download ADB." && exit 1; }
    unzip -j "$ADB_ZIP" platform-tools/adb -d . || { echo "Failed to extract the ADB archive." && exit 1; }
    sudo cp adb "$ADB_BIN_PATH" || { echo "Failed to copy the extracted the ADB binary." && exit 1; }
    echo "Installed ADB to: $ADB_BIN_PATH"
fi

# Downloading APKSigner
if [ ! -e "$APKSIGN_JAR_PATH" ]; then
    sudo curl -L $APKSIG_DOWNLOAD_URL --output "$APKSIGN_ZIP" || { echo "Failed to install APKSigner." && exit 1; }
    unzip -j "$APKSIGN_ZIP" "*/lib/apksigner.jar" -d . || { echo "Failed to extract the APKSigner JAR." && exit 1; }
    sudo cp apksigner.jar "$APKSIGN_JAR_PATH" || { echo "Failed to copy the extracted the APKSigner JAR." && exit 1; } 
    echo "Downloaded APKSigner.jar to: $APKSIGN_JAR_PATH"

# Creating the wrapper script to execute the extracted apksigner.jar
# This mirrors the functionality of the apksigner script bundled with Android Studio, without the associated bloat (1.4GB+)
sudo tee "$APKSIGN_BIN_PATH" > /dev/null << EOF
#!/bin/bash
exec java -jar $APKSIGN_JAR_PATH "\$@"
EOF


# Providing the APKSigner binary with executable permissions.
sudo chmod +x "$APKSIGN_BIN_PATH" || { echo "Failed to provide the APKSigner wrapper with executable permissions" && exit 1; }

echo "Installed APKSigner to $APKSIGN_BIN_PATH"

rm -rf $TEMP_INSTALL_DIR || { echo "Failed to remove the temporary installation directory, please run: rm -rf $TEMP_INSTALL_DIR" && exit 1; }
fi

