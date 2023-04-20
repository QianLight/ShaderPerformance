using CFEngine;
using UnityEngine;
using UnityEngine.CFUI;

public class TimelineMovieUI : TimelineBaseUI<TimelineMovieUI, Mp4Signal>
{
    private XVideoPlayer player;

    protected override string prefab { get { return "Movie"; } }

    protected override void OnCreated()
    {
        Transform tf = go.transform;
        if (player == null)
        {
            player = XVideoPlayer.Get<XVideoPlayer>(tf, "T");
        }
        else
        {
            player.gameObject.SetActive(true);
        }
    }


    public override void Show(Mp4Signal sig)
    {
        if (Application.isPlaying)
        {
            base.Show(sig);
            if (player != null)
            {
                if (sig.movie.StartsWith("file:") ||
                   sig.movie.StartsWith("http") ||
                   sig.movie.StartsWith("//"))
                {
                    player.Play(sig.movie, false, End);
                }
                else
                    player.Play(GetVideoURL(sig.movie), false, End);
            }
        }
    }

    private void End(IMediaController arg)
    {
        Clear();
        player.gameObject.SetActive(false);
    }

    public void Clear()
    {
        if (player != null)
        {
            player.Stop();
        }
    }
    
    public string GetVideoURL(string videoName)
    {
        return LoadMgr.singleton.GetAssetPath("video/" + videoName.ToLower() + ".mp4");
        
        // if (Application.platform == RuntimePlatform.WindowsEditor ||
        //                  Application.platform == RuntimePlatform.OSXEditor ||
        //                  Application.platform == RuntimePlatform.LinuxEditor)
        // {
        //     return Application.dataPath + "/BundleRes/Video/" + videoName.ToLower() + ".mp4";
        // }
        // else
        // {
        //     return BundleMgr.BundleRootDefault + "assets/bundleres/video/" + videoName.ToLower() + ".mp4";
        // }
    }
}
