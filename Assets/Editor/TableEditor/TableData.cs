using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TableEditor
{
    public class TableFullData
    {
        public string tableName;
        public string keyName;
        public List<TableData> dataList;  //可以用行号去查找 查找逻辑是 表的索引值 ->行号->value值
        public List<string> nameList;    //指的是表格列名
        public List<string> noteList;    //列的中文备注
        //public List<int> rowNumber;  //用列表存行号

        public TableFullData()
        {
            dataList = new List<TableData>();
            nameList = new List<string>();
            noteList = new List<string>();
            tableName = string.Empty;
            keyName = string.Empty;
        }

        public void Reset()
        {
            keyName = string.Empty;
            tableName = string.Empty;
            dataList.Clear();
            nameList.Clear();
            noteList.Clear();
        }
    }
    public class TableData
    {
        public List<string> valueList;
        public string keyName;   //存的是处理后的键值（为了解决表格中有重复键值的情况）
        public int index;
        //public bool fromTemp = false;

        public TableData()
        {
            valueList = new List<string>();
        }
        public static T DeepCopyByXml<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = xml.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }
    }
    public class TableCreateConfigData
    {
        public List<string> nameList;    //指的是表格列名
        public List<string> noteList;    //列的中文备注
        public List<bool> toggleList; //需要在主页上显示并支持搜索的列


        public TableCreateConfigData()
        {
            nameList = new List<string>();
            noteList = new List<string>();
            toggleList = new List<bool>();
        }

        public void Reset()
        {
            nameList.Clear();
            noteList.Clear();
            toggleList.Clear();
        }
    }
    public class TableConfig
    {
        public List<TableConfigData> configList;

        public TableConfig()
        {
            configList = new List<TableConfigData>();
        }
    }
    public class TableConfigData
    {
        public string tableName;
        public string keyName;
        public List<string> tagList; //需要在主页上显示并支持搜索的列
        //public List<string> contentList;//此列表中的变量不予以修改

        public TableConfigData()
        {
            tagList = new List<string>();
        }
    }
    public class TableConfigKey
    {
        public SerizlizerDictionary<string,string> tableKey;
        //public List<string> contentList;//此列表中的变量不予以修改
        public TableConfigKey()
        {
            tableKey = new SerizlizerDictionary<string, string>();
        }
    }

    public class TableTempFullData
    {
        public static readonly string[] TempType = new string[]
        {
            "Add",
            "Modify",
            "AddModify",
            "Delete",
            //"Exchange",
        };

        public string tableName;
        public List<TableTempData> tempDataList;

        public SerizlizerDictionary<string, int> fullFlagDic; //该表所有数据的索引值-行号
        public SerizlizerDictionary<string, List<string>> OriginData; //该表修改数据的索引值-ValueList
        public TableTempFullData()
        {
            fullFlagDic = new SerizlizerDictionary<string, int>();
            OriginData = new SerizlizerDictionary<string, List<string>>();
            tempDataList = new List<TableTempData>();
            tableName = string.Empty;
        }
        public void Reset()
        {
            tableName = string.Empty;
            fullFlagDic.Clear();
            OriginData.Clear();
            tempDataList.Clear();
        }
    }

    public class TableTempData
    {
        public int index; //在fullData里的行号，这个主要是为了合并delete类型的数据而存在的,因为被删除的数据不存在 xml的键值-行号列表中
        public string type;
        public string keyName;
        public List<string> valueList;
        public List<string> ModifyCol;  //基本上所有列都存的是列名而不是index，这是为了防止excel列名改变导致读存错误

        public TableTempData()
        {
            type = TableTempFullData.TempType[0];
            valueList = new List<string>();
            ModifyCol = new List<string>();
        }
        public static T DeepCopyByXml<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = xml.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }
    }

    public class ActiveTable
    {
        public string tableName;
        public float scrollerVale;
        public bool isSaveChange;
        public bool isKeyOnly;

        public ActiveTable(string name)
        {
            tableName = name;
            scrollerVale = 0;
            isSaveChange = true;
            isKeyOnly = true;
        }
        public ActiveTable(string name,float line)
        {
            tableName = name;
            scrollerVale = line;
            isSaveChange = true;
        }
    }

    public class OriginData
    {
        public List<TableData> value;

        public OriginData()
        {
            value = new List<TableData>();
        }
    }
}
