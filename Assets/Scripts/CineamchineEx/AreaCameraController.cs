using CFUtilPoolLib;
using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CineamchineEx
{
    class AreaCameraController : FreeLookController, IAreaCamera
    {
        public override void Init()
        {
            base.Init();
            isFreelook = false;
        }
        public void SetAreaParam(AreaCameraList.RowData[] data, Transform obj, uint CamID)
        {
            if (ForSpecialSetting) return;
            int chosenCam = 0;
            while (chosenCam < data.Length)
            {
                if (data[chosenCam].ID == CamID)
                    break;
                chosenCam++;
            }
            if (chosenCam == data.Length)
                return;//没有相符的镜头

            m_camParamData = data[chosenCam];
            m_trackObj = obj;
            FreeLook.m_useDistance = true;
        }

        #region TableParamData
        private AreaCameraList.RowData m_camParamData;
        private Transform m_trackObj;
        [HideInInspector] private bool useLerp = true;
        protected override void Update()
        {
            base.Update();
            if (FreeLook != null)
            {
                UpdataByDistance();
            }
        }

        /// <summary>
        /// 根据距离更新参数,贼丑，有空改
        /// </summary>
        private void UpdataByDistance()
        {
            if (FreeLook.m_useDistance == false
                || m_camParamData == null || m_trackObj == null || Follow == null) return;
            var length = Vector3.Distance(m_trackObj.position, freeLook.Follow.position);
            int tableLength = m_camParamData.Distance.Length;
            float[] param = new float[10];
            if (length < m_camParamData.Distance[0])
            {
                for (int j = 0; j < 3; j++)
                {
                    param[j] = m_camParamData.Height[0, j];
                    param[j + 3] = m_camParamData.Radius[0, j];
                    param[j + 6] = m_camParamData.ScreenY[0, j];
                }
                param[9] = m_camParamData.FOV[0];
            }
            else if (length > m_camParamData.Distance[tableLength - 1])
            {
                for (int j = 0; j < 3; j++)
                {
                    param[j] = m_camParamData.Height[tableLength - 1, j];
                    param[j + 3] = m_camParamData.Radius[tableLength - 1, j];
                    param[j + 6] = m_camParamData.ScreenY[tableLength - 1, j];
                }
                param[9] = m_camParamData.FOV[tableLength - 1];
            }
            else
            {
                int i = 1;
                while (i < tableLength)
                {
                    if (length < m_camParamData.Distance[i])
                        break;
                    i++;
                }
                var left = m_camParamData.Distance[i - 1];
                float percent = (length - left) / (m_camParamData.Distance[i] - left);
                float subPercent = 1 - percent;
                for (int j = 0; j < 3; j++)
                {
                    param[j] = subPercent * m_camParamData.Height[i - 1, j] + percent * m_camParamData.Height[i, j];
                    param[j + 3] = subPercent * m_camParamData.Radius[i - 1, j] + percent * m_camParamData.Radius[i, j];
                    param[j + 6] = subPercent * m_camParamData.ScreenY[i - 1, j] + percent * m_camParamData.ScreenY[i, j];
                }
                param[9] = (1 - percent) * m_camParamData.FOV[i - 1] + percent * m_camParamData.FOV[i];
            }
            SetParamByLerp(ref param);
        }
        private void SetParamByLerp(ref float[] param)
        {
            for (int i = 0; i < 3; ++i)
            {
                FreeLook.m_Orbits[i].Scale = 1;
                FreeLook.m_Orbits[i].m_Height = param[i] + (useLerp ? (FreeLook.m_Orbits[i].m_Height - param[i]) / 2 : 0);
                FreeLook.m_Orbits[i].m_Radius = param[i + 3] + (useLerp ? (FreeLook.m_Orbits[i].m_Radius - param[i + 3]) / 2 : 0);
                FreeLook.GetRig(i).GetCinemachineComponent<CinemachineComposer>().ScreenY = param[i + 6] + (useLerp ? (FreeLook.GetRig(i).GetCinemachineComponent<CinemachineComposer>().ScreenY - param[i + 6]) / 2 : 0);
            }
            if (param.Length > 9) FieldOfView = param[9] + (useLerp ? (FieldOfView - param[9]) / 2 : 0);
        }

        #endregion

        #region gizmos
        private void OnDrawGizmos()
        {
            if (m_trackObj == null || m_camParamData == null) return;
            var center = m_trackObj.position;
            var table = m_camParamData;
            Gizmos.color = Color.red;
            for (int j = 0; j < table.Distance.Length; j++)
                DrawCircle(center, table.Distance[j]);
        }
        private static void DrawCircle(Vector3 _center, float _radius, int _lineNum = 60)
        {
            Vector3 forwardLine = Vector3.forward * _radius;
            Vector3 curPos = _center + forwardLine;
            Vector3 prePos = curPos;
            for (int i = 0; i < _lineNum; i++)
            {
                forwardLine = _radius * XCommon.singleton.HorizontalRotateVetor3(forwardLine, 360f / _lineNum);
                curPos = forwardLine + _center;
                Gizmos.DrawLine(prePos, curPos);
                prePos = curPos;
            }
        }
        #endregion
    }
}
