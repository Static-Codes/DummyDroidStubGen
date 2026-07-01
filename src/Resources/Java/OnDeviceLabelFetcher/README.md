# ODLF

## Backstory
When developing DDSG, I ran into a significant number of hurdles, due to restrictions in newer versions of Android. With older versions App Names (Labels) were often directly accessible through [`dumpsys`](https://stackoverflow.com/questions/16650765/get-application-name-label-via-adb-shell-or-terminal) this is no longer reliably the case. I first tried using `app_process`, much like [`Shizuku`](https://github.com/RikkaApps/Shizuku) does, however, this was a deadend, as the app label names could not be reliably decoded from the XML manifest files. After more trial and error, I remembered `ActivityLauncher` provided this functionality (on-device), so I then decided to look further into it. After realizing it was written in Kotlin, I took the relevant [file](https://github.com/butzist/ActivityLauncher/blob/master/app/src/main/java/de/szalkowski/activitylauncher/services/PackageListService.kt) and converted it to Vanilla Java.

## How It Works
1. OnDeviceLabelFetcher is compiled from source
2. It is sideloaded onto the current device over [ADB](https://developer.android.com/tools/adb)
3. OnDeviceLabelFetcher returns a list of package names and their associated app labels (names)
4. OnDeviceLabelFetcher is removed from your device.
5. You are then prompted to select an application you wish to bind to the compiled stub.