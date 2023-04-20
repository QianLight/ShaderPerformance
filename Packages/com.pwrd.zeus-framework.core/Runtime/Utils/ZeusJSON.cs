using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public enum ZeusJSONExceptionType
{
    NotSupport,
    Invalid,
}

public class ZeusJSONInvalidException : ZeusJSONException
{
    public ZeusJSONInvalidException(string msg) : base(ZeusJSONExceptionType.Invalid, msg)
    {
        
    }
}

public class ZeusJSONNotSupportedException : ZeusJSONException
{
    public ZeusJSONNotSupportedException (string msg) : base(ZeusJSONExceptionType.NotSupport, msg)
    {
        
    }
}

public abstract class ZeusJSONException : Exception
{
    private ZeusJSONExceptionType _exceptionType;
    public ZeusJSONException(ZeusJSONExceptionType exceptionType, string msg):base(msg)
    {
        _exceptionType = exceptionType;
    }
}

/// <summary>
/// 基于SimpleJson，做了些优化
/// </summary>
public class ZeusJSON
{
    public static List<T> DeserializetList<T>(string json)
    {
        var valType = typeof(T);
        if(valType == typeof(string))
        {
            return ListParser<string>.Parse(json, String2String) as List<T>;
        }
        else if(valType == typeof(int))
        {
            return ListParser<int>.Parse(json, String2Int) as List<T>;
        }
        else if(valType == typeof(float))
        {
            return ListParser<float>.Parse(json, String2Float) as List<T>;
        }
        else if(valType == typeof(bool))
        {
            return ListParser<bool>.Parse(json, String2Bool) as List<T>;
        }
        else
        {
            throw new ZeusJSONNotSupportedException($"List with type {valType} is not supported");
        }

    }
    /// <summary>
    /// 只支持类型为string，int，float，bool类型的列表的反序列化
    /// 如果需要更多的支持请联系zeus的支持人员
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T[] DeserializetArray<T>(string json)
    {
        var valType = typeof(T);
        if(valType == typeof(string))
        {
            return ArrayParser<string>.Parse(json, String2String) as T[];
        }
        else if(valType == typeof(int))
        {
            return ArrayParser<int>.Parse(json, String2Int) as T[];
        }
        else if(valType == typeof(float))
        {
            return ArrayParser<float>.Parse(json, String2Float) as T[];
        }
        else if(valType == typeof(bool))
        {
            return ArrayParser<bool>.Parse(json, String2Bool) as T[];
        }
        else
        {
            throw new ZeusJSONNotSupportedException($"List with type {valType} is not supported");
        }
    }

    /// <summary>
    /// 只支持key为string，value为string，int，float，bool类型的Dictionary的反序列化
    /// 如果需要更多的支持请联系zeus的支持人员
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static Dictionary<TKey, TValue> DeserializeDic<TKey, TValue>(string json)
    {
        var keyType = typeof(TKey);
        var valType = typeof(TValue);
        if (keyType == typeof(string))
        {
            if (valType == typeof(string))
            {
                return StringStringDicParser(json) as Dictionary<TKey, TValue>;
            }
            else if (valType == typeof(int))
            {
                return StringIntDicParser(json) as Dictionary<TKey, TValue>;
            }
            else if (valType == typeof(float))
            {

                return StringFloatDicParser(json) as Dictionary<TKey, TValue>;
            }
            else if (valType == typeof(bool))
            {
                return StringBoolDicParser(json) as Dictionary<TKey, TValue>;
            }
            else
            {
                throw new ZeusJSONNotSupportedException($"dic with {keyType} key and {valType} val is not supported");
            }
        }
        else
        {
            throw new ZeusJSONNotSupportedException($"dic with {keyType} key and {valType} val is not supported");
        }
    }

    static StringBuilder builder = new StringBuilder();

    /// <summary>
    /// 从MiniJson贴过来改的
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string Serialize(object obj)
    {

        builder.Length = 0;
        SerializeValue(obj);

        return builder.ToString();
    }

