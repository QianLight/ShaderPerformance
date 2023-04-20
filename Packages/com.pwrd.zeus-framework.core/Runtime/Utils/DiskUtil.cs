/*

Class: DiskUtils.cs
==============================================
Last update: 2016-05-12  (by Dikra)
==============================================

Copyright (c) 2016  M Dikra Prasetya

 * MIT LICENSE
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace Zeus.Framework
{

    public class DiskUtils
    {

#if UNITY_STANDALONE || UNITY_EDITOR

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
		[DllImport ("diskutils")]
		private static extern int getAvailableDiskSpace ();

		[DllImport ("diskutils")]
		private static extern int getTotalDiskSpace ();

		[DllImport ("diskutils")]
		private static extern int getBusyDiskSpace ();

		/// <summary>
		/// Checks the available space.
		/// </summary>
		/// <returns>The available space in MB.</returns>
		public static int CheckAvailableSpace ()
		{
			return DiskUtils.getAvailableDiskSpace ();
		}
        
        /// <summary>
		/// Checks the available space.
		/// </summary>
		/// <returns>The available space in Bytes.</returns>
		public static long CheckAvailableSpaceBytes()
        {
            return DiskUtils.getAvailableDiskSpace() * 1024L * 1024L;
        }

		/// <summary>
		/// Checks the total space.
		/// </summary>
		/// <returns>The total space in MB.</returns>
		public static int CheckTotalSpace ()
		{
			return DiskUtils.getTotalDiskSpace ();
		}

		/// <summary>
		/// Checks the busy space.
		/// </summary>
		/// <returns>The busy space in MB.</returns>
		public static int CheckBusySpace ()
		{
			return DiskUtils.getBusyDiskSpace ();
		}
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        [DllImport("DiskUtilsWinAPI")]
        private static extern int getAvailableDiskSpace(StringBuilder drive);

        [DllImport("DiskUtilsWinAPI")]
        private static extern int getTotalDiskSpace(StringBuilder drive);

        [DllImport("DiskUtilsWinAPI")]
        private static extern int getBusyDiskSpace(StringBuilder drive);

        private const string DEFAULT_DRIVE = "C:/";

        /// <summary>
	    /// Checks the available space.
	    /// </summary>
	    /// <returns>The available spaces in MB.</returns>
	    /// <param name="diskName">Disk name. For example, "C:/"</param>
		public static int CheckAvailableSpace(string drive = DEFAULT_DRIVE)
        {
            try
            {
                ulong freeBytesAvail;
                ulong totalNumOfBytes;
                ulong totalNumOfFreeBytes;
                if (GetDiskFreeSpaceEx(drive, out freeBytesAvail, out totalNumOfBytes, out totalNumOfFreeBytes))
                {
                    return (int)(freeBytesAvail / 1024L / 1024L);
                }
                return DiskUtils.getAvailableDiskSpace(new StringBuilder(drive));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("CheckAvailableSpace:" + e.ToString());
                return 100 * 1024;
            }
        }

        /// <summary>
        /// Checks the available space.
        /// </summary>
        /// <returns>The available spaces in Bytes.</returns>
        /// <param name="diskName">Disk name. For example, "C:/"</param>
        public static long CheckAvailableSpaceBytes(string drive = DEFAULT_DRIVE)
        {
            try
            {
                ulong freeBytesAvail;
                ulong totalNumOfBytes;
                ulong totalNumOfFreeBytes;

                if (GetDiskFreeSpaceEx(drive, out freeBytesAvail, out totalNumOfBytes, out totalNumOfFreeBytes))
                {
                    if (freeBytesAvail > long.MaxValue)
                    {
                        return long.MaxValue;
                    }
                    return (long)freeBytesAvail;
                }
                return DiskUtils.getAvailableDiskSpace(new StringBuilder(drive)) * 1024L * 1024L;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("CheckAvailableSpaceBytes:" + e.ToString());
                return 100L * 1024L * 1024L * 1024L;
            }
        }

        /// <summary>
	    /// Checks the total space.
	    /// </summary>
	    /// <returns>The total space in MB.</returns>
	    /// <param name="diskName">Disk name. For example, "C:/"</param>
        public static int CheckTotalSpace(string drive = DEFAULT_DRIVE)
        {
            try
            {
                ulong freeBytesAvail;
                ulong totalNumOfBytes;
                ulong totalNumOfFreeBytes;

                if (GetDiskFreeSpaceEx(drive, out freeBytesAvail, out totalNumOfBytes, out totalNumOfFreeBytes))
                {
                    return (int)(totalNumOfBytes / 1024L / 1024L);
                }
                return DiskUtils.getTotalDiskSpace(new StringBuilder(drive));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("CheckTotalSpace:" + e.ToString());
                return 100 * 1024;
            }
        }

        /// <summary>
	    /// Checks the busy space.
	    /// </summary>
	    /// <returns>The busy space in MB.</returns>
	    /// <param name="diskName">Disk name. For example, "C:/"</param>
        public static int CheckBusySpace(string drive = DEFAULT_DRIVE)
        {
            try
            {
                ulong freeBytesAvail;
                ulong totalNumOfBytes;
                ulong totalNumOfFreeBytes;

                if (GetDiskFreeSpaceEx(drive, out freeBytesAvail, out totalNumOfBytes, out totalNumOfFreeBytes))
                {
                    return (int)((totalNumOfBytes - freeBytesAvail) / 1024L / 1024L);
                }
                return DiskUtils.getBusyDiskSpace(new StringBuilder(drive));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("CheckBusySpace:" + e.ToString());
                return 100 * 1024;
            }
        }

        public static string[] GetDriveNames()
        {
            return Directory.GetLogicalDrives();
        }
#else
        private const long MEGA_BYTE = 1048576;
	    private const string DEFAULT_DRIVE = "/";
        
        private static DriveInfo GetDrive(string driveName)
        {
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == driveName)
                {
                    return drive;
                }
            }
            return null;
        }

	    /// Checks the available space.
	    /// </summary>
	    /// <returns>The available space in MB.</returns>
	    public static int CheckAvailableSpace(){
		    DriveInfo drive = GetDrive (DEFAULT_DRIVE);
		    if (drive == null)
			    return -1;
		    return int.Parse((drive.AvailableFreeSpace / MEGA_BYTE).ToString());
	    }

        /// <summary>
	    /// Checks the available space.
	    /// </summary>
	    /// <returns>The available space in Bytes.</returns>
	    public static long CheckAvailableSpaceBytes()
        {
            DriveInfo drive = GetDrive(DEFAULT_DRIVE);
            if (drive == null)
                return -1;
            return drive.AvailableFreeSpace;
        }

	    /// <summary>
	    /// Checks the total space.
	    /// </summary>
	    /// <returns>The total space in MB.</returns>
	    public static int CheckTotalSpace(){
		    DriveInfo drive = GetDrive (DEFAULT_DRIVE);
		    if (drive == null)
			    return -1;
		    return int.Parse ((drive.TotalSize / MEGA_BYTE).ToString());
	    }

	    /// <summary>
	    /// Checks the busy space.
	    /// </summary>
	    /// <returns>The busy space in MB.</returns>
	    public static int CheckBusySpace(){
		    DriveInfo drive = GetDrive (DEFAULT_DRIVE);
		    if (drive == null)
			    return -1;

		    return int.Parse (((drive.TotalSize - drive.AvailableFreeSpace) / MEGA_BYTE).ToString());
	    }

#endif


#elif UNITY_ANDROID

        /// <summary>
        /// Checks the available space.
        /// </summary>
        /// <returns>The available space in MB.</returns>
        /// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
        public static int CheckAvailableSpace(bool isExternalStorage = true)
        {
            using (AndroidJavaClass dataUtils = new AndroidJavaClass("com.dikra.diskutils.DiskUtils"))
            {
                return dataUtils.CallStatic<int>("availableSpace", isExternalStorage);
            }
        }

        /// <summary>
        /// Checks the available space.
        /// </summary>
        /// <returns>The available space in Bytes.</returns>
        /// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
        public static long CheckAvailableSpaceBytes(bool isExternalStorage = true)
        {
            using (AndroidJavaClass dataUtils = new AndroidJavaClass("com.dikra.diskutils.DiskUtils"))
            {
                return dataUtils.CallStatic<int>("availableSpace", isExternalStorage) * 1024L * 1024L;    
            }
        }

        /// <summary>
        /// Checks the total space.
        /// </summary>
        /// <returns>The total space in MB.</returns>
        /// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
        public static int CheckTotalSpace(bool isExternalStorage = true)
        {
            using (AndroidJavaClass dataUtils = new AndroidJavaClass("com.dikra.diskutils.DiskUtils"))
            {
                return dataUtils.CallStatic<int>("totalSpace", isExternalStorage);    
            }            
        }

        /// <summary>
        /// Checks the busy space.
        /// </summary>
        /// <returns>The busy space in MB.</returns>
        /// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
        public static int CheckBusySpace(bool isExternalStorage = true)
        {
            using (AndroidJavaClass dataUtils = new AndroidJavaClass("com.dikra.diskutils.DiskUtils"))
            {
                return dataUtils.CallStatic<int>("busySpace", isExternalStorage);    
            }
        }


#elif UNITY_IOS

        [DllImport("__Internal")]
        private static extern ulong getAvailableDiskSpace();
        [DllImport("__Internal")]
        private static extern ulong getTotalDiskSpace();
        [DllImport("__Internal")]
        private static extern ulong getBusyDiskSpace();

        /// <summary>
        /// Checks the available space.
        /// </summary>
        /// <returns>The available space in MB.</returns>
        public static int CheckAvailableSpace()
        {
            ulong ret = DiskUtils.getAvailableDiskSpace();
            return int.Parse(ret.ToString());
        }

        /// <summary>
        /// Checks the available space.
        /// </summary>
        /// <returns>The available space in Bytes.</returns>
        public static long CheckAvailableSpaceBytes()
        {
            ulong ret = DiskUtils.getAvailableDiskSpace();
            return (long)(ret * 1024 * 1024);
        }

        /// <summary>
        /// Checks the total space.
        /// </summary>
        /// <returns>The total space in MB.</returns>
        public static int CheckTotalSpace()
        {
            ulong ret = DiskUtils.getTotalDiskSpace();
            return int.Parse(ret.ToString());
        }

        /// <summary>
        /// Checks the busy space.
        /// </summary>
        /// <returns>The busy space in MB.</returns>
        public static int CheckBusySpace()
        {
            ulong ret = DiskUtils.getBusyDiskSpace();
            return int.Parse(ret.ToString());
        }

#endif

        const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
        const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);
        /// <summary>
        /// 是否存储空间不足异常
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsHardDiskFull(IOException e)
        {
            //存储空间不足
            if (e.HResult == HR_ERROR_HANDLE_DISK_FULL || e.HResult == HR_ERROR_DISK_FULL)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}