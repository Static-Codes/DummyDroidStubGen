# DDSG Install Guide

### Dependencies Used
- aapt2
- adb
- apksigner
- java (8+)
- javac
- unzip
- zip


### Dependency Installation
#### Debian/Ubuntu
```bash
sudo apt-get update && sudo apt install -y aapt2 adb apksigner openjdk-21-jdk unzip zip
```

#### Fedora 
```bash
sudo dnf install -y android-tools java-21-openjdk-devel unzip zip
```

#### CentOS
Note: While CentOS is a Fedora Based Distro, its package repository is handled differently. As such, an installer script is required to simplify this process.

```bash
# Downloading curl, if not already installed.
sudo dnf install curl -y
# Downloading and executing the installation shell script.
curl https://raw.githubusercontent.com/Static-Codes/DummyDroidStubGen/refs/heads/main/docs/installation/centos.sh | /bin/bash
```

#### FreeBSD

```bash
sudo pkg install -y android-tools openjdk21 zip unzip
```

#### Arch 
```bash
sudo pacman -Syu android-tools jdk21-openjdk unzip zip
```

#### SUSE
```bash
sudo zypper install -y android-tools java-21-openjdk-devel unzip zip
```
