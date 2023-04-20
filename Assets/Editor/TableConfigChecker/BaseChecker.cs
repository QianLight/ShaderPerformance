using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDTools
{
    public delegate void MethordDelegate(string TableName, string ColumnName);
    public class BaseChecker
    {
        public static List<MethordInfo> CheckList = new List<MethordInfo>();
        //public DataStructCreator DSC = new DataStructCreator();

        public static void EmptyCheck(string TableName,string ColumnName)
        {
            DataStructCreator DSC = new DataStructCreator();
            List<string> CheckedColumn;
            CheckedColumn = DSC.GetTableSpecificColumnContent(TableName, ColumnName);
            List<int> Result = new List<int>();
            for(int i = 0; i < CheckedColumn.Count; i++)
            {
                if(CheckedColumn[i] == "")
                {
                    Result.Add(i+3);
                    TableConfigCheckerUI.ResultOutput($"表{TableName}的{ColumnName}列的第{i+3}行为空");
                }
            }
            if(Result.Count == 0)
            {
                TableConfigCheckerUI.ResultOutput($"表{TableName}的{ColumnName}列没有任何一行为空");
            }
        }

        public static void RepeatedCheck(string TableName, string ColumnName)
        {
            List<string> CheckedColumn;
            DataStructCreator DSC = new DataStructCreator();
            CheckedColumn = DSC.GetTableSpecificColumnContent(TableName, ColumnName);
            List<int> Result = new List<int>();
            List<int> RepeatedRow = new List<int>();

            for (int i = 0; i < CheckedColumn.Count; i++)
            {
                if (CheckedColumn[i] == "")
                    continue;
                else if (RepeatedRow.Contains(i))
                    continue;
                for (int j = i+1; j < CheckedColumn.Count; j++)
                {
                    if(CheckedColumn[i] == CheckedColumn[j])
                    {
                        Result.Add(j);
                    }
                }
                if(Result.Count != 0)
                {
                    string repeatedrow = $"{i+3},";
                    foreach(int result in Result)
                    {
                        repeatedrow = string.Concat(repeatedrow,result+3,",");
                    }
                    TableConfigCheckerUI.ResultOutput($"表{TableName}的{ColumnName}列中以下行存在相同内容({CheckedColumn[i]})：{repeatedrow}");
                    RepeatedRow.AddRange(Result);
                    Result.Clear();
                }  
            }
            if(RepeatedRow.Count == 0)
            {
                TableConfigCheckerUI.ResultOutput($"表{TableName}的{ColumnName}列中没有任何内容重复的行");
            }
        }

        public static void ForeignkeyCheck(string TableName, string OtherName)
        {
            string[] othername = OtherName.Split('|');
            List<string> CheckedColumn;
            List<string> AimColumn;
            DataStructCreator DSC = new DataStructCreator();
            CheckedColumn = DSC.GetTableSpecificColumnContent(TableName, othername[0]);
            AimColumn = DSC.GetTableSpecificColumnContent(othername[1], othername[2]);
            List<int> Result = new List<int>();

            for (int i = 0; i < CheckedColumn.Count; i++)
            {
                if (CheckedColumn[i] == "")
                    continue;
                if (!AimColumn.Contains(CheckedColumn[i]))
                {
                    Result.Add(i);
                    TableConfigCheckerUI.ResultOutput($"表{TableName}的{othername[0]}列的第{i + 3}行内容无法在表{othername[1]}的{othername[2]}列中找到");
                }
            }
            if(Result.Count == 0)
            {
                TableConfigCheckerUI.ResultOutput($"表{TableName}的{othername[0]}列的所有内容都能在表{othername[1]}的{othername[2]}列中找到");
            }
            
        }
    }

    public class MethordInfo
    {
        public string ObjectName;
        public string MethordName;    
        public string Tips;
        public int Importance;
        public MethordDelegate TheMethord;

        public MethordInfo(string objectname, string methordname, string tips , int importance, MethordDelegate themethord)
        {
            ObjectName = objectname;
            MethordName = methordname;
            Tips = tips;
            Importance = importance;
            TheMethord = themethord;
        }
    }
}