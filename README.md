# DummyDroidStubGen
DummyDroidStubGen (**DDSG**) is a simple, yet versatile linux application to generate *"Dummy"* applications (known as *"Stubs"*) that serve as entry points for another application.

While there are a variety of use cases for **DDSG**, it was originally developed to reintroduce more granular gesture controls to [Lawnchair](https://github.com/LawnchairLauncher/lawnchair/). Specifically, a double-tap gesture to open a sandboxed application (One that is installed in the work profile through services like [Insular](https://github.com/proletarius101/Insular), [Island](https://github.com/oasisfeng/island), etc).

With the release of Lawnchair 15 Beta, the entire codebase was reworked.

This reworking affected more than just gesture controls, however, these other changes aren't relevant to **DDSG**.

For more information on the changes from Lawnchair 14 and Lawnchair 15, see:
- [Beta 1](https://github.com/LawnchairLauncher/lawnchair/releases/tag/v15.0.0-beta1) 
- [Beta 2](https://github.com/LawnchairLauncher/lawnchair/releases/tag/v15.0.0-beta2)
- [Beta 2.1](https://github.com/LawnchairLauncher/lawnchair/releases/tag/v15.0.0-beta2.1) 
- [Beta 3.0](https://github.com/LawnchairLauncher/lawnchair/releases/tag/v15.0.0-beta3.0) 


## Compatibility
#### OS Version: Android 5 or higher
#### SDK Version: SDK 21 or higher
#### Root Support: Yes, but there's no added benefits


## FAQ
Click [here](./FAQ.md) to see the dedicated FAQ page

## TODO

- Further implement the blacklist and graylist within both the 
stub generator and stubs themselves.

- Add checks so a user cannot use the stub to launch an application that is not already installed on the system.

- Add instructions for usage, from see [here](https://developer.android.com/tools/adb)


