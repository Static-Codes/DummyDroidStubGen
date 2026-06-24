> [!CAUTION]
> ## [Keep Android Open](https://keepandroidopen.org/)
> ###  Your phone is about to stop being yours.
> Starting September 2026, a silent update, nonconsensually pushed by Google, will block every
> Android app whose developer hasn't registered with Google, signed their contract, paid up, and
> handed over government ID.
> **Every app and every device, worldwide, with no opt-out.**

DDSG (**DummyDroidStubGen**) is a simple, yet versatile linux app to generate *"Dummy"* packages (known as *"Stubs"*) that serve as entry points for another app.

## Use Cases
While there are a variety of use cases for **DDSG**, it was originally developed to reintroduce more granular gesture controls to [Lawnchair](https://github.com/LawnchairLauncher/lawnchair/). Specifically, a double-tap gesture to open a sandboxed application (One that is installed in the work profile through services like [Insular](https://github.com/proletarius101/Insular), [Island](https://github.com/oasisfeng/island), etc).

## Hardware Requirements
- **Android 5.0+ (Lollipop)**
- **A machine running linux that is atleast as powerful as a [Raspberry PI 3 Model B](https://www.raspberrypi.com/products/raspberry-pi-3-model-b/)**

## Features
- Completely De-Googled*
> DDSG contains a [feature](https://github.com/Static-Codes/DummyDroidStubGen/blob/main/src/Core/Types/Packaging/Package.cs#L81) that makes a request to `play.google.com` to prevent duplicate package generation, however, this is disabled by default.

- No Invasive Permissions

- Customizable Icons
> DDSG supports `.SVG`, `.XML` (DrawableVector), and `.WEBP` for the generated stub!

## Restrictions
DDSG has a set of blacklisted apps that cannot be used to create a stub. For more information on the blacklist, click [here](https://github.com/Static-Codes/DummyDroidStubGen/blob/main/FAQ.md#q-why-are-there-restrictions-on-the-apps-ddsg-can-open)


## FAQ
Click [here](./FAQ.md) to see the dedicated FAQ page

## Backstory
With the release of Lawnchair 15 Beta, the entire codebase was reworked.
This reworking affected more than just gesture controls, however, these other changes aren't relevant to **DDSG**.

For more information on the changes from Lawnchair 14 and Lawnchair 15, see:
> - [Beta 1](https://github.com/LawnchairLauncher/lawnchair/releases/tag/v15.0.0-beta1) 
> - [Beta 2](https://github.com/LawnchairLauncher/lawnchair/releases/tag/v15.0.0-beta2)
> - [Beta 2.1](https://github.com/LawnchairLauncher/lawnchair/releases/tag/v15.0.0-beta2.1) 
> - [Beta 3.0](https://github.com/LawnchairLauncher/lawnchair/releases/tag/v15.0.0-beta3.0) 
