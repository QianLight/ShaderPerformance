using System;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TDTools
{
    public class CARUtility
    {
#region CARUtility
        public static void TreeToList<T> (T root, IList<T> result) where T : TreeViewElement
        {
            if (result == null)
                throw new NullReferenceException("The input 'IList<T> result' list is null");
            result.Clear ();

            Stack<T> stack = new Stack<T> ();
            stack.Push (root);

            while (stack.Count > 0)
            {
                T current  = stack.Pop ();
                result.Add (current);

                if (current.children != null && current.children.Count > 0)
                {
                    for (int i = current.children.Count - 1; i >= 0; --i)
                        stack.Push ((T)current.children[i]);
                }
            }
        }

        public static T ListToTree<T> (IList<T> list) where T : TreeViewElement
        {
            ValidateDepthValue (list);

            foreach (T element in list)
            {
                element.parent = null;
                element.children = null;
            }

            RecursivelyFindChildren (list, 0);
            return list[0];
        }

        public static void ValidateDepthValue<T> (IList<T> list) where T : TreeViewElement
        {
            if (list.Count == 0)
                throw new ArgumentException ("list should have items, count is 0, check before calling ValidateDepthValue", "list");
            
            if (list[0].depth != -1 )
                throw new ArgumentException ("list item at index 0 should have a depth of -1. Depth is: " + list[0].depth, "list");
            
            for (int i = 0; i < list.Count - 1; ++i)
            {
                int depth = list[i].depth;
                int nextDepth = list[i + 1].depth;
                if (nextDepth - depth > 1)
                    throw new ArgumentException (string.Format("Invalid depth info. Depth cannot increase more than 1 per row. Index {0} has depth {1} while index {2} has depth {3}", i, depth, i + 1, nextDepth));
            }

            for (int i = 1; i < list.Count; ++i)
                if (list[i].depth < 0)
                    throw new ArgumentException ($"Invalid depth value for item at index {i}. Only the first item should have depth less than 0.");
            
            if (list.Count > 1 && list[1].depth != 0)
                throw new ArgumentException ("Input list item at index 1 should have a depth of 0.", "list");
        }

        public static void RecursivelyFindChildren<T> (IList<T> list, int currentIndex) where T : TreeViewElement
        {
            List<TreeViewElement> childList = new List<TreeViewElement>();
            for (int i = currentIndex + 1; i < list.Count; ++i)
            {
                if (list[i].depth <= list[currentIndex].depth)
                    break;

                if (list[i].depth == list[currentIndex].depth + 1)
                {
                    childList.Add (list[i]);
                    list[i].parent = list[currentIndex];
                    RecursivelyFindChildren (list, i);
                }
            }

            if (childList.Count == 0)
                childList = null;

            list[currentIndex].children = childList;
        }

        public static IList<T> FindCommonAncestorsWithinList<T> (IList<T> elements) where T : TreeViewElement
        {
            if (elements.Count == 1)
                return new List<T>(elements);
            
            List<T> result = new List<T>(elements);
            result.RemoveAll (g => IsChildOf (g, elements));
            return result;
        }

        public static void UpdateDepthValues<T> (T root) where T : TreeViewElement
        {
            if (root == null)
                throw new ArgumentException ("root", "The root is null.");

            if (!root.hasChildren)
                return;

            Stack<TreeViewElement> stack = new Stack<TreeViewElement>();
            stack.Push (root);
            while (stack.Count > 0)
            {
                TreeViewElement current = stack.Pop ();
                if (current.children != null)
                {
                    foreach (var child in current.children)
                    {
                        child.depth = current.depth + 1;
                        stack.Push (child);
                    }
                }
            }
        }

        static bool IsChildOf<T> (T child, IList<T> elements) where T : TreeViewElement
        {
            while (child != null)
            {
                child = (T) child.parent;
                if (elements.Contains (child))
                    return true;
            }
            return false;
        }

#endregion

#region RefreshAssets
        public static void RefreshArtAssets (CARTreeAsset asset)
        {
            RefreshModelArtAssets (asset);
            RefreshAnimationArtAssets (asset);
            RefreshEffectArtAssets (asset);
            RefreshIDs (false, asset);
        }

        public static void RefreshModelArtAssets (CARTreeAsset asset)
        {
            asset.modelArtAssets.Clear ();

            string folderPath = Application.dataPath + "/BundleRes/Prefabs";
            DirectoryInfo directoryInfo = new DirectoryInfo (folderPath);
        
            string path = directoryInfo.FullName.Substring (directoryInfo.FullName.IndexOf ("Assets") + 7);
            asset.modelArtAssets.Add (new ModelArtAsset (ArtAssetEnum.Model, "Prefabs", 0, 0, path));
            AddAssetRecursively (directoryInfo, 1, asset.modelArtAssets.Cast<TreeViewElement>().ToList(), ArtAssetEnum.Model, asset);
        }

        public static void RefreshAnimationArtAssets (CARTreeAsset asset)
        {
            asset.animationArtAssets.Clear ();

            string folderPath = Application.dataPath + "/BundleRes/Animation";
            DirectoryInfo directoryInfo = new DirectoryInfo (folderPath);
        
            string path = directoryInfo.FullName.Substring (directoryInfo.FullName.IndexOf ("Assets") + 7);
            asset.animationArtAssets.Add (new AnimationArtAsset (ArtAssetEnum.Animation, "Animation", 0, 0, path));
            AddAssetRecursively (directoryInfo, 1, asset.animationArtAssets.Cast<TreeViewElement>().ToList(), ArtAssetEnum.Animation, asset);
        }

        public static void RefreshEffectArtAssets (CARTreeAsset asset)
        {
            asset.effectArtAssets.Clear ();

            string folderPath = Application.dataPath + "/BundleRes/Effects";
            DirectoryInfo directoryInfo = new DirectoryInfo (folderPath);
        
            string path = directoryInfo.FullName.Substring (directoryInfo.FullName.IndexOf ("Assets") + 7);
            asset.effectArtAssets.Add (new EffectArtAsset (ArtAssetEnum.Effect, "Effects", 0, 0, path));
            AddAssetRecursively (directoryInfo, 1, asset.effectArtAssets.Cast<TreeViewElement>().ToList(), ArtAssetEnum.Effect, asset);
        }

        public static void RefreshConfigAssets (CARTreeAsset asset)
        {
            RefreshTimelineConfigAssets (asset);
            RefreshLevelConfigAssets (asset);
            RefreshSkillConfigAssets (asset);
            RefreshBehitConfigAssets (asset);
            RefreshIDs (true, asset);
        }

        public static void RefreshTimelineConfigAssets (CARTreeAsset asset)
        {
            asset.timelineConfigAssets.Clear ();

            string folderPath = Application.dataPath + "/BundleRes/Timeline";
            DirectoryInfo directoryInfo = new DirectoryInfo (folderPath);

            string path = directoryInfo.FullName.Substring (directoryInfo.FullName.IndexOf ("Assets") + 7);
            asset.timelineConfigAssets.Add (new TimelineConfigAsset (ConfigAssetEnum.Timeline, "Timeline", 0, 0, path));
            AddAssetRecursively (directoryInfo, 1, asset.timelineConfigAssets.Cast<TreeViewElement>().ToList(), ConfigAssetEnum.Timeline, asset);
        }

        public static void RefreshLevelConfigAssets (CARTreeAsset asset)
        {
            asset.levelConfigAssets.Clear ();

            string folderPath = Application.dataPath + "/BundleRes/Table/Level";
            DirectoryInfo directoryInfo = new DirectoryInfo (folderPath);
        
            string path = directoryInfo.FullName.Substring (directoryInfo.FullName.IndexOf ("Assets") + 7);
            asset.levelConfigAssets.Add (new LevelConfigAsset (ConfigAssetEnum.Level, "Level", 0, 0, path));
            AddAssetRecursively (directoryInfo, 1, asset.levelConfigAssets.Cast<TreeViewElement>().ToList(), ConfigAssetEnum.Level, asset);
        }

        public static void RefreshSkillConfigAssets (CARTreeAsset asset)
        {
            asset.skillConfigAssets.Clear ();

            string folderPath = Application.dataPath + "/BundleRes/SkillPackage";
            DirectoryInfo directoryInfo = new DirectoryInfo (folderPath);
        
            string path = directoryInfo.FullName.Substring (directoryInfo.FullName.IndexOf ("Assets") + 7);
            asset.skillConfigAssets.Add (new SkillConfigAsset (ConfigAssetEnum.Skill, "SkillPackage", 0, 0, path));
            AddAssetRecursively (directoryInfo, 1, asset.skillConfigAssets.Cast<TreeViewElement>().ToList(), ConfigAssetEnum.Skill, asset);
        }

        public static void RefreshBehitConfigAssets (CARTreeAsset asset)
        {
            asset.behitConfigAssets.Clear ();

            string folderPath = Application.dataPath + "/BundleRes/HitPackage";
            DirectoryInfo directoryInfo = new DirectoryInfo (folderPath);
        
            string path = directoryInfo.FullName.Substring (directoryInfo.FullName.IndexOf ("Assets") + 7);
            asset.behitConfigAssets.Add (new BehitConfigAsset (ConfigAssetEnum.Behit, "HitPackage", 0, 0, path));
            AddAssetRecursively (directoryInfo, 1, asset.behitConfigAssets.Cast<TreeViewElement>().ToList(), ConfigAssetEnum.Behit, asset);
        }

        public static void RefreshReferences (CARTreeAsset asset)
        {
            RefreshModelArtReferences (asset);
            RefreshAnimationArtReferences (asset);
            RefreshEffectArtReferences (asset);
        }

        public static void RefreshModelArtReferences (CARTreeAsset asset)
        {
            
        }

        public static void RefreshAnimationArtReferences (CARTreeAsset asset)
        {
            
        }

        public static void RefreshEffectArtReferences (CARTreeAsset asset)
        {
            
        }

        public static List<Commit> FindCommits (string filePath, int maxCount = 1)
        {
            List<Commit> commits = new List<Commit> ();


            return commits;
        }

        public static int GetDepthFromDirectorySplit (string fullPath, string searchString)
        {
            return fullPath.Substring (fullPath.IndexOf (searchString)).Split('\\').Length - 1;
        }

        static void AddAssetRecursively (DirectoryInfo parentDirectoryInfo, int depth, IList<TreeViewElement> result, Enum e, CARTreeAsset asset)
        {
            foreach (var directoryInfo in parentDirectoryInfo.EnumerateDirectories())
            {
                string name = directoryInfo.Name;
                string path = directoryInfo.FullName.Substring (directoryInfo.FullName.IndexOf ("Assets") + 7);
                AddElement (name, depth, 0, path, e, result); 
                AddAssetRecursively (directoryInfo, depth + 1, result, e, asset);
            }

            foreach (var fileInfo in parentDirectoryInfo.EnumerateFiles(CARTreeAsset.dict[e]))
            {
                string name = Path.GetFileNameWithoutExtension (fileInfo.Name);
                string path = fileInfo.FullName.Substring (fileInfo.FullName.IndexOf ("Assets") + 7);
                AddElement (name, depth, 0, path, e, result);
            }

            if (depth == 1)
                DuplicateAssets (result, e, asset);
        }

        static void AddElement (string name, int depth, int id, string path, Enum e, IList<TreeViewElement> result)
        {
            switch (e)
            {
                case ConfigAssetEnum.Timeline:
                    result.Add (new TimelineConfigAsset (ConfigAssetEnum.Timeline, name, depth, id, path));
                    break;
                case ConfigAssetEnum.Level:
                    result.Add (new LevelConfigAsset (ConfigAssetEnum.Level, name, depth, id, path));
                    break;
                case ConfigAssetEnum.Skill:
                    result.Add (new SkillConfigAsset (ConfigAssetEnum.Skill, name, depth, id, path));
                    break;
                case ConfigAssetEnum.Behit:
                    result.Add (new BehitConfigAsset (ConfigAssetEnum.Behit, name, depth, id, path));
                    break;
                case ArtAssetEnum.Animation:
                    result.Add (new AnimationArtAsset (ArtAssetEnum.Animation, name, depth, id, path));
                    break;
                case ArtAssetEnum.Model:
                    result.Add (new ModelArtAsset (ArtAssetEnum.Model, name, depth, id, path));
                    break;
                case ArtAssetEnum.Effect:
                    result.Add (new EffectArtAsset (ArtAssetEnum.Effect, name, depth, id, path));
                    break;
                default:
                    throw new ArgumentException ("Wrong enum type.");
            }
        }

        static void DuplicateAssets (IList<TreeViewElement> result, Enum e, CARTreeAsset asset)
        {
            switch (e)
            {
                case ConfigAssetEnum.Timeline:
                    asset.timelineConfigAssets = result.Cast<TimelineConfigAsset>().ToList();
                    break;
                case ConfigAssetEnum.Level:
                    asset.levelConfigAssets = result.Cast<LevelConfigAsset>().ToList();
                    break;
                case ConfigAssetEnum.Skill:
                    asset.skillConfigAssets = result.Cast<SkillConfigAsset>().ToList();
                    break;
                case ConfigAssetEnum.Behit:
                    asset.behitConfigAssets = result.Cast<BehitConfigAsset>().ToList();
                    break;
                case ArtAssetEnum.Animation:
                    asset.animationArtAssets = result.Cast<AnimationArtAsset>().ToList();
                    break;
                case ArtAssetEnum.Model:
                    asset.modelArtAssets = result.Cast<ModelArtAsset>().ToList();
                    break;
                case ArtAssetEnum.Effect:
                    asset.effectArtAssets = result.Cast<EffectArtAsset>().ToList();
                    break;
                default:
                    throw new ArgumentException ("Wrong enum type.");
            }
        }

        public static void RefreshIDs (bool isConfig, CARTreeAsset asset)
        {
            int countID = 0;

            if (isConfig)
            {
                foreach (var a in asset.timelineConfigAssets)
                    a.id = countID++;
                foreach (var a in asset.levelConfigAssets)
                    a.id = countID++;
                foreach (var a in asset.skillConfigAssets)
                    a.id = countID++;
                foreach (var a in asset.behitConfigAssets)
                    a.id = countID++;
            }
            else
            {
                foreach (var a in asset.modelArtAssets)
                    a.id = countID++;
                foreach (var a in asset.animationArtAssets)
                    a.id = countID++;
                foreach (var a in asset.effectArtAssets)
                    a.id = countID++;
            }
        }
#endregion

#region GUI styles
        static GUIStyle m_firstHeadingStyle;
        static GUIStyle m_secondHeadingStyle;
        static GUIStyle m_boldLabelStyle;
        static GUIStyle m_layoutStyle;
        public static GUIStyle firstHeadingStyle
        {
            get
            {
                if (m_firstHeadingStyle == null)
                {
                    m_firstHeadingStyle = new GUIStyle (GUI.skin.label)
                    {
                        normal = new GUIStyleState () {textColor = Color.white},
                        fontSize = 18,
                        fontStyle = FontStyle.Bold,
                    };
                }
                return m_firstHeadingStyle;
            }
        }

        public static GUIStyle secondHeadingStyle
        {
            get
            {
                if (m_secondHeadingStyle == null)
                {
                    m_secondHeadingStyle = new GUIStyle (GUI.skin.label)
                    {
                        normal = new GUIStyleState () {textColor = Color.white},
                        fontSize = 15,
                        fontStyle = FontStyle.Bold,
                    };
                }
                return m_secondHeadingStyle;
            }
        }

        public static GUIStyle boldLabelStyle
        {
            get 
            {
                if (m_boldLabelStyle == null)
                {
                    m_boldLabelStyle = new GUIStyle (GUI.skin.label)
                    {
                        normal = new GUIStyleState () {textColor = Color.white},
                        fontStyle = FontStyle.Bold,
                    };
                }
                return m_boldLabelStyle;
            }
        }
    }
#endregion

#region CommandRunner

    public class CommandRunner
    {   
        string m_FilePath;
        public string filePath
        {
            get
            {
                if (m_FilePath == null)
                {
                    XmlDocument config = new XmlDocument ();
                    config.Load (Application.dataPath + "/Editor/TDTools/CheckAssetReference/config.xml");
                    XmlNode node = config.DocumentElement.SelectSingleNode ("/filePath");
                    m_FilePath = node.InnerText;
                }
                return m_FilePath;
            }
        }

        public CommandRunner () { }

        public List<Commit> Run (string arguments, string workingDirectory)
        {
            var info =  new ProcessStartInfo (filePath, arguments)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };

            var process = new Process () { StartInfo = info };
            process.Start ();
            string output = process.StandardOutput.ReadToEnd ();
            process.WaitForExit ();
            process.Close ();

            return GetCommitsFromString (output);
        }

        List<Commit> GetCommitsFromString (string output)
        {
            List<Commit> commits = new List<Commit> ();

            string[] strings = output.Split ('\n');
            foreach (string s in strings)
            {
                commits.Add (GetSingleCommitFromString (s));
            }

            return commits;
        }

        Commit GetSingleCommitFromString (string s)
        {
            string[] split = s.Split ('|');
            string id = split[0].Substring (split[0].Length - 6);
            string author = split[1];;
            string date = split[2];
            string message = split[3];

            Commit commit = new Commit (id, author, message, date);
            return commit;
        }
    }

#endregion
}