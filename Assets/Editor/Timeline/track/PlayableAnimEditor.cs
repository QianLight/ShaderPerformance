using System.Collections.Generic;
using CFEngine;
using CFUtilPoolLib;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XEditor
{
    public class PlayableAnimEditor : PlayableBaseEditor, ITimelineAsset
    {
        private bool ismaindummy = true;
        private string strack;
        private GameObject go;
        private static GameObject cineRoot;
        private bool tfFolder;
        [SerializeField]
        private string goName;
        private int t_select = -1;

        private static string[] t_prefabs;
        private static uint[] t_pids;

        public PlayableAnimEditor (int indx, PlayableDirectorInspector parent) : base (indx, parent)
        {
            strack = "track" + trackIndex + "_";
            if (t_prefabs == null)
            {
                XEntityPresentationReader.GetAllEntities (out t_pids, out t_prefabs);
            }
        }

        public PlayableAssetType assetType
        {
            get { return PlayableAssetType.ANIM; }
        }

        public override void Reset () { }

        public void CheckCVRecord (PlayableBinding pb, ref DirectorAnimBinding bind)
        {
            if (cineRoot == null) cineRoot = GameObject.Find ("cineroot");
            AnimationTrack track = pb.sourceObject as AnimationTrack;
            var tf = ExternalHelp.FetchAttachOfTrack (DirectorHelper.GetDirector (), track);
            if (tf && tf.GetComponent<CinemachineVirtualCameraBase> ())
            {
                bind.type = PlayableAnimType.VCamera;
                bind.val = TransformIndex (tf);
            }
        }

        public override void OnInspectorGUI (PlayableBinding pb)
        {
            DirectorAnimBinding bind = editor.EdtData.GetNewAnimBinding (pb.streamName);
            EditorGUILayout.LabelField (pb.streamName, EditorStyles.boldLabel);
            bind.type = (PlayableAnimType) EditorGUILayout.EnumPopup ("Type", bind.type);
            CheckCVRecord (pb, ref bind);
            if (bind.type == PlayableAnimType.Entity)
            {
                EditorGUILayout.BeginHorizontal ();

                uint cache = bind.val;
                bind.val = (uint) EditorGUILayout.IntField ("present id", (int) bind.val);
                if (cache != bind.val) t_select = SearchPresentID (bind.val);
                t_select = EditorGUILayout.Popup (t_select, t_prefabs);
                if (t_select >= 0) bind.val = t_pids[t_select];
                EditorGUILayout.EndHorizontal ();
            }
            else if (bind.type == PlayableAnimType.Dummy)
            {
                ismaindummy = EditorGUILayout.Toggle ("main dummy", ismaindummy);
                if (ismaindummy) bind.val = 0;
                else bind.val = (uint) EditorGUILayout.IntField ("presentid", (int) bind.val);
            }
            else if (bind.type == PlayableAnimType.Inner|| bind.type == PlayableAnimType.SyncInner)
            {
                bind.val = (uint) EditorGUILayout.IntField ("index", (int) bind.val);
                if (bind.val > 10) EditorGUILayout.HelpBox ("Are you sure to set index more than 10'", MessageType.Warning);
            }
            else if (bind.type == PlayableAnimType.VCamera)
            {
                EditorGUILayout.LabelField ("child: " + bind.val);
            }
            if (bind.type != PlayableAnimType.VCamera)
            {
                tfFolder = EditorGUILayout.Foldout (tfFolder, "Transform");
                if (tfFolder)
                {
                    bind.pos = EditorGUILayout.Vector3Field ("position", bind.pos);
                    bind.rot = EditorGUILayout.Vector3Field ("rotation", bind.rot);
                    bind.scale = EditorGUILayout.Vector3Field ("scale", bind.scale);
                }
            }
            editor.EdtData.Set (pb.streamName, ref bind);
            // var track = pb.sourceObject as AnimationTrack;
            // bind.clips = track.GetClips().Select(x => x.displayName).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            EditorGUILayout.Space ();
        }

        private void GetRefAsset (List<string> paths, AnimationTrack track)
        {
            if (track != null && !track.hasClips && track.infiniteClip != null)
            {
                var t = ExternalHelp.FetchTrack (DirectorHelper.GetDirector (), track);
                string path = null;
                if (editor.GetAnimBindingObj (t.name, out path))
                {
                    int index = path.IndexOf ("Animation/Main_Camera/");
                    if (index >= 0)
                    {
                        path = path.Substring (index);
                        path = path.Replace (".anim", "");
                        paths.Add (path);
                    }
                }
            }
        }
        public List<string> ReferenceAssets (PlayableBinding pb)
        {
            List<string> list = new List<string> ();
            var track = pb.sourceObject as AnimationTrack;
            var clips = track.GetClips ();
            foreach (var item in clips)
            {
                string path = AssetDatabase.GetAssetPath (item.animationClip);
                path = path.Replace ("Assets/BundleRes/", "");
                path = path.Replace (".anim", "");
                list.Add (path);
            }
            GetRefAsset (list, track);
            var childs = track.GetChildTracks ();
            foreach (var child in childs)
            {
                GetRefAsset (list, child as AnimationTrack);
            }
            return list;
        }
        public static void OnLoad (ref DirectorAnimBinding bind, DirectorTrackAsset track, ICallbackData cbData)
        {
            string prefix = "";
            uint id = 0;
            if (bind.type == PlayableAnimType.Entity)
            {
                prefix = "entity";
                id = bind.val;
            }
            else if (bind.type == PlayableAnimType.Player ||
                bind.type == PlayableAnimType.Dummy ||
                bind.type == PlayableAnimType.Inner||
                bind.type == PlayableAnimType.SyncInner)
            {
                id = 101;
                if (bind.type == PlayableAnimType.Dummy)
                {
                    id = bind.val;
                }
                prefix = "player";
                if (bind.type == PlayableAnimType.Dummy) prefix = id == 0 ? "maindummy" : "dummy";
                else if (bind.type == PlayableAnimType.Inner) prefix = "inner";
            }
            else if (bind.type == PlayableAnimType.VCamera)
            {
                if (cineRoot == null) cineRoot = GameObject.Find ("cineroot");
                if (cineRoot)
                {
                    Transform tf = cineRoot.transform.GetChild ((int) bind.val);
                    if (tf)
                    {
                        Animator ator = tf.gameObject.GetComponent<Animator> ();
                        if (ator) DirectorHelper.singleton.BindObject2Track (track, ator, cbData);
                    }
                }
            }
            if (!string.IsNullOrEmpty (prefix))
            {
                Animator ator = RTimeline.LoadDummy (prefix, bind.streamName, bind, id);
                DirectorHelper.singleton.BindObject2Track (track, ator, cbData);
            }
        }

        private static void CheckNotused (string trackName, string prefix, uint id)
        {
            GameObject root = RTimeline.timelineRoot;
            if (root != null)
            {
                string start = "track" + trackName + "_";
                string name = string.Format ("track{0}_{1}_{2}", trackName, prefix, id);
                int cnt = root.transform.childCount;
                for (int i = 0; i < cnt; i++)
                {
                    Transform tf = root.transform.GetChild (i);
                    if (tf.name.StartsWith (start) && !tf.name.Equals (name))
                    {
                        GameObject.DestroyImmediate (tf.gameObject);
                        break;
                    }
                }
            }
        }

        public override void OnLoad (PlayableBinding pb)
        {
            DirectorAnimBinding bind = editor.EdtData.GetNewAnimBinding (pb.streamName);
            string prefix = "";
            uint id = 0;
            if (bind.type == PlayableAnimType.Entity)
            {
                prefix = "entity";
                id = bind.val;
            }
            else if (bind.type == PlayableAnimType.Player ||
                bind.type == PlayableAnimType.Dummy ||
                bind.type == PlayableAnimType.Inner ||
                bind.type == PlayableAnimType.SyncInner)
            {
                id = 101;
                if (bind.type == PlayableAnimType.Dummy)
                {
                    id = bind.val;
                }
                prefix = "player";
                if (bind.type == PlayableAnimType.Dummy) prefix = id == 0 ? "maindummy" : "dummy";
                else if (bind.type == PlayableAnimType.Inner || bind.type == PlayableAnimType.SyncInner) prefix = "inner";
            }
            else if (bind.type == PlayableAnimType.VCamera)
            {
                if (cineRoot == null) cineRoot = GameObject.Find ("cineroot");
                if (cineRoot)
                {
                    Transform tf = cineRoot.transform.GetChild ((int) bind.val);
                    if (tf)
                    {
                        Animator ator = tf.gameObject.GetComponent<Animator> ();
                        TrackAsset ta = pb.sourceObject as TrackAsset;
                        if (ta != null && ator != null)
                        {
                            DirectorHelper.singleton.BindObject2Track (ta, ator);
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty (prefix))
            {
                CheckNotused (pb.streamName, prefix, id);
                Animator ator = RTimeline.LoadDummy (prefix, pb.streamName, bind, id);
                if (ator != null) ator.tag = tag;
                TrackAsset ta = pb.sourceObject as TrackAsset;
                if (ta != null && ator != null)
                {
                    DirectorHelper.singleton.BindObject2Track (ta, ator);
                }
            }
            else if (bind.type != PlayableAnimType.VCamera)
            {
                DebugLog.AddErrorLog2 ("error bind type:{0}", bind.type.ToString ());
                //must bind null,other wise memory leak
                //DirectorHelper.singleton.BindObject2Track (track, null, cbData);
            }
        }

        private static int SearchPresentID (uint pid)
        {
            if (t_pids != null)
            {
                for (int i = 0; i < t_pids.Length; i++)
                {
                    if (t_pids[i] == pid) return i;
                }
            }
            return -1;
        }
    }

}