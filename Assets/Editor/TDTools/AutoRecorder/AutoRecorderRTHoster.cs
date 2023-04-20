using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TDTools
{
    public class AutoRecorderRTHoster : MonoBehaviour
    { 
        public Action AutoFire;
        public void Update()
        {
            AutoFire?.Invoke();
        }

        public static AutoRecorderRTHoster GetHoster
        {
            get
            {
                GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
                if (camera == null) return null;
                var hoster = camera.GetComponent<AutoRecorderRTHoster>();
                if (hoster == null)
                    hoster = camera.AddComponent<AutoRecorderRTHoster>();
                return hoster;
            }
        }


        public void ChangeCameraLookAt()
        {

        }

        public void AddPuppt(int sid, float face = 0, float offsetX = 0,
            float offsetY = 0, float offsetZ = 0)
        {

        }

        public void FireSkill()
        {

        }

        void CheckSkillFinish()
        {

        }
    }
}
