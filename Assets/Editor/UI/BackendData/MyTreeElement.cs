using System;
using System.Collections.Generic;
using UIAnalyer;
using UnityEngine;
using UnityEngine.CFUI;
using Random = UnityEngine.Random;


namespace UnityEditor.TreeViewExamples
{

    [Serializable]
	internal class MyTreeElement : TreeElement
	{
        public string lableName;
        public string lableValue;
		public string prefabName;
		public string shirtName;
        public string lablePath;
		public GameObject go;
        
		public bool enabled;

		public MyTreeElement (string name, int depth, int id) : base (name, depth, id)
		{
			enabled = true;
		}

		public void Setup(CFText ct , string prefab)
		{
            lableName = ct.name;
            lableValue = ct.text;
            prefabName = prefab.Replace('\\', '/' );

            lablePath = GetPath(ct.transform);
            shirtName = ToShortName(prefab);

        }
        public static string GetPath(Transform transform)
        {
            string fname = transform.name;
            while (transform.parent != null && transform.parent.parent != null)
            {
                fname = transform.parent.name + "/" + fname;
                transform = transform.parent;
            }
            return fname;
        }
        public virtual string ToShortName(string path)
        {
            return path.Substring(path.LastIndexOf("\\") + 1).Replace(".prefab","");
        }
    }
}
