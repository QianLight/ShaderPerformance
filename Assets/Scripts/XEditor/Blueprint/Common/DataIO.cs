#if UNITY_EDITOR
using System;
using UnityEngine;

using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using CFEngine;

public class DataIO
{
    [MenuItem(@"Assets/BytesEncrypt/加密并另存为")]
    static void BytesEncryptSaveAs() {
        string defaultFolder = EditorPrefs.GetString("BYTES_ENCRYPT_DEFAULT_FOLDER");
        string folder = EditorUtility.SaveFolderPanel("保存至文件夹", defaultFolder, "");
        if (folder == "")
            return;
        EditorPrefs.SetString("BYTES_ENCRYPT_DEFAULT_FOLDER", folder);

        UnityEngine.Object[] os = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        int index = 0;
        foreach (UnityEngine.Object o in os) {
            string path = AssetDatabase.GetAssetPath(o);
            if (!path.EndsWith(".bytes")) continue;
            string text = File.ReadAllText(path);
            byte[] bytes = SimpleTools.String2Bytes(ref text);
            SimpleTools.Lock(ref bytes);
            Directory.CreateDirectory($@"{folder}\{path.Substring(0, path.LastIndexOf('/'))}");
            File.WriteAllText($@"{folder}\{path}", text);
            ++index;
            EditorUtility.DisplayProgressBar("Progress Bar " + index.ToString() + "/" + os.Length.ToString(), path, (++index / (1.0f * os.Length)));
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem(@"Assets/BytesEncrypt/解密并另存为")]
    static void BytesDecryptSaveAs() {
        string defaultFolder = EditorPrefs.GetString("BYTES_ENCRYPT_DEFAULT_FOLDER");
        string folder = EditorUtility.SaveFolderPanel("保存至文件夹", defaultFolder, "");
        if (folder == "")
            return;
        EditorPrefs.SetString("BYTES_ENCRYPT_DEFAULT_FOLDER", folder);

        UnityEngine.Object[] os = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        int index = 0;
        foreach (UnityEngine.Object o in os) {
            string path = AssetDatabase.GetAssetPath(o);
            if (!path.EndsWith(".bytes")) continue;
            byte[] bytes = File.ReadAllBytes(path);
            SimpleTools.Unlock(ref bytes);
            string text = SimpleTools.Bytes2String(ref bytes, 0, bytes.Length);
            Directory.CreateDirectory($@"{folder}\{path.Substring(0, path.LastIndexOf('/'))}");
            File.WriteAllText($@"{folder}\{path}", text);
            ++index;
            EditorUtility.DisplayProgressBar("Progress Bar " + index.ToString() + "/" + os.Length.ToString(), path, (++index / (1.0f * os.Length)));
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem(@"Assets/BytesEncrypt/Encrypt")]
    static void BytesEncrypt()
    {
        UnityEngine.Object[] os = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        int index = 0;
        foreach (UnityEngine.Object o in os)
        {
            string path = AssetDatabase.GetAssetPath(o);
            if (!path.EndsWith(".bytes")) continue;
            string text = File.ReadAllText(path);
            byte[] bytes = SimpleTools.String2Bytes(ref text);
            SimpleTools.Lock(ref bytes);
            File.WriteAllBytes(path, bytes);
            ++index;
            EditorUtility.DisplayProgressBar("Progress Bar " + index.ToString() + "/" + os.Length.ToString(), path, (++index / (1.0f * os.Length)));
        }
        EditorUtility.ClearProgressBar();
    }
    [MenuItem(@"Assets/BytesEncrypt/Decrypt")]
    static void BytesDecrypt()
    {
        UnityEngine.Object[] os = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        int index = 0;
        foreach (UnityEngine.Object o in os)
        {
            string path = AssetDatabase.GetAssetPath(o);
            if (!path.EndsWith(".bytes")) continue;
            byte[] bytes = File.ReadAllBytes(path);
            SimpleTools.Unlock(ref bytes);
            string text = SimpleTools.Bytes2String(ref bytes, 0, bytes.Length);
            File.WriteAllText(path, text);
            ++index;
            EditorUtility.DisplayProgressBar("Progress Bar " + index.ToString() + "/" + os.Length.ToString(), path, (++index / (1.0f * os.Length)));
        }
        EditorUtility.ClearProgressBar();
    }

    public static void SerializeData<T>(string pathwithname, T data, bool import = true)
    {
        XmlSerializer _formatter = new XmlSerializer(typeof(T));
        using (FileStream writer = new FileStream(pathwithname, FileMode.Create))
        {
            //using Encoding
            StreamWriter sw = new StreamWriter(writer, Encoding.UTF8);
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            //empty name spaces
            xsn.Add(string.Empty, string.Empty);

            _formatter.Serialize(sw, data, xsn);

        }
        if (import) AssetDatabase.ImportAsset(pathwithname.Remove(0, pathwithname.IndexOf("Assets")));
    }

    public static void SerializeData<T>(string pathwithname, T data, Type[] types, bool import = true)
    {
        using (FileStream writer = new FileStream(pathwithname, FileMode.Create))
        {
            //using Encoding
            StreamWriter sw = new StreamWriter(writer, Encoding.UTF8);
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            //empty name spaces
            xsn.Add(string.Empty, string.Empty);

            XmlSerializer formatter = new XmlSerializer(typeof(T), types);
            formatter.Serialize(sw, data, xsn);

        }
        if (import) AssetDatabase.ImportAsset(pathwithname.Remove(0, pathwithname.IndexOf("Assets")));
    }

    public static T DeserializeData<T>(string pathwithname)
    {
        try
        {
            using (FileStream reader = new FileStream(pathwithname, FileMode.Open))
            {
                //IFormatter formatter = new BinaryFormatter();
                System.Xml.Serialization.XmlSerializer formatter = new System.Xml.Serialization.XmlSerializer(typeof(T));
                return (T)formatter.Deserialize(reader);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(pathwithname + "\n" + e.Message);
            return default(T);
        }
    }

    public static T DeserializeData<T>(Stream stream)
    {
        try
        {
            //IFormatter formatter = new BinaryFormatter();
            System.Xml.Serialization.XmlSerializer formatter = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return (T)formatter.Deserialize(stream);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return default(T);
        }
    }

    public static T DeserializeData<T>(string pathwithname, Type[] types)
    {
        using (FileStream reader = new FileStream(pathwithname, FileMode.Open))
        {
            //IFormatter formatter = new BinaryFormatter();
            System.Xml.Serialization.XmlSerializer formatter = new System.Xml.Serialization.XmlSerializer(typeof(T), types);
            return (T)formatter.Deserialize(reader);
        }
    }

    public static void SerializeEcsData<T>(string path, T data)
    {
        string json = JsonUtility.ToJson(data);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        SimpleTools.Lock(ref bytes);
        File.WriteAllBytes(path, bytes);
    }

    public static T DeserializeEcsData<T>(string path)
    {
        T data;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            byte[] bytes = SimpleTools.FileStream2Bytes(fs);
            SimpleTools.Unlock(ref bytes, 0, (int)fs.Length);
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            data = JsonUtility.FromJson<T>(json);
        }
        return data;
    }
}
#endif