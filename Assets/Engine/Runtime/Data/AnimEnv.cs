using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
namespace CFEngine
{
#if UNITY_EDITOR
    public class EnvOpContext
    {
        public delegate void OnAddParam (EnvParam envParam);
        public delegate void OnRemoveParam (EnvParam envParam);
        public delegate void OnClearParam ();

        public delegate void OnMaskChange (EnvParam envParam);
        public delegate void OnAnimMaskChange (EnvParam envParam);

        public delegate void OnValueChange (EnvParam envParam);
        public OnAddParam onAdd;
        public OnRemoveParam onRemove;
        public OnClearParam onClear;
        public OnAnimMaskChange onAnimMaskChange;
        public OnValueChange onValueChange;
    }
#endif
    [Serializable]
    public sealed class AnimEnvProfile : IAnimEnv
    {
        public AnimEnvParam[] animEnvParam;
        public AnimPack animPack = new AnimPack ();
#if UNITY_EDITOR
       // [NonSerialized]
        //public EnvAreaProfile profile;
        // [NonSerialized]
        [NonSerialized]
        public bool test = false;
        private EnvOpContext envOP = new EnvOpContext ();

        private Transform t;
        private PlayableDirector pd;
        private List<AnimContextBase> editingAnims = new List<AnimContextBase> ();
        private float editTime = 0;

        private static AnimEnvParam tmpAEP = new AnimEnvParam ();

        public void Init (Transform t)
        {
            if (t != null)
                this.t = t;
            envOP.onAnimMaskChange = OnAnimMaskChanged;
            envOP.onValueChange = OnValueChanged;
            //find profile & pd
            pd = null;
            if (t != null)
            {
                var tt = t;
                while (tt.parent != null)
                {
                    tt = tt.parent;
                }
                pd = tt.GetComponentInChildren<PlayableDirector> ();
            }
            /*if (pd != null)
            {
                var asset = pd.playableAsset;
                if (asset != null && profile == null)
                {
                    string assetPath;
                    string dir = AssetsPath.GetAssetDir (asset, out assetPath);
                    string path = string.Format ("{0}/{1}_env.asset", dir, asset.name);
                    profile = AssetDatabase.LoadAssetAtPath<EnvAreaProfile> (path);
                }
            }
            if (profile != null)
            {
                EnvArea.BindEnvBlock (profile.envBlock,
                    t, true);
            }*/
        }

        public List<AnimContextBase> GetEditAnims ()
        {
            return editingAnims;
        }

        public void RefreshEditAnims ()
        {
            editingAnims.Clear ();
            /*if (profile != null)
            {
                for (int i = 0; i < profile.envBlock.envParams.Count; ++i)
                {
                    var envParam = profile.envBlock.envParams[i];
                    if (envParam.param != null)
                    {
                        envParam.param.GetEditingCurve(editingAnims, envParam);
                    }
                }
            }*/
        }

        public void SetEditTime (float time)
        {
            editTime = time;
        }
        private static EnvAreaProfile CreateEnvProfile (PlayableAsset asset)
        {
            string assetPath;
            string dir = AssetsPath.GetAssetDir (asset, out assetPath);
            var profile = EnvAreaProfile.CreateInstance<EnvAreaProfile> ();
            string assetname = asset.name + "_env";
            profile.name = assetname;
            return EditorCommon.CreateAsset<EnvAreaProfile> (dir, profile.name, ".asset", profile);
        }
        void OnAnimMaskChanged (EnvParam envParam)
        {
            RefreshEditAnims ();
        }

        void OnValueChanged (EnvParam envParam)
        {
            envParam.param.SetCurveValue(editTime, envParam);
        }

        void OnSave ()
        {
            animEnvParam = null;
            animPack.Reset ();
            //if (profile != null)
            {
                /*for (int i = 0; i < profile.envBlock.envParams.Count; ++i)
                {
                    var envParam = profile.envBlock.envParams[i];
                    if(envParam.param!=null)
                    {
                        AnimEnvParam aep = new AnimEnvParam();
                        aep.hash = envParam.hash;
                        aep.valueMask = envParam.valueMask;
                        animPack.animParam.Add(aep);
                        envParam.param.SaveAnim(aep, ref animPack);
                    }
        
                }*/
                
                //for (int i = 0; i < profile.envBlock.envParams.Count; ++i)
                //{
                //    var envParam = profile.envBlock.envParams[i];
                //    if (envParam.paramWrapper != null)
                //    {
                //        envParam.paramWrapper.SaveAnim (envParam, ref animPack);
                //    }
                //}
                animPack.OnPostSave (ref animEnvParam);
                BindAnimEnvParam ();
            }
            if (t != null)
            {
                var tt = t;
                while (tt.parent != null)
                {
                    tt = tt.parent;
                }
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource (tt.gameObject) as GameObject;
                string path = AssetDatabase.GetAssetPath (prefab);
                PrefabUtility.SaveAsPrefabAsset (tt.gameObject, path);
            }
        }

