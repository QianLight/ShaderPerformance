using CFEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;


namespace XEditor
{
    public partial class OriginalSyncLoadEditor
    {
        static readonly GUIContent fx_process = new GUIContent("Process Fx", "自动处理所有特效，用来生成特效配置");
        static readonly GUIContent fx_load = new GUIContent("Load All", "动态加载所有的特效");
        static readonly GUIContent fx_clean = new GUIContent("Clean All", "重置所有的特效配置");
        static readonly GUIContent fx_rm = new GUIContent("Rm NoReference", "清除没有引用的fx");
        static readonly GUIContent fx_layer = new GUIContent("Layer Process", "刷特效Layer");

        private void GuiFxs()
        {
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(fx_load))
            {
                LoadAllFx();
            }
            if (GUILayout.Button(fx_process))
            {
                ProcessFx();
            }
            if (GUILayout.Button(fx_clean))
            {
                CleanFx();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(fx_rm))
            {
                RmNoRefrences();
            }
            if (GUILayout.Button(fx_layer))
            {
                ProcessFxLayer();
            }
            GUILayout.EndHorizontal();
            if (fx_statistic > RTimeline.fx_max)
            {
                var mx = RTimeline.fx_max;
                var t = string.Format("fx count overange, current:{0} max:{1}", fx_statistic, mx);
                EditorGUILayout.HelpBox(t, MessageType.Error);
            }
        }

        private void CleanFx()
        {
            foreach (var tr in fx_tracks)
            {
                var clips = tr.GetClips();
                foreach (var clip in clips)
                {
                    var asset = clip.asset as ControlPlayableAsset;
                    asset.fxData = null;
                }
            }
        }

        private void ProcessFxLayer()
        {
            SetupRoot();

            foreach (var tr in fx_tracks)
            {
                var clips = tr.GetClips();
                foreach (var c in clips)
                {
                    var asset = c.asset as ControlPlayableAsset;
                    var obj = asset.sourceGameObject.Resolve(director);
                    if (obj != null)
                    {
                        var ps = obj.transform.GetComponentsInChildren<ParticleSystem>();
                        foreach (var it in ps)
                        {
                            Renderer r = it.GetComponent<Renderer>();
                            r.renderingLayerMask |= DefaultGameObjectLayer.SRPLayer_DefaultMask;
                            Debug.Log("render: " + r.name);
                        }
                    }
                }
            }
        }

        private void RmNoRefrences()
        {
            var tlName = "Timeline/" + director.playableAsset.name;
            bool find = false;
            foreach (var tr in fx_tracks)
            {
                var clips = tr.GetClips();
                foreach (var clip in clips)
                {
                    var asset = clip.asset as ControlPlayableAsset;
                    var go = asset.sourceGameObject.Resolve(director);
                    if (go == null)
                    {
                        tr.RemoveClip(clip);
                        find = true;
                    }
                }
            }
            if (find)
            {
                director.RebuildGraph();
            }
        }

        private void ProcessFx()
        {
            fx_statistic = 0;
            var tlName = "Timeline/" + director.playableAsset.name;
            foreach (var tr in fx_tracks)
            {
                var clips = tr.GetClips();
                foreach (var clip in clips)
                {
                    var asset = clip.asset as ControlPlayableAsset;

                    // close update for efficient
                    asset.updateITimeControl = false;
                    asset.updateDirector = false;
                    asset.searchHierarchy = true;

                    // record control asset's info
                    var go = asset.sourceGameObject.Resolve(director);
                    if (go)
                    {
                        bool boneFx = AnalyFx(go, out var role, out string bonePat);

                        var pat = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                        if (!string.IsNullOrEmpty(pat))
                        {
                            pat = pat.Replace("Assets/BundleRes/", string.Empty).Replace(".prefab", string.Empty);
                            if (pat == tlName)
                            {
                                Debug.LogWarning(tr.name + " " + pat + " is not valid fx");
                                continue;
                            }
                            if (asset.fxData == null)
                                asset.fxData = new ControlFxData();
                            asset.fxData.path = pat;
                            var tf = go.transform;
                            asset.fxData.pos = tf.localPosition;
                            asset.fxData.rot = tf.localRotation;
                            asset.fxData.scale = tf.localScale;
                            asset.fxData.avatar = boneFx ? role.name : string.Empty;
                            asset.fxData.bonePath = bonePat;
                            float start = (float)clip.start;
                            start = Mathf.Max(0, start - Random.Range(0, 0.1f));
                            asset.fxData.loadtime = start;
                            fx_statistic++;
                        }
                    }
                }
            }
        }


