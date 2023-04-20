/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Zeus.Core
{
	public class FileUtil
	{
		public static Encoding GetFileEncodingType(FileStream fileStream)
		{
			BinaryReader fileReader = new BinaryReader(fileStream);

			byte[] firstTwoBytes = fileReader.ReadBytes(2);

			if (firstTwoBytes[0] > 0xEF)
			{
				if (firstTwoBytes[0] == 0xEF && firstTwoBytes[1] == 0xBB)
				{
					return Encoding.UTF8;
				}
				else if (firstTwoBytes[0] == 0xFE && firstTwoBytes[1] == 0xFF)
				{
					return Encoding.BigEndianUnicode;
				}
				else if (firstTwoBytes[0] == 0xFF && firstTwoBytes[1] == 0xFE)
				{
					return Encoding.Unicode;
				}
				else
				{
					return Encoding.Default;
				}
			}
			else
			{
				return Encoding.Default;
			}
		}

		public static long GetFileLength(string filename)
		{
			FileInfo fi = new FileInfo(filename);
			return fi.Length;
		}

		// Do not use the function to load large file.
		public static string LoadFileText(string filename)
		{
			return File.ReadAllText(filename);
		}

		public static byte[] LoadFileBytes(string filename)
		{
			return File.ReadAllBytes(filename);
		}

		public static void SaveFileText(string content, string filename)
		{
			File.WriteAllText(filename, content);
		}

		public static void SaveFileText(List<string> contentList, string filename)
		{
			File.WriteAllLines(filename, contentList.ToArray());
		}

		public static void SaveFileByte(byte[] bytes, string filename)
		{
			File.WriteAllBytes(filename, bytes);
		}

		public static void DeleteFile(string filename)
		{
			if (!File.Exists(filename))
			{
				return;
			}

			File.Delete(filename);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		public static bool EnsureFolder(string filepath)
		{
			var folder = Path.GetDirectoryName(filepath);
			if (null != folder && !Directory.Exists(folder))
			{
				try
				{
					Directory.CreateDirectory(folder);
				}
				catch(System.UnauthorizedAccessException)
				{
					//没有权限
					UnityEngine.Debug.LogError("EnsureFolder failed with UnauthorizedAccessException");
					return false;
				}
			}
			return true;
		}

		public static void ClearFolder(string path)
		{
			var folder = Path.GetDirectoryName(path);
			if (null != folder && Directory.Exists(folder))
			{
				Directory.Delete(path, true);
			}

			Directory.CreateDirectory(path);
		}

		public static string[] GetFiles(string path, string searchPattern, bool includeSubDir)
		{
			return Directory.GetFiles(path, searchPattern,
				includeSubDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		}
		
		//if folder not exit , create folder, then create file with content
		public static void CreateFileWithText(string filepath, string content)
		{
			EnsureFolder(filepath);
			File.WriteAllText(filepath, content);
		}

		public static void MoveFile(string sourceFileName, string destFileName, bool overwrite = false)
		{
			if (File.Exists(destFileName))
			{
				if (overwrite)
				{
					File.Delete(destFileName);
				}
				else
				{
					return;
				}
			}
			else
            {
				EnsureFolder(destFileName);
			}
			File.Move(sourceFileName, destFileName);
		}

		/// <summary>
		/// 移动文件夹
		/// </summary>
		/// <param name="sourceDirName"></param>
		/// <param name="destDirName">目标路径的父文件夹，整个文件夹会被移动到该文件夹下</param>
		/// <param name="overwrite"></param>
		/// <param name="moveMeta"></param>
		public static void MoveDirectory(string sourceDirName, string destDirName, bool overwrite = false, bool moveMeta = true)
		{
			string formatSrcDir = Framework.PathUtil.FormatPathSeparator(sourceDirName);
			foreach (string file in Directory.GetFiles(formatSrcDir, "*", SearchOption.AllDirectories))
			{
				if (!moveMeta && Path.GetExtension(file) == ".meta")
				{
					continue;
				}
				string relativePath = file.Replace(Path.GetDirectoryName(formatSrcDir), "").Substring(1);
				string desPath = Path.Combine(destDirName, relativePath);
				MoveFile(file, desPath, overwrite);
			}
			Directory.Delete(sourceDirName, true);
		}

		public static void CopyFilesInDirectory(string sourceDirName, string destDirName, bool overwrite = false, bool copyMeta = true)
		{
			string formatSrcDir = Framework.PathUtil.FormatPathSeparator(sourceDirName);
			foreach (string file in Directory.GetFiles(formatSrcDir, "*", SearchOption.AllDirectories))
			{
				if (!copyMeta && Path.GetExtension(file) == ".meta")
				{
					continue;
				}
				string desPath;
				string fileName = Path.GetFileName(file);
				desPath = Path.Combine(destDirName, fileName);
				EnsureFolder(desPath);
				UnityEngine.Debug.Log(file + " " + desPath);
				File.Copy(file, desPath, overwrite);
			}

		}

		public static void CopyDirectory(string sourceDirName, string destDirName, bool overwrite = false, bool copyMeta = true, string newDirName = null) 
		{
			string formatSrcDir = Framework.PathUtil.FormatPathSeparator(sourceDirName);
			foreach (string file in Directory.GetFiles(formatSrcDir, "*", SearchOption.AllDirectories))
			{
				if (!copyMeta && Path.GetExtension(file) == ".meta")
				{
					continue;
				}
				string desPath;
				if (!string.IsNullOrEmpty(newDirName))
				{
					desPath = Path.Combine(destDirName, newDirName, Path.GetFileName(file));
				}
				else
				{
					string relativePath = file.Replace(Path.GetDirectoryName(formatSrcDir), "").Substring(1);
					desPath = Path.Combine(destDirName, relativePath);
				}
				EnsureFolder(desPath);
				UnityEngine.Debug.Log(file + " " + desPath);
				File.Copy(file, desPath, overwrite);
			}
		}

		/// <summary>
		/// 递归删除空文件夹
		/// </summary>
		/// <param name="rootDirectory"></param>
		/// <param name="isMetaFileIgnored">是否要忽略meta文件，选择忽略的话只存在meta文件的文件夹也被当做空文件夹处理</param>
		public static void DeleteEmptyDirectory(string rootDirectory, bool isMetaFileIgnored = true)
		{
			foreach (var directory in Directory.GetDirectories(rootDirectory))
			{
				DeleteEmptyDirectory(directory);
				if (Directory.GetFiles(directory).Length == 0 &&
					Directory.GetDirectories(directory).Length == 0)
				{
					Directory.Delete(directory, false);
				}
			}
		}
	}
}