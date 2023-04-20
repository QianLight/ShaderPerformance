#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    public class SFXSequence : Sequence
    {

    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public partial class SFXWrapper : MonoBehaviour, IMatObject, ISceneAnim
    {
        public bool notControl;
        public bool customSample;
        public uint areaMask = 0xffffffff;
        public string exString = "";
        public FlagMask flag;
        public List<GameObject> avoidToei;
        [NonSerialized]
        public SFXAsset asset;
        [NonSerialized]
        public TextAsset data;
        [NonSerialized]
        public SFXConfig config;
        [NonSerialized]
        public Transform sfx;
        [NonSerialized]
        public List<SFXData> sfxComps = new List<SFXData> ();
        [NonSerialized]
        public Animator ator = null;
        [NonSerialized]
        public Animation anim = null;
        [NonSerialized]
        private string stateName;
        [NonSerialized]
        public bool isPlaying = false;
        [NonSerialized]
        public float duration = -1;
        [NonSerialized]
        public float time = 0;
        [NonSerialized]
        public float timeScale = 1;
        [NonSerialized]
        public bool lockTime = false;
        [NonSerialized]
        public SFXSequence sequence;

        private List<SFXProcessData> processor;
        private static GUI.WindowFunction sfxSceneGUI = DrawSFXWindow;
        public static HashSet<SFXWrapper> currentSFXs = new HashSet<SFXWrapper> ();
        private static List<SFXWrapper> removeSfx = new List<SFXWrapper> ();
        private void OnDisable ()
        {
            Reset ();
        }
        private void RefreshComponent (Transform t, SFXData parent)
        {
            if (ator == null)
                t.TryGetComponent (out ator);
            if (anim == null)
                t.TryGetComponent(out anim);
            for (int i = 0; i < processor.Count; ++i)
            {
                var sfxData = processor[i].Process(t, parent, out var d,out var processChild);
                if (sfxData != null)
                {
                    if (d > duration)
                    {
                        duration = d;
                    }
                    sfxComps.Add(sfxData);
                    //SequenceComponent comp = SharedObjectPool<SequenceComponent>.Get();
                    //comp.target = sfxData;
                    //sequence.components.Add(comp);
                    parent = sfxData;
                    if (!processChild)
                        return;
                    break;
                }
            }           
            for (int i = 0; i < t.childCount; ++i)
            {
                var child = t.GetChild (i);
                if (child.gameObject.activeSelf)
                {
                    RefreshComponent (child, parent);
                }
            }
        }
        public void Reset ()
        {
            for (int i = 0; i < sfxComps.Count; ++i)
            {
                sfxComps[i].Reset ();
            }
            sfxComps.Clear ();
            ator = null;
            anim = null;
            duration = -1;
            time = 0;
        }
        public void Refresh ()
        {
            Reset();
            if (processor == null)
            {
                processor = new List<SFXProcessData>();
                var types = EngineUtility.GetAssemblyType(typeof(SFXProcessData));
                foreach (var t in types)
                {
                    var process = Activator.CreateInstance(t) as SFXProcessData;
                    if (process != null)
                    {
                        processor.Add(process);
                    }
                }
            }

            //if (sequence == null)
            //{
            //    sequence = new SFXSequence();
            //}
            //sequence.Reset();
            RefreshComponent(this.transform, null);
        }

        public void Play()
        {
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                if (!SFXWrapper.currentSFXs.Contains(this))
                    SFXWrapper.currentSFXs.Add(this);
                duration = -1;
                time = 0;
                if (ator != null)
                {
                    if (ator.runtimeAnimatorController != null)
                    {
                        ator.enabled = false;
                        ator.enabled = true;
                        stateName = ator.runtimeAnimatorController.name;
                        ator.Play(stateName, 0, 0);
                    }

                }
                if (anim != null)
                {
                    anim.Play();
                }
                for (int i = 0; i < sfxComps.Count; ++i)
                {
                    var sfxComp = sfxComps[i];
                    sfxComp.Refresh(0, out var d);
                    if (d > duration)
                    {
                        duration = d;
                    }
                }
            }
            else
            {
                SFXWrapper.currentSFXs.Remove(this);
            }
        }

        public void Restart ()
        {
            if (ator != null && !string.IsNullOrEmpty (stateName))
            {
                ator.PlayInFixedTime (stateName, 0, time);
                if (!Application.isPlaying)
                {
                    ator.Update (time);
                }
            }
            if (anim != null)
            {
                var clip = anim.clip;
                if (clip != null)
                {
                    var state = anim[clip.name];
                    state.time = time;
                }
            }
            for (int i = 0; i < sfxComps.Count; ++i)
            {
                var sfxComp = sfxComps[i];
                sfxComp.OnUpdate (time, time, true, false);
            }
        }

        public void OnCompGUI ()
        {
            EditorGUILayout.ObjectField ("", ator, typeof (Animator), true);
            EditorGUILayout.ObjectField ("", anim, typeof (Animation), true);
            float w = EditorGUIUtility.currentViewWidth;
            for (int i = 0; i < sfxComps.Count; ++i)
            {
                var sfxComp = sfxComps[i];
                sfxComp.OnGUI (w);
            }
        }
        public void Update ()
        {
            if (isPlaying)
            {
                float deltaTime = Time.deltaTime * timeScale;
                if (!lockTime)
                    time += deltaTime;
                if (ator != null && !Application.isPlaying)
                {
                    ator.Update (time);
                }
                for (int i = 0; i < sfxComps.Count; ++i)
                {
                    var sfxComp = sfxComps[i];
                    sfxComp.OnUpdate (time, deltaTime, false, lockTime);
                }

            }
        }
        static void DrawSFX (SFXWrapper sfx)
        {
            Handles.BeginGUI ();
            EditorGUILayout.ObjectField ("", sfx, typeof (SFXWrapper), true);
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Stop", GUILayout.MaxWidth (80)))
            {
                sfx.isPlaying = false;
                sfx.time = 0;
                sfx.Restart ();
                removeSfx.Add (sfx);
            }
            if (GUILayout.Button ("RePlay", GUILayout.MaxWidth (80)))
            {
                sfx.time = 0;
                sfx.Restart ();
            }
            if (GUILayout.Button (sfx.lockTime? "UnLockTime": "LockTime", GUILayout.MaxWidth (80)))
            {
                sfx.lockTime = !sfx.lockTime;
            }
            EditorGUILayout.EndHorizontal ();
            if (sfx != null)
            {
                EditorGUILayout.BeginHorizontal ();
                //EditorGUILayout.LabelField ("Time", GUILayout.MaxWidth (40));
                EditorGUI.BeginChangeCheck ();
                if (sfx.duration > 0)
                {
                    sfx.time = EditorGUILayout.Slider ("Time", sfx.time, 0, sfx.duration);
                }
                else
                {
                    sfx.time = EditorGUILayout.FloatField ("Time", sfx.time);
                }

                if (EditorGUI.EndChangeCheck ())
                {
                    sfx.Restart ();
                }
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                sfx.duration = EditorGUILayout.FloatField ("Duration", sfx.duration);
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField ("TimeScale", GUILayout.MaxWidth (60));
                sfx.timeScale = EditorGUILayout.Slider ("", sfx.timeScale, 0, 10);
                EditorGUILayout.EndHorizontal ();

            }
            EditorGUILayout.Space ();
            Handles.EndGUI ();
        }
        static void DrawSFXWindow (int windowID)
        {
            var it = currentSFXs.GetEnumerator ();
            while (it.MoveNext ())
            {
                DrawSFX (it.Current);
            }
            for (int i = 0; i < removeSfx.Count; ++i)
            {
                currentSFXs.Remove (removeSfx[i]);
            }
            removeSfx.Clear ();
        }

        public static void OnSceneGUI (SceneView sceneView)
        {
            if (currentSFXs.Count > 0)
            {
                var rect = sceneView.position;
                float w = rect.width;
                float h = rect.height;
                float width = 300;
                float height = (110 + 5) * currentSFXs.Count;
                GUILayout.Window (11111, new Rect (w - width - 10, h - height - 20, width, height),
                    sfxSceneGUI, string.Format ("SFXs({0})", currentSFXs.Count.ToString ()));

            }
        }

        public void Refresh(RenderingManager mgr)
        {

        }
        public void OnDrawGizmo(EngineContext context)
        {

        }
        public void SetAreaMask(uint area)
        {
            areaMask = area;
        }

        #region sceneAnim
        public void SetAnim(string exString, uint f)
        {
            flag.SetFlag(SceneObject.HasAnim | f, true);
            if (!string.IsNullOrEmpty(exString) &&
                this.exString != exString)
            {
                DebugLog.AddErrorLog2("multi animation exstrinig: src {0} current{1} obj:{2}",
                    this.exString,
                    exString, this.name);
            }
            this.exString = exString;
        }

        public void SetUVOffset(ref Vector2 offset)
        {

        }
        #endregion
    }
}
#endif