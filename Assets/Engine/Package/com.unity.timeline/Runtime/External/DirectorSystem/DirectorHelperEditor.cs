#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CFEngine;
using UnityEditor;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    public class StringInfo
    {
        public string str;
        public int isNotAsset;
    }
    class SaveContext
    {
        public List<TimelineClip> saveClips = new List<TimelineClip> ();
        public List<Marker> saveSignals = new List<Marker> ();
        public List<TrackAsset> saveTracks = new List<TrackAsset> ();
        public TrackSaveContext trackContext = new TrackSaveContext ()
        {
            presave = true,
        };
        public SignalSaveContext signalContext = new SignalSaveContext ()
        {
            presave = true,
        };
    }
    public delegate byte GetAssetType (UnityEngine.Object obj);
    public delegate byte GetSignalType (Marker signal);
    public delegate void SavePlayableAsset (BinaryWriter bw, PlayableAsset asset, bool presave);

    public delegate bool GetAnimBindingObj (string streamName, out string path);

    public partial class DirectorHelper : CFSingleton<DirectorHelper>
    {
        public static SavePlayableAsset[] savePlayableFun;
        public static List<StringInfo> stringList = new List<StringInfo> ();
        public static List<EnvRefParam> envParamList = new List<EnvRefParam> ();
        public static GetAssetType getAssetType;
        public static GetSignalType getSignalType;

        public static uint DebugFlag_Read = 1;
        public static uint DebugFlag_Save = 2;

        public static int Timeline_Version_Env = 1;
        public static int Timeline_Version_AnimOverride = 2;
        public static int Timeline_Version = 2;
        public static GetAnimBindingObj getAnimBindingObj = null;
        static List<DrawCurveContext> curveIndex = new List<DrawCurveContext> ();
        public static bool useEngineAnimCurve
        {
            get
            {
                var director = GetDirector ();
                if (director != null)
                {
                    return director.playableAsset is TimelineAsset;
                }
                return true;
            }
        }
        public static void InitEditor (int assetTypeCount)
        {
            savePlayableFun = new SavePlayableAsset[assetTypeCount];
            savePlayableFun[AssetType_Animation] = AnimationPlayableAsset.SaveAsset;
            savePlayableFun[AssetType_Active] = ActivationPlayableAsset.SaveAsset;
            savePlayableFun[AssetType_Control] = ControlPlayableAsset.SaveAsset;
            savePlayableFun[AssetType_AnimationClip] = AnimationClipPlayAsset.SaveAsset;

        }


        public static void PreSave ()
        {
            stringList.Clear ();
            envParamList.Clear ();
        }

        public static void Save (TimelineAsset asset, BinaryWriter bw)
        {
            var directorHelper = DirectorHelper.singleton;
            if (directorHelper.animCurve == null)
                directorHelper.animCurve = new AnimCurveShare ();
            directorHelper.animCurve.BeginAddCurves ();

            DebugLog.DebugStream (bw, "", DebugSream.Clear);
            directorHelper.assetCount = (byte) stringList.Count;
            //presave
            var filterTracks = asset.flattenedTracks.Where (f => f.GetType () != typeof (GroupTrack)).ToList ();
            byte tracksCount = 0;
            List<TimelineClip> clips = new List<TimelineClip> ();
            List<Marker> saveSignals = new List<Marker> ();
            List<TrackAsset> saveTracks = new List<TrackAsset> ();
            TrackSaveContext trackContext = new TrackSaveContext ()
            {
                presave = true,
            };
            SignalSaveContext signalContext = new SignalSaveContext ()
            {
                presave = true,
            };
            for (int i = 0; i < filterTracks.Count; ++i)
            {
                var track = filterTracks[i];
                byte trackType = track.GetTrackType ();
                if ((track.IsCompilable () || trackType == TrackType_Mark) && trackType != 255)
                {
                    track.SortClips ();
                    var cs = track.clips;
                    bool trackValid = false;
                    if (cs.Length == 0)
                    {
                        if (track is AnimationTrack && getAnimBindingObj != null)
                        {
                            AnimationTrack at = track as AnimationTrack;
                            var attachTrack = ExternalHelp.FetchTrack (GetDirector (), at);
                            string path = null;
                            if (attachTrack != null && getAnimBindingObj (attachTrack.name, out path))
                            {
                                AnimationClip anim = null;
                                if (File.Exists (path))
                                {
                                    anim = AssetDatabase.LoadAssetAtPath<AnimationClip> (path);
                                }
                                else
                                {
                                    anim = at.infiniteClip;
                                }
                                if (anim != null)
                                {
                                    track = AnimClipTrack.CreateInstance<AnimClipTrack> ();
                                    track.parent = at.parent;
                                    track.name = at.name;
                                    track.clipStart = (short) clips.Count;
                                    var recordAnimationClip = new RecordTimelineClip ();
                                    var recordAnimationAsset = AnimationPlayableAsset.CreateInstance<AnimationClipPlayAsset> ();
                                    recordAnimationClip.asset = recordAnimationAsset;
                                    recordAnimationAsset.clip = anim;
                                    recordAnimationAsset.OffsetPos = at.infiniteClipOffsetPosition;
                                    recordAnimationAsset.OffsetRot = at.infiniteClipOffsetRotation;
                                    clips.Add (recordAnimationClip);
                                    track.clipEnd = (short) clips.Count;
                                    trackValid = true;
                                }
                                else
                                {
                                    DebugLog.AddErrorLog2 ("record anim not exist {0}", path);
                                }

                            }
                            else
                            {
                                DebugLog.AddErrorLog2 ("track bind obj not load {0}", track.name);
                            }
                        }
                        else if (track is MarkerTrack)
                        {
                            var marks = track.GetMarkers ();
                            if (marks.GetEnumerator ().MoveNext ())
                                trackValid = true;
                        }
                    }
                    else
                    {
                        track.clipStart = (short) clips.Count;
                        for (int j = 0; j < cs.Length; ++j)
                        {
                            var c = cs[j];
                            byte clipType = GetAssetType (c.asset);
                            if (clipType != 255)
                            {
                                clips.Add (c);
                            }
                        }
                        track.clipEnd = (short) clips.Count;
                        trackValid = true;

                    }
                    if (trackValid)
                    {
                        var marks = track.GetMarkers ();
                        foreach (var mark in marks)
                        {
                            if (mark is INotification && mark is Marker)
                            {
                                var signal = mark as Marker;
                                byte signalType = signal.GetSignalType ();
                                if (signalType != 255)
                                {
                                    saveSignals.Add (signal);
                                    signal.trackIndex = (byte) saveTracks.Count;
                                }
                            }
                        }
                        saveTracks.Add (track);
                        tracksCount++;
                    }
                }
            }
            short clipsCount = (short) clips.Count;
            for (int i = 0; i < clipsCount; ++i)
            {
                clips[i].Save (null, true);
            }

            for (int i = 0; i < saveTracks.Count; ++i)
            {
                saveTracks[i].Save (null, ref trackContext);
            }
            for (int i = 0; i < saveSignals.Count; ++i)
            {
                saveSignals[i].Save (null, ref signalContext);
            }
            //real save start 
            DebugLog.DebugStream (bw, string.Format ("debug save director asset:{0}", asset.name));
            bw.Write (Timeline_Version);
            //strings
            short strCount = (short) (stringList.Count - directorHelper.assetCount);
            bw.Write (strCount);
            bw.Write (directorHelper.assetCount);
            DebugLog.DebugStream (bw, string.Format ("debug asset count:{0} str count:{1}", directorHelper.assetCount, strCount));
            string readStr = "";
            for (int i = directorHelper.assetCount; i < stringList.Count; ++i)
            {
                bw.Write (stringList[i].str);
                readStr += stringList[i].str + "\r\n";

            }
            DebugLog.AddLog2 ("save str:{0}", readStr);
            //head
            directorHelper.animCurve.FlushCurves (bw);
            short envParamCount = (short) envParamList.Count;
            bw.Write (envParamCount);
            for (int i = 0; i < envParamCount; ++i)
            {
                var erp = envParamList[i];
                EnvRefParam.SaveEnvParam (bw, ref erp);
            }
            bw.Write (asset.duration);
            //clips
            bw.Write (clipsCount);
            for (int i = 0; i < clipsCount; ++i)
            {
                clips[i].Save (bw, false);
                DebugLog.DebugStream (bw, string.Format ("clips index:{0}", i));
            }
            DebugLog.DebugStream (bw, string.Format ("clips count:{0}", clipsCount));
            //tracks
            trackContext.presave = false;
            bw.Write (tracksCount);
            for (int i = 0; i < saveTracks.Count; ++i)
            {
                var track = saveTracks[i];
                byte trackType = track.GetTrackType ();
                bw.Write (trackType);
                var parent = track.parent;
                short parentIndex = (short) saveTracks.FindIndex (x => x == parent);
                bw.Write (parentIndex);
                trackContext.flag = 0;
                track.Save (bw, ref trackContext);
            }
            DebugLog.DebugStream (bw,  string.Format ("tracks count:{0}", tracksCount));
            //signals
            signalContext.presave = false;
            short signalsCount = (short) saveSignals.Count;
            bw.Write (signalsCount);
            for (int i = 0; i < signalsCount; ++i)
            {
                byte signalType = saveSignals[i].GetSignalType ();
                bw.Write (signalType);
                signalContext.flag = 0;
                saveSignals[i].Save (bw, ref signalContext);
            }
            DebugLog.DebugStream (bw, string.Format ("signals count:{0}", signalsCount), DebugSream.Output);
        }
        
        public static string GetAssetPath (UnityEngine.Object obj, string ext)
        {
            string assetPath = "";
            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath (obj);
                if (path.StartsWith ("Assets/BundleRes/"))
                {
                    assetPath = path.Replace ("Assets/BundleRes/", "").Replace (ext, "");
                }
            }
            return assetPath;
        }

        public static void SaveAsset (BinaryWriter bw, UnityEngine.Object obj, string ext, bool presave)
        {
            string assetPath = GetAssetPath (obj, ext);
            SaveStringIndex (bw, assetPath, presave, true);
        }

        public static short SaveStringIndex (BinaryWriter bw, string str, bool presave, bool isAsset = false)
        {
            short index = -1;
            if (!string.IsNullOrEmpty (str))
            {
                index = (short) stringList.FindIndex (x => x.str == str);
                if (index < 0 && presave)
                {
                    index = (short) stringList.Count;
                    stringList.Add (new StringInfo ()
                    {
                        str = str,
                            isNotAsset = isAsset?0 : 1,
                    });
                }

            }
            if (!presave)
                bw.Write (index);
            return index;
        }
        public static void SaveVector (BinaryWriter bw, ref Vector2 vec)
        {
            if (bw != null)
            {
                bw.Write (vec.x);
                bw.Write (vec.y);
            }
        }

        public static void SaveVector (BinaryWriter bw, ref Vector3 vec)
        {
            if (bw != null)
            {
                bw.Write (vec.x);
                bw.Write (vec.y);
                bw.Write (vec.z);
            }
        }

        public static void SaveQuaternion (BinaryWriter bw, ref Quaternion q)
        {
            if (bw != null)
            {
                bw.Write (q.x);
                bw.Write (q.y);
                bw.Write (q.z);
                bw.Write (q.w);
            }
        }
        public static void SaveColor (BinaryWriter bw, ref Color c)
        {
            if (bw != null)
            {
                bw.Write (c.r);
                bw.Write (c.g);
                bw.Write (c.b);
                bw.Write (c.a);
            }
        }

        public static short SaveCurve (AnimationCurve[] curve, ref CurveContext context, ref EnvRefParam erp)
        {
            DirectorHelper.singleton.animCurve.AddCurve (curve, ref context);
            erp.curveIndex = context.curveIndex;
            envParamList.Add (erp);
            return (short) (envParamList.Count - 1);
        }

        public static void SaveCurve (BinaryWriter bw, ref CustomCurveRef c, bool presave)
        {
            if (presave)
            {
                DirectorHelper.singleton.animCurve.AddCurve (c.curve, ref c.context);
            }
            else
            {
                bw.Write (c.context.curveIndex);
            }
        }

        public static byte GetAssetType (UnityEngine.Object asset)
        {
            if (asset is AnimationPlayableAsset)
            {
                return AssetType_Animation;
            }
            else if (asset is ActivationPlayableAsset)
            {
                return AssetType_Active;
            }
            else if (asset is ControlPlayableAsset)
            {
                return AssetType_Control;
            }
            else if (asset is AnimationClipPlayAsset)
            {
                return AssetType_AnimationClip;
            }
            if (getAssetType != null)
                return getAssetType (asset);
            return 255;
        }
        public static byte GetSignalType (Marker signal)
        {
            if (signal is ActiveSignalEmmiter)
            {
                return DirectorSignalEmmiter.SignalType_Active;
            }
            else if (signal is JumpSignalEmmiter)
            {
                return DirectorSignalEmmiter.SignalType_Jump;
            }
            else if (signal is SlowSignalEmitter)
            {
                return DirectorSignalEmmiter.SignalType_Slow;
            }
            if (getSignalType != null)
                return getSignalType (signal);
            return 255;
        }

        public static TrackAsset FindTrack (DirectorTrackAsset dta)
        {
            if (timelineAsset != null)
            {
                var it = timelineAsset.outputs.GetEnumerator ();
                while (it.MoveNext ())
                {
                    PlayableBinding pb = it.Current;
                    var track = pb.sourceObject as TrackAsset;
                    if (track != null)
                    {
                        var o = track.outputs;
                        var tit = o.GetEnumerator ();
                        string sn = null;
                        while (tit.MoveNext ())
                        {
                            sn = tit.Current.streamName;
                            break;
                        }
                        if (dta != null&& dta.streamName == sn)
                        {
                            track.runtimeTrack = dta;
                            dta.track = track;
                            track.BindTrack ();
                            var clips = track.clips;
                            if (clips != null)
                            {
                                for (int i = 0; i < clips.Length; ++i)
                                {
                                    var clip = clips[i];
                                    var a = clip.asset as DirectPlayableAsset;
                                    if (a != null)
                                    {
                                        a.track = track.runtimeTrack;
                                    }
                                }
                            }
                            return track;
                        }
                    }
                }
            }
            return null;
        }

        public void BindObject2Track (TrackAsset track, UnityEngine.Object obj)
        {
            var director = GetDirector ();
            director.SetGenericBinding (track, obj);
        }
        public static void PostBindTrack ()
        {
            var tracks = DirectorHelper.singleton.tracks;
            if (tracks != null)
            {
                for (int i = 0; i < DirectorHelper.singleton.trackCount; ++i)
                {
                    var t = tracks[i];
                    FindTrack (t);
                }
            }
        }
        // public static void LoadRefAsset (TimelineAsset asset)
        // {
        //     var tracks = DirectorHelper.singleton.tracks;
        //     editorTracks.Clear ();
        //     var it = asset.outputs.GetEnumerator ();
        //     while (it.MoveNext ())
        //     {
        //         PlayableBinding pb = it.Current;
        //         var track = pb.sourceObject as TrackAsset;
        //         if (track != null)
        //         {
        //             var o = track.outputs;
        //             var tit = o.GetEnumerator ();
        //             string sn = null;
        //             while (tit.MoveNext ())
        //             {
        //                 sn = tit.Current.streamName;
        //                 break;
        //             }

        //             editorTracks.Add (track);
        //             if (track.runtimeTrack == null)
        //             {
        //                 if (tracks != null)
        //                 {
        //                     for (int i = 0; i < DirectorHelper.singleton.trackCount; ++i)
        //                     {
        //                         var t = tracks[i];
        //                         if (t != null && t.streamName == sn)
        //                         {
        //                             track.runtimeTrack = t;
        //                             break;
        //                         }
        //                     }
        //                 }
        //                 if (track.runtimeTrack == null)
        //                     track.runtimeTrack = ScriptableObject.CreateInstance<DirectorUberTrack> ();
        //             }

        //             track.BindTrack ();
        //             var clips = track.clips;
        //             if (clips != null)
        //             {
        //                 for (int i = 0; i < clips.Length; ++i)
        //                 {
        //                     var clip = clips[i];
        //                     var a = clip.asset as DirectPlayableAsset;
        //                     if (a != null)
        //                     {
        //                         a.track = track.runtimeTrack;
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }

        public static void OnDrawGizmos ()
        {
            var animCurve = DirectorHelper.singleton.animCurve;
            if (animCurve != null)
            {
                for (int i = 0; i < curveIndex.Count; ++i)
                {
                    var context = curveIndex[i];
                    animCurve.DrawCurve (context);
                }
            }
        }
    }
}
#endif