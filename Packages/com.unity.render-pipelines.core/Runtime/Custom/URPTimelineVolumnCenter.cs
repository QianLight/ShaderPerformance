using System.Collections;
using CFEngine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine;
namespace UnityEngine.Rendering.Universal
{
    public class URPTimelineVolumnCenter
    {
        public static URPTimelineVolumnCenter instance { get; } = new URPTimelineVolumnCenter();
        public Volume TimelineVolume;
        private bool flag_GetValue;
        public int componentNum = 0;

        public void EnableComponent(Volume volume)
        {
            componentNum++;
            DebugLog.AddLog2("新timeline volume animation组件生效，当前数量{0}", componentNum);
            if (TimelineVolume == null)
            {
                TimelineVolume = volume;
                TimelineVolume.priority = 1;
                TimelineVolume.isGlobal = true;
                TimelineVolume.weight = 1;
            }
        }

        public void DisableComponent()
        {
            componentNum--;
            DebugLog.AddLog2("有timeline volume animation组件关闭，当前数量{0}", componentNum);
            if(instance.componentNum == 0)
            {
                DebugLog.AddLog("所有timeline volume animation组件移除，关闭timeline volume");
                instance.TimelineVolume.weight = 0;
                instance.TimelineVolume = null;
            }
        }
        public void SetValueFlag()
        {
            // flag_GetValue = true;
        }

        public void Effect(Camera camera)
        {
            // if (TimelineVolume != null)
            // {
            //     TimelineVolume.weight = flag_GetValue ? 1 : 0;
            // }
            //
            // if (camera.name == "UICamera")
            // {
            //     flag_GetValue = false;
            // }
        }
        // public URPTimelineVolumnLight()
        // {
        //     StartCoroutine(Clear());
        // }
        // public void EnableWeight()
        // {
        //     if (flag_GetValue)
        //     {
        //         TimelineVolume.weight = 1;
        //     }
        // }

        // public void Inactive()
        // {
        //     if (flag_GetValue)
        //     {
        //         TimelineVolume.weight = 0;
        //         flag_GetValue = false;
        //     }
        // }

        // IEnumerator Clear()
        // {
        //     while (true)
        //     {
        //         yield return new WaitForEndOfFrame();
        //         Inactive();
        //     }
        // }
    }
}