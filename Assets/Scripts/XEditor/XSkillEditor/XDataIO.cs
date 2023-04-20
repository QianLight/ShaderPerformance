﻿#if UNITY_EDITOR
using System;
using UnityEngine;
using CFUtilPoolLib;

using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;

namespace XEditor
{
    public class XDataIO<T> : XSingleton<XDataIO<T>>
	{
        XmlSerializer _formatter = new XmlSerializer(typeof(T));

        public void SerializeData(string pathwithname, T data)
        {
            using (FileStream writer = new FileStream(pathwithname, FileMode.Create))
            {
                //using Encoding
                StreamWriter sw = new StreamWriter(writer, Encoding.UTF8);
                XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
                //empty name spaces
                xsn.Add(string.Empty, string.Empty);

                _formatter.Serialize(sw, data, xsn);

            }
            AssetDatabase.Refresh();
        }

        public void SerializeData(string pathwithname, T data, Type[] types)
        {
            using (FileStream writer = new FileStream(pathwithname, FileMode.Create))
            {
                //using Encoding
                StreamWriter sw = new StreamWriter(writer, Encoding.UTF8);
                XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
                //empty name spaces
                xsn.Add(string.Empty, string.Empty);

                XmlSerializer formatter = new XmlSerializer(typeof(T), types);
                formatter.Serialize(sw, data, xsn);

            }
            AssetDatabase.Refresh();
        }

        public T DeserializeData(string pathwithname)
        {
            try 
            {
                using (FileStream reader = new FileStream(pathwithname, FileMode.Open))
                {
                    //IFormatter formatter = new BinaryFormatter();
                    System.Xml.Serialization.XmlSerializer formatter = new System.Xml.Serialization.XmlSerializer(typeof(T));
                    return (T)formatter.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return default(T);
            }
        }

        public T DeserializeData(Stream stream)
        {
            try
            {
                //IFormatter formatter = new BinaryFormatter();
                System.Xml.Serialization.XmlSerializer formatter = new System.Xml.Serialization.XmlSerializer(typeof(T));
                return (T)formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return default(T);
            }
        }

        public T DeserializeData(string pathwithname, Type[] types)
        {
            using (FileStream reader = new FileStream(pathwithname, FileMode.Open))
            {
                //IFormatter formatter = new BinaryFormatter();
                System.Xml.Serialization.XmlSerializer formatter = new System.Xml.Serialization.XmlSerializer(typeof(T), types);
                return (T)formatter.Deserialize(reader);
            }
        }
	}
}
#endif