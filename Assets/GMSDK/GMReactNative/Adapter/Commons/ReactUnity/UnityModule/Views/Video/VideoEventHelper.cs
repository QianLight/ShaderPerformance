using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace GSDK.RNU
{
    public class VideoEventHelper: MonoBehaviour
    {
        private ReactVideo rVideo;

        private bool isInited = false;

        public void InitEventHelper(ReactVideo video)
        {
            rVideo = video;
            isInited = true;
        }

        private void Update()
        {
            if (rVideo == null || isInited == false)
            {
                return;
            }
            rVideo.PlayProgress();
        }
    }
}