    static void SerializeValue(object value)
    {
        IList asList;
        IDictionary asDict;
        string asStr;

        if (value == null)
        {
            builder.Append("null");
        }
        else if ((asStr = value as string) != null)
        {
            SerializeString(asStr);
        }
        else if (value is bool)
        {
            builder.Append((bool)value ? "true" : "false");
        }
        else if ((asList = value as IList) != null)
        {
            SerializeArray(asList);
        }
        else if ((asDict = value as IDictionary) != null)
        {
            SerializeObject(asDict);
        }
        else if (value is char)
        {
            SerializeString(new string((char)value, 1));
        }
        else
        {
            SerializeOther(value);
        }
    }

    static void SerializeObject(IDictionary obj)
    {
        bool first = true;
        builder.Append('{');
        foreach (var e in obj.Keys)
        {
            if (!first)
            {
                builder.Append(',');
            }

            SerializeString(e.ToString());
            builder.Append(':');
            SerializeValue(obj[e]);
            first = false;
        }
        builder.Append('}');
    }

    static void SerializeArray(IList anArray)
    {
        builder.Append('[');

        bool first = true;

        foreach (object obj in anArray)
        {
            if (!first)
            {
                builder.Append(',');
            }

            SerializeValue(obj);

            first = false;
        }

        builder.Append(']');
    }

