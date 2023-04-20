using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace TableEditor
{
    class TableReader
    {

        private static TableReader reader;
        public static TableReader Instance
        {
            get
            {
                if (reader == null)
                    reader = new TableReader();
                return reader;
            }
        }

        public static bool WriteTable(string tablePath,TableFullData data, bool editorNotice = true)
        {
            try
            {
                StreamWriter writer = new StreamWriter(tablePath, false, Encoding.Unicode);
                var names = ConcatStringList(data.nameList);
                writer.WriteLine(names);
                var notes = ConcatStringList(data.noteList);
                writer.WriteLine(notes);
                for (var i = 0; i < data.dataList.Count; i++)
                {
                    writer.WriteLine(ConcatStringList(data.dataList[i].valueList));
                }
                writer.Close();
                return true;
            }
            catch
            {
                if(editorNotice)
                    TableEditor.Instance.ShowNotice($"{data.tableName}已打开，无法保存。\n请先关闭表格",5);
                else
                {
                    Debug.LogError($"{data.tableName}已打开，无法保存。\n请先关闭表格");
                }
                return false;
            }
        }

        private static string ConcatStringList(List<string> list)
        {
            return string.Join("\t", list);
        }

        //ReadTableByFileStream可以在excel开启的情况下读取excel内容，ReadTable不行
        public static void ReadTableByFileStream(string tablePath, ref TableFullData data,string Encode= "Unicode")
        {
            using (FileStream stream = File.Open(tablePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] array = new byte[stream.Length];
                stream.Read(array, 0, array.Length);
                string[] strArr=new string[1];
                if (Encode== "Unicode")
                    strArr = Encoding.Unicode.GetString(array, 2, array.Length - 2).Split('\n'); //UNICODE编码开头为FF FE，4个十六进制数为2字节
                else if(Encode == "gb2312")
                    strArr = Encoding.GetEncoding("gb2312").GetString(array, 2, array.Length - 2).Split('\n'); 
                data.tableName = tablePath.Substring(tablePath.LastIndexOf('/') + 1);
                for (int i = 0; i < strArr.Length; ++i)
                {
                    var line = strArr[i].TrimEnd('\r');
                    if(i==0)
                    {
                        var names = line.Split('\t');
                        foreach (var name in names)
                        {
                            data.nameList.Add(name);
                        }
                    }
                    else if(i==1)
                    {
                        var notes = line.Split('\t');
                        foreach (var note in notes)
                        {
                            data.noteList.Add(note);
                        }
                    }
                    else if(line != "")
                    {
                        var value = new TableData();
                        var datas = line.Split('\t');
                        foreach (var stringData in datas)
                        {
                            value.valueList.Add(stringData);
                            value.index = i - 2;
                        }
                        data.dataList.Add(value);
                    }
                }
                stream.Close();
            }
        }

        public static void ReadTableByFileStream(string tablePath, ref TableCreateConfigData data)
        {
            using (FileStream stream = File.Open(tablePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] array = new byte[stream.Length];
                stream.Read(array, 0, array.Length);
                string[] strArr = Encoding.Unicode.GetString(array, 2, array.Length - 2).Split('\n');
                for (int i = 0; i < strArr.Length; ++i)
                {
                    var line = strArr[i].TrimEnd('\r');
                    if (i == 0)
                    {
                        var names = line.Split('\t');
                        foreach (var name in names)
                        {
                            data.nameList.Add(name);
                        }
                    }
                    else if (i == 1)
                    {
                        var notes = line.Split('\t');
                        foreach (var note in notes)
                        {
                            data.noteList.Add(note);
                        }
                    }
                    else break;
                }
                stream.Close();
            }
        }
        public static void ReadTable(string tablePath, ref TableFullData data)
        {
            StreamReader reader = new StreamReader(tablePath);
            string line = string.Empty;
            int lineCount = 0;
            data.tableName = tablePath.Substring(tablePath.LastIndexOf('/') + 1);
            while ((line = reader.ReadLine()) != null)
            {
                if (lineCount == 0)//首行为变量名
                {
                    var names = line.Split('\t');
                    foreach (var name in names)
                    {
                        data.nameList.Add(name);
                    }
                }
                else if (lineCount == 1)//第二行为中文注释
                {
                    var notes = line.Split('\t');
                    foreach (var note in notes)
                    {
                        data.noteList.Add(note);
                    }
                }
                else//剩余均为数据行
                {
                    var value = new TableData();
                    var datas = line.Split('\t');
                    foreach (var stringData in datas)
                    {
                        value.valueList.Add(stringData);
                    }

                    data.dataList.Add(value);
                }
                lineCount += 1;
            }
            reader.Close();
        }
        public static void ReadTable(string tablePath, ref TableCreateConfigData data)
        {
            StreamReader reader = new StreamReader(tablePath);
            string line = string.Empty;
            int lineCount = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (lineCount == 0)//首行为变量名
                {
                    var names = line.Split('\t');
                    foreach (var name in names)
                    {
                        data.nameList.Add(name);
                        data.toggleList.Add(false);
                    }
                }
                else if (lineCount == 1)//第二行为中文注释
                {
                    var notes = line.Split('\t');
                    foreach (var note in notes)
                    {
                        data.noteList.Add(note);
                    }
                    break;
                }
                lineCount++;
            }
            reader.Close();
        }
    }
    public class SerizlizerDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer keySer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSer = new XmlSerializer(typeof(TValue));
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                TKey key = (TKey)keySer.Deserialize(reader);
                TValue value = (TValue)valueSer.Deserialize(reader);
                Add(key, value);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer keySer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSer = new XmlSerializer(typeof(TValue));

            foreach (KeyValuePair<TKey, TValue> kv in this)
            {
                keySer.Serialize(writer, kv.Key);
                valueSer.Serialize(writer, kv.Value);
            }
        }

    }
}
