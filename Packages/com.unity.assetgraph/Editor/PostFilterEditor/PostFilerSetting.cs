using System.Text;
using System.IO;
using System.Collections.Generic;

public class PostFilterSetting
{
    private static PostFilterSetting m_vFileSettingInstance;
    private const string filePath = @"Assets/ZeusSetting/EditorSetting/PostFilterSetting.json";

    public List<string> filterFolders;
    public List<string> filterExtensions;

    public static PostFilterSetting GetInstance()
    {
        if (m_vFileSettingInstance == null)
        {

            m_vFileSettingInstance = new PostFilterSetting();

            if(!Directory.Exists("Assets/ZeusSetting/EditorSetting"))
                Directory.CreateDirectory("Assets/ZeusSetting/EditorSetting");

            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath, Encoding.UTF8);
                m_vFileSettingInstance = UnityEngine.JsonUtility.FromJson<PostFilterSetting>(jsonData);
            }
            else
            {
                m_vFileSettingInstance = new PostFilterSetting();
                File.WriteAllText(filePath, UnityEngine.JsonUtility.ToJson(m_vFileSettingInstance, true));
            }
        }

        return m_vFileSettingInstance;
    }

    private PostFilterSetting()
    {
        filterFolders = new List<string>();
        filterExtensions = new List<string>();
    }

    public void Save()
    {
        string jsonData = UnityEngine.JsonUtility.ToJson(this, true);
        System.IO.File.WriteAllText(filePath, jsonData);
    }

    //所有文件都忽略才忽略
    public static bool IsShouldIgnore(string[] importedAssets,
            string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var setting = GetInstance();

        if (setting.filterFolders.Count == 0 && setting.filterExtensions.Count == 0)
            return false;

        foreach (var item in importedAssets)
        {
            if (!IsUnderSomeFolder(item, setting.filterFolders) && !IsSomeFilterFileType(item, setting.filterExtensions))
                return false;
        }

        foreach (var item in deletedAssets)
        {
            if (!IsUnderSomeFolder(item, setting.filterFolders) && !IsSomeFilterFileType(item, setting.filterExtensions))
                return false;
        }

        foreach (var item in movedAssets)
        {
            if (!IsUnderSomeFolder(item, setting.filterFolders) && !IsSomeFilterFileType(item, setting.filterExtensions))
                return false;
        }

        foreach (var item in movedFromAssetPaths)
        {
            if (!IsUnderSomeFolder(item, setting.filterFolders) && !IsSomeFilterFileType(item, setting.filterExtensions))
                return false;
        }

        return true;
    }

    public static bool IsSomeFilterFileType(string file, List<string> extensions)
    {
        bool isSomeFilterFileType = false;
        foreach (var item in extensions)
        {
            if (file.EndsWith(item))
                isSomeFilterFileType = true;
        }

        return isSomeFilterFileType;
    }

    public static bool IsUnderSomeFolder(string file, List<string> folders)
    {
        bool isUnderSomeFilterFolder = false;
        foreach (var folder in folders)
            if (IsPathUnderFolderOrTheSame(folder, file))
                isUnderSomeFilterFolder = true;

        return isUnderSomeFilterFolder;
    }


    public static bool IsPathUnderFolderOrTheSame(string folder, string path)
    {
        if (path.Length < folder.Length)
            return false;

        if (path.Length == folder.Length)
        {
            if (folder == path)
                return true;

            return false;
        }

        if (path.Substring(0, folder.Length) == folder && path[folder.Length] == '/')
            return true;

        return false;
    }
}
