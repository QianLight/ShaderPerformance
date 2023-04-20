using UnityEngine;
using UnityEngine.SceneManagement;

public class TimelineConfig
{

    private static bool _startWithTimeline = false;


    public static bool isEditorMode
    {
        get
        {
            if (Application.isPlaying)
            {
                return _startWithTimeline;
            }
            else
            {
                return true;
            }
        }
    }


    public static void SetEditorMode()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "entrance")
        {
            _startWithTimeline = true;
        }
    }
    
}

