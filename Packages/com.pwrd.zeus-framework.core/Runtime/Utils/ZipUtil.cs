/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using ICSharpCode.ZeusSharpZipLib.Zip;
using System;
using System.IO;

namespace Zeus.Core
{
    public static class ZipUtil
    {
        public static void ZipFileFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
        {
            ZipFileFromDirectoryExceptInvalidFileTypes(sourceDirectoryName, destinationArchiveFileName);
        }

        public static void ZipFileFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, UseZip64 useZip64 = UseZip64.Dynamic)
        {
            ZipFileFromDirectoryExceptInvalidFileTypes(sourceDirectoryName, destinationArchiveFileName, null, useZip64);
        }

        public static void ZipFileFromDirectoryExceptInvalidFileTypes(string sourceDirectoryName, string destinationArchiveFileName, string[] invalidFileTypes = null, UseZip64 useZip64 = UseZip64.Dynamic)
        {
            destinationArchiveFileName = UnityEngine.Application.dataPath.Replace("Assets", "SubPackage.subpackage");
            using (var fs = File.Open(destinationArchiveFileName, FileMode.Create))
            {
                using (ZipOutputStream outputStream = new ZipOutputStream(fs))
                {
                    outputStream.UseZip64 = useZip64;
                    ZipFolder(sourceDirectoryName, sourceDirectoryName, outputStream, invalidFileTypes);
                }
            }
        }

        private static void ZipFolder(string RootFolder, string CurrentFolder, ZipOutputStream zStream, string[] invalidFileTypes = null)
        {
            string[] SubFolders = Directory.GetDirectories(CurrentFolder);

            foreach (string Folder in SubFolders)
                ZipFolder(RootFolder, Folder, zStream, invalidFileTypes);

            string relativePath = CurrentFolder.Substring(RootFolder.Length) + "/";

            if (relativePath.Length > 1)
            {
                ZipEntry dirEntry;

                dirEntry = new ZipEntry(relativePath);
                dirEntry.DateTime = DateTime.Now;
            }

            foreach (string file in Directory.GetFiles(CurrentFolder))
            {
                if (IsFileValid(file, invalidFileTypes))
                {
                    AddFileToZip(zStream, relativePath, file);
                }
            }
        }

        private static bool IsFileValid(string file, string[] invalidFileTypes)
        {
            if (invalidFileTypes == null)
                return true;
            foreach (string fileType in invalidFileTypes)
            {
                int lastIndex = file.LastIndexOf(fileType);
                if (fileType.Length > 0 && lastIndex + fileType.Length == file.Length)
                {
                    return false;
                }
            }
            return true;
        }
        private static void AddFileToZip(ZipOutputStream zStream, string relativePath, string file)
        {
            byte[] buffer = new byte[4096];
            string fileRelativePath = (relativePath.Length > 1 ? relativePath : string.Empty) + Path.GetFileName(file);
            ZipEntry entry = new ZipEntry(fileRelativePath);
            entry.CompressionMethod = CompressionMethod.Stored;

            zStream.PutNextEntry(entry);

            using (FileStream fs = File.OpenRead(file))
            {
                int sourceBytes;

                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zStream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="archiveFileName">压缩包文件</param>
        /// <param name="destPath">解压到</param>
        /// <param name="entryFullName">目标文件 为null时全部解压</param>
        public static void ExtractFileFormZipArchive(string archiveFileName, string destPath, string entryFullName = null)
        {
            ZipEntry[] targetEntryList;
            using (ZipFile archive = new ZipFile(archiveFileName))
            {
                if (entryFullName == null)
                {
                    targetEntryList = new ZipEntry[archive.Count];
                    for (int i = 0; i < archive.Count; i++)
                    {
                        targetEntryList[i] = archive[i];
                    }
                }
                else if (archive.GetEntry(entryFullName) != null)
                {
                    targetEntryList = new ZipEntry[1] { archive.GetEntry(entryFullName) };
                }
                else
                {
                    UnityEngine.Debug.LogError("目标文件: " + entryFullName + " 不存在");
                    return;
                }

                foreach (var targetEntry in targetEntryList)
                {
                    if (targetEntry == null)
                        continue;
                    using (Stream readStream = archive.GetInputStream(targetEntry))
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(destPath + "/" + targetEntry)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destPath + "/" + targetEntry));
                        }
                        using (Stream writeStream = File.Create(destPath + "/" + targetEntry))
                        {
                            byte[] buffer = new byte[1024];
                            while (true)
                            {
                                int readCount = readStream.Read(buffer, 0, buffer.Length);
                                if (readCount <= 0)
                                {
                                    break;
                                }
                                writeStream.Write(buffer, 0, readCount);
                            }
                            writeStream.Flush();
                        }
                    }
                }
            }
        }
    }
}
