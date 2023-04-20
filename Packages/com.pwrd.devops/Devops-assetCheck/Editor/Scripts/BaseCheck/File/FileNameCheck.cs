using System.Text.RegularExpressions;


namespace AssetCheck
{

    [CheckRuleDescription("File", "检查文件名包含中文项", "custom:AssetCheck.AssetHelper.FindAllFiles", "")]
    public class FileNameCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            bool bChinese = IsFileNameValid(path);
            output = bChinese ? "包含中文项" : string.Empty;
            return !bChinese;
        }

        public bool IsFileNameValid(string filePath)
        {
            return Regex.IsMatch(filePath, @"[\u4e00-\u9fa5]");
        }
    }

}