        public void OnInspectorGUI ()
        {
            /*EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.ObjectField (profile, typeof (EnvAreaProfile), false);
            if (profile == null)
            {
                PlayableAsset asset = pd != null?pd.playableAsset : null;
                if (asset != null)
                {
                    if (GUILayout.Button ("Create", GUILayout.MaxWidth (80)))
                    {
                        profile = CreateEnvProfile (asset);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField ("Create Timeline Asset First");
                }
            }
            else
            {
                if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
                {
                    EditorCommon.SaveAsset (profile);
                    OnSave ();
                }
                if (GUILayout.Button (test? "Testing": "Test", GUILayout.MaxWidth (80)))
                {
                    test = !test;
                    if (test)
                    {
                        BindAnimEnvParam ();
                    }
                }
            }
            EditorGUILayout.EndHorizontal ();

            if (profile != null)
            {
                EnvArea.OnEnvBlockGUI (profile, profile.envBlock, null, false, true, envOP);
            }*/
        }
#endif
        public void BindAnimEnvParam ()
        {
            if (animEnvParam != null)
            {
                for (int i = 0; i < animEnvParam.Length; ++i)
                {
                    var ep = animEnvParam[i];
                    ep.BindAnim (ep.hash, ref animPack);
                }
            }
        }
        public void BindEnvParam (Transform t)
        {

#if UNITY_EDITOR
            this.t = t;
            Init (t);
#endif
            BindAnimEnvParam ();
        }
        public void UnInit ()
        {
            if (animEnvParam != null)
            {
                for (int i = 0; i < animEnvParam.Length; ++i)
                {
                    var aep = animEnvParam[i];
                    var runtime = aep.runtimeParam != null ? aep.runtimeParam.runtime : null;
                    if (runtime != null)
                    {
                        runtime.overrideState = false;
                        aep.runtimeParam.SetDirty ();
                    }
                }
            }
        }
        public void Update (float t)
        {
#if UNITY_EDITOR
            AnimEnvParam.test = test;
            if (!EngineContext.IsRunning && !test)
            {
                /*if (profile != null)
                {
                    for (int i = 0; i < profile.envBlock.envParams.Count; ++i)
                    {
                        var envParam = profile.envBlock.envParams[i];
                        tmpAEP.envParam = envParam;
                        tmpAEP.hash = envParam.hash;
                        tmpAEP.runtimeParam = envParam.runtimeParam;
                        tmpAEP.Update (t, ref animPack);
                    }
                }*/
            }
            else
#endif
            {
                if (animEnvParam != null)
                {
                    for (int i = 0; i < animEnvParam.Length; ++i)
                    {
                        var ep = animEnvParam[i];
                        /*if (ep.runtimeParam.runtime is ColorParam && animPack.cCurve.Length <= 0)
                        {
                            DebugLog.AddErrorLog($"animEnvParam from {this.t.name}, index{i} is empty colorparam");
                        }*/
                        ep.Update (t, ref animPack);
                    }
                }
            }
        }
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class AnimEnv : MonoBehaviour
    {
        public AnimEnvProfile animEnvProfile = new AnimEnvProfile ();
        public static AnimEnvProfile currentAE;

        public static IAnimEnv GetAnimEnv (GameObject go)
        {
            currentAE = null;
            var t = go.transform;
            var env = t.Find ("Env");
            if (env != null)
            {
                AnimEnv ae;
                if (env.TryGetComponent (out ae))
                {
                    var aep = ae.animEnvProfile;
                    if (aep != null)
                    {
                        currentAE = aep;
                        aep.BindEnvParam (ae.transform);
                    }
                    return aep;
                }
            }
            return null;
        }
    }
}