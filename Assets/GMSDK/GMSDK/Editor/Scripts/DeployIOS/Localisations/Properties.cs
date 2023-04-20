using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GMSDK
{
    public class Properties
    {
        private Dictionary<String, String>  _list;
        private String _filename;

        public Properties(String file)
        {
            Reload(file);
        }

        public String Get(String field, String defValue)
        {
            return (Get(field) == null) ? (defValue) : (Get(field));
        }
        public String Get(String field)
        {
            return (_list.ContainsKey(field))?(_list[field]):(null);
        }

        public bool IsContain(String field)
        {
	        return _list.ContainsKey(field);
        }

        public void Set(String field, System.Object value)
        {
	        if (!_list.ContainsKey(field))
	        {
		        _list.Add(field, value.ToString());
	        }
	        else
	        {
		        _list[field] = value.ToString();
	        }
        }

        public void Remove(String field)
        {
            if (_list.ContainsKey(field))
            {
                _list.Remove(field);
            }
        }

        public void Save()
        {
            Save(_filename);
        }

        public void Save(String filename)
        {
            _filename = filename;
            if (!File.Exists(filename))
            {
	            File.Create(filename);
            }
            StreamWriter file = new StreamWriter(filename);

            foreach (String prop in _list.Keys.ToArray())
            {
	            if (!String.IsNullOrEmpty(_list[prop]))
	            {
		            file.WriteLine("\"" + prop + "\" = \"" + _list[prop] + "\";" );
	            }
            }
            file.Close();
        }
        public void Reload()
        {
            Reload(_filename);
        }
        public void Reload(String filename)
        {
            _filename = filename;
            _list = new Dictionary<String, String>();
            if (File.Exists(filename))
            {
	            LoadFromFile(filename);
            }
            else
            {
	            File.Create(filename);
            }
        }

        private void LoadFromFile(String file)
        {
            foreach (String line in File.ReadAllLines(file))
            {
	            //Debug.Log("line:" + line);
                if ((!String.IsNullOrEmpty(line)) &&
                    (!line.StartsWith(";")) &&
                    (!line.StartsWith("#")) &&
                    (!line.StartsWith("'")) &&
                    (line.Contains('=')))
                {
                    int index = line.IndexOf('=');
                    //Debug.Log("index:" + index);
                    String key = line.Substring(0, index).Trim().Trim('"');
                    String value = line.Substring(index + 1).Trim().Trim('"',';');

                    if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    try
                    {
	                    //ignore dublicates
	                    //Debug.Log("key:" + key + ", value:" + value);
	                    _list.Add(key, value);
                    }
                    catch
                    {
	                    //ignore
                    }
                }
            }
        }
    }
}