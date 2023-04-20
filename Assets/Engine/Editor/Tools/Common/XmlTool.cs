//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.IO;
//using System.Xml;
//using UnityEngine;
//using UnityEditor;

//namespace CFEngine.Editor
//{
//    public class XmlTool
//    {
//        private const string XmlPath = "/";

//        public static void InitXml(ScaneResult data,string xmlName)
//        {
//            XmlDocument xml= new XmlDocument(); 
//            if(File.Exists(Application.dataPath+XmlPath+xmlName))
//            {
//                File.Delete(Application.dataPath+XmlPath + xmlName);
//            }
//            var root=xml.CreateElement("root");
//            SetXmlData(xml, data,root);
//            xml.AppendChild(root);
//            xml.Save(Application.dataPath+XmlPath + xmlName+".xml");
//        }

//        private static void SetXmlData(XmlDocument xml,ScaneResult data,XmlElement root)
//        {
//            foreach(var element in data.allRes.res)
//            {
//                if (element.parents != null)
//                    continue;
//                else
//                {
//                    var xmlElement=SetElement(root,element,xml);
//                    root.AppendChild(xmlElement);
//                }
//            }
//        }

//        private static XmlElement SetElement(XmlElement parent,ResItem item,XmlDocument xml)
//        {
//            var index = item.name.IndexOf(' ');
//            var name = item.name;
//            while(index>0)
//            {
//                name = name.Remove(index, 1);
//                index= name.IndexOf(' ');
//            }
//            XmlElement element=xml.CreateElement("Node");
//            if(item.childs!=null&&item.childs.Count>0)
//            {
//                foreach(var child in item.childs)
//                {
//                    element.AppendChild(SetElement(element, child,xml));               
//                }
//            }
//            element.SetAttribute("Name",item.name);
//            element.SetAttribute("Path", item.path);
//            element.SetAttribute("Size", item.size.ToString());
//            element.SetAttribute("TotalSize", item.totalSize.ToString());
//            parent.AppendChild(element);
//            return element;
//        }
//    }
//}
