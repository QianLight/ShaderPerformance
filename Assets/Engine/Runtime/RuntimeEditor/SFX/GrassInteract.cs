#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class GrassInteract : MonoBehaviour
    {
      //     public Vector4 starttime;
      //     public Vector4 endtime;
      //   public float raidussdefaut=1F;
        public AnimationCurve raiduss;
        public Animation _trackani;
        public Transform _testobj;
        public void Refresh()
        {
            if (_trackani == null)
                transform.TryGetComponent(out _trackani);
            if (_trackani == null)
            {
                _trackani = transform.gameObject.AddComponent<Animation>();
            }
          //  GrassInteractManager.singleton.Updaterolepostion(transform.position);
            GrassInteractManager.singleton.Updaterolepostion(transform.parent.transform.position);
            if (_testobj != null)
            {
                GrassInteractManager.singleton.gettesttrans(_testobj);
            }
            if (_testobj == null)
            {
                GrassInteractManager.singleton.cleartran();
            }
        }

        private void OnDrawGizmos()
        {
            var c = Gizmos.color;
            Color EEE = new Color(1,0,0,0.8F);
            Gizmos.color = EEE;
            Gizmos.DrawSphere(this.transform.position, transform.localScale.x/2f);
            Gizmos.color = c;
        }

        public void setanimation(float time)
        {
            if (_trackani != null)
            {         
                var clip = _trackani.clip;
                if (clip != null)
                {
 
                     clip.SampleAnimation(this.gameObject, time);
                     GrassInteractManager.singleton.Postionsetin(transform.position, transform.localScale.x / 2f);
          
                }
            }
        }
    }

    public class SFXGrassInteractDataProcessor : SFXProcessData
    {
        public override SFXData Process(Transform t, SFXData parent, out float duration, out bool processChind)
        {
            duration = 0;
            processChind = false;
            if (t.TryGetComponent<GrassInteract>(out var sga))
            {
                SFXData sfxData = new GrassInteractData();
                sfxData.Refresh(t, sga, parent, 0, out var d);
                return sfxData;
            }
            return null;
        }
    }

    public partial class GrassInteractData : SFXData
    {
        private GrassInteract sa;
        public override void OnUpdate(float time, float deltaTime, bool restart, bool lockTime)
        {
            if (!lockTime)
            {
                sa.setanimation(time);
            }
        }
        public override void Refresh(Transform t, Component comp, SFXData parent, float time,
        out float duration)
        {
            base.Refresh(t, comp, parent, time, out duration);
            sa = comp as GrassInteract;
            sa.Refresh();

        }
    }


        [CustomEditor(typeof(GrassInteract))]
    public class GrassInteractEditor: UnityEngineEditor
    {
    //    public SerializedProperty starttime;
    //    public SerializedProperty endtime;
      //  public SerializedProperty raiduss;
        public SerializedProperty _testobj;
        void OnEnable()
        {
            //   starttime = serializedObject.FindProperty("starttime");
            //   endtime = serializedObject.FindProperty("endtime");
            _testobj = serializedObject.FindProperty("_testobj");
        }
        public override void OnInspectorGUI()
        {
            var ga = target as GrassInteract;
         //   EditorGUILayout.PropertyField(starttime);
         //   EditorGUILayout.PropertyField(endtime);
            EditorGUILayout.PropertyField(_testobj);
            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(80)))
            {
                ga.Refresh();
            }
            serializedObject.ApplyModifiedProperties();
        }

    

    }
}
#endif
