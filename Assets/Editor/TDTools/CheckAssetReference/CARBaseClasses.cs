using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.Timeline;
using System;
using System.IO;
using XEditor;


namespace TDTools
{
    public enum ArtAssetEnum
    {
        Model,
        Animation,
        Effect
    }

    public enum ConfigAssetEnum
    {
        Timeline,
        Level,
        Skill,
        Behit
    }

    interface IOpen
    {
        void Open ();
    }

    [Serializable]
    public class Commit
    {
        [SerializeField] string m_Id;
        [SerializeField] string m_Author;
        [SerializeField] string m_Message;
        [SerializeField] string m_Date;

        public string id { get { return m_Id; } set { m_Id = value; } }
        public string author { get { return m_Author; } set { m_Author = value; } }
        public string message { get { return m_Message; } set { m_Message = value; } }
        public string date { get { return m_Date; } set { m_Date = value; } }

        public Commit () { }

        public Commit (string id, string author, string message, string date)
        {
            m_Id = id;
            m_Author = author;
            m_Message = message;
            m_Date = date;
        }
    }

    [Serializable]
    public class TreeViewElement
    {
        [SerializeField] int m_ID;
        [SerializeField] int m_Depth;
        [SerializeField] string m_Name;
        [SerializeField] string m_Path;  // m_Path is the file path following "Assets/"
        [NonSerialized] TreeViewElement m_Parent;
        [NonSerialized] List<TreeViewElement> m_Children;

        public int id
        {
            get { return m_ID; } set { m_ID = value; }
        }

        public int depth
        {
            get { return m_Depth; } set { m_Depth = value; }
        }

        public string name
        {
            get { return m_Name; } set { m_Name = value; }
        }

        public string path
        {
            get { return m_Path; } set { m_Path = value; }
        }

        public TreeViewElement parent
        {
            get { return m_Parent; } set { m_Parent = value; }
        }

        public List<TreeViewElement> children
        {
            get { return m_Children; } set { m_Children = value; }
        }

        public bool hasChildren
        {
            get { return children != null && children.Count > 0; }
        }

        public TreeViewElement () { }
        public TreeViewElement (string name, int depth, int id)
        {
            m_Name = name;
            m_Depth = depth;
            m_ID = id;
        }

        public virtual void Open () 
        {
            Debug.Log ("Base Open () called!");
        }
    }

    [Serializable]
    public class ArtAsset : TreeViewElement
    {
        [SerializeField] ArtAssetEnum m_Type;
        [SerializeField] List<ConfigAsset> m_References;

        public ArtAssetEnum type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        public List<ConfigAsset> references
        {
            get { return m_References; }
            set { m_References = value; }
        }

        public ArtAsset () : base () { }
        public ArtAsset (ArtAssetEnum type, 
                         string name, 
                         int depth,
                         int id) : base (name, depth, id)
        {
            m_Type = type;
        }
    }

    [Serializable]
    public class ConfigAsset : TreeViewElement
    {
        [SerializeField] ConfigAssetEnum m_Type;

        public ConfigAssetEnum type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        public ConfigAsset () : base () { }
        public ConfigAsset (ConfigAssetEnum type,
                            string name,
                            int depth,
                            int id) : base (name, depth, id)
        {
            m_Type = type;
        }
    }
}