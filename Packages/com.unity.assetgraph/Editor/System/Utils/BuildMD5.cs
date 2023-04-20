using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine.AssetGraph;

namespace UnityEngine.AssetBundles.GraphTool
{
    public class BuildMD5
    {
        public static void CreateMD5XML(string path,string manifestName)
        {

            AssetBundleBuildMap map = AssetBundleBuildMap.GetBuildMap();
            string[] assetBundles = map.GetAllAssetBundleNames();
            HashSet<string> assetNameSet = new HashSet<string>(assetBundles);

            // filename  -> <md5,size>
            Dictionary<string, List<string>> dicFileMD5 = new Dictionary<string, List<string>>();
            //md5 生成器
            MD5CryptoServiceProvider md5GenGenerator = new MD5CryptoServiceProvider();

            DirectoryInfo info = Directory.CreateDirectory(path);

            foreach(FileInfo fileInfo in info.GetFiles())
            {
                if (!fileInfo.Name.Equals(manifestName + ".manifest")
                    && !fileInfo.Name.Equals(manifestName)
                    && !fileInfo.Name.Equals("assetMapName.xml")
                    &&!assetNameSet.Contains(fileInfo.Name) )
                    continue;


                FileStream file = fileInfo.OpenRead();
                byte[] hash = md5GenGenerator.ComputeHash(file);
                List<string> tempList = new List<string>();
                string strMD5 = System.BitConverter.ToString(hash).Replace("-","");
                string size = file.Length.ToString();
                tempList.Add(strMD5);
                tempList.Add(size);
                file.Close();

               

                if (!dicFileMD5.ContainsKey(fileInfo.Name))
                    dicFileMD5.Add(fileInfo.Name, tempList);
                else
                    Debug.LogWarning("<Two File has the same name> name = " + fileInfo.Name);
            }

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement xmlRoot = xmlDoc.CreateElement("MD5Files");
            xmlDoc.AppendChild(xmlRoot);
            foreach (KeyValuePair<string, List<string>> pair in dicFileMD5)
            {
                XmlElement xmlElem = xmlDoc.CreateElement("File");
                xmlRoot.AppendChild(xmlElem);

                xmlElem.SetAttribute("name", pair.Key);
                xmlElem.SetAttribute("md5", pair.Value[0]);
                xmlElem.SetAttribute("size", pair.Value[1]);
            }

            xmlDoc.Save(path + "/"+GetVersionName());
            xmlDoc = null;
            AssetDatabase.Refresh(); //刷新一下
        }

        public static string GetVersionName()
        {
            return "MD5Version.xml";
        }

    }
}