    static void SerializeString(string str)
    {
        builder.Append('\"');
        foreach (var c in str)
        {
            switch (c)
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    builder.Append(c);
                    break;
            }
        }
        builder.Append('\"');
    }

    //G9 和 G17 来源 https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
    static void SerializeOther(object value)
    {
        if (value is float)
        {
            builder.Append(((float)value).ToString("G9", System.Globalization.CultureInfo.InvariantCulture));
        }
        else if (value is double)
        {
            builder.Append(((double)value).ToString("G17", System.Globalization.CultureInfo.InvariantCulture));
        }
        else if (value is int
                 || value is uint
                 || value is long
                 || value is sbyte
                 || value is byte
                 || value is short
                 || value is ushort
                 || value is ulong)
        {
            builder.Append(value);
        }
        else if (value is decimal)
        {
            builder.Append(Convert.ToDouble(value).ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        }
        else
        {
            SerializeString(value.ToString());
        }
    }

    private static Dictionary<string, string> StringStringDicParser(string json)
    {
        return DicParser<string, string>.Parse(json, String2String, String2String);
    }
    private static Dictionary<string, int> StringIntDicParser(string json)
    {
        return DicParser<string, int>.Parse(json, String2String, String2Int);
    }
    private static Dictionary<string, float> StringFloatDicParser(string json)
    {
        return DicParser<string, float>.Parse(json, String2String, String2Float);
    }
    private static Dictionary<string, bool> StringBoolDicParser(string json)
    {
        return DicParser<string, bool>.Parse(json, String2String, String2Bool);
    }


    private static string String2String(string val)
    {
        return val;
    }

    private static int String2Int(string val)
    {
        return int.Parse(val);
    }

    private static float String2Float(string val)
    {
        return float.Parse(val);
    }

    private static bool String2Bool(string val)
    {
        return bool.Parse(val);
    }

    private static StringBuilder _reuseableToken = new StringBuilder();
    private static class Parser
    {
        public static bool Parese(string json, ref bool QuoteMode, ref int position, StringBuilder _token)
        {
            switch(json[position])
            {
                    case '"':
                        QuoteMode ^= true;
                        break;
                    case '\r':
                        if (QuoteMode)
                        {
                            _token.Append(json[position]);
                        }
                        break;
                    case '\n':
                        if (QuoteMode)
                        {
                            _token.Append(json[position]);
                        }
                        break;
                    case ' ':
                    case '\t':
                        if (QuoteMode)
                        {
                            _token.Append(json[position]);
                        }
                        break;
                    case '\\':
                        ++position;
                        if (QuoteMode)
                        {
                            char C = json[position];
                            switch (C)
                            {
                                case 't':
                                    _token.Append('\t');
                                    break;
                                case 'r':
                                    _token.Append('\r');
                                    break;
                                case 'n':
                                    _token.Append('\n');
                                    break;
                                case 'b':
                                    _token.Append('\b');
                                    break;
                                case 'f':
                                    _token.Append('\f');
                                    break;
                                case 'u':
                                    {
                                        string s = json.Substring(position + 1, 4);
                                        _token.Append((char)int.Parse(
                                            s,
                                            System.Globalization.NumberStyles.AllowHexSpecifier));
                                        position += 4;
                                        break;
                                    }
                                default:
                                    _token.Append(C);
                                    break;
                            }
                        }
                        break;
                    case '/':
                        //不支持注释
                        _token.Append(json[position]);
                        break;
                    case '\uFEFF': // remove / ignore BOM (Byte Order Mark)
                        break;

                default:
                    return false;
            }
            return true;
        }
    }


    private static class DicParser<TKey, TValue>
    {
        public static Dictionary<TKey, TValue> Parse(string json, Func<string, TKey> str2key, Func<string, TValue> str2Val)
        {
            bool start = false;
            Dictionary<TKey, TValue> ctx = new Dictionary<TKey, TValue>();
            int position = 0;
            _reuseableToken.Length = 0;
            string tokenName = "";
            bool quoteMode = false;
            while (position < json.Length)
            {

                if (!Parser.Parese(json, ref quoteMode, ref position, _reuseableToken))
                {
                    switch (json[position])
                    {
                        case '{':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            if (start)
                            {
                                ThrowInvalidException('{', position);
                            }
                            start = true;
                            tokenName = "";
                            _reuseableToken.Length = 0;
                            break;
                        case '[':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            ThrowInvalidException('[', position);
                            break;
                        case ']':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            ThrowInvalidException(']', position);
                            break;
                        case ':':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            tokenName = _reuseableToken.ToString();
                            _reuseableToken.Length = 0;
                            break;
                        case ',':
                        case '}':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            if (_reuseableToken.Length > 0)
                            {
                                ctx.Add(str2key(tokenName), str2Val(_reuseableToken.ToString()));
                            }
                            else
                            {
                                ctx.Add(str2key(tokenName), default(TValue));
                            }
                            tokenName = "";
                            _reuseableToken.Length = 0;
                            break;
                        default:
                            _reuseableToken.Append(json[position]);
                            break;
                    }
                }
                ++position;
            }
            if (quoteMode)
            {
                throw new ZeusJSONInvalidException("another \" is needed");
            }
            return ctx;
        }
    }

    private static void ThrowInvalidException(char c, int index)
    {
        throw new ZeusJSONInvalidException($"invlid json, '{c}' at {index}");
    }

    private static class ListParser<T>
    {
        public static List<T> Parse(string json, Func<string, T> str2Val, List<T> list = null)
        {
            bool start = false;
            int position = 0;
            bool quoteMode = false;
            if(null == list)
            {
                list = new List<T>();
            }
            _reuseableToken.Length = 0;
            while (position < json.Length)
            {

                if (!Parser.Parese(json, ref quoteMode, ref position, _reuseableToken))
                {
                    switch (json[position])
                    {
                        case '{':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            ThrowInvalidException('{', position);
                            break;
                        case '[':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            if (start)
                            {
                                ThrowInvalidException('[', position);
                            }
                            _reuseableToken.Length = 0;
                            start = true;
                            break;
                        case '}':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            ThrowInvalidException('}', position);
                            break;
                        case ']':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            if (!start)
                            {
                                ThrowInvalidException('[', position);
                            }
                            start = false;
                            if (_reuseableToken.Length > 0)
                                list.Add(str2Val(_reuseableToken.ToString()));
                            else
                                list.Add(default(T));
                            _reuseableToken.Length = 0;
                            break;
                        case ':':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            ThrowInvalidException(':', position);
                            break;
                        case ',':
                            if (quoteMode)
                            {
                                _reuseableToken.Append(json[position]);
                                break;
                            }
                            if (_reuseableToken.Length > 0)
                                list.Add(str2Val(_reuseableToken.ToString()));
                            else
                                list.Add(default(T));
                            _reuseableToken.Length = 0;
                            break;
                        default:
                            _reuseableToken.Append(json[position]);
                            break;
                    }
                }
                ++position;
            }
            if (quoteMode)
            {
                throw new ZeusJSONInvalidException("a another \" needed");
            }
            return list;
        }
    }

    private static class ArrayParser<T>
    {
        private static List<T> _reuseableList = new List<T>();

        public static T[] Parse(string json, Func<string, T> str2Val)
        {
            _reuseableList.Clear();
            ListParser<T>.Parse(json, str2Val, _reuseableList);
            var array = new T[_reuseableList.Count];
            for(var index =0; index < _reuseableList.Count; index++)
            {
                array[index] = _reuseableList[index];
            }
            _reuseableList.Clear();
            return array;
        }
    }
}
