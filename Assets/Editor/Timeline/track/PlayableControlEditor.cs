using CFEngine;
using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XEditor
{

    public class PlayableControlEditor : PlayableBaseEditor, ITimelineAsset
    {
        private int clip_cnt = 0;
        private ActivationControlPlayable.PostPlaybackState tempEnum;
        private List<ControlNode> list = new List<ControlNode> ();
        private List<bool> foldout = new List<bool> ();
        private const string sig = "item";
        private string strack;

        public PlayableControlEditor (int indx, PlayableDirectorInspector parent) : base (indx,parent)
        {
            strack = "track" + trackIndex + "_";
        }

        public PlayableAssetType assetType
        {
            get { return PlayableAssetType.PREFAB; }
        }

        private bool CheckNull (ref DirectorControlBinding bind)
        {
            if (list.Count <= 0 && bind.nodes != null && bind.nodes.Length > 0)
            {
                list.Clear ();
                foldout.Clear ();
                for (int i = 0; i < bind.nodes.Length; i++)
                {
                    foldout.Add (false);
                    list.Add (bind.nodes[i]);
                }
                return true;
            }
            return false;
        }

        public override void Reset ()
        {
            foldout.Clear ();
        }

        private void CheckRemv (ControlTrack track)
        {
            int index = 0;
            foreach (var item in track.GetClips ())
            {
                string strindex = item.displayName.Remove (0, sig.Length);
                int newindx = 0;
                if (int.TryParse (strindex, out newindx) && newindx != index)
                {
                    list.RemoveAt (index);
                    foldout.RemoveAt (index);
                }
                else
                {
                    index++;
                }
            }
        }

        public override void OnInspectorGUI (PlayableBinding pb)
        {
            DirectorControlBinding bind = editor.EdtData.GetNewControlBinding (pb.streamName);

            if (CheckNull (ref bind) && !Application.isPlaying)
            {
                //InnerLoad (editor, pb, ref bind);
            }

            EditorGUILayout.LabelField (pb.streamName, EditorStyles.boldLabel);
            var track = pb.sourceObject as ControlTrack;
            clip_cnt = 0;
            var clips = track.GetClips ();
            ControlPlayableAsset[] controls;
            foreach (var item in clips)
            {
                clip_cnt++;
            }
            controls = new ControlPlayableAsset[clip_cnt];
            if (clip_cnt < foldout.Count)
            {
                CheckRemv (track);
            }
            clip_cnt = 0;
            foreach (var item in clips)
            {
                controls[clip_cnt] = item.asset as ControlPlayableAsset;
                item.displayName = sig + clip_cnt;
                clip_cnt++;
            }
            for (int i = 0; i < clip_cnt; i++)
            {
                if (foldout.Count <= i)
                {
                    foldout.Add (false);
                    list.Add (new ControlNode ());
                }
                ControlNode node = list[i];
                foldout[i] = EditorGUILayout.Foldout (foldout[i], "item" + i);
                var prefab = controls[i].prefabGameObject;
                track.fxPool.RemoveAll(x => x == null);
                var go = track.fxPool.Find(x => x.name == prefab.name);
                if (go != null)
                {
                    node.pos = go.position;
                    node.rot = go.eulerAngles;
                    node.scl = go.localScale;
                }

                if (foldout[i])
                {

                    string clip = AssetDatabase.GetAssetPath (controls[i].prefabGameObject);
                    clip = clip.Replace ("Assets/BundleRes/", string.Empty).Replace (".prefab", string.Empty);
                    node.source = EditorGUILayout.TextField ("clip", clip);
                    if (string.IsNullOrEmpty (node.source))
                    {
                        EditorGUILayout.HelpBox (XEditorUtil.Config.tip_fx, MessageType.Error);
                    }
                    EditorGUILayout.LabelField("position：" + node.pos );
                    EditorGUILayout.LabelField("rotation：" + node.rot );
                    EditorGUILayout.LabelField("scale： " + node.scl);
                }
                else
                {
                    string clip = AssetDatabase.GetAssetPath (controls[i].prefabGameObject);
                    clip = clip.Replace ("Assets/BundleRes/", string.Empty).Replace (".prefab", string.Empty);
                    node.source = clip;
                }
                list[i] = node;
            }
            bind.nodes = list.ToArray ();
            editor.EdtData.Set (pb.streamName, ref bind);
            EditorGUILayout.Space ();
        }

        // public override void UnloadRef(PlayableDirectorInspector editor, PlayableBinding pb)
        // {
        //     var track = pb.sourceObject as ControlTrack;
        //     foreach (var item in track.GetClips())
        //     {
        //         ControlPlayableAsset asset = item.asset as ControlPlayableAsset;
        //         editor.Director.SetReferenceValue(asset.sourceGameObject.exposedName, null);
        //     }
        // }

        public static void OnLoad(ref DirectorControlBinding bind, DirectorTrackAsset track, ICallbackData cbData)
        {
            var controlTrack = DirectorHelper.FindTrack(track) as ControlTrack;
            if (controlTrack != null)
            {
                int index = 0;
                var clips = DirectorHelper.singleton.clips;
                controlTrack.fxPool.Clear();
                foreach (var item in controlTrack.GetClips())
                {
                    if (bind.nodes.Length <= index) break;
                    var node = bind.nodes[index];
                    CheckNotused(track.trackIndex, index);
                    GameObject go = RTimeline.LoadPrefab("particle", track.trackIndex, index, node.source);
                    go.tag = tag;
                    go.transform.position = node.pos;
                    if (node.rot!=Vector3.zero) go.transform.rotation = Quaternion.Euler(node.rot);
                    if (node.scl != Vector3.zero) go.transform.localScale = node.scl;
                    controlTrack.fxPool.Add(go.transform);
                    ControlPlayableAsset asset = item.asset as ControlPlayableAsset;
                    DirectorHelper.GetDirector().SetReferenceValue(asset.sourceGameObject.exposedName, go);

                    var directorAsset = clips[track.clipStart + index].asset as DirectorControlAsset;
                    if (directorAsset != null)
                    {
                        //to do 
                        // directorAsset.prefab = go;
                    }
                    index++;
                }
            }
        }


        private static void CheckNotused(int trackIndex, int id)
        {
            GameObject root = RTimeline.timelineRoot;
            if (root != null)
            {
                string start = "track" + trackIndex + "_";
                string name = string.Format("track{0}_particle_{1}", trackIndex, id);
                int cnt = root.transform.childCount;
                for (int i = 0; i < cnt; i++)
                {
                    Transform tf = root.transform.GetChild(i);
                    if (tf.name.StartsWith(start) && !tf.name.Equals(name))
                    {
                        GameObject.DestroyImmediate(tf.gameObject);
                        break;
                    }
                }
            }
        }


        // public void InnerLoad (PlayableDirectorInspector editor, PlayableBinding pb, ref DirectorControlBinding bind)
        // {
        //     // var track = pb.sourceObject as ControlTrack;
        //     // int index = 0;
        //     // if (list.Count > 0)
        //     // {
        //     //     foreach (var item in track.GetClips ())
        //     //     {
        //     //         if (list.Count <= index) break;
        //     //         string name = strack + "particle_" + index;
        //     //         ControlNode node = list[index];
        //     //         GameObject go = GameObject.Find (name);
        //     //         if (go == null)
        //     //         {
        //     //             go = new GameObject (name);
        //     //             GameObject orig = AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/BundleRes/" + node.source + ".prefab");
        //     //             if (orig == null) continue;
        //     //             GameObject fx = GameObject.Instantiate (orig);
        //     //             fx.transform.SetParent (go.transform);
        //     //             var tf = fx.transform.GetChild (0);
        //     //             tf.gameObject.SetActive (false);
        //     //         }
        //     //         go.tag = tag;
        //     //         go.transform.position = node.pos;
        //     //         ControlPlayableAsset aset = item.asset as ControlPlayableAsset;
        //     //         GameObject fxx = go.transform.GetChild (0).gameObject;
        //     //         editor.Director.SetReferenceValue (aset.sourceGameObject.exposedName, fxx);
        //     //         fxx.transform.localPosition = Vector3.zero;
        //     //         fxx.transform.localRotation = Quaternion.identity;
        //     //         index++;
        //     //     }
        //     // }
        // }
        public override void OnLoad (PlayableBinding pb)
        {
            list.Clear ();
            DirectorControlBinding bind = editor.EdtData.GetNewControlBinding (pb.streamName);
            CheckNull (ref bind);
        }

        public List<string> ReferenceAssets (PlayableBinding pb)
        {
            List<string> rst = new List<string> ();
            foreach (var item in list)
            {
                rst.Add (item.source);
            }
            return rst;
        }

    }
}