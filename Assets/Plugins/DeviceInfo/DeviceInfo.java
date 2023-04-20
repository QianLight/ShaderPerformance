
package com.deviceinfo;
import java.io.*;
import android.os.Build;

public class DeviceInfo
{
   public static String GetCPU() {
        String cpuName = Build.HARDWARE;
        return cpuName;
    }
}