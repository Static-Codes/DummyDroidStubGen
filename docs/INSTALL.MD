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
sudo apt-get update && sudo apt install -y aapt2 adb apksigner java javac unzip zip
```

#### Fedora 
```bash
sudo dnf install -y android-tools java-21-openjdk-devel unzip zip
```

#### CentOS
Note: While CentOS is a Fedora Based Distro, its package repository is handled differently. As such, an installer script is required to simplify this process.

```bash
# Downloading curl, if not already installed.
sudo apt install curl -y
# Downloading and executing the installation shell script.
curl https://raw.githubusercontent.com/Static-Codes/DummyDroidStubGen/refs/heads/main/docs/installation/centos.sh | /bin/bash
```


