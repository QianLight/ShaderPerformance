using System.IO;
using System.Xml;
using UnityEditor;

public class MergeLinkXML
{

    /*
     * 代码裁剪 和xlua整合
     */


    public static void MergeXML()
    {
        string path = "Assets/XLua/Gen/link.xml";
        string target = "Assets/link.xml";
        if (File.Exists(path))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement group = doc.DocumentElement;
            XmlNodeList childs = group.ChildNodes;
            int cnt = childs.Count;

            for (int i = cnt - 1; i >= 0; i--)
            {
                XmlElement elemnet = childs[i] as XmlElement;
                if (elemnet.HasAttribute("fullname"))
                {
                    var att = elemnet.GetAttribute("fullname");
                    if (att.Contains("CFUtilPoolLib"))
                    {
                        group.RemoveChild(elemnet);
                    }
                    if (att.Contains("CFEngine"))
                    {
                        group.RemoveChild(elemnet);
                    }
                }
            }


            AddAssimbly(doc, "CFClient");
            AddAssimbly(doc, "CFEngine");
            AddAssimbly(doc, "CFUtilPoolLib");

            doc.Save(target);
        }

        File.Delete(path);
        AssetDatabase.ImportAsset(target);
        AssetDatabase.Refresh();
    }


    static void AddAssimbly(XmlDocument doc, string assimbly)
    {
        XmlElement element = doc.CreateElement("assembly");
        element.SetAttribute("fullname", assimbly);
        element.SetAttribute("preserve", "all");
        XmlElement group = doc.DocumentElement;
        group.AppendChild(element);
    }

}
