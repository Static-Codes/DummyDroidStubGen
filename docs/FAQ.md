# DDSG FAQ!

##### Q: Can I use a rooted device?
You can, however, there are no added benefits from doing so, this message serves as a compatibility notice only.

#### Q: Why are there restrictions on the apps DDSG can open?
To clarify the only applications that are blacklisted are related to money, such as banking, crypto, etc. When I set out to create a solution for my issue, I quickly realized that this project could hypothetically serve as a basis for malware generation. While this is largely unfounded, I prefer to air on the side of caution. The blacklist is less of a limitation, and more of a suggestion, as it can easily be removed in a fork. 

#### Q: Why does DDSG only support Linux?
The simple answer is time. I didn't want to increase the development window by another 2+ months adding macOS and Windows support.

The good news is, you can still run DDSG in a live linux environment without uninstalling Windows or macOS.
    
<details>
<summary>Windows Guide</summary>

1. Coming Soon

</details>

<details>
<summary>macOS Guide</summary>

1. **Note**: This method requires atleast 40GB of free disk space and atleast 6GB of free RAM.

1. Install [Brew](https://brew.sh/) (An open source Package Manager)

2. Run the following commands to install and launch [Multipass](https://github.com/canonical/multipass)
    ```bash
    # Credits to Ryan Pazrin for the first 3 steps of this installation guide.
    
    # Pulled from: https://dev.to/ryfazrin/how-to-run-ubuntu-on-macos-like-wsl-wsl-style-experience-4cd4

    # Installing Multipass
    brew install --cask multipass

    # Launching an instance of ubuntu using 4GB of RAM and 20GB of disk space
    multipass launch --name ubuntu --mem 4G --disk 20G

    # Opening the shell for the instance above
    multipass shell ubuntu

    # Creating a directory inside the instance to hold files.
    mkdir -p /home/ubuntu/ddsg-data

    # Returning to the host machine.
    exit 

    # Mounting the User Download Directory to the guest instance.
    multipass mount ~/Downloads ubuntu:/home/ubuntu/ddsg-data
    ```

    Once these commands have been executed, you should have a configured Ubuntu environment. You should also see the files in your Downloads folder inside the Ubuntu environment at the path: `/home/ubuntu/ddsg-data`. This is where you will access any image files you wish to use for the Stub's Icon.

    Simply download an icon image to your Mac and place it in the Downloads directory, once this has been done, DDSG will be able to see these files.

</details>