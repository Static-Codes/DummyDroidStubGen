# DummyDroidStubGen
DummyDroidStubGen (DDSG) is a simple, yet versatile linux application to generate "Dummy" applications (known as "Stubs") that serve as entry points for another application.

While there are a variety of use cases for DDSG, it was originally developed to reintroduce more granular gesture controls to [Lawnchair](https://github.com/LawnchairLauncher/lawnchair/). Specifically, a double-tap gesture to open a sandboxed application (One that is installed in the work profile through services like [Insular](https://github.com/proletarius101/Insular), [Island](https://github.com/oasisfeng/island), etc).

With the release of Lawnchair 15 Beta, the entire codebase was reworked.

This reworking affected more than just gesture controls, however, these other changes aren't relevant to DDSG.

For more information on the changes from Lawnchair 14 and Lawnchair 15, see [here](https://lawnchair.app/blog/lawnchair-15-beta-1/). 

# Compatibility
#### OS Version: Android 14 or higher
#### SDK Version: SDK 34 or higher
#### Root Support: No, see the first question on the [FAQ](./FAQ.md)


# FAQ
Click [here](./FAQ.md) to see the dedicated FAQ page

# TODO
- Add Root Check and exit

- Add SDK version and Android Version detection.

- Further implement the blacklist and graylist within both the 
stub generator and stubs themselves.

- Check for insular, island, etc + If work profile is configured.

- Add checks so a user cannot use the stub to launch an application that is not already installed on the system.

- Add instructions for usage, from see [here](https://developer.android.com/tools/adb)