        public void LoadAllFx()
        {
            SetupRoot();

            foreach (var tr in fx_tracks)
            {
                var clips = tr.GetClips();
                foreach (var c in clips)
                {
                    var asset = c.asset as ControlPlayableAsset;
                    if (asset?.fxData != null)
                    {
                        var dat = asset.fxData;
                        if (!string.IsNullOrEmpty(dat.path))
                        {
                            var obj = asset.sourceGameObject.Resolve(director);
                            if (obj != null && dat.path.Contains(obj.name))
                                continue;

                            var pat = "Assets/BundleRes/" + dat.path + ".prefab";
                            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pat);

                            if (prefab == null)
                            {
                                EditorUtility.DisplayDialog("warn", "fx is not exist in disk, path: " + pat, "ok");
                                continue;
                            }
                            var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                            if (string.IsNullOrEmpty(dat.avatar))
                            {
                                if (fx_root) go.transform.parent = fx_root.transform;
                            }
                            else
                            {
                                BindFx2Char(dat, go, asset);
                            }
                            go.transform.localPosition = dat.pos;
                            go.transform.localRotation = dat.rot;
                            go.transform.localScale = dat.scale;
                            director.SetReferenceValue(asset.sourceGameObject.exposedName, go);
                        }
                    }
                }
            }
        }

        private void BindFx2Char(ControlFxData dat, GameObject fx, ControlPlayableAsset asset)
        {
            for (int i = 0; i < m_char_data?.Length; i++)
            {
                var ch = m_char_data[i];
                if (ch == null) continue;

                if (asset.m_roleIndex != -1 && i == (asset.m_roleIndex - 1))
                {
                    Transform child = null;
                    if (ch.tf != null)
                    {
                        child = (ch.tf as Transform).Find(dat.bonePath);
                    }
                    else if (ch.xobj != null)
                    {
                        child = ch.xobj.Find(dat.bonePath);
                    }
                    if (child != null)
                    {
                        fx.transform.parent = child;
                        break;
                    }
                }
                else if (ch.prefab == dat.avatar)
                {
                    Transform child = null;
                    if (ch.tf != null)
                    {
                        child = (ch.tf as Transform).Find(dat.bonePath);
                    }
                    else if (ch.xobj != null)
                    {
                        child = ch.xobj.Find(dat.bonePath);
                    }
                    if (child != null)
                    {
                        fx.transform.parent = child;
                    }
                }
            }
        }

        private void UnloadFxs()
        {
            foreach (var tr in fx_tracks)
            {
                var clips = tr.GetClips();
                foreach (var clip in clips)
                {
                    var asset = clip.asset as ControlPlayableAsset;
                    director.SetReferenceValue(asset.sourceGameObject.exposedName, null);
                }
            }
        }

        private bool AnalyFx(GameObject fx, out GameObject role, out string path)
        {
            var tf = fx.transform;
            path = string.Empty;
            while (tf.parent)
            {
                if (tf.name == "root") // Avatar root bone
                {
                    path = path.Remove(path.Length - 1);
                    role = tf.parent.gameObject;
                    return true;
                }
                tf = tf.parent;
                path = tf.name + "/" + path;
            }

            if (path.StartsWith("timeline_Role_Root"))
            {
                path = path.TrimEnd('/');
                string[] strs = path.Split('/');
                if (strs.Length >= 2)
                {
                    string rootPath = strs[0] + "/" + strs[1];
                    role = GameObject.Find(rootPath);
                    path = path.Replace(rootPath, string.Empty);
                    path = path.TrimStart('/');
                    return true;
                }
                else
                {
                    role = null;
                    path = string.Empty;
                    return false;
                }
            }

            role = null;
            return false;
        }
    }
}