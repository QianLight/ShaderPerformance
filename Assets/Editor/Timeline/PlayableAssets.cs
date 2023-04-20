using System.Collections.Generic;
using System.IO;
using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XEditor
{

    public partial class PlayableDirectorInspector
    {

        private Dictionary<PlayableAssetType, List<string>> dic;

        /// <summary>
        /// check all used assets for preload
        /// </summary>
        public void FetchUsedAssets ()
        {
            EmptyDic ();
            data.useCine = false;
            data.useGameCamera = true;
            foreach (PlayableBinding pb in _director.playableAsset.outputs)
            {
                if (_map.ContainsKey (pb.streamName))
                {
                    ITimelineAsset asset = _map[pb.streamName] as ITimelineAsset;
                    ParsePlayableAsset (asset, pb);
                }
                else if (pb.streamName.Equals ("Markers") &&
                    pb.sourceObject != null)
                {
                    MarkerTrack track = pb.sourceObject as MarkerTrack;
                    var marks = track.GetMarkers ();
                    foreach (var item in marks)
                    {
                        if (item is ITimelineAsset)
                        {
                            ITimelineAsset asset = pb.sourceObject as ITimelineAsset;
                            ParsePlayableAsset (asset, pb);
                        }
                    }
                }
                else
                {
                    if (pb.sourceObject is ITimelineAsset)
                    {
                        ITimelineAsset asset = pb.sourceObject as ITimelineAsset;
                        ParsePlayableAsset (asset, pb);
                    }
                }

                if (pb.sourceObject != null && pb.sourceObject is CinemachineTrack)
                {
                    data.useCine = true;
                    data.useGameCamera = false;
                }
                if (pb.sourceObject != null && pb.sourceObject is CameraPlayableTrack)
                {
                    data.useGameCamera = false;
                }
            }
            OutputInfo ();
        }

        private void EmptyDic ()
        {
            if (dic == null)
            {
                dic = new Dictionary<PlayableAssetType, List<string>> ();
            }
            dic.Clear ();
        }

        private void ParsePlayableAsset (ITimelineAsset asset, PlayableBinding pb)
        {
            if (asset != null)
            {
                var list = asset.ReferenceAssets (pb);
                AppendAsset (asset.assetType, list);
            }
        }

        private void AppendAsset (PlayableAssetType type, List<string> list)
        {
            List<string> lst;
            if (dic.TryGetValue (type, out lst))
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    var str = list[i];
                    if (!lst.Contains (str))
                    {
                        lst.Add (str);
                    }
                }
            }
            else
                dic.Add (type, list);
        }

        private void OutputInfo ()
        {
            foreach (var item in dic)
            {
                string list = "";
                foreach (var it in item.Value)
                {
                    list += it + "\t";
                }
                Debug.Log (item.Key + ":\n" + list);
            }
        }

        public Dictionary<byte, List<string>> WritePreload (BinaryWriter writer)
        {
            //save str list
            Dictionary<byte, List<string>> ret = new Dictionary<byte, List<string>> ();
            DirectorHelper.PreSave ();
            foreach (var item in dic)
            {
                byte assetType = (byte) item.Key;
                List<string> res = new List<string> ();
                foreach (var str in item.Value)
                {
                    if (!string.IsNullOrEmpty (str) &&
                        !str.StartsWith ("Assets/"))
                    {
                        DirectorHelper.SaveStringIndex (null, str, true, true);
                        res.Add (str);
                    }
                }
                if (res.Count > 0)
                {
                    ret.Add (assetType, res);
                }
            }
            if (writer != null)
            {
                byte count = (byte) ret.Count;
                writer.Write (count);
                foreach (var item in ret)
                {
                    byte assetType = (byte) item.Key;
                    writer.Write (assetType);
                    byte assetCount = (byte) item.Value.Count;
                    writer.Write (assetCount);
                    foreach (var str in item.Value)
                    {
                        writer.Write (str);
                    }
                }
            }

            return ret;
        }

    }

}