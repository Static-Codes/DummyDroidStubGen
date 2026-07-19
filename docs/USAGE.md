# DDSG Usage Guide

The following document contains in-depth information relating to the usage of DDSG.

Before continuing, please ensure you've completed the installation process, it can be found [here](./INSTALL.md).

## Connection Information

<details open>
<summary>Connecting over USB</summary>

#### USB Cable and Port information.

- DDSG supports connections over both USB-A and USB-C ports.

- DDSG supports USB 2.0, 3.X, and 4.X ports.

- When connecting, it is recommended to use a USB 3+ port due to the increased bus speed and bandwidth size, however, USB 2 ports are also supported for legacy purposes (albeit at a lower speed)

#### Configuring your Device

- Ensure developer options are enabled alongside USB Debugging.
    - [Enabling Developer Options](https://developer.android.com/studio/debug/dev-options#enable)


    - [Enabling USB Debugging](https://developer.android.com/studio/debug/dev-options#Enable-debugging)

#### Connecting your Device
1. Plug your charging cable into a port on your system's motherboard. It is highly advised to avoid using ports on the Front Panel (for desktops) or USB hubs (even powered ones) as they share the same bandwidth across all their ports.

2. Plug the other end into your Android Device.

3. Wait for the notification on your device that "USB Debugging is enabled/connected".

4. Proceed to the `Usage Information` section below.

</details>

---


<details open>
<summary>Connecting over WIFI</summary>

#### Configuring your Device

- Ensure developer options are enabled alongside USB Debugging.
    - [Enabling Developer Options](https://developer.android.com/studio/debug/dev-options#enable)

    - [Enabling Wireless Debugging](https://developer.android.com/studio/debug/dev-options#Enable-debugging)

#### Connecting your Device

1. Ensure your computer and phone are on the same WiFi network.

2. Complete the first four steps from the following [article](https://developer.android.com/tools/adb#connect-to-a-device-over-wi-fi) on Android's official website.

3. Remaining on the pairing screen and proceed to the `Usage Information` section below.


</details>


## Usage Information

<details open>
<summary>Click to see detailed usage information</summary>

1. Start DDSG.

2. Confirm the device connected is the one you wish to use for the Stub Generation.

3. Select one of the two methods DDSG provides to retrieve installed apps (packages) on the connected device.

4. Wait for the retrieval process to complete, this usually takes 10 seconds, but can take up to a minute on older or lower resource systems.

5. Select a app to use as the target for the compiled stub.
    - This is the application that will be opened when you click on the stub.

    - You can also configure this stub to be opened via a gesture control, depending on your launcher.

6. Press O on your keyboard, you will be prompted to select an image that will be used as the icon for the compiled stub.
    - As outlined in the repository's [README](../README.md) this process supports the following image types.

        * Scalable Vector Graphics (SVG)
        * Android VectorDrawable (XML)
        * Web Picture (WEBP)
    
7. Select an Icon Image

8. Select a shell for compilation
    - Unless you have a reason to change this, it is highly recommended to select `SH` from this menu.

    - Once you select a shell, the stub will be compiled.

9. Select your desired installation workflow.
    - If you wish to view the source code before installing, select `Yes (Requires manual installation)`. This will open the stub's source code in your default file browser, display an installation command in the terminal, then close DDSG. 

    - Otherwise select `No, continue with the installation (Recommended)` for DDSG to assist you in the installation process.