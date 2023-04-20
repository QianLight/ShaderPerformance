/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
#if UNITY_EDITOR_WIN
using System.Runtime.InteropServices;
#endif

namespace Zeus.Build
{
    public static class FileTool
    {
#if UNITY_EDITOR_WIN
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool CreateHardLink(string fileName, string existingFileName, IntPtr securityAttributes);
#endif
    
        public static void TryHardLinkCopy(string originalFile, string linkedFile, bool overwrite = false)
        {
#if UNITY_EDITOR_WIN
            if (File.Exists(linkedFile))
            {
                if (overwrite)
                {
                    File.Delete(linkedFile);
                }
                else
                {
                    return;
                }
            }
            try
            {
                CreateHardLink(linkedFile, originalFile, IntPtr.Zero);
            }
            catch (Exception)
            {
                File.Copy(originalFile, linkedFile, overwrite);
            }
#else
            File.Copy(originalFile, linkedFile, overwrite);
#endif
        }
    }
}