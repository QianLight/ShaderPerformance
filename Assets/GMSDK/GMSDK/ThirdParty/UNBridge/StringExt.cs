using System;
using System.Text;
using UnityEngine;

public static class StringExt
{

    /// <summary>
    /// Unicode转utf8编码
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static  string UnicodeToUtf8(this string s){
        try
        {
            Encoding utf8 = Encoding.UTF8;
            Encoding unicode = Encoding.Unicode;
            byte[] temp = unicode.GetBytes(s);
            byte[] temp1 = Encoding.Convert(unicode, utf8, temp);
            string result = utf8.GetString(temp1);
            return result;
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
        return s;
    }
}
