using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CFEngine.Quantify
{
    public static class CsvTool {

        public static List<string[]> LoadFile(string path)
        {
            List<string[]> m_ArrayData = new List<string[]>();

            StreamReader sr = null;
            try
            {
                sr = File.OpenText(path);
                Debug.Log("file finded!");
            }
            catch
            {
                Debug.Log("file don't finded!");
                return m_ArrayData;
            }

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                m_ArrayData.Add(line.Split(','));
            }

            sr.Close();
            sr.Dispose();

            return m_ArrayData;
        }
    }

}

