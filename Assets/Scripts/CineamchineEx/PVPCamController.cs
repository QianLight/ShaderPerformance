using CFEngine;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CineamchineEx
{
    class PVPCamController:FreeLookController,IPVPCamera, ICameraAxis
    {
        private XGameObject target=null;
        private Transform player = null;

        //private float timer = 0;
        //private readonly float ResetTimer=2f;

        private float lockIntensity = 0.5f;

        public bool IsLocking { get =>isLocking; set => isLocking=value; }
        private bool isLocking = true;
        void IPVPCamera.SetTarget(XGameObject go)
        {
            target = go;
            player = FreeLook.Follow;
        }

        public override void Init()
        {
            base.Init();
            isFreelook = false;
        }
        protected override void Update()
        {
            base.Update();

            if (FreeLook != null 
                && target!=null && player!=null
                && isLocking)
            {
                var targetDir = target.Pos - player.position;
                targetDir.y = 0;
                var camDir = player.position - transform.position;
                camDir.y = 0;

                xAxisValue = Vector3.SignedAngle(camDir, targetDir, Vector3.up)*lockIntensity;
                yAxisValue = yAxisValue+(0.5f-yAxisValue)*lockIntensity;
            }
        }
        private void OnDisable()
        {
            target = null;
            player = null;
        }
    }
}
