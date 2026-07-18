#!/bin/bash

# Versions and Download URLs
LATEST_AAPT2_VERSION="9.3.0-15703166"
LATEST_APKSIGN_VERSION="9.3.0"
AAPT2_DOWNLOAD_URL="https://dl.google.com/dl/android/maven2/com/android/tools/build/aapt2/$LATEST_AAPT2_VERSION/aapt2-$LATEST_AAPT2_VERSION-linux.jar"
ADB_DOWNLOAD_URL="https://dl.google.com/android/repository/platform-tools-latest-linux.zip"
APKSIG_DOWNLOAD_URL="https://dl.google.com/android/maven2/com/android/tools/build/apksig/$LATEST_APKSIGN_VERSION/apksig-$LATEST_APKSIGN_VERSION.jar"
EPEL_DOWNLOAD_URL="https://dl.fedoraproject.org/pub/epel/epel-release-latest-10.noarch.rpm"

# File Paths
AAPT2_JAR="aapt2-$LATEST_AAPT2_VERSION-linux.jar"
AAPT2_BIN_PATH="/usr/bin/aapt2"
ADB_BIN_PATH="/usr/bin/adb"
ADB_ZIP="platform-tools.zip"
APKSIGN_JAR_PATH="/opt/android-tools/apksigner.jar"
APKSIGN_BIN_PATH="/usr/bin/apksigner"
TEMP_INSTALL_DIR="$HOME/tmp"

# Creating the android-tools dir (if not already on disk.)
sudo mkdir -p /opt/android-tools

# Installing the EPEL Repository which contains the Java SDK 21 maintained by OpenSDK.
sudo dnf install -y $EPEL_DOWNLOAD_URL

# Installing OpenJDK 21, Curl, Zip, and Unzip
sudo dnf install -y java-21-openjdk-devel curl zip unzip

# Creating the temporary installation directory for the downloaded files.
mkdir -p "$TEMP_INSTALL_DIR"
cd "$TEMP_INSTALL_DIR"

# Downloading the AAPT2 binary.
if [ ! -e "$AAPT2_BIN_PATH" ]; then
    curl -L $AAPT2_DOWNLOAD_URL --output "$AAPT2_JAR"
    unzip "$AAPT2_JAR" aapt2 && sudo cp aapt2 "$AAPT2_BIN_PATH"
fi

# Downloading the ADB binary.
if [ ! -e "$ADB_BIN_PATH" ]; then
    curl -L $ADB_DOWNLOAD_URL --output "$ADB_ZIP"
    unzip -j "$ADB_ZIP" platform-tools/adb -d .
    sudo cp adb "$ADB_BIN_PATH"
fi

# Downloading APKSign
sudo curl -L $APKSIG_DOWNLOAD_URL --output "$APKSIGN_JAR_PATH"

# Creating a wrapper around APKSign to execute the JAR (This replicates the official APKSigner binary's behavior)
sudo tee "$APKSIGN_BIN_PATH" > /dev/null << EOF
#!/bin/bash
# Specifying the classpath via -cp to avoid a main attribute not found error.
java -cp /opt/android-tools/apksigner.jar com.android.apksigner.ApkSignerTool "$@"
EOF

# Providing the APKSIGNER wrapper with executable permissions.
sudo chmod +x "$APKSIGN_BIN_PATH"
