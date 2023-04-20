using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CFEngine;
using CFEngine.Editor;
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;

public class DirectorDataSerialize
{
    public static void GetReadWriteFieldStr (FieldInfo fi,
        string arrayFieldName, string dataName,
        string writeStr, string readStr,
        out string write, out string read)
    {
        Type t = fi.FieldType;
        if (t == typeof (Vector2) ||
            t == typeof (Vector3) ||
            t == typeof (Vector4) ||
            t == typeof (Vector2Int) ||
            t == typeof (Vector3Int) ||
            t == typeof (Vector4Int))
        {
            write = string.Format ("EditorCommon.WriteVector({0}, {1}{2});", writeStr, dataName, fi.Name);
            read = string.Format ("{0}.ReadVector(ref {1}{2});", readStr, dataName, fi.Name);
        }
        else
        {
            if (t == typeof (short))
            {
                if (fi.IsDefined (typeof (CFStringReIndexAttribute), false))
                {
                    CFStringReIndexAttribute attr = System.Attribute.GetCustomAttribute (fi, typeof (CFStringReIndexAttribute)) as CFStringReIndexAttribute;
                    write = string.Format ("short {2}Index = (short)Array.IndexOf({0}, {1}{2});\r\n\t\t{3}.Write({2}Index);",
                        arrayFieldName, dataName, attr.strFieldName, writeStr);
                }
                else
                    write = string.Format ("{0}.Write({1}{2});", writeStr, dataName, fi.Name);
            }
            else
            {
                write = string.Format ("{0}.Write({1}{2});", writeStr, dataName, fi.Name);
            }

            if (t == typeof (char))
            {
                read = string.Format ("{0}{1} = {2}.ReadChar();", dataName, fi.Name, readStr);
            }
            else if (t == typeof (byte))
            {
                read = string.Format ("{0}{1} = {2}.ReadByte();", dataName, fi.Name, readStr);
            }
            else if (t == typeof (int))
            {
                read = string.Format ("{0}{1} = {2}.ReadInt32();", dataName, fi.Name, readStr);
            }
            else if (t == typeof (uint))
            {
                read = string.Format ("{0}{1} = {2}.ReadUInt32();", dataName, fi.Name, readStr);
            }
            else if (t == typeof (ushort))
            {
                read = string.Format ("{0}{1} = {2}.ReadUInt16();", dataName, fi.Name, readStr);
            }
            else if (t == typeof (float))
            {
                read = string.Format ("{0}{1} = {2}.ReadSingle();", dataName, fi.Name, readStr);
            }
            else if (t == typeof (long))
            {
                read = string.Format ("{0}{1} = {2}.ReadInt64();", dataName, fi.Name, readStr);
            }
            else if (t == typeof (ulong))
            {
                read = string.Format ("{0}{1} = {2}.ReadUInt64();", dataName, fi.Name, readStr);
            }
            else if (t == typeof (bool))
            {
                read = string.Format ("{0}{1} = {2}.ReadBoolean();", dataName, fi.Name, readStr);
            }
            else if (t == typeof (short))
            {
                if (fi.IsDefined (typeof (CFStringReIndexAttribute), false))
                {
                    CFStringReIndexAttribute attr = System.Attribute.GetCustomAttribute (fi, typeof (CFStringReIndexAttribute)) as CFStringReIndexAttribute;
                    read = string.Format ("{0}{1} = GetStr({2}, {3}.ReadInt16());", dataName, attr.strFieldName, arrayFieldName, readStr);
                }
                else
                    read = string.Format ("{0}{1} = {2}.ReadInt16();", dataName, fi.Name, readStr);
            }
            else
            {
                read = "";
            }
        }
    }

    [MenuItem (@"Assets/Tool/Director_CreateDataSerialize")]
    public static void CreateDirectorSerialize ()
    {
        Type[] directorDataType = new Type[]
        {
            typeof (DirectorData),
            typeof (TrackTargetData),
            typeof (AnimClipData),
            typeof (FxClipData),
            typeof (IndexTrackData),
        };
        string template = File.ReadAllText ("Assets/Editor/Timeline/Data/DirectorDataTemplate.txt");
        string writeDataTemplate = @"
    public static void Save_{0} (BinaryWriter bw, TimelineData td, ref {0} data)
    {{
        {1}
    }}";
        string readDataTemplate = @"
    public static void Load_{0} (CFBinaryReader reader, TimelineData td, ref {0} data)
    {{
        {1}
    }}";

        string writePlaceHolder = "write_Data";
        string readPlaceHolder = "read_Data";
        StringBuilder writeDataStr = new StringBuilder ();
        StringBuilder readDataStr = new StringBuilder ();
        StringBuilder writeDataFieldStr = new StringBuilder ();
        StringBuilder readDataFieldStr = new StringBuilder ();
        for (int i = 0; i < directorDataType.Length; ++i)
        {
            Type t = directorDataType[i];
            writeDataFieldStr.Length = 0;
            readDataFieldStr.Length = 0;
            var fields = t.GetFields ().Where (f => f.IsDefined (typeof (CFDirectorDataAttribute), false)).ToArray ();
            for (int j = 0; j < fields.Length; ++j)
            {
                var f = fields[j];

                string writeFieldStr;
                string readFieldStr;
                GetReadWriteFieldStr (f, "td.strPool", "data.", "bw", "reader", out writeFieldStr, out readFieldStr);
                if (!string.IsNullOrEmpty (writeFieldStr) && !string.IsNullOrEmpty (readFieldStr))
                {
                    if (j != 0)
                    {
                        writeDataFieldStr.AppendLine ();
                        writeDataFieldStr.Append ("\t\t");
                        readDataFieldStr.AppendLine ();
                        readDataFieldStr.Append ("\t\t");
                    }
                    writeDataFieldStr.Append (writeFieldStr);
                    readDataFieldStr.Append (readFieldStr);
                }

            }
            writeDataStr.AppendFormat (writeDataTemplate, t.Name, writeDataFieldStr.ToString ());
            writeDataStr.AppendLine ();

            readDataStr.AppendFormat (readDataTemplate, t.Name, readDataFieldStr.ToString ());
            readDataStr.AppendLine ();

        }
        template = template.Replace (writePlaceHolder, writeDataStr.ToString ());
        template = template.Replace (readPlaceHolder, readDataStr.ToString ());
        using (StreamWriter writer = new StreamWriter ("Assets/Scripts/Timeline/Data/DataSerializeImp.cs"))
        {
            writer.Write (template);
        }

    }

    [MenuItem (@"Assets/Tool/Director_SaveHeadData")]
    public static void SaveTimelineHeadData ()
    {
        Dictionary<string, int> headLength = new Dictionary<string, int> ();
        CommonAssets.enumSO.cb = (so, path, context) =>
        {
            if (so is TimelineEditorData)
            {
                var ted = so as TimelineEditorData;
                headLength.Add (ted.name, ted.headLength);

            }
            return;
        };

        CommonAssets.EnumAsset<ScriptableObject> (CommonAssets.enumSO, "SaveHeadData",
            string.Format ("{0}/Timeline", LoadMgr.singleton.bytesDataDir));

        using (FileStream fs = new FileStream (string.Format ("{0}/Config/TimelineConfig.bytes", AssetsConfig.instance.ResourcePath), FileMode.Create))
        {
            BinaryWriter bw = new BinaryWriter (fs);
            short count = (short) headLength.Count;
            bw.Write (count);
            var it = headLength.GetEnumerator ();
            while (it.MoveNext ())
            {
                bw.Write (it.Current.Key.ToLower());
                bw.Write (it.Current.Value);
            }
        }
    }

}