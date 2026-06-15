// Implementation Inspiration:
// https://github.com/butzist/ActivityLauncher/blob/master/app/src/main/java/de/szalkowski/activitylauncher/services/PackageListService.kt
// Replaced Kotlin specific calls like runCatching with Java's standard try/catch blocks.
// Removed most of the existing logic as only the packageName and appLabel are required.
// Supports Android 5.0+ (Lollipop) (SDK 21+)
package com.staticcodes.odlf;

import android.app.Activity;
import android.content.Context;
import android.content.pm.LauncherActivityInfo;
import android.content.pm.LauncherApps;
import android.os.Bundle;
import android.os.UserManager;
import android.os.UserHandle;
import android.util.Log;

import java.io.IOException;
import java.io.OutputStreamWriter;
import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;

public class OnDeviceLabelFetcher extends Activity
{
    private LauncherApps launcherApps;
    private UserManager userManager;

    @Override
    protected void onCreate(Bundle b) {
        super.onCreate(b);
        
        this.launcherApps = (LauncherApps) getSystemService(Context.LAUNCHER_APPS_SERVICE);
        this.userManager = (UserManager) getSystemService(Context.USER_SERVICE);
        
        runActivity();
        finish();
    }

    private void runActivity() 
    {
        List<AppInfo> appInfos = getPackages();
        
        if (appInfos.isEmpty()) {
            Log.e("Exception", "Failed to write to list.txt: appsInfo is empty");
            return;
        }

        StringBuilder builder = new StringBuilder();

        for (AppInfo appInfo : appInfos) 
        {
            builder.append(appInfo.packageName)
                   .append("|")
                   .append(appInfo.labelName)
                   .append("\n");
        }

        tryDeleteExistingFile(this);

        writeToFile(builder.toString(), this);
    }

    private void tryDeleteExistingFile(Context context) 
    {
        try 
        {
            File file = new File(context.getFilesDir(), "list.txt");
            if (file.exists()) {
                file.delete();
            }
        }

        catch (Exception e) {
            Log.e("Exception", "Failed to delete old copy of list.txt: " + e.toString());
        } 
    }

    private void writeToFile(String data, Context context) 
    {
        try 
        {
            OutputStreamWriter outputStreamWriter = new OutputStreamWriter(
                context.openFileOutput("list.txt", Context.MODE_PRIVATE)
            );

            outputStreamWriter.write(data);
            outputStreamWriter.close();
        }

        catch (IOException e) {
            Log.e("Exception", "Failed to write to list.txt: " + e.toString());
        } 
    }


    public List<AppInfo> getPackages() 
    {
        // While null can easily be passed to getActivityList, this is easier to audit at a glance.
        String packageName = null;

        List<AppInfo> foundApps = new ArrayList<>();
        List<UserHandle> profiles = userManager.getUserProfiles();

        for (UserHandle profile : profiles) 
        {
            List<LauncherActivityInfo> apps = launcherApps.getActivityList(packageName, profile);
            for (LauncherActivityInfo info : apps) 
            {
                foundApps.add
                (
                    new AppInfo(
                        info.getComponentName().getPackageName(),
                        info.getLabel().toString()
                    )
                );
            }
        }
        
        // Performing a simple alphabetical sort using the package label names.
        //
        // If the same package is installed across two profiles:
        // The copy of the package that is installed in UserHandle[0] (Personal Profile) is used.
        // This will ignore the Sandboxed (Work Profile) installation, this is not a bug but a feature.
        Collections.sort(foundApps, new Comparator<AppInfo>() 
        {
            @Override
            public int compare(AppInfo info1, AppInfo info2) {
                return info1.labelName
                            .toLowerCase()
                            .compareTo(info2.labelName.toLowerCase());
            }
        });

        return foundApps;
    }

    public static class AppInfo 
    {
        public final String packageName;
        public final String labelName;

        public AppInfo(String packageName, String labelName) {
            this.packageName = packageName;
            this.labelName = labelName;
        }
    }
